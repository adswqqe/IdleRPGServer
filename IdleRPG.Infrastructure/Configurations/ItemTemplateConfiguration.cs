using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdleRPG.Infrastructure.Configurations
{
    public class ItemTemplateConfiguration : IEntityTypeConfiguration<ItemTemplate>
    {
        public void Configure(EntityTypeBuilder<ItemTemplate> builder)
        {
            builder.ToTable("ItemTemplates");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(t => t.Description)
                .IsRequired(false)
                .HasMaxLength(500);

            // Enum → String 변환
            builder.Property(t => t.Type)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(t => t.IconUrl)
                .IsRequired(false)
                .HasMaxLength(500);

            builder.Property(t => t.MaxStackSize)
                .IsRequired()
                .HasDefaultValue(999);
        }
    }
}
