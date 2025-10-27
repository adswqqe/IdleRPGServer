using IdleRPG.Domain.Entities;

namespace IdleRPG.Domain.Repositories
{
    /// <summary>
    /// 장착된 펫 Repository 인터페이스
    /// </summary>
    public interface IEquippedPetsRepository
    {
        /// <summary>
        /// 캐릭터의 특정 슬롯에 장착된 펫을 조회합니다.
        /// </summary>
        Task<EquippedPets?> GetBySlotAsync(Guid characterId, int slotIndex, CancellationToken cancellationToken = default);

        /// <summary>
        /// 특정 펫이 장착된 정보를 조회합니다. (중복 장착 체크용)
        /// </summary>
        Task<EquippedPets?> GetByPetIdAsync(int petId, CancellationToken cancellationToken = default);

        /// <summary>
        /// 캐릭터가 장착한 모든 펫을 조회합니다. (Include Pet, PetTemplate)
        /// </summary>
        Task<List<EquippedPets>> GetByCharacterIdAsync(Guid characterId, CancellationToken cancellationToken = default);

        /// <summary>
        /// 펫을 슬롯에 장착합니다.
        /// </summary>
        Task AddAsync(EquippedPets equippedPet, CancellationToken cancellationToken = default);

        /// <summary>
        /// 장착된 펫을 해제합니다.
        /// </summary>
        Task DeleteAsync(EquippedPets equippedPet, CancellationToken cancellationToken = default);
    }
}
