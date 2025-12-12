namespace IdleRPG.Application.DTOs.Characters
{
    /// <summary>
    /// 캐릭터 전투 스탯 DTO
    /// CharacterDto의 중첩 객체로 사용되어 스탯 정보를 구조화
    /// </summary>
    public class CharacterStatsDto
    {
        /// <summary>
        /// 공격력
        /// </summary>
        public long Attack { get; set; }

        /// <summary>
        /// 방어력
        /// </summary>
        public long Defense { get; set; }

        /// <summary>
        /// 최대 체력
        /// </summary>
        public long MaxHealth { get; set; }

        /// <summary>
        /// 크리티컬 확률 (0.0 ~ 1.0)
        /// </summary>
        public float CritRate { get; set; }

        /// <summary>
        /// 크리티컬 데미지 배율 (예: 1.5 = 150%)
        /// </summary>
        public float CritDamage { get; set; }

        /// <summary>
        /// 회피율 (0.0 ~ 1.0)
        /// </summary>
        public float Evasion { get; set; }

        /// <summary>
        /// 공격 속도 (초당 공격 횟수, 예: 1.0 = 1회/초)
        /// </summary>
        public float AttackSpeed { get; set; }
    }
}
