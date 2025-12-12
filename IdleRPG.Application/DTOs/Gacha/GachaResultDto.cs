namespace IdleRPG.Application.DTOs.Gacha
{
    /// <summary>
    /// 가챠 결과를 클라이언트에 전달하는 DTO
    /// </summary>
    public class GachaResultDto
    {
        /// <summary>
        /// 획득한 스킬 목록
        /// </summary>
        public List<AcquiredSkillDto> AcquiredSkills { get; set; } = new List<AcquiredSkillDto>();

        /// <summary>
        /// 가챠 실행 후 남은 크리스탈
        /// </summary>
        public long RemainingCrystals { get; set; }

        /// <summary>
        /// 가챠 실행 후 현재 천장 카운트
        /// </summary>
        public int CurrentPityCount { get; set; }
    }

    /// <summary>
    /// 획득한 스킬 정보 (가챠 결과용)
    /// </summary>
    public class AcquiredSkillDto
    {
        /// <summary>
        /// 획득한 스킬 정보
        /// </summary>
        public SkillDto Skill { get; set; } = null!;

        /// <summary>
        /// 천장 시스템으로 획득했는지 여부
        /// </summary>
        public bool WasPity { get; set; }

        /// <summary>
        /// 신규 획득 여부 (true: 처음 획득, false: 중복)
        /// </summary>
        public bool IsNew { get; set; }
    }
}
