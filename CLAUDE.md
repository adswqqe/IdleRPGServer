# CLAUDE.md

## 🎓 Project Purpose

**이 프로젝트는 학습 목적의 프로젝트입니다.**

**학습 목표**:
- Clean Architecture 패턴 이해 및 실습
- ASP.NET Core 8.0 백엔드 개발
- EF Core + PostgreSQL 데이터 액세스
- Unity 클라이언트 연동 (서버-클라이언트 통합 학습)
- JWT 인증, 테스트 코드 작성

**핵심 원칙**:
- ✅ **아키텍처와 코드 구조**에 집중
- ✅ 게임 밸런스(확률, 재화 비용 등)는 **AI가 합리적인 기본값 제안** (학습자는 그대로 사용)
- ✅ **TODO(human)**은 아키텍처 학습 포인트에만 사용

---

## Language and Communication
**모든 응답은 한국어로 제공**. 코드와 명령어는 원문 유지.

**Architecture Layers** (Dependency: API → Application → Domain):
- **API**: Controllers, Middleware, SignalR
- **Application**: Services, DTOs (MediatR, FluentValidation, AutoMapper)
- **Domain**: Entities, Business rules
- **Infrastructure**: Repositories (EF Core + PostgreSQL)

## Critical Rules

### 🗃️ Database Migration
**마이그레이션 형식**: `IdleRPG.Infrastructure/migration.sql` (단일 파일, Idempotent 패턴)

## Development Standards

**핵심 원칙**:
- **Feature Development Order**: Domain → Application → Infrastructure → API → Tests
- **Naming**: PascalCase (클래스), camelCase (변수), _camelCase (private 필드)

## Kiro Workflow (Spec-Driven Development)

> ⚠️ **문서 규칙 기준 (Source of Truth)**:
> 본 문서가 Kiro 워크플로우의 **유일한 진실 공급원(Single Source of Truth)**입니다.
> 템플릿은 문서 구조와 간단한 리마인더만 제공하며, 상세 규칙은 이 문서를 참조하세요.

**Kiro**는 코드 작성 전 요구사항-설계-작업을 체계적으로 문서화하는 방법론입니다.

**핵심 철학**: "AI가 추측하지 않고, 과거 결정을 존중하며, 체계적으로 작업하도록"

### 📏 Spec 사이즈 시스템 (S/M/L)

**AI가 자동으로 사이즈를 판단하고 추천하며, 사용자가 최종 결정합니다.**

| 사이즈 | 복잡도 | 리스크 점수 | 워크플로우 | 소요 시간 | 예시 |
|--------|--------|----------|----------|----------|------|
| **S (Small)** | 단순 CRUD, 단일 엔드포인트 | 3-5점 (저위험) | 인라인 구현 + PR 템플릿 | 30분~1시간 | GET /api/characters/{id}/stats |
| **M (Medium)** | 2-3 엔드포인트, 중간 로직 | 6-9점 (중위험) | spec-lite.md (단일 파일) | 2~4시간 | 캐릭터 이름 변경 |
| **L (Large)** | 복잡한 시스템, 새 기술 도입 | 10-15점 (고위험) | Full Spec (requirements + design + tasks) | 1~3일 | 펫 시스템, 결제 연동 |

**AI 판단 프로세스**:
1. **복잡도 분석**: Entity 개수, 엔드포인트, 비즈니스 로직
2. **리스크 점수**: Blast Radius + Novelty + Unknowns (3-15점)
3. **자동 승격 검사**: 트리거 충족 시 상위 사이즈
4. **최종 결정**: 복잡도와 리스크 중 높은 기준 적용

**기본 복잡도 기준**:
- **S**: 새 Entity 0개, 단일 엔드포인트, 비즈니스 로직 없음
- **M**: 새 Entity 1개 이하, 2-3 엔드포인트, 간단한 비즈니스 로직
- **L**: 새 Entity 2개 이상, 4개+ 엔드포인트, 복잡한 로직, 새 기술 도입

> 💡 **Note**: 리스크 점수와 자동 승격 규칙은 복잡도 기준을 보완합니다. 자세한 내용은 `.claude/commands/spec-init.md` 참조.

### 🔄 자동 사이즈 승격 규칙

AI는 다음 트리거 **하나라도** 충족 시 자동으로 상위 사이즈로 승격:

#### S → M 승격 트리거
- ✅ 기존 Entity 수정 시 새 필드 **3개 이상** 추가
- ✅ **비즈니스 로직 포함** (단순 CRUD 아님: 계산, 검증, 상태 변경)
- ✅ **외부 API 호출** (다른 마이크로서비스, 외부 서비스)
- ✅ **복잡한 쿼리** (Join 3개 이상, 집계 함수, 서브쿼리)

#### M → L 승격 트리거 (강제 승격)
- ✅ **새 기술/라이브러리 첫 도입** (SignalR, Redis, gRPC, MediatR)
- ✅ **데이터 모델 마이그레이션** (기존 테이블 스키마 변경, 관계 수정)
- ✅ **외부 서비스 연동** (결제 PG, 푸시 알림, SMS, 이메일)
- ✅ **성능 요구사항 명시** (응답 시간 < 100ms, 동시 접속 1000명+)
- ✅ **보안/권한 모델 변경** (새 Role 추가, 인증 방식 변경)
- ✅ **크로스컷팅 도입** (공통 캐싱, 로깅 전략, 트랜잭션 정책)

#### 승격 프로세스
```
1. AI가 기능 분석 중 트리거 감지
2. 사용자에게 승격 사유 설명:

   ⚠️ **자동 승격 감지: S → L**

   **트리거**: Redis 첫 도입 (새 기술)
   **사유**:
   - Spike 필요 (연결 설정, 성능 측정)
   - 캐싱 전략 설계 → ADR 필수
   - 장애 처리 (Redis 다운 시)

   L 사이즈로 진행하시겠습니까? [Y/n/Skip]

3. 사용자 선택:
   - Y: 승격된 사이즈로 진행
   - n: 원래 사이즈 유지 (리스크 인지 확인)
   - Skip: 자동 승격 무시 (전문가 판단)
```

**예외 처리**:
- 사용자가 "n" 선택 시: 리스크 경고 표시 후 진행
- "Skip" 선택 시: 책임은 사용자에게 (문서화 권장)

### 🔄 사이즈별 워크플로우

#### S (Small): 인라인 구현 + 최소 문서화
```
1. AI 사이즈 판단 → S 추천 → 사용자 승인
2. PR 생성 시 5줄 템플릿 작성 (지식 단절 방지)
3. 바로 코드 구현 (Domain → Infrastructure → Application → API)
4. Unity 문서 업데이트 (기존 API_SPEC.md에 추가)
```

**S 사이즈 PR 템플릿** (5줄, 작성 시간 3분 이내):
- **Context**: 왜 필요한가? (비즈니스 목적)
- **Changes**: 무엇을 변경했나? (핵심 변경사항)
- **Alternative**: 고려한 대안은? (간단히, 없으면 "해당 없음")
- **Risk**: 잠재 위험은? (없으면 "없음")
- **Verification**: 어떻게 검증했나? (테스트/수동 확인)

#### M (Medium): Spec Lite
```
1. AI 사이즈 판단 → M 추천 → 사용자 승인
2. spec-lite.md 생성 (Intent, API, Data Model, Logic, Tests)
3. [조건부] Spike (0-1개) 또는 ADR (간단한 경우 spec-lite.md 내 "Decisions" 섹션)
4. 사용자 승인
5. 구현 → Unity 문서 → Self-Review (10개 항목)
```

#### L (Large): Full Spec
```
1. AI 사이즈 판단 → L 추천 → 사용자 승인
2. Requirements: requirements.md 생성 → 사용자 승인
3. [조건부] Spike: 최대 3개 (1 Spike = 1 Question, 승인 필수)
4. Design: design.md 생성 → Decision Log (Spike/ADR 링크) → Self-Review (10개) → 승인
5. Tasks: tasks.md 생성 → 품질 검증 (5개) → 승인
6. Implementation: Task 단위 실행 → Unity 문서
```

**Spec 위치** (L 사이즈):
- `.claude/memories/specs/{feature-name}/requirements.md`
- `.claude/memories/specs/{feature-name}/design.md`
- `.claude/memories/specs/{feature-name}/tasks.md`

### 📐 Design vs Implementation 경계

> ⚠️ **핵심 원칙**: Design은 "무엇을(What)" 정의, Implementation은 "어떻게(How)" 구현

**Design 단계 (명세)**:
- **데이터 모델**: 필드 목록, 제약사항, 관계, 인덱스 요구사항
- **API 설계**: 엔드포인트, Request/Response 구조, 에러 조건
- **비즈니스 로직**: 알고리즘 흐름, 계산식 (의사코드)
- **아키텍처**: 계층별 책임, 컴포넌트 관계

**Implementation 단계 (코드)**:
- **SQL DDL**: CREATE TABLE, ALTER TABLE, 상세 타입 (varchar(100), serial)
- **C# 코드**: 클래스 정의, 메서드 구현, 구체적 로직
- **설정 파일**: appsettings.json, Fluent API 구성

**왜 분리하는가?**:
- ✅ **문서 가독성**: 핵심 설계 의도에 집중
- ✅ **유연성**: 구현 변경 시 문서 수정 불필요
- ✅ **학습 효과**: Implementation에서 세부 결정 경험 (varchar vs text, serial vs uuid)
- ✅ **Clean Architecture**: 상위(Design)가 하위(구체적 코드) 디테일에 의존 금지

---

### 🔬 Spike & ADR

#### Spike (기술 검증) - 조건부 실행
**트리거** (하나라도 충족 시):
- ✅ 새 기술/라이브러리 첫 도입 (SignalR, Redis)
- ✅ 성능 검증 필요 (측정 필요)
- ✅ 2개 이상 기술 대안 비교 실험
- ✅ 외부 서비스 연동 테스트
- ❌ 문서 조사 (Spike 불필요)

**완화 방안**:
- **Spike Question 승인 프로세스** (필수): AI가 바로 실행하지 않고 사용자 승인 필요
- **1 Spike = 1 Question 원칙**: 여러 질문은 별도 Spike로 분리
- **개수 제한**: S (0개), M (0-1개), L (최대 3개)

**위치**: `docs/spikes/YYYY-MM/spike-xxx.md`

#### ADR (아키텍처 결정 기록) - 조건부 작성
**트리거** (하나라도 충족 시):
1. 새 외부 서비스/SDK/라이브러리 도입
2. 데이터 모델/스키마 마이그레이션 (비호환 변화)
3. 배포/인프라 변경
4. 보안/권한 모델 영향
5. 성능 제약 또는 리소스 비용 영향
6. 크로스컷팅 (캐싱, 로깅, 동시성) 도입

**완화**: 500단어 제한, 템플릿 고정
**위치**: `docs/adr/ADR-XXXX-topic.md`
**통합**: M (spec-lite.md "Decisions" 섹션), L (design.md "Decision Log" 테이블)

### 🎓 학습 프로젝트 특화 규칙

**TODO(human) 재정의**:
- ✅ **아키텍처 학습**: "이 로직은 Domain Service vs Application Service?"
- ✅ **설계 패턴**: "Repository 패턴 vs CQRS?"
- ✅ **데이터 모델링**: "1:N vs M:N 관계?"
- ❌ **게임 밸런스**: AI가 기본값 제안 (Legendary 1%, 크리스탈 100개)

**AI 제안 vs TODO(human)**:
- **게임 밸런스** (확률, 보상, 비용) → AI 자동 제안
- **아키텍처 결정** (계층 분리, 패턴 선택) → TODO(human)
- **비즈니스 로직** (계산식, 알고리즘) → AI 제안, 로직 위치는 TODO(human)

### ✅ 새 기능 시작 체크리스트

#### S (Small):
- [ ] PR 생성 및 5줄 템플릿 작성
- [ ] 바로 구현 (Domain → Infrastructure → Application → API)
- [ ] Unity 문서 업데이트

#### M (Medium):
- [ ] spec-lite.md 작성 (단일 파일)
- [ ] [조건부] Spike/ADR
- [ ] **[필수] `/spec-review` 품질 검증 → 70점 이상 필요**
- [ ] 사용자 승인
- [ ] 구현 → Unity 문서

#### L (Large):
- [ ] requirements.md 작성 및 승인
- [ ] [조건부] Spike (최대 3개)
- [ ] design.md 작성 및 **[필수] `/spec-review` 품질 검증 (70점 이상)**
- [ ] 사용자 승인
- [ ] tasks.md 작성 및 **[필수] `/spec-review` 최종 검증 (90점 이상)**
- [ ] 사용자 승인
- [ ] Task 실행 (/spec-execute)

### 📚 Commands

#### Spec 생성
- `/spec-init {feature-name}` - Requirements 생성 (L 사이즈)
- `/spec-init-lite {feature-name}` - spec-lite.md 생성 (M 사이즈)
- `/spec-design {feature-name}` - Design 생성
- `/spec-tasks {feature-name}` - Tasks 생성

#### Spec 실행 & 관리
- `/spec-execute {feature-name} {task-id}` - Task 실행
- `/spec-review {feature-name} [--format summary|detailed]` - 품질 검증 및 상세 보고서
- `/spec-status {feature-name}` - 기본 상태 조회
- `/spec-list` - 전체 Spec 목록

## Reference

### 시스템 문서 (_system/)
- **Architecture**: `.claude/memories/_system/architecture.md` - Clean Architecture 패턴
- **Game Design**: `.claude/memories/_system/game-design.md` - 게임 시스템 우선순위
- **Tech Stack**: `.claude/memories/_system/tech-stack.md` - 기술 스택
- **API Standards**: `.claude/memories/_system/api-standards.md` - 개발 표준
- **Roadmap**: `.claude/memories/_system/roadmap.md` - 프로젝트 진행 상황 (20개 시스템)

### Kiro 워크플로우
- **Commands**: `.claude/commands/spec-*.md` (8개)
- **Templates**: `.claude/memories/kiro-system-templates/` (requirements, design, tasks, spec-lite, ADR, spike)

### 기타
- **Learning Plan**: `docs/learning/PROJECT_ROADMAP.md` - 8주 학습 전략 (상세)
- **Deployment**: `docs/jenkins/DEPLOYMENT_GUIDE.md`
- **Unity Docs**: `docs/unity/UNITY_DOCUMENTATION_GUIDE.md`
- **PRD**: `docs/MUSHROOM_GAME_PRD.md`