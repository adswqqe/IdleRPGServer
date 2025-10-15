using IdleRPG.Application.Character.Services;
using IdleRPG.Application.DTOs.Battle;
using IdleRPG.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IdleRPG.API.Controllers
{
    /// <summary>
    /// 전투 시스템 API 컨트롤러
    /// </summary>
    [ApiController]
    [Route("api/battle")]
    public class BattleController : BaseController
    {
        private readonly IBattleService _battleService;
        private readonly ICharacterService _characterService;
        private readonly ILogger<BattleController> _logger;

        public BattleController(
            IBattleService battleService,
            ICharacterService characterService,
            ILogger<BattleController> logger)
        {
            _battleService = battleService;
            _characterService = characterService;
            _logger = logger;
        }

        /// <summary>
        /// 전투 시작 (서버 시뮬레이션 방식 - 보스, 랭킹 던전, PVP용)
        /// </summary>
        /// <remarks>
        /// 서버에서 전투를 시뮬레이션하고 결과를 반환합니다.
        /// 승리 시 경험치와 골드를 자동으로 지급하며, 레벨업도 자동 처리됩니다.
        ///
        /// **사용 시나리오**:
        /// - 보스 스테이지
        /// - 랭킹이 있는 던전
        /// - PVP 전투
        /// - 오프라인 보상 계산
        /// </remarks>
        [HttpPost("start")]
        [Authorize]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 403)]
        [ProducesResponseType(typeof(object), 404)]
        public async Task<IActionResult> StartBattle([FromBody] StartBattleRequest request)
        {
            try
            {
                // 1. 캐릭터 소유권 검증
                var character = await _characterService.GetCharacterByIdAsync(request.CharacterId);
                if (character == null)
                {
                    return NotFound(new { message = "캐릭터를 찾을 수 없습니다" });
                }

                var currentUserId = GetCurrentUserId();
                if (character.PlayerId != currentUserId)
                {
                    return StatusCode(403, new { message = "본인의 캐릭터만 사용할 수 있습니다" });
                }

                // 2. 전투 시뮬레이션 실행 (보상 지급 포함)
                var battleResult = await _battleService.SimulateBattleAsync(request.CharacterId, request.MonsterId);

                return Ok(new { response = battleResult });
            }
            catch (InvalidOperationException ex)
            {
                // 몬스터를 찾을 수 없거나 기타 비즈니스 로직 오류
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "전투 처리 중 오류 발생");
                return StatusCode(500, new { message = "전투 처리 중 오류가 발생했습니다" });
            }
        }

        /// <summary>
        /// 전투 히스토리 조회 (페이징)
        /// </summary>
        /// <param name="characterId">캐릭터 ID</param>
        /// <param name="page">페이지 번호 (기본값: 1)</param>
        /// <param name="pageSize">페이지 크기 (기본값: 20)</param>
        [HttpGet("logs")]
        [Authorize]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 403)]
        [ProducesResponseType(typeof(object), 404)]
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
                var logs = await _battleService.GetBattleLogsAsync(characterId, page, pageSize);
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
        [HttpGet("logs/recent")]
        [Authorize]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 403)]
        [ProducesResponseType(typeof(object), 404)]
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
                var logs = await _battleService.GetRecentBattleLogsAsync(characterId, count);
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
        [HttpGet("stats")]
        [Authorize]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 403)]
        [ProducesResponseType(typeof(object), 404)]
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
                var stats = await _battleService.GetBattleStatsAsync(characterId);
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
