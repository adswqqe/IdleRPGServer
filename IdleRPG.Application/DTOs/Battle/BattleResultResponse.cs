using IdleRPG.Application.DTOs.Characters;
using IdleRPG.Application.DTOs.Rewards;
using BattleLogEntity = IdleRPG.Domain.Entities.BattleLog;

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
        /// 전투 후 업데이트된 캐릭터 정보 (레벨업 여부 확인용)
        /// </summary>
        public CharacterDto? UpdatedCharacter { get; set; }

        /// <summary>
        /// 전투 로그 엔티티 (DB 미저장)
        /// DungeonService가 트랜잭션의 일부로 저장
        /// 일반 전투 시에는 BattleService가 직접 저장하므로 null
        /// </summary>
        public BattleLogEntity? BattleLog { get; set; }
    }
}