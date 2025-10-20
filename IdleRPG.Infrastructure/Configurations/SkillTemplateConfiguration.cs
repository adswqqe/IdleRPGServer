using IdleRPG.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdleRPG.Infrastructure.Configurations
{
    public class SkillTemplateConfiguration : IEntityTypeConfiguration<SkillTemplate>
    {
        public void Configure(EntityTypeBuilder<SkillTemplate> builder)
        {
            builder.ToTable("SkillTemplates");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(s => s.Rarity)
                .IsRequired()
                .HasConversion<int>(); // Enum을 int로 저장

            builder.HasIndex(s => s.Rarity); // 희귀도별 조회 최적화
        }
    }
}
