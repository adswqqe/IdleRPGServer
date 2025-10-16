namespace IdleRPG.Domain.Enums
{
    /// <summary>
    /// 장비 슬롯 종류 (버섯키우기 스타일)
    /// </summary>
    public enum EquipmentSlot
    {
        /// <summary>
        /// 무기 (공격력 주 스탯)
        /// </summary>
        Weapon = 1,

        /// <summary>
        /// 방어구 (방어력 주 스탯)
        /// </summary>
        Armor = 2,

        /// <summary>
        /// 헬멧 (체력 주 스탯)
        /// </summary>
        Helmet = 3,

        /// <summary>
        /// 장갑 (크리티컬 주 스탯)
        /// </summary>
        Gloves = 4,

        /// <summary>
        /// 신발 (공격속도/회피 주 스탯)
        /// </summary>
        Boots = 5
    }
}
