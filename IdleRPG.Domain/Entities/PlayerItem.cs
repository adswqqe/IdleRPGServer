using System;

namespace IdleRPG.Domain.Entities
{
    /// <summary>
    /// 플레이어가 소유한 중첩 가능 아이템의 인스턴스 데이터입니다.
    /// Master-Instance 패턴의 'Instance' 역할을 하며, ItemTemplate에 정의된 마스터 데이터를 참조하여
    /// 플레이어의 소유 정보(수량, 획득 시간 등)를 나타냅니다.
    /// 상세한 패턴 설명은 ItemTemplate.cs를 참조하세요.
    /// </summary>
    public class PlayerItem
    {
        /// <summary>
        /// 인벤토리 슬롯의 고유 식별자
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 아이템 소유자 캐릭터의 ID
        /// </summary>
        public Guid CharacterId { get; set; }

        /// <summary>
        /// 아이템 템플릿의 FK
        /// </summary>
        public Guid ItemTemplateId { get; set; }

        /// <summary>
        /// 소유한 아이템의 수량
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// 아이템 최초 획득 시간
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 아이템 수량 변경 시간
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


        // Navigation Properties
        public virtual Character Character { get; set; } = null!;
        public virtual ItemTemplate ItemTemplate { get; set; } = null!;
    }
}
