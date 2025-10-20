using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Enums;

namespace IdleRPG.Domain.Repositories
{
    /// <summary>
    /// Repository interface for CharacterSkill entity.
    /// Handles character skill inventory operations.
    /// </summary>
    public interface ICharacterSkillRepository : IRepository<CharacterSkill>
    {
        /// <summary>
        /// Gets a character skill by its ID.
        /// </summary>
        /// <param name="id">The character skill ID.</param>
        /// <returns>The character skill if found; otherwise, null.</returns>
        Task<CharacterSkill?> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets all skills owned by a specific character.
        /// </summary>
        /// <param name="characterId">The character ID.</param>
        /// <returns>A list of skills owned by the character.</returns>
        Task<List<CharacterSkill>> GetByCharacterIdAsync(Guid characterId);

        /// <summary>
        /// Gets all equipped skills for a specific character.
        /// </summary>
        /// <param name="characterId">The character ID.</param>
        /// <returns>A list of equipped skills.</returns>
        Task<List<CharacterSkill>> GetEquippedByCharacterIdAsync(Guid characterId);

        /// <summary>
        /// Gets skills of a specific rarity owned by a character.
        /// </summary>
        /// <param name="characterId">The character ID.</param>
        /// <param name="rarity">The skill rarity to filter by.</param>
        /// <returns>A list of character skills matching the rarity.</returns>
        Task<List<CharacterSkill>> GetByCharacterIdAndRarityAsync(Guid characterId, SkillRarity rarity);

        /// <summary>
        /// Checks if a character already owns a specific skill template.
        /// </summary>
        /// <param name="characterId">The character ID.</param>
        /// <param name="skillTemplateId">The skill template ID.</param>
        /// <returns>True if the character owns the skill; otherwise, false.</returns>
        Task<bool> HasSkillAsync(Guid characterId, int skillTemplateId);

        /// <summary>
        /// Counts the total number of skills owned by a character.
        /// </summary>
        /// <param name="characterId">The character ID.</param>
        /// <returns>The total count of skills.</returns>
        Task<int> CountByCharacterIdAsync(Guid characterId);

        /// <summary>
        /// Deletes a character skill.
        /// </summary>
        /// <param name="characterSkill">The character skill to delete.</param>
        void Delete(CharacterSkill characterSkill);
    }
}
