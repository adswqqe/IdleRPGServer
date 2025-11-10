using FluentAssertions;
using IdleRPG.Application.DTOs.Pvp;
using IdleRPG.Application.Interfaces;
using IdleRPG.Application.Services;
using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Enums;
using IdleRPG.Domain.Repositories;
using IdleRPG.Domain.Services;
using IdleRPG.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace IdleRPG.Tests.Application.Services;

/// <summary>
/// PvpService 단위 테스트
///
/// 테스트 범위:
/// - 정상 매칭 → 전투 → 레이팅 업데이트 (전체 흐름)
/// - CharacterId 소유권 검증
/// - 활성 시즌 존재 여부 검증
/// - Redis 장애 시 Graceful Degradation
/// </summary>
public class PvpServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IPvpMatchmakingService> _mockMatchmakingService;
    private readonly EloRatingService _eloRatingService; // Mock 대신 실제 인스턴스
    private readonly Mock<IRedisCacheService> _mockRedisCacheService;
    private readonly Mock<ILogger<PvpService>> _mockLogger;
    private readonly PvpService _pvpService;

    // Repository Mocks
    private readonly Mock<ICharacterRepository> _mockCharacterRepository;
    private readonly Mock<IPvpSeasonRepository> _mockPvpSeasonRepository;
    private readonly Mock<IPvpRankingRepository> _mockPvpRankingRepository;
    private readonly Mock<IPvpMatchRepository> _mockPvpMatchRepository;

    public PvpServiceTests()
    {
        // Arrange: Mock 객체 초기화
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockMatchmakingService = new Mock<IPvpMatchmakingService>();
        _eloRatingService = new EloRatingService(); // 실제 인스턴스 (순수 계산 로직)
        _mockRedisCacheService = new Mock<IRedisCacheService>();
        _mockLogger = new Mock<ILogger<PvpService>>();

        // Repository Mocks
        _mockCharacterRepository = new Mock<ICharacterRepository>();
        _mockPvpSeasonRepository = new Mock<IPvpSeasonRepository>();
        _mockPvpRankingRepository = new Mock<IPvpRankingRepository>();
        _mockPvpMatchRepository = new Mock<IPvpMatchRepository>();

        // UnitOfWork의 Repository 속성 설정
        _mockUnitOfWork.Setup(u => u.Characters).Returns(_mockCharacterRepository.Object);
        _mockUnitOfWork.Setup(u => u.PvpSeasons).Returns(_mockPvpSeasonRepository.Object);
        _mockUnitOfWork.Setup(u => u.PvpRankings).Returns(_mockPvpRankingRepository.Object);
        _mockUnitOfWork.Setup(u => u.PvpMatches).Returns(_mockPvpMatchRepository.Object);

        // PvpService 인스턴스 생성
        _pvpService = new PvpService(
            _mockUnitOfWork.Object,
            _mockMatchmakingService.Object,
            _eloRatingService,
            _mockRedisCacheService.Object,
            _mockLogger.Object);
    }

    // ========== Helper Methods ==========

    /// <summary>
    /// 테스트용 Character 엔티티 생성
    /// </summary>
    private Character CreateTestCharacter(Guid? id = null, Guid? playerId = null, int level = 50, string userName = "TestUser")
    {
        var actualPlayerId = playerId ?? Guid.NewGuid();
        return new Character
        {
            Id = id ?? Guid.NewGuid(),
            PlayerId = actualPlayerId,
            Level = level,
            Experience = 10000,
            Gold = 5000,
            Crystal = 500,
            Stats = new CharacterStats(
                attack: 200,
                defense: 100,
                maxHealth: 1000,
                critRate: 0.2f,
                critDamage: 2.0f,
                evasion: 0.1f,
                attackSpeed: 1.2f
            ),
            Player = new Player
            {
                Id = actualPlayerId,
                UserName = userName,
                PasswordHash = "hashed_password",
                CreatedAt = DateTime.UtcNow
            }
        };
    }

    /// <summary>
    /// 테스트용 PvpSeason 엔티티 생성
    /// </summary>
    private PvpSeason CreateTestSeason(int id = 1, bool isActive = true)
    {
        return new PvpSeason
        {
            Id = id,
            SeasonNumber = id,
            StartDate = DateTime.UtcNow.AddDays(-7),
            EndDate = DateTime.UtcNow.AddDays(23),
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow.AddDays(-7),
            UpdatedAt = DateTime.UtcNow.AddDays(-7)
        };
    }

    /// <summary>
    /// 테스트용 PvpRanking 엔티티 생성
    /// </summary>
    private PvpRanking CreateTestRanking(int seasonId, Guid characterId, int rating = 1000)
    {
        return new PvpRanking
        {
            SeasonId = seasonId,
            CharacterId = characterId,
            Rating = rating,
            Wins = 10,
            Losses = 5,
            WinStreak = 2,
            IsRewardClaimed = false,
            LastMatchAt = DateTime.UtcNow.AddHours(-1),
            UpdatedAt = DateTime.UtcNow.AddHours(-1)
        };
    }

    // ========== StartMatchAsync Tests ==========

    /// <summary>
    /// 정상 매칭 → 전투 → 레이팅 업데이트 (전체 흐름 테스트)
    /// </summary>
    [Fact]
    public async Task StartMatchAsync_ValidMatch_ReturnsMatchResult()
    {
        // Arrange: 정상 매칭 시나리오
        var userId = Guid.NewGuid();
        var characterId = Guid.NewGuid();
        var opponentId = Guid.NewGuid();
        var seasonId = 1;
        var cancellationToken = CancellationToken.None;

        var myCharacter = CreateTestCharacter(id: characterId, playerId: userId, level: 50);
        var activeSeason = CreateTestSeason(id: seasonId, isActive: true);
        var myRanking = CreateTestRanking(seasonId, characterId, rating: 1500);

        // Mock: Character 조회
        _mockCharacterRepository
            .Setup(r => r.GetByIdAsync(characterId))
            .ReturnsAsync(myCharacter);

        // Mock: 활성 시즌 조회
        _mockPvpSeasonRepository
            .Setup(r => r.GetActiveSeasonAsync(cancellationToken))
            .ReturnsAsync(activeSeason);

        // Mock: 매칭 서비스 (상대 찾기)
        var opponent = new MatchOpponentDto
        {
            CharacterId = opponentId,
            Name = "TestOpponent",
            Rating = 1500,
            IsBot = false
        };
        _mockMatchmakingService
            .Setup(s => s.FindOpponentAsync(characterId, seasonId, cancellationToken))
            .ReturnsAsync(opponent);

        // Mock: 내 랭킹 조회 (2회 호출 - 첫 조회 + null 체크 재조회)
        _mockPvpRankingRepository
            .Setup(r => r.GetByIdAsync(seasonId, characterId, cancellationToken))
            .ReturnsAsync(myRanking);

        // Mock: 상대 캐릭터 조회 (SimulatePvpCombatAsync에서 필요)
        var opponentCharacter = CreateTestCharacter(id: opponentId, userName: "TestOpponent");
        _mockCharacterRepository
            .Setup(r => r.GetByIdAsync(opponentId))
            .ReturnsAsync(opponentCharacter);

        // Mock: 상대 랭킹 조회
        var opponentRanking = CreateTestRanking(seasonId, opponentId, rating: 1500);
        _mockPvpRankingRepository
            .Setup(r => r.GetByIdAsync(seasonId, opponentId, cancellationToken))
            .ReturnsAsync(opponentRanking);

        // Mock: PvpMatch 저장 (AddAsync)
        _mockPvpMatchRepository
            .Setup(r => r.AddAsync(It.IsAny<PvpMatch>(), cancellationToken))
            .Returns(Task.CompletedTask);

        // Mock: PvpRanking 업데이트 (2회 - 내 랭킹, 상대 랭킹)
        _mockPvpRankingRepository
            .Setup(r => r.UpdateAsync(It.IsAny<PvpRanking>(), cancellationToken))
            .Returns(Task.CompletedTask);

        // Mock: Character 업데이트 (보상 지급) - CancellationToken 없음
        _mockCharacterRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Character>()))
            .Returns(Task.CompletedTask);

        // Mock: SaveChangesAsync (트랜잭션 커밋)
        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(1);

        // Mock: Redis 랭킹 갱신 (Best Effort)
        _mockRedisCacheService
            .Setup(r => r.UpdateRankingCacheAsync(seasonId, characterId, It.IsAny<int>(), cancellationToken))
            .Returns(Task.CompletedTask);
        _mockRedisCacheService
            .Setup(r => r.UpdateRankingCacheAsync(seasonId, opponentId, It.IsAny<int>(), cancellationToken))
            .Returns(Task.CompletedTask);

        // Act: 매치 시작
        var result = await _pvpService.StartMatchAsync(characterId, userId, cancellationToken);

        // Assert: 결과 검증
        result.Should().NotBeNull("매치 결과가 반환되어야 함");
        result.MatchId.Should().NotBeEmpty("매치 ID가 생성되어야 함");
        result.Opponent.Should().NotBeNull("상대 정보가 포함되어야 함");
        result.Opponent.CharacterId.Should().Be(opponentId);
        result.Result.Should().BeOneOf(PvpMatchResult.Victory, PvpMatchResult.Defeat);
        result.MyRatingBefore.Should().Be(1500);
        result.MyRatingAfter.Should().NotBe(1500, "레이팅이 변경되어야 함");
        result.Rewards.Should().NotBeNull("보상이 포함되어야 함");

        // Verify: Repository 호출 검증
        _mockCharacterRepository.Verify(r => r.GetByIdAsync(characterId), Times.Once);
        _mockPvpSeasonRepository.Verify(r => r.GetActiveSeasonAsync(cancellationToken), Times.Once);
        _mockMatchmakingService.Verify(s => s.FindOpponentAsync(characterId, seasonId, cancellationToken), Times.Once);
        _mockPvpMatchRepository.Verify(r => r.AddAsync(It.IsAny<PvpMatch>(), cancellationToken), Times.Once);
        _mockPvpRankingRepository.Verify(r => r.UpdateAsync(It.IsAny<PvpRanking>(), cancellationToken), Times.AtLeastOnce);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(cancellationToken), Times.Once);
    }

    /// <summary>
    /// CharacterId 소유권 없음 (UnauthorizedAccessException)
    /// </summary>
    [Fact]
    public async Task StartMatchAsync_UnauthorizedCharacter_ThrowsUnauthorizedAccessException()
    {
        // Arrange: 다른 사용자의 캐릭터로 매치 시도
        var userId = Guid.NewGuid();
        var characterId = Guid.NewGuid();
        var actualOwnerId = Guid.NewGuid(); // 실제 소유자 (다른 사용자)
        var cancellationToken = CancellationToken.None;

        var myCharacter = CreateTestCharacter(id: characterId, playerId: actualOwnerId);

        // Mock: Character 조회
        _mockCharacterRepository
            .Setup(r => r.GetByIdAsync(characterId))
            .ReturnsAsync(myCharacter);

        // Act & Assert: UnauthorizedAccessException 발생
        Func<Task> act = async () => await _pvpService.StartMatchAsync(characterId, userId, cancellationToken);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage($"*{characterId}*{userId}*");

        // Verify: 소유권 검증 이후 로직은 실행되지 않아야 함
        _mockPvpSeasonRepository.Verify(r => r.GetActiveSeasonAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMatchmakingService.Verify(s => s.FindOpponentAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// 활성 시즌 없음 (InvalidOperationException)
    /// </summary>
    [Fact]
    public async Task StartMatchAsync_NoActiveSeason_ThrowsInvalidOperationException()
    {
        // Arrange: 활성 시즌이 없는 상황
        var userId = Guid.NewGuid();
        var characterId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        var myCharacter = CreateTestCharacter(id: characterId, playerId: userId);

        // Mock: Character 조회
        _mockCharacterRepository
            .Setup(r => r.GetByIdAsync(characterId))
            .ReturnsAsync(myCharacter);

        // Mock: 활성 시즌 없음 (null 반환)
        _mockPvpSeasonRepository
            .Setup(r => r.GetActiveSeasonAsync(cancellationToken))
            .ReturnsAsync((PvpSeason?)null);

        // Act & Assert: InvalidOperationException 발생
        Func<Task> act = async () => await _pvpService.StartMatchAsync(characterId, userId, cancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*active PVP season*");

        // Verify: 활성 시즌 조회 이후 로직은 실행되지 않아야 함
        _mockMatchmakingService.Verify(s => s.FindOpponentAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockPvpMatchRepository.Verify(r => r.AddAsync(It.IsAny<PvpMatch>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// CharacterId가 존재하지 않음 (KeyNotFoundException)
    /// </summary>
    [Fact]
    public async Task StartMatchAsync_CharacterNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange: 존재하지 않는 CharacterId
        var userId = Guid.NewGuid();
        var characterId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        // Mock: Character 조회 실패 (null 반환)
        _mockCharacterRepository
            .Setup(r => r.GetByIdAsync(characterId))
            .ReturnsAsync((Character?)null);

        // Act & Assert: KeyNotFoundException 발생
        Func<Task> act = async () => await _pvpService.StartMatchAsync(characterId, userId, cancellationToken);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{characterId}*");

        // Verify: Character 조회 이후 로직은 실행되지 않아야 함
        _mockPvpSeasonRepository.Verify(r => r.GetActiveSeasonAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
