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
            builder.ToTable("equipped_pets");

            // Composite Primary Key
            builder.HasKey(ep => new { ep.CharacterId, ep.SlotIndex });

            // Properties
            builder.Property(ep => ep.CharacterId)
                .HasColumnName("character_id")
                .IsRequired();

            builder.Property(ep => ep.SlotIndex)
                .HasColumnName("slot_index")
                .IsRequired();

            builder.Property(ep => ep.PetId)
                .HasColumnName("pet_id")
                .IsRequired();

            builder.Property(ep => ep.EquippedAt)
                .HasColumnName("equipped_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            // UNIQUE Constraint: 한 펫은 하나의 슬롯에만 장착 가능
            builder.HasIndex(ep => ep.PetId)
                .IsUnique()
                .HasDatabaseName("uk_equipped_pets_pet_id");

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
                t.HasCheckConstraint("chk_equipped_pets_slot_index", "slot_index >= 1 AND slot_index <= 3");
            });
        }
    }
}
