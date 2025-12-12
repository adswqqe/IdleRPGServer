using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Enums;

namespace IdleRPG.Domain.Services
{
    /// <summary>
    /// LootTable 기반 확률 계산 로직 (순수 도메인 서비스)
    ///
    /// [설계 원칙]
    /// - Application Layer (DTO) 의존 금지
    /// - 순수 비즈니스 로직만 포함
    /// - Random 객체 외부 주입 (테스트 용이성)
    /// </summary>
    public class LootCalculator
    {
        /// <summary>
        /// LootTable에서 확률 기반으로 LootItem 목록을 선택합니다
        /// </summary>
        /// <param name="lootTable">보상 테이블</param>
        /// <param name="random">난수 생성기 (Seed 적용된 객체 전달)</param>
        /// <param name="allowDuplicates">중복 허용 여부</param>
        /// <returns>선택된 LootItem 목록 (Item, Quantity 튜플)</returns>
        public List<(LootItem Item, int Quantity)> RollLootItems(
            LootTable lootTable,
            Random random,
            bool allowDuplicates = false)
        {
            if (lootTable == null)
                throw new ArgumentNullException(nameof(lootTable));

            if (lootTable.Items == null || !lootTable.Items.Any())
                return new List<(LootItem, int)>();

            var results = new List<(LootItem, int)>();

            // 1단계: IsGuaranteed = true 아이템들을 무조건 추가
            var guaranteedItems = lootTable.Items.Where(i => i.IsGuaranteed).ToList();
            foreach (var item in guaranteedItems)
            {
                int quantity = CalculateQuantity(item, random);
                results.Add((item, quantity));
            }

            // 2단계: IsGuaranteed = false 아이템들을 확률 기반으로 추첨
            var probabilityItems = lootTable.Items.Where(i => !i.IsGuaranteed).ToList();
            if (!probabilityItems.Any())
                return results;

            var selectedItemIds = new HashSet<Guid?>();

            // 중복 방지 모드일 경우 Guaranteed 아이템 ID 등록
            if (!allowDuplicates)
            {
                foreach (var item in guaranteedItems)
                {
                    if (item.ItemId.HasValue)
                        selectedItemIds.Add(item.ItemId.Value);
                }
            }

            // NumberOfRolls 횟수만큼 추첨
            for (int roll = 0; roll < lootTable.NumberOfRolls; roll++)
            {
                var availableItems = allowDuplicates
                    ? probabilityItems
                    : probabilityItems.Where(i => !i.ItemId.HasValue || !selectedItemIds.Contains(i.ItemId.Value)).ToList();

                if (!availableItems.Any())
                    break;

                var selectedItem = SelectItemByWeight(availableItems, random);
                if (selectedItem != null)
                {
                    int quantity = CalculateQuantity(selectedItem, random);
                    results.Add((selectedItem, quantity));

                    if (!allowDuplicates && selectedItem.ItemId.HasValue)
                        selectedItemIds.Add(selectedItem.ItemId.Value);
                }
            }

            return results;
        }

        /// <summary>
        /// Weight 기반 확률 계산으로 아이템 선택
        /// </summary>
        public LootItem? SelectItemByWeight(List<LootItem> items, Random random)
        {
            if (items == null || items.Count == 0)
                return null;

            // 양수 가중치만 필터링
            var validItems = items.Where(i => i.Weight > 0).ToList();
            if (!validItems.Any())
                return null;

            // 총 가중치 계산
            long totalWeight = validItems.Sum(i => (long)i.Weight);
            if (totalWeight <= 0)
                return null;

            // 랜덤 값 생성
            long randomValue = random.NextInt64(0, totalWeight);

            // 누적 가중치로 아이템 선택
            long cumulativeWeight = 0;
            foreach (var item in validItems)
            {
                cumulativeWeight += item.Weight;
                if (randomValue < cumulativeWeight)
                    return item;
            }

            // 폴백
            return validItems[validItems.Count - 1];
        }

        /// <summary>
        /// LootItem의 MinQuantity/MaxQuantity 범위에서 랜덤 수량 결정
        /// </summary>
        private int CalculateQuantity(LootItem item, Random random)
        {
            int minQ = Math.Max(0, Math.Min(item.MinQuantity, item.MaxQuantity));
            int maxQ = Math.Max(0, Math.Max(item.MinQuantity, item.MaxQuantity));
            maxQ = Math.Min(maxQ, int.MaxValue - 1);

            return minQ == maxQ ? minQ : random.Next(minQ, maxQ + 1);
        }
    }
}
