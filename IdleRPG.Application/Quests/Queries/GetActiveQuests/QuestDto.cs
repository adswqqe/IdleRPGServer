namespace IdleRPG.Application.Quests.Queries.GetActiveQuests;

/// <summary>
/// 퀘스트 조회 결과 DTO
/// </summary>
/// <remarks>
/// DTO (Data Transfer Object)의 목적:
/// - Entity를 직접 노출하지 않음 (캡슐화)
/// - API 응답에 필요한 필드만 선택적으로 포함
/// - Entity 변경이 API 응답에 영향을 주지 않음 (결합도 낮춤)
/// </remarks>
public record QuestDto
{
    /// <summary>
    /// 퀘스트 ID
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// 퀘스트 템플릿 ID
    /// </summary>
    public int QuestTemplateId { get; init; }

    /// <summary>
    /// 퀘스트 상태 (문자열)
    /// </summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// 현재 진행도
    /// </summary>
    public int Progress { get; init; }

    /// <summary>
    /// 목표 수량
    /// </summary>
    public int TargetCount { get; init; }

    /// <summary>
    /// 퀘스트 수락 시간
    /// </summary>
    public DateTime AcceptedAt { get; init; }
}
