using IdleRPG.Application.Character.Services;
using IdleRPG.Application.DTOs.Battle;
using IdleRPG.Application.DTOs.Characters;
using IdleRPG.Application.DTOs.Rewards;
using IdleRPG.Application.Interfaces;
using IdleRPG.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace IdleRPG.Infrastructure.Service
{
    /// <summary>
    /// Priority Queue 기반 Event-driven 전투 시뮬레이션 서비스
    /// 보상 지급 및 레벨업 로직 포함 (Option A: 통합 방식)
    /// </summary>
    public class BattleService : IBattleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICharacterService _characterService;
        private readonly ILogger<BattleService> _logger;
        private readonly Random _random = new Random();

        public BattleService(
            IUnitOfWork unitOfWork,
            ICharacterService characterService,
            ILogger<BattleService> logger)
        {
            _unitOfWork = unitOfWork;
            _characterService = characterService;
            _logger = logger;
        }

        public async Task<BattleResultResponse> SimulateBattleAsync(Guid characterId, Guid monsterId)
        {
            // 1. 엔티티 로드
            var character = await _unitOfWork.Characters.GetByIdAsync(characterId);
            if (character == null)
                throw new InvalidOperationException("캐릭터를 찾을 수 없습니다");

            var monster = await _unitOfWork.Monsters.GetByIdAsync(monsterId);
            if (monster == null)
                throw new InvalidOperationException("몬스터를 찾을 수 없습니다");

            // 2. 전투 시뮬레이션 실행
            var result = SimulateCombat(character, monster);

            // 3. 승리 시 보상 자동 지급 (경험치 + 골드 + 레벨업)
            CharacterDto? updatedCharacter = null;
            if (result.IsVictory && result.Reward != null)
            {
                updatedCharacter = await ApplyRewardAsync(character, result.Reward);
                result.UpdatedCharacter = updatedCharacter;
            }

            _logger.LogInformation(
                "전투 완료 - 캐릭터: {CharacterId}, 몬스터: {MonsterId}, 승리: {IsVictory}, 경험치: {Exp}, 골드: {Gold}",
                characterId, monsterId, result.IsVictory, result.Reward?.Experience ?? 0, result.Reward?.Gold ?? 0);

            // 4. 전투 로그 저장
            var battleLog = new BattleLog
            {
                CharacterId = characterId,
                MonsterId = monsterId,
                IsVictory = result.IsVictory,
                ExperienceGained = (int)(result.Reward?.Experience ?? 0),
                GoldGained = (int)(result.Reward?.Gold ?? 0),
                DamageDealt = result.Statistics.TotalDamageDealt,
                DamageTaken = result.Statistics.TotalDamageTaken,
                BattleDate = DateTime.UtcNow
            };

            await _unitOfWork.BattleLogs.AddAsync(battleLog);
            await _unitOfWork.SaveChangesAsync();

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
        /// 보상 지급 및 레벨업 처리 (한 트랜잭션으로 처리)
        /// </summary>
        private async Task<CharacterDto> ApplyRewardAsync(Character character, RewardDto reward)
        {
            // 1. 경험치 지급 및 레벨업 (SaveChanges 없음)
            _characterService.ProcessExperienceGain(character, (int)reward.Experience);

            // 2. 골드 지급
            character.Gold += reward.Gold;
            character.UpdatedAt = DateTime.UtcNow;

            // 3. 한 번의 트랜잭션으로 모든 변경사항 저장
            await _unitOfWork.SaveChangesAsync();

            // 4. 최종 업데이트된 정보 DTO로 반환
            var updatedCharacter = await _characterService.GetCharacterByIdAsync(character.Id);
            return updatedCharacter!;
        }

        /// <summary>
        /// 전투 이벤트 (캐릭터 또는 몬스터의 공격)
        /// </summary>
        private class CombatEvent
        {
            public bool IsCharacterAttack { get; set; }
        }

        /// <summary>
        /// 캐릭터의 전투 히스토리 조회 (페이징)
        /// </summary>
        public async Task<BattleLogsResponse> GetBattleLogsAsync(Guid characterId, int page, int pageSize)
        {
            var logs = await _unitOfWork.BattleLogs.GetByCharacterIdAsync(characterId, page, pageSize);

            // Entity → DTO 변환
            var logDtos = logs.Select(log => new BattleLogDto
            {
                Id = log.Id,
                MonsterName = log.Monster.Name,
                MonsterLevel = log.Monster.Level,
                IsVictory = log.IsVictory,
                ExperienceGained = log.ExperienceGained,
                GoldGained = log.GoldGained,
                DamageDealt = log.DamageDealt,
                DamageTaken = log.DamageTaken,
                BattleDate = log.BattleDate
            }).ToList();

            // 전체 로그 수 계산 (페이징 정보용)
            // TODO: Repository에 카운트 메서드 추가 고려
            var allLogs = await _unitOfWork.BattleLogs.FindAsync(bl => bl.CharacterId == characterId);
            int totalCount = allLogs.Count();

            return new BattleLogsResponse
            {
                Logs = logDtos,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        /// <summary>
        /// 캐릭터의 최근 N개 전투 로그 조회
        /// </summary>
        public async Task<List<BattleLogDto>> GetRecentBattleLogsAsync(Guid characterId, int count)
        {
            var logs = await _unitOfWork.BattleLogs.GetRecentByCharacterIdAsync(characterId, count);

            // Entity → DTO 변환
            return logs.Select(log => new BattleLogDto
            {
                Id = log.Id,
                MonsterName = log.Monster.Name,
                MonsterLevel = log.Monster.Level,
                IsVictory = log.IsVictory,
                ExperienceGained = log.ExperienceGained,
                GoldGained = log.GoldGained,
                DamageDealt = log.DamageDealt,
                DamageTaken = log.DamageTaken,
                BattleDate = log.BattleDate
            }).ToList();
        }

        /// <summary>
        /// 캐릭터의 전투 통계 조회
        /// </summary>
        public async Task<Domain.Repositories.BattleStatistics> GetBattleStatsAsync(Guid characterId)
        {
            return await _unitOfWork.BattleLogs.GetStatsByCharacterIdAsync(characterId);
        }
    }
}
