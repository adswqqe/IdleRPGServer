using IdleRPG.Application.DTOs.Battle;
using IdleRPG.Application.DTOs.Rewards;
using IdleRPG.Application.Interfaces;
using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace IdleRPG.Infrastructure.Service
{
    /// <summary>
    /// Priority Queue 기반 Event-driven 전투 시뮬레이션 서비스
    /// </summary>
    public class BattleService : IBattleService
    {
        private readonly ICharacterRepository _characterRepository;
        private readonly IMonsterRepository _monsterRepository;
        private readonly ILogger<BattleService> _logger;
        private readonly Random _random = new Random();

        public BattleService(
            ICharacterRepository characterRepository,
            IMonsterRepository monsterRepository,
            ILogger<BattleService> logger)
        {
            _characterRepository = characterRepository;
            _monsterRepository = monsterRepository;
            _logger = logger;
        }

        public async Task<BattleResultResponse> SimulateBattleAsync(Guid characterId, Guid monsterId)
        {
            // 1. 엔티티 로드
            var character = await _characterRepository.GetByIdAsync(characterId);
            if (character == null)
                throw new InvalidOperationException("캐릭터를 찾을 수 없습니다");

            var monster = await _monsterRepository.GetByIdAsync(monsterId);
            if (monster == null)
                throw new InvalidOperationException("몬스터를 찾을 수 없습니다");

            // 2. 전투 시뮬레이션 실행
            var result = SimulateCombat(character, monster);

            _logger.LogInformation(
                "전투 완료 - 캐릭터: {CharacterId}, 몬스터: {MonsterId}, 승리: {IsVictory}",
                characterId, monsterId, result.IsVictory);

            return result;
        }

        /// <summary>
        /// Priority Queue 기반 Event-driven 전투 시뮬레이션
        /// </summary>
        private BattleResultResponse SimulateCombat(Character character, Monster monster)
        {
            // 전투 초기화
            long characterHP = character.Stats.MaxHealth;
            long monsterHP = monster.MaxHealth;

            // 통계 수집용
            var statistics = new BattleStatisticsDto
            {
                TotalTurns = 0,
                TotalDamageDealt = 0,
                TotalDamageTaken = 0,
                CriticalHitCount = 0,
                EvasionCount = 0
            };

            // Priority Queue: (CombatEvent, 발생 시간)
            var eventQueue = new PriorityQueue<CombatEvent, float>();

            // 초기 공격 이벤트 스케줄링
            float characterAttackInterval = 1.0f / character.Stats.AttackSpeed;
            float monsterAttackInterval = 1.0f / monster.AttackSpeed;

            eventQueue.Enqueue(new CombatEvent { IsCharacterAttack = true }, characterAttackInterval);
            eventQueue.Enqueue(new CombatEvent { IsCharacterAttack = false }, monsterAttackInterval);

            float currentTime = 0f;
            const float maxBattleDuration = 300f; // PVE는 제한 없음, PVP는 30초

            // Event-driven 전투 루프
            while (characterHP > 0 && monsterHP > 0 && currentTime < maxBattleDuration)
            {
                if (eventQueue.Count == 0)
                    break;

                // 다음 이벤트 처리
                eventQueue.TryDequeue(out var combatEvent, out float eventTime);
                currentTime = eventTime;
                statistics.TotalTurns++;

                if (combatEvent.IsCharacterAttack)
                {
                    // 캐릭터 공격
                    var damage = CalculateDamage(
                        character.Stats.Attack,
                        monster.Defense,
                        character.Stats.CritRate,
                        character.Stats.CritDamage,
                        monster.Evasion,
                        out bool isCritical,
                        out bool isEvaded);

                    monsterHP -= damage;
                    statistics.TotalDamageDealt += (int)damage;

                    if (isCritical) statistics.CriticalHitCount++;
                    if (isEvaded) statistics.EvasionCount++;

                    // 다음 캐릭터 공격 스케줄링
                    if (monsterHP > 0)
                    {
                        eventQueue.Enqueue(
                            new CombatEvent { IsCharacterAttack = true },
                            currentTime + characterAttackInterval);
                    }
                }
                else
                {
                    // 몬스터 공격
                    var damage = CalculateDamage(
                        monster.Attack,
                        character.Stats.Defense,
                        monster.CritRate,
                        monster.CritDamage,
                        character.Stats.Evasion,
                        out bool isCritical,
                        out bool isEvaded);

                    characterHP -= damage;
                    statistics.TotalDamageTaken += (int)damage;

                    if (isCritical) statistics.CriticalHitCount++;
                    if (isEvaded) statistics.EvasionCount++;

                    // 다음 몬스터 공격 스케줄링
                    if (characterHP > 0)
                    {
                        eventQueue.Enqueue(
                            new CombatEvent { IsCharacterAttack = false },
                            currentTime + monsterAttackInterval);
                    }
                }
            }

            // 전투 결과 생성
            bool isVictory = monsterHP <= 0 && characterHP > 0;

            return new BattleResultResponse
            {
                IsVictory = isVictory,
                Reward = isVictory ? CalculateReward(monster) : new RewardDto(),
                Statistics = statistics,
                UpdatedCharacter = null // Controller에서 보상 지급 후 설정
            };
        }

        /// <summary>
        /// 데미지 계산: 회피 체크 → 크리티컬 체크 → 최종 데미지
        /// </summary>
        private long CalculateDamage(
            long attack,
            long defense,
            float critRate,
            float critDamage,
            float evasion,
            out bool isCritical,
            out bool isEvaded)
        {
            isCritical = false;
            isEvaded = false;

            // 1. 회피 체크 (먼저 확인)
            if (_random.NextDouble() < evasion)
            {
                isEvaded = true;
                return 0;
            }

            // 2. 기본 데미지 계산
            long baseDamage = Math.Max(1, attack - defense);

            // 3. 크리티컬 체크
            if (_random.NextDouble() < critRate)
            {
                isCritical = true;
                return (long)(baseDamage * critDamage);
            }

            return baseDamage;
        }

        /// <summary>
        /// 보상 계산 (나중에 데이터 테이블로 확장 예정)
        /// </summary>
        private RewardDto CalculateReward(Monster monster)
        {
            // TODO: 스테이지별 보상 데이터 테이블 구현
            return new RewardDto
            {
                Experience = monster.Level * 50, // 임시 공식
                Gold = monster.Level * 10        // 임시 공식
            };
        }

        /// <summary>
        /// 전투 이벤트 (캐릭터 또는 몬스터의 공격)
        /// </summary>
        private class CombatEvent
        {
            public bool IsCharacterAttack { get; set; }
        }
    }
}
