namespace IdleRPG.Domain.Entities
{
    /// <summary>
    /// 몬스터 엔티티 - 전투 시스템에서 사용되는 적 캐릭터
    /// </summary>
    public class Monster
    {
        /// <summary>
        /// 몬스터 고유 ID
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 몬스터 이름 (예: "슬라임", "고블린")
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 몬스터 레벨
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 최대 체력
        /// </summary>
        public int MaxHealth { get; set; }

        /// <summary>
        /// 공격력
        /// </summary>
        public int Attack { get; set; }

        /// <summary>
        /// 방어력
        /// </summary>
        public int Defense { get; set; }

        /// <summary>
        /// 처치 시 획득 경험치
        /// </summary>
        public int ExperienceReward { get; set; }

        /// <summary>
        /// 처치 시 획득 골드
        /// </summary>
        public int GoldReward { get; set; }

        /// <summary>
        /// 생성 일시
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 수정 일시
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
