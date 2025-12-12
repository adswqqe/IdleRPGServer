using IdleRPG.API;
using IdleRPG.Application.Services;
using IdleRPG.Domain.Entities;
using IdleRPG.Domain.ValueObjects;
using IdleRPG.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace IdleRPG.Tests.Helpers;

/// <summary>
/// WebApplicationFactory를 커스터마이즈하여 통합 테스트를 위한 환경을 구성합니다.
/// In-Memory Database와 Test Doubles를 사용하여 외부 의존성을 제거합니다.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // 테스트 환경 설정 (Program.cs에서 감지)
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // 1. 기존 GameDBContext 제거 (PostgreSQL Provider 포함)
            var descriptorDbContext = services.SingleOrDefault(d => d.ServiceType == typeof(GameDBContext));
            if (descriptorDbContext != null)
            {
                services.Remove(descriptorDbContext);
            }

            var descriptorOptions = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<GameDBContext>));
            if (descriptorOptions != null)
            {
                services.Remove(descriptorOptions);
            }

            // 2. In-Memory Database로 교체 (테스트용)
            services.AddDbContext<GameDBContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryTestDb");
            });

            // 3. RedisCacheService Mock 처리 (실제 Redis 없이 테스트)
            services.RemoveAll<IRedisCacheService>();
            services.AddScoped<IRedisCacheService, FakeRedisCacheService>();

            // 4. 데이터베이스 초기화 (모든 서비스 등록 후 실행)
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<GameDBContext>();
            var logger = scopedServices.GetRequiredService<ILogger<CustomWebApplicationFactory>>();

            // 기존 데이터 삭제 후 초기화
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            try
            {
                // 초기 테스트 데이터 생성 (Helper 메서드 사용)
                SeedTestData(db);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "테스트 데이터 Seed 중 오류 발생");
            }
        });
    }

    /// <summary>
    /// 테스트용 초기 데이터를 생성합니다 (Player, Character, PvpSeason 등)
    /// </summary>
    private static void SeedTestData(GameDBContext context)
    {
        // 1. Player 생성 (Id 속성 사용)
        var player1 = new Player
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            UserName = "TestPlayer1",
            PasswordHash = "test-hash-1",
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow
        };

        var player2 = new Player
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
            UserName = "TestPlayer2",
            PasswordHash = "test-hash-2",
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow
        };

        context.Players.AddRange(player1, player2);

        // 2. Character 생성 (BaseEntity의 Id 속성 사용, Stats는 CharacterStats)
        var character1 = new Character
        {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
            PlayerId = player1.Id,
            Level = 10,
            Experience = 1000,
            Gold = 5000,
            Crystal = 1000,
            Stats = new CharacterStats(100, 50, 500, 0.1f, 1.5f, 0.05f, 1.0f), // Attack, Defense, HP, CritRate, CritDamage, Evasion, AttackSpeed
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var character2 = new Character
        {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000002"),
            PlayerId = player2.Id,
            Level = 12,
            Experience = 1500,
            Gold = 6000,
            Crystal = 1200,
            Stats = new CharacterStats(120, 60, 600, 0.1f, 1.5f, 0.05f, 1.0f),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Characters.AddRange(character1, character2);

        // 3. PvpSeason 생성
        var season = new PvpSeason
        {
            Id = 1,
            SeasonNumber = 1,
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow.AddDays(30),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Set<PvpSeason>().Add(season);

        // 4. PvpRanking 생성
        var ranking1 = new PvpRanking
        {
            SeasonId = 1,
            CharacterId = character1.Id,
            Rating = 1200,
            Wins = 5,
            Losses = 3,
            WinStreak = 2,
            IsRewardClaimed = false,
            LastMatchAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow
        };

        var ranking2 = new PvpRanking
        {
            SeasonId = 1,
            CharacterId = character2.Id,
            Rating = 1300,
            Wins = 7,
            Losses = 2,
            WinStreak = 4,
            IsRewardClaimed = false,
            LastMatchAt = DateTime.UtcNow.AddHours(-5),
            UpdatedAt = DateTime.UtcNow
        };

        context.Set<PvpRanking>().AddRange(ranking1, ranking2);

        context.SaveChanges();
    }
}

/// <summary>
/// 테스트용 Fake RedisCacheService 구현체 (Redis 없이 테스트 가능)
/// Redis 실패 시 null 반환하여 PostgreSQL Fallback을 트리거합니다.
/// </summary>
public class FakeRedisCacheService : IRedisCacheService
{
    public Task UpdateRankingCacheAsync(int seasonId, Guid characterId, int rating, CancellationToken cancellationToken = default)
    {
        // Redis 없이 테스트하므로 아무 작업도 하지 않음 (Best Effort)
        return Task.CompletedTask;
    }

    public Task<Dictionary<Guid, int>?> GetTopRankingsAsync(int seasonId, int count, CancellationToken cancellationToken = default)
    {
        // null 반환 → Controller에서 PostgreSQL Fallback 트리거
        return Task.FromResult<Dictionary<Guid, int>?>(null);
    }

    public Task<int?> GetMyRankAsync(int seasonId, Guid characterId, CancellationToken cancellationToken = default)
    {
        // null 반환 → PostgreSQL Fallback
        return Task.FromResult<int?>(null);
    }

    public Task<Dictionary<Guid, int>?> GetRankingsAroundMeAsync(int seasonId, Guid characterId, int range, CancellationToken cancellationToken = default)
    {
        // null 반환 → PostgreSQL Fallback
        return Task.FromResult<Dictionary<Guid, int>?>(null);
    }

    public Task ClearRankingCacheAsync(int seasonId, CancellationToken cancellationToken = default)
    {
        // Redis 없이 테스트하므로 아무 작업도 하지 않음
        return Task.CompletedTask;
    }

    public Task<bool> AcquireLockAsync(string lockKey, string lockToken, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        // 분산 락 없이 테스트 (단일 서버 가정)
        return Task.FromResult(true);
    }

    public Task<bool> ExtendLockAsync(string lockKey, string lockToken, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        // 분산 락 없이 테스트
        return Task.FromResult(true);
    }

    public Task<bool> ReleaseLockAsync(string lockKey, string lockToken, CancellationToken cancellationToken = default)
    {
        // 분산 락 없이 테스트
        return Task.FromResult(true);
    }

    public Task<bool> UpdateRankingCacheBatchAsync(int seasonId, IEnumerable<(Guid CharacterId, int Rating)> rankings, CancellationToken cancellationToken = default)
    {
        // Batch 업데이트 없이 테스트
        return Task.FromResult(true);
    }
}
