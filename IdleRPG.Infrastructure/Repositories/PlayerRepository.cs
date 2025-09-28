using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Repositories;
using IdleRPG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
namespace IdleRPG.Infrastructure.Repositories
{
    public class PlayerRepository : IPlayerRepository
    {
        private readonly GameDBContext _context;
        private readonly ILogger<PlayerRepository> _logger;
        
        public PlayerRepository(GameDBContext context, ILogger<PlayerRepository> logger)
        {
            _context = context;
            _logger = logger;
        }
        
        // Unity의 FindObjectOfType<Player>()와 유사
        public async Task<Player?> GetByIdAsync(Guid id)
        {
            try
            {
                return await _context.Players
                    .Include(p => p.Stats)
                    .Include(p => p.Characters.Where(c => c.IsMain)) // 메인 캐릭터만
                    .FirstOrDefaultAsync(p => p.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting player by ID: {id}");
                throw;
            }
        }
        
        // Unity의 Resources.FindObjectsOfTypeAll<Player>()와 유사
        public async Task<List<Player>> GetTopPlayersByLevelAsync(int count)
        {
            return await _context.Players
                .Include(p => p.Stats)
                .OrderByDescending(p => p.Stats.Level)
                .ThenByDescending(p => p.Stats.Experience)
                .Take(count)
                .AsNoTracking() // 읽기 전용 최적화 (Unity의 ReadOnly와 유사)
                .ToListAsync();
        }
        
        // Unity의 Instantiate()와 유사
        public async Task<IEnumerable<Player>> GetAllAsync()
        {
            try
            {
                return await _context.Players.ToListAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<Player> AddAsync(Player player)
        {
            try
            {
                _context.Players.Add(player);
                await _context.SaveChangesAsync();
            
                _logger.LogInformation($"Player created: {player.UserName} (ID: {player.Id})");
                return player;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating player: {player.UserName}");
                throw;
            }
        }
        
        // Unity의 GameObject 수정과 유사
        public async Task UpdateAsync(Player player)
        {
            try
            {
                _context.Players.Update(player);
                await _context.SaveChangesAsync();
            
                _logger.LogInformation($"Player updated: {player.UserName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating player: {player.Id}");
                throw;
            }
        }
        
        public Task<IEnumerable<Player>> FindAsync(Expression<Func<Player, bool>> predicate)
        {
            // TODO 코드 구현해야 함
            return null;
        }

        // Unity의 Destroy()와 유사
        public async Task DeleteAsync(Guid id)
        {
            var player = await GetByIdAsync(id);
            if (player != null)
            {
                _context.Players.Remove(player);
                await _context.SaveChangesAsync();
            
                _logger.LogInformation("Player deleted: {Guid}", id);
            }
        }
        
        // 도메인별 특화 메서드
        public async Task<Player?> GetByUsernameAsync(string username)
        {
            return await _context.Players
                .Include(p => p.Stats)
                .FirstOrDefaultAsync(p => p.UserName == username);
        }
    
        public async Task<bool> IsUsernameAvailableAsync(string username)
        {
            return !await _context.Players
                .AnyAsync(p => p.UserName == username);
        }

        public Task<Player?> GetPlayerWithCharactersAsync(Guid playerId)
        {
            try
            {
                return _context.Players
                    .Include(p => p.Characters)
                    .Include(p => p.Stats)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == playerId);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        // 복잡한 쿼리 - Unity의 FindObjectsOfType with condition과 유사
        public async Task<List<Player>> GetActivePlayersAsync()
        {
            var cutoffTime = DateTime.UtcNow.AddDays(-7); // 7일 이내 로그인
        
            return await _context.Players
                .Include(p => p.Stats)
                .Where(p => p.IsActive && p.LastLogin > cutoffTime)
                .OrderByDescending(p => p.LastLogin)
                .ToListAsync();
        }

        public async Task<List<Player>> GetPlayersWithOfflineRewardsAsync()
        {
            return await _context.Players
                .Where(p => _context.OfflineRewards
                           .Any(o => o.Character.PlayerId == p.Id && o.EffectiveOfflineMinutes >= 1))
                .AsNoTracking()
                .Distinct()
                .ToListAsync();
        }
    }
}