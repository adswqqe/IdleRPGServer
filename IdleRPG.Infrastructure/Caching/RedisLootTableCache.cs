using IdleRPG.Application.Interfaces;
using IdleRPG.Domain.Entities;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace IdleRPG.Infrastructure.Caching
{
    /// <summary>
    /// Redis (IDistributedCache) 기반 LootTable 캐싱 구현
    ///
    /// [학습 포인트 1] IDistributedCache vs IMemoryCache
    /// - IMemoryCache: 단일 서버 메모리에 객체 직접 저장
    /// - IDistributedCache: 여러 서버가 공유하는 Redis에 byte[] 저장
    ///
    /// [학습 포인트 2] 직렬화/역직렬화
    /// - Redis는 네트워크 전송을 위해 byte[] 필요
    /// - C# 객체 → JSON → UTF-8 byte[] → Redis
    /// - Redis → byte[] → UTF-8 string → JSON → C# 객체
    ///
    /// [학습 포인트 3] EF Core Entity 직렬화 주의사항
    /// - Navigation Property (virtual ICollection)는 Lazy Loading Proxy 객체
    /// - 순환 참조 가능성 (LootTable ↔ LootItem)
    /// - 해결: Repository에서 Include로 로드 후 Detach 또는 DTO 변환
    ///
    /// [학습 포인트 4] Redis 전환 시점
    /// - 단일 서버: IMemoryCache로 충분
    /// - 다중 서버 (로드 밸런싱): Redis 필요
    /// - 캐시 공유가 필요한 순간: InMemoryLootTableCache → RedisLootTableCache로 DI만 변경
    ///
    /// [현재 상태] 스켈레톤 코드 (미래 확장 준비)
    /// - Redis 패키지가 설치되지 않았으므로 주석 처리
    /// - 실제 사용 시: Program.cs에서 DI 등록 변경만으로 전환 가능
    /// </summary>
    public class RedisLootTableCache : ILootTableCache
    {
        private readonly IDistributedCache _cache;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RedisLootTableCache> _logger;

        // 캐시 키 관리
        private const string CacheKeyPrefix = "loot_table_";
        private static readonly TimeSpan DefaultExpiration = TimeSpan.FromHours(1);

        // JSON 직렬화 옵션
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            // EF Core Navigation Property 순환 참조 방지
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
            PropertyNameCaseInsensitive = true,
            WriteIndented = false  // Redis에는 압축된 JSON 저장
        };

        public RedisLootTableCache(
            IDistributedCache cache,
            IUnitOfWork unitOfWork,
            ILogger<RedisLootTableCache> logger)
        {
            _cache = cache;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<LootTable?> GetAsync(int id)
        {
            var cacheKey = GetCacheKey(id);

            // 1. Redis에서 조회
            var cachedBytes = await _cache.GetAsync(cacheKey);

            if (cachedBytes != null)
            {
                // 2. Cache Hit - 역직렬화
                try
                {
                    var json = Encoding.UTF8.GetString(cachedBytes);
                    var lootTable = JsonSerializer.Deserialize<LootTable>(json, JsonOptions);

                    _logger.LogDebug("Cache HIT for LootTable {Id} from Redis.", id);
                    return lootTable;
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Failed to deserialize LootTable {Id} from Redis cache. Removing corrupted cache.", id);
                    await _cache.RemoveAsync(cacheKey);
                    // Fall through to DB query
                }
            }

            // 3. Cache Miss - DB 조회
            _logger.LogInformation("Cache MISS for LootTable {Id}. Loading from database.", id);

            var table = await _unitOfWork.LootTables.GetWithItemsAsync(id);

            if (table == null)
            {
                _logger.LogWarning("LootTable {Id} not found in database.", id);
                return null;
            }

            // 4. Redis에 저장
            try
            {
                var json = JsonSerializer.Serialize(table, JsonOptions);
                var bytesToCache = Encoding.UTF8.GetBytes(json);

                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = DefaultExpiration
                };

                await _cache.SetAsync(cacheKey, bytesToCache, options);

                _logger.LogDebug("LootTable {Id} cached to Redis.", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cache LootTable {Id} to Redis.", id);
                // 캐싱 실패해도 DB 결과는 반환 (캐시는 optional)
            }

            return table;
        }

        public async Task RemoveAsync(int id)
        {
            var cacheKey = GetCacheKey(id);
            await _cache.RemoveAsync(cacheKey);

            _logger.LogInformation("Cache invalidated for LootTable {Id} in Redis.", id);
        }

        public async Task RemoveAllAsync()
        {
            // ⚠️ IDistributedCache도 전체 삭제 메서드가 없음!
            // 해결책:
            // Option A: Redis 직접 접근 (IConnectionMultiplexer) → FLUSHDB 명령
            // Option B: Key Pattern으로 검색 후 삭제 (SCAN loot_table_* → DEL)
            // Option C: 별도 Set으로 키 추적

            _logger.LogWarning(
                "RemoveAllAsync called. IDistributedCache does not support pattern-based deletion. " +
                "Consider using StackExchange.Redis directly for SCAN + DEL operations.");

            // TODO: Redis 직접 접근 구현
            await Task.CompletedTask;
        }

        /// <summary>
        /// 캐시 키 생성 헬퍼 메서드
        /// </summary>
        private static string GetCacheKey(int id) => $"{CacheKeyPrefix}{id}";
    }
}
