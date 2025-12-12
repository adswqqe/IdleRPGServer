# ADR-0009: 하이브리드 마이그레이션 전략 (레거시 SQL + EF Core)

> **ADR 작성 조건**: 인프라 변경, 새 기술 도입, 배포 전략 변경
>
> **관련 Spec**: `.claude/memories/specs/ef-core-migrations/`

**Status**: Accepted
**Date**: 2025-11-11
**Decider(s)**: Development Team
**Related Spike**: N/A (EF Core는 이미 프로젝트에 사용 중)

---

## Context (배경)

### 문제 상황
현재 프로젝트는 단일 SQL 파일(`migration.sql`, 1413줄)에 모든 마이그레이션을 누적하는 방식을 사용 중입니다. 이는 다음과 같은 문제를 야기합니다:

1. **롤백 불가능**: 마이그레이션 실패 시 수동 SQL 작성 필요 (10-30분 다운타임)
2. **파일 크기 무한 증가**: 1413줄 → 6개월 후 5000줄 예상
3. **팀 협업 충돌**: 여러 개발자가 동일 파일 수정 시 Git 충돌
4. **배포 시간 증가**: 매번 전체 파일 실행 (IF NOT EXISTS 체크)

### 현재 상태
- ✅ EF Core 9.0 이미 사용 중 (`GameDBContext`, 54개 Entity Configurations)
- ✅ `__EFMigrationsHistory` 테이블 존재 (10개 레거시 레코드)
- ⚠️ `migration.sql`: 1413줄, Idempotent 패턴 (IF NOT EXISTS)
- ✅ Jenkins CI/CD: Database Migration 스테이지 운영 중

### 고려사항
- **리스크 최소화**: 점진적 전환으로 안전망 유지
- **팀 학습 곡선**: EF Core Migrations CLI 사용법 습득 필요
- **배포 안정성**: 10대 서버 수평 확장 환경에서 안전한 마이그레이션
- **롤백 시간**: 30분 → 2분 (93% 개선) 목표

---

## Decision (결정)

### 선택한 방안
**하이브리드 접근**: 레거시 `migration.sql`과 EF Core Migrations를 병행 실행

**Jenkins 실행 순서**:
1. `psql -f migration.sql` (레거시, 안전망)
2. `dotnet ef database update` (EF Core, 새로운 변경사항)

### 선택 이유

#### 1. 점진적 전환으로 리스크 최소화
- 레거시 마이그레이션은 1-2주간 안전망으로 유지
- 문제 발생 시 즉시 레거시로 복귀 가능
- 팀원들이 EF Core Migrations에 적응할 시간 확보

#### 2. 기존 히스토리 보존
- `__EFMigrationsHistory` 테이블의 10개 레거시 레코드 유지
- 마이그레이션 히스토리 연속성 보장
- 감사(audit) 요구사항 충족

#### 3. 배포 파이프라인 안정성
- 레거시 마이그레이션 실패 시 EF Core 실행 안 함 (순차 보장)
- 각 단계 실패 시 `|| exit 1`로 파이프라인 중단
- 10대 서버는 마이그레이션 완료된 DB에만 접근

#### 4. 학습 프로젝트 목표 부합
- **아키텍처 학습**: 점진적 마이그레이션 전략 경험
- **DevOps 이해**: CI/CD 파이프라인 변경 과정 학습
- **롤백 전략**: 데이터베이스 버전 관리 및 복구 방법 이해

---

## Alternatives Considered (검토한 대안들)

### Alternative 1: EF Core Migrations 즉시 전환 (레거시 제거)

**장점**:
- 간결한 구조 (마이그레이션 도구 단일화)
- Jenkins 파이프라인 단순화 (1단계만)
- 레거시 코드 제거 (유지보수 부담 감소)

**단점**:
- ❌ **높은 리스크**: 첫 배포 실패 시 롤백 어려움
- ❌ **팀 준비 부족**: EF Core CLI 경험 부족 상태에서 즉시 전환
- ❌ **히스토리 손실 가능성**: 레거시 10개 레코드 보존 실패 시 추적 불가

**기각 이유**:
- 학습 프로젝트이지만 **안정성 우선** (10대 서버 운영 중)
- 팀원들이 EF Core Migrations 사용법을 익힐 시간 필요

---

### Alternative 2: Application-Driven Migration (앱 서버 시작 시 자동 실행)

**장점**:
- 간편한 구성 (`Program.cs`에서 `context.Database.MigrateAsync()` 호출)
- Jenkins에서 별도 단계 불필요
- 각 앱 서버가 독립적으로 마이그레이션 실행 (분산 환경 대비)

**단점**:
- ❌ **첫 서버 시작 시간 증가**: 마이그레이션 완료 대기 (2-10초)
- ❌ **10대 서버 경합 위험**: 동시 실행 시 `__EFMigrationsHistory` 락 경쟁
- ❌ **롤백 복잡도 증가**: 앱 서버와 DB 버전 불일치 가능성

**기각 이유**:
- **명시적 제어 선호**: Jenkins에서 1회만 실행하여 안정성 보장
- 10대 서버 환경에서 경합 조건 방지

---

### Alternative 3: Flyway/Liquibase 도입 (엔터프라이즈 도구)

**장점**:
- 엔터프라이즈급 안정성 (대규모 프로젝트 검증)
- 플랫폼 독립적 (Java 기반)
- DBA 친화적 (SQL 중심)

**단점**:
- ❌ **학습 곡선**: 새로운 도구 학습 필요 (EF Core는 이미 사용 중)
- ❌ **추가 의존성**: Java 런타임 필요 (현재 .NET 환경)
- ❌ **EF Core와 충돌 가능성**: 두 도구 동시 사용 시 혼란

**기각 이유**:
- **학습 프로젝트 목표**: EF Core 생태계에 집중 (Clean Architecture 학습)
- EF Core Migrations만으로도 요구사항 충족 가능

---

## Consequences (결과 및 영향)

### Positive (긍정적 영향)

#### ✅ 롤백 시간 93% 개선
- 기존: 30분 (수동 SQL 작성 + 배포)
- 신규: 1-2분 (`dotnet ef database update {PreviousMigration}`)

#### ✅ 팀 협업 충돌 제거
- 각 마이그레이션이 개별 파일 (`{Timestamp}_{Name}.cs`)
- Git 병합 충돌 최소화
- 코드 리뷰 가능 (Pull Request에서 Up/Down 메서드 검증)

#### ✅ 마이그레이션 히스토리 추적
- `__EFMigrationsHistory` 테이블로 적용 이력 보존
- 각 마이그레이션에 타임스탬프, 버전 기록
- 감사(audit) 요구사항 충족

#### ✅ 배포 시간 단축
- 기존: 5-10초 (1413줄 전체 IF NOT EXISTS 체크)
- 신규: 2-3초 (새로운 마이그레이션만 실행)
- 50% 시간 단축

---

### Negative (부정적 영향 또는 Trade-off)

#### ⚠️ Jenkins 파이프라인 복잡도 증가
- 기존: 1단계 (`psql -f migration.sql`)
- 신규: 4단계 (도구 복원 → 레거시 → EF Core → 검증)
- 트레이드오프: 안정성 향상 vs 복잡도 증가

**대응책**:
- Jenkins 스크립트에 명확한 주석 추가
- 각 단계별 에러 핸들링 (`|| exit 1`)
- 로그 출력으로 진행 상황 추적

#### ⚠️ 팀 학습 곡선
- 개발자들이 EF Core CLI 명령어 습득 필요
- 로컬 테스트 절차 변경 (Up/Down 반복 테스트)

**대응책**:
- 문서화: `docs/development/EF_CORE_MIGRATIONS_GUIDE.md` (별도 작성)
- 로컬 테스트 절차 표준화 (Design 문서 Phase 5 참조)

#### ⚠️ 1-2주간 이중 관리 부담
- `migration.sql` 유지 + EF Core Migrations 신규 작성
- 불일치 가능성 (레거시 수정 금지 규칙 필요)

**대응책**:
- **규칙**: 모든 신규 변경은 EF Core Migrations로만 작성
- `migration.sql`은 수정 금지 (읽기 전용)
- 2-3주 후 레거시 제거 결정 (팀 동의 필요)

---

### Neutral (중립적 변화)

#### 🔄 `.config/dotnet-tools.json` 추가
- 프로젝트 로컬 도구로 EF Core CLI 9.0.0 등록
- 팀원 모두 `dotnet tool restore`로 동일 버전 사용
- Git으로 버전 관리

#### 🔄 `InitialFromExistingDb` 마이그레이션
- 빈 Up/Down 메서드 (Baseline Migration)
- 현재 스키마를 EF Core ModelSnapshot으로 변환
- 이후 변경사항의 기준점 역할

---

## Implementation Notes (구현 시 주의사항)

### 필수 조치

- [x] `.config/dotnet-tools.json` 생성 및 Git 커밋
- [x] `InitialFromExistingDb` 마이그레이션 생성 (빈 Up/Down)
- [x] Jenkinsfile 수정 (4단계 하이브리드 실행)
- [ ] 로컬 테스트 (Up/Down 반복 3회 이상)
- [ ] 첫 배포 후 `__EFMigrationsHistory` 레코드 검증 (10 + 1 = 11개)
- [ ] 2-3주 모니터링 후 레거시 제거 여부 결정

### 권장 사항

#### 1. Pull Request 코드 리뷰 체크리스트
```markdown
- [ ] `Migrations/` 폴더 변경 확인
- [ ] Up() 메서드가 스키마를 올바르게 변경하는가?
- [ ] Down() 메서드가 Up()을 정확히 되돌리는가?
- [ ] 데이터 손실 위험이 있는가? (DROP COLUMN, ALTER NOT NULL)
- [ ] 인덱스 추가/삭제가 성능에 미치는 영향 분석
```

#### 2. 로컬 테스트 의무화
```bash
# 개발자는 커밋 전 반드시 실행
dotnet ef database update                    # Up
dotnet ef database update {PreviousMigration} # Down
dotnet ef database update                    # Up 다시
# 3회 반복 성공 → 커밋
```

#### 3. 마이그레이션 네이밍 규칙
```
{Timestamp}_{PascalCase}.cs
예: 20251111120000_RemovePvpRankingTierColumn.cs
```

- 동사로 시작 (Add, Remove, Alter, Create, Drop)
- 변경 대상 명확히 기술 (테이블명 + 컬럼명)

#### 4. 데이터 손실 방지 (2-Step Migration)
```csharp
// ❌ 나쁜 예: 데이터 손실 위험
migrationBuilder.DropColumn(
    name: "OldColumn",
    table: "Characters");

// ✅ 좋은 예: 2단계 분리
// Migration 1: AddNewColumn (Nullable)
migrationBuilder.AddColumn<int>(
    name: "NewColumn",
    table: "Characters",
    nullable: true);

// Migration 2: MigrateData + AlterColumn NOT NULL
// (별도 마이그레이션으로 분리)
```

---

## References (참고 자료)

### EF Core 공식 문서
- [Managing Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [Applying Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying)
- [Reverting Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/managing)

### 프로젝트 내부 문서
- [requirements.md](../../../.claude/memories/specs/ef-core-migrations/requirements.md)
- [design.md](../../../.claude/memories/specs/ef-core-migrations/design.md)
- [CLAUDE.md - Kiro Workflow](../../../CLAUDE.md#kiro-workflow-spec-driven-development)

### 관련 Issue/PR
- N/A (초기 도입)

---

## Review History (검토 이력)

| Date | Reviewer | Comment | Action |
|------|----------|---------|--------|
| 2025-11-11 | Development Team | 하이브리드 전략 승인, 2-3주 모니터링 후 재평가 | Accepted |

---

**Next Steps**:
1. ✅ ADR-0009 Accepted
2. ➡️ Design 문서 Self-Review Checklist 실행
3. ➡️ 사용자 승인 후 `/spec-tasks ef-core-migrations` 실행
