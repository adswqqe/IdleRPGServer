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

        // TODO: 직업 시스템 추가 시 JobType enum 필드 추가
        // public JobType Job { get; set; } = JobType.Warrior;

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