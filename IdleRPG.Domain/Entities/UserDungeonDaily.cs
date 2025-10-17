using System;

namespace IdleRPG.Domain.Entities
{
    /// <summary>
    /// 플레이어의 일일 던전 입장 횟수를 추적하는 데이터입니다.
    ///
    /// 시간 기반 데이터 모델링 (Temporal Data Modeling):
    /// - Date 필드를 기준으로 매일 00:00 UTC에 자동으로 리셋됩니다.
    /// - 새로운 날짜의 레코드는 플레이어가 던전에 입장할 때 Lazy Creation됩니다.
    /// - 모든 플레이어에 대해 미리 레코드를 생성하지 않아 효율적입니다.
    ///
    /// Unique 제약:
    /// - (UserId, DungeonTemplateId, DifficultyCode, Date) 복합키로 데이터 무결성 보장
    /// - 하루에 같은 던전 난이도 조합은 하나의 레코드만 존재
    /// </summary>
    public class UserDungeonDaily
    {
        /// <summary>
        /// 일일 기록의 고유 식별자
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 사용자 ID
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 던전 템플릿 ID
        /// </summary>
        public int DungeonTemplateId { get; set; }

        /// <summary>
        /// 난이도 코드
        /// </summary>
        public DifficultyCode DifficultyCode { get; set; }

        /// <summary>
        /// 해당 날짜의 입장 횟수
        /// DungeonDifficulty.DailyEntryLimit와 비교하여 입장 가능 여부 판단
        /// </summary>
        public int EntryCount { get; set; } = 0;

        /// <summary>
        /// 기록 날짜 (시간 정보 없음)
        /// DateOnly 타입 사용으로 의미상 명확하고 저장 공간 효율적
        /// </summary>
        public DateOnly Date { get; set; }

        // Navigation Properties
        public virtual Player Player { get; set; } = null!;
        public virtual DungeonTemplate DungeonTemplate { get; set; } = null!;
    }
}
