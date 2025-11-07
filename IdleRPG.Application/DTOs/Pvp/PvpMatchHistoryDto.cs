using IdleRPG.Domain.Enums;

namespace IdleRPG.Application.DTOs.Pvp;

/// <summary>
/// PVP 매치 히스토리 DTO
/// </summary>
public class PvpMatchHistoryDto
{
    /// <summary>
    /// 매치 ID
    /// </summary>
    public Guid MatchId { get; set; }

    /// <summary>
    /// 시즌 번호
    /// </summary>
    /// <example>1</example>
    public int SeasonNumber { get; set; }

    /// <summary>
    /// 상대방 캐릭터 ID
    /// </summary>
    public Guid OpponentCharacterId { get; set; }

    /// <summary>
    /// 상대방 캐릭터 이름
    /// </summary>
    /// <example>ShadowKnight</example>
    public string OpponentName { get; set; } = string.Empty;

    /// <summary>
    /// 전투 결과 (Victory, Defeat)
    /// </summary>
    /// <example>Victory</example>
    public PvpMatchResult Result { get; set; }

    /// <summary>
    /// 내 레이팅 변화량
    /// </summary>
    /// <remarks>
    /// 양수: 레이팅 상승 (승리)
    /// 음수: 레이팅 하락 (패배)
    /// </remarks>
    /// <example>16</example>
    public int MyRatingChange { get; set; }

    /// <summary>
    /// 상대방 레이팅 변화량
    /// </summary>
    /// <example>-16</example>
    public int OpponentRatingChange { get; set; }

    /// <summary>
    /// 매치 생성 일시 (UTC)
    /// </summary>
    /// <example>2025-11-07T14:30:00Z</example>
    public DateTime CreatedAt { get; set; }
}
