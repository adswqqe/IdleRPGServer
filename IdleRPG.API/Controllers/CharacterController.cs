using IdleRPG.Application.Character.Services;
using IdleRPG.Application.DTOs.Characters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace IdleRPG.API.Controllers
{
    [ApiController]
    [Route("api/character")]
    public class CharacterController : BaseController
    {
        private readonly ICharacterService _characterService;
        private readonly ILogger<CharacterController> _logger;

        public CharacterController(ICharacterService characterService, ILogger<CharacterController> logger)
        {
            _characterService = characterService;
            _logger = logger;
        }

        /// <summary>
        /// 캐릭터 생성
        /// </summary>
        [HttpPost("Create")]
        [Authorize]
        public async Task<IActionResult> Create(CreateCharacterDto characterDto)
        {
            try
            {
                var character = await _characterService.CreateCharacterAsync(GetCurrentUserId(), characterDto);
                return Ok(character);  // 직접 반환
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
        /// 캐릭터 조회
        /// </summary>
        /// <remarks>
        /// 읽기 전용 API - Public (리더보드, 랭킹에서 다른 플레이어 캐릭터 조회 가능)
        /// </remarks>
        [HttpGet("{characterId}")]
        public async Task<IActionResult> GetById([FromRoute]Guid characterId)
        {
            try
            {
                var character = await _characterService.GetCharacterByIdAsync(characterId);
                if (character == null)
                    return NotFound();

                return Ok(character);
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
        /// 캐릭터 전체 조회
        /// </summary>
        [HttpGet("GetCharacters")]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var characters = await _characterService.GetPlayerCharactersAsync(GetCurrentUserId());
                return Ok(characters);
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
        /// 캐릭터 삭제
        /// </summary>
        [HttpDelete("{characterId}")]
        [Authorize]
        public async Task<IActionResult> Delete([FromRoute]Guid characterId)
        {
            try
            {
                await _characterService.DeleteCharacterAsync(characterId);
                return Ok(new { message = "캐릭터가 삭제되었습니다" });
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
        /// 경험치 획득 (자동 레벨업 및 스탯 성장)
        /// </summary>
        [HttpPost("{characterId}/experience")]
        [Authorize]
        public async Task<IActionResult> AddExperience([FromRoute] Guid characterId, [FromBody] AddExperienceDto dto)
        {
            try
            {
                var character = await _characterService.AddExperienceAsync(characterId, dto.Amount);
                return Ok(character);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

    }
}