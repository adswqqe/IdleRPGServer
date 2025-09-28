using IdleRPG.Domain.Entities;
namespace IdleRPG.Domain.Repositories
{
    public interface ICharacterRepository : IRepository<Character>
    {
        // Character만의 특별한 기능들
        Task<List<Character>> GetCharactersByPlayerIdAsync(Guid playerId);
        Task<Character> GetMainCharacterAsync(Guid playerId);
        Task<List<Character>> GetCharactersByLevelRangeAsync(int minLevel, int maxLevel);
        Task<List<Character>> GetCharactersWithInventoryAsync(Guid playerId);
    }
}