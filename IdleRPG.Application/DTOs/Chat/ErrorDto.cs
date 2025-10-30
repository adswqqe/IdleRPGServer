namespace IdleRPG.Application.DTOs.Chat;

/// <summary>
/// 채팅 에러 응답 DTO
/// </summary>
public class ErrorDto
{
    /// <summary>
    /// 에러 코드
    /// </summary>
    /// <example>
    /// "INVALID_MESSAGE", "COOLDOWN_ACTIVE", "FORBIDDEN", "INVALID_ROOM_ID", "SERVER_ERROR"
    /// </example>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 에러 메시지 (사용자에게 표시할 텍스트)
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
