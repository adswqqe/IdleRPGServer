using IdleRPG.Application.Interfaces;
using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IdleRPG.Application.Quests.Commands.AcceptQuest;

/// <summary>
/// AcceptQuestCommand를 처리하는 Handler
/// </summary>
/// <remarks>
/// Handler의 책임:
/// 1. Command에 담긴 의도를 해석하고 실행
/// 2. 비즈니스 로직 처리 (DB 저장)
/// 3. 결과 반환
/// 4. Domain Event 발행 (후속 처리 트리거)
///
/// 🎓 Domain Event 발행 패턴:
/// - Command Handler는 "무엇이 일어났는지"만 알림
/// - 후속 작업(업적, 알림 등)은 Event Handler가 처리
/// - 느슨한 결합으로 확장성 확보
/// </remarks>
public class AcceptQuestCommandHandler : IRequestHandler<AcceptQuestCommand, AcceptQuestResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly ILogger<AcceptQuestCommandHandler> _logger;

    /// <summary>
    /// 생성자 - 의존성 주입
    /// IUnitOfWork를 통해 Repository와 트랜잭션에 접근합니다.
    /// IMediator를 통해 Domain Event를 발행합니다.
    /// </summary>
    public AcceptQuestCommandHandler(
        IUnitOfWork unitOfWork,
        IMediator mediator,
        ILogger<AcceptQuestCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Command를 처리하는 핵심 메서드
    /// </summary>
    public async Task<AcceptQuestResult> Handle(
        AcceptQuestCommand request,
        CancellationToken cancellationToken)
    {
        var quest = new Quest()
        {
            Id = Guid.NewGuid(),
            CharacterId = request.CharacterId,
            QuestTemplateId = request.QuestTemplateId,
            Status = QuestStatus.InProgress,
            AcceptedAt = DateTime.UtcNow,
        };

        await _unitOfWork.Quests.AddAsync(quest);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("🎯 Quest {QuestId} created for Character {CharacterId}",
            quest.Id, request.CharacterId);

        // Domain Event 발행
        // 🎓 Publish vs Send:
        // - Send(): Command/Query → 단일 Handler 호출, 응답 있음
        // - Publish(): Event → 모든 구독 Handler 호출, 응답 없음 (Fire-and-Forget)
        var questAcceptedEvent = new QuestAcceptedEvent(
            quest.Id,
            quest.CharacterId,
            quest.QuestTemplateId,
            quest.AcceptedAt);

        await _mediator.Publish(questAcceptedEvent, cancellationToken);

        _logger.LogInformation("📢 QuestAcceptedEvent published for Quest {QuestId}", quest.Id);

        return new AcceptQuestResult(true, quest.Id, "Quest accepted successfully!");
    }
}
