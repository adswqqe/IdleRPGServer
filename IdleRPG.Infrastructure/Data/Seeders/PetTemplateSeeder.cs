using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IdleRPG.Infrastructure.Data.Seeders
{
    /// <summary>
    /// 펫 템플릿 초기 데이터 생성기
    ///
    /// [가챠 확률 설계]
    /// - Common: 60% (5개 펫)
    /// - Rare: 30% (3개 펫)
    /// - Epic: 9% (2개 펫)
    /// - Legendary: 1% (1개 펫)
    ///
    /// [펫 테마]
    /// - 판타지 동물/몬스터 (Slime, Wolf, Dragon 등)
    /// - 스탯 밸런스: Attack 기준, Mana는 Attack의 50% 수준
    /// </summary>
    public class PetTemplateSeeder
    {
        private readonly GameDBContext _context;
        private readonly ILogger<PetTemplateSeeder> _logger;

        public PetTemplateSeeder(GameDBContext context, ILogger<PetTemplateSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 펫 템플릿 시드 데이터 생성 (Idempotent)
        /// </summary>
        public async Task SeedAsync()
        {
            // 이미 데이터가 있으면 스킵 (Idempotent 보장)
            if (await _context.Set<PetTemplate>().AnyAsync())
            {
                _logger.LogInformation("펫 템플릿 데이터가 이미 존재합니다. Seed 작업을 건너뜁니다.");
                return;
            }

            _logger.LogInformation("펫 템플릿 시드 데이터 생성 시작...");

            var petTemplates = new List<PetTemplate>();

            // ========================================
            // Common 펫 (60% 확률) - 5개
            // ========================================
            petTemplates.Add(new PetTemplate
            {
                Id = 1,
                Name = "Slime",
                Rarity = Rarity.Common,
                BaseAttack = 50,
                BaseMana = 25
            });

            petTemplates.Add(new PetTemplate
            {
                Id = 2,
                Name = "Wolf",
                Rarity = Rarity.Common,
                BaseAttack = 60,
                BaseMana = 20
            });

            petTemplates.Add(new PetTemplate
            {
                Id = 3,
                Name = "Bat",
                Rarity = Rarity.Common,
                BaseAttack = 55,
                BaseMana = 30
            });

            petTemplates.Add(new PetTemplate
            {
                Id = 4,
                Name = "Goblin",
                Rarity = Rarity.Common,
                BaseAttack = 65,
                BaseMana = 15
            });

            petTemplates.Add(new PetTemplate
            {
                Id = 5,
                Name = "Rabbit",
                Rarity = Rarity.Common,
                BaseAttack = 45,
                BaseMana = 35
            });

            // ========================================
            // Rare 펫 (30% 확률) - 3개
            // ========================================
            petTemplates.Add(new PetTemplate
            {
                Id = 6,
                Name = "Fire Fox",
                Rarity = Rarity.Rare,
                BaseAttack = 100,
                BaseMana = 50
            });

            petTemplates.Add(new PetTemplate
            {
                Id = 7,
                Name = "Ice Wolf",
                Rarity = Rarity.Rare,
                BaseAttack = 110,
                BaseMana = 45
            });

            petTemplates.Add(new PetTemplate
            {
                Id = 8,
                Name = "Thunder Eagle",
                Rarity = Rarity.Rare,
                BaseAttack = 105,
                BaseMana = 55
            });

            // ========================================
            // Epic 펫 (9% 확률) - 2개
            // ========================================
            petTemplates.Add(new PetTemplate
            {
                Id = 9,
                Name = "Dark Dragon",
                Rarity = Rarity.Epic,
                BaseAttack = 200,
                BaseMana = 100
            });

            petTemplates.Add(new PetTemplate
            {
                Id = 10,
                Name = "Light Phoenix",
                Rarity = Rarity.Epic,
                BaseAttack = 190,
                BaseMana = 110
            });

            // ========================================
            // Legendary 펫 (1% 확률) - 1개
            // ========================================
            petTemplates.Add(new PetTemplate
            {
                Id = 11,
                Name = "Ancient Guardian",
                Rarity = Rarity.Legendary,
                BaseAttack = 350,
                BaseMana = 200
            });

            // DB에 저장
            await _context.Set<PetTemplate>().AddRangeAsync(petTemplates);
            await _context.SaveChangesAsync();

            _logger.LogInformation("펫 템플릿 {Count}개 생성 완료 (Common: 5, Rare: 3, Epic: 2, Legendary: 1)",
                petTemplates.Count);
        }
    }
}
