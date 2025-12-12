using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Enums;
using IdleRPG.Domain.Repositories;
using IdleRPG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IdleRPG.Infrastructure.Repositories
{
    /// <summary>
    /// 펫 템플릿 리포지토리 구현체
    /// </summary>
    public class PetTemplateRepository : IPetTemplateRepository
    {
        private readonly GameDBContext _context;

        public PetTemplateRepository(GameDBContext context)
        {
            _context = context;
        }

        public Task<List<PetTemplate>> GetByRarityAsync(Rarity rarity, CancellationToken cancellationToken = default)
        {
            return _context.Set<PetTemplate>()
                .Where(pt => pt.Rarity == rarity)
                .OrderBy(pt => pt.Id)
                .ToListAsync(cancellationToken);
        }

        public Task<PetTemplate?> GetByIdAsync(int templateId, CancellationToken cancellationToken = default)
        {
            return _context.Set<PetTemplate>()
                .SingleOrDefaultAsync(pt => pt.Id == templateId, cancellationToken);
        }

        public Task<List<PetTemplate>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return _context.Set<PetTemplate>()
                .OrderBy(pt => pt.Rarity)
                .ThenBy(pt => pt.Id)
                .ToListAsync(cancellationToken);
        }
    }
}
