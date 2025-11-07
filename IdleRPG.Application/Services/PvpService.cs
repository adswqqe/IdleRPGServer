using IdleRPG.Application.DTOs.Pvp;
using IdleRPG.Application.Interfaces;
using IdleRPG.Domain.Enums;
using IdleRPG.Domain.Services;
using Microsoft.Extensions.Logging;
using CharacterEntity = IdleRPG.Domain.Entities.Character;
using PvpMatch = IdleRPG.Domain.Entities.PvpMatch;

namespace IdleRPG.Application.Services;

/// <summary>
/// PVP 매치 진행 서비스 구현체
/// </summary>
public class PvpService : IPvpService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPvpMatchmakingService _pvpMatchmakingService;
    private readonly EloRatingService _eloRatingService;
    private readonly IRedisCacheService _redisCacheService;
    private readonly ILogger<PvpService> _logger;

    public PvpService(
        IUnitOfWork unitOfWork,
        IPvpMatchmakingService pvpMatchmakingService,
        EloRatingService eloRatingService,
        IRedisCacheService redisCacheService,
        ILogger<PvpService> logger)
    {
        _unitOfWork = unitOfWork;
        _pvpMatchmakingService = pvpMatchmakingService;
        _eloRatingService = eloRatingService;
        _redisCacheService = redisCacheService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<PvpMatchResponseDto> StartMatchAsync(
        Guid characterId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("StartMatchAsync started: CharacterId={CharacterId}, UserId={UserId}",
            characterId, userId);

        // 1. CharacterId 소유권 검증
        var myCharacter = await _unitOfWork.Characters.GetByIdAsync(characterId);
        if (myCharacter == null)
        {
            _logger.LogWarning("Character not found: CharacterId={CharacterId}", characterId);
            throw new KeyNotFoundException($"Character {characterId} not found.");
        }

        if (myCharacter.PlayerId != userId)
        {
            _logger.LogWarning("Unauthorized access: CharacterId={CharacterId}, UserId={UserId}, ActualPlayerId={PlayerId}",
                characterId, userId, myCharacter.PlayerId);
            throw new UnauthorizedAccessException($"Character {characterId} does not belong to user {userId}.");
        }

        // 2. 현재 활성 시즌 조회
        var activeSeason = await _unitOfWork.PvpSeasons.GetActiveSeasonAsync(cancellationToken);
        if (activeSeason == null)
        {
            _logger.LogWarning("No active PVP season found");
            throw new InvalidOperationException("No active PVP season. Please wait for the next season to start.");
        }

        // 3. 매칭 실행
        var opponent = await _pvpMatchmakingService.FindOpponentAsync(characterId, activeSeason.Id, cancellationToken);

        _logger.LogInformation("Matched: CharacterId={CharacterId}, OpponentId={OpponentId}, OpponentIsBot={IsBot}",
            characterId, opponent.CharacterId, opponent.IsBot);

        // 4. 전투 시뮬레이션 (Character vs Character)
        var (isVictory, combatLog) = await SimulatePvpCombatAsync(myCharacter, opponent, cancellationToken);

        var result = isVictory ? PvpMatchResult.Victory : PvpMatchResult.Defeat;
        var winnerId = isVictory ? myCharacter.Id : opponent.CharacterId;

        _logger.LogInformation("Combat finished: CharacterId={CharacterId}, Result={Result}",
            characterId, result);

        // 5. 레이팅 조회 (내 레이팅, 상대 레이팅)
        var myRanking = await _unitOfWork.PvpRankings.GetByIdAsync(activeSeason.Id, characterId, cancellationToken);
        if (myRanking == null)
        {
            // 첫 매치: 초기 레이팅 1000 생성 (이미 PvpMatchmakingService에서 생성됨)
            myRanking = await _unitOfWork.PvpRankings.GetByIdAsync(activeSeason.Id, characterId, cancellationToken);
            if (myRanking == null)
            {
                _logger.LogError("Failed to create initial ranking: CharacterId={CharacterId}", characterId);
                throw new InvalidOperationException("Failed to initialize PVP ranking.");
            }
        }

        int myRatingBefore = myRanking.Rating;
        int opponentRatingBefore = opponent.Rating;

        // 6. 레이팅 계산 (ELO)
        int myRatingAfter, opponentRatingAfter;
        if (isVictory)
        {
            (myRatingAfter, opponentRatingAfter) = _eloRatingService.CalculateNewRatings(myRatingBefore, opponentRatingBefore);
        }
        else
        {
            (opponentRatingAfter, myRatingAfter) = _eloRatingService.CalculateNewRatings(opponentRatingBefore, myRatingBefore);
        }

        _logger.LogInformation(
            "Rating calculated: MyRating={MyBefore}→{MyAfter}, OpponentRating={OpBefore}→{OpAfter}",
            myRatingBefore, myRatingAfter, opponentRatingBefore, opponentRatingAfter);

        // 7. 트랜잭션 시작 (PvpMatch 생성 + PvpRanking 업데이트 + Character 보상 지급)
        var matchId = Guid.NewGuid();
        var pvpMatch = new Domain.Entities.PvpMatch
        {
            Id = matchId,
            SeasonId = activeSeason.Id,
            AttackerId = myCharacter.Id,
            DefenderId = opponent.IsBot ? Guid.Empty : opponent.CharacterId, // NPC 봇은 Guid.Empty
            WinnerId = winnerId,
            AttackerRatingBefore = myRatingBefore,
            AttackerRatingAfter = myRatingAfter,
            DefenderRatingBefore = opponentRatingBefore,
            DefenderRatingAfter = opponentRatingAfter,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.PvpMatches.AddAsync(pvpMatch, cancellationToken);

        // PvpRanking 업데이트 (내 랭킹)
        myRanking.Rating = myRatingAfter;
        if (isVictory)
        {
            myRanking.Wins++;
            myRanking.WinStreak++;
        }
        else
        {
            myRanking.Losses++;
            myRanking.WinStreak = 0;
        }
        myRanking.LastMatchAt = DateTime.UtcNow;
        myRanking.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.PvpRankings.UpdateAsync(myRanking, cancellationToken);

        // PvpRanking 업데이트 (상대 랭킹, IsBot = false인 경우만)
        if (!opponent.IsBot)
        {
            var opponentRanking = await _unitOfWork.PvpRankings.GetByIdAsync(activeSeason.Id, opponent.CharacterId, cancellationToken);
            if (opponentRanking != null)
            {
                opponentRanking.Rating = opponentRatingAfter;
                if (!isVictory)
                {
                    opponentRanking.Wins++;
                    opponentRanking.WinStreak++;
                }
                else
                {
                    opponentRanking.Losses++;
                    opponentRanking.WinStreak = 0;
                }
                opponentRanking.LastMatchAt = DateTime.UtcNow;
                opponentRanking.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.PvpRankings.UpdateAsync(opponentRanking, cancellationToken);
            }
        }

        // Character 보상 지급
        var rewards = CalculateRewards(isVictory);
        ApplyRewardsToCharacter(myCharacter, rewards);

        // 트랜잭션 커밋
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "PVP match saved: MatchId={MatchId}, Winner={WinnerId}, Rewards={Rewards}",
            matchId, winnerId, string.Join(", ", rewards.Select(r => $"{r.Key}:{r.Value}")));

        // 8. Redis 랭킹 갱신 (트랜잭션 외부, Best Effort)
        try
        {
            await _redisCacheService.UpdateRankingCacheAsync(activeSeason.Id, myCharacter.Id, myRatingAfter, cancellationToken);
            if (!opponent.IsBot)
            {
                await _redisCacheService.UpdateRankingCacheAsync(activeSeason.Id, opponent.CharacterId, opponentRatingAfter, cancellationToken);
            }
            _logger.LogDebug("Redis ranking cache updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to update Redis ranking cache (Best Effort, continuing)");
            // Redis 실패해도 계속 진행 (PostgreSQL이 Source of Truth)
        }

        // 9. 응답 DTO 생성
        return new PvpMatchResponseDto
        {
            MatchId = matchId,
            Opponent = opponent,
            Result = result,
            MyRatingBefore = myRatingBefore,
            MyRatingAfter = myRatingAfter,
            MyRatingChange = myRatingAfter - myRatingBefore,
            OpponentRatingChange = opponentRatingAfter - opponentRatingBefore,
            Rewards = rewards,
            CombatLog = combatLog
        };
    }

    /// <summary>
    /// PVP 전투 시뮬레이션 (Character vs Character)
    /// </summary>
    /// <remarks>
    /// 간단한 전투 로직: 공격력 + 랜덤 요소 비교
    /// Phase 2에서 CombatService와 통합 예정
    /// </remarks>
    private async Task<(bool isVictory, List<string> combatLog)> SimulatePvpCombatAsync(
        CharacterEntity myCharacter,
        MatchOpponentDto opponent,
        CancellationToken cancellationToken)
    {
        var combatLog = new List<string>();

        // 내 전투력 계산
        int myPower = (int)(myCharacter.Stats.Attack + myCharacter.Stats.Defense + myCharacter.Stats.MaxHealth / 10);
        combatLog.Add($"[{myCharacter.Player.UserName}] Combat Power: {myPower}");

        // 상대 전투력 계산 (NPC 봇은 레이팅 기반 스탯 생성)
        int opponentPower;
        if (opponent.IsBot)
        {
            // NPC 봇: 레이팅 기반 스탯 (간단한 공식)
            opponentPower = opponent.Rating; // 레이팅 = 전투력
            combatLog.Add($"[{opponent.Name} (Bot)] Combat Power: {opponentPower}");
        }
        else
        {
            // 실제 플레이어: DB에서 스탯 조회
            CharacterEntity? opponentCharacter = await _unitOfWork.Characters.GetByIdAsync(opponent.CharacterId);
            if (opponentCharacter == null)
            {
                _logger.LogWarning("Opponent character not found: CharacterId={CharacterId}", opponent.CharacterId);
                throw new KeyNotFoundException($"Opponent character {opponent.CharacterId} not found.");
            }
            opponentPower = (int)(opponentCharacter.Stats.Attack + opponentCharacter.Stats.Defense + opponentCharacter.Stats.MaxHealth / 10);
            combatLog.Add($"[{opponentCharacter.Player.UserName}] Combat Power: {opponentPower}");
        }

        // 랜덤 요소 추가 (±10%)
        var random = new Random();
        int myFinalPower = myPower + random.Next(-myPower / 10, myPower / 10);
        int opponentFinalPower = opponentPower + random.Next(-opponentPower / 10, opponentPower / 10);

        combatLog.Add($"[Combat] {myCharacter.Player.UserName} ({myFinalPower}) vs {opponent.Name} ({opponentFinalPower})");

        // 승패 결정
        bool isVictory = myFinalPower > opponentFinalPower;
        combatLog.Add($"[Result] {(isVictory ? myCharacter.Player.UserName : opponent.Name)} Wins!");

        _logger.LogDebug("PVP combat simulated: MyPower={MyPower}, OpponentPower={OpponentPower}, Victory={Victory}",
            myFinalPower, opponentFinalPower, isVictory);

        return (isVictory, combatLog);
    }

    /// <summary>
    /// 보상 계산 (승리/패배)
    /// </summary>
    /// <remarks>
    /// AI가 제공하는 게임 밸런스 값입니다. 학습자는 구조에 집중하세요.
    /// </remarks>
    private Dictionary<string, int> CalculateRewards(bool isVictory)
    {
        if (isVictory)
        {
            return new Dictionary<string, int>
            {
                { "Gold", 100 },
                { "Crystal", 10 },
                { "Experience", 50 }
            };
        }
        else
        {
            return new Dictionary<string, int>
            {
                { "Gold", 50 },
                { "Experience", 25 }
            };
        }
    }

    /// <summary>
    /// 보상을 캐릭터에 지급
    /// </summary>
    private void ApplyRewardsToCharacter(CharacterEntity character, Dictionary<string, int> rewards)
    {
        foreach (var (rewardName, amount) in rewards)
        {
            switch (rewardName)
            {
                case "Gold":
                    character.Gold += amount;
                    break;
                case "Crystal":
                    character.Crystal += amount;
                    break;
                case "Experience":
                    character.Experience += amount;
                    // TODO: 레벨업 체크 (Phase 2)
                    break;
                default:
                    _logger.LogWarning("Unknown reward type: {RewardName}", rewardName);
                    break;
            }
        }

        _logger.LogDebug("Rewards applied: CharacterId={CharacterId}, Rewards={Rewards}",
            character.Id, string.Join(", ", rewards.Select(r => $"{r.Key}:{r.Value}")));
    }
}
