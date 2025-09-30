using IdleRPG.Application.DTOs.Player;
using IdleRPG.Application.Players.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IdleRPG.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlayerController : ControllerBase
    {
        private readonly IPlayerService _playerService;
        private readonly ILogger<PlayerController> _logger;

        public PlayerController(IPlayerService playerService, ILogger<PlayerController> logger)
        {
            _playerService = playerService;
            _logger = logger;
        }

        /// <summary>
        /// 내 프로필 정보 업데이트
        /// </summary>
        [HttpPatch("me")]
        [Authorize] // 로그인된 사용자만 호출 가능
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdatePlayerDto dto)
        {
            try
            {
                // JWT에서 사용자 ID 추출
                var currentUserId = GetCurrentUserId();

                // DTO의 ID를 토큰의 ID로 강제 설정 (보안)
                dto.Id = currentUserId;

                await _playerService.UpdatePlayer(dto);
                return Ok(new { message = "Profile updated successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in UpdateMyProfile");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// 내 프로필 정보 조회
        /// </summary>
        [HttpGet("me")]
        [Authorize] // 로그인된 사용자만 호출 가능
        public async Task<IActionResult> GetMyProfile()
        {
            try
            {
                // JWT에서 사용자 ID 추출
                var currentUserId = GetCurrentUserId();

                var response = await _playerService.GetPlayer(currentUserId);
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GetMyProfile");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// JWT 토큰에서 현재 로그인한 사용자 ID 가져오기
        /// </summary>
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                _logger.LogWarning("User ID not found in JWT token");
                throw new UnauthorizedAccessException("User ID not found in token");
            }

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                _logger.LogWarning($"Invalid user ID format in token: {userIdClaim}");
                throw new UnauthorizedAccessException("Invalid user ID format");
            }

            return userId;
        }
    }
}