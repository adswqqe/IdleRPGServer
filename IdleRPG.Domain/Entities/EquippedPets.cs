namespace IdleRPG.Domain.Entities
{
    /// <summary>
    /// 캐릭터의 펫 장착 정보 (최대 3슬롯)
    /// </summary>
    public class EquippedPets
    {
        /// <summary>
        /// 캐릭터 ID (Composite PK)
        /// </summary>
        public Guid CharacterId { get; set; }

        /// <summary>
        /// 슬롯 번호 (1, 2, 3) (Composite PK)
        /// </summary>
        public int SlotIndex { get; set; }

        /// <summary>
        /// 장착된 펫 ID (FK → pets.id, UNIQUE)
        /// </summary>
        public int PetId { get; set; }

        /// <summary>
        /// 장착 시간
        /// </summary>
        public DateTime EquippedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        /// <summary>
        /// 펫을 장착한 캐릭터
        /// </summary>
        public Character Character { get; set; } = null!;

        /// <summary>
        /// 장착된 펫
        /// </summary>
        public Pet Pet { get; set; } = null!;
    }
}
