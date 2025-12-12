using IdleRPG.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdleRPG.Infrastructure.Configurations
{
    public class DungeonDifficultyConfiguration : IEntityTypeConfiguration<DungeonDifficulty>
    {
        public void Configure(EntityTypeBuilder<DungeonDifficulty> builder)
        {
            builder.ToTable("DungeonDifficulties");

            builder.HasKey(d => d.Id);

            // Enum → String 변환
            builder.Property(d => d.Code)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(d => d.RecommendedPower)
                .IsRequired();

            builder.Property(d => d.BaseGold)
                .IsRequired();

            builder.Property(d => d.BaseExp)
                .IsRequired();

            // LootTableId는 nullable (보상 테이블이 없을 수도 있음)
            builder.Property(d => d.LootTableId)
                .IsRequired(false);

            builder.Property(d => d.MaxWaves)
                .IsRequired();

            builder.Property(d => d.DailyEntryLimit)
                .IsRequired()
                .HasDefaultValue(3);

            builder.Property(d => d.EntryCostGold)
                .IsRequired()
                .HasDefaultValue(0);

            // DungeonDifficulty → LootTable: Restrict (마스터 데이터 보호)
            builder.HasOne(d => d.LootTable)
                .WithMany()
                .HasForeignKey(d => d.LootTableId)
                .OnDelete(DeleteBehavior.Restrict);

            // DungeonDifficulty → DungeonWave: Cascade Delete (Master-Detail)
            builder.HasMany(d => d.Waves)
                .WithOne(w => w.Difficulty)
                .HasForeignKey(w => w.DifficultyId)
                .OnDelete(DeleteBehavior.Cascade);

            // FK 인덱스
            builder.HasIndex(d => d.TemplateId);
            builder.HasIndex(d => d.LootTableId);
        }
    }
}
