using IdleRPG.Application.Character.Services;
using IdleRPG.Application.DTOs;
using IdleRPG.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace IdleRPG.Infrastructure.Services;

/// <summary>
/// 오프라인 보상 계산 및 지급 서비스
/// </summary>
public class OfflineRewardService : IOfflineRewardService
{
    private readonly ICharacterService _characterService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OfflineRewardService> _logger;

    public OfflineRewardService(
        ICharacterService characterService,
        IUnitOfWork unitOfWork,
        ILogger<OfflineRewardService> logger)
    {
        _characterService = characterService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// 오프라인 보상 계산 (조회만, 실제 지급 없음)
    /// </summary>
    public async Task<OfflineRewardDto> CalculateOfflineRewardsAsync(Guid characterId)
    {
        // TODO(human): 오프라인 보상 계산 로직 구현
        // 1. 캐릭터 조회 (Character 엔티티)
        var character = await _unitOfWork.Characters.GetByIdAsync(characterId);
        if (character == null)
            throw new KeyNotFoundException($"Character with id {characterId} doesn't exist");

        // 2. OfflineRewardType 조회 (기본 보상 타입 사용)
        var rewardData = await _unitOfWork.OfflineRewardTypes.GetDefaultAsync();
        if (rewardData == null)
            throw new NullReferenceException($"Reward type Null");
        
        // 3. 오프라인 경과 시간 계산 (현재 시각 - LastLoginTime)
        var currTime = DateTime.UtcNow - character.LastLoginTime;
        int totalMinutes = Convert.ToInt32(currTime.TotalMinutes);
        // 4. MaxMinutes 제한 적용
        int offlineMinutes = (int)Math.Min(totalMinutes, rewardData.MaxMinutes);
        
        // 5. 보상 계산:
        //    - calculatedExperience = 캐릭터레벨 × ExperiencePerMinute × 경과시간(분)
        //    - calculatedGold = 캐릭터레벨 × GoldPerMinute × 경과시간(분)
        var calculatedExperience = character.Level * rewardData.ExperiencePerMinute * offlineMinutes;
        var calculatedGold = character.Level * rewardData.GoldPerMinute * offlineMinutes;
        
        // 6. OfflineRewardDto 반환
        return new OfflineRewardDto()
        {
            CalculatedExperience = calculatedExperience,
            CalculatedGold = calculatedGold,
            OfflineMinutes = offlineMinutes,
            RewardTypeId = rewardData.Id
        };
    }

    /// <summary>
    /// 오프라인 보상 실제 지급
    /// </summary>
    public async Task<ClaimOfflineRewardDto> ClaimOfflineRewardsAsync(Guid characterId)
    {
        // 1. 보상 계산
        var rewardInfo = await CalculateOfflineRewardsAsync(characterId);

        // 2. Entity 직접 조회 (DTO 아님!)
        var character = await _unitOfWork.Characters.GetByIdAsync(characterId);
        if (character == null)
            throw new KeyNotFoundException($"Character with id {characterId} doesn't exist");

        // 3. 경험치 지급 및 레벨업 처리 (SaveChanges 없음)
        _characterService.ProcessExperienceGain(character, rewardInfo.CalculatedExperience);

        // 4. 골드 지급
        character.Gold += rewardInfo.CalculatedGold;

        // 5. LastLoginTime 업데이트
        character.LastLoginTime = DateTime.UtcNow;

        // 6. DB 저장 (한 번만!)
        await _unitOfWork.SaveChangesAsync();

        // 7. DTO 변환하여 반환
        var updatedCharacterDto = await _characterService.GetCharacterByIdAsync(characterId);

        return new ClaimOfflineRewardDto
        {
            Claimed = true,
            Experience = rewardInfo.CalculatedExperience,
            Gold = rewardInfo.CalculatedGold,
            UpdatedCharacter = updatedCharacterDto!
        };
    }
}
