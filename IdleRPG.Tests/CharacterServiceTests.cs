using FluentAssertions;
using IdleRPG.Application.Character.Services;
using IdleRPG.Application.DTOs.Characters;
using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Repositories;
using IdleRPG.Domain.ValueObjects;
using IdleRPG.Infrastructure.Service;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace IdleRPG.Tests;

/// <summary>
/// CharacterService의 핵심 비즈니스 로직을 검증하는 단위 테스트
/// </summary>
public class CharacterServiceTests
{
    private readonly Mock<ICharacterRepository> _mockRepository;
    private readonly Mock<ILogger<CharacterService>> _mockLogger;
    private readonly ICharacterService _service;

    public CharacterServiceTests()
    {
        _mockRepository = new Mock<ICharacterRepository>();
        _mockLogger = new Mock<ILogger<CharacterService>>();
        _service = new CharacterService(_mockRepository.Object, _mockLogger.Object);
    }

    #region AddExperienceAsync Tests (레벨업 로직)

    [Fact]
    public async Task AddExperience_ShouldNotLevelUp_WhenExperienceIsInsufficient()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var character = CreateTestCharacter(characterId, level: 1, experience: 0);

        _mockRepository.Setup(r => r.GetByIdAsync(characterId))
            .ReturnsAsync(character);
        _mockRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _service.AddExperienceAsync(characterId, 50);

        // Assert
        result.Level.Should().Be(1, "50 경험치는 레벨업에 부족함 (필요: 100)");
        result.Experience.Should().Be(50, "경험치가 누적되어야 함");
        result.Attack.Should().Be(10, "레벨업하지 않았으므로 스탯 변화 없음");
        result.Defense.Should().Be(5, "레벨업하지 않았으므로 스탯 변화 없음");
        result.MaxHealth.Should().Be(100, "레벨업하지 않았으므로 스탯 변화 없음");
    }

    [Fact]
    public async Task AddExperience_ShouldLevelUpOnce_WhenExperienceExactlyMeetsRequirement()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var character = CreateTestCharacter(characterId, level: 1, experience: 0);

        _mockRepository.Setup(r => r.GetByIdAsync(characterId))
            .ReturnsAsync(character);
        _mockRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _service.AddExperienceAsync(characterId, 100);

        // Assert
        result.Level.Should().Be(2, "100 경험치로 Lv1 → Lv2 레벨업");
        result.Experience.Should().Be(0, "정확히 레벨업했으므로 남은 경험치 0");
        result.Attack.Should().Be(20, "Lv2 자동 성장: 10 + 10 = 20");
        result.Defense.Should().Be(10, "Lv2 자동 성장: 5 + 5 = 10");
        result.MaxHealth.Should().Be(150, "Lv2 자동 성장: 100 + 50 = 150");
    }

    [Fact]
    public async Task AddExperience_ShouldLevelUpOnce_WithExcessExperience()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var character = CreateTestCharacter(characterId, level: 1, experience: 0);

        _mockRepository.Setup(r => r.GetByIdAsync(characterId))
            .ReturnsAsync(character);
        _mockRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _service.AddExperienceAsync(characterId, 150);

        // Assert
        result.Level.Should().Be(2, "150 경험치로 Lv1 → Lv2 레벨업");
        result.Experience.Should().Be(50, "초과 경험치 50이 다음 레벨로 이월");
        result.Attack.Should().Be(20, "Lv2 자동 성장: 10 + 10 = 20");
        result.Defense.Should().Be(10, "Lv2 자동 성장: 5 + 5 = 10");
        result.MaxHealth.Should().Be(150, "Lv2 자동 성장: 100 + 50 = 150");
    }

    [Theory]
    [InlineData(1, 0, 250, 2, 150, 20, 10, 150)]   // Lv1 → Lv2 (100 소모, 150 남음)
    [InlineData(2, 0, 500, 4, 0, 40, 20, 250)]      // Lv2 → Lv4 (200 + 300 소모, 0 남음)
    [InlineData(1, 50, 450, 3, 200, 30, 15, 200)]   // Lv1(50 exp) → Lv3 (100 + 200 소모, 200 남음)
    [InlineData(1, 0, 350, 3, 50, 30, 15, 200)]     // Lv1 → Lv3 (100 + 200 소모, 50 남음)
    [InlineData(1, 0, 600, 4, 0, 40, 20, 250)]      // Lv1 → Lv4 (100 + 200 + 300 소모, 0 남음)
    public async Task AddExperience_ShouldLevelUpMultipleTimes(
        int startLevel, int startExp, int addExp,
        int expectedLevel, int expectedExp, long expectedAttack, long expectedDefense, long expectedMaxHealth)
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var character = CreateTestCharacter(characterId, startLevel, startExp);

        _mockRepository.Setup(r => r.GetByIdAsync(characterId))
            .ReturnsAsync(character);
        _mockRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _service.AddExperienceAsync(characterId, addExp);

        // Assert
        result.Level.Should().Be(expectedLevel, $"Lv{startLevel} → Lv{expectedLevel} 레벨업");
        result.Experience.Should().Be(expectedExp, "초과 경험치 정확히 계산");
        result.Attack.Should().Be(expectedAttack, "자동 성장으로 공격력 증가");
        result.Defense.Should().Be(expectedDefense, "자동 성장으로 방어력 증가");
        result.MaxHealth.Should().Be(expectedMaxHealth, "자동 성장으로 최대 체력 증가");
    }

    [Fact]
    public async Task AddExperience_ShouldThrow_WhenCharacterNotFound()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetByIdAsync(characterId))
            .ReturnsAsync((Character?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.AddExperienceAsync(characterId, 100)
        );
    }

    #endregion

    #region CreateCharacterAsync Tests (캐릭터 생성)

    [Fact]
    public async Task CreateCharacter_ShouldSucceed_WhenUnderLimit()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        _mockRepository.Setup(r => r.CountByPlayerIdAsync(playerId))
            .ReturnsAsync(2); // 현재 2개 보유
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Character>()))
            .Returns((Character c) => Task.FromResult(c));
        _mockRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _service.CreateCharacterAsync(playerId, new CreateCharacterDto());

        // Assert
        result.Should().NotBeNull();
        result.Level.Should().Be(1);
        result.Attack.Should().Be(10, "초기 공격력 10");
        result.Defense.Should().Be(5, "초기 방어력 5");
        result.MaxHealth.Should().Be(100, "초기 최대 체력 100");
        result.CritRate.Should().BeApproximately(0.05f, 0.001f, "크리티컬 확률 5%");
        result.CritDamage.Should().BeApproximately(1.5f, 0.001f, "크리티컬 데미지 150%");
        result.Evasion.Should().BeApproximately(0.05f, 0.001f, "회피율 5%");
    }

    [Fact]
    public async Task CreateCharacter_ShouldThrow_WhenAtMaximumLimit()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        _mockRepository.Setup(r => r.CountByPlayerIdAsync(playerId))
            .ReturnsAsync(3); // 이미 3개 보유 (최대치)

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateCharacterAsync(playerId, new CreateCharacterDto())
        );

        exception.Message.Should().Contain("3개");
    }

    #endregion

    #region DeleteCharacterAsync Tests (캐릭터 삭제)

    [Fact]
    public async Task DeleteCharacter_ShouldSucceed_WhenCharacterExists()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var character = CreateTestCharacter(characterId, 1, 0);

        _mockRepository.Setup(r => r.GetByIdAsync(characterId))
            .ReturnsAsync(character);
        _mockRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        await _service.DeleteCharacterAsync(characterId);

        // Assert
        _mockRepository.Verify(r => r.Delete(character), Times.Once);
        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteCharacter_ShouldThrow_WhenCharacterNotFound()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetByIdAsync(characterId))
            .ReturnsAsync((Character?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.DeleteCharacterAsync(characterId)
        );
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// 테스트용 캐릭터 객체 생성 헬퍼 메서드 (자동 성장 방식)
    /// </summary>
    private Character CreateTestCharacter(Guid id, int level, int experience)
    {
        // 레벨에 따른 자동 성장 스탯 계산
        long attack = 10 + (level - 1) * 10;      // 초기 10 + 레벨당 +10
        long defense = 5 + (level - 1) * 5;       // 초기 5 + 레벨당 +5
        long maxHealth = 100 + (level - 1) * 50;  // 초기 100 + 레벨당 +50

        return new Character
        {
            Id = id,
            PlayerId = Guid.NewGuid(),
            Level = level,
            Experience = experience,
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

    #endregion
}
