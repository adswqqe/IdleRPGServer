# Self-Review Checklist for AI-Generated Specs

**Purpose**: AI가 spec 문서(requirements.md, design.md, spec.md)를 생성한 후, 스스로 품질을 검증하고 누락된 부분이 없는지 확인하는 체크리스트입니다.

**Usage**: Spec 작성 완료 후, 이 10개 항목을 모두 확인하고 통과해야 사용자에게 "승인 준비 완료" 상태로 제시합니다.

---

## 10-Item Checklist

### 1. 요구사항 추적성 (Requirement Traceability)
- [ ] 모든 설계 항목이 `requirements.md`의 특정 요구사항(US-1, AC-2 등)과 명확히 연결되는가?
- [ ] "왜 이 API가 필요한가?"에 대한 답이 requirements에 있는가?

**예시 체크**:
```markdown
✅ Good:
Design: "POST /api/characters/{id}/name endpoint"
→ Requirements: "US-1: As a player, I want to change character name"

❌ Bad:
Design에 API만 나열, requirements와 연결고리 없음
```

---

### 2. Clean Architecture 준수 (Architecture Compliance)
- [ ] `Domain` 계층이 다른 계층(Application, Infrastructure)에 의존하지 않는가?
- [ ] `architecture.md`에 정의된 의존성 규칙을 지켰는가?
- [ ] Pure Business Logic이 Domain에 있고, Infrastructure 세부사항이 없는가?

**예시 체크**:
```csharp
✅ Good (Domain):
public class Character
{
    public void ChangeName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new DomainException("Name cannot be empty");
        Name = newName;
    }
}

❌ Bad (Domain에 EF Core 참조):
public class Character
{
    [Required] // ← Infrastructure 의존
    public string Name { get; set; }
}
```

---

### 3. API 계약 정의 (API Contract)
- [ ] 엔드포인트, HTTP 메서드, Request/Response DTO가 명확히 정의되었는가?
- [ ] `api-standards.md`의 네이밍 컨벤션을 따르는가?
- [ ] RESTful 원칙을 준수하는가? (GET: 조회, POST: 생성, PUT: 전체 수정, PATCH: 부분 수정, DELETE: 삭제)

**예시 체크**:
```markdown
✅ Good:
PUT /api/characters/{id}/name
Request: { "newName": "string" }
Response: { "characterId": "guid", "name": "string" }

❌ Bad:
POST /api/updateCharacterName (RESTful 위반)
Request/Response 정의 누락
```

---

### 4. 데이터 모델 정의 (Data Model)
- [ ] Entity, 속성, 관계(1:N, M:N), 제약 조건(Not Null, Unique)이 명시되었는가?
- [ ] EF Core Configuration이 필요한 복잡한 관계가 식별되었는가?
- [ ] 인덱스(Index)가 쿼리 성능을 위해 정의되었는가?

**예시 체크**:
```markdown
✅ Good:
Entity: Character
- Id (Guid, PK)
- Name (string, Not Null, Max 20, Unique per UserId)
- Level (int, Default 1)
Relationship: User 1:N Character
Index: IX_Character_UserId (for query optimization)

❌ Bad:
Entity 이름만 나열, 제약조건/관계/인덱스 누락
```

---

### 5. 인증 및 권한 (Auth & Authz)
- [ ] 각 API에 필요한 인증 여부가 정의되었는가?
- [ ] 권한(Role)이 필요한 경우 명시되었는가? (예: `[Authorize(Roles="Admin")]`)
- [ ] 리소스 소유권 검증이 필요한 경우 식별되었는가? (예: 자신의 캐릭터만 수정 가능)

**예시 체크**:
```markdown
✅ Good:
PUT /api/characters/{id}/name
Authorization: Bearer token required
Role: Player (본인 캐릭터만 수정 가능)
Validation: userId from token == character.userId

❌ Bad:
Authorization 언급 없음
```

---

### 6. 유효성 검사 (Validation)
- [ ] Request DTO의 각 필드에 대한 유효성 검사 규칙이 포함되었는가?
- [ ] FluentValidation 또는 Data Annotations로 구현 가능하도록 명시되었는가?

**예시 체크**:
```markdown
✅ Good:
Request DTO: ChangeCharacterNameRequest
- NewName (string, Required, Length 2-20, No special chars)

Validation Rules:
- RuleFor(x => x.NewName).NotEmpty().Length(2, 20).Matches("^[a-zA-Z0-9]+$")

❌ Bad:
Request DTO만 정의, 유효성 규칙 누락
```

---

### 7. 에러 처리 (Error Handling)
- [ ] 실패 시나리오(404 Not Found, 400 Bad Request, 409 Conflict 등)가 정의되었는가?
- [ ] 각 에러 상황에서 반환될 에러 메시지가 명시되었는가?
- [ ] 비즈니스 규칙 위반 시 적절한 상태 코드(409 Conflict)를 사용하는가?

**예시 체크**:
```markdown
✅ Good:
Error Responses:
- 400 Bad Request: "Name must be 2-20 characters"
- 401 Unauthorized: "Invalid or missing token"
- 404 Not Found: "Character not found"
- 409 Conflict: "Character name already exists for this user"

❌ Bad:
에러 처리 언급 없음 또는 "에러 발생 시 400 반환" (불명확)
```

---

### 8. 트랜잭션 경계 (Transaction Boundary)
- [ ] 여러 DB 작업을 하나의 원자적 단위로 묶어야 하는 로직이 식별되었는가?
- [ ] 재화 차감, 아이템 지급 등 중요한 작업의 트랜잭션 처리가 명시되었는가?

**예시 체크**:
```markdown
✅ Good:
@Transactional:
1. Deduct 100 gold from character
2. Grant item to character inventory
3. Log transaction
→ 실패 시 전체 롤백

❌ Bad:
재화 차감과 아이템 지급이 별도 작업으로 표시 (원자성 보장 없음)
```

---

### 9. 비기능적 요구사항 (Non-Functional Requirements)
- [ ] 로깅이 필요한 주요 작업이 식별되었는가?
- [ ] 성능 고려사항(대규모 데이터, 응답 시간)이 언급되었는가?
- [ ] 보안 고려사항(민감 데이터 처리)이 포함되었는가?

**예시 체크**:
```markdown
✅ Good:
Logging:
- Log character name change (Info level)
- Log validation failures (Warning level)

Performance:
- Name uniqueness check: Index scan (< 10ms expected)

Security:
- Do not log sensitive user data (email, password)

❌ Bad:
비기능 요구사항 언급 없음
```

---

### 10. Unity 문서화 계획 (Unity Documentation Plan)
- [ ] API 변경에 따른 Unity 클라이언트 문서 업데이트 계획이 포함되었는가?
- [ ] `../IdleRPGClient/Docs/unity/[feature]/` 경로가 명시되었는가?
- [ ] `API_SPEC.md`와 `DTOs.cs`가 계획에 포함되었는가? (`CLAUDE.md` 규칙)

**예시 체크**:
```markdown
✅ Good:
Unity Documentation:
- Create: ../IdleRPGClient/Docs/unity/character-management/API_SPEC.md
- Create: ../IdleRPGClient/Docs/unity/character-management/DTOs.cs
- Update: ../IdleRPGClient/Docs/unity/README.md (add index entry)

DTOs.cs Content:
[JsonObject(MemberSerialization.OptIn)]
public class ChangeCharacterNameRequest { ... }

❌ Bad:
Unity 문서화 계획 누락
```

---

## Usage Workflow

### Step 1: Spec 작성 완료
AI가 `requirements.md`, `design.md`, 또는 `spec.md` 작성 완료

### Step 2: Self-Review 실행
```markdown
Self-Review 실행 중...

✅ 1. 요구사항 추적성: Passed
✅ 2. Clean Architecture: Passed
✅ 3. API 계약 정의: Passed
⚠️ 4. 데이터 모델 정의: Warning - Index 정의 누락
✅ 5. 인증 및 권한: Passed
...
```

### Step 3: 수정 및 재검토
누락된 항목 수정 후 재검토

### Step 4: 사용자 승인 요청
```markdown
Self-Review 완료: 10/10 항목 통과 ✅

이제 사용자 승인 준비가 완료되었습니다.
승인하시겠습니까? [Y/n]
```

---

## Notes

- **통과 기준**: 10개 항목 중 최소 9개 통과 (1개 Warning 허용)
- **Critical 항목**: 2, 5, 8번은 반드시 통과 (Architecture, Auth, Transaction)
- **주기적 갱신**: 프로젝트 진화에 따라 체크리스트 항목 조정
