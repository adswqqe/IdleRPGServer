using IdleRPG.Domain.Entities;

namespace IdleRPG.Domain.Repositories
{
    /// <summary>
    /// PvpSeason Repository 인터페이스
    /// 시즌 마스터 데이터 관리 (생성, 조회, 업데이트)
    /// </summary>
    public interface IPvpSeasonRepository
    {
        /// <summary>
        /// ID로 시즌 조회
        /// </summary>
        /// <param name="id">시즌 ID (int)</param>
        /// <param name="cancellationToken">취소 토큰</param>
        /// <returns>시즌 정보 (없으면 null)</returns>
        Task<PvpSeason?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// 현재 활성 시즌 조회 (IsActive = true)
        /// 시스템 전체에서 활성 시즌은 최대 1개만 존재
        /// </summary>
        /// <param name="cancellationToken">취소 토큰</param>
        /// <returns>활성 시즌 정보 (없으면 null)</returns>
        Task<PvpSeason?> GetActiveSeasonAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 시즌 번호로 조회
        /// </summary>
        /// <param name="seasonNumber">시즌 번호 (1, 2, 3...)</param>
        /// <param name="cancellationToken">취소 토큰</param>
        /// <returns>시즌 정보 (없으면 null)</returns>
        Task<PvpSeason?> GetBySeasonNumberAsync(int seasonNumber, CancellationToken cancellationToken = default);

        /// <summary>
        /// 새 시즌 추가
        /// </summary>
        /// <param name="season">시즌 엔티티</param>
        /// <param name="cancellationToken">취소 토큰</param>
        Task AddAsync(PvpSeason season, CancellationToken cancellationToken = default);

        /// <summary>
        /// 시즌 정보 업데이트 (IsActive 토글, 시즌 종료 등)
        /// </summary>
        /// <param name="season">시즌 엔티티</param>
        /// <param name="cancellationToken">취소 토큰</param>
        Task UpdateAsync(PvpSeason season, CancellationToken cancellationToken = default);
    }
}
