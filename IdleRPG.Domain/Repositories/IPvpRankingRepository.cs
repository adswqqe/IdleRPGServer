using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Enums;

namespace IdleRPG.Domain.Repositories
{
    /// <summary>
    /// PvpRanking Repository 인터페이스
    /// 시즌별 캐릭터 랭킹 관리 (복합키: SeasonId, CharacterId)
    /// </summary>
    public interface IPvpRankingRepository
    {
        /// <summary>
        /// 복합키로 랭킹 조회 (SeasonId, CharacterId)
        /// </summary>
        /// <param name="seasonId">시즌 ID</param>
        /// <param name="characterId">캐릭터 ID</param>
        /// <param name="cancellationToken">취소 토큰</param>
        /// <returns>랭킹 정보 (없으면 null)</returns>
        Task<PvpRanking?> GetByIdAsync(int seasonId, Guid characterId, CancellationToken cancellationToken = default);

        /// <summary>
        /// 여러 캐릭터의 랭킹 벌크 조회 (N+1 쿼리 방지)
        /// Redis에서 가져온 CharacterId 목록으로 DB 조회 시 사용
        /// </summary>
        /// <param name="seasonId">시즌 ID</param>
        /// <param name="characterIds">캐릭터 ID 목록</param>
        /// <param name="cancellationToken">취소 토큰</param>
        /// <returns>랭킹 리스트 (존재하는 것만 반환)</returns>
        Task<List<PvpRanking>> GetByCharacterIdsAsync(int seasonId, IEnumerable<Guid> characterIds, CancellationToken cancellationToken = default);

        /// <summary>
        /// Top N 랭킹 조회 (레이팅 내림차순)
        /// Redis 캐싱 미스 시 PostgreSQL Fallback 용도
        /// </summary>
        /// <param name="seasonId">시즌 ID</param>
        /// <param name="count">조회할 랭킹 개수 (예: 100)</param>
        /// <param name="cancellationToken">취소 토큰</param>
        /// <returns>Top N 랭킹 리스트</returns>
        Task<List<PvpRanking>> GetTopRankingsAsync(int seasonId, int count, CancellationToken cancellationToken = default);

        /// <summary>
        /// 특정 레이팅 주변 랭킹 조회 (내 주변 ±range등)
        /// </summary>
        /// <param name="seasonId">시즌 ID</param>
        /// <param name="rating">기준 레이팅</param>
        /// <param name="range">조회 범위 (예: ±10등)</param>
        /// <param name="cancellationToken">취소 토큰</param>
        /// <returns>주변 랭킹 리스트</returns>
        Task<List<PvpRanking>> GetRankingsAroundAsync(int seasonId, int rating, int range, CancellationToken cancellationToken = default);

        /// <summary>
        /// 티어별 랭킹 조회 (페이징)
        /// </summary>
        /// <param name="seasonId">시즌 ID</param>
        /// <param name="tier">PVP 티어 (Bronze, Silver, Gold, Platinum, Diamond)</param>
        /// <param name="page">페이지 번호 (1부터 시작)</param>
        /// <param name="pageSize">페이지 크기 (기본 50)</param>
        /// <param name="cancellationToken">취소 토큰</param>
        /// <returns>해당 티어 랭킹 리스트</returns>
        Task<List<PvpRanking>> GetByTierAsync(int seasonId, PvpTier tier, int page, int pageSize, CancellationToken cancellationToken = default);

        /// <summary>
        /// 새 랭킹 추가 (첫 매칭 시)
        /// </summary>
        /// <param name="ranking">랭킹 엔티티</param>
        /// <param name="cancellationToken">취소 토큰</param>
        Task AddAsync(PvpRanking ranking, CancellationToken cancellationToken = default);

        /// <summary>
        /// 랭킹 정보 업데이트 (레이팅, 승패 횟수, 연승 등)
        /// </summary>
        /// <param name="ranking">랭킹 엔티티</param>
        /// <param name="cancellationToken">취소 토큰</param>
        Task UpdateAsync(PvpRanking ranking, CancellationToken cancellationToken = default);
    }
}
