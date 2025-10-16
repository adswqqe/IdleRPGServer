// Unity C# DTOs for Equipment System
// Copy this file to your Unity project's Scripts/DTOs folder

using System;

namespace IdleRPG.Client.DTOs
{
    #region Equipment Enums
    
    /// <summary>
    /// 장비 장착 부위 (5개 슬롯)
    /// </summary>
    public enum EquipmentSlot
    {
        Weapon = 1,   // 무기
        Helmet = 2,   // 투구
        Armor = 3,    // 갑옷
        Gloves = 4,   // 장갑
        Boots = 5     // 신발
    }

    /// <summary>
    /// 장비 등급 (버섯키우기 스타일)
    /// </summary>
    public enum EquipmentRarity
    {
        Common = 1,      // 일반 (회색)
        Uncommon = 2,    // 고급 (초록)
        Rare = 3,        // 희귀 (파랑)
        Epic = 4,        // 영웅 (보라)
        Legendary = 5    // 전설 (주황)
    }
    
    #endregion

    #region Equipment DTOs

    /// <summary>
    /// 장비 정보 DTO
    /// </summary>
    [Serializable]
    public class EquipmentDto
    {
        public string id;
        public string name;
        public int slot;              // EquipmentSlot enum as int
        public int rarity;            // EquipmentRarity enum as int
        public string ownerId;
        public string characterId;    // null = in inventory
        public int enhancementLevel;  // 0 ~ 10
        public int baseAttack;
        public int baseDefense;
        public int baseHp;
        
        // Calculated stats (base + enhancement bonus)
        public int totalAttack;
        public int totalDefense;
        public int totalHp;
        
        public string createdAt;
        public string updatedAt;

        /// <summary>
        /// Returns user-friendly slot name
        /// </summary>
        public string GetSlotName()
        {
            return ((EquipmentSlot)slot).ToString();
        }

        /// <summary>
        /// Returns user-friendly rarity name
        /// </summary>
        public string GetRarityName()
        {
            return ((EquipmentRarity)rarity).ToString();
        }

        /// <summary>
        /// Checks if equipment is equipped
        /// </summary>
        public bool IsEquipped()
        {
            return !string.IsNullOrEmpty(characterId);
        }
    }

    /// <summary>
    /// 장비 생성 요청 DTO (가챠, 드랍 등)
    /// </summary>
    [Serializable]
    public class CreateEquipmentRequest
    {
        public string name;
        public int slot;              // 1=Weapon, 2=Helmet, 3=Armor, 4=Gloves, 5=Boots
        public int rarity;            // 1=Common, 2=Uncommon, 3=Rare, 4=Epic, 5=Legendary
        public string ownerId;
        public int baseAttack;
        public int baseDefense;
        public int baseHp;
    }

    /// <summary>
    /// 장비 장착 요청 DTO
    /// </summary>
    [Serializable]
    public class EquipItemRequest
    {
        public string equipmentId;
        public string characterId;
    }

    /// <summary>
    /// 장비 해제 요청 DTO
    /// </summary>
    [Serializable]
    public class UnequipItemRequest
    {
        public string equipmentId;
    }

    /// <summary>
    /// 장비 강화 요청 DTO
    /// </summary>
    [Serializable]
    public class EnhanceEquipmentRequest
    {
        public string equipmentId;
    }

    #endregion

    #region API Response Wrappers

    /// <summary>
    /// Single equipment response wrapper
    /// </summary>
    [Serializable]
    public class EquipmentResponse
    {
        public EquipmentDto equipment;
        public string message;
    }

    /// <summary>
    /// Equipment list response wrapper
    /// </summary>
    [Serializable]
    public class EquipmentListResponse
    {
        public EquipmentDto[] equipments;
    }

    #endregion
}
