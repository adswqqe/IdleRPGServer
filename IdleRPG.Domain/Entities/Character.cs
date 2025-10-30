using IdleRPG.Domain.Repositories;
using IdleRPG.Domain.ValueObjects;

namespace IdleRPG.Domain.Entities
{
    /// <summary>
    /// 캐릭터 Entity
    /// </summary>
    public class Character : BaseEntity
    {
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

        public long Crystal { get; set; } = 0;

        /// <summary>
        /// 스킬 가챠 천장 카운터 (100회)
        /// </summary>
        public int GachaPityCount { get; set; } = 0;

        /// <summary>
        /// 펫 가챠 천장 카운터 (50회)
        /// </summary>
        public int PetGachaCount { get; set; } = 0;

        /// <summary>
        /// 마지막 로그인 시간 (오프라인 보상 계산 기준)
        /// </summary>
        public DateTime LastLoginTime { get; set; } = DateTime.UtcNow;

        public CharacterStats Stats { get; set; }

        /// <summary>
        /// 마지막 업데이트 시간 (UTC)
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Property
        public Player Player { get; set; }
        public ICollection<CharacterSkill> Skills { get; set; } = new List<CharacterSkill>();
    }
}