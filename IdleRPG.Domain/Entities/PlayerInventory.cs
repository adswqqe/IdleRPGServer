using System.ComponentModel.DataAnnotations;
namespace IdleRPG.Domain.Entities
{
    /// <summary>
    /// 플레이어 인벤토리 슬롯 정보
    /// Unity의 InventorySlot과 동일한 개념
    /// </summary>
    public class PlayerInventory
    {
        public Guid Id { get; set; } = Guid.NewGuid();
    
        /// <summary>
        /// 소유자 캐릭터 ID (FK)
        /// </summary>
        public Guid CharacterId { get; set; }
    
        /// <summary>
        /// 인벤토리 슬롯 인덱스 (0~99 등)
        /// Unity의 배열 인덱스와 동일한 개념
        /// </summary>
        [Range(0, 999)]
        public int SlotIndex { get; set; }
    
        /// <summary>
        /// 아이템 템플릿 ID (게임 데이터 테이블의 아이템 ID)
        /// </summary>
        public int ItemTemplateId { get; set; }
    
        /// <summary>
        /// 아이템 수량 (스택 가능한 경우)
        /// </summary>
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } = 1;
    
        /// <summary>
        /// 아이템 강화 레벨 (+1, +2 등)
        /// </summary>
        [Range(0, 20)]
        public int EnhancementLevel { get; set; } = 0;
    
        /// <summary>
        /// 아이템 획득 시간
        /// </summary>
        public DateTime AcquiredAt { get; set; } = DateTime.UtcNow;
    
        /// <summary>
        /// 아이템 만료 시간 (기간 제한 아이템용)
        /// null이면 영구 아이템
        /// </summary>
        public DateTime? ExpiresAt { get; set; }
    
        /// <summary>
        /// 아이템별 추가 옵션 (랜덤 옵션 등)
        /// JSON 형태로 저장
        /// </summary>
        public string AdditionalOptions { get; set; } = "{}";
    
        /// <summary>
        /// 아이템이 잠겨있는지 (판매/삭제 방지)
        /// </summary>
        public bool IsLocked { get; set; } = false;
    
        // Navigation Properties (Entity Framework용)
        public Character Character { get; set; }
        public ItemTemplate ItemTemplate { get; set; } // 🆕 추가
    }
}