using IdleRPG.Domain.Entities;

namespace IdleRPG.Domain.Repositories;

/// <summary>
/// 퀘스트 Repository 인터페이스
/// Domain 계층에 위치하여 Infrastructure에 대한 의존성을 역전시킵니다.
/// </summary>
public interface IQuestRepository : IRepository<Quest>
{
    /// <summary>
    /// ID로 퀘스트 조회
    /// </summary>
    Task<Quest?> GetByIdAsync(Guid id);

    /// <summary>
    /// 캐릭터의 진행 중인 퀘스트 목록 조회
    /// </summary>
    Task<List<Quest>> GetActiveQuestsByCharacterIdAsync(Guid characterId);

    /// <summary>
    /// 캐릭터가 특정 퀘스트를 이미 진행 중인지 확인
    /// (중복 수락 방지용 - Step 5에서 사용)
    /// </summary>
    Task<bool> IsQuestInProgressAsync(Guid characterId, int questTemplateId);
}
