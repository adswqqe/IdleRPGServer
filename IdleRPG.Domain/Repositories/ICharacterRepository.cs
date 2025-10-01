using IdleRPG.Domain.Entities;

namespace IdleRPG.Domain.Repositories
{
    public interface ICharacterRepository
    {
        Task<Character?> GetByIdAsync(Guid id);
        Task<List<Character>> GetByPlayerIdAsync(Guid playerId);
        Task AddAsync(Character character);
        void Update(Character character);
        void Delete(Character character);
        Task<bool> ExistsAsync(Guid id);
    }
}