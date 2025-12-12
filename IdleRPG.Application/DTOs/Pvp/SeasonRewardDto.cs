namespace IdleRPG.Application.DTOs.Pvp;

/// <summary>
/// 시즌 보상 정보 DTO
/// </summary>
public class SeasonRewardDto
{
    /// <summary>
    /// 시즌 번호
    /// </summary>
    /// <example>1</example>
    public int SeasonNumber { get; set; }

    /// <summary>
    /// 달성한 티어
    /// </summary>
    /// <example>Gold</example>
    public string Tier { get; set; } = string.Empty;

    /// <summary>
    /// 최종 레이팅
    /// </summary>
    /// <example>1850</example>
    public int FinalRating { get; set; }

    /// <summary>
    /// 최종 순위
    /// </summary>
    /// <remarks>
    /// 1등부터 시작 (1-based)
    /// </remarks>
    /// <example>42</example>
    public int FinalRank { get; set; }

    /// <summary>
    /// 보상 아이템 목록
    /// </summary>
    /// <remarks>
    /// Key: 아이템 이름 (예: "Crystal", "LegendaryEquipmentBox")
    /// Value: 수량
    /// </remarks>
    /// <example>
    /// {
    ///   "Crystal": 500,
    ///   "LegendaryEquipmentBox": 1
    /// }
    /// </example>
    public Dictionary<string, int> Rewards { get; set; } = new();

    /// <summary>
    /// 이미 보상을 수령했는지 여부
    /// </summary>
    /// <remarks>
    /// true: 이미 수령 완료 (중복 수령 방지)
    /// false: 수령 가능
    /// </remarks>
    public bool AlreadyClaimed { get; set; }
}
