using IdleRPG.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace IdleRPG.Infrastructure.Data;

public class GameDBContext : DbContext
{
    public GameDBContext(DbContextOptions<GameDBContext> options) : base(options) { }
    
    public DbSet<Player> Players { get; set; }
    public DbSet<PlayerStats> PlayerStats { get; set; }
    public DbSet<Character> Characters { get; set; }
    public DbSet<ItemTemplate> ItemTemplates { get; set; }
    public DbSet<PlayerInventory> PlayerInventories { get; set; }
    public DbSet<OfflineReward>  OfflineRewards { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        ConfigurePlayerEntity(modelBuilder);
        ConfigureCharacterEntity(modelBuilder);
        ConfigureInventoryEntity(modelBuilder);
        ConfigureOfflineRewardEntity(modelBuilder);
        
        SeedDefaultData(modelBuilder);
    }

private void ConfigureOfflineRewardEntity(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<OfflineReward>(entity =>
    {
        entity.HasKey(o => o.Id);

        // ✅ 1:N 관계로 수정 (캐릭터는 여러 오프라인 보상을 가질 수 있음)
        entity.HasOne(o => o.Character)
              .WithMany(c => c.OfflineRewards) // List<OfflineReward>
              .HasForeignKey(o => o.CharacterId)
              .OnDelete(DeleteBehavior.Cascade);

        // 성능 최적화 인덱스
        entity.HasIndex(o => o.CharacterId);
        entity.HasIndex(o => o.IsClaimed);
        entity.HasIndex(o => new { o.CharacterId, o.IsClaimed }); // 미수령 보상 찾기

        // ✅ 논리적인 기본값 설정
        entity.Property(o => o.OfflineStartTime)
              .HasDefaultValueSql("CURRENT_TIMESTAMP"); // 오프라인 시작은 현재 시간 OK
              
        entity.Property(o => o.OfflineEndTime)
              .IsRequired(false); // NULL 허용 = 아직 오프라인 진행중
              
        entity.Property(o => o.EffectiveOfflineMinutes)
              .HasDefaultValue(0);
              
        entity.Property(o => o.RewardType)
              .HasDefaultValue(OfflineRewardType.AutoBattle);
              
        entity.Property(o => o.ExperienceGained)
              .HasDefaultValue(0);
              
        entity.Property(o => o.GoldGained)
              .HasDefaultValue(0);
              
        entity.Property(o => o.RewardItems)
              .HasColumnType("jsonb")
              .HasDefaultValueSql("'{}'");
              
        entity.Property(o => o.RewardMultiplier)
              .HasDefaultValue(1.0)
              .HasPrecision(3, 2); // 최대 9.99배
              
        entity.Property(o => o.IsClaimed)
              .HasDefaultValue(false);
              
        entity.Property(o => o.ClaimedAt)
              .IsRequired(false); // NULL 허용 = 아직 수령하지 않음
              
        entity.Property(o => o.CreatedAt)
              .HasDefaultValueSql("CURRENT_TIMESTAMP");

        entity.Property(o => o.RewardMultiplier).HasDefaultValue(1.0f);
    });
}

    private void ConfigureInventoryEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PlayerInventory>(entity =>
        {
            // 기본키 설정
            entity.HasKey(pi => pi.Id);

            // Character와의 1:N 관계 설정 (완전한 관계)
            entity.HasOne(pi => pi.Character)
                .WithMany(c => c.Inventory)
                .HasForeignKey(pi => pi.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);

            // ItemTemplate과의 관계 설정 (아이템 정보 참조)
            entity.HasOne(pi => pi.ItemTemplate)
                .WithMany()
                .HasForeignKey(pi => pi.ItemTemplateId)
                .OnDelete(DeleteBehavior.Restrict); // 아이템 템플릿은 삭제 방지

            // 성능 최적화 인덱스들
            entity.HasIndex(pi => pi.CharacterId);    // 캐릭터별 인벤토리 조회
            entity.HasIndex(pi => pi.ItemTemplateId); // 같은 아이템 검색

            entity.HasIndex(pi => new
                {
                    pi.CharacterId,
                    pi.SlotIndex
                })                               // 슬롯 검색
                .IsUnique();                     // 같은 슬롯에 중복 아이템 방지
            entity.HasIndex(pi => pi.ExpiresAt); // 만료 아이템 정리용

            // 속성 설정 (게임 밸런싱 고려)
            entity.Property(pi => pi.SlotIndex)
                .HasDefaultValue(-1); // -1 = 자동 배치

            entity.Property(pi => pi.ItemTemplateId)
                .IsRequired(); // 반드시 아이템 템플릿 필요

            entity.Property(pi => pi.Quantity)
                .HasDefaultValue(1);

            entity.Property(pi => pi.EnhancementLevel)
                .HasDefaultValue(0);

            entity.Property(pi => pi.AcquiredAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // 만료일 (NULL = 영구 아이템)
            entity.Property(pi => pi.ExpiresAt)
                .IsRequired(false);

            // JSON 데이터 (PostgreSQL 최적화)
            entity.Property(pi => pi.AdditionalOptions)
                .HasColumnType("jsonb")
                .HasDefaultValueSql("'{}'");

            entity.Property(pi => pi.IsLocked)
                .HasDefaultValue(false);
        });
    }
    

    private void ConfigureCharacterEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Character>(entity =>
        {
            entity.HasKey(e => e.Id);
            // Player와의 1:N 관계 설정 (완전한 관계)
            entity.HasOne(c => c.Player)
                .WithMany(p => p.Characters)
                .HasForeignKey(c => c.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(c => c.PlayerId);
            entity.HasIndex(c => new { c.PlayerId, c.IsMain }); // 메인 캐릭터 찾기용
            
            entity.Property(c => c.Experience).HasDefaultValue(0);
            entity.Property(c => c.Health).HasDefaultValue(100);
            entity.Property(c => c.Mana).HasDefaultValue(100);
            entity.Property(c => c.Attack).HasDefaultValue(10);
            entity.Property(c => c.Defense).HasDefaultValue(0);
            
            entity.Property(c => c.CreateAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }

    private void ConfigurePlayerEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Player>(entity =>
        {
            // Primart Key
            entity.HasKey(e => e.Id);

            // 인덱스 설정
            entity.HasIndex(p => p.UserName).IsUnique();
            entity.HasIndex(p => p.Email).IsUnique();
            
            // 기본 값 설정
            entity.Property(p => p.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(p => p.LastLogin).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(p => p.IsActive).HasDefaultValue(true);
            
            // 문자열 길이 제한
            entity.Property(p => p.UserName).HasMaxLength(50);
            entity.Property(p => p.Email).HasMaxLength(100);
            
            // 🆕 Characters와의 1:N 관계 설정
            entity.HasMany(p => p.Characters)
                .WithOne(c => c.Player)
                .HasForeignKey(c => c.PlayerId)
                .OnDelete(DeleteBehavior.Cascade); // Player 삭제시 Character도 삭제
        });

        modelBuilder.Entity<PlayerStats>(entity =>
        {
            entity.HasKey(ps => ps.PlayerId);
            entity.HasOne(ps => ps.Player)
                .WithOne(p => p.Stats)
                .HasForeignKey<PlayerStats>(ps => ps.PlayerId);
            
            entity.Property(ps => ps.Level).HasDefaultValue(1);
            entity.Property(ps => ps.Experience).HasDefaultValue(0);
            entity.Property(ps => ps.Gold).HasDefaultValue(1000);
            entity.Property(ps => ps.Gems).HasDefaultValue(0);
        });
    }
    
    private void SeedDefaultData(ModelBuilder modelBuilder)
    {
        // 기본 아이템 템플릿들 추가
        modelBuilder.Entity<ItemTemplate>().HasData(
            new ItemTemplate 
            { 
                Id = 1001, 
                Name = "나무 검", 
                Description = "초보자용 나무 검",
                Type = ItemType.Weapon,
                Rarity = ItemRarity.Common,
                BaseStats = """{"attack": 5, "durability": 100}""",
                SellPrice = 10,
                MaxStackSize = 1
            },
            new ItemTemplate 
            { 
                Id = 1002, 
                Name = "체력 포션", 
                Description = "HP를 50 회복한다",
                Type = ItemType.Consumable,
                Rarity = ItemRarity.Common,
                BaseStats = """{"healAmount": 50}""",
                SellPrice = 5,
                MaxStackSize = 99
            }
        );
    }
}