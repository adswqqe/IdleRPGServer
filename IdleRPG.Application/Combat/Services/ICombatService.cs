using IdleRPG.Application.DTOs.Battle;
using IdleRPG.Domain.Enums;

namespace IdleRPG.Application.Combat.Services
{
    /// <summary>
    /// 순수 전투 시뮬레이션 서비스 인터페이스
    ///
    /// 책임:
    /// - 전투 계산만 수행 (Stateless)
    /// - 보상 지급, BattleLog 저장, 트랜잭션 관리는 호출자가 책임
    ///
    /// 재사용성:
    /// - StageService: 메인 스테이지 전투
    /// - SpecialDungeonService: 보스 던전 전투
    /// - PVPService: PVP 전투 (Phase 2)
    /// </summary>
    public interface ICombatService
    {
        /// <summary>
        /// 캐릭터와 몬스터 간의 전투를 시뮬레이션합니다.
        ///
        /// Priority Queue 기반 Event-driven 방식으로 실시간 전투를 처리합니다.
        /// </summary>
        /// <param name="characterId">전투를 수행할 캐릭터 ID</param>
        /// <param name="monsterId">전투할 몬스터 ID</param>
        /// <param name="difficulty">던전 난이도 (배율 적용, Optional)</param>
        /// <returns>전투 결과 (승패, 전투 통계)</returns>
        /// <remarks>
        /// ⚠️ 주의사항:
        /// - 이 메서드는 DB에 아무것도 저장하지 않습니다
        /// - 보상 계산은 포함하지 않습니다 (호출자가 Monster 정보로 계산)
        /// - BattleLog 생성도 하지 않습니다 (BattleLogService 사용)
        /// </remarks>
        Task<CombatResultDto> SimulateCombatAsync(
            Guid characterId,
            Guid monsterId,
            DungeonDifficulty? difficulty = null);
    }
}
