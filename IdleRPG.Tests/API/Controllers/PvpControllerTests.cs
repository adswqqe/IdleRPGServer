using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using IdleRPG.Application.DTOs.Pvp;
using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Enums;
using IdleRPG.Infrastructure.Data;
using IdleRPG.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IdleRPG.Tests.API.Controllers;

/// <summary>
/// PvpController의 통합 테스트를 수행합니다.
/// WebApplicationFactory를 사용하여 실제 HTTP 요청 → Controller → Service → Repository → Database 흐름을 검증합니다.
/// </summary>
/// <remarks>
/// 🎓 학습 목적: 통합 테스트의 기본 구조와 WebApplicationFactory 사용법 이해
/// ⚠️ 인증 테스트는 JWT 구현이 복잡하므로 Anonymous(Public) 엔드포인트 위주로 작성
/// ⚠️ 실제 프로젝트에서는 JWT 토큰 생성 Helper를 만들어 인증 테스트도 추가 권장
/// </remarks>
public class PvpControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly Guid _character1Id = Guid.Parse("10000000-0000-0000-0000-000000000001");
    private readonly Guid _character2Id = Guid.Parse("10000000-0000-0000-0000-000000000002");

    public PvpControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    #region GET /api/pvp/seasons/current (Public Endpoint)

    [Fact]
    public async Task GetCurrentSeason_ShouldReturn200OK_WithSeasonInfo()
    {
        // Arrange & Act (Public 엔드포인트, 인증 불필요)
        var response = await _client.GetAsync("/api/pvp/seasons/current");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PvpSeasonDto>();
        result.Should().NotBeNull();
        result!.SeasonId.Should().Be(1);
        result.SeasonNumber.Should().Be(1);
        result.IsActive.Should().BeTrue();
        result.DaysRemaining.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetCurrentSeason_ShouldReturn404NotFound_WhenNoActiveSeasonExists()
    {
        // Arrange: 활성 시즌 제거
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<GameDBContext>();
            var season = await dbContext.Set<PvpSeason>().FirstAsync();
            season.IsActive = false;
            await dbContext.SaveChangesAsync();
        }

        // Act
        var response = await _client.GetAsync("/api/pvp/seasons/current");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Cleanup: 시즌 다시 활성화 (다른 테스트에 영향 최소화)
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<GameDBContext>();
            var season = await dbContext.Set<PvpSeason>().FirstAsync();
            season.IsActive = true;
            await dbContext.SaveChangesAsync();
        }
    }

    #endregion

    #region GET /api/pvp/rankings (Public Endpoint - Top N조회만)

    [Fact]
    public async Task GetRankings_ShouldReturn200OK_WhenGettingTopRankings()
    {
        // Arrange: Top 100 조회
        var url = "/api/pvp/rankings?top=100";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<PvpRankingDto>>();
        result.Should().NotBeNull();
        result!.Should().HaveCountGreaterThanOrEqualTo(2); // Seed 데이터에 2명

        // 레이팅 내림차순 정렬 확인
        var ratings = result.Select(r => r.Rating).ToList();
        ratings.Should().BeInDescendingOrder();

        // Rank 1부터 시작 확인
        result.First().Rank.Should().Be(1);
    }

    [Fact]
    public async Task GetRankings_ShouldReturn200OK_WhenFilteredByTier()
    {
        // Arrange: Silver 티어 조회 (Rating 1000-1500 = Silver)
        var url = "/api/pvp/rankings?tier=Silver&page=1&pageSize=10";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<PvpRankingDto>>();
        result.Should().NotBeNull();

        // 모든 결과가 Silver 티어인지 확인 (PvpRankingDto.Tier는 string 타입)
        result!.Should().OnlyContain(r => r.Tier == "Silver");
    }

    #endregion

    #region GET /api/pvp/matches/history (인증 필요 - TODO(human) 구현)

    /// <summary>
    /// 🎓 TODO(human): JWT 토큰 생성 Helper를 작성하여 인증 테스트를 완성하세요.
    /// </summary>
    /// <remarks>
    /// 현재 프로젝트의 JWT 설정:
    /// - Program.cs의 JwtOptions (Issuer, Audience, SigningKey)
    /// - JwtSecurityTokenHandler를 사용하여 테스트용 토큰 생성
    /// - Authorization 헤더에 "Bearer {token}" 추가
    ///
    /// 참고 구조:
    /// 1. JwtTokenHelper.GenerateTestToken(userId, claims) 생성
    /// 2. _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token)
    /// 3. 인증이 필요한 엔드포인트 테스트 (POST /api/pvp/matches, GET /api/pvp/matches/history 등)
    /// </remarks>
    [Fact(Skip = "JWT 인증 구현 필요 (TODO(human))")]
    public async Task GetMatchHistory_ShouldReturn200OK_WithPaginatedHistory()
    {
        // TODO: JWT 토큰 생성 후 Authorization 헤더 추가
        // AddAuthorizationHeader(_player1Id);

        // var response = await _client.GetAsync($"/api/pvp/matches/history?characterId={_character1Id}&page=1&pageSize=3");
        // response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region POST /api/pvp/matches (인증 필요 - TODO(human) 구현)

    [Fact(Skip = "JWT 인증 구현 필요 (TODO(human))")]
    public async Task StartMatch_ShouldReturn201Created_WhenMatchIsSuccessful()
    {
        // TODO: JWT 토큰 생성 후 Authorization 헤더 추가
        // var request = new PvpMatchRequestDto { CharacterId = _character1Id };
        // var response = await _client.PostAsJsonAsync("/api/pvp/matches", request);
        // response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// 테스트용 PvpMatch 데이터를 생성합니다 (Helper 예시)
    /// </summary>
    private async Task CreateTestMatchesAsync(Guid characterId, int count)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GameDBContext>();

        for (int i = 0; i < count; i++)
        {
            var match = new PvpMatch
            {
                Id = Guid.NewGuid(),
                SeasonId = 1,
                AttackerId = characterId,
                DefenderId = _character2Id,
                WinnerId = i % 2 == 0 ? characterId : _character2Id, // 승/패 번갈아가며
                AttackerRatingBefore = 1200,
                AttackerRatingAfter = 1200 + (i % 2 == 0 ? 16 : -16),
                DefenderRatingBefore = 1300,
                DefenderRatingAfter = 1300 + (i % 2 == 0 ? -16 : 16),
                CreatedAt = DateTime.UtcNow.AddHours(-i) // 시간 순서대로
            };

            dbContext.Set<PvpMatch>().Add(match);
        }

        await dbContext.SaveChangesAsync();
    }

    #endregion
}
