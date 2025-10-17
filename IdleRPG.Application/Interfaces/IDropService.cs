using IdleRPG.Application.DTOs.Rewards;

namespace IdleRPG.Application.Interfaces
{
    /// <summary>
    /// Drop System의 최상위 Orchestrator 인터페이스
    ///
    /// [학습 포인트 1] Orchestrator Pattern (오케스트레이터 패턴)
    /// - 여러 하위 서비스를 조합하여 복잡한 비즈니스 프로세스 완성
    /// - DropService = ILootTableCache + IDropCalculator
    ///
    /// 비유:
    /// - ILootTableCache: "악보 담당" (데이터 제공)
    /// - IDropCalculator: "연주자" (로직 실행)
    /// - IDropService: "지휘자" (전체 조율)
    ///
    /// [학습 포인트 2] 책임 분리의 장점
    /// - 캐시 전략 변경 (IMemoryCache → Redis): ILootTableCache만 교체
    /// - 확률 알고리즘 변경 (누적 가중치 → Alias Method): IDropCalculator만 교체
    /// - DropService는 변경 불필요 (Open-Closed Principle)
    ///
    /// [학습 포인트 3] Seed 관리 전략
    /// - seed = null: 진짜 랜덤 (실제 게임 플레이)
    /// - seed = 고정값: 재현 가능 (테스트, 디버깅, 전투 로그 재현)
    ///
    /// [학습 포인트 4] 비즈니스 규칙 추가 가능
    /// - 예: VIP 유저는 드랍률 1.5배
    /// - 예: 이벤트 기간에는 특정 아이템 확률 2배
    /// - 이런 규칙은 DropService에서 처리 (Calculator는 순수 로직만)
    /// </summary>
    public interface IDropService
    {
        /// <summary>
        /// LootTable ID를 기반으로 보상을 계산합니다.
        ///
        /// 실행 프로세스:
        /// 1. ILootTableCache를 통해 LootTable 조회 (캐시 우선, 미스 시 DB)
        /// 2. LootTable 존재 여부 검증
        /// 3. Seed 생성 (null이면 자동 생성)
        /// 4. IDropCalculator를 통해 확률 계산
        /// 5. 보상 결과 반환
        /// </summary>
        /// <param name="lootTableId">보상 테이블 ID</param>
        /// <param name="seed">난수 시드 (null이면 자동 생성)</param>
        /// <param name="allowDuplicates">중복 드랍 허용 여부 (기본값: false)</param>
        /// <returns>계산된 보상 리스트</returns>
        Task<List<RewardDto>> RollLootTableAsync(
            int lootTableId,
            int? seed = null,
            bool allowDuplicates = false);

        /// <summary>
        /// 캐시를 무효화합니다.
        /// 사용 시나리오: 관리자가 LootTable 수정 후 호출
        /// </summary>
        /// <param name="lootTableId">무효화할 LootTable ID</param>
        Task InvalidateCacheAsync(int lootTableId);

        /// <summary>
        /// 모든 LootTable 캐시를 무효화합니다.
        /// 사용 시나리오: 대규모 밸런스 패치 후 전체 갱신
        /// </summary>
        Task InvalidateAllCachesAsync();
    }
}
