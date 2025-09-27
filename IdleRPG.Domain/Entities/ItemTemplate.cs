namespace IdleRPG.Domain.Entities
{
    /// <summary>
    /// 아이템 템플릿 정보 (게임 데이터)
    /// Unity의 ScriptableObject ItemData와 유사
    /// </summary>
    public class ItemTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ItemType Type { get; set; }
        public ItemRarity Rarity { get; set; }
    
        /// <summary>
        /// 최대 스택 수량
        /// </summary>
        public int MaxStackSize { get; set; } = 1;
        
        /// <summary>
        /// 판매 가격
        /// </summary>
        public long SellPrice { get; set; }
    
        /// <summary>
        /// 아이템 기본 스탯 (JSON)
        /// </summary>
        public string BaseStats { get; set; } = "{}";
    
        /// <summary>
        /// 강화 가능 여부
        /// </summary>
        public bool CanEnhance { get; set; } = false;
    
        /// <summary>
        /// 거래 가능 여부
        /// </summary>
        public bool IsTradeable { get; set; } = true;
    }

    public enum ItemType
    {
        Weapon,     // 무기
        Armor,      // 방어구
        Accessory,  // 악세서리
        Consumable, // 소모품
        Material,   // 재료
        Quest,      // 퀘스트 아이템
        Currency    // 화폐류
    }

    public enum ItemRarity
    {
        Common = 1,   // 일반 (회색)
        Uncommon = 2, // 고급 (녹색)
        Rare = 3,     // 희귀 (파란색)
        Epic = 4,     // 영웅 (보라색)
        Legendary = 5 // 전설 (주황색)
    }
}