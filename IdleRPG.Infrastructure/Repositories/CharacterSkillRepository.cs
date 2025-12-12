using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Enums;
using IdleRPG.Domain.Repositories;
using IdleRPG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IdleRPG.Infrastructure.Repositories
{
    /// <summary>
    /// CharacterSkill Repository 구현
    /// 캐릭터 스킬 인벤토리 데이터 접근 담당
    /// </summary>
    public class CharacterSkillRepository : ICharacterSkillRepository
    {
        private readonly GameDBContext _context;

        public CharacterSkillRepository(GameDBContext context)
        {
            _context = context;
        }

        public async Task<CharacterSkill?> GetByIdAsync(Guid id)
        {
            return await _context.CharacterSkills
                .Include(cs => cs.SkillTemplate)
                .SingleOrDefaultAsync(cs => cs.Id == id);
        }

        public async Task<List<CharacterSkill>> GetByCharacterIdAsync(Guid characterId)
        {
            return await _context.CharacterSkills
                .Include(cs => cs.SkillTemplate)
                .Where(cs => cs.CharacterId == characterId)
                .OrderByDescending(cs => cs.SkillTemplate.Rarity) // 등급 높은 순
                .ThenByDescending(cs => cs.AcquiredAt) // 최신 획득 순
                .ToListAsync();
        }

        public async Task<List<CharacterSkill>> GetEquippedByCharacterIdAsync(Guid characterId)
        {
            return await _context.CharacterSkills
                .Include(cs => cs.SkillTemplate)
                .Where(cs => cs.CharacterId == characterId && cs.IsEquipped)
                .OrderBy(cs => cs.AcquiredAt)
                .ToListAsync();
        }

        public async Task<List<CharacterSkill>> GetByCharacterIdAndRarityAsync(
            Guid characterId,
            SkillRarity rarity)
        {
            return await _context.CharacterSkills
                .Include(cs => cs.SkillTemplate)
                .Where(cs => cs.CharacterId == characterId && cs.SkillTemplate.Rarity == rarity)
                .OrderByDescending(cs => cs.AcquiredAt)
                .ToListAsync();
        }

        public async Task<bool> HasSkillAsync(Guid characterId, int skillTemplateId)
        {
            return await _context.CharacterSkills
                .AnyAsync(cs => cs.CharacterId == characterId && cs.SkillTemplateId == skillTemplateId);
        }

        public async Task<int> CountByCharacterIdAsync(Guid characterId)
        {
            return await _context.CharacterSkills
                .CountAsync(cs => cs.CharacterId == characterId);
        }

        public async Task<List<CharacterSkill>> GetAllAsync()
        {
            return await _context.CharacterSkills
                .Include(cs => cs.SkillTemplate)
                .Include(cs => cs.Character)
                .ToListAsync();
        }

        public async Task AddAsync(CharacterSkill entity)
        {
            await _context.CharacterSkills.AddAsync(entity);
        }

        public void Update(CharacterSkill entity)
        {
            _context.CharacterSkills.Update(entity);
        }

        public void Delete(CharacterSkill entity)
        {
            _context.CharacterSkills.Remove(entity);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
