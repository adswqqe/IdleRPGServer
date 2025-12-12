# Player Entity

## 🎯 핵심 개념
방치형 RPG의 중심이 되는 플레이어 데이터 모델

## 🔗 연결 관계
- [[Week 1 - Domain 모델링과 Clean Architecture]]
- [[Entity Framework Core]]
- [[Character System]]
- [[Offline Reward System]]
- [[PlayerStats]]

## 💻 구현 예시
```csharp
public class Player
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public DateTime LastLogin { get; set; }
    
    // Navigation Properties
    public PlayerStats Stats { get; set; }
    public List<Character> Characters { get; set; }
    public List<OfflineReward> OfflineRewards { get; set; }
}
```

## 🎮 Unity 연동
Unity 클라이언트에서 플레이어 정보를 동기화하고 관리하는 핵심 모델

## 📚 관련 개념
- [[User Authentication]]
- [[Repository Pattern]]
- [[Character System]]

---

#Player #Entity #도메인모델 #방치형RPG