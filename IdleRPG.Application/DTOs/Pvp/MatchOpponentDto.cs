namespace IdleRPG.Application.DTOs.Pvp;

/// <summary>
/// 매칭된 상대방 정보 DTO
/// </summary>
public class MatchOpponentDto
{
    /// <summary>
    /// 상대방 캐릭터 ID
    /// </summary>
    /// <remarks>
    /// IsBot = true인 경우 Guid.Empty (NPC 봇)
    /// </remarks>
    public Guid CharacterId { get; set; }

    /// <summary>
    /// 상대방 이름
    /// </summary>
    /// <remarks>
    /// IsBot = true인 경우 "Bot_1234" 형식
    /// </remarks>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 상대방 레이팅
    /// </summary>
    /// <remarks>
    /// IsBot = true인 경우 내 레이팅 ± Random(-100, 100)
    /// </remarks>
    public int Rating { get; set; }

    /// <summary>
    /// NPC 봇 여부
    /// </summary>
    /// <remarks>
    /// true: 매칭 타임아웃 시 생성된 NPC 봇
    /// false: 실제 플레이어 캐릭터
    /// </remarks>
    public bool IsBot { get; set; }
}
