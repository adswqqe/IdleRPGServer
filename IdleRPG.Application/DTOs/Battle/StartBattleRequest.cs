namespace IdleRPG.Application.DTOs.Battle
{
    /// <summary>
    /// 전투 시작 요청 DTO
    /// </summary>
    public class StartBattleRequest
    {
        /// <summary>
        /// 전투할 몬스터 ID
        /// </summary>
        public int MonsterId { get; set; }
    }
}
