using IdleRPG.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdleRPG.API.Controllers
{
    /// <summary>
    /// 스킬 가챠 및 스킬 관리 API
    /// </summary>
    [ApiController]
    [Route("api/skills")]
    [Authorize]
    public class SkillController : ControllerBase
    {
        private readonly ISkillService _skillService;
        private readonly ILogger<SkillController> _logger;

        public SkillController(ISkillService skillService, ILogger<SkillController> logger)
        {
            _skillService = skillService;
            _logger = logger;
        }

        /// <summary>
        /// 스킬 가챠를 수행합니다.
        /// </summary>
        /// <param name="characterId">가챠를 수행하는 캐릭터 ID</param>
        /// <returns>획득한 스킬 정보</returns>
        /// <response code="200">가챠 성공 - 획득한 스킬 정보 반환</response>
        /// <response code="400">Crystal 부족 또는 잘못된 요청</response>
        /// <response code="404">캐릭터를 찾을 수 없음</response>
        /// <response code="500">서버 오류</response>
        [HttpPost("gacha")]
        [ProducesResponseType(typeof(Application.DTOs.Gacha.SkillDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PerformGacha([FromBody] GachaRequest request)
        {
            try
            {
                var result = await _skillService.PerformGachaAsync(request.CharacterId);

                _logger.LogInformation("스킬 가챠 성공: CharacterId={CharacterId}, SkillId={SkillId}, Rarity={Rarity}",
                    request.CharacterId, result.Id, result.Rarity);

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("스킬 가챠 실패: {Message}", ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "스킬 가챠 처리 중 오류 발생: CharacterId={CharacterId}", request.CharacterId);
                return StatusCode(500, new { Message = "서버 오류가 발생했습니다." });
            }
        }
    }

    /// <summary>
    /// 스킬 가챠 요청 DTO
    /// </summary>
    public class GachaRequest
    {
        /// <summary>
        /// 가챠를 수행하는 캐릭터 ID
        /// </summary>
        public Guid CharacterId { get; set; }
    }
}
