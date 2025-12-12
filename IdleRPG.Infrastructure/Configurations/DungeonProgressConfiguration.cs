using IdleRPG.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdleRPG.Infrastructure.Configurations
{
    public class DungeonProgressConfiguration : IEntityTypeConfiguration<DungeonProgress>
    {
        public void Configure(EntityTypeBuilder<DungeonProgress> builder)
        {
            builder.ToTable("DungeonProgresses");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.CurrentWave)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(p => p.CurrentHealth)
                .IsRequired();

            builder.Property(p => p.StartedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // DungeonProgress → Character: Restrict (플레이어 데이터 보호)
            builder.HasOne(p => p.Character)
                .WithMany()
                .HasForeignKey(p => p.CharacterId)
                .OnDelete(DeleteBehavior.Restrict);

            // DungeonProgress → DungeonDifficulty: Restrict (마스터 데이터 보호)
            builder.HasOne(p => p.Difficulty)
                .WithMany()
                .HasForeignKey(p => p.DifficultyId)
                .OnDelete(DeleteBehavior.Restrict);

            // Unique Index: 한 캐릭터는 동시에 하나의 던전만 진행 가능
            builder.HasIndex(p => p.CharacterId)
                .IsUnique();

            // FK 인덱스
            builder.HasIndex(p => p.DifficultyId);
        }
    }
}
