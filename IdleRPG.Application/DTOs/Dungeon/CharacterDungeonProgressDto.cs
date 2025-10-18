using IdleRPG.Domain.Enums;

namespace IdleRPG.Application.DTOs.Dungeon
{
    /// <summary>
    /// 캐릭터의 던전 진행 상황 DTO
    /// 각 난이도별로 최고 클리어한 스테이지를 추적합니다.
    /// </summary>
    public class CharacterDungeonProgressDto
    {
        /// <summary>
        /// 진행 상황 ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 캐릭터 ID
        /// </summary>
        public Guid CharacterId { get; set; }

        /// <summary>
        /// Normal 난이도 최고 클리어 스테이지 ID (0 = 클리어한 스테이지 없음)
        /// </summary>
        public int HighestStageClearedNormal { get; set; }

        /// <summary>
        /// Hard 난이도 최고 클리어 스테이지 ID (0 = 클리어한 스테이지 없음)
        /// </summary>
        public int HighestStageClearedHard { get; set; }

        /// <summary>
        /// Hell 난이도 최고 클리어 스테이지 ID (0 = 클리어한 스테이지 없음)
        /// </summary>
        public int HighestStageClearedHell { get; set; }

        /// <summary>
        /// 마지막 업데이트 시각
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}
