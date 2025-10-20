using IdleRPG.Domain.Enums;

namespace IdleRPG.Domain.Entities
{
    /// <summary>
    /// Represents a skill template.
    /// </summary>
    public class SkillTemplate
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The name of the skill.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The rarity of the skill.
        /// </summary>
        public SkillRarity Rarity { get; set; }
    }
}