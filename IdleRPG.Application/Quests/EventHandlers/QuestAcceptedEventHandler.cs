using IdleRPG.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IdleRPG.Application.Quests.EventHandlers;

/// <summary>
/// 퀘스트 수락 시 업적(Achievement) 확인을 담당하는 Event Handler
/// </summary>
/// <remarks>
/// 🎓 Event Handler의 핵심 개념:
///
/// 1. INotificationHandler<TEvent>
///    - 특정 이벤트를 구독하는 Handler
///    - 하나의 이벤트에 여러 Handler 등록 가능
///    - MediatR이 자동으로 모든 Handler 호출
///
/// 2. 느슨한 결합 (Loose Coupling)
///    - 이 Handler는 AcceptQuestCommandHandler를 모름
///    - AcceptQuestCommandHandler도 이 Handler를 모름
///    - 오직 "이벤트"만 공유
///
/// 3. 단일 책임
///    - 이 Handler는 "업적 확인"만 담당
///    - 알림, 통계 등은 다른 Handler가 처리
/// </remarks>
public class QuestAcceptedEventHandler : INotificationHandler<QuestAcceptedEvent>
{
    private readonly ILogger<QuestAcceptedEventHandler> _logger;

    public QuestAcceptedEventHandler(ILogger<QuestAcceptedEventHandler> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 이벤트 처리 - 업적 확인 로직
    /// </summary>
    public Task Handle(QuestAcceptedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("[Achievement] Checking achievements for Quest {QuestId}", notification.QuestId);

        if (notification.QuestTemplateId == 1)
            _logger.LogInformation("Achievement check completed for Character {CharacterId}", notification.CharacterId);

        return Task.CompletedTask;
    }
}
