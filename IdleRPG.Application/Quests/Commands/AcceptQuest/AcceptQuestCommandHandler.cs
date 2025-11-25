using IdleRPG.Application.Interfaces;
using IdleRPG.Domain.Entities;
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
/// </remarks>
public class AcceptQuestCommandHandler : IRequestHandler<AcceptQuestCommand, AcceptQuestResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AcceptQuestCommandHandler> _logger;

    /// <summary>
    /// 생성자 - 의존성 주입
    /// IUnitOfWork를 통해 Repository와 트랜잭션에 접근합니다.
    /// </summary>
    public AcceptQuestCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<AcceptQuestCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
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

        return new AcceptQuestResult(true, quest.Id, "Quest accepted successfully!");
    }
}
