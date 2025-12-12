namespace IdleRPG.Application.DTOs.Battle
{
    /// <summary>
    /// 전투 통계 DTO (전투 과정의 상세 정보)
    /// </summary>
    public class BattleStatisticsDto
    {
        /// <summary>
        /// 총 턴 수
        /// </summary>
        public int TotalTurns { get; set; }

        /// <summary>
        /// 캐릭터가 입힌 총 데미지
        /// </summary>
        public int TotalDamageDealt { get; set; }

        /// <summary>
        /// 캐릭터가 받은 총 데미지
        /// </summary>
        public int TotalDamageTaken { get; set; }

        /// <summary>
        /// 크리티컬 발생 횟수
        /// </summary>
        public int CriticalHitCount { get; set; }

        /// <summary>
        /// 회피 성공 횟수
        /// </summary>
        public int EvasionCount { get; set; }
    }
}
