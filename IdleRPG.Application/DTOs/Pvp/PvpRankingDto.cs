namespace IdleRPG.Application.DTOs.Pvp;

/// <summary>
/// PVP 랭킹 정보 DTO
/// </summary>
public class PvpRankingDto
{
    /// <summary>
    /// 순위 (1등부터 시작)
    /// </summary>
    /// <remarks>
    /// 1-based ranking (1, 2, 3...)
    /// Redis Sorted Set ZREVRANK 결과 + 1
    /// </remarks>
    /// <example>15</example>
    public int Rank { get; set; }

    /// <summary>
    /// 캐릭터 ID
    /// </summary>
    public Guid CharacterId { get; set; }

    /// <summary>
    /// 캐릭터 이름
    /// </summary>
    /// <example>DragonSlayer</example>
    public string CharacterName { get; set; } = string.Empty;

    /// <summary>
    /// 현재 레이팅
    /// </summary>
    /// <example>1850</example>
    public int Rating { get; set; }

    /// <summary>
    /// 승리 횟수
    /// </summary>
    /// <example>42</example>
    public int Wins { get; set; }

    /// <summary>
    /// 패배 횟수
    /// </summary>
    /// <example>18</example>
    public int Losses { get; set; }

    /// <summary>
    /// 승률 (백분율, 0~100)
    /// </summary>
    /// <remarks>
    /// 계산식: (Wins / (Wins + Losses)) * 100
    /// 전적이 없으면 0
    /// </remarks>
    /// <example>70.0</example>
    public double WinRate { get; set; }

    /// <summary>
    /// 티어 (Bronze, Silver, Gold, Platinum, Diamond)
    /// </summary>
    /// <example>Gold</example>
    public string Tier { get; set; } = string.Empty;
}
