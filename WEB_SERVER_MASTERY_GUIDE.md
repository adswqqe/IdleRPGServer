# 🎮 Unity 개발자를 위한 웹서버 마스터리 가이드

> **실습 중심 심화 학습으로 Idle RPG 서버를 완성하며 웹서버 개발 전문가 되기**

## 🎯 학습 목표

- **Unity 개발 경험을 활용**하여 웹서버 개발 핵심 개념 마스터
- **Idle RPG 서버를 직접 구현**하며 실무 경험 습득
- **동접 1만 명급 서버**를 설계할 수 있는 실력 달성
- **나중에 후회하지 않을 견고한 기초** 구축

## 📅 전체 학습 로드맵 (20-25일)

```
🏗️  1단계: HTTP & Clean Architecture    (3-4일)
🗄️  2단계: 데이터베이스 & EF Core       (4-5일)
⚡  3단계: 비동기 & 성능                (3-4일)
🔐  4단계: 보안 & JWT 심화             (3-4일)
📊  5단계: Critical 기반 구축          (2-3일)
🎮  6단계: 게임 로직 구현              (3-4일)
😴  7단계: Idle 시스템 & Background    (2-3일)
⚡  8단계: 실시간 & SignalR            (2-3일)
🚀  9단계: 최적화 & 배포               (2-3일)
```

---

## 🏗️ 1단계: HTTP & Clean Architecture 마스터리 (3-4일)

### 📚 **학습 목표**
Unity의 MonoBehaviour 패턴을 넘어서 **엔터프라이즈급 아키텍처 패턴**을 완전히 이해하고 적용할 수 있게 됩니다.

### 🔍 **Day 1: HTTP 프로토콜 완전 정복**

#### **오전: HTTP 심화 이론** (3시간)

##### HTTP의 본질 이해
```
Unity와 HTTP 비교:
Unity: GameObject간 직접 통신 (SendMessage, Events)
HTTP: Request-Response 기반 무상태 통신

핵심 차이점:
- Stateless vs Stateful
- 동기 vs 비동기 처리
- 에러 처리 방식
```

##### HTTP 메서드별 의미와 활용
```http
GET /api/characters/123
- 안전함 (Safe): 서버 상태 변경 없음
- 멱등성 (Idempotent): 여러 번 호출해도 결과 동일
- 캐시 가능 (Cacheable)

POST /api/characters
- 비안전: 새로운 리소스 생성
- 비멱등: 호출할 때마다 새 캐릭터 생성
- 캐시 불가

PUT vs PATCH:
PUT /api/characters/123   → 전체 교체
PATCH /api/characters/123 → 부분 업데이트
```

##### HTTP 상태 코드 전략적 활용
```
게임 서버 관점 상태 코드:
200 OK         → 성공적인 게임 액션
201 Created    → 캐릭터/아이템 생성 완료
400 Bad Request → 잘못된 게임 입력 (음수 경험치 등)
401 Unauthorized → 로그인 필요
403 Forbidden   → 권한 부족 (관리자 전용 기능)
404 Not Found   → 존재하지 않는 캐릭터/아이템
409 Conflict    → 이미 존재하는 닉네임
422 Unprocessable → 비즈니스 룰 위반 (레벨 부족 등)
429 Too Many Requests → Rate Limiting
500 Internal Error → 서버 오류
503 Service Unavailable → 점검 모드
```

##### HTTP 헤더 활용 전략
```http
Content-Type: application/json
Accept: application/json
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
Cache-Control: no-cache, no-store
ETag: "33a64df551425fcc55e4d42a148795d9f25f89d4"
Last-Modified: Wed, 21 Oct 2015 07:28:00 GMT
X-RateLimit-Remaining: 999
X-Request-ID: 550e8400-e29b-41d4-a716-446655440000
```

#### **오후: 첫 번째 API 구현** (4시간)

##### 프로젝트 초기 설정
```bash
# 프로젝트 생성
dotnet new webapi -n IdleRPGServer
cd IdleRPGServer

# 필수 패키지 추가
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Serilog.AspNetCore
```

##### 기본 API 구현
```csharp
// Controllers/HealthController.cs
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult GetHealth()
    {
        return Ok(new
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0",
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
        });
    }

    [HttpGet("detailed")]
    public IActionResult GetDetailedHealth()
    {
        // 실제 서비스에서는 데이터베이스, Redis 등 의존성 체크
        return Ok(new
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Checks = new
            {
                Database = "Connected",
                Redis = "Connected",
                ExternalAPI = "Available"
            },
            Uptime = TimeSpan.FromMilliseconds(Environment.TickCount64)
        });
    }
}
```

#### **저녁: HTTP 테스트 및 분석** (1시간)
```bash
# Swagger UI에서 테스트
# Postman/Insomnia로 다양한 시나리오 테스트
# 브라우저 개발자 도구로 네트워크 분석
```

### 🔍 **Day 2: Clean Architecture 설계 & 프로젝트 구조**

#### **오전: Clean Architecture 이론 심화** (3시간)

##### Unity vs Clean Architecture 비교
```
Unity 구조:
GameObject → MonoBehaviour → 직접 참조
- 강한 결합 (Tight Coupling)
- 테스트 어려움
- 코드 재사용성 낮음

Clean Architecture:
외부 → 인터페이스 → 내부 로직
- 약한 결합 (Loose Coupling)
- 테스트 용이
- 높은 재사용성
```

##### 4개 레이어 상세 분석
```
🔵 Domain (가장 안쪽)
- 엔티티 (Entities)
- 값 객체 (Value Objects)
- 도메인 서비스 (Domain Services)
- 비즈니스 룰 (Business Rules)

🟡 Application (응용)
- 유스케이스 (Use Cases)
- 인터페이스 정의 (Interfaces)
- DTO (Data Transfer Objects)
- 애플리케이션 서비스 (Application Services)

🟠 Infrastructure (기반구조)
- 데이터베이스 구현 (EF Core)
- 외부 API 클라이언트
- 파일 시스템 접근
- 캐싱 구현

🔴 API/Presentation (표현)
- 컨트롤러 (Controllers)
- 미들웨어 (Middleware)
- HTTP 관련 로직
- 요청/응답 변환
```

##### 의존성 규칙 (Dependency Rule)
```
의존성 방향: 외부 → 내부 (단방향만 허용)

❌ 잘못된 의존성:
Domain → Infrastructure (절대 안됨!)
Domain → Application (절대 안됨!)

✅ 올바른 의존성:
API → Application → Domain
Infrastructure → Application
Infrastructure → Domain (인터페이스를 통해서만)
```

#### **오후: Clean Architecture 프로젝트 구조 생성** (4시간)

##### 솔루션 구조 생성
```bash
# 솔루션 생성
dotnet new sln -n IdleRPGServer

# 프로젝트들 생성
dotnet new classlib -n IdleRPG.Domain
dotnet new classlib -n IdleRPG.Application
dotnet new classlib -n IdleRPG.Infrastructure
dotnet new webapi -n IdleRPG.API

# 솔루션에 프로젝트 추가
dotnet sln add IdleRPG.Domain/IdleRPG.Domain.csproj
dotnet sln add IdleRPG.Application/IdleRPG.Application.csproj
dotnet sln add IdleRPG.Infrastructure/IdleRPG.Infrastructure.csproj
dotnet sln add IdleRPG.API/IdleRPG.API.csproj

# 프로젝트간 참조 설정 (의존성 규칙 준수)
cd IdleRPG.Application && dotnet add reference ../IdleRPG.Domain/IdleRPG.Domain.csproj
cd ../IdleRPG.Infrastructure && dotnet add reference ../IdleRPG.Application/IdleRPG.Application.csproj
cd ../IdleRPG.Infrastructure && dotnet add reference ../IdleRPG.Domain/IdleRPG.Domain.csproj
cd ../IdleRPG.API && dotnet add reference ../IdleRPG.Application/IdleRPG.Application.csproj
cd ../IdleRPG.API && dotnet add reference ../IdleRPG.Infrastructure/IdleRPG.Infrastructure.csproj
```

##### 각 프로젝트 기본 구조 생성
```csharp
// IdleRPG.Domain/Entities/BaseEntity.cs
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}

// IdleRPG.Domain/Entities/Player.cs
public class Player : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public DateTime LastLoginUtc { get; set; }

    // Navigation Properties
    public List<Character> Characters { get; set; } = new();
}

// IdleRPG.Application/Interfaces/IPlayerRepository.cs
public interface IPlayerRepository
{
    Task<Player?> GetByIdAsync(Guid id);
    Task<Player?> GetByEmailAsync(string email);
    Task<Player> CreateAsync(Player player);
    Task UpdateAsync(Player player);
    Task DeleteAsync(Guid id);
}

// IdleRPG.Application/Services/PlayerService.cs
public class PlayerService : IPlayerService
{
    private readonly IPlayerRepository _playerRepository;

    public PlayerService(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository;
    }

    public async Task<Player?> GetPlayerAsync(Guid id)
    {
        return await _playerRepository.GetByIdAsync(id);
    }
}
```

### 🔍 **Day 3: JWT 인증 시스템 심화 구현**

#### **오전: JWT 이론 완전 정복** (3시간)

##### JWT 구조 분석
```
JWT = Header.Payload.Signature

Header (알고리즘 정보):
{
  "alg": "HS256",
  "typ": "JWT"
}

Payload (실제 데이터):
{
  "sub": "123e4567-e89b-12d3-a456-426614174000",  // Subject (사용자 ID)
  "email": "player@example.com",
  "role": "Player",
  "iat": 1516239022,  // Issued At
  "exp": 1516242622,  // Expiration
  "jti": "uuid"       // JWT ID (고유 식별자)
}

Signature (위변조 방지):
HMACSHA256(
  base64UrlEncode(header) + "." +
  base64UrlEncode(payload),
  secret)
```

##### JWT vs Session 비교
```
Unity 게임 관점:
- Session: 서버에 플레이어 상태 저장 (메모리 사용)
- JWT: 토큰에 플레이어 정보 포함 (Stateless)

장단점 비교:
Session:
✅ 서버에서 즉시 무효화 가능
✅ 민감한 정보 숨김 가능
❌ 확장성 문제 (서버간 공유 어려움)
❌ 메모리 사용량 증가

JWT:
✅ 확장성 우수 (서버간 공유 쉬움)
✅ 서버 메모리 절약
❌ 토큰 무효화 어려움
❌ 토큰 크기가 클 수 있음
```

##### JWT 보안 고려사항
```
🔐 보안 위협과 대응:
1. XSS (Cross-Site Scripting)
   - 대응: HttpOnly 쿠키 사용 또는 적절한 저장소 선택

2. CSRF (Cross-Site Request Forgery)
   - 대응: SameSite 쿠키 설정, CSRF 토큰

3. 토큰 탈취
   - 대응: 짧은 만료 시간, Refresh Token 패턴

4. 토큰 재사용 공격
   - 대응: JTI(JWT ID) 활용한 일회용 토큰
```

#### **오후: JWT 인증 시스템 구현** (4시간)

##### JWT 서비스 구현
```csharp
// IdleRPG.Application/Services/IJwtTokenService.cs
public interface IJwtTokenService
{
    string GenerateAccessToken(Player player);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token);
    Task<string?> RefreshTokenAsync(string refreshToken);
}

// IdleRPG.Infrastructure/Services/JwtTokenService.cs
public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly IPlayerRepository _playerRepository;

    public JwtTokenService(IOptions<JwtSettings> jwtSettings, IPlayerRepository playerRepository)
    {
        _jwtSettings = jwtSettings.Value;
        _playerRepository = playerRepository;
    }

    public string GenerateAccessToken(Player player)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, player.Id.ToString()),
            new Claim(ClaimTypes.Email, player.Email),
            new Claim(ClaimTypes.Name, player.DisplayName),
            new Claim(ClaimTypes.Role, "Player"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            return principal;
        }
        catch
        {
            return null;
        }
    }
}
```

##### 인증 컨트롤러 구현
```csharp
// IdleRPG.API/Controllers/AuthController.cs
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IPlayerService _playerService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IPlayerService playerService,
        IJwtTokenService jwtTokenService,
        ILogger<AuthController> logger)
    {
        _playerService = playerService;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            // 입력 검증
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { Message = "이메일과 비밀번호는 필수입니다." });
            }

            // 이메일 중복 확인
            var existingPlayer = await _playerService.GetPlayerByEmailAsync(request.Email);
            if (existingPlayer != null)
            {
                return Conflict(new { Message = "이미 존재하는 이메일입니다." });
            }

            // 비밀번호 해싱 (BCrypt 사용)
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var newPlayer = new Player
            {
                Email = request.Email,
                DisplayName = request.DisplayName ?? request.Email.Split('@')[0],
                PasswordHash = passwordHash
            };

            var createdPlayer = await _playerService.CreatePlayerAsync(newPlayer);

            // JWT 토큰 생성
            var accessToken = _jwtTokenService.GenerateAccessToken(createdPlayer);
            var refreshToken = _jwtTokenService.GenerateRefreshToken();

            // Refresh Token 저장 (DB 또는 Redis)
            await _playerService.SaveRefreshTokenAsync(createdPlayer.Id, refreshToken);

            _logger.LogInformation("새 사용자 가입 완료: {Email}", request.Email);

            return Created("", new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Player = new PlayerDto
                {
                    Id = createdPlayer.Id,
                    Email = createdPlayer.Email,
                    DisplayName = createdPlayer.DisplayName
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "사용자 가입 중 오류 발생");
            return StatusCode(500, new { Message = "서버 내부 오류가 발생했습니다." });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            // 사용자 존재 확인
            var player = await _playerService.GetPlayerByEmailAsync(request.Email);
            if (player == null)
            {
                return Unauthorized(new { Message = "이메일 또는 비밀번호가 올바르지 않습니다." });
            }

            // 비밀번호 검증
            if (!BCrypt.Net.BCrypt.Verify(request.Password, player.PasswordHash))
            {
                return Unauthorized(new { Message = "이메일 또는 비밀번호가 올바르지 않습니다." });
            }

            // 마지막 로그인 시간 업데이트
            await _playerService.UpdateLastLoginAsync(player.Id);

            // JWT 토큰 생성
            var accessToken = _jwtTokenService.GenerateAccessToken(player);
            var refreshToken = _jwtTokenService.GenerateRefreshToken();

            await _playerService.SaveRefreshTokenAsync(player.Id, refreshToken);

            _logger.LogInformation("사용자 로그인: {Email}", request.Email);

            return Ok(new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Player = new PlayerDto
                {
                    Id = player.Id,
                    Email = player.Email,
                    DisplayName = player.DisplayName
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "로그인 중 오류 발생");
            return StatusCode(500, new { Message = "서버 내부 오류가 발생했습니다." });
        }
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var playerGuid))
            {
                return Unauthorized();
            }

            var player = await _playerService.GetPlayerAsync(playerGuid);
            if (player == null)
            {
                return NotFound();
            }

            return Ok(new PlayerDto
            {
                Id = player.Id,
                Email = player.Email,
                DisplayName = player.DisplayName,
                LastLoginUtc = player.LastLoginUtc
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "사용자 정보 조회 중 오류 발생");
            return StatusCode(500, new { Message = "서버 내부 오류가 발생했습니다." });
        }
    }
}
```

### 🔍 **Day 4: 보안 심화 & 코드 리뷰**

#### **오전: 웹 보안 핵심 개념** (3시간)

##### OWASP Top 10 게임 서버 관점
```
1. Injection (인젝션)
   게임 서버 위험: SQL 인젝션으로 캐릭터 정보 탈취
   대응: 매개변수화된 쿼리, ORM 사용

2. Broken Authentication
   게임 서버 위험: 약한 JWT로 계정 탈취
   대응: 강력한 암호화, 적절한 토큰 만료

3. Sensitive Data Exposure
   게임 서버 위험: 비밀번호 평문 저장, 게임 데이터 노출
   대응: 해싱, 암호화, HTTPS 필수

4. Security Misconfiguration
   게임 서버 위험: 개발 모드로 운영, 디버그 정보 노출
   대응: 운영 환경 설정 점검

5. Cross-Site Scripting (XSS)
   게임 서버 위험: 채팅, 닉네임에 스크립트 삽입
   대응: 입력 검증, HTML 인코딩

6. Insecure Direct Object Reference
   게임 서버 위험: 다른 플레이어 캐릭터 조작
   대응: 권한 검증, 소유권 확인

7. Cross-Site Request Forgery (CSRF)
   게임 서버 위험: 무의식적 게임 액션 수행
   대응: CSRF 토큰, SameSite 쿠키

8. Using Components with Known Vulnerabilities
   게임 서버 위험: 취약한 라이브러리 사용
   대응: 정기적 업데이트, 보안 스캔

9. Insufficient Logging & Monitoring
   게임 서버 위험: 치팅, 해킹 탐지 실패
   대응: 포괄적 로깅, 실시간 모니터링

10. API Security Issues
    게임 서버 위험: Rate Limiting 없음, 과도한 정보 노출
    대응: API Gateway, 적절한 응답 설계
```

##### Rate Limiting 전략
```csharp
// Rate Limiting 미들웨어 구현
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly MemoryCache _cache;
    private readonly RateLimitOptions _options;

    public RateLimitingMiddleware(
        RequestDelegate next,
        IOptions<RateLimitOptions> options)
    {
        _next = next;
        _cache = new MemoryCache(new MemoryCacheOptions());
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var key = GetClientKey(context);
        var requests = _cache.GetOrCreate(key, entry =>
        {
            entry.SetAbsoluteExpiration(_options.Window);
            return new Queue<DateTime>();
        }) as Queue<DateTime>;

        var now = DateTime.UtcNow;

        // 윈도우 밖의 요청들 제거
        while (requests.Count > 0 && requests.Peek() < now - _options.Window)
        {
            requests.Dequeue();
        }

        // 제한 확인
        if (requests.Count >= _options.Limit)
        {
            context.Response.StatusCode = 429;
            await context.Response.WriteAsync("Too Many Requests");
            return;
        }

        requests.Enqueue(now);
        await _next(context);
    }

    private string GetClientKey(HttpContext context)
    {
        // IP 주소 + 사용자 ID 조합으로 더 정교한 제한
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
        return $"{ip}:{userId}";
    }
}
```

#### **오후: 보안 기능 통합 테스트** (3시간)

##### 통합 테스트 시나리오
```csharp
// 보안 테스트 케이스들
public class SecurityIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public SecurityIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithWeakPassword_ShouldReturnBadRequest()
    {
        // 약한 비밀번호로 가입 시도
        var request = new { Email = "test@example.com", Password = "123" };
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithIncorrectPassword_ShouldReturnUnauthorized()
    {
        // 잘못된 비밀번호로 로그인 시도
        var request = new { Email = "test@example.com", Password = "wrongpassword" };
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_ShouldReturnUnauthorized()
    {
        // 토큰 없이 보호된 엔드포인트 접근
        var response = await _client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithExpiredToken_ShouldReturnUnauthorized()
    {
        // 만료된 토큰으로 접근 (토큰 만료 시간을 짧게 설정해서 테스트)
    }

    [Fact]
    public async Task RateLimit_ExceedingLimit_ShouldReturn429()
    {
        // Rate Limit 초과 시나리오 테스트
        for (int i = 0; i < 11; i++) // 제한이 10이라고 가정
        {
            var response = await _client.GetAsync("/api/health");
            if (i < 10)
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            else
                Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
        }
    }
}
```

#### **저녁: 1단계 종합 리뷰** (1시간)

##### 체크리스트
```
✅ HTTP 프로토콜 이해도 확인
- 상태 코드 적절한 사용
- 헤더 활용
- 캐싱 전략

✅ Clean Architecture 구현도 확인
- 의존성 방향 준수
- 레이어별 책임 분리
- 인터페이스 활용

✅ JWT 인증 보안성 확인
- 적절한 클레임 구성
- 토큰 만료 처리
- Refresh Token 패턴

✅ 보안 고려사항 적용
- 비밀번호 해싱
- Rate Limiting
- 입력 검증
```

---

## 🗄️ 2단계: 데이터베이스 & EF Core 마스터리 (4-5일)

### 📚 **학습 목표**
게임 데이터의 특성을 고려한 **최적화된 데이터베이스 설계**와 **고성능 쿼리 작성** 능력을 습득합니다.

### 🔍 **Day 1: 관계형 데이터베이스 설계 원칙**

#### **오전: 정규화 이론과 게임 데이터 특성** (3시간)

##### 정규화 vs 비정규화 전략
```
게임 데이터의 특징:
- 읽기 중심 (Read-Heavy): 캐릭터 조회 >> 캐릭터 생성
- 실시간성: 빠른 응답 속도 필요
- 대용량: 플레이어, 로그 데이터 급증
- 복합적 관계: 플레이어-캐릭터-아이템-길드 등

정규화 적용 전략:
✅ 마스터 데이터 (아이템 템플릿, 몬스터 정보): 3NF까지 적용
✅ 사용자 데이터 (플레이어, 캐릭터): 3NF 기본, 성능상 필요시 일부 비정규화
❌ 로그 데이터: 비정규화로 빠른 삽입 우선
❌ 집계 데이터: 비정규화로 빠른 조회 우선
```

##### 게임 데이터베이스 스키마 설계
```sql
-- 1NF: 원자값, 중복 제거
-- 잘못된 예시
CREATE TABLE Characters_Wrong (
    Id GUID PRIMARY KEY,
    Name NVARCHAR(50),
    Items NVARCHAR(MAX) -- '아이템1,아이템2,아이템3' (원자값 위반!)
);

-- 올바른 예시
CREATE TABLE Characters (
    Id GUID PRIMARY KEY,
    Name NVARCHAR(50),
    PlayerId GUID FOREIGN KEY
);

CREATE TABLE CharacterItems (
    Id GUID PRIMARY KEY,
    CharacterId GUID FOREIGN KEY,
    ItemTemplateId INT FOREIGN KEY,
    Quantity INT
);

-- 2NF: 부분적 함수 종속 제거
-- 잘못된 예시
CREATE TABLE CharacterSkills_Wrong (
    CharacterId GUID,
    SkillId INT,
    SkillLevel INT,
    SkillName NVARCHAR(50),  -- SkillId에만 종속 (부분 종속!)
    SkillDescription NVARCHAR(200), -- SkillId에만 종속
    PRIMARY KEY (CharacterId, SkillId)
);

-- 올바른 예시
CREATE TABLE Skills (
    Id INT PRIMARY KEY,
    Name NVARCHAR(50),
    Description NVARCHAR(200)
);

CREATE TABLE CharacterSkills (
    CharacterId GUID,
    SkillId INT,
    Level INT,
    PRIMARY KEY (CharacterId, SkillId),
    FOREIGN KEY (CharacterId) REFERENCES Characters(Id),
    FOREIGN KEY (SkillId) REFERENCES Skills(Id)
);

-- 3NF: 이행적 함수 종속 제거
-- 잘못된 예시
CREATE TABLE Characters_Wrong_3NF (
    Id GUID PRIMARY KEY,
    Name NVARCHAR(50),
    GuildId GUID,
    GuildName NVARCHAR(50), -- GuildId를 통해 종속 (이행적 종속!)
    GuildLevel INT -- GuildId를 통해 종속
);

-- 올바른 예시
CREATE TABLE Guilds (
    Id GUID PRIMARY KEY,
    Name NVARCHAR(50),
    Level INT
);

CREATE TABLE Characters (
    Id GUID PRIMARY KEY,
    Name NVARCHAR(50),
    GuildId GUID FOREIGN KEY REFERENCES Guilds(Id)
);
```

##### 성능을 위한 전략적 비정규화
```sql
-- 케이스 1: 자주 조회되는 계산값 비정규화
CREATE TABLE Characters (
    Id GUID PRIMARY KEY,
    Name NVARCHAR(50),
    Level INT,
    Experience BIGINT,

    -- 비정규화: 매번 계산하지 않고 저장
    TotalPower INT, -- Attack + Defense + Speed 합계
    LastCalculatedAt DATETIME2,

    -- 비정규화: 자주 함께 조회되는 길드 정보
    GuildId GUID,
    GuildName NVARCHAR(50), -- 길드 이름은 자주 바뀌지 않으므로 복사 저장

    PlayerId GUID FOREIGN KEY
);

-- 케이스 2: 집계 데이터 비정규화
CREATE TABLE Players (
    Id GUID PRIMARY KEY,
    Email NVARCHAR(100),

    -- 비정규화: 집계 정보 저장 (매번 COUNT 하지 않음)
    CharacterCount INT DEFAULT 0,
    TotalPlayTime BIGINT DEFAULT 0,
    LastActivityUtc DATETIME2,

    CreatedAtUtc DATETIME2
);

-- 트리거 또는 애플리케이션 레벨에서 일관성 유지
CREATE TRIGGER UpdatePlayerStats ON Characters
AFTER INSERT, DELETE
AS BEGIN
    UPDATE Players
    SET CharacterCount = (SELECT COUNT(*) FROM Characters WHERE PlayerId = Players.Id)
    WHERE Id IN (SELECT DISTINCT PlayerId FROM inserted UNION SELECT DISTINCT PlayerId FROM deleted)
END
```

#### **오후: EF Core 모델 설계 및 관계 설정** (4시간)

##### 엔티티 설계와 관계 매핑
```csharp
// IdleRPG.Domain/Entities/Player.cs
public class Player : BaseAuditableEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public DateTime LastLoginUtc { get; set; }

    // 비정규화 필드들 (성능 최적화)
    public int CharacterCount { get; set; } = 0;
    public long TotalPlayTimeSeconds { get; set; } = 0;
    public DateTime LastActivityUtc { get; set; }

    // Navigation Properties
    public List<Character> Characters { get; set; } = new();
    public List<RefreshToken> RefreshTokens { get; set; } = new();
    public List<GameActionLog> ActionLogs { get; set; } = new();
}

// IdleRPG.Domain/Entities/Character.cs
public class Character : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; } = 1;
    public long Experience { get; set; } = 0;
    public int Gold { get; set; } = 0;

    // 스탯
    public int Attack { get; set; } = 10;
    public int Defense { get; set; } = 10;
    public int Speed { get; set; } = 10;

    // 비정규화: 자주 사용되는 계산값
    public int TotalPower { get; set; } = 30; // Attack + Defense + Speed
    public DateTime LastCalculatedAt { get; set; } = DateTime.UtcNow;

    // 에너지 시스템
    public int Energy { get; set; } = 100;
    public int MaxEnergy { get; set; } = 100;
    public DateTime LastEnergyUpdateUtc { get; set; } = DateTime.UtcNow;

    // 길드 관계 (optional)
    public Guid? GuildId { get; set; }
    public string? GuildName { get; set; } // 비정규화

    // Foreign Key
    public Guid PlayerId { get; set; }

    // Navigation Properties
    public Player Player { get; set; } = null!;
    public Guild? Guild { get; set; }
    public List<CharacterItem> Items { get; set; } = new();
    public List<CharacterSkill> Skills { get; set; } = new();
    public List<GameActionLog> ActionLogs { get; set; } = new();
}

// IdleRPG.Domain/Entities/ItemTemplate.cs (마스터 데이터)
public class ItemTemplate
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ItemType Type { get; set; }
    public ItemRarity Rarity { get; set; }

    // 스탯 보너스
    public int AttackBonus { get; set; } = 0;
    public int DefenseBonus { get; set; } = 0;
    public int SpeedBonus { get; set; } = 0;

    // 경제
    public int Price { get; set; } = 0;
    public int SellPrice { get; set; } = 0;

    // 게임 밸런스 (이후 GameConfig로 이동 예정)
    public int RequiredLevel { get; set; } = 1;
    public bool IsStackable { get; set; } = true;
    public int MaxStackSize { get; set; } = 99;

    // Navigation Properties
    public List<CharacterItem> CharacterItems { get; set; } = new();
}

// IdleRPG.Domain/Entities/CharacterItem.cs (다대다 관계 테이블)
public class CharacterItem : BaseAuditableEntity
{
    public Guid CharacterId { get; set; }
    public int ItemTemplateId { get; set; }
    public int Quantity { get; set; } = 1;
    public bool IsEquipped { get; set; } = false;

    // 강화 시스템 (향후 확장)
    public int EnhancementLevel { get; set; } = 0;

    // Navigation Properties
    public Character Character { get; set; } = null!;
    public ItemTemplate ItemTemplate { get; set; } = null!;
}
```

##### EF Core 설정 및 Fluent API
```csharp
// IdleRPG.Infrastructure/Data/Configurations/PlayerConfiguration.cs
public class PlayerConfiguration : IEntityTypeConfiguration<Player>
{
    public void Configure(EntityTypeBuilder<Player> builder)
    {
        builder.ToTable("Players");

        // Primary Key
        builder.HasKey(p => p.Id);

        // Properties
        builder.Property(p => p.Email)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.DisplayName)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.PasswordHash)
            .HasMaxLength(200)
            .IsRequired();

        // Indexes
        builder.HasIndex(p => p.Email)
            .IsUnique()
            .HasDatabaseName("IX_Players_Email");

        builder.HasIndex(p => p.LastActivityUtc)
            .HasDatabaseName("IX_Players_LastActivity");

        // Relationships
        builder.HasMany(p => p.Characters)
            .WithOne(c => c.Player)
            .HasForeignKey(c => c.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.RefreshTokens)
            .WithOne(rt => rt.Player)
            .HasForeignKey(rt => rt.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

// IdleRPG.Infrastructure/Data/Configurations/CharacterConfiguration.cs
public class CharacterConfiguration : IEntityTypeConfiguration<Character>
{
    public void Configure(EntityTypeBuilder<Character> builder)
    {
        builder.ToTable("Characters");

        builder.HasKey(c => c.Id);

        // Properties
        builder.Property(c => c.Name)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.GuildName)
            .HasMaxLength(50); // nullable

        // Computed Columns (SQL Server specific)
        builder.Property(c => c.TotalPower)
            .HasComputedColumnSql("[Attack] + [Defense] + [Speed]", stored: true);

        // Indexes for game queries
        builder.HasIndex(c => c.PlayerId)
            .HasDatabaseName("IX_Characters_PlayerId");

        builder.HasIndex(c => c.Level)
            .HasDatabaseName("IX_Characters_Level");

        builder.HasIndex(c => c.GuildId)
            .HasDatabaseName("IX_Characters_GuildId");

        // Composite index for ranking queries
        builder.HasIndex(c => new { c.Level, c.Experience })
            .HasDatabaseName("IX_Characters_LevelExperience");

        // Relationships
        builder.HasOne(c => c.Player)
            .WithMany(p => p.Characters)
            .HasForeignKey(c => c.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Guild)
            .WithMany(g => g.Members)
            .HasForeignKey(c => c.GuildId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(c => c.Items)
            .WithOne(ci => ci.Character)
            .HasForeignKey(ci => ci.CharacterId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

// IdleRPG.Infrastructure/Data/Configurations/CharacterItemConfiguration.cs
public class CharacterItemConfiguration : IEntityTypeConfiguration<CharacterItem>
{
    public void Configure(EntityTypeBuilder<CharacterItem> builder)
    {
        builder.ToTable("CharacterItems");

        builder.HasKey(ci => ci.Id);

        // Alternative: Composite Key 방식
        // builder.HasKey(ci => new { ci.CharacterId, ci.ItemTemplateId });

        // Properties
        builder.Property(ci => ci.Quantity)
            .HasDefaultValue(1);

        builder.Property(ci => ci.IsEquipped)
            .HasDefaultValue(false);

        builder.Property(ci => ci.EnhancementLevel)
            .HasDefaultValue(0);

        // Indexes
        builder.HasIndex(ci => ci.CharacterId)
            .HasDatabaseName("IX_CharacterItems_CharacterId");

        builder.HasIndex(ci => ci.ItemTemplateId)
            .HasDatabaseName("IX_CharacterItems_ItemTemplateId");

        // 장착 아이템 조회용 인덱스
        builder.HasIndex(ci => new { ci.CharacterId, ci.IsEquipped })
            .HasDatabaseName("IX_CharacterItems_CharacterEquipped");

        // Relationships
        builder.HasOne(ci => ci.Character)
            .WithMany(c => c.Items)
            .HasForeignKey(ci => ci.CharacterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ci => ci.ItemTemplate)
            .WithMany(it => it.CharacterItems)
            .HasForeignKey(ci => ci.ItemTemplateId)
            .OnDelete(DeleteBehavior.Restrict); // 마스터 데이터는 삭제 방지
    }
}
```

### 🔍 **Day 2: 인덱스 전략 및 쿼리 최적화**

#### **오전: 인덱스 이론과 게임 쿼리 패턴 분석** (3시간)

##### 게임에서 자주 발생하는 쿼리 패턴
```sql
-- 패턴 1: 플레이어의 캐릭터 목록 조회 (매우 빈번)
SELECT c.Id, c.Name, c.Level, c.Gold, c.TotalPower
FROM Characters c
WHERE c.PlayerId = @playerId
ORDER BY c.Level DESC, c.CreatedAtUtc ASC;

-- 필요한 인덱스: IX_Characters_PlayerId_Level_CreatedAt
CREATE INDEX IX_Characters_PlayerId_Level_CreatedAt
ON Characters (PlayerId, Level DESC, CreatedAtUtc ASC);

-- 패턴 2: 레벨별 랭킹 조회 (빈번)
SELECT TOP 100 c.Name, c.Level, c.Experience, c.TotalPower
FROM Characters c
WHERE c.Level >= @minLevel
ORDER BY c.Level DESC, c.Experience DESC;

-- 필요한 인덱스: IX_Characters_Level_Experience
CREATE INDEX IX_Characters_Level_Experience
ON Characters (Level DESC, Experience DESC)
INCLUDE (Name, TotalPower);

-- 패턴 3: 길드 멤버 조회 (보통)
SELECT c.Id, c.Name, c.Level, c.LastActivityUtc
FROM Characters c
WHERE c.GuildId = @guildId
ORDER BY c.Level DESC;

-- 필요한 인덱스: IX_Characters_GuildId_Level
CREATE INDEX IX_Characters_GuildId_Level
ON Characters (GuildId, Level DESC)
INCLUDE (Name, LastActivityUtc);

-- 패턴 4: 활성 사용자 조회 (관리용)
SELECT p.Id, p.Email, p.DisplayName, p.LastActivityUtc
FROM Players p
WHERE p.LastActivityUtc >= @cutoffDate
ORDER BY p.LastActivityUtc DESC;

-- 필요한 인덱스: IX_Players_LastActivity
CREATE INDEX IX_Players_LastActivity
ON Players (LastActivityUtc DESC)
INCLUDE (Email, DisplayName);
```

##### 인덱스 설계 전략
```csharp
// 복합 인덱스 순서 결정 방법
// 1. 카디널리티 (Cardinality): 높은 것 우선
// 2. 선택도 (Selectivity): WHERE 절에서 많이 사용되는 것 우선
// 3. 정렬 (ORDER BY): 정렬에 사용되는 컬럼 포함

// 예시: 캐릭터 검색 쿼리 최적화
public class CharacterQueryOptimization
{
    // 나쁜 예시: 모든 컬럼을 개별 인덱스로
    /*
    CREATE INDEX IX_Characters_Level ON Characters (Level);
    CREATE INDEX IX_Characters_PlayerId ON Characters (PlayerId);
    CREATE INDEX IX_Characters_GuildId ON Characters (GuildId);

    문제점:
    - 여러 WHERE 조건 시 인덱스 스캔 후 Merge 필요
    - ORDER BY 절 별도 정렬 필요
    - 인덱스 개수 증가로 INSERT/UPDATE 성능 저하
    */

    // 좋은 예시: 쿼리 패턴별 복합 인덱스
    /*
    -- 패턴 1: 내 캐릭터 목록 (PlayerId + 정렬)
    CREATE INDEX IX_Characters_PlayerId_Level_Name
    ON Characters (PlayerId, Level DESC, Name ASC);

    -- 패턴 2: 길드 멤버 목록 (GuildId + 정렬)
    CREATE INDEX IX_Characters_GuildId_Level_Name
    ON Characters (GuildId, Level DESC, Name ASC);

    -- 패턴 3: 전체 랭킹 (레벨 + 경험치)
    CREATE INDEX IX_Characters_Ranking
    ON Characters (Level DESC, Experience DESC)
    INCLUDE (Name, PlayerId, TotalPower);

    장점:
    - Single Index Scan으로 모든 조건 처리
    - ORDER BY도 인덱스로 처리 (Sort 제거)
    - INCLUDE로 커버링 인덱스 구성
    */
}
```

##### 실행 계획 분석 및 최적화
```sql
-- 실행 계획 분석 도구들
SET STATISTICS IO ON;    -- IO 통계 보기
SET STATISTICS TIME ON;  -- 시간 통계 보기

-- 실행 계획 확인
SET SHOWPLAN_ALL ON;
-- 또는 SQL Server Management Studio에서 Ctrl+M

-- 문제가 되는 쿼리 예시
SELECT c.Name, c.Level, p.DisplayName, g.Name as GuildName
FROM Characters c
    INNER JOIN Players p ON c.PlayerId = p.Id
    LEFT JOIN Guilds g ON c.GuildId = g.Id
WHERE c.Level >= 50
    AND p.LastActivityUtc >= '2024-01-01'
ORDER BY c.Level DESC, c.Experience DESC;

-- 실행 계획 문제점 찾기:
-- 1. Table Scan → Index Seek로 변경 필요
-- 2. Hash Join → Nested Loop로 개선 가능한지 확인
-- 3. Sort 연산 → 인덱스로 제거 가능한지 확인
-- 4. Key Lookup → 커버링 인덱스로 제거

-- 최적화된 인덱스
CREATE INDEX IX_Characters_Optimized
ON Characters (Level DESC, Experience DESC)
INCLUDE (Name, PlayerId, GuildId);

CREATE INDEX IX_Players_LastActivity
ON Players (LastActivityUtc)
INCLUDE (Id, DisplayName);

-- 최적화 후 성능 비교
-- Before: CPU: 100ms, Duration: 150ms, Reads: 1000
-- After:  CPU: 10ms,  Duration: 15ms,  Reads: 50
```

#### **오후: EF Core 쿼리 최적화 실습** (4시간)

##### LINQ to SQL 변환 최적화
```csharp
// IdleRPG.Application/Services/CharacterService.cs
public class CharacterService : ICharacterService
{
    private readonly IGameDbContext _context;
    private readonly ILogger<CharacterService> _logger;

    public CharacterService(IGameDbContext context, ILogger<CharacterService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // 나쁜 예시: N+1 문제 발생
    public async Task<List<CharacterSummaryDto>> GetPlayerCharacters_Bad(Guid playerId)
    {
        var characters = await _context.Characters
            .Where(c => c.PlayerId == playerId)
            .ToListAsync(); // 첫 번째 쿼리

        var result = new List<CharacterSummaryDto>();
        foreach (var character in characters)
        {
            // 각 캐릭터마다 추가 쿼리 발생! (N+1 문제)
            var itemCount = await _context.CharacterItems
                .Where(ci => ci.CharacterId == character.Id)
                .CountAsync();

            result.Add(new CharacterSummaryDto
            {
                Id = character.Id,
                Name = character.Name,
                Level = character.Level,
                ItemCount = itemCount // N+1!
            });
        }

        return result;
    }

    // 좋은 예시: 단일 쿼리로 최적화
    public async Task<List<CharacterSummaryDto>> GetPlayerCharacters_Good(Guid playerId)
    {
        var characters = await _context.Characters
            .Where(c => c.PlayerId == playerId)
            .Select(c => new CharacterSummaryDto
            {
                Id = c.Id,
                Name = c.Name,
                Level = c.Level,
                Gold = c.Gold,
                TotalPower = c.TotalPower,
                ItemCount = c.Items.Count(), // 서브쿼리로 한 번에 처리
                EquippedItemCount = c.Items.Count(i => i.IsEquipped),
                GuildName = c.Guild != null ? c.Guild.Name : null
            })
            .OrderByDescending(c => c.Level)
            .ThenBy(c => c.Name)
            .ToListAsync();

        return characters;
    }

    // 복잡한 조건의 최적화된 쿼리
    public async Task<PagedResult<CharacterRankingDto>> GetCharacterRanking(
        int page, int pageSize, int? minLevel = null)
    {
        var query = _context.Characters
            .AsQueryable();

        // 조건부 WHERE 절
        if (minLevel.HasValue)
        {
            query = query.Where(c => c.Level >= minLevel.Value);
        }

        // 총 개수 조회 (COUNT 쿼리)
        var totalCount = await query.CountAsync();

        // 데이터 조회 (최적화된 SELECT)
        var characters = await query
            .OrderByDescending(c => c.Level)
            .ThenByDescending(c => c.Experience)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CharacterRankingDto
            {
                Id = c.Id,
                Name = c.Name,
                Level = c.Level,
                Experience = c.Experience,
                TotalPower = c.TotalPower,
                PlayerName = c.Player.DisplayName,
                GuildName = c.Guild != null ? c.Guild.Name : "길드 없음"
            })
            .ToListAsync();

        return new PagedResult<CharacterRankingDto>
        {
            Items = characters,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };
    }

    // Include vs Select 성능 비교
    public async Task<CharacterDetailDto> GetCharacterDetail_Include(Guid characterId)
    {
        // Include 방식: 모든 관련 데이터 로드 (메모리 사용량 많음)
        var character = await _context.Characters
            .Include(c => c.Player)
            .Include(c => c.Guild)
            .Include(c => c.Items)
                .ThenInclude(ci => ci.ItemTemplate)
            .Include(c => c.Skills)
                .ThenInclude(cs => cs.Skill)
            .FirstOrDefaultAsync(c => c.Id == characterId);

        if (character == null) return null;

        // 매핑 로직...
        return new CharacterDetailDto();
    }

    public async Task<CharacterDetailDto> GetCharacterDetail_Select(Guid characterId)
    {
        // Select 방식: 필요한 데이터만 로드 (성능 우수)
        var characterDetail = await _context.Characters
            .Where(c => c.Id == characterId)
            .Select(c => new CharacterDetailDto
            {
                Id = c.Id,
                Name = c.Name,
                Level = c.Level,
                Experience = c.Experience,
                Gold = c.Gold,
                Attack = c.Attack,
                Defense = c.Defense,
                Speed = c.Speed,
                TotalPower = c.TotalPower,
                Energy = c.Energy,
                MaxEnergy = c.MaxEnergy,

                // 플레이어 정보 (조인)
                PlayerName = c.Player.DisplayName,
                PlayerEmail = c.Player.Email,

                // 길드 정보 (Left Join)
                GuildName = c.Guild != null ? c.Guild.Name : null,
                GuildLevel = c.Guild != null ? c.Guild.Level : (int?)null,

                // 아이템 정보 (서브쿼리)
                Items = c.Items.Select(ci => new CharacterItemDto
                {
                    Id = ci.Id,
                    ItemName = ci.ItemTemplate.Name,
                    Quantity = ci.Quantity,
                    IsEquipped = ci.IsEquipped,
                    EnhancementLevel = ci.EnhancementLevel,
                    AttackBonus = ci.ItemTemplate.AttackBonus,
                    DefenseBonus = ci.ItemTemplate.DefenseBonus,
                    SpeedBonus = ci.ItemTemplate.SpeedBonus
                }).ToList(),

                // 스킬 정보 (서브쿼리)
                Skills = c.Skills.Select(cs => new CharacterSkillDto
                {
                    SkillName = cs.Skill.Name,
                    Level = cs.Level,
                    Description = cs.Skill.Description
                }).ToList()
            })
            .FirstOrDefaultAsync();

        return characterDetail;
    }
}
```

##### 배치 처리 최적화
```csharp
// 대량 데이터 처리 최적화
public class BulkOperationService
{
    private readonly IGameDbContext _context;

    // 나쁜 예시: 개별 INSERT
    public async Task CreateMultipleCharacters_Bad(List<CreateCharacterRequest> requests)
    {
        foreach (var request in requests)
        {
            var character = new Character
            {
                Name = request.Name,
                PlayerId = request.PlayerId
            };

            _context.Characters.Add(character);
            await _context.SaveChangesAsync(); // 각각 개별 트랜잭션!
        }
    }

    // 좋은 예시: 배치 INSERT
    public async Task CreateMultipleCharacters_Good(List<CreateCharacterRequest> requests)
    {
        var characters = requests.Select(request => new Character
        {
            Name = request.Name,
            PlayerId = request.PlayerId,
            CreatedAtUtc = DateTime.UtcNow
        }).ToList();

        _context.Characters.AddRange(characters);
        await _context.SaveChangesAsync(); // 단일 트랜잭션
    }

    // 대량 업데이트 최적화 (EF Core Extensions 사용)
    public async Task BulkUpdateCharacterStats(Dictionary<Guid, CharacterStats> updates)
    {
        // 일반적인 EF Core 방식 (느림)
        /*
        foreach (var kvp in updates)
        {
            var character = await _context.Characters.FindAsync(kvp.Key);
            if (character != null)
            {
                character.Attack = kvp.Value.Attack;
                character.Defense = kvp.Value.Defense;
                character.Speed = kvp.Value.Speed;
                character.TotalPower = kvp.Value.Attack + kvp.Value.Defense + kvp.Value.Speed;
            }
        }
        await _context.SaveChangesAsync();
        */

        // 원시 SQL 사용 (빠름)
        var sql = @"
            UPDATE Characters
            SET Attack = @attack, Defense = @defense, Speed = @speed, TotalPower = @totalPower, UpdatedAtUtc = @now
            WHERE Id = @characterId";

        var now = DateTime.UtcNow;

        foreach (var kvp in updates)
        {
            await _context.Database.ExecuteSqlRawAsync(sql,
                new SqlParameter("@characterId", kvp.Key),
                new SqlParameter("@attack", kvp.Value.Attack),
                new SqlParameter("@defense", kvp.Value.Defense),
                new SqlParameter("@speed", kvp.Value.Speed),
                new SqlParameter("@totalPower", kvp.Value.Attack + kvp.Value.Defense + kvp.Value.Speed),
                new SqlParameter("@now", now));
        }
    }

    // 페이징 최적화 (Offset 문제 해결)
    public async Task<List<Character>> GetCharactersPaged_Offset(int page, int pageSize)
    {
        // 문제: 큰 OFFSET은 성능 저하 (1000번째 페이지 = OFFSET 50000)
        return await _context.Characters
            .OrderBy(c => c.Id)
            .Skip((page - 1) * pageSize) // 큰 값일 때 느림!
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<Character>> GetCharactersPaged_Cursor(Guid? lastCharacterId, int pageSize)
    {
        // 해결: Cursor 기반 페이징 (항상 빠름)
        var query = _context.Characters.AsQueryable();

        if (lastCharacterId.HasValue)
        {
            query = query.Where(c => c.Id.CompareTo(lastCharacterId.Value) > 0);
        }

        return await query
            .OrderBy(c => c.Id)
            .Take(pageSize)
            .ToListAsync();
    }
}
```

---

## **Stage 3: 비동기 & 성능 (3-4일)**

### **3단계 학습 목표**
- C# async/await 패턴을 완전히 마스터하여 게임 서버의 동시성 처리
- Unity의 Coroutine과 웹서버 비동기의 차이점 이해
- 게임 서버에서 발생하는 성능 병목점 식별 및 해결
- 메모리 관리와 GC 최적화로 안정적인 서버 운영

### **Day 1: C# 비동기 프로그래밍 마스터**

#### **오전: async/await 심화 이론** (3시간)

##### Unity Coroutine vs C# async/await 비교
```csharp
// Unity에서 익숙한 Coroutine 방식
public class UnityExample : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(LoadPlayerData());
    }

    IEnumerator LoadPlayerData()
    {
        // HTTP 요청
        using (UnityWebRequest request = UnityWebRequest.Get("api/player"))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // 데이터 파싱
                yield return StartCoroutine(ParsePlayerData(request.downloadHandler.text));
            }
        }
    }

    IEnumerator ParsePlayerData(string jsonData)
    {
        // 무거운 JSON 파싱 작업
        yield return null; // 다음 프레임까지 대기
        var playerData = JsonUtility.FromJson<PlayerData>(jsonData);

        // UI 업데이트
        UpdatePlayerUI(playerData);
    }
}

// 웹서버에서 동등한 async/await 방식
public class PlayerController : ControllerBase
{
    private readonly IPlayerService _playerService;

    [HttpGet("{playerId}")]
    public async Task<ActionResult<PlayerDto>> GetPlayer(Guid playerId)
    {
        // 데이터베이스 비동기 조회 (Unity의 yield return과 유사)
        var player = await _playerService.GetPlayerAsync(playerId);

        if (player == null)
        {
            return NotFound();
        }

        // 추가 데이터 병렬 로드 (Unity에서는 어려운 부분)
        var (characters, guilds, achievements) = await Task.WhenAll(
            _playerService.GetCharactersAsync(playerId),
            _playerService.GetGuildMembershipsAsync(playerId),
            _playerService.GetAchievementsAsync(playerId)
        );

        return Ok(new PlayerDto
        {
            // 매핑...
        });
    }
}
```

##### ConfigureAwait의 중요성 (게임서버 필수 지식)
```csharp
// 웹서버에서는 ConfigureAwait(false) 사용이 성능 최적화의 핵심
public class GameService
{
    // 나쁜 예시: Context 캡처로 인한 성능 저하
    public async Task<GameResult> ProcessGameActionBad(GameAction action)
    {
        // ASP.NET Context가 캡처됨 (불필요한 오버헤드)
        var validationResult = await ValidateAction(action);

        if (!validationResult.IsValid)
        {
            return GameResult.Failure("Invalid action");
        }

        // 여전히 Context 유지 (메모리 낭비)
        var result = await ExecuteAction(action);

        return result;
    }

    // 좋은 예시: ConfigureAwait(false)로 성능 최적화
    public async Task<GameResult> ProcessGameActionGood(GameAction action)
    {
        // Context 캡처 방지로 성능 향상
        var validationResult = await ValidateAction(action).ConfigureAwait(false);

        if (!validationResult.IsValid)
        {
            return GameResult.Failure("Invalid action");
        }

        // 병렬 처리로 성능 극대화
        var (actionResult, logResult) = await Task.WhenAll(
            ExecuteAction(action).ConfigureAwait(false),
            LogAction(action).ConfigureAwait(false)
        ).ConfigureAwait(false);

        return actionResult;
    }

    private async Task<ValidationResult> ValidateAction(GameAction action)
    {
        // 데이터베이스 조회도 ConfigureAwait(false) 적용
        var playerStats = await _dbContext.Characters
            .Where(c => c.Id == action.CharacterId)
            .Select(c => new { c.Energy, c.Level, c.Gold })
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);

        return ValidateActionInternal(action, playerStats);
    }
}
```

##### Task 생명주기와 상태 관리
```csharp
// 게임 서버에서 자주 마주치는 Task 상태 시나리오들
public class TaskLifecycleExamples
{
    // 시나리오 1: 플레이어 동시 접속 처리
    public async Task HandlePlayerLogin(string playerId)
    {
        var loginTask = ProcessLogin(playerId);

        // Task 상태 모니터링
        Console.WriteLine($"Task Status: {loginTask.Status}"); // Running

        try
        {
            var result = await loginTask.ConfigureAwait(false);
            Console.WriteLine($"Task Status: {loginTask.Status}"); // RanToCompletion
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Task Status: {loginTask.Status}"); // Faulted
            Console.WriteLine($"Exception: {ex.Message}");
        }
    }

    // 시나리오 2: 타임아웃 처리 (게임에서 중요)
    public async Task<BattleResult> ProcessBattleWithTimeout(BattleRequest request)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        try
        {
            // 30초 내에 완료되어야 함
            var battleTask = ExecuteBattle(request, cts.Token);
            var timeoutTask = Task.Delay(30000, cts.Token);

            var completedTask = await Task.WhenAny(battleTask, timeoutTask)
                .ConfigureAwait(false);

            if (completedTask == timeoutTask)
            {
                return BattleResult.Timeout();
            }

            return await battleTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return BattleResult.Cancelled();
        }
    }

    // 시나리오 3: 복수 작업 병렬 처리
    public async Task<RaidResult> ProcessRaid(List<Player> players)
    {
        // 모든 플레이어 상태 병렬 확인
        var validationTasks = players.Select(async player =>
        {
            var isOnline = await CheckPlayerOnline(player.Id).ConfigureAwait(false);
            var hasEnergy = await CheckPlayerEnergy(player.Id).ConfigureAwait(false);

            return new { Player = player, IsValid = isOnline && hasEnergy };
        });

        var validationResults = await Task.WhenAll(validationTasks)
            .ConfigureAwait(false);

        var validPlayers = validationResults
            .Where(r => r.IsValid)
            .Select(r => r.Player)
            .ToList();

        if (validPlayers.Count < 2)
        {
            return RaidResult.InsufficientPlayers();
        }

        // 레이드 실행
        return await ExecuteRaid(validPlayers).ConfigureAwait(false);
    }
}
```

##### 데드락 방지 패턴
```csharp
// Unity에서는 잘 발생하지 않지만 웹서버에서는 치명적인 데드락
public class DeadlockExamples
{
    // 위험한 패턴: 동기적 대기
    public ActionResult GetPlayerSync(Guid playerId)
    {
        // 절대 하지 말 것! 데드락 발생 가능
        var player = GetPlayerAsync(playerId).Result;

        return Ok(player);
    }

    // 위험한 패턴: GetAwaiter().GetResult()
    public ActionResult GetPlayerSyncAwaiter(Guid playerId)
    {
        // 이것도 위험함!
        var player = GetPlayerAsync(playerId).GetAwaiter().GetResult();

        return Ok(player);
    }

    // 안전한 패턴 1: 완전 비동기
    public async Task<ActionResult> GetPlayerAsync(Guid playerId)
    {
        var player = await GetPlayerDataAsync(playerId).ConfigureAwait(false);

        return Ok(player);
    }

    // 안전한 패턴 2: ConfigureAwait(false) + 예외 처리
    public async Task<ActionResult> GetPlayerSafeAsync(Guid playerId)
    {
        try
        {
            var player = await GetPlayerDataAsync(playerId).ConfigureAwait(false);

            if (player == null)
            {
                return NotFound($"Player {playerId} not found");
            }

            return Ok(player);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting player {PlayerId}", playerId);
            return StatusCode(500, "Internal server error");
        }
    }
}
```

#### **오후: 실전 비동기 패턴 구현** (4시간)

##### IdleRPG 비동기 서비스 구현
```csharp
// IdleRPG.Application/Services/CharacterProgressService.cs
public class CharacterProgressService : ICharacterProgressService
{
    private readonly IGameDbContext _context;
    private readonly ILogger<CharacterProgressService> _logger;
    private readonly ICacheService _cache;
    private readonly IMessageQueue _messageQueue;

    public CharacterProgressService(
        IGameDbContext context,
        ILogger<CharacterProgressService> logger,
        ICacheService cache,
        IMessageQueue messageQueue)
    {
        _context = context;
        _logger = logger;
        _cache = cache;
        _messageQueue = messageQueue;
    }

    // 복잡한 비동기 워크플로우: 캐릭터 레벨업
    public async Task<LevelUpResult> ProcessLevelUp(Guid characterId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity("CharacterLevelUp");
        activity?.SetTag("character.id", characterId.ToString());

        try
        {
            // 1단계: 캐릭터 정보 조회 (캐시 우선)
            var character = await GetCharacterWithCaching(characterId, cancellationToken)
                .ConfigureAwait(false);

            if (character == null)
            {
                return LevelUpResult.CharacterNotFound();
            }

            // 2단계: 레벨업 조건 확인 (병렬 처리)
            var (canLevelUp, requiredExp, currentExp) = await Task.Run(async () =>
            {
                var required = await CalculateRequiredExperience(character.Level)
                    .ConfigureAwait(false);
                var current = character.Experience;
                var canLevel = current >= required;

                return (canLevel, required, current);
            }, cancellationToken).ConfigureAwait(false);

            if (!canLevelUp)
            {
                return LevelUpResult.InsufficientExperience(requiredExp - currentExp);
            }

            // 3단계: 데이터베이스 트랜잭션 (원자성 보장)
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken)
                .ConfigureAwait(false);

            try
            {
                // 레벨업 처리
                var oldLevel = character.Level;
                var newLevel = oldLevel + 1;

                // 스탯 증가 계산
                var statGains = await CalculateStatGains(character, newLevel, cancellationToken)
                    .ConfigureAwait(false);

                // 데이터베이스 업데이트
                character.Level = newLevel;
                character.Experience -= requiredExp;
                character.Attack += statGains.Attack;
                character.Defense += statGains.Defense;
                character.Speed += statGains.Speed;
                character.TotalPower = character.Attack + character.Defense + character.Speed;
                character.UpdatedAtUtc = DateTime.UtcNow;

                await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                // 레벨업 로그 기록
                var levelUpLog = new CharacterLevelUpLog
                {
                    CharacterId = characterId,
                    OldLevel = oldLevel,
                    NewLevel = newLevel,
                    StatGains = statGains,
                    Timestamp = DateTime.UtcNow
                };

                _context.CharacterLevelUpLogs.Add(levelUpLog);
                await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

                // 4단계: 캐시 무효화 및 이벤트 발생 (비동기 백그라운드)
                var backgroundTasks = new[]
                {
                    InvalidateCharacterCache(characterId),
                    PublishLevelUpEvent(character, oldLevel, newLevel),
                    UpdateCharacterRanking(character),
                    CheckAchievements(character)
                };

                // 백그라운드 작업들은 await 하지 않음 (Fire-and-forget)
                _ = Task.WhenAll(backgroundTasks).ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        _logger.LogError(task.Exception,
                            "Background tasks failed for character {CharacterId}", characterId);
                    }
                }, TaskScheduler.Default);

                return LevelUpResult.Success(newLevel, statGains);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                throw;
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Level up cancelled for character {CharacterId}", characterId);
            return LevelUpResult.Cancelled();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing level up for character {CharacterId}", characterId);
            return LevelUpResult.Error(ex.Message);
        }
    }

    // 캐시를 활용한 비동기 조회
    private async Task<Character> GetCharacterWithCaching(Guid characterId, CancellationToken cancellationToken)
    {
        var cacheKey = $"character:{characterId}";

        // 캐시 확인
        var cachedCharacter = await _cache.GetAsync<Character>(cacheKey, cancellationToken)
            .ConfigureAwait(false);

        if (cachedCharacter != null)
        {
            _logger.LogDebug("Character {CharacterId} loaded from cache", characterId);
            return cachedCharacter;
        }

        // 데이터베이스에서 조회
        var character = await _context.Characters
            .Include(c => c.Player)
            .Include(c => c.Guild)
            .FirstOrDefaultAsync(c => c.Id == characterId, cancellationToken)
            .ConfigureAwait(false);

        if (character != null)
        {
            // 캐시에 저장 (5분 TTL)
            await _cache.SetAsync(cacheKey, character, TimeSpan.FromMinutes(5), cancellationToken)
                .ConfigureAwait(false);

            _logger.LogDebug("Character {CharacterId} loaded from database and cached", characterId);
        }

        return character;
    }

    // 병렬 처리를 활용한 배치 작업
    public async Task<BatchResult> ProcessMultipleCharacters(
        List<Guid> characterIds,
        Func<Character, Task<bool>> processor,
        CancellationToken cancellationToken = default)
    {
        const int maxConcurrency = 10; // 동시 처리 제한

        using var semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
        var results = new ConcurrentBag<ProcessResult>();

        var tasks = characterIds.Select(async characterId =>
        {
            await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                var character = await GetCharacterWithCaching(characterId, cancellationToken)
                    .ConfigureAwait(false);

                if (character == null)
                {
                    results.Add(ProcessResult.NotFound(characterId));
                    return;
                }

                var success = await processor(character).ConfigureAwait(false);
                results.Add(success ?
                    ProcessResult.Success(characterId) :
                    ProcessResult.Failed(characterId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing character {CharacterId}", characterId);
                results.Add(ProcessResult.Error(characterId, ex.Message));
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks).ConfigureAwait(false);

        return new BatchResult(results.ToList());
    }
}
```

### **Day 2: 메모리 관리 & GC 최적화**

#### **오전: .NET GC 이해와 모니터링** (3시간)

##### Unity vs .NET Server GC 차이점
```csharp
// Unity에서는 GC가 게임 프레임에 영향을 줌
public class UnityGCExample : MonoBehaviour
{
    void Update()
    {
        // Unity에서 자주 하는 실수들 (GC 압박)

        // 1. 매 프레임 new 연산
        Vector3 playerPos = new Vector3(x, y, z); // GC Alloc!

        // 2. string 연산
        string debugText = "Player HP: " + playerHP; // GC Alloc!

        // 3. LINQ 사용
        var activeEnemies = enemies.Where(e => e.IsActive).ToList(); // GC Alloc!

        // 4. Boxing
        Dictionary<string, object> data = new Dictionary<string, object>();
        data["level"] = playerLevel; // int -> object boxing!
    }
}

// 웹서버에서는 다른 방식의 GC 최적화가 필요
public class ServerGCOptimization
{
    // 객체 풀링으로 GC 압박 감소
    private static readonly ObjectPool<StringBuilder> StringBuilderPool =
        new DefaultObjectPool<StringBuilder>(new StringBuilderPooledObjectPolicy());

    private static readonly ObjectPool<List<Character>> ListPool =
        new DefaultObjectPool<List<Character>>(new ListPooledObjectPolicy<Character>());

    public string FormatPlayerData(Player player)
    {
        // StringBuilder 풀 사용
        var sb = StringBuilderPool.Get();
        try
        {
            sb.Clear();
            sb.Append("Player: ");
            sb.Append(player.DisplayName);
            sb.Append(", Level: ");
            sb.Append(player.Level);
            sb.Append(", Gold: ");
            sb.Append(player.Gold);

            return sb.ToString();
        }
        finally
        {
            StringBuilderPool.Return(sb);
        }
    }

    public async Task<List<Character>> GetActiveCharacters(Guid playerId)
    {
        var characters = ListPool.Get();
        try
        {
            characters.Clear();

            // 데이터베이스에서 조회
            var dbCharacters = await _context.Characters
                .Where(c => c.PlayerId == playerId && c.IsActive)
                .ToListAsync()
                .ConfigureAwait(false);

            characters.AddRange(dbCharacters);

            // 새 List를 반환 (풀링된 List는 재사용)
            return new List<Character>(characters);
        }
        finally
        {
            ListPool.Return(characters);
        }
    }
}
```

##### 메모리 누수 감지 및 해결
```csharp
// 메모리 누수의 일반적인 원인들
public class MemoryLeakExamples
{
    // 문제 1: Event Handler 누수
    public class PlayerService
    {
        public event Action<Player> PlayerLevelUp;

        private readonly Timer _cleanupTimer;

        public PlayerService()
        {
            // 타이머가 이 객체를 참조하게 되어 GC 방지
            _cleanupTimer = new Timer(DoCleanup, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));
        }

        private void DoCleanup(object state)
        {
            // 정리 작업
        }

        // Dispose 패턴 미구현으로 메모리 누수
    }

    // 해결책: 제대로 된 Dispose 패턴
    public class PlayerServiceFixed : IDisposable
    {
        public event Action<Player> PlayerLevelUp;

        private readonly Timer _cleanupTimer;
        private bool _disposed = false;

        public PlayerServiceFixed()
        {
            _cleanupTimer = new Timer(DoCleanup, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));
        }

        private void DoCleanup(object state)
        {
            if (_disposed) return;

            // 정리 작업
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _cleanupTimer?.Dispose();
                    PlayerLevelUp = null; // 이벤트 핸들러 정리
                }

                _disposed = true;
            }
        }
    }

    // 문제 2: 캐시에서 만료된 항목 미제거
    public class BadCacheService
    {
        private static readonly ConcurrentDictionary<string, CacheItem> _cache = new();

        public void Set<T>(string key, T value, TimeSpan expiry)
        {
            _cache[key] = new CacheItem
            {
                Value = value,
                ExpiresAt = DateTime.UtcNow.Add(expiry)
            };
            // 만료된 항목 정리 없음 -> 메모리 누수!
        }

        public T Get<T>(string key)
        {
            if (_cache.TryGetValue(key, out var item))
            {
                if (DateTime.UtcNow < item.ExpiresAt)
                {
                    return (T)item.Value;
                }
                // 만료된 항목을 제거하지 않음!
            }

            return default(T);
        }
    }

    // 해결책: 자동 만료 처리가 있는 캐시
    public class GoodCacheService : IDisposable
    {
        private readonly ConcurrentDictionary<string, CacheItem> _cache = new();
        private readonly Timer _cleanupTimer;

        public GoodCacheService()
        {
            // 주기적으로 만료된 항목 제거
            _cleanupTimer = new Timer(CleanupExpiredItems, null,
                TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
        }

        public void Set<T>(string key, T value, TimeSpan expiry)
        {
            _cache[key] = new CacheItem
            {
                Value = value,
                ExpiresAt = DateTime.UtcNow.Add(expiry)
            };
        }

        public T Get<T>(string key)
        {
            if (_cache.TryGetValue(key, out var item))
            {
                if (DateTime.UtcNow < item.ExpiresAt)
                {
                    return (T)item.Value;
                }
                else
                {
                    // 만료된 항목 즉시 제거
                    _cache.TryRemove(key, out _);
                }
            }

            return default(T);
        }

        private void CleanupExpiredItems(object state)
        {
            var now = DateTime.UtcNow;
            var expiredKeys = _cache
                .Where(kvp => now >= kvp.Value.ExpiresAt)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                _cache.TryRemove(key, out _);
            }
        }

        public void Dispose()
        {
            _cleanupTimer?.Dispose();
            _cache.Clear();
        }
    }
}
```

#### **오후: 성능 프로파일링 실습** (4시간)

##### dotMemory와 PerfView 사용법
```csharp
// 성능 측정을 위한 커스텀 벤치마킹
public class PerformanceBenchmark
{
    private readonly Stopwatch _stopwatch = new();
    private readonly List<long> _measurements = new();

    public IDisposable StartMeasurement(string operationName)
    {
        return new MeasurementScope(this, operationName);
    }

    private class MeasurementScope : IDisposable
    {
        private readonly PerformanceBenchmark _benchmark;
        private readonly string _operationName;
        private readonly long _startMemory;

        public MeasurementScope(PerformanceBenchmark benchmark, string operationName)
        {
            _benchmark = benchmark;
            _operationName = operationName;

            // GC 강제 수행 후 메모리 측정
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            _startMemory = GC.GetTotalMemory(false);
            _benchmark._stopwatch.Restart();
        }

        public void Dispose()
        {
            _benchmark._stopwatch.Stop();

            var endMemory = GC.GetTotalMemory(false);
            var memoryUsed = endMemory - _startMemory;

            Console.WriteLine($"{_operationName}:");
            Console.WriteLine($"  Time: {_benchmark._stopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine($"  Memory: {memoryUsed:N0} bytes");
            Console.WriteLine($"  Gen0 Collections: {GC.CollectionCount(0)}");
            Console.WriteLine($"  Gen1 Collections: {GC.CollectionCount(1)}");
            Console.WriteLine($"  Gen2 Collections: {GC.CollectionCount(2)}");

            _benchmark._measurements.Add(_benchmark._stopwatch.ElapsedMilliseconds);
        }
    }

    // 게임 서버 시나리오별 벤치마킹
    public async Task BenchmarkCharacterOperations()
    {
        const int iterations = 1000;

        // 시나리오 1: 단순 캐릭터 조회
        using (_benchmark.StartMeasurement("Simple Character Query"))
        {
            for (int i = 0; i < iterations; i++)
            {
                var character = await GetCharacterSimple(Guid.NewGuid());
            }
        }

        // 시나리오 2: 복잡한 캐릭터 조회 (N+1 문제)
        using (_benchmark.StartMeasurement("Complex Character Query (N+1)"))
        {
            for (int i = 0; i < iterations; i++)
            {
                var character = await GetCharacterWithNPlusOneProblem(Guid.NewGuid());
            }
        }

        // 시나리오 3: 최적화된 캐릭터 조회
        using (_benchmark.StartMeasurement("Optimized Character Query"))
        {
            for (int i = 0; i < iterations; i++)
            {
                var character = await GetCharacterOptimized(Guid.NewGuid());
            }
        }
    }
}

// ASP.NET Core에서 메모리 사용량 모니터링
public class MemoryMonitoringMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<MemoryMonitoringMiddleware> _logger;

    public MemoryMonitoringMiddleware(RequestDelegate next, ILogger<MemoryMonitoringMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var startMemory = GC.GetTotalMemory(false);
        var startTime = Stopwatch.GetTimestamp();

        try
        {
            await _next(context);
        }
        finally
        {
            var endTime = Stopwatch.GetTimestamp();
            var endMemory = GC.GetTotalMemory(false);

            var elapsedMs = (endTime - startTime) * 1000.0 / Stopwatch.Frequency;
            var memoryDelta = endMemory - startMemory;

            // 임계값을 넘으면 로그 기록
            if (elapsedMs > 1000 || memoryDelta > 1024 * 1024) // 1초 또는 1MB
            {
                _logger.LogWarning(
                    "Slow request detected. Path: {Path}, Method: {Method}, Duration: {Duration}ms, Memory: {Memory} bytes",
                    context.Request.Path,
                    context.Request.Method,
                    elapsedMs,
                    memoryDelta);
            }
        }
    }
}
```

### **Day 3: 동시성과 스레드 안전성**

#### **오전: 동시성 문제 식별** (3시간)

##### Race Condition 시나리오와 해결책
```csharp
// 게임 서버에서 자주 발생하는 Race Condition
public class GameResourceService
{
    // 문제: 플레이어 골드 업데이트 시 Race Condition
    private static readonly ConcurrentDictionary<Guid, Player> _playerCache = new();

    // 위험한 코드: 읽기-수정-쓰기 패턴
    public async Task<bool> SpendGold_Unsafe(Guid playerId, int amount)
    {
        var player = await GetPlayer(playerId);

        if (player.Gold < amount)
        {
            return false; // 골드 부족
        }

        // 여기서 다른 스레드가 골드를 변경할 수 있음!
        await Task.Delay(10); // 네트워크 지연 시뮬레이션

        player.Gold -= amount; // Race Condition 발생 가능!
        await SavePlayer(player);

        return true;
    }

    // 해결책 1: 락을 사용한 동기화
    private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> _playerLocks = new();

    public async Task<bool> SpendGold_WithLock(Guid playerId, int amount)
    {
        var playerLock = _playerLocks.GetOrAdd(playerId, _ => new SemaphoreSlim(1, 1));

        await playerLock.WaitAsync();
        try
        {
            var player = await GetPlayer(playerId);

            if (player.Gold < amount)
            {
                return false;
            }

            player.Gold -= amount;
            await SavePlayer(player);

            return true;
        }
        finally
        {
            playerLock.Release();
        }
    }

    // 해결책 2: 원자적 데이터베이스 연산
    public async Task<bool> SpendGold_Atomic(Guid playerId, int amount)
    {
        // SQL에서 원자적으로 처리
        var rowsAffected = await _context.Database.ExecuteSqlRawAsync(@"
            UPDATE Players
            SET Gold = Gold - @amount, UpdatedAtUtc = @now
            WHERE Id = @playerId AND Gold >= @amount",
            new SqlParameter("@playerId", playerId),
            new SqlParameter("@amount", amount),
            new SqlParameter("@now", DateTime.UtcNow));

        return rowsAffected > 0;
    }

    // 해결책 3: Optimistic Concurrency Control
    public async Task<bool> SpendGold_OptimisticLocking(Guid playerId, int amount)
    {
        const int maxRetries = 3;

        for (int retry = 0; retry < maxRetries; retry++)
        {
            try
            {
                var player = await _context.Players
                    .FirstOrDefaultAsync(p => p.Id == playerId);

                if (player == null || player.Gold < amount)
                {
                    return false;
                }

                player.Gold -= amount;
                player.UpdatedAtUtc = DateTime.UtcNow;

                // RowVersion으로 동시성 체크
                await _context.SaveChangesAsync();

                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                // 다른 스레드가 먼저 업데이트함, 재시도
                if (retry == maxRetries - 1)
                {
                    throw; // 최대 재시도 횟수 초과
                }

                await Task.Delay(TimeSpan.FromMilliseconds(50 * (retry + 1))); // 지수 백오프
            }
        }

        return false;
    }
}
```

##### 스레드 안전한 컬렉션 활용
```csharp
// 게임 서버의 실시간 데이터 관리
public class OnlinePlayerManager
{
    // 스레드 안전한 컬렉션들
    private readonly ConcurrentDictionary<Guid, PlayerSession> _onlinePlayers = new();
    private readonly ConcurrentQueue<GameEvent> _eventQueue = new();
    private readonly ConcurrentBag<string> _recentActions = new();

    // 일반 컬렉션 + 락 (성능이 중요하지 않은 경우)
    private readonly HashSet<Guid> _bannedPlayers = new();
    private readonly object _bannedPlayersLock = new();

    public void AddPlayer(PlayerSession session)
    {
        _onlinePlayers.TryAdd(session.PlayerId, session);

        // 이벤트 큐에 추가
        _eventQueue.Enqueue(new GameEvent
        {
            Type = GameEventType.PlayerJoined,
            PlayerId = session.PlayerId,
            Timestamp = DateTime.UtcNow
        });
    }

    public void RemovePlayer(Guid playerId)
    {
        if (_onlinePlayers.TryRemove(playerId, out var session))
        {
            session.Dispose(); // 리소스 정리

            _eventQueue.Enqueue(new GameEvent
            {
                Type = GameEventType.PlayerLeft,
                PlayerId = playerId,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    public bool IsPlayerBanned(Guid playerId)
    {
        lock (_bannedPlayersLock)
        {
            return _bannedPlayers.Contains(playerId);
        }
    }

    public void BanPlayer(Guid playerId)
    {
        lock (_bannedPlayersLock)
        {
            _bannedPlayers.Add(playerId);
        }

        // 온라인이면 강제 로그아웃
        RemovePlayer(playerId);
    }

    // 이벤트 처리 루프 (백그라운드 서비스)
    public async Task ProcessEvents(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (_eventQueue.TryDequeue(out var gameEvent))
            {
                await ProcessGameEvent(gameEvent);
            }
            else
            {
                // 큐가 비어있으면 잠시 대기
                await Task.Delay(100, cancellationToken);
            }
        }
    }

    // 플레이어 통계 (스레드 안전)
    public PlayerStats GetPlayerStats()
    {
        return new PlayerStats
        {
            OnlineCount = _onlinePlayers.Count,
            BannedCount = GetBannedCount(),
            RecentActionsCount = _recentActions.Count
        };
    }

    private int GetBannedCount()
    {
        lock (_bannedPlayersLock)
        {
            return _bannedPlayers.Count;
        }
    }
}
```

#### **오후: 고성능 동시 처리 구현** (4시간)

##### Producer-Consumer 패턴으로 게임 이벤트 처리
```csharp
// 고성능 게임 이벤트 처리 시스템
public class GameEventProcessor : BackgroundService
{
    private readonly Channel<GameEvent> _eventChannel;
    private readonly ChannelWriter<GameEvent> _writer;
    private readonly ChannelReader<GameEvent> _reader;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<GameEventProcessor> _logger;

    public GameEventProcessor(IServiceProvider serviceProvider, ILogger<GameEventProcessor> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        // 채널 생성 (고성능 큐)
        var options = new BoundedChannelOptions(10000)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = false, // 여러 컨슈머 허용
            SingleWriter = false  // 여러 프로듀서 허용
        };

        _eventChannel = Channel.CreateBounded<GameEvent>(options);
        _writer = _eventChannel.Writer;
        _reader = _eventChannel.Reader;
    }

    // 이벤트 발행 (논블로킹)
    public async ValueTask<bool> PublishEventAsync(GameEvent gameEvent)
    {
        return await _writer.WaitToWriteAsync() && _writer.TryWrite(gameEvent);
    }

    // 백그라운드에서 이벤트 처리
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // 멀티 컨슈머로 처리 성능 향상
        var consumerTasks = new List<Task>();

        for (int i = 0; i < Environment.ProcessorCount; i++)
        {
            consumerTasks.Add(ProcessEventsAsync(stoppingToken, i));
        }

        await Task.WhenAll(consumerTasks);
    }

    private async Task ProcessEventsAsync(CancellationToken cancellationToken, int consumerId)
    {
        _logger.LogInformation("Event consumer {ConsumerId} started", consumerId);

        try
        {
            await foreach (var gameEvent in _reader.ReadAllAsync(cancellationToken))
            {
                using var scope = _serviceProvider.CreateScope();
                var eventHandler = scope.ServiceProvider.GetRequiredService<IGameEventHandler>();

                try
                {
                    await eventHandler.HandleAsync(gameEvent);
                    _logger.LogDebug("Consumer {ConsumerId} processed event {EventType} for player {PlayerId}",
                        consumerId, gameEvent.Type, gameEvent.PlayerId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Consumer {ConsumerId} failed to process event {EventType} for player {PlayerId}",
                        consumerId, gameEvent.Type, gameEvent.PlayerId);

                    // 실패한 이벤트는 DLQ(Dead Letter Queue)로 이동
                    await SendToDeadLetterQueue(gameEvent, ex);
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Event consumer {ConsumerId} stopped", consumerId);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _writer.Complete();
        await base.StopAsync(cancellationToken);
    }
}

// 배치 처리로 데이터베이스 성능 최적화
public class CharacterStatsBatchProcessor
{
    private readonly IGameDbContext _context;
    private readonly ILogger<CharacterStatsBatchProcessor> _logger;

    // 배치 크기와 처리 간격 설정
    private const int BatchSize = 100;
    private const int ProcessingIntervalMs = 1000;

    private readonly Timer _processingTimer;
    private readonly ConcurrentQueue<CharacterStatsUpdate> _pendingUpdates = new();
    private volatile bool _isProcessing = false;

    public CharacterStatsBatchProcessor(IGameDbContext context, ILogger<CharacterStatsBatchProcessor> logger)
    {
        _context = context;
        _logger = logger;

        _processingTimer = new Timer(ProcessBatch, null,
            TimeSpan.FromMilliseconds(ProcessingIntervalMs),
            TimeSpan.FromMilliseconds(ProcessingIntervalMs));
    }

    public void QueueStatsUpdate(CharacterStatsUpdate update)
    {
        _pendingUpdates.Enqueue(update);
    }

    private async void ProcessBatch(object state)
    {
        if (_isProcessing) return; // 이전 배치가 아직 처리 중

        _isProcessing = true;

        try
        {
            var batch = new List<CharacterStatsUpdate>();

            // 배치 크기만큼 큐에서 가져오기
            for (int i = 0; i < BatchSize && _pendingUpdates.TryDequeue(out var update); i++)
            {
                batch.Add(update);
            }

            if (batch.Count == 0) return;

            await ProcessUpdatesBatch(batch);

            _logger.LogDebug("Processed {Count} character stats updates", batch.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing character stats batch");
        }
        finally
        {
            _isProcessing = false;
        }
    }

    private async Task ProcessUpdatesBatch(List<CharacterStatsUpdate> batch)
    {
        // 캐릭터 ID별로 그룹화 (같은 캐릭터의 중복 업데이트 병합)
        var groupedUpdates = batch
            .GroupBy(u => u.CharacterId)
            .Select(g => g.Aggregate((latest, current) =>
                current.Timestamp > latest.Timestamp ? current : latest))
            .ToList();

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // SQL MERGE 문으로 고성능 업데이트
            var sql = @"
                UPDATE Characters
                SET
                    Attack = @attack,
                    Defense = @defense,
                    Speed = @speed,
                    TotalPower = @attack + @defense + @speed,
                    UpdatedAtUtc = @timestamp
                WHERE Id = @characterId";

            foreach (var update in groupedUpdates)
            {
                await _context.Database.ExecuteSqlRawAsync(sql,
                    new SqlParameter("@characterId", update.CharacterId),
                    new SqlParameter("@attack", update.Attack),
                    new SqlParameter("@defense", update.Defense),
                    new SqlParameter("@speed", update.Speed),
                    new SqlParameter("@timestamp", update.Timestamp));
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();

            // 실패한 업데이트들을 다시 큐에 추가 (재시도)
            foreach (var update in groupedUpdates)
            {
                _pendingUpdates.Enqueue(update);
            }

            throw;
        }
    }

    public void Dispose()
    {
        _processingTimer?.Dispose();

        // 남은 업데이트들 처리
        if (_pendingUpdates.Count > 0)
        {
            _logger.LogInformation("Processing {Count} remaining updates during shutdown",
                _pendingUpdates.Count);

            var remainingUpdates = new List<CharacterStatsUpdate>();
            while (_pendingUpdates.TryDequeue(out var update))
            {
                remainingUpdates.Add(update);
            }

            if (remainingUpdates.Count > 0)
            {
                ProcessUpdatesBatch(remainingUpdates).GetAwaiter().GetResult();
            }
        }
    }
}
```

### **3단계 체크리스트**

#### **Day 1 완료 기준**
- [ ] Unity Coroutine과 async/await 차이점 완전 이해
- [ ] ConfigureAwait(false) 사용 시점과 이유 설명 가능
- [ ] Task 생명주기와 상태 전환 이해
- [ ] 데드락 발생 원인과 예방법 숙지
- [ ] 실제 게임 서버 비동기 코드 작성 완료

#### **Day 2 완료 기준**
- [ ] .NET GC 작동 방식과 세대별 수집 이해
- [ ] ObjectPool 활용한 메모리 최적화 구현
- [ ] 메모리 누수 패턴 식별 및 해결
- [ ] 성능 프로파일링 도구 사용법 습득
- [ ] 게임 서버 메모리 모니터링 시스템 구축

#### **Day 3 완료 기준**
- [ ] Race Condition 시나리오 식별 및 해결책 구현
- [ ] 스레드 안전한 컬렉션 적절한 활용
- [ ] Producer-Consumer 패턴으로 이벤트 처리 시스템 구현
- [ ] 배치 처리를 통한 데이터베이스 성능 최적화
- [ ] 동시성 제어 메커니즘 (Lock, Semaphore, Channel) 활용

#### **전체 평가 기준**
- [ ] 1만 동접 게임 서버의 비동기 요구사항 이해
- [ ] Unity 개발 경험을 웹서버 개발에 효과적으로 연결
- [ ] 성능 병목점 식별 및 최적화 전략 수립 능력
- [ ] 실제 IdleRPG 서버에 적용 가능한 코드 작성

---

## **Stage 4: 보안 & JWT 심화 (3-4일)**

### **4단계 학습 목표**
- 웹 보안 취약점과 게임 서버 특화 보안 위협 이해
- JWT 토큰 시스템을 활용한 안전한 인증/인가 구현
- OWASP Top 10을 기반으로 한 실전 보안 대책 수립
- 게임 서버에서 발생하는 보안 이슈 (치팅, 중복 로그인 등) 해결

### **Day 1: 웹 보안 기초와 게임 서버 보안**

#### **오전: OWASP Top 10 & 게임 서버 취약점** (3시간)

##### Unity 클라이언트 vs 웹 클라이언트 보안 차이점
```csharp
// Unity 게임에서의 보안 사고방식
public class UnitySecurityExample : MonoBehaviour
{
    // Unity에서는 클라이언트가 신뢰할 수 없음을 전제
    void Start()
    {
        // 클라이언트에서 점수 계산 (위험!)
        int score = CalculateScore();

        // 서버로 점수 전송 - 검증 없이 믿으면 안됨
        StartCoroutine(SendScoreToServer(score));
    }

    int CalculateScore()
    {
        // 클라이언트 로직은 언제든 조작 가능
        return playerKills * 100 + timeBonus;
    }

    IEnumerator SendScoreToServer(int score)
    {
        // 네트워크 통신은 항상 암호화되지 않을 수 있음
        string url = $"https://game-server.com/api/submit-score?score={score}";

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Score submitted successfully");
            }
        }
    }
}

// 웹 서버에서는 모든 입력을 검증해야 함
[ApiController]
[Route("api/[controller]")]
public class GameScoreController : ControllerBase
{
    private readonly IGameScoreService _scoreService;
    private readonly ILogger<GameScoreController> _logger;
    private readonly ICurrentUserService _currentUser;

    [HttpPost("submit-score")]
    [Authorize] // 인증 필수
    public async Task<ActionResult<ScoreSubmissionResult>> SubmitScore(
        [FromBody] SubmitScoreRequest request)
    {
        // 1. 입력 검증 (Validation)
        if (request.Score < 0 || request.Score > 1000000)
        {
            _logger.LogWarning("Invalid score submitted: {Score} by user {UserId}",
                request.Score, _currentUser.UserId);
            return BadRequest("Invalid score value");
        }

        // 2. 비즈니스 로직 검증 (서버에서 재계산)
        var gameSession = await _scoreService.GetActiveGameSession(_currentUser.UserId);
        if (gameSession == null)
        {
            return BadRequest("No active game session found");
        }

        // 3. 서버에서 점수 재계산으로 치팅 방지
        var calculatedScore = await _scoreService.CalculateServerScore(gameSession);
        var scoreDifference = Math.Abs(request.Score - calculatedScore);

        // 4. 임계값을 넘으면 치팅 의심
        if (scoreDifference > calculatedScore * 0.1) // 10% 오차 허용
        {
            _logger.LogWarning("Potential cheating detected. Client: {ClientScore}, Server: {ServerScore}, User: {UserId}",
                request.Score, calculatedScore, _currentUser.UserId);

            // 치팅 의심 사용자 플래그
            await _scoreService.FlagPotentialCheater(_currentUser.UserId, request.Score, calculatedScore);

            return BadRequest("Score validation failed");
        }

        // 5. 점수 저장 (서버 계산 값 사용)
        var result = await _scoreService.SaveScore(_currentUser.UserId, calculatedScore);

        return Ok(result);
    }
}
```

##### SQL Injection 방지 (게임 서버 특화)
```csharp
// 게임 서버에서 자주 발생하는 SQL Injection 시나리오들
public class GameDataService
{
    private readonly IGameDbContext _context;

    // 위험한 코드: 동적 SQL 생성
    public async Task<List<Character>> SearchCharacters_Vulnerable(string characterName, string guildName)
    {
        // 절대 이렇게 하지 말 것!
        string sql = $@"
            SELECT * FROM Characters c
            INNER JOIN Guilds g ON c.GuildId = g.Id
            WHERE c.Name LIKE '%{characterName}%'
            AND g.Name = '{guildName}'";

        return await _context.Characters.FromSqlRaw(sql).ToListAsync();

        /*
        공격 예시:
        characterName = "'; DROP TABLE Characters; --"
        guildName = "' OR '1'='1"

        결과 SQL:
        SELECT * FROM Characters c
        INNER JOIN Guilds g ON c.GuildId = g.Id
        WHERE c.Name LIKE '%'; DROP TABLE Characters; --%'
        AND g.Name = '' OR '1'='1'
        */
    }

    // 안전한 코드: 매개변수화된 쿼리
    public async Task<List<Character>> SearchCharacters_Safe(string characterName, string guildName)
    {
        // EF Core의 매개변수화된 쿼리 사용
        return await _context.Characters
            .Include(c => c.Guild)
            .Where(c => c.Name.Contains(characterName) && c.Guild.Name == guildName)
            .ToListAsync();
    }

    // 복잡한 쿼리가 필요한 경우: 매개변수 사용
    public async Task<List<PlayerRanking>> GetPlayerRanking_Safe(
        int minLevel, string serverRegion, DateTime startDate)
    {
        var sql = @"
            SELECT
                p.Id as PlayerId,
                p.DisplayName,
                c.Name as CharacterName,
                c.Level,
                c.TotalPower,
                RANK() OVER (ORDER BY c.TotalPower DESC) as Rank
            FROM Players p
            INNER JOIN Characters c ON p.Id = c.PlayerId
            WHERE c.Level >= @minLevel
            AND p.Region = @region
            AND c.CreatedAtUtc >= @startDate
            ORDER BY c.TotalPower DESC";

        return await _context.PlayerRankings
            .FromSqlRaw(sql,
                new SqlParameter("@minLevel", minLevel),
                new SqlParameter("@region", serverRegion),
                new SqlParameter("@startDate", startDate))
            .ToListAsync();
    }

    // 동적 정렬과 필터링을 안전하게 처리
    public async Task<List<Character>> GetCharactersWithDynamicFilter(
        CharacterSearchRequest request)
    {
        var query = _context.Characters.Include(c => c.Guild).AsQueryable();

        // 안전한 동적 필터링
        if (!string.IsNullOrEmpty(request.CharacterName))
        {
            query = query.Where(c => c.Name.Contains(request.CharacterName));
        }

        if (request.MinLevel.HasValue)
        {
            query = query.Where(c => c.Level >= request.MinLevel.Value);
        }

        if (request.GuildId.HasValue)
        {
            query = query.Where(c => c.GuildId == request.GuildId.Value);
        }

        // 안전한 동적 정렬 (화이트리스트 방식)
        query = request.SortBy?.ToLower() switch
        {
            "level" => request.SortDirection == "desc"
                ? query.OrderByDescending(c => c.Level)
                : query.OrderBy(c => c.Level),
            "power" => request.SortDirection == "desc"
                ? query.OrderByDescending(c => c.TotalPower)
                : query.OrderBy(c => c.TotalPower),
            "name" => request.SortDirection == "desc"
                ? query.OrderByDescending(c => c.Name)
                : query.OrderBy(c => c.Name),
            _ => query.OrderByDescending(c => c.Level) // 기본 정렬
        };

        return await query
            .Skip(request.Skip)
            .Take(Math.Min(request.Take, 100)) // 최대 100개 제한
            .ToListAsync();
    }
}
```

##### XSS 방지 (게임 채팅, 길드 이름 등)
```csharp
// 게임에서 사용자 입력 처리 시 XSS 방지
public class GameContentService
{
    private readonly ILogger<GameContentService> _logger;
    private readonly HtmlSanitizer _htmlSanitizer;

    public GameContentService(ILogger<GameContentService> logger)
    {
        _logger = logger;

        // HtmlSanitizer 설정 (AntiXSS 라이브러리)
        _htmlSanitizer = new HtmlSanitizer();
        _htmlSanitizer.AllowedTags.Clear(); // 모든 HTML 태그 금지
        _htmlSanitizer.AllowedAttributes.Clear();
    }

    // 게임 채팅 메시지 처리
    public async Task<ChatMessageResult> ProcessChatMessage(Guid playerId, string message)
    {
        // 1. 입력 검증
        if (string.IsNullOrWhiteSpace(message))
        {
            return ChatMessageResult.Error("Empty message");
        }

        if (message.Length > 500)
        {
            return ChatMessageResult.Error("Message too long");
        }

        // 2. XSS 방지: HTML 태그 제거
        var sanitizedMessage = _htmlSanitizer.Sanitize(message);

        // 3. 추가 보안: 스크립트 패턴 검사
        if (ContainsScriptPattern(sanitizedMessage))
        {
            _logger.LogWarning("Potential XSS attempt in chat message from player {PlayerId}: {Message}",
                playerId, message);
            return ChatMessageResult.Error("Invalid message content");
        }

        // 4. 욕설/금지어 필터링
        var filteredMessage = await ApplyProfanityFilter(sanitizedMessage);

        // 5. 스팸 방지 체크
        if (await IsSpamMessage(playerId, filteredMessage))
        {
            return ChatMessageResult.Error("Message sent too frequently");
        }

        // 6. 메시지 저장 및 브로드캐스트
        var chatMessage = new ChatMessage
        {
            PlayerId = playerId,
            Content = filteredMessage,
            SanitizedContent = sanitizedMessage,
            OriginalContent = message, // 로깅용
            Timestamp = DateTime.UtcNow,
            MessageType = ChatMessageType.General
        };

        await _context.ChatMessages.AddAsync(chatMessage);
        await _context.SaveChangesAsync();

        // 실시간 브로드캐스트 (SignalR)
        await BroadcastChatMessage(chatMessage);

        return ChatMessageResult.Success(chatMessage);
    }

    // 길드 이름/설명 처리
    public async Task<ValidationResult> ValidateGuildContent(string guildName, string description)
    {
        var errors = new List<string>();

        // 길드 이름 검증
        if (string.IsNullOrWhiteSpace(guildName))
        {
            errors.Add("Guild name is required");
        }
        else
        {
            // HTML/스크립트 태그 제거
            var sanitizedName = _htmlSanitizer.Sanitize(guildName);

            if (sanitizedName.Length < 2 || sanitizedName.Length > 50)
            {
                errors.Add("Guild name must be 2-50 characters");
            }

            // 특수 문자 제한
            if (!Regex.IsMatch(sanitizedName, @"^[a-zA-Z0-9가-힣\s\-_.]+$"))
            {
                errors.Add("Guild name contains invalid characters");
            }

            // 금지어 체크
            if (await ContainsProfanity(sanitizedName))
            {
                errors.Add("Guild name contains inappropriate content");
            }
        }

        // 길드 설명 검증
        if (!string.IsNullOrEmpty(description))
        {
            var sanitizedDescription = _htmlSanitizer.Sanitize(description);

            if (sanitizedDescription.Length > 1000)
            {
                errors.Add("Guild description too long (max 1000 characters)");
            }

            if (await ContainsProfanity(sanitizedDescription))
            {
                errors.Add("Guild description contains inappropriate content");
            }
        }

        return new ValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors,
            SanitizedGuildName = _htmlSanitizer.Sanitize(guildName),
            SanitizedDescription = _htmlSanitizer.Sanitize(description ?? string.Empty)
        };
    }

    private bool ContainsScriptPattern(string input)
    {
        var dangerousPatterns = new[]
        {
            @"<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>",
            @"javascript:",
            @"vbscript:",
            @"onload\s*=",
            @"onerror\s*=",
            @"onclick\s*=",
            @"eval\s*\(",
            @"expression\s*\("
        };

        return dangerousPatterns.Any(pattern =>
            Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase));
    }

    private async Task<string> ApplyProfanityFilter(string message)
    {
        // 간단한 예시 - 실제로는 더 정교한 필터링 시스템 사용
        var profanityWords = await GetProfanityList();

        foreach (var word in profanityWords)
        {
            message = Regex.Replace(message, Regex.Escape(word),
                new string('*', word.Length), RegexOptions.IgnoreCase);
        }

        return message;
    }
}
```

#### **오후: 게임 서버 특화 보안 위협 대응** (4시간)

##### 치팅 탐지 시스템
```csharp
// 게임 서버의 안티-치팅 시스템
public class AntiCheatService
{
    private readonly IGameDbContext _context;
    private readonly ILogger<AntiCheatService> _logger;
    private readonly ICacheService _cache;

    // 의심스러운 행동 패턴 탐지
    public async Task<CheatDetectionResult> AnalyzePlayerBehavior(
        Guid playerId, GameAction action)
    {
        var suspiciousActivities = new List<SuspiciousActivity>();

        // 1. 속도 분석 (너무 빠른 액션)
        var speedAnalysis = await AnalyzeActionSpeed(playerId, action);
        if (speedAnalysis.IsSuspicious)
        {
            suspiciousActivities.Add(new SuspiciousActivity
            {
                Type = SuspiciousActivityType.UnhumanSpeed,
                Severity = speedAnalysis.Severity,
                Details = $"Action completed in {speedAnalysis.Duration}ms (expected minimum: {speedAnalysis.ExpectedMinimum}ms)"
            });
        }

        // 2. 패턴 분석 (반복적인 동일한 액션)
        var patternAnalysis = await AnalyzeActionPattern(playerId, action);
        if (patternAnalysis.IsSuspicious)
        {
            suspiciousActivities.Add(new SuspiciousActivity
            {
                Type = SuspiciousActivityType.RepetitivePattern,
                Severity = patternAnalysis.Severity,
                Details = $"Repeated same action {patternAnalysis.RepetitionCount} times in {patternAnalysis.TimeWindow}"
            });
        }

        // 3. 리소스 분석 (불가능한 리소스 획득)
        var resourceAnalysis = await AnalyzeResourceGain(playerId, action);
        if (resourceAnalysis.IsSuspicious)
        {
            suspiciousActivities.Add(new SuspiciousActivity
            {
                Type = SuspiciousActivityType.ImpossibleGain,
                Severity = resourceAnalysis.Severity,
                Details = resourceAnalysis.Details
            });
        }

        // 4. 위치 분석 (순간이동 등)
        if (action.HasLocationData)
        {
            var locationAnalysis = await AnalyzeLocationChange(playerId, action);
            if (locationAnalysis.IsSuspicious)
            {
                suspiciousActivities.Add(new SuspiciousActivity
                {
                    Type = SuspiciousActivityType.Teleportation,
                    Severity = locationAnalysis.Severity,
                    Details = locationAnalysis.Details
                });
            }
        }

        // 5. 종합 판정
        var result = new CheatDetectionResult
        {
            PlayerId = playerId,
            Action = action,
            SuspiciousActivities = suspiciousActivities,
            OverallRiskScore = CalculateRiskScore(suspiciousActivities),
            Timestamp = DateTime.UtcNow
        };

        // 6. 임계값 초과 시 자동 조치
        if (result.OverallRiskScore > 0.8) // 80% 이상 의심
        {
            await TakeAutomaticAction(playerId, result);
        }

        // 7. 로그 기록
        await LogCheatDetection(result);

        return result;
    }

    private async Task<ActionSpeedAnalysis> AnalyzeActionSpeed(Guid playerId, GameAction action)
    {
        // 플레이어의 최근 액션들 조회
        var recentActions = await _context.GameActions
            .Where(a => a.PlayerId == playerId && a.ActionType == action.ActionType)
            .OrderByDescending(a => a.Timestamp)
            .Take(10)
            .ToListAsync();

        if (recentActions.Count < 2)
        {
            return ActionSpeedAnalysis.NotSuspicious();
        }

        var timeBetweenActions = (action.Timestamp - recentActions.First().Timestamp).TotalMilliseconds;

        // 액션 타입별 최소 시간 정의
        var minimumTimes = new Dictionary<GameActionType, double>
        {
            { GameActionType.Attack, 500 },      // 0.5초
            { GameActionType.Move, 100 },        // 0.1초
            { GameActionType.UseItem, 1000 },    // 1초
            { GameActionType.CastSkill, 2000 },  // 2초
            { GameActionType.Trade, 5000 }       // 5초
        };

        var expectedMinimum = minimumTimes.GetValueOrDefault(action.ActionType, 1000);

        if (timeBetweenActions < expectedMinimum * 0.5) // 50% 이하로 빠름
        {
            return new ActionSpeedAnalysis
            {
                IsSuspicious = true,
                Severity = timeBetweenActions < expectedMinimum * 0.1 ? 0.9 : 0.6,
                Duration = timeBetweenActions,
                ExpectedMinimum = expectedMinimum
            };
        }

        return ActionSpeedAnalysis.NotSuspicious();
    }

    private async Task<ResourceAnalysis> AnalyzeResourceGain(Guid playerId, GameAction action)
    {
        if (action.ActionType != GameActionType.GainResource)
            return ResourceAnalysis.NotSuspicious();

        var resourceGain = action.ResourceChanges;
        var playerLevel = await GetPlayerLevel(playerId);

        // 레벨별 최대 획득 가능량 정의
        var maxGoldPerAction = playerLevel * 100;
        var maxExpPerAction = playerLevel * 50;

        var suspiciousGains = new List<string>();

        if (resourceGain.Gold > maxGoldPerAction)
        {
            suspiciousGains.Add($"Gold gain: {resourceGain.Gold} (max expected: {maxGoldPerAction})");
        }

        if (resourceGain.Experience > maxExpPerAction)
        {
            suspiciousGains.Add($"Experience gain: {resourceGain.Experience} (max expected: {maxExpPerAction})");
        }

        if (suspiciousGains.Any())
        {
            return new ResourceAnalysis
            {
                IsSuspicious = true,
                Severity = 0.8,
                Details = string.Join(", ", suspiciousGains)
            };
        }

        return ResourceAnalysis.NotSuspicious();
    }

    private async Task TakeAutomaticAction(Guid playerId, CheatDetectionResult detection)
    {
        var player = await _context.Players.FindAsync(playerId);
        if (player == null) return;

        // 1. 임시 제재 (자동)
        var temporaryBan = new PlayerSanction
        {
            PlayerId = playerId,
            Type = SanctionType.TemporaryBan,
            Duration = TimeSpan.FromHours(1), // 1시간 임시 정지
            Reason = "Automatic detection: Suspicious activity",
            Details = JsonSerializer.Serialize(detection.SuspiciousActivities),
            IssuedAt = DateTime.UtcNow,
            IsAutomatic = true
        };

        _context.PlayerSanctions.Add(temporaryBan);

        // 2. 관리자 알림
        await NotifyAdministrators(playerId, detection);

        // 3. 추가 모니터링 플래그
        await _cache.SetAsync($"monitor:{playerId}", true, TimeSpan.FromDays(7));

        await _context.SaveChangesAsync();

        _logger.LogWarning("Automatic action taken against player {PlayerId} for suspicious activity. Risk score: {RiskScore}",
            playerId, detection.OverallRiskScore);
    }
}
```

##### 중복 로그인 방지 시스템
```csharp
// 게임 서버의 세션 관리 및 중복 로그인 방지
public class GameSessionManager
{
    private readonly ICacheService _cache;
    private readonly ILogger<GameSessionManager> _logger;
    private readonly IHubContext<GameHub> _hubContext;

    // 활성 세션 관리
    private static readonly ConcurrentDictionary<Guid, GameSession> _activeSessions = new();

    public async Task<LoginResult> ProcessLogin(Guid playerId, LoginRequest request)
    {
        var sessionKey = $"session:{playerId}";

        // 1. 기존 세션 확인
        var existingSession = await _cache.GetAsync<GameSession>(sessionKey);

        if (existingSession != null)
        {
            // 2. 중복 로그인 정책 적용
            switch (request.DuplicateLoginPolicy)
            {
                case DuplicateLoginPolicy.Reject:
                    return LoginResult.Failed("Already logged in from another device");

                case DuplicateLoginPolicy.DisconnectOther:
                    await DisconnectExistingSession(existingSession);
                    break;

                case DuplicateLoginPolicy.Allow:
                    // 여러 세션 허용 (모바일 + PC 등)
                    break;
            }
        }

        // 3. 새 세션 생성
        var newSession = new GameSession
        {
            SessionId = Guid.NewGuid(),
            PlayerId = playerId,
            DeviceInfo = request.DeviceInfo,
            IpAddress = request.IpAddress,
            LoginTime = DateTime.UtcNow,
            LastActivity = DateTime.UtcNow,
            IsActive = true
        };

        // 4. 세션 저장 (캐시 + 데이터베이스)
        await _cache.SetAsync(sessionKey, newSession, TimeSpan.FromHours(24));
        _activeSessions.TryAdd(playerId, newSession);

        // 5. 데이터베이스에 로그인 기록
        var loginLog = new PlayerLoginLog
        {
            PlayerId = playerId,
            SessionId = newSession.SessionId,
            IpAddress = request.IpAddress,
            DeviceInfo = JsonSerializer.Serialize(request.DeviceInfo),
            LoginTime = DateTime.UtcNow,
            LoginMethod = request.LoginMethod
        };

        _context.PlayerLoginLogs.Add(loginLog);
        await _context.SaveChangesAsync();

        // 6. JWT 토큰 생성
        var token = await GenerateJwtToken(playerId, newSession.SessionId);

        _logger.LogInformation("Player {PlayerId} logged in successfully from {IpAddress}",
            playerId, request.IpAddress);

        return LoginResult.Success(token, newSession);
    }

    public async Task<bool> ValidateSession(Guid playerId, Guid sessionId)
    {
        // 1. 메모리에서 빠른 검증
        if (_activeSessions.TryGetValue(playerId, out var memorySession))
        {
            if (memorySession.SessionId == sessionId && memorySession.IsActive)
            {
                // 마지막 활동 시간 업데이트
                memorySession.LastActivity = DateTime.UtcNow;
                return true;
            }
        }

        // 2. 캐시에서 검증
        var sessionKey = $"session:{playerId}";
        var cachedSession = await _cache.GetAsync<GameSession>(sessionKey);

        if (cachedSession?.SessionId == sessionId && cachedSession.IsActive)
        {
            // 세션 유효성 확인 (만료 체크)
            var sessionAge = DateTime.UtcNow - cachedSession.LoginTime;
            if (sessionAge > TimeSpan.FromHours(24))
            {
                await InvalidateSession(playerId, sessionId, "Session expired");
                return false;
            }

            // 메모리 캐시 갱신
            _activeSessions.TryAdd(playerId, cachedSession);
            return true;
        }

        return false;
    }

    private async Task DisconnectExistingSession(GameSession existingSession)
    {
        try
        {
            // 1. SignalR로 기존 클라이언트에 강제 로그아웃 알림
            await _hubContext.Clients.User(existingSession.PlayerId.ToString())
                .SendAsync("ForceLogout", new
                {
                    Reason = "Logged in from another device",
                    Timestamp = DateTime.UtcNow
                });

            // 2. 기존 세션 무효화
            await InvalidateSession(existingSession.PlayerId, existingSession.SessionId,
                "Disconnected due to new login");

            _logger.LogInformation("Existing session {SessionId} for player {PlayerId} disconnected due to new login",
                existingSession.SessionId, existingSession.PlayerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disconnecting existing session {SessionId}",
                existingSession.SessionId);
        }
    }

    public async Task InvalidateSession(Guid playerId, Guid sessionId, string reason)
    {
        // 1. 메모리에서 제거
        _activeSessions.TryRemove(playerId, out _);

        // 2. 캐시에서 제거
        var sessionKey = $"session:{playerId}";
        await _cache.RemoveAsync(sessionKey);

        // 3. 데이터베이스 로그아웃 기록
        var logoutLog = new PlayerLogoutLog
        {
            PlayerId = playerId,
            SessionId = sessionId,
            LogoutTime = DateTime.UtcNow,
            LogoutReason = reason
        };

        _context.PlayerLogoutLogs.Add(logoutLog);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Session {SessionId} for player {PlayerId} invalidated: {Reason}",
            sessionId, playerId, reason);
    }

    // 주기적 세션 정리 (백그라운드 서비스)
    public async Task CleanupExpiredSessions()
    {
        var expiredSessions = _activeSessions.Values
            .Where(s => DateTime.UtcNow - s.LastActivity > TimeSpan.FromMinutes(30))
            .ToList();

        foreach (var session in expiredSessions)
        {
            await InvalidateSession(session.PlayerId, session.SessionId, "Session timeout");
        }

        _logger.LogDebug("Cleaned up {Count} expired sessions", expiredSessions.Count);
    }

    // IP 기반 동시 접속 제한
    public async Task<bool> CheckIpConcurrentLimit(string ipAddress, Guid playerId)
    {
        const int maxConcurrentPerIp = 5; // IP당 최대 5개 동시 접속

        var concurrentSessions = _activeSessions.Values
            .Where(s => s.IpAddress == ipAddress && s.PlayerId != playerId)
            .Count();

        if (concurrentSessions >= maxConcurrentPerIp)
        {
            _logger.LogWarning("IP {IpAddress} exceeded concurrent session limit. Current: {Current}, Max: {Max}",
                ipAddress, concurrentSessions, maxConcurrentPerIp);
            return false;
        }

        return true;
    }
}
```

### **Day 2: JWT 토큰 시스템 구현**

#### **오전: JWT 이론과 게임 서버 적용** (3시간)

##### JWT vs Session 비교 (게임 서버 관점)
```csharp
// 전통적인 세션 방식 (Unity에서 일반적)
public class SessionBasedAuth
{
    // Unity에서 자주 사용하는 방식
    public class UnitySessionExample
    {
        private string sessionId;

        public void Login(string username, string password)
        {
            // 서버에 로그인 요청
            var response = PostToServer("/login", new { username, password });

            if (response.success)
            {
                // 세션 ID를 받아서 저장
                sessionId = response.sessionId;
                PlayerPrefs.SetString("SessionId", sessionId);
            }
        }

        public void MakeApiCall()
        {
            // 모든 API 요청에 세션 ID 포함
            var headers = new Dictionary<string, string>
            {
                { "X-Session-Id", sessionId }
            };

            GetFromServer("/api/player-data", headers);
        }
    }

    // 서버 측 세션 관리 (메모리/Redis 저장)
    public class ServerSessionManager
    {
        private readonly ICacheService _cache;

        public async Task<string> CreateSession(Guid playerId)
        {
            var sessionId = Guid.NewGuid().ToString();
            var session = new PlayerSession
            {
                PlayerId = playerId,
                CreatedAt = DateTime.UtcNow,
                LastActivity = DateTime.UtcNow
            };

            // 세션을 서버 메모리/Redis에 저장
            await _cache.SetAsync($"session:{sessionId}", session, TimeSpan.FromHours(24));

            return sessionId;
        }

        public async Task<PlayerSession> ValidateSession(string sessionId)
        {
            return await _cache.GetAsync<PlayerSession>($"session:{sessionId}");
        }
    }
}

// JWT 방식 (웹 서버에서 권장)
public class JwtBasedAuth
{
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<JwtBasedAuth> _logger;

    // JWT 토큰 생성
    public string GenerateToken(Player player, GameSession session)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

        // 게임 서버에 특화된 클레임들
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, player.Id.ToString()),
            new(ClaimTypes.Name, player.DisplayName),
            new(ClaimTypes.Email, player.Email),
            new("session_id", session.SessionId.ToString()),
            new("player_level", player.Level.ToString()),
            new("guild_id", player.GuildId?.ToString() ?? ""),
            new("server_region", player.ServerRegion),
            new("account_status", player.AccountStatus.ToString()),
            new("premium_expires", player.PremiumExpiresAt?.ToString() ?? ""),

            // 권한 관련
            new(ClaimTypes.Role, player.Role.ToString()),
            new("permissions", JsonSerializer.Serialize(player.Permissions))
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(_jwtSettings.ExpirationHours),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    // JWT 토큰 검증
    public async Task<TokenValidationResult> ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

            // 추가 게임 서버 검증
            var sessionId = principal.FindFirst("session_id")?.Value;
            if (string.IsNullOrEmpty(sessionId))
            {
                return TokenValidationResult.Invalid("Missing session ID");
            }

            var playerId = Guid.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            // 세션이 여전히 유효한지 확인
            var isSessionValid = await _sessionManager.ValidateSession(playerId, Guid.Parse(sessionId));
            if (!isSessionValid)
            {
                return TokenValidationResult.Invalid("Session expired or invalid");
            }

            return TokenValidationResult.Valid(principal);
        }
        catch (SecurityTokenExpiredException)
        {
            return TokenValidationResult.Invalid("Token expired");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token validation failed");
            return TokenValidationResult.Invalid("Invalid token");
        }
    }

    // 리프레시 토큰 시스템
    public async Task<RefreshTokenResult> RefreshToken(string refreshToken, Guid playerId)
    {
        // 1. 리프레시 토큰 검증
        var storedRefreshToken = await _context.PlayerRefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken &&
                                      rt.PlayerId == playerId &&
                                      rt.IsActive);

        if (storedRefreshToken == null || storedRefreshToken.ExpiresAt < DateTime.UtcNow)
        {
            return RefreshTokenResult.Invalid("Refresh token expired or invalid");
        }

        // 2. 플레이어 정보 조회
        var player = await _context.Players
            .Include(p => p.Guild)
            .FirstOrDefaultAsync(p => p.Id == playerId);

        if (player == null || player.AccountStatus != AccountStatus.Active)
        {
            return RefreshTokenResult.Invalid("Player account not active");
        }

        // 3. 새 액세스 토큰 생성
        var session = await _sessionManager.GetActiveSession(playerId);
        var newAccessToken = GenerateToken(player, session);

        // 4. 새 리프레시 토큰 생성 (선택적)
        string newRefreshToken = null;
        if (storedRefreshToken.ExpiresAt < DateTime.UtcNow.AddDays(7))
        {
            // 만료 7일 전이면 새 리프레시 토큰 발급
            newRefreshToken = await GenerateRefreshToken(playerId);

            // 기존 리프레시 토큰 비활성화
            storedRefreshToken.IsActive = false;
        }

        // 5. 마지막 사용 시간 업데이트
        storedRefreshToken.LastUsedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return RefreshTokenResult.Success(newAccessToken, newRefreshToken);
    }

    private async Task<string> GenerateRefreshToken(Guid playerId)
    {
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

        var tokenEntity = new PlayerRefreshToken
        {
            PlayerId = playerId,
            Token = refreshToken,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            IsActive = true
        };

        _context.PlayerRefreshTokens.Add(tokenEntity);
        await _context.SaveChangesAsync();

        return refreshToken;
    }
}

// ASP.NET Core JWT 미들웨어 설정
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // JWT 인증 설정
        var jwtSettings = Configuration.GetSection("JwtSettings").Get<JwtSettings>();
        var key = Encoding.ASCII.GetBytes(jwtSettings.SecretKey);

        services.AddAuthentication(x =>
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(x =>
        {
            x.RequireHttpsMetadata = false;
            x.SaveToken = true;
            x.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            // 게임 서버 특화 이벤트 처리
            x.Events = new JwtBearerEvents
            {
                OnTokenValidated = async context =>
                {
                    // JWT 유효성 검사 후 추가 게임 서버 검증
                    var jwtAuth = context.HttpContext.RequestServices
                        .GetRequiredService<JwtBasedAuth>();

                    var token = context.Request.Headers["Authorization"]
                        .FirstOrDefault()?.Split(" ").Last();

                    if (!string.IsNullOrEmpty(token))
                    {
                        var validationResult = await jwtAuth.ValidateToken(token);
                        if (!validationResult.IsValid)
                        {
                            context.Fail(validationResult.ErrorMessage);
                        }
                    }
                },

                OnAuthenticationFailed = context =>
                {
                    if (context.Exception is SecurityTokenExpiredException)
                    {
                        context.Response.Headers.Add("Token-Expired", "true");
                    }
                    return Task.CompletedTask;
                }
            };
        });
    }
}
```

#### **오후: 권한 기반 접근 제어 구현** (4시간)

##### 게임 서버 권한 시스템
```csharp
// 게임 서버의 역할 기반 접근 제어 (RBAC)
public enum GameRole
{
    Player = 1,
    VipPlayer = 2,
    Moderator = 10,
    GameMaster = 20,
    Admin = 100
}

public enum GamePermission
{
    // 기본 플레이어 권한
    PlayGame = 1,
    ChatGeneral = 2,
    TradeItems = 3,
    CreateGuild = 4,

    // VIP 플레이어 권한
    ChatVip = 10,
    AccessPremiumContent = 11,
    FastTravel = 12,

    // 모더레이터 권한
    ChatModerate = 20,
    KickPlayer = 21,
    MutePlayer = 22,
    ViewPlayerReports = 23,

    // 게임마스터 권한
    SpawnItems = 30,
    ModifyPlayerStats = 31,
    TeleportPlayers = 32,
    BanPlayer = 33,
    ViewAllPlayerData = 34,

    // 관리자 권한
    ManageServer = 40,
    AccessAnalytics = 41,
    ModifyGameConfig = 42,
    ViewSystemLogs = 43
}

// 권한 체크 속성
public class RequirePermissionAttribute : Attribute, IAuthorizationFilter
{
    private readonly GamePermission _requiredPermission;

    public RequirePermissionAttribute(GamePermission permission)
    {
        _requiredPermission = permission;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (!user.Identity.IsAuthenticated)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // 플레이어 권한 확인
        var permissions = GetPlayerPermissions(user);

        if (!permissions.Contains(_requiredPermission))
        {
            context.Result = new ForbidResult($"Missing required permission: {_requiredPermission}");
            return;
        }

        // 추가 검증: 계정 상태 확인
        var accountStatus = user.FindFirst("account_status")?.Value;
        if (accountStatus != AccountStatus.Active.ToString())
        {
            context.Result = new ForbidResult("Account is not active");
            return;
        }
    }

    private List<GamePermission> GetPlayerPermissions(ClaimsPrincipal user)
    {
        var permissionsJson = user.FindFirst("permissions")?.Value;
        if (string.IsNullOrEmpty(permissionsJson))
        {
            return new List<GamePermission>();
        }

        return JsonSerializer.Deserialize<List<GamePermission>>(permissionsJson) ?? new List<GamePermission>();
    }
}

// 게임 서버 컨트롤러에서 권한 사용
[ApiController]
[Route("api/[controller]")]
[Authorize] // JWT 인증 필수
public class GameAdminController : ControllerBase
{
    private readonly IPlayerService _playerService;
    private readonly ICurrentUserService _currentUser;

    // 일반 플레이어도 접근 가능
    [HttpGet("my-profile")]
    [RequirePermission(GamePermission.PlayGame)]
    public async Task<ActionResult<PlayerProfileDto>> GetMyProfile()
    {
        var playerId = _currentUser.PlayerId;
        var profile = await _playerService.GetPlayerProfile(playerId);
        return Ok(profile);
    }

    // VIP 플레이어만 접근 가능
    [HttpPost("premium-shop/purchase")]
    [RequirePermission(GamePermission.AccessPremiumContent)]
    public async Task<ActionResult<PurchaseResult>> PurchasePremiumItem(PremiumPurchaseRequest request)
    {
        var result = await _playerService.PurchasePremiumItem(_currentUser.PlayerId, request);
        return Ok(result);
    }

    // 모더레이터 이상만 접근 가능
    [HttpPost("moderation/mute-player")]
    [RequirePermission(GamePermission.MutePlayer)]
    public async Task<ActionResult> MutePlayer(MutePlayerRequest request)
    {
        await _playerService.MutePlayer(request.PlayerId, request.Duration, request.Reason);
        return Ok();
    }

    // 게임마스터만 접근 가능
    [HttpPost("gm/spawn-item")]
    [RequirePermission(GamePermission.SpawnItems)]
    public async Task<ActionResult> SpawnItem(SpawnItemRequest request)
    {
        // 게임마스터 액션 로깅
        await LogGameMasterAction("SpawnItem", new
        {
            TargetPlayer = request.PlayerId,
            ItemId = request.ItemId,
            Quantity = request.Quantity,
            Reason = request.Reason
        });

        await _playerService.SpawnItem(request.PlayerId, request.ItemId, request.Quantity);
        return Ok();
    }

    // 관리자만 접근 가능
    [HttpGet("admin/server-stats")]
    [RequirePermission(GamePermission.ViewSystemLogs)]
    public async Task<ActionResult<ServerStatsDto>> GetServerStats()
    {
        var stats = await _adminService.GetServerStatistics();
        return Ok(stats);
    }
}

// 동적 권한 체크 서비스
public class GamePermissionService
{
    private readonly IGameDbContext _context;
    private readonly ICacheService _cache;

    // 플레이어 권한 실시간 확인
    public async Task<bool> HasPermission(Guid playerId, GamePermission permission)
    {
        var cacheKey = $"permissions:{playerId}";

        // 캐시에서 권한 확인 (5분 캐시)
        var cachedPermissions = await _cache.GetAsync<List<GamePermission>>(cacheKey);
        if (cachedPermissions != null)
        {
            return cachedPermissions.Contains(permission);
        }

        // 데이터베이스에서 권한 조회
        var player = await _context.Players
            .Include(p => p.Role)
            .Include(p => p.CustomPermissions)
            .FirstOrDefaultAsync(p => p.Id == playerId);

        if (player == null)
            return false;

        var permissions = await CalculatePlayerPermissions(player);

        // 캐시에 저장
        await _cache.SetAsync(cacheKey, permissions, TimeSpan.FromMinutes(5));

        return permissions.Contains(permission);
    }

    private async Task<List<GamePermission>> CalculatePlayerPermissions(Player player)
    {
        var permissions = new List<GamePermission>();

        // 1. 역할 기반 권한
        permissions.AddRange(await GetRolePermissions(player.Role));

        // 2. VIP 상태 권한
        if (player.PremiumExpiresAt > DateTime.UtcNow)
        {
            permissions.AddRange(GetVipPermissions());
        }

        // 3. 커스텀 권한 (개별 부여)
        permissions.AddRange(player.CustomPermissions.Select(cp => cp.Permission));

        // 4. 임시 제재 중인 권한 제거
        var activeSanctions = await _context.PlayerSanctions
            .Where(s => s.PlayerId == player.Id &&
                       s.IsActive &&
                       s.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();

        foreach (var sanction in activeSanctions)
        {
            permissions.RemoveAll(p => sanction.RestrictedPermissions.Contains(p));
        }

        return permissions.Distinct().ToList();
    }

    // 권한 임시 부여/제거 (이벤트 등)
    public async Task GrantTemporaryPermission(Guid playerId, GamePermission permission, TimeSpan duration, string reason)
    {
        var tempPermission = new TemporaryPlayerPermission
        {
            PlayerId = playerId,
            Permission = permission,
            GrantedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(duration),
            Reason = reason,
            IsActive = true
        };

        _context.TemporaryPlayerPermissions.Add(tempPermission);
        await _context.SaveChangesAsync();

        // 권한 캐시 무효화
        await InvalidatePermissionCache(playerId);
    }

    public async Task RevokePermission(Guid playerId, GamePermission permission, string reason)
    {
        var sanction = new PlayerSanction
        {
            PlayerId = playerId,
            Type = SanctionType.PermissionRestriction,
            RestrictedPermissions = new List<GamePermission> { permission },
            Reason = reason,
            IssuedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.PlayerSanctions.Add(sanction);
        await _context.SaveChangesAsync();

        await InvalidatePermissionCache(playerId);
    }

    private async Task InvalidatePermissionCache(Guid playerId)
    {
        var cacheKey = $"permissions:{playerId}";
        await _cache.RemoveAsync(cacheKey);
    }
}

// 리소스 기반 권한 체크 (자신의 캐릭터만 수정 가능 등)
public class ResourcePermissionAttribute : Attribute, IAuthorizationFilter
{
    private readonly string _resourceIdParameter;

    public ResourcePermissionAttribute(string resourceIdParameter = "id")
    {
        _resourceIdParameter = resourceIdParameter;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        var currentUserId = Guid.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

        // URL에서 리소스 ID 추출
        if (context.RouteData.Values.TryGetValue(_resourceIdParameter, out var resourceIdObj))
        {
            if (Guid.TryParse(resourceIdObj.ToString(), out var resourceId))
            {
                // 게임마스터 이상은 모든 리소스 접근 가능
                var permissions = GetPlayerPermissions(user);
                if (permissions.Contains(GamePermission.ModifyPlayerStats))
                {
                    return; // 허용
                }

                // 일반 플레이어는 자신의 리소스만 접근 가능
                var resourceService = context.HttpContext.RequestServices
                    .GetRequiredService<IResourceOwnershipService>();

                if (!resourceService.IsOwner(currentUserId, resourceId))
                {
                    context.Result = new ForbidResult("Cannot access another player's resource");
                    return;
                }
            }
        }
    }
}

// 사용 예시: 캐릭터 수정은 본인만 가능
[HttpPut("characters/{id}")]
[RequirePermission(GamePermission.PlayGame)]
[ResourcePermission("id")] // 자신의 캐릭터만 수정 가능
public async Task<ActionResult> UpdateCharacter(Guid id, UpdateCharacterRequest request)
{
    await _characterService.UpdateCharacter(id, request);
    return Ok();
}
```

### **Day 3: 고급 보안 기법**

#### **오전: Rate Limiting과 DDoS 방어** (3시간)

##### 게임 서버 Rate Limiting 구현
```csharp
// 게임 액션별 차등 Rate Limiting
public class GameRateLimitingService
{
    private readonly ICacheService _cache;
    private readonly ILogger<GameRateLimitingService> _logger;

    // 액션 타입별 제한 설정
    private readonly Dictionary<GameActionType, RateLimitConfig> _rateLimits = new()
    {
        // 일반 게임 액션
        { GameActionType.Move, new RateLimitConfig(100, TimeSpan.FromSeconds(1)) },
        { GameActionType.Attack, new RateLimitConfig(10, TimeSpan.FromSeconds(1)) },
        { GameActionType.UseItem, new RateLimitConfig(5, TimeSpan.FromSeconds(1)) },
        { GameActionType.CastSkill, new RateLimitConfig(3, TimeSpan.FromSeconds(1)) },

        // 소셜 액션
        { GameActionType.SendChatMessage, new RateLimitConfig(10, TimeSpan.FromMinutes(1)) },
        { GameActionType.SendFriendRequest, new RateLimitConfig(5, TimeSpan.FromMinutes(5)) },
        { GameActionType.CreateGuild, new RateLimitConfig(1, TimeSpan.FromDays(1)) },

        // 경제 액션 (더 엄격한 제한)
        { GameActionType.TradeItem, new RateLimitConfig(10, TimeSpan.FromMinutes(5)) },
        { GameActionType.SellItem, new RateLimitConfig(20, TimeSpan.FromMinutes(1)) },
        { GameActionType.PurchaseItem, new RateLimitConfig(15, TimeSpan.FromMinutes(1)) },

        // API 액션
        { GameActionType.GetPlayerData, new RateLimitConfig(60, TimeSpan.FromMinutes(1)) },
        { GameActionType.GetLeaderboard, new RateLimitConfig(10, TimeSpan.FromMinutes(1)) }
    };

    public async Task<RateLimitResult> CheckRateLimit(
        Guid playerId, GameActionType actionType, string? additionalKey = null)
    {
        if (!_rateLimits.TryGetValue(actionType, out var config))
        {
            // 기본 제한: 분당 30회
            config = new RateLimitConfig(30, TimeSpan.FromMinutes(1));
        }

        var cacheKey = $"ratelimit:{playerId}:{actionType}";
        if (!string.IsNullOrEmpty(additionalKey))
        {
            cacheKey += $":{additionalKey}";
        }

        var currentCount = await _cache.GetAsync<int?>(cacheKey) ?? 0;

        if (currentCount >= config.MaxRequests)
        {
            _logger.LogWarning("Rate limit exceeded for player {PlayerId}, action {ActionType}. Current: {Current}, Max: {Max}",
                playerId, actionType, currentCount, config.MaxRequests);

            return RateLimitResult.Exceeded(config.MaxRequests, config.Window, currentCount);
        }

        // 카운터 증가
        await _cache.SetAsync(cacheKey, currentCount + 1, config.Window);

        return RateLimitResult.Allowed(config.MaxRequests - currentCount - 1);
    }

    // IP 기반 Rate Limiting (DDoS 방어)
    public async Task<RateLimitResult> CheckIpRateLimit(string ipAddress, string endpoint)
    {
        var config = GetIpRateLimitConfig(endpoint);
        var cacheKey = $"ip_ratelimit:{ipAddress}:{endpoint}";

        var currentCount = await _cache.GetAsync<int?>(cacheKey) ?? 0;

        if (currentCount >= config.MaxRequests)
        {
            _logger.LogWarning("IP rate limit exceeded for {IpAddress} on endpoint {Endpoint}. Current: {Current}, Max: {Max}",
                ipAddress, endpoint, currentCount, config.MaxRequests);

            return RateLimitResult.Exceeded(config.MaxRequests, config.Window, currentCount);
        }

        await _cache.SetAsync(cacheKey, currentCount + 1, config.Window);
        return RateLimitResult.Allowed(config.MaxRequests - currentCount - 1);
    }

    private RateLimitConfig GetIpRateLimitConfig(string endpoint)
    {
        return endpoint.ToLower() switch
        {
            "/api/auth/login" => new RateLimitConfig(10, TimeSpan.FromMinutes(5)), // 로그인 시도
            "/api/auth/register" => new RateLimitConfig(5, TimeSpan.FromMinutes(10)), // 회원가입
            "/api/players/forgot-password" => new RateLimitConfig(3, TimeSpan.FromMinutes(15)), // 비밀번호 찾기
            _ => new RateLimitConfig(1000, TimeSpan.FromMinutes(1)) // 일반 API
        };
    }

    // 버스트 허용 Rate Limiting (순간적인 높은 요청 허용)
    public async Task<RateLimitResult> CheckBurstRateLimit(Guid playerId, GameActionType actionType)
    {
        var burstKey = $"burst:{playerId}:{actionType}";
        var sustainedKey = $"sustained:{playerId}:{actionType}";

        var burstConfig = new RateLimitConfig(50, TimeSpan.FromSeconds(1)); // 1초에 50개
        var sustainedConfig = new RateLimitConfig(100, TimeSpan.FromMinutes(1)); // 1분에 100개

        // 버스트 체크
        var burstCount = await _cache.GetAsync<int?>(burstKey) ?? 0;
        if (burstCount >= burstConfig.MaxRequests)
        {
            return RateLimitResult.Exceeded(burstConfig.MaxRequests, burstConfig.Window, burstCount);
        }

        // 지속적 사용량 체크
        var sustainedCount = await _cache.GetAsync<int?>(sustainedKey) ?? 0;
        if (sustainedCount >= sustainedConfig.MaxRequests)
        {
            return RateLimitResult.Exceeded(sustainedConfig.MaxRequests, sustainedConfig.Window, sustainedCount);
        }

        // 카운터 증가
        await _cache.SetAsync(burstKey, burstCount + 1, burstConfig.Window);
        await _cache.SetAsync(sustainedKey, sustainedCount + 1, sustainedConfig.Window);

        return RateLimitResult.Allowed(Math.Min(
            burstConfig.MaxRequests - burstCount - 1,
            sustainedConfig.MaxRequests - sustainedCount - 1));
    }
}

// Rate Limiting 미들웨어
public class GameRateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly GameRateLimitingService _rateLimitService;
    private readonly ILogger<GameRateLimitingMiddleware> _logger;

    public GameRateLimitingMiddleware(
        RequestDelegate next,
        GameRateLimitingService rateLimitService,
        ILogger<GameRateLimitingMiddleware> logger)
    {
        _next = next;
        _rateLimitService = rateLimitService;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.Request.Path.ToString();
        var ipAddress = GetClientIpAddress(context);

        // IP 기반 Rate Limiting (DDoS 방어)
        var ipRateLimit = await _rateLimitService.CheckIpRateLimit(ipAddress, endpoint);
        if (!ipRateLimit.IsAllowed)
        {
            await HandleRateLimitExceeded(context, ipRateLimit, "IP");
            return;
        }

        // 인증된 사용자의 경우 추가 Rate Limiting
        if (context.User.Identity.IsAuthenticated)
        {
            var playerId = Guid.Parse(context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var actionType = MapEndpointToActionType(endpoint);

            var playerRateLimit = await _rateLimitService.CheckRateLimit(playerId, actionType);
            if (!playerRateLimit.IsAllowed)
            {
                await HandleRateLimitExceeded(context, playerRateLimit, "Player");
                return;
            }

            // Rate limit 헤더 추가
            context.Response.Headers.Add("X-RateLimit-Remaining", playerRateLimit.RemainingRequests.ToString());
        }

        await _next(context);
    }

    private async Task HandleRateLimitExceeded(
        HttpContext context, RateLimitResult rateLimitResult, string limitType)
    {
        context.Response.StatusCode = 429; // Too Many Requests
        context.Response.Headers.Add("X-RateLimit-Limit", rateLimitResult.MaxRequests.ToString());
        context.Response.Headers.Add("X-RateLimit-Remaining", "0");
        context.Response.Headers.Add("X-RateLimit-Reset", rateLimitResult.ResetTime.ToString());
        context.Response.Headers.Add("Retry-After", ((int)rateLimitResult.RetryAfter.TotalSeconds).ToString());

        var response = new
        {
            error = "Rate limit exceeded",
            type = limitType,
            limit = rateLimitResult.MaxRequests,
            current = rateLimitResult.CurrentRequests,
            retryAfter = rateLimitResult.RetryAfter.TotalSeconds
        };

        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private GameActionType MapEndpointToActionType(string endpoint)
    {
        return endpoint.ToLower() switch
        {
            var path when path.Contains("/characters") => GameActionType.GetPlayerData,
            var path when path.Contains("/chat") => GameActionType.SendChatMessage,
            var path when path.Contains("/trade") => GameActionType.TradeItem,
            var path when path.Contains("/guild") => GameActionType.CreateGuild,
            var path when path.Contains("/leaderboard") => GameActionType.GetLeaderboard,
            _ => GameActionType.GetPlayerData
        };
    }
}
```

#### **오후: 데이터 암호화와 민감정보 보호** (4시간)

##### 게임 데이터 암호화 시스템
```csharp
// 게임 서버의 데이터 암호화 서비스
public class GameDataEncryptionService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<GameDataEncryptionService> _logger;

    // 암호화 키 (실제로는 Azure Key Vault 등에서 관리)
    private readonly byte[] _encryptionKey;
    private readonly byte[] _signingKey;

    public GameDataEncryptionService(IConfiguration configuration, ILogger<GameDataEncryptionService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        // 키 초기화 (실제로는 보안 저장소에서 로드)
        _encryptionKey = Convert.FromBase64String(_configuration["Encryption:DataKey"]);
        _signingKey = Convert.FromBase64String(_configuration["Encryption:SigningKey"]);
    }

    // 플레이어 민감 정보 암호화 (이메일, 개인정보 등)
    public string EncryptPersonalData(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;

        try
        {
            using (var aes = Aes.Create())
            {
                aes.Key = _encryptionKey;
                aes.GenerateIV();

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (var msEncrypt = new MemoryStream())
                {
                    // IV를 암호화된 데이터 앞에 추가
                    msEncrypt.Write(aes.IV, 0, aes.IV.Length);

                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    using (var swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }

                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error encrypting personal data");
            throw;
        }
    }

    public string DecryptPersonalData(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return cipherText;

        try
        {
            var fullCipher = Convert.FromBase64String(cipherText);

            using (var aes = Aes.Create())
            {
                aes.Key = _encryptionKey;

                // IV 추출 (처음 16바이트)
                var iv = new byte[aes.BlockSize / 8];
                var cipher = new byte[fullCipher.Length - iv.Length];

                Array.Copy(fullCipher, iv, iv.Length);
                Array.Copy(fullCipher, iv.Length, cipher, 0, cipher.Length);

                aes.IV = iv;

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                using (var msDecrypt = new MemoryStream(cipher))
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (var srDecrypt = new StreamReader(csDecrypt))
                {
                    return srDecrypt.ReadToEnd();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decrypting personal data");
            throw;
        }
    }

    // 게임 세이브 데이터 무결성 검증
    public string CreateSaveDataSignature(string saveData, Guid playerId)
    {
        var dataToSign = $"{saveData}|{playerId}|{DateTimeOffset.UtcNow:yyyyMMddHH}";

        using (var hmac = new HMACSHA256(_signingKey))
        {
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(dataToSign));
            return Convert.ToBase64String(hash);
        }
    }

    public bool VerifySaveDataSignature(string saveData, Guid playerId, string signature)
    {
        // 현재 시간과 1시간 전 시간으로 검증 (시간 차이 허용)
        var currentHour = DateTimeOffset.UtcNow.ToString("yyyyMMddHH");
        var previousHour = DateTimeOffset.UtcNow.AddHours(-1).ToString("yyyyMMddHH");

        var isCurrentValid = VerifySignatureForTime(saveData, playerId, signature, currentHour);
        var isPreviousValid = VerifySignatureForTime(saveData, playerId, signature, previousHour);

        return isCurrentValid || isPreviousValid;
    }

    private bool VerifySignatureForTime(string saveData, Guid playerId, string signature, string timeString)
    {
        var dataToVerify = $"{saveData}|{playerId}|{timeString}";

        using (var hmac = new HMACSHA256(_signingKey))
        {
            var expectedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(dataToVerify));
            var expectedSignature = Convert.ToBase64String(expectedHash);

            return CryptographicOperations.FixedTimeEquals(
                Convert.FromBase64String(signature),
                Convert.FromBase64String(expectedSignature));
        }
    }
}

// 게임 통신 암호화 (클라이언트-서버)
public class GameCommunicationSecurity
{
    private readonly ILogger<GameCommunicationSecurity> _logger;

    // 클라이언트와 서버 간 메시지 암호화
    public EncryptedMessage EncryptMessage(object message, string sessionKey)
    {
        var json = JsonSerializer.Serialize(message);
        var key = Convert.FromBase64String(sessionKey);

        using (var aes = Aes.Create())
        {
            aes.Key = key.Take(32).ToArray(); // 256비트 키 사용
            aes.GenerateIV();

            using (var encryptor = aes.CreateEncryptor())
            {
                var plainBytes = Encoding.UTF8.GetBytes(json);
                var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

                return new EncryptedMessage
                {
                    Data = Convert.ToBase64String(encryptedBytes),
                    IV = Convert.ToBase64String(aes.IV),
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                };
            }
        }
    }

    public T DecryptMessage<T>(EncryptedMessage encryptedMessage, string sessionKey)
    {
        var key = Convert.FromBase64String(sessionKey);
        var iv = Convert.FromBase64String(encryptedMessage.IV);
        var encryptedData = Convert.FromBase64String(encryptedMessage.Data);

        // 메시지 유효 시간 검증 (5분)
        var messageAge = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - encryptedMessage.Timestamp;
        if (messageAge > 300)
        {
            throw new SecurityException("Message too old");
        }

        using (var aes = Aes.Create())
        {
            aes.Key = key.Take(32).ToArray();
            aes.IV = iv;

            using (var decryptor = aes.CreateDecryptor())
            {
                var decryptedBytes = decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
                var json = Encoding.UTF8.GetString(decryptedBytes);

                return JsonSerializer.Deserialize<T>(json);
            }
        }
    }
}

// 비밀번호 해싱 (게임 서버용)
public class GamePasswordService
{
    private const int SaltSize = 32; // 256 bits
    private const int HashSize = 32; // 256 bits
    private const int Iterations = 100000; // PBKDF2 반복 횟수

    public string HashPassword(string password)
    {
        // 솔트 생성
        var salt = RandomNumberGenerator.GetBytes(SaltSize);

        // PBKDF2로 해싱
        using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
        {
            var hash = pbkdf2.GetBytes(HashSize);

            // 솔트와 해시를 합쳐서 저장
            var hashBytes = new byte[SaltSize + HashSize];
            Array.Copy(salt, 0, hashBytes, 0, SaltSize);
            Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

            return Convert.ToBase64String(hashBytes);
        }
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        try
        {
            var hashBytes = Convert.FromBase64String(hashedPassword);

            // 솔트 추출
            var salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);

            // 해시 추출
            var hash = new byte[HashSize];
            Array.Copy(hashBytes, SaltSize, hash, 0, HashSize);

            // 입력된 비밀번호로 해시 재생성
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                var testHash = pbkdf2.GetBytes(HashSize);

                // 타이밍 공격 방지를 위한 상수 시간 비교
                return CryptographicOperations.FixedTimeEquals(hash, testHash);
            }
        }
        catch (Exception ex)
        {
            // 해싱 검증 실패는 보안상 자세한 에러를 노출하지 않음
            return false;
        }
    }

    // 비밀번호 강도 검증
    public PasswordValidationResult ValidatePassword(string password)
    {
        var errors = new List<string>();

        if (string.IsNullOrEmpty(password))
        {
            errors.Add("Password is required");
            return new PasswordValidationResult { IsValid = false, Errors = errors };
        }

        if (password.Length < 8)
            errors.Add("Password must be at least 8 characters long");

        if (password.Length > 100)
            errors.Add("Password must be less than 100 characters");

        if (!password.Any(char.IsUpper))
            errors.Add("Password must contain at least one uppercase letter");

        if (!password.Any(char.IsLower))
            errors.Add("Password must contain at least one lowercase letter");

        if (!password.Any(char.IsDigit))
            errors.Add("Password must contain at least one digit");

        if (!password.Any(c => "!@#$%^&*()_+-=[]{}|;:,.<>?".Contains(c)))
            errors.Add("Password must contain at least one special character");

        // 일반적인 패턴 검사
        if (IsCommonPassword(password))
            errors.Add("This password is too common");

        return new PasswordValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors
        };
    }

    private bool IsCommonPassword(string password)
    {
        // 간단한 예시 - 실제로는 더 큰 사전을 사용
        var commonPasswords = new[]
        {
            "password", "123456", "12345678", "qwerty", "abc123",
            "password1", "admin", "letmein", "welcome", "monkey"
        };

        return commonPasswords.Contains(password.ToLower());
    }
}

// EF Core에서 자동 암호화 적용
public class EncryptedPlayerEntity
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;

    // 암호화된 필드들
    [PersonalData] // 커스텀 속성으로 암호화 대상 표시
    public string Email { get; set; } = string.Empty;

    [PersonalData]
    public string? PhoneNumber { get; set; }

    [PersonalData]
    public string? RealName { get; set; }

    // 해시된 비밀번호
    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

// EF Core Value Converter로 자동 암호화/복호화
public class EncryptedStringConverter : ValueConverter<string, string>
{
    public EncryptedStringConverter(GameDataEncryptionService encryptionService)
        : base(
            v => encryptionService.EncryptPersonalData(v),
            v => encryptionService.DecryptPersonalData(v))
    {
    }
}

// DbContext에서 암호화 적용
public class GameDbContext : DbContext
{
    private readonly GameDataEncryptionService _encryptionService;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // PersonalData 속성이 있는 모든 string 속성을 자동 암호화
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(string) &&
                    property.PropertyInfo?.GetCustomAttribute<PersonalDataAttribute>() != null)
                {
                    property.SetValueConverter(new EncryptedStringConverter(_encryptionService));
                }
            }
        }
    }
}
```

### **4단계 체크리스트**

#### **Day 1 완료 기준**
- [ ] OWASP Top 10 취약점과 게임 서버 적용점 이해
- [ ] SQL Injection 방지 기법 숙지 및 구현
- [ ] XSS 방지 시스템 구축 (채팅, 길드 등)
- [ ] 게임 특화 치팅 탐지 시스템 구현
- [ ] 중복 로그인 방지 및 세션 관리 시스템 완성

#### **Day 2 완료 기준**
- [ ] JWT vs Session 차이점과 게임 서버 선택 기준 이해
- [ ] JWT 토큰 생성/검증/갱신 시스템 구현
- [ ] 게임 서버 권한 시스템 (RBAC) 설계 및 구현
- [ ] 리소스 기반 접근 제어 구현
- [ ] 동적 권한 관리 시스템 구축

#### **Day 3 완료 기준**
- [ ] 게임 액션별 차등 Rate Limiting 시스템 구현
- [ ] DDoS 방어를 위한 IP 기반 Rate Limiting
- [ ] 개인정보 암호화/복호화 시스템 구현
- [ ] 게임 데이터 무결성 검증 시스템
- [ ] 안전한 비밀번호 해싱 및 검증 시스템

#### **전체 평가 기준**
- [ ] 게임 서버 보안 위협 모델 이해 및 대응 방안 수립
- [ ] 프로덕션 환경에서 적용 가능한 보안 시스템 구축
- [ ] 성능과 보안의 균형점 찾기 (Rate Limiting 등)
- [ ] 개인정보보호법 준수를 위한 데이터 암호화 시스템 완성