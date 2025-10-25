using FluentAssertions;
using IdleRPG.Application.Character.Services;
using IdleRPG.Application.DTOs.Battle;
using IdleRPG.Application.DTOs.Characters;
using IdleRPG.Application.DTOs.Dungeon;
using IdleRPG.Application.DTOs.Rewards;
using IdleRPG.Application.Interfaces;
using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Repositories;
using IdleRPG.Domain.Services;
using IdleRPG.Domain.ValueObjects;
using IdleRPG.Infrastructure.Service;
using Microsoft.Extensions.Logging;
using Moq;
using DifficultyEnum = IdleRPG.Domain.Enums.DungeonDifficulty;

namespace IdleRPG.Tests;

public class DungeonServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IDungeonStageRepository> _mockDungeonStageRepository;
    private readonly Mock<ICharacterDungeonProgressRepository> _mockCharacterDungeonProgressRepository;
    private readonly Mock<IBattleLogRepository> _mockBattleLogRepository;
    private readonly Mock<ICharacterRepository> _mockCharacterRepository;
    private readonly Mock<IBattleService> _mockBattleService;
    private readonly Mock<ICharacterService> _mockCharacterService;
    private readonly Mock<LootCalculator> _mockLootCalculator;
    private readonly Mock<ILogger<DungeonService>> _mockLogger;
    private readonly DungeonService _service;

    public DungeonServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockDungeonStageRepository = new Mock<IDungeonStageRepository>();
        _mockCharacterDungeonProgressRepository = new Mock<ICharacterDungeonProgressRepository>();
        _mockBattleLogRepository = new Mock<IBattleLogRepository>();
        _mockCharacterRepository = new Mock<ICharacterRepository>();
        _mockBattleService = new Mock<IBattleService>();
        _mockCharacterService = new Mock<ICharacterService>();
        _mockLootCalculator = new Mock<LootCalculator>();
        _mockLogger = new Mock<ILogger<DungeonService>>();

        _mockUnitOfWork.Setup(u => u.DungeonStages).Returns(_mockDungeonStageRepository.Object);
        _mockUnitOfWork.Setup(u => u.CharacterDungeonProgresses).Returns(_mockCharacterDungeonProgressRepository.Object);
        _mockUnitOfWork.Setup(u => u.BattleLogs).Returns(_mockBattleLogRepository.Object);
        _mockUnitOfWork.Setup(u => u.Characters).Returns(_mockCharacterRepository.Object);

        _service = new DungeonService(
            _mockUnitOfWork.Object,
            _mockCharacterService.Object,
            _mockBattleService.Object,
            _mockLootCalculator.Object,
            _mockLogger.Object);
    }

    #region Helper Methods

    private Character CreateTestCharacter(Guid id, Guid playerId, int level, int experience, long gold)
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
            PlayerId = playerId,
            Level = level,
            Experience = experience,
            Gold = gold,
            Stats = stats
        };
    }

    private DungeonStage CreateTestDungeonStage(int id, string name, int requiredLevel, Guid monsterId)
    {
        return new DungeonStage
        {
            Id = id,
            Name = name,
            RequiredLevel = requiredLevel,
            MonsterId = monsterId,
            BaseGold = 100 * id,
            BaseExperience = 50 * id,
            Monster = new Monster
            {
                Id = monsterId,
                Name = "Test Monster",
                Level = requiredLevel,
                Attack = 50,
                Defense = 30,
                MaxHealth = 500,
                AttackSpeed = 1.0f
            }
        };
    }

    private CharacterDungeonProgress CreateTestProgress(Guid characterId, int normalCleared, int hardCleared, int nightmareCleared)
    {
        return new CharacterDungeonProgress
        {
            Id = Guid.NewGuid(),
            CharacterId = characterId,
            HighestStageClearedNormal = normalCleared,
            HighestStageClearedHard = hardCleared,
            HighestStageClearedNightmare = nightmareCleared,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    #endregion

    #region ClearStageAsync - Success Cases

    [Fact]
    public async Task ClearStageAsync_ShouldSucceed_WhenBattleWon()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var monsterId = Guid.NewGuid();
        var character = CreateTestCharacter(characterId, playerId, level: 5, experience: 0, gold: 100);

        var stage = CreateTestDungeonStage(id: 1, name: "슬라임의 동굴", requiredLevel: 1, monsterId: monsterId);
        var progress = CreateTestProgress(characterId, normalCleared: 0, hardCleared: 0, nightmareCleared: 0);

        var request = new DungeonClearRequestDto
        {
            CharacterId = characterId,
            StageId = 1,
            Difficulty = DifficultyEnum.Normal
        };

        var battleResult = new BattleResultResponse
        {
            IsVictory = true,
            Reward = new RewardDto { Gold = 100, Experience = 50 },
            Statistics = new BattleStatisticsDto
            {
                TotalTurns = 10,
                TotalDamageDealt = 500,
                TotalDamageTaken = 100,
                CriticalHitCount = 2,
                EvasionCount = 1
            },
            BattleLog = new BattleLog
            {
                CharacterId = characterId,
                MonsterId = monsterId,
                DungeonStageId = 1,
                IsVictory = true,
                ExperienceGained = 50,
                GoldGained = 100,
                DamageDealt = 500,
                DamageTaken = 100
            }
        };

        _mockCharacterRepository
            .Setup(r => r.GetByIdAsync(characterId))
            .ReturnsAsync(character);

        _mockDungeonStageRepository
            .Setup(r => r.GetByIdWithMonsterAsync(1))
            .ReturnsAsync(stage);

        _mockCharacterDungeonProgressRepository
            .Setup(r => r.GetOrCreateByCharacterIdAsync(characterId))
            .ReturnsAsync(progress);

        _mockBattleService
            .Setup(s => s.SimulateBattleAsync(
                characterId,
                monsterId,
                1,
                DifficultyEnum.Normal))
            .ReturnsAsync(battleResult);

        _mockCharacterService
            .Setup(s => s.ProcessExperienceGain(It.IsAny<Character>(), 50))
            .Callback<Character, int>((c, exp) => c.Experience += exp);

        _mockBattleLogRepository
            .Setup(r => r.AddAsync(It.IsAny<BattleLog>()))
            .ReturnsAsync((BattleLog log) => log);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.ClearStageAsync(characterId, request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Reward.Should().NotBeNull();
        result.Reward.Gold.Should().Be(100);
        result.Reward.Experience.Should().Be(50);
        result.NewHighestStage.Should().Be(1);
        result.BattleStatistics.Should().NotBeNull();
        result.BattleStatistics.TotalTurns.Should().Be(10);

        // 트랜잭션 원자성 검증: SaveChangesAsync는 정확히 한 번만 호출
        _mockUnitOfWork.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once,
            "트랜잭션 원자성을 위해 SaveChangesAsync는 한 번만 호출되어야 함");

        // BattleLog 저장 검증
        _mockBattleLogRepository.Verify(
            r => r.AddAsync(It.Is<BattleLog>(log =>
                log.CharacterId == characterId &&
                log.DungeonStageId == 1 &&
                log.IsVictory == true)),
            Times.Once);
    }

    #endregion

    #region ClearStageAsync - Failure Cases

    [Fact]
    public async Task ClearStageAsync_ShouldFail_WhenBattleLost()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var monsterId = Guid.NewGuid();
        var character = CreateTestCharacter(characterId, playerId, level: 7, experience: 0, gold: 50);

        var stage = CreateTestDungeonStage(id: 5, name: "어두운 숲", requiredLevel: 7, monsterId: monsterId);
        var progress = CreateTestProgress(characterId, normalCleared: 4, hardCleared: 0, nightmareCleared: 0);

        var request = new DungeonClearRequestDto
        {
            CharacterId = characterId,
            StageId = 5,
            Difficulty = DifficultyEnum.Normal
        };

        var battleResult = new BattleResultResponse
        {
            IsVictory = false,
            Reward = new RewardDto(),
            Statistics = new BattleStatisticsDto
            {
                TotalTurns = 15,
                TotalDamageDealt = 200,
                TotalDamageTaken = 1000,
                CriticalHitCount = 1,
                EvasionCount = 0
            },
            BattleLog = new BattleLog
            {
                CharacterId = characterId,
                MonsterId = monsterId,
                DungeonStageId = 5,
                IsVictory = false,
                ExperienceGained = 0,
                GoldGained = 0,
                DamageDealt = 200,
                DamageTaken = 1000
            }
        };

        _mockCharacterRepository
            .Setup(r => r.GetByIdAsync(characterId))
            .ReturnsAsync(character);

        _mockDungeonStageRepository
            .Setup(r => r.GetByIdWithMonsterAsync(5))
            .ReturnsAsync(stage);

        _mockCharacterDungeonProgressRepository
            .Setup(r => r.GetOrCreateByCharacterIdAsync(characterId))
            .ReturnsAsync(progress);

        _mockBattleService
            .Setup(s => s.SimulateBattleAsync(
                characterId,
                monsterId,
                5,
                DifficultyEnum.Normal))
            .ReturnsAsync(battleResult);

        // Act
        var result = await _service.ClearStageAsync(characterId, request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("전투에서 패배했습니다");
        result.BattleStatistics.Should().NotBeNull("패배 시에도 전투 통계는 반환되어야 함");
        result.BattleStatistics.TotalTurns.Should().Be(15);
        result.BattleStatistics.TotalDamageTaken.Should().Be(1000);

        // 패배 시에는 보상이 없고 진행도 업데이트도 없음
        result.Reward.Should().BeNull();
        result.NewHighestStage.Should().Be(0);

        // 전투 패배는 early return이므로 SaveChangesAsync가 호출되지 않음
        _mockUnitOfWork.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never,
            "전투 패배 시에는 데이터베이스 변경이 없어야 함");

        // BattleLog도 저장되지 않음
        _mockBattleLogRepository.Verify(
            r => r.AddAsync(It.IsAny<BattleLog>()),
            Times.Never);
    }

    [Fact]
    public async Task ClearStageAsync_ShouldFail_WhenLevelTooLow()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var monsterId = Guid.NewGuid();
        var character = CreateTestCharacter(characterId, playerId, level: 3, experience: 0, gold: 50);

        var stage = CreateTestDungeonStage(id: 5, name: "버려진 광산", requiredLevel: 9, monsterId: monsterId);
        var progress = CreateTestProgress(characterId, normalCleared: 4, hardCleared: 0, nightmareCleared: 0);

        var request = new DungeonClearRequestDto
        {
            CharacterId = characterId,
            StageId = 5,
            Difficulty = DifficultyEnum.Normal
        };

        _mockCharacterRepository
            .Setup(r => r.GetByIdAsync(characterId))
            .ReturnsAsync(character);

        _mockDungeonStageRepository
            .Setup(r => r.GetByIdWithMonsterAsync(5))
            .ReturnsAsync(stage);

        _mockCharacterDungeonProgressRepository
            .Setup(r => r.GetOrCreateByCharacterIdAsync(characterId))
            .ReturnsAsync(progress);

        // Act
        var result = await _service.ClearStageAsync(characterId, request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().StartWith("레벨이 부족합니다");

        // 전투가 발생하지 않음
        _mockBattleService.Verify(
            s => s.SimulateBattleAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<int?>(),
                It.IsAny<DifficultyEnum?>()),
            Times.Never);
    }

    #endregion

    #region ClearStageAsync - Transaction Atomicity

    [Fact]
    public async Task ClearStageAsync_ShouldSaveAllChanges_InSingleTransaction()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var monsterId = Guid.NewGuid();
        var character = CreateTestCharacter(characterId, playerId, level: 10, experience: 0, gold: 100);

        var stage = CreateTestDungeonStage(id: 3, name: "고블린 마을", requiredLevel: 5, monsterId: monsterId);
        var progress = CreateTestProgress(characterId, normalCleared: 2, hardCleared: 0, nightmareCleared: 0);

        var request = new DungeonClearRequestDto
        {
            CharacterId = characterId,
            StageId = 3,
            Difficulty = DifficultyEnum.Normal
        };

        var battleResult = new BattleResultResponse
        {
            IsVictory = true,
            Reward = new RewardDto { Gold = 300, Experience = 150 },
            Statistics = new BattleStatisticsDto
            {
                TotalTurns = 20,
                TotalDamageDealt = 1500,
                TotalDamageTaken = 300,
                CriticalHitCount = 5,
                EvasionCount = 2
            },
            BattleLog = new BattleLog
            {
                CharacterId = characterId,
                MonsterId = monsterId,
                DungeonStageId = 3,
                IsVictory = true,
                ExperienceGained = 150,
                GoldGained = 300,
                DamageDealt = 1500,
                DamageTaken = 300
            }
        };

        _mockCharacterRepository
            .Setup(r => r.GetByIdAsync(characterId))
            .ReturnsAsync(character);

        _mockDungeonStageRepository
            .Setup(r => r.GetByIdWithMonsterAsync(3))
            .ReturnsAsync(stage);

        _mockCharacterDungeonProgressRepository
            .Setup(r => r.GetOrCreateByCharacterIdAsync(characterId))
            .ReturnsAsync(progress);

        _mockBattleService
            .Setup(s => s.SimulateBattleAsync(
                characterId,
                monsterId,
                3,
                DifficultyEnum.Normal))
            .ReturnsAsync(battleResult);

        _mockCharacterService
            .Setup(s => s.ProcessExperienceGain(It.IsAny<Character>(), 150))
            .Callback<Character, int>((c, exp) => c.Experience += exp);

        _mockBattleLogRepository
            .Setup(r => r.AddAsync(It.IsAny<BattleLog>()))
            .ReturnsAsync((BattleLog log) => log);

        var saveChangesCallCount = 0;
        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => saveChangesCallCount++)
            .ReturnsAsync(1);

        // Act
        var result = await _service.ClearStageAsync(characterId, request);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // 핵심 검증: SaveChangesAsync는 정확히 한 번만 호출
        saveChangesCallCount.Should().Be(1, "모든 변경사항은 단일 트랜잭션으로 저장되어야 함");

        _mockUnitOfWork.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once,
            "트랜잭션 원자성: 전투 결과, 보상, BattleLog가 모두 하나의 트랜잭션으로 저장되어야 함");

        // BattleLog가 SaveChanges 전에 AddAsync로 추가되었는지 검증
        _mockBattleLogRepository.Verify(
            r => r.AddAsync(It.IsAny<BattleLog>()),
            Times.Once);

        // 캐릭터 경험치 처리 검증
        _mockCharacterService.Verify(
            s => s.ProcessExperienceGain(It.IsAny<Character>(), 150),
            Times.Once);
    }

    #endregion

    #region ClearStageAsync - BattleStatistics Return

    [Fact]
    public async Task ClearStageAsync_ShouldReturnBattleStatistics_OnBothSuccessAndFailure()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var monsterId = Guid.NewGuid();
        var character = CreateTestCharacter(characterId, playerId, level: 15, experience: 0, gold: 500);

        var stage = CreateTestDungeonStage(id: 7, name: "독거미 둥지", requiredLevel: 13, monsterId: monsterId);
        var progress = CreateTestProgress(characterId, normalCleared: 6, hardCleared: 0, nightmareCleared: 0);

        var request = new DungeonClearRequestDto
        {
            CharacterId = characterId,
            StageId = 7,
            Difficulty = DifficultyEnum.Normal
        };

        var battleResult = new BattleResultResponse
        {
            IsVictory = true,
            Reward = new RewardDto { Gold = 700, Experience = 350 },
            Statistics = new BattleStatisticsDto
            {
                TotalTurns = 25,
                TotalDamageDealt = 3500,
                TotalDamageTaken = 600,
                CriticalHitCount = 8,
                EvasionCount = 3
            },
            BattleLog = new BattleLog
            {
                CharacterId = characterId,
                MonsterId = monsterId,
                DungeonStageId = 7,
                IsVictory = true,
                ExperienceGained = 350,
                GoldGained = 700,
                DamageDealt = 3500,
                DamageTaken = 600
            }
        };

        _mockCharacterRepository
            .Setup(r => r.GetByIdAsync(characterId))
            .ReturnsAsync(character);

        _mockDungeonStageRepository
            .Setup(r => r.GetByIdWithMonsterAsync(7))
            .ReturnsAsync(stage);

        _mockCharacterDungeonProgressRepository
            .Setup(r => r.GetOrCreateByCharacterIdAsync(characterId))
            .ReturnsAsync(progress);

        _mockBattleService
            .Setup(s => s.SimulateBattleAsync(
                characterId,
                monsterId,
                7,
                DifficultyEnum.Normal))
            .ReturnsAsync(battleResult);

        _mockCharacterService
            .Setup(s => s.ProcessExperienceGain(It.IsAny<Character>(), 350))
            .Callback<Character, int>((c, exp) => c.Experience += exp);

        _mockBattleLogRepository
            .Setup(r => r.AddAsync(It.IsAny<BattleLog>()))
            .ReturnsAsync((BattleLog log) => log);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.ClearStageAsync(characterId, request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.BattleStatistics.Should().NotBeNull();
        result.BattleStatistics.TotalTurns.Should().Be(25);
        result.BattleStatistics.TotalDamageDealt.Should().Be(3500);
        result.BattleStatistics.TotalDamageTaken.Should().Be(600);
        result.BattleStatistics.CriticalHitCount.Should().Be(8);
        result.BattleStatistics.EvasionCount.Should().Be(3);
    }

    #endregion
}
