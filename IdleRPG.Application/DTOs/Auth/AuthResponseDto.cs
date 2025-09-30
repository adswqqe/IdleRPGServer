using IdleRPG.Application.DTOs.Player;
namespace IdleRPG.Application.DTOs.Auth
{
    /// <summary>
    /// 인증 응답 DTO (로그인 성공 시)
    /// </summary>
    public class AuthResponseDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public PlayerDto Player { get; set; }
    }
}