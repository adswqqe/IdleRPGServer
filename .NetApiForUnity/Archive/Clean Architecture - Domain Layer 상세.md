 [[🎮 방치형 RPG 서버 개발 완전 가이드 - Week 1]]# Clean Architecture - Domain Layer 상세

## 🎯 Domain Layer란?
**게임의 핵심 비즈니스 로직**을 담당하는 가장 중요한 레이어입니다. Unity의 **GameObject/MonoBehaviour**와 같은 역할을 합니다.

## 🔑 핵심 특징
- **외부 의존성 없음**: 다른 레이어나 프레임워크에 의존하지 않음
- **순수한 C# 코드**: 비즈니스 규칙만 포함
- **테스트 용이성**: 독립적으로 유닛 테스트 가능
- **재사용 가능**: 다른 프로젝트에서도 활용 가능

## 🎮 방치형 RPG에서의 Domain 엔티티들

### 1. Player 엔티티
```csharp
public class Player
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Username { get; private set; }
    public string Email { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime LastLogin { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    
    // 생성자 (비즈니스 규칙 적용)
    public Player(string username, string email)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be empty");
        if (!IsValidEmail(email))
            throw new ArgumentException("Invalid email format");
            
        Username = username;
        Email = email;
    }
    
    // 도메인 로직 메소드
    public TimeSpan GetOfflineTime()
    {
        return DateTime.UtcNow - LastLogin;
    }
    
    public void UpdateLastLogin()
    {
        LastLogin = DateTime.UtcNow;
    }
    
    private bool IsValidEmail(string email)
    {
        return email.Contains("@") && email.Contains(".");
    }
}
```

### 2. Character 엔티티 (방치형 RPG 핵심)
```csharp
public class Character
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid PlayerId { get; private set; }
    public string Name { get; private set; }
    public CharacterClass Class { get; private set; }
    public int Level { get; private set; } = 1;
    public long Experience { get; private set; } = 0;
    
    // 캐릭터 스탯
    public int Health { get; private set; }
    public int Mana { get; private set; }
    public int Attack { get; private set; }
    public int Defense { get; private set; }
    
    public Character(Guid playerId, string name, CharacterClass characterClass)
    {
        PlayerId = playerId;
        Name = name;
        Class = characterClass;
        
        // 클래스별 기본 스탯 설정
        InitializeBaseStats();
    }
    
    // 도메인 로직: 레벨업
    public bool TryLevelUp()
    {
        var requiredExp = CalculateRequiredExperience(Level);
        if (Experience >= requiredExp)
        {
            Level++;
            Experience -= requiredExp;
            RecalculateStats();
            return true;
        }
        return false;
    }
    
    // 도메인 로직: 경험치 획득
    public void GainExperience(long amount)
    {
        if (amount <= 0) return;
        
        Experience += amount;
        
        // 연속 레벨업 처리
        while (TryLevelUp()) { }
    }
    
    // 도메인 로직: 스탯 재계산
    private void RecalculateStats()
    {
        var baseStats = GetBaseStatsByClass();
        var multiplier = 1 + (Level - 1) * 0.1f;
        
        Health = (int)(baseStats.Health * multiplier);
        Mana = (int)(baseStats.Mana * multiplier);
        Attack = (int)(baseStats.Attack * multiplier);
        Defense = (int)(baseStats.Defense * multiplier);
    }
    
    private long CalculateRequiredExperience(int level)
    {
        // 지수적 증가 공식 (방치형 게임 특성)
        return (long)(100 * Math.Pow(1.2, level - 1));
    }
    
    private (int Health, int Mana, int Attack, int Defense) GetBaseStatsByClass()
    {
        return Class switch
        {
            CharacterClass.Warrior => (120, 30, 25, 20),
            CharacterClass.Mage => (80, 100, 30, 10),
            CharacterClass.Archer => (90, 50, 35, 15),
            _ => (100, 50, 20, 15)
        };
    }
}

public enum CharacterClass
{
    Warrior,
    Mage, 
    Archer
}
```

### 3. OfflineReward 엔티티 (방치형 게임 핵심!)
```csharp
public class OfflineReward
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid PlayerId { get; private set; }
    public DateTime OfflineStart { get; private set; }
    public DateTime OfflineEnd { get; private set; }
    public long CalculatedGold { get; private set; }
    public long CalculatedExperience { get; private set; }
    public decimal BonusMultiplier { get; private set; } = 1.0m;
    public bool IsClaimed { get; private set; } = false;
    
    public OfflineReward(Guid playerId, DateTime offlineStart, DateTime offlineEnd, int playerLevel)
    {
        PlayerId = playerId;
        OfflineStart = offlineStart;
        OfflineEnd = offlineEnd;
        
        // 방치형 게임 핵심 로직: 오프라인 보상 계산
        CalculateRewards(playerLevel);
    }
    
    // 도메인 로직: 오프라인 보상 계산
    private void CalculateRewards(int playerLevel)
    {
        var offlineTime = OfflineEnd - OfflineStart;
        
        // 최대 12시간까지만 보상 (방치형 게임 밸런싱)
        var rewardHours = Math.Min(offlineTime.TotalHours, 12.0);
        
        // 레벨에 비례한 시간당 보상
        var hourlyGold = playerLevel * 100;
        var hourlyExp = playerLevel * 50;
        
        CalculatedGold = (long)(rewardHours * hourlyGold * BonusMultiplier);
        CalculatedExperience = (long)(rewardHours * hourlyExp * BonusMultiplier);
    }
    
    // 도메인 로직: 보상 수령
    public void Claim()
    {
        if (IsClaimed)
            throw new InvalidOperationException("Reward already claimed");
            
        IsClaimed = true;
    }
    
    // 도메인 로직: 보너스 적용
    public void ApplyBonus(decimal multiplier)
    {
        if (IsClaimed)
            throw new InvalidOperationException("Cannot apply bonus to claimed reward");
            
        BonusMultiplier = multiplier;
        
        // 보상 재계산은 필요시에만
        CalculatedGold = (long)(CalculatedGold / BonusMultiplier * multiplier);
        CalculatedExperience = (long)(CalculatedExperience / BonusMultiplier * multiplier);
        BonusMultiplier = multiplier;
    }
}
```

### 4. Item 엔티티
```csharp
public class Item
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; }
    public string Description { get; private set; }
    public ItemType Type { get; private set; }
    public ItemRarity Rarity { get; private set; }
    public Dictionary<StatType, int> BaseStats { get; private set; }
    public int MaxStackSize { get; private set; }
    public long SellPrice { get; private set; }
    
    public Item(string name, ItemType type, ItemRarity rarity)
    {
        Name = name;
        Type = type;
        Rarity = rarity;
        BaseStats = new Dictionary<StatType, int>();
        MaxStackSize = type == ItemType.Consumable ? 999 : 1;
        SellPrice = CalculateBaseSellPrice();
    }
    
    // 도메인 로직: 아이템 강화
    public Item Enhance(int enhancementLevel)
    {
        if (Type == ItemType.Consumable)
            throw new InvalidOperationException("Cannot enhance consumable items");
            
        var enhancedStats = new Dictionary<StatType, int>();
        foreach (var stat in BaseStats)
        {
            var enhancedValue = stat.Value + (stat.Value * enhancementLevel * 0.1f);
            enhancedStats[stat.Key] = (int)enhancedValue;
        }
        
        return new Item(Name, Type, Rarity)
        {
            BaseStats = enhancedStats,
            SellPrice = SellPrice * (1 + enhancementLevel)
        };
    }
    
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
}

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

public enum StatType
{
    Attack,
    Defense,
    Health,
    Mana,
    CriticalChance,
    CriticalDamage
}
```

## 🎮 Unity 개발자 관점에서의 이해

### Unity Component vs Domain Entity
```csharp
// Unity에서
public class PlayerComponent : MonoBehaviour
{
    public int health = 100;
    public int level = 1;
    
    void LevelUp()
    {
        level++;
        health += 20;
    }
}

// Clean Architecture Domain에서  
public class Player
{
    public int Health { get; private set; } = 100;
    public int Level { get; private set; } = 1;
    
    public void LevelUp()
    {
        Level++;
        Health += 20;
    }
}
```

**차이점**:
- **캡슐화**: private setter로 데이터 보호
- **비즈니스 규칙**: 생성자와 메소드에서 유효성 검사
- **순수성**: Unity 의존성 없이 순수 C# 코드

## 🧪 Domain Layer 테스트 예시

```csharp
[Test]
public void Character_GainExperience_Should_Level_Up()
{
    // Arrange
    var character = new Character(Guid.NewGuid(), "TestHero", CharacterClass.Warrior);
    var initialLevel = character.Level;
    
    // Act  
    character.GainExperience(1000);
    
    // Assert
    Assert.Greater(character.Level, initialLevel);
}

[Test] 
public void OfflineReward_Calculate_Should_Limit_To_12_Hours()
{
    // Arrange
    var playerId = Guid.NewGuid();
    var start = DateTime.UtcNow.AddHours(-24); // 24시간 전
    var end = DateTime.UtcNow;
    
    // Act
    var reward = new OfflineReward(playerId, start, end, playerLevel: 10);
    
    // Assert  
    var expected12HourReward = 12 * 10 * 100; // 12시간 * 레벨10 * 시간당100골드
    Assert.AreEqual(expected12HourReward, reward.CalculatedGold);
}
```

## 💡 주요 설계 원칙

### 1. 불변성 (Immutability)
- `private set`으로 외부에서 직접 수정 방지
- 메소드를 통해서만 상태 변경 허용

### 2. 유효성 검사
- 생성자에서 필수 검증 수행
- 비즈니스 규칙 위반시 예외 발생

### 3. 비즈니스 로직 집중화
- 모든 게임 규칙을 Domain에 집중
- 외부 레이어에서는 Domain 로직 사용만

## 🔗 관련 문서
- [[Clean Architecture 개념 정리]]
- [[Clean Architecture - Application Layer 상세]]
- [[Unity vs Clean Architecture 비교분석]]

*작성일: 2025년 9월 26일*