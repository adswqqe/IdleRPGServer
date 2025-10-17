using IdleRPG.Application.Interfaces;
using IdleRPG.Domain.Entities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace IdleRPG.Infrastructure.Caching
{
    /// <summary>
    /// IMemoryCache 기반 LootTable 캐싱 구현
    ///
    /// [학습 포인트 1] Cache-Aside Pattern (Lazy Loading)
    /// 1. 캐시 조회
    /// 2. Hit → 반환
    /// 3. Miss → DB 조회 → 캐시 저장 → 반환
    ///
    /// [학습 포인트 2] GetOrCreateAsync
    /// - 1-3 과정을 자동으로 처리해주는 편의 메서드
    /// - 캐시 미스 시에만 factory 람다 실행
    ///
    /// [학습 포인트 3] 캐시 만료 전략
    /// - AbsoluteExpiration: 생성 후 N시간 뒤 무조건 삭제
    /// - SlidingExpiration: 마지막 사용 후 N시간 미사용 시 삭제
    /// - 이 클래스는 Absolute 사용 (마스터 데이터 특성)
    ///
    /// [학습 포인트 4] CacheItemPriority
    /// - High: 메모리 압박 시에도 최대한 유지
    /// - Normal: 일반적인 우선순위
    /// - Low: 메모리 부족 시 먼저 제거
    /// - LootTable은 자주 사용되므로 High 설정
    /// </summary>
    public class InMemoryLootTableCache : ILootTableCache
    {
        private readonly IMemoryCache _cache;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<InMemoryLootTableCache> _logger;

        // 캐시 키 관리 (중복 방지, 오타 방지)
        private const string CacheKeyPrefix = "loot_table_";
        private static readonly TimeSpan DefaultExpiration = TimeSpan.FromHours(1);

        public InMemoryLootTableCache(
            IMemoryCache cache,
            IUnitOfWork unitOfWork,
            ILogger<InMemoryLootTableCache> logger)
        {
            _cache = cache;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<LootTable?> GetAsync(int id)
        {
            var cacheKey = GetCacheKey(id);

            // GetOrCreateAsync: 캐시 Hit 시 바로 반환, Miss 시 factory 실행
            var lootTable = await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                _logger.LogInformation("Cache MISS for LootTable {Id}. Loading from database.", id);

                // 캐시 엔트리 설정
                entry.AbsoluteExpirationRelativeToNow = DefaultExpiration;  // 1시간 후 자동 삭제
                entry.Priority = CacheItemPriority.High;  // 메모리 압박 시에도 유지

                // 콜백 등록: 캐시가 제거될 때 로그 출력
                entry.RegisterPostEvictionCallback((key, value, reason, state) =>
                {
                    _logger.LogInformation(
                        "Cache evicted for key {Key}. Reason: {Reason}",
                        key, reason);
                });

                // DB에서 LootTable 조회 (Items 포함)
                var table = await _unitOfWork.LootTables.GetWithItemsAsync(id);

                if (table == null)
                {
                    _logger.LogWarning("LootTable {Id} not found in database.", id);
                }

                return table;
            });

            if (lootTable != null)
            {
                _logger.LogDebug("Cache HIT for LootTable {Id}.", id);
            }

            return lootTable;
        }

        public Task RemoveAsync(int id)
        {
            var cacheKey = GetCacheKey(id);
            _cache.Remove(cacheKey);

            _logger.LogInformation("Cache invalidated for LootTable {Id}.", id);

            return Task.CompletedTask;
        }

        public Task RemoveAllAsync()
        {
            // ⚠️ 주의: IMemoryCache는 전체 삭제 메서드가 없음!
            // 해결책: 모든 키를 추적하거나, 캐시를 새로 생성
            // 현재는 로그만 남김 (실제로는 별도 키 관리 필요)

            _logger.LogWarning(
                "RemoveAllAsync called, but IMemoryCache does not support bulk removal. " +
                "Individual keys must be tracked and removed. " +
                "Consider using IDistributedCache (Redis) for this feature.");

            // TODO: 키 추적 메커니즘 구현 또는 Redis 전환 시 해결
            return Task.CompletedTask;
        }

        /// <summary>
        /// 캐시 키 생성 헬퍼 메서드
        /// </summary>
        private static string GetCacheKey(int id) => $"{CacheKeyPrefix}{id}";
    }
}
