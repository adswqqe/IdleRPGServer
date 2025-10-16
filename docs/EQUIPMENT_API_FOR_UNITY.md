# Equipment API Specification for Unity

> **Last Updated**: 2025-10-16  
> **Server Version**: Week 3 - Equipment System

## 📋 Overview

Equipment System API provides 7 endpoints for managing character equipment:
- Create equipment (Gacha/Drop)
- View equipped items (5 slots)
- View inventory (unequipped items)
- Equip/Unequip items
- Enhance equipment
- Delete equipment

---

## 🔑 Authentication

All Equipment endpoints require JWT Bearer token:
```
Authorization: Bearer {access_token}
```

---

## 📡 API Endpoints

### 1. Create Equipment (Gacha/Drop)

Creates a new equipment item for a character.

**Endpoint**: `POST /api/equipment/create`

**Request Body**:
```json
{
  "name": "Legend Sword",
  "slot": 1,
  "rarity": 5,
  "ownerId": "character-guid",
  "baseAttack": 100,
  "baseDefense": 0,
  "baseHp": 50
}
```

**Response** (200 OK):
```json
{
  "equipment": {
    "id": "equipment-guid",
    "name": "Legend Sword",
    "slot": 1,
    "rarity": 5,
    "ownerId": "character-guid",
    "characterId": null,
    "enhancementLevel": 0,
    "baseAttack": 100,
    "baseDefense": 0,
    "baseHp": 50,
    "totalAttack": 100,
    "totalDefense": 0,
    "totalHp": 50,
    "createdAt": "2025-10-16T12:00:00Z",
    "updatedAt": "2025-10-16T12:00:00Z"
  }
}
```

**Unity C# Example**:
```csharp
[System.Serializable]
public class CreateEquipmentRequest
{
    public string name;
    public int slot;           // 1=Weapon, 2=Helmet, 3=Armor, 4=Gloves, 5=Boots
    public int rarity;         // 1=Common, 2=Uncommon, 3=Rare, 4=Epic, 5=Legendary
    public string ownerId;
    public int baseAttack;
    public int baseDefense;
    public int baseHp;
}

IEnumerator CreateEquipment(CreateEquipmentRequest request)
{
    string json = JsonUtility.ToJson(request);
    using (UnityWebRequest www = UnityWebRequest.Post(
        "http://your-server.com/api/equipment/create", json, "application/json"))
    {
        www.SetRequestHeader("Authorization", "Bearer " + accessToken);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            EquipmentResponse response = JsonUtility.FromJson<EquipmentResponse>(www.downloadHandler.text);
            Debug.Log($"Created equipment: {response.equipment.name}");
        }
    }
}
```

---

### 2. Get Equipped Items

Retrieves all equipped items for a character (5 slots).

**Endpoint**: `GET /api/equipment/equipped/{characterId}`

**Response** (200 OK):
```json
{
  "equipments": [
    {
      "id": "equipment-guid",
      "name": "Legend Sword",
      "slot": 1,
      "rarity": 5,
      "ownerId": "character-guid",
      "characterId": "character-guid",
      "enhancementLevel": 3,
      "baseAttack": 100,
      "totalAttack": 115,
      "totalDefense": 0,
      "totalHp": 50,
      "createdAt": "2025-10-16T12:00:00Z",
      "updatedAt": "2025-10-16T12:00:00Z"
    }
  ]
}
```

**Unity C# Example**:
```csharp
IEnumerator GetEquippedItems(string characterId)
{
    using (UnityWebRequest www = UnityWebRequest.Get(
        $"http://your-server.com/api/equipment/equipped/{characterId}"))
    {
        www.SetRequestHeader("Authorization", "Bearer " + accessToken);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            EquipmentListResponse response = JsonUtility.FromJson<EquipmentListResponse>(www.downloadHandler.text);
            Debug.Log($"Equipped items: {response.equipments.Length}");
        }
    }
}
```

---

### 3. Get Inventory

Retrieves all unequipped items owned by a character.

**Endpoint**: `GET /api/equipment/inventory/{ownerId}`

**Response** (200 OK):
```json
{
  "equipments": [
    {
      "id": "equipment-guid",
      "name": "Common Shield",
      "slot": 3,
      "rarity": 1,
      "ownerId": "character-guid",
      "characterId": null,
      "enhancementLevel": 0,
      "baseAttack": 0,
      "baseDefense": 30,
      "baseHp": 100,
      "totalAttack": 0,
      "totalDefense": 30,
      "totalHp": 100,
      "createdAt": "2025-10-16T12:00:00Z",
      "updatedAt": "2025-10-16T12:00:00Z"
    }
  ]
}
```

---

### 4. Equip Item

Equips an item to a character. Automatically unequips existing item in the same slot.

**Endpoint**: `POST /api/equipment/equip`

**Request Body**:
```json
{
  "equipmentId": "equipment-guid",
  "characterId": "character-guid"
}
```

**Response** (200 OK):
```json
{
  "equipment": {
    "id": "equipment-guid",
    "name": "Legend Sword",
    "slot": 1,
    "characterId": "character-guid",
    "enhancementLevel": 0,
    "totalAttack": 100
  },
  "message": "장비가 장착되었습니다"
}
```

**Unity C# Example**:
```csharp
[System.Serializable]
public class EquipItemRequest
{
    public string equipmentId;
    public string characterId;
}

IEnumerator EquipItem(string equipmentId, string characterId)
{
    EquipItemRequest request = new EquipItemRequest {
        equipmentId = equipmentId,
        characterId = characterId
    };
    
    string json = JsonUtility.ToJson(request);
    using (UnityWebRequest www = UnityWebRequest.Post(
        "http://your-server.com/api/equipment/equip", json, "application/json"))
    {
        www.SetRequestHeader("Authorization", "Bearer " + accessToken);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Item equipped successfully!");
        }
    }
}
```

---

### 5. Unequip Item

Unequips an item from a character (moves to inventory).

**Endpoint**: `POST /api/equipment/unequip`

**Request Body**:
```json
{
  "equipmentId": "equipment-guid"
}
```

**Response** (200 OK):
```json
{
  "equipment": {
    "id": "equipment-guid",
    "characterId": null
  },
  "message": "장비가 해제되었습니다"
}
```

---

### 6. Enhance Equipment

Enhances equipment to increase stats (max level: +10).

**Endpoint**: `POST /api/equipment/enhance`

**Enhancement Bonus**:
- Attack: +5 per level
- Defense: +3 per level
- HP: +10 per level

**Request Body**:
```json
{
  "equipmentId": "equipment-guid"
}
```

**Response** (200 OK):
```json
{
  "equipment": {
    "id": "equipment-guid",
    "enhancementLevel": 3,
    "baseAttack": 100,
    "totalAttack": 115
  },
  "message": "장비가 +3로 강화되었습니다"
}
```

**Unity C# Example**:
```csharp
IEnumerator EnhanceEquipment(string equipmentId)
{
    string json = "{\"equipmentId\":\"" + equipmentId + "\"}";
    using (UnityWebRequest www = UnityWebRequest.Post(
        "http://your-server.com/api/equipment/enhance", json, "application/json"))
    {
        www.SetRequestHeader("Authorization", "Bearer " + accessToken);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            EnhanceResponse response = JsonUtility.FromJson<EnhanceResponse>(www.downloadHandler.text);
            Debug.Log($"Enhanced to +{response.equipment.enhancementLevel}");
        }
    }
}
```

---

### 7. Delete Equipment

Deletes an equipment item permanently.

**Endpoint**: `DELETE /api/equipment/{equipmentId}`

**Response** (200 OK):
```json
{
  "message": "장비가 삭제되었습니다"
}
```

---

## 📊 Enums Reference

### EquipmentSlot
```csharp
public enum EquipmentSlot
{
    Weapon = 1,   // 무기
    Helmet = 2,   // 투구
    Armor = 3,    // 갑옷
    Gloves = 4,   // 장갑
    Boots = 5     // 신발
}
```

### EquipmentRarity
```csharp
public enum EquipmentRarity
{
    Common = 1,      // 일반 (회색)
    Uncommon = 2,    // 고급 (초록)
    Rare = 3,        // 희귀 (파랑)
    Epic = 4,        // 영웅 (보라)
    Legendary = 5    // 전설 (주황)
}
```

---

## 🎮 Unity Integration Example

### Complete Equipment Manager
```csharp
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class EquipmentManager : MonoBehaviour
{
    private string serverUrl = "http://your-server.com";
    private string accessToken;

    [System.Serializable]
    public class Equipment
    {
        public string id;
        public string name;
        public int slot;
        public int rarity;
        public string ownerId;
        public string characterId;
        public int enhancementLevel;
        public int baseAttack;
        public int baseDefense;
        public int baseHp;
        public int totalAttack;
        public int totalDefense;
        public int totalHp;
    }

    [System.Serializable]
    public class EquipmentResponse
    {
        public Equipment equipment;
        public string message;
    }

    [System.Serializable]
    public class EquipmentListResponse
    {
        public Equipment[] equipments;
    }

    public IEnumerator GetEquippedItems(string characterId)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(
            $"{serverUrl}/api/equipment/equipped/{characterId}"))
        {
            www.SetRequestHeader("Authorization", "Bearer " + accessToken);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                EquipmentListResponse response = 
                    JsonUtility.FromJson<EquipmentListResponse>(www.downloadHandler.text);
                
                foreach (var equipment in response.equipments)
                {
                    Debug.Log($"Slot {equipment.slot}: {equipment.name} +{equipment.enhancementLevel}");
                }
            }
        }
    }

    public IEnumerator EquipItem(string equipmentId, string characterId)
    {
        string json = $"{{\"equipmentId\":\"{equipmentId}\",\"characterId\":\"{characterId}\"}}";
        
        using (UnityWebRequest www = UnityWebRequest.Post(
            $"{serverUrl}/api/equipment/equip", json, "application/json"))
        {
            www.SetRequestHeader("Authorization", "Bearer " + accessToken);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                EquipmentResponse response = 
                    JsonUtility.FromJson<EquipmentResponse>(www.downloadHandler.text);
                Debug.Log(response.message);
            }
        }
    }
}
```

---

## ⚠️ Error Responses

**400 Bad Request** - Invalid input or business logic error:
```json
{
  "message": "다른 캐릭터의 장비는 장착할 수 없습니다"
}
```

**404 Not Found** - Equipment not found:
```json
{
  "message": "장비를 찾을 수 없습니다"
}
```

**500 Internal Server Error** - Server error:
```json
{
  "message": "서버 오류가 발생했습니다"
}
```

---

## 📌 Important Notes

1. **OwnerId vs CharacterId**:
   - `OwnerId`: Who owns the equipment (set when created)
   - `CharacterId`: Who has it equipped (null = in inventory)

2. **Auto-Swap Logic**:
   - When equipping an item, any existing item in the same slot is automatically unequipped

3. **Enhancement**:
   - Max level: +10
   - Enhancement bonus applies to all stats
   - Stats calculation: `totalStat = baseStat + (enhancementLevel * bonus)`

4. **Inventory Management**:
   - After 10-pull gacha, all items go to inventory
   - Items persist even after logout
   - Use `GetInventory` to show all unequipped items

---

**Next**: Copy this file to `IdleRPGClient/Docs/unity/Equipment_API.md`
