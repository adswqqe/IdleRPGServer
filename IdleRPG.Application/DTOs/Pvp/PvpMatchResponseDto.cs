using IdleRPG.Application.DTOs.Battle;
using IdleRPG.Domain.Enums;

namespace IdleRPG.Application.DTOs.Pvp;

/// <summary>
/// PVP 매치 결과 응답 DTO
/// </summary>
public class PvpMatchResponseDto
{
    /// <summary>
    /// 매치 ID (PvpMatch.Id)
    /// </summary>
    public Guid MatchId { get; set; }

    /// <summary>
    /// 상대방 정보
    /// </summary>
    public MatchOpponentDto Opponent { get; set; } = null!;

    /// <summary>
    /// 전투 결과 (Victory, Defeat)
    /// </summary>
    /// <example>Victory</example>
    public PvpMatchResult Result { get; set; }

    /// <summary>
    /// 내 변경 전 레이팅
    /// </summary>
    /// <example>1450</example>
    public int MyRatingBefore { get; set; }

    /// <summary>
    /// 내 변경 후 레이팅
    /// </summary>
    /// <example>1466</example>
    public int MyRatingAfter { get; set; }

    /// <summary>
    /// 레이팅 변화량 (MyRatingAfter - MyRatingBefore)
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
    /// 획득한 보상
    /// </summary>
    /// <remarks>
    /// Key: 보상 타입 (Gold, Crystal, Experience)
    /// Value: 수량
    /// </remarks>
    /// <example>
    /// {
    ///   "Gold": 100,
    ///   "Crystal": 10,
    ///   "Experience": 50
    /// }
    /// </example>
    public Dictionary<string, int> Rewards { get; set; } = new();

    /// <summary>
    /// 전투 로그 (상세 전투 기록, Optional)
    /// </summary>
    /// <remarks>
    /// 향후 리플레이 시스템 구현 시 사용
    /// </remarks>
    public List<string> CombatLog { get; set; } = new();
}
