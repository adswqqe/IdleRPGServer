using IdleRPG.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdleRPG.Infrastructure.Configurations
{
    /// <summary>
    /// EquippedPets 엔티티 EF Core Configuration
    /// </summary>
    public class EquippedPetsConfiguration : IEntityTypeConfiguration<EquippedPets>
    {
        public void Configure(EntityTypeBuilder<EquippedPets> builder)
        {
            // Table name
            builder.ToTable("EquippedPets");

            // Composite Primary Key
            builder.HasKey(ep => new { ep.CharacterId, ep.SlotIndex });

            // Properties
            builder.Property(ep => ep.CharacterId)
                .IsRequired();

            builder.Property(ep => ep.SlotIndex)
                .IsRequired();

            builder.Property(ep => ep.PetId)
                .IsRequired();

            builder.Property(ep => ep.EquippedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            // UNIQUE Constraint: 한 펫은 하나의 슬롯에만 장착 가능
            builder.HasIndex(ep => ep.PetId)
                .IsUnique()
                .HasDatabaseName("UK_EquippedPets_PetId");

            // Foreign Keys
            builder.HasOne(ep => ep.Character)
                .WithMany()
                .HasForeignKey(ep => ep.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ep => ep.Pet)
                .WithMany()
                .HasForeignKey(ep => ep.PetId)
                .OnDelete(DeleteBehavior.Cascade);

            // Check Constraints (PostgreSQL)
            builder.ToTable(t =>
            {
                t.HasCheckConstraint("CK_EquippedPets_SlotIndex", "\"SlotIndex\" >= 1 AND \"SlotIndex\" <= 3");
            });
        }
    }
}
