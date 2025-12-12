using IdleRPG.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdleRPG.Infrastructure.Configurations
{
    /// <summary>
    /// BattleLog 엔티티의 EF Core 설정
    /// - 외래 키: Character, Monster
    /// - 복합 인덱스: (CharacterId, BattleDate) - 캐릭터별 전투 히스토리 조회 최적화
    /// </summary>
    public class BattleLogConfiguration : IEntityTypeConfiguration<BattleLog>
    {
        public void Configure(EntityTypeBuilder<BattleLog> builder)
        {
            // 테이블 이름 설정
            builder.ToTable("BattleLogs");

            // 기본 키 설정
            builder.HasKey(bl => bl.Id);

            // 필수 속성 설정
            builder.Property(bl => bl.CharacterId)
                .IsRequired();

            builder.Property(bl => bl.MonsterId)
                .IsRequired();

            builder.Property(bl => bl.IsVictory)
                .IsRequired();

            builder.Property(bl => bl.ExperienceGained)
                .IsRequired();

            builder.Property(bl => bl.GoldGained)
                .IsRequired();

            builder.Property(bl => bl.DamageDealt)
                .IsRequired();

            builder.Property(bl => bl.DamageTaken)
                .IsRequired();

            builder.Property(bl => bl.BattleDate)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // 외래 키 관계 설정
            // BattleLog N : 1 Character
            builder.HasOne(bl => bl.Character)
                .WithMany() // Character에는 BattleLogs 컬렉션 없음 (단방향)
                .HasForeignKey(bl => bl.CharacterId)
                .OnDelete(DeleteBehavior.Restrict); // 캐릭터 삭제 시 로그 보존

            // BattleLog N : 1 Monster
            builder.HasOne(bl => bl.Monster)
                .WithMany() // Monster에는 BattleLogs 컬렉션 없음 (단방향)
                .HasForeignKey(bl => bl.MonsterId)
                .OnDelete(DeleteBehavior.Restrict); // 몬스터 삭제 시 로그 보존

            // 인덱스 설정
            // 복합 인덱스: 캐릭터별 전투 히스토리 조회 최적화
            builder.HasIndex(bl => new { bl.CharacterId, bl.BattleDate })
                .HasDatabaseName("IX_BattleLogs_CharacterId_BattleDate");

            // 몬스터별 통계 조회용 인덱스
            builder.HasIndex(bl => bl.MonsterId)
                .HasDatabaseName("IX_BattleLogs_MonsterId");
        }
    }
}
