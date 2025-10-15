using IdleRPG.Application.Character.Services;
using IdleRPG.Application.DTOs;
using IdleRPG.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdleRPG.API.Controllers;

/// <summary>
/// 오프라인 보상 API 컨트롤러
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RewardController : BaseController
{
    private readonly IOfflineRewardService _offlineRewardService;
    private readonly ICharacterService _characterService;
    private readonly ILogger<RewardController> _logger;

    public RewardController(
        IOfflineRewardService offlineRewardService,
        ICharacterService characterService,
        ILogger<RewardController> logger)
    {
        _offlineRewardService = offlineRewardService;
        _characterService = characterService;
        _logger = logger;
    }

    /// <summary>
    /// 오프라인 보상 조회 (실제 지급 없이 정보만 확인)
    /// </summary>
    /// <param name="characterId">캐릭터 ID</param>
    /// <returns>계산된 오프라인 보상 정보</returns>
    [HttpGet("offline/{characterId}")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(object), 403)]
    [ProducesResponseType(typeof(object), 404)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<IActionResult> GetOfflineRewards([FromRoute] Guid characterId)
    {
        try
        {
            // 1. 캐릭터 소유권 검증
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

            // 2. 보상 계산 (조회만, 지급 안함)
            var rewardInfo = await _offlineRewardService.CalculateOfflineRewardsAsync(characterId);

            return Ok(new { response = rewardInfo });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "오프라인 보상 조회 중 오류 발생");
            return StatusCode(500, new { message = "오프라인 보상 조회 중 오류가 발생했습니다" });
        }
    }

    /// <summary>
    /// 오프라인 보상 수령 (실제 지급 및 캐릭터 업데이트)
    /// </summary>
    /// <param name="characterId">캐릭터 ID</param>
    /// <returns>지급 결과 및 업데이트된 캐릭터 정보</returns>
    [HttpPost("offline/{characterId}/claim")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 403)]
    [ProducesResponseType(typeof(object), 404)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<IActionResult> ClaimOfflineRewards([FromRoute] Guid characterId)
    {
        try
        {
            // 1. 캐릭터 소유권 검증
            var character = await _characterService.GetCharacterByIdAsync(characterId);
            if (character == null)
            {
                return NotFound(new { message = "캐릭터를 찾을 수 없습니다" });
            }

            var currentUserId = GetCurrentUserId();
            if (character.PlayerId != currentUserId)
            {
                return StatusCode(403, new { message = "본인의 캐릭터만 사용할 수 있습니다" });
            }

            // 2. 보상 지급 (경험치, 골드, LastLoginTime 업데이트)
            var claimResult = await _offlineRewardService.ClaimOfflineRewardsAsync(characterId);

            return Ok(new { response = claimResult });
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
            _logger.LogError(ex, "오프라인 보상 수령 중 오류 발생");
            return StatusCode(500, new { message = "오프라인 보상 수령 중 오류가 발생했습니다" });
        }
    }
}
