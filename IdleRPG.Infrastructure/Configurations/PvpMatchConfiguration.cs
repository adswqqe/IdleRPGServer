using IdleRPG.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdleRPG.Infrastructure.Configurations
{
    /// <summary>
    /// EF Core configuration for PvpMatch entity.
    /// </summary>
    public class PvpMatchConfiguration : IEntityTypeConfiguration<PvpMatch>
    {
        public void Configure(EntityTypeBuilder<PvpMatch> builder)
        {
            builder.ToTable("PvpMatch");

            // Primary Key: Id (Guid)
            builder.HasKey(m => m.Id);

            // Properties: All Rating fields required
            builder.Property(m => m.SeasonId)
                .IsRequired();

            builder.Property(m => m.AttackerId)
                .IsRequired();

            builder.Property(m => m.DefenderId)
                .IsRequired();

            builder.Property(m => m.WinnerId)
                .IsRequired();

            builder.Property(m => m.AttackerRatingBefore)
                .IsRequired();

            builder.Property(m => m.AttackerRatingAfter)
                .IsRequired();

            builder.Property(m => m.DefenderRatingBefore)
                .IsRequired();

            builder.Property(m => m.DefenderRatingAfter)
                .IsRequired();

            builder.Property(m => m.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");

            // Relationships (다중 FK: 3개의 Character 참조)
            // EF Core는 같은 엔티티에 대한 다중 관계를 구분하기 위해 WithMany() 사용

            // Season 관계
            builder.HasOne(m => m.Season)
                .WithMany(s => s.Matches)
                .HasForeignKey(m => m.SeasonId)
                .OnDelete(DeleteBehavior.Restrict);
            // PvpMatch는 히스토리 데이터. 시즌 삭제 시에도 매치 기록 보존 필요

            // Attacker 관계
            builder.HasOne(m => m.Attacker)
                .WithMany() // Character에는 역방향 네비게이션 없음 (양방향 관계 아님)
                .HasForeignKey(m => m.AttackerId)
                .OnDelete(DeleteBehavior.Restrict);
            // 캐릭터 삭제 시에도 과거 매치 기록 보존 (히스토리 데이터)

            // Defender 관계
            builder.HasOne(m => m.Defender)
                .WithMany()
                .HasForeignKey(m => m.DefenderId)
                .OnDelete(DeleteBehavior.Restrict);
            // 캐릭터 삭제 시에도 과거 매치 기록 보존 (히스토리 데이터)

            // Winner 관계
            builder.HasOne(m => m.Winner)
                .WithMany()
                .HasForeignKey(m => m.WinnerId)
                .OnDelete(DeleteBehavior.Restrict);
            // 캐릭터 삭제 시에도 과거 매치 기록 보존 (히스토리 데이터)

            // Indexes
            // IX_PvpMatch_AttackerId_CreatedAt: 공격자의 매치 히스토리 조회 최적화
            builder.HasIndex(m => new { m.AttackerId, m.CreatedAt })
                .HasDatabaseName("IX_PvpMatch_AttackerId_CreatedAt");

            // IX_PvpMatch_DefenderId_CreatedAt: 방어자의 매치 히스토리 조회 최적화
            builder.HasIndex(m => new { m.DefenderId, m.CreatedAt })
                .HasDatabaseName("IX_PvpMatch_DefenderId_CreatedAt");

            // IX_PvpMatch_SeasonId: 시즌별 매치 조회 최적화
            builder.HasIndex(m => m.SeasonId)
                .HasDatabaseName("IX_PvpMatch_SeasonId");
        }
    }
}
