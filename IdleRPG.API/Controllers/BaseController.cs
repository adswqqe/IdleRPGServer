using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IdleRPG.API.Controllers
{
    /// <summary>
    /// 모든 게임 컨트롤러의 공통 베이스 클래스
    /// JWT 인증, 권한 검증 등 공통 로직 제공
    /// </summary>
    public abstract class BaseController : ControllerBase
    {
        /// <summary>
        /// JWT 토큰에서 현재 로그인한 사용자 ID 추출
        /// </summary>
        /// <returns>PlayerId (Guid)</returns>
        /// <exception cref="UnauthorizedAccessException">JWT 토큰이 없거나 NameIdentifier 클레임이 없을 때</exception>
        protected Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new UnauthorizedAccessException("인증 정보를 찾을 수 없습니다");
            }

            return Guid.Parse(userIdClaim);
        }
    }
}
