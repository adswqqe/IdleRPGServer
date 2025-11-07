using IdleRPG.Application.DTOs.Pvp;

namespace IdleRPG.Application.Services;

/// <summary>
/// PVP 매칭 서비스 인터페이스
/// </summary>
/// <remarks>
/// - 매칭 범위: ±200 레이팅
/// - 매칭 타임아웃: 30초 (후보 없으면 NPC 봇 생성)
/// - 첫 매칭: 초기 레이팅 1000으로 PvpRanking 자동 생성
/// </remarks>
public interface IPvpMatchmakingService
{
    /// <summary>
    /// 상대방을 찾아 매칭합니다 (±200 레이팅 범위).
    /// </summary>
    /// <param name="characterId">매칭 요청 캐릭터 ID</param>
    /// <param name="seasonId">현재 시즌 ID</param>
    /// <param name="cancellationToken">작업 취소 토큰</param>
    /// <returns>매칭된 상대방 정보 (CharacterId, Name, Rating, IsBot)</returns>
    /// <exception cref="NotFoundException">characterId에 해당하는 캐릭터가 없음</exception>
    /// <remarks>
    /// <para><strong>매칭 프로세스</strong>:</para>
    /// <list type="number">
    /// <item>내 레이팅 조회 (PvpRanking WHERE SeasonId = ? AND CharacterId = ?)</item>
    /// <item>레이팅이 없으면 초기 레이팅 1000으로 PvpRanking 생성</item>
    /// <item>±200 레이팅 범위 내 후보 조회 (최대 100명, 자기 자신 제외)</item>
    /// <item>후보가 있으면 랜덤 선택, 없으면 NPC 봇 생성</item>
    /// </list>
    /// <para><strong>NPC 봇 생성</strong> (후보 없을 시):</para>
    /// <list type="bullet">
    /// <item>CharacterId: Guid.Empty</item>
    /// <item>Name: "Bot_" + Random(1000, 9999)</item>
    /// <item>Rating: 내 레이팅 ± Random(-100, 100)</item>
    /// <item>IsBot: true</item>
    /// </list>
    /// <para><strong>성능</strong>:</para>
    /// <list type="bullet">
    /// <item>평균 매칭 시간: 5초 이내 (±200 레이팅 범위)</item>
    /// <item>타임아웃: 30초 (후보 없으면 NPC 봇 매칭)</item>
    /// </list>
    /// </remarks>
    Task<MatchOpponentDto> FindOpponentAsync(Guid characterId, int seasonId, CancellationToken cancellationToken = default);
}
