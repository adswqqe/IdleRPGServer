using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IdleRPG.Infrastructure.Data.Seeders
{
    /// <summary>
    /// LootTable 및 LootItem 초기 데이터 생성기
    ///
    /// [던전 보상 설계]
    /// - Stage 1-5: Common 장비 (80%), Common 스킬 (10%)
    /// - Stage 6-10: Rare 장비 (60%), Rare 스킬 (15%)
    /// - Stage 11-15: Epic/Legendary 장비 (40%), Epic 스킬 (20%)
    ///
    /// [보상 확률 설계]
    /// - Gold/Experience: 100% 보장 (IsGuaranteed = true)
    /// - Equipment: 가중치 기반 확률 드랍
    /// - Skill: 낮은 확률의 보너스 보상
    /// </summary>
    public class LootTableSeeder
    {
        private readonly GameDBContext _context;
        private readonly ILogger<LootTableSeeder> _logger;

        public LootTableSeeder(GameDBContext context, ILogger<LootTableSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// LootTable 시드 데이터 생성
        /// </summary>
        public async Task SeedAsync()
        {
            // 이미 데이터가 있으면 스킵
            if (await _context.LootTables.AnyAsync())
            {
                _logger.LogInformation("LootTable 데이터가 이미 존재합니다. Seed 작업을 건너뜁니다.");
                return;
            }

            _logger.LogInformation("LootTable 시드 데이터 생성 시작...");

            // 1. DungeonStage 확인 (FK 참조 위해 필요)
            var stages = await _context.DungeonStages.OrderBy(s => s.Id).ToListAsync();
            if (!stages.Any())
            {
                _logger.LogWarning("던전 스테이지 데이터가 없어 LootTable을 생성할 수 없습니다.");
                return;
            }

            // 2. SkillTemplate 확인 (스킬 보상용)
            var skillTemplates = await _context.SkillTemplates.ToListAsync();
            if (!skillTemplates.Any())
            {
                _logger.LogWarning("SkillTemplate 데이터가 없어 스킬 보상을 추가할 수 없습니다.");
            }

            // 3. LootTable 및 LootItem 생성
            int lootTableId = 1;
            foreach (var stage in stages)
            {
                var lootTable = CreateLootTableForStage(stage, lootTableId, skillTemplates);
                await _context.LootTables.AddAsync(lootTable);

                // DungeonStage에 LootTable 연결
                stage.LootTableId = lootTableId;
                _context.DungeonStages.Update(stage);

                lootTableId++;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("LootTable {Count}개 생성 완료", stages.Count);
        }

        /// <summary>
        /// 던전 스테이지별 LootTable 생성
        /// </summary>
        private LootTable CreateLootTableForStage(
            DungeonStage stage,
            int lootTableId,
            List<SkillTemplate> skillTemplates)
        {
            var lootTable = new LootTable
            {
                Id = lootTableId,
                Name = $"{stage.Name} 보상",
                NumberOfRolls = 1,
                Items = new List<LootItem>()
            };

            // 보상 난이도 구간 결정
            int stageId = stage.Id;
            EquipmentRarity primaryRarity;
            EquipmentRarity secondaryRarity;
            SkillRarity skillRarity;

            if (stageId <= 5)
            {
                // 초보 던전 (Stage 1-5)
                primaryRarity = EquipmentRarity.Common;
                secondaryRarity = EquipmentRarity.Rare;
                skillRarity = SkillRarity.Common;
            }
            else if (stageId <= 10)
            {
                // 중급 던전 (Stage 6-10)
                primaryRarity = EquipmentRarity.Rare;
                secondaryRarity = EquipmentRarity.Epic;
                skillRarity = SkillRarity.Rare;
            }
            else
            {
                // 고급 던전 (Stage 11-15)
                primaryRarity = EquipmentRarity.Epic;
                secondaryRarity = EquipmentRarity.Legendary;
                skillRarity = SkillRarity.Epic;
            }

            // 1. Gold 보상 (100% 보장)
            lootTable.Items.Add(new LootItem
            {
                LootTableId = lootTableId,
                Type = RewardType.Gold,
                IsGuaranteed = true,
                Weight = 0,
                MinQuantity = stage.BaseGold,
                MaxQuantity = stage.BaseGold
            });

            // 2. Experience 보상 (100% 보장)
            lootTable.Items.Add(new LootItem
            {
                LootTableId = lootTableId,
                Type = RewardType.Experience,
                IsGuaranteed = true,
                Weight = 0,
                MinQuantity = stage.BaseExperience,
                MaxQuantity = stage.BaseExperience
            });

            // 3. 장비 드랍 (확률 기반)
            // 주 희귀도 장비 (높은 확률)
            AddEquipmentLoot(lootTable, lootTableId, primaryRarity, weight: 70);

            // 부 희귀도 장비 (낮은 확률)
            AddEquipmentLoot(lootTable, lootTableId, secondaryRarity, weight: 30);

            // 4. 스킬 드랍 (낮은 확률의 보너스 보상)
            if (skillTemplates.Any())
            {
                var suitableSkills = skillTemplates
                    .Where(s => s.Rarity == skillRarity)
                    .ToList();

                if (suitableSkills.Any())
                {
                    // 랜덤하게 2-3개의 스킬을 보상 풀에 추가
                    var skillCount = Math.Min(3, suitableSkills.Count);
                    var selectedSkills = suitableSkills.OrderBy(_ => Guid.NewGuid()).Take(skillCount);

                    foreach (var skill in selectedSkills)
                    {
                        lootTable.Items.Add(new LootItem
                        {
                            LootTableId = lootTableId,
                            Type = RewardType.Skill,
                            ItemTemplateId = skill.Id,
                            IsGuaranteed = false,
                            Weight = 15, // 낮은 확률
                            MinQuantity = 1,
                            MaxQuantity = 1
                        });
                    }
                }
            }

            return lootTable;
        }

        /// <summary>
        /// 특정 희귀도의 모든 장비 슬롯 LootItem 추가
        /// </summary>
        private void AddEquipmentLoot(
            LootTable lootTable,
            int lootTableId,
            EquipmentRarity rarity,
            int weight)
        {
            // 모든 장비 슬롯에 대해 LootItem 생성
            var slots = new[]
            {
                EquipmentSlot.Weapon,
                EquipmentSlot.Helmet,
                EquipmentSlot.Armor,
                EquipmentSlot.Gloves,
                EquipmentSlot.Boots
            };

            foreach (var slot in slots)
            {
                lootTable.Items.Add(new LootItem
                {
                    LootTableId = lootTableId,
                    Type = RewardType.Equipment,
                    EquipmentSlot = slot,
                    EquipmentRarity = rarity,
                    IsGuaranteed = false,
                    Weight = weight,
                    MinQuantity = 1,
                    MaxQuantity = 1
                });
            }
        }
    }
}
