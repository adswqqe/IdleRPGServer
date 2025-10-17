using System;

namespace IdleRPG.Domain.Entities
{
    /// <summary>
    /// 던전 난이도 내의 특정 웨이브를 정의하는 마스터 데이터입니다.
    ///
    /// 설계 결정:
    /// - Phase 1에서는 "웨이브당 단일 몬스터" 구조를 채택합니다 (YAGNI 원칙).
    /// - 향후 "웨이브당 여러 몬스터"가 필요하면 중간 테이블(WaveMonster)로 확장 가능합니다.
    /// </summary>
    public class DungeonWave
    {
        /// <summary>
        /// 웨이브의 고유 식별자
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 부모 난이도의 FK
        /// </summary>
        public int DifficultyId { get; set; }

        /// <summary>
        /// 웨이브 순서 (1부터 시작)
        /// 예: 1 = 첫 번째 웨이브, 5 = 최종 보스 웨이브
        /// </summary>
        public int WaveNumber { get; set; }

        /// <summary>
        /// 이 웨이브에 등장하는 몬스터의 FK
        /// </summary>
        public Guid MonsterId { get; set; }

        // Navigation Properties
        /// <summary>
        /// 부모 난이도
        /// </summary>
        public virtual DungeonDifficulty Difficulty { get; set; } = null!;

        /// <summary>
        /// 등장 몬스터
        /// </summary>
        public virtual Monster Monster { get; set; } = null!;
    }
}
