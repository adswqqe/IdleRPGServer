# Unity Client Documentation Guide

## Policy

**CRITICAL**: When adding or modifying any API endpoint or DTO, the corresponding Unity client documentation **MUST** be updated.

This ensures Unity developers always have up-to-date integration guides.

---

## Documentation Structure

```
../IdleRPGClient/Docs/
├── README.md                              ← Documentation index
├── Unity-DTOs.cs                          ← Unity C# DTO classes (copy-paste ready)
├── unity/                                 ← Unity client documentation
│   ├── API_SPEC_FOR_UNITY.md             ← Server API specification for Unity
│   ├── Unity-API-Reference.md            ← Detailed API reference with examples
│   ├── Unity-Quick-Reference.md          ← Quick lookup tables and examples
│   └── UNITY_PROJECT_CONTEXT.md          ← Unity project context and settings
└── server/                                ← Server integration documentation
    └── SERVER_DEPLOYMENT.md               ← Server deployment guide
```

---

## Required Updates Checklist

### When Creating a New API Endpoint

- [ ] **1. Update `unity/API_SPEC_FOR_UNITY.md`**
  - Endpoint path and HTTP method
  - Request/Response JSON examples
  - Authentication requirements
  - Unity C# usage example with UnityWebRequest

- [ ] **2. Update `Unity-DTOs.cs`** (root level)
  - Add corresponding DTO classes
  - Include `[Serializable]` attribute
  - Add XML comments

- [ ] **3. Update `unity/Unity-Quick-Reference.md`**
  - Add endpoint to summary table
  - Include quick example

- [ ] **4. (Optional) Update `unity/Unity-API-Reference.md`**
  - Only if detailed reference is needed

### When Adding a New DTO

- [ ] **1. Create in `IdleRPG.Application/DTOs/`** (server)
- [ ] **2. Add C# `[Serializable]` version to `Unity-DTOs.cs`**
- [ ] **3. Document all properties with XML comments**
- [ ] **4. Add usage example in code comments**

### When Modifying Existing Endpoints or DTOs

- [ ] **1. Update all relevant documentation files simultaneously**
- [ ] **2. Mark breaking changes with ⚠️ BREAKING CHANGE**
- [ ] **3. Update version date at bottom of markdown files**
- [ ] **4. Consider updating `unity/UNITY_PROJECT_CONTEXT.md` if project structure changes**

---

## Example: Creating a New Endpoint

### Step 1: Add to API_SPEC_FOR_UNITY.md

```markdown
## 6.1 Get Equipment Inventory

**Endpoint**: `GET /api/equipment/inventory/{ownerId}`

**Description**: Gets all unequipped items owned by the player.

**Authentication**: Required (JWT Bearer Token)

**Request:**
```http
GET https://your-server.com/api/equipment/inventory/550e8400-e29b-41d4-a716-446655440000
Authorization: Bearer YOUR_ACCESS_TOKEN
```

**Response (200 OK):**
```json
{
  "response": {
    "equipments": [
      {
        "id": "550e8400-e29b-41d4-a716-446655440001",
        "name": "Iron Sword",
        "slot": 1,
        "rarity": 2,
        "ownerId": "550e8400-e29b-41d4-a716-446655440000",
        "characterId": null,
        "enhancementLevel": 0,
        "baseAttack": 50,
        "baseDefense": 0,
        "baseHp": 0,
        "totalAttack": 50,
        "totalDefense": 0,
        "totalHp": 0
      }
    ]
  }
}
```

**Unity C# Example:**
```csharp
IEnumerator GetInventory(string ownerId)
{
    string url = $"{baseUrl}/api/equipment/inventory/{ownerId}";
    using (UnityWebRequest request = UnityWebRequest.Get(url))
    {
        request.SetRequestHeader("Authorization", $"Bearer {accessToken}");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            var response = JsonUtility.FromJson<EquipmentListResponse>(request.downloadHandler.text);
            // Process response.equipments
        }
    }
}
```
```

### Step 2: Add DTO to Unity-DTOs.cs

```csharp
/// <summary>
/// Equipment list response wrapper
/// </summary>
[Serializable]
public class EquipmentListResponse
{
    public EquipmentDto[] equipments;
}

/// <summary>
/// Equipment information DTO
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
    public int enhancementLevel;
    public int baseAttack;
    public int totalAttack;       // Calculated on server
    // ... other fields
}
```

### Step 3: Update Unity-Quick-Reference.md

Add to endpoint table:

```markdown
| `/api/equipment/inventory/{ownerId}` | GET | ✅ | Get unequipped items |
```

---

## File Location Reference

### Server Documentation Files (This Repo)

Located in `docs/unity/`:
- `API_SPEC_FOR_UNITY.md` - Created here, then copied to client
- `EQUIPMENT_API_FOR_UNITY.md` - Created here, then copied to client

### Unity Client Documentation Files (IdleRPGClient Repo)

Located in `../IdleRPGClient/Docs/`:
- All documentation is consumed by Unity developers here
- This is the **source of truth** for Unity integration

### Synchronization

When you update server docs, you need to:

1. **Create/Update in server repo**: `docs/unity/`
2. **Copy to Unity client repo**: `../IdleRPGClient/Docs/unity/`
3. **Update Unity README**: `../IdleRPGClient/Docs/unity/README.md`

---

## Breaking Changes Protocol

When making breaking changes:

1. **Mark clearly in docs**:
   ```markdown
   ## ⚠️ BREAKING CHANGE (v2.0)

   The `/api/character/stats` endpoint has been removed.
   Use `/api/character/{id}` instead.
   ```

2. **Update version in README**:
   ```markdown
   > **Server Version**: v2.0
   > **Last Updated**: 2025-10-17
   ```

3. **Notify Unity developers** (if working with team)

---

## Validation

Before completing any API work, verify:

```bash
# Check Unity docs are updated
ls -la ../IdleRPGClient/Docs/unity/

# Verify endpoint documented
grep -r "your-endpoint-path" ../IdleRPGClient/Docs/

# Verify DTO added
grep -r "YourDtoClass" ../IdleRPGClient/Docs/Unity-DTOs.cs
```

---

**Last Updated**: 2025-10-17
