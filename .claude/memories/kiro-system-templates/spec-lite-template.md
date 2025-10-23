# [Feature Name] - Lite Spec

**Status**: Draft | Approved
**Size**: M (Medium)
**Created**: YYYY-MM-DD
**Author**: [Your Name]

---

## 1. Intent & Scope

**What**: 이 기능이 무엇을 하는가? (1-2문장)

**Why**: 왜 필요한가? 어떤 문제를 해결하는가? (1-2문장)

**Scope**: 포함되는 것 / 포함되지 않는 것
- ✅ In Scope: ...
- ❌ Out of Scope: ...

---

## 2. User Stories & Acceptance Criteria

### User Story
**As a** [역할],
**I want** [기능],
**So that** [목적].

### Acceptance Criteria (EARS 형식)
- **AC-1**: WHEN [조건] THEN system SHALL [동작]
- **AC-2**: WHEN [조건] THEN system SHALL [동작]
- **AC-3**: WHEN [조건] THEN system SHALL [동작]

---

## 3. API Design

### Endpoint
```
[HTTP METHOD] /api/[resource]/[action]
Authorization: [Required Role]
```

### Request
```json
{
  "field1": "type (constraints)",
  "field2": "type (constraints)"
}
```

### Response (Success)
```json
{
  "field1": "type",
  "field2": "type"
}
```

### Error Responses
- **400 Bad Request**: Invalid input (validation failed)
- **401 Unauthorized**: Missing or invalid token
- **404 Not Found**: Resource not found
- **409 Conflict**: Business rule violation (e.g., duplicate name)

---

## 4. Data Model

### Entities
**[EntityName]**
- `Field1` (Type, constraints): 설명
- `Field2` (Type, constraints): 설명

**Relationships**:
- [Entity1] 1:N [Entity2]

**Indexes**:
- `IX_EntityName_Field1` (for query performance)

### Migration
- [ ] Add new column/table
- [ ] Update existing data (if needed)
- [ ] Idempotent migration pattern

---

## 5. Business Logic

### Core Logic (AI 제안)
```
1. Validate input
2. Check business rules (e.g., uniqueness, permissions)
3. Perform operation (update DB)
4. Return result
```

**계산식/확률 (AI 제안)**:
- [예: "보상 Gold = BaseGold * (1 + Level * 0.1)"]
- [예: "성공 확률 = 80% (고정값)"]

### Business Rules
- **BR-1**: [Rule description - AI가 일반적인 규칙 제안]
- **BR-2**: [Rule description]

### Transaction Boundary
```
@Transactional:
- Operation 1 (e.g., deduct currency)
- Operation 2 (e.g., grant item)
```

### 🎓 학습 포인트 (아키텍처 결정)
**TODO(human)**: 다음 아키텍처 결정이 필요합니다:
- [ ] [예: "이 로직은 Domain Service vs Application Service?"]
- [ ] [예: "트랜잭션 경계: Service Layer vs Repository?"]
- [ ] [예: "에러 핸들링: try-catch vs Result<T> 패턴?"]

> 💡 **학습 가이드**: 계산식은 AI가 제공합니다. **계층 분리**, **의존성 주입**, **트랜잭션 관리**에 집중하세요.
> 상세 규칙: [CLAUDE.md - 학습 프로젝트 특화 규칙](../../../CLAUDE.md#학습-프로젝트-특화-규칙)

---

## 6. Constraints & Risks

### Constraints
- 제약사항 1 (e.g., API rate limit: 10 req/min per user)
- 제약사항 2

### Known Risks
- 리스크 1: [설명] → Mitigation: [대응 방안]
- 리스크 2: [설명] → Mitigation: [대응 방안]

---

## 7. Test Strategy

### Unit Tests
- [ ] Service layer: Business logic validation
- [ ] Repository layer: Data access

### Integration Tests
- [ ] API endpoint: Request/Response validation
- [ ] Database: Transaction rollback

### Test Cases
- **TC-1**: Valid input → Success response
- **TC-2**: Invalid input → 400 Bad Request
- **TC-3**: Unauthorized access → 401 Unauthorized

---

## 8. Implementation Checklist

- [ ] Domain: Entity, Enum (AI 구현)
- [ ] Application: DTO, Service, Validator (AI 구현)
- [ ] Infrastructure: Repository, EF Configuration (AI 구현)
- [ ] API: Controller, Route (AI 구현)
- [ ] Database: Migration SQL (AI 작성)
- [ ] Tests: Unit + Integration (AI 작성)
- [ ] Unity Docs: API_SPEC.md, DTOs.cs
- [ ] 🎓 TODO(human): 학습 포인트 구현 (아키텍처 결정, SQL 인덱싱 전략 등)
- [ ] Self-Review: 10-item checklist passed

> 💡 **구현 순서**: Domain → Infrastructure → Application → API → Database → Tests (의존성 순서)

---

## 9. Unity Documentation Plan

- [ ] `../IdleRPGClient/Docs/unity/[feature]/API_SPEC.md`
- [ ] `../IdleRPGClient/Docs/unity/[feature]/DTOs.cs`
- [ ] Update `unity/README.md` index

---

## 10. Decisions (선택적)

> ⚠️ **사용 조건**: 아키텍처 영향이 있는 결정이 있을 때만 작성
>
> **ADR 필요 조건**: [CLAUDE.md - ADR (아키텍처 결정 기록)](../../../CLAUDE.md#spike--adr) 참조

### Decision 1: [간단한 제목]
- **Context**: 왜 이 결정이 필요했는가? (1-2문장)
- **Decision**: 무엇을 선택했는가?
- **Rationale**: 왜 이것을 선택했는가? (1-3문장)
- **ADR**: [ADR-XXX](../../docs/adr/ADR-XXX-topic.md) (복잡한 경우만)

### Decision 2: [제목] (필요 시)
- **Context**: ...
- **Decision**: ...
- **Rationale**: ...

---

💡 **가이드**:
- 간단한 결정: 이 섹션에 1-3문장으로 기록
- 복잡한 결정: ADR 별도 작성 후 링크
- 아키텍처 영향 없음: 이 섹션 생략
