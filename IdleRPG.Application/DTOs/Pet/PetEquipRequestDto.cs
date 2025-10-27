using System.ComponentModel.DataAnnotations;

namespace IdleRPG.Application.DTOs.Pet
{
    /// <summary>
    /// 펫 장착 요청 DTO
    /// </summary>
    public class PetEquipRequestDto
    {
        /// <summary>
        /// 펫을 장착할 캐릭터 ID
        /// </summary>
        [Required(ErrorMessage = "캐릭터 ID는 필수입니다.")]
        public Guid CharacterId { get; set; }

        /// <summary>
        /// 장착할 펫 ID
        /// </summary>
        [Required(ErrorMessage = "펫 ID는 필수입니다.")]
        public int PetId { get; set; }

        /// <summary>
        /// 장착 슬롯 번호 (1, 2, 3)
        /// </summary>
        [Required(ErrorMessage = "슬롯 번호는 필수입니다.")]
        [Range(1, 3, ErrorMessage = "슬롯 번호는 1~3 사이여야 합니다.")]
        public int SlotIndex { get; set; }
    }
}
