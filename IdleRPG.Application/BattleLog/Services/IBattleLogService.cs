using IdleRPG.Application.DTOs.Battle;
using IdleRPG.Domain.Repositories;

namespace IdleRPG.Application.BattleLog.Services
{
    /// <summary>
    /// 전투 로그 관리 서비스 인터페이스
    ///
    /// 책임:
    /// - 전투 로그 생성 및 저장 (SaveChanges는 호출자가 트랜잭션으로 처리)
    /// - 전투 로그 조회 (페이징, 최근 N개, 통계)
    ///
    /// 특징:
    /// - 횡단 관심사(Cross-Cutting Concern) 분리
    /// - 모든 전투(Stage, SpecialDungeon, PVP)에서 동일한 로그 생성 로직 사용
    /// - 로그 생성 + 조회 + 통계를 한 Service에서 관리 (응집도)
    /// </summary>
    public interface IBattleLogService
    {
        /// <summary>
        /// 전투 로그를 생성하고 Repository에 추가합니다.
        ///
        /// ⚠️ 주의: SaveChanges는 호출하지 않으므로 호출자가 트랜잭션으로 묶어야 합니다.
        /// </summary>
        /// <param name="characterId">캐릭터 ID</param>
        /// <param name="monsterId">몬스터 ID</param>
        /// <param name="combatResult">전투 결과 (CombatService 반환값)</param>
        /// <param name="experienceGained">획득 경험치 (보상 계산 후)</param>
        /// <param name="goldGained">획득 골드 (보상 계산 후)</param>
        /// <param name="dungeonStageId">던전 스테이지 ID (던전 전투인 경우, Optional)</param>
        /// <returns>생성된 BattleLog 엔티티 (미저장 상태)</returns>
        Task<Domain.Entities.BattleLog> CreateAndSaveLogAsync(
            Guid characterId,
            Guid monsterId,
            CombatResultDto combatResult,
            int experienceGained,
            int goldGained,
            int? dungeonStageId = null);

        /// <summary>
        /// 캐릭터의 전투 히스토리 조회 (페이징)
        /// </summary>
        /// <param name="characterId">캐릭터 ID</param>
        /// <param name="page">페이지 번호 (1부터 시작)</param>
        /// <param name="pageSize">페이지 크기</param>
        /// <returns>전투 로그 목록 (페이징 정보 포함)</returns>
        Task<BattleLogsResponse> GetLogsAsync(Guid characterId, int page, int pageSize);

        /// <summary>
        /// 캐릭터의 최근 N개 전투 로그 조회
        /// </summary>
        /// <param name="characterId">캐릭터 ID</param>
        /// <param name="count">조회할 개수</param>
        /// <returns>최근 전투 로그 목록</returns>
        Task<List<BattleLogDto>> GetRecentLogsAsync(Guid characterId, int count);

        /// <summary>
        /// 캐릭터의 전투 통계 조회
        /// </summary>
        /// <param name="characterId">캐릭터 ID</param>
        /// <returns>전투 통계 (총 전투 수, 승률, 데미지 등)</returns>
        Task<BattleStatistics> GetStatsAsync(Guid characterId);
    }
}
