using System.ComponentModel.DataAnnotations;

namespace IdleRPG.Application.DTOs.Pet
{
    /// <summary>
    /// 펫 레벨업 요청 DTO
    /// </summary>
    public class PetLevelUpRequestDto
    {
        /// <summary>
        /// 레벨업할 펫의 소유자 캐릭터 ID (소유권 검증용)
        /// </summary>
        [Required(ErrorMessage = "캐릭터 ID는 필수입니다.")]
        public Guid CharacterId { get; set; }
    }
}
