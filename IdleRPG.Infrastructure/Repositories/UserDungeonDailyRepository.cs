using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Repositories;
using IdleRPG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace IdleRPG.Infrastructure.Repositories
{
    public class UserDungeonDailyRepository : IUserDungeonDailyRepository
    {
        private readonly GameDBContext _context;

        public UserDungeonDailyRepository(GameDBContext context)
        {
            _context = context;
        }

        public Task<UserDungeonDaily?> GetTodayEntryAsync(Guid userId, int dungeonTemplateId, DifficultyCode code)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            // Unique Index (UserId, DungeonTemplateId, DifficultyCode, Date) 활용
            return _context.UserDungeonDailies
                .FirstOrDefaultAsync(d => d.UserId == userId
                                       && d.DungeonTemplateId == dungeonTemplateId
                                       && d.DifficultyCode == code
                                       && d.Date == today);
        }

        public void Update(UserDungeonDaily daily)
        {
            _context.UserDungeonDailies.Update(daily);
        }

        // IRepository<T> 구현
        public async Task<IEnumerable<UserDungeonDaily>> GetAllAsync()
        {
            return await _context.UserDungeonDailies.ToListAsync();
        }

        public async Task<UserDungeonDaily> AddAsync(UserDungeonDaily entity)
        {
            await _context.UserDungeonDailies.AddAsync(entity);
            return entity;
        }

        public async Task UpdateAsync(UserDungeonDaily entity)
        {
            _context.UserDungeonDailies.Update(entity);
            await Task.CompletedTask;
        }

        public async Task<IEnumerable<UserDungeonDaily>> FindAsync(Expression<Func<UserDungeonDaily, bool>> predicate)
        {
            return await _context.UserDungeonDailies.Where(predicate).ToListAsync();
        }
    }
}
