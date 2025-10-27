namespace IdleRPG.Application.DTOs.Pet
{
    /// <summary>
    /// 가챠에서 중복 펫 획득 시 골드 보상 정보 DTO
    /// </summary>
    public class DuplicateRewardDto
    {
        /// <summary>
        /// 중복된 펫 템플릿 이름
        /// </summary>
        public string PetTemplateName { get; set; } = string.Empty;

        /// <summary>
        /// 골드 보상 금액
        /// Common: 100, Rare: 500, Epic: 2,000, Legendary: 10,000
        /// </summary>
        public int GoldReward { get; set; }
    }
}
