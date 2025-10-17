using System.Collections.Generic;

namespace IdleRPG.Domain.Entities
{
    public enum DifficultyCode
    {
        Easy,
        Normal,
        Hard
    }

    /// <summary>
    /// 던전의 특정 난이도에 대한 마스터 데이터.
    /// 권장 전투력, 보상, 입장 제한 등 실제 게임플레이 파라미터를 정의합니다.
    /// </summary>
    public class DungeonDifficulty
    {
        public int Id { get; set; }

        /// <summary>
        /// 부모가 되는 던전 템플릿의 FK
        /// </summary>
        public int TemplateId { get; set; }

        /// <summary>
        /// 난이도 코드 (쉬움, 보통, 어려움 등)
        /// </summary>
        public DifficultyCode Code { get; set; }

        /// <summary>
        /// 입장 권장 전투력 (UI 표시용)
        /// </summary>
        public int RecommendedPower { get; set; }

        /// <summary>
        /// 클리어 시 확정적으로 지급되는 기본 골드 보상입니다.
        /// LootTable의 확률적 보상과 별개로 항상 지급됩니다.
        /// </summary>
        public long BaseGold { get; set; }

        /// <summary>
        /// 클리어 시 확정적으로 지급되는 기본 경험치 보상입니다.
        /// LootTable의 확률적 보상과 별개로 항상 지급됩니다.
        /// </summary>
        public int BaseExp { get; set; }

        /// <summary>
        /// 클리어 시 지급될 확률적/가변적 보상 테이블의 FK입니다.
        /// 기본 보상(BaseGold/BaseExp) 외에 추가적인 아이템 드랍 등을 처리하며,
        /// 설정되지 않을 수도 있습니다 (nullable).
        /// </summary>
        public int? LootTableId { get; set; }

        /// <summary>
        /// 해당 난이도의 최대 웨이브 수
        /// </summary>
        public int MaxWaves { get; set; }

        /// <summary>
        /// 일일 입장 가능 횟수 (0이면 무제한)
        /// </summary>
        public int DailyEntryLimit { get; set; } = 3;

        /// <summary>
        /// 입장 시 소모되는 골드 (0이면 무료)
        /// </summary>
        public int EntryCostGold { get; set; } = 0;

        // Navigation Properties

        /// <summary>
        /// 부모 던전 템플릿
        /// </summary>
        public virtual DungeonTemplate Template { get; set; } = null!;

        /// <summary>
        /// 이 난이도를 구성하는 웨이브 목록
        /// </summary>
        public virtual ICollection<DungeonWave> Waves { get; set; } = new List<DungeonWave>();

        /// <summary>
        /// 이 난이도에 연결된 보상 테이블
        /// </summary>
        public virtual LootTable? LootTable { get; set; }
    }
}
