namespace IdleRPG.Domain.Enums
{
    /// <summary>
    /// PVP 티어 (레이팅 기반 등급 분류)
    /// </summary>
    public enum PvpTier
    {
        /// <summary>
        /// 브론즈 (Rating 0-999)
        /// </summary>
        Bronze = 0,

        /// <summary>
        /// 실버 (Rating 1000-1499)
        /// </summary>
        Silver = 1,

        /// <summary>
        /// 골드 (Rating 1500-1999)
        /// </summary>
        Gold = 2,

        /// <summary>
        /// 플래티넘 (Rating 2000-2499)
        /// </summary>
        Platinum = 3,

        /// <summary>
        /// 다이아몬드 (Rating 2500+)
        /// </summary>
        Diamond = 4
    }
}
