using System.ComponentModel.DataAnnotations;

namespace IdleRPG.Domain.Entities
{
    public class Player
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(50)]
        public string UserName { get; set; }

        [Required, MaxLength(100)]
        public string PasswordHash { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastLoginAt { get; set; }
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// The amount of crystals (premium currency) the player owns.
        /// Used for gacha pulls and other premium features.
        /// </summary>
        public long Crystals { get; set; } = 0;

        //
        // Navigation Properties (Week 1에서 Character 추가 예정)
        public List<Character> Characters;
    }
}
