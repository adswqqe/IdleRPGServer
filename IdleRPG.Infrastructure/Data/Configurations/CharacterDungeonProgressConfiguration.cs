using IdleRPG.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdleRPG.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for CharacterDungeonProgress entity.
/// Defines table mappings, indexes, and relationships.
/// </summary>
public class CharacterDungeonProgressConfiguration : IEntityTypeConfiguration<CharacterDungeonProgress>
{
    public void Configure(EntityTypeBuilder<CharacterDungeonProgress> builder)
    {
        // Table name
        builder.ToTable("CharacterDungeonProgresses");

        // Primary Key
        builder.HasKey(p => p.Id);

        // Properties
        builder.Property(p => p.HighestStageClearedNormal)
            .IsRequired()
            .HasDefaultValue(0); // Default to 0 (no stages cleared)

        builder.Property(p => p.HighestStageClearedHard)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.HighestStageClearedNightmare)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .IsRequired();

        // Foreign Key
        builder.HasOne(p => p.Character)
            .WithMany()
            .HasForeignKey(p => p.CharacterId)
            .OnDelete(DeleteBehavior.Cascade); // Delete progress when character is deleted

        // Unique Index: One progress record per character
        builder.HasIndex(p => p.CharacterId)
            .IsUnique()
            .HasDatabaseName("IX_CharacterDungeonProgresses_CharacterId_Unique");
    }
}
