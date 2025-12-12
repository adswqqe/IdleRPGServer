# 코드 컨벤션 및 스타일

## 네이밍 규칙

### C# 네이밍

**PascalCase**:
- 클래스명: `PlayerService`, `CharacterController`
- 메서드명: `GetCharacterAsync`, `CreatePlayer`
- 프로퍼티: `CharacterId`, `PlayerName`
- 상수: `MaxLevel`, `DefaultGoldPerHour`

**camelCase**:
- 로컬 변수: `var characterId`, `int level`
- 파라미터: `public void Method(string username, int level)`

**_camelCase** (Private 필드):
```csharp
private readonly IPlayerRepository _repository;
private readonly ILogger<PlayerService> _logger;
private readonly GachaLogicService _gachaService;
```

### 파일 네이밍

**엔티티**:
```
Domain/Entities/Character.cs
Domain/Entities/PlayerSkill.cs
```

**DTO**:
```
Application/DTOs/Characters/CharacterDto.cs
Application/DTOs/Characters/CreateCharacterDto.cs
Application/DTOs/Auth/LoginDto.cs
```

**서비스**:
```
Application/Services/ICharacterService.cs
Infrastructure/Services/CharacterService.cs
```

**레포지토리**:
```
Domain/Repositories/ICharacterRepository.cs
Infrastructure/Repositories/CharacterRepository.cs
```

---

## 코드 스타일

### 1. Using 정렬

```csharp
// System namespaces
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Microsoft namespaces
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

// Third-party namespaces
using FluentValidation;
using Newtonsoft.Json;

// Project namespaces
using IdleRPG.Domain.Entities;
using IdleRPG.Application.DTOs;
using IdleRPG.Application.Services;
```

### 2. 메서드 순서

```csharp
public class CharacterService
{
    // 1. Fields
    private readonly ICharacterRepository _repository;
    private readonly ILogger<CharacterService> _logger;

    // 2. Constructor
    public CharacterService(ICharacterRepository repository, ILogger<CharacterService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    // 3. Public Methods
    public async Task<Character> GetCharacterAsync(Guid id) { }
    public async Task<Character> CreateCharacterAsync(CreateCharacterDto dto) { }

    // 4. Private Methods
    private void ValidateCharacter(Character character) { }
    private int CalculateTotalStats(Character character) { }
}
```

### 3. 중괄호 스타일

```csharp
// ✅ 항상 새 줄에 중괄호
public void Method()
{
    if (condition)
    {
        DoSomething();
    }
    else
    {
        DoSomethingElse();
    }
}

// ❌ 한 줄로 줄이지 않기 (명확성 우선)
if (condition) DoSomething();
```

### 4. Null 체크

```csharp
// ✅ Null-coalescing operator
var name = character?.Name ?? "Unknown";

// ✅ Null-conditional operator
var level = character?.Level ?? 1;

// ✅ ArgumentNullException
public CharacterService(ICharacterRepository repository)
{
    _repository = repository ?? throw new ArgumentNullException(nameof(repository));
}
```

---

## 주석 규칙

### 1. XML 문서 주석

**Public API는 반드시 XML 주석**:
```csharp
/// <summary>
/// 캐릭터 ID로 캐릭터를 조회합니다.
/// </summary>
/// <param name="characterId">조회할 캐릭터의 고유 ID</param>
/// <param name="cancellationToken">취소 토큰</param>
/// <returns>캐릭터 정보</returns>
/// <exception cref="ArgumentException">characterId가 유효하지 않을 때</exception>
public async Task<Character> GetCharacterAsync(
    Guid characterId,
    CancellationToken cancellationToken = default)
{
    // ...
}
```

### 2. TODO 주석

```csharp
// TODO(human): 확률 공식 사용자와 확정 후 구현
// TODO: Redis 캐싱 추가 (Week 6 예정)
// FIXME: N+1 쿼리 문제 해결 필요
```

**TODO(human)** 규칙:
- 사용자와 함께 결정해야 할 비즈니스 로직
- 디자인 결정이 필요한 부분
- Claude가 독단적으로 구현하면 안 되는 부분

### 3. 인라인 주석

```csharp
// ✅ 복잡한 로직 설명
var damage = (attacker.Attack - defender.Defense) * Random(0.9, 1.1);  // 10% 편차

// ❌ 자명한 코드 주석 (불필요)
var level = 1;  // 레벨을 1로 설정
```

---

## 한국어 사용 규칙

### 1. 코드는 영어, 주석은 한국어

```csharp
// ✅ 좋은 예
public class Character  // 영어 클래스명
{
    /// <summary>
    /// 캐릭터 레벨 (한국어 주석)
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// 총 공격력을 계산합니다. (한국어 설명)
    /// </summary>
    public int CalculateTotalAttack()
    {
        // 장비 공격력 + 스킬 버프 (한국어 인라인 주석)
        return BaseAttack + EquipmentAttack + SkillBuffAttack;
    }
}
```

### 2. 에러 메시지는 한국어

```csharp
// ✅ 사용자 대상 메시지는 한국어
return BadRequest(new { Message = "캐릭터를 찾을 수 없습니다." });
return NotFound(new { Message = "이미 존재하는 사용자명입니다." });

// ✅ 로그는 영어 (검색/분석 용이)
_logger.LogError("Character not found: {CharacterId}", characterId);
_logger.LogWarning("Duplicate username attempt: {Username}", username);
```

### 3. Unity 문서는 한국어

```markdown
# 캐릭터 API 명세

## 캐릭터 생성
**Endpoint**: `POST /api/characters`

**Request**:
\`\`\`json
{
  "name": "전사",
  "level": 1
}
\`\`\`
```

---

## Git Commit 규칙

### Commit Message 형식

```
<type>(<scope>): <subject>

<body>

🤖 Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

### Type

- **feat**: 새 기능 추가
- **fix**: 버그 수정
- **refactor**: 리팩토링
- **test**: 테스트 추가/수정
- **docs**: 문서 수정
- **chore**: 빌드/설정 변경

### 예시

```
feat(character): Add character level-up system

- Implement auto stat increase on level up
- Add experience calculation formula
- Create CharacterService.LevelUpAsync method

🤖 Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

```
fix(dungeon): Fix difficulty unlock logic

- Change prerequisite from previous stage to same stage
- Update DungeonService.GetUnlockedDifficulties
- Add unit tests for unlock scenarios

🤖 Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

---

## 테스트 파일 구조

### 네이밍

```
IdleRPG.Tests/
├── Domain/
│   └── Services/
│       └── GachaLogicServiceTests.cs
├── Application/
│   └── Services/
│       └── CharacterServiceTests.cs
└── API/
    └── Controllers/
        └── CharacterControllerTests.cs
```

**규칙**: `{ClassName}Tests.cs`

### 테스트 메서드 네이밍

**패턴**: `{MethodName}_{Scenario}_{ExpectedResult}`

```csharp
[Fact]
public void DetermineRarity_PityCount100_ReturnsLegendary() { }

[Fact]
public void GetCharacter_InvalidId_ThrowsArgumentException() { }

[Theory]
[InlineData(0, SkillRarity.Legendary)]
[InlineData(50, SkillRarity.Common)]
public void DetermineRarity_RandomValues_ReturnsCorrectRarity(int random, SkillRarity expected) { }
```

---

## DTO 설계 규칙

### Request DTO

```csharp
// POST /api/characters
public class CreateCharacterDto
{
    [Required(ErrorMessage = "캐릭터 이름은 필수입니다.")]
    [StringLength(20, MinimumLength = 2, ErrorMessage = "이름은 2-20자여야 합니다.")]
    public string Name { get; set; }
}
```

### Response DTO

```csharp
public class CharacterDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public int Level { get; set; }
    public int HP { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### Unity DTO (Newtonsoft.Json)

```csharp
[Serializable]
public class CharacterDto
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("level")]
    public int Level { get; set; }
}
```

---

## 상수 관리

### 클래스 상수

```csharp
public class GachaLogicService
{
    private const int PITY_THRESHOLD = 100;
    private const int LEGENDARY_RATE = 1;
    private const int EPIC_RATE = 9;
    private const int RARE_RATE = 30;
    // Common은 나머지 60%
}
```

### Configuration (향후)

```csharp
// appsettings.json
{
  "GameBalance": {
    "GachaPityThreshold": 100,
    "MaxOfflineHours": 12,
    "GoldPerHour": 10
  }
}

// Service
public class GachaService
{
    private readonly GameBalanceSettings _settings;

    public GachaService(IOptions<GameBalanceSettings> settings)
    {
        _settings = settings.Value;
    }
}
```

---

## 폴더 구조 규칙

### 기능별 폴더링

```
Application/DTOs/
├── Auth/
│   ├── LoginDto.cs
│   ├── RegisterDto.cs
│   └── AuthResponseDto.cs
├── Characters/
│   ├── CharacterDto.cs
│   ├── CreateCharacterDto.cs
│   └── UpdateCharacterDto.cs
└── Equipments/
    ├── EquipmentDto.cs
    └── EnhanceEquipmentDto.cs
```

### 계층별 분리

- **Domain**: 순수 비즈니스 로직, 외부 의존 없음
- **Application**: DTO, 인터페이스 정의
- **Infrastructure**: 외부 라이브러리 의존 (EF Core, Redis)
- **API**: ASP.NET Core 의존
