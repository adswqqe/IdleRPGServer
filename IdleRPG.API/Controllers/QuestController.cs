using IdleRPG.Application.Quests.Commands.AcceptQuest;
using IdleRPG.Application.Quests.Queries.GetActiveQuests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdleRPG.API.Controllers;

/// <summary>
/// 퀘스트 관련 API Controller
/// </summary>
/// <remarks>
/// Controller의 책임:
/// 1. HTTP 요청을 받아서 Command/Query로 변환
/// 2. MediatR에게 위임
/// 3. 결과를 HTTP 응답으로 변환
///
/// 핵심 원칙: "Controller는 얇게" - 비즈니스 로직은 Handler에서!
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Authorize] // JWT 인증 필요
public class QuestController : BaseController
{
    private readonly IMediator _mediator;

    public QuestController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// 퀘스트 수락
    /// </summary>
    /// <param name="command">수락할 퀘스트 정보</param>
    /// <returns>수락 결과</returns>
    /// <remarks>
    /// 테스트 방법:
    /// 1. /api/auth/login 으로 JWT 토큰 획득
    /// 2. Swagger "Authorize" 버튼 → "Bearer {토큰}" 입력
    /// 3. 이 엔드포인트 호출
    ///
    /// Request Body 예시:
    /// {
    ///   "characterId": "your-character-id",
    ///   "questTemplateId": 1
    /// }
    /// </remarks>
    [HttpPost("accept")]
    [ProducesResponseType(typeof(AcceptQuestResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AcceptQuestResult>> AcceptQuest(
        [FromBody] AcceptQuestCommand command)
    {
        // MediatR에게 Command 처리를 위임
        // MediatR이 자동으로 AcceptQuestCommandHandler를 찾아서 실행합니다
        var result = await _mediator.Send(command);

        return Ok(result);
    }

    /// <summary>
    /// 진행 중인 퀘스트 목록 조회
    /// </summary>
    /// <param name="characterId">조회할 캐릭터 ID</param>
    /// <returns>진행 중인 퀘스트 목록</returns>
    /// <remarks>
    /// CQRS 패턴의 Query 측면:
    /// - 데이터를 조회만 하고 상태를 변경하지 않음
    /// - Command(AcceptQuest)와 책임이 명확히 분리됨
    ///
    /// 테스트 방법:
    /// 1. Step 3에서 퀘스트를 수락한 후
    /// 2. 해당 캐릭터 ID로 이 엔드포인트 호출
    /// 3. 진행 중인 퀘스트 목록 확인
    /// </remarks>
    [HttpGet("active/{characterId:guid}")]
    [ProducesResponseType(typeof(List<QuestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<QuestDto>>> GetActiveQuests(Guid characterId)
    {
        // Query 객체 생성 후 MediatR에게 위임
        var query = new GetActiveQuestsQuery(characterId);
        var result = await _mediator.Send(query);

        return Ok(result);
    }
}
