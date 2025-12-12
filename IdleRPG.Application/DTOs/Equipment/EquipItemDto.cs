namespace IdleRPG.Application.DTOs.Equipment
{
    /// <summary>
    /// 장비 장착 요청 DTO
    /// </summary>
    public class EquipItemDto
    {
        public Guid EquipmentId { get; set; }
        public Guid CharacterId { get; set; }
    }
}
