using IdleRPG.Application.DTOs.Auth;
using IdleRPG.Application.Auth.Services;
using IdleRPG.Application.DTOs.Player;
using IdleRPG.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace IdleRPG.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// 회원가입 (Unity의 새 플레이어 생성)
        /// </summary>
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            try
            {
                var response = await _authService.RegisterAsync(dto);

                return CreatedAtAction(nameof(GetProfile), new
                {
                }, response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }
        
        /// <summary>
        /// 로그인 (Unity의 플레이어 인증)
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                var response = await _authService.LoginAsync(dto);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }
        
        /// <summary>
        /// Refresh Token으로 새 Access Token 발급
        /// Unity에서 세션 갱신할 때 호출
        /// </summary>
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto)
        {
            try
            {
                var response = await _authService.RefreshTokenAsync(dto.RefreshToken);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }
        
        /// <summary>
        /// 로그아웃 (Refresh Token 무효화)
        /// Unity에서 게임 종료 시 호출
        /// </summary>
        [HttpPost("logout")]
        [Authorize]  // 로그인된 사용자만 호출 가능
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenDto dto)
        {
            var userId = GetCurrentUserId();
            await _authService.RevokeTokenAsync(userId, dto.RefreshToken);
            return Ok(new { message = "Logged out successfully" });
        }
        
        /// <summary>
        /// 내 프로필 조회 (인증된 사용자만 가능)
        /// </summary>
        [HttpGet("profile")]
        [Authorize]  // JWT 토큰 필수!
        [ProducesResponseType(typeof(PlayerDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetProfile()
        {
            return Ok(new PlayerDto
            {
                Id = Guid.NewGuid(),
                UserName = "Test",
            });
        }
        
        /// <summary>
        /// 현재 로그인한 사용자 ID 가져오기
        /// Unity의 PlayerPrefs.GetInt("userId")처럼
        /// </summary>
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim);
        }
    }
}