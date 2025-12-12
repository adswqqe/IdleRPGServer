using IdleRPG.Domain.Repositories;

namespace IdleRPG.Domain.Entities
{
    /// <summary>
    /// PVP 매치 기록 (히스토리 데이터, Immutable)
    /// 플레이어 간 대결 결과와 레이팅 변화를 기록합니다.
    /// </summary>
    public class PvpMatch : BaseEntity
    {
        /// <summary>
        /// 시즌 ID (FK → PvpSeason)
        /// </summary>
        public int SeasonId { get; set; }

        /// <summary>
        /// 공격자 캐릭터 ID (FK → Character)
        /// </summary>
        public Guid AttackerId { get; set; }

        /// <summary>
        /// 방어자 캐릭터 ID (FK → Character)
        /// </summary>
        public Guid DefenderId { get; set; }

        /// <summary>
        /// 승자 캐릭터 ID (FK → Character)
        /// AttackerId 또는 DefenderId 중 하나
        /// </summary>
        public Guid WinnerId { get; set; }

        /// <summary>
        /// 매치 전 공격자 레이팅
        /// </summary>
        public int AttackerRatingBefore { get; set; }

        /// <summary>
        /// 매치 후 공격자 레이팅
        /// </summary>
        public int AttackerRatingAfter { get; set; }

        /// <summary>
        /// 매치 전 방어자 레이팅
        /// </summary>
        public int DefenderRatingBefore { get; set; }

        /// <summary>
        /// 매치 후 방어자 레이팅
        /// </summary>
        public int DefenderRatingAfter { get; set; }

        /// <summary>
        /// 매치 유효성 검증
        /// - AttackerId != DefenderId (자기 자신과 매칭 불가)
        /// - WinnerId는 AttackerId 또는 DefenderId 중 하나
        /// </summary>
        public bool IsValid()
        {
            if (AttackerId == DefenderId)
                return false;

            if (WinnerId != AttackerId && WinnerId != DefenderId)
                return false;

            return true;
        }

        // Navigation Properties

        /// <summary>
        /// 시즌 정보
        /// </summary>
        public virtual PvpSeason Season { get; set; } = null!;

        /// <summary>
        /// 공격자 캐릭터 정보
        /// </summary>
        public virtual Character Attacker { get; set; } = null!;

        /// <summary>
        /// 방어자 캐릭터 정보
        /// </summary>
        public virtual Character Defender { get; set; } = null!;

        /// <summary>
        /// 승자 캐릭터 정보
        /// </summary>
        public virtual Character Winner { get; set; } = null!;
    }
}
