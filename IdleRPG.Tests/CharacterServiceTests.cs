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
        var character = CreateTestCharacter(characterId, level: 1, experience: 0, statPoints: 0);

        _mockRepository.Setup(r => r.GetByIdAsync(characterId))
            .ReturnsAsync(character);
        _mockRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _service.AddExperienceAsync(characterId, 50);

        // Assert
        result.Level.Should().Be(1, "50 경험치는 레벨업에 부족함 (필요: 100)");
        result.Experience.Should().Be(50, "경험치가 누적되어야 함");
        result.StatPoints.Should().Be(0, "레벨업하지 않았으므로 스탯 포인트 증가 없음");
    }

    [Fact]
    public async Task AddExperience_ShouldLevelUpOnce_WhenExperienceExactlyMeetsRequirement()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var character = CreateTestCharacter(characterId, level: 1, experience: 0, statPoints: 0);

        _mockRepository.Setup(r => r.GetByIdAsync(characterId))
            .ReturnsAsync(character);
        _mockRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _service.AddExperienceAsync(characterId, 100);

        // Assert
        result.Level.Should().Be(2, "100 경험치로 Lv1 → Lv2 레벨업");
        result.Experience.Should().Be(0, "정확히 레벨업했으므로 남은 경험치 0");
        result.StatPoints.Should().Be(5, "레벨업 시 스탯 포인트 +5");
    }

    [Fact]
    public async Task AddExperience_ShouldLevelUpOnce_WithExcessExperience()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var character = CreateTestCharacter(characterId, level: 1, experience: 0, statPoints: 0);

        _mockRepository.Setup(r => r.GetByIdAsync(characterId))
            .ReturnsAsync(character);
        _mockRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _service.AddExperienceAsync(characterId, 150);

        // Assert
        result.Level.Should().Be(2, "150 경험치로 Lv1 → Lv2 레벨업");
        result.Experience.Should().Be(50, "초과 경험치 50이 다음 레벨로 이월");
        result.StatPoints.Should().Be(5, "레벨업 1회 = 스탯 포인트 +5");
    }

    [Theory]
    [InlineData(1, 0, 250, 2, 150, 5)]   // Lv1 → Lv2 (100 소모, 150 남음, 스탯 +5)
    [InlineData(2, 0, 500, 4, 0, 10)]    // Lv2 → Lv4 (200 + 300 소모, 0 남음, 스탯 +10)
    [InlineData(1, 50, 450, 3, 200, 10)] // Lv1(50 exp) → Lv3 (100 + 200 소모, 200 남음, 스탯 +10)
    [InlineData(1, 0, 350, 3, 50, 10)]   // Lv1 → Lv3 (100 + 200 소모, 50 남음, 스탯 +10)
    [InlineData(1, 0, 600, 4, 0, 15)]    // Lv1 → Lv4 (100 + 200 + 300 소모, 0 남음, 스탯 +15)
    public async Task AddExperience_ShouldLevelUpMultipleTimes(
        int startLevel, int startExp, int addExp,
        int expectedLevel, int expectedExp, int expectedStatPoints)
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var character = CreateTestCharacter(characterId, startLevel, startExp, 0);

        _mockRepository.Setup(r => r.GetByIdAsync(characterId))
            .ReturnsAsync(character);
        _mockRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _service.AddExperienceAsync(characterId, addExp);

        // Assert
        result.Level.Should().Be(expectedLevel, $"Lv{startLevel} → Lv{expectedLevel} 레벨업");
        result.Experience.Should().Be(expectedExp, "초과 경험치 정확히 계산");
        result.StatPoints.Should().Be(expectedStatPoints, "레벨업 횟수만큼 스탯 포인트 증가");
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

    #region AllocateStatPointsAsync Tests (스탯 분배)

    [Fact]
    public async Task AllocateStats_ShouldDistributeCorrectly_WhenValidInput()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var character = CreateTestCharacter(characterId, level: 2, experience: 0, statPoints: 10);

        _mockRepository.Setup(r => r.GetByIdAsync(characterId))
            .ReturnsAsync(character);
        _mockRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _service.AllocateStatPointsAsync(characterId, 3, 2, 2, 3);

        // Assert
        result.Strength.Should().Be(8, "초기 5 + 3 = 8");
        result.Dexterity.Should().Be(7, "초기 5 + 2 = 7");
        result.Intelligence.Should().Be(7, "초기 5 + 2 = 7");
        result.Vitality.Should().Be(13, "초기 10 + 3 = 13");
        result.StatPoints.Should().Be(0, "10 포인트 모두 사용");
    }

    [Fact]
    public async Task AllocateStats_ShouldThrow_WhenInsufficientStatPoints()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var character = CreateTestCharacter(characterId, level: 1, experience: 0, statPoints: 3);

        _mockRepository.Setup(r => r.GetByIdAsync(characterId))
            .ReturnsAsync(character);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.AllocateStatPointsAsync(characterId, 2, 2, 0, 0) // 총 4 포인트 시도
        );

        exception.Message.Should().Contain("부족");
    }

    [Fact]
    public async Task AllocateStats_ShouldThrow_WhenNegativeStatValue()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var character = CreateTestCharacter(characterId, level: 1, experience: 0, statPoints: 5);

        _mockRepository.Setup(r => r.GetByIdAsync(characterId))
            .ReturnsAsync(character);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.AllocateStatPointsAsync(characterId, -1, 2, 2, 1)
        );

        exception.Message.Should().Contain("음수");
    }

    [Fact]
    public async Task AllocateStats_ShouldThrow_WhenCharacterNotFound()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetByIdAsync(characterId))
            .ReturnsAsync((Character?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.AllocateStatPointsAsync(characterId, 1, 1, 1, 1)
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
        result.StatPoints.Should().Be(0);
        result.Strength.Should().Be(5);
        result.Vitality.Should().Be(10);
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
        var character = CreateTestCharacter(characterId, 1, 0, 0);

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
    /// 테스트용 캐릭터 객체 생성 헬퍼 메서드
    /// </summary>
    private Character CreateTestCharacter(Guid id, int level, int experience, int statPoints)
    {
        return new Character
        {
            Id = id,
            PlayerId = Guid.NewGuid(),
            Level = level,
            Experience = experience,
            StatPoints = statPoints,
            Stats = new CharacterStats(5, 5, 5, 10), // 기본 스탯
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    #endregion
}
