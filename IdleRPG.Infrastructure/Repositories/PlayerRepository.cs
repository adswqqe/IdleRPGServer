using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Repositories;
using IdleRPG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IdleRPG.Infrastructure.Repositories
{
    public class PlayerRepository : IPlayerRepository
    {
        private readonly GameDBContext _context;

        public PlayerRepository(GameDBContext context)
        {
            _context = context;
        }

        public async Task<Player?> GetByIdAsync(Guid id)
        {
            return await _context.Players.FindAsync(id);
        }

        public async Task<Player?> GetByUsernameAsync(string username)
        {
            return await _context.Players.FirstOrDefaultAsync(p => p.UserName == username);
        }

        public async Task<bool> IsUsernameAvailableAsync(string username)
        {
            return !await _context.Players.AnyAsync(p => p.UserName == username);
        }

        public async Task<Player?> GetPlayerWithCharactersAsync(Guid playerId)
        {
            return await _context.Players
                .Include(p => p.Characters)
                .FirstOrDefaultAsync(p => p.Id == playerId);
        }

        public async Task<Player?> GetPlayerWithCharactersForUpdateAsync(Guid playerId)
        {
            return await _context.Players
                .Include(p => p.Characters)
                .FirstOrDefaultAsync(p => p.Id == playerId);
        }

        public async Task<List<Player>> GetTopPlayersByLevelAsync(int count)
        {
            // TODO: 구현 예정 (플레이어 레벨 시스템 추가 후)
            throw new NotImplementedException();
        }

        public async Task<List<Player>> GetActivePlayersAsync()
        {
            // TODO: 구현 예정 (활성 유저 정의 후)
            throw new NotImplementedException();
        }

        public async Task<List<Player>> GetPlayersWithOfflineRewardsAsync()
        {
            // TODO: 구현 예정 (오프라인 보상 시스템 추가 후)
            throw new NotImplementedException();
        }

        public async Task<Player> AddAsync(Player player)
        {
            await _context.Players.AddAsync(player);
            return player;
        }

        public async Task<IEnumerable<Player>> GetAllAsync()
        {
            return await _context.Players.ToListAsync();
        }

        public async Task UpdateAsync(Player player)
        {
            _context.Players.Update(player);
            await Task.CompletedTask;
        }

        public async Task<IEnumerable<Player>> FindAsync(System.Linq.Expressions.Expression<Func<Player, bool>> predicate)
        {
            return await _context.Players.Where(predicate).ToListAsync();
        }

        public void Update(Player player)
        {
            _context.Players.Update(player);
        }

        public void Delete(Player player)
        {
            _context.Players.Remove(player);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
