using IdleRPG.Application.BattleLog.Services;
using IdleRPG.Application.Character.Services;
using IdleRPG.Application.Combat.Services;
using IdleRPG.Application.DTOs.Combat;
using IdleRPG.Application.DTOs.Equipment;
using IdleRPG.Application.DTOs.Rewards;
using IdleRPG.Application.Interfaces;
using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Enums;
using IdleRPG.Domain.Services;
using IdleRPG.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using DungeonDifficultyEnum = IdleRPG.Domain.Enums.DungeonDifficulty;
using RewardType = IdleRPG.Domain.Enums.RewardType;

namespace IdleRPG.Infrastructure.Service
{
    /// <summary>
    /// 메인 스테이지 시스템 서비스
    ///
    /// [서버 중심 아키텍처]
    /// - 모든 비즈니스 검증을 서버에서 수행
    /// - 트랜잭션으로 진행도 + 보상 지급 원자성 보장
    /// - 동시성 제어로 치트 방지
    ///
    /// [Combat System Refactoring 반영]
    /// - 구 DungeonService → StageService로 리네이밍
    /// - ICombatService: 순수 전투 시뮬레이션
    /// - IBattleLogService: 전투 로그 생성 및 조회
    /// - StageService: 보상 지급 + 트랜잭션 관리
    /// </summary>
    public class StageService : IStageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICharacterService _characterService;
        private readonly ICombatService _combatService;
        private readonly IBattleLogService _battleLogService;
        private readonly LootCalculator _lootCalculator;
        private readonly ILogger<StageService> _logger;

        // 캐릭터별 동시성 제어 (중복 보상 지급 방지)
        private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> _characterLocks = new();

        public StageService(
            IUnitOfWork unitOfWork,
            ICharacterService characterService,
            ICombatService combatService,
            IBattleLogService battleLogService,
            LootCalculator lootCalculator,
            ILogger<StageService> logger)
        {
            _unitOfWork = unitOfWork;
            _characterService = characterService;
            _combatService = combatService;
            _battleLogService = battleLogService;
            _lootCalculator = lootCalculator;
            _logger = logger;
        }

        public async Task<List<DungeonStageDto>> GetAvailableStagesAsync(Guid characterId, DungeonDifficultyEnum? difficulty = null)
        {
            // 1. 캐릭터 정보 로드
            var character = await _unitOfWork.Characters.GetByIdAsync(characterId);
            if (character == null)
                throw new InvalidOperationException("캐릭터를 찾을 수 없습니다");

            // 2. 스테이지 진행 상황 로드 (없으면 새로 생성)
            var progress = await _unitOfWork.CharacterDungeonProgresses.GetOrCreateByCharacterIdAsync(characterId);

            // 3. 모든 스테이지 조회
            var allStages = await _unitOfWork.DungeonStages.GetAllAsync();

            // 4. 난이도가 지정되지 않으면 모든 난이도 표시 (Normal, Hard, Nightmare 각각)
            var result = new List<DungeonStageDto>();

            // 표시할 난이도 목록 결정
            var difficultiesToShow = difficulty.HasValue
                ? new[] { difficulty.Value }
                : new[] { DungeonDifficultyEnum.Normal, DungeonDifficultyEnum.Hard, DungeonDifficultyEnum.Nightmare };

            foreach (var diff in difficultiesToShow)
            {
                var multiplier = DifficultyMultiplier.Create(diff);

                foreach (var stage in allStages)
                {
                    var dto = new DungeonStageDto
                    {
                        Id = stage.Id,
                        Name = stage.Name,
                        Description = string.Empty,
                        Difficulty = diff,
                        RequiredLevel = stage.RequiredLevel,
                        BossMonsterId = stage.MonsterId,
                        BossMonsterName = stage.Monster?.Name,
                        BaseGoldReward = stage.BaseGold,
                        BaseExpReward = stage.BaseExperience
                    };

                    // 난이도별 배수 적용
                    dto.FinalGoldReward = (long)(stage.BaseGold * multiplier.RewardMultiplier);
                    dto.FinalExpReward = (long)(stage.BaseExperience * multiplier.RewardMultiplier);

                    // 도전 가능 여부 판단
                    dto.IsAvailable = IsStageAvailable(character, stage, progress, diff);

                    result.Add(dto);
                }
            }

            // 5. 난이도 → StageId 순으로 정렬
            return result.OrderBy(s => s.Difficulty).ThenBy(s => s.Id).ToList();
        }

        public async Task<CharacterMainBattleProgressDto> GetProgressAsync(Guid characterId)
        {
            // 캐릭터 존재 여부 확인
            var character = await _unitOfWork.Characters.GetByIdAsync(characterId);
            if (character == null)
                throw new InvalidOperationException("캐릭터를 찾을 수 없습니다");

            // 진행 상황 조회 (없으면 새로 생성)
            var progress = await _unitOfWork.CharacterDungeonProgresses.GetOrCreateByCharacterIdAsync(characterId);

            return new CharacterMainBattleProgressDto
            {
                Id = progress.Id,
                CharacterId = progress.CharacterId,
                HighestStageClearedNormal = progress.HighestStageClearedNormal,
                HighestStageClearedHard = progress.HighestStageClearedHard,
                HighestStageClearedHell = progress.HighestStageClearedNightmare, // Nightmare → Hell 매핑
                UpdatedAt = progress.UpdatedAt
            };
        }

        public async Task<DungeonClearResultDto> ClearStageAsync(Guid characterId, DungeonClearRequestDto request)
        {
            // 캐릭터별 잠금 획득 (중복 요청 방지)
            var lockObj = _characterLocks.GetOrAdd(characterId, _ => new SemaphoreSlim(1, 1));
            await lockObj.WaitAsync();

            try
            {
                // 1. 입력 검증
                if (request == null)
                {
                    return new DungeonClearResultDto
                    {
                        IsSuccess = false,
                        ErrorMessage = "잘못된 요청입니다"
                    };
                }

                // 2. 캐릭터 로드
                var character = await _unitOfWork.Characters.GetByIdAsync(characterId);
                if (character == null)
                {
                    return new DungeonClearResultDto
                    {
                        IsSuccess = false,
                        ErrorMessage = "캐릭터를 찾을 수 없습니다"
                    };
                }

                // 3. 스테이지 로드
                var stage = await _unitOfWork.DungeonStages.GetByIdWithMonsterAsync(request.StageId);
                if (stage == null)
                {
                    return new DungeonClearResultDto
                    {
                        IsSuccess = false,
                        ErrorMessage = "존재하지 않는 스테이지입니다"
                    };
                }

                // 4. 진행도 로드 (없으면 자동 생성)
                var progress = await _unitOfWork.CharacterDungeonProgresses.GetOrCreateByCharacterIdAsync(characterId);

                // 5. 레벨 검증
                if (character.Level < stage.RequiredLevel)
                {
                    return new DungeonClearResultDto
                    {
                        IsSuccess = false,
                        ErrorMessage = $"레벨이 부족합니다 (필요 레벨: {stage.RequiredLevel})"
                    };
                }

                // 6. 이전 스테이지 클리어 여부 검증
                int highestCleared = progress.GetHighestStageCleared(request.Difficulty);

                if (stage.Id > 1 && highestCleared < (stage.Id - 1))
                {
                    return new DungeonClearResultDto
                    {
                        IsSuccess = false,
                        ErrorMessage = "이전 스테이지를 먼저 클리어해야 합니다"
                    };
                }

                // 6-1. 난이도 검증 (서버 권위: 허용된 난이도인지 확인)
                var allowedDifficulties = GetAllowedDifficulties(highestCleared, stage.Id);
                if (!allowedDifficulties.Contains(request.Difficulty))
                {
                    return new DungeonClearResultDto
                    {
                        IsSuccess = false,
                        ErrorMessage = "허용되지 않은 난이도입니다"
                    };
                }

                // ✅ 7. 전투 시뮬레이션 실행 (CombatService 사용)
                var combatResult = await _combatService.SimulateCombatAsync(
                    characterId,
                    stage.MonsterId,
                    difficulty: request.Difficulty);

                // 7-1. 전투 패배 시 조기 반환 (보상 없음)
                if (!combatResult.IsVictory)
                {
                    _logger.LogInformation(
                        "스테이지 전투 패배 - 캐릭터: {CharacterId}, 스테이지: {StageId}, 난이도: {Difficulty}",
                        characterId, request.StageId, request.Difficulty);

                    return new DungeonClearResultDto
                    {
                        IsSuccess = false,
                        ErrorMessage = "전투에서 패배했습니다",
                        BattleStatistics = combatResult.Statistics
                    };
                }

                // 8. 보상 계산 (난이도 배율 적용)
                var multiplier = DifficultyMultiplier.Create(request.Difficulty);
                var finalGoldReward = (long)(stage.BaseGold * multiplier.RewardMultiplier);
                var finalExpReward = (long)(stage.BaseExperience * multiplier.RewardMultiplier);

                // 9. 트랜잭션 시작 - 보상 지급 + 진행도 업데이트 + 로그 저장
                // 9-1. 진행도 업데이트
                int newHighScore = Math.Max(highestCleared, request.StageId);
                progress.SetHighestStageCleared(request.Difficulty, newHighScore);

                // 9-2. 보상 지급 (Gold)
                character.Gold += finalGoldReward;

                // 9-3. 경험치 지급 및 레벨업 처리 (SaveChanges 없이)
                var (isLevelUp, levelUps) = _characterService.ProcessExperienceGain(character, (int)finalExpReward);

                // 9-4. 장비 드랍 처리 (LootTable이 있는 경우)
                List<EquipmentDto>? droppedEquipments = null;
                if (stage.LootTableId.HasValue)
                {
                    droppedEquipments = await ProcessEquipmentDropAsync(stage, character, request.Difficulty);
                }

                // ✅ 9-5. BattleLog 생성 및 저장 (BattleLogService 사용)
                await _battleLogService.CreateAndSaveLogAsync(
                    characterId,
                    stage.MonsterId,
                    combatResult,
                    experienceGained: (int)finalExpReward,
                    goldGained: (int)finalGoldReward,
                    dungeonStageId: request.StageId);

                // 10. 트랜잭션 커밋 (모든 변경사항 한 번에 저장 - 원자성 보장)
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "스테이지 클리어 - 캐릭터: {CharacterId}, 스테이지: {StageId}, 난이도: {Difficulty}, 레벨업: {IsLevelUp} ({LevelUps}회), 최종 레벨: {NewLevel}",
                    characterId, request.StageId, request.Difficulty, isLevelUp, levelUps, character.Level);

                // 11. 성공 응답 반환
                return new DungeonClearResultDto
                {
                    IsSuccess = true,
                    Reward = new RewardDto
                    {
                        Gold = finalGoldReward,
                        Experience = finalExpReward
                    },
                    NewHighestStage = newHighScore,
                    CurrentLevel = character.Level,
                    IsLevelUp = isLevelUp,
                    DroppedEquipments = droppedEquipments,
                    BattleStatistics = combatResult.Statistics
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "스테이지 클리어 처리 중 오류 발생 - 캐릭터: {CharacterId}, 스테이지: {StageId}", characterId, request.StageId);
                return new DungeonClearResultDto
                {
                    IsSuccess = false,
                    ErrorMessage = "스테이지 클리어 처리 중 오류가 발생했습니다"
                };
            }
            finally
            {
                // 잠금 해제
                lockObj.Release();
            }
        }

        // [Private Helper Methods - 다음 메시지에 계속]

        /// <summary>
        /// 스테이지 도전 가능 여부 판단 (서버 검증 로직)
        /// </summary>
        private bool IsStageAvailable(Character character, DungeonStage stage, CharacterDungeonProgress progress, DungeonDifficultyEnum difficulty)
        {
            // 레벨 체크
            if (character.Level < stage.RequiredLevel)
                return false;

            // 이전 스테이지 클리어 여부 체크
            int highestCleared = progress.GetHighestStageCleared(difficulty);

            // 첫 번째 스테이지(Id=1)는 항상 도전 가능
            return stage.Id == 1 || highestCleared >= (stage.Id - 1);
        }

        /// <summary>
        /// 스테이지 클리어 시 장비 드랍 처리
        /// </summary>
        private async Task<List<EquipmentDto>> ProcessEquipmentDropAsync(
            DungeonStage stage,
            Character character,
            DungeonDifficultyEnum difficulty)
        {
            var droppedEquipments = new List<EquipmentDto>();

            // 1. LootTable 조회
            var lootTable = await _unitOfWork.LootTables.GetWithItemsAsync(stage.LootTableId!.Value);
            if (lootTable == null || lootTable.Items == null || !lootTable.Items.Any())
            {
                _logger.LogWarning(
                    "LootTable {LootTableId}를 찾을 수 없거나 아이템이 없습니다 - 스테이지: {StageId}",
                    stage.LootTableId, stage.Id);
                return droppedEquipments;
            }

            // 2. LootCalculator로 확률 계산
            var random = new Random();
            var rolledItems = _lootCalculator.RollLootItems(lootTable, random, allowDuplicates: false);

            _logger.LogInformation(
                "LootTable {LootTableId} 추첨 완료 - {Count}개 아이템 선택됨",
                lootTable.Id, rolledItems.Count);

            // 3. LootItem을 Equipment/Skill로 변환
            foreach (var (item, quantity) in rolledItems)
            {
                if (item.Type == RewardType.Equipment)
                {
                    if (!item.EquipmentSlot.HasValue || !item.EquipmentRarity.HasValue)
                    {
                        _logger.LogWarning(
                            "LootItem {ItemId}에 EquipmentSlot 또는 EquipmentRarity가 없습니다",
                            item.Id);
                        continue;
                    }

                    for (int i = 0; i < quantity; i++)
                    {
                        var equipment = CreateEquipmentFromLootItem(item, stage, character.PlayerId);
                        await _unitOfWork.Equipments.AddAsync(equipment);

                        droppedEquipments.Add(new EquipmentDto
                        {
                            Id = equipment.Id,
                            Name = equipment.Name,
                            Slot = equipment.Slot,
                            Rarity = equipment.Rarity,
                            OwnerId = equipment.OwnerId,
                            CharacterId = equipment.CharacterId,
                            EnhancementLevel = equipment.EnhancementLevel,
                            BaseAttack = equipment.BaseAttack,
                            BaseDefense = equipment.BaseDefense,
                            BaseHp = equipment.BaseHp,
                            TotalAttack = equipment.GetTotalAttack(),
                            TotalDefense = equipment.GetTotalDefense(),
                            TotalHp = equipment.GetTotalHp(),
                            CreatedAt = equipment.CreatedAt,
                            UpdatedAt = equipment.UpdatedAt
                        });

                        _logger.LogInformation(
                            "Equipment 드랍: {Slot} {Rarity} (BaseAttack: {Attack})",
                            equipment.Slot, equipment.Rarity, equipment.BaseAttack);
                    }
                }
                else if (item.Type == RewardType.Skill)
                {
                    if (!item.ItemTemplateId.HasValue)
                    {
                        _logger.LogWarning(
                            "LootItem {ItemId}에 SkillTemplate ID가 없습니다",
                            item.Id);
                        continue;
                    }

                    await GrantSkillToCharacterAsync(character, item.ItemTemplateId.Value, quantity);

                    _logger.LogInformation(
                        "Skill 드랍: SkillTemplateId={SkillId}, Quantity={Quantity}",
                        item.ItemTemplateId.Value, quantity);
                }
                else if (item.Type == RewardType.Gold)
                {
                    character.Gold += quantity;
                    _logger.LogDebug("추가 Gold 드랍: {Quantity}", quantity);
                }
            }

            return droppedEquipments;
        }

        /// <summary>
        /// LootItem과 스테이지 컨텍스트로 Equipment 엔티티 생성
        /// </summary>
        private Equipment CreateEquipmentFromLootItem(
            LootItem lootItem,
            DungeonStage stage,
            Guid ownerId)
        {
            // 스테이지 레벨 기반 기본 스탯 계산
            int baseAttack = stage.RequiredLevel * 10;
            int baseDefense = stage.RequiredLevel * 5;
            int baseHp = stage.RequiredLevel * 20;

            // 희귀도에 따른 스탯 배율
            float rarityMultiplier = lootItem.EquipmentRarity!.Value switch
            {
                EquipmentRarity.Common => 1.0f,
                EquipmentRarity.Rare => 1.3f,
                EquipmentRarity.Epic => 1.6f,
                EquipmentRarity.Legendary => 2.0f,
                _ => 1.0f
            };

            baseAttack = (int)(baseAttack * rarityMultiplier);
            baseDefense = (int)(baseDefense * rarityMultiplier);
            baseHp = (int)(baseHp * rarityMultiplier);

            // 장비 이름 생성
            string rarityPrefix = lootItem.EquipmentRarity.Value switch
            {
                EquipmentRarity.Common => "일반",
                EquipmentRarity.Rare => "희귀",
                EquipmentRarity.Epic => "영웅",
                EquipmentRarity.Legendary => "전설",
                _ => "미지"
            };

            string slotName = lootItem.EquipmentSlot!.Value switch
            {
                EquipmentSlot.Weapon => "무기",
                EquipmentSlot.Armor => "갑옷",
                EquipmentSlot.Helmet => "투구",
                EquipmentSlot.Boots => "신발",
                EquipmentSlot.Gloves => "장갑",
                _ => "장비"
            };

            return new Equipment
            {
                Id = Guid.NewGuid(),
                Name = $"{rarityPrefix} {slotName} (Lv.{stage.RequiredLevel})",
                Slot = lootItem.EquipmentSlot.Value,
                Rarity = lootItem.EquipmentRarity.Value,
                OwnerId = ownerId,
                CharacterId = null,
                EnhancementLevel = 0,
                BaseAttack = baseAttack,
                BaseDefense = baseDefense,
                BaseHp = baseHp,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// 캐릭터에게 스킬 지급
        /// </summary>
        private async Task GrantSkillToCharacterAsync(
            Character character,
            int skillTemplateId,
            int quantity)
        {
            var skillTemplate = await _unitOfWork.SkillTemplates.GetByIdAsync(skillTemplateId);
            if (skillTemplate == null)
            {
                _logger.LogWarning(
                    "존재하지 않는 SkillTemplate: {SkillTemplateId}",
                    skillTemplateId);
                return;
            }

            var hasSkill = await _unitOfWork.CharacterSkills
                .HasSkillAsync(character.Id, skillTemplateId);

            if (!hasSkill)
            {
                var newSkill = new CharacterSkill
                {
                    Id = Guid.NewGuid(),
                    CharacterId = character.Id,
                    SkillTemplateId = skillTemplateId,
                    IsEquipped = false,
                    AcquiredAt = DateTime.UtcNow
                };

                await _unitOfWork.CharacterSkills.AddAsync(newSkill);

                _logger.LogInformation(
                    "새 스킬 지급: Character={CharacterId}, Skill={SkillName}",
                    character.Id, skillTemplate.Name);
            }
            else
            {
                _logger.LogDebug(
                    "중복 스킬 획득 무시: Character={CharacterId}, Skill={SkillName}",
                    character.Id, skillTemplate.Name);
            }
        }

        /// <summary>
        /// 현재 진행도에 따라 허용된 난이도 목록 반환
        /// </summary>
        private List<DungeonDifficultyEnum> GetAllowedDifficulties(int highestCleared, int currentStageId)
        {
            var allowed = new List<DungeonDifficultyEnum> { DungeonDifficultyEnum.Normal };

            if (highestCleared >= currentStageId)
            {
                allowed.Add(DungeonDifficultyEnum.Hard);
                allowed.Add(DungeonDifficultyEnum.Nightmare);
            }

            return allowed;
        }
    }
}
