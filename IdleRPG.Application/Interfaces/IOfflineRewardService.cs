using IdleRPG.Application.DTOs;

namespace IdleRPG.Application.Interfaces;

/// <summary>
/// 오프라인 보상 계산 및 지급 서비스 인터페이스
/// </summary>
public interface IOfflineRewardService
{
    /// <summary>
    /// 캐릭터의 오프라인 보상을 계산하여 조회
    /// 실제 보상 지급 없이 정보만 반환
    /// </summary>
    /// <param name="characterId">캐릭터 ID</param>
    /// <returns>계산된 오프라인 보상 정보</returns>
    Task<OfflineRewardDto> CalculateOfflineRewardsAsync(Guid characterId);

    /// <summary>
    /// 캐릭터의 오프라인 보상을 실제로 지급
    /// 캐릭터의 경험치, 골드를 업데이트하고 LastLoginTime을 갱신
    /// </summary>
    /// <param name="characterId">캐릭터 ID</param>
    /// <returns>지급 결과 및 업데이트된 캐릭터 정보</returns>
    Task<ClaimOfflineRewardDto> ClaimOfflineRewardsAsync(Guid characterId);
}
