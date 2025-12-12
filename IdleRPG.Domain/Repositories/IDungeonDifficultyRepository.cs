using IdleRPG.Domain.Entities;

namespace IdleRPG.Domain.Repositories
{
    /// <summary>
    /// 던전 난이도 Repository 인터페이스
    /// </summary>
    public interface IDungeonDifficultyRepository : IRepository<DungeonDifficulty>
    {
        /// <summary>
        /// ID로 난이도 조회
        /// </summary>
        Task<DungeonDifficulty?> GetByIdAsync(int id);

        /// <summary>
        /// 웨이브 정보를 포함하여 난이도 조회
        /// </summary>
        Task<DungeonDifficulty?> GetWithWavesAsync(int id);

        /// <summary>
        /// 특정 던전 템플릿의 모든 난이도 조회
        /// </summary>
        Task<List<DungeonDifficulty>> GetByTemplateIdAsync(int templateId);

        /// <summary>
        /// 특정 던전 템플릿의 특정 난이도 조회
        /// </summary>
        Task<DungeonDifficulty?> GetByTemplateAndCodeAsync(int templateId, DifficultyCode code);
    }
}
