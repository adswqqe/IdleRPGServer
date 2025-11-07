namespace IdleRPG.Application.Services;

/// <summary>
/// Redis 캐싱 서비스 인터페이스 (PVP 랭킹 전용)
/// </summary>
/// <remarks>
/// - Redis Sorted Set 사용: Key = pvp:ranking:season:{seasonId}, Score = Rating, Member = CharacterId
/// - Write-Through 전략: PostgreSQL 업데이트 성공 후 Redis 갱신
/// - Fallback: Redis 장애 시 PostgreSQL 직접 조회 (성능 저하 허용)
/// </remarks>
public interface IRedisCacheService
{
    /// <summary>
    /// PVP 랭킹 캐시를 업데이트합니다 (Write-Through).
    /// </summary>
    /// <param name="seasonId">시즌 ID</param>
    /// <param name="characterId">캐릭터 ID</param>
    /// <param name="rating">새 레이팅 (Sorted Set Score)</param>
    /// <param name="cancellationToken">작업 취소 토큰</param>
    /// <returns>작업 완료 Task</returns>
    /// <exception cref="RedisException">Redis 연결 실패 또는 명령 실행 실패</exception>
    /// <remarks>
    /// - Redis 명령: ZADD pvp:ranking:season:{seasonId} {rating} {characterId}
    /// - 시간 복잡도: O(log N)
    /// - 트랜잭션 외부에서 호출 (Best Effort, 실패 시 로깅만)
    /// </remarks>
    Task UpdateRankingCacheAsync(int seasonId, Guid characterId, int rating, CancellationToken cancellationToken = default);

    /// <summary>
    /// Top N 랭킹을 조회합니다 (Redis Sorted Set, 레이팅 DESC).
    /// </summary>
    /// <param name="seasonId">시즌 ID</param>
    /// <param name="count">조회할 상위 랭커 수 (기본 100, 최대 1000)</param>
    /// <param name="cancellationToken">작업 취소 토큰</param>
    /// <returns>CharacterId와 Rating의 Dictionary (레이팅 DESC 정렬), 데이터 없거나 실패 시 null</returns>
    /// <exception cref="RedisException">Redis 연결 실패 또는 명령 실행 실패</exception>
    /// <remarks>
    /// - Redis 명령: ZREVRANGE pvp:ranking:season:{seasonId} 0 {count-1} WITHSCORES
    /// - 시간 복잡도: O(log N + count)
    /// - 실패 시 null 반환 → Controller에서 PostgreSQL Fallback
    /// </remarks>
    Task<Dictionary<Guid, int>?> GetTopRankingsAsync(int seasonId, int count, CancellationToken cancellationToken = default);

    /// <summary>
    /// 내 랭킹 순위를 조회합니다 (Redis Sorted Set, 1-based index).
    /// </summary>
    /// <param name="seasonId">시즌 ID</param>
    /// <param name="characterId">캐릭터 ID</param>
    /// <param name="cancellationToken">작업 취소 토큰</param>
    /// <returns>내 순위 (1-based, 1등 = 1), 데이터 없거나 실패 시 null</returns>
    /// <exception cref="RedisException">Redis 연결 실패 또는 명령 실행 실패</exception>
    /// <remarks>
    /// - Redis 명령: ZREVRANK pvp:ranking:season:{seasonId} {characterId}
    /// - 시간 복잡도: O(log N)
    /// - ZREVRANK는 0-based index 반환 → +1 변환 (1-based)
    /// - characterId가 랭킹에 없으면 null 반환
    /// </remarks>
    Task<int?> GetMyRankAsync(int seasonId, Guid characterId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 내 주변 ±range 등 랭킹을 조회합니다 (Redis Sorted Set).
    /// </summary>
    /// <param name="seasonId">시즌 ID</param>
    /// <param name="characterId">캐릭터 ID</param>
    /// <param name="range">내 앞뒤 조회 범위 (기본 10, 최대 50)</param>
    /// <param name="cancellationToken">작업 취소 토큰</param>
    /// <returns>내 주변 CharacterId와 Rating의 Dictionary (레이팅 DESC 정렬), 데이터 없거나 실패 시 null</returns>
    /// <exception cref="RedisException">Redis 연결 실패 또는 명령 실행 실패</exception>
    /// <remarks>
    /// - Redis 명령: ZREVRANK (내 순위 조회) → ZREVRANGE (내 순위 ± range)
    /// - 시간 복잡도: O(log N + 2*range)
    /// - 예: 내가 42등이고 range=10이면 32등~52등 조회
    /// - characterId가 랭킹에 없으면 null 반환
    /// </remarks>
    Task<Dictionary<Guid, int>?> GetRankingsAroundMeAsync(int seasonId, Guid characterId, int range, CancellationToken cancellationToken = default);

    /// <summary>
    /// 특정 시즌의 랭킹 캐시를 초기화합니다 (시즌 종료 또는 Soft Reset 시).
    /// </summary>
    /// <param name="seasonId">시즌 ID</param>
    /// <param name="cancellationToken">작업 취소 토큰</param>
    /// <returns>작업 완료 Task</returns>
    /// <exception cref="RedisException">Redis 연결 실패 또는 명령 실행 실패</exception>
    /// <remarks>
    /// - Redis 명령: DEL pvp:ranking:season:{seasonId}
    /// - 시간 복잡도: O(1)
    /// - 시즌 종료 후 새 시즌 시작 시 호출
    /// </remarks>
    Task ClearRankingCacheAsync(int seasonId, CancellationToken cancellationToken = default);
}
