using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Repositories;
using IdleRPG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace IdleRPG.Infrastructure.Repositories
{
    public class DungeonRunHistoryRepository : IDungeonRunHistoryRepository
    {
        private readonly GameDBContext _context;

        public DungeonRunHistoryRepository(GameDBContext context)
        {
            _context = context;
        }

        public Task<List<DungeonRunHistory>> GetRecentByCharacterIdAsync(Guid characterId, int limit = 10)
        {
            // 복합 인덱스 (CharacterId, CompletedAt) 활용
            return _context.DungeonRunHistories
                .Where(h => h.CharacterId == characterId)
                .OrderByDescending(h => h.CompletedAt)
                .Take(limit)
                .ToListAsync();
        }

        public Task<List<DungeonRunHistory>> GetTopClearedAsync(int difficultyId, int limit = 10)
        {
            return _context.DungeonRunHistories
                .Where(h => h.DifficultyId == difficultyId && h.IsCleared)
                .OrderByDescending(h => h.ClearedWave)
                .ThenBy(h => h.CompletedAt)
                .Take(limit)
                .ToListAsync();
        }

        public Task<bool> HasClearedAsync(Guid characterId, int difficultyId)
        {
            return _context.DungeonRunHistories
                .AnyAsync(h => h.CharacterId == characterId
                            && h.DifficultyId == difficultyId
                            && h.IsCleared);
        }

        // IRepository<T> 구현
        public async Task<IEnumerable<DungeonRunHistory>> GetAllAsync()
        {
            return await _context.DungeonRunHistories.ToListAsync();
        }

        public async Task<DungeonRunHistory> AddAsync(DungeonRunHistory entity)
        {
            await _context.DungeonRunHistories.AddAsync(entity);
            return entity;
        }

        public async Task UpdateAsync(DungeonRunHistory entity)
        {
            _context.DungeonRunHistories.Update(entity);
            await Task.CompletedTask;
        }

        public async Task<IEnumerable<DungeonRunHistory>> FindAsync(Expression<Func<DungeonRunHistory, bool>> predicate)
        {
            return await _context.DungeonRunHistories.Where(predicate).ToListAsync();
        }
    }
}
