using IdleRPG.Application.DTOs.Chat;

namespace IdleRPG.Application.Services;

/// <summary>
/// 채팅 시스템 비즈니스 로직을 처리하는 서비스 인터페이스
/// </summary>
public interface IChatService
{
    /// <summary>
    /// 채팅 메시지를 전송합니다.
    /// </summary>
    /// <param name="roomId">채팅방 ID</param>
    /// <param name="senderId">발신자 캐릭터 ID</param>
    /// <param name="content">메시지 내용 (1-500자)</param>
    /// <param name="cancellationToken">작업 취소 토큰</param>
    /// <returns>전송된 메시지 DTO</returns>
    /// <exception cref="ValidationException">메시지 길이 초과 (501자 이상)</exception>
    /// <exception cref="InvalidOperationException">쿨다운 위반 (1초 이내 재전송)</exception>
    /// <exception cref="NotFoundException">채팅방 또는 캐릭터 미존재</exception>
    /// <exception cref="ForbiddenException">채팅방 접근 권한 없음</exception>
    Task<ChatMessageDto> SendMessageAsync(
        Guid roomId,
        Guid senderId,
        string content,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 채팅방의 메시지 히스토리를 조회합니다 (Cursor 페이징).
    /// </summary>
    /// <param name="roomId">채팅방 ID</param>
    /// <param name="characterId">요청자 캐릭터 ID (권한 체크용)</param>
    /// <param name="beforeId">이전 메시지 ID (null이면 최신 메시지부터)</param>
    /// <param name="take">조회할 메시지 개수 (기본 50, 최소 10, 최대 100)</param>
    /// <param name="cancellationToken">작업 취소 토큰</param>
    /// <returns>메시지 목록 (최신순 정렬)</returns>
    /// <exception cref="NotFoundException">채팅방 미존재</exception>
    /// <exception cref="ForbiddenException">채팅방 접근 권한 없음</exception>
    Task<List<ChatMessageDto>> GetMessagesAsync(
        Guid roomId,
        Guid characterId,
        Guid? beforeId = null,
        int take = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 캐릭터가 특정 채팅방에 접근할 수 있는지 확인합니다.
    /// </summary>
    /// <param name="characterId">캐릭터 ID</param>
    /// <param name="roomId">채팅방 ID</param>
    /// <param name="cancellationToken">작업 취소 토큰</param>
    /// <returns>접근 가능 여부</returns>
    /// <remarks>
    /// - Global 채팅방: 모든 캐릭터 접근 가능
    /// - Guild 채팅방: 같은 길드 소속 캐릭터만 접근 가능
    /// - Whisper 채팅방: 참여자로 등록된 캐릭터만 접근 가능
    /// </remarks>
    Task<bool> CanAccessRoomAsync(
        Guid characterId,
        Guid roomId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 캐릭터가 접근 가능한 모든 채팅방 목록을 조회합니다.
    /// </summary>
    /// <param name="characterId">캐릭터 ID</param>
    /// <param name="cancellationToken">작업 취소 토큰</param>
    /// <returns>접근 가능한 채팅방 목록 (Global + Guild + Whisper)</returns>
    /// <exception cref="NotFoundException">캐릭터 미존재</exception>
    Task<List<ChatRoomDto>> GetAccessibleRoomsAsync(
        Guid characterId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 두 캐릭터 간의 1:1 귓속말 채팅방을 생성하거나 기존 방을 조회합니다.
    /// </summary>
    /// <param name="characterAId">캐릭터 A ID</param>
    /// <param name="characterBId">캐릭터 B ID</param>
    /// <param name="cancellationToken">작업 취소 토큰</param>
    /// <returns>생성되거나 조회된 채팅방 ID</returns>
    /// <exception cref="InvalidOperationException">자기 자신과 귓속말 시도 (A == B)</exception>
    /// <exception cref="NotFoundException">캐릭터 미존재</exception>
    /// <remarks>
    /// - 기존 방이 있으면 재사용 (A-B 또는 B-A 동일하게 처리)
    /// - 없으면 새로운 Whisper 타입 채팅방 생성 및 참여자 2명 추가
    /// </remarks>
    Task<Guid> CreateWhisperRoomAsync(
        Guid characterAId,
        Guid characterBId,
        CancellationToken cancellationToken = default);
}
