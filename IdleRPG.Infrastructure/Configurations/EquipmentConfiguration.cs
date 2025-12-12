using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdleRPG.Infrastructure.Configurations
{
    public class EquipmentConfiguration : IEntityTypeConfiguration<Equipment>
    {
        public void Configure(EntityTypeBuilder<Equipment> builder)
        {
            builder.ToTable("Equipments");

            builder.HasKey(e => e.Id);

            // 장비 이름
            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            // 장비 슬롯 (Enum → int)
            builder.Property(e => e.Slot)
                .IsRequired()
                .HasConversion<int>();

            // 장비 등급 (Enum → int)
            builder.Property(e => e.Rarity)
                .IsRequired()
                .HasConversion<int>();

            // 소유자 캐릭터 (필수)
            builder.Property(e => e.OwnerId)
                .IsRequired();

            // 장착 중인 캐릭터 (NULL 가능 - 인벤토리 보관 중)
            builder.Property(e => e.CharacterId)
                .IsRequired(false);

            // 강화 레벨
            builder.Property(e => e.EnhancementLevel)
                .IsRequired()
                .HasDefaultValue(0);

            // 스탯 필드
            builder.Property(e => e.BaseAttack)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(e => e.BaseDefense)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(e => e.BaseHp)
                .IsRequired()
                .HasDefaultValue(0);

            // 타임스탬프
            builder.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Foreign Key 관계
            // 1. 소유자 관계 (필수)
            builder.HasOne(e => e.Owner)
                .WithMany() // Character.Equipments Navigation Property 추가 가능
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.Cascade); // 소유자 삭제 시 장비도 삭제

            // 2. 장착 관계 (선택)
            builder.HasOne(e => e.EquippedCharacter)
                .WithMany()
                .HasForeignKey(e => e.CharacterId)
                .OnDelete(DeleteBehavior.SetNull); // 장착 해제 시 CharacterId만 NULL

            // 인덱스 (조회 성능 최적화)
            builder.HasIndex(e => e.OwnerId); // 소유자별 인벤토리 조회
            builder.HasIndex(e => e.CharacterId); // 장착 중인 장비 조회
            builder.HasIndex(e => new { e.CharacterId, e.Slot }); // 캐릭터별 슬롯 조회
            builder.HasIndex(e => e.Rarity); // 등급별 조회
        }
    }
}
