using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Enums;

namespace IdleRPG.Domain.Repositories
{
    /// <summary>
    /// Equipment Repository 인터페이스
    /// </summary>
    public interface IEquipmentRepository : IRepository<Equipment>
    {
        /// <summary>
        /// ID로 장비 조회
        /// </summary>
        Task<Equipment?> GetByIdAsync(Guid id);

        /// <summary>
        /// 캐릭터가 장착 중인 모든 장비 조회 (5개 슬롯)
        /// </summary>
        Task<List<Equipment>> GetEquippedByCharacterIdAsync(Guid characterId);

        /// <summary>
        /// 캐릭터의 특정 슬롯 장비 조회
        /// </summary>
        Task<Equipment?> GetEquippedBySlotAsync(Guid characterId, EquipmentSlot slot);

        /// <summary>
        /// 캐릭터의 인벤토리 조회 (소유한 모든 미장착 장비)
        /// </summary>
        Task<List<Equipment>> GetInventoryByOwnerIdAsync(Guid ownerId);

        /// <summary>
        /// 등급별 장비 조회
        /// </summary>
        Task<List<Equipment>> GetByRarityAsync(EquipmentRarity rarity);

        /// <summary>
        /// 장비가 존재하는지 확인
        /// </summary>
        Task<bool> ExistsAsync(Guid id);

        /// <summary>
        /// 장비 삭제
        /// </summary>
        void Delete(Equipment equipment);
    }
}
