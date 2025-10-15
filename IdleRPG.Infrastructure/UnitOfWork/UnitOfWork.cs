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
        private IOfflineRewardTypeRepository? _offlineRewardTypes;
        private IBattleLogRepository? _battleLogs;
        private IPlayerRepository? _players;
        private IRefreshTokenRepository? _refreshTokens;

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
        /// 오프라인 보상 타입 Repository (Lazy 초기화)
        /// </summary>
        public IOfflineRewardTypeRepository OfflineRewardTypes
        {
            get
            {
                if (_offlineRewardTypes == null)
                {
                    _offlineRewardTypes = new OfflineRewardTypeRepository(_context);
                }
                return _offlineRewardTypes;
            }
        }

        /// <summary>
        /// 전투 로그 Repository (Lazy 초기화)
        /// </summary>
        public IBattleLogRepository BattleLogs
        {
            get
            {
                if (_battleLogs == null)
                {
                    _battleLogs = new BattleLogRepository(_context);
                }
                return _battleLogs;
            }
        }

        /// <summary>
        /// 플레이어 Repository (Lazy 초기화)
        /// </summary>
        public IPlayerRepository Players
        {
            get
            {
                if (_players == null)
                {
                    _players = new PlayerRepository(_context);
                }
                return _players;
            }
        }

        /// <summary>
        /// Refresh Token Repository (Lazy 초기화)
        /// </summary>
        public IRefreshTokenRepository RefreshTokens
        {
            get
            {
                if (_refreshTokens == null)
                {
                    _refreshTokens = new RefreshTokenRepository(_context);
                }
                return _refreshTokens;
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
