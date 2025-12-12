using IdleRPG.Application.DTOs.Characters;

namespace IdleRPG.Application.DTOs;

/// <summary>
/// 오프라인 보상 조회 응답 DTO
/// </summary>
public class OfflineRewardDto
{
    /// <summary>
    /// 보상 타입 ID (클라이언트가 로컬 데이터 테이블에서 조회용)
    /// </summary>
    public Guid RewardTypeId { get; set; }

    /// <summary>
    /// 오프라인 경과 시간 (분)
    /// </summary>
    public int OfflineMinutes { get; set; }

    /// <summary>
    /// 계산된 경험치 보상
    /// </summary>
    public int CalculatedExperience { get; set; }

    /// <summary>
    /// 계산된 골드 보상
    /// </summary>
    public long CalculatedGold { get; set; }
}

/// <summary>
/// 오프라인 보상 수령 응답 DTO
/// </summary>
public class ClaimOfflineRewardDto
{
    /// <summary>
    /// 보상 수령 성공 여부
    /// </summary>
    public bool Claimed { get; set; }

    /// <summary>
    /// 지급된 경험치
    /// </summary>
    public int Experience { get; set; }

    /// <summary>
    /// 지급된 골드
    /// </summary>
    public long Gold { get; set; }

    /// <summary>
    /// 업데이트된 캐릭터 정보
    /// </summary>
    public CharacterDto UpdatedCharacter { get; set; } = null!;
}
