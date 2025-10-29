using FluentAssertions;
using IdleRPG.Application.Combat.Services;
using IdleRPG.Application.Interfaces;
using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Repositories;
using IdleRPG.Domain.ValueObjects;
using IdleRPG.Infrastructure.Service;
using Microsoft.Extensions.Logging;
using Moq;
using DungeonDifficultyEnum = IdleRPG.Domain.Enums.DungeonDifficulty;

namespace IdleRPG.Tests;

/// <summary>
/// CombatService 단위 테스트
///
/// 검증 항목:
/// - 전투 시뮬레이션 정확성 (승리/패배 판정)
/// - 난이도 배율 적용
/// - 크리티컬/회피 통계 수집
/// - 예외 처리 (캐릭터/몬스터 미존재)
/// </summary>
public class CombatServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ICharacterRepository> _mockCharacterRepository;
    private readonly Mock<IMonsterRepository> _mockMonsterRepository;
    private readonly Mock<ILogger<CombatService>> _mockLogger;
    private readonly ICombatService _service;

    public CombatServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockCharacterRepository = new Mock<ICharacterRepository>();
        _mockMonsterRepository = new Mock<IMonsterRepository>();
        _mockLogger = new Mock<ILogger<CombatService>>();

        _mockUnitOfWork.Setup(u => u.Characters).Returns(_mockCharacterRepository.Object);
        _mockUnitOfWork.Setup(u => u.Monsters).Returns(_mockMonsterRepository.Object);

        _service = new CombatService(
            _mockUnitOfWork.Object,
            _mockLogger.Object);
    }

    #region Helper Methods

    private Character CreateTestCharacter(Guid id, int level, long attack, long defense, long maxHealth)
    {
        var stats = new CharacterStats(
            attack: attack,
            defense: defense,
            maxHealth: maxHealth,
            critRate: 0.15f,
            critDamage: 1.5f,
            evasion: 0.1f,
            attackSpeed: 1.0f
        );

        return new Character
        {
            Id = id,
            PlayerId = Guid.NewGuid(),
            Level = level,
            Experience = 0,
            Gold = 0,
            Stats = stats
        };
    }

    private Monster CreateTestMonster(Guid id, string name, int level, int attack, int defense, int maxHealth)
    {
        return new Monster
        {
            Id = id,
            Name = name,
            Level = level,
            Attack = attack,
            Defense = defense,
            MaxHealth = maxHealth,
            AttackSpeed = 0.8f,
            Evasion = 0.05f,
            ExperienceReward = level * 100,
            GoldReward = level * 20,
            CritRate = 0.1f,
            CritDamage = 1.5f
        };
    }

    #endregion

    #region SimulateCombatAsync Tests

    [Fact]
    public async Task SimulateCombatAsync_CharacterWins_ReturnsVictory()
    {
        // Arrange: 캐릭터가 압도적으로 강함
        var characterId = Guid.NewGuid();
        var monsterId = Guid.NewGuid();

        var character = CreateTestCharacter(characterId, level: 10, attack: 100, defense: 50, maxHealth: 1000);
        var monster = CreateTestMonster(monsterId, "약한 슬라임", level: 1, attack: 5, defense: 2, maxHealth: 50);

        _mockCharacterRepository.Setup(r => r.GetByIdAsync(characterId))
            .ReturnsAsync(character);
        _mockMonsterRepository.Setup(r => r.GetByIdAsync(monsterId))
            .ReturnsAsync(monster);

        // Act
        var result = await _service.SimulateCombatAsync(characterId, monsterId);

        // Assert
        result.Should().NotBeNull();
        result.IsVictory.Should().BeTrue("캐릭터가 압도적으로 강하므로 승리해야 함");
        result.Statistics.Should().NotBeNull();
        result.Statistics.TotalDamageDealt.Should().BeGreaterThan(0, "데미지를 입혔어야 함");
    }

    [Fact]
    public async Task SimulateCombatAsync_CharacterLoses_ReturnsDefeat()
    {
        // Arrange: 몬스터가 압도적으로 강함
        var characterId = Guid.NewGuid();
        var monsterId = Guid.NewGuid();

        var character = CreateTestCharacter(characterId, level: 1, attack: 5, defense: 2, maxHealth: 50);
        var monster = CreateTestMonster(monsterId, "강한 드래곤", level: 100, attack: 500, defense: 100, maxHealth: 10000);

        _mockCharacterRepository.Setup(r => r.GetByIdAsync(characterId))
            .ReturnsAsync(character);
        _mockMonsterRepository.Setup(r => r.GetByIdAsync(monsterId))
            .ReturnsAsync(monster);

        // Act
        var result = await _service.SimulateCombatAsync(characterId, monsterId);

        // Assert
        result.Should().NotBeNull();
        result.IsVictory.Should().BeFalse("몬스터가 압도적으로 강하므로 패배해야 함");
        result.Statistics.TotalDamageTaken.Should().BeGreaterThan(0, "데미지를 받았어야 함");
    }

    [Fact]
    public async Task SimulateCombatAsync_WithDifficulty_AppliesMultiplier()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var monsterId = Guid.NewGuid();

        var character = CreateTestCharacter(characterId, level: 10, attack: 100, defense: 20, maxHealth: 1000);
        var monster = CreateTestMonster(monsterId, "슬라임", level: 5, attack: 50, defense: 10, maxHealth: 100);

        _mockCharacterRepository.Setup(r => r.GetByIdAsync(characterId))
            .ReturnsAsync(character);
        _mockMonsterRepository.Setup(r => r.GetByIdAsync(monsterId))
            .ReturnsAsync(monster);

        // Act
        var resultNormal = await _service.SimulateCombatAsync(characterId, monsterId, DungeonDifficultyEnum.Normal);
        var resultHard = await _service.SimulateCombatAsync(characterId, monsterId, DungeonDifficultyEnum.Hard);

        // Assert
        resultNormal.Should().NotBeNull();
        resultHard.Should().NotBeNull();
        resultHard.Statistics.TotalDamageTaken.Should().BeGreaterThan(
            resultNormal.Statistics.TotalDamageTaken,
            "Hard 난이도는 몬스터 스탯이 증가하므로 더 많은 데미지를 받아야 함");
    }

    [Fact]
    public async Task SimulateCombatAsync_CriticalHit_IncreasesCount()
    {
        // Arrange: 크리티컬 확률 100%
        var characterId = Guid.NewGuid();
        var monsterId = Guid.NewGuid();

        var stats = new CharacterStats(
            attack: 100,
            defense: 50,
            maxHealth: 1000,
            critRate: 1.0f, // 100% 크리티컬
            critDamage: 2.0f,
            evasion: 0f,
            attackSpeed: 1.0f
        );

        var character = new Character
        {
            Id = characterId,
            PlayerId = Guid.NewGuid(),
            Level = 10,
            Stats = stats
        };

        var monster = CreateTestMonster(monsterId, "슬라임", level: 1, attack: 5, defense: 2, maxHealth: 50);

        _mockCharacterRepository.Setup(r => r.GetByIdAsync(characterId))
            .ReturnsAsync(character);
        _mockMonsterRepository.Setup(r => r.GetByIdAsync(monsterId))
            .ReturnsAsync(monster);

        // Act
        var result = await _service.SimulateCombatAsync(characterId, monsterId);

        // Assert
        result.Statistics.CriticalHitCount.Should().BeGreaterThan(0, "크리티컬 확률이 100%이므로 크리티컬이 발생해야 함");
    }

    [Fact]
    public async Task SimulateCombatAsync_Evasion_ReducesDamage()
    {
        // Arrange: 회피율 100% (테스트용)
        var characterId = Guid.NewGuid();
        var monsterId = Guid.NewGuid();

        var stats = new CharacterStats(
            attack: 100,
            defense: 50,
            maxHealth: 1000,
            critRate: 0f,
            critDamage: 1.5f,
            evasion: 0f, // 캐릭터 회피 없음
            attackSpeed: 1.0f
        );

        var character = new Character
        {
            Id = characterId,
            PlayerId = Guid.NewGuid(),
            Level = 10,
            Stats = stats
        };

        var monster = CreateTestMonster(monsterId, "슬라임", level: 1, attack: 5, defense: 2, maxHealth: 50);
        monster.Evasion = 1.0f; // 100% 회피 (테스트용)

        _mockCharacterRepository.Setup(r => r.GetByIdAsync(characterId))
            .ReturnsAsync(character);
        _mockMonsterRepository.Setup(r => r.GetByIdAsync(monsterId))
            .ReturnsAsync(monster);

        // Act
        var result = await _service.SimulateCombatAsync(characterId, monsterId);

        // Assert
        result.Statistics.EvasionCount.Should().BeGreaterThan(0, "몬스터 회피율이 100%이므로 회피가 발생해야 함");
    }

    [Fact]
    public async Task SimulateCombatAsync_ShouldThrowException_WhenCharacterNotFound()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var monsterId = Guid.NewGuid();

        _mockCharacterRepository.Setup(r => r.GetByIdAsync(characterId))
            .ReturnsAsync((Character?)null);

        // Act
        Func<Task> act = async () => await _service.SimulateCombatAsync(characterId, monsterId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("캐릭터를 찾을 수 없습니다");
    }

    [Fact]
    public async Task SimulateCombatAsync_ShouldThrowException_WhenMonsterNotFound()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var monsterId = Guid.NewGuid();

        var character = CreateTestCharacter(characterId, level: 10, attack: 100, defense: 50, maxHealth: 1000);

        _mockCharacterRepository.Setup(r => r.GetByIdAsync(characterId))
            .ReturnsAsync(character);
        _mockMonsterRepository.Setup(r => r.GetByIdAsync(monsterId))
            .ReturnsAsync((Monster?)null);

        // Act
        Func<Task> act = async () => await _service.SimulateCombatAsync(characterId, monsterId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("몬스터를 찾을 수 없습니다");
    }

    #endregion
}
