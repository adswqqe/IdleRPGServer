using MediatR;

namespace IdleRPG.Application.Quests.Commands.AcceptQuest;

/// <summary>
/// 퀘스트 수락 Command
/// "나는 이 캐릭터로 이 퀘스트를 수락하고 싶다"는 의도를 담은 요청 객체
/// </summary>
/// <remarks>
/// record 타입을 사용하는 이유:
/// 1. 불변성(Immutable) - 요청은 한번 생성되면 변경되지 않아야 함
/// 2. 값 기반 동등성 - 같은 값이면 같은 요청으로 취급
/// 3. 간결한 문법 - 프로퍼티 선언이 매우 짧음
/// </remarks>
/// <param name="CharacterId">퀘스트를 수락할 캐릭터 ID</param>
/// <param name="QuestTemplateId">수락할 퀘스트 템플릿 ID</param>
public record AcceptQuestCommand(
    Guid CharacterId,
    int QuestTemplateId
) : IRequest<AcceptQuestResult>;

/// <summary>
/// 퀘스트 수락 결과 DTO
/// </summary>
/// <param name="Success">성공 여부</param>
/// <param name="QuestId">생성된 퀘스트 ID (Step 3에서 실제 ID 반환)</param>
/// <param name="Message">결과 메시지</param>
public record AcceptQuestResult(
    bool Success,
    Guid? QuestId,
    string Message
);
