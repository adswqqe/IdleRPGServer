using IdleRPG.Application.DTOs.Battle;
using IdleRPG.Domain.Enums;

namespace IdleRPG.Application.Interfaces
{
    /// <summary>
    /// 전투 시뮬레이션 서비스 인터페이스
    /// </summary>
    public interface IBattleService
    {
        /// <summary>
        /// 캐릭터와 몬스터 간의 전투를 시뮬레이션합니다.
        /// Priority Queue 기반 Event-driven 방식으로 실시간 전투를 처리합니다.
        /// </summary>
        /// <param name="characterId">전투를 수행할 캐릭터 ID</param>
        /// <param name="monsterId">전투할 몬스터 ID</param>
        /// <param name="dungeonStageId">던전 스테이지 ID (던전 전투인 경우)</param>
        /// <param name="difficulty">던전 난이도 (난이도 배율 적용)</param>
        /// <returns>전투 결과 (승리 여부, 보상, 통계)</returns>
        Task<BattleResultResponse> SimulateBattleAsync(
            Guid characterId, 
            Guid monsterId, 
            int? dungeonStageId = null, 
            DungeonDifficulty? difficulty = null);

        /// <summary>
        /// 캐릭터의 전투 히스토리 조회 (페이징)
        /// </summary>
        /// <param name="characterId">캐릭터 ID</param>
        /// <param name="page">페이지 번호 (1부터 시작)</param>
        /// <param name="pageSize">페이지 크기</param>
        /// <returns>전투 로그 목록</returns>
        Task<BattleLogsResponse> GetBattleLogsAsync(Guid characterId, int page, int pageSize);

        /// <summary>
        /// 캐릭터의 최근 N개 전투 로그 조회
        /// </summary>
        /// <param name="characterId">캐릭터 ID</param>
        /// <param name="count">조회할 개수</param>
        /// <returns>최근 전투 로그 목록</returns>
        Task<List<BattleLogDto>> GetRecentBattleLogsAsync(Guid characterId, int count);

        /// <summary>
        /// 캐릭터의 전투 통계 조회
        /// </summary>
        /// <param name="characterId">캐릭터 ID</param>
        /// <returns>전투 통계</returns>
        Task<Domain.Repositories.BattleStatistics> GetBattleStatsAsync(Guid characterId);
    }
}
