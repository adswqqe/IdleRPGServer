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
        public int StatPoints { get; set; } = 0;

        /// <summary>
        /// 캐릭터 보유 골드
        /// </summary>
        public long Gold { get; set; } = 0;

        /// <summary>
        /// 마지막 로그인 시간 (오프라인 보상 계산 기준)
        /// </summary>
        public DateTime LastLoginTime { get; set; } = DateTime.UtcNow;

        public CharacterStats Stats { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Property
        public Player Player { get; set; }
    }
}