using IdleRPG.Domain.Entities;

namespace IdleRPG.Domain.Repositories
{
    /// <summary>
    /// 몬스터 데이터 접근 인터페이스
    /// </summary>
    public interface IMonsterRepository : IRepository<Monster>
    {
        /// <summary>
        /// ID로 몬스터 조회
        /// </summary>
        Task<Monster?> GetByIdAsync(Guid id);

        /// <summary>
        /// 레벨 범위로 몬스터 조회 (전투 시 적절한 몬스터 선택용)
        /// </summary>
        /// <param name="minLevel">최소 레벨</param>
        /// <param name="maxLevel">최대 레벨</param>
        /// <returns>레벨 범위 내 몬스터 목록</returns>
        Task<List<Monster>> GetByLevelRangeAsync(int minLevel, int maxLevel);

        /// <summary>
        /// 몬스터 생성
        /// </summary>
        Task<Monster> CreateAsync(Monster monster);

        /// <summary>
        /// 변경사항 저장
        /// </summary>
        Task<int> SaveChangesAsync();
    }
}
