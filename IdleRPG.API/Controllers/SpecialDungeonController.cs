using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdleRPG.API.Controllers
{
    /// <summary>
    /// 특수 던전 시스템 API 컨트롤러 (Placeholder)
    ///
    /// [미래 확장용]
    /// - 보스 던전
    /// - 일일 던전
    /// - 레이드 던전
    /// - 이벤트 던전
    ///
    /// [Combat System Refactoring]
    /// - Phase 3에서 구조만 생성
    /// - Phase 3+에서 실제 구현 예정
    /// - Route: /api/special-dungeons
    ///
    /// [설계 의도]
    /// - 메인 스테이지(StageController)와 특수 던전을 명확히 구분
    /// - 특수 던전은 별도의 보상 체계와 진행 방식을 가짐
    /// - 아키텍처 확장성 확보
    /// </summary>
    [ApiController]
    [Route("api/special-dungeons")]
    public class SpecialDungeonController : BaseController
    {
        private readonly ILogger<SpecialDungeonController> _logger;

        public SpecialDungeonController(ILogger<SpecialDungeonController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 보스 던전 도전 (미구현 - Placeholder)
        /// </summary>
        /// <remarks>
        /// **Phase 3+에서 구현 예정**
        ///
        /// 예상 구현 내용:
        /// - ICombatService로 전투 시뮬레이션
        /// - 특수 보상 지급 (레어 아이템, 스킬 등)
        /// - 일일 도전 횟수 제한
        /// - BattleLogService로 로그 저장
        /// </remarks>
        [HttpPost("boss/challenge")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        public IActionResult ChallengeBoss()
        {
            _logger.LogWarning("보스 던전 도전 API 호출됨 (미구현)");
            return StatusCode(501, new
            {
                message = "보스 던전 기능은 아직 구현되지 않았습니다",
                plannedPhase = "Phase 3+",
                estimatedRelease = "TBD"
            });
        }

        /// <summary>
        /// 일일 던전 도전 (미구현 - Placeholder)
        /// </summary>
        [HttpPost("daily/challenge")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        public IActionResult ChallengeDaily()
        {
            _logger.LogWarning("일일 던전 도전 API 호출됨 (미구현)");
            return StatusCode(501, new
            {
                message = "일일 던전 기능은 아직 구현되지 않았습니다",
                plannedPhase = "Phase 3+",
                estimatedRelease = "TBD"
            });
        }

        /// <summary>
        /// 특수 던전 목록 조회 (미구현 - Placeholder)
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        public IActionResult GetSpecialDungeons()
        {
            _logger.LogWarning("특수 던전 목록 조회 API 호출됨 (미구현)");
            return StatusCode(501, new
            {
                message = "특수 던전 목록 기능은 아직 구현되지 않았습니다",
                plannedPhase = "Phase 3+",
                estimatedRelease = "TBD"
            });
        }
    }
}
