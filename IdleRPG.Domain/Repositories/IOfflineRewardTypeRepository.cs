using IdleRPG.Domain.Entities;

namespace IdleRPG.Domain.Repositories;

/// <summary>
/// 오프라인 보상 타입 Repository 인터페이스
/// </summary>
public interface IOfflineRewardTypeRepository
{
    /// <summary>
    /// 기본 오프라인 보상 타입 조회
    /// 현재는 시딩된 1개의 보상 타입만 존재
    /// </summary>
    Task<OfflineRewardType> GetDefaultAsync();

    /// <summary>
    /// ID로 오프라인 보상 타입 조회
    /// </summary>
    Task<OfflineRewardType?> GetByIdAsync(Guid id);
}
