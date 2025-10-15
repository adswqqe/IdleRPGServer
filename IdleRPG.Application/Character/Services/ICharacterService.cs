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
        /// 경험치 추가 및 레벨업 처리 (SaveChanges 없음)
        /// 여러 작업을 한 트랜잭션으로 묶을 때 사용
        /// </summary>
        void ProcessExperienceGain(CharacterEntity character, int amount);
    }
}