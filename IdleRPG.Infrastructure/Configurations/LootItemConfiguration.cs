using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdleRPG.Infrastructure.Configurations
{
    public class LootItemConfiguration : IEntityTypeConfiguration<LootItem>
    {
        public void Configure(EntityTypeBuilder<LootItem> builder)
        {
            builder.ToTable("LootItems");

            builder.HasKey(i => i.Id);

            // Enum → String 변환 (가독성, 안정성)
            builder.Property(i => i.Type)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            // ItemId는 nullable (Gold/Exp/Equipment는 null)
            builder.Property(i => i.ItemId)
                .IsRequired(false);

            builder.Property(i => i.IsGuaranteed)
                .IsRequired()
                .HasDefaultValue(false);

            // Weight는 기본값 없음 - 기획자가 의식적으로 설정해야 함
            builder.Property(i => i.Weight)
                .IsRequired();

            builder.Property(i => i.MinQuantity)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(i => i.MaxQuantity)
                .IsRequired()
                .HasDefaultValue(1);

            // FK 인덱스 (조회 성능)
            builder.HasIndex(i => i.LootTableId);
        }
    }
}
