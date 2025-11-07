using IdleRPG.Application.DTOs.Pvp;
using IdleRPG.Application.Services;
using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdleRPG.API.Controllers;

/// <summary>
/// PVP 아레나 관련 API 컨트롤러
/// </summary>
[ApiController]
[Route("api/pvp")]
[Authorize]
public class PvpController : BaseController
{
    private readonly IPvpService _pvpService;
    private readonly IPvpMatchRepository _pvpMatchRepository;
    private readonly IRedisCacheService _redisCacheService;
    private readonly IPvpRankingRepository _pvpRankingRepository;
    private readonly IPvpSeasonRepository _pvpSeasonRepository;
    private readonly IPvpSeasonService _pvpSeasonService;
    private readonly ICharacterRepository _characterRepository;
    private readonly ILogger<PvpController> _logger;

    public PvpController(
        IPvpService pvpService,
        IPvpMatchRepository pvpMatchRepository,
        IRedisCacheService redisCacheService,
        IPvpRankingRepository pvpRankingRepository,
        IPvpSeasonRepository pvpSeasonRepository,
        IPvpSeasonService pvpSeasonService,
        ICharacterRepository characterRepository,
        ILogger<PvpController> logger)
    {
        _pvpService = pvpService;
        _pvpMatchRepository = pvpMatchRepository;
        _redisCacheService = redisCacheService;
        _pvpRankingRepository = pvpRankingRepository;
        _pvpSeasonRepository = pvpSeasonRepository;
        _pvpSeasonService = pvpSeasonService;
        _characterRepository = characterRepository;
        _logger = logger;
    }

    /// <summary>
    /// PVP 매치 시작
    /// </summary>
    /// <remarks>
    /// **동작 흐름:**
    /// 1. 현재 활성 시즌 조회
    /// 2. 매칭 알고리즘 실행 (±200 레이팅 범위, 없으면 NPC 봇)
    /// 3. 전투 시뮬레이션 (간단한 전투력 비교)
    /// 4. ELO 레이팅 계산 및 업데이트
    /// 5. 보상 지급 (Gold, Crystal, Experience)
    /// 6. Redis 랭킹 갱신
    ///
    /// **에러 코드:**
    /// - 400: CharacterId 소유권 없음
    /// - 404: 활성 시즌 없음
    /// - 401: 인증 토큰 없음
    /// </remarks>
    /// <param name="request">매치 시작 요청 (CharacterId)</param>
    /// <returns>매치 결과 (상대방 정보, 전투 결과, 레이팅 변화, 보상)</returns>
    /// <response code="201">매치 성공</response>
    /// <response code="400">CharacterId 소유권 없음</response>
    /// <response code="404">활성 시즌 없음</response>
    [HttpPost("matches")]
    [ProducesResponseType(typeof(PvpMatchResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> StartMatch([FromBody] PvpMatchRequestDto request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _pvpService.StartMatchAsync(request.CharacterId, userId, HttpContext.RequestAborted);

            _logger.LogInformation(
                "PVP Match started: CharacterId={CharacterId}, Result={Result}, RatingChange={RatingChange}",
                request.CharacterId, result.Result, result.MyRatingChange);

            return CreatedAtAction(
                nameof(GetMatchHistory),
                new { characterId = request.CharacterId },
                result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized PVP match attempt: CharacterId={CharacterId}", request.CharacterId);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid PVP match operation: CharacterId={CharacterId}", request.CharacterId);
            return NotFound(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "PVP match resource not found: CharacterId={CharacterId}", request.CharacterId);
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// PVP 매치 히스토리 조회
    /// </summary>
    /// <remarks>
    /// **기능:**
    /// - 특정 캐릭터의 PVP 전적 조회
    /// - 시즌별 필터링 가능 (seasonId null이면 전체 시즌)
    /// - 페이징 지원 (기본: page=1, pageSize=20)
    ///
    /// **응답:**
    /// - matches: 매치 목록 (최신순)
    /// - totalCount: 전체 매치 수
    /// - page: 현재 페이지
    /// - pageSize: 페이지 크기
    /// - totalPages: 전체 페이지 수
    /// </remarks>
    /// <param name="request">히스토리 조회 요청 (characterId, seasonId, page, pageSize)</param>
    /// <returns>매치 히스토리 목록 (페이징 포함)</returns>
    /// <response code="200">조회 성공</response>
    /// <response code="400">잘못된 요청 (CharacterId 형식 오류, PageSize 범위 초과)</response>
    [HttpGet("matches/history")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMatchHistory([FromQuery] GetMatchHistoryRequest request)
    {
        try
        {
            var (matches, totalCount) = await _pvpMatchRepository.GetMatchHistoryAsync(
                request.CharacterId,
                request.SeasonId,
                request.Page,
                request.PageSize,
                HttpContext.RequestAborted);

            // DTO 변환
            var historyDtos = matches.Select(m =>
            {
                var isAttacker = m.AttackerId == request.CharacterId;
                var opponent = isAttacker ? m.Defender : m.Attacker;
                var isVictory = m.WinnerId == request.CharacterId;

                return new PvpMatchHistoryDto
                {
                    MatchId = m.Id,
                    SeasonNumber = m.Season.SeasonNumber, // PvpSeason 대신 Season
                    OpponentCharacterId = opponent.Id,
                    OpponentName = opponent.Player.UserName, // Character.Name 대신 Player.UserName 사용
                    Result = isVictory ? Domain.Enums.PvpMatchResult.Victory : Domain.Enums.PvpMatchResult.Defeat,
                    MyRatingChange = isAttacker
                        ? (m.AttackerRatingAfter - m.AttackerRatingBefore)
                        : (m.DefenderRatingAfter - m.DefenderRatingBefore),
                    OpponentRatingChange = isAttacker
                        ? (m.DefenderRatingAfter - m.DefenderRatingBefore)
                        : (m.AttackerRatingAfter - m.AttackerRatingBefore),
                    CreatedAt = m.CreatedAt
                };
            }).ToList();

            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            return Ok(new
            {
                matches = historyDtos,
                totalCount,
                page = request.Page,
                pageSize = request.PageSize,
                totalPages
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving PVP match history: CharacterId={CharacterId}", request.CharacterId);
            return BadRequest(new { message = "매치 히스토리 조회 중 오류가 발생했습니다." });
        }
    }

    /// <summary>
    /// PVP 랭킹 조회
    /// </summary>
    /// <remarks>
    /// **조회 모드 (3가지 중 1개만 사용):**
    /// 1. **Top N 조회** (top 파라미터): 상위 N명 랭킹 조회
    ///    - Redis 우선 조회 (O(log N + count), 고속)
    ///    - Redis 실패 시 PostgreSQL Fallback (성능 저하 허용)
    ///
    /// 2. **내 주변 조회** (nearMe=true, range 파라미터): 내 순위 ±range 조회
    ///    - Redis 우선 조회 (O(log N + 2*range), 고속)
    ///    - Redis 실패 시 PostgreSQL Fallback (내 레이팅 기준 ±range)
    ///    - 인증 필수 (JWT 토큰 필요)
    ///
    /// 3. **티어별 조회** (tier 파라미터): 특정 티어 랭킹 조회 (페이징)
    ///    - PostgreSQL 직접 조회 (Redis는 티어 필터링 불가)
    ///    - Bronze, Silver, Gold, Platinum, Diamond
    ///
    /// **Redis vs PostgreSQL 전략:**
    /// - **Top N**: Redis 우선 (Sorted Set, 레이팅 DESC) → PostgreSQL Fallback
    /// - **내 주변**: Redis 우선 (ZREVRANK + ZREVRANGE) → PostgreSQL Fallback
    /// - **티어별**: PostgreSQL 직접 (Generated Column 활용, 복잡한 필터링)
    /// - **Fallback 정책**: Redis 장애 시 PostgreSQL로 대체 (Graceful Degradation, 성능 저하 허용)
    ///
    /// **에러 코드:**
    /// - 404: 시즌 없음
    /// - 401: nearMe 조회 시 인증 필요
    /// </remarks>
    /// <param name="request">랭킹 조회 요청 (seasonId, top, nearMe, range, tier, page, pageSize)</param>
    /// <returns>랭킹 목록 (PvpRankingDto)</returns>
    /// <response code="200">조회 성공</response>
    /// <response code="400">잘못된 요청</response>
    /// <response code="401">인증 필요 (nearMe 사용 시)</response>
    /// <response code="404">시즌 없음</response>
    [HttpGet("rankings")]
    [AllowAnonymous] // Top N, 티어별 조회는 Public
    [ProducesResponseType(typeof(List<PvpRankingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRankings([FromQuery] GetRankingsRequest request)
    {
        try
        {
            // 1. 시즌 ID 결정 (null이면 현재 활성 시즌)
            int seasonId;
            if (request.SeasonId.HasValue)
            {
                seasonId = request.SeasonId.Value;
            }
            else
            {
                var activeSeason = await _pvpSeasonRepository.GetActiveSeasonAsync(HttpContext.RequestAborted);
                if (activeSeason == null)
                {
                    return NotFound(new { message = "활성 시즌이 없습니다." });
                }
                seasonId = activeSeason.Id;
            }

            List<PvpRankingDto> rankingDtos;

            // 2. 조회 모드 분기
            if (request.Top.HasValue)
            {
                // 모드 1: Top N 조회 (Redis 우선 → PostgreSQL Fallback)
                rankingDtos = await GetTopNRankingsAsync(seasonId, request.Top.Value);
            }
            else if (request.NearMe == true)
            {
                // 모드 2: 내 주변 조회 (인증 필수)
                if (!User.Identity?.IsAuthenticated ?? true)
                {
                    return Unauthorized(new { message = "내 주변 랭킹 조회는 로그인이 필요합니다." });
                }

                var userId = GetCurrentUserId();
                var myCharacterId = await GetUserCharacterIdAsync(userId);
                if (myCharacterId == null)
                {
                    return NotFound(new { message = "캐릭터를 찾을 수 없습니다." });
                }

                int range = request.Range ?? 10; // 기본값 10
                rankingDtos = await GetRankingsAroundMeAsync(seasonId, myCharacterId.Value, range);
            }
            else if (request.Tier.HasValue)
            {
                // 모드 3: 티어별 조회 (PostgreSQL 직접)
                rankingDtos = await GetRankingsByTierAsync(seasonId, request.Tier.Value, request.Page, request.PageSize);
            }
            else
            {
                // 기본값: Top 100
                rankingDtos = await GetTopNRankingsAsync(seasonId, 100);
            }

            return Ok(rankingDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving PVP rankings");
            return BadRequest(new { message = "랭킹 조회 중 오류가 발생했습니다." });
        }
    }

    /// <summary>
    /// Top N 랭킹 조회 (Redis 우선 → PostgreSQL Fallback)
    /// </summary>
    private async Task<List<PvpRankingDto>> GetTopNRankingsAsync(int seasonId, int count)
    {
        // 1. Redis 시도
        var redisRankings = await _redisCacheService.GetTopRankingsAsync(seasonId, count, HttpContext.RequestAborted);

        if (redisRankings != null && redisRankings.Count > 0)
        {
            // Redis 성공: CharacterId로 PvpRanking 조회 (Character.Player.UserName 포함)
            _logger.LogInformation("Redis hit: Top {Count} rankings for season {SeasonId}", count, seasonId);

            var characterIds = redisRankings.Keys.ToList();

            // 벌크 조회 (N+1 쿼리 방지: 100개 조회 시 100번 쿼리 → 1번 쿼리)
            var rankings = await _pvpRankingRepository.GetByCharacterIdsAsync(seasonId, characterIds, HttpContext.RequestAborted);

            // DTO 변환 (Redis 순서 유지, Rank = 1, 2, 3...)
            return rankings
                .OrderByDescending(r => r.Rating)
                .Select((r, index) => CreateRankingDto(r, index + 1))
                .ToList();
        }

        // 2. PostgreSQL Fallback
        _logger.LogWarning("Redis miss: Falling back to PostgreSQL for Top {Count} rankings", count);

        // 3. 분산 락 획득 (동시 Warm-up 방지, 초기 TTL 10초)
        var lockKey = $"lock:warm-up:season:{seasonId}";
        var lockToken = Guid.NewGuid().ToString();
        var lockAcquired = await _redisCacheService.AcquireLockAsync(lockKey, lockToken, TimeSpan.FromSeconds(10), HttpContext.RequestAborted);

        if (lockAcquired)
        {
            // Heartbeat Task 시작 (3초마다 TTL 10초 연장)
            var cts = CancellationTokenSource.CreateLinkedTokenSource(HttpContext.RequestAborted);
            var heartbeatTask = Task.Run(async () =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(3000, cts.Token); // 3초 대기
                        await _redisCacheService.ExtendLockAsync(lockKey, lockToken, TimeSpan.FromSeconds(10), cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        // 정상 종료
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Heartbeat 실패: LockKey={LockKey}", lockKey);
                        break; // Redis 장애 시 Heartbeat 중단
                    }
                }
            }, cts.Token);

            try
            {
                // 이 서버만 Warm-up 수행
                var dbRankings = await _pvpRankingRepository.GetTopRankingsAsync(seasonId, count, HttpContext.RequestAborted);

                // 4. Transaction으로 원자적 저장 (All-or-Nothing)
                var rankingData = dbRankings.Select(r => (r.CharacterId, r.Rating)).ToList();
                await _redisCacheService.UpdateRankingCacheBatchAsync(seasonId, rankingData, HttpContext.RequestAborted);

                _logger.LogInformation("Cache warm-up completed: SeasonId={SeasonId}, Count={Count}", seasonId, dbRankings.Count);

                return dbRankings
                    .Select((r, index) => CreateRankingDto(r, index + 1))
                    .ToList();
            }
            finally
            {
                // 5. Heartbeat 중지 후 락 해제
                cts.Cancel();
                try
                {
                    await heartbeatTask; // Heartbeat 종료 대기
                }
                catch (OperationCanceledException)
                {
                    // 정상 종료
                }

                // 6. Lua Script로 안전하게 락 해제 (소유권 검증)
                await _redisCacheService.ReleaseLockAsync(lockKey, lockToken, HttpContext.RequestAborted);
            }
        }
        else
        {
            // 다른 서버가 Warm-up 중 → 짧은 대기 후 재조회
            _logger.LogInformation("Another server is warming-up cache, waiting 100ms...");
            await Task.Delay(100, HttpContext.RequestAborted);

            // 재조회 (다른 서버가 저장했을 수 있음)
            var retryRankings = await _redisCacheService.GetTopRankingsAsync(seasonId, count, HttpContext.RequestAborted);

            if (retryRankings != null && retryRankings.Count > 0)
            {
                // Redis Hit 성공!
                var characterIds = retryRankings.Keys.ToList();
                var rankings = await _pvpRankingRepository.GetByCharacterIdsAsync(seasonId, characterIds, HttpContext.RequestAborted);

                return rankings
                    .OrderByDescending(r => r.Rating)
                    .Select((r, index) => CreateRankingDto(r, index + 1))
                    .ToList();
            }

            // 그래도 없으면 PostgreSQL 직접 조회 (캐시 없이 응답)
            var dbRankings = await _pvpRankingRepository.GetTopRankingsAsync(seasonId, count, HttpContext.RequestAborted);
            return dbRankings
                .Select((r, index) => CreateRankingDto(r, index + 1))
                .ToList();
        }
    }

    /// <summary>
    /// 내 주변 랭킹 조회 (Redis 우선 → PostgreSQL Fallback)
    /// </summary>
    private async Task<List<PvpRankingDto>> GetRankingsAroundMeAsync(int seasonId, Guid characterId, int range)
    {
        // 1. Redis 시도
        var redisRankings = await _redisCacheService.GetRankingsAroundMeAsync(seasonId, characterId, range, HttpContext.RequestAborted);
        var myRank = await _redisCacheService.GetMyRankAsync(seasonId, characterId, HttpContext.RequestAborted);

        if (redisRankings != null && redisRankings.Count > 0 && myRank.HasValue)
        {
            _logger.LogInformation("Redis hit: Rankings around Character {CharacterId}, Rank {MyRank}", characterId, myRank.Value);

            var characterIds = redisRankings.Keys.ToList();

            // 벌크 조회 (N+1 쿼리 방지: 20개 조회 시 20번 쿼리 → 1번 쿼리)
            var rankings = await _pvpRankingRepository.GetByCharacterIdsAsync(seasonId, characterIds, HttpContext.RequestAborted);

            // DTO 변환 (Rank 계산: myRank ± range)
            int startRank = Math.Max(1, myRank.Value - range);
            return rankings
                .OrderByDescending(r => r.Rating)
                .Select((r, index) => CreateRankingDto(r, startRank + index))
                .ToList();
        }

        // 2. PostgreSQL Fallback (내 레이팅 기준)
        _logger.LogWarning("Redis miss: Falling back to PostgreSQL for rankings around Character {CharacterId}", characterId);

        var myRanking = await _pvpRankingRepository.GetByIdAsync(seasonId, characterId, HttpContext.RequestAborted);
        if (myRanking == null)
        {
            return new List<PvpRankingDto>();
        }

        // 3. 분산 락 획득 (동시 Warm-up 방지, 초기 TTL 10초)
        var lockKey = $"lock:warm-up:around:season:{seasonId}:char:{characterId}";
        var lockToken = Guid.NewGuid().ToString();
        var lockAcquired = await _redisCacheService.AcquireLockAsync(lockKey, lockToken, TimeSpan.FromSeconds(10), HttpContext.RequestAborted);

        if (lockAcquired)
        {
            // Heartbeat Task 시작 (3초마다 TTL 10초 연장)
            var cts = CancellationTokenSource.CreateLinkedTokenSource(HttpContext.RequestAborted);
            var heartbeatTask = Task.Run(async () =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(3000, cts.Token); // 3초 대기
                        await _redisCacheService.ExtendLockAsync(lockKey, lockToken, TimeSpan.FromSeconds(10), cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        // 정상 종료
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Heartbeat 실패: LockKey={LockKey}", lockKey);
                        break; // Redis 장애 시 Heartbeat 중단
                    }
                }
            }, cts.Token);

            try
            {
                // 이 서버만 Warm-up 수행
                var dbRankings = await _pvpRankingRepository.GetRankingsAroundAsync(seasonId, myRanking.Rating, range, HttpContext.RequestAborted);

                // 4. Transaction으로 원자적 저장 (All-or-Nothing)
                var rankingData = dbRankings.Select(r => (r.CharacterId, r.Rating)).ToList();
                await _redisCacheService.UpdateRankingCacheBatchAsync(seasonId, rankingData, HttpContext.RequestAborted);

                _logger.LogInformation("Cache warm-up completed (around me): SeasonId={SeasonId}, CharacterId={CharacterId}, Count={Count}",
                    seasonId, characterId, dbRankings.Count);

                // Rank 계산 (PostgreSQL은 정확한 Rank를 모르므로 Rating 정렬 후 추정)
                return dbRankings
                    .OrderByDescending(r => r.Rating)
                    .Select((r, index) => CreateRankingDto(r, index + 1)) // 근사값
                    .ToList();
            }
            finally
            {
                // 5. Heartbeat 중지 후 락 해제
                cts.Cancel();
                try
                {
                    await heartbeatTask; // Heartbeat 종료 대기
                }
                catch (OperationCanceledException)
                {
                    // 정상 종료
                }

                // 6. Lua Script로 안전하게 락 해제 (소유권 검증)
                await _redisCacheService.ReleaseLockAsync(lockKey, lockToken, HttpContext.RequestAborted);
            }
        }
        else
        {
            // 다른 서버가 Warm-up 중 → 짧은 대기 후 재조회
            _logger.LogInformation("Another server is warming-up cache (around me), waiting 100ms...");
            await Task.Delay(100, HttpContext.RequestAborted);

            // 재조회 (다른 서버가 저장했을 수 있음)
            var retryRankings = await _redisCacheService.GetRankingsAroundMeAsync(seasonId, characterId, range, HttpContext.RequestAborted);
            var retryMyRank = await _redisCacheService.GetMyRankAsync(seasonId, characterId, HttpContext.RequestAborted);

            if (retryRankings != null && retryRankings.Count > 0 && retryMyRank.HasValue)
            {
                // Redis Hit 성공!
                var characterIds = retryRankings.Keys.ToList();
                var rankings = await _pvpRankingRepository.GetByCharacterIdsAsync(seasonId, characterIds, HttpContext.RequestAborted);

                int startRank = Math.Max(1, retryMyRank.Value - range);
                return rankings
                    .OrderByDescending(r => r.Rating)
                    .Select((r, index) => CreateRankingDto(r, startRank + index))
                    .ToList();
            }

            // 그래도 없으면 PostgreSQL 직접 조회 (캐시 없이 응답)
            var dbRankings = await _pvpRankingRepository.GetRankingsAroundAsync(seasonId, myRanking.Rating, range, HttpContext.RequestAborted);
            return dbRankings
                .OrderByDescending(r => r.Rating)
                .Select((r, index) => CreateRankingDto(r, index + 1))
                .ToList();
        }
    }

    /// <summary>
    /// 티어별 랭킹 조회 (PostgreSQL 직접, 페이징)
    /// </summary>
    private async Task<List<PvpRankingDto>> GetRankingsByTierAsync(int seasonId, Domain.Enums.PvpTier tier, int page, int pageSize)
    {
        var dbRankings = await _pvpRankingRepository.GetByTierAsync(seasonId, tier, page, pageSize, HttpContext.RequestAborted);

        // Rank 계산 (페이지 오프셋 고려)
        int startRank = (page - 1) * pageSize + 1;
        return dbRankings
            .Select((r, index) => CreateRankingDto(r, startRank + index))
            .ToList();
    }

    /// <summary>
    /// PvpRanking → PvpRankingDto 변환
    /// </summary>
    private PvpRankingDto CreateRankingDto(PvpRanking ranking, int rank)
    {
        int totalMatches = ranking.Wins + ranking.Losses;
        double winRate = totalMatches > 0 ? (ranking.Wins / (double)totalMatches) * 100 : 0;

        return new PvpRankingDto
        {
            Rank = rank,
            CharacterId = ranking.CharacterId,
            CharacterName = ranking.Character?.Player?.UserName ?? "Unknown",
            Rating = ranking.Rating,
            Wins = ranking.Wins,
            Losses = ranking.Losses,
            WinRate = Math.Round(winRate, 1),
            Tier = ranking.Tier.ToString()
        };
    }

    /// <summary>
    /// 사용자 ID로 캐릭터 ID 조회 (첫 번째 캐릭터)
    /// </summary>
    private async Task<Guid?> GetUserCharacterIdAsync(Guid userId)
    {
        var characters = await _characterRepository.GetByPlayerIdAsync(userId);
        return characters.FirstOrDefault()?.Id;
    }

    /// <summary>
    /// 현재 활성 시즌 조회
    /// </summary>
    /// <remarks>
    /// **기능:**
    /// - 현재 진행 중인 PVP 시즌 정보 조회
    /// - 시즌 종료까지 남은 일수 계산 (DaysRemaining)
    /// - 인증 불필요 (Public API)
    ///
    /// **응답:**
    /// - SeasonId: 시즌 ID
    /// - SeasonNumber: 시즌 번호 (1, 2, 3...)
    /// - StartDate, EndDate: 시즌 기간 (UTC)
    /// - DaysRemaining: 종료까지 남은 일수 (음수면 시즌 종료됨)
    /// - IsActive: 활성화 여부 (항상 true)
    ///
    /// **에러 코드:**
    /// - 404: 활성 시즌 없음
    /// </remarks>
    /// <returns>현재 활성 시즌 정보</returns>
    /// <response code="200">조회 성공</response>
    /// <response code="404">활성 시즌 없음</response>
    [HttpGet("seasons/current")]
    [AllowAnonymous] // Public API
    [ProducesResponseType(typeof(PvpSeasonDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrentSeason()
    {
        try
        {
            var activeSeason = await _pvpSeasonRepository.GetActiveSeasonAsync(HttpContext.RequestAborted);
            if (activeSeason == null)
            {
                return NotFound(new { message = "활성 시즌이 없습니다." });
            }

            var daysRemaining = (int)(activeSeason.EndDate - DateTime.UtcNow).TotalDays;

            var seasonDto = new PvpSeasonDto
            {
                SeasonId = activeSeason.Id,
                SeasonNumber = activeSeason.SeasonNumber,
                StartDate = activeSeason.StartDate,
                EndDate = activeSeason.EndDate,
                DaysRemaining = daysRemaining,
                IsActive = activeSeason.IsActive
            };

            return Ok(seasonDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current PVP season");
            return BadRequest(new { message = "시즌 조회 중 오류가 발생했습니다." });
        }
    }

    /// <summary>
    /// 시즌 보상 수령
    /// </summary>
    /// <remarks>
    /// **기능:**
    /// - 종료된 시즌의 랭킹 보상 수령
    /// - 티어별 차등 보상 지급 (Bronze: Crystal 100, Gold: Crystal 500 + 전설 장비 상자 등)
    /// - 중복 수령 방지 (IsRewardClaimed 체크)
    /// - 인증 필수 (JWT 토큰 필요)
    ///
    /// **보상 지급 조건:**
    /// - 시즌이 종료되어야 함 (IsActive = false)
    /// - 해당 시즌에 참여한 기록이 있어야 함 (PvpRanking 존재)
    /// - 아직 보상을 수령하지 않았어야 함 (IsRewardClaimed = false)
    ///
    /// **티어별 보상:**
    /// - Bronze: Crystal 100
    /// - Silver: Crystal 300
    /// - Gold: Crystal 500 + 전설 장비 상자 1개
    /// - Platinum: Crystal 1000 + 전설 장비 상자 3개
    /// - Diamond: Crystal 2000 + 신화 장비 상자 1개
    ///
    /// **에러 코드:**
    /// - 400: 시즌 진행 중 (아직 종료 안 됨), 이미 보상 수령함
    /// - 404: 시즌 없음, 랭킹 없음 (참여하지 않음)
    /// </remarks>
    /// <param name="seasonId">시즌 ID</param>
    /// <returns>수령한 보상 정보</returns>
    /// <response code="200">보상 수령 성공</response>
    /// <response code="400">시즌 진행 중 또는 이미 수령함</response>
    /// <response code="404">시즌 없음 또는 랭킹 없음</response>
    [HttpPost("seasons/{seasonId}/rewards")]
    [ProducesResponseType(typeof(SeasonRewardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ClaimSeasonReward([FromRoute] int seasonId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var characterId = await GetUserCharacterIdAsync(userId);

            if (characterId == null)
            {
                return NotFound(new { message = "캐릭터를 찾을 수 없습니다." });
            }

            var reward = await _pvpSeasonService.ClaimSeasonRewardAsync(seasonId, characterId.Value, HttpContext.RequestAborted);

            _logger.LogInformation(
                "Season reward claimed: SeasonId={SeasonId}, CharacterId={CharacterId}, Tier={Tier}",
                seasonId, characterId.Value, reward.Tier);

            return Ok(reward);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid season reward claim: SeasonId={SeasonId}", seasonId);
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Season or ranking not found: SeasonId={SeasonId}", seasonId);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error claiming season reward: SeasonId={SeasonId}", seasonId);
            return BadRequest(new { message = "시즌 보상 수령 중 오류가 발생했습니다." });
        }
    }
}
