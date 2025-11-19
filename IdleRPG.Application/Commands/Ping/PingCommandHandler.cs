using MediatR;

namespace IdleRPG.Application.Commands.Ping;

/// <summary>
/// PingCommand를 처리하는 Handler
/// IRequestHandler<TRequest, TResponse>를 구현합니다.
/// MediatR이 자동으로 이 클래스를 찾아서 DI 컨테이너에 등록합니다.
/// </summary>
public class PingCommandHandler : IRequestHandler<PingCommand, PingResponse>
{
    /// <summary>
    /// Command를 처리하는 메서드
    /// Controller에서 mediator.Send()를 호출하면 자동으로 이 메서드가 실행됩니다.
    /// </summary>
    /// <param name="request">PingCommand 요청</param>
    /// <param name="cancellationToken">취소 토큰</param>
    /// <returns>PingResponse 응답</returns>
    public Task<PingResponse> Handle(PingCommand request, CancellationToken cancellationToken)
    {
        // 간단한 응답 생성
        var message = $"Pong! You said: '{request.Message}'. MediatR is working! 🎉";
        var response = new PingResponse(message);

        return Task.FromResult(response);
    }
}
