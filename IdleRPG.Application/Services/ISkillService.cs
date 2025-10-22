using IdleRPG.Application.DTOs.Gacha;

namespace IdleRPG.Application.Services
{
    /// <summary>
    /// 스킬 관련 비즈니스 로직을 처리하는 서비스 인터페이스
    /// </summary>
    public interface ISkillService
    {
        /// <summary>
        /// 스킬 가챠를 수행합니다.
        /// </summary>
        /// <param name="characterId">가챠를 수행하는 캐릭터 ID</param>
        /// <param name="gachaCost">가챠 비용 (Crystal)</param>
        /// <returns>가챠 결과 (획득한 스킬 정보)</returns>
        Task<SkillDto> PerformGachaAsync(Guid characterId, int gachaCost = 100);
    }
}
