namespace IdleRPG.Application.DTOs.Pet
{
    /// <summary>
    /// 장착된 펫 정보 DTO (슬롯 정보 포함)
    /// </summary>
    public class EquippedPetDto
    {
        /// <summary>
        /// 장착 슬롯 번호 (1, 2, 3)
        /// </summary>
        public int SlotIndex { get; set; }

        /// <summary>
        /// 장착된 펫 ID
        /// </summary>
        public int PetId { get; set; }

        /// <summary>
        /// 펫 이름
        /// </summary>
        public string PetName { get; set; } = string.Empty;

        /// <summary>
        /// 펫 레벨
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 이 펫이 제공하는 공격력 버프 (CurrentAttack * 0.1)
        /// </summary>
        public int BuffAttack { get; set; }

        /// <summary>
        /// 이 펫이 제공하는 마나 버프 (CurrentMana * 0.1)
        /// </summary>
        public int BuffMana { get; set; }
    }
}
