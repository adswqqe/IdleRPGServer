using IdleRPG.Domain.Entities;

namespace IdleRPG.Domain.Repositories
{
    /// <summary>
    /// 일일 던전 입장 횟수 Repository 인터페이스
    /// </summary>
    public interface IUserDungeonDailyRepository : IRepository<UserDungeonDaily>
    {
        /// <summary>
        /// 오늘 특정 던전 난이도의 입장 기록 조회
        /// </summary>
        Task<UserDungeonDaily?> GetTodayEntryAsync(Guid userId, int dungeonTemplateId, DifficultyCode code);

        /// <summary>
        /// 입장 횟수 증가
        /// </summary>
        void Update(UserDungeonDaily daily);
    }
}
