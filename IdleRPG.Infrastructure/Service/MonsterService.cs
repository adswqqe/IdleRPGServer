using IdleRPG.Application.DTOs.Monster;
using IdleRPG.Application.Interfaces;

namespace IdleRPG.Infrastructure.Service
{
    /// <summary>
    /// 몬스터 서비스 구현
    /// </summary>
    public class MonsterService : IMonsterService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly Random _random;

        public MonsterService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _random = new Random();
        }

        /// <summary>
        /// 레벨 범위 내에서 랜덤 몬스터를 선택합니다.
        /// 범위 내 몬스터가 없으면 가장 높은 레벨의 몬스터를 반환합니다.
        /// </summary>
        public async Task<RandomMonsterResponse> GetRandomMonsterAsync(int minLevel, int maxLevel)
        {
            // 1. 입력 검증
            if (minLevel > maxLevel)
            {
                throw new ArgumentException("최소 레벨은 최대 레벨보다 클 수 없습니다.");
            }

            // 2. 레벨 범위 내 몬스터 조회
            var monsters = await _unitOfWork.Monsters.GetByLevelRangeAsync(minLevel, maxLevel);

            // 3. 범위 내 몬스터가 있으면 랜덤 선택
            if (monsters.Any())
            {
                var selectedMonster = monsters[_random.Next(monsters.Count)];
                return new RandomMonsterResponse
                {
                    MonsterId = selectedMonster.Id,
                    Level = selectedMonster.Level
                };
            }

            // 4. 범위 내 몬스터가 없으면 가장 높은 레벨의 몬스터 반환
            var allMonsters = await _unitOfWork.Monsters.GetAllAsync();

            if (!allMonsters.Any())
            {
                throw new InvalidOperationException("데이터베이스에 몬스터가 존재하지 않습니다.");
            }

            var highestLevelMonster = allMonsters.OrderByDescending(m => m.Level).First();
            return new RandomMonsterResponse
            {
                MonsterId = highestLevelMonster.Id,
                Level = highestLevelMonster.Level
            };
        }

        /// <summary>
        /// 모든 몬스터 목록을 조회합니다.
        /// Unity 클라이언트의 초기 로딩 시 사용됩니다.
        /// </summary>
        public async Task<List<MonsterDto>> GetAllMonstersAsync()
        {
            var monsters = await _unitOfWork.Monsters.GetAllAsync();

            return monsters.Select(m => new MonsterDto
            {
                Id = m.Id,
                Name = m.Name,
                Level = m.Level,
                MaxHealth = m.MaxHealth,
                Attack = m.Attack,
                Defense = m.Defense,
                AttackSpeed = m.AttackSpeed,
                CritRate = m.CritRate,
                CritDamage = m.CritDamage,
                Evasion = m.Evasion,
                ExperienceReward = m.ExperienceReward,
                GoldReward = m.GoldReward
            }).ToList();
        }
    }
}
