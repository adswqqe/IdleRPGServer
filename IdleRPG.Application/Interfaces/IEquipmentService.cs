using IdleRPG.Application.DTOs.Equipment;

namespace IdleRPG.Application.Interfaces
{
    /// <summary>
    /// 장비 관리 서비스 인터페이스
    /// </summary>
    public interface IEquipmentService
    {
        /// <summary>
        /// 장비 생성 (가챠, 드랍 등)
        /// </summary>
        Task<EquipmentDto> CreateEquipmentAsync(CreateEquipmentDto dto);

        /// <summary>
        /// 캐릭터의 장착 중인 장비 조회 (5개 슬롯)
        /// </summary>
        Task<List<EquipmentDto>> GetEquippedItemsAsync(Guid characterId);

        /// <summary>
        /// 캐릭터의 인벤토리 조회 (미장착 장비)
        /// </summary>
        Task<List<EquipmentDto>> GetInventoryAsync(Guid ownerId);

        /// <summary>
        /// 장비 장착
        /// </summary>
        Task<EquipmentDto> EquipItemAsync(EquipItemDto dto);

        /// <summary>
        /// 장비 해제
        /// </summary>
        Task<EquipmentDto> UnequipItemAsync(UnequipItemDto dto);

        /// <summary>
        /// 장비 강화
        /// </summary>
        Task<EquipmentDto> EnhanceEquipmentAsync(EnhanceEquipmentDto dto);

        /// <summary>
        /// 장비 삭제
        /// </summary>
        Task<bool> DeleteEquipmentAsync(Guid equipmentId);
    }
}
