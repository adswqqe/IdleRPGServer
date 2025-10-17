using System;

namespace IdleRPG.Domain.Entities
{
    /// <summary>
    /// 플레이어의 던전 플레이 기록을 영구 보관하는 Instance 데이터입니다.
    /// 던전 완료 또는 실패 시 생성되며, 삭제되지 않습니다 (영구 데이터).
    ///
    /// 용도:
    /// - 플레이어 통계 (총 클리어 횟수, 성공률 등)
    /// - 랭킹 시스템 (최고 클리어 웨이브, 최단 클리어 시간 등)
    /// - 보상 지급 내역 추적
    ///
    /// Data Lifecycle Pattern에 대한 상세 설명은 DungeonProgress.cs를 참조하세요.
    /// </summary>
    public class DungeonRunHistory
    {
        /// <summary>
        /// 기록의 고유 식별자
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 플레이한 캐릭터의 ID
        /// </summary>
        public Guid CharacterId { get; set; }

        /// <summary>
        /// 플레이한 난이도의 FK
        /// </summary>
        public int DifficultyId { get; set; }

        /// <summary>
        /// 던전 클리어 여부
        /// true: 모든 웨이브 클리어 성공
        /// false: 중간에 실패 (ClearedWave 필드 참조)
        /// </summary>
        public bool IsCleared { get; set; }

        /// <summary>
        /// 클리어한 마지막 웨이브 번호
        /// IsCleared가 true이면 MaxWaves와 동일
        /// IsCleared가 false이면 실패한 웨이브 직전 번호
        /// </summary>
        public int ClearedWave { get; set; }

        /// <summary>
        /// 던전 플레이 완료 시각
        /// </summary>
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual Character Character { get; set; } = null!;
        public virtual DungeonDifficulty Difficulty { get; set; } = null!;
    }
}
