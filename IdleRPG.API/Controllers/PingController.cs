using IdleRPG.Application.Commands.Ping;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IdleRPG.API.Controllers;

/// <summary>
/// MediatR 동작 확인용 Ping Controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PingController : ControllerBase
{
    private readonly IMediator _mediator;

    public PingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// MediatR 동작 테스트 엔드포인트
    /// </summary>
    /// <param name="command">Ping Command (Message는 선택사항)</param>
    /// <returns>Pong 응답</returns>
    [HttpPost]
    public async Task<ActionResult<PingResponse>> Ping([FromBody] PingCommand command)
    {
        // MediatR에게 Command 처리를 위임
        // MediatR이 자동으로 PingCommandHandler를 찾아서 실행합니다
        var response = await _mediator.Send(command);
        return Ok(response);
    }
}
