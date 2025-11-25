using MediatR;
using Microsoft.Extensions.Logging;

namespace IdleRPG.Application.Quests.Commands.AcceptQuest;

/// <summary>
/// AcceptQuestCommand를 처리하는 Handler
/// </summary>
/// <remarks>
/// Handler의 책임:
/// 1. Command에 담긴 의도를 해석하고 실행
/// 2. 비즈니스 로직 처리
/// 3. 결과 반환
///
/// Step 2에서는 로그만 출력합니다.
/// Step 3에서 실제 DB 저장 로직을 추가합니다.
/// </remarks>
public class AcceptQuestCommandHandler : IRequestHandler<AcceptQuestCommand, AcceptQuestResult>
{
    private readonly ILogger<AcceptQuestCommandHandler> _logger;

    /// <summary>
    /// 생성자 - 의존성 주입
    /// </summary>
    /// <remarks>
    /// MediatR이 Handler를 실행할 때, DI 컨테이너에서
    /// ILogger를 자동으로 주입합니다.
    ///
    /// Step 3에서 IUnitOfWork도 여기에 추가됩니다:
    /// public AcceptQuestCommandHandler(IUnitOfWork unitOfWork, ILogger logger)
    /// </remarks>
    public AcceptQuestCommandHandler(ILogger<AcceptQuestCommandHandler> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Command를 처리하는 핵심 메서드
    /// </summary>
    /// <param name="request">AcceptQuestCommand (CharacterId, QuestTemplateId 포함)</param>
    /// <param name="cancellationToken">취소 토큰</param>
    /// <returns>AcceptQuestResult (성공 여부, 퀘스트 ID, 메시지)</returns>
    public Task<AcceptQuestResult> Handle(
        AcceptQuestCommand request,
        CancellationToken cancellationToken)
    {
        // Step 2: 로그 출력만 수행
        // Step 3에서 실제 DB 저장 로직으로 대체됩니다
        _logger.LogInformation(
            "🎯 Quest {QuestTemplateId} accepted by Character {CharacterId}",
            request.QuestTemplateId,
            request.CharacterId);

        // 임시 ID 생성 (Step 3에서 실제 DB 저장 후 반환)
        var tempQuestId = Guid.NewGuid();

        var result = new AcceptQuestResult(
            Success: true,
            QuestId: tempQuestId,
            Message: $"Quest {request.QuestTemplateId} accepted successfully!"
        );

        return Task.FromResult(result);
    }
}
