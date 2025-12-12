# 🎮 방치형 RPG 서버 개발 완전 가이드 - Week 1 (상세 실습편)
## Unity 개발자를 위한 실무 중심 백엔드 부트캠프

### 📋 목표 설정
- **최종 목표**: 1만 동시 접속자를 지원하는 방치형 2D RPG 서버
- **Week 1 목표**: 핵심 플레이어 관리 API + 오프라인 보상 시스템 구축
- **기술 스택**: .NET 8 + ASP.NET Core + PostgreSQL + Docker + GitHub

---

## 🛠️ 개발 환경 구축 (Day 0) - GUI 방식

### 1. 필수 소프트웨어 설치

#### JetBrains Rider 설치 🎯
1. **다운로드**: https://www.jetbrains.com/rider/
2. **라이선스**: 30일 평가판 또는 학생 라이선스 활용
3. **Unity 개발자에게 완벽한 IDE** - 이미 익숙한 인터페이스!

#### .NET 8 SDK 설치
1. **공식 사이트**: https://dotnet.microsoft.com/download/dotnet/8.0
2. **Windows**: `.NET 8.0 SDK x64 Installer` 다운로드 후 실행
3. **macOS**: `.NET 8.0 SDK macOS Installer` 다운로드 후 실행
4. **설치 확인**: 터미널에서 `dotnet --version` 실행 → `8.0.x` 출력 확인

#### PostgreSQL 설치 (Docker Desktop 사용 권장)
1. **Docker Desktop 설치**: https://www.docker.com/products/docker-desktop/
2. **PostgreSQL 컨테이너 실행**:
   - Docker Desktop 실행
   - 검색에서 `postgres:16` 이미지 검색
   - Run 클릭 후 다음 설정:
     - **Container Name**: `idle-rpg-postgres`
     - **Port**: `5432:5432`
     - **Environment Variables**:
       - `POSTGRES_DB=idlerpgdb`
       - `POSTGRES_USER=gamedev` 
       - `POSTGRES_PASSWORD=dev123!`

#### Git 설치 및 GitHub 계정 연결
1. **Git 설치**: https://git-scm.com/downloads
2. **GitHub Desktop 설치** (GUI 선호 시): https://desktop.github.com/
3. **GitHub 계정 준비**: https://github.com 가입

### 2. Rider에서 프로젝트 생성 🚀

#### Step 1: 새 솔루션 생성
1. **Rider 실행** → `New Solution` 클릭
2. **Templates**에서 `.NET` → `ASP.NET Core Web Application` 선택
3. **Project Settings**:
   - **Solution name**: `IdleRPGServer`
   - **Project name**: `IdleRPG.API`
   - **Location**: 원하는 폴더 선택
   - **Framework**: `.NET 8.0` 선택
   - **Project Type**: `Web API` 선택
   - ✅ **Enable Docker** 체크
   - ✅ **Enable OpenAPI support** 체크
4. **Create** 클릭

#### Step 2: Clean Architecture 프로젝트 추가
**솔루션 탐색기**에서 솔루션 우클릭 → `Add` → `New Project`:

**🔍 Rider에서 Class Library 찾기:**
- Templates 창에서 **".NET"** 카테고리 선택
- 다음 중 하나를 찾으세요:
  - ✅ **"Class Library"** (가장 일반적)
  - ✅ **"Library"**  
  - ✅ **".NET Class Library"**
  - ✅ **"C# Class Library"**

**없다면 이렇게 찾으세요:**
1. **Search 박스**에 `library` 입력
2. **Language 필터**에서 `C#` 선택  
3. **Project Type 필터**에서 `Library` 선택

1. **IdleRPG.Domain** 추가:
   - Template: `Class Library` (또는 위의 대안 중 하나)
   - Name: `IdleRPG.Domain`
   - Framework: `.NET 8.0`

2. **IdleRPG.Application** 추가:
   - Template: `Class Library`
   - Name: `IdleRPG.Application` 
   - Framework: `.NET 8.0`

3. **IdleRPG.Infrastructure** 추가:
   - Template: `Class Library`
   - Name: `IdleRPG.Infrastructure`
   - Framework: `.NET 8.0`

**⚠️ 주의: Web API는 선택하지 마세요!**
- **Web API** = 웹 서버 프로젝트 (IdleRPG.API와 같은 용도)
- **Class Library** = 코드 라이브러리 프로젝트 (Domain, Application, Infrastructure용)

#### Step 3: 프로젝트 의존성 설정
**각 프로젝트 우클릭** → `Manage References`:

1. **IdleRPG.API**:
   - ✅ `IdleRPG.Application` 체크
   - ✅ `IdleRPG.Infrastructure` 체크

2. **IdleRPG.Application**:
   - ✅ `IdleRPG.Domain` 체크

3. **IdleRPG.Infrastructure**:
   - ✅ `IdleRPG.Domain` 체크
   - ✅ `IdleRPG.Application` 체크

### 3. NuGet 패키지 설치 📦

#### Rider NuGet Manager 사용:
각 프로젝트 우클릭 → `Manage NuGet Packages`:

**IdleRPG.API 패키지**:
- `Microsoft.EntityFrameworkCore.Design`
- `Serilog.AspNetCore`
- `Swashbuckle.AspNetCore` (이미 설치됨)

**IdleRPG.Infrastructure 패키지**:
- `Microsoft.EntityFrameworkCore`
- `Npgsql.EntityFrameworkCore.PostgreSQL`
- `Microsoft.EntityFrameworkCore.Tools`

**IdleRPG.Application 패키지**:
- `AutoMapper`
- `FluentValidation`
- `MediatR`

### 4. GitHub 연결 (Rider GUI) 🔗

#### Step 1: Git 초기화
1. **Rider 메뉴**: `VCS` → `Enable Version Control Integration`
2. **Git** 선택 → `OK`
3. **Initial commit**: 자동으로 커밋 제안 → `Commit`

#### Step 2: GitHub 저장소 생성
1. **Rider 메뉴**: `Git` → `GitHub` → `Share Project on GitHub`
2. **Repository settings**:
   - **Repository name**: `IdleRPGServer`
   - **Description**: `2D Idle RPG Game Server`
   - ✅ **Private** 체크 (개인 프로젝트)
3. **Share** 클릭
4. GitHub 로그인 프롬프트 시 계정 연동

#### Step 3: .gitignore 설정 최적화
**솔루션 루트**에서 `.gitignore` 파일 열기 → 다음 내용 추가:
```gitignore
# Custom ignores for Idle RPG Server
*.user
appsettings.Development.json
logs/
*.db
.env
.idea/
bin/
obj/
```

### 5. Docker 환경 설정 (Docker Desktop GUI) 🐳

#### Step 1: Docker Compose 파일 생성
**Rider에서** 솔루션 루트에 `docker-compose.yml` 파일 생성:

1. **솔루션 탐색기**에서 솔루션 우클릭 → `Add` → `New Item`
2. **File Type**: `Text File`
3. **Name**: `docker-compose.yml`
4. 다음 내용 복사-붙여넣기:

```yaml
version: '3.8'

services:
  # PostgreSQL Database
  postgres:
    image: postgres:16
    container_name: idlerpg-postgres
    environment:
      POSTGRES_DB: idlerpgdb
      POSTGRES_USER: gamedev
      POSTGRES_PASSWORD: dev123!
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
    networks:
      - idlerpg-network

  # Redis Cache (미래 확장용)
  redis:
    image: redis:7-alpine
    container_name: idlerpg-redis
    ports:
      - "6379:6379"
    networks:
      - idlerpg-network

  # pgAdmin (데이터베이스 관리 도구)
  pgadmin:
    image: dpage/pgadmin4
    container_name: idlerpg-pgadmin
    environment:
      PGADMIN_DEFAULT_EMAIL: admin@idlerpg.com
      PGADMIN_DEFAULT_PASSWORD: admin123
    ports:
      - "8082:80"
    depends_on:
      - postgres
    networks:
      - idlerpg-network

volumes:
  postgres_data:

networks:
  idlerpg-network:
    driver: bridge
```

#### Step 2: Docker Desktop에서 컨테이너 실행
1. **Docker Desktop** 실행
2. **터미널 창**에서 프로젝트 폴더로 이동
3. `docker-compose up -d postgres redis pgadmin` 실행
4. **Docker Desktop**에서 확인:
   - 🟢 `idlerpg-postgres` 컨테이너 실행 중
   - 🟢 `idlerpg-redis` 컨테이너 실행 중  
   - 🟢 `idlerpg-pgadmin` 컨테이너 실행 중

#### Step 3: pgAdmin 웹 접속 설정
1. **브라우저**에서 `http://localhost:8082` 접속
2. **로그인**: 
   - Email: `admin@idlerpg.com`
   - Password: `admin123`
3. **서버 추가**:
   - 우클릭 → `Create` → `Server`
   - **General** 탭: Name = `IdleRPG Database`
   - **Connection** 탭:
     - Host: `postgres` (컨테이너 이름)
     - Port: `5432`
     - Database: `idlerpgdb`
     - Username: `gamedev`
     - Password: `dev123!`
4. **Save** → 데이터베이스 연결 성공! ✅

### 6. Rider 프로젝트 실행 설정 ⚙️

#### Step 1: 연결 문자열 설정
**IdleRPG.API** → `appsettings.Development.json` 파일 수정:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=idlerpgdb;Username=gamedev;Password=dev123!"
  }
}
```

#### Step 2: Rider 실행 구성 설정
1. **Rider 상단** → 실행 구성 드롭다운 클릭
2. **Edit Configurations** 선택
3. **IdleRPG.API** 구성 설정:
   - **Environment variables** 추가:
     - `ASPNETCORE_ENVIRONMENT=Development`
   - **Working directory**: 프로젝트 루트 확인
4. **OK** → 설정 저장

#### Step 3: 첫 실행 테스트
1. **Rider**에서 ▶️ **Run** 버튼 클릭
2. **브라우저 자동 열림** → Swagger UI 확인
3. **주요 URL들**:
   - 🌐 **API 문서**: `https://localhost:7260/swagger`
   - 🗄️ **pgAdmin**: `http://localhost:8082`
   - 📊 **개발 로그**: Rider 콘솔창

## 🎯 학습 목표

Unity의 ScriptableObject 설계 경험을 바탕으로, **방치형 RPG의 핵심 데이터 구조**를 데이터베이스로 확장합니다.

**Unity → Database 개념 매핑**:
- ScriptableObject → Database Table
- Prefab Reference → Foreign Key
- Serialized Fields → Table Columns
- Inspector Validation → Database Constraints

### 📋 방치형 RPG 핵심 데이터 모델

#### 1. 플레이어 관리
```sql
-- Players 테이블 (게임의 핵심)
CREATE TABLE players (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    username VARCHAR(50) UNIQUE NOT NULL,
    email VARCHAR(100) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_login TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    is_active BOOLEAN DEFAULT true
);

-- Player Stats (방치형 게임 핵심)
CREATE TABLE player_stats (
    player_id UUID PRIMARY KEY REFERENCES players(id),
    level INTEGER DEFAULT 1,
    experience BIGINT DEFAULT 0,
    gold BIGINT DEFAULT 1000,
    gems INTEGER DEFAULT 10,
    offline_hours DECIMAL DEFAULT 0,
    total_idle_time BIGINT DEFAULT 0,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

#### 2. 캐릭터 시스템
```sql
-- Characters (플레이어가 소유한 캐릭터들)
CREATE TABLE characters (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    player_id UUID NOT NULL REFERENCES players(id),
    name VARCHAR(30) NOT NULL,
    character_class VARCHAR(20) NOT NULL, -- 'warrior', 'mage', 'archer'
    level INTEGER DEFAULT 1,
    experience BIGINT DEFAULT 0,
    health INTEGER NOT NULL,
    mana INTEGER NOT NULL,
    attack INTEGER NOT NULL,
    defense INTEGER NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    is_main BOOLEAN DEFAULT false
);

-- 인덱스 최적화 (1만 동접 고려)
CREATE INDEX idx_characters_player_id ON characters(player_id);
CREATE INDEX idx_characters_level ON characters(level);
```

#### 3. 아이템 & 인벤토리
```sql
-- Item Templates (게임 내 모든 아이템 정의)
CREATE TABLE item_templates (
    id INTEGER PRIMARY KEY,
    name VARCHAR(50) NOT NULL,
    description TEXT,
    item_type VARCHAR(20) NOT NULL, -- 'weapon', 'armor', 'consumable'
    rarity VARCHAR(20) NOT NULL,    -- 'common', 'rare', 'epic', 'legendary'
    base_stats JSONB,  -- PostgreSQL JSON으로 유연한 스탯 저장
    sell_price INTEGER DEFAULT 0,
    max_stack INTEGER DEFAULT 1
);

-- Player Inventory
CREATE TABLE player_inventories (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    player_id UUID NOT NULL REFERENCES players(id),
    item_id INTEGER NOT NULL REFERENCES item_templates(id),
    quantity INTEGER NOT NULL DEFAULT 1,
    slot_index INTEGER,
    enhancement_level INTEGER DEFAULT 0,
    acquired_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_inventory_player_id ON player_inventories(player_id);
```

#### 4. 오프라인 보상 시스템 (방치형 핵심!)
```sql
-- Offline Rewards (방치형 게임의 핵심 시스템)
CREATE TABLE offline_rewards (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    player_id UUID NOT NULL REFERENCES players(id),
    offline_start TIMESTAMP NOT NULL,
    offline_end TIMESTAMP,
    calculated_gold BIGINT DEFAULT 0,
    calculated_exp BIGINT DEFAULT 0,
    bonus_multiplier DECIMAL DEFAULT 1.0,
    is_claimed BOOLEAN DEFAULT false,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Auto Battle Results
CREATE TABLE auto_battle_logs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    player_id UUID NOT NULL REFERENCES players(id),
    stage_level INTEGER NOT NULL,
    battles_won INTEGER DEFAULT 0,
    total_exp_gained BIGINT DEFAULT 0,
    total_gold_gained BIGINT DEFAULT 0,
    items_dropped JSONB,
    battle_time TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

### 💻 Entity Framework Core 모델 생성

#### Domain Models (IdleRPG.Domain/Entities/)
```csharp
// Player.cs
using System.ComponentModel.DataAnnotations;

namespace IdleRPG.Domain.Entities
{
    public class Player
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required, MaxLength(50)]
        public string Username { get; set; }
        
        [Required, EmailAddress, MaxLength(100)]
        public string Email { get; set; }
        
        [Required]
        public string PasswordHash { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastLogin { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        
        // Navigation Properties
        public PlayerStats Stats { get; set; }
        public List<Character> Characters { get; set; } = new();
        public List<PlayerInventory> Inventory { get; set; } = new();
        public List<OfflineReward> OfflineRewards { get; set; } = new();
    }
    
    public class PlayerStats
    {
        public Guid PlayerId { get; set; }
        public Player Player { get; set; }
        
        public int Level { get; set; } = 1;
        public long Experience { get; set; } = 0;
        public long Gold { get; set; } = 1000;
        public int Gems { get; set; } = 10;
        public decimal OfflineHours { get; set; } = 0;
        public long TotalIdleTime { get; set; } = 0;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // 경험치 계산 메소드 (Unity와 유사한 패턴)
        public bool CanLevelUp()
        {
            var requiredExp = CalculateRequiredExp(Level);
            return Experience >= requiredExp;
        }
        
        private long CalculateRequiredExp(int level)
        {
            return (long)(100 * Math.Pow(1.2, level - 1));
        }
    }
}

// Character.cs
namespace IdleRPG.Domain.Entities
{
    public class Character
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid PlayerId { get; set; }
        public Player Player { get; set; }
        
        [Required, MaxLength(30)]
        public string Name { get; set; }
        
        [Required, MaxLength(20)]
        public string CharacterClass { get; set; } // warrior, mage, archer
        
        public int Level { get; set; } = 1;
        public long Experience { get; set; } = 0;
        public int Health { get; set; }
        public int Mana { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsMain { get; set; } = false;
        
        // 스탯 계산 (Unity Component와 유사한 패턴)
        public void RecalculateStats()
        {
            var baseStats = GetBaseStatsByClass();
            var levelMultiplier = 1 + (Level - 1) * 0.1f;
            
            Health = (int)(baseStats.Health * levelMultiplier);
            Mana = (int)(baseStats.Mana * levelMultiplier);
            Attack = (int)(baseStats.Attack * levelMultiplier);
            Defense = (int)(baseStats.Defense * levelMultiplier);
        }
        
        private (int Health, int Mana, int Attack, int Defense) GetBaseStatsByClass()
        {
            return CharacterClass.ToLower() switch
            {
                "warrior" => (120, 30, 25, 20),
                "mage" => (80, 100, 30, 10),
                "archer" => (90, 50, 35, 15),
                _ => (100, 50, 20, 15)
            };
        }
    }
}
```

#### Database Context 설정
```csharp
// IdleRPG.Infrastructure/Data/GameDbContext.cs
using Microsoft.EntityFrameworkCore;
using IdleRPG.Domain.Entities;

namespace IdleRPG.Infrastructure.Data
{
    public class GameDbContext : DbContext
    {
        public GameDbContext(DbContextOptions<GameDbContext> options) : base(options) { }
        
        public DbSet<Player> Players { get; set; }
        public DbSet<PlayerStats> PlayerStats { get; set; }
        public DbSet<Character> Characters { get; set; }
        public DbSet<ItemTemplate> ItemTemplates { get; set; }
        public DbSet<PlayerInventory> PlayerInventories { get; set; }
        public DbSet<OfflineReward> OfflineRewards { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Player 설정
            modelBuilder.Entity<Player>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.HasIndex(p => p.Username).IsUnique();
                entity.HasIndex(p => p.Email).IsUnique();
                entity.Property(p => p.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
            
            // PlayerStats 1:1 관계
            modelBuilder.Entity<PlayerStats>(entity =>
            {
                entity.HasKey(ps => ps.PlayerId);
                entity.HasOne(ps => ps.Player)
                      .WithOne(p => p.Stats)
                      .HasForeignKey<PlayerStats>(ps => ps.PlayerId);
            });
            
            // Character 설정
            modelBuilder.Entity<Character>(entity =>
            {
                entity.HasIndex(c => c.PlayerId); // 성능 최적화
                entity.HasIndex(c => c.Level);
                entity.HasOne(c => c.Player)
                      .WithMany(p => p.Characters)
                      .HasForeignKey(c => c.PlayerId);
            });
            
            // 데이터 시딩 (기본 아이템들)
            SeedItemTemplates(modelBuilder);
        }
        
        private void SeedItemTemplates(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ItemTemplate>().HasData(
                // 기본 무기들
                new ItemTemplate 
                { 
                    Id = 1, 
                    Name = "나무 검", 
                    Description = "초보자용 검",
                    ItemType = "weapon",
                    Rarity = "common",
                    BaseStats = """{"attack": 5, "durability": 100}""",
                    SellPrice = 10
                },
                new ItemTemplate 
                { 
                    Id = 2, 
                    Name = "철 검", 
                    Description = "튼튼한 철제 검",
                    ItemType = "weapon", 
                    Rarity = "rare",
                    BaseStats = """{"attack": 15, "durability": 200}""",
                    SellPrice = 50
                }
                // 더 많은 아이템들...
            );
        }
    }
}
```

## 🎮 Day 3-4: ASP.NET Core Web API - 플레이어 관리 시스템

### 🎯 학습 목표

Unity의 GameManager처럼 중앙에서 게임 상태를 관리하는 **플레이어 관리 API**를 구축합니다. 회원가입부터 캐릭터 생성, 기본 게임플레이까지의 핵심 엔드포인트를 만듭니다.

### 🏗️ Clean Architecture 적용

#### Application Layer - Services
```csharp
// IdleRPG.Application/Services/IPlayerService.cs
namespace IdleRPG.Application.Services
{
    public interface IPlayerService
    {
        Task<PlayerDto> CreatePlayerAsync(CreatePlayerDto dto);
        Task<PlayerDto> GetPlayerByIdAsync(Guid playerId);
        Task<PlayerStatsDto> GetPlayerStatsAsync(Guid playerId);
        Task<bool> UpdatePlayerStatsAsync(Guid playerId, UpdatePlayerStatsDto dto);
        Task<List<PlayerDto>> GetTopPlayersByLevelAsync(int count = 10);
    }
    
    public class PlayerService : IPlayerService
    {
        private readonly GameDbContext _context;
        private readonly ILogger<PlayerService> _logger;
        
        public PlayerService(GameDbContext context, ILogger<PlayerService> logger)
        {
            _context = context;
            _logger = logger;
        }
        
        public async Task<PlayerDto> CreatePlayerAsync(CreatePlayerDto dto)
        {
            // Unity의 Instantiate와 유사한 패턴
            var player = new Player
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };
            
            // 기본 스탯 생성 (Unity의 기본 컴포넌트 추가와 유사)
            player.Stats = new PlayerStats
            {
                PlayerId = player.Id,
                Level = 1,
                Experience = 0,
                Gold = 1000,
                Gems = 10
            };
            
            _context.Players.Add(player);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"New player created: {player.Username} (ID: {player.Id})");
            
            return MapToDto(player);
        }
        
        public async Task<PlayerStatsDto> GetPlayerStatsAsync(Guid playerId)
        {
            var stats = await _context.PlayerStats
                .Include(ps => ps.Player)
                .FirstOrDefaultAsync(ps => ps.PlayerId == playerId);
                
            if (stats == null)
                throw new NotFoundException($"Player stats not found for ID: {playerId}");
                
            return new PlayerStatsDto
            {
                PlayerId = stats.PlayerId,
                Username = stats.Player.Username,
                Level = stats.Level,
                Experience = stats.Experience,
                Gold = stats.Gold,
                Gems = stats.Gems,
                OfflineHours = stats.OfflineHours
            };
        }
        
        // 방치형 게임 핵심: 오프라인 보상 계산
        public async Task<OfflineRewardDto> CalculateOfflineRewardsAsync(Guid playerId)
        {
            var player = await _context.Players
                .Include(p => p.Stats)
                .FirstOrDefaultAsync(p => p.Id == playerId);
                
            if (player == null)
                throw new NotFoundException("Player not found");
                
            var offlineHours = (DateTime.UtcNow - player.LastLogin).TotalHours;
            
            // 최대 12시간까지만 보상 (방치형 게임 밸런싱)
            var rewardHours = Math.Min(offlineHours, 12.0);
            var hourlyGold = player.Stats.Level * 100; // 레벨당 시간당 100골드
            var hourlyExp = player.Stats.Level * 50;   // 레벨당 시간당 50경험치
            
            var reward = new OfflineReward
            {
                PlayerId = playerId,
                OfflineStart = player.LastLogin,
                OfflineEnd = DateTime.UtcNow,
                CalculatedGold = (long)(rewardHours * hourlyGold),
                CalculatedExp = (long)(rewardHours * hourlyExp),
                BonusMultiplier = 1.0m
            };
            
            _context.OfflineRewards.Add(reward);
            await _context.SaveChangesAsync();
            
            return new OfflineRewardDto
            {
                OfflineHours = rewardHours,
                GoldReward = reward.CalculatedGold,
                ExpReward = reward.CalculatedExp,
                BonusMultiplier = reward.BonusMultiplier
            };
        }
    }
}
```

#### API Controllers
```csharp
// IdleRPG.API/Controllers/PlayersController.cs
using Microsoft.AspNetCore.Mvc;
using IdleRPG.Application.Services;

namespace IdleRPG.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlayersController : ControllerBase
    {
        private readonly IPlayerService _playerService;
        private readonly ILogger<PlayersController> _logger;
        
        public PlayersController(IPlayerService playerService, ILogger<PlayersController> logger)
        {
            _playerService = playerService;
            _logger = logger;
        }
        
        /// <summary>
        /// 새로운 플레이어 계정 생성 (회원가입)
        /// </summary>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PlayerDto>> Register([FromBody] CreatePlayerDto dto)
        {
            try
            {
                var player = await _playerService.CreatePlayerAsync(dto);
                return CreatedAtAction(nameof(GetPlayer), new { id = player.Id }, player);
            }
            catch (DuplicateException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        /// <summary>
        /// 플레이어 기본 정보 조회
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PlayerDto>> GetPlayer(Guid id)
        {
            try
            {
                var player = await _playerService.GetPlayerByIdAsync(id);
                return Ok(player);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }
        
        /// <summary>
        /// 플레이어 스탯 조회
        /// </summary>
        [HttpGet("{id}/stats")]
        public async Task<ActionResult<PlayerStatsDto>> GetPlayerStats(Guid id)
        {
            var stats = await _playerService.GetPlayerStatsAsync(id);
            return Ok(stats);
        }
        
        /// <summary>
        /// 오프라인 보상 계산 및 지급 (방치형 게임 핵심!)
        /// </summary>
        [HttpPost("{id}/offline-rewards")]
        public async Task<ActionResult<OfflineRewardDto>> ClaimOfflineRewards(Guid id)
        {
            var reward = await _playerService.CalculateOfflineRewardsAsync(id);
            
            _logger.LogInformation($"Player {id} claimed offline rewards: {reward.GoldReward} gold, {reward.ExpReward} exp");
            
            return Ok(reward);
        }
        
        /// <summary>
        /// 레벨 랭킹 조회 (Top 100)
        /// </summary>
        [HttpGet("leaderboard")]
        public async Task<ActionResult<List<PlayerDto>>> GetLeaderboard([FromQuery] int count = 10)
        {
            var topPlayers = await _playerService.GetTopPlayersByLevelAsync(count);
            return Ok(topPlayers);
        }
    }
}
```

### 📱 Unity 클라이언트 연동

#### Unity API 클라이언트
```csharp
// Unity Project: Scripts/APIClient/GameAPIClient.cs
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using Newtonsoft.Json;

public class GameAPIClient : MonoBehaviour
{
    [SerializeField] private string baseURL = "http://localhost:8080/api";
    [SerializeField] private bool useHttps = false;
    
    private string authToken;
    
    public static GameAPIClient Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 플레이어 등록 (회원가입)
    /// </summary>
    public IEnumerator RegisterPlayer(string username, string email, string password, 
        System.Action<PlayerData> onSuccess, System.Action<string> onError)
    {
        var registerData = new
        {
            username = username,
            email = email,
            password = password
        };
        
        string json = JsonConvert.SerializeObject(registerData);
        
        using (UnityWebRequest request = UnityWebRequest.Post($"{baseURL}/players/register", json, "application/json"))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                var playerData = JsonConvert.DeserializeObject<PlayerData>(request.downloadHandler.text);
                onSuccess?.Invoke(playerData);
            }
            else
            {
                var errorMsg = request.error ?? "Registration failed";
                Debug.LogError($"Registration error: {errorMsg}");
                onError?.Invoke(errorMsg);
            }
        }
    }
    
    /// <summary>
    /// 플레이어 스탯 조회
    /// </summary>
    public IEnumerator GetPlayerStats(System.Guid playerId, 
        System.Action<PlayerStatsData> onSuccess, System.Action<string> onError)
    {
        using (UnityWebRequest request = UnityWebRequest.Get($"{baseURL}/players/{playerId}/stats"))
        {
            if (!string.IsNullOrEmpty(authToken))
            {
                request.SetRequestHeader("Authorization", $"Bearer {authToken}");
            }
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                var statsData = JsonConvert.DeserializeObject<PlayerStatsData>(request.downloadHandler.text);
                onSuccess?.Invoke(statsData);
            }
            else
            {
                onError?.Invoke($"Failed to get player stats: {request.error}");
            }
        }
    }
    
    /// <summary>
    /// 오프라인 보상 수령 (방치형 게임 핵심 기능!)
    /// </summary>
    public IEnumerator ClaimOfflineRewards(System.Guid playerId,
        System.Action<OfflineRewardData> onSuccess, System.Action<string> onError)
    {
        using (UnityWebRequest request = UnityWebRequest.Post($"{baseURL}/players/{playerId}/offline-rewards", ""))
        {
            if (!string.IsNullOrEmpty(authToken))
            {
                request.SetRequestHeader("Authorization", $"Bearer {authToken}");
            }
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                var rewardData = JsonConvert.DeserializeObject<OfflineRewardData>(request.downloadHandler.text);
                onSuccess?.Invoke(rewardData);
                
                Debug.Log($"Offline rewards claimed: {rewardData.goldReward} gold, {rewardData.expReward} exp");
            }
            else
            {
                onError?.Invoke($"Failed to claim offline rewards: {request.error}");
            }
        }
    }
}

// Unity Data Models
[System.Serializable]
public class PlayerData
{
    public string id;
    public string username;
    public string email;
    public string createdAt;
}

[System.Serializable]
public class PlayerStatsData
{
    public string playerId;
    public string username;
    public int level;
    public long experience;
    public long gold;
    public int gems;
    public double offlineHours;
}

[System.Serializable]
public class OfflineRewardData
{
    public double offlineHours;
    public long goldReward;
    public long expReward;
    public float bonusMultiplier;
}
```

## ✅ Week 1 완료 체크리스트

### 🎯 학습 목표 달성 확인

- [ ] **환경 설정 완료**
  - [ ] Rider + .NET 8 + PostgreSQL + Docker 설치
  - [ ] 프로젝트 생성 및 Clean Architecture 구조
  - [ ] GitHub 저장소 연결
  
- [ ] **데이터베이스 설계**
  - [ ] 방치형 RPG 핵심 테이블 설계 (Players, Characters, Items, OfflineRewards)
  - [ ] Entity Framework Core 모델 생성
  - [ ] 마이그레이션 적용
  
- [ ] **API 구현**
  - [ ] 플레이어 CRUD API 완성
  - [ ] 오프라인 보상 API 구현 (방치형 게임 핵심!)
  - [ ] Swagger 문서화 완료
  
- [ ] **Unity 연동**
  - [ ] Unity 클라이언트에서 API 호출 성공
  - [ ] 플레이어 등록 → 스탯 조회 → 오프라인 보상 수령 플로우 완성

### 🧪 실습 과제

#### 필수 과제
1. **플레이어 등록 API 테스트**
   - Swagger에서 새 플레이어 생성
   - 생성된 플레이어의 스탯 조회
   - Unity 클라이언트에서 동일한 동작 구현

2. **오프라인 보상 시스템 테스트**
   - 플레이어 마지막 로그인 시간을 과거로 설정
   - 오프라인 보상 API 호출
   - 골드와 경험치가 정확히 계산되는지 확인

#### 도전 과제
1. **자동 전투 시스템 구현**
   - 캐릭터가 몬스터와 자동으로 전투
   - 승리 시 경험치와 아이템 획득
   - 패배 시 일정 시간 후 부활

2. **길드 시스템 기초**
   - 길드 생성/가입 API
   - 길드원 목록 조회
   - 길드 랭킹 시스템

이제 방치형 RPG 서버의 기초가 완성되었습니다! 🎉
Unity 개발자로서 가지고 계신 게임 로직 구현 능력과 서버 개발 지식이 결합되어, 곧 완전한 게임 서버를 구축하실 수 있을 것입니다!
