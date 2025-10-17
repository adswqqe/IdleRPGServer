using IdleRPG.Domain.Entities;

namespace IdleRPG.Domain.Repositories
{
    /// <summary>
    /// 던전 플레이 기록 Repository 인터페이스 (Cold Table)
    /// </summary>
    public interface IDungeonRunHistoryRepository : IRepository<DungeonRunHistory>
    {
        /// <summary>
        /// 캐릭터의 최근 던전 기록 조회
        /// </summary>
        Task<List<DungeonRunHistory>> GetRecentByCharacterIdAsync(Guid characterId, int limit = 10);

        /// <summary>
        /// 특정 난이도의 클리어 기록 조회 (랭킹용)
        /// </summary>
        Task<List<DungeonRunHistory>> GetTopClearedAsync(int difficultyId, int limit = 10);

        /// <summary>
        /// 캐릭터의 특정 난이도 클리어 여부 확인
        /// </summary>
        Task<bool> HasClearedAsync(Guid characterId, int difficultyId);
    }
}
