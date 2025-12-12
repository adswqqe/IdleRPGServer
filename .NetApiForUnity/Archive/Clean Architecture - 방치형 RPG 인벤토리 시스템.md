 [[🎮 방치형 RPG 서버 개발 완전 가이드 - Week 1]]# Clean Architecture - 방치형 RPG 인벤토리 시스템

## 🎒 PlayerInventory 클래스 구현

방치형 RPG에서 **플레이어가 실제로 소유한 아이템들**을 관리하는 클래스입니다.

### Unity 인벤토리 vs 서버 인벤토리 비교

#### Unity에서의 인벤토리 (클라이언트)
```csharp
[System.Serializable]
public class InventorySlot
{
    public ItemData itemData;
    public int quantity;
    public int slotIndex;
}

public class PlayerInventory : MonoBehaviour
{
    public List<InventorySlot> inventorySlots = new List<InventorySlot>(50);
    
    public void AddItem(ItemData item, int quantity)
    {
        // 클라이언트에서 인벤토리 관리
    }
}
```

#### 서버에서의 인벤토리 (Domain Entity)
```csharp
public class PlayerInventory
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid PlayerId { get; private set; }
    public Player Player { get; set; } // Navigation Property
    
    public int ItemTemplateId { get; private set; }  // 아이템 종류 (FK)
    public ItemTemplate ItemTemplate { get; set; }   // Navigation Property
    
    public int Quantity { get; private set; } = 1;
    public int SlotIndex { get; private set; }
    public int EnhancementLevel { get; private set; } = 0;
    
    // 개별 아이템 속성
    public Dictionary<string, int> CustomStats { get; private set; } = new();
    public DateTime AcquiredAt { get; private set; } = DateTime.UtcNow;
    public bool IsEquipped { get; private set; } = false;
    
    // 생성자
    public PlayerInventory(Guid playerId, int itemTemplateId, int quantity = 1, int slotIndex = -1)
    {
        PlayerId = playerId;
        ItemTemplateId = itemTemplateId;
        Quantity = quantity;
        SlotIndex = slotIndex;
    }
    
    // 도메인 로직: 아이템 수량 증가
    public void AddQuantity(int amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive");
            
        Quantity += amount;
    }
    
    // 도메인 로직: 아이템 수량 감소
    public bool TryRemoveQuantity(int amount)
    {
        if (amount <= 0 || amount > Quantity)
            return false;
            
        Quantity -= amount;
        return true;
    }
    
    // 도메인 로직: 아이템 강화
    public void Enhance()
    {
        if (ItemTemplate?.Type == ItemType.Consumable)
            throw new InvalidOperationException("Cannot enhance consumable items");
            
        EnhancementLevel++;
        RecalculateStats();
    }
    
    // 도메인 로직: 장비 착용/해제
    public void SetEquipped(bool equipped)
    {
        if (ItemTemplate?.Type == ItemType.Consumable && equipped)
            throw new InvalidOperationException("Cannot equip consumable items");
            
        IsEquipped = equipped;
    }
    
    // 도메인 로직: 슬롯 이동
    public void MoveToSlot(int newSlotIndex)
    {
        if (newSlotIndex < 0)
            throw new ArgumentException("Slot index cannot be negative");
            
        SlotIndex = newSlotIndex;
    }
    
    // 도메인 로직: 강화에 따른 스탯 재계산
    private void RecalculateStats()
    {
        CustomStats.Clear();
        
        if (ItemTemplate?.BaseStats != null)
        {
            foreach (var baseStat in ItemTemplate.BaseStats)
            {
                var enhancedValue = baseStat.Value + (baseStat.Value * EnhancementLevel * 0.1f);
                CustomStats[baseStat.Key] = (int)enhancedValue;
            }
        }
    }
    
    // 도메인 로직: 아이템 판매 가격 계산
    public long CalculateSellPrice()
    {
        if (ItemTemplate == null) return 0;
        
        var basePrice = ItemTemplate.SellPrice;
        var enhancementBonus = EnhancementLevel * basePrice * 0.5f;
        
        return (long)(basePrice + enhancementBonus);
    }
}
```

## 🛡️ ItemTemplate 클래스 (완전 구현)

```csharp
public class ItemTemplate
{
    public int Id { get; private set; }  // int ID (템플릿은 관리 편의성)
    public string Name { get; private set; }
    public string Description { get; private set; }
    public ItemType Type { get; private set; }
    public ItemRarity Rarity { get; private set; }
    
    public Dictionary<string, int> BaseStats { get; private set; } = new();
    public int MaxStackSize { get; private set; } = 1;
    public long SellPrice { get; private set; }
    public int RequiredLevel { get; private set; } = 1;
    
    // 드롭 관련 정보
    public float DropRate { get; private set; } = 0.1f;
    public List<string> DropSources { get; private set; } = new();
    
    public ItemTemplate(int id, string name, ItemType type, ItemRarity rarity)
    {
        Id = id;
        Name = name;
        Type = type;
        Rarity = rarity;
        
        // 타입별 기본값 설정
        MaxStackSize = type == ItemType.Consumable ? 999 : 1;
        SellPrice = CalculateBaseSellPrice();
    }
    
    // 도메인 로직: 기본 판매가 계산
    private long CalculateBaseSellPrice()
    {
        var basePrice = Type switch
        {
            ItemType.Weapon => 100,
            ItemType.Armor => 80,
            ItemType.Accessory => 50,
            ItemType.Consumable => 10,
            _ => 20
        };
        
        var rarityMultiplier = Rarity switch
        {
            ItemRarity.Common => 1,
            ItemRarity.Rare => 3,
            ItemRarity.Epic => 10,
            ItemRarity.Legendary => 50,
            _ => 1
        };
        
        return basePrice * rarityMultiplier;
    }
    
    // 도메인 로직: 스탯 추가
    public void AddBaseStat(string statName, int value)
    {
        BaseStats[statName] = value;
    }
    
    // 도메인 로직: 드롭 소스 추가
    public void AddDropSource(string source)
    {
        if (!DropSources.Contains(source))
        {
            DropSources.Add(source);
        }
    }
}

// Enum들
public enum ItemType
{
    Weapon,
    Armor,
    Accessory,
    Consumable
}

public enum ItemRarity
{
    Common,
    Rare,
    Epic,
    Legendary
}
```

## 🎁 OfflineReward 클래스 (완전 구현)

```csharp
public class OfflineReward
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid PlayerId { get; private set; }
    public Player Player { get; set; } // Navigation Property
    
    public DateTime OfflineStart { get; private set; }
    public DateTime OfflineEnd { get; private set; }
    
    // 보상 내역
    public long CalculatedGold { get; private set; }
    public long CalculatedExperience { get; private set; }
    public Dictionary<int, int> CalculatedItems { get; private set; } = new(); // ItemTemplateId, Quantity
    
    public decimal BonusMultiplier { get; private set; } = 1.0m;
    public bool IsClaimed { get; private set; } = false;
    public DateTime? ClaimedAt { get; private set; }
    
    public OfflineReward(Guid playerId, DateTime offlineStart, DateTime offlineEnd, int playerLevel)
    {
        PlayerId = playerId;
        OfflineStart = offlineStart;
        OfflineEnd = offlineEnd;
        
        CalculateRewards(playerLevel);
    }
    
    // 도메인 로직: 오프라인 보상 계산 (방치형 게임 핵심!)
    private void CalculateRewards(int playerLevel)
    {
        var offlineTime = OfflineEnd - OfflineStart;
        
        // 최대 12시간까지만 보상 (방치형 게임 밸런싱)
        var rewardHours = Math.Min(offlineTime.TotalHours, 12.0);
        
        // 레벨에 비례한 시간당 보상
        var hourlyGold = playerLevel * 100;
        var hourlyExp = playerLevel * 50;
        
        CalculatedGold = (long)(rewardHours * hourlyGold);
        CalculatedExperience = (long)(rewardHours * hourlyExp);
        
        // 아이템 보상 계산 (확률 기반)
        CalculateItemRewards(playerLevel, rewardHours);
    }
    
    // 도메인 로직: 아이템 보상 계산
    private void CalculateItemRewards(int playerLevel, double hours)
    {
        var random = new Random();
        
        // 시간당 30% 확률로 아이템 드롭
        var itemDropChance = hours * 0.3;
        var itemDropCount = (int)itemDropChance + (random.NextDouble() < (itemDropChance % 1) ? 1 : 0);
        
        for (int i = 0; i < itemDropCount; i++)
        {
            // 레벨에 따른 아이템 등급 결정
            var itemId = DetermineRandomItem(playerLevel, random);
            if (itemId > 0)
            {
                CalculatedItems[itemId] = CalculatedItems.GetValueOrDefault(itemId) + 1;
            }
        }
    }
    
    // 도메인 로직: 랜덤 아이템 결정
    private int DetermineRandomItem(int playerLevel, Random random)
    {
        // 기본 아이템 풀 (실제로는 ItemTemplate에서 가져와야 함)
        var itemPool = new List<(int ItemId, float Weight)>
        {
            (1001, 0.5f),  // 체력 포션 (Common)
            (1002, 0.3f),  // 마나 포션 (Common)
            (2001, 0.15f), // 철 검 (Rare)
            (2002, 0.04f), // 미스릴 갑옷 (Epic)
            (2003, 0.01f)  // 전설의 반지 (Legendary)
        };
        
        var totalWeight = itemPool.Sum(item => item.Weight);
        var randomValue = random.NextDouble() * totalWeight;
        
        float currentWeight = 0;
        foreach (var (itemId, weight) in itemPool)
        {
            currentWeight += weight;
            if (randomValue <= currentWeight)
            {
                return itemId;
            }
        }
        
        return 0; // 아이템 없음
    }
    
    // 도메인 로직: 보너스 적용
    public void ApplyBonus(decimal multiplier)
    {
        if (IsClaimed)
            throw new InvalidOperationException("Cannot apply bonus to claimed reward");
            
        BonusMultiplier = multiplier;
        
        // 보상 재계산
        CalculatedGold = (long)(CalculatedGold * multiplier);
        CalculatedExperience = (long)(CalculatedExperience * multiplier);
    }
    
    // 도메인 로직: 보상 수령
    public void Claim()
    {
        if (IsClaimed)
            throw new InvalidOperationException("Reward already claimed");
            
        IsClaimed = true;
        ClaimedAt = DateTime.UtcNow;
    }
    
    // 도메인 로직: 보상 만료 확인 (7일 후 만료)
    public bool IsExpired()
    {
        return DateTime.UtcNow - OfflineEnd > TimeSpan.FromDays(7);
    }
    
    // 도메인 로직: 총 보상 가치 계산 (표시용)
    public long CalculateTotalValue()
    {
        var totalValue = CalculatedGold;
        
        // 경험치는 골드 가치로 환산 (경험치 1 = 골드 0.1)
        totalValue += (long)(CalculatedExperience * 0.1);
        
        // 아이템 가치 합산 (ItemTemplate에서 가격 정보 필요)
        foreach (var (itemId, quantity) in CalculatedItems)
        {
            // 실제로는 ItemTemplate에서 가격 조회
            var estimatedPrice = itemId switch
            {
                1001 => 10,  // 체력 포션
                1002 => 15,  // 마나 포션
                2001 => 100, // 철 검
                2002 => 500, // 미스릴 갑옷
                2003 => 5000, // 전설의 반지
                _ => 1
            };
            totalValue += estimatedPrice * quantity;
        }
        
        return totalValue;
    }
}
```

## 🔗 Entity Framework 관계 설정

```csharp
// GameDbContext.cs에서 관계 설정
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Player ↔ PlayerInventory (1:N 관계)
    modelBuilder.Entity<PlayerInventory>(entity =>
    {
        entity.HasKey(pi => pi.Id);
        entity.HasIndex(pi => pi.PlayerId);
        entity.HasIndex(pi => new { pi.PlayerId, pi.SlotIndex }).IsUnique(); // 복합 인덱스
        
        entity.HasOne(pi => pi.Player)
              .WithMany(p => p.Inventory)
              .HasForeignKey(pi => pi.PlayerId)
              .OnDelete(DeleteBehavior.Cascade);
              
        entity.HasOne(pi => pi.ItemTemplate)
              .WithMany()
              .HasForeignKey(pi => pi.ItemTemplateId)
              .OnDelete(DeleteBehavior.Restrict);
              
        // JSON 컬럼으로 CustomStats 저장
        entity.Property(pi => pi.CustomStats)
              .HasConversion(
                  v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                  v => JsonSerializer.Deserialize<Dictionary<string, int>>(v, (JsonSerializerOptions)null));
    });
    
    // ItemTemplate 설정
    modelBuilder.Entity<ItemTemplate>(entity =>
    {
        entity.HasKey(it => it.Id);
        entity.HasIndex(it => it.Name).IsUnique();
        entity.HasIndex(it => it.Type);
        entity.HasIndex(it => it.Rarity);
        
        // JSON 컬럼으로 BaseStats 저장
        entity.Property(it => it.BaseStats)
              .HasConversion(
                  v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                  v => JsonSerializer.Deserialize<Dictionary<string, int>>(v, (JsonSerializerOptions)null));
                  
        entity.Property(it => it.DropSources)
              .HasConversion(
                  v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                  v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null));
    });
    
    // OfflineReward 설정
    modelBuilder.Entity<OfflineReward>(entity =>
    {
        entity.HasKey(or => or.Id);
        entity.HasIndex(or => or.PlayerId);
        entity.HasIndex(or => new { or.PlayerId, or.IsClaimed });
        
        entity.HasOne(or => or.Player)
              .WithMany(p => p.OfflineRewards)
              .HasForeignKey(or => or.PlayerId)
              .OnDelete(DeleteBehavior.Cascade);
              
        // JSON 컬럼으로 CalculatedItems 저장
        entity.Property(or => or.CalculatedItems)
              .HasConversion(
                  v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                  v => JsonSerializer.Deserialize<Dictionary<int, int>>(v, (JsonSerializerOptions)null));
                  
        entity.Property(or => or.BonusMultiplier)
              .HasPrecision(5, 2);
    });
}
```

## 📋 데이터 시딩 예시

```csharp
private void SeedItemTemplates(ModelBuilder modelBuilder)
{
    var items = new List<ItemTemplate>
    {
        // 소모품
        new ItemTemplate(1001, "체력 포션", ItemType.Consumable, ItemRarity.Common),
        new ItemTemplate(1002, "마나 포션", ItemType.Consumable, ItemRarity.Common),
        
        // 무기
        new ItemTemplate(2001, "나무 검", ItemType.Weapon, ItemRarity.Common),
        new ItemTemplate(2002, "철 검", ItemType.Weapon, ItemRarity.Rare),
        new ItemTemplate(2003, "미스릴 검", ItemType.Weapon, ItemRarity.Epic),
        
        // 방어구
        new ItemTemplate(3001, "가죽 갑옷", ItemType.Armor, ItemRarity.Common),
        new ItemTemplate(3002, "철 갑옷", ItemType.Armor, ItemRarity.Rare),
        
        // 악세서리
        new ItemTemplate(4001, "힘의 반지", ItemType.Accessory, ItemRarity.Epic),
        new ItemTemplate(4002, "전설의 목걸이", ItemType.Accessory, ItemRarity.Legendary)
    };
    
    // 스탯 설정
    items[2].AddBaseStat("Attack", 10);      // 나무 검
    items[3].AddBaseStat("Attack", 25);      // 철 검
    items[4].AddBaseStat("Attack", 50);      // 미스릴 검
    
    items[5].AddBaseStat("Defense", 15);     // 가죽 갑옷
    items[6].AddBaseStat("Defense", 35);     // 철 갑옷
    
    items[7].AddBaseStat("Strength", 10);    // 힘의 반지
    items[8].AddBaseStat("AllStats", 20);    // 전설의 목걸이
    
    modelBuilder.Entity<ItemTemplate>().HasData(items);
}
```

## 🎯 Unity 개발자를 위한 요약

이제 **완전한 인벤토리 시스템**이 구성되었습니다:

1. **ItemTemplate**: 아이템의 기본 정보 (Unity의 ScriptableObject)
2. **PlayerInventory**: 플레이어가 소유한 개별 아이템 (Unity의 InventorySlot)
3. **OfflineReward**: 방치형 게임의 핵심 보상 시스템

**Unity 인벤토리와의 주요 차이점**:
- **서버**: 모든 아이템이 데이터베이스에 영구 저장
- **클라이언트**: 세션 동안만 메모리에 존재
- **보안**: 서버에서 모든 아이템 조작 검증

이제 **Day 1-2 Entity Framework 실습**에서 이 클래스들을 실제로 구현해볼 수 있습니다! 🚀

## 🔗 관련 문서
- [[Clean Architecture - Domain Layer 상세]]
- [[Clean Architecture - Infrastructure Layer 상세]]  
- [[Week 1 Day 0 - 환경구축 완료]]

*작성일: 2025년 9월 26일*