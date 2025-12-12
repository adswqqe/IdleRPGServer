using IdleRPG.Application.Character.Services;
using IdleRPG.Application.DTOs.Combat;
using IdleRPG.Application.Interfaces;
using IdleRPG.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdleRPG.API.Controllers
{
    /// <summary>
    /// 메인 스테이지 시스템 API 컨트롤러
    ///
    /// [보안 설계]
    /// - JWT 인증 필수 ([Authorize])
    /// - 캐릭터 소유권 검증 (PlayerId 확인)
    /// - 서버 중심 비즈니스 로직 (클라이언트는 UI만)
    ///
    /// [변경사항 - Combat System Refactoring]
    /// - 구 DungeonController → StageController로 리네이밍
    /// - Route: /api/dungeons → /api/stages
    /// - 메인 스테이지 진행도 관리에 집중 (특수 던전은 SpecialDungeonController로 분리 예정)
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/stages")]
    public class StageController : BaseController
    {
        private readonly IStageService _stageService;
        private readonly ICharacterService _characterService;
        private readonly ILogger<StageController> _logger;

        public StageController(
            IStageService stageService,
            ICharacterService characterService,
            ILogger<StageController> logger)
        {
            _stageService = stageService;
            _characterService = characterService;
            _logger = logger;
        }

        /// <summary>
        /// 특정 캐릭터가 도전 가능한 메인 스테이지 목록을 조회합니다
        /// </summary>
        /// <param name="characterId">조회할 캐릭터 ID</param>
        /// <param name="difficulty">필터링할 난이도 (선택, null이면 모든 난이도)</param>
        /// <returns>도전 가능한 스테이지 목록 (난이도 배수 적용된 보상 포함)</returns>
        /// <response code="200">스테이지 목록 조회 성공</response>
        /// <response code="404">존재하지 않는 캐릭터</response>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<DungeonStageDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAvailableStages(
            [FromQuery] Guid characterId,
            [FromQuery] DungeonDifficulty? difficulty = null)
        {
            try
            {
                // 읽기 전용 API - Public (리더보드, 랭킹 대비)
                var character = await _characterService.GetCharacterByIdAsync(characterId);
                if (character == null)
                    return NotFound("캐릭터를 찾을 수 없습니다");

                var stages = await _stageService.GetAvailableStagesAsync(characterId, difficulty);
                return Ok(stages);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "스테이지 조회 실패 - 캐릭터: {CharacterId}", characterId);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "스테이지 조회 중 오류 - 캐릭터: {CharacterId}", characterId);
                return StatusCode(500, "서버 오류가 발생했습니다");
            }
        }

        /// <summary>
        /// 특정 캐릭터의 메인 스테이지 진행도를 조회합니다
        /// </summary>
        /// <param name="characterId">조회할 캐릭터 ID</param>
        /// <returns>난이도별 최고 클리어 스테이지</returns>
        /// <response code="200">진행도 조회 성공</response>
        /// <response code="404">존재하지 않는 캐릭터</response>
        [HttpGet("progress")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(CharacterMainBattleProgressDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProgress([FromQuery] Guid characterId)
        {
            try
            {
                // 읽기 전용 API - Public (다른 플레이어 진행도 조회 가능)
                var character = await _characterService.GetCharacterByIdAsync(characterId);
                if (character == null)
                    return NotFound("캐릭터를 찾을 수 없습니다");

                var progress = await _stageService.GetProgressAsync(characterId);
                return Ok(progress);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "진행도 조회 실패 - 캐릭터: {CharacterId}", characterId);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "진행도 조회 중 오류 - 캐릭터: {CharacterId}", characterId);
                return StatusCode(500, "서버 오류가 발생했습니다");
            }
        }

        /// <summary>
        /// 메인 스테이지 클리어를 요청합니다
        ///
        /// [서버 검증]
        /// - 캐릭터 소유권 (PlayerId 확인)
        /// - 캐릭터 레벨 (RequiredLevel 이상)
        /// - 진행도 (이전 스테이지 클리어 여부)
        /// - 동시성 제어 (중복 보상 방지)
        ///
        /// [트랜잭션]
        /// - 진행도 업데이트 + 보상 지급 + 레벨업을 원자적으로 처리
        /// </summary>
        /// <param name="request">클리어 요청 (CharacterId, StageId, Difficulty)</param>
        /// <returns>클리어 결과 (성공 여부, 보상, 레벨업 정보)</returns>
        /// <response code="200">클리어 처리 완료 (성공/실패 모두 200, IsSuccess로 구분)</response>
        /// <response code="401">인증되지 않은 사용자</response>
        /// <response code="403">다른 플레이어의 캐릭터로 클리어 시도</response>
        /// <response code="404">존재하지 않는 캐릭터</response>
        [HttpPost("clear")]
        [ProducesResponseType(typeof(DungeonClearResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ClearStage([FromBody] DungeonClearRequestDto request)
        {
            try
            {
                // 1. PlayerId 추출 (JWT)
                var playerId = GetCurrentUserId();

                // 2. 소유권 검증
                var character = await _characterService.GetCharacterByIdAsync(request.CharacterId);
                if (character == null)
                    return NotFound("캐릭터를 찾을 수 없습니다");

                if (character.PlayerId != playerId)
                {
                    _logger.LogWarning(
                        "소유권 검증 실패 - PlayerId: {PlayerId}, CharacterId: {CharacterId}, OwnerPlayerId: {OwnerPlayerId}",
                        playerId, request.CharacterId, character.PlayerId);
                    return Forbid(); // 다른 플레이어의 캐릭터
                }

                // 3. 스테이지 클리어 처리 (비즈니스 로직)
                var result = await _stageService.ClearStageAsync(request.CharacterId, request);

                // 4. DTO 기반 에러 처리 (IsSuccess=false도 200 OK)
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "스테이지 클리어 처리 중 오류 - CharacterId: {CharacterId}, StageId: {StageId}",
                    request.CharacterId, request.StageId);
                return StatusCode(500, "서버 오류가 발생했습니다");
            }
        }
    }
}
