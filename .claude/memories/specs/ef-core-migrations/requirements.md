# Requirements: EF Core Migrations 전환

**Feature ID**: `ef-core-migrations`
**Created**: 2025-11-11
**Status**: Draft
**Size**: L (Large) - 인프라 변경, 새 기술 도입, Blast Radius 5점

---

## 📋 Overview

### Problem Statement
현재 프로젝트는 단일 SQL 파일(`migration.sql`)에 모든 마이그레이션을 누적하는 방식을 사용 중입니다. 이 방식은 다음과 같은 치명적인 문제가 있습니다:

- ❌ **롤백 불가능**: 마이그레이션 실패 시 수동 SQL 작성 필요 (10-30분 다운타임)
- ❌ **파일 크기 무한 증가**: 1413줄 → 6개월 후 5000줄 예상
- ❌ **팀 협업 충돌**: 여러 개발자가 동일 파일 수정 시 Git 충돌
- ❌ **배포 시간 증가**: 매번 전체 파일 실행 (IF NOT EXISTS 체크)

10대 서버 수평 확장 환경에서, 마이그레이션 실패 시 전체 서비스 장애로 이어질 수 있습니다.

### Solution
**EF Core Migrations**로 전환하여 자동화된 마이그레이션 관리 및 롤백 기능을 도입합니다.

### Goals
1. **1-2분 내 롤백 가능**: `dotnet ef database update PreviousMigration` 한 줄로 복구
2. **버전 관리 강화**: 각 마이그레이션이 개별 파일로 분리, Git으로 추적
3. **팀 동기화**: 프로젝트 로컬 도구(manifest)로 동일한 EF Core 버전 보장
4. **점진적 전환**: 레거시 migration.sql과 병행하여 안전망 유지

### Success Metrics
- ✅ 롤백 시간: 30분 → **2분** (93% 개선)
- ✅ 마이그레이션 히스토리 보존율: **100%** (기존 10개 레코드 유지)
- ✅ 첫 배포 성공률: **100%** (하이브리드 안전망)
- ✅ 개발자 온보딩 시간: 30분 → **5분** (`dotnet tool restore` 자동화)

---

## 👥 User Stories

### US-1: 개발자 - 마이그레이션 자동 생성
**AS-A** 백엔드 개발자
**I-WANT** Entity 변경 시 마이그레이션 코드를 자동 생성
**SO-THAT** 수동 SQL 작성 시간을 절약하고 휴먼 에러를 방지할 수 있다

**Value**: High
**Effort**: Medium

---

### US-2: DevOps - 배포 실패 시 즉시 롤백
**AS-A** DevOps 엔지니어
**I-WANT** 마이그레이션 실패 시 1-2분 내 이전 버전으로 롤백
**SO-THAT** 10대 서버 장애 시 사용자 영향을 최소화할 수 있다

**Value**: Critical
**Effort**: Low

---

### US-3: 팀 리드 - 마이그레이션 히스토리 추적
**AS-A** 팀 리드
**I-WANT** 각 마이그레이션이 개별 파일로 Git 커밋에 포함
**SO-THAT** 변경 이력을 추적하고 코드 리뷰를 수행할 수 있다

**Value**: High
**Effort**: Low

---

## ✅ Acceptance Criteria (EARS Format)

### AC-1: EF Core Migrations 도구 설치
**WHEN** 개발자가 프로젝트를 클론한 후
**THEN** `dotnet tool restore` 실행 시
**THE SYSTEM SHALL** `.config/dotnet-tools.json`을 읽고 EF Core 9.0.0 도구를 자동 설치한다

**WHERE** 프로젝트 로컬 도구 (manifest 관리)
**IF** Jenkins CI/CD 환경일 경우
**THEN** 도구가 자동으로 복원되어 `dotnet ef` 명령어 사용 가능하다

---

### AC-2: 마이그레이션 자동 생성
**WHEN** 개발자가 Entity를 수정하고 `dotnet ef migrations add {Name}` 실행 시
**THEN** `IdleRPG.Infrastructure/Migrations/` 폴더에
**THE SYSTEM SHALL** 다음 3개 파일을 생성한다:
- `{Timestamp}_{Name}.cs` (Up/Down 메서드)
- `{Timestamp}_{Name}.Designer.cs` (메타데이터)
- `GameDBContextModelSnapshot.cs` (현재 스키마)

**WHERE** Up() 메서드는 스키마를 최신 버전으로 업그레이드
**AND** Down() 메서드는 이전 버전으로 롤백하는 로직을 포함한다

---

### AC-3: Jenkins 하이브리드 마이그레이션
**WHEN** Jenkins가 배포를 시작할 때
**THEN** Database Migration 스테이지에서
**THE SYSTEM SHALL** 다음 순서로 실행한다:
1. `psql -f migration.sql` (레거시, Idempotent)
2. `dotnet ef database update` (EF Core, 새로운 변경사항만)

**WHERE** 레거시 마이그레이션이 실패하거나 존재하지 않을 경우
**IF** 에러가 발생하면
**THEN** 배포를 중단하고 실패를 보고한다

**INVARIANT** 마이그레이션 완료 후에만 Docker 빌드 및 배포가 진행된다

---

### AC-4: 롤백 시나리오
**WHEN** 배포 후 장애가 발생하여 롤백이 필요할 때
**THEN** DevOps가 `dotnet ef database update {PreviousMigration}` 실행 시
**THE SYSTEM SHALL** Down() 메서드를 실행하여 DB를 이전 상태로 복구한다

**WHERE** 복구 시간은 **2분 이내**
**AND** `__EFMigrationsHistory` 테이블에서 해당 마이그레이션 레코드가 제거된다

---

### AC-5: 기존 마이그레이션 히스토리 보존
**WHEN** EF Core Migrations로 전환할 때
**THEN** `__EFMigrationsHistory` 테이블의 기존 10개 레코드는
**THE SYSTEM SHALL** 그대로 유지한다

**WHERE** 새로운 EF Core 마이그레이션은 기존 레코드 다음에 추가된다
**INVARIANT** 총 레코드 수 = 10 (레거시) + N (EF Core 신규)

---

### AC-6: 로컬 테스트 환경
**WHEN** 개발자가 로컬에서 마이그레이션을 테스트할 때
**THEN** `docker-compose up -d postgres` 후 `dotnet ef database update` 실행 시
**THE SYSTEM SHALL** 로컬 PostgreSQL에 마이그레이션을 적용한다

**WHERE** Up/Down 반복 테스트 가능
**AND** `dotnet ef migrations remove`로 마이그레이션 삭제 가능

---

## 🔧 Technical Requirements

### TR-1: EF Core Tools 설치 (프로젝트 로컬)
```bash
# .config/dotnet-tools.json 생성
dotnet new tool-manifest
dotnet tool install dotnet-ef --version 9.0.0 --local
git add .config/dotnet-tools.json
```

**근거**:
- 팀원 모두 동일한 EF Core 버전 사용 (버전 불일치 방지)
- Jenkins에서 `dotnet tool restore` 자동 실행
- Git으로 버전 관리

---

### TR-2: Jenkinsfile 수정 (하이브리드 접근)
```groovy
stage('Database Migration') {
    steps {
        echo 'Running hybrid migrations...'
        sh '''
            cd /home/ec2-user/IdleRPGServer

            # 1. 도구 복원
            dotnet tool restore

            # 2. 레거시 마이그레이션 (안전망)
            if [ -f "IdleRPG.Infrastructure/migration.sql" ]; then
                echo "Running legacy migration.sql..."
                export PGPASSWORD=$PGPASSWORD
                psql -h idlerpg-dev.chiqweeeuidv.ap-northeast-2.rds.amazonaws.com \
                     -U postgres \
                     -d idlerpg \
                     -f IdleRPG.Infrastructure/migration.sql || exit 1
            fi

            # 3. EF Core 마이그레이션 (새로운 변경사항)
            echo "Running EF Core migrations..."
            dotnet ef database update \
                --project IdleRPG.Infrastructure \
                --connection "Host=idlerpg-dev...;Database=idlerpg;..." || exit 1

            # 4. 마이그레이션 검증
            psql -h idlerpg-dev... -U postgres -d idlerpg -c "
                SELECT MigrationId, ProductVersion
                FROM __EFMigrationsHistory
                ORDER BY MigrationId DESC LIMIT 5;
            "
        '''
    }
}
```

---

### TR-3: 기존 히스토리 보존 전략
```bash
# 1. 현재 DB 스키마를 EF Core ModelSnapshot으로 생성
dotnet ef migrations add InitialFromExistingDb --project IdleRPG.Infrastructure

# 2. __EFMigrationsHistory는 그대로 유지 (삭제 금지)

# 3. 레거시 migration.sql 보존
mv IdleRPG.Infrastructure/migration.sql docs/migrations/legacy/migration.sql

# 4. 이후 변경사항은 EF Core로만 관리
dotnet ef migrations add RemoveTierColumn --project IdleRPG.Infrastructure
```

---

### TR-4: 롤백 명령어 (긴급 상황)
```bash
# EC2 SSH 접속
ssh ec2-user@13.209.66.253

# 현재 마이그레이션 확인
dotnet ef migrations list --project /home/ec2-user/IdleRPGServer/IdleRPG.Infrastructure

# 이전 버전으로 롤백
dotnet ef database update {PreviousMigrationId} \
    --project /home/ec2-user/IdleRPGServer/IdleRPG.Infrastructure \
    --connection "Host=idlerpg-dev...;Database=idlerpg;..."

# 검증
psql -h idlerpg-dev... -U postgres -d idlerpg -c "SELECT * FROM __EFMigrationsHistory ORDER BY MigrationId DESC LIMIT 3;"
```

---

## 📦 Dependencies

### Internal
- ✅ `IdleRPG.Infrastructure/Data/GameDBContext.cs` - EF Core DbContext
- ✅ `IdleRPG.Infrastructure/Configurations/` - Entity 설정 (54개 파일)
- ✅ `Jenkinsfile` - CI/CD 파이프라인
- ⚠️ `migration.sql` - 레거시 마이그레이션 (1413줄, 점진적 제거)

### External
- ✅ **EF Core 9.0.0**: 이미 사용 중 (`Npgsql.EntityFrameworkCore.PostgreSQL`)
- 🆕 **dotnet-ef CLI 9.0.0**: 새로 설치 (프로젝트 로컬 도구)
- ✅ **PostgreSQL 14**: RDS 운영 중
- ✅ **Jenkins**: CI/CD 구축 완료

---

## 🚀 Non-Functional Requirements

### Performance
- **마이그레이션 실행 시간**:
  - 기존: 5-10초 (1413줄 전체 체크)
  - 목표: 2-3초 (새로운 마이그레이션만)
- **롤백 시간**: 1-2분 이내

### Reliability
- **배포 실패 시 자동 중단**: 마이그레이션 실패 → 배포 중단 (10대 서버 보호)
- **Idempotent**: 동일 마이그레이션 재실행 시 안전 (EF Core 자동 보장)

### Scalability
- **10대 서버 환경**: Jenkins에서 1회만 실행 → 10대 서버는 마이그레이션 완료된 DB 접근
- **경합 조건 없음**: EF Core가 `__EFMigrationsHistory` 테이블 락으로 동시 실행 방지

### Security
- **DB Credentials**: Jenkins Credentials로 관리 (코드에 하드코딩 금지)
- **접근 제어**: EC2 SSH 롤백은 DevOps 권한 필요

### Maintainability
- **마이그레이션 파일**: Git으로 버전 관리, 코드 리뷰 가능
- **문서화**: 각 마이그레이션에 주석 필수 (왜 변경했는지)

---

## 🚫 Out of Scope

### Phase 1 (현재 Spec)에서 제외
- ❌ **레거시 migration.sql 완전 제거**: 1-2주 모니터링 후 Phase 2에서 결정
- ❌ **Staging 환경 구축**: 로컬 테스트로 대체 (비용 절감)
- ❌ **자동화된 롤백 스크립트**: 수동 롤백 명령어로 충분 (학습 목적)
- ❌ **마이그레이션 Dry-Run CI 단계**: DBA 승인 프로세스 없음 (학습 프로젝트)

### 향후 고려 사항
- 📋 **Flyway/Liquibase 비교**: 엔터프라이즈 도구 평가 (Phase 3)
- 📋 **Blue-Green 배포**: 무중단 배포 패턴 (Phase 4)
- 📋 **자동화된 통합 테스트**: 마이그레이션 후 API 테스트 (Phase 2)

---

## 🎯 TODO(human) - 아키텍처 학습 포인트

> **학습 목표**: 게임 밸런스가 아닌 **인프라/아키텍처 결정**을 직접 경험

### TODO-1: 마이그레이션 실행 타이밍 선택
**위치**: Design 단계 - "Deployment Strategy" 섹션

**학습 포인트**: CI/CD vs Application-Driven Migration

**결정 필요**:
- **Option A (현재 선택)**: Jenkins에서 배포 전 1회 실행
- **Option B**: 각 앱 서버 시작 시 자동 실행 (`Program.cs`에서 `MigrateAsync()`)

**트레이드오프**:
- A: 배포 전 1회 실행, 안전, 10대 서버 경합 없음
- B: 앱 코드에서 자동 실행, 간단, 첫 서버 시작 시간 증가

**근거 작성**: Design 문서에 선택 이유 기록

---

### TODO-2: 롤백 전략 수립
**위치**: Design 단계 - "Rollback Strategy" 섹션

**학습 포인트**: Backward Compatibility & Zero-Downtime

**결정 필요**:
- 데이터 손실 위험이 있는 마이그레이션 처리 방법
  - DropColumn → 2-Step Migration (Nullable → 데이터 이관 → NOT NULL)
  - AddColumn NOT NULL → DefaultValue 설정

**시나리오 작성**:
- 배포 후 10분 뒤 장애 발견 → DB 롤백 vs Code 롤백 vs Forward Fix

**근거 작성**: 각 시나리오별 복구 시간 & 리스크 분석

---

## ✅ Approval

- [ ] **Technical Lead**: 아키텍처 검토 완료
- [ ] **DevOps**: Jenkins 파이프라인 변경 확인
- [ ] **Developer**: TODO(human) 학습 포인트 확인

**Approved By**: _____________
**Date**: _____________

---

**Next Steps**:
1. ✅ Requirements 승인 완료
2. 🔄 Spike 제안 검토 (선택: N - 건너뛰기)
3. ➡️ Run: `/spec-design ef-core-migrations`

