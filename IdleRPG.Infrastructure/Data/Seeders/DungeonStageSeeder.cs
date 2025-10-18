using IdleRPG.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IdleRPG.Infrastructure.Data.Seeders
{
    /// <summary>
    /// 던전 스테이지 초기 데이터 생성기
    ///
    /// [게임 밸런스 설계]
    /// - Stage 1-5: 초보 던전 (Lv 1-10 몬스터)
    /// - Stage 6-10: 중급 던전 (Lv 11-20 몬스터)
    /// - Stage 11-15: 고급 던전 (Lv 21-30 몬스터)
    ///
    /// [보상 공식]
    /// - BaseGold = RequiredLevel × 100
    /// - BaseExperience = RequiredLevel × 50
    /// - 난이도 배수는 DifficultyMultiplier에서 자동 적용
    /// </summary>
    public class DungeonStageSeeder
    {
        private readonly GameDBContext _context;
        private readonly ILogger<DungeonStageSeeder> _logger;

        public DungeonStageSeeder(GameDBContext context, ILogger<DungeonStageSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 던전 스테이지 시드 데이터 생성
        /// </summary>
        public async Task SeedAsync()
        {
            // 이미 데이터가 있으면 스킵
            if (await _context.DungeonStages.AnyAsync())
            {
                _logger.LogInformation("던전 스테이지 데이터가 이미 존재합니다. Seed 작업을 건너뜁니다.");
                return;
            }

            _logger.LogInformation("던전 스테이지 시드 데이터 생성 시작...");

            // 1. 몬스터 데이터 확인 (FK 참조 위해 필요)
            var monsters = await _context.Monsters.OrderBy(m => m.Level).ToListAsync();
            if (!monsters.Any())
            {
                _logger.LogWarning("몬스터 데이터가 없어 던전 스테이지를 생성할 수 없습니다. 먼저 MonsterSeeder를 실행하세요.");
                return;
            }

            // 2. 던전 스테이지 생성
            var stages = new List<DungeonStage>();

            // Stage 1-5: 초보 던전
            stages.Add(CreateStage(1, "슬라임의 동굴", 1, monsters, 100, 50));
            stages.Add(CreateStage(2, "숲의 입구", 3, monsters, 300, 150));
            stages.Add(CreateStage(3, "고블린 마을", 5, monsters, 500, 250));
            stages.Add(CreateStage(4, "어두운 숲", 7, monsters, 700, 350));
            stages.Add(CreateStage(5, "버려진 광산", 9, monsters, 900, 450));

            // Stage 6-10: 중급 던전
            stages.Add(CreateStage(6, "오크 주둔지", 11, monsters, 1100, 550));
            stages.Add(CreateStage(7, "독거미 둥지", 13, monsters, 1300, 650));
            stages.Add(CreateStage(8, "좀비 묘지", 15, monsters, 1500, 750));
            stages.Add(CreateStage(9, "고대 유적", 17, monsters, 1700, 850));
            stages.Add(CreateStage(10, "용암 동굴", 19, monsters, 1900, 950));

            // Stage 11-15: 고급 던전
            stages.Add(CreateStage(11, "얼음 성채", 21, monsters, 2100, 1050));
            stages.Add(CreateStage(12, "어둠의 탑", 23, monsters, 2300, 1150));
            stages.Add(CreateStage(13, "드래곤 둥지", 25, monsters, 2500, 1250));
            stages.Add(CreateStage(14, "악마의 제단", 27, monsters, 2700, 1350));
            stages.Add(CreateStage(15, "최종 관문", 30, monsters, 3000, 1500));

            // 3. DB에 저장
            await _context.DungeonStages.AddRangeAsync(stages);
            await _context.SaveChangesAsync();

            _logger.LogInformation("던전 스테이지 {Count}개 생성 완료", stages.Count);
        }

        /// <summary>
        /// 던전 스테이지 생성 헬퍼 메서드
        /// </summary>
        private DungeonStage CreateStage(
            int id,
            string name,
            int requiredLevel,
            List<Monster> monsters,
            int baseGold,
            int baseExperience)
        {
            // 요구 레벨에 맞는 몬스터 선택 (레벨 ±2 범위)
            var suitableMonster = monsters
                .Where(m => Math.Abs(m.Level - requiredLevel) <= 2)
                .OrderBy(m => Math.Abs(m.Level - requiredLevel))
                .FirstOrDefault() ?? monsters.First();

            return new DungeonStage
            {
                Id = id,
                Name = name,
                RequiredLevel = requiredLevel,
                MonsterId = suitableMonster.Id,
                BaseGold = baseGold,
                BaseExperience = baseExperience,
                // FirstClearBonus는 nullable이므로 null로 시작 (추후 기능 확장 시 추가)
                FirstClearBonusGold = null,
                FirstClearBonusExp = null
            };
        }
    }
}
