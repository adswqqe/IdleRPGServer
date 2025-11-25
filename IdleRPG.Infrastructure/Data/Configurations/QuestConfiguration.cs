using IdleRPG.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdleRPG.Infrastructure.Data.Configurations;

/// <summary>
/// Quest 엔티티에 대한 EF Core 구성
/// 테이블 매핑, 인덱스, 관계를 정의합니다.
/// </summary>
public class QuestConfiguration : IEntityTypeConfiguration<Quest>
{
    public void Configure(EntityTypeBuilder<Quest> builder)
    {
        // 테이블 이름
        builder.ToTable("Quests");

        // 기본 키
        builder.HasKey(q => q.Id);

        // 속성 설정
        builder.Property(q => q.CharacterId)
            .IsRequired();

        builder.Property(q => q.QuestTemplateId)
            .IsRequired();

        builder.Property(q => q.Status)
            .IsRequired()
            .HasDefaultValue(QuestStatus.InProgress);

        builder.Property(q => q.AcceptedAt)
            .IsRequired();

        builder.Property(q => q.CompletedAt)
            .IsRequired(false); // nullable

        builder.Property(q => q.Progress)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(q => q.TargetCount)
            .IsRequired()
            .HasDefaultValue(1);

        // 외래 키 - Character와의 관계
        builder.HasOne(q => q.Character)
            .WithMany()
            .HasForeignKey(q => q.CharacterId)
            .OnDelete(DeleteBehavior.Cascade); // 캐릭터 삭제 시 퀘스트도 삭제

        // 인덱스: 캐릭터별 진행 중인 퀘스트 조회 최적화
        builder.HasIndex(q => new { q.CharacterId, q.Status })
            .HasDatabaseName("IX_Quests_CharacterId_Status");

        // 인덱스: 중복 수락 방지 체크 최적화
        builder.HasIndex(q => new { q.CharacterId, q.QuestTemplateId, q.Status })
            .HasDatabaseName("IX_Quests_CharacterId_QuestTemplateId_Status");
    }
}
