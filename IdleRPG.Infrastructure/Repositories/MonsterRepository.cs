using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Repositories;
using IdleRPG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IdleRPG.Infrastructure.Repositories
{
    /// <summary>
    /// 몬스터 Repository 구현
    /// </summary>
    public class MonsterRepository : IMonsterRepository
    {
        private readonly GameDBContext _context;

        public MonsterRepository(GameDBContext context)
        {
            _context = context;
        }

        public async Task<Monster?> GetByIdAsync(Guid id)
        {
            return await _context.Monsters.SingleOrDefaultAsync(m => m.Id == id);
        }

        public async Task<List<Monster>> GetByLevelRangeAsync(int minLevel, int maxLevel)
        {
            return await _context.Monsters
                .Where(m => m.Level >= minLevel && m.Level <= maxLevel)
                .ToListAsync();
        }

        public async Task<List<Monster>> GetAllAsync()
        {
            return await _context.Monsters.ToListAsync();
        }

        public async Task<Monster> CreateAsync(Monster monster)
        {
            await _context.Monsters.AddAsync(monster);
            return monster;
        }

        // IRepository<Monster> 인터페이스 구현
        public async Task<Monster> AddAsync(Monster entity)
        {
            await _context.Monsters.AddAsync(entity);
            return entity;
        }

        public async Task<IEnumerable<Monster>> FindAsync(System.Linq.Expressions.Expression<Func<Monster, bool>> predicate)
        {
            return await _context.Monsters.Where(predicate).ToListAsync();
        }

        async Task<IEnumerable<Monster>> IRepository<Monster>.GetAllAsync()
        {
            return await _context.Monsters.ToListAsync();
        }

        public async Task UpdateAsync(Monster entity)
        {
            _context.Monsters.Update(entity);
            await Task.CompletedTask;
        }

        public void Update(Monster entity)
        {
            _context.Monsters.Update(entity);
        }

        public void Delete(Monster entity)
        {
            _context.Monsters.Remove(entity);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
