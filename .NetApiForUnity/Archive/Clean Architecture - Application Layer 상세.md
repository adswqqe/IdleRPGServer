 [[🎮 방치형 RPG 서버 개발 완전 가이드 - Week 1]]# Clean Architecture - Application Layer 상세

## 🎯 Application Layer란?
**비즈니스 유스케이스를 조율**하는 레이어입니다. Unity의 **Manager 클래스들**(GameManager, UIManager)과 같은 역할을 합니다.

## 🔑 핵심 특징
- **유스케이스 구현**: "플레이어 생성", "오프라인 보상 계산" 등의 비즈니스 시나리오
- **Domain 조율**: 여러 Domain 엔티티들을 조합해서 복잡한 로직 수행  
- **인터페이스 정의**: Infrastructure가 구현해야 할 계약 명시
- **트랜잭션 관리**: 데이터 일관성 보장

## 🎮 방치형 RPG에서의 Application Services

### 1. PlayerService (플레이어 관리)
```csharp
public interface IPlayerService
{
    Task<PlayerDto> CreatePlayerAsync(CreatePlayerDto request);
    Task<PlayerDto> GetPlayerAsync(Guid playerId);
    Task<PlayerStatsDto> GetPlayerStatsAsync(Guid playerId);
    Task<bool> UpdateLastLoginAsync(Guid playerId);
}

public class PlayerService : IPlayerService
{
    private readonly IPlayerRepository _playerRepository;
    private readonly ILogger<PlayerService> _logger;
    private readonly IMapper _mapper;
    
    public PlayerService(
        IPlayerRepository playerRepository,
        ILogger<PlayerService> logger,
        IMapper mapper)
    {
        _playerRepository = playerRepository;
        _logger = logger;
        _mapper = mapper;
    }
    
    public async Task<PlayerDto> CreatePlayerAsync(CreatePlayerDto request)
    {
        _logger.LogInformation($"Creating new player: {request.Username}");
        
        // 1. 비즈니스 규칙 검증
        if (await _playerRepository.UsernameExistsAsync(request.Username))
        {
            throw new DuplicateException($"Username '{request.Username}' already exists");
        }
        
        // 2. Domain 엔티티 생성 (비즈니스 로직 위임)
        var player = new Player(request.Username, request.Email);
        
        // 3. 영속성 처리 (Infrastructure에 위임)
        await _playerRepository.AddAsync(player);
        
        // 4. DTO 변환 후 반환
        _logger.LogInformation($"Player created successfully: {player.Id}");
        return _mapper.Map<PlayerDto>(player);
    }
    
    public async Task<PlayerStatsDto> GetPlayerStatsAsync(Guid playerId)
    {
        var player = await _playerRepository.GetByIdAsync(playerId);
        if (player == null)
        {
            throw new NotFoundException($"Player not found: {playerId}");
        }
        
        // Domain 로직 활용
        var offlineTime = player.GetOfflineTime();
        
        return new PlayerStatsDto
        {
            PlayerId = player.Id,
            Username = player.Username,
            LastLogin = player.LastLogin,
            OfflineHours = offlineTime.TotalHours,
            IsActive = player.IsActive
        };
    }
}
```

### 2. OfflineRewardService (방치형 게임 핵심!)
```csharp
public interface IOfflineRewardService
{
    Task<OfflineRewardDto> CalculateOfflineRewardAsync(Guid playerId);
    Task<OfflineRewardDto> ClaimOfflineRewardAsync(Guid playerId, Guid rewardId);
    Task<List<OfflineRewardDto>> GetPendingRewardsAsync(Guid playerId);
}

public class OfflineRewardService : IOfflineRewardService
{
    private readonly IPlayerRepository _playerRepository;
    private readonly ICharacterRepository _characterRepository;
    private readonly IOfflineRewardRepository _rewardRepository;
    private readonly ILogger<OfflineRewardService> _logger;
    
    public async Task<OfflineRewardDto> CalculateOfflineRewardAsync(Guid playerId)
    {
        _logger.LogInformation($"Calculating offline reward for player: {playerId}");
        
        // 1. 플레이어 정보 조회
        var player = await _playerRepository.GetByIdAsync(playerId);
        if (player == null)
        {
            throw new NotFoundException("Player not found");
        }
        
        // 2. 메인 캐릭터 조회 (보상 계산을 위해)
        var mainCharacter = await _characterRepository.GetMainCharacterAsync(playerId);
        if (mainCharacter == null)
        {
            throw new InvalidOperationException("No main character found");
        }
        
        // 3. 이미 계산된 미수령 보상이 있는지 확인
        var existingReward = await _rewardRepository.GetPendingRewardAsync(playerId);
        if (existingReward != null)
        {
            return _mapper.Map<OfflineRewardDto>(existingReward);
        }
        
        // 4. Domain 로직으로 새 보상 계산
        var offlineStart = player.LastLogin;
        var offlineEnd = DateTime.UtcNow;
        var offlineReward = new OfflineReward(playerId, offlineStart, offlineEnd, mainCharacter.Level);
        
        // 5. VIP 보너스 적용 (비즈니스 로직)
        if (await IsVipPlayerAsync(playerId))
        {
            offlineReward.ApplyBonus(1.5m); // 50% 보너스
        }
        
        // 6. 보상 저장
        await _rewardRepository.AddAsync(offlineReward);
        
        _logger.LogInformation($"Offline reward calculated: {offlineReward.CalculatedGold} gold, {offlineReward.CalculatedExperience} exp");
        
        return _mapper.Map<OfflineRewardDto>(offlineReward);
    }
    
    public async Task<OfflineRewardDto> ClaimOfflineRewardAsync(Guid playerId, Guid rewardId)
    {
        using var transaction = await _rewardRepository.BeginTransactionAsync();
        
        try
        {
            // 1. 보상 조회
            var reward = await _rewardRepository.GetByIdAsync(rewardId);
            if (reward == null || reward.PlayerId != playerId)
            {
                throw new NotFoundException("Reward not found");
            }
            
            // 2. Domain 로직으로 보상 수령 처리
            reward.Claim(); // 도메인 규칙 적용
            
            // 3. 플레이어와 캐릭터에 보상 적용
            var player = await _playerRepository.GetByIdAsync(playerId);
            var mainCharacter = await _characterRepository.GetMainCharacterAsync(playerId);
            
            // 골드 지급 (추후 Player 엔티티에 Gold 속성 추가 예정)
            // player.AddGold(reward.CalculatedGold);
            
            // 경험치 지급 (Domain 로직 사용)
            mainCharacter.GainExperience(reward.CalculatedExperience);
            
            // 4. 마지막 로그인 시간 업데이트
            player.UpdateLastLogin();
            
            // 5. 변경사항 저장
            await _rewardRepository.UpdateAsync(reward);
            await _playerRepository.UpdateAsync(player);
            await _characterRepository.UpdateAsync(mainCharacter);
            
            // 6. 트랜잭션 커밋
            await transaction.CommitAsync();
            
            _logger.LogInformation($"Offline reward claimed: Player {playerId}, Reward {rewardId}");
            
            return _mapper.Map<OfflineRewardDto>(reward);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    
    private async Task<bool> IsVipPlayerAsync(Guid playerId)
    {
        // 추후 VIP 시스템 구현 예정
        return false;
    }
}
```

### 3. CharacterService (캐릭터 관리)
```csharp
public interface ICharacterService
{
    Task<CharacterDto> CreateCharacterAsync(Guid playerId, CreateCharacterDto request);
    Task<CharacterDto> GetCharacterAsync(Guid characterId);
    Task<List<CharacterDto>> GetPlayerCharactersAsync(Guid playerId);
    Task<CharacterDto> LevelUpCharacterAsync(Guid characterId);
    Task<bool> SetMainCharacterAsync(Guid playerId, Guid characterId);
}

public class CharacterService : ICharacterService
{
    private readonly ICharacterRepository _characterRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly ILogger<CharacterService> _logger;
    private readonly IMapper _mapper;
    
    public async Task<CharacterDto> CreateCharacterAsync(Guid playerId, CreateCharacterDto request)
    {
        // 1. 플레이어 존재 여부 확인
        var playerExists = await _playerRepository.ExistsAsync(playerId);
        if (!playerExists)
        {
            throw new NotFoundException("Player not found");
        }
        
        // 2. 캐릭터 수 제한 확인 (비즈니스 규칙)
        var existingCharacters = await _characterRepository.GetByPlayerIdAsync(playerId);
        if (existingCharacters.Count >= 5) // 최대 5캐릭터
        {
            throw new BusinessRuleException("Maximum character limit reached");
        }
        
        // 3. 캐릭터 이름 중복 확인
        if (await _characterRepository.NameExistsAsync(request.Name))
        {
            throw new DuplicateException($"Character name '{request.Name}' already exists");
        }
        
        // 4. Domain 엔티티 생성
        var character = new Character(playerId, request.Name, request.Class);
        
        // 5. 첫 캐릭터면 자동으로 메인 캐릭터 설정
        if (existingCharacters.Count == 0)
        {
            // character.SetAsMain(); // Domain 메소드 추가 예정
        }
        
        // 6. 저장
        await _characterRepository.AddAsync(character);
        
        _logger.LogInformation($"Character created: {character.Name} ({character.Class}) for player {playerId}");
        
        return _mapper.Map<CharacterDto>(character);
    }
    
    public async Task<CharacterDto> LevelUpCharacterAsync(Guid characterId)
    {
        var character = await _characterRepository.GetByIdAsync(characterId);
        if (character == null)
        {
            throw new NotFoundException("Character not found");
        }
        
        // Domain 로직으로 레벨업 시도
        var leveledUp = character.TryLevelUp();
        if (!leveledUp)
        {
            throw new BusinessRuleException("Not enough experience to level up");
        }
        
        // 변경사항 저장
        await _characterRepository.UpdateAsync(character);
        
        _logger.LogInformation($"Character leveled up: {character.Name} is now level {character.Level}");
        
        return _mapper.Map<CharacterDto>(character);
    }
}
```

## 📋 DTOs (Data Transfer Objects)

### 요청 DTOs
```csharp
public class CreatePlayerDto
{
    [Required, StringLength(50, MinimumLength = 3)]
    public string Username { get; set; }
    
    [Required, EmailAddress]
    public string Email { get; set; }
    
    [Required, MinLength(6)]
    public string Password { get; set; }
}

public class CreateCharacterDto
{
    [Required, StringLength(30, MinimumLength = 2)]
    public string Name { get; set; }
    
    [Required]
    public CharacterClass Class { get; set; }
}
```

### 응답 DTOs  
```csharp
public class PlayerDto
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastLogin { get; set; }
    public bool IsActive { get; set; }
}

public class OfflineRewardDto
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public double OfflineHours { get; set; }
    public long GoldReward { get; set; }
    public long ExperienceReward { get; set; }
    public decimal BonusMultiplier { get; set; }
    public bool IsClaimed { get; set; }
    public DateTime CalculatedAt { get; set; }
}

public class CharacterDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public CharacterClass Class { get; set; }
    public int Level { get; set; }
    public long Experience { get; set; }
    public int Health { get; set; }
    public int Mana { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    public bool IsMain { get; set; }
}
```

## 🎮 Unity 개발자 관점에서의 이해

### Unity Manager vs Application Service
```csharp
// Unity에서
public class GameManager : MonoBehaviour
{
    public void CreatePlayer(string name)
    {
        var player = Instantiate(playerPrefab);
        player.name = name;
        SavePlayerData(player);
    }
    
    void SavePlayerData(Player player)
    {
        PlayerPrefs.SetString("PlayerName", player.name);
        PlayerPrefs.Save();
    }
}

// Application Layer에서
public class PlayerService : IPlayerService  
{
    public async Task<PlayerDto> CreatePlayerAsync(CreatePlayerDto request)
    {
        // 1. 비즈니스 규칙 검증
        await ValidatePlayerCreation(request);
        
        // 2. Domain 엔티티 생성
        var player = new Player(request.Username, request.Email);
        
        // 3. 영속성 처리 (Repository 패턴)
        await _playerRepository.AddAsync(player);
        
        return _mapper.Map<PlayerDto>(player);
    }
}
```

**차이점**:
- **관심사 분리**: 비즈니스 로직과 데이터 저장 분리
- **테스트 용이성**: Mock Repository로 독립적 테스트 가능
- **비동기 처리**: async/await로 성능 최적화

## 🧪 Application Layer 테스트 예시

```csharp
[Test]
public async Task CreatePlayer_Should_Create_New_Player()
{
    // Arrange
    var mockRepository = new Mock<IPlayerRepository>();
    mockRepository.Setup(r => r.UsernameExistsAsync("testuser"))
              .ReturnsAsync(false);
              
    var service = new PlayerService(mockRepository.Object, logger, mapper);
    var request = new CreatePlayerDto 
    { 
        Username = "testuser", 
        Email = "test@example.com" 
    };
    
    // Act
    var result = await service.CreatePlayerAsync(request);
    
    // Assert
    Assert.AreEqual("testuser", result.Username);
    mockRepository.Verify(r => r.AddAsync(It.IsAny<Player>()), Times.Once);
}

[Test]
public async Task ClaimOfflineReward_Should_Apply_VIP_Bonus()
{
    // Arrange - VIP 플레이어 설정
    var mockRewardRepo = new Mock<IOfflineRewardRepository>();
    var mockPlayerRepo = new Mock<IPlayerRepository>();
    
    var service = new OfflineRewardService(mockPlayerRepo.Object, 
        mockCharacterRepo.Object, mockRewardRepo.Object, logger);
    
    // Act & Assert - VIP 보너스 적용 확인
    // ...
}
```

## 💡 주요 설계 원칙

### 1. 단일 책임 원칙
- 각 Service는 하나의 비즈니스 도메인만 담당
- PlayerService는 플레이어 관련 유스케이스만

### 2. 의존성 주입
- Interface에 의존하여 테스트 용이성 확보
- Repository 구현체를 주입받아 사용

### 3. 트랜잭션 관리
- 데이터 일관성이 중요한 작업은 트랜잭션으로 처리
- 실패시 롤백으로 데이터 무결성 보장

### 4. DTO 변환
- Domain 엔티티를 직접 노출하지 않음
- API 계약과 Domain 구현 분리

## 🔗 관련 문서
- [[Clean Architecture 개념 정리]]
- [[Clean Architecture - Domain Layer 상세]]
- [[Clean Architecture - Infrastructure Layer 상세]]
- [[방치형 RPG 비즈니스 로직 설계]]

*작성일: 2025년 9월 26일*