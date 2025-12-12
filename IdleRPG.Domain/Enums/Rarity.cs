namespace IdleRPG.Domain.Enums
{
    /// <summary>
    /// 게임 콘텐츠의 희귀도 (공용)
    /// </summary>
    /// <remarks>
    /// 사용처: Pet, Skill, Equipment 등 모든 레어도 시스템
    /// 가챠 확률: Legendary(1%), Epic(9%), Rare(30%), Common(60%)
    /// </remarks>
    public enum Rarity
    {
        /// <summary>
        /// 일반 (60% 확률)
        /// </summary>
        Common = 0,

        /// <summary>
        /// 희귀 (30% 확률)
        /// </summary>
        Rare = 1,

        /// <summary>
        /// 영웅 (9% 확률)
        /// </summary>
        Epic = 2,

        /// <summary>
        /// 전설 (1% 확률)
        /// </summary>
        Legendary = 3
    }
}
