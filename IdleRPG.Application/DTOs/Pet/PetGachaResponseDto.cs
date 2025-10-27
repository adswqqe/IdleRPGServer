namespace IdleRPG.Application.DTOs.Pet
{
    /// <summary>
    /// 펫 가챠 결과를 클라이언트에 전달하는 DTO
    /// </summary>
    public class PetGachaResponseDto
    {
        /// <summary>
        /// 획득한 펫 목록 (신규 펫만 포함)
        /// </summary>
        public List<PetDto> Pets { get; set; } = new List<PetDto>();

        /// <summary>
        /// 중복 펫으로 받은 골드 보상 목록
        /// </summary>
        public List<DuplicateRewardDto> DuplicateRewards { get; set; } = new List<DuplicateRewardDto>();

        /// <summary>
        /// 가챠 실행 후 현재 천장 카운트 (0-50)
        /// </summary>
        public int CurrentPityCount { get; set; }

        /// <summary>
        /// 가챠 실행 후 남은 크리스탈
        /// </summary>
        public long RemainingCrystal { get; set; }

        /// <summary>
        /// 중복 펫으로 획득한 총 골드 (DuplicateRewards 합계)
        /// </summary>
        public int TotalGoldFromDuplicates { get; set; }
    }
}
