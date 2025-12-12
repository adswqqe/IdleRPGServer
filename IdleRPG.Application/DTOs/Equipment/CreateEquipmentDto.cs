using IdleRPG.Domain.Enums;

namespace IdleRPG.Application.DTOs.Equipment
{
    /// <summary>
    /// 장비 생성 DTO (가챠, 드랍 등)
    /// </summary>
    public class CreateEquipmentDto
    {
        public string Name { get; set; } = string.Empty;
        public EquipmentSlot Slot { get; set; }
        public EquipmentRarity Rarity { get; set; }
        public Guid OwnerId { get; set; }
        public int BaseAttack { get; set; }
        public int BaseDefense { get; set; }
        public int BaseHp { get; set; }
    }
}
