using IdleRPG.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdleRPG.Infrastructure.Data.Configurations;

/// <summary>
/// CharacterDungeonProgress 엔티티에 대한 EF Core 구성입니다.
/// 테이블 매핑, 인덱스, 관계를 정의합니다.
/// </summary>
public class CharacterDungeonProgressConfiguration : IEntityTypeConfiguration<CharacterDungeonProgress>
{
    public void Configure(EntityTypeBuilder<CharacterDungeonProgress> builder)
    {
        // 테이블 이름
        builder.ToTable("CharacterDungeonProgresses");

        // 기본 키
        builder.HasKey(p => p.Id);

        // 속성
        builder.Property(p => p.HighestStageClearedNormal)
            .IsRequired()
            .HasDefaultValue(0); // 기본값 0 (클리어한 스테이지 없음)

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

        // 외래 키
        builder.HasOne(p => p.Character)
            .WithMany()
            .HasForeignKey(p => p.CharacterId)
            .OnDelete(DeleteBehavior.Cascade); // 캐릭터 삭제 시 진행 상황도 삭제

        // 고유 인덱스: 캐릭터당 하나의 진행 상황 레코드
        builder.HasIndex(p => p.CharacterId)
            .IsUnique()
            .HasDatabaseName("IX_CharacterDungeonProgresses_CharacterId_Unique");
    }
}
