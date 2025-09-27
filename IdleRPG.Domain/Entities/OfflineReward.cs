namespace IdleRPG.Domain.Entities
{
/// <summary>
/// 오프라인 보상 정보
/// 플레이어가 게임을 하지 않은 시간 동안 누적된 보상
/// </summary>
public class OfflineReward
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// 보상 받을 캐릭터 ID
    /// </summary>
    public Guid CharacterId { get; set; }
    
    /// <summary>
    /// 오프라인 시작 시간 (마지막 로그아웃 시간)
    /// </summary>
    public DateTime OfflineStartTime { get; set; }
    
    /// <summary>
    /// 오프라인 종료 시간 (다시 로그인한 시간)
    /// </summary>
    public DateTime? OfflineEndTime { get; set; }
    
    /// <summary>
    /// 실제 오프라인 시간 (분 단위)
    /// 최대 보상 시간 제한을 적용한 시간
    /// </summary>
    public int EffectiveOfflineMinutes { get; set; }
    
    /// <summary>
    /// 보상 유형
    /// </summary>
    public OfflineRewardType RewardType { get; set; }
    
    /// <summary>
    /// 보상받은 경험치
    /// </summary>
    public long ExperienceGained { get; set; }
    
    /// <summary>
    /// 보상받은 골드
    /// </summary>
    public long GoldGained { get; set; }
    
    /// <summary>
    /// 보상받은 아이템들 (JSON 배열)
    /// [{"itemId": 1001, "quantity": 5}, {"itemId": 2001, "quantity": 1}]
    /// </summary>
    public string RewardItems { get; set; } = "[]";
    
    /// <summary>
    /// 보상 배율 (프리미엄 효과 등)
    /// </summary>
    public float RewardMultiplier { get; set; } = 1.0f;
    
    /// <summary>
    /// 보상 수령 여부
    /// </summary>
    public bool IsClaimed { get; set; } = false;
    
    /// <summary>
    /// 보상 수령 시간
    /// </summary>
    public DateTime? ClaimedAt { get; set; }
    
    /// <summary>
    /// 보상 생성 시간
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation Properties
    public Character Character { get; set; }
}

/// <summary>
/// 오프라인 보상 타입
/// </summary>
public enum OfflineRewardType
{
    Idle,           // 방치형 보상
    AutoBattle,     // 자동 전투 보상
    AutoGathering,  // 자동 채집 보상
    TimeBased       // 시간 기반 보상
}

/// <summary>
/// 오프라인 보상 아이템 정보
/// JSON 직렬화용 클래스
/// </summary>
public class OfflineRewardItem
{
    public int ItemId { get; set; }
    public int Quantity { get; set; }
    public int EnhancementLevel { get; set; } = 0;
}
}