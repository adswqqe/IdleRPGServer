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

    /// <summary>
    /// 분산 락을 획득합니다 (Cache Warm-up 동시 실행 방지).
    /// </summary>
    /// <param name="lockKey">락 키 (예: "lock:warm-up:season:1")</param>
    /// <param name="lockToken">락 소유자 식별 토큰 (Guid 권장)</param>
    /// <param name="ttl">락 유효 시간 (기본 30초)</param>
    /// <param name="cancellationToken">작업 취소 토큰</param>
    /// <returns>락 획득 성공 여부 (true: 성공, false: 다른 서버가 이미 보유)</returns>
    /// <exception cref="RedisException">Redis 연결 실패 또는 명령 실행 실패</exception>
    /// <remarks>
    /// - Redis 명령: SET lock:key token NX EX ttl
    /// - 시간 복잡도: O(1)
    /// - NX: 키가 없을 때만 설정 (원자적)
    /// - EX: TTL 만료 시 자동 삭제 (서버 크래시 대비)
    /// - lockToken은 락 해제 시 소유권 검증용
    /// </remarks>
    Task<bool> AcquireLockAsync(string lockKey, string lockToken, TimeSpan ttl, CancellationToken cancellationToken = default);

    /// <summary>
    /// 분산 락을 안전하게 해제합니다 (Lua Script로 소유권 검증).
    /// </summary>
    /// <param name="lockKey">락 키</param>
    /// <param name="lockToken">락 획득 시 사용한 토큰</param>
    /// <param name="cancellationToken">작업 취소 토큰</param>
    /// <returns>락 해제 성공 여부 (true: 성공, false: 이미 만료되었거나 다른 서버의 락)</returns>
    /// <exception cref="RedisException">Redis 연결 실패 또는 명령 실행 실패</exception>
    /// <remarks>
    /// - Lua Script: GET + 비교 + DEL (원자적 실행)
    /// - 소유권 검증: 현재 락의 토큰이 내 토큰과 일치하는지 확인
    /// - Race Condition 방지: TTL 만료 후 다른 서버의 락 삭제 방지
    /// - 시간 복잡도: O(1)
    /// </remarks>
    Task<bool> ReleaseLockAsync(string lockKey, string lockToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// 여러 랭킹을 Transaction으로 일괄 저장합니다 (All-or-Nothing).
    /// </summary>
    /// <param name="seasonId">시즌 ID</param>
    /// <param name="rankings">랭킹 목록 (CharacterId, Rating)</param>
    /// <param name="cancellationToken">작업 취소 토큰</param>
    /// <returns>저장 성공 여부 (true: 모두 성공, false: 모두 실패)</returns>
    /// <exception cref="RedisException">Redis 연결 실패 또는 명령 실행 실패</exception>
    /// <remarks>
    /// - Redis Transaction (MULTI/EXEC) 사용
    /// - 원자성 보장: 100개 중 1개 실패 → 모두 롤백
    /// - 시간 복잡도: O(N log M), N = 추가 개수, M = Sorted Set 크기
    /// - Cache Warm-up 시 부분 실패 방지용
    /// </remarks>
    Task<bool> UpdateRankingCacheBatchAsync(int seasonId, IEnumerable<(Guid CharacterId, int Rating)> rankings, CancellationToken cancellationToken = default);
}
