using System;
using System.Collections.Generic;
using System.Linq;
using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Enums;

namespace IdleRPG.Domain.Services
{
    public class GachaLogicService
    {
        private readonly IRandomProvider _randomProvider;

        // 가챠 확률 상수 정의
        private const int PITY_THRESHOLD = 100;
        private const int LEGENDARY_RATE = 1;    // 1%
        private const int EPIC_RATE = 9;         // 9%
        private const int RARE_RATE = 30;        // 30%
        // Common은 나머지 60%

        /// <summary>
        /// GachaLogicService 생성자
        /// </summary>
        /// <param name="randomProvider">난수 생성 제공자</param>
        public GachaLogicService(IRandomProvider randomProvider)
        {
            _randomProvider = randomProvider ?? throw new ArgumentNullException(nameof(randomProvider));
        }

        /// <summary>
        /// 천장 카운트를 기반으로 가챠 희귀도를 결정합니다.
        /// </summary>
        /// <param name="pityCount">현재 천장 카운트</param>
        /// <returns>결정된 스킬 희귀도</returns>
        public SkillRarity DetermineRarity(int pityCount)
        {
            // 천장 시스템: 100회 이상이면 무조건 Legendary
            if (pityCount >= PITY_THRESHOLD)
                return SkillRarity.Legendary;
            
            // 0~99 범위의 난수 생성 (누적 확률 방식)
            int rand = _randomProvider.Next(100);

            // Legendary: 1% (0)
            if (rand < LEGENDARY_RATE)
                return SkillRarity.Legendary;
            
            // Epic: 9% (1~9)
            else if (rand < LEGENDARY_RATE + EPIC_RATE)
                return SkillRarity.Epic;
            
            // Rare: 30% (10~39)
            else if (rand < LEGENDARY_RATE + EPIC_RATE + RARE_RATE)
                return SkillRarity.Rare;
            
            // Common: 60% (40~99)
            else
                return SkillRarity.Common;
        }

        /// <summary>
        /// 주어진 희귀도의 스킬 목록에서 무작위로 하나의 스킬을 선택합니다.
        /// </summary>
        /// <param name="rarity">선택할 스킬의 희귀도</param>
        /// <param name="availableSkills">해당 희귀도를 가진 사용 가능한 스킬 목록</param>
        /// <returns>무작위로 선택된 스킬 템플릿</returns>
        public SkillTemplate SelectRandomSkill(SkillRarity rarity, IEnumerable<SkillTemplate> availableSkills)
        {
            // 주어진 희귀도에 해당하는 스킬만 필터링
            var skills = availableSkills
                .Where(s => s.Rarity == rarity)
                .ToList();
                
            if (!skills.Any())
            {
                throw new ArgumentException(
                    $"선택할 수 있는 '{rarity}' 등급의 스킬이 없습니다.", 
                    nameof(availableSkills));
            }

            return skills[_randomProvider.Next(skills.Count)];
        }

        /// <summary>
        /// 뽑기 후 새로운 천장 카운트를 계산합니다.
        /// </summary>
        /// <param name="drawnRarity">뽑기로 획득한 스킬의 희귀도</param>
        /// <param name="currentPityCount">현재 천장 카운트</param>
        /// <returns>조정된 새로운 천장 카운트</returns>
        public int GetPityCountAfterDraw(SkillRarity drawnRarity, int currentPityCount)
        {
            if (drawnRarity == SkillRarity.Legendary)
                return 0;
            else
                return currentPityCount + 1;
        }
    }
}
