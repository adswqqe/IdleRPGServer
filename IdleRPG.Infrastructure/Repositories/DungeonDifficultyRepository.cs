using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Repositories;
using IdleRPG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace IdleRPG.Infrastructure.Repositories
{
    public class DungeonDifficultyRepository : IDungeonDifficultyRepository
    {
        private readonly GameDBContext _context;

        public DungeonDifficultyRepository(GameDBContext context)
        {
            _context = context;
        }

        public Task<DungeonDifficulty?> GetByIdAsync(int id)
        {
            return _context.DungeonDifficulties.FindAsync(id).AsTask();
        }

        public Task<DungeonDifficulty?> GetWithWavesAsync(int id)
        {
            return _context.DungeonDifficulties
                .Include(d => d.Waves)
                    .ThenInclude(w => w.Monster)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public Task<List<DungeonDifficulty>> GetByTemplateIdAsync(int templateId)
        {
            return _context.DungeonDifficulties
                .Where(d => d.TemplateId == templateId)
                .ToListAsync();
        }

        public Task<DungeonDifficulty?> GetByTemplateAndCodeAsync(int templateId, DifficultyCode code)
        {
            return _context.DungeonDifficulties
                .FirstOrDefaultAsync(d => d.TemplateId == templateId && d.Code == code);
        }

        // IRepository<T> 구현
        public async Task<IEnumerable<DungeonDifficulty>> GetAllAsync()
        {
            return await _context.DungeonDifficulties.ToListAsync();
        }

        public async Task<DungeonDifficulty> AddAsync(DungeonDifficulty entity)
        {
            await _context.DungeonDifficulties.AddAsync(entity);
            return entity;
        }

        public async Task UpdateAsync(DungeonDifficulty entity)
        {
            _context.DungeonDifficulties.Update(entity);
            await Task.CompletedTask;
        }

        public async Task<IEnumerable<DungeonDifficulty>> FindAsync(Expression<Func<DungeonDifficulty, bool>> predicate)
        {
            return await _context.DungeonDifficulties.Where(predicate).ToListAsync();
        }
    }
}
