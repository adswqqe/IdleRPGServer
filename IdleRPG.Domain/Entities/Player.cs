using IdleRPG.Domain.Repositories;
using System.ComponentModel.DataAnnotations;
namespace IdleRPG.Domain.Entities
{
    public class Player
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(50)]
        public string UserName { get; set; }

        [Required, EmailAddress, MaxLength(50)]
        public string Email { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime LastLogin { get; set; }
        public bool IsActive { get; set; }
        
        public int CurrentStage { get; set; }
        public long TotalIdleTime { get; set; }

        // Navigation Properties
        public PlayerStats Stats { get; set; }
        // 🆕 추가: Player가 여러 Character를 가질 수 있음 (1:N 관계)
        public List<Character> Characters { get; set; }

        public TimeSpan GetOfflineTime()
        {
            return DateTime.UtcNow - LastLogin;
        }
    }
}