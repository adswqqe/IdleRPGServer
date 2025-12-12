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

        /// <summary>
        /// The description of the skill.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// The type of the skill (Active or Passive).
        /// </summary>
        public SkillType Type { get; set; }
    }
}