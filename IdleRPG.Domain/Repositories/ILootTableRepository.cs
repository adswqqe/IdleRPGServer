using IdleRPG.Domain.Entities;

namespace IdleRPG.Domain.Repositories
{
    /// <summary>
    /// Loot Table Repository 인터페이스
    /// </summary>
    public interface ILootTableRepository : IRepository<LootTable>
    {
        /// <summary>
        /// ID로 Loot Table 조회
        /// </summary>
        Task<LootTable?> GetByIdAsync(int id);

        /// <summary>
        /// LootItem들을 포함하여 Loot Table 조회 (보상 계산용)
        /// </summary>
        Task<LootTable?> GetWithItemsAsync(int id);
    }
}
