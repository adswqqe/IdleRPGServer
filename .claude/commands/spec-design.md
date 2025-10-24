# /spec-design - Design 문서 생성

**목적**: 승인된 Requirements를 기반으로 기술 설계 문서를 생성합니다.

---

## 실행 절차

### 1. 전제 조건 확인
- `requirements.md`가 존재하는지 확인
- Requirements 승인 여부 확인:
  ```markdown
  ## Approval
  - [x] Requirements reviewed by team
  - [x] Business value confirmed
  **Approved by**: Development Team
  **Date**: YYYY-MM-DD
  ```
- 승인되지 않았으면 에러 메시지 출력 후 중단

### 2. 시스템 문서 참조
다음 문서들을 읽어서 설계 컨텍스트 파악:
- `.claude/memories/_system/architecture.md` - Clean Architecture 계층
- `.claude/memories/_system/game-design.md` - 서버 권위 원칙, 엔티티 관계
- `.claude/memories/_system/tech-stack.md` - 사용 가능한 라이브러리
- `.claude/memories/_system/api-standards.md` - API 설계 표준

### 3. [조건부] Spike 실행 (트리거 충족 시)
AI가 Spike 필요성 감지 → Spike Question 승인 요청:

**트리거**: 새 기술 도입, 성능 검증, 2개 이상 기술 선택, 외부 서비스 연동

**Spike 제안 예시**:
```
📋 Spike Proposal
Question: EF Core Include vs Select 성능 비교 (10만 건 데이터)?
Why: design.md 성능 최적화 전략 결정 필요
Method: 부하 테스트 (10만 건 데이터 쿼리)
Expected: 20분

실행하시겠습니까? [Y/n/Skip]
```

**Spike 결과**:
- `docs/spikes/YYYY-MM/spike-xxx.md` 생성
- Go/No-Go 판정 → ADR 작성 입력값

### 4. 템플릿 기반 초안 작성
`.claude/memories/kiro-system-templates/design-template.md`를 기반으로 `design.md` 생성:

> ⚠️ **중요**: Design은 **개념적 명세만** 작성 (SQL DDL, C# 코드 ❌)
> 참고: [CLAUDE.md - Design vs Implementation 경계](../../../CLAUDE.md#design-vs-implementation-경계)

**포함 내용** (개념적 명세):
- **Architecture Overview**: API/Application/Domain/Infrastructure 계층별 책임
- **Data Model**:
  - ✅ 필드 목록, 제약사항, 관계, 인덱스 요구사항
  - ❌ SQL DDL (CREATE TABLE, ALTER TABLE 등)
  - ❌ 구체적 타입 (varchar(100), serial 등)
- **API Design**:
  - ✅ 엔드포인트, Request/Response 구조, 에러 조건
  - ❌ JSON 샘플은 OK, 하지만 구체적 DTO 클래스 정의는 ❌
- **Business Logic**:
  - ✅ 알고리즘 흐름, 계산식 (의사코드)
  - ❌ 구체적 메서드 구현
  - **TODO(human)**: 아키텍처 결정 (로직 위치, 패턴 선택)
- **Service Layer Design**:
  - ✅ 메서드 시그니처, 책임, 프로세스 흐름
  - ❌ C# 코드 블록
- **Testing Strategy**: Unit/Integration 테스트 계획
- **Error Handling**: Exception Types, Error Response Format
- **Security Considerations**: Authentication, Authorization
- **Performance Considerations**: Indexes, Caching, N+1 Prevention
- **Migration Plan**:
  - ✅ 마이그레이션 요구사항 (어떤 테이블, 어떤 변경)
  - ❌ Idempotent SQL 코드
- **Decision Log** ⭐ **L 사이즈 필수**: Spike/ADR 링크 테이블
- **Unity Client Integration**: Unity 문서화 필요 항목

### 5. [조건부] ADR 작성 (아키텍처 영향 결정 발생 시)
중요한 아키텍처 결정 발생 시 ADR 작성:

**트리거**: 새 기술 도입, 데이터 모델 변경, 인프라 변경, 보안/성능/크로스컷팅 결정

**ADR 작성 예시**:
```
ADR-0005: SignalR 채택 결정 (vs Polling vs WebSocket Raw)

Context: 1000명 동시 채팅 요구사항
Decision: SignalR 채택
Alternatives: Polling (실시간성 부족), WebSocket Raw (복잡도 높음)
Consequences: ✅ 실시간 통신, ⚠️ Redis Backplane 필요

파일: docs/adr/ADR-0005-signalr-adoption.md
```

**design.md Decision Log에 링크**:
```markdown
## Decision Log

| ID | Decision | ADR Link | Spike Link | Status |
|----|----------|----------|------------|--------|
| D1 | SignalR 채택 | [ADR-0005](../../docs/adr/ADR-0005-signalr.md) | [Spike-001](../../docs/spikes/2025-10/signalr-perf.md) | Accepted |
```

### 6. Requirements 추적성 확인
- 모든 User Story (US-1, US-2 등)가 Design에 매핑되었는지 확인
- 각 섹션에 `**Requirements**: [US-X]` 표기

### 7. Self-Review Checklist 실행
`.claude/memories/kiro-system-templates/self-review-checklist.md`의 10개 항목 검증:

**검증 항목**:
1. ✅ 요구사항 추적성: 모든 설계 항목이 requirements.md의 US-X와 연결?
2. ✅ **Clean Architecture 준수 (CRITICAL)**: Domain이 다른 계층에 의존하지 않음?
3. ✅ API 계약 정의: 엔드포인트, Request/Response DTO 명확?
4. ✅ 데이터 모델 정의: Entity, 관계, 제약조건, 인덱스 명시?
5. ✅ **인증 및 권한 (CRITICAL)**: 각 API의 인증 여부, Role 정의?
6. ✅ 유효성 검사: Request DTO 필드별 유효성 규칙 포함?
7. ✅ 에러 처리: 실패 시나리오(404, 400, 409)와 에러 메시지 정의?
8. ✅ **트랜잭션 경계 (CRITICAL)**: 여러 DB 작업의 원자성 보장?
9. ✅ 비기능적 요구사항: 로깅, 성능, 보안 고려사항 포함?
10. ✅ Unity 문서화 계획: API_SPEC.md, DTOs.cs 계획 포함?

**통과 기준**:
- 최소 9/10 항목 통과
- **CRITICAL 3개 항목 (#2, #5, #8)은 필수 통과**
- 통과하지 못하면 design.md 수정 후 재검증

### 8. 출력
```
✅ {feature-name} design created

Created:
- .claude/memories/specs/{feature-name}/design.md
- [조건부] docs/spikes/YYYY-MM/spike-xxx.md (Spike 실행 시)
- [조건부] docs/adr/ADR-XXXX-topic.md (ADR 작성 시)

Design includes:
- 4 layers (API/Application/Domain/Infrastructure)
- 3 database tables (Entity, Template, History)
- 2 API endpoints
- TODO(human): 1개 (아키텍처 학습 포인트)
- Decision Log: 2개 결정 (Spike/ADR 링크)

Spike/ADR:
- Spike-001: ✅ Go (성능 요구사항 충족)
- ADR-0005: SignalR 채택 결정 (Accepted)

Requirements coverage:
- US-1: ✅ Covered in API/Data Model
- US-2: ✅ Covered in Business Logic
- US-3: ✅ Covered in Service Layer

📋 Self-Review Checklist: 10/10 통과 ✅
- #1 요구사항 추적성: ✅ Passed
- #2 Clean Architecture (CRITICAL): ✅ Passed
- #3 API 계약 정의: ✅ Passed
- #4 데이터 모델 정의: ✅ Passed
- #5 인증/권한 (CRITICAL): ✅ Passed
- #6 유효성 검사: ✅ Passed
- #7 에러 처리: ✅ Passed
- #8 트랜잭션 경계 (CRITICAL): ✅ Passed
- #9 비기능 요구사항: ✅ Passed
- #10 Unity 문서화 계획: ✅ Passed

✨ 모든 CRITICAL 항목 통과! 사용자 승인 준비 완료.

Next steps:
1. Review design.md
2. Review Spike/ADR 결과 (해당 시)
3. Decide TODO(human) 아키텍처 학습 포인트
4. 게임 밸런스 AI 제안값 확인
5. Mark approval section
6. Run: /spec-tasks {feature-name}
```

---

## 주의사항

- ⚠️ **Requirements 승인 필수**: 승인되지 않으면 실행 중단
- ⚠️ **모든 Requirements 커버**: US-1, US-2 등 모두 Design에 반영
- ⚠️ **TODO(human)은 아키텍처 학습 포인트만**: 게임 밸런스는 AI 제안
- ⚠️ **Self-Review 필수**: 10개 항목 중 최소 9개 통과, CRITICAL 3개 필수
- ⚠️ **Unity 문서화 계획 포함**: API/DTO 변경 시 필수

---

## 예시

### 입력
```
/spec-design pet-system
```

### 에러 케이스
```
❌ Requirements not approved

Please approve requirements.md first:
1. Review .claude/memories/specs/pet-system/requirements.md
2. Decide all TODO(human) items
3. Mark approval section:
   - [x] Requirements reviewed by team
   **Approved by**: Your Name
   **Date**: 2025-10-23

Then run: /spec-design pet-system
```

### 성공 케이스
```
✅ pet-system design created

Created:
- .claude/memories/specs/pet-system/design.md

Design includes:
- Database: Pet, PetTemplate, CharacterPet tables
- API: POST /api/pets/gacha, GET /api/pets
- Business Logic (AI 제안): Pet 가챠 확률 (Legendary 1%, Epic 9%, ...), 스탯 버프 공식 (Attack * 1.05)
- TODO(human): 아키텍처 학습 포인트 (버프 계산: Domain Service? Application Service?)

📋 Self-Review Checklist: 10/10 통과 ✅
- Critical 항목 (#2 Clean Architecture, #5 인증, #8 트랜잭션) 모두 통과

Next steps:
1. Review design.md
2. Decide TODO(human): 버프 계산 로직 위치 (Domain vs Application)
3. 게임 밸런스 AI 제안값 확인
4. **[필수] Run: /spec-review pet-system (목표: 70점 이상)**
5. Mark approval
6. Run: /spec-tasks pet-system
```
