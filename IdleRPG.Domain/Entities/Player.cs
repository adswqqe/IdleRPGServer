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

        // Navigation Properties (Week 1에서 Character 추가 예정)
        public List<Character> Characters;
    }
}
