using IdleRPG.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdleRPG.Infrastructure.Configurations
{
    public class DungeonWaveConfiguration : IEntityTypeConfiguration<DungeonWave>
    {
        public void Configure(EntityTypeBuilder<DungeonWave> builder)
        {
            builder.ToTable("DungeonWaves");

            builder.HasKey(w => w.Id);

            builder.Property(w => w.WaveNumber)
                .IsRequired();

            // DungeonWave → Monster: Restrict (마스터 데이터 보호)
            builder.HasOne(w => w.Monster)
                .WithMany()
                .HasForeignKey(w => w.MonsterId)
                .OnDelete(DeleteBehavior.Restrict);

            // FK 인덱스
            builder.HasIndex(w => w.DifficultyId);
            builder.HasIndex(w => w.MonsterId);
        }
    }
}
