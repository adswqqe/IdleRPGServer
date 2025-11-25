using MediatR;

namespace IdleRPG.Application.Quests.Queries.GetActiveQuests;

/// <summary>
/// 진행 중인 퀘스트 목록 조회 Query
/// </summary>
/// <remarks>
/// Query의 특징:
/// - 데이터를 조회만 하고, 상태를 변경하지 않음
/// - 부수효과(Side Effect)가 없어야 함
/// - 같은 입력에 항상 같은 결과 반환 (멱등성)
///
/// Command와의 차이:
/// - Command: 상태 변경 (AcceptQuestCommand)
/// - Query: 상태 조회 (GetActiveQuestsQuery)
/// </remarks>
/// <param name="CharacterId">조회할 캐릭터 ID</param>
public record GetActiveQuestsQuery(Guid CharacterId) : IRequest<List<QuestDto>>;
