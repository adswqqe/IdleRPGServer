namespace IdleRPG.Application.DTOs.Chat;

/// <summary>
/// 채팅 메시지 DTO (Select Projection용)
/// </summary>
public class ChatMessageDto
{
    /// <summary>
    /// 메시지 ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 채팅방 ID
    /// </summary>
    public Guid RoomId { get; set; }

    /// <summary>
    /// 발신자 정보 (중첩 DTO)
    /// </summary>
    public CharacterSummaryDto Sender { get; set; } = null!;

    /// <summary>
    /// 메시지 내용 (1-500자)
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 메시지 전송 시간 (UTC)
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 메시지 반응 목록 (미래 확장용)
    /// </summary>
    public List<ReactionDto>? Reactions { get; set; }

    /// <summary>
    /// 메시지 편집 여부 (미래 확장용)
    /// </summary>
    public bool? IsEdited { get; set; }

    /// <summary>
    /// 답장 대상 메시지 ID (미래 확장용)
    /// </summary>
    public Guid? ParentMessageId { get; set; }
}

/// <summary>
/// 캐릭터 요약 정보 (중첩 DTO)
/// </summary>
public class CharacterSummaryDto
{
    /// <summary>
    /// 캐릭터 ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 캐릭터 이름
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 캐릭터 레벨
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// 캐릭터 아바타 URL (미래 확장용)
    /// </summary>
    public string? AvatarUrl { get; set; }
}

/// <summary>
/// 메시지 반응 DTO (미래 확장용)
/// </summary>
public class ReactionDto
{
    /// <summary>
    /// 이모지 코드 (예: ":thumbsup:", ":heart:")
    /// </summary>
    public string Emoji { get; set; } = string.Empty;

    /// <summary>
    /// 반응한 캐릭터 ID 목록
    /// </summary>
    public List<Guid> CharacterIds { get; set; } = new();

    /// <summary>
    /// 반응 횟수
    /// </summary>
    public int Count { get; set; }
}
