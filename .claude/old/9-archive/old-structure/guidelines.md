# 개발 가이드라인

## 보안 원칙

### 1. 비밀번호 보안
```csharp
// ✅ 올바른 방식
var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);

// ❌ 절대 금지
var plainPassword = password;  // 평문 저장 금지
```

### 2. JWT 토큰 관리
- **Access Token**: 15분 (짧게)
- **Refresh Token**: 7일 (길게)
- **Token Rotation**: Refresh 시 새 Refresh Token 발급
- **HttpOnly Cookie**: XSS 방지 (선택)

### 3. SQL Injection 방지
```csharp
// ✅ EF Core 사용 (자동 파라미터화)
var player = await _context.Players
    .FirstOrDefaultAsync(p => p.Username == username);

// ❌ Raw SQL 직접 조합 금지
var sql = $"SELECT * FROM Players WHERE Username = '{username}'";
```

### 4. 민감 정보 로깅 금지
```csharp
// ❌ 금지
_logger.LogInformation($"User password: {password}");
_logger.LogInformation($"JWT Token: {token}");

// ✅ 허용
_logger.LogInformation($"User {username} logged in");
_logger.LogError($"Login failed for user {username}");
```

---

## 비동기 프로그래밍

### 원칙

1. **모든 DB 작업은 `async/await`**
```csharp
public async Task<Player> GetPlayerAsync(Guid id)
{
    return await _context.Players.FindAsync(id);
}
```

2. **메서드 네이밍: `Async` 접미사**
```csharp
GetPlayerAsync, CreateCharacterAsync, UpdateEquipmentAsync
```

3. **CancellationToken 전달 (장기 작업)**
```csharp
public async Task<List<Player>> GetAllPlayersAsync(CancellationToken cancellationToken = default)
{
    return await _context.Players.ToListAsync(cancellationToken);
}
```

4. **`ConfigureAwait(false)` (라이브러리에서)**
```csharp
// 라이브러리 코드에서만 (API는 필요 없음)
var result = await SomeMethodAsync().ConfigureAwait(false);
```

---

## 에러 핸들링

### HTTP 상태 코드

- **200 OK**: 성공 (GET, PUT, DELETE)
- **201 Created**: 생성 성공 (POST)
- **400 Bad Request**: 잘못된 요청 (검증 실패)
- **401 Unauthorized**: 인증 실패 (로그인 필요)
- **403 Forbidden**: 권한 없음
- **404 Not Found**: 리소스 없음
- **500 Internal Server Error**: 서버 오류

### 예외 처리 패턴

```csharp
public async Task<IActionResult> GetCharacter(Guid id)
{
    try
    {
        var character = await _service.GetCharacterAsync(id);

        if (character == null)
            return NotFound(new { Message = "캐릭터를 찾을 수 없습니다." });

        return Ok(character);
    }
    catch (UnauthorizedAccessException ex)
    {
        _logger.LogWarning(ex, "Unauthorized access attempt for character {CharacterId}", id);
        return Unauthorized(new { Message = "권한이 없습니다." });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting character {CharacterId}", id);
        return StatusCode(500, new { Message = "서버 오류가 발생했습니다." });
    }
}
```

**원칙**:
- 사용자에게는 간단한 메시지
- 상세 정보는 로그에만 기록

---

## 테스트 작성 원칙

### AAA 패턴

```csharp
[Fact]
public async Task GetCharacter_ValidId_ReturnsCharacter()
{
    // Arrange (준비)
    var characterId = Guid.NewGuid();
    _mockRepository.Setup(r => r.GetByIdAsync(characterId))
        .ReturnsAsync(new Character { Id = characterId, Name = "TestChar" });

    // Act (실행)
    var result = await _service.GetCharacterAsync(characterId);

    // Assert (검증)
    result.Should().NotBeNull();
    result.Id.Should().Be(characterId);
    result.Name.Should().Be("TestChar");
}
```

### Theory & InlineData

```csharp
[Theory]
[InlineData(0, SkillRarity.Legendary)]
[InlineData(5, SkillRarity.Epic)]
[InlineData(20, SkillRarity.Rare)]
[InlineData(50, SkillRarity.Common)]
public void DetermineRarity_RandomValues_ReturnsCorrectRarity(int randomValue, SkillRarity expected)
{
    // Arrange
    _mockRandomProvider.Setup(r => r.Next(100)).Returns(randomValue);

    // Act
    var result = _service.DetermineRarity(0);

    // Assert
    result.Should().Be(expected);
}
```

### Mock 사용

```csharp
// Repository Mock
var mockRepository = new Mock<IPlayerRepository>();
mockRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
    .ReturnsAsync(new Player { Id = Guid.NewGuid() });

// Verify 호출 검증
_mockRepository.Verify(r => r.AddAsync(It.IsAny<Player>()), Times.Once);
```

### FluentAssertions

```csharp
result.Should().NotBeNull();
result.Should().BeOfType<Player>();
result.Id.Should().Be(expectedId);
result.Name.Should().Be("TestPlayer");
list.Should().HaveCount(5);
list.Should().Contain(item => item.Level > 10);
```

---

## 데이터베이스 설계 원칙

### 1. 기본키 선택

**GUID 사용** (대부분):
```csharp
public Guid Id { get; set; }  // Player, Character, Equipment
```
- 분산 환경 대비
- 클라이언트에서 미리 생성 가능
- URL 노출 시 순차적 ID보다 안전

**Int 사용** (마스터 데이터):
```csharp
public int Id { get; set; }  // MonsterTemplate, SkillTemplate, DungeonStage
```
- 읽기 전용 데이터
- 성능 (인덱스 크기 작음)
- 순서 의미 있음

### 2. Navigation Properties

```csharp
public class Character
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }

    // Navigation Property
    public Player Player { get; set; }  // N:1
    public ICollection<Equipment> Equipments { get; set; }  // 1:N
}
```

**지연 로딩 주의**:
```csharp
// ❌ N+1 문제
foreach (var character in characters)  // 1 쿼리
{
    Console.WriteLine(character.Player.Name);  // N 쿼리
}

// ✅ Eager Loading
var characters = await _context.Characters
    .Include(c => c.Player)  // Join
    .ToListAsync();
```

### 3. Index 설정

```csharp
// Fluent API
modelBuilder.Entity<Player>()
    .HasIndex(p => p.Username)
    .IsUnique();

modelBuilder.Entity<Character>()
    .HasIndex(c => c.PlayerId);

modelBuilder.Entity<DungeonStage>()
    .HasIndex(d => d.RequiredLevel);
```

**인덱스 전략**:
- **Unique Index**: Username, Email
- **Foreign Key**: 자동 인덱스 (대부분)
- **검색 조건**: WHERE 절에 자주 나오는 컬럼

### 4. Soft Delete

```csharp
public class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
}

// Query Filter
modelBuilder.Entity<Player>()
    .HasQueryFilter(p => !p.IsDeleted);
```

---

## API 설계 원칙

### 1. RESTful 설계

**리소스 기반 URL**:
```
GET    /api/characters           # 목록 조회
GET    /api/characters/{id}      # 단일 조회
POST   /api/characters           # 생성
PUT    /api/characters/{id}      # 수정
DELETE /api/characters/{id}      # 삭제
```

**동작 기반 엔드포인트** (예외):
```
POST /api/characters/{id}/experience   # 경험치 획득
POST /api/characters/{id}/levelup       # 레벨업
POST /api/battle/start                  # 전투 시작
POST /api/equipment/enhance             # 장비 강화
```

### 2. 응답 형식

**성공 응답**:
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "TestCharacter",
  "level": 10
}
```

**에러 응답**:
```json
{
  "message": "캐릭터를 찾을 수 없습니다.",
  "statusCode": 404
}
```

### 3. 페이징 (향후)

**요청**:
```
GET /api/characters?page=1&size=20&sort=level,desc
```

**응답**:
```json
{
  "data": [...],
  "page": 1,
  "size": 20,
  "total": 150,
  "totalPages": 8
}
```

---

## Unity 문서화 규칙

### 필수 업데이트 시점

**API/DTO 추가/수정 시 반드시 Unity 문서 업데이트** (CRITICAL)

### 체크리스트

1. **폴더 생성**: `../IdleRPGClient/Docs/unity/{feature}/`
2. **API 명세**: `{feature}/API_SPEC.md`
   - 엔드포인트별 Request/Response 예제
   - Unity C# 코드 예제 포함
3. **DTO 작성**: `{feature}/DTOs.cs`
   - JsonProperty 어트리뷰트
   - Newtonsoft.Json 기준
4. **메인 인덱스**: `unity/README.md` 업데이트
   - 구현 상태 테이블에 엔드포인트 추가

---

## 코드 컨벤션

### 네이밍

**PascalCase**:
- 클래스, 메서드, 프로퍼티
- `PlayerService`, `GetCharacterAsync`, `CharacterId`

**camelCase**:
- 로컬 변수, 파라미터
- `var characterId`, `string username`

**_camelCase**:
- Private 필드
- `private readonly IPlayerRepository _repository;`

### 파일 구조

**엔티티**:
```
Domain/Entities/Character.cs
```

**DTO**:
```
Application/DTOs/Characters/CharacterDto.cs
Application/DTOs/Characters/CreateCharacterDto.cs
```

**서비스**:
```
Application/Services/ICharacterService.cs
Infrastructure/Services/CharacterService.cs
```

### 주석

**XML 주석** (Public API):
```csharp
/// <summary>
/// 캐릭터 ID로 캐릭터를 조회합니다.
/// </summary>
/// <param name="characterId">캐릭터 ID</param>
/// <returns>캐릭터 정보</returns>
public async Task<Character> GetCharacterAsync(Guid characterId) { }
```

**TODO 주석**:
```csharp
// TODO(human): 확률 공식 확정 후 구현
// TODO: Redis 캐싱 추가 (Week 6)
```
