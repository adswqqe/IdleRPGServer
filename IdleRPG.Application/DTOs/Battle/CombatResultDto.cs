namespace IdleRPG.Application.DTOs.Battle
{
    /// <summary>
    /// 순수 전투 시뮬레이션 결과 DTO
    ///
    /// CombatService가 반환하는 전투 결과입니다.
    /// 보상 정보, 캐릭터 업데이트, BattleLog는 포함하지 않습니다.
    /// </summary>
    public class CombatResultDto
    {
        /// <summary>
        /// 승리 여부
        /// </summary>
        public bool IsVictory { get; set; }

        /// <summary>
        /// 전투 통계 (턴 수, 데미지, 크리티컬 등)
        /// </summary>
        public BattleStatisticsDto Statistics { get; set; } = new();
    }
}
