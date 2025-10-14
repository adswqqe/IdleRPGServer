using IdleRPG.Domain.Entities;

namespace IdleRPG.Domain.Repositories
{
    public interface ICharacterRepository : IRepository<Character>
    {
        // Character 전용 메서드들
        Task<Character?> GetByIdAsync(Guid id);
        Task<List<Character>> GetByPlayerIdAsync(Guid playerId);
        Task<bool> ExistsAsync(Guid id);
        Task<int> CountByPlayerIdAsync(Guid playerId);
        
        // 기본 CRUD (IRepository에서 상속받지만 명시적으로 선언)
        void Delete(Character character);
    } 
}