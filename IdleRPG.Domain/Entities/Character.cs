using IdleRPG.Domain.ValueObjects;

namespace IdleRPG.Domain.Entities
{
    public class Character
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Foreign Key to Player
        public Guid PlayerId { get; set; }

        public int Level { get; set; } = 1;
        public int Experience { get; set; } = 0;

        public CharacterStats Stats { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Property
        public Player Player { get; set; }
    }
}