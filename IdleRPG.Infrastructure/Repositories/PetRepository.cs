using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Repositories;
using IdleRPG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IdleRPG.Infrastructure.Repositories
{
    /// <summary>
    /// 펫 리포지토리 구현체
    /// </summary>
    public class PetRepository : IPetRepository
    {
        private readonly GameDBContext _context;

        public PetRepository(GameDBContext context)
        {
            _context = context;
        }

        public Task<Pet?> GetByIdAsync(int petId, CancellationToken cancellationToken = default)
        {
            return _context.Set<Pet>()
                .Include(p => p.PetTemplate)
                .Include(p => p.Character)
                .SingleOrDefaultAsync(p => p.Id == petId, cancellationToken);
        }

        public Task<List<Pet>> GetByCharacterIdAsync(Guid characterId, CancellationToken cancellationToken = default)
        {
            return _context.Set<Pet>()
                .Include(p => p.PetTemplate)
                .Where(p => p.CharacterId == characterId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public Task<Pet?> GetDuplicateAsync(Guid characterId, int templateId, CancellationToken cancellationToken = default)
        {
            return _context.Set<Pet>()
                .FirstOrDefaultAsync(p => p.CharacterId == characterId && p.TemplateId == templateId, cancellationToken);
        }

        public async Task AddAsync(Pet pet, CancellationToken cancellationToken = default)
        {
            await _context.Set<Pet>().AddAsync(pet, cancellationToken);
        }

        public Task UpdateAsync(Pet pet, CancellationToken cancellationToken = default)
        {
            _context.Set<Pet>().Update(pet);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Pet pet, CancellationToken cancellationToken = default)
        {
            _context.Set<Pet>().Remove(pet);
            return Task.CompletedTask;
        }
    }
}
