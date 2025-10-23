# Clean Architecture 구조

> **참조 목적**: 새 스펙 작성 시 아키텍처 패턴 및 계층 구조를 참조합니다.

---

## 계층별 책임

### API Layer (`IdleRPG.API`)
**책임**: HTTP 요청 처리, 라우팅, 인증/인가

```
IdleRPG.API/
├── Controllers/          # API 엔드포인트
├── Middleware/           # 요청 파이프라인
├── Hubs/                 # SignalR 실시간 통신
├── Program.cs            # DI 설정, 애플리케이션 구성
└── appsettings.json      # 설정 파일
```

### Application Layer (`IdleRPG.Application`)
**책임**: 비즈니스 로직, DTO, 서비스 인터페이스

```
IdleRPG.Application/
├── DTOs/                 # 데이터 전송 객체
├── Services/             # 서비스 인터페이스
├── Commands/             # MediatR Commands (CQRS)
└── Queries/              # MediatR Queries (CQRS)
```

### Domain Layer (`IdleRPG.Domain`)
**책임**: 엔티티, 비즈니스 규칙, 도메인 서비스

```
IdleRPG.Domain/
├── Entities/             # 도메인 엔티티 (20+ 종류)
├── ValueObjects/         # 불변 값 객체 (DDD)
├── Services/             # 도메인 서비스
├── Repositories/         # 레포지토리 인터페이스
└── Enums/                # 열거형
```

### Infrastructure Layer (`IdleRPG.Infrastructure`)
**책임**: 데이터 액세스, 외부 서비스 연동

```
IdleRPG.Infrastructure/
├── Data/
│   └── GameDBContext.cs  # EF Core DbContext
├── Repositories/         # 레포지토리 구현
├── Services/             # 서비스 구현
├── Authentication/       # JWT
├── Configurations/       # EF Core Entity 설정
├── Migrations/           # EF Core 마이그레이션
└── Seeders/              # 초기 데이터 시딩
```

---

## 의존성 규칙

**Dependency Flow**: API → Application → Domain ← Infrastructure

- **API** → Application (DTO, 서비스 인터페이스 사용)
- **Application** → Domain (엔티티, 레포지토리 인터페이스 참조)
- **Infrastructure** → Application (인터페이스 구현)
- **Infrastructure** → Domain (엔티티 사용, 레포지토리 구현)

**핵심 원칙**: Domain은 다른 계층에 의존하지 않음 (Pure Business Logic)

---

## 주요 설계 패턴

### Repository Pattern
- **인터페이스**: Domain Layer (`IPlayerRepository`)
- **구현**: Infrastructure Layer (`PlayerRepository`)
- **DI 등록**: `Program.cs`에서 Scoped 라이프사이클

### Service Pattern
- **인터페이스**: Application Layer (`IAuthService`)
- **구현**: Infrastructure Layer (`AuthService`)
- **현재 사용 중**: CQRS(MediatR)는 필요시 점진적 도입

### ValueObject Pattern (DDD)
- **목적**: 불변 값 객체, 비즈니스 로직 캡슐화
- **예시**: `DifficultyMultiplier` (던전 난이도별 배율)
- **특징**: Immutable, Equality by Value

### DTO Pattern
- **목적**: 계층 간 데이터 전송, API 응답 구조화
- **위치**: `Application/DTOs/`
- **네이밍**: `[기능]Dto.cs`

---

## Dependency Injection

### Program.cs 설정 구조

```csharp
// Database
builder.Services.AddDbContext<GameDBContext>(options =>
    options.UseNpgsql(connectionString));

// Repositories (Scoped)
builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();

// Services (Scoped)
builder.Services.AddScoped<IAuthService, AuthService>();

// Domain Services (Scoped)
builder.Services.AddScoped<GachaLogicService>();

// Singletons
builder.Services.AddSingleton<IRandomProvider, SystemRandomProvider>();
```

**라이프사이클**:
- **Scoped**: 대부분의 서비스/레포지토리 (HTTP 요청당 인스턴스)
- **Singleton**: 상태 없는 유틸리티 (RandomProvider, IConfiguration)
- **Transient**: 거의 사용 안 함

---

## 데이터베이스 설계 원칙

### 기본키
- **GUID 사용**: 분산 환경 대비, 클라이언트에서 미리 생성 가능 (Player, Character, Equipment)
- **Int 사용**: 템플릿/마스터 데이터 (MonsterTemplate, SkillTemplate, DungeonStage)

### 관계 설정
- **1:N**: `Player` → `Characters`
- **1:N**: `Character` → `Equipments`
- **M:N**: `Character` ↔ `Skills` (PlayerSkill 중간 테이블)

### Soft Delete
- `IsDeleted` 플래그 사용 (실제 삭제 대신)
- 데이터 복구 가능, 통계 유지

### Timestamp
- `CreatedAt`, `UpdatedAt` (BaseEntity 상속)
- `LastLogin` (Player, Character)

---

## 비동기 프로그래밍 패턴

```csharp
public async Task<Character> GetCharacterAsync(Guid characterId, CancellationToken cancellationToken = default)
{
    return await _context.Characters
        .Include(c => c.Equipments)
        .FirstOrDefaultAsync(c => c.Id == characterId, cancellationToken);
}
```

---

## 방치형 게임 특화 패턴

### 오프라인 진행 계산

```csharp
public TimeSpan GetOfflineTime() => DateTime.UtcNow - LastLogin;
public int CalculateOfflineGold(TimeSpan offlineTime) => (int)(offlineTime.TotalHours * GoldPerHour);
```

### 계산 속성 패턴

```csharp
// Equipment.cs
public int BaseAttack { get; set; }          // 기본 공격력
public int EnhancementLevel { get; set; }    // 강화 수치
public int TotalAttack => BaseAttack + (EnhancementLevel * 5);  // 계산 속성
```

### Background Service
- `IHostedService` 구현 (자동 사냥, 일일 리셋)
- Redis 분산 락 (멀티 인스턴스 환경)

---

## Kiro 스펙 작성 시 참고

### Design 단계에서
- 이 문서의 계층별 책임을 참조하여 컴포넌트 배치
- Dependency Flow 준수 (Domain은 다른 계층에 의존 금지)
- 기본키 타입 결정 (Entity vs Template)

### Tasks 단계에서
- Milestone 순서: Domain → Infrastructure → Application → API
- DI 등록을 Milestone 5 (Database)에 포함
