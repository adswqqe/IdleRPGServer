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
        /// 오프라인 보상 타입 Repository
        /// </summary>
        IOfflineRewardTypeRepository OfflineRewardTypes { get; }

        /// <summary>
        /// 전투 로그 Repository
        /// </summary>
        IBattleLogRepository BattleLogs { get; }

        /// <summary>
        /// 플레이어 Repository
        /// </summary>
        IPlayerRepository Players { get; }

        /// <summary>
        /// Refresh Token Repository
        /// </summary>
        IRefreshTokenRepository RefreshTokens { get; }

        /// <summary>
        /// 장비 Repository
        /// </summary>
        IEquipmentRepository Equipments { get; }

        /// <summary>
        /// 던전 템플릿 Repository
        /// </summary>
        IDungeonTemplateRepository DungeonTemplates { get; }

        /// <summary>
        /// 던전 난이도 Repository
        /// </summary>
        IDungeonDifficultyRepository DungeonDifficulties { get; }

        /// <summary>
        /// 던전 진행 상황 Repository
        /// </summary>
        IDungeonProgressRepository DungeonProgresses { get; }

        /// <summary>
        /// 던전 플레이 기록 Repository
        /// </summary>
        IDungeonRunHistoryRepository DungeonRunHistories { get; }

        /// <summary>
        /// Loot Table Repository
        /// </summary>
        ILootTableRepository LootTables { get; }

        /// <summary>
        /// 일일 던전 입장 횟수 Repository
        /// </summary>
        IUserDungeonDailyRepository UserDungeonDailies { get; }

        /// <summary>
        /// 던전 스테이지 Repository (Week 3 - New Stage-based Dungeon System)
        /// </summary>
        IDungeonStageRepository DungeonStages { get; }

        /// <summary>
        /// 캐릭터 전투 진행도 Repository (Week 3 - New Stage-based Battle System)
        /// </summary>
        ICharacterBattleProgressRepository CharacterBattleProgresses { get; }

        /// <summary>
        /// 스킬 템플릿 Repository (Week 3 - Skill Gacha System)
        /// </summary>
        ISkillTemplateRepository SkillTemplates { get; }

        /// <summary>
        /// 캐릭터 스킬 Repository (Week 3 - Skill Gacha System)
        /// </summary>
        ICharacterSkillRepository CharacterSkills { get; }

        /// <summary>
        /// 펫 Repository (Pet System)
        /// </summary>
        IPetRepository Pets { get; }

        /// <summary>
        /// 펫 템플릿 Repository (Pet System)
        /// </summary>
        IPetTemplateRepository PetTemplates { get; }

        /// <summary>
        /// 장착된 펫 Repository (Pet System)
        /// </summary>
        IEquippedPetsRepository EquippedPets { get; }

        /// <summary>
        /// 채팅방 Repository (Realtime Chat System)
        /// </summary>
        IChatRoomRepository ChatRooms { get; }

        /// <summary>
        /// 채팅 메시지 Repository (Realtime Chat System)
        /// </summary>
        IChatMessageRepository ChatMessages { get; }

        /// <summary>
        /// 변경사항을 데이터베이스에 저장 (트랜잭션 커밋)
        /// </summary>
        /// <returns>영향받은 행 수</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
