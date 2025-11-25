using MediatR;

namespace IdleRPG.Domain.Events;

/// <summary>
/// 퀘스트가 수락되었을 때 발행되는 Domain Event
/// </summary>
/// <remarks>
/// 🎓 Domain Event의 핵심 개념:
///
/// 1. INotification 인터페이스
///    - MediatR에서 "이벤트"를 나타내는 마커 인터페이스
///    - IRequest와 달리 응답을 기대하지 않음 (Fire-and-Forget)
///    - 여러 Handler가 동시에 구독 가능
///
/// 2. 불변성 (Immutability)
///    - record 타입 사용으로 불변 객체 보장
///    - 이벤트는 "이미 발생한 사실"이므로 변경되면 안 됨
///
/// 3. 도메인 중심 명명
///    - "QuestAccepted" = 비즈니스 관점의 이름
///    - 기술적 이름(QuestCreatedMessage) 대신 비즈니스 용어 사용
/// </remarks>
public record QuestAcceptedEvent(
    Guid QuestId,
    Guid CharacterId,
    int QuestTemplateId,
    DateTime AcceptedAt
) : INotification;
