using IdleRPG.Application.DTOs.Characters;
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
    }
}