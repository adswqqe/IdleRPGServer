using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Enums;

namespace IdleRPG.Domain.Repositories
{
    /// <summary>
    /// Repository interface for SkillTemplate entity.
    /// Handles skill template data access operations.
    /// </summary>
    public interface ISkillTemplateRepository : IRepository<SkillTemplate>
    {
        /// <summary>
        /// Gets a skill template by its ID.
        /// </summary>
        /// <param name="id">The skill template ID.</param>
        /// <returns>The skill template if found; otherwise, null.</returns>
        Task<SkillTemplate?> GetByIdAsync(int id);

        /// <summary>
        /// Gets all skill templates with the specified rarity.
        /// </summary>
        /// <param name="rarity">The skill rarity to filter by.</param>
        /// <returns>A list of skill templates matching the rarity.</returns>
        Task<List<SkillTemplate>> GetByRarityAsync(SkillRarity rarity);

        /// <summary>
        /// Checks if a skill template exists with the given ID.
        /// </summary>
        /// <param name="id">The skill template ID.</param>
        /// <returns>True if the skill template exists; otherwise, false.</returns>
        Task<bool> ExistsAsync(int id);

        /// <summary>
        /// Gets a random skill template from the specified rarity pool.
        /// This is useful for gacha operations.
        /// </summary>
        /// <param name="rarity">The rarity pool to select from.</param>
        /// <returns>A random skill template from the specified rarity.</returns>
        Task<SkillTemplate?> GetRandomByRarityAsync(SkillRarity rarity);
    }
}
