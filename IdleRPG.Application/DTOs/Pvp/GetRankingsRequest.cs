using IdleRPG.Domain.Enums;

namespace IdleRPG.Application.DTOs.Pvp;

/// <summary>
/// PVP 랭킹 조회 요청 (Query Parameters)
/// </summary>
public class GetRankingsRequest
{
    /// <summary>
    /// 조회할 시즌 ID (null이면 현재 활성 시즌)
    /// </summary>
    /// <example>1</example>
    public int? SeasonId { get; set; }

    /// <summary>
    /// 상위 N명 조회 (1~1000)
    /// </summary>
    /// <remarks>
    /// Top, NearMe, Tier 중 하나만 사용
    /// </remarks>
    /// <example>100</example>
    public int? Top { get; set; }

    /// <summary>
    /// 내 주변 랭킹 조회 여부
    /// </summary>
    /// <remarks>
    /// true일 때 Range와 함께 사용 (내 주변 ±Range 조회)
    /// </remarks>
    public bool? NearMe { get; set; }

    /// <summary>
    /// 내 주변 범위 (1~50)
    /// </summary>
    /// <remarks>
    /// NearMe=true일 때만 사용
    /// </remarks>
    /// <example>10</example>
    public int? Range { get; set; }

    /// <summary>
    /// 티어별 필터링 (Bronze, Silver, Gold, Platinum, Diamond)
    /// </summary>
    /// <example>Gold</example>
    public PvpTier? Tier { get; set; }

    /// <summary>
    /// 페이지 번호 (1부터 시작, 티어별 조회 시 사용)
    /// </summary>
    /// <example>1</example>
    public int Page { get; set; } = 1;

    /// <summary>
    /// 페이지 크기 (티어별 조회 시 사용)
    /// </summary>
    /// <example>20</example>
    public int PageSize { get; set; } = 20;
}
