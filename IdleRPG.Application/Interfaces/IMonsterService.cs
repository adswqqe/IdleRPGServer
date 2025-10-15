using IdleRPG.Application.DTOs.Monster;

namespace IdleRPG.Application.Interfaces
{
    /// <summary>
    /// 몬스터 서비스 인터페이스
    /// </summary>
    public interface IMonsterService
    {
        /// <summary>
        /// 레벨 범위 내에서 랜덤 몬스터를 선택합니다.
        /// 범위 내 몬스터가 없으면 가장 높은 레벨의 몬스터를 반환합니다.
        /// </summary>
        /// <param name="minLevel">최소 레벨</param>
        /// <param name="maxLevel">최대 레벨</param>
        /// <returns>선택된 몬스터의 ID와 레벨</returns>
        Task<RandomMonsterResponse> GetRandomMonsterAsync(int minLevel, int maxLevel);
    }
}
