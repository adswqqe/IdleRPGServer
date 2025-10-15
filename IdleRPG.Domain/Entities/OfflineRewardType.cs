namespace IdleRPG.Domain.Entities;

/// <summary>
/// 오프라인 보상 타입 (마스터 데이터)
/// 플레이어가 오프라인 상태일 때 획득할 수 있는 보상의 계산 공식을 정의
/// </summary>
public class OfflineRewardType
{
    /// <summary>
    /// 보상 타입 고유 ID
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 보상 타입 이름 (예: "Basic Offline Reward")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 분당 경험치 계수 (실제 보상 = 캐릭터 레벨 × 이 값 × 경과 시간(분))
    /// </summary>
    public int ExperiencePerMinute { get; set; }

    /// <summary>
    /// 분당 골드 계수 (실제 보상 = 캐릭터 레벨 × 이 값 × 경과 시간(분))
    /// </summary>
    public int GoldPerMinute { get; set; }

    /// <summary>
    /// 최대 누적 가능 시간 (분 단위)
    /// 이 시간을 초과한 오프라인 시간은 보상 계산에 포함되지 않음
    /// </summary>
    public int MaxMinutes { get; set; }
}
