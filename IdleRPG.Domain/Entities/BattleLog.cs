namespace IdleRPG.Domain.Entities
{
    /// <summary>
    /// 전투 기록 엔티티 - 전투 결과 및 통계를 저장
    /// 용도: 밸런싱 데이터 수집, 사용자 전투 히스토리, 치팅 방지
    /// </summary>
    public class BattleLog
    {
        /// <summary>
        /// 전투 기록 고유 ID
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 캐릭터 ID (FK)
        /// </summary>
        public Guid CharacterId { get; set; }

        /// <summary>
        /// 몬스터 ID (FK)
        /// </summary>
        public Guid MonsterId { get; set; }

        /// <summary>
        /// 승리 여부
        /// </summary>
        public bool IsVictory { get; set; }

        /// <summary>
        /// 획득 경험치
        /// </summary>
        public int ExperienceGained { get; set; }

        /// <summary>
        /// 획득 골드
        /// </summary>
        public int GoldGained { get; set; }

        /// <summary>
        /// 캐릭터가 입힌 총 데미지
        /// </summary>
        public int DamageDealt { get; set; }

        /// <summary>
        /// 캐릭터가 받은 총 데미지
        /// </summary>
        public int DamageTaken { get; set; }

        /// <summary>
        /// 전투 발생 일시
        /// </summary>
        public DateTime BattleDate { get; set; } = DateTime.UtcNow;


        /// <summary>
        /// 던전 스테이지 ID (nullable) - 던전 전투인 경우에만 값이 있음
        /// null이면 일반 몬스터 전투, 값이 있으면 던전 전투
        /// </summary>
        public int? DungeonStageId { get; set; }

        // Navigation Properties
        /// <summary>
        /// 전투를 수행한 캐릭터
        /// </summary>
        public Character Character { get; set; } = null!;

        /// <summary>
        /// 전투 대상 몬스터
        /// </summary>
        public Monster Monster { get; set; } = null!;
    }
}
