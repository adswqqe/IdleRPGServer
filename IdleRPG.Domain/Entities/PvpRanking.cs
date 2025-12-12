using IdleRPG.Domain.Enums;

namespace IdleRPG.Domain.Entities
{
    /// <summary>
    /// 시즌별 PVP 랭킹 정보
    /// 복합키: (SeasonId, CharacterId)
    /// 한 캐릭터는 시즌마다 하나의 랭킹 레코드를 가집니다.
    /// </summary>
    public class PvpRanking
    {
        /// <summary>
        /// 시즌 ID (FK → PvpSeason, PK 일부)
        /// </summary>
        public int SeasonId { get; set; }

        /// <summary>
        /// 캐릭터 ID (FK → Character, PK 일부)
        /// </summary>
        public Guid CharacterId { get; set; }

        /// <summary>
        /// 현재 레이팅 (기본값: 1000)
        /// </summary>
        public int Rating { get; set; } = 1000;

        /// <summary>
        /// 승리 횟수
        /// </summary>
        public int Wins { get; set; } = 0;

        /// <summary>
        /// 패배 횟수
        /// </summary>
        public int Losses { get; set; } = 0;

        /// <summary>
        /// 연승 횟수 (패배 시 0으로 초기화)
        /// </summary>
        public int WinStreak { get; set; } = 0;

        /// <summary>
        /// 시즌 보상 수령 여부
        /// </summary>
        public bool IsRewardClaimed { get; set; } = false;

        /// <summary>
        /// 마지막 매치 시각 (UTC, Nullable)
        /// </summary>
        public DateTime? LastMatchAt { get; set; }

        /// <summary>
        /// 마지막 업데이트 시각 (UTC)
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// PVP 티어 (Rating 기반 계산)
        /// Bronze: 0-999, Silver: 1000-1499, Gold: 1500-1999,
        /// Platinum: 2000-2499, Diamond: 2500+
        /// </summary>
        public PvpTier Tier
        {
            get
            {
                return Rating switch
                {
                    >= 2500 => PvpTier.Diamond,
                    >= 2000 => PvpTier.Platinum,
                    >= 1500 => PvpTier.Gold,
                    >= 1000 => PvpTier.Silver,
                    _ => PvpTier.Bronze
                };
            }
        }

        // Navigation Properties

        /// <summary>
        /// 시즌 정보
        /// </summary>
        public virtual PvpSeason Season { get; set; } = null!;

        /// <summary>
        /// 캐릭터 정보
        /// </summary>
        public virtual Character Character { get; set; } = null!;
    }
}
