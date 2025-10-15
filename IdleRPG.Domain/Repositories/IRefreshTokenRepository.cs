using IdleRPG.Domain.Entities;

namespace IdleRPG.Domain.Repositories
{
    /// <summary>
    /// Refresh Token Repository 인터페이스
    /// </summary>
    public interface IRefreshTokenRepository : IRepository<RefreshToken>
    {
        /// <summary>
        /// Token 문자열로 RefreshToken을 조회 (Player 포함)
        /// </summary>
        Task<RefreshToken?> GetByTokenWithPlayerAsync(string token);

        /// <summary>
        /// PlayerId와 Token으로 RefreshToken을 조회
        /// </summary>
        Task<RefreshToken?> GetByPlayerIdAndTokenAsync(Guid playerId, string token);

        /// <summary>
        /// RefreshToken 삭제
        /// </summary>
        void Delete(RefreshToken entity);
    }
}
