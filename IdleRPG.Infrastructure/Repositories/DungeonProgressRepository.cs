using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Repositories;
using IdleRPG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace IdleRPG.Infrastructure.Repositories
{
    public class DungeonProgressRepository : IDungeonProgressRepository
    {
        private readonly GameDBContext _context;

        public DungeonProgressRepository(GameDBContext context)
        {
            _context = context;
        }

        public Task<DungeonProgress?> GetByCharacterIdAsync(Guid characterId)
        {
            // Unique Index 활용 - 한 캐릭터는 동시에 하나의 던전만 진행 가능
            return _context.DungeonProgresses
                .FirstOrDefaultAsync(p => p.CharacterId == characterId);
        }

        public Task<DungeonProgress?> GetWithDifficultyAsync(Guid characterId)
        {
            return _context.DungeonProgresses
                .Include(p => p.Difficulty)
                    .ThenInclude(d => d.Waves)
                .FirstOrDefaultAsync(p => p.CharacterId == characterId);
        }

        public void Delete(DungeonProgress progress)
        {
            _context.DungeonProgresses.Remove(progress);
        }

        // IRepository<T> 구현
        public async Task<IEnumerable<DungeonProgress>> GetAllAsync()
        {
            return await _context.DungeonProgresses.ToListAsync();
        }

        public async Task<DungeonProgress> AddAsync(DungeonProgress entity)
        {
            await _context.DungeonProgresses.AddAsync(entity);
            return entity;
        }

        public async Task UpdateAsync(DungeonProgress entity)
        {
            _context.DungeonProgresses.Update(entity);
            await Task.CompletedTask;
        }

        public async Task<IEnumerable<DungeonProgress>> FindAsync(Expression<Func<DungeonProgress, bool>> predicate)
        {
            return await _context.DungeonProgresses.Where(predicate).ToListAsync();
        }
    }
}
