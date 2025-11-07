using IdleRPG.Application.DTOs.Pvp;
using IdleRPG.Application.Interfaces;
using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace IdleRPG.Application.Services;

/// <summary>
/// PVP 시즌 관리 서비스 구현체
/// </summary>
public class PvpSeasonService : IPvpSeasonService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRedisCacheService _redisCacheService;
    private readonly ILogger<PvpSeasonService> _logger;

    public PvpSeasonService(
        IUnitOfWork unitOfWork,
        IRedisCacheService redisCacheService,
        ILogger<PvpSeasonService> logger)
    {
        _unitOfWork = unitOfWork;
        _redisCacheService = redisCacheService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<SeasonRewardDto> ClaimSeasonRewardAsync(
        int seasonId,
        Guid characterId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("ClaimSeasonRewardAsync started: SeasonId={SeasonId}, CharacterId={CharacterId}",
            seasonId, characterId);

        // 1. 시즌 검증 및 종료 확인
        var season = await _unitOfWork.PvpSeasons.GetByIdAsync(seasonId, cancellationToken);
        if (season == null)
        {
            _logger.LogWarning("Season not found: SeasonId={SeasonId}", seasonId);
            throw new KeyNotFoundException($"Season with ID {seasonId} not found.");
        }

        if (season.IsActive)
        {
            _logger.LogWarning("Season is still active: SeasonId={SeasonId}", seasonId);
            throw new InvalidOperationException($"Season {seasonId} is still active. Rewards can only be claimed after season ends.");
        }

        // 2. PvpRanking 조회
        var ranking = await _unitOfWork.PvpRankings.GetByIdAsync(seasonId, characterId, cancellationToken);
        if (ranking == null)
        {
            _logger.LogWarning("PvpRanking not found: SeasonId={SeasonId}, CharacterId={CharacterId}",
                seasonId, characterId);
            throw new KeyNotFoundException($"No ranking found for Character {characterId} in Season {seasonId}.");
        }

        // 3. 중복 수령 방지
        if (ranking.IsRewardClaimed)
        {
            _logger.LogWarning("Reward already claimed: SeasonId={SeasonId}, CharacterId={CharacterId}",
                seasonId, characterId);
            throw new InvalidOperationException($"Reward for Season {seasonId} has already been claimed.");
        }

        // 4. 티어별 보상 계산
        var rewards = CalculateTierRewards(ranking.Tier);

        // 5. 최종 순위 조회 (Redis 시도 → PostgreSQL Fallback)
        int finalRank = await GetFinalRankAsync(seasonId, characterId, cancellationToken);

        // 6. 트랜잭션: Character 보상 지급 + PvpRanking 업데이트
        var character = await _unitOfWork.Characters.GetByIdAsync(characterId, cancellationToken);
        if (character == null)
        {
            _logger.LogWarning("Character not found: CharacterId={CharacterId}", characterId);
            throw new KeyNotFoundException($"Character {characterId} not found.");
        }

        // 보상 지급
        foreach (var (rewardName, amount) in rewards)
        {
            ApplyRewardToCharacter(character, rewardName, amount);
        }

        // 랭킹 수령 플래그 업데이트
        ranking.IsRewardClaimed = true;
        ranking.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.PvpRankings.UpdateAsync(ranking, cancellationToken);

        // 트랜잭션 커밋
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Season reward claimed successfully: SeasonId={SeasonId}, CharacterId={CharacterId}, Tier={Tier}, Rank={Rank}",
            seasonId, characterId, ranking.Tier, finalRank);

        // 7. 응답 DTO 생성
        return new SeasonRewardDto
        {
            SeasonNumber = season.SeasonNumber,
            Tier = ranking.Tier.ToString(),
            FinalRating = ranking.Rating,
            FinalRank = finalRank,
            Rewards = rewards,
            AlreadyClaimed = true
        };
    }

    /// <inheritdoc/>
    public async Task<PvpSeason> StartNewSeasonAsync(
        int seasonNumber,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("StartNewSeasonAsync started: SeasonNumber={SeasonNumber}, StartDate={StartDate}, EndDate={EndDate}",
            seasonNumber, startDate, endDate);

        // 1. 날짜 검증
        if (startDate >= endDate)
        {
            _logger.LogWarning("Invalid season dates: StartDate={StartDate}, EndDate={EndDate}", startDate, endDate);
            throw new ArgumentException("StartDate must be earlier than EndDate.");
        }

        // 2. 중복 시즌 번호 확인
        var existingSeason = await _unitOfWork.PvpSeasons.GetBySeasonNumberAsync(seasonNumber, cancellationToken);
        if (existingSeason != null)
        {
            _logger.LogWarning("Season number already exists: SeasonNumber={SeasonNumber}", seasonNumber);
            throw new InvalidOperationException($"Season {seasonNumber} already exists.");
        }

        // 3. 기존 활성 시즌 비활성화
        var activeSeason = await _unitOfWork.PvpSeasons.GetActiveSeasonAsync(cancellationToken);
        if (activeSeason != null)
        {
            activeSeason.IsActive = false;
            activeSeason.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.PvpSeasons.UpdateAsync(activeSeason, cancellationToken);

            _logger.LogInformation("Deactivated previous season: SeasonId={SeasonId}, SeasonNumber={SeasonNumber}",
                activeSeason.Id, activeSeason.SeasonNumber);

            // Redis 캐시 초기화 (기존 시즌)
            try
            {
                await _redisCacheService.ClearRankingCacheAsync(activeSeason.Id, cancellationToken);
                _logger.LogInformation("Cleared Redis cache for previous season: SeasonId={SeasonId}", activeSeason.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to clear Redis cache for previous season: SeasonId={SeasonId}", activeSeason.Id);
                // Best Effort: Redis 실패해도 계속 진행
            }
        }

        // 4. 새 시즌 생성
        var newSeason = new PvpSeason
        {
            SeasonNumber = seasonNumber,
            StartDate = startDate,
            EndDate = endDate,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.PvpSeasons.AddAsync(newSeason, cancellationToken);

        // 5. Soft Reset 적용 (새 레이팅 = (기존 레이팅 + 1000) / 2)
        // TODO(human): Soft Reset 로직 구현
        // 이전 시즌의 모든 PvpRanking을 조회하고, 각 플레이어의 새 시즌 PvpRanking을 생성하여
        // Soft Reset 공식을 적용하세요.
        //
        // 가이드:
        // - activeSeason이 null이 아닌 경우에만 Soft Reset 적용
        // - 이전 시즌 랭킹 조회: await _unitOfWork.PvpRankings.GetTopRankingsAsync(activeSeason.Id, int.MaxValue, cancellationToken)
        if (activeSeason != null)
        {
            var beSeason = await _unitOfWork.PvpRankings.GetTopRankingsAsync(activeSeason.Id, int.MaxValue, cancellationToken);

            foreach (var pvpRanking in beSeason)
            {
                var newRanking = new PvpRanking()
                {
                    SeasonId = newSeason.Id,
                    CharacterId = pvpRanking.CharacterId,
                    Rating = (pvpRanking.Rating + 1000) / 2,
                    Wins = 0,
                    Losses = 0,
                    WinStreak = 0,
                    IsRewardClaimed = false,
                    UpdatedAt = DateTime.UtcNow
                };
                await _unitOfWork.PvpRankings.AddAsync(new PvpRanking(), cancellationToken);
            }ㅠ
        } 
        // - 각 랭킹마다 새 PvpRanking 생성:
        //   - SeasonId = newSeason.Id
        //   - CharacterId = 기존 랭킹의 CharacterId
        //   - Rating = (기존 Rating + 1000) / 2
        //   - Wins, Losses, WinStreak = 0 (초기화)
        //   - IsRewardClaimed = false
        //   - UpdatedAt = DateTime.UtcNow
        // - 생성한 랭킹을 await _unitOfWork.PvpRankings.AddAsync()로 추가

        // 6. 트랜잭션 커밋
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("New season created successfully: SeasonId={SeasonId}, SeasonNumber={SeasonNumber}",
            newSeason.Id, newSeason.SeasonNumber);

        return newSeason;
    }

    /// <summary>
    /// 티어별 보상 계산
    /// </summary>
    /// <remarks>
    /// AI가 제공하는 게임 밸런스 값입니다. 학습자는 구조에 집중하세요.
    /// </remarks>
    private Dictionary<string, int> CalculateTierRewards(PvpTier tier)
    {
        return tier switch
        {
            PvpTier.Bronze => new Dictionary<string, int>
            {
                { "Crystal", 100 }
            },
            PvpTier.Silver => new Dictionary<string, int>
            {
                { "Crystal", 300 }
            },
            PvpTier.Gold => new Dictionary<string, int>
            {
                { "Crystal", 500 },
                { "LegendaryEquipmentBox", 1 }
            },
            PvpTier.Platinum => new Dictionary<string, int>
            {
                { "Crystal", 1000 },
                { "LegendaryEquipmentBox", 3 }
            },
            PvpTier.Diamond => new Dictionary<string, int>
            {
                { "Crystal", 2000 },
                { "MythicEquipmentBox", 1 }
            },
            _ => new Dictionary<string, int>()
        };
    }

    /// <summary>
    /// 보상을 캐릭터에 지급
    /// </summary>
    private void ApplyRewardToCharacter(Character character, string rewardName, int amount)
    {
        switch (rewardName)
        {
            case "Crystal":
                character.Crystal += amount;
                _logger.LogDebug("Applied Crystal: CharacterId={CharacterId}, Amount={Amount}", character.Id, amount);
                break;
            case "LegendaryEquipmentBox":
            case "MythicEquipmentBox":
                // TODO: 장비 상자 시스템 구현 시 처리
                // 현재는 로그만 기록
                _logger.LogInformation("Equipment box reward (not implemented yet): CharacterId={CharacterId}, Box={Box}, Amount={Amount}",
                    character.Id, rewardName, amount);
                break;
            default:
                _logger.LogWarning("Unknown reward type: {RewardName}", rewardName);
                break;
        }
    }

    /// <summary>
    /// 최종 순위 조회 (Redis → PostgreSQL Fallback)
    /// </summary>
    private async Task<int> GetFinalRankAsync(int seasonId, Guid characterId, CancellationToken cancellationToken)
    {
        // Redis 시도
        try
        {
            var rank = await _redisCacheService.GetMyRankAsync(seasonId, characterId, cancellationToken);
            if (rank.HasValue)
            {
                _logger.LogDebug("Final rank from Redis: SeasonId={SeasonId}, CharacterId={CharacterId}, Rank={Rank}",
                    seasonId, characterId, rank.Value);
                return rank.Value;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get rank from Redis: SeasonId={SeasonId}, CharacterId={CharacterId}",
                seasonId, characterId);
        }

        // PostgreSQL Fallback
        var allRankings = await _unitOfWork.PvpRankings.GetTopRankingsAsync(seasonId, int.MaxValue, cancellationToken);
        var myRank = allRankings.FindIndex(r => r.CharacterId == characterId) + 1; // 1-based

        if (myRank == 0)
        {
            _logger.LogWarning("Character not found in rankings: SeasonId={SeasonId}, CharacterId={CharacterId}",
                seasonId, characterId);
            return 0; // 순위 없음
        }

        _logger.LogDebug("Final rank from PostgreSQL: SeasonId={SeasonId}, CharacterId={CharacterId}, Rank={Rank}",
            seasonId, characterId, myRank);
        return myRank;
    }
}
