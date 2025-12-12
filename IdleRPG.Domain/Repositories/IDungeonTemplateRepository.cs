using IdleRPG.Domain.Entities;

namespace IdleRPG.Domain.Repositories
{
    /// <summary>
    /// 던전 템플릿 Repository 인터페이스
    /// </summary>
    public interface IDungeonTemplateRepository : IRepository<DungeonTemplate>
    {
        /// <summary>
        /// ID로 던전 템플릿 조회
        /// </summary>
        Task<DungeonTemplate?> GetByIdAsync(int id);

        /// <summary>
        /// 난이도 정보를 포함하여 던전 템플릿 조회
        /// </summary>
        Task<DungeonTemplate?> GetWithDifficultiesAsync(int id);

        /// <summary>
        /// 활성화된 모든 던전 템플릿 조회
        /// </summary>
        Task<List<DungeonTemplate>> GetAllEnabledAsync();

        /// <summary>
        /// 카테고리별 던전 템플릿 조회
        /// </summary>
        Task<List<DungeonTemplate>> GetByCategoryAsync(DungeonCategory category);
    }
}
