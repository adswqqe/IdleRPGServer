namespace IdleRPG.Domain.Entities
{
    public class PlayerStats
    {
        public Guid PlayerId { get; set; }
        public Player Player { get; set; }

        public int Level { get; set; } = 1;
        public long Experience { get; set; } = 0;
        public long Gold { get; set; } = 0;
        public int Gems { get; set; } = 0;
        public decimal OfflineHours { get; set; } = 0;
        public long TotalIdleTime { get; set; } = 0;
        public DateTime UpdateAt { get; set; } = DateTime.UtcNow;

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