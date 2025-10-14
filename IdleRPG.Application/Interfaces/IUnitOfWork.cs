using IdleRPG.Domain.Repositories;

namespace IdleRPG.Application.Interfaces
{
    /// <summary>
    /// Unit of Work 패턴 - 여러 Repository 작업을 단일 트랜잭션으로 관리
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// 캐릭터 Repository
        /// </summary>
        ICharacterRepository Characters { get; }

        /// <summary>
        /// 몬스터 Repository
        /// </summary>
        IMonsterRepository Monsters { get; }

        /// <summary>
        /// 변경사항을 데이터베이스에 저장 (트랜잭션 커밋)
        /// </summary>
        /// <returns>영향받은 행 수</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
