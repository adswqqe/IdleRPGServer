using IdleRPG.Application.Character.Services;
using IdleRPG.Application.DTOs.Characters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace IdleRPG.API.Controllers
{
    [ApiController]
    [Route("api/character")]
    public class CharacterController : ControllerBase
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
                var response = await _characterService.CreateCharacterAsync(GetCurrentUserId(), characterDto);
                return Ok(new
                {
                    response
                });
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
        [HttpGet("{characterId}")]
        [Authorize]
        public async Task<IActionResult> GetById([FromRoute]Guid characterId)
        {
            try
            {
                var response = await _characterService.GetCharacterByIdAsync(characterId);
                if (response == null)
                    return BadRequest(new
                    {
                    });

                return Ok(new
                {
                    response
                });

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
                var response = await _characterService.GetPlayerCharactersAsync(GetCurrentUserId());
                return Ok(new
                {
                    response = response
                });
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
                return Ok();
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
                var response = await _characterService.AddExperienceAsync(characterId, dto.Amount);
                return Ok(new
                {
                    response
                });
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
        /// 현재 로그인한 사용자 ID 가져오기
        /// Unity의 PlayerPrefs.GetInt("userId")처럼
        /// </summary>
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim);
        }
    }
}