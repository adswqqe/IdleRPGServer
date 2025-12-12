using IdleRPG.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdleRPG.Infrastructure.Configurations
{
    /// <summary>
    /// EF Core configuration for PvpSeason entity.
    /// </summary>
    public class PvpSeasonConfiguration : IEntityTypeConfiguration<PvpSeason>
    {
        public void Configure(EntityTypeBuilder<PvpSeason> builder)
        {
            builder.ToTable("PvpSeason");

            // Primary Key: Id (int, auto-increment)
            builder.HasKey(s => s.Id);
            builder.Property(s => s.Id)
                .ValueGeneratedOnAdd();

            // SeasonNumber: Required, Unique
            builder.Property(s => s.SeasonNumber)
                .IsRequired();

            // StartDate: Required
            builder.Property(s => s.StartDate)
                .IsRequired()
                .HasColumnType("timestamp without time zone");

            // EndDate: Required
            builder.Property(s => s.EndDate)
                .IsRequired()
                .HasColumnType("timestamp without time zone");

            // IsActive: Default false
            builder.Property(s => s.IsActive)
                .IsRequired()
                .HasDefaultValue(false);

            // CreatedAt: Required
            builder.Property(s => s.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");

            // UpdatedAt: Required
            builder.Property(s => s.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");

            // Indexes
            // IX_PvpSeason_IsActive: 활성 시즌 빠른 조회
            builder.HasIndex(s => s.IsActive)
                .HasDatabaseName("IX_PvpSeason_IsActive");

            // IX_PvpSeason_SeasonNumber: SeasonNumber 중복 방지 및 검색
            builder.HasIndex(s => s.SeasonNumber)
                .IsUnique()
                .HasDatabaseName("IX_PvpSeason_SeasonNumber");

            // Navigation Properties
            builder.HasMany(s => s.Rankings)
                .WithOne()
                .HasForeignKey(r => r.SeasonId)
                .OnDelete(DeleteBehavior.Restrict); // 시즌 삭제 시 랭킹 보존

            builder.HasMany(s => s.Matches)
                .WithOne()
                .HasForeignKey(m => m.SeasonId)
                .OnDelete(DeleteBehavior.Restrict); // 시즌 삭제 시 매치 기록 보존
        }
    }
}
