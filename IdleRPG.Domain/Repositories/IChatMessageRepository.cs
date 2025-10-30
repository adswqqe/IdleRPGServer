using IdleRPG.Domain.Entities;

namespace IdleRPG.Domain.Repositories;

/// <summary>
/// 채팅 메시지 Repository 인터페이스
/// </summary>
public interface IChatMessageRepository
{
    /// <summary>
    /// 메시지 ID로 조회 (Eager Loading: Sender, Room 포함)
    /// </summary>
    /// <param name="id">메시지 ID</param>
    /// <param name="cancellationToken">취소 토큰</param>
    /// <returns>ChatMessage Entity (없으면 null)</returns>
    Task<ChatMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 채팅방의 메시지 목록 조회 (Cursor 기반 페이징, Eager Loading)
    /// </summary>
    /// <param name="roomId">채팅방 ID</param>
    /// <param name="beforeId">이 메시지 이전의 메시지들 조회 (Cursor, nullable)</param>
    /// <param name="take">조회 개수 (10-100)</param>
    /// <param name="cancellationToken">취소 토큰</param>
    /// <returns>ChatMessage Entity 목록 (Sender Eager Loading)</returns>
    Task<List<ChatMessage>> GetByRoomIdAsync(
        Guid roomId,
        Guid? beforeId,
        int take,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 메시지 추가
    /// </summary>
    /// <param name="message">채팅 메시지 Entity</param>
    /// <param name="cancellationToken">취소 토큰</param>
    Task AddAsync(ChatMessage message, CancellationToken cancellationToken = default);
}
