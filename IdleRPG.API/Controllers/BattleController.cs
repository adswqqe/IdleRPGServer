using IdleRPG.Application.Character.Services;
using IdleRPG.Application.DTOs.Battle;
using IdleRPG.Application.Interfaces;
using IdleRPG.Domain.Repositories;
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
    public class BattleController : ControllerBase
    {
        private readonly IBattleService _battleService;
        private readonly ICharacterService _characterService;
        private readonly ICharacterRepository _characterRepository;
        private readonly ILogger<BattleController> _logger;

        public BattleController(
            IBattleService battleService,
            ICharacterService characterService,
            ICharacterRepository characterRepository,
            ILogger<BattleController> logger)
        {
            _battleService = battleService;
            _characterService = characterService;
            _characterRepository = characterRepository;
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

                // 2. 전투 시뮬레이션 실행
                var battleResult = await _battleService.SimulateBattleAsync(request.CharacterId, request.MonsterId);

                // 3. 승리 시 보상 자동 지급
                if (battleResult.IsVictory && battleResult.Reward != null)
                {
                    // 경험치 지급 (자동 레벨업 포함)
                    var updatedCharacter = await _characterService.AddExperienceAsync(
                        request.CharacterId,
                        (int)battleResult.Reward.Experience);

                    // 골드 지급 (Character 엔티티 직접 업데이트)
                    var characterEntity = await _characterRepository.GetByIdAsync(request.CharacterId);
                    if (characterEntity != null)
                    {
                        characterEntity.Gold += battleResult.Reward.Gold;
                        characterEntity.UpdatedAt = DateTime.UtcNow;
                        await _characterRepository.SaveChangesAsync();

                        // 최종 업데이트된 캐릭터 정보 다시 조회
                        updatedCharacter = await _characterService.GetCharacterByIdAsync(request.CharacterId);
                    }

                    // 업데이트된 캐릭터 정보 설정
                    battleResult.UpdatedCharacter = updatedCharacter;
                }

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
        /// 현재 로그인한 사용자 ID 가져오기
        /// </summary>
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim);
        }
    }
}
