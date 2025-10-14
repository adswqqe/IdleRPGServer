namespace IdleRPG.Application.DTOs.Battle
{
    /// <summary>
    /// 전투 중 스탯 정보 DTO
    /// </summary>
    public class BattleStatsDto
    {
        public string Name { get; set; } = string.Empty;
        public long Attack { get; set; }
        public long Defense { get; set; }
        public long MaxHealth { get; set; }
        public long CurrentHealth { get; set; }
        public float CritRate { get; set; }
        public float CritDamage { get; set; }
        public float Evasion { get; set; }
        public float AttackSpeed { get; set; }
    }
}
