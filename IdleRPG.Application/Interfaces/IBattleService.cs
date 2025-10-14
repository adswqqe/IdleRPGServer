using IdleRPG.Application.DTOs.Battle;

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
        /// <returns>전투 결과 (승리 여부, 보상, 통계)</returns>
        Task<BattleResultResponse> SimulateBattleAsync(Guid characterId, Guid monsterId);
    }
}
