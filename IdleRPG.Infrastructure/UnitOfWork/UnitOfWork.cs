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
        private IEquipmentRepository? _equipments;
        private IDungeonTemplateRepository? _dungeonTemplates;
        private IDungeonDifficultyRepository? _dungeonDifficulties;
        private IDungeonProgressRepository? _dungeonProgresses;
        private IDungeonRunHistoryRepository? _dungeonRunHistories;
        private ILootTableRepository? _lootTables;
        private IUserDungeonDailyRepository? _userDungeonDailies;
        private IDungeonStageRepository? _dungeonStages;
        private ICharacterDungeonProgressRepository? _characterDungeonProgresses;

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
        /// 장비 Repository (Lazy 초기화)
        /// </summary>
        public IEquipmentRepository Equipments
        {
            get
            {
                if (_equipments == null)
                {
                    _equipments = new EquipmentRepository(_context);
                }
                return _equipments;
            }
        }

        /// <summary>
        /// 던전 템플릿 Repository (Lazy 초기화)
        /// </summary>
        public IDungeonTemplateRepository DungeonTemplates
        {
            get
            {
                if (_dungeonTemplates == null)
                {
                    _dungeonTemplates = new DungeonTemplateRepository(_context);
                }
                return _dungeonTemplates;
            }
        }

        /// <summary>
        /// 던전 난이도 Repository (Lazy 초기화)
        /// </summary>
        public IDungeonDifficultyRepository DungeonDifficulties
        {
            get
            {
                if (_dungeonDifficulties == null)
                {
                    _dungeonDifficulties = new DungeonDifficultyRepository(_context);
                }
                return _dungeonDifficulties;
            }
        }

        /// <summary>
        /// 던전 진행 상황 Repository (Lazy 초기화)
        /// </summary>
        public IDungeonProgressRepository DungeonProgresses
        {
            get
            {
                if (_dungeonProgresses == null)
                {
                    _dungeonProgresses = new DungeonProgressRepository(_context);
                }
                return _dungeonProgresses;
            }
        }

        /// <summary>
        /// 던전 플레이 기록 Repository (Lazy 초기화)
        /// </summary>
        public IDungeonRunHistoryRepository DungeonRunHistories
        {
            get
            {
                if (_dungeonRunHistories == null)
                {
                    _dungeonRunHistories = new DungeonRunHistoryRepository(_context);
                }
                return _dungeonRunHistories;
            }
        }

        /// <summary>
        /// Loot Table Repository (Lazy 초기화)
        /// </summary>
        public ILootTableRepository LootTables
        {
            get
            {
                if (_lootTables == null)
                {
                    _lootTables = new LootTableRepository(_context);
                }
                return _lootTables;
            }
        }

        /// <summary>
        /// 일일 던전 입장 횟수 Repository (Lazy 초기화)
        /// </summary>
        public IUserDungeonDailyRepository UserDungeonDailies
        {
            get
            {
                if (_userDungeonDailies == null)
                {
                    _userDungeonDailies = new UserDungeonDailyRepository(_context);
                }
                return _userDungeonDailies;
            }
        }

        /// <summary>
        /// 던전 스테이지 Repository (Lazy 초기화) - Week 3
        /// </summary>
        public IDungeonStageRepository DungeonStages
        {
            get
            {
                if (_dungeonStages == null)
                {
                    _dungeonStages = new DungeonStageRepository(_context);
                }
                return _dungeonStages;
            }
        }

        /// <summary>
        /// 캐릭터 던전 진행도 Repository (Lazy 초기화) - Week 3
        /// </summary>
        public ICharacterDungeonProgressRepository CharacterDungeonProgresses
        {
            get
            {
                if (_characterDungeonProgresses == null)
                {
                    _characterDungeonProgresses = new CharacterDungeonProgressRepository(_context);
                }
                return _characterDungeonProgresses;
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
