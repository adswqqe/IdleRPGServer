using IdleRPG.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdleRPG.Infrastructure.Configurations;

/// <summary>
/// EF Core configuration for ChatRoomParticipant entity
/// </summary>
public class ChatRoomParticipantConfiguration : IEntityTypeConfiguration<ChatRoomParticipant>
{
    public void Configure(EntityTypeBuilder<ChatRoomParticipant> builder)
    {
        // Table name
        builder.ToTable("ChatRoomParticipants");

        // Primary key
        builder.HasKey(p => p.Id);

        // Properties
        builder.Property(p => p.RoomId)
            .IsRequired();

        builder.Property(p => p.CharacterId)
            .IsRequired();

        builder.Property(p => p.JoinedAt)
            .IsRequired();

        // Relationships
        // ChatRoomParticipant (N) -> ChatRoom (1)
        builder.HasOne(p => p.Room)
            .WithMany(r => r.Participants)
            .HasForeignKey(p => p.RoomId)
            .OnDelete(DeleteBehavior.Cascade);

        // ChatRoomParticipant (N) -> Character (1)
        builder.HasOne(p => p.Character)
            .WithMany()
            .HasForeignKey(p => p.CharacterId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(p => p.RoomId)
            .HasDatabaseName("IX_ChatRoomParticipants_RoomId");

        builder.HasIndex(p => p.CharacterId)
            .HasDatabaseName("IX_ChatRoomParticipants_CharacterId");

        // Unique constraint: 한 사용자는 같은 방에 중복 참여 불가
        builder.HasIndex(p => new { p.RoomId, p.CharacterId })
            .IsUnique()
            .HasDatabaseName("IX_ChatRoomParticipants_RoomId_CharacterId");
    }
}
