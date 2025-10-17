/*
 * =====================================================================================
 *   Data Lifecycle Pattern: Live vs Historical Data Separation
 * =====================================================================================
 * DungeonProgress와 DungeonRunHistory는 데이터의 생명주기에 따라 분리되었습니다.
 *
 * DungeonProgress (Hot Table - 실시간 데이터):
 *   - 던전 진행 중 플레이어의 현재 상태를 저장
 *   - 빈번한 UPDATE 발생 (웨이브 진행, 체력 변화 등)
 *   - 던전 완료/실패 시 삭제되는 휘발성 데이터
 *   - 항상 작은 크기 유지 (Active Users만)
 *
 * DungeonRunHistory (Cold Table - 기록 데이터):
 *   - 던전 완료 후 결과를 영구 보관
 *   - INSERT만 발생, UPDATE 없음
 *   - 통계, 랭킹, 보상 내역 조회용
 *   - 시간이 지남에 따라 커지는 누적 데이터
 *
 * 분리 이유:
 *   - 성능: Hot 테이블과 Cold 테이블을 분리하여 쿼리 효율 향상
 *   - 관심사 분리: "현재 상태" vs "과거 기록"
 *   - 유지보수: 각 테이블의 인덱스 전략을 독립적으로 최적화 가능
 * =====================================================================================
 */

using System;

namespace IdleRPG.Domain.Entities
{
    /// <summary>
    /// 플레이어의 던전 진행 상황을 추적하는 Instance 데이터입니다.
    /// 던전 시작 시 생성되고, 완료/실패 시 삭제됩니다 (휘발성 데이터).
    /// </summary>
    public class DungeonProgress
    {
        /// <summary>
        /// 진행 상황의 고유 식별자
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 던전을 플레이하는 캐릭터의 ID
        /// </summary>
        public Guid CharacterId { get; set; }

        /// <summary>
        /// 플레이 중인 난이도의 FK
        /// </summary>
        public int DifficultyId { get; set; }

        /// <summary>
        /// 현재 진행 중인 웨이브 번호 (1부터 시작)
        /// </summary>
        public int CurrentWave { get; set; } = 1;

        /// <summary>
        /// 현재 캐릭터의 체력
        /// 전투 중 실시간으로 업데이트됩니다.
        /// </summary>
        public int CurrentHealth { get; set; }

        /// <summary>
        /// 던전 시작 시각
        /// </summary>
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual Character Character { get; set; } = null!;
        public virtual DungeonDifficulty Difficulty { get; set; } = null!;
    }
}
