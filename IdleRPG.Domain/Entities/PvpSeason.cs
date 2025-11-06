using IdleRPG.Domain.Repositories;

namespace IdleRPG.Domain.Entities
{
    /// <summary>
    /// PVP 시즌 마스터 데이터
    /// 시즌 기간, 활성 여부를 관리하며, 랭킹과 매치 기록의 기준이 됩니다.
    /// </summary>
    public class PvpSeason : BaseEntity<int>
    {
        /// <summary>
        /// 시즌 번호 (1부터 시작, UI 표시용)
        /// 예: 1, 2, 3... (Unique)
        /// </summary>
        public int SeasonNumber { get; set; }

        /// <summary>
        /// 시즌 시작 시각 (UTC)
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// 시즌 종료 시각 (UTC)
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// 현재 활성 시즌 여부
        /// 시스템 전체에서 활성 시즌은 최대 1개만 존재해야 합니다.
        /// </summary>
        public bool IsActive { get; set; } = false;

        /// <summary>
        /// 마지막 업데이트 시각 (UTC)
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 시즌 기간 검증 (StartDate < EndDate)
        /// </summary>
        public bool IsValid()
        {
            return StartDate < EndDate;
        }

        // Navigation Properties
        /// <summary>
        /// 이 시즌의 랭킹 목록
        /// </summary>
        public virtual ICollection<PvpRanking> Rankings { get; set; } = new List<PvpRanking>();

        /// <summary>
        /// 이 시즌의 매치 기록 목록
        /// </summary>
        public virtual ICollection<PvpMatch> Matches { get; set; } = new List<PvpMatch>();
    }
}
