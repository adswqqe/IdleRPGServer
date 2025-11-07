using IdleRPG.Application.DTOs.Pvp;
using IdleRPG.Application.Interfaces;
using IdleRPG.Application.Services;
using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IdleRPG.Infrastructure.Services;

/// <summary>
/// PVP 매칭 서비스 구현체
/// </summary>
/// <remarks>
/// - 매칭 범위: ±200 레이팅
/// - 매칭 타임아웃: 동기 처리 (즉시 NPC 봇 생성, MVP 단순화)
/// - 초기 레이팅: 1000 (첫 매칭 시 자동 생성)
/// </remarks>
public class PvpMatchmakingService : IPvpMatchmakingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRandomProvider _randomProvider;
    private readonly ILogger<PvpMatchmakingService> _logger;

    private const int InitialRating = 1000; // 초기 레이팅
    private const int MatchingRange = 200;  // ±200 레이팅
    private const int MaxCandidates = 100;  // 최대 후보 수
    private const int BotRatingVariance = 100; // NPC 봇 레이팅 변동폭 ±100

    public PvpMatchmakingService(
        IUnitOfWork unitOfWork,
        IRandomProvider randomProvider,
        ILogger<PvpMatchmakingService> logger)
    {
        _unitOfWork = unitOfWork;
        _randomProvider = randomProvider;
        _logger = logger;
    }

    /// <summary>
    /// 상대방을 찾아 매칭합니다 (±200 레이팅 범위).
    /// </summary>
    public async Task<MatchOpponentDto> FindOpponentAsync(Guid characterId, int seasonId, CancellationToken cancellationToken = default)
    {
        // 1. 캐릭터 검증
        var character = await _unitOfWork.Characters.GetByIdAsync(characterId);
        if (character == null)
        {
            _logger.LogWarning("매칭 실패: 캐릭터를 찾을 수 없습니다. CharacterId={CharacterId}", characterId);
            throw new KeyNotFoundException($"캐릭터를 찾을 수 없습니다. (ID: {characterId})");
        }

        // 2. 내 레이팅 조회 (없으면 초기 레이팅 생성)
        var myRanking = await _unitOfWork.PvpRankings.GetByIdAsync(seasonId, characterId, cancellationToken);

        if (myRanking == null)
        {
            // 첫 매칭: 초기 레이팅 1000으로 PvpRanking 생성
            myRanking = new PvpRanking
            {
                SeasonId = seasonId,
                CharacterId = characterId,
                Rating = InitialRating,
                Wins = 0,
                Losses = 0,
                WinStreak = 0,
                IsRewardClaimed = false,
                LastMatchAt = null,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.PvpRankings.AddAsync(myRanking, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("첫 매칭: 초기 레이팅 생성. CharacterId={CharacterId}, Rating={Rating}",
                characterId, InitialRating);
        }

        int myRating = myRanking.Rating;

        // 3. ±200 레이팅 범위 내 후보 조회 (최대 100명, 자기 자신 제외)
        var candidates = await _unitOfWork.PvpRankings.GetRankingsAroundAsync(
            seasonId,
            myRating,
            MatchingRange,
            cancellationToken);

        // 자기 자신 제외
        candidates = candidates.Where(r => r.CharacterId != characterId).ToList();

        // 최대 100명으로 제한
        if (candidates.Count > MaxCandidates)
        {
            candidates = candidates.Take(MaxCandidates).ToList();
        }

        _logger.LogDebug("매칭 후보 조회: MyRating={MyRating}, Candidates={Count}",
            myRating, candidates.Count);

        // 4. 후보가 있으면 랜덤 선택, 없으면 NPC 봇 생성
        if (candidates.Count > 0)
        {
            // 랜덤 선택
            int randomIndex = _randomProvider.Next(candidates.Count);
            var selectedRanking = candidates[randomIndex];

            // 상대방 캐릭터 정보 조회
            var opponentCharacter = await _unitOfWork.Characters.GetByIdAsync(selectedRanking.CharacterId);
            if (opponentCharacter == null)
            {
                // 이론적으로 발생하지 않지만, 안전장치
                _logger.LogWarning("매칭 후보의 캐릭터 정보를 찾을 수 없습니다. CharacterId={CharacterId}. NPC 봇으로 대체합니다.",
                    selectedRanking.CharacterId);
                return CreateNpcBot(myRating);
            }

            _logger.LogInformation("매칭 성공: 실제 플레이어. Opponent={OpponentName}, Rating={Rating}",
                opponentCharacter.Player.UserName, selectedRanking.Rating);

            return new MatchOpponentDto
            {
                CharacterId = opponentCharacter.Id,
                Name = opponentCharacter.Player.UserName,
                Rating = selectedRanking.Rating,
                IsBot = false
            };
        }
        else
        {
            // NPC 봇 생성
            var botOpponent = CreateNpcBot(myRating);

            _logger.LogInformation("매칭 타임아웃: NPC 봇 생성. BotName={BotName}, Rating={Rating}",
                botOpponent.Name, botOpponent.Rating);

            return botOpponent;
        }
    }

    /// <summary>
    /// NPC 봇 상대를 생성합니다.
    /// </summary>
    /// <param name="myRating">내 레이팅</param>
    /// <returns>NPC 봇 DTO</returns>
    private MatchOpponentDto CreateNpcBot(int myRating)
    {
        // NPC 봇 레이팅: 내 레이팅 ± Random(-100, 100)
        int botRatingOffset = _randomProvider.Next(-BotRatingVariance, BotRatingVariance + 1);
        int botRating = Math.Max(0, myRating + botRatingOffset); // 최소 0

        // NPC 봇 이름: "Bot_1234" 형식
        int botNumber = _randomProvider.Next(1000, 10000);
        string botName = $"Bot_{botNumber}";

        return new MatchOpponentDto
        {
            CharacterId = Guid.Empty, // NPC 봇 식별용
            Name = botName,
            Rating = botRating,
            IsBot = true
        };
    }
}
