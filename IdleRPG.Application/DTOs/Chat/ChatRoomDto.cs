namespace IdleRPG.Application.DTOs.Chat;

/// <summary>
/// 채팅방 정보 DTO
/// </summary>
public class ChatRoomDto
{
    /// <summary>
    /// 채팅방 ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 채팅방 타입 (문자열 형식)
    /// </summary>
    /// <example>
    /// "Global", "Guild", "Whisper"
    /// </example>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// 채팅방 이름
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 마지막 메시지 정보 (미래 확장용)
    /// </summary>
    public LastMessageDto? LastMessage { get; set; }
}

/// <summary>
/// 마지막 메시지 요약 DTO (미래 확장용)
/// </summary>
public class LastMessageDto
{
    /// <summary>
    /// 메시지 ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 발신자 이름
    /// </summary>
    public string SenderName { get; set; } = string.Empty;

    /// <summary>
    /// 메시지 내용 (50자로 자름)
    /// </summary>
    public string ContentPreview { get; set; } = string.Empty;

    /// <summary>
    /// 메시지 전송 시간 (UTC)
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 안 읽은 메시지 개수 (미래 확장용)
    /// </summary>
    public int? UnreadCount { get; set; }
}
