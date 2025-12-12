using IdleRPG.Application.BattleLog.Services;
using IdleRPG.Application.Character.Services;
using IdleRPG.Application.DTOs.Battle;
using IdleRPG.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdleRPG.API.Controllers
{
    /// <summary>
    /// 전투 로그 조회 API 컨트롤러
    ///
    /// [책임]
    /// - 전투 히스토리 조회 (페이징)
    /// - 최근 N개 전투 로그 조회
    /// - 전투 통계 조회
    ///
    /// [Combat System Refactoring]
    /// - 구 BattleController에서 로그 조회 기능만 분리
    /// - Route: /api/battle/logs → /api/battle-logs
    /// - IBattleLogService 사용 (읽기 전용)
    /// </summary>
    [ApiController]
    [Route("api/battle-logs")]
    public class BattleLogController : BaseController
    {
        private readonly IBattleLogService _battleLogService;
        private readonly ICharacterService _characterService;
        private readonly ILogger<BattleLogController> _logger;

        public BattleLogController(
            IBattleLogService battleLogService,
            ICharacterService characterService,
            ILogger<BattleLogController> logger)
        {
            _battleLogService = battleLogService;
            _characterService = characterService;
            _logger = logger;
        }

        /// <summary>
        /// 전투 히스토리 조회 (페이징)
        /// </summary>
        /// <param name="characterId">캐릭터 ID</param>
        /// <param name="page">페이지 번호 (기본값: 1)</param>
        /// <param name="pageSize">페이지 크기 (기본값: 20)</param>
        /// <response code="200">전투 로그 목록 조회 성공</response>
        /// <response code="403">본인의 캐릭터가 아님</response>
        /// <response code="404">캐릭터를 찾을 수 없음</response>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(BattleLogsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBattleLogs(
            [FromQuery] Guid characterId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                // 캐릭터 소유권 검증
                var character = await _characterService.GetCharacterByIdAsync(characterId);
                if (character == null)
                {
                    return NotFound(new { message = "캐릭터를 찾을 수 없습니다" });
                }

                var currentUserId = GetCurrentUserId();
                if (character.PlayerId != currentUserId)
                {
                    return StatusCode(403, new { message = "본인의 캐릭터만 조회할 수 있습니다" });
                }

                // 전투 로그 조회
                var logs = await _battleLogService.GetLogsAsync(characterId, page, pageSize);
                return Ok(new { response = logs });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "전투 로그 조회 중 오류 발생");
                return StatusCode(500, new { message = "전투 로그 조회 중 오류가 발생했습니다" });
            }
        }

        /// <summary>
        /// 최근 N개 전투 로그 조회
        /// </summary>
        /// <param name="characterId">캐릭터 ID</param>
        /// <param name="count">조회할 개수 (기본값: 10)</param>
        /// <response code="200">최근 전투 로그 조회 성공</response>
        /// <response code="403">본인의 캐릭터가 아님</response>
        /// <response code="404">캐릭터를 찾을 수 없음</response>
        [HttpGet("recent")]
        [Authorize]
        [ProducesResponseType(typeof(List<BattleLogDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRecentBattleLogs(
            [FromQuery] Guid characterId,
            [FromQuery] int count = 10)
        {
            try
            {
                // 캐릭터 소유권 검증
                var character = await _characterService.GetCharacterByIdAsync(characterId);
                if (character == null)
                {
                    return NotFound(new { message = "캐릭터를 찾을 수 없습니다" });
                }

                var currentUserId = GetCurrentUserId();
                if (character.PlayerId != currentUserId)
                {
                    return StatusCode(403, new { message = "본인의 캐릭터만 조회할 수 있습니다" });
                }

                // 최근 로그 조회
                var logs = await _battleLogService.GetRecentLogsAsync(characterId, count);
                return Ok(new { response = logs });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "최근 전투 로그 조회 중 오류 발생");
                return StatusCode(500, new { message = "최근 전투 로그 조회 중 오류가 발생했습니다" });
            }
        }

        /// <summary>
        /// 전투 통계 조회
        /// </summary>
        /// <param name="characterId">캐릭터 ID</param>
        /// <response code="200">전투 통계 조회 성공</response>
        /// <response code="403">본인의 캐릭터가 아님</response>
        /// <response code="404">캐릭터를 찾을 수 없음</response>
        [HttpGet("stats")]
        [Authorize]
        [ProducesResponseType(typeof(BattleStatistics), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBattleStats([FromQuery] Guid characterId)
        {
            try
            {
                // 캐릭터 소유권 검증
                var character = await _characterService.GetCharacterByIdAsync(characterId);
                if (character == null)
                {
                    return NotFound(new { message = "캐릭터를 찾을 수 없습니다" });
                }

                var currentUserId = GetCurrentUserId();
                if (character.PlayerId != currentUserId)
                {
                    return StatusCode(403, new { message = "본인의 캐릭터만 조회할 수 있습니다" });
                }

                // 전투 통계 조회
                var stats = await _battleLogService.GetStatsAsync(characterId);
                return Ok(new { response = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "전투 통계 조회 중 오류 발생");
                return StatusCode(500, new { message = "전투 통계 조회 중 오류가 발생했습니다" });
            }
        }
    }
}
