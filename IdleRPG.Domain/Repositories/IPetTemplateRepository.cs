using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Enums;

namespace IdleRPG.Domain.Repositories
{
    /// <summary>
    /// 펫 템플릿 리포지토리 인터페이스
    /// </summary>
    public interface IPetTemplateRepository
    {
        /// <summary>
        /// 희귀도별로 펫 템플릿을 조회합니다.
        /// </summary>
        /// <param name="rarity">펫 희귀도</param>
        /// <param name="cancellationToken">취소 토큰</param>
        /// <returns>해당 희귀도의 펫 템플릿 목록</returns>
        Task<List<PetTemplate>> GetByRarityAsync(Rarity rarity, CancellationToken cancellationToken = default);

        /// <summary>
        /// ID로 펫 템플릿을 조회합니다.
        /// </summary>
        /// <param name="templateId">펫 템플릿 ID</param>
        /// <param name="cancellationToken">취소 토큰</param>
        /// <returns>펫 템플릿 (없으면 null)</returns>
        Task<PetTemplate?> GetByIdAsync(int templateId, CancellationToken cancellationToken = default);

        /// <summary>
        /// 모든 펫 템플릿을 조회합니다.
        /// </summary>
        /// <param name="cancellationToken">취소 토큰</param>
        /// <returns>전체 펫 템플릿 목록</returns>
        Task<List<PetTemplate>> GetAllAsync(CancellationToken cancellationToken = default);
    }
}
