using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IdleRPG.Infrastructure.Data.Seeders
{
    /// <summary>
    /// 스킬 템플릿 초기 데이터 생성기
    ///
    /// [가챠 확률 설계]
    /// - Common: 60% (10개 스킬)
    /// - Rare: 30% (7개 스킬)
    /// - Epic: 9% (4개 스킬)
    /// - Legendary: 1% (3개 스킬)
    ///
    /// [스킬 타입]
    /// - Active: 직접 사용하는 공격/지원 스킬
    /// - Passive: 자동으로 적용되는 버프 스킬
    /// </summary>
    public class SkillTemplateSeeder
    {
        private readonly GameDBContext _context;
        private readonly ILogger<SkillTemplateSeeder> _logger;

        public SkillTemplateSeeder(GameDBContext context, ILogger<SkillTemplateSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 스킬 템플릿 시드 데이터 생성
        /// </summary>
        public async Task SeedAsync()
        {
            // 이미 데이터가 있으면 스킵
            if (await _context.SkillTemplates.AnyAsync())
            {
                _logger.LogInformation("스킬 템플릿 데이터가 이미 존재합니다. Seed 작업을 건너뜁니다.");
                return;
            }

            _logger.LogInformation("스킬 템플릿 시드 데이터 생성 시작...");

            var skills = new List<SkillTemplate>();

            // ========================================
            // Common 스킬 (60% 확률) - 10개
            // ========================================
            skills.Add(new SkillTemplate
            {
                Id = 1,
                Name = "화염구",
                Rarity = SkillRarity.Common,
                Type = SkillType.Active,
                Description = "작은 화염구를 발사하여 적에게 피해를 입힙니다."
            });

            skills.Add(new SkillTemplate
            {
                Id = 2,
                Name = "치유",
                Rarity = SkillRarity.Common,
                Type = SkillType.Active,
                Description = "체력을 소량 회복합니다."
            });

            skills.Add(new SkillTemplate
            {
                Id = 3,
                Name = "강타",
                Rarity = SkillRarity.Common,
                Type = SkillType.Active,
                Description = "무기로 강하게 내리쳐 피해를 입힙니다."
            });

            skills.Add(new SkillTemplate
            {
                Id = 4,
                Name = "방어 태세",
                Rarity = SkillRarity.Common,
                Type = SkillType.Passive,
                Description = "방어력이 10% 증가합니다."
            });

            skills.Add(new SkillTemplate
            {
                Id = 5,
                Name = "빠른 발놀림",
                Rarity = SkillRarity.Common,
                Type = SkillType.Passive,
                Description = "회피율이 5% 증가합니다."
            });

            skills.Add(new SkillTemplate
            {
                Id = 6,
                Name = "독 화살",
                Rarity = SkillRarity.Common,
                Type = SkillType.Active,
                Description = "독이 묻은 화살을 발사하여 지속 피해를 입힙니다."
            });

            skills.Add(new SkillTemplate
            {
                Id = 7,
                Name = "마나 회복",
                Rarity = SkillRarity.Common,
                Type = SkillType.Active,
                Description = "마나를 소량 회복합니다."
            });

            skills.Add(new SkillTemplate
            {
                Id = 8,
                Name = "전투 본능",
                Rarity = SkillRarity.Common,
                Type = SkillType.Passive,
                Description = "공격력이 5% 증가합니다."
            });

            skills.Add(new SkillTemplate
            {
                Id = 9,
                Name = "얼음 화살",
                Rarity = SkillRarity.Common,
                Type = SkillType.Active,
                Description = "얼음 화살을 발사하여 적의 이동 속도를 감소시킵니다."
            });

            skills.Add(new SkillTemplate
            {
                Id = 10,
                Name = "정신 집중",
                Rarity = SkillRarity.Common,
                Type = SkillType.Passive,
                Description = "치명타 확률이 3% 증가합니다."
            });

            // ========================================
            // Rare 스킬 (30% 확률) - 7개
            // ========================================
            skills.Add(new SkillTemplate
            {
                Id = 11,
                Name = "연쇄 번개",
                Rarity = SkillRarity.Rare,
                Type = SkillType.Active,
                Description = "번개가 여러 적에게 연쇄적으로 피해를 입힙니다."
            });

            skills.Add(new SkillTemplate
            {
                Id = 12,
                Name = "광역 치유",
                Rarity = SkillRarity.Rare,
                Type = SkillType.Active,
                Description = "주변 모든 아군의 체력을 회복합니다."
            });

            skills.Add(new SkillTemplate
            {
                Id = 13,
                Name = "회전 베기",
                Rarity = SkillRarity.Rare,
                Type = SkillType.Active,
                Description = "주변의 모든 적에게 피해를 입힙니다."
            });

            skills.Add(new SkillTemplate
            {
                Id = 14,
                Name = "강철 피부",
                Rarity = SkillRarity.Rare,
                Type = SkillType.Passive,
                Description = "받는 피해가 15% 감소합니다."
            });

            skills.Add(new SkillTemplate
            {
                Id = 15,
                Name = "흡혈",
                Rarity = SkillRarity.Rare,
                Type = SkillType.Passive,
                Description = "가한 피해의 10%만큼 체력을 회복합니다."
            });

            skills.Add(new SkillTemplate
            {
                Id = 16,
                Name = "화염 폭풍",
                Rarity = SkillRarity.Rare,
                Type = SkillType.Active,
                Description = "화염 폭풍을 일으켜 광범위한 지역에 피해를 입힙니다."
            });

            skills.Add(new SkillTemplate
            {
                Id = 17,
                Name = "전투 광기",
                Rarity = SkillRarity.Rare,
                Type = SkillType.Passive,
                Description = "공격 속도가 20% 증가합니다."
            });

            // ========================================
            // Epic 스킬 (9% 확률) - 4개
            // ========================================
            skills.Add(new SkillTemplate
            {
                Id = 18,
                Name = "메테오",
                Rarity = SkillRarity.Epic,
                Type = SkillType.Active,
                Description = "하늘에서 거대한 운석을 떨어뜨려 막대한 피해를 입힙니다."
            });

            skills.Add(new SkillTemplate
            {
                Id = 19,
                Name = "부활",
                Rarity = SkillRarity.Epic,
                Type = SkillType.Passive,
                Description = "사망 시 1회 부활합니다 (전투당 1회)."
            });

            skills.Add(new SkillTemplate
            {
                Id = 20,
                Name = "신성한 축복",
                Rarity = SkillRarity.Epic,
                Type = SkillType.Active,
                Description = "모든 능력치가 일정 시간 동안 크게 증가합니다."
            });

            skills.Add(new SkillTemplate
            {
                Id = 21,
                Name = "광전사의 분노",
                Rarity = SkillRarity.Epic,
                Type = SkillType.Passive,
                Description = "체력이 낮을수록 공격력이 증가합니다 (최대 50%)."
            });

            // ========================================
            // Legendary 스킬 (1% 확률) - 3개
            // ========================================
            skills.Add(new SkillTemplate
            {
                Id = 22,
                Name = "시간 정지",
                Rarity = SkillRarity.Legendary,
                Type = SkillType.Active,
                Description = "모든 적의 시간을 정지시켜 행동 불능 상태로 만듭니다."
            });

            skills.Add(new SkillTemplate
            {
                Id = 23,
                Name = "절대 방어",
                Rarity = SkillRarity.Legendary,
                Type = SkillType.Passive,
                Description = "치명적인 피해를 받을 때 1회 무적 상태가 됩니다 (전투당 1회)."
            });

            skills.Add(new SkillTemplate
            {
                Id = 24,
                Name = "신의 심판",
                Rarity = SkillRarity.Legendary,
                Type = SkillType.Active,
                Description = "신성한 빛으로 모든 적을 심판하여 즉사시킬 확률이 있습니다."
            });

            // DB에 저장
            await _context.SkillTemplates.AddRangeAsync(skills);
            await _context.SaveChangesAsync();

            _logger.LogInformation("스킬 템플릿 {Count}개 생성 완료 (Common: 10, Rare: 7, Epic: 4, Legendary: 3)",
                skills.Count);
        }
    }
}
