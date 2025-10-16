using FluentAssertions;
using IdleRPG.Application.Character.Services;
using IdleRPG.Application.DTOs;
using IdleRPG.Application.DTOs.Characters;
using IdleRPG.Application.Interfaces;
using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Repositories;
using IdleRPG.Domain.ValueObjects;
using IdleRPG.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace IdleRPG.Tests;

/// <summary>
/// OfflineRewardService의 핵심 비즈니스 로직을 검증하는 단위 테스트
/// 오프라인 보상 계산 및 지급 로직 검증
/// </summary>
public class OfflineRewardServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ICharacterRepository> _mockCharacterRepository;
    private readonly Mock<IOfflineRewardTypeRepository> _mockRewardTypeRepository;
    private readonly Mock<ICharacterService> _mockCharacterService;
    private readonly Mock<ILogger<OfflineRewardService>> _mockLogger;
    private readonly IOfflineRewardService _service;

    public OfflineRewardServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockCharacterRepository = new Mock<ICharacterRepository>();
        _mockRewardTypeRepository = new Mock<IOfflineRewardTypeRepository>();
        _mockCharacterService = new Mock<ICharacterService>();
        _mockLogger = new Mock<ILogger<OfflineRewardService>>();

        // UnitOfWork가 Repository들을 반환하도록 설정
        _mockUnitOfWork.Setup(u => u.Characters).Returns(_mockCharacterRepository.Object);
        _mockUnitOfWork.Setup(u => u.OfflineRewardTypes).Returns(_mockRewardTypeRepository.Object);

        _service = new OfflineRewardService(
            _mockCharacterService.Object,
            _mockUnitOfWork.Object,
            _mockLogger.Object
        );
    }

    #region CalculateOfflineRewardsAsync Tests (보상 계산)

    [Fact]
    public async Task CalculateOfflineRewards_ShouldCalculateCorrectly_WithinMaxMinutes()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var lastLoginTime = DateTime.UtcNow.AddMinutes(-120); // 2시간 전 로그인
        var character = CreateTestCharacter(characterId, level: 5, lastLoginTime: lastLoginTime);
        var rewardType = CreateTestRewardType(
            expPerMinute: 2,
            goldPerMinute: 1,
            maxMinutes: 480
        );

        _mockCharacterRepository.Setup(r => r.GetByIdAsync(characterId))
            .ReturnsAsync(character);
        _mockRewardTypeRepository.Setup(r => r.GetDefaultAsync())
            .ReturnsAsync(rewardType);

        // Act
        var result = await _service.CalculateOfflineRewardsAsync(characterId);

        // Assert
        result.Should().NotBeNull();
        result.OfflineMinutes.Should().Be(120, "2시간 = 120분");
        result.CalculatedExperience.Should().Be(5 * 2 * 120, "레벨5 × 2경험치/분 × 120분 = 1200");
        result.CalculatedGold.Should().Be(5 * 1 * 120, "레벨5 × 1골드/분 × 120분 = 600");
        result.RewardTypeId.Should().Be(rewardType.Id);
    }

    [Fact]
    public async Task CalculateOfflineRewards_ShouldApplyMaxMinutesLimit()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var lastLoginTime = DateTime.UtcNow.AddMinutes(-600); // 10시간 전 (최대치 초과)
        var character = CreateTestCharacter(characterId, level: 3, lastLoginTime: lastLoginTime);
        var rewardType = CreateTestRewardType(
            expPerMinute: 2,
            goldPerMinute: 1,
            maxMinutes: 480 // 8시간 제한
        );

        _mockCharacterRepository.Setup(r => r.GetByIdAsync(characterId))
            .ReturnsAsync(character);
        _mockRewardTypeRepository.Setup(r => r.GetDefaultAsync())
            .ReturnsAsync(rewardType);

        // Act
        var result = await _service.CalculateOfflineRewardsAsync(characterId);

        // Assert
        result.OfflineMinutes.Should().Be(480, "최대 480분(8시간)으로 제한");
        result.CalculatedExperience.Should().Be(3 * 2 * 480, "레벨3 × 2경험치/분 × 480분 = 2880");
        result.CalculatedGold.Should().Be(3 * 1 * 480, "레벨3 × 1골드/분 × 480분 = 1440");
    }

    [Theory]
    [InlineData(1, 60, 120, 60)]   // 레벨1, 1시간 오프라인 = 120 경험치, 60 골드
    [InlineData(5, 30, 300, 150)]  // 레벨5, 30분 오프라인 = 300 경험치, 150 골드
    [InlineData(10, 120, 2400, 1200)] // 레벨10, 2시간 오프라인 = 2400 경험치, 1200 골드
    [InlineData(20, 480, 19200, 9600)] // 레벨20, 8시간(최대) = 19200 경험치, 9600 골드
    public async Task CalculateOfflineRewards_ShouldCalculateCorrectly_ForVariousLevelsAndTimes(
        int level, int offlineMinutes, int expectedExp, long expectedGold)
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var lastLoginTime = DateTime.UtcNow.AddMinutes(-offlineMinutes);
        var character = CreateTestCharacter(characterId, level: level, lastLoginTime: lastLoginTime);
        var rewardType = CreateTestRewardType(expPerMinute: 2, goldPerMinute: 1, maxMinutes: 480);

        _mockCharacterRepository.Setup(r => r.GetByIdAsync(characterId))
            .ReturnsAsync(character);
        _mockRewardTypeRepository.Setup(r => r.GetDefaultAsync())
            .ReturnsAsync(rewardType);

        // Act
        var result = await _service.CalculateOfflineRewardsAsync(characterId);

        // Assert
        result.CalculatedExperience.Should().Be(expectedExp);
        result.CalculatedGold.Should().Be(expectedGold);
    }

    [Fact]
    public async Task CalculateOfflineRewards_ShouldThrow_WhenCharacterNotFound()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        _mockCharacterRepository.Setup(r => r.GetByIdAsync(characterId))
            .ReturnsAsync((Character?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.CalculateOfflineRewardsAsync(characterId)
        );
    }

    #endregion

    #region ClaimOfflineRewardsAsync Tests (보상 수령)

    [Fact]
    public async Task ClaimOfflineRewards_ShouldSucceed_WithoutLevelUp()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var lastLoginTime = DateTime.UtcNow.AddMinutes(-60); // 1시간 전
        var character = CreateTestCharacter(characterId, level: 5, experience: 50, gold: 100, lastLoginTime: lastLoginTime);
        character.PlayerId = playerId;

        var rewardType = CreateTestRewardType(expPerMinute: 2, goldPerMinute: 1, maxMinutes: 480);

        _mockCharacterRepository.Setup(r => r.GetByIdAsync(characterId))
            .ReturnsAsync(character);
        _mockRewardTypeRepository.Setup(r => r.GetDefaultAsync())
            .ReturnsAsync(rewardType);

        // ProcessExperienceGain 모의: 경험치만 추가, 레벨업 없음
        _mockCharacterService.Setup(s => s.ProcessExperienceGain(It.IsAny<Character>(), It.IsAny<int>()))
            .Callback<Character, int>((c, exp) =>
            {
                c.Experience += exp;
                c.UpdatedAt = DateTime.UtcNow;
            });

        // GetCharacterByIdAsync가 업데이트된 캐릭터 DTO 반환
        // 실제 character 엔티티의 값을 반영하도록 설정
        _mockCharacterService.Setup(s => s.GetCharacterByIdAsync(characterId))
            .ReturnsAsync(() => new CharacterDto
            {
                Id = character.Id,
                PlayerId = character.PlayerId,
                Level = character.Level,
                Experience = character.Experience,
                Gold = character.Gold,
                Stats = new CharacterStatsDto
                {
                    Attack = character.Stats.Attack,
                    Defense = character.Stats.Defense,
                    MaxHealth = character.Stats.MaxHealth,
                    CritRate = character.Stats.CritRate,
                    CritDamage = character.Stats.CritDamage,
                    Evasion = character.Stats.Evasion,
                    AttackSpeed = character.Stats.AttackSpeed
                },
                CreatedAt = character.CreatedAt,
                UpdatedAt = character.UpdatedAt,
                LastLoginTime = character.LastLoginTime
            });

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(default))
            .ReturnsAsync(1);

        // Act
        var result = await _service.ClaimOfflineRewardsAsync(characterId);

        // Assert
        result.Should().NotBeNull();
        result.Claimed.Should().BeTrue();
        result.Experience.Should().Be(600, "레벨5 × 2경험치/분 × 60분");
        result.Gold.Should().Be(300, "레벨5 × 1골드/분 × 60분 = 300");
        result.UpdatedCharacter.Should().NotBeNull();
        result.UpdatedCharacter.Level.Should().Be(5, "레벨업 없음");
        result.UpdatedCharacter.Experience.Should().Be(650, "50 + 600");
        result.UpdatedCharacter.Gold.Should().Be(400, "100 + 300");

        // ProcessExperienceGain 호출 검증
        _mockCharacterService.Verify(
            s => s.ProcessExperienceGain(It.IsAny<Character>(), 600),
            Times.Once
        );

        // SaveChanges 한 번만 호출되었는지 검증
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task ClaimOfflineRewards_ShouldSucceed_WithLevelUp()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var lastLoginTime = DateTime.UtcNow.AddMinutes(-30); // 30분 전
        var character = CreateTestCharacter(characterId, level: 1, experience: 50, gold: 50, lastLoginTime: lastLoginTime);
        character.PlayerId = playerId;

        var rewardType = CreateTestRewardType(expPerMinute: 2, goldPerMinute: 1, maxMinutes: 480);

        _mockCharacterRepository.Setup(r => r.GetByIdAsync(characterId))
            .ReturnsAsync(character);
        _mockRewardTypeRepository.Setup(r => r.GetDefaultAsync())
            .ReturnsAsync(rewardType);

        // ProcessExperienceGain 모의: 경험치 추가 + 레벨업 (Lv1 → Lv2)
        _mockCharacterService.Setup(s => s.ProcessExperienceGain(It.IsAny<Character>(), It.IsAny<int>()))
            .Callback<Character, int>((c, exp) =>
            {
                c.Experience += exp; // 50 + 60 = 110
                // Lv1 requires 100 exp to level up
                if (c.Experience >= 100)
                {
                    c.Experience -= 100; // 110 - 100 = 10
                    c.Level = 2;
                    c.Stats = new CharacterStats(
                        attack: 20,   // 10 + 10
                        defense: 10,  // 5 + 5
                        maxHealth: 150, // 100 + 50
                        critRate: 0.05f,
                        critDamage: 1.5f,
                        evasion: 0.05f,
                        attackSpeed: 1.0f
                    );
                }
                c.UpdatedAt = DateTime.UtcNow;
            });

        // GetCharacterByIdAsync가 레벨업된 캐릭터 DTO 반환
        // 실제 character 엔티티의 값을 반영하도록 설정
        _mockCharacterService.Setup(s => s.GetCharacterByIdAsync(characterId))
            .ReturnsAsync(() => new CharacterDto
            {
                Id = character.Id,
                PlayerId = character.PlayerId,
                Level = character.Level,
                Experience = character.Experience,
                Gold = character.Gold,
                Stats = new CharacterStatsDto
                {
                    Attack = character.Stats.Attack,
                    Defense = character.Stats.Defense,
                    MaxHealth = character.Stats.MaxHealth,
                    CritRate = character.Stats.CritRate,
                    CritDamage = character.Stats.CritDamage,
                    Evasion = character.Stats.Evasion,
                    AttackSpeed = character.Stats.AttackSpeed
                },
                CreatedAt = character.CreatedAt,
                UpdatedAt = character.UpdatedAt,
                LastLoginTime = character.LastLoginTime
            });

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(default))
            .ReturnsAsync(1);

        // Act
        var result = await _service.ClaimOfflineRewardsAsync(characterId);

        // Assert
        result.Should().NotBeNull();
        result.Claimed.Should().BeTrue();
        result.Experience.Should().Be(60, "레벨1 × 2경험치/분 × 30분");
        result.Gold.Should().Be(30, "레벨1 × 1골드/분 × 30분");
        result.UpdatedCharacter.Level.Should().Be(2, "Lv1 → Lv2 레벨업");
        result.UpdatedCharacter.Experience.Should().Be(10, "레벨업 후 남은 경험치");
        result.UpdatedCharacter.Gold.Should().Be(80, "초기 50 + 보상 30");
        result.UpdatedCharacter.Stats.Attack.Should().Be(20, "레벨업 후 스탯 증가");
        result.UpdatedCharacter.Stats.Defense.Should().Be(10);
        result.UpdatedCharacter.Stats.MaxHealth.Should().Be(150);

        _mockCharacterService.Verify(
            s => s.ProcessExperienceGain(It.IsAny<Character>(), 60),
            Times.Once
        );
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task ClaimOfflineRewards_ShouldThrow_WhenCharacterNotFound()
    {
        // Arrange
        var characterId = Guid.NewGuid();

        // CalculateOfflineRewardsAsync는 성공하지만 ClaimOfflineRewardsAsync에서 실패
        var character = CreateTestCharacter(characterId, level: 5, lastLoginTime: DateTime.UtcNow.AddMinutes(-60));
        _mockCharacterRepository.SetupSequence(r => r.GetByIdAsync(characterId))
            .ReturnsAsync(character)  // CalculateOfflineRewardsAsync에서는 성공
            .ReturnsAsync((Character?)null); // ClaimOfflineRewardsAsync에서는 실패

        var rewardType = CreateTestRewardType(expPerMinute: 2, goldPerMinute: 1, maxMinutes: 480);
        _mockRewardTypeRepository.Setup(r => r.GetDefaultAsync())
            .ReturnsAsync(rewardType);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.ClaimOfflineRewardsAsync(characterId)
        );
    }

    [Fact]
    public async Task ClaimOfflineRewards_ShouldUpdateLastLoginTime()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var oldLastLoginTime = DateTime.UtcNow.AddHours(-2);
        var character = CreateTestCharacter(characterId, level: 3, lastLoginTime: oldLastLoginTime);
        character.PlayerId = playerId;

        var rewardType = CreateTestRewardType(expPerMinute: 2, goldPerMinute: 1, maxMinutes: 480);

        _mockCharacterRepository.Setup(r => r.GetByIdAsync(characterId))
            .ReturnsAsync(character);
        _mockRewardTypeRepository.Setup(r => r.GetDefaultAsync())
            .ReturnsAsync(rewardType);

        _mockCharacterService.Setup(s => s.ProcessExperienceGain(It.IsAny<Character>(), It.IsAny<int>()))
            .Callback<Character, int>((c, exp) =>
            {
                c.Experience += exp;
                c.UpdatedAt = DateTime.UtcNow;
            });

        _mockCharacterService.Setup(s => s.GetCharacterByIdAsync(characterId))
            .ReturnsAsync(() => new CharacterDto
            {
                Id = character.Id,
                PlayerId = character.PlayerId,
                Level = character.Level,
                Experience = character.Experience,
                Gold = character.Gold,
                Stats = new CharacterStatsDto
                {
                    Attack = character.Stats.Attack,
                    Defense = character.Stats.Defense,
                    MaxHealth = character.Stats.MaxHealth,
                    CritRate = character.Stats.CritRate,
                    CritDamage = character.Stats.CritDamage,
                    Evasion = character.Stats.Evasion,
                    AttackSpeed = character.Stats.AttackSpeed
                },
                CreatedAt = character.CreatedAt,
                UpdatedAt = character.UpdatedAt,
                LastLoginTime = character.LastLoginTime
            });

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(default))
            .ReturnsAsync(1);

        // Act
        await _service.ClaimOfflineRewardsAsync(characterId);

        // Assert
        character.LastLoginTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5),
            "LastLoginTime이 현재 시각으로 업데이트되어야 함");
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// 테스트용 캐릭터 객체 생성 헬퍼 메서드
    /// </summary>
    private Character CreateTestCharacter(Guid id, int level, int experience = 0, long gold = 0, DateTime? lastLoginTime = null)
    {
        // 레벨에 따른 자동 성장 스탯 계산
        long attack = 10 + (level - 1) * 10;
        long defense = 5 + (level - 1) * 5;
        long maxHealth = 100 + (level - 1) * 50;

        return new Character
        {
            Id = id,
            PlayerId = Guid.NewGuid(),
            Level = level,
            Experience = experience,
            Gold = gold,
            LastLoginTime = lastLoginTime ?? DateTime.UtcNow,
            Stats = new CharacterStats(
                attack: attack,
                defense: defense,
                maxHealth: maxHealth,
                critRate: 0.05f,
                critDamage: 1.5f,
                evasion: 0.05f,
                attackSpeed: 1.0f
            ),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// 테스트용 OfflineRewardType 객체 생성 헬퍼 메서드
    /// </summary>
    private OfflineRewardType CreateTestRewardType(int expPerMinute, int goldPerMinute, int maxMinutes)
    {
        return new OfflineRewardType
        {
            Id = Guid.NewGuid(),
            Name = "Test Reward Type",
            ExperiencePerMinute = expPerMinute,
            GoldPerMinute = goldPerMinute,
            MaxMinutes = maxMinutes
        };
    }

    #endregion
}
