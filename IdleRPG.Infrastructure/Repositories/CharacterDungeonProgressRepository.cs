using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Repositories;
using IdleRPG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace IdleRPG.Infrastructure.Repositories;

/// <summary>
/// CharacterDungeonProgress Repository implementation.
/// Tracks character progression through dungeon stages.
/// </summary>
public class CharacterDungeonProgressRepository : ICharacterDungeonProgressRepository
{
    private readonly GameDBContext _context;

    public CharacterDungeonProgressRepository(GameDBContext context)
    {
        _context = context;
    }

    // ICharacterDungeonProgressRepository specific methods

    /// <summary>
    /// Gets the dungeon progress for a specific character.
    /// Creates a new progress record if one doesn't exist.
    /// Uses unique CharacterId index for efficient lookup.
    /// </summary>
    public async Task<CharacterDungeonProgress> GetOrCreateByCharacterIdAsync(Guid characterId)
    {
        var progress = await _context.CharacterDungeonProgresses
            .SingleOrDefaultAsync(p => p.CharacterId == characterId);

        if (progress == null)
        {
            progress = new CharacterDungeonProgress
            {
                Id = Guid.NewGuid(),
                CharacterId = characterId,
                HighestStageClearedNormal = 0,
                HighestStageClearedHard = 0,
                HighestStageClearedNightmare = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.CharacterDungeonProgresses.AddAsync(progress);
        }

        return progress;
    }

    // IRepository<CharacterDungeonProgress> interface implementation

    public async Task<IEnumerable<CharacterDungeonProgress>> GetAllAsync()
    {
        return await _context.CharacterDungeonProgresses.ToListAsync();
    }

    public async Task<CharacterDungeonProgress> AddAsync(CharacterDungeonProgress entity)
    {
        await _context.CharacterDungeonProgresses.AddAsync(entity);
        return entity;
    }

    public async Task UpdateAsync(CharacterDungeonProgress entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.CharacterDungeonProgresses.Update(entity);
        await Task.CompletedTask;
    }

    public async Task<IEnumerable<CharacterDungeonProgress>> FindAsync(Expression<Func<CharacterDungeonProgress, bool>> predicate)
    {
        return await _context.CharacterDungeonProgresses.Where(predicate).ToListAsync();
    }
}
