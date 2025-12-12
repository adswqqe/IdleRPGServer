namespace IdleRPG.Domain.Enums
{
    /// <summary>
    /// 보상의 종류를 정의합니다.
    ///
    /// 이 enum은 Loot Table Pattern의 핵심 구성 요소로, LootItem이 어떤 타입의 보상인지 구분합니다.
    /// string이나 int 같은 매직 넘버 대신 enum을 사용하여 타입 안전성을 확보하고 코드 가독성을 높입니다.
    /// </summary>
    public enum RewardType
    {
        /// <summary>
        /// 게임 내 재화
        /// LootItem.ItemId는 null이며, MinQuantity/MaxQuantity로 지급량 결정
        /// </summary>
        Gold,

        /// <summary>
        /// 캐릭터 경험치
        /// LootItem.ItemId는 null이며, MinQuantity/MaxQuantity로 지급량 결정
        /// </summary>
        Experience,

        /// <summary>
        /// ItemTemplate에 정의된 아이템 (소모품, 재료 등)
        /// LootItem.ItemId는 ItemTemplate.Id를 참조
        /// Master-Instance 패턴: ItemTemplate(Master) → PlayerItem(Instance)
        /// </summary>
        Item,

        /// <summary>
        /// 동적으로 생성되는 장비 아이템
        /// LootItem.ItemId는 null이며, 획득 시점에 Equipment 엔티티가 동적 생성됨
        /// 레벨, 옵션 등은 획득 시점의 난이도/컨텍스트에 따라 결정
        /// </summary>
        Equipment,

        /// <summary>
        /// SkillTemplate에 정의된 스킬
        /// LootItem.ItemId는 SkillTemplate.Id를 참조
        /// 중복 획득 시 스킬 레벨업 또는 다른 보상으로 변환
        /// </summary>
        Skill
    }
}
