
using System;

namespace IdleRPG.Domain.Entities
{
    /// <summary>
    /// Represents a skill possessed by a character.
    /// </summary>
    public class CharacterSkill
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Foreign key for the character who owns this skill.
        /// </summary>
        public Guid CharacterId { get; set; }

        /// <summary>
        /// Foreign key for the skill template.
        /// </summary>
        public int SkillTemplateId { get; set; }

        /// <summary>
        /// Indicates whether the skill is currently equipped in a slot.
        /// </summary>
        public bool IsEquipped { get; set; }

        /// <summary>
        /// The timestamp when the skill was acquired.
        /// </summary>
        public DateTime AcquiredAt { get; set; }

        // Navigation properties

        /// <summary>
        /// Navigation property for the character.
        /// </summary>
        public Character Character { get; set; } = null!;

        /// <summary>
        /// Navigation property for the skill template.
        /// </summary>
        public SkillTemplate SkillTemplate { get; set; } = null!;
    }
}
