 [[🎮 방치형 RPG 서버 개발 완전 가이드 - Week 1]]# Clean Architecture - Infrastructure Layer 상세

## 🎯 Infrastructure Layer란?
**외부 시스템과의 연결**을 담당하는 레이어입니다. Unity의 **Services**(Networking, Analytics, CloudSave)와 같은 역할을 합니다.

## 🔑 핵심 특징
- **외부 의존성 처리**: 데이터베이스, 파일 시스템, 웹 API, 캐시 등
- **인터페이스 구현**: Application Layer에서 정의한 계약을 실제로 구현
- **기술적 세부사항**: ORM, HTTP 클라이언트, 메시지 큐 등 기술적 구현
- **환경 분리**: 개발/테스트/운영 환경별 다른 구현 가능

## 🎮 방치형 RPG에서의 Infrastructure 구현

### 1. Entity Framework Core 설정 (Data Persistence)

#### GameDbContext
```csharp
public class GameDbContext : DbContext
{
    public GameDbContext(DbContextOptions<GameDbContext> options) : base(options) { }
    
    // DbSet 정의 (Unity의 Addressable Asset과 유사한 개념)
    public DbSet<Player> Players { get; set; }
    public DbSet<Character> Characters { get; set; }
    public DbSet<OfflineReward> OfflineRewards { get; set; }
    public DbSet<Item> Items { get; set; }
    public DbSet<PlayerInventory> PlayerInventories { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Player 엔티티 설정
        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.HasIndex(p => p.Username).IsUnique();
            entity.HasIndex(p => p.Email).IsUnique();
            
            entity.Property(p => p.Username)
                  .HasMaxLength(50)
                  .IsRequired();
                  
            entity.Property(p => p.Email)
                  .HasMaxLength(100)  
                  .IsRequired();
                  
            entity.Property(p => p.CreatedAt)
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
        
        // Character 엔티티 설정  
        modelBuilder.Entity<Character>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.HasIndex(c => c.PlayerId); // 성능 최적화
            entity.HasIndex(c => c.Name).IsUnique();
            entity.HasIndex(c => c.Level); // 랭킹 조회 최적화
            
            entity.HasOne<Player>()
                  .WithMany()
                  .HasForeignKey(c => c.PlayerId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.Property(c => c.Class)
                  .HasConversion<string>(); // Enum을 문자열로 저장
        });
        
        // OfflineReward 엔티티 설정 (방치형 게임 핵심!)
        modelBuilder.Entity<OfflineReward>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.HasIndex(r => r.PlayerId);
            entity.HasIndex(r => new { r.PlayerId, r.IsClaimed }); // 복합 인덱스
            
            entity.HasOne<Player>()
                  .WithMany()
                  .HasForeignKey(r => r.PlayerId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.Property(r => r.BonusMultiplier)
                  .HasPrecision(5, 2); // decimal(5,2)
        });
        
        // 기본 데이터 시딩 (Unity의 Default Assets과 유사)
        SeedInitialData(modelBuilder);
    }
    
    private void SeedInitialData(ModelBuilder modelBuilder)
    {
        // 기본 아이템들 시딩
        modelBuilder.Entity<Item>().HasData(
            new Item("나무 검", ItemType.Weapon, ItemRarity.Common),
            new Item("철 검", ItemType.Weapon, ItemRarity.Rare),
            new Item("가죽 갑옷", ItemType.Armor, ItemRarity.Common),
            new Item("체력 포션", ItemType.Consumable, ItemRarity.Common)
        );
    }
}
```

### 2. Repository 패턴 구현 (Data Access)

#### IPlayerRepository 구현
```csharp
public class PlayerRepository : IPlayerRepository
{
    private readonly GameDbContext _context;
    private readonly ILogger<PlayerRepository> _logger;
    private readonly ICacheService _cache;
    
    public PlayerRepository(
        GameDbContext context,
        ILogger<PlayerRepository> logger,
        ICacheService cache)
    {
        _context = context;
        _logger = logger;
        _cache = cache;
    }
    
    public async Task<Player> GetByIdAsync(Guid id)
    {
        // 캐시 먼저 확인 (Unity의 Object Pool과 유사한 개념)
        var cacheKey = $"player:{id}";
        var cachedPlayer = await _cache.GetAsync<Player>(cacheKey);
        if (cachedPlayer != null)
        {
            return cachedPlayer;
        }
        
        // 데이터베이스에서 조회
        var player = await _context.Players
            .Include(p => p.Characters.Where(c => c.IsMain)) // 메인 캐릭터만 로드
            .FirstOrDefaultAsync(p => p.Id == id);
            
        if (player != null)
        {
            // 캐시에 저장 (5분간)
            await _cache.SetAsync(cacheKey, player, TimeSpan.FromMinutes(5));
        }
        
        return player;
    }
    
    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await _context.Players
            .AnyAsync(p => p.Username == username);
    }
    
    public async Task<List<Player>> GetTopPlayersByLevelAsync(int count)
    {
        // 복잡한 쿼리는 캐시 활용
        var cacheKey = $"leaderboard:top{count}";
        var cachedResult = await _cache.GetAsync<List<Player>>(cacheKey);
        if (cachedResult != null)
        {
            return cachedResult;
        }
        
        var topPlayers = await _context.Players
            .Join(_context.Characters.Where(c => c.IsMain),
                  p => p.Id,
                  c => c.PlayerId,
                  (p, c) => new { Player = p, Character = c })
            .OrderByDescending(pc => pc.Character.Level)
            .ThenByDescending(pc => pc.Character.Experience)
            .Take(count)
            .Select(pc => pc.Player)
            .ToListAsync();
            
        // 랭킹은 자주 변하지 않으므로 10분간 캐시
        await _cache.SetAsync(cacheKey, topPlayers, TimeSpan.FromMinutes(10));
        
        return topPlayers;
    }
    
    public async Task AddAsync(Player player)
    {
        await _context.Players.AddAsync(player);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation($"Player added: {player.Username} (ID: {player.Id})");
    }
    
    public async Task UpdateAsync(Player player)
    {
        _context.Players.Update(player);
        await _context.SaveChangesAsync();
        
        // 캐시 무효화
        await _cache.RemoveAsync($"player:{player.Id}");
        
        _logger.LogInformation($"Player updated: {player.Id}");
    }
}
```

### 3. Redis 캐싱 시스템

#### ICacheService 구현
```csharp
public interface ICacheService
{
    Task<T> GetAsync<T>(string key) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
    Task RemoveAsync(string key);
    Task<bool> ExistsAsync(string key);
}

public class RedisCacheService : ICacheService
{
    private readonly IDatabase _database;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    
    public RedisCacheService(
        IConnectionMultiplexer redis, 
        ILogger<RedisCacheService> logger)
    {
        _database = redis.GetDatabase();
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }
    
    public async Task<T> GetAsync<T>(string key) where T : class
    {
        try
        {
            var value = await _database.StringGetAsync(key);
            if (!value.HasValue)
                return null;
                
            return JsonSerializer.Deserialize<T>(value, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting cache key: {key}");
            return null; // 캐시 실패시 null 반환 (graceful degradation)
        }
    }
    
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        try
        {
            var json = JsonSerializer.Serialize(value, _jsonOptions);
            await _database.StringSetAsync(key, json, expiry ?? TimeSpan.FromHours(1));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error setting cache key: {key}");
            // 캐시 실패는 치명적이지 않음 - 로그만 기록
        }
    }
    
    public async Task RemoveAsync(string key)
    {
        try
        {
            await _database.KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error removing cache key: {key}");
        }
    }
    
    public async Task<bool> ExistsAsync(string key)
    {
        try
        {
            return await _database.KeyExistsAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error checking cache key existence: {key}");
            return false;
        }
    }
}
```

### 4. 백그라운드 작업 시스템 (Background Jobs)

#### OfflineRewardBackgroundService (방치형 게임 핵심!)
```csharp
public class OfflineRewardBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OfflineRewardBackgroundService> _logger;
    
    public OfflineRewardBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<OfflineRewardBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Offline reward background service started");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOfflineRewards();
                
                // 5분마다 실행 (Unity의 InvokeRepeating과 유사)
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in offline reward background service");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
        
        _logger.LogInformation("Offline reward background service stopped");
    }
    
    private async Task ProcessOfflineRewards()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<GameDbContext>();
        var rewardService = scope.ServiceProvider.GetRequiredService<IOfflineRewardService>();
        
        // 5분 이상 오프라인인 플레이어들 조회
        var cutoffTime = DateTime.UtcNow.AddMinutes(-5);
        var offlinePlayers = await context.Players
            .Where(p => p.LastLogin < cutoffTime && p.IsActive)
            .Take(100) // 배치 크기 제한 (성능 고려)
            .ToListAsync();
            
        var processedCount = 0;
        foreach (var player in offlinePlayers)
        {
            try
            {
                // 이미 계산된 미수령 보상이 있는지 확인
                var hasPendingReward = await context.OfflineRewards
                    .AnyAsync(r => r.PlayerId == player.Id && !r.IsClaimed);
                    
                if (!hasPendingReward)
                {
                    await rewardService.CalculateOfflineRewardAsync(player.Id);
                    processedCount++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to process offline reward for player {player.Id}");
            }
        }
        
        if (processedCount > 0)
        {
            _logger.LogInformation($"Processed offline rewards for {processedCount} players");
        }
    }
}
```

### 5. 외부 API 연동 (External Services)

#### Unity Analytics 연동 예시
```csharp
public interface IAnalyticsService
{
    Task TrackPlayerRegistrationAsync(Guid playerId, string username);
    Task TrackOfflineRewardClaimedAsync(Guid playerId, long goldAmount, long expAmount);
    Task TrackCharacterLevelUpAsync(Guid playerId, Guid characterId, int newLevel);
}

public class UnityAnalyticsService : IAnalyticsService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<UnityAnalyticsService> _logger;
    
    public UnityAnalyticsService(
        HttpClient httpClient,
        IConfiguration config,
        ILogger<UnityAnalyticsService> logger)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;
        
        // Unity Analytics API 설정
        _httpClient.BaseAddress = new Uri("https://analytics.unity.com/api/");
        _httpClient.DefaultRequestHeaders.Add("Authorization", 
            $"Bearer {_config["Unity:Analytics:ApiKey"]}");
    }
    
    public async Task TrackPlayerRegistrationAsync(Guid playerId, string username)
    {
        var eventData = new
        {
            eventName = "player_registration",
            userId = playerId.ToString(),
            customParameters = new
            {
                username = username,
                registrationDate = DateTime.UtcNow.ToString("O")
            }
        };
        
        await SendAnalyticsEventAsync(eventData);
    }
    
    public async Task TrackOfflineRewardClaimedAsync(Guid playerId, long goldAmount, long expAmount)
    {
        var eventData = new
        {
            eventName = "offline_reward_claimed",
            userId = playerId.ToString(),
            customParameters = new
            {
                goldAmount = goldAmount,
                experienceAmount = expAmount,
                claimTime = DateTime.UtcNow.ToString("O")
            }
        };
        
        await SendAnalyticsEventAsync(eventData);
    }
    
    private async Task SendAnalyticsEventAsync(object eventData)
    {
        try
        {
            var json = JsonSerializer.Serialize(eventData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("events", content);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Analytics API error: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send analytics event");
            // 분석 실패는 치명적이지 않음 - 로그만 기록
        }
    }
}
```

## 🔧 의존성 주입 설정

### Program.cs에서 Infrastructure 등록
```csharp
var builder = WebApplication.CreateBuilder(args);

// Entity Framework 설정
builder.Services.AddDbContext<GameDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("IdleRPG.Infrastructure")));

// Redis 설정
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});
builder.Services.AddSingleton<IConnectionMultiplexer>(provider =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")));

// Repository 등록
builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();
builder.Services.AddScoped<ICharacterRepository, CharacterRepository>();
builder.Services.AddScoped<IOfflineRewardRepository, OfflineRewardRepository>();

// Cache 서비스 등록
builder.Services.AddScoped<ICacheService, RedisCacheService>();

// 외부 서비스 등록
builder.Services.AddHttpClient<IAnalyticsService, UnityAnalyticsService>();

// 백그라운드 서비스 등록
builder.Services.AddHostedService<OfflineRewardBackgroundService>();

var app = builder.Build();

// 자동 마이그레이션 (개발 환경에서만)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<GameDbContext>();
    await context.Database.MigrateAsync();
}
```

## 🎮 Unity 개발자 관점에서의 이해

### Unity Services vs Infrastructure Layer
```csharp
// Unity에서
public class SaveSystem : MonoBehaviour
{
    void SavePlayerData(PlayerData data)
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("SaveData", json);
        PlayerPrefs.Save();
    }
    
    PlayerData LoadPlayerData()
    {
        string json = PlayerPrefs.GetString("SaveData");
        return JsonUtility.FromJson<PlayerData>(json);
    }
}

// Infrastructure Layer에서
public class PlayerRepository : IPlayerRepository
{
    public async Task AddAsync(Player player)
    {
        await _context.Players.AddAsync(player);
        await _context.SaveChangesAsync(); // 트랜잭션 보장
    }
    
    public async Task<Player> GetByIdAsync(Guid id)
    {
        // 캐시 확인 → DB 조회 → 캐시 저장
        return await GetWithCacheAsync(id);
    }
}
```

**차이점**:
- **확장성**: 여러 데이터베이스, 캐시, 외부 API 지원
- **성능**: 캐싱, 인덱싱, 배치 처리로 최적화
- **안정성**: 트랜잭션, 재시도, 에러 처리

## 💡 주요 설계 원칙

### 1. 관심사 분리
- 데이터 접근, 캐싱, 외부 API를 각각 분리된 서비스로 구현
- 하나의 기능 변경이 다른 기능에 영향 없음

### 2. 인터페이스 기반 설계  
- Application Layer가 구체 구현에 의존하지 않음
- 테스트시 Mock 객체로 쉽게 대체 가능

### 3. 성능 최적화
- Redis 캐싱으로 데이터베이스 부하 감소
- 인덱싱으로 쿼리 성능 향상
- 백그라운드 작업으로 비동기 처리

### 4. 장애 대응
- 외부 시스템 실패시 graceful degradation
- 재시도 메커니즘과 서킷 브레이커 패턴 적용

## 🔗 관련 문서
- [[Clean Architecture 개념 정리]]
- [[Clean Architecture - Domain Layer 상세]]
- [[Clean Architecture - Application Layer 상세]]
- [[Entity Framework Core 설정 가이드]]
- [[Redis 캐싱 전략]]

*작성일: 2025년 9월 26일*