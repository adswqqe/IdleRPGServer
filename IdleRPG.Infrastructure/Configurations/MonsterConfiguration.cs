using IdleRPG.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdleRPG.Infrastructure.Configurations
{
    /// <summary>
    /// Monster 엔티티 EF Core 구성
    /// </summary>
    public class MonsterConfiguration : IEntityTypeConfiguration<Monster>
    {
        public void Configure(EntityTypeBuilder<Monster> builder)
        {
            builder.ToTable("Monsters");

            builder.HasKey(m => m.Id);

            // 필수 필드 설정
            builder.Property(m => m.Name)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(m => m.Level)
                .IsRequired();

            builder.Property(m => m.MaxHealth)
                .IsRequired();

            builder.Property(m => m.Attack)
                .IsRequired();

            builder.Property(m => m.Defense)
                .IsRequired();

            builder.Property(m => m.ExperienceReward)
                .IsRequired();

            builder.Property(m => m.GoldReward)
                .IsRequired();

            builder.Property(m => m.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(m => m.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // 인덱스 생성 (레벨 범위 조회 성능 향상)
            builder.HasIndex(m => m.Level);

            // 초기 몬스터 5종 시딩
            builder.HasData(
                new Monster
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Name = "슬라임",
                    Level = 1,
                    MaxHealth = 50,
                    Attack = 10,
                    Defense = 5,
                    ExperienceReward = 10,
                    GoldReward = 5,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Monster
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Name = "고블린",
                    Level = 5,
                    MaxHealth = 150,
                    Attack = 30,
                    Defense = 15,
                    ExperienceReward = 50,
                    GoldReward = 25,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Monster
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Name = "오크",
                    Level = 10,
                    MaxHealth = 300,
                    Attack = 60,
                    Defense = 30,
                    ExperienceReward = 100,
                    GoldReward = 50,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Monster
                {
                    Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    Name = "트롤",
                    Level = 15,
                    MaxHealth = 500,
                    Attack = 100,
                    Defense = 50,
                    ExperienceReward = 150,
                    GoldReward = 75,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Monster
                {
                    Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                    Name = "드래곤",
                    Level = 20,
                    MaxHealth = 1000,
                    Attack = 200,
                    Defense = 100,
                    ExperienceReward = 300,
                    GoldReward = 150,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }
    }
}
