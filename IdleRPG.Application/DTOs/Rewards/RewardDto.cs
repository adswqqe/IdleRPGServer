namespace IdleRPG.Application.DTOs.Rewards
{
    /// <summary>
    /// 보상 정보 DTO (전투, 퀘스트, 던전 등 공통 사용)
    /// </summary>
    public class RewardDto
    {
        /// <summary>
        /// 획득 골드
        /// </summary>
        public int Gold { get; set; }

        /// <summary>
        /// 획득 경험치
        /// </summary>
        public int Experience { get; set; }

        // TODO: 나중에 아이템 보상, 스킬북 등 추가 가능
        // public List<ItemReward> Items { get; set; } = new();
    }
}
