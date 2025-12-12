using IdleRPG.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdleRPG.Infrastructure.Configurations
{
    public class UserDungeonDailyConfiguration : IEntityTypeConfiguration<UserDungeonDaily>
    {
        public void Configure(EntityTypeBuilder<UserDungeonDaily> builder)
        {
            builder.ToTable("UserDungeonDailies");

            builder.HasKey(d => d.Id);

            // Enum → String 변환
            builder.Property(d => d.DifficultyCode)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(d => d.EntryCount)
                .IsRequired()
                .HasDefaultValue(0);

            // DateOnly는 EF Core 9.0에서 자동으로 PostgreSQL date 타입으로 매핑
            builder.Property(d => d.Date)
                .IsRequired();

            // UserDungeonDaily → Player: Restrict (플레이어 데이터 보호)
            builder.HasOne(d => d.Player)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // UserDungeonDaily → DungeonTemplate: Restrict (마스터 데이터 보호)
            builder.HasOne(d => d.DungeonTemplate)
                .WithMany()
                .HasForeignKey(d => d.DungeonTemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            // Unique Index: (UserId, DungeonTemplateId, DifficultyCode, Date) 조합은 유일
            builder.HasIndex(d => new { d.UserId, d.DungeonTemplateId, d.DifficultyCode, d.Date })
                .IsUnique();
        }
    }
}
