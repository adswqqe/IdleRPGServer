using IdleRPG.Application.DTOs.Rewards;
using IdleRPG.Domain.Entities;

namespace IdleRPG.Application.Interfaces
{
    /// <summary>
    /// LootTable 기반 확률 계산을 담당하는 순수 로직 인터페이스
    ///
    /// [학습 포인트 1] 순수 함수 (Pure Function)
    /// - 입력: LootTable + Seed → 출력: 보상 리스트
    /// - 동일한 입력 → 동일한 출력 (결정적, Deterministic)
    /// - DB, 파일, 시간 등 외부 의존성 없음
    /// - 테스트가 매우 쉬움 (단위 테스트의 이상향)
    ///
    /// [학습 포인트 2] Seed RNG (Seeded Random Number Generator)
    /// - Seed를 고정하면 Random 시퀀스가 재현 가능
    /// - 용도: 디버깅, 테스트, 전투 로그 재현
    /// - 예: new Random(12345)는 항상 동일한 난수 시퀀스 생성
    ///
    /// [학습 포인트 3] 책임 분리 (SRP)
    /// - DropCalculator: 순수 수학적 확률 계산만
    /// - DropService: 캐시 조회, DB 저장, 보상 지급 등 오케스트레이션
    /// - 각자의 책임이 명확하여 유지보수 용이
    ///
    /// [학습 포인트 4] 중복 제거 정책
    /// - allowDuplicates = true: 동일 아이템이 여러 번 나올 수 있음 (예: Gold 3번)
    /// - allowDuplicates = false: 한 번 나온 아이템은 제외하고 재추첨
    /// </summary>
    public interface IDropCalculator
    {
        /// <summary>
        /// LootTable을 기반으로 보상을 계산합니다.
        ///
        /// 계산 프로세스:
        /// 1. IsGuaranteed = true 아이템들을 결과 리스트에 추가
        /// 2. IsGuaranteed = false 아이템들을 Weight 기반 확률로 추첨
        /// 3. NumberOfRolls만큼 반복 (중복 정책 적용)
        /// 4. 각 아이템의 수량을 MinQuantity ~ MaxQuantity 범위에서 랜덤 결정
        /// </summary>
        /// <param name="lootTable">보상 테이블 (Items 포함되어야 함)</param>
        /// <param name="seed">난수 시드 (재현 가능성 보장)</param>
        /// <param name="allowDuplicates">중복 드랍 허용 여부 (기본값: false)</param>
        /// <returns>계산된 보상 리스트</returns>
        List<RewardDto> CalculateRewards(
            LootTable lootTable,
            int seed,
            bool allowDuplicates = false);

        /// <summary>
        /// Weight 기반 확률 계산의 핵심 로직을 별도 메서드로 분리
        ///
        /// [학습용 메서드] 누적 가중치 알고리즘 이해
        /// - Items: [A(50), B(30), C(20)]
        /// - Total: 100
        /// - Ranges: A[0-50), B[50-80), C[80-100)
        /// - Random(0-100) = 65 → B 선택
        /// </summary>
        /// <param name="items">확률 계산 대상 아이템 리스트</param>
        /// <param name="random">난수 생성기 (Seed로 초기화됨)</param>
        /// <returns>선택된 LootItem, 없으면 null</returns>
        LootItem? SelectItemByWeight(List<LootItem> items, Random random);
    }
}
