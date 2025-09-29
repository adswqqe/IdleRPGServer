using IdleRPG.Domain.Entities;
namespace IdleRPG.Application.Services
{
    /// <summary>
    /// JWT 토큰 생성 및 검증 서비스
    /// Unity의 PlayerPrefs처럼 토큰을 다루는 서비스
    /// </summary>
    public interface IJwtTokenService
    {
        /// <summary>
        /// Access Token 생성 (짧은 수명, 15분~1시간)
        /// </summary>
        string GenerateAccessToken(Player player);

        /// <summary>
        /// Refresh Token 생성 (긴 수명, 7일~30일)
        /// </summary>
        string GenerateRefreshToken();

        /// <summary>
        /// 토큰에서 사용자 ID 추출
        /// </summary>
        Guid? GetUserIdFromToken(string token);

        /// <summary>
        /// 토큰 유효성 검증
        /// </summary>
        bool ValidateToken(string token);
    }
}