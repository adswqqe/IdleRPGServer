using IdleRPG.Application.DTOs.Pet;
using IdleRPG.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdleRPG.API.Controllers
{
    /// <summary>
    /// 펫 시스템 API (가챠, 레벨업, 장착, 조회)
    /// </summary>
    [ApiController]
    [Route("api/pets")]
    public class PetsController : ControllerBase
    {
        private readonly IPetService _petService;
        private readonly ILogger<PetsController> _logger;

        public PetsController(IPetService petService, ILogger<PetsController> logger)
        {
            _petService = petService;
            _logger = logger;
        }

        /// <summary>
        /// 펫 가챠를 수행합니다 (1회 또는 10연차)
        /// </summary>
        /// <param name="request">가챠 요청 (캐릭터 ID, 횟수)</param>
        /// <returns>획득한 펫 목록, 중복 보상, 천장 카운터</returns>
        /// <response code="201">가챠 성공 - 획득한 펫 정보 반환</response>
        /// <response code="400">Crystal 부족 또는 잘못된 요청 (count는 1 또는 10만 허용)</response>
        /// <response code="401">인증 실패 (JWT 토큰 없음)</response>
        /// <response code="404">캐릭터를 찾을 수 없음</response>
        /// <response code="500">서버 오류</response>
        [HttpPost("gacha")]
        [Authorize]
        [ProducesResponseType(typeof(PetGachaResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PerformPetGacha([FromBody] PetGachaRequestDto request)
        {
            try
            {
                // Validation: count는 1 또는 10만 허용
                if (request.Count != 1 && request.Count != 10)
                {
                    return BadRequest(new { message = "가챠 횟수는 1 또는 10만 허용됩니다." });
                }

                var result = await _petService.DrawPetsAsync(request.CharacterId, request.Count, HttpContext.RequestAborted);

                _logger.LogInformation(
                    "펫 가챠 성공: CharacterId={CharacterId}, Count={Count}, NewPets={NewPets}, Duplicates={Duplicates}, PityCount={PityCount}",
                    request.CharacterId, request.Count, result.Pets.Count, result.DuplicateRewards.Count, result.CurrentPityCount);

                return Created($"/api/pets?characterId={request.CharacterId}", result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("펫 가챠 실패: CharacterId={CharacterId}, Message={Message}",
                    request.CharacterId, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("캐릭터를 찾을 수 없음: CharacterId={CharacterId}", request.CharacterId);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "펫 가챠 처리 중 오류 발생: CharacterId={CharacterId}", request.CharacterId);
                return StatusCode(500, new { message = "서버 오류가 발생했습니다." });
            }
        }

        /// <summary>
        /// 캐릭터가 보유한 펫 목록을 조회합니다 (읽기 전용 - Public)
        /// </summary>
        /// <param name="characterId">캐릭터 ID</param>
        /// <param name="rarity">등급 필터 (선택, 예: Common, Rare, Epic, Legendary)</param>
        /// <param name="sortBy">정렬 기준 (선택, 예: level, attack, mana, createdAt)</param>
        /// <returns>펫 목록</returns>
        /// <response code="200">조회 성공</response>
        /// <response code="500">서버 오류</response>
        /// <remarks>
        /// 읽기 전용 API - Public (다른 플레이어의 펫 도감 조회 가능)
        /// </remarks>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<PetDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPets(
            [FromQuery] Guid characterId,
            [FromQuery] string? rarity = null,
            [FromQuery] string? sortBy = null)
        {
            try
            {
                var pets = await _petService.GetPetsByCharacterIdAsync(characterId, HttpContext.RequestAborted);

                // TODO: rarity, sortBy 필터링 로직 (향후 확장)
                // 현재는 전체 목록 반환

                return Ok(new { characterId, count = pets.Count, pets });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "펫 목록 조회 실패: CharacterId={CharacterId}", characterId);
                return StatusCode(500, new { message = "서버 오류가 발생했습니다." });
            }
        }

        /// <summary>
        /// 펫 레벨업을 수행합니다 (골드 소모, 스탯 증가)
        /// </summary>
        /// <param name="petId">레벨업할 펫 ID</param>
        /// <param name="request">레벨업 요청 (캐릭터 ID)</param>
        /// <returns>레벨업 결과 (새 레벨, 스탯, 비용, 잔여 골드)</returns>
        /// <response code="200">레벨업 성공</response>
        /// <response code="400">최대 레벨 도달 또는 골드 부족</response>
        /// <response code="401">인증 실패</response>
        /// <response code="403">소유하지 않은 펫</response>
        /// <response code="404">펫을 찾을 수 없음</response>
        /// <response code="500">서버 오류</response>
        [HttpPost("{petId}/level-up")]
        [Authorize]
        [ProducesResponseType(typeof(PetLevelUpResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> LevelUpPet(
            [FromRoute] int petId,
            [FromBody] PetLevelUpRequestDto request)
        {
            try
            {
                var result = await _petService.LevelUpPetAsync(petId, request.CharacterId, HttpContext.RequestAborted);

                _logger.LogInformation(
                    "펫 레벨업 성공: PetId={PetId}, CharacterId={CharacterId}, NewLevel={NewLevel}, CostGold={CostGold}",
                    petId, request.CharacterId, result.NewLevel, result.CostGold);

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("펫 레벨업 실패: PetId={PetId}, Message={Message}", petId, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("권한 없음: PetId={PetId}, CharacterId={CharacterId}", petId, request.CharacterId);
                return Forbid();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("펫을 찾을 수 없음: PetId={PetId}", petId);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "펫 레벨업 처리 중 오류 발생: PetId={PetId}", petId);
                return StatusCode(500, new { message = "서버 오류가 발생했습니다." });
            }
        }

        /// <summary>
        /// 펫을 슬롯에 장착합니다 (최대 3개 슬롯)
        /// </summary>
        /// <param name="request">장착 요청 (캐릭터 ID, 펫 ID, 슬롯 인덱스)</param>
        /// <returns>장착 결과 (장착된 펫 목록, 총 버프)</returns>
        /// <response code="200">장착 성공</response>
        /// <response code="400">잘못된 슬롯 인덱스 (1-3)</response>
        /// <response code="401">인증 실패</response>
        /// <response code="403">소유하지 않은 펫</response>
        /// <response code="404">펫을 찾을 수 없음</response>
        /// <response code="500">서버 오류</response>
        [HttpPost("equip")]
        [Authorize]
        [ProducesResponseType(typeof(PetEquipResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EquipPet([FromBody] PetEquipRequestDto request)
        {
            try
            {
                // Validation: slotIndex는 1-3만 허용
                if (request.SlotIndex < 1 || request.SlotIndex > 3)
                {
                    return BadRequest(new { message = "슬롯 인덱스는 1-3만 허용됩니다." });
                }

                var result = await _petService.EquipPetAsync(
                    request.CharacterId, request.PetId, request.SlotIndex, HttpContext.RequestAborted);

                _logger.LogInformation(
                    "펫 장착 성공: CharacterId={CharacterId}, PetId={PetId}, SlotIndex={SlotIndex}",
                    request.CharacterId, request.PetId, request.SlotIndex);

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("펫 장착 실패: Message={Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("권한 없음: PetId={PetId}, CharacterId={CharacterId}",
                    request.PetId, request.CharacterId);
                return Forbid();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("펫을 찾을 수 없음: PetId={PetId}", request.PetId);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "펫 장착 처리 중 오류 발생: PetId={PetId}", request.PetId);
                return StatusCode(500, new { message = "서버 오류가 발생했습니다." });
            }
        }

        /// <summary>
        /// 펫 장착을 해제합니다
        /// </summary>
        /// <param name="request">장착 해제 요청 (캐릭터 ID, 슬롯 인덱스)</param>
        /// <returns>204 No Content</returns>
        /// <response code="204">장착 해제 성공</response>
        /// <response code="401">인증 실패</response>
        /// <response code="404">해당 슬롯에 장착된 펫 없음</response>
        /// <response code="500">서버 오류</response>
        [HttpPost("unequip")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UnequipPet([FromBody] UnequipPetRequestDto request)
        {
            try
            {
                await _petService.UnequipPetAsync(request.CharacterId, request.SlotIndex, HttpContext.RequestAborted);

                _logger.LogInformation(
                    "펫 장착 해제 성공: CharacterId={CharacterId}, SlotIndex={SlotIndex}",
                    request.CharacterId, request.SlotIndex);

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("장착된 펫을 찾을 수 없음: CharacterId={CharacterId}, SlotIndex={SlotIndex}",
                    request.CharacterId, request.SlotIndex);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "펫 장착 해제 처리 중 오류 발생: CharacterId={CharacterId}, SlotIndex={SlotIndex}",
                    request.CharacterId, request.SlotIndex);
                return StatusCode(500, new { message = "서버 오류가 발생했습니다." });
            }
        }

        /// <summary>
        /// 캐릭터가 장착한 펫 목록을 조회합니다 (읽기 전용 - Public)
        /// </summary>
        /// <param name="characterId">캐릭터 ID</param>
        /// <returns>장착된 펫 목록 (슬롯별)</returns>
        /// <response code="200">조회 성공</response>
        /// <response code="500">서버 오류</response>
        /// <remarks>
        /// 읽기 전용 API - Public (다른 플레이어의 장착 펫 조회 가능)
        /// </remarks>
        [HttpGet("equipped")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<EquippedPetDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetEquippedPets([FromQuery] Guid characterId)
        {
            try
            {
                var equippedPets = await _petService.GetEquippedPetsAsync(characterId, HttpContext.RequestAborted);

                return Ok(new { characterId, equippedPets });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "장착된 펫 목록 조회 실패: CharacterId={CharacterId}", characterId);
                return StatusCode(500, new { message = "서버 오류가 발생했습니다." });
            }
        }

        /// <summary>
        /// 펫 상세 정보를 조회합니다 (읽기 전용 - Public)
        /// </summary>
        /// <param name="petId">펫 ID</param>
        /// <returns>펫 상세 정보</returns>
        /// <response code="200">조회 성공</response>
        /// <response code="404">펫을 찾을 수 없음</response>
        /// <response code="500">서버 오류</response>
        [HttpGet("{petId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PetDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPetById([FromRoute] int petId)
        {
            try
            {
                var pet = await _petService.GetPetByIdAsync(petId, HttpContext.RequestAborted);

                return Ok(pet);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("펫을 찾을 수 없음: PetId={PetId}", petId);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "펫 상세 조회 실패: PetId={PetId}", petId);
                return StatusCode(500, new { message = "서버 오류가 발생했습니다." });
            }
        }

        /// <summary>
        /// 펫을 삭제합니다 (장착된 펫은 삭제 불가)
        /// </summary>
        /// <param name="petId">펫 ID</param>
        /// <param name="characterId">캐릭터 ID (쿼리 파라미터)</param>
        /// <returns>204 No Content</returns>
        /// <response code="204">삭제 성공</response>
        /// <response code="400">장착된 펫은 삭제 불가</response>
        /// <response code="401">인증 실패</response>
        /// <response code="403">소유하지 않은 펫</response>
        /// <response code="404">펫을 찾을 수 없음</response>
        /// <response code="500">서버 오류</response>
        [HttpDelete("{petId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeletePet(
            [FromRoute] int petId,
            [FromQuery] Guid characterId)
        {
            try
            {
                await _petService.DeletePetAsync(petId, characterId, HttpContext.RequestAborted);

                _logger.LogInformation("펫 삭제 성공: PetId={PetId}, CharacterId={CharacterId}",
                    petId, characterId);

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("펫 삭제 실패: PetId={PetId}, Message={Message}", petId, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("권한 없음: PetId={PetId}, CharacterId={CharacterId}", petId, characterId);
                return Forbid();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("펫을 찾을 수 없음: PetId={PetId}", petId);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "펫 삭제 처리 중 오류 발생: PetId={PetId}", petId);
                return StatusCode(500, new { message = "서버 오류가 발생했습니다." });
            }
        }
    }

    /// <summary>
    /// 펫 장착 해제 요청 DTO (Controller 내부 클래스)
    /// </summary>
    public class UnequipPetRequestDto
    {
        /// <summary>
        /// 캐릭터 ID
        /// </summary>
        public Guid CharacterId { get; set; }

        /// <summary>
        /// 슬롯 인덱스 (1-3)
        /// </summary>
        public int SlotIndex { get; set; }
    }
}
