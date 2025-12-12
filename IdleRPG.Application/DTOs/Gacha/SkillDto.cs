using IdleRPG.Domain.Enums;

namespace IdleRPG.Application.DTOs.Gacha
{
    /// <summary>
    /// 스킬 정보를 클라이언트에 전달하는 DTO
    /// 가챠 결과, 인벤토리 조회 등에 사용
    /// </summary>
    public class SkillDto
    {
        /// <summary>
        /// 스킬 템플릿 ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 스킬 이름
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 스킬 설명
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 스킬 등급 (Common, Rare, Epic, Legendary)
        /// </summary>
        public SkillRarity Rarity { get; set; }

        /// <summary>
        /// 스킬 타입 (Active, Passive)
        /// </summary>
        public SkillType Type { get; set; }
    }
}
