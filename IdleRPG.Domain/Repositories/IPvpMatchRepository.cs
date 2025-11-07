using IdleRPG.Domain.Entities;

namespace IdleRPG.Domain.Repositories
{
    /// <summary>
    /// PvpMatch Repository 인터페이스
    /// PVP 매치 기록 관리 (생성, 조회, 히스토리)
    /// </summary>
    public interface IPvpMatchRepository
    {
        /// <summary>
        /// ID로 PVP 매치 조회
        /// </summary>
        /// <param name="id">매치 ID (Guid)</param>
        /// <param name="cancellationToken">취소 토큰</param>
        /// <returns>매치 정보 (없으면 null)</returns>
        Task<PvpMatch?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// 캐릭터별 매치 히스토리 조회 (페이징)
        /// </summary>
        /// <param name="characterId">캐릭터 ID</param>
        /// <param name="seasonId">시즌 ID (null이면 전체 시즌)</param>
        /// <param name="page">페이지 번호 (1부터 시작)</param>
        /// <param name="pageSize">페이지 크기</param>
        /// <param name="cancellationToken">취소 토큰</param>
        /// <returns>매치 기록 목록 및 전체 개수 (최신순)</returns>
        Task<(List<PvpMatch> matches, int totalCount)> GetMatchHistoryAsync(
            Guid characterId,
            int? seasonId,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 새 PVP 매치 추가
        /// </summary>
        /// <param name="match">매치 엔티티</param>
        /// <param name="cancellationToken">취소 토큰</param>
        Task AddAsync(PvpMatch match, CancellationToken cancellationToken = default);
    }
}
