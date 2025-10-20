
using IdleRPG.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdleRPG.Infrastructure.Configurations
{
    public class GachaHistoryConfiguration : IEntityTypeConfiguration<GachaHistory>
    {
        public void Configure(EntityTypeBuilder<GachaHistory> builder)
        {
            builder.ToTable("GachaHistories");

            builder.HasKey(gh => gh.Id);

            builder.Property(gh => gh.WasPity).IsRequired();

            builder.Property(gh => gh.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.HasOne(gh => gh.Character)
                .WithMany() // Assuming Character doesn't have a navigation property back to GachaHistory
                .HasForeignKey(gh => gh.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(gh => gh.SkillTemplate)
                .WithMany() // Assuming SkillTemplate doesn't have a navigation property back
                .HasForeignKey(gh => gh.SkillTemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(gh => new { gh.CharacterId, gh.CreatedAt }).IsDescending();
        }
    }
}
