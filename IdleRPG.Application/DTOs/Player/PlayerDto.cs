using IdleRPG.Domain.Entities;
namespace IdleRPG.Application.DTOs.Player
{
    /// <summary>
    /// 플레이어 정보
    /// </summary>
    public class PlayerDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public int CurrentStage { get; set; }
        public long TotalIdleTime { get; set; }
        public PlayerStats Stats { get; set; }
        public List<Character> Characters { get; set; }
    }
}