using IdleRPG.Domain.Entities;

namespace IdleRPG.Domain.Entities;

/// <summary>
/// Represents a dungeon stage with static configuration data.
/// Each stage has a boss monster, level requirements, and reward values.
/// </summary>
public class DungeonStage
{
    /// <summary>
    /// Stage number (Primary Key). Stage 1, 2, 3, etc.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Display name of the dungeon stage.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Minimum character level required to enter this stage.
    /// </summary>
    public int RequiredLevel { get; set; }

    /// <summary>
    /// The boss monster ID for this stage.
    /// </summary>
    public Guid MonsterId { get; set; }

    /// <summary>
    /// Navigation property to the boss monster.
    /// </summary>
    public Monster? Monster { get; set; }

    /// <summary>
    /// Base experience reward for clearing this stage.
    /// Will be multiplied by difficulty multiplier.
    /// </summary>
    public int BaseExperience { get; set; }

    /// <summary>
    /// Base gold reward for clearing this stage.
    /// Will be multiplied by difficulty multiplier.
    /// </summary>
    public int BaseGold { get; set; }

    /// <summary>
    /// Bonus experience granted on first clear of this stage (any difficulty).
    /// Nullable - not all stages may have first clear bonuses.
    /// </summary>
    public int? FirstClearBonusExp { get; set; }

    /// <summary>
    /// Bonus gold granted on first clear of this stage (any difficulty).
    /// Nullable - not all stages may have first clear bonuses.
    /// </summary>
    public int? FirstClearBonusGold { get; set; }

    /// <summary>
    /// When this stage was created in the database.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
