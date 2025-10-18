using IdleRPG.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdleRPG.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for DungeonStage entity.
/// Defines table mappings, indexes, and relationships.
/// </summary>
public class DungeonStageConfiguration : IEntityTypeConfiguration<DungeonStage>
{
    public void Configure(EntityTypeBuilder<DungeonStage> builder)
    {
        // Table name
        builder.ToTable("DungeonStages");

        // Primary Key
        builder.HasKey(d => d.Id);

        // Properties
        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.RequiredLevel)
            .IsRequired();

        builder.Property(d => d.BaseExperience)
            .IsRequired();

        builder.Property(d => d.BaseGold)
            .IsRequired();

        // Nullable fields
        builder.Property(d => d.FirstClearBonusExp)
            .IsRequired(false);

        builder.Property(d => d.FirstClearBonusGold)
            .IsRequired(false);

        builder.Property(d => d.CreatedAt)
            .IsRequired();

        // Foreign Key
        builder.HasOne(d => d.Monster)
            .WithMany()
            .HasForeignKey(d => d.MonsterId)
            .OnDelete(DeleteBehavior.Restrict); // Don't delete monster if dungeon exists

        // Indexes
        // Option A: Index on RequiredLevel for character level filtering queries
        builder.HasIndex(d => d.RequiredLevel)
            .HasDatabaseName("IX_DungeonStages_RequiredLevel");

        // Optional: Composite index for name + level queries (uncomment if needed)
        // builder.HasIndex(d => new { d.Name, d.RequiredLevel })
        //     .HasDatabaseName("IX_DungeonStages_Name_RequiredLevel");
    }
}
