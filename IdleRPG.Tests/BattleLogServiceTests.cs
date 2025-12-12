using FluentAssertions;
using IdleRPG.Application.BattleLog.Services;
using IdleRPG.Application.DTOs.Battle;
using IdleRPG.Application.Interfaces;
using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Repositories;
using IdleRPG.Infrastructure.Service;
using Microsoft.Extensions.Logging;
using Moq;
using BattleLogEntity = IdleRPG.Domain.Entities.BattleLog;

namespace IdleRPG.Tests;

/// <summary>
/// BattleLogService 단위 테스트
///
/// 검증 항목:
/// - 전투 로그 생성 및 저장 (SaveChanges 호출 안 함 검증)
/// - 로그 조회 (페이징)
/// - 최근 로그 조회
/// - 통계 조회
/// </summary>
public class BattleLogServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IBattleLogRepository> _mockBattleLogRepository;
    private readonly Mock<ILogger<BattleLogService>> _mockLogger;
    private readonly IBattleLogService _service;

    public BattleLogServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockBattleLogRepository = new Mock<IBattleLogRepository>();
        _mockLogger = new Mock<ILogger<BattleLogService>>();

        _mockUnitOfWork.Setup(u => u.BattleLogs).Returns(_mockBattleLogRepository.Object);

        _service = new BattleLogService(
            _mockUnitOfWork.Object,
            _mockLogger.Object);
    }

    #region CreateAndSaveLogAsync Tests

    [Fact]
    public async Task CreateAndSaveLogAsync_ValidLog_SavesSuccessfully()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var monsterId = Guid.NewGuid();
        var combatResult = new CombatResultDto
        {
            IsVictory = true,
            Statistics = new BattleStatisticsDto
            {
                TotalTurns = 10,
                TotalDamageDealt = 500,
                TotalDamageTaken = 100,
                CriticalHitCount = 2,
                EvasionCount = 1
            }
        };

        _mockBattleLogRepository.Setup(r => r.AddAsync(It.IsAny<BattleLogEntity>()))
            .ReturnsAsync((BattleLogEntity log) => log);

        // Act
        var result = await _service.CreateAndSaveLogAsync(
            characterId,
            monsterId,
            combatResult,
            experienceGained: 100,
            goldGained: 50,
            dungeonStageId: 1);

        // Assert
        result.Should().NotBeNull();
        result.CharacterId.Should().Be(characterId);
        result.MonsterId.Should().Be(monsterId);
        result.IsVictory.Should().BeTrue();
        result.ExperienceGained.Should().Be(100);
        result.GoldGained.Should().Be(50);
        result.DamageDealt.Should().Be(500);
        result.DamageTaken.Should().Be(100);
        result.DungeonStageId.Should().Be(1);

        // Repository.AddAsync 호출 검증
        _mockBattleLogRepository.Verify(r => r.AddAsync(It.IsAny<BattleLogEntity>()), Times.Once);

        // ⚠️ SaveChanges는 호출되지 않아야 함 (호출자가 트랜잭션 관리)
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAndSaveLogAsync_WithoutDungeonStageId_SavesSuccessfully()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var monsterId = Guid.NewGuid();
        var combatResult = new CombatResultDto
        {
            IsVictory = false,
            Statistics = new BattleStatisticsDto
            {
                TotalTurns = 5,
                TotalDamageDealt = 200,
                TotalDamageTaken = 300,
                CriticalHitCount = 0,
                EvasionCount = 0
            }
        };

        _mockBattleLogRepository.Setup(r => r.AddAsync(It.IsAny<BattleLogEntity>()))
            .ReturnsAsync((BattleLogEntity log) => log);

        // Act
        var result = await _service.CreateAndSaveLogAsync(
            characterId,
            monsterId,
            combatResult,
            experienceGained: 0,
            goldGained: 0);

        // Assert
        result.Should().NotBeNull();
        result.DungeonStageId.Should().BeNull();
        result.IsVictory.Should().BeFalse();
        result.ExperienceGained.Should().Be(0);
        result.GoldGained.Should().Be(0);
    }

    #endregion

    #region GetLogsAsync Tests

    [Fact]
    public async Task GetLogsAsync_ValidPaging_ReturnsCorrectPage()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var monsterId = Guid.NewGuid();

        var monster = new Monster { Id = monsterId, Name = "슬라임", Level = 5 };

        var battleLogs = new List<BattleLogEntity>
        {
            new BattleLogEntity
            {
                Id = Guid.NewGuid(),
                CharacterId = characterId,
                MonsterId = monsterId,
                Monster = monster,
                IsVictory = true,
                ExperienceGained = 100,
                GoldGained = 50,
                DamageDealt = 500,
                DamageTaken = 100,
                BattleDate = DateTime.UtcNow
            },
            new BattleLogEntity
            {
                Id = Guid.NewGuid(),
                CharacterId = characterId,
                MonsterId = monsterId,
                Monster = monster,
                IsVictory = false,
                ExperienceGained = 0,
                GoldGained = 0,
                DamageDealt = 200,
                DamageTaken = 300,
                BattleDate = DateTime.UtcNow.AddMinutes(-5)
            }
        };

        _mockBattleLogRepository.Setup(r => r.GetByCharacterIdAsync(characterId, 1, 20))
            .ReturnsAsync(battleLogs);

        _mockBattleLogRepository.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<BattleLogEntity, bool>>>()))
            .ReturnsAsync(battleLogs);

        // Act
        var result = await _service.GetLogsAsync(characterId, page: 1, pageSize: 20);

        // Assert
        result.Should().NotBeNull();
        result.Logs.Should().HaveCount(2);
        result.CurrentPage.Should().Be(1);
        result.PageSize.Should().Be(20);
        result.TotalCount.Should().Be(2);
        result.TotalPages.Should().Be(1);

        result.Logs[0].MonsterName.Should().Be("슬라임");
        result.Logs[0].MonsterLevel.Should().Be(5);
        result.Logs[0].IsVictory.Should().BeTrue();
    }

    #endregion

    #region GetRecentLogsAsync Tests

    [Fact]
    public async Task GetRecentLogsAsync_ValidCount_ReturnsRecentLogs()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var monsterId = Guid.NewGuid();

        var monster = new Monster { Id = monsterId, Name = "고블린", Level = 3 };

        var recentLogs = new List<BattleLogEntity>
        {
            new BattleLogEntity
            {
                Id = Guid.NewGuid(),
                CharacterId = characterId,
                MonsterId = monsterId,
                Monster = monster,
                IsVictory = true,
                ExperienceGained = 50,
                GoldGained = 25,
                DamageDealt = 300,
                DamageTaken = 50,
                BattleDate = DateTime.UtcNow
            }
        };

        _mockBattleLogRepository.Setup(r => r.GetRecentByCharacterIdAsync(characterId, 10))
            .ReturnsAsync(recentLogs);

        // Act
        var result = await _service.GetRecentLogsAsync(characterId, count: 10);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].MonsterName.Should().Be("고블린");
        result[0].IsVictory.Should().BeTrue();
    }

    #endregion

    #region GetStatsAsync Tests

    [Fact]
    public async Task GetStatsAsync_ValidCharacter_ReturnsAggregatedStats()
    {
        // Arrange
        var characterId = Guid.NewGuid();

        var stats = new BattleStatistics
        {
            TotalBattles = 100,
            Victories = 80,
            Defeats = 20,
            TotalExperience = 5000,
            TotalGold = 2500,
            TotalDamageDealt = 50000,
            TotalDamageTaken = 10000
        };

        _mockBattleLogRepository.Setup(r => r.GetStatsByCharacterIdAsync(characterId))
            .ReturnsAsync(stats);

        // Act
        var result = await _service.GetStatsAsync(characterId);

        // Assert
        result.Should().NotBeNull();
        result.TotalBattles.Should().Be(100);
        result.Victories.Should().Be(80);
        result.Defeats.Should().Be(20);
        result.WinRate.Should().BeApproximately(80.0, 0.01);
        result.TotalDamageDealt.Should().Be(50000);
        result.TotalDamageTaken.Should().Be(10000);
    }

    #endregion
}
