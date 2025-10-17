using IdleRPG.Application.DTOs.Rewards;
using IdleRPG.Application.Interfaces;
using IdleRPG.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace IdleRPG.Application.Services
{
    /// <summary>
    /// LootTable 기반 확률 계산 구현
    ///
    /// [학습 포인트 1] 누적 가중치 알고리즘 (Weighted Random Selection)
    ///
    /// 예시:
    /// Items: [Potion(50), Sword(30), Shield(20)]
    /// Total Weight: 100
    ///
    /// 누적 범위 계산:
    /// - Potion: 0 ~ 49 (누적: 0 + 50 = 50)
    /// - Sword: 50 ~ 79 (누적: 50 + 30 = 80)
    /// - Shield: 80 ~ 99 (누적: 80 + 20 = 100)
    ///
    /// Random(0, 100) = 65 생성:
    /// - 65 < 50? No
    /// - 65 < 80? Yes → Sword 선택!
    ///
    /// [학습 포인트 2] Seed RNG의 재현성
    ///
    /// var random = new Random(12345);
    /// random.Next(0, 100);  // 항상 동일한 값 (예: 87)
    /// random.Next(0, 100);  // 항상 동일한 값 (예: 23)
    ///
    /// 용도:
    /// - 단위 테스트: Assert.Equal(expected, actual)
    /// - 디버깅: 같은 Seed로 버그 재현
    /// - 전투 로그 재현: BattleLog에 Seed 저장
    ///
    /// [학습 포인트 3] 중복 제거 로직
    ///
    /// allowDuplicates = true:
    /// - NumberOfRolls = 3 → [Gold, Gold, Potion] 가능
    ///
    /// allowDuplicates = false:
    /// - NumberOfRolls = 3 → [Gold, Potion, Sword]
    /// - 이미 선택된 아이템은 제외하고 재추첨
    /// - Items가 부족하면 조기 종료
    /// </summary>
    public class DropCalculator : IDropCalculator
    {
        private readonly ILogger<DropCalculator> _logger;

        public DropCalculator(ILogger<DropCalculator> logger)
        {
            _logger = logger;
        }

        public List<RewardDto> CalculateRewards(
            LootTable lootTable,
            int seed,
            bool allowDuplicates = false)
        {
            if (lootTable == null)
                throw new ArgumentNullException(nameof(lootTable));

            if (lootTable.Items == null || !lootTable.Items.Any())
            {
                _logger.LogWarning("LootTable {Id} has no items.", lootTable.Id);
                return new List<RewardDto>();
            }

            var rewards = new List<RewardDto>();
            var random = new Random(seed);  // Seed로 난수 생성기 초기화

            _logger.LogDebug(
                "Calculating rewards for LootTable {Id} with seed {Seed}, allowDuplicates: {AllowDuplicates}",
                lootTable.Id, seed, allowDuplicates);

            // Step 1: IsGuaranteed = true 아이템들을 무조건 추가
            var guaranteedItems = lootTable.Items.Where(i => i.IsGuaranteed).ToList();
            foreach (var item in guaranteedItems)
            {
                var reward = CreateReward(item, random);
                rewards.Add(reward);

                _logger.LogDebug(
                    "Guaranteed reward added: {Type} x {Quantity} (LootItemId: {ItemId})",
                    reward.Type, reward.Quantity, item.Id);
            }

            // Step 2: IsGuaranteed = false 아이템들을 확률 기반으로 추첨
            var probabilityItems = lootTable.Items.Where(i => !i.IsGuaranteed).ToList();

            if (probabilityItems.Any())
            {
                var selectedItems = new HashSet<int>();  // 중복 제거용 (LootItem.Id 저장)

                for (int roll = 0; roll < lootTable.NumberOfRolls; roll++)
                {
                    // 중복 제거 모드일 경우, 이미 선택된 아이템 제외
                    var availableItems = allowDuplicates
                        ? probabilityItems
                        : probabilityItems.Where(i => !selectedItems.Contains(i.Id)).ToList();

                    if (!availableItems.Any())
                    {
                        _logger.LogDebug("No more available items for roll {Roll}. Stopping early.", roll + 1);
                        break;  // 더 이상 선택할 아이템 없음
                    }

                    var selectedItem = SelectItemByWeight(availableItems, random);

                    if (selectedItem != null)
                    {
                        var reward = CreateReward(selectedItem, random);
                        rewards.Add(reward);

                        if (!allowDuplicates)
                        {
                            selectedItems.Add(selectedItem.Id);  // 중복 방지
                        }

                        _logger.LogDebug(
                            "Roll {Roll}: {Type} x {Quantity} (LootItemId: {ItemId}, Weight: {Weight})",
                            roll + 1, reward.Type, reward.Quantity, selectedItem.Id, selectedItem.Weight);
                    }
                }
            }

            _logger.LogInformation(
                "Rewards calculated for LootTable {Id}: {Count} items (Seed: {Seed})",
                lootTable.Id, rewards.Count, seed);

            return rewards;
        }

        public LootItem? SelectItemByWeight(List<LootItem> items, Random random)
        {
            if (items == null || !items.Any())
                return null;

            // Step 1: 총 가중치 계산
            int totalWeight = items.Sum(i => i.Weight);

            if (totalWeight <= 0)
            {
                _logger.LogWarning("Total weight is 0 or negative. Cannot select item.");
                return null;
            }

            // Step 2: 0 ~ totalWeight 범위에서 랜덤 값 생성
            int randomValue = random.Next(0, totalWeight);

            _logger.LogTrace("Random value: {RandomValue} / {TotalWeight}", randomValue, totalWeight);

            // Step 3: 누적 가중치로 아이템 선택
            int cumulativeWeight = 0;

            foreach (var item in items)
            {
                cumulativeWeight += item.Weight;

                if (randomValue < cumulativeWeight)
                {
                    _logger.LogTrace(
                        "Item selected: LootItemId {Id} (Weight: {Weight}, Cumulative: {Cumulative})",
                        item.Id, item.Weight, cumulativeWeight);

                    return item;
                }
            }

            // 이론상 도달 불가능 (total weight가 양수이고 randomValue < totalWeight이면)
            _logger.LogError("No item selected despite valid weights. This should not happen.");
            return items.Last();  // Fallback
        }

        /// <summary>
        /// LootItem으로부터 RewardDto 생성 (수량 랜덤 결정)
        /// </summary>
        private RewardDto CreateReward(LootItem item, Random random)
        {
            // MinQuantity ~ MaxQuantity 범위에서 랜덤 수량 결정
            int quantity = item.MinQuantity == item.MaxQuantity
                ? item.MinQuantity
                : random.Next(item.MinQuantity, item.MaxQuantity + 1);  // Max는 exclusive이므로 +1

            return new RewardDto
            {
                Type = item.Type,
                ItemId = item.ItemId,
                Quantity = quantity,
                SourceLootItemId = item.Id
            };
        }
    }
}
