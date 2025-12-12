using IdleRPG.Domain.Entities;

namespace IdleRPG.Domain.Repositories
{
    /// <summary>
    /// 전투 로그 데이터 접근 인터페이스
    /// </summary>
    public interface IBattleLogRepository : IRepository<BattleLog>
    {
        /// <summary>
        /// 캐릭터별 전투 히스토리 조회 (페이징)
        /// </summary>
        /// <param name="characterId">캐릭터 ID</param>
        /// <param name="page">페이지 번호 (1부터 시작)</param>
        /// <param name="pageSize">페이지 크기</param>
        /// <returns>전투 로그 목록 (최신순)</returns>
        Task<List<BattleLog>> GetByCharacterIdAsync(Guid characterId, int page, int pageSize);

        /// <summary>
        /// 캐릭터의 최근 N개 전투 로그 조회
        /// </summary>
        /// <param name="characterId">캐릭터 ID</param>
        /// <param name="count">조회할 개수</param>
        /// <returns>최근 전투 로그 목록 (최신순)</returns>
        Task<List<BattleLog>> GetRecentByCharacterIdAsync(Guid characterId, int count);

        /// <summary>
        /// 캐릭터의 전투 통계 조회
        /// </summary>
        /// <param name="characterId">캐릭터 ID</param>
        /// <returns>총 전투 수, 승리 수, 패배 수, 총 획득 경험치, 총 획득 골드</returns>
        Task<BattleStatistics> GetStatsByCharacterIdAsync(Guid characterId);
    }

    /// <summary>
    /// 전투 통계 정보
    /// </summary>
    public class BattleStatistics
    {
        /// <summary>
        /// 총 전투 횟수
        /// </summary>
        public int TotalBattles { get; set; }

        /// <summary>
        /// 승리 횟수
        /// </summary>
        public int Victories { get; set; }

        /// <summary>
        /// 패배 횟수
        /// </summary>
        public int Defeats { get; set; }

        /// <summary>
        /// 승률 (0~100)
        /// </summary>
        public double WinRate => TotalBattles > 0 ? (double)Victories / TotalBattles * 100 : 0;

        /// <summary>
        /// 총 획득 경험치
        /// </summary>
        public int TotalExperience { get; set; }

        /// <summary>
        /// 총 획득 골드
        /// </summary>
        public int TotalGold { get; set; }

        /// <summary>
        /// 총 입힌 데미지
        /// </summary>
        public int TotalDamageDealt { get; set; }

        /// <summary>
        /// 총 받은 데미지
        /// </summary>
        public int TotalDamageTaken { get; set; }
    }
}
