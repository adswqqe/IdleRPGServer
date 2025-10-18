namespace IdleRPG.Domain.Entities;

/// <summary>
/// Tracks a character's dungeon progression across all difficulty levels.
/// Uses three separate fields to track the highest cleared stage for each difficulty.
/// This is Gemini's "three-field" enhancement - simple yet flexible.
/// </summary>
public class CharacterDungeonProgress
{
    /// <summary>
    /// Primary key.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Character who owns this progress record.
    /// </summary>
    public Guid CharacterId { get; set; }

    /// <summary>
    /// Navigation property to the character.
    /// </summary>
    public Character? Character { get; set; }

    /// <summary>
    /// Highest stage cleared on Normal difficulty.
    /// 0 means no stages cleared yet.
    /// </summary>
    public int HighestStageClearedNormal { get; set; }

    /// <summary>
    /// Highest stage cleared on Hard difficulty.
    /// 0 means no stages cleared yet.
    /// </summary>
    public int HighestStageClearedHard { get; set; }

    /// <summary>
    /// Highest stage cleared on Nightmare difficulty.
    /// 0 means no stages cleared yet.
    /// </summary>
    public int HighestStageClearedNightmare { get; set; }

    /// <summary>
    /// When this progress record was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When this progress record was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
