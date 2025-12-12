using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Repositories;
using IdleRPG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IdleRPG.Infrastructure.Repositories
{
    /// <summary>
    /// 전투 로그 Repository 구현
    /// </summary>
    public class BattleLogRepository : IBattleLogRepository
    {
        private readonly GameDBContext _context;

        public BattleLogRepository(GameDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 캐릭터별 전투 히스토리 조회 (페이징)
        /// </summary>
        public async Task<List<BattleLog>> GetByCharacterIdAsync(Guid characterId, int page, int pageSize)
        {
            return await _context.BattleLogs
                .Include(bl => bl.Monster) // 몬스터 정보 포함
                .Where(bl => bl.CharacterId == characterId)
                .OrderByDescending(bl => bl.BattleDate) // 최신순 정렬
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// 캐릭터의 최근 N개 전투 로그 조회
        /// </summary>
        public async Task<List<BattleLog>> GetRecentByCharacterIdAsync(Guid characterId, int count)
        {
            return await _context.BattleLogs
                .Include(bl => bl.Monster) // 몬스터 정보 포함
                .Where(bl => bl.CharacterId == characterId)
                .OrderByDescending(bl => bl.BattleDate) // 최신순 정렬
                .Take(count)
                .ToListAsync();
        }

        /// <summary>
        /// 캐릭터의 전투 통계 조회
        /// </summary>
        public async Task<BattleStatistics> GetStatsByCharacterIdAsync(Guid characterId)
        {
            var logs = await _context.BattleLogs
                .Where(bl => bl.CharacterId == characterId)
                .ToListAsync();

            if (!logs.Any())
            {
                return new BattleStatistics();
            }

            return new BattleStatistics
            {
                TotalBattles = logs.Count,
                Victories = logs.Count(bl => bl.IsVictory),
                Defeats = logs.Count(bl => !bl.IsVictory),
                TotalExperience = logs.Sum(bl => bl.ExperienceGained),
                TotalGold = logs.Sum(bl => bl.GoldGained),
                TotalDamageDealt = logs.Sum(bl => bl.DamageDealt),
                TotalDamageTaken = logs.Sum(bl => bl.DamageTaken)
            };
        }

        // IRepository<BattleLog> 인터페이스 구현
        public async Task<IEnumerable<BattleLog>> GetAllAsync()
        {
            return await _context.BattleLogs
                .Include(bl => bl.Character)
                .Include(bl => bl.Monster)
                .ToListAsync();
        }

        public async Task<BattleLog> AddAsync(BattleLog entity)
        {
            await _context.BattleLogs.AddAsync(entity);
            return entity;
        }

        public async Task UpdateAsync(BattleLog entity)
        {
            _context.BattleLogs.Update(entity);
            await Task.CompletedTask;
        }

        public async Task<IEnumerable<BattleLog>> FindAsync(System.Linq.Expressions.Expression<Func<BattleLog, bool>> predicate)
        {
            return await _context.BattleLogs
                .Include(bl => bl.Character)
                .Include(bl => bl.Monster)
                .Where(predicate)
                .ToListAsync();
        }
    }
}
