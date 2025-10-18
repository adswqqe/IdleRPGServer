using IdleRPG.Domain.Enums;

namespace IdleRPG.Domain.ValueObjects;

/// <summary>
/// A Value Object that encapsulates the multipliers for a given dungeon difficulty.
/// This object is immutable. Once created, its values cannot be changed.
/// </summary>
public sealed class DifficultyMultiplier
{
    /// <summary>
    /// The multiplier applied to monster stats (e.g., HP, Attack).
    /// </summary>
    public double MonsterStatMultiplier { get; }

    /// <summary>
    /// The multiplier applied to rewards (e.g., experience, currency).
    /// </summary>
    public double RewardMultiplier { get; }

    /// <summary>
    /// The multiplier applied to the chance of finding rare items.
    /// </summary>
    public double DropChanceMultiplier { get; }

    // The constructor is private to enforce creation through the factory method.
    private DifficultyMultiplier(double monsterStatMultiplier, double rewardMultiplier, double dropChanceMultiplier)
    {
        // In a more complex scenario, you might add validation here, e.g., ensuring multipliers are positive.
        MonsterStatMultiplier = monsterStatMultiplier;
        RewardMultiplier = rewardMultiplier;
        DropChanceMultiplier = dropChanceMultiplier;
    }

    /// <summary>
    /// Factory method to create a DifficultyMultiplier instance based on the selected difficulty.
    /// This is the single point of entry for creating this object, which centralizes the business logic.
    /// </summary>
    /// <param name="difficulty">The dungeon difficulty enum.</param>
    /// <returns>A new instance of DifficultyMultiplier with the correct values.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the difficulty is not supported.</exception>
    public static DifficultyMultiplier Create(DungeonDifficulty difficulty)
    {
        return difficulty switch
        {
            DungeonDifficulty.Normal    => new DifficultyMultiplier(1.0, 1.0, 1.0),
            DungeonDifficulty.Hard      => new DifficultyMultiplier(1.5, 1.25, 1.1),
            DungeonDifficulty.Nightmare => new DifficultyMultiplier(2.5, 1.75, 1.25),
            // This is the guard clause. It ensures that if a new enum value is added in the future,
            // this code will fail loudly, forcing the developer to define its multipliers.
            _ => throw new ArgumentOutOfRangeException(nameof(difficulty), $"Difficulty '{difficulty}' is not supported.")
        };
    }
}
