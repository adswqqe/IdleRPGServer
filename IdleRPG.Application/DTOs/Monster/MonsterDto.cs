namespace IdleRPG.Application.DTOs.Monster
{
    /// <summary>
    /// 몬스터 정보 DTO
    /// </summary>
    public class MonsterDto
    {
        /// <summary>
        /// 몬스터 ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 몬스터 이름
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 레벨
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
        /// 공격 속도 (초당 공격 횟수)
        /// </summary>
        public float AttackSpeed { get; set; }

        /// <summary>
        /// 크리티컬 확률 (0.0 ~ 1.0)
        /// </summary>
        public float CritRate { get; set; }

        /// <summary>
        /// 크리티컬 데미지 배율
        /// </summary>
        public float CritDamage { get; set; }

        /// <summary>
        /// 회피율 (0.0 ~ 1.0)
        /// </summary>
        public float Evasion { get; set; }

        /// <summary>
        /// 처치 시 획득 경험치
        /// </summary>
        public int ExperienceReward { get; set; }

        /// <summary>
        /// 처치 시 획득 골드
        /// </summary>
        public int GoldReward { get; set; }
    }

    /// <summary>
    /// 랜덤 몬스터 조회 응답 DTO
    /// 클라이언트가 몬스터 데이터를 보유하고 있으므로 ID와 Level만 전달
    /// </summary>
    public class RandomMonsterResponse
    {
        /// <summary>
        /// 선택된 몬스터 ID
        /// </summary>
        public Guid MonsterId { get; set; }

        /// <summary>
        /// 몬스터 레벨
        /// </summary>
        public int Level { get; set; }
    }
}
