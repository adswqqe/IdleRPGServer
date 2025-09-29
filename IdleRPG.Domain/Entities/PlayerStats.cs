namespace IdleRPG.Domain.Entities
{
    public class PlayerStats
    {
        public Guid PlayerId { get; set; }
        public Player Player { get; set; }

        public int Level { get; set; } = 1;
        public long Experience { get; set; } = 0;
        public long Gold { get; set; } = 1000;
        public int Gems { get; set; } = 10;
        public int VipLevel { get; set; } = 0;
    
        // 방치형 게임 전용 스탯
        public decimal OfflineGoldMultiplier { get; set; } = 1.0m;
        public decimal OfflineExpMultiplier { get; set; } = 1.0m;
        public int MaxOfflineHours { get; set; } = 12;
    
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool CanLevelUp()
        {
            long requiredExp = CalculateRequiredExp(Level);
            return Experience >= requiredExp;
        }

        private long CalculateRequiredExp(int level)
        {
            return (long)(100 * Math.Pow(1.2, level - 1));
        }
    }
}