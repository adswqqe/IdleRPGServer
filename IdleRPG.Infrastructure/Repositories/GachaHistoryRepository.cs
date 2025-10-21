using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Repositories;
using IdleRPG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IdleRPG.Infrastructure.Repositories
{
    /// <summary>
    /// GachaHistory Repository 구현
    /// 가챠 히스토리 추적 및 분석 데이터 접근 담당
    /// </summary>
    public class GachaHistoryRepository : IGachaHistoryRepository
    {
        private readonly GameDBContext _context;

        public GachaHistoryRepository(GameDBContext context)
        {
            _context = context;
        }

        public async Task<GachaHistory?> GetByIdAsync(Guid id)
        {
            return await _context.GachaHistories
                .Include(gh => gh.SkillTemplate)
                .Include(gh => gh.Character)
                .SingleOrDefaultAsync(gh => gh.Id == id);
        }

        public async Task<List<GachaHistory>> GetByCharacterIdAsync(Guid characterId)
        {
            return await _context.GachaHistories
                .Include(gh => gh.SkillTemplate)
                .Where(gh => gh.CharacterId == characterId)
                .OrderByDescending(gh => gh.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<GachaHistory>> GetRecentByCharacterIdAsync(Guid characterId, int count)
        {
            return await _context.GachaHistories
                .Include(gh => gh.SkillTemplate)
                .Where(gh => gh.CharacterId == characterId)
                .OrderByDescending(gh => gh.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<GachaHistory>> GetByCharacterIdAndDateRangeAsync(
            Guid characterId,
            DateTime startDate,
            DateTime endDate)
        {
            return await _context.GachaHistories
                .Include(gh => gh.SkillTemplate)
                .Where(gh => gh.CharacterId == characterId
                             && gh.CreatedAt >= startDate
                             && gh.CreatedAt <= endDate)
                .OrderByDescending(gh => gh.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> CountByCharacterIdAsync(Guid characterId)
        {
            return await _context.GachaHistories
                .CountAsync(gh => gh.CharacterId == characterId);
        }

        public async Task<int> CountPityPullsByCharacterIdAsync(Guid characterId)
        {
            return await _context.GachaHistories
                .CountAsync(gh => gh.CharacterId == characterId && gh.WasPity);
        }

        public async Task<List<GachaHistory>> GetAllAsync()
        {
            return await _context.GachaHistories
                .Include(gh => gh.SkillTemplate)
                .Include(gh => gh.Character)
                .OrderByDescending(gh => gh.CreatedAt)
                .ToListAsync();
        }

        public async Task AddAsync(GachaHistory entity)
        {
            await _context.GachaHistories.AddAsync(entity);
        }

        public void Update(GachaHistory entity)
        {
            _context.GachaHistories.Update(entity);
        }

        public void Delete(GachaHistory entity)
        {
            _context.GachaHistories.Remove(entity);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
