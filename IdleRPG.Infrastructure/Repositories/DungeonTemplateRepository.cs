using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Repositories;
using IdleRPG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace IdleRPG.Infrastructure.Repositories
{
    public class DungeonTemplateRepository : IDungeonTemplateRepository
    {
        private readonly GameDBContext _context;

        public DungeonTemplateRepository(GameDBContext context)
        {
            _context = context;
        }

        public Task<DungeonTemplate?> GetByIdAsync(int id)
        {
            return _context.DungeonTemplates.FindAsync(id).AsTask();
        }

        public Task<DungeonTemplate?> GetWithDifficultiesAsync(int id)
        {
            return _context.DungeonTemplates
                .Include(t => t.Difficulties)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public Task<List<DungeonTemplate>> GetAllEnabledAsync()
        {
            return _context.DungeonTemplates
                .Where(t => t.IsEnabled)
                .ToListAsync();
        }

        public Task<List<DungeonTemplate>> GetByCategoryAsync(DungeonCategory category)
        {
            return _context.DungeonTemplates
                .Where(t => t.Category == category && t.IsEnabled)
                .ToListAsync();
        }

        // IRepository<T> 구현
        public async Task<IEnumerable<DungeonTemplate>> GetAllAsync()
        {
            return await _context.DungeonTemplates.ToListAsync();
        }

        public async Task<DungeonTemplate> AddAsync(DungeonTemplate entity)
        {
            await _context.DungeonTemplates.AddAsync(entity);
            return entity;
        }

        public async Task UpdateAsync(DungeonTemplate entity)
        {
            _context.DungeonTemplates.Update(entity);
            await Task.CompletedTask;
        }

        public async Task<IEnumerable<DungeonTemplate>> FindAsync(Expression<Func<DungeonTemplate, bool>> predicate)
        {
            return await _context.DungeonTemplates.Where(predicate).ToListAsync();
        }
    }
}
