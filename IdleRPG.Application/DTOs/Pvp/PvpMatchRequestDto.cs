namespace IdleRPG.Application.DTOs.Pvp;

/// <summary>
/// PVP 매치 시작 요청 DTO
/// </summary>
public class PvpMatchRequestDto
{
    /// <summary>
    /// 매치에 참가할 캐릭터 ID
    /// </summary>
    /// <remarks>
    /// 요청한 userId의 소유 캐릭터여야 함 (소유권 검증 필수)
    /// </remarks>
    /// <example>a1b2c3d4-e5f6-7890-abcd-ef1234567890</example>
    public Guid CharacterId { get; set; }
}
