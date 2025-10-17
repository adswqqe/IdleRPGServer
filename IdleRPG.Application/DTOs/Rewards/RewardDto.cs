using IdleRPG.Domain.Enums;

namespace IdleRPG.Application.DTOs.Rewards
{
    /// <summary>
    /// 통합 보상 정보 DTO (전투, 던전, 퀘스트 등 모든 보상 시스템에서 공통 사용)
    ///
    /// [학습 포인트] DTO 통합 전략
    /// - 이전: 간단한 RewardDto (Gold, Experience)와 상세한 LootRewardDto 분리
    /// - 현재: 하나의 RewardDto로 통합, 선택적 필드로 유연성 확보
    /// - 장점: 코드 중복 제거, 확장성 향상, 일관성 유지
    ///
    /// [사용 패턴]
    /// 1. 간단한 보상 (BattleService):
    ///    new RewardDto { Gold = 100, Experience = 50 }
    ///
    /// 2. Loot Table 보상 (DropService):
    ///    new RewardDto { Type = RewardType.Item, ItemId = guid, Quantity = 3 }
    ///
    /// 3. 혼합 보상:
    ///    new RewardDto { Type = RewardType.Gold, Gold = 1000, Quantity = 1 }
    /// </summary>
    public class RewardDto
    {
        /// <summary>
        /// 획득 골드 (long 타입: 방치형 게임 특성상 큰 숫자 처리)
        /// Type이 Gold일 때 사용, 또는 기존 간단한 보상 방식에서 사용
        /// </summary>
        public long Gold { get; set; }

        /// <summary>
        /// 획득 경험치 (long 타입: 방치형 게임 특성상 큰 숫자 처리)
        /// Type이 Experience일 때 사용, 또는 기존 간단한 보상 방식에서 사용
        /// </summary>
        public long Experience { get; set; }

        // ===== Loot Table Pattern 확장 필드 =====

        /// <summary>
        /// 보상 타입 (Gold, Experience, Item, Equipment)
        /// null이면 기존 방식 (Gold + Experience 혼합)
        /// </summary>
        public RewardType? Type { get; set; }

        /// <summary>
        /// 아이템 템플릿 ID (Type이 Item일 경우)
        /// Gold, Experience, Equipment는 null
        /// </summary>
        public Guid? ItemId { get; set; }

        /// <summary>
        /// 보상 수량
        /// - Type = Gold: Quantity를 Gold 필드에도 복사 권장
        /// - Type = Experience: Quantity를 Experience 필드에도 복사 권장
        /// - Type = Item: 아이템 개수
        /// - Type = Equipment: 1 (장비는 개별 생성)
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// 디버깅용: 어떤 LootItem에서 계산되었는지
        /// Loot Table 보상에서만 사용, 일반 보상은 0
        /// </summary>
        public int SourceLootItemId { get; set; }
    }
}
