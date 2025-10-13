using IdleRPG.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace IdleRPG.Infrastructure.Configurations
{
    public class CharacterConfiguration : IEntityTypeConfiguration<Character>
    {
        public void Configure(EntityTypeBuilder<Character> builder)
        {
            builder.ToTable("Characters");

            builder.HasKey(c => c.Id);

            // Gold 필드 설정
            builder.Property(c => c.Gold)
                .IsRequired()
                .HasDefaultValue(0);

            // LastLoginTime 필드 설정
            builder.Property(c => c.LastLoginTime)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.OwnsOne(c => c.Stats, stats =>
            {
                stats.Property(s => s.Strength).HasColumnName("Strength");
                stats.Property(s => s.Dexterity).HasColumnName("Dexterity");
                stats.Property(s => s.Intelligence).HasColumnName("Intelligence");
                stats.Property(s => s.Vitality).HasColumnName("Vitality");
            });

            builder.HasOne(c => c.Player)
                .WithMany(p => p.Characters)
                .HasForeignKey(c => c.PlayerId);

            builder.HasIndex(c => c.PlayerId);
        }
    }
}