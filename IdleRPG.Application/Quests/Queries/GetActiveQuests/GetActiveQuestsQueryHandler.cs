using IdleRPG.Application.Interfaces;
using MediatR;

namespace IdleRPG.Application.Quests.Queries.GetActiveQuests;

/// <summary>
/// 진행 중인 퀘스트 목록 조회 Handler
/// </summary>
/// <remarks>
/// Query Handler의 특징:
/// - 데이터 조회만 담당 (상태 변경 없음)
/// - SaveChangesAsync() 호출 없음
/// - 결과를 DTO로 변환하여 반환
/// </remarks>
public class GetActiveQuestsQueryHandler : IRequestHandler<GetActiveQuestsQuery, List<QuestDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetActiveQuestsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<QuestDto>> Handle(GetActiveQuestsQuery request, CancellationToken cancellationToken)
    {
        // 1. Repository에서 캐릭터의 활성 퀘스트 목록 조회
        var activeQuests = await _unitOfWork.Quests.GetActiveQuestsByCharacterIdAsync(request.CharacterId);

        // 2. Quest Entity를 QuestDto로 변환
        var questDtos = activeQuests.Select(q => new QuestDto
        {
            Id = q.Id,
            QuestTemplateId = q.QuestTemplateId,
            Status = q.Status.ToString(),
            Progress = q.Progress,
            TargetCount = q.TargetCount,
            AcceptedAt = q.AcceptedAt
        }).ToList();

        return questDtos;
    }
}
