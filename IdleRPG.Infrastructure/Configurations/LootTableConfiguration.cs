using IdleRPG.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdleRPG.Infrastructure.Configurations
{
    public class LootTableConfiguration : IEntityTypeConfiguration<LootTable>
    {
        public void Configure(EntityTypeBuilder<LootTable> builder)
        {
            builder.ToTable("LootTables");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(t => t.NumberOfRolls)
                .IsRequired()
                .HasDefaultValue(1);

            // LootTable → LootItem: Cascade Delete (Master-Detail 관계)
            builder.HasMany(t => t.Items)
                .WithOne(i => i.LootTable)
                .HasForeignKey(i => i.LootTableId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
