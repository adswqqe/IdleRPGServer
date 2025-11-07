namespace IdleRPG.Application.DTOs.Pvp;

/// <summary>
/// PVP 시즌 정보 DTO
/// </summary>
public class PvpSeasonDto
{
    /// <summary>
    /// 시즌 ID
    /// </summary>
    /// <example>1</example>
    public int SeasonId { get; set; }

    /// <summary>
    /// 시즌 번호
    /// </summary>
    /// <example>1</example>
    public int SeasonNumber { get; set; }

    /// <summary>
    /// 시즌 시작일 (UTC)
    /// </summary>
    /// <example>2025-11-01T00:00:00Z</example>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// 시즌 종료일 (UTC)
    /// </summary>
    /// <example>2026-02-01T00:00:00Z</example>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// 시즌 종료까지 남은 일수
    /// </summary>
    /// <remarks>
    /// 계산식: (EndDate - DateTime.UtcNow).Days
    /// 음수면 시즌 종료됨
    /// </remarks>
    /// <example>85</example>
    public int DaysRemaining { get; set; }

    /// <summary>
    /// 시즌 활성화 여부
    /// </summary>
    /// <remarks>
    /// true: 현재 진행 중인 시즌
    /// false: 종료된 과거 시즌
    /// </remarks>
    public bool IsActive { get; set; }
}
