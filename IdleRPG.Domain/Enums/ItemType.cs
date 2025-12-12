namespace IdleRPG.Domain.Enums
{
    /// <summary>
    /// ItemTemplate의 종류를 정의합니다.
    ///
    /// 중첩 가능한(stackable) 아이템을 기능별로 분류하여 게임 로직에서 타입별 처리를 가능하게 합니다.
    /// 예: Consumable은 사용 가능, Material은 제작 재료로만 사용 가능
    /// </summary>
    public enum ItemType
    {
        /// <summary>
        /// 사용 시 효과가 발동하는 아이템 (예: 물약)
        /// </summary>
        Consumable,

        /// <summary>
        /// 제작이나 강화에 사용되는 아이템 (예: 강화석)
        /// </summary>
        Material,

        /// <summary>
        /// 퀘스트 진행에 필요한 아이템
        /// </summary>
        QuestItem
    }
}
