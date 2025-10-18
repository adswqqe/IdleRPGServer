using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Repositories;
using IdleRPG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace IdleRPG.Infrastructure.Repositories;

/// <summary>
/// DungeonStage Repository implementation.
/// Provides data access methods for dungeon stage information.
/// </summary>
public class DungeonStageRepository : IDungeonStageRepository
{
    private readonly GameDBContext _context;

    public DungeonStageRepository(GameDBContext context)
    {
        _context = context;
    }

    // IDungeonStageRepository specific methods

    /// <summary>
    /// Gets all dungeon stages accessible by a character of the specified level.
    /// Uses RequiredLevel index for efficient filtering.
    /// </summary>
    public async Task<List<DungeonStage>> GetAccessibleStagesAsync(int characterLevel)
    {
        return await _context.DungeonStages
            .Where(d => d.RequiredLevel <= characterLevel)
            .OrderBy(d => d.Id)
            .ToListAsync();
    }

    /// <summary>
    /// Gets a dungeon stage by its ID, including the Monster navigation property.
    /// </summary>
    public async Task<DungeonStage?> GetByIdWithMonsterAsync(int stageId)
    {
        return await _context.DungeonStages
            .Include(d => d.Monster)
            .SingleOrDefaultAsync(d => d.Id == stageId);
    }

    // IRepository<DungeonStage> interface implementation

    public async Task<IEnumerable<DungeonStage>> GetAllAsync()
    {
        return await _context.DungeonStages.ToListAsync();
    }

    public async Task<DungeonStage> AddAsync(DungeonStage entity)
    {
        await _context.DungeonStages.AddAsync(entity);
        return entity;
    }

    public async Task UpdateAsync(DungeonStage entity)
    {
        _context.DungeonStages.Update(entity);
        await Task.CompletedTask;
    }

    public async Task<IEnumerable<DungeonStage>> FindAsync(Expression<Func<DungeonStage, bool>> predicate)
    {
        return await _context.DungeonStages.Where(predicate).ToListAsync();
    }
}
