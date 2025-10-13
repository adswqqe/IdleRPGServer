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
