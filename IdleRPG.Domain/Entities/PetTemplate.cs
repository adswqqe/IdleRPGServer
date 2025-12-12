using IdleRPG.Domain.Enums;

namespace IdleRPG.Domain.Entities
{
    /// <summary>
    /// 펫 마스터 데이터 (종류별 기본 정보)
    /// </summary>
    public class PetTemplate
    {
        /// <summary>
        /// 펫 템플릿 고유 식별자 (PK)
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 펫 이름 (예: "Fire Dragon", "Ice Wolf")
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 희귀도 (Common, Rare, Epic, Legendary)
        /// </summary>
        public Rarity Rarity { get; set; }

        /// <summary>
        /// 기본 공격력 (Lv1 기준)
        /// </summary>
        public int BaseAttack { get; set; }

        /// <summary>
        /// 기본 마나 (Lv1 기준)
        /// </summary>
        public int BaseMana { get; set; }

        // Navigation Property
        /// <summary>
        /// 이 템플릿을 사용하는 펫 인스턴스들
        /// </summary>
        public ICollection<Pet> Pets { get; set; } = new List<Pet>();
    }
}
