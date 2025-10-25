using IdleRPG.Application.DTOs.Equipment;
using IdleRPG.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdleRPG.API.Controllers
{
    [ApiController]
    [Route("api/equipment")]
    [Authorize]
    public class EquipmentController : BaseController
    {
        private readonly IEquipmentService _equipmentService;
        private readonly ILogger<EquipmentController> _logger;

        public EquipmentController(IEquipmentService equipmentService, ILogger<EquipmentController> logger)
        {
            _equipmentService = equipmentService;
            _logger = logger;
        }

        /// <summary>
        /// 장비 생성 (가챠, 드랍 등)
        /// </summary>
        [HttpPost("create")]
        public async Task<IActionResult> CreateEquipment([FromBody] CreateEquipmentDto dto)
        {
            try
            {
                var equipment = await _equipmentService.CreateEquipmentAsync(dto);
                return Ok(new { equipment });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// 캐릭터의 장착 중인 장비 조회 (5개 슬롯)
        /// </summary>
        /// <remarks>
        /// 읽기 전용 API - Public (리더보드, 랭킹 시스템에서 다른 플레이어 장비 조회 가능)
        /// </remarks>
        [HttpGet("equipped/{characterId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetEquippedItems([FromRoute] Guid characterId)
        {
            try
            {
                var equipments = await _equipmentService.GetEquippedItemsAsync(characterId);
                return Ok(new { equipments });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "장착 장비 조회 실패: {CharacterId}", characterId);
                return StatusCode(500, new { message = "서버 오류가 발생했습니다" });
            }
        }

        /// <summary>
        /// 캐릭터의 인벤토리 조회 (미장착 장비)
        /// </summary>
        /// <remarks>
        /// 읽기 전용 API - Public (다른 플레이어 인벤토리 조회 가능)
        /// </remarks>
        [HttpGet("inventory/{ownerId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetInventory([FromRoute] Guid ownerId)
        {
            try
            {
                var equipments = await _equipmentService.GetInventoryAsync(ownerId);
                return Ok(new { equipments });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "인벤토리 조회 실패: {OwnerId}", ownerId);
                return StatusCode(500, new { message = "서버 오류가 발생했습니다" });
            }
        }

        /// <summary>
        /// 장비 장착
        /// </summary>
        [HttpPost("equip")]
        public async Task<IActionResult> EquipItem([FromBody] EquipItemDto dto)
        {
            try
            {
                var equipment = await _equipmentService.EquipItemAsync(dto);
                return Ok(new { equipment, message = "장비가 장착되었습니다" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// 장비 해제
        /// </summary>
        [HttpPost("unequip")]
        public async Task<IActionResult> UnequipItem([FromBody] UnequipItemDto dto)
        {
            try
            {
                var equipment = await _equipmentService.UnequipItemAsync(dto);
                return Ok(new { equipment, message = "장비가 해제되었습니다" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// 장비 강화
        /// </summary>
        [HttpPost("enhance")]
        public async Task<IActionResult> EnhanceEquipment([FromBody] EnhanceEquipmentDto dto)
        {
            try
            {
                var equipment = await _equipmentService.EnhanceEquipmentAsync(dto);
                return Ok(new { equipment, message = $"장비가 +{equipment.EnhancementLevel}로 강화되었습니다" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// 장비 삭제
        /// </summary>
        [HttpDelete("{equipmentId}")]
        public async Task<IActionResult> DeleteEquipment([FromRoute] Guid equipmentId)
        {
            try
            {
                var result = await _equipmentService.DeleteEquipmentAsync(equipmentId);
                if (!result)
                    return NotFound(new { message = "장비를 찾을 수 없습니다" });

                return Ok(new { message = "장비가 삭제되었습니다" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "장비 삭제 실패: {EquipmentId}", equipmentId);
                return StatusCode(500, new { message = "서버 오류가 발생했습니다" });
            }
        }
    }
}
