namespace IdleRPG.Application.DTOs.Auth
{
    /// <summary>
    /// 인증 응답 DTO (로그인 성공 시)
    /// </summary>
    public class AuthResponseDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

        /// <summary>
        /// 액세스 토큰 만료까지 남은 시간 (초 단위)
        /// Unity 클라이언트에서 타이머 표시 용이
        /// </summary>
        public long ExpiresIn { get; set; }

        public Guid PlayerId { get; set; }
        public string UserName { get; set; }
    }
}
