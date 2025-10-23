# Dungeon Stage System 구현 완료 (Week 3, Day 2 - 2025-10-18)

## 📋 구현 완료 항목 (Milestone 1 & 2)

### 1. Domain Layer 엔티티

**Enum**:
- `IdleRPG.Domain/Enums/DungeonDifficulty.cs`
  - Normal = 1, Hard = 2, Nightmare = 3

**ValueObject**:
- `IdleRPG.Domain/ValueObjects/DifficultyMultiplier.cs`
  - Gemini 2.5 Pro 제안: 인스턴스 패턴 (static 클래스 대신)
  - Factory 메서드: `Create(DungeonDifficulty difficulty)`
  - Properties: MonsterStatMultiplier, RewardMultiplier, DropChanceMultiplier
  - Immutable 패턴으로 구현

**Entities**:
- `IdleRPG.Domain/Entities/DungeonStage.cs`
  - PK: int Id (자동 증가, 스테이지 번호)
  - FK: Guid MonsterId (기존 Monster 스키마와 일관성)
  - Fields: Name, RequiredLevel, BaseExperience, BaseGold
  - Nullable: FirstClearBonusExp, FirstClearBonusGold
  
- `IdleRPG.Domain/Entities/CharacterDungeonProgress.cs`
  - PK: Guid Id
  - FK: Guid CharacterId
  - 3필드 진행도 트래킹 (Gemini 제안):
    - HighestStageClearedNormal
    - HighestStageClearedHard
    - HighestStageClearedNightmare
  - Timestamps: CreatedAt, UpdatedAt

**기존 Entity 수정**:
- `IdleRPG.Domain/Entities/BattleLog.cs`
  - 추가: `int? DungeonStageId` (던전 전투와 일반 전투 구분)

### 2. Repository Layer

**인터페이스**:
- `IdleRPG.Domain/Repositories/IDungeonStageRepository.cs`
  - `GetAccessibleStagesAsync(int characterLevel)`: 레벨별 접근 가능 스테이지
  - `GetByIdWithMonsterAsync(int stageId)`: Monster 포함 조회

- `IdleRPG.Domain/Repositories/ICharacterDungeonProgressRepository.cs`
  - `GetOrCreateByCharacterIdAsync(Guid characterId)`: 진행도 조회 또는 생성

**구현체**:
- `IdleRPG.Infrastructure/Repositories/DungeonStageRepository.cs`
  - RequiredLevel 인덱스 활용 (Option A)
  - Include(d => d.Monster) 지원
  
- `IdleRPG.Infrastructure/Repositories/CharacterDungeonProgressRepository.cs`
  - CharacterId Unique 인덱스 활용
  - GetOrCreate 패턴 구현 (없으면 기본값 0으로 생성)

### 3. EF Core Configuration

- `IdleRPG.Infrastructure/Data/Configurations/DungeonStageConfiguration.cs`
  - RequiredLevel 인덱스: `IX_DungeonStages_RequiredLevel`
  - MonsterId FK with Restrict (몬스터 삭제 방지)
  - Name MaxLength(100)

- `IdleRPG.Infrastructure/Data/Configurations/CharacterDungeonProgressConfiguration.cs`
  - CharacterId Unique 인덱스: `IX_CharacterDungeonProgresses_CharacterId_Unique`
  - CharacterId FK with Cascade (캐릭터 삭제 시 진행도도 삭제)
  - DefaultValue(0) for all HighestStageCleared fields

### 4. Infrastructure 통합

**GameDBContext**:
- `IdleRPG.Infrastructure/Data/GameDBContext.cs`
  - 추가: `DbSet<DungeonStage> DungeonStages`
  - 추가: `DbSet<CharacterDungeonProgress> CharacterDungeonProgresses`
  - ApplyConfigurationsFromAssembly() 자동 적용

**UnitOfWork**:
- `IdleRPG.Application/Interfaces/IUnitOfWork.cs`
  - 추가: `IDungeonStageRepository DungeonStages { get; }`
  - 추가: `ICharacterDungeonProgressRepository CharacterDungeonProgresses { get; }`

- `IdleRPG.Infrastructure/UnitOfWork/UnitOfWork.cs`
  - Lazy Initialization 패턴으로 Repository 구현
  - 모든 Repository가 같은 GameDBContext 공유 → 트랜잭션 보장

### 5. EF Core Migration

**Migration 생성**:
```bash
dotnet ef migrations add AddDungeonStageSystem --startup-project ../IdleRPG.API
```

**생성된 파일**:
- `Migrations/20251018112141_AddDungeonStageSystem.cs`
- `Migrations/GameDBContextModelSnapshot.cs` (업데이트)
- `migration.sql` (idempotent SQL 스크립트)

**Migration 내용**:
1. BattleLogs 테이블: DungeonStageId 컬럼 추가 (nullable)
2. CharacterDungeonProgresses 테이블 생성 (DEFAULT 0, CASCADE DELETE)
3. DungeonStages 테이블 생성 (IDENTITY PK, RESTRICT DELETE)
4. 인덱스 3개 생성:
   - CharacterId Unique
   - MonsterId (FK)
   - RequiredLevel (필터링)

### 6. 빌드 검증

```bash
dotnet build IdleRPGServer.sln
```
- 결과: 성공 (경고 28개, 에러 0개)
- 모든 레이어 컴파일 성공

---

## 🎯 설계 결정 사항 (AI 협업)

### Gemini 2.5 Pro 제안 채택

1. **ValueObject 패턴**: Static 클래스 대신 Immutable Instance
   - 이유: 더 나은 OOP, DDD 원칙 준수, 테스트 용이성

2. **3필드 진행도 트래킹**: 각 난이도별 별도 필드
   - 이유: 쿼리 성능, 간단한 비즈니스 로직, 확장성
   - 대안 거부: 단일 필드 (부족), 별도 테이블 (과도한 복잡성)

3. **RequiredLevel 인덱스 추가** (Option A)
   - 이유: 캐릭터 레벨별 스테이지 필터링 쿼리 최적화
   - 사용 시나리오: `GetAccessibleStagesAsync(int characterLevel)`

### Primary Key 선택

- **DungeonStage**: int (IDENTITY)
  - 이유: 순차적 스테이지 번호, 자연 키, 성능 우수
  
- **CharacterDungeonProgress**: Guid
  - 이유: 분산 환경 확장성, 기존 스키마 일관성

---

## 📁 생성/수정된 파일 목록

### 생성된 파일 (14개)

**Domain Layer (6)**:
1. IdleRPG.Domain/Enums/DungeonDifficulty.cs
2. IdleRPG.Domain/ValueObjects/DifficultyMultiplier.cs
3. IdleRPG.Domain/Entities/DungeonStage.cs
4. IdleRPG.Domain/Entities/CharacterDungeonProgress.cs
5. IdleRPG.Domain/Repositories/IDungeonStageRepository.cs
6. IdleRPG.Domain/Repositories/ICharacterDungeonProgressRepository.cs

**Infrastructure Layer (4)**:
7. IdleRPG.Infrastructure/Repositories/DungeonStageRepository.cs
8. IdleRPG.Infrastructure/Repositories/CharacterDungeonProgressRepository.cs
9. IdleRPG.Infrastructure/Data/Configurations/DungeonStageConfiguration.cs
10. IdleRPG.Infrastructure/Data/Configurations/CharacterDungeonProgressConfiguration.cs

**Migration (4)**:
11. IdleRPG.Infrastructure/Migrations/20251018112141_AddDungeonStageSystem.cs
12. IdleRPG.Infrastructure/Migrations/20251018112141_AddDungeonStageSystem.Designer.cs
13. IdleRPG.Infrastructure/Migrations/GameDBContextModelSnapshot.cs (업데이트)
14. migration.sql (업데이트)

### 수정된 파일 (4개)

1. IdleRPG.Domain/Entities/BattleLog.cs
2. IdleRPG.Infrastructure/Data/GameDBContext.cs
3. IdleRPG.Application/Interfaces/IUnitOfWork.cs
4. IdleRPG.Infrastructure/UnitOfWork/UnitOfWork.cs

---

## 🔜 다음 작업 (Milestone 3: Application Layer)

### Application Layer 구현 필요

**DTOs 생성**:
- DungeonStageDto (스테이지 정보)
- DungeonProgressDto (진행도 정보)
- EnterDungeonRequestDto (입장 요청)
- DungeonBattleResultDto (전투 결과)

**Service 인터페이스**:
- IDungeonService
  - GetAllStagesAsync()
  - GetCharacterProgressAsync(Guid characterId)
  - GetStageByIdAsync(int stageId)
  - EnterDungeonStageAsync(Guid characterId, int stageId, DungeonDifficulty difficulty)

**Service 구현**:
- DungeonService
  - 전투 시스템 연동 (IBattleService)
  - DifficultyMultiplier 적용
  - 진행도 업데이트 로직
  - First Clear 보너스 계산
  - UnitOfWork 트랜잭션 관리

### Seed Data 준비

- 초기 던전 스테이지 데이터 (10~20개 스테이지)
- 몬스터 연동 (기존 Monster 데이터 활용)

### API Layer

**Controller 생성**:
- DungeonsController
  - GET /api/dungeons (모든 스테이지)
  - GET /api/dungeons/progress (내 진행도)
  - GET /api/dungeons/{stageId} (특정 스테이지)
  - POST /api/dungeons/{stageId}/enter (입장 + 전투)

---

## 💡 학습 포인트

### Repository Pattern
- Lazy Initialization으로 메모리 효율
- 공유 DbContext로 트랜잭션 보장
- 도메인별 특화 메서드 vs 일반 CRUD 분리

### EF Core 인덱스 전략
- Unique Index: 비즈니스 제약 (1:1 관계)
- FK Index: 조인 성능
- Filter Index: WHERE 절 성능

### ValueObject vs Static Class
- ValueObject: 인스턴스, 불변, 테스트 용이
- Static Class: 간단하지만 OOP 원칙 위배

### Migration Best Practice
- Idempotent SQL (IF NOT EXISTS)
- Jenkins CI/CD 안전한 재실행
- Nullable 추가로 기존 데이터 영향 최소화

---

## 🛠️ 기술 스택 활용

- **Clean Architecture**: Domain → Application → Infrastructure 의존성 흐름
- **DDD**: ValueObject, Entity, Repository 패턴
- **EF Core 9.0**: Fluent API, Migration, Index 최적화
- **PostgreSQL**: IDENTITY, RESTRICT/CASCADE, Unique Index
- **Unit of Work**: 트랜잭션 관리, Lazy Loading

---

## ✅ 체크리스트

- [x] Domain Entities 생성
- [x] Repository 인터페이스 정의
- [x] Repository 구현
- [x] EF Core Configuration
- [x] GameDBContext 업데이트
- [x] UnitOfWork 업데이트
- [x] Migration 생성
- [x] Migration SQL 검증
- [x] 빌드 테스트
- [ ] 로컬 DB 적용 (선택)
- [ ] Application Layer 구현
- [ ] Seed Data 생성
- [ ] API Controller 구현
- [ ] Unity 문서 업데이트

---

## 📌 중요 노트

1. **Migration 적용**: 아직 로컬 DB에 적용 안 함 (재부팅 후 필요시 적용)
2. **Jenkins 배포**: migration.sql이 자동으로 RDS에 적용됨
3. **다음 세션**: Application Layer (DTOs, Service) 구현부터 시작
