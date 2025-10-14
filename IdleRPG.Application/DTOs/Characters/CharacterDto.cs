namespace IdleRPG.Application.DTOs.Characters
{
    /// <summary>
    /// 캐릭터 응답 DTO (자동 성장 방식)
    /// </summary>
    public class CharacterDto
    {
        public Guid Id { get; set; }
        public Guid PlayerId { get; set; }
        public int Level { get; set; }
        public int Experience { get; set; }
        public long Gold { get; set; }
        public DateTime LastLoginTime { get; set; }

        // 전투 스탯 (레벨업 시 자동 증가)
        public long Attack { get; set; }
        public long Defense { get; set; }
        public long MaxHealth { get; set; }
        public float CritRate { get; set; }
        public float CritDamage { get; set; }
        public float Evasion { get; set; }
        public float AttackSpeed { get; set; }  // 공격 속도

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}