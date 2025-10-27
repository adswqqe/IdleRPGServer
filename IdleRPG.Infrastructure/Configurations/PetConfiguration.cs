using IdleRPG.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdleRPG.Infrastructure.Configurations
{
    /// <summary>
    /// Pet 엔티티 EF Core Configuration
    /// </summary>
    public class PetConfiguration : IEntityTypeConfiguration<Pet>
    {
        public void Configure(EntityTypeBuilder<Pet> builder)
        {
            // Table name
            builder.ToTable("pets");

            // Primary Key
            builder.HasKey(p => p.Id);

            // Properties
            builder.Property(p => p.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.Property(p => p.CharacterId)
                .HasColumnName("character_id")
                .IsRequired();

            builder.Property(p => p.TemplateId)
                .HasColumnName("template_id")
                .IsRequired();

            builder.Property(p => p.Level)
                .HasColumnName("level")
                .HasDefaultValue(1)
                .IsRequired();

            builder.Property(p => p.CurrentAttack)
                .HasColumnName("current_attack")
                .IsRequired();

            builder.Property(p => p.CurrentMana)
                .HasColumnName("current_mana")
                .IsRequired();

            builder.Property(p => p.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            builder.Property(p => p.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            // Foreign Keys
            builder.HasOne(p => p.Character)
                .WithMany()
                .HasForeignKey(p => p.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(p => p.PetTemplate)
                .WithMany(pt => pt.Pets)
                .HasForeignKey(p => p.TemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(p => p.CharacterId)
                .HasDatabaseName("idx_pets_character_id");

            // Check Constraints (PostgreSQL)
            builder.ToTable(t =>
            {
                t.HasCheckConstraint("chk_pets_level", "level >= 1 AND level <= 50");
                t.HasCheckConstraint("chk_pets_current_attack", "current_attack >= 0");
                t.HasCheckConstraint("chk_pets_current_mana", "current_mana >= 0");
            });
        }
    }
}
