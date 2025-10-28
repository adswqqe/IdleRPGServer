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
            builder.ToTable("PetTemplates");

            // Primary Key
            builder.HasKey(pt => pt.Id);

            // Properties
            builder.Property(pt => pt.Id)
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.Property(pt => pt.Name)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(pt => pt.Rarity)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(pt => pt.BaseAttack)
                .IsRequired();

            builder.Property(pt => pt.BaseMana)
                .IsRequired();

            // Unique Constraints
            builder.HasIndex(pt => pt.Name)
                .IsUnique()
                .HasDatabaseName("UK_PetTemplates_Name");

            // Indexes
            builder.HasIndex(pt => pt.Rarity)
                .HasDatabaseName("IX_PetTemplates_Rarity");

            // Relationships
            builder.HasMany(pt => pt.Pets)
                .WithOne(p => p.PetTemplate)
                .HasForeignKey(p => p.TemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            // Check Constraints (PostgreSQL)
            builder.ToTable(t =>
            {
                t.HasCheckConstraint("CK_PetTemplates_Rarity", "\"Rarity\" >= 0 AND \"Rarity\" <= 3");
                t.HasCheckConstraint("CK_PetTemplates_BaseAttack", "\"BaseAttack\" >= 0");
                t.HasCheckConstraint("CK_PetTemplates_BaseMana", "\"BaseMana\" >= 0");
            });
        }
    }
}
