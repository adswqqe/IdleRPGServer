using IdleRPG.Application.Combat.Services;
using IdleRPG.Application.DTOs.Battle;
using IdleRPG.Application.Interfaces;
using IdleRPG.Domain.Entities;
using IdleRPG.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using DungeonDifficultyEnum = IdleRPG.Domain.Enums.DungeonDifficulty;

namespace IdleRPG.Infrastructure.Service
{
    /// <summary>
    /// Priority Queue 기반 Event-driven 전투 시뮬레이션 서비스
    ///
    /// 책임:
    /// - 순수 전투 계산만 수행 (Stateless)
    /// - 보상 지급, BattleLog 저장, 트랜잭션 관리 제외
    ///
    /// 특징:
    /// - Repository를 통해 Character/Monster 조회
    /// - 난이도 배율 적용
    /// - Priority Queue 기반 실시간 전투 시뮬레이션
    /// - DB SaveChanges 호출 안 함 (호출자가 트랜잭션 관리)
    ///
    /// 재사용:
    /// - StageService (메인 스테이지)
    /// - SpecialDungeonService (보스 던전, Phase 3)
    /// - PVPService (PVP, Phase 2)
    /// </summary>
    public class CombatService : ICombatService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CombatService> _logger;
        private readonly Random _random = new Random();

        public CombatService(
            IUnitOfWork unitOfWork,
            ILogger<CombatService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<CombatResultDto> SimulateCombatAsync(
            Guid characterId,
            Guid monsterId,
            DungeonDifficultyEnum? difficulty = null)
        {
            // 1. 엔티티 로드
            var character = await _unitOfWork.Characters.GetByIdAsync(characterId);
            if (character == null)
                throw new InvalidOperationException("캐릭터를 찾을 수 없습니다");

            var monster = await _unitOfWork.Monsters.GetByIdAsync(monsterId);
            if (monster == null)
                throw new InvalidOperationException("몬스터를 찾을 수 없습니다");

            // 2. 난이도 배율 적용 (던전 전투인 경우)
            var combatMonster = difficulty.HasValue
                ? ApplyDifficultyMultiplier(monster, difficulty.Value)
                : monster;

            // 3. 전투 시뮬레이션 실행
            var result = SimulateCombat(character, combatMonster);

            _logger.LogInformation(
                "전투 완료 - 캐릭터: {CharacterId}, 몬스터: {MonsterId}, 난이도: {Difficulty}, 승리: {IsVictory}",
                characterId, monsterId, difficulty?.ToString() ?? "Normal", result.IsVictory);

            return result;
        }

        /// <summary>
        /// 난이도 배율을 적용한 Monster 인스턴스 생성 (원본 보존)
        /// </summary>
        private Monster ApplyDifficultyMultiplier(Monster original, DungeonDifficultyEnum difficulty)
        {
            var multiplier = DifficultyMultiplier.Create(difficulty);

            // 원본을 보존하기 위해 새로운 인스턴스 생성
            return new Monster
            {
                Id = original.Id,
                Name = original.Name,
                Level = original.Level,
                MaxHealth = (int)(original.MaxHealth * multiplier.MonsterStatMultiplier),
                Attack = (int)(original.Attack * multiplier.MonsterStatMultiplier),
                Defense = (int)(original.Defense * multiplier.MonsterStatMultiplier),
                AttackSpeed = original.AttackSpeed,
                CritRate = original.CritRate,
                CritDamage = original.CritDamage,
                Evasion = original.Evasion
            };
        }

        /// <summary>
        /// Priority Queue 기반 Event-driven 전투 시뮬레이션
        /// </summary>
        private CombatResultDto SimulateCombat(Character character, Monster monster)
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

            return new CombatResultDto
            {
                IsVictory = isVictory,
                Statistics = statistics
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
        /// 전투 이벤트 (캐릭터 또는 몬스터의 공격)
        /// </summary>
        private class CombatEvent
        {
            public bool IsCharacterAttack { get; set; }
        }
    }
}
