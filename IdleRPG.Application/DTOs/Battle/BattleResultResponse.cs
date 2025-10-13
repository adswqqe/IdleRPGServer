using IdleRPG.Application.DTOs.Rewards;

namespace IdleRPG.Application.DTOs.Battle
{
    /// <summary>
    /// 전투 결과 응답 DTO
    /// </summary>
    public class BattleResultResponse
    {
        /// <summary>
        /// 승리 여부
        /// </summary>
        public bool IsVictory { get; set; }

        /// <summary>
        /// 전투 보상 정보 (골드, 경험치 등)
        /// </summary>
        public RewardDto Reward { get; set; } = new();

        /// <summary>
        /// 전투 통계 (턴 수, 데미지, 크리티컬 등)
        /// </summary>
        public BattleStatisticsDto Statistics { get; set; } = new();

        /// <summary>
        /// 캐릭터 전투 스탯 정보
        /// </summary>
        public BattleStatsDto CharacterStats { get; set; } = new();

        /// <summary>
        /// 몬스터 전투 스탯 정보
        /// </summary>
        public BattleStatsDto MonsterStats { get; set; } = new();
    }
}