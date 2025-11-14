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

            // 전투 스탯 Value Object 매핑 (자동 성장 방식)
            builder.OwnsOne(c => c.Stats, stats =>
            {
                stats.Property(s => s.Attack)
                    .HasColumnName("Attack")
                    .HasColumnType("bigint")
                    .IsRequired();

                stats.Property(s => s.Defense)
                    .HasColumnName("Defense")
                    .HasColumnType("bigint")
                    .IsRequired();

                stats.Property(s => s.MaxHealth)
                    .HasColumnName("MaxHealth")
                    .HasColumnType("bigint")
                    .IsRequired();

                stats.Property(s => s.CritRate)
                    .HasColumnName("CritRate")
                    .HasColumnType("real")
                    .IsRequired();

                stats.Property(s => s.CritDamage)
                    .HasColumnName("CritDamage")
                    .HasColumnType("real")
                    .IsRequired();

                stats.Property(s => s.Evasion)
                    .HasColumnName("Evasion")
                    .HasColumnType("real")
                    .IsRequired();

                stats.Property(s => s.AttackSpeed)
                    .HasColumnName("AttackSpeed")
                    .HasColumnType("real")
                    .IsRequired();
            });

            builder.HasOne(c => c.Player)
                .WithMany(p => p.Characters)
                .HasForeignKey(c => c.PlayerId);

            builder.HasIndex(c => c.PlayerId);
        }
    }
}