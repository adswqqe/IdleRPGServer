using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Enums;
using IdleRPG.Domain.Repositories;
using IdleRPG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace IdleRPG.Infrastructure.Repositories
{
    /// <summary>
    /// Equipment Repository 구현
    /// </summary>
    public class EquipmentRepository : IEquipmentRepository
    {
        private readonly GameDBContext _context;

        public EquipmentRepository(GameDBContext context)
        {
            _context = context;
        }

        public Task<Equipment?> GetByIdAsync(Guid id)
        {
            return _context.Equipments.SingleOrDefaultAsync(e => e.Id == id);
        }

        public Task<List<Equipment>> GetEquippedByCharacterIdAsync(Guid characterId)
        {
            return _context.Equipments
                .Where(e => e.CharacterId == characterId)
                .OrderBy(e => e.Slot) // 슬롯 순서대로 정렬 (Weapon, Helmet, Armor, Gloves, Boots)
                .ToListAsync();
        }

        public Task<Equipment?> GetEquippedBySlotAsync(Guid characterId, EquipmentSlot slot)
        {
            return _context.Equipments
                .SingleOrDefaultAsync(e => e.CharacterId == characterId && e.Slot == slot);
        }

        public Task<List<Equipment>> GetInventoryByOwnerIdAsync(Guid ownerId)
        {
            // 소유자가 ownerId이고, 장착되지 않은 (CharacterId가 NULL) 장비 조회
            return _context.Equipments
                .Where(e => e.OwnerId == ownerId && e.CharacterId == null)
                .OrderByDescending(e => e.Rarity) // 등급 높은 순
                .ThenByDescending(e => e.CreatedAt) // 최신 획득 순
                .ToListAsync();
        }

        public Task<List<Equipment>> GetByRarityAsync(EquipmentRarity rarity)
        {
            return _context.Equipments
                .Where(e => e.Rarity == rarity)
                .ToListAsync();
        }

        public Task<bool> ExistsAsync(Guid id)
        {
            return _context.Equipments.AnyAsync(e => e.Id == id);
        }

        public async Task<Equipment> AddAsync(Equipment equipment)
        {
            await _context.Equipments.AddAsync(equipment);
            return equipment;
        }

        public async Task UpdateAsync(Equipment equipment)
        {
            _context.Equipments.Update(equipment);
            await Task.CompletedTask;
        }

        public void Delete(Equipment equipment)
        {
            _context.Equipments.Remove(equipment);
        }

        public async Task<IEnumerable<Equipment>> GetAllAsync()
        {
            return await _context.Equipments.ToListAsync();
        }

        public async Task<IEnumerable<Equipment>> FindAsync(Expression<Func<Equipment, bool>> predicate)
        {
            return await _context.Equipments.Where(predicate).ToListAsync();
        }
    }
}
