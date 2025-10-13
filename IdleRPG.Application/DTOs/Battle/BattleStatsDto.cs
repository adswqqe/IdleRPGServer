namespace IdleRPG.Application.DTOs.Battle
{
    /// <summary>
    /// 전투 중 스탯 정보 DTO
    /// </summary>
    public class BattleStatsDto
    {
        public string Name { get; set; } = string.Empty;
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int MaxHealth { get; set; }
        public int CurrentHealth { get; set; }
        public float CritRate { get; set; }
        public float CritDamage { get; set; }
        public float Evasion { get; set; }
    }
}
