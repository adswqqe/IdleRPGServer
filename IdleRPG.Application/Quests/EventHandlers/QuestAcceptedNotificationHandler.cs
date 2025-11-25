using IdleRPG.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IdleRPG.Application.Quests.EventHandlers;

/// <summary>
/// 퀘스트 수락 시 알림 전송을 담당하는 Event Handler
/// </summary>
/// <remarks>
/// 🎓 여러 Handler가 같은 이벤트를 구독하는 패턴:
///
/// QuestAcceptedEvent 발행 시:
/// ┌─────────────────────────────────────┐
/// │     AcceptQuestCommandHandler       │
/// │         _mediator.Publish()         │
/// └──────────────┬──────────────────────┘
///                │ QuestAcceptedEvent
///                ▼
/// ┌──────────────────────────────────────┐
/// │           MediatR                    │
/// │   (모든 Handler 자동 호출)            │
/// └───────┬───────────────┬──────────────┘
///         │               │
///         ▼               ▼
/// ┌───────────────┐ ┌────────────────────┐
/// │ Achievement   │ │ Notification       │
/// │ Handler       │ │ Handler            │
/// │ (업적 확인)    │ │ (알림 전송)         │
/// └───────────────┘ └────────────────────┘
///
/// 핵심: 각 Handler는 서로를 모르고, 독립적으로 실행됨
/// </remarks>
public class QuestAcceptedNotificationHandler : INotificationHandler<QuestAcceptedEvent>
{
    private readonly ILogger<QuestAcceptedNotificationHandler> _logger;

    public QuestAcceptedNotificationHandler(ILogger<QuestAcceptedNotificationHandler> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 이벤트 처리 - 알림 전송 로직
    /// </summary>
    public Task Handle(QuestAcceptedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("[Notification] Preparing notification for Character {CharacterId}...", notification.CharacterId);
        _logger.LogInformation("Notification sent: 퀘스트 #{QuestTemplateId}를 수락했습니다!", notification.QuestTemplateId);

        return Task.CompletedTask;
    }
}
