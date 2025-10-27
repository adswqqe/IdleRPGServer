using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Repositories;
using IdleRPG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IdleRPG.Infrastructure.Repositories
{
    /// <summary>
    /// 장착된 펫 Repository 구현체
    /// </summary>
    public class EquippedPetsRepository : IEquippedPetsRepository
    {
        private readonly GameDBContext _context;

        public EquippedPetsRepository(GameDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 캐릭터의 특정 슬롯에 장착된 펫을 조회합니다.
        /// </summary>
        public async Task<EquippedPets?> GetBySlotAsync(Guid characterId, int slotIndex, CancellationToken cancellationToken = default)
        {
            return await _context.Set<EquippedPets>()
                .Include(ep => ep.Pet)
                    .ThenInclude(p => p.Template)
                .FirstOrDefaultAsync(ep => ep.CharacterId == characterId && ep.SlotIndex == slotIndex, cancellationToken);
        }

        /// <summary>
        /// 특정 펫이 장착된 정보를 조회합니다. (중복 장착 체크용)
        /// </summary>
        public async Task<EquippedPets?> GetByPetIdAsync(int petId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<EquippedPets>()
                .FirstOrDefaultAsync(ep => ep.PetId == petId, cancellationToken);
        }

        /// <summary>
        /// 캐릭터가 장착한 모든 펫을 조회합니다.
        /// </summary>
        public async Task<List<EquippedPets>> GetByCharacterIdAsync(Guid characterId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<EquippedPets>()
                .Include(ep => ep.Pet)
                    .ThenInclude(p => p.Template)
                .Where(ep => ep.CharacterId == characterId)
                .OrderBy(ep => ep.SlotIndex)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// 펫을 슬롯에 장착합니다.
        /// </summary>
        public async Task AddAsync(EquippedPets equippedPet, CancellationToken cancellationToken = default)
        {
            await _context.Set<EquippedPets>().AddAsync(equippedPet, cancellationToken);
        }

        /// <summary>
        /// 장착된 펫을 해제합니다.
        /// </summary>
        public Task DeleteAsync(EquippedPets equippedPet, CancellationToken cancellationToken = default)
        {
            _context.Set<EquippedPets>().Remove(equippedPet);
            return Task.CompletedTask;
        }
    }
}
