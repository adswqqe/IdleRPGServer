# API 및 개발 표준

> **참조 목적**: 새 스펙 작성 시 코딩 컨벤션, API 설계, 보안, 테스트 표준을 참조합니다.

---

## Code Conventions

### Naming

- **PascalCase**: 클래스, 메서드, 프로퍼티
  - 예: `PlayerService`, `GetCharacterAsync`
- **camelCase**: 로컬 변수, 파라미터
  - 예: `var characterId`, `string username`
- **_camelCase**: Private 필드
  - 예: `private readonly IPlayerRepository _repository`

### File Structure

- `Domain/Entities/{Entity}.cs`
- `Application/DTOs/{Feature}/{Feature}Dto.cs`
- `Infrastructure/Repositories/{Entity}Repository.cs`
- `API/Controllers/{Entity}Controller.cs`

### Comments

- **XML 문서 주석**: Public API 필수
- **TODO(human)**: 사용자와 결정 필요한 비즈니스 로직

---

## Security Principles

### 인증 및 권한
- **비밀번호**: BCrypt (WorkFactor 12)
- **JWT**: Access 15분, Refresh 7일
- **권한 검증**: `[Authorize]` 속성 사용

### SQL Injection 방지
- **EF Core 파라미터화**: 항상 사용
- **Raw SQL 직접 조합 금지**: `FromSqlRaw($"... {변수}")` 절대 사용 금지
- **올바른 사용**: `FromSqlRaw("... WHERE Id = {0}", id)`

### 로깅
- **민감 정보 로깅 금지**: 비밀번호, 토큰, 개인정보
- **로그 수준**: Error, Warning, Information, Debug
- **구조화된 로깅**: Serilog 사용

---

## API Design

### RESTful 원칙

**Resource 중심 URL**:
- `GET /api/characters` - 목록 조회
- `GET /api/characters/{id}` - 단일 조회
- `POST /api/characters` - 생성
- `PUT /api/characters/{id}` - 전체 수정
- `PATCH /api/characters/{id}` - 부분 수정
- `DELETE /api/characters/{id}` - 삭제

**동작 중심 엔드포인트** (특수한 경우):
- `POST /api/characters/{id}/experience` - 경험치 추가
- `POST /api/equipment/enhance` - 장비 강화
- `POST /api/skills/gacha` - 스킬 가챠

### HTTP 상태 코드

- **200 OK**: 성공 (GET, PUT, PATCH)
- **201 Created**: 생성 성공 (POST)
- **204 No Content**: 성공, 응답 본문 없음 (DELETE)
- **400 Bad Request**: 잘못된 요청 (Validation 실패)
- **401 Unauthorized**: 인증 실패 (JWT 없음/만료)
- **403 Forbidden**: 권한 없음
- **404 Not Found**: 리소스 미존재
- **500 Internal Server Error**: 서버 오류

### 응답 형식

**성공 응답** (200, 201):
```json
{
  "id": "uuid",
  "name": "캐릭터 이름",
  "level": 10
}
```

**에러 응답** (400, 404, 500):
```json
{
  "message": "캐릭터를 찾을 수 없습니다.",
  "statusCode": 404
}
```

### DTO 설계

- **Request DTO**: `{Action}RequestDto.cs`
  - 예: `CreateCharacterRequestDto`, `SkillGachaRequestDto`
- **Response DTO**: `{Action}ResponseDto.cs`
  - 예: `CharacterDto`, `SkillGachaResponseDto`
- **Validation**: FluentValidation 또는 Data Annotations

---

## Testing Standards

### AAA 패턴

```csharp
[Fact]
public async Task GetCharacterAsync_ValidId_ReturnsCharacter()
{
    // Arrange
    var characterId = Guid.NewGuid();
    _mockRepository.Setup(r => r.GetAsync(characterId))
        .ReturnsAsync(new Character { Id = characterId });
    
    // Act
    var result = await _service.GetCharacterAsync(characterId);
    
    // Assert
    result.Should().NotBeNull();
    result.Id.Should().Be(characterId);
}
```

### Mock 사용 (Moq)

```csharp
_mockRepository.Setup(r => r.GetAsync(id)).ReturnsAsync(entity);
_mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Character>()), Times.Once);
```

### FluentAssertions

```csharp
result.Should().NotBeNull();
result.Id.Should().Be(expectedId);
result.Name.Should().BeEquivalentTo("Expected Name");
result.Items.Should().HaveCount(3);
```

### 테스트 네이밍

**패턴**: `{MethodName}_{Scenario}_{ExpectedResult}`

**예시**:
- `GetCharacterAsync_ValidId_ReturnsCharacter`
- `CreateCharacterAsync_DuplicateName_ThrowsException`
- `DrawSkillsAsync_InsufficientCrystal_ThrowsInvalidOperationException`

### 테스트 커버리지 목표

- **Domain Services**: 90%+ (핵심 비즈니스 로직)
- **Application Services**: 80%+
- **Controllers**: 70%+

---

## Feature Development Order

**필수 순서** (의존성):

1. **Domain Layer**: Entity, Enum, Domain Service, Value Object
2. **Application Layer**: DTO, Service Interface
3. **Infrastructure Layer**: Repository 구현, EF Core Configuration
4. **API Layer**: Controller
5. **Tests**: Unit Tests, Integration Tests

**이유**: Domain은 다른 계층에 의존하지 않으므로 먼저 작성

---

## Kiro 스펙 작성 시 참고

### Requirements 단계에서
- API 엔드포인트 정의 (RESTful vs 동작 중심)
- 보안 요구사항 (인증/권한)

### Design 단계에서
- DTO 설계 (Request/Response)
- HTTP 상태 코드 정의
- 에러 처리 방식

### Tasks 단계에서
- Feature Development Order 준수
- 테스트 작성을 별도 Task로 포함
- Swagger 주석 추가 Task 포함

### TODO(human) 기준
- 복잡한 Validation 규칙 (비즈니스 로직)
- 보안 정책 결정 (권한 레벨)
