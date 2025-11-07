using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdleRPG.Infrastructure.Configurations
{
    /// <summary>
    /// EF Core configuration for PvpRanking entity.
    /// </summary>
    public class PvpRankingConfiguration : IEntityTypeConfiguration<PvpRanking>
    {
        public void Configure(EntityTypeBuilder<PvpRanking> builder)
        {
            builder.ToTable("PvpRanking");

            // Composite Primary Key: (SeasonId, CharacterId)
            builder.HasKey(pr => new { pr.SeasonId, pr.CharacterId });

            // Properties
            builder.Property(pr => pr.Rating)
                .IsRequired()
                .HasDefaultValue(1000);

            builder.Property(pr => pr.Wins)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(pr => pr.Losses)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(pr => pr.WinStreak)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(pr => pr.IsRewardClaimed)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(pr => pr.LastMatchAt)
                .HasColumnType("timestamp without time zone");

            builder.Property(pr => pr.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");

            // Tier Enum: String Conversion + ValueGeneratedOnAddOrUpdate
            // Tier는 Rating 기반 계산 프로퍼티이므로, DB에 저장 시 Rating 변경마다 자동 갱신
            builder.Property(pr => pr.Tier)
                .HasConversion<string>()
                .ValueGeneratedOnAddOrUpdate();

            // TODO(human): Cascade Delete vs Restrict 전략 근거 작성
            //
            // **학습 포인트**: 외래 키 삭제 정책 (OnDelete Behavior)
            //
            // 두 가지 외래 키 관계에 대해 OnDelete 정책을 결정해야 합니다:
            // 1. PvpRanking.SeasonId → PvpSeason.Id
            // 2. PvpRanking.CharacterId → Character.Id
            //
            // 각 관계에 대해 Restrict 또는 Cascade를 선택하고, 그 근거를 아래 주석으로 작성하세요.
            //
            // 고려 사항:
            // - **Restrict**: 부모 엔티티 삭제 시 자식이 남아있으면 삭제 실패 (히스토리 보존)
            // - **Cascade**: 부모 엔티티 삭제 시 자식도 자동 삭제 (종속 데이터 정리)
            //
            // 예시:
            // - 시즌 삭제 시 랭킹 데이터를 보존해야 하는가? (히스토리)
            // - 캐릭터 삭제 시 랭킹 데이터를 함께 삭제해야 하는가? (GDPR, 개인정보 정리)

            // Relationships
            builder.HasOne(pr => pr.Season)
                .WithMany(s => s.Rankings)
                .HasForeignKey(pr => pr.SeasonId)
                .OnDelete(DeleteBehavior.Restrict);
            // 시즌은 히스토리 마스터 데이터. 과거 시즌 랭킹 기록 보존 필요 (명예의 전당, 통계 분석)
            // 실수로 시즌 삭제 시 수천 개 랭킹 레코드 손실 방지

            builder.HasOne(pr => pr.Character)
                .WithMany()
                .HasForeignKey(pr => pr.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);
            // GDPR 준수: 캐릭터 삭제 시 개인정보 종속 데이터 완전 제거
            // PvpRanking은 CharacterId 없이 조회/표시 불가능 (복합키 구조)
            
            // Indexes
            // IX_PvpRanking_SeasonId_Rating_DESC: 시즌별 랭킹 조회 최적화 (ORDER BY Rating DESC)
            builder.HasIndex(pr => new { pr.SeasonId, pr.Rating })
                .HasDatabaseName("IX_PvpRanking_SeasonId_Rating_DESC")
                .IsDescending(false, true); // SeasonId ASC, Rating DESC
        }
    }
}
