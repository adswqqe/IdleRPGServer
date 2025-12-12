using IdleRPG.Domain.Enums;

namespace IdleRPG.Domain.Services
{
    /// <summary>
    /// 펫 가챠 확률 계산을 담당하는 Domain Service
    /// </summary>
    /// <remarks>
    /// Soft Pity: 40회부터 Legendary 확률 증가 (1% → 2%, 3%, ...)
    /// Hard Pity: 50회에 Legendary 보장
    /// </remarks>
    public class PetGachaService
    {
        // 가챠 확률 상수 정의
        private const int HARD_PITY_THRESHOLD = 50;      // 50회에 무조건 Legendary
        private const int SOFT_PITY_THRESHOLD = 40;      // 40회부터 Soft Pity 시작
        private const int BASE_LEGENDARY_RATE = 1;       // 1%
        private const int EPIC_RATE = 9;                 // 9%
        private const int RARE_RATE = 30;                // 30%
        // Common은 나머지 60%

        /// <summary>
        /// 천장 카운트를 기반으로 펫 가챠 희귀도를 결정합니다.
        /// </summary>
        /// <param name="pityCount">현재 천장 카운트 (0-50)</param>
        /// <param name="randomProvider">난수 생성 제공자</param>
        /// <returns>결정된 펫 희귀도</returns>
        /// <exception cref="ArgumentOutOfRangeException">pityCount가 0-50 범위를 벗어난 경우</exception>
        /// <exception cref="ArgumentNullException">randomProvider가 null인 경우</exception>
        public Rarity DrawPet(int pityCount, IRandomProvider randomProvider)
        {
            // Input Validation
            if (pityCount < 0 || pityCount > HARD_PITY_THRESHOLD)
                throw new ArgumentOutOfRangeException(nameof(pityCount),
                    $"Pity count must be between 0 and {HARD_PITY_THRESHOLD}.");

            if (randomProvider == null)
                throw new ArgumentNullException(nameof(randomProvider));

            // Hard Pity: 50회 이상이면 무조건 Legendary
            if (pityCount >= HARD_PITY_THRESHOLD)
                return Rarity.Legendary;

            // Soft Pity: 40회부터 Legendary 확률 증가
            int adjustedLegendaryRate = BASE_LEGENDARY_RATE;
            if (pityCount >= SOFT_PITY_THRESHOLD)
            {
                // 40회: 1% → 2% (1 + 1)
                // 41회: 1% → 3% (1 + 2)
                // ...
                // 49회: 1% → 11% (1 + 10)
                adjustedLegendaryRate = BASE_LEGENDARY_RATE + (pityCount - SOFT_PITY_THRESHOLD + 1);
            }

            // 0~99 범위의 난수 생성 (누적 확률 방식)
            int rand = randomProvider.Next(100);

            // Legendary: adjustedLegendaryRate% (0 ~ adjustedRate-1)
            if (rand < adjustedLegendaryRate)
                return Rarity.Legendary;

            // Epic: 9% (adjustedRate ~ adjustedRate+8)
            else if (rand < adjustedLegendaryRate + EPIC_RATE)
                return Rarity.Epic;

            // Rare: 30% (adjustedRate+9 ~ adjustedRate+38)
            else if (rand < adjustedLegendaryRate + EPIC_RATE + RARE_RATE)
                return Rarity.Rare;

            // Common: 나머지 (adjustedRate+39 ~ 99)
            else
                return Rarity.Common;
        }

        /// <summary>
        /// 뽑기 후 새로운 천장 카운트를 계산합니다.
        /// </summary>
        /// <param name="drawnRarity">뽑기로 획득한 펫의 희귀도</param>
        /// <param name="currentPityCount">현재 천장 카운트</param>
        /// <returns>조정된 새로운 천장 카운트 (Legendary 획득 시 0, 아니면 +1)</returns>
        public int GetPityCountAfterDraw(Rarity drawnRarity, int currentPityCount)
        {
            // Legendary 획득 시 천장 카운터 초기화
            if (drawnRarity == Rarity.Legendary)
                return 0;
            else
                return currentPityCount + 1;
        }
    }
}
