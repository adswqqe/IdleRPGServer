using IdleRPG.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdleRPG.Infrastructure.Data.Configurations;

/// <summary>
/// OfflineRewardType 엔티티에 대한 EF Core 구성
/// </summary>
public class OfflineRewardTypeConfiguration : IEntityTypeConfiguration<OfflineRewardType>
{
    public void Configure(EntityTypeBuilder<OfflineRewardType> builder)
    {
        builder.ToTable("OfflineRewardTypes");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(o => o.ExperiencePerMinute)
            .IsRequired();

        builder.Property(o => o.GoldPerMinute)
            .IsRequired();

        builder.Property(o => o.MaxMinutes)
            .IsRequired();

        // 시딩 데이터
        builder.HasData(
            new OfflineRewardType
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Basic Offline Reward",
                ExperiencePerMinute = 2,  // 레벨 × 2
                GoldPerMinute = 1,        // 레벨 × 1
                MaxMinutes = 480          // 8시간
            }
        );
    }
}
