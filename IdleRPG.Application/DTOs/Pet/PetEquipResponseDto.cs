namespace IdleRPG.Application.DTOs.Pet
{
    /// <summary>
    /// 펫 장착 결과를 클라이언트에 전달하는 DTO
    /// </summary>
    public class PetEquipResponseDto
    {
        /// <summary>
        /// 펫을 장착한 캐릭터 ID
        /// </summary>
        public Guid CharacterId { get; set; }

        /// <summary>
        /// 현재 장착된 모든 펫 목록 (슬롯 1-3)
        /// </summary>
        public List<EquippedPetDto> EquippedPets { get; set; } = new List<EquippedPetDto>();

        /// <summary>
        /// 모든 장착된 펫의 총 공격력 버프
        /// </summary>
        public int TotalBuffAttack { get; set; }

        /// <summary>
        /// 모든 장착된 펫의 총 마나 버프
        /// </summary>
        public int TotalBuffMana { get; set; }
    }
}
