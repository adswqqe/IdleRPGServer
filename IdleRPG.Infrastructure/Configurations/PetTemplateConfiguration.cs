using IdleRPG.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdleRPG.Infrastructure.Configurations
{
    /// <summary>
    /// PetTemplate 엔티티 EF Core Configuration
    /// </summary>
    public class PetTemplateConfiguration : IEntityTypeConfiguration<PetTemplate>
    {
        public void Configure(EntityTypeBuilder<PetTemplate> builder)
        {
            // Table name
            builder.ToTable("pet_templates");

            // Primary Key
            builder.HasKey(pt => pt.Id);

            // Properties
            builder.Property(pt => pt.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.Property(pt => pt.Name)
                .HasColumnName("name")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(pt => pt.Rarity)
                .HasColumnName("rarity")
                .HasConversion<int>()
                .IsRequired();

            builder.Property(pt => pt.BaseAttack)
                .HasColumnName("base_attack")
                .IsRequired();

            builder.Property(pt => pt.BaseMana)
                .HasColumnName("base_mana")
                .IsRequired();

            // Unique Constraints
            builder.HasIndex(pt => pt.Name)
                .IsUnique()
                .HasDatabaseName("uk_pet_templates_name");

            // Indexes
            builder.HasIndex(pt => pt.Rarity)
                .HasDatabaseName("idx_pet_templates_rarity");

            // Relationships
            builder.HasMany(pt => pt.Pets)
                .WithOne(p => p.PetTemplate)
                .HasForeignKey(p => p.TemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            // Check Constraints (PostgreSQL)
            builder.ToTable(t =>
            {
                t.HasCheckConstraint("chk_pet_templates_rarity", "rarity >= 0 AND rarity <= 3");
                t.HasCheckConstraint("chk_pet_templates_base_attack", "base_attack >= 0");
                t.HasCheckConstraint("chk_pet_templates_base_mana", "base_mana >= 0");
            });
        }
    }
}
