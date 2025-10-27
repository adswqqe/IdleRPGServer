using IdleRPG.Application.DTOs.Pet;

namespace IdleRPG.Application.Services
{
    /// <summary>
    /// 펫 관련 비즈니스 로직을 처리하는 서비스 인터페이스
    /// </summary>
    public interface IPetService
    {
        /// <summary>
        /// 펫 가챠를 수행합니다 (1회 또는 10연차).
        /// </summary>
        /// <param name="characterId">가챠를 수행하는 캐릭터 ID</param>
        /// <param name="count">가챠 횟수 (1 또는 10)</param>
        /// <param name="cancellationToken">취소 토큰</param>
        /// <returns>가챠 결과 (획득한 펫 목록, 중복 보상, 천장 카운터)</returns>
        Task<PetGachaResponseDto> DrawPetsAsync(Guid characterId, int count, CancellationToken cancellationToken = default);

        /// <summary>
        /// 펫 ID로 펫 상세 정보를 조회합니다.
        /// </summary>
        /// <param name="petId">조회할 펫 ID</param>
        /// <param name="cancellationToken">취소 토큰</param>
        /// <returns>펫 정보</returns>
        Task<PetDto> GetPetByIdAsync(int petId, CancellationToken cancellationToken = default);

        /// <summary>
        /// 특정 캐릭터가 소유한 모든 펫을 조회합니다.
        /// </summary>
        /// <param name="characterId">캐릭터 ID</param>
        /// <param name="cancellationToken">취소 토큰</param>
        /// <returns>펫 목록</returns>
        Task<List<PetDto>> GetPetsByCharacterIdAsync(Guid characterId, CancellationToken cancellationToken = default);

        /// <summary>
        /// 펫의 레벨을 1 올립니다 (골드 소모).
        /// </summary>
        /// <param name="petId">레벨업할 펫 ID</param>
        /// <param name="characterId">펫 소유자 캐릭터 ID (소유권 검증용)</param>
        /// <param name="cancellationToken">취소 토큰</param>
        /// <returns>레벨업 결과 (새 레벨, 스탯, 비용)</returns>
        Task<PetLevelUpResponseDto> LevelUpPetAsync(int petId, Guid characterId, CancellationToken cancellationToken = default);

        /// <summary>
        /// 펫을 특정 슬롯에 장착합니다 (최대 3슬롯).
        /// </summary>
        /// <param name="characterId">펫을 장착할 캐릭터 ID</param>
        /// <param name="petId">장착할 펫 ID</param>
        /// <param name="slotIndex">장착 슬롯 번호 (1-3)</param>
        /// <param name="cancellationToken">취소 토큰</param>
        /// <returns>장착 결과 (모든 슬롯 정보, 총 버프)</returns>
        Task<PetEquipResponseDto> EquipPetAsync(Guid characterId, int petId, int slotIndex, CancellationToken cancellationToken = default);

        /// <summary>
        /// 특정 슬롯에서 펫을 해제합니다.
        /// </summary>
        /// <param name="characterId">펫을 해제할 캐릭터 ID</param>
        /// <param name="slotIndex">해제할 슬롯 번호 (1-3)</param>
        /// <param name="cancellationToken">취소 토큰</param>
        Task UnequipPetAsync(Guid characterId, int slotIndex, CancellationToken cancellationToken = default);

        /// <summary>
        /// 특정 캐릭터가 장착한 모든 펫을 조회합니다.
        /// </summary>
        /// <param name="characterId">캐릭터 ID</param>
        /// <param name="cancellationToken">취소 토큰</param>
        /// <returns>장착된 펫 목록 (슬롯 정보 포함)</returns>
        Task<List<EquippedPetDto>> GetEquippedPetsAsync(Guid characterId, CancellationToken cancellationToken = default);

        /// <summary>
        /// 펫을 삭제합니다 (장착된 펫은 삭제 불가).
        /// </summary>
        /// <param name="petId">삭제할 펫 ID</param>
        /// <param name="characterId">펫 소유자 캐릭터 ID (소유권 검증용)</param>
        /// <param name="cancellationToken">취소 토큰</param>
        Task DeletePetAsync(int petId, Guid characterId, CancellationToken cancellationToken = default);
    }
}
