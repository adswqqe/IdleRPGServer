using System.ComponentModel.DataAnnotations;

namespace IdleRPG.Application.DTOs.Pet
{
    /// <summary>
    /// 펫 가챠 요청 DTO
    /// </summary>
    public class PetGachaRequestDto
    {
        /// <summary>
        /// 가챠를 실행할 캐릭터 ID
        /// </summary>
        [Required(ErrorMessage = "캐릭터 ID는 필수입니다.")]
        public Guid CharacterId { get; set; }

        /// <summary>
        /// 가챠 실행 횟수 (1회 또는 10회만 허용)
        /// </summary>
        [Required(ErrorMessage = "가챠 횟수는 필수입니다.")]
        [Range(1, 10, ErrorMessage = "가챠 횟수는 1 또는 10이어야 합니다.")]
        public int Count { get; set; } = 1;
    }
}
