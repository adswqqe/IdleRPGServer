# Repository Pattern

## 🎯 개념 이해
데이터 액세스 로직을 캡슐화하는 디자인 패턴 - Unity의 Manager 패턴과 유사

## 🔗 연결 관계
- [[Week 1 - Domain 모델링과 Clean Architecture]]
- [[Entity Framework Core]]
- [[Player Entity]]
- [[Unit of Work Pattern]]

## 💻 구현 패턴
### 인터페이스 정의
```csharp
public interface IPlayerRepository
{
    Task<Player> GetByIdAsync(Guid id);
    Task<List<Player>> GetAllAsync();
    Task<Player> AddAsync(Player player);
    Task UpdateAsync(Player player);
    Task DeleteAsync(Guid id);
}
```

### 구현 클래스
```csharp
public class PlayerRepository : IPlayerRepository
{
    private readonly GameDbContext _context;
    
    public async Task<Player> GetByIdAsync(Guid id)
    {
        return await _context.Players
            .Include(p => p.Characters)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}
```

## 🎮 Unity 개발자 관점
Unity의 Manager 클래스와 비슷한 역할
```csharp
// Unity Manager 패턴
public class PlayerManager : MonoBehaviour
{
    public Player GetPlayer(int id) { /* 로직 */ }
    public void SavePlayer(Player player) { /* 로직 */ }
}

// Repository 패턴
public class PlayerRepository : IPlayerRepository  
{
    public async Task<Player> GetByIdAsync(Guid id) { /* 로직 */ }
    public async Task SaveAsync(Player player) { /* 로직 */ }
}
```

## ✅ 장점
- **테스트 용이성**: 인터페이스로 Mock 가능
- **관심사 분리**: 비즈니스 로직과 데이터 액세스 분리
- **재사용성**: 다른 데이터 소스로 교체 가능
- **일관성**: 표준화된 데이터 액세스 패턴

## 🔄 Unity와의 통합
```csharp
// Unity Service에서 Repository 사용
public class PlayerService : MonoBehaviour
{
    private IPlayerRepository _playerRepository;
    
    public async void LoadPlayer(Guid playerId)
    {
        var player = await _playerRepository.GetByIdAsync(playerId);
        UpdateUI(player);
    }
}
```

## 📚 관련 학습
- [[Unit of Work Pattern]]
- [[Specification Pattern]]  
- [[Dependency Injection]]

---

#Repository #DesignPattern #DataAccess #아키텍처