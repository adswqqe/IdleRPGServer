using IdleRPG.Domain.Repositories;

namespace IdleRPG.Domain.Entities;

/// <summary>
/// 채팅방 참여자 Entity (Whisper 1:1 채팅 참여자 관리)
/// </summary>
public class ChatRoomParticipant : BaseEntity
{
    /// <summary>
    /// 채팅방 ID (FK)
    /// </summary>
    public Guid RoomId { get; set; }

    /// <summary>
    /// 캐릭터 ID (FK)
    /// </summary>
    public Guid CharacterId { get; set; }

    /// <summary>
    /// 채팅방 입장 시간 (UTC)
    /// </summary>
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties

    /// <summary>
    /// 채팅방 정보
    /// </summary>
    public ChatRoom Room { get; set; } = null!;

    /// <summary>
    /// 캐릭터 정보
    /// </summary>
    public Character Character { get; set; } = null!;
}
