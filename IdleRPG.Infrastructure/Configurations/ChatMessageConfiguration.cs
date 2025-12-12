using IdleRPG.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdleRPG.Infrastructure.Configurations;

/// <summary>
/// EF Core configuration for ChatMessage entity
/// </summary>
public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        // Table name
        builder.ToTable("ChatMessages");

        // Primary key
        builder.HasKey(m => m.Id);

        // Properties
        builder.Property(m => m.Content)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(m => m.RoomId)
            .IsRequired();

        builder.Property(m => m.SenderId)
            .IsRequired();

        builder.Property(m => m.CreatedAt)
            .IsRequired();

        // Relationships
        // ChatMessage (N) -> ChatRoom (1)
        builder.HasOne(m => m.Room)
            .WithMany(r => r.Messages)
            .HasForeignKey(m => m.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        // ChatMessage (N) -> Character (1)
        builder.HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        // 복합 인덱스: Cursor 페이징 최적화 (WHERE RoomId = ? ORDER BY CreatedAt DESC)
        builder.HasIndex(m => new { m.RoomId, m.CreatedAt })
            .HasDatabaseName("IX_ChatMessages_RoomId_CreatedAt");
    }
}
