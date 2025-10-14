using IdleRPG.Application.Interfaces;
using IdleRPG.Domain.Repositories;
using IdleRPG.Infrastructure.Data;
using IdleRPG.Infrastructure.Repositories;

namespace IdleRPG.Infrastructure.UnitOfWork
{
    /// <summary>
    /// Unit of Work 구현 - 여러 Repository를 단일 DbContext로 관리
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly GameDBContext _context;

        // Lazy initialization을 위한 필드
        private ICharacterRepository? _characters;
        private IMonsterRepository? _monsters;

        public UnitOfWork(GameDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 캐릭터 Repository (Lazy 초기화)
        /// </summary>
        public ICharacterRepository Characters
        {
            get
            {
                if (_characters == null)
                {
                    _characters = new CharacterRepository(_context);
                }
                return _characters;
            }
        }

        /// <summary>
        /// 몬스터 Repository (Lazy 초기화)
        /// </summary>
        public IMonsterRepository Monsters
        {
            get
            {
                if (_monsters == null)
                {
                    _monsters = new MonsterRepository(_context);
                }
                return _monsters;
            }
        }

        /// <summary>
        /// 모든 변경사항을 데이터베이스에 저장
        /// </summary>
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// 리소스 정리
        /// </summary>
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
