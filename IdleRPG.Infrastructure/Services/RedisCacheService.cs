using IdleRPG.Application.Services;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace IdleRPG.Infrastructure.Services;

/// <summary>
/// Redis 캐싱 서비스 구현체 (PVP 랭킹 전용)
/// </summary>
/// <remarks>
/// - Redis Sorted Set 사용: Key = pvp:ranking:season:{seasonId}
/// - Write-Through 전략: PostgreSQL 업데이트 성공 후 Redis 갱신
/// - Best Effort: Redis 장애 시 로깅만 하고 null 반환 (PostgreSQL Fallback)
/// </remarks>
public class RedisCacheService : IRedisCacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(
        IConnectionMultiplexer redis,
        ILogger<RedisCacheService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    /// <summary>
    /// PVP 랭킹 캐시를 업데이트합니다 (Write-Through).
    /// </summary>
    public async Task UpdateRankingCacheAsync(int seasonId, Guid characterId, int rating, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var key = GetRankingKey(seasonId);

            // Redis 명령어: ZADD pvp:ranking:season:{seasonId} {rating} {characterId}
            // C# API 순서: SortedSetAddAsync(key, member, score)
            await db.SortedSetAddAsync(key, characterId.ToString(), rating);

            _logger.LogDebug("Redis 랭킹 갱신 성공: SeasonId={SeasonId}, CharacterId={CharacterId}, Rating={Rating}",
                seasonId, characterId, rating);
        }
        catch (RedisException ex)
        {
            // Redis 장애 시 로깅만 (PostgreSQL이 Source of Truth, Best Effort)
            _logger.LogWarning(ex, "Redis 랭킹 갱신 실패 (SeasonId={SeasonId}, CharacterId={CharacterId}): {Error}",
                seasonId, characterId, ex.Message);
            throw; // Controller에서 catch하여 로깅 후 계속 진행
        }
    }

    /// <summary>
    /// Top N 랭킹을 조회합니다 (Redis Sorted Set, 레이팅 DESC).
    /// </summary>
    public async Task<Dictionary<Guid, int>?> GetTopRankingsAsync(int seasonId, int count, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var key = GetRankingKey(seasonId);

            // ZREVRANGE pvp:ranking:season:{seasonId} 0 {count-1} WITHSCORES
            var results = await db.SortedSetRangeByRankWithScoresAsync(
                key,
                start: 0,
                stop: count - 1,
                order: Order.Descending);

            if (results.Length == 0)
            {
                _logger.LogDebug("Redis Top 랭킹 조회 결과 없음: SeasonId={SeasonId}, Count={Count}", seasonId, count);
                return null;
            }

            var rankings = new Dictionary<Guid, int>();
            foreach (var entry in results)
            {
                if (Guid.TryParse(entry.Element, out var characterId))
                {
                    rankings[characterId] = (int)entry.Score;
                }
            }

            _logger.LogDebug("Redis Top 랭킹 조회 성공: SeasonId={SeasonId}, Count={Count}, ResultCount={ResultCount}",
                seasonId, count, rankings.Count);

            return rankings;
        }
        catch (RedisException ex)
        {
            _logger.LogWarning(ex, "Redis Top 랭킹 조회 실패 (SeasonId={SeasonId}): {Error}", seasonId, ex.Message);
            return null; // Fallback to PostgreSQL
        }
    }

    /// <summary>
    /// 내 랭킹 순위를 조회합니다 (Redis Sorted Set, 1-based index).
    /// </summary>
    public async Task<int?> GetMyRankAsync(int seasonId, Guid characterId, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var key = GetRankingKey(seasonId);

            // ZREVRANK pvp:ranking:season:{seasonId} {characterId}
            var rank = await db.SortedSetRankAsync(key, characterId.ToString(), order: Order.Descending);

            if (!rank.HasValue)
            {
                _logger.LogDebug("Redis 내 순위 조회 결과 없음: SeasonId={SeasonId}, CharacterId={CharacterId}",
                    seasonId, characterId);
                return null;
            }

            // 0-based → 1-based 변환 (1등 = 1)
            var oneBased = (int)rank.Value + 1;

            _logger.LogDebug("Redis 내 순위 조회 성공: SeasonId={SeasonId}, CharacterId={CharacterId}, Rank={Rank}",
                seasonId, characterId, oneBased);

            return oneBased;
        }
        catch (RedisException ex)
        {
            _logger.LogWarning(ex, "Redis 내 순위 조회 실패 (SeasonId={SeasonId}, CharacterId={CharacterId}): {Error}",
                seasonId, characterId, ex.Message);
            return null; // Fallback to PostgreSQL
        }
    }

    /// <summary>
    /// 내 주변 ±range 등 랭킹을 조회합니다 (Redis Sorted Set).
    /// </summary>
    public async Task<Dictionary<Guid, int>?> GetRankingsAroundMeAsync(int seasonId, Guid characterId, int range, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var key = GetRankingKey(seasonId);

            // Step 1: ZREVRANK - 내 순위 조회
            var myRank = await db.SortedSetRankAsync(key, characterId.ToString(), order: Order.Descending);

            if (!myRank.HasValue)
            {
                _logger.LogDebug("Redis 내 주변 랭킹 조회 실패: 내 순위 없음. SeasonId={SeasonId}, CharacterId={CharacterId}",
                    seasonId, characterId);
                return null;
            }

            // Step 2: ZREVRANGE - 내 순위 ± range (0-based 인덱스)
            long startRank = Math.Max(0, myRank.Value - range);
            long endRank = myRank.Value + range;

            var results = await db.SortedSetRangeByRankWithScoresAsync(
                key,
                start: startRank,
                stop: endRank,
                order: Order.Descending);

            if (results.Length == 0)
            {
                _logger.LogDebug("Redis 내 주변 랭킹 조회 결과 없음: SeasonId={SeasonId}, CharacterId={CharacterId}, Range={Range}",
                    seasonId, characterId, range);
                return null;
            }

            var rankings = new Dictionary<Guid, int>();
            foreach (var entry in results)
            {
                if (Guid.TryParse(entry.Element, out var charId))
                {
                    rankings[charId] = (int)entry.Score;
                }
            }

            _logger.LogDebug("Redis 내 주변 랭킹 조회 성공: SeasonId={SeasonId}, CharacterId={CharacterId}, Range={Range}, ResultCount={ResultCount}",
                seasonId, characterId, range, rankings.Count);

            return rankings;
        }
        catch (RedisException ex)
        {
            _logger.LogWarning(ex, "Redis 내 주변 랭킹 조회 실패 (SeasonId={SeasonId}, CharacterId={CharacterId}): {Error}",
                seasonId, characterId, ex.Message);
            return null; // Fallback to PostgreSQL
        }
    }

    /// <summary>
    /// 특정 시즌의 랭킹 캐시를 초기화합니다 (시즌 종료 또는 Soft Reset 시).
    /// </summary>
    public async Task ClearRankingCacheAsync(int seasonId, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var key = GetRankingKey(seasonId);

            // DEL pvp:ranking:season:{seasonId}
            await db.KeyDeleteAsync(key);

            _logger.LogInformation("Redis 랭킹 캐시 초기화 성공: SeasonId={SeasonId}", seasonId);
        }
        catch (RedisException ex)
        {
            _logger.LogWarning(ex, "Redis 랭킹 캐시 초기화 실패 (SeasonId={SeasonId}): {Error}", seasonId, ex.Message);
            throw; // 시즌 관리 로직에서 에러 처리 필요
        }
    }

    /// <summary>
    /// Redis Key 생성 헬퍼 메서드
    /// </summary>
    private static string GetRankingKey(int seasonId) => $"pvp:ranking:season:{seasonId}";
}
