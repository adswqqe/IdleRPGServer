using IdleRPG.Domain.Entities;
namespace IdleRPG.Domain.Repositories
{
    public interface IPlayerRepository : IRepository<Player>
    {
        // Player만의 특별한 기능들 (Unity의 Player 전용 메서드처럼)
        Task<Player?> GetByIdAsync(Guid id);
        Task<Player?> GetByUsernameAsync(string username);
        Task<List<Player>> GetTopPlayersByLevelAsync(int count);
        Task<bool> IsUsernameAvailableAsync(string username);
        Task<Player?> GetPlayerWithCharactersAsync(Guid playerId);
        Task<Player?> GetPlayerWithCharactersForUpdateAsync(Guid playerId);

        // 복잡한 쿼리들
        Task<List<Player>> GetActivePlayersAsync();
        Task<List<Player>> GetPlayersWithOfflineRewardsAsync();
        
        // SaveChanges
        Task<int> SaveChangesAsync();
    }
}