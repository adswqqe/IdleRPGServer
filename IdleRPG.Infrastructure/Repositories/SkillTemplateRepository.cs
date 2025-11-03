using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Enums;
using IdleRPG.Domain.Repositories;
using IdleRPG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IdleRPG.Infrastructure.Repositories
{
    /// <summary>
    /// SkillTemplate Repository 구현
    /// 스킬 템플릿 데이터 접근 담당
    /// </summary>
    public class SkillTemplateRepository : ISkillTemplateRepository
    {
        private readonly GameDBContext _context;

        public SkillTemplateRepository(GameDBContext context)
        {
            _context = context;
        }

        public async Task<SkillTemplate?> GetByIdAsync(int id)
        {
            return await _context.SkillTemplates
                .SingleOrDefaultAsync(s => s.Id == id);
        }

        public async Task<List<SkillTemplate>> GetByRarityAsync(SkillRarity rarity)
        {
            return await _context.SkillTemplates
                .Where(s => s.Rarity == rarity)
                .OrderBy(s => s.Id)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.SkillTemplates
                .AnyAsync(s => s.Id == id);
        }

        public async Task<SkillTemplate?> GetRandomByRarityAsync(SkillRarity rarity)
        {
            var skills = await GetByRarityAsync(rarity);

            if (skills == null || !skills.Any())
                return null;

            // EF Core에서는 Random()을 직접 사용할 수 없으므로,
            // 메모리에 로드 후 랜덤 선택
            var random = new Random();
            var randomIndex = random.Next(skills.Count);
            return skills[randomIndex];
        }

        public async Task<List<SkillTemplate>> GetAllAsync()
        {
            return await _context.SkillTemplates
                .OrderBy(s => s.Rarity)
                .ThenBy(s => s.Id)
                .ToListAsync();
        }

        public async Task AddAsync(SkillTemplate entity)
        {
            await _context.SkillTemplates.AddAsync(entity);
        }

        public void Update(SkillTemplate entity)
        {
            _context.SkillTemplates.Update(entity);
        }

        public void Delete(SkillTemplate entity)
        {
            _context.SkillTemplates.Remove(entity);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
