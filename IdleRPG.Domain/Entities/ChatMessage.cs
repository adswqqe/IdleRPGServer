using IdleRPG.Domain.Repositories;

namespace IdleRPG.Domain.Entities;

/// <summary>
/// 채팅 메시지 Entity
/// </summary>
public class ChatMessage : BaseEntity
{
    /// <summary>
    /// 채팅방 ID (FK)
    /// </summary>
    public Guid RoomId { get; set; }

    /// <summary>
    /// 발신자 캐릭터 ID (FK)
    /// </summary>
    public Guid SenderId { get; set; }

    /// <summary>
    /// 메시지 내용 (1-1000자, Application에서 500자 검증)
    /// </summary>
    public string Content { get; set; } = string.Empty;

    // Navigation Properties

    /// <summary>
    /// 채팅방 정보
    /// </summary>
    public ChatRoom Room { get; set; } = null!;

    /// <summary>
    /// 발신자 캐릭터 정보
    /// </summary>
    public Character Sender { get; set; } = null!;
}
