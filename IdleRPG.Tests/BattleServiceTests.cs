using FluentAssertions;
using IdleRPG.Application.Character.Services;
using IdleRPG.Application.DTOs.Battle;
using IdleRPG.Application.DTOs.Characters;
using IdleRPG.Application.Interfaces;
using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Repositories;
using IdleRPG.Domain.ValueObjects;
using IdleRPG.Infrastructure.Service;
using Microsoft.Extensions.Logging;
using Moq;

namespace IdleRPG.Tests;

public class BattleServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ICharacterRepository> _mockCharacterRepository;
    private readonly Mock<IMonsterRepository> _mockMonsterRepository;
    private readonly Mock<IBattleLogRepository> _mockBattleLogRepository;
    private readonly Mock<ICharacterService> _mockCharacterService;
    private readonly Mock<ILogger<BattleService>> _mockLogger;
    private readonly BattleService _service;

    public BattleServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockCharacterRepository = new Mock<ICharacterRepository>();
        _mockMonsterRepository = new Mock<IMonsterRepository>();
        _mockBattleLogRepository = new Mock<IBattleLogRepository>();
        _mockCharacterService = new Mock<ICharacterService>();
        _mockLogger = new Mock<ILogger<BattleService>>();

        _mockUnitOfWork.Setup(u => u.Characters).Returns(_mockCharacterRepository.Object);
        _mockUnitOfWork.Setup(u => u.Monsters).Returns(_mockMonsterRepository.Object);
        _mockUnitOfWork.Setup(u => u.BattleLogs).Returns(_mockBattleLogRepository.Object);

        _service = new BattleService(
            _mockUnitOfWork.Object,
            _mockCharacterService.Object,
            _mockLogger.Object);
    }

    #region Helper Methods

    private Character CreateTestCharacter(Guid id, int level, int experience, long gold)
    {
        var stats = new CharacterStats(
            attack: level * 10,
            defense: level * 5,
            maxHealth: 100 + (level * 50),
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
            Experience = experience,
            Gold = gold,
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

    #region SimulateBattleAsync Tests

    [Fact]
    public async Task SimulateBattleAsync_ShouldThrowException_WhenCharacterNotFound()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var monsterId = Guid.NewGuid();

        _mockCharacterRepository
            .Setup(r => r.GetByIdAsync(characterId))
            .ReturnsAsync((Character)null!);

        // Act
        Func<Task> act = async () => await _service.SimulateBattleAsync(characterId, monsterId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("캐릭터를 찾을 수 없습니다");
    }

    [Fact]
    public async Task SimulateBattleAsync_ShouldThrowException_WhenMonsterNotFound()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var monsterId = Guid.NewGuid();
        var character = CreateTestCharacter(characterId, 5, 0, 100);

        _mockCharacterRepository
            .Setup(r => r.GetByIdAsync(characterId))
            .ReturnsAsync(character);

        _mockMonsterRepository
            .Setup(r => r.GetByIdAsync(monsterId))
            .ReturnsAsync((Monster)null!);

        // Act
        Func<Task> act = async () => await _service.SimulateBattleAsync(characterId, monsterId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("몬스터를 찾을 수 없습니다");
    }

    [Fact]
    public async Task SimulateBattleAsync_ShouldReturnVictory_WhenCharacterIsStronger()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var monsterId = Guid.NewGuid();
        var character = CreateTestCharacter(characterId, level: 10, experience: 0, gold: 100);
        var monster = CreateTestMonster(monsterId, "슬라임", level: 1, attack: 5, defense: 2, maxHealth: 50);

        _mockCharacterRepository
            .Setup(r => r.GetByIdAsync(characterId))
            .ReturnsAsync(character);

        _mockMonsterRepository
            .Setup(r => r.GetByIdAsync(monsterId))
            .ReturnsAsync(monster);

        var leveledCharacter = new CharacterDto { Id = characterId, Level = 10, Experience = 50, Gold = 110 };

        _mockCharacterService
            .Setup(s => s.ProcessExperienceGain(It.IsAny<Character>(), It.IsAny<int>()))
            .Callback<Character, int>((c, exp) => { c.Experience += exp; });

        _mockCharacterService
            .Setup(s => s.GetCharacterByIdAsync(characterId))
            .ReturnsAsync(leveledCharacter);

        _mockBattleLogRepository
            .Setup(r => r.AddAsync(It.IsAny<BattleLog>()))
            .ReturnsAsync((BattleLog log) => log);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.SimulateBattleAsync(characterId, monsterId);

        // Assert
        result.IsVictory.Should().BeTrue();
        result.Reward.Should().NotBeNull();
        result.Reward.Experience.Should().BeGreaterThan(0);
        result.Reward.Gold.Should().BeGreaterThan(0);
        result.Statistics.TotalTurns.Should().BeGreaterThan(0);
        result.UpdatedCharacter.Should().NotBeNull();

        _mockBattleLogRepository.Verify(
            r => r.AddAsync(It.Is<BattleLog>(log =>
                log.CharacterId == characterId &&
                log.MonsterId == monsterId &&
                log.IsVictory == true)),
            Times.Once);

        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task SimulateBattleAsync_ShouldReturnDefeat_WhenMonsterIsStronger()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var monsterId = Guid.NewGuid();
        var character = CreateTestCharacter(characterId, level: 1, experience: 0, gold: 100);
        var monster = CreateTestMonster(monsterId, "드래곤", level: 50, attack: 500, defense: 300, maxHealth: 5000);

        _mockCharacterRepository
            .Setup(r => r.GetByIdAsync(characterId))
            .ReturnsAsync(character);

        _mockMonsterRepository
            .Setup(r => r.GetByIdAsync(monsterId))
            .ReturnsAsync(monster);

        _mockBattleLogRepository
            .Setup(r => r.AddAsync(It.IsAny<BattleLog>()))
            .ReturnsAsync((BattleLog log) => log);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.SimulateBattleAsync(characterId, monsterId);

        // Assert
        result.IsVictory.Should().BeFalse();
        result.Reward.Experience.Should().Be(0);
        result.Reward.Gold.Should().Be(0);
        result.Statistics.TotalTurns.Should().BeGreaterThan(0);

        _mockBattleLogRepository.Verify(
            r => r.AddAsync(It.Is<BattleLog>(log =>
                log.CharacterId == characterId &&
                log.MonsterId == monsterId &&
                log.IsVictory == false)),
            Times.Once);

        _mockCharacterService.Verify(
            s => s.ProcessExperienceGain(It.IsAny<Character>(), It.IsAny<int>()),
            Times.Never);
    }

    [Fact]
    public async Task SimulateBattleAsync_ShouldApplyRewards_WhenVictorious()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var monsterId = Guid.NewGuid();
        var character = CreateTestCharacter(characterId, level: 5, experience: 0, gold: 50);
        var monster = CreateTestMonster(monsterId, "고블린", level: 5, attack: 25, defense: 15, maxHealth: 250);

        _mockCharacterRepository
            .Setup(r => r.GetByIdAsync(characterId))
            .ReturnsAsync(character);

        _mockMonsterRepository
            .Setup(r => r.GetByIdAsync(monsterId))
            .ReturnsAsync(monster);

        var leveledUpCharacter = new CharacterDto { Id = characterId, Level = 6, Experience = 50, Gold = 100 };

        _mockCharacterService
            .Setup(s => s.ProcessExperienceGain(It.IsAny<Character>(), It.IsAny<int>()))
            .Callback<Character, int>((c, exp) =>
            {
                c.Experience += exp;
                c.Level = 6; // 레벨업 시뮬레이션
            });

        _mockCharacterService
            .Setup(s => s.GetCharacterByIdAsync(characterId))
            .ReturnsAsync(leveledUpCharacter);

        _mockBattleLogRepository
            .Setup(r => r.AddAsync(It.IsAny<BattleLog>()))
            .ReturnsAsync((BattleLog log) => log);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.SimulateBattleAsync(characterId, monsterId);

        // Assert
        result.IsVictory.Should().BeTrue();
        result.UpdatedCharacter.Should().NotBeNull();
        result.UpdatedCharacter.Level.Should().Be(6);

        _mockCharacterService.Verify(
            s => s.ProcessExperienceGain(It.IsAny<Character>(), It.IsAny<int>()),
            Times.Once);
    }

    #endregion

    #region GetBattleLogsAsync Tests

    [Fact]
    public async Task GetBattleLogsAsync_ShouldReturnPagedLogs()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var monsterId = Guid.NewGuid();
        var monster = CreateTestMonster(monsterId, "슬라임", 1, 10, 5, 50);

        var logs = new List<BattleLog>
        {
            new BattleLog { CharacterId = characterId, MonsterId = monsterId, Monster = monster, IsVictory = true, ExperienceGained = 100, GoldGained = 20, DamageDealt = 150, DamageTaken = 30, BattleDate = DateTime.UtcNow },
            new BattleLog { CharacterId = characterId, MonsterId = monsterId, Monster = CreateTestMonster(Guid.NewGuid(), "고블린", 3, 15, 8, 100), IsVictory = true, ExperienceGained = 300, GoldGained = 60, DamageDealt = 450, DamageTaken = 90, BattleDate = DateTime.UtcNow },
            new BattleLog { CharacterId = characterId, MonsterId = monsterId, Monster = CreateTestMonster(Guid.NewGuid(), "오크", 5, 25, 15, 250), IsVictory = false, ExperienceGained = 0, GoldGained = 0, DamageDealt = 200, DamageTaken = 400, BattleDate = DateTime.UtcNow }
        };

        _mockBattleLogRepository
            .Setup(r => r.GetByCharacterIdAsync(characterId, 1, 10))
            .ReturnsAsync(logs);

        _mockBattleLogRepository
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<BattleLog, bool>>>()))
            .ReturnsAsync(logs);

        // Act
        var result = await _service.GetBattleLogsAsync(characterId, 1, 10);

        // Assert
        result.Logs.Should().HaveCount(3);
        result.CurrentPage.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().Be(3);
        result.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task GetBattleLogsAsync_ShouldReturnEmptyList_WhenNoLogsExist()
    {
        // Arrange
        var characterId = Guid.NewGuid();

        _mockBattleLogRepository
            .Setup(r => r.GetByCharacterIdAsync(characterId, 1, 10))
            .ReturnsAsync(new List<BattleLog>());

        _mockBattleLogRepository
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<BattleLog, bool>>>()))
            .ReturnsAsync(new List<BattleLog>());

        // Act
        var result = await _service.GetBattleLogsAsync(characterId, 1, 10);

        // Assert
        result.Logs.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    #endregion

    #region GetRecentBattleLogsAsync Tests

    [Fact]
    public async Task GetRecentBattleLogsAsync_ShouldReturnRecentLogs()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var dragon = CreateTestMonster(Guid.NewGuid(), "드래곤", 10, 500, 300, 5000);
        var troll = CreateTestMonster(Guid.NewGuid(), "트롤", 8, 400, 200, 3000);

        var recentLogs = new List<BattleLog>
        {
            new BattleLog { CharacterId = characterId, MonsterId = dragon.Id, Monster = dragon, IsVictory = false, ExperienceGained = 0, GoldGained = 0, DamageDealt = 500, DamageTaken = 1000, BattleDate = DateTime.UtcNow.AddMinutes(-1) },
            new BattleLog { CharacterId = characterId, MonsterId = troll.Id, Monster = troll, IsVictory = true, ExperienceGained = 800, GoldGained = 160, DamageDealt = 1200, DamageTaken = 400, BattleDate = DateTime.UtcNow.AddMinutes(-5) }
        };

        _mockBattleLogRepository
            .Setup(r => r.GetRecentByCharacterIdAsync(characterId, 5))
            .ReturnsAsync(recentLogs);

        // Act
        var result = await _service.GetRecentBattleLogsAsync(characterId, 5);

        // Assert
        result.Should().HaveCount(2);
        result.First().MonsterName.Should().Be("드래곤");
        result.Last().MonsterName.Should().Be("트롤");
    }

    #endregion

    #region GetBattleStatsAsync Tests

    [Fact]
    public async Task GetBattleStatsAsync_ShouldReturnCorrectStatistics()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var stats = new Domain.Repositories.BattleStatistics
        {
            TotalBattles = 4,
            Victories = 3,
            Defeats = 1,
            TotalExperience = 1200,
            TotalGold = 240,
            TotalDamageDealt = 2000,
            TotalDamageTaken = 870
        };

        _mockBattleLogRepository
            .Setup(r => r.GetStatsByCharacterIdAsync(characterId))
            .ReturnsAsync(stats);

        // Act
        var result = await _service.GetBattleStatsAsync(characterId);

        // Assert
        result.TotalBattles.Should().Be(4);
        result.Victories.Should().Be(3);
        result.Defeats.Should().Be(1);
        result.WinRate.Should().BeApproximately(75.0f, 0.01f);
        result.TotalExperience.Should().Be(1200);
        result.TotalGold.Should().Be(240);
        result.TotalDamageDealt.Should().Be(2000);
        result.TotalDamageTaken.Should().Be(870);
    }

    [Fact]
    public async Task GetBattleStatsAsync_ShouldReturnZeroStats_WhenNoLogsExist()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var stats = new Domain.Repositories.BattleStatistics
        {
            TotalBattles = 0,
            Victories = 0,
            Defeats = 0,
            TotalExperience = 0,
            TotalGold = 0,
            TotalDamageDealt = 0,
            TotalDamageTaken = 0
        };

        _mockBattleLogRepository
            .Setup(r => r.GetStatsByCharacterIdAsync(characterId))
            .ReturnsAsync(stats);

        // Act
        var result = await _service.GetBattleStatsAsync(characterId);

        // Assert
        result.TotalBattles.Should().Be(0);
        result.Victories.Should().Be(0);
        result.Defeats.Should().Be(0);
        result.WinRate.Should().Be(0);
        result.TotalExperience.Should().Be(0);
        result.TotalGold.Should().Be(0);
        result.TotalDamageDealt.Should().Be(0);
        result.TotalDamageTaken.Should().Be(0);
    }

    #endregion
}
