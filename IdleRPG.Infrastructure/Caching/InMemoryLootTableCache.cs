using IdleRPG.Application.Interfaces;
using IdleRPG.Domain.Entities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System.Collections.Concurrent;

namespace IdleRPG.Infrastructure.Caching
{
    /// <summary>
    /// IMemoryCache 기반 LootTable 캐싱 구현 (개선 버전)
    ///
    /// [학습 포인트 1] Cache-Aside Pattern (Lazy Loading)
    /// 1. 캐시 조회
    /// 2. Hit → 반환
    /// 3. Miss → DB 조회 → 캐시 저장 → 반환
    ///
    /// [학습 포인트 2] GetOrCreateAsync의 동시성 문제
    ///
    /// 기존 방식 (문제):
    /// - 100명이 동시에 같은 LootTable 요청
    /// - GetOrCreateAsync는 동시성 보장 안 함
    /// - 100번 DB 쿼리 발생! (Cache Stampede)
    ///
    /// 개선 방식 (Single-Flight):
    /// - Lazy<Task<T>>로 래핑
    /// - 첫 번째 요청만 DB 쿼리
    /// - 나머지 99개는 첫 번째 결과 await
    ///
    /// [학습 포인트 3] Lazy<T>의 동시성 모드
    ///
    /// LazyThreadSafetyMode 옵션:
    /// - None: 스레드 안전 없음 (단일 스레드 전용)
    /// - PublicationOnly: 여러 스레드가 초기화 시도, 첫 완료 값 사용
    /// - ExecutionAndPublication: 단 1개 스레드만 초기화 (우리 선택)
    ///
    /// 비유:
    /// - None: 문 잠금 없음 (충돌 위험)
    /// - PublicationOnly: 여러 명이 요리, 첫 완성품만 사용
    /// - ExecutionAndPublication: 1명만 요리, 나머지는 대기
    ///
    /// [학습 포인트 4] Global Cache Invalidation
    ///
    /// 문제: IMemoryCache는 전체 삭제 메서드 없음
    ///
    /// 해결: CancellationChangeToken
    /// - 모든 캐시 엔트리에 토큰 등록
    /// - RemoveAllAsync 호출 시 토큰 취소
    /// - IMemoryCache가 자동으로 모든 엔트리 제거
    ///
    /// [학습 포인트 5] ConcurrentDictionary<TKey, TValue>
    ///
    /// 일반 Dictionary vs ConcurrentDictionary:
    /// - Dictionary: lock 필요 (성능 저하, 데드락 위험)
    /// - ConcurrentDictionary: 락 없는 동시성 (Lock-Free)
    /// - 내부: Compare-And-Swap (CAS) 연산 활용
    ///
    /// 주의:
    /// - GetOrAdd는 atomic하지만 factory는 여러 번 실행 가능
    /// - 우리는 Lazy로 한 번만 실행 보장
    /// </summary>
    public class InMemoryLootTableCache : ILootTableCache
    {
        private readonly IMemoryCache _cache;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<InMemoryLootTableCache> _logger;

        // Single-Flight 패턴용: 동시 요청 중복 제거
        private readonly ConcurrentDictionary<string, Lazy<Task<LootTable?>>> _ongoingRequests = new();

        // Global Invalidation용: 전체 캐시 무효화 토큰
        private CancellationTokenSource _globalCts = new();

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

        /// <summary>
        /// LootTable 조회 (Single-Flight 패턴으로 동시성 문제 해결)
        ///
        /// [실행 흐름]
        /// 1. 캐시 확인 → Hit 시 즉시 반환
        /// 2. Miss 시 Lazy<Task<T>> 생성 (단 1회만 DB 쿼리 보장)
        /// 3. ConcurrentDictionary에 Lazy 저장 (다른 스레드가 재사용)
        /// 4. await Lazy.Value로 DB 결과 대기
        /// 5. 완료 후 ongoingRequests에서 제거 (메모리 정리)
        ///
        /// [동시성 시나리오]
        /// - Thread A: GetAsync(1) 호출 → Lazy 생성 → DB 쿼리 시작
        /// - Thread B: GetAsync(1) 호출 → 동일 Lazy 발견 → await (DB 쿼리 재사용)
        /// - Thread C: GetAsync(1) 호출 → 동일 Lazy 발견 → await (DB 쿼리 재사용)
        /// - 결과: DB 쿼리 1회, 3개 스레드 모두 동일 결과 받음
        /// </summary>
        public async Task<LootTable?> GetAsync(int id)
        {
            var cacheKey = GetCacheKey(id);

            // 1단계: 캐시 확인 (캐시된 Lazy<Task<T>> 확인)
            if (_cache.TryGetValue<LootTable>(cacheKey, out var cachedTable))
            {
                _logger.LogDebug("Cache HIT for LootTable {Id}.", id);
                return cachedTable;
            }

            // 2단계: Cache Miss → Lazy<Task<T>> 생성 또는 재사용
            // GetOrAdd: key가 없으면 factory 실행, 있으면 기존 값 반환
            var lazyTask = _ongoingRequests.GetOrAdd(cacheKey, _ =>
            {
                _logger.LogInformation("Cache MISS for LootTable {Id}. Initiating database load.", id);

                // Lazy 생성: 첫 .Value 접근 시 1회만 실행
                return new Lazy<Task<LootTable?>>(async () =>
                {
                    _logger.LogDebug("Executing DB query for LootTable {Id}.", id);

                    // DB에서 LootTable 조회
                    var table = await _unitOfWork.LootTables.GetWithItemsAsync(id);

                    if (table == null)
                    {
                        _logger.LogWarning("LootTable {Id} not found in database.", id);
                        return null;
                    }

                    // 캐시에 저장
                    var entryOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(DefaultExpiration)  // 1시간 후 만료
                        .SetPriority(CacheItemPriority.High)        // 높은 우선순위
                        .AddExpirationToken(new CancellationChangeToken(_globalCts.Token))  // Global Invalidation 지원
                        .RegisterPostEvictionCallback((key, value, reason, state) =>
                        {
                            _logger.LogInformation(
                                "Cache evicted for key {Key}. Reason: {Reason}",
                                key, reason);
                        });

                    _cache.Set(cacheKey, table, entryOptions);

                    _logger.LogDebug("LootTable {Id} cached successfully.", id);

                    return table;
                }, LazyThreadSafetyMode.ExecutionAndPublication);  // 단 1개 스레드만 실행
            });

            try
            {
                // 3단계: Lazy.Value await (다른 스레드가 이미 실행 중이면 대기)
                var result = await lazyTask.Value;
                return result;
            }
            finally
            {
                // 4단계: 완료 후 정리 (메모리 누수 방지)
                // TryRemove: 다른 스레드가 이미 제거했을 수 있으므로 Try 사용
                _ongoingRequests.TryRemove(cacheKey, out _);
            }
        }

        public Task RemoveAsync(int id)
        {
            var cacheKey = GetCacheKey(id);
            _cache.Remove(cacheKey);

            // ongoingRequests에서도 제거 (진행 중인 요청이 있을 수 있음)
            _ongoingRequests.TryRemove(cacheKey, out _);

            _logger.LogInformation("Cache invalidated for LootTable {Id}.", id);

            return Task.CompletedTask;
        }

        /// <summary>
        /// 전체 캐시 무효화 (CancellationChangeToken 활용)
        ///
        /// [학습 포인트] IMemoryCache는 RemoveAll()이 없음
        ///
        /// 해결 방법 3가지:
        /// 1. 모든 키 추적 (메모리 오버헤드, 복잡도 증가)
        /// 2. 캐시 인스턴스 재생성 (DI 구조상 어려움)
        /// 3. CancellationChangeToken (우리 선택)
        ///
        /// CancellationChangeToken 원리:
        /// - 캐시 엔트리 생성 시 토큰 등록
        /// - 토큰 취소 시 IMemoryCache가 자동으로 해당 엔트리 제거
        /// - O(1) 시간 복잡도 (키 순회 불필요)
        ///
        /// 주의사항:
        /// - CTS 취소 후에는 재사용 불가
        /// - 새로운 CTS 생성 필요
        /// - Dispose() 호출로 메모리 정리
        /// </summary>
        public Task RemoveAllAsync()
        {
            _logger.LogInformation("Invalidating all LootTable cache entries via global token.");

            // 1단계: 기존 토큰 취소 (모든 캐시 엔트리 제거 트리거)
            _globalCts.Cancel();

            // 2단계: 기존 CTS 리소스 해제
            _globalCts.Dispose();

            // 3단계: 새로운 CTS 생성 (향후 캐시용)
            _globalCts = new CancellationTokenSource();

            // 4단계: ongoingRequests도 전체 정리
            _ongoingRequests.Clear();

            _logger.LogInformation("All LootTable cache entries invalidated successfully.");

            return Task.CompletedTask;
        }

        /// <summary>
        /// 캐시 키 생성 헬퍼 메서드
        /// </summary>
        private static string GetCacheKey(int id) => $"{CacheKeyPrefix}{id}";
    }
}
