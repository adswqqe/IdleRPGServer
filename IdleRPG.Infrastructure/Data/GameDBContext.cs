using IdleRPG.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdleRPG.Infrastructure.Data;

public class GameDBContext : DbContext
{
    public GameDBContext(DbContextOptions<GameDBContext> options) : base(options) { }
    
    public DbSet<Player> Players { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Character> Characters { get; set; }
    public DbSet<Monster> Monsters { get; set; }
    public DbSet<BattleLog> BattleLogs { get; set; }
    public DbSet<OfflineRewardType> OfflineRewardTypes { get; set; }
    public DbSet<Equipment> Equipments { get; set; }

    // Loot Table Pattern - 보상 시스템
    public DbSet<LootTable> LootTables { get; set; }
    public DbSet<LootItem> LootItems { get; set; }
    public DbSet<ItemTemplate> ItemTemplates { get; set; }
    public DbSet<PlayerItem> PlayerItems { get; set; }

    // Dungeon System
    public DbSet<DungeonTemplate> DungeonTemplates { get; set; }
    public DbSet<DungeonDifficulty> DungeonDifficulties { get; set; }
    public DbSet<DungeonWave> DungeonWaves { get; set; }
    public DbSet<DungeonProgress> DungeonProgresses { get; set; }
    public DbSet<DungeonRunHistory> DungeonRunHistories { get; set; }
    public DbSet<UserDungeonDaily> UserDungeonDailies { get; set; }

    // Battle System - Stage-based approach (Week 3)
    public DbSet<DungeonStage> DungeonStages { get; set; }
    public DbSet<CharacterBattleProgress> CharacterBattleProgresses { get; set; }

    // Skill System
    public DbSet<CharacterSkill> CharacterSkills { get; set; }
    public DbSet<SkillTemplate> SkillTemplates { get; set; }
    public DbSet<GachaHistory> GachaHistories { get; set; }

    // Pet System
    public DbSet<Pet> Pets { get; set; }
    public DbSet<PetTemplate> PetTemplates { get; set; }
    public DbSet<EquippedPets> EquippedPets { get; set; }

    // Chat System (Realtime Chat)
    public DbSet<ChatRoom> ChatRooms { get; set; }
    public DbSet<ChatRoomParticipant> ChatRoomParticipants { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }

    // PVP Arena System
    public DbSet<PvpSeason> PvpSeasons { get; set; }
    public DbSet<PvpRanking> PvpRankings { get; set; }
    public DbSet<PvpMatch> PvpMatches { get; set; }

    // Quest System (CQRS Tutorial - Step 3)
    public DbSet<Quest> Quests { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuration 클래스 자동 적용 (권장)
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GameDBContext).Assembly);

        // 기존 방식 유지 (나중에 Configuration으로 이전 가능)
        ConfigurePlayerEntity(modelBuilder);
        ConfigureRefreshTokenEntity(modelBuilder);
    }

    private void ConfigurePlayerEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(p => p.UserName).IsUnique();
            entity.Property(p => p.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(p => p.LastLoginAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(p => p.IsActive).HasDefaultValue(true);
            entity.Property(p => p.UserName).HasMaxLength(50);
            entity.Property(p => p.PasswordHash).HasMaxLength(100);
        });
    }
    
    private void ConfigureRefreshTokenEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(rt => rt.Player)
                .WithMany()
                .HasForeignKey(rt => rt.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(rt => rt.Token).IsUnique();
            entity.HasIndex(rt => rt.PlayerId);
        });
    }
}
