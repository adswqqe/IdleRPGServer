using IdleRPG.Domain.Entities;

namespace IdleRPG.Domain.Repositories
{
    /// <summary>
    /// 던전 진행 상황 Repository 인터페이스 (Hot Table)
    /// </summary>
    public interface IDungeonProgressRepository : IRepository<DungeonProgress>
    {
        /// <summary>
        /// 캐릭터의 현재 진행 중인 던전 조회 (Unique Index 활용)
        /// </summary>
        Task<DungeonProgress?> GetByCharacterIdAsync(Guid characterId);

        /// <summary>
        /// 난이도 정보를 포함하여 진행 상황 조회
        /// </summary>
        Task<DungeonProgress?> GetWithDifficultyAsync(Guid characterId);

        /// <summary>
        /// 진행 상황 삭제 (던전 완료/실패 시)
        /// </summary>
        void Delete(DungeonProgress progress);
    }
}
