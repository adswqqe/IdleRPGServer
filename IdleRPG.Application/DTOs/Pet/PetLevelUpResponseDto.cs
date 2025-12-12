namespace IdleRPG.Application.DTOs.Pet
{
    /// <summary>
    /// 펫 레벨업 결과를 클라이언트에 전달하는 DTO
    /// </summary>
    public class PetLevelUpResponseDto
    {
        /// <summary>
        /// 레벨업한 펫 ID
        /// </summary>
        public int PetId { get; set; }

        /// <summary>
        /// 레벨업 후 새 레벨
        /// </summary>
        public int NewLevel { get; set; }

        /// <summary>
        /// 레벨업 후 새 공격력
        /// </summary>
        public int NewAttack { get; set; }

        /// <summary>
        /// 레벨업 후 새 마나
        /// </summary>
        public int NewMana { get; set; }

        /// <summary>
        /// 레벨업에 소모된 골드
        /// </summary>
        public int CostGold { get; set; }

        /// <summary>
        /// 레벨업 후 남은 골드
        /// </summary>
        public long RemainingGold { get; set; }
    }
}
