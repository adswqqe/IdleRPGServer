using IdleRPG.Domain.Enums;

namespace IdleRPG.Application.DTOs.Equipment
{
    /// <summary>
    /// 장비 정보 DTO
    /// </summary>
    public class EquipmentDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public EquipmentSlot Slot { get; set; }
        public EquipmentRarity Rarity { get; set; }
        public Guid OwnerId { get; set; }
        public Guid? CharacterId { get; set; }
        public int EnhancementLevel { get; set; }
        public int BaseAttack { get; set; }
        public int BaseDefense { get; set; }
        public int BaseHp { get; set; }
        
        /// <summary>
        /// 강화 포함 최종 공격력
        /// </summary>
        public int TotalAttack { get; set; }
        
        /// <summary>
        /// 강화 포함 최종 방어력
        /// </summary>
        public int TotalDefense { get; set; }
        
        /// <summary>
        /// 강화 포함 최종 HP
        /// </summary>
        public int TotalHp { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
