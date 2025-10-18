using IdleRPG.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdleRPG.API.Controllers
{
    /// <summary>
    /// 몬스터 API 컨트롤러
    /// </summary>
    [ApiController]
    [Route("api/monster")]
    public class MonsterController : ControllerBase
    {
        private readonly IMonsterService _monsterService;
        private readonly ILogger<MonsterController> _logger;

        public MonsterController(
            IMonsterService monsterService,
            ILogger<MonsterController> logger)
        {
            _monsterService = monsterService;
            _logger = logger;
        }

        /// <summary>
        /// 전체 몬스터 목록 조회
        /// </summary>
        /// <remarks>
        /// 데이터베이스에 등록된 모든 몬스터의 정보를 반환합니다.
        /// Unity 클라이언트가 게임 시작 시 한 번 호출하여 캐싱합니다.
        ///
        /// **사용 시나리오**:
        /// - 게임 초기 로딩 시 몬스터 데이터 다운로드
        /// - 몬스터 도감 UI 표시
        /// - 클라이언트 캐싱 (24시간 유효)
        ///
        /// **응답 예시**:
        /// ```json
        /// {
        ///   "monsters": [
        ///     {
        ///       "id": "guid",
        ///       "name": "슬라임",
        ///       "level": 1,
        ///       "maxHealth": 100,
        ///       "attack": 10,
        ///       "defense": 5,
        ///       "attackSpeed": 1.0,
        ///       "critRate": 0.05,
        ///       "critDamage": 1.5,
        ///       "evasion": 0.05,
        ///       "experienceReward": 10,
        ///       "goldReward": 5
        ///     }
        ///   ]
        /// }
        /// ```
        /// </remarks>
        [HttpGet("all")]
        [Authorize]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> GetAllMonsters()
        {
            try
            {
                var monsters = await _monsterService.GetAllMonstersAsync();
                return Ok(new { monsters });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "전체 몬스터 목록 조회 중 오류 발생");
                return StatusCode(500, new { message = "몬스터 목록 조회 중 오류가 발생했습니다." });
            }
        }

        /// <summary>
        /// 레벨 범위 내에서 랜덤 몬스터 선택
        /// </summary>
        /// <remarks>
        /// 플레이어의 레벨 범위에 맞는 랜덤 몬스터를 반환합니다.
        /// 범위 내 몬스터가 없으면 가장 높은 레벨의 몬스터를 반환합니다.
        ///
        /// **사용 시나리오**:
        /// - 일반 전투에서 적절한 난이도의 몬스터 선택
        /// - 플레이어 레벨에 따른 자동 매칭
        /// </remarks>
        /// <param name="minLevel">최소 레벨</param>
        /// <param name="maxLevel">최대 레벨</param>
        [HttpGet("random")]
        [Authorize]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> GetRandomMonster(
            [FromQuery] int minLevel,
            [FromQuery] int maxLevel)
        {
            try
            {
                // 입력 검증
                if (minLevel < 1 || maxLevel < 1)
                {
                    return BadRequest(new { message = "레벨은 1 이상이어야 합니다." });
                }

                if (minLevel > maxLevel)
                {
                    return BadRequest(new { message = "최소 레벨은 최대 레벨보다 클 수 없습니다." });
                }

                var result = await _monsterService.GetRandomMonsterAsync(minLevel, maxLevel);
                return Ok(new { response = result });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "몬스터 조회 중 오류 발생");
                return StatusCode(500, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "랜덤 몬스터 조회 중 예상치 못한 오류 발생");
                return StatusCode(500, new { message = "몬스터 조회 중 오류가 발생했습니다." });
            }
        }
    }
}
