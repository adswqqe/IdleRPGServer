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
            builder.ToTable("Pets");

            // Primary Key
            builder.HasKey(p => p.Id);

            // Properties
            builder.Property(p => p.Id)
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.Property(p => p.CharacterId)
                .IsRequired();

            builder.Property(p => p.TemplateId)
                .IsRequired();

            builder.Property(p => p.Level)
                .HasDefaultValue(1)
                .IsRequired();

            builder.Property(p => p.CurrentAttack)
                .IsRequired();

            builder.Property(p => p.CurrentMana)
                .IsRequired();

            builder.Property(p => p.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            builder.Property(p => p.UpdatedAt)
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
                .HasDatabaseName("IX_Pets_CharacterId");

            // Check Constraints (PostgreSQL)
            builder.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Pets_Level", "\"Level\" >= 1 AND \"Level\" <= 50");
                t.HasCheckConstraint("CK_Pets_CurrentAttack", "\"CurrentAttack\" >= 0");
                t.HasCheckConstraint("CK_Pets_CurrentMana", "\"CurrentMana\" >= 0");
            });
        }
    }
}
