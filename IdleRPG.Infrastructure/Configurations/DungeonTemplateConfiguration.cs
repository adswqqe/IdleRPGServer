using IdleRPG.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdleRPG.Infrastructure.Configurations
{
    public class DungeonTemplateConfiguration : IEntityTypeConfiguration<DungeonTemplate>
    {
        public void Configure(EntityTypeBuilder<DungeonTemplate> builder)
        {
            builder.ToTable("DungeonTemplates");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(t => t.Description)
                .IsRequired(false)
                .HasMaxLength(500);

            // Enum → String 변환
            builder.Property(t => t.Category)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(t => t.MinLevel)
                .IsRequired();

            builder.Property(t => t.IsEnabled)
                .IsRequired()
                .HasDefaultValue(true);

            // DungeonTemplate → DungeonDifficulty: Cascade Delete (Master-Detail)
            builder.HasMany(t => t.Difficulties)
                .WithOne(d => d.Template)
                .HasForeignKey(d => d.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
