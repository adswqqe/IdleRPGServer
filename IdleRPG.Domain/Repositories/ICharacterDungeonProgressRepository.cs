using IdleRPG.Domain.Entities;

namespace IdleRPG.Domain.Repositories;

/// <summary>
/// Repository interface for CharacterDungeonProgress entity.
/// Tracks character progression through dungeon stages.
/// </summary>
public interface ICharacterDungeonProgressRepository : IRepository<CharacterDungeonProgress>
{
    /// <summary>
    /// Gets the dungeon progress for a specific character.
    /// Creates a new progress record if one doesn't exist.
    /// </summary>
    /// <param name="characterId">The character's ID</param>
    /// <returns>The character's dungeon progress</returns>
    Task<CharacterDungeonProgress> GetOrCreateByCharacterIdAsync(Guid characterId);
}
