namespace IdleRPG.Domain.Entities
{
    /// <summary>
    /// 캐릭터가 소유한 펫 인스턴스
    /// </summary>
    public class Pet
    {
        /// <summary>
        /// 펫 고유 식별자 (PK)
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 소유자 캐릭터 ID (FK → characters.id)
        /// </summary>
        public Guid CharacterId { get; set; }

        /// <summary>
        /// 펫 템플릿 ID (FK → pet_templates.id)
        /// </summary>
        public int TemplateId { get; set; }

        /// <summary>
        /// 펫 레벨 (1-50)
        /// </summary>
        public int Level { get; set; } = 1;

        /// <summary>
        /// 현재 공격력 (레벨업 시 증가)
        /// </summary>
        public int CurrentAttack { get; set; }

        /// <summary>
        /// 현재 마나 (레벨업 시 증가)
        /// </summary>
        public int CurrentMana { get; set; }

        /// <summary>
        /// 획득 시간
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 최종 수정 시간
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        /// <summary>
        /// 소유자 캐릭터
        /// </summary>
        public Character Character { get; set; } = null!;

        /// <summary>
        /// 펫 템플릿 (마스터 데이터)
        /// </summary>
        public PetTemplate PetTemplate { get; set; } = null!;
    }
}
