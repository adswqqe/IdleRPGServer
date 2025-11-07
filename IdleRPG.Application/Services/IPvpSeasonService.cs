using IdleRPG.Application.DTOs.Pvp;
using IdleRPG.Domain.Entities;

namespace IdleRPG.Application.Services;

/// <summary>
/// PVP 시즌 관리 서비스 인터페이스
/// </summary>
/// <remarks>
/// 시즌 보상 지급, 새 시즌 시작 등의 비즈니스 로직을 담당합니다.
/// </remarks>
public interface IPvpSeasonService
{
    /// <summary>
    /// 시즌 종료 후 보상 수령
    /// </summary>
    /// <param name="seasonId">시즌 ID</param>
    /// <param name="characterId">캐릭터 ID</param>
    /// <param name="cancellationToken">취소 토큰</param>
    /// <returns>보상 정보 DTO</returns>
    /// <remarks>
    /// <para><b>요구사항</b>: [US-4]</para>
    /// <para><b>비즈니스 규칙</b>:</para>
    /// <list type="bullet">
    ///   <item>시즌이 종료되어야 함 (IsActive = false)</item>
    ///   <item>중복 수령 불가 (IsRewardClaimed = false)</item>
    ///   <item>티어별 보상 차등 지급:
    ///     <list type="bullet">
    ///       <item>Bronze: Crystal 100</item>
    ///       <item>Silver: Crystal 300</item>
    ///       <item>Gold: Crystal 500 + 전설 장비 상자 1개</item>
    ///       <item>Platinum: Crystal 1000 + 전설 장비 상자 3개</item>
    ///       <item>Diamond: Crystal 2000 + 신화 장비 상자 1개</item>
    ///     </list>
    ///   </item>
    /// </list>
    /// <para><b>트랜잭션</b>: Character 보상 지급 + PvpRanking.IsRewardClaimed = true</para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">시즌이 아직 진행 중이거나 이미 보상을 수령한 경우</exception>
    /// <exception cref="KeyNotFoundException">시즌 또는 랭킹 정보를 찾을 수 없는 경우</exception>
    Task<SeasonRewardDto> ClaimSeasonRewardAsync(int seasonId, Guid characterId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 새로운 시즌 시작
    /// </summary>
    /// <param name="seasonNumber">새 시즌 번호 (1, 2, 3...)</param>
    /// <param name="startDate">시즌 시작 날짜</param>
    /// <param name="endDate">시즌 종료 날짜</param>
    /// <param name="cancellationToken">취소 토큰</param>
    /// <returns>생성된 PvpSeason 엔티티</returns>
    /// <remarks>
    /// <para><b>요구사항</b>: [US-4]</para>
    /// <para><b>비즈니스 규칙</b>:</para>
    /// <list type="bullet">
    ///   <item>기존 활성 시즌 비활성화 (IsActive = false)</item>
    ///   <item>새 시즌 생성 및 활성화 (IsActive = true)</item>
    ///   <item>Soft Reset 적용: 새 레이팅 = (기존 레이팅 + 1000) / 2
    ///     <list type="bullet">
    ///       <item>예: 2000점 → (2000 + 1000) / 2 = 1500점</item>
    ///       <item>예: 800점 → (800 + 1000) / 2 = 900점</item>
    ///     </list>
    ///   </item>
    ///   <item>Redis 랭킹 캐시 초기화</item>
    /// </list>
    /// <para><b>트랜잭션</b>: 기존 시즌 비활성화 + 새 시즌 생성 + 전체 PvpRanking Soft Reset</para>
    /// </remarks>
    /// <exception cref="ArgumentException">startDate >= endDate인 경우</exception>
    /// <exception cref="InvalidOperationException">동일한 seasonNumber가 이미 존재하는 경우</exception>
    Task<PvpSeason> StartNewSeasonAsync(int seasonNumber, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}
