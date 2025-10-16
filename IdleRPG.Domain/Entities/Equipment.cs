using IdleRPG.Domain.Enums;

namespace IdleRPG.Domain.Entities
{
    /// <summary>
    /// 장비 엔티티 (Direct-Equip 방식)
    /// CharacterId FK로 장착 상태 관리
    /// </summary>
    public class Equipment
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 장비 이름
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 장비 슬롯 (무기, 투구, 갑옷, 장갑, 신발)
        /// </summary>
        public EquipmentSlot Slot { get; set; }

        /// <summary>
        /// 장비 등급 (일반, 고급, 희귀, 영웅, 전설)
        /// </summary>
        public EquipmentRarity Rarity { get; set; }

        /// <summary>
        /// 장비 소유자 캐릭터 ID (가챠로 획득한 캐릭터)
        /// </summary>
        public Guid OwnerId { get; set; }

        /// <summary>
        /// 장착 중인 캐릭터 ID (NULL이면 인벤토리에 보관 중)
        /// </summary>
        public Guid? CharacterId { get; set; }

        /// <summary>
        /// 강화 레벨 (0~10, 기본값 0)
        /// </summary>
        public int EnhancementLevel { get; set; } = 0;

        /// <summary>
        /// 기본 공격력 (무기만 적용)
        /// </summary>
        public int BaseAttack { get; set; } = 0;

        /// <summary>
        /// 기본 방어력 (방어구만 적용)
        /// </summary>
        public int BaseDefense { get; set; } = 0;

        /// <summary>
        /// 기본 HP 증가량
        /// </summary>
        public int BaseHp { get; set; } = 0;

        /// <summary>
        /// 장비 획득 시간
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 장비 정보 업데이트 시간 (강화, 장착 시)
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public Character Owner { get; set; } = null!;
        public Character? EquippedCharacter { get; set; }

        /// <summary>
        /// 현재 공격력 (기본 공격력 + 강화 보너스)
        /// </summary>
        public int GetTotalAttack()
        {
            return BaseAttack + (EnhancementLevel * 5); // 강화당 +5 공격력
        }

        /// <summary>
        /// 현재 방어력 (기본 방어력 + 강화 보너스)
        /// </summary>
        public int GetTotalDefense()
        {
            return BaseDefense + (EnhancementLevel * 3); // 강화당 +3 방어력
        }

        /// <summary>
        /// 현재 HP (기본 HP + 강화 보너스)
        /// </summary>
        public int GetTotalHp()
        {
            return BaseHp + (EnhancementLevel * 10); // 강화당 +10 HP
        }
    }
}
