using IdleRPG.Domain.Entities;

namespace IdleRPG.Domain.Repositories;

/// <summary>
/// Repository interface for DungeonStage entity.
/// Provides data access methods for dungeon stage information.
/// </summary>
public interface IDungeonStageRepository : IRepository<DungeonStage>
{
    /// <summary>
    /// Gets all dungeon stages accessible by a character of the specified level.
    /// </summary>
    /// <param name="characterLevel">The character's current level</param>
    /// <returns>List of accessible dungeon stages</returns>
    Task<List<DungeonStage>> GetAccessibleStagesAsync(int characterLevel);

    /// <summary>
    /// Gets a dungeon stage by its ID, including the Monster navigation property.
    /// </summary>
    /// <param name="stageId">The stage number</param>
    /// <returns>The dungeon stage with monster data, or null if not found</returns>
    Task<DungeonStage?> GetByIdWithMonsterAsync(int stageId);
}
