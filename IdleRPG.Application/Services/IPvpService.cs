using IdleRPG.Application.DTOs.Pvp;

namespace IdleRPG.Application.Services;

/// <summary>
/// PVP 매치 진행 서비스 인터페이스
/// </summary>
/// <remarks>
/// <para><b>책임</b>: 매칭 → 전투 → 레이팅 업데이트 → 보상 지급 전체 흐름 오케스트레이션</para>
/// <para><b>트랜잭션 관리</b>: Service 계층에서 UnitOfWork를 통해 여러 Repository 작업을 하나의 트랜잭션으로 묶음</para>
/// </remarks>
public interface IPvpService
{
    /// <summary>
    /// PVP 매치를 시작하고 전체 흐름을 실행합니다
    /// </summary>
    /// <param name="characterId">매치를 시작할 캐릭터 ID</param>
    /// <param name="userId">요청한 사용자 ID (소유권 검증용)</param>
    /// <param name="cancellationToken">취소 토큰</param>
    /// <returns>매치 결과 DTO (상대 정보, 전투 결과, 레이팅 변화, 보상)</returns>
    /// <remarks>
    /// <para><b>실행 흐름</b>:</para>
    /// <list type="number">
    ///   <item>CharacterId 소유권 검증 (userId와 Character.OwnerId 일치 확인)</item>
    ///   <item>현재 활성 시즌 조회 (IsActive = true)</item>
    ///   <item>매칭 실행: PvpMatchmakingService.FindOpponentAsync
    ///     <list type="bullet">
    ///       <item>±200 레이팅 범위 내 후보 조회</item>
    ///       <item>후보 없으면 NPC 봇 생성</item>
    ///     </list>
    ///   </item>
    ///   <item>전투 시뮬레이션 (Character vs Character)
    ///     <list type="bullet">
    ///       <item>공격자(나)의 스탯 조회</item>
    ///       <item>방어자(상대)의 스탯 조회</item>
    ///       <item>전투 결과 계산 (승패 결정)</item>
    ///     </list>
    ///   </item>
    ///   <item>레이팅 계산: EloRatingService.CalculateNewRatings
    ///     <list type="bullet">
    ///       <item>승자/패자 기대 승률 계산</item>
    ///       <item>레이팅 변화 계산 (K-Factor = 32)</item>
    ///     </list>
    ///   </item>
    ///   <item><b>트랜잭션 시작</b> (UnitOfWork):
    ///     <list type="bullet">
    ///       <item>PvpMatch 생성 (SeasonId, AttackerId, DefenderId, WinnerId, Rating Before/After)</item>
    ///       <item>PvpRanking 업데이트 (공격자/방어자 레이팅, Wins/Losses, WinStreak)</item>
    ///       <item>Character 보상 지급 (승리: Gold 100 + Crystal 10 + Experience 50, 패배: Gold 50 + Experience 25)</item>
    ///       <item>트랜잭션 커밋 (SaveChangesAsync)</item>
    ///     </list>
    ///   </item>
    ///   <item><b>Redis 랭킹 갱신</b> (트랜잭션 외부, Best Effort):
    ///     <list type="bullet">
    ///       <item>공격자 레이팅 Redis 업데이트</item>
    ///       <item>방어자 레이팅 Redis 업데이트 (IsBot = false인 경우)</item>
    ///       <item>실패 시 로그만 기록 (PostgreSQL이 Source of Truth)</item>
    ///     </list>
    ///   </item>
    ///   <item>응답 DTO 생성 및 반환</item>
    /// </list>
    /// <para><b>트랜잭션 경계 설정 근거</b>:</para>
    /// <list type="bullet">
    ///   <item><b>Service 계층에서 트랜잭션 관리</b>: 여러 Repository 작업(PvpMatch 생성, PvpRanking 업데이트, Character 보상)을 하나의 원자적 단위로 묶어야 함</item>
    ///   <item><b>Redis는 트랜잭션 외부</b>: Redis 실패가 전체 매치를 롤백시키면 안 됨 (Best Effort, Graceful Degradation)</item>
    ///   <item><b>PostgreSQL이 Source of Truth</b>: Redis는 성능 최적화용 캐시, 실패해도 PostgreSQL에서 조회 가능</item>
    /// </list>
    /// <para><b>에러 처리</b>:</para>
    /// <list type="bullet">
    ///   <item>UnauthorizedAccessException: CharacterId 소유권 없음</item>
    ///   <item>InvalidOperationException: 활성 시즌 없음</item>
    ///   <item>KeyNotFoundException: 캐릭터 또는 상대 정보 없음</item>
    /// </list>
    /// </remarks>
    /// <exception cref="UnauthorizedAccessException">CharacterId가 userId와 일치하지 않는 경우</exception>
    /// <exception cref="InvalidOperationException">활성 시즌이 없는 경우</exception>
    /// <exception cref="KeyNotFoundException">캐릭터 또는 상대 정보를 찾을 수 없는 경우</exception>
    Task<PvpMatchResponseDto> StartMatchAsync(Guid characterId, Guid userId, CancellationToken cancellationToken = default);
}
