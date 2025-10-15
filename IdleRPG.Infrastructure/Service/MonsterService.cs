using IdleRPG.Application.DTOs.Monster;
using IdleRPG.Application.Interfaces;
using IdleRPG.Domain.Repositories;

namespace IdleRPG.Infrastructure.Service
{
    /// <summary>
    /// 몬스터 서비스 구현
    /// </summary>
    public class MonsterService : IMonsterService
    {
        private readonly IMonsterRepository _monsterRepository;
        private readonly Random _random;

        public MonsterService(IMonsterRepository monsterRepository)
        {
            _monsterRepository = monsterRepository;
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
            var monsters = await _monsterRepository.GetByLevelRangeAsync(minLevel, maxLevel);

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
            var allMonsters = await _monsterRepository.GetAllAsync();

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
    }
}
