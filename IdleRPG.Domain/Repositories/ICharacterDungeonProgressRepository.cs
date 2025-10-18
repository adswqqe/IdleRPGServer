using IdleRPG.Domain.Entities;

namespace IdleRPG.Domain.Repositories;

/// <summary>
/// CharacterDungeonProgress 엔티티의 Repository 인터페이스입니다.
/// 던전 스테이지를 통한 캐릭터 진행 상황을 추적합니다.
/// </summary>
public interface ICharacterDungeonProgressRepository : IRepository<CharacterDungeonProgress>
{
    /// <summary>
    /// 특정 캐릭터의 던전 진행 상황을 가져옵니다.
    /// 진행 상황 레코드가 없으면 새로 생성합니다.
    /// </summary>
    /// <param name="characterId">캐릭터의 ID</param>
    /// <returns>캐릭터의 던전 진행 상황</returns>
    Task<CharacterDungeonProgress> GetOrCreateByCharacterIdAsync(Guid characterId);
}
