using IdleRPG.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdleRPG.Infrastructure.Configurations
{
    public class DungeonRunHistoryConfiguration : IEntityTypeConfiguration<DungeonRunHistory>
    {
        public void Configure(EntityTypeBuilder<DungeonRunHistory> builder)
        {
            builder.ToTable("DungeonRunHistories");

            builder.HasKey(h => h.Id);

            builder.Property(h => h.IsCleared)
                .IsRequired();

            builder.Property(h => h.ClearedWave)
                .IsRequired();

            builder.Property(h => h.CompletedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // DungeonRunHistory → Character: Restrict (플레이어 데이터 보호)
            builder.HasOne(h => h.Character)
                .WithMany()
                .HasForeignKey(h => h.CharacterId)
                .OnDelete(DeleteBehavior.Restrict);

            // DungeonRunHistory → DungeonDifficulty: Restrict (마스터 데이터 보호)
            builder.HasOne(h => h.Difficulty)
                .WithMany()
                .HasForeignKey(h => h.DifficultyId)
                .OnDelete(DeleteBehavior.Restrict);

            // 복합 인덱스: 유저별 기록 조회 (시간 역순)
            builder.HasIndex(h => new { h.CharacterId, h.CompletedAt });

            // FK 인덱스
            builder.HasIndex(h => h.DifficultyId);
        }
    }
}
