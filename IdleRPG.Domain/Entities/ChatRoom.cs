using IdleRPG.Domain.Enums;
using IdleRPG.Domain.Repositories;

namespace IdleRPG.Domain.Entities;

/// <summary>
/// 채팅방 Entity (전체 채팅, 길드 채팅, 귓속말)
/// </summary>
public class ChatRoom : BaseEntity
{
    /// <summary>
    /// 채팅방 타입 (Global, Guild, Whisper)
    /// </summary>
    public RoomType Type { get; set; }

    /// <summary>
    /// 채팅방 이름 (2-100자)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 길드 ID (Guild 타입일 때만 사용, nullable)
    /// </summary>
    public Guid? GuildId { get; set; }

    // Navigation Properties

    /// <summary>
    /// 길드 정보 (Guild 타입일 때만)
    /// TODO: Guild Entity 구현 후 주석 제거
    /// </summary>
    // public Guild? Guild { get; set; }

    /// <summary>
    /// 채팅방의 메시지 목록
    /// </summary>
    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();

    /// <summary>
    /// 채팅방 참여자 목록 (Whisper 타입에서 사용)
    /// </summary>
    public ICollection<ChatRoomParticipant> Participants { get; set; } = new List<ChatRoomParticipant>();
}
