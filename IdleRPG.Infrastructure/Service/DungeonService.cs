using IdleRPG.Application.Character.Services;
using IdleRPG.Application.DTOs.Dungeon;
using IdleRPG.Application.DTOs.Rewards;
using IdleRPG.Application.Interfaces;
using IdleRPG.Domain.Entities;
using IdleRPG.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using DungeonDifficultyEnum = IdleRPG.Domain.Enums.DungeonDifficulty;

namespace IdleRPG.Infrastructure.Service
{
    /// <summary>
    /// 던전 시스템 서비스
    ///
    /// [서버 중심 아키텍처]
    /// - 모든 비즈니스 검증을 서버에서 수행
    /// - 트랜잭션으로 진행도 + 보상 지급 원자성 보장
    /// - 동시성 제어로 치트 방지
    ///
    /// [Week 3 설계 변경사항]
    /// - DungeonStage는 모든 난이도 공통 (Difficulty 속성 없음)
    /// - 난이도는 CharacterDungeonProgress에서만 관리
    /// - DifficultyMultiplier로 난이도별 보상 계산
    /// </summary>
    public class DungeonService : IDungeonService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICharacterService _characterService;
        private readonly ILogger<DungeonService> _logger;

        // 캐릭터별 동시성 제어 (중복 보상 지급 방지)
        private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> _characterLocks = new();

        public DungeonService(
            IUnitOfWork unitOfWork,
            ICharacterService characterService,
            ILogger<DungeonService> logger)
        {
            _unitOfWork = unitOfWork;
            _characterService = characterService;
            _logger = logger;
        }

        public async Task<List<DungeonStageDto>> GetAvailableStagesAsync(Guid characterId, DungeonDifficultyEnum? difficulty = null)
        {
            // 1. 캐릭터 정보 로드
            var character = await _unitOfWork.Characters.GetByIdAsync(characterId);
            if (character == null)
                throw new InvalidOperationException("캐릭터를 찾을 수 없습니다");

            // 2. 던전 진행 상황 로드 (없으면 새로 생성)
            var progress = await _unitOfWork.CharacterDungeonProgresses.GetOrCreateByCharacterIdAsync(characterId);

            // 3. 모든 던전 스테이지 조회
            var allStages = await _unitOfWork.DungeonStages.GetAllAsync();

            // 4. 난이도가 지정되지 않으면 모든 난이도 표시 (Normal, Hard, Nightmare 각각)
            //    난이도가 지정되면 해당 난이도만 표시
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
                        Description = string.Empty, // DungeonStage에 Description 없음
                        Difficulty = diff,
                        RequiredLevel = stage.RequiredLevel,
                        BossMonsterId = stage.MonsterId,
                        BossMonsterName = stage.Monster?.Name, // 조인 시 자동 설정
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

        public async Task<CharacterDungeonProgressDto> GetProgressAsync(Guid characterId)
        {
            // 캐릭터 존재 여부 확인
            var character = await _unitOfWork.Characters.GetByIdAsync(characterId);
            if (character == null)
                throw new InvalidOperationException("캐릭터를 찾을 수 없습니다");

            // 진행 상황 조회 (없으면 새로 생성)
            var progress = await _unitOfWork.CharacterDungeonProgresses.GetOrCreateByCharacterIdAsync(characterId);

            return new CharacterDungeonProgressDto
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

                // 3. 던전 스테이지 로드
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

                // 7. 진행도 업데이트
                int newHighScore = Math.Max(highestCleared, request.StageId);
                progress.SetHighestStageCleared(request.Difficulty, newHighScore);

                // 8. 보상 계산
                var multiplier = DifficultyMultiplier.Create(request.Difficulty);
                var finalGoldReward = (long)(stage.BaseGold * multiplier.RewardMultiplier);
                var finalExpReward = (long)(stage.BaseExperience * multiplier.RewardMultiplier);

                // 9. 보상 지급 (Gold)
                character.Gold += finalGoldReward;

                // 10. 경험치 지급 및 레벨업 처리 (SaveChanges 없이)
                var (isLevelUp, levelUps) = _characterService.ProcessExperienceGain(character, (int)finalExpReward);

                // 11. 트랜잭션 커밋 (모든 변경사항 한 번에 저장)
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "던전 클리어 - 캐릭터: {CharacterId}, 스테이지: {StageId}, 난이도: {Difficulty}, 레벨업: {IsLevelUp} ({LevelUps}회), 최종 레벨: {NewLevel}",
                    characterId, request.StageId, request.Difficulty, isLevelUp, levelUps, character.Level);

                // 12. 성공 응답 반환
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
                    IsLevelUp = isLevelUp
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "던전 클리어 처리 중 오류 발생 - 캐릭터: {CharacterId}, 스테이지: {StageId}", characterId, request.StageId);
                return new DungeonClearResultDto
                {
                    IsSuccess = false,
                    ErrorMessage = "던전 클리어 처리 중 오류가 발생했습니다"
                };
            }
            finally
            {
                // 잠금 해제
                lockObj.Release();
            }
        }

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
            // 그 외 스테이지는 이전 스테이지(Id-1)를 클리어해야 함
            return stage.Id == 1 || highestCleared >= (stage.Id - 1);
        }
    }
}
