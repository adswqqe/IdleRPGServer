using IdleRPG.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdleRPG.Infrastructure.Data.Configurations;

/// <summary>
/// DungeonStage 엔티티에 대한 EF Core 구성입니다.
/// 테이블 매핑, 인덱스, 관계를 정의합니다.
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

        // Nullable 필드
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
            .OnDelete(DeleteBehavior.Restrict); // 던전이 존재하면 몬스터 삭제 안 함

        // 인덱스
        // Option A: 캐릭터 레벨 필터링 쿼리를 위한 RequiredLevel 인덱스
        builder.HasIndex(d => d.RequiredLevel)
            .HasDatabaseName("IX_DungeonStages_RequiredLevel");

        // 선택사항: 이름 + 레벨 쿼리를 위한 복합 인덱스 (필요시 주석 해제)
        // builder.HasIndex(d => new { d.Name, d.RequiredLevel })
        //     .HasDatabaseName("IX_DungeonStages_Name_RequiredLevel");
    }
}
