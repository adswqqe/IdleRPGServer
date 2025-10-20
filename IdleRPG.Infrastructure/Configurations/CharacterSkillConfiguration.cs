
using IdleRPG.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdleRPG.Infrastructure.Configurations
{
    public class CharacterSkillConfiguration : IEntityTypeConfiguration<CharacterSkill>
    {
        public void Configure(EntityTypeBuilder<CharacterSkill> builder)
        {
            builder.HasKey(cs => cs.Id);

            builder.HasIndex(cs => new { cs.CharacterId, cs.SkillTemplateId }).IsUnique();

            builder.HasOne(cs => cs.Character)
                .WithMany(c => c.Skills)
                .HasForeignKey(cs => cs.CharacterId);

            builder.HasOne(cs => cs.SkillTemplate)
                .WithMany()
                .HasForeignKey(cs => cs.SkillTemplateId);
        }
    }
}
