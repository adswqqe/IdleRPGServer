# 🎮 Unity 개발자를 위한 방치형 RPG 서버 개발 완전 커리큘럼

## 📊 현재 상황 분석

### ✅ 완료된 학습
- **개발 환경**: Rider + .NET 8 + Docker + PostgreSQL
- **프로젝트 구조**: Clean Architecture (Domain, Application, Infrastructure, API)
- **Entity Framework**: 기본 모델 생성 (Player, Character, Inventory, OfflineReward)
- **마이그레이션**: 초기 설정 및 문제 해결

### ⚠️ 보완 필요 영역
- 데이터베이스 심화 (트랜잭션, 인덱스, 동시성)
- API 실제 구현 (Controller, Service, Repository)
- JWT 인증/인가 시스템
- 방치형 게임 핵심 로직
- Redis 캐싱 및 성능 최적화
- SignalR 실시간 통신
- Unity 클라이언트 연동

---

## 🗓️ 10주 완성 로드맵

### **Week 1-2: 데이터베이스 마스터리**

#### 📚 학습 목표
- SQL 기초와 PostgreSQL 특징 이해
- Entity Framework Core 심화
- 트랜잭션과 동시성 제어
- 1만 동접을 위한 DB 설계

#### 💻 실습 내용
```csharp
// 1. Repository Pattern 구현
public interface IPlayerRepository
{
    Task<Player> GetByIdAsync(Guid id);
    Task<List<Player>> GetTopPlayersAsync(int count);
    Task<Player> CreateAsync(Player player);
    Task UpdateAsync(Player player);
    Task<bool> ExistsAsync(string username);
}

// 2. Unit of Work Pattern
public interface IUnitOfWork
{
    IPlayerRepository Players { get; }
    ICharacterRepository Characters { get; }
    IInventoryRepository Inventories { get; }
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
}

// 3. 트랜잭션 처리 예제
public async Task<PurchaseResult> PurchaseItem(Guid playerId, int itemId)
{
    using var transaction = await _unitOfWork.BeginTransactionAsync();
    try
    {
        var player = await _unitOfWork.Players.GetByIdAsync(playerId);
        var item = await _unitOfWork.Items.GetByIdAsync(itemId);
        
        if (player.Gold < item.Price)
            return PurchaseResult.InsufficientGold;
        
        player.Gold -= item.Price;
        player.Inventory.Add(new PlayerInventory 
        { 
            ItemId = itemId,
            AcquiredAt = DateTime.UtcNow
        });
        
        await _unitOfWork.SaveChangesAsync();
        await _unitOfWork.CommitAsync();
        
        return PurchaseResult.Success;
    }
    catch (Exception ex)
    {
        await _unitOfWork.RollbackAsync();
        throw;
    }
}
```

#### 📊 DB 설계 베스트 프랙티스
```sql
-- 인덱스 최적화 (1만 동접 대비)
CREATE INDEX idx_players_level ON players(level DESC);
CREATE INDEX idx_characters_player_id ON characters(player_id);
CREATE INDEX idx_offline_rewards_player_id_claimed ON offline_rewards(player_id, is_claimed);

-- 파티셔닝 (대용량 데이터 대비)
CREATE TABLE battle_logs_2024_01 PARTITION OF battle_logs
    FOR VALUES FROM ('2024-01-01') TO ('2024-02-01');
```

---

### **Week 3-4: API 개발과 인증 시스템**

#### 📚 학습 목표
- RESTful API 설계 원칙
- JWT 토큰 기반 인증
- Role-based Access Control
- API 버저닝과 문서화

#### 💻 실습 내용
```csharp
// 1. JWT 인증 서비스
public class AuthService
{
    private readonly IConfiguration _configuration;
    
    public string GenerateToken(Player player)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, player.Id.ToString()),
            new Claim(ClaimTypes.Name, player.Username),
            new Claim("Level", player.Level.ToString()),
            new Claim(ClaimTypes.Role, player.Role)
        };
        
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials
        );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

// 2. PlayerController 구현
[ApiController]
[Route("api/v1/[controller]")]
[ApiVersion("1.0")]
public class PlayerController : ControllerBase
{
    private readonly IPlayerService _playerService;
    private readonly ILogger<PlayerController> _logger;
    
    [HttpPost("register")]
    [ProducesResponseType(typeof(PlayerDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (await _playerService.ExistsAsync(dto.Username))
            return BadRequest("Username already exists");
        
        var player = await _playerService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = player.Id }, player);
    }
    
    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(PlayerDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var player = await _playerService.GetByIdAsync(id);
        if (player == null)
            return NotFound();
        
        return Ok(player);
    }
}
```

---

### **Week 5-6: 방치형 게임 핵심 시스템**

#### 📚 학습 목표
- 오프라인 보상 시스템
- 자동 전투 로직
- 스테이지 진행 시스템
- 아이템 드롭 테이블

#### 💻 실습 내용
```csharp
// 1. 오프라인 보상 계산
public class OfflineRewardService
{
    private const int MAX_OFFLINE_HOURS = 12;
    private const double OFFLINE_EFFICIENCY = 0.5; // 온라인 대비 50% 효율
    
    public OfflineRewardResult CalculateRewards(Player player, DateTime lastLogin)
    {
        var offlineTime = DateTime.UtcNow - lastLogin;
        var effectiveHours = Math.Min(offlineTime.TotalHours, MAX_OFFLINE_HOURS);
        
        // 스테이지별 기본 보상
        var stageReward = GetStageReward(player.CurrentStage);
        
        // 캐릭터 전투력 보너스
        var combatPower = CalculateCombatPower(player.Characters);
        var combatBonus = combatPower * 0.001; // 0.1% per combat power
        
        // 최종 보상 계산
        var goldPerHour = stageReward.GoldPerHour * (1 + combatBonus) * OFFLINE_EFFICIENCY;
        var expPerHour = stageReward.ExpPerHour * (1 + combatBonus) * OFFLINE_EFFICIENCY;
        
        return new OfflineRewardResult
        {
            Gold = (long)(goldPerHour * effectiveHours),
            Experience = (long)(expPerHour * effectiveHours),
            Items = GenerateRandomItems(effectiveHours, player.CurrentStage),
            OfflineTime = offlineTime,
            EffectiveTime = TimeSpan.FromHours(effectiveHours)
        };
    }
}

// 2. 자동 전투 시스템
public class AutoBattleService
{
    public async Task<BattleResult> SimulateBattle(Character character, Monster monster)
    {
        var battle = new BattleSimulation();
        
        // 전투 시뮬레이션 (Unity의 물리 엔진처럼 서버에서 계산)
        while (character.CurrentHp > 0 && monster.CurrentHp > 0)
        {
            // 플레이어 공격
            var playerDamage = CalculateDamage(character, monster);
            monster.CurrentHp -= playerDamage;
            battle.AddLog($"Player deals {playerDamage} damage");
            
            if (monster.CurrentHp <= 0) break;
            
            // 몬스터 공격
            var monsterDamage = CalculateDamage(monster, character);
            character.CurrentHp -= monsterDamage;
            battle.AddLog($"Monster deals {monsterDamage} damage");
            
            battle.TurnCount++;
        }
        
        return new BattleResult
        {
            Victory = character.CurrentHp > 0,
            TurnCount = battle.TurnCount,
            Rewards = battle.Victory ? GenerateRewards(monster) : null,
            BattleLog = battle.GetLogs()
        };
    }
}

// 3. 스테이지 진행 시스템
public class StageProgressionService
{
    public async Task<StageResult> AttemptStage(Guid playerId, int stageId)
    {
        var player = await _playerRepository.GetByIdAsync(playerId);
        var stage = await _stageRepository.GetByIdAsync(stageId);
        
        // 스테이지 입장 조건 체크
        if (player.CurrentStage < stageId - 1)
            return StageResult.Locked;
        
        // 자동 전투 실행
        var battleResults = new List<BattleResult>();
        foreach (var wave in stage.Waves)
        {
            var result = await _autoBattleService.SimulateBattle(
                player.MainCharacter, wave.Monsters);
            
            battleResults.Add(result);
            
            if (!result.Victory)
                return StageResult.Failed(battleResults);
        }
        
        // 스테이지 클리어 보상
        await GrantStageRewards(player, stage);
        player.CurrentStage = Math.Max(player.CurrentStage, stageId);
        
        return StageResult.Success(battleResults, stage.Rewards);
    }
}
```

---

### **Week 7-8: 성능 최적화와 캐싱**

#### 📚 학습 목표
- Redis 캐싱 전략
- 응답 시간 최적화
- 데이터베이스 쿼리 최적화
- 부하 분산 전략

#### 💻 실습 내용
```csharp
// 1. Redis 캐싱 레이어
public class RedisCacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisCacheService> _logger;
    
    public async Task<T> GetOrSetAsync<T>(
        string key, 
        Func<Task<T>> factory, 
        TimeSpan? expiry = null)
    {
        var db = _redis.GetDatabase();
        
        // 캐시 조회
        var cached = await db.StringGetAsync(key);
        if (cached.HasValue)
        {
            _logger.LogDebug($"Cache hit: {key}");
            return JsonSerializer.Deserialize<T>(cached);
        }
        
        // 캐시 미스 - 데이터 생성
        _logger.LogDebug($"Cache miss: {key}");
        var value = await factory();
        
        // 캐시 저장
        var json = JsonSerializer.Serialize(value);
        await db.StringSetAsync(key, json, expiry ?? TimeSpan.FromMinutes(5));
        
        return value;
    }
    
    // 랭킹 시스템용 Sorted Set
    public async Task UpdateLeaderboard(Guid playerId, double score)
    {
        var db = _redis.GetDatabase();
        await db.SortedSetAddAsync("leaderboard:global", playerId.ToString(), score);
    }
    
    public async Task<LeaderboardEntry[]> GetTopPlayers(int count = 100)
    {
        var db = _redis.GetDatabase();
        var entries = await db.SortedSetRangeByRankWithScoresAsync(
            "leaderboard:global", 0, count - 1, Order.Descending);
        
        return entries.Select(e => new LeaderboardEntry
        {
            PlayerId = Guid.Parse(e.Element),
            Score = e.Score
        }).ToArray();
    }
}

// 2. 서비스 레이어 캐싱 적용
public class CachedPlayerService : IPlayerService
{
    private readonly IPlayerService _innerService;
    private readonly RedisCacheService _cache;
    
    public async Task<PlayerDto> GetByIdAsync(Guid id)
    {
        return await _cache.GetOrSetAsync(
            $"player:{id}",
            async () => await _innerService.GetByIdAsync(id),
            TimeSpan.FromMinutes(10)
        );
    }
    
    public async Task UpdateAsync(PlayerDto player)
    {
        await _innerService.UpdateAsync(player);
        // 캐시 무효화
        await _cache.InvalidateAsync($"player:{player.Id}");
    }
}

// 3. 응답 압축과 페이징
[ApiController]
[ResponseCompression]
public class LeaderboardController : ControllerBase
{
    [HttpGet("top")]
    [ResponseCache(Duration = 60)] // 1분 캐싱
    public async Task<IActionResult> GetTopPlayers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        if (pageSize > 100) pageSize = 100; // 최대 제한
        
        var players = await _leaderboardService.GetTopPlayersAsync(
            skip: (page - 1) * pageSize,
            take: pageSize
        );
        
        return Ok(new PagedResult<PlayerRankDto>
        {
            Items = players,
            Page = page,
            PageSize = pageSize,
            TotalCount = await _leaderboardService.GetTotalCountAsync()
        });
    }
}
```

---

### **Week 9: 실시간 통신과 알림**

#### 📚 학습 목표
- SignalR 실시간 통신
- 길드 채팅 시스템
- 실시간 이벤트 알림
- Unity 클라이언트 연동

#### 💻 실습 내용
```csharp
// 1. SignalR Hub 구현
public class GameHub : Hub
{
    private readonly IPlayerService _playerService;
    private readonly IGuildService _guildService;
    
    public override async Task OnConnectedAsync()
    {
        var playerId = Context.UserIdentifier;
        await Groups.AddToGroupAsync(Context.ConnectionId, $"player-{playerId}");
        
        // 길드 그룹 추가
        var guild = await _guildService.GetPlayerGuildAsync(playerId);
        if (guild != null)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"guild-{guild.Id}");
        }
        
        await base.OnConnectedAsync();
    }
    
    // 길드 채팅
    public async Task SendGuildMessage(string message)
    {
        var playerId = Context.UserIdentifier;
        var guild = await _guildService.GetPlayerGuildAsync(playerId);
        
        if (guild == null)
            throw new HubException("Not in a guild");
        
        var chatMessage = new GuildChatMessage
        {
            PlayerId = playerId,
            PlayerName = Context.User.Identity.Name,
            Message = message,
            Timestamp = DateTime.UtcNow
        };
        
        await Clients.Group($"guild-{guild.Id}")
            .SendAsync("ReceiveGuildMessage", chatMessage);
        
        // 채팅 로그 저장
        await _guildService.SaveChatMessageAsync(guild.Id, chatMessage);
    }
    
    // 실시간 전투 결과 알림
    public async Task NotifyBattleResult(BattleResult result)
    {
        await Clients.User(result.WinnerId)
            .SendAsync("BattleVictory", result);
        
        await Clients.User(result.LoserId)
            .SendAsync("BattleDefeat", result);
    }
}

// 2. Unity 클라이언트 SignalR 연결
public class SignalRManager : MonoBehaviour
{
    private HubConnection _connection;
    
    async void Start()
    {
        _connection = new HubConnectionBuilder()
            .WithUrl($"{ServerConfig.BaseUrl}/gamehub", options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(AuthManager.Token);
            })
            .Build();
        
        // 이벤트 핸들러 등록
        _connection.On<GuildChatMessage>("ReceiveGuildMessage", OnGuildMessage);
        _connection.On<BattleResult>("BattleVictory", OnBattleVictory);
        
        await _connection.StartAsync();
    }
    
    private void OnGuildMessage(GuildChatMessage message)
    {
        // Unity UI 업데이트
        ChatUI.Instance.AddMessage(message);
    }
    
    public async void SendGuildMessage(string text)
    {
        await _connection.InvokeAsync("SendGuildMessage", text);
    }
}
```

---

### **Week 10: 배포와 모니터링**

#### 📚 학습 목표
- Docker 컨테이너화
- CI/CD 파이프라인
- 로드 밸런싱
- 모니터링과 로깅

#### 💻 실습 내용
```yaml
# 1. Docker Compose 프로덕션 설정
version: '3.8'

services:
  nginx:
    image: nginx:alpine
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx/nginx.conf:/etc/nginx/nginx.conf
      - ./ssl:/etc/nginx/ssl
    depends_on:
      - api1
      - api2
      - api3
  
  api1:
    build: 
      context: .
      dockerfile: Dockerfile
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=${DB_CONNECTION}
      - Redis__ConnectionString=${REDIS_CONNECTION}
    depends_on:
      - postgres
      - redis
  
  api2:
    build: 
      context: .
      dockerfile: Dockerfile
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=${DB_CONNECTION}
      - Redis__ConnectionString=${REDIS_CONNECTION}
    depends_on:
      - postgres
      - redis
  
  api3:
    build: 
      context: .
      dockerfile: Dockerfile
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=${DB_CONNECTION}
      - Redis__ConnectionString=${REDIS_CONNECTION}
    depends_on:
      - postgres
      - redis
  
  postgres:
    image: postgres:15-alpine
    environment:
      - POSTGRES_USER=${DB_USER}
      - POSTGRES_PASSWORD=${DB_PASSWORD}
      - POSTGRES_DB=idle_rpg_prod
      - POSTGRES_MAX_CONNECTIONS=300
    volumes:
      - postgres_data:/var/lib/postgresql/data
    command: 
      - "postgres"
      - "-c"
      - "shared_buffers=256MB"
      - "-c"
      - "effective_cache_size=1GB"
      - "-c"
      - "maintenance_work_mem=64MB"
      - "-c"
      - "checkpoint_completion_target=0.9"
  
  redis:
    image: redis:7-alpine
    volumes:
      - redis_data:/data
    command: redis-server --appendonly yes --maxmemory 2gb --maxmemory-policy allkeys-lru
  
  prometheus:
    image: prom/prometheus
    volumes:
      - ./prometheus/prometheus.yml:/etc/prometheus/prometheus.yml
      - prometheus_data:/prometheus
  
  grafana:
    image: grafana/grafana
    ports:
      - "3000:3000"
    volumes:
      - grafana_data:/var/lib/grafana

volumes:
  postgres_data:
  redis_data:
  prometheus_data:
  grafana_data:
```

```csharp
// 2. 헬스체크와 메트릭
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // 헬스체크
        services.AddHealthChecks()
            .AddNpgSql(Configuration.GetConnectionString("DefaultConnection"))
            .AddRedis(Configuration.GetConnectionString("Redis"))
            .AddSignalRHub("/gamehub");
        
        // Prometheus 메트릭
        services.AddSingleton<IMetricReporter, PrometheusMetricReporter>();
    }
    
    public void Configure(IApplicationBuilder app)
    {
        // 헬스체크 엔드포인트
        app.UseHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
        
        // 메트릭 엔드포인트
        app.UseHttpMetrics();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapMetrics(); // /metrics
        });
    }
}

// 3. 부하 테스트
public class LoadTestScenarios
{
    [Test]
    public async Task Test_10000_ConcurrentUsers()
    {
        var tasks = new List<Task>();
        
        for (int i = 0; i < 10000; i++)
        {
            tasks.Add(SimulateUser(i));
        }
        
        var stopwatch = Stopwatch.StartNew();
        await Task.WhenAll(tasks);
        stopwatch.Stop();
        
        Console.WriteLine($"10,000 users processed in {stopwatch.ElapsedMilliseconds}ms");
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(60000)); // 1분 이내
    }
    
    private async Task SimulateUser(int userId)
    {
        // 로그인
        await LoginAsync($"user{userId}");
        
        // 오프라인 보상 수령
        await ClaimOfflineRewardsAsync();
        
        // 자동 전투 10회
        for (int i = 0; i < 10; i++)
        {
            await AutoBattleAsync();
            await Task.Delay(Random.Next(100, 500));
        }
        
        // 랭킹 조회
        await GetLeaderboardAsync();
    }
}
```

---

## 🎯 핵심 체크리스트

### **필수 구현 기능**
- [ ] 사용자 인증 (회원가입/로그인/JWT)
- [ ] 캐릭터 관리 (생성/레벨업/장비)
- [ ] 오프라인 보상 시스템
- [ ] 자동 전투 시스템
- [ ] 스테이지 진행
- [ ] 인벤토리 관리
- [ ] 아이템 강화/합성
- [ ] 가챠 시스템
- [ ] 길드 시스템
- [ ] 랭킹 시스템
- [ ] 일일/주간 퀘스트
- [ ] 이벤트 시스템

### **성능 목표**
- [ ] 동시 접속 10,000명 지원
- [ ] API 응답시간 100ms 이하
- [ ] 데이터베이스 쿼리 50ms 이하
- [ ] Redis 캐시 적중률 80% 이상
- [ ] 서버 가동률 99.9% 이상

### **보안 체크리스트**
- [ ] SQL Injection 방어
- [ ] XSS/CSRF 방어
- [ ] Rate Limiting
- [ ] DDoS 방어
- [ ] 민감 정보 암호화
- [ ] 안전한 통신 (HTTPS/WSS)

---

## 📚 추천 학습 자료

### **온라인 강좌**
1. **Microsoft Learn**: ASP.NET Core 공식 문서
2. **Pluralsight**: "Building a RESTful API with ASP.NET Core"
3. **Udemy**: "Complete guide to building an app with .Net Core and React"

### **책**
1. **"Clean Architecture"** - Robert C. Martin
2. **"Designing Data-Intensive Applications"** - Martin Kleppmann
3. **"Building Microservices"** - Sam Newman

### **유용한 라이브러리**
- **MediatR**: CQRS 패턴 구현
- **FluentValidation**: 유효성 검사
- **AutoMapper**: 객체 매핑
- **Polly**: 재시도 정책
- **Serilog**: 구조화된 로깅
- **Hangfire**: 백그라운드 작업

### **도구**
- **Postman/Insomnia**: API 테스트
- **Redis Desktop Manager**: Redis 관리
- **pgAdmin**: PostgreSQL 관리
- **Grafana**: 모니터링 대시보드
- **k6/JMeter**: 부하 테스트

---

## 🚀 최종 목표

### **3개월 후 달성 가능한 수준**
- ✅ 완전한 방치형 RPG 백엔드 구축
- ✅ 10,000 동시 접속 처리 가능
- ✅ Unity 클라이언트 완벽 연동
- ✅ 실시간 멀티플레이어 기능
- ✅ 자동화된 배포 파이프라인
- ✅ 24/7 모니터링 시스템

### **6개월 후 목표**
- 🎯 마이크로서비스 아키텍처 전환
- 🎯 Kubernetes 오케스트레이션
- 🎯 글로벌 서비스 (다국어/다중 리전)
- 🎯 AI 기반 게임 밸런싱
- 🎯 블록체인 연동 (NFT/토큰)

---

## 💡 Unity 개발자를 위한 팁

### **사고의 전환**
- **GameObject → Entity**: DB의 레코드가 Unity의 GameObject
- **Component → Service**: 비즈니스 로직을 서비스로 분리
- **Prefab → DTO**: 데이터 전송 객체로 템플릿화
- **Scene → Database**: 영구 저장소로 상태 관리
- **Coroutine → Async/Await**: 비동기 처리 패턴

### **공통 개념**
- **SOLID 원칙**: Unity와 서버 모두 동일하게 적용
- **디자인 패턴**: Singleton, Factory, Observer 등
- **의존성 주입**: Unity의 GetComponent와 유사
- **이벤트 시스템**: Unity의 UnityEvent와 SignalR

### **주의사항**
- **상태 관리**: 서버는 Stateless가 기본
- **동시성**: 멀티스레드 환경 고려
- **트랜잭션**: 데이터 일관성 보장
- **보안**: 클라이언트를 절대 신뢰하지 않음

---

**이제 체계적인 학습 로드맵이 완성되었습니다! 🎉**

Unity 개발 경험을 최대한 활용하면서 서버 개발 역량을 쌓아가세요. 궁금한 점이나 막히는 부분이 있으면 언제든지 질문해주세요!
