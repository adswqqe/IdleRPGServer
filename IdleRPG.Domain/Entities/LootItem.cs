using IdleRPG.Domain.Enums;
using System;

namespace IdleRPG.Domain.Entities
{
    /// <summary>
    /// LootTable에 포함되는 개별 보상 항목을 정의합니다.
    /// Loot Table Pattern의 상세한 설계 의도는 LootTable.cs를 참조하세요.
    /// </summary>
    public class LootItem
    {
        public int Id { get; set; }

        /// <summary>
        /// 부모가 되는 LootTable의 FK
        /// </summary>
        public int LootTableId { get; set; }

        /// <summary>
        /// 보상 종류 (재화, 경험치, 아이템, 장비 등)
        /// </summary>
        public RewardType Type { get; set; }

        /// <summary>
        /// 보상 아이템의 템플릿 ID (Type이 Item일 경우).
        /// Gold, Experience, Equipment 타입의 경우 null입니다.
        /// </summary>
        public Guid? ItemId { get; set; }

        /// <summary>
        /// 100% 지급되는 보상인지 여부. true이면 Weight는 무시됩니다.
        /// </summary>
        public bool IsGuaranteed { get; set; } = false;

        /// <summary>
        /// 확률 드랍 시 사용되는 가중치. IsGuaranteed가 false일 때만 의미가 있습니다.
        /// 값이 높을수록 선택될 확률이 높아집니다.
        /// 기본값 없음 - 기획자가 의식적으로 결정해야 합니다.
        /// </summary>
        public int Weight { get; set; }

        /// <summary>
        /// 보상 획득 시 지급될 최소 수량
        /// </summary>
        public int MinQuantity { get; set; } = 1;

        /// <summary>
        /// 보상 획득 시 지급될 최대 수량
        /// </summary>
        public int MaxQuantity { get; set; } = 1;

        // Navigation Property
        /// <summary>
        /// 부모 LootTable
        /// </summary>
        public virtual LootTable LootTable { get; set; } = null!;
    }
}
