using IdleRPG.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdleRPG.Infrastructure.Configurations
{
    public class PlayerItemConfiguration : IEntityTypeConfiguration<PlayerItem>
    {
        public void Configure(EntityTypeBuilder<PlayerItem> builder)
        {
            builder.ToTable("PlayerItems");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Quantity)
                .IsRequired();

            builder.Property(p => p.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(p => p.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // PlayerItem → Character: Restrict (플레이어 데이터 보호)
            builder.HasOne(p => p.Character)
                .WithMany()
                .HasForeignKey(p => p.CharacterId)
                .OnDelete(DeleteBehavior.Restrict);

            // PlayerItem → ItemTemplate: Restrict (마스터 데이터 보호)
            builder.HasOne(p => p.ItemTemplate)
                .WithMany()
                .HasForeignKey(p => p.ItemTemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            // 조회 성능을 위한 인덱스
            builder.HasIndex(p => p.CharacterId);
            builder.HasIndex(p => p.ItemTemplateId);
        }
    }
}
