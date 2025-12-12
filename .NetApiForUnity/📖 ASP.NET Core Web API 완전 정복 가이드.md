# ASP.NET Core Web API 완전 정복 가이드 - Unity 개발자를 위한 서버 개발 입문

Unity 개발자로서 서버 개발을 처음 시작하시는 것을 환영합니다! 이 가이드는 3년차 게임 개발자가 서버 개발을 쉽게 이해할 수 있도록 설계되었습니다. 게임 개발 경험을 활용하여 서버 개발을 빠르게 마스터하실 수 있도록 도와드리겠습니다.

## 1. ASP.NET Core Web API 기초 이론

### Web API란 무엇인가?

Web API는 Unity 게임이 서버와 통신할 수 있게 해주는 다리입니다. Unity에서 `UnityWebRequest`로 데이터를 요청하면, 서버의 Web API가 이를 받아 처리하고 결과를 돌려줍니다. 게임의 리더보드, 캐릭터 정보 저장, 멀티플레이어 매칭 등이 모두 Web API를 통해 이루어집니다.

### HTTP 프로토콜 기초

HTTP는 클라이언트(Unity 게임)와 서버 간의 통신 규약입니다. Unity의 `Coroutine`처럼 요청(Request)을 보내고 응답(Response)을 받는 비동기 패턴으로 동작합니다.

**HTTP 요청 구조:**
```http
POST /api/characters HTTP/1.1
Host: gameserver.com
Content-Type: application/json
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...

{
  "name": "DragonSlayer",
  "class": "Warrior",
  "level": 1
}
```

### REST API 원칙

REST는 URL을 게임의 리소스처럼 다루는 설계 방식입니다. Unity의 GameObject 계층구조처럼, URL도 계층적으로 설계합니다:

```
/api/players               # 모든 플레이어 (GameObject[]처럼)
/api/players/123           # 특정 플레이어 (GameObject.Find()처럼)
/api/players/123/inventory # 플레이어의 인벤토리 (player.GetComponent<Inventory>()처럼)
```

### JSON 데이터 형식

JSON은 Unity의 `JsonUtility`로 처리하는 그 형식입니다. 서버에서도 동일한 형식을 사용합니다:

```csharp
// Unity와 서버 모두에서 사용 가능한 모델
[System.Serializable]
public class Character
{
    public string name;
    public int level;
    public CharacterStats stats;
}

public class CharacterStats
{
    public int health;
    public int mana;
    public int attack;
    public int defense;
}
```

### HTTP 메서드 상세 설명

각 HTTP 메서드를 Unity의 개념과 비교해보겠습니다:

**GET - 데이터 읽기** (Unity의 `GameObject.Find()`와 유사)
```csharp
// 서버 코드
[HttpGet("characters/{id}")]
public async Task<ActionResult<Character>> GetCharacter(int id)
{
    var character = await _database.FindCharacterAsync(id);
    return character ?? NotFound();
}
```

**POST - 새 데이터 생성** (Unity의 `Instantiate()`와 유사)
```csharp
[HttpPost("characters")]
public async Task<ActionResult<Character>> CreateCharacter(Character newCharacter)
{
    newCharacter.Id = Guid.NewGuid();
    await _database.AddCharacterAsync(newCharacter);
    return CreatedAtAction(nameof(GetCharacter), new { id = newCharacter.Id }, newCharacter);
}
```

**PUT - 전체 데이터 업데이트** (GameObject 전체 교체와 유사)
```csharp
[HttpPut("characters/{id}")]
public async Task<IActionResult> UpdateCharacter(int id, Character updatedCharacter)
{
    if (id != updatedCharacter.Id) return BadRequest();
    await _database.UpdateCharacterAsync(updatedCharacter);
    return NoContent();
}
```

**DELETE - 데이터 삭제** (Unity의 `Destroy()`와 유사)
```csharp
[HttpDelete("characters/{id}")]
public async Task<IActionResult> DeleteCharacter(int id)
{
    await _database.DeleteCharacterAsync(id);
    return NoContent();
}
```

### HTTP 상태 코드의 의미

Unity에서 에러를 처리하듯, HTTP 상태 코드로 요청 결과를 판단합니다:

| 상태 코드 | 의미 | Unity에서의 처리 |
|---------|------|-----------------|
| **200 OK** | 성공 | 정상적으로 데이터 사용 |
| **201 Created** | 생성 성공 | 새 캐릭터/아이템 생성 완료 |
| **204 No Content** | 성공 (응답 데이터 없음) | 업데이트/삭제 완료 |
| **400 Bad Request** | 잘못된 요청 | 입력값 확인 필요 |
| **401 Unauthorized** | 인증 필요 | 로그인 화면으로 이동 |
| **404 Not Found** | 리소스 없음 | "캐릭터를 찾을 수 없습니다" |
| **500 Internal Server Error** | 서버 오류 | "서버 오류가 발생했습니다" |

## 2. ASP.NET Core Web API 개발 환경 설정

### .NET SDK 설치

1. **.NET 8.0 LTS 설치** (안정적인 장기 지원 버전)
   - [dotnet.microsoft.com](https://dotnet.microsoft.com)에서 다운로드
   - Unity 개발할 때처럼 LTS 버전을 선택하는 것이 안전합니다

2. **설치 확인**
```bash
dotnet --version
# 8.0.x 버전이 표시되어야 함
```

### Visual Studio 설정

Visual Studio 2022 설치 시 다음 워크로드를 선택하세요:
- **ASP.NET 및 웹 개발** (필수)
- **.NET 데스크톱 개발** (선택)

Unity 개발에 익숙하시다면 Visual Studio의 인터페이스가 친숙하실 겁니다.

### 새 Web API 프로젝트 생성

**Visual Studio에서:**
1. 파일 → 새로 만들기 → 프로젝트
2. "ASP.NET Core Web API" 템플릿 선택
3. 프로젝트 구성:
   - 프레임워크: .NET 8.0 (Long Term Support)
   - 인증 형식: 없음 (처음에는)
   - HTTPS 구성: 체크
   - OpenAPI 지원 사용: 체크 (Swagger)
   - 최상위 문 사용: 체크

**명령줄에서:**
```bash
dotnet new webapi -n GameServerAPI
cd GameServerAPI
dotnet run
```

### 프로젝트 구조 설명

Unity 프로젝트와 비교하여 설명드리겠습니다:

```
GameServerAPI/
├── Controllers/          # Unity의 Scripts/Controllers와 유사
│   └── WeatherForecastController.cs  # 샘플 컨트롤러
├── Models/              # Unity의 Scripts/Models와 유사
│   └── Character.cs     # 데이터 모델 클래스
├── Program.cs           # Unity의 Main Camera + GameManager 역할
├── appsettings.json     # Unity의 ProjectSettings와 유사
└── GameServerAPI.csproj # Unity의 .unityproj와 유사
```

**Program.cs - 서버의 진입점**
```csharp
var builder = WebApplication.CreateBuilder(args);

// 서비스 등록 (Unity의 Awake()와 유사)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 미들웨어 파이프라인 구성 (Unity의 Start()와 유사)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run(); // Unity의 게임 루프 시작과 유사
```

## 3. 첫 번째 Web API 만들기 단계별 실습

### Hello World API 컨트롤러 작성

Unity의 MonoBehaviour처럼, ASP.NET Core에서는 ControllerBase를 상속받습니다:

```csharp
using Microsoft.AspNetCore.Mvc;

namespace GameServerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase
    {
        // Unity의 Start() 메서드처럼 간단한 시작점
        [HttpGet("hello")]
        public IActionResult HelloWorld()
        {
            return Ok("Hello from Game Server!");
        }
        
        // 플레이어 이름을 받아 인사하기
        [HttpGet("greet/{playerName}")]
        public IActionResult GreetPlayer(string playerName)
        {
            return Ok($"Welcome to the game, {playerName}!");
        }
    }
}
```

### 라우팅 설정 방법

라우팅은 Unity의 이벤트 시스템처럼 URL을 특정 메서드에 연결합니다:

```csharp
[ApiController]
[Route("api/[controller]")]  // 기본 경로: api/character
public class CharacterController : ControllerBase
{
    // GET: api/character
    [HttpGet]
    public ActionResult<List<Character>> GetAllCharacters()
    {
        return new List<Character>
        {
            new Character { Id = 1, Name = "Warrior", Level = 10 },
            new Character { Id = 2, Name = "Mage", Level = 15 }
        };
    }
    
    // GET: api/character/5
    [HttpGet("{id}")]
    public ActionResult<Character> GetCharacter(int id)
    {
        var character = FindCharacterById(id);
        if (character == null)
            return NotFound($"Character with ID {id} not found");
        
        return character;
    }
    
    // GET: api/character/search?name=Warrior
    [HttpGet("search")]
    public ActionResult<List<Character>> SearchCharacters([FromQuery] string name)
    {
        var results = SearchByName(name);
        return Ok(results);
    }
}
```

## 4. JSON 데이터 주고받기 실습

### 모델 클래스 정의 방법

Unity의 Serializable 클래스와 매우 유사합니다:

```csharp
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class Player
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required(ErrorMessage = "Player name is required")]
    [StringLength(50, MinimumLength = 3)]
    public string Name { get; set; }
    
    [EmailAddress]
    public string Email { get; set; }
    
    [Range(1, 100)]
    public int Level { get; set; } = 1;
    
    public PlayerStats Stats { get; set; }
    
    [JsonIgnore]  // JSON 직렬화에서 제외 (Unity의 [HideInInspector]와 유사)
    public string PasswordHash { get; set; }
}
```

### [FromBody], [FromQuery], [FromRoute] 속성 사용법

데이터가 어디서 오는지 명시적으로 지정합니다:

```csharp
[ApiController]
[Route("api/[controller]")]
public class BattleController : ControllerBase
{
    // FromRoute - URL 경로에서 값 추출
    [HttpGet("room/{roomId}/player/{playerId}")]
    public IActionResult GetBattleStatus(
        [FromRoute] int roomId,
        [FromRoute] int playerId)
    {
        return Ok($"Room {roomId}, Player {playerId} status");
    }
    
    // FromQuery - 쿼리 스트링에서 값 추출
    [HttpGet("search")]
    public IActionResult SearchBattles(
        [FromQuery] int level,
        [FromQuery] string @class,
        [FromQuery] int page = 1)
    {
        var results = new { Level = level, Class = @class, Page = page };
        return Ok(results);
    }
    
    // FromBody - 요청 본문에서 JSON 추출
    [HttpPost("start")]
    public IActionResult StartBattle([FromBody] BattleRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        var battleId = CreateBattle(request);
        return Ok(new { BattleId = battleId });
    }
}
```

## 5. Postman 사용법 완전 가이드

### Postman 설치 및 기본 인터페이스

1. **설치**: [postman.com](https://www.postman.com)에서 다운로드
2. **계정 생성**: 클라우드 동기화를 위해 권장
3. **워크스페이스 생성**: Unity 프로젝트처럼 API 테스트를 관리

### GET 요청 테스트

```http
# 모든 캐릭터 조회
GET {{baseUrl}}/api/character

# 특정 캐릭터 조회
GET {{baseUrl}}/api/character/123

# 쿼리 파라미터 사용
GET {{baseUrl}}/api/character/search?name=Warrior&level=10
```

### POST 요청 테스트

```json
// Body 탭 → raw → JSON 선택
{
  "name": "DragonSlayer",
  "class": "Warrior",
  "level": 1,
  "stats": {
    "health": 100,
    "mana": 50,
    "strength": 15,
    "defense": 10
  }
}
```

### 환경 변수 활용

Unity의 ScriptableObject처럼 설정을 저장:

```json
// Development Environment
{
  "baseUrl": "https://localhost:7260",
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "testUserId": "12345",
  "testCharacterId": "67890"
}
```

### 테스트 스크립트 작성

Unity의 테스트처럼 자동 검증:

```javascript
// Tests 탭에 작성
pm.test("Status code is 200", function () {
    pm.response.to.have.status(200);
});

pm.test("Response time is less than 500ms", function () {
    pm.expect(pm.response.responseTime).to.be.below(500);
});

pm.test("Character has required fields", function () {
    const character = pm.response.json();
    pm.expect(character).to.have.property("id");
    pm.expect(character).to.have.property("name");
    pm.expect(character.level).to.be.at.least(1);
});

// 응답 데이터를 변수로 저장
pm.environment.set("characterId", pm.response.json().id);
```

## 6. 실무 중심 예제 프로젝트

### 간단한 사용자 관리 API (User CRUD)

```csharp
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;
    
    // 회원가입
    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto dto)
    {
        try
        {
            if (await _userService.UsernameExistsAsync(dto.Username))
                return BadRequest("Username already exists");
            
            var user = await _userService.CreateUserAsync(dto);
            _logger.LogInformation($"New user registered: {user.Username}");
            
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration");
            return StatusCode(500, "An error occurred during registration");
        }
    }
    
    // 로그인
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login(LoginDto dto)
    {
        var user = await _userService.ValidateCredentialsAsync(dto.Username, dto.Password);
        
        if (user == null)
            return Unauthorized("Invalid username or password");
        
        var token = _userService.GenerateJwtToken(user);
        
        return Ok(new LoginResponseDto
        {
            Token = token,
            User = user,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        });
    }
}
```

### 게임 캐릭터 정보 API (RPG 게임용)

```csharp
[ApiController]
[Route("api/game/[controller]")]
[Authorize]
public class CharacterController : ControllerBase
{
    // 캐릭터 생성
    [HttpPost]
    public async Task<ActionResult<GameCharacter>> CreateCharacter(CreateCharacterDto dto)
    {
        var userId = GetCurrentUserId();
        
        // 캐릭터 수 제한 확인
        var characterCount = await _characterService.GetCharacterCountAsync(userId);
        if (characterCount >= 5)
            return BadRequest("Maximum character limit reached");
        
        // 이름 중복 확인
        if (await _characterService.NameExistsAsync(dto.Name))
            return BadRequest("Character name already exists");
        
        var character = await _characterService.CreateCharacterAsync(userId, dto);
        
        // 시작 아이템 지급
        await _inventoryService.GrantStarterItemsAsync(character.Id, character.Class);
        
        return CreatedAtAction(nameof(GetCharacter), new { id = character.Id }, character);
    }
    
    // 캐릭터 레벨업
    [HttpPost("{id}/levelup")]
    public async Task<ActionResult<LevelUpResultDto>> LevelUp(Guid id)
    {
        var character = await _characterService.GetCharacterAsync(id);
        
        if (character == null)
            return NotFound();
        
        if (!IsOwner(character.UserId))
            return Forbid();
        
        // 경험치 확인
        var requiredExp = CalculateRequiredExperience(character.Level);
        if (character.Experience < requiredExp)
            return BadRequest($"Not enough experience. Required: {requiredExp}");
        
        // 레벨업 처리
        var result = await _characterService.LevelUpAsync(id);
        
        return Ok(new LevelUpResultDto
        {
            NewLevel = result.Level,
            StatsIncreased = result.StatsIncreased,
            NewSkillsUnlocked = result.NewSkills
        });
    }
}
```

## 7. 문제 해결 및 디버깅

### 자주 발생하는 오류들과 해결 방법

**1. NullReferenceException**

```csharp
// 문제 코드
[HttpGet("{id}")]
public ActionResult<Character> GetCharacter(int id)
{
    var character = _characters.Find(c => c.Id == id);
    return Ok(character.Name); // character가 null일 때 에러
}

// 해결 방법
[HttpGet("{id}")]
public ActionResult<Character> GetCharacter(int id)
{
    var character = _characters.Find(c => c.Id == id);
    
    if (character == null)
        return NotFound($"Character with ID {id} not found");
    
    return Ok(character);
}
```

**2. 비동기 데드락**

```csharp
// 문제 코드
public ActionResult<User> GetUser(int id)
{
    // .Result나 .Wait()는 데드락 유발
    var user = _userService.GetUserAsync(id).Result;
    return Ok(user);
}

// 해결 방법
public async Task<ActionResult<User>> GetUser(int id)
{
    var user = await _userService.GetUserAsync(id);
    return Ok(user);
}
```

### CORS 문제 해결

Unity WebGL에서 자주 발생하는 CORS 에러 해결:

```csharp
// Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("GameClientPolicy", policy =>
    {
        // 개발 환경
        if (builder.Environment.IsDevelopment())
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
        // 운영 환경
        else
        {
            policy.WithOrigins(
                    "https://yourgame.com",
                    "https://cdn.yourgame.com",
                    "https://localhost:3000"  // Unity WebGL 로컬 테스트
                  )
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
    });
});

// 미들웨어 순서 중요!
app.UseCors("GameClientPolicy");  // UseAuthentication 전에 위치
app.UseAuthentication();
app.UseAuthorization();
```

## Unity 클라이언트와의 통합

### Unity에서 API 호출하기

```csharp
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class GameAPIClient : MonoBehaviour
{
    private const string BASE_URL = "https://localhost:7260/api";
    private string authToken;
    
    // 로그인 처리
    public IEnumerator Login(string username, string password, System.Action<bool> callback)
    {
        var loginData = new { username = username, password = password };
        string json = JsonUtility.ToJson(loginData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        
        using (UnityWebRequest request = new UnityWebRequest($"{BASE_URL}/user/login", "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);
                authToken = response.token;
                callback(true);
            }
            else
            {
                Debug.LogError($"Login failed: {request.error}");
                callback(false);
            }
        }
    }
    
    // 캐릭터 데이터 가져오기
    public IEnumerator GetCharacterData(string characterId, System.Action<Character> callback)
    {
        using (UnityWebRequest request = UnityWebRequest.Get($"{BASE_URL}/character/{characterId}"))
        {
            request.SetRequestHeader("Authorization", $"Bearer {authToken}");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                var character = JsonUtility.FromJson<Character>(request.downloadHandler.text);
                callback(character);
            }
            else
            {
                Debug.LogError($"Failed to get character: {request.error}");
                callback(null);
            }
        }
    }
}
```

이 가이드가 서버 개발 여정의 든든한 시작점이 되기를 바랍니다!
