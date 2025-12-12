namespace IdleRPG.Application.DTOs.Pvp;

/// <summary>
/// PVP 매치 히스토리 조회 요청 (Query Parameters)
/// </summary>
public class GetMatchHistoryRequest
{
    /// <summary>
    /// 조회할 캐릭터 ID
    /// </summary>
    /// <example>a1b2c3d4-e5f6-7890-abcd-ef1234567890</example>
    public Guid CharacterId { get; set; }

    /// <summary>
    /// 조회할 시즌 ID (null이면 전체 시즌)
    /// </summary>
    /// <example>1</example>
    public int? SeasonId { get; set; }

    /// <summary>
    /// 페이지 번호 (1부터 시작)
    /// </summary>
    /// <example>1</example>
    public int Page { get; set; } = 1;

    /// <summary>
    /// 페이지 크기 (1~50)
    /// </summary>
    /// <example>20</example>
    public int PageSize { get; set; } = 20;
}
