using IdleRPG.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdleRPG.Infrastructure.Configurations;

/// <summary>
/// EF Core configuration for ChatRoom entity
/// </summary>
public class ChatRoomConfiguration : IEntityTypeConfiguration<ChatRoom>
{
    public void Configure(EntityTypeBuilder<ChatRoom> builder)
    {
        // Table name
        builder.ToTable("ChatRooms");

        // Primary key
        builder.HasKey(r => r.Id);

        // Properties
        builder.Property(r => r.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(r => r.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(r => r.GuildId)
            .IsRequired(false);

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        // Relationships
        // ChatRoom (N) -> Guild (1)
        // TODO: Guild Entity 구현 후 주석 제거
        // builder.HasOne(r => r.Guild)
        //     .WithMany()
        //     .HasForeignKey(r => r.GuildId)
        //     .OnDelete(DeleteBehavior.Restrict)
        //     .IsRequired(false);

        // ChatRoom (1) -> ChatMessages (N)
        builder.HasMany(r => r.Messages)
            .WithOne(m => m.Room)
            .HasForeignKey(m => m.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        // ChatRoom (1) -> ChatRoomParticipants (N)
        builder.HasMany(r => r.Participants)
            .WithOne(p => p.Room)
            .HasForeignKey(p => p.RoomId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(r => r.Type)
            .HasDatabaseName("IX_ChatRooms_Type");

        builder.HasIndex(r => r.GuildId)
            .HasDatabaseName("IX_ChatRooms_GuildId");
    }
}
