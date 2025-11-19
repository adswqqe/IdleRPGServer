using MediatR;

namespace IdleRPG.Application.Commands.Ping;

/// <summary>
/// Ping Command - MediatR 동작 확인용
/// IRequest<PingResponse>를 구현하여 이것이 "요청"임을 표시합니다.
/// </summary>
/// <param name="Message">선택적 메시지 (기본값: "Hello")</param>
public record PingCommand(string Message = "Hello") : IRequest<PingResponse>;

/// <summary>
/// Ping 응답 DTO
/// </summary>
/// <param name="Message">응답 메시지</param>
public record PingResponse(string Message);
