using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Repositories;
using IdleRPG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace IdleRPG.Infrastructure.Repositories
{
    public class LootTableRepository : ILootTableRepository
    {
        private readonly GameDBContext _context;

        public LootTableRepository(GameDBContext context)
        {
            _context = context;
        }

        public Task<LootTable?> GetByIdAsync(int id)
        {
            return _context.LootTables.FindAsync(id).AsTask();
        }

        public Task<LootTable?> GetWithItemsAsync(int id)
        {
            // 보상 계산을 위해 LootItem들을 함께 로드
            return _context.LootTables
                .Include(t => t.Items)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        // IRepository<T> 구현
        public async Task<IEnumerable<LootTable>> GetAllAsync()
        {
            return await _context.LootTables.ToListAsync();
        }

        public async Task<LootTable> AddAsync(LootTable entity)
        {
            await _context.LootTables.AddAsync(entity);
            return entity;
        }

        public async Task UpdateAsync(LootTable entity)
        {
            _context.LootTables.Update(entity);
            await Task.CompletedTask;
        }

        public async Task<IEnumerable<LootTable>> FindAsync(Expression<Func<LootTable, bool>> predicate)
        {
            return await _context.LootTables.Where(predicate).ToListAsync();
        }
    }
}
