using IdleRPG.Domain.Entities;

namespace IdleRPG.Application.Interfaces
{
    /// <summary>
    /// LootTable 캐싱을 위한 추상화 인터페이스
    ///
    /// [학습 포인트] Dependency Inversion Principle (DIP)
    /// - Application Layer는 구체적인 캐시 구현(IMemoryCache, Redis)을 몰라도 됨
    /// - Infrastructure Layer에서 구현체를 교체해도 비즈니스 로직은 변경 불필요
    ///
    /// [확장 계획]
    /// - 현재: InMemoryLootTableCache (IMemoryCache 사용)
    /// - 미래: RedisLootTableCache (IDistributedCache 사용)
    /// - DI 설정만 변경하면 전환 가능
    /// </summary>
    public interface ILootTableCache
    {
        /// <summary>
        /// LootTable을 캐시에서 조회합니다.
        /// Cache-Aside Pattern: 캐시 미스 시 DB에서 조회 후 캐시에 저장
        /// </summary>
        /// <param name="id">LootTable ID</param>
        /// <returns>LootTable (Items 포함), 없으면 null</returns>
        Task<LootTable?> GetAsync(int id);

        /// <summary>
        /// 특정 LootTable의 캐시를 무효화합니다.
        /// 사용 시나리오: 관리자가 LootTable 수정 후 즉시 반영
        /// </summary>
        /// <param name="id">무효화할 LootTable ID</param>
        Task RemoveAsync(int id);

        /// <summary>
        /// 모든 LootTable 캐시를 무효화합니다.
        /// 사용 시나리오: 대규모 밸런스 패치 후 전체 갱신
        /// </summary>
        Task RemoveAllAsync();
    }
}
