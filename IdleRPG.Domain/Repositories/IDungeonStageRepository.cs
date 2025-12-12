using IdleRPG.Domain.Entities;

namespace IdleRPG.Domain.Repositories;

/// <summary>
/// DungeonStage 엔티티의 Repository 인터페이스입니다.
/// 던전 스테이지 정보에 대한 데이터 액세스 메서드를 제공합니다.
/// </summary>
public interface IDungeonStageRepository : IRepository<DungeonStage>
{
    /// <summary>
    /// 지정된 레벨의 캐릭터가 접근 가능한 모든 던전 스테이지를 가져옵니다.
    /// </summary>
    /// <param name="characterLevel">캐릭터의 현재 레벨</param>
    /// <returns>접근 가능한 던전 스테이지 목록</returns>
    Task<List<DungeonStage>> GetAccessibleStagesAsync(int characterLevel);

    /// <summary>
    /// ID로 던전 스테이지를 가져오며, Monster 탐색 속성을 포함합니다.
    /// </summary>
    /// <param name="stageId">스테이지 번호</param>
    /// <returns>몬스터 데이터를 포함한 던전 스테이지, 찾지 못하면 null</returns>
    Task<DungeonStage?> GetByIdWithMonsterAsync(int stageId);
}
