using IdleRPG.Domain.Repositories;

namespace IdleRPG.Domain.Entities;

/// <summary>
/// 퀘스트 상태
/// </summary>
public enum QuestStatus
{
    /// <summary>
    /// 진행 중
    /// </summary>
    InProgress = 0,

    /// <summary>
    /// 완료됨
    /// </summary>
    Completed = 1,

    /// <summary>
    /// 포기함
    /// </summary>
    Abandoned = 2
}

/// <summary>
/// 퀘스트 Entity
/// 캐릭터가 수락한 퀘스트 정보를 저장합니다.
/// </summary>
public class Quest : BaseEntity
{
    /// <summary>
    /// 퀘스트를 수락한 캐릭터 ID (FK)
    /// </summary>
    public Guid CharacterId { get; set; }

    /// <summary>
    /// 퀘스트 템플릿 ID (정적 데이터 참조)
    /// 예: 1 = "고블린 10마리 처치", 2 = "보스 처치" 등
    /// </summary>
    public int QuestTemplateId { get; set; }

    /// <summary>
    /// 퀘스트 상태
    /// </summary>
    public QuestStatus Status { get; set; } = QuestStatus.InProgress;

    /// <summary>
    /// 퀘스트 수락 시간
    /// </summary>
    public DateTime AcceptedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 퀘스트 완료 시간 (완료된 경우에만 값이 있음)
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// 현재 진행도 (예: 고블린 7/10 처치)
    /// </summary>
    public int Progress { get; set; } = 0;

    /// <summary>
    /// 목표 수량 (예: 10마리)
    /// Step 3에서 QuestTemplate을 참조하도록 변경할 수 있음
    /// </summary>
    public int TargetCount { get; set; } = 1;

    // Navigation Property
    public Character Character { get; set; } = null!;
}
