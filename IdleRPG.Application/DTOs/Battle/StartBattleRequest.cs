using System.ComponentModel.DataAnnotations;

namespace IdleRPG.Application.DTOs.Battle
{
    /// <summary>
    /// 전투 시작 요청 DTO
    /// </summary>
    public class StartBattleRequest
    {
        /// <summary>
        /// 전투를 수행할 캐릭터 ID
        /// </summary>
        [Required(ErrorMessage = "캐릭터 ID는 필수입니다")]
        public Guid CharacterId { get; set; }

        /// <summary>
        /// 전투할 몬스터 ID
        /// </summary>
        [Required(ErrorMessage = "몬스터 ID는 필수입니다")]
        public Guid MonsterId { get; set; }
    }
}
