using IdleRPG.Application.DTOs.Characters;
using CharacterEntity = IdleRPG.Domain.Entities.Character;

namespace IdleRPG.Application.Character.Services
{
    public interface ICharacterService
    {
        Task<CharacterDto> CreateCharacterAsync(Guid playerId, CreateCharacterDto dto);
        Task<CharacterDto?> GetCharacterByIdAsync(Guid characterId);
        Task<List<CharacterDto>> GetPlayerCharactersAsync(Guid playerId);
        Task DeleteCharacterAsync(Guid characterId);

        // 경험치 & 레벨업 (자동 성장 방식)
        Task<CharacterDto> AddExperienceAsync(Guid characterId, int amount);

        /// <summary>
        /// 경험치 추가 및 레벨업 처리 (레벨업 정보 반환)
        /// DungeonService 등에서 레벨업 여부를 확인해야 할 때 사용
        /// </summary>
        /// <returns>(IsLevelUp: 레벨업 여부, NewLevel: 최종 레벨, LevelUps: 레벨업 횟수)</returns>
        Task<(bool IsLevelUp, int NewLevel, int LevelUps)> AddExperienceWithResultAsync(Guid characterId, int amount);

        /// <summary>
        /// 경험치 추가 및 레벨업 처리 (SaveChanges 없음)
        /// 여러 작업을 한 트랜잭션으로 묶을 때 사용
        /// </summary>
        /// <returns>(IsLevelUp: 레벨업 여부, LevelUps: 레벨업 횟수)</returns>
        (bool IsLevelUp, int LevelUps) ProcessExperienceGain(CharacterEntity character, int amount);
    }
}