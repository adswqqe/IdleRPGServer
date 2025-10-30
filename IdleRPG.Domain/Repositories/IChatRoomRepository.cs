using IdleRPG.Domain.Entities;

namespace IdleRPG.Domain.Repositories;

/// <summary>
/// 채팅방 Repository 인터페이스
/// </summary>
public interface IChatRoomRepository
{
    /// <summary>
    /// ID로 채팅방 조회
    /// </summary>
    /// <param name="id">채팅방 ID</param>
    /// <param name="cancellationToken">취소 토큰</param>
    /// <returns>채팅방 Entity (없으면 null)</returns>
    Task<ChatRoom?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 캐릭터가 접근 가능한 채팅방 목록 조회
    /// (Global: 모든 방, Guild: 같은 길드, Whisper: 참여 중인 방)
    /// </summary>
    /// <param name="characterId">캐릭터 ID</param>
    /// <param name="guildId">길드 ID (nullable)</param>
    /// <param name="cancellationToken">취소 토큰</param>
    /// <returns>접근 가능한 채팅방 목록</returns>
    Task<List<ChatRoom>> GetAccessibleRoomsAsync(
        Guid characterId,
        Guid? guildId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 채팅방 추가
    /// </summary>
    /// <param name="room">채팅방 Entity</param>
    /// <param name="cancellationToken">취소 토큰</param>
    Task AddAsync(ChatRoom room, CancellationToken cancellationToken = default);
}
