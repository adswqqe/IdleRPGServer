namespace IdleRPG.Domain.Enums;

/// <summary>
/// 채팅방 타입을 정의하는 열거형
/// </summary>
public enum RoomType
{
    /// <summary>
    /// 전체 채팅 - 모든 사용자가 접근 가능
    /// </summary>
    Global = 1,

    /// <summary>
    /// 길드 채팅 - 같은 길드 멤버만 접근 가능
    /// </summary>
    Guild = 2,

    /// <summary>
    /// 귓속말 (1:1 채팅) - 참여자만 접근 가능
    /// </summary>
    Whisper = 3
}
