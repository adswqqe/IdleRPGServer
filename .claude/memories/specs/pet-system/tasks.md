# Tasks: Pet System

> 이 문서는 펫 시스템 기능의 구현 작업 목록입니다.
>
> **작성 가이드**:
> - Design 문서의 모든 컴포넌트를 구현 가능한 작업으로 분해
> - 각 작업은 독립적으로 완료 및 테스트 가능해야 함
> - 순서는 의존성을 고려하여 정렬 (Domain → Infrastructure → Application → API)

---

## 📊 Progress Overview

**전체 진행률**: 0/27 (0%)

| Milestone | 작업 수 | 완료 | 진행률 |
|-----------|---------|------|--------|
| Domain Layer | 5 | 0 | 0% |
| Infrastructure Layer | 6 | 0 | 0% |
| Application Layer | 5 | 0 | 0% |
| API Layer | 4 | 0 | 0% |
| Database | 3 | 0 | 0% |
| Testing & Documentation | 4 | 0 | 0% |

**예상 총 소요 시간**: ~18.5시간

---

## 🏗️ Milestone 1: Domain Layer

### 1.1 Create PetRarity Enum ⏱️ 15분
- [ ] Create `IdleRPG.Domain/Enums/PetRarity.cs`
- [ ] Define enum values: `Common = 0`, `Rare = 1`, `Epic = 2`, `Legendary = 3`
- [ ] Add XML documentation comment: "Probabilities: Common(60%), Rare(30%), Epic(9%), Legendary(1%)"

**Requirements**: [US-1]
**Design Reference**: [Domain Layer - Enums]

---

### 1.2 Create PetTemplate Entity ⏱️ 30분
- [ ] Create `IdleRPG.Domain/Entities/PetTemplate.cs`
- [ ] Add properties:
  - `int Id` (PK, Identity)
  - `string Name` (Required, MaxLength 100)
  - `PetRarity Rarity`
  - `int BaseAttack`
  - `int BaseDefense`
  - `string Description` (MaxLength 500)
  - `DateTime CreatedAt`, `DateTime UpdatedAt`
- [ ] Add XML documentation for each property

**Requirements**: [US-1, US-2]
**Design Reference**: [Data Model - PetTemplates Table]

---

### 1.3 Create CharacterPet Entity ⏱️ 45분
- [ ] Create `IdleRPG.Domain/Entities/CharacterPet.cs`
- [ ] Add properties:
  - `Guid Id` (PK)
  - `int PetTemplateId` (FK)
  - `Guid CharacterId` (FK)
  - `int Level` (Default 1, Range 1-50)
  - `int Experience` (Default 0)
  - `DateTime CreatedAt`, `DateTime UpdatedAt`
- [ ] Add navigation properties:
  - `PetTemplate PetTemplate`
  - `Character Character`
- [ ] **Implement Entity Methods** (스탯 버프 계산):
  - `int CalculateAttackBuff()`: BaseAttack + (Level - 1) * AttackPerLevel
  - `int CalculateDefenseBuff()`: BaseDefense + (Level - 1) * DefensePerLevel
  - AttackPerLevel: Common=2, Rare=4, Epic=8, Legendary=16
  - DefensePerLevel: Common=1, Rare=2, Epic=4, Legendary=8

**Requirements**: [US-2, US-3]
**Design Reference**: [Data Model - CharacterPets Table, Business Logic - 스탯 버프 계산]

---

### 1.4 Update Character Entity ⏱️ 15분
- [ ] Open `IdleRPG.Domain/Entities/Character.cs`
- [ ] Add property: `Guid? EquippedPetId` (Nullable FK to CharacterPet)
- [ ] Add navigation property: `ICollection<CharacterPet> CharacterPets` (1:N relationship)
- [ ] Add XML documentation for new properties

**Requirements**: [US-2]
**Design Reference**: [Data Model - Characters Table 수정]

---

### 1.5 Extend GachaLogicService for Pet Gacha ⏱️ 1시간
- [ ] Open `IdleRPG.Domain/Services/GachaLogicService.cs`
- [ ] Add method: `PetRarity DeterminePetRarity()` (확률: Legendary 1%, Epic 9%, Rare 30%, Common 60%)
- [ ] Add method: `PetTemplate SelectRandomPet(PetRarity rarity, IEnumerable<PetTemplate> availablePets)`
  - Filter by rarity
  - Return random pet from filtered list
  - Throw ArgumentException if no pets available
- [ ] Reuse existing `IRandomProvider` dependency
- [ ] Add XML documentation

**Requirements**: [US-1]
**Design Reference**: [Business Logic - 펫 가챠 확률 계산, Decision Log D5]

---

## 🔧 Milestone 2: Infrastructure Layer

### 2.1 Create PetTemplateRepository ⏱️ 30분
- [ ] Create `IdleRPG.Domain/Repositories/IPetTemplateRepository.cs` (interface)
  - `Task<PetTemplate?> GetAsync(int id, CancellationToken cancellationToken = default)`
  - `Task<IEnumerable<PetTemplate>> GetAllAsync(CancellationToken cancellationToken = default)`
  - `Task<IEnumerable<PetTemplate>> GetByRarityAsync(PetRarity rarity, CancellationToken cancellationToken = default)`
- [ ] Create `IdleRPG.Infrastructure/Repositories/PetTemplateRepository.cs` (implementation)
- [ ] Implement all interface methods using EF Core
- [ ] Use `AsNoTracking()` for read-only queries

**Requirements**: [US-1]
**Design Reference**: [Infrastructure Layer - Repositories]

---

### 2.2 Create CharacterPetRepository ⏱️ 45분
- [ ] Create `IdleRPG.Domain/Repositories/ICharacterPetRepository.cs` (interface)
  - `Task<CharacterPet?> GetAsync(Guid id, CancellationToken cancellationToken = default)`
  - `Task<IEnumerable<CharacterPet>> GetByCharacterIdAsync(Guid characterId, CancellationToken cancellationToken = default)`
  - `Task AddAsync(CharacterPet pet, CancellationToken cancellationToken = default)`
  - `Task UpdateAsync(CharacterPet pet, CancellationToken cancellationToken = default)`
  - `Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)`
- [ ] Create `IdleRPG.Infrastructure/Repositories/CharacterPetRepository.cs` (implementation)
- [ ] Use `Include(p => p.PetTemplate)` for GetByCharacterIdAsync (N+1 방지)

**Requirements**: [US-1, US-2, US-3, US-4]
**Design Reference**: [Infrastructure Layer - Repositories]

---

### 2.3 Create PetTemplate EF Core Configuration ⏱️ 20분
- [ ] Create `IdleRPG.Infrastructure/Configurations/PetTemplateConfiguration.cs`
- [ ] Implement `IEntityTypeConfiguration<PetTemplate>`
- [ ] Configure:
  - Table name: "PetTemplates"
  - Primary key: Id (Identity)
  - Name: Required, MaxLength(100)
  - Description: MaxLength(500)
  - Index: `IX_PetTemplates_Rarity` on Rarity column

**Requirements**: [US-1]
**Design Reference**: [Data Model - EF Core Configuration]

---

### 2.4 Create CharacterPet EF Core Configuration ⏱️ 30분
- [ ] Create `IdleRPG.Infrastructure/Configurations/CharacterPetConfiguration.cs`
- [ ] Implement `IEntityTypeConfiguration<CharacterPet>`
- [ ] Configure:
  - Table name: "CharacterPets"
  - Primary key: Id (Guid)
  - Level: Range 1-50
  - Experience: Min 0
  - Relationship: PetTemplate (N:1, OnDelete Restrict)
  - Relationship: Character (N:1, OnDelete Cascade)
  - Index: `IX_CharacterPets_CharacterId`
  - Index: `IX_CharacterPets_PetTemplateId`

**Requirements**: [US-1, US-2, US-3]
**Design Reference**: [Data Model - EF Core Configuration]

---

### 2.5 Update Character EF Core Configuration ⏱️ 15분
- [ ] Open `IdleRPG.Infrastructure/Configurations/CharacterConfiguration.cs`
- [ ] Add configuration for `EquippedPetId`:
  - Relationship: CharacterPet (0..1, OnDelete SetNull)
  - Index: `IX_Characters_EquippedPetId`
- [ ] Add configuration for `CharacterPets` navigation property:
  - HasMany(c => c.CharacterPets).WithOne(p => p.Character)

**Requirements**: [US-2]
**Design Reference**: [Data Model - Characters Table 수정]

---

### 2.6 Register Configurations in GameDBContext ⏱️ 10분
- [ ] Open `IdleRPG.Infrastructure/Data/GameDBContext.cs`
- [ ] Add DbSet properties:
  - `DbSet<PetTemplate> PetTemplates`
  - `DbSet<CharacterPet> CharacterPets`
- [ ] Register configurations in `OnModelCreating`:
  - `modelBuilder.ApplyConfiguration(new PetTemplateConfiguration())`
  - `modelBuilder.ApplyConfiguration(new CharacterPetConfiguration())`

**Requirements**: [All]
**Design Reference**: [Infrastructure Layer - Data Access]

---

## 📦 Milestone 3: Application Layer

### 3.1 Create Pet Request DTOs ⏱️ 30분
- [ ] Create `IdleRPG.Application/DTOs/Pet/PetGachaRequestDto.cs`:
  - `Guid CharacterId`
  - `int Count` (1 or 10)
- [ ] Create `IdleRPG.Application/DTOs/Pet/PetEquipRequestDto.cs`:
  - `bool Equip`
- [ ] Create `IdleRPG.Application/DTOs/Pet/PetLevelUpRequestDto.cs`:
  - `int ExperienceToAdd`
- [ ] Add XML documentation and validation attributes

**Requirements**: [US-1, US-2, US-3]
**Design Reference**: [API Design - Request DTOs]

---

### 3.2 Create Pet Response DTOs ⏱️ 45분
- [ ] Create `IdleRPG.Application/DTOs/Pet/PetDto.cs`:
  - `Guid PetId`, `int PetTemplateId`, `string Name`, `string Rarity`
  - `int Level`, `int Experience`, `int AttackBuff`, `int DefenseBuff`, `bool IsEquipped`
- [ ] Create `IdleRPG.Application/DTOs/Pet/PetDetailDto.cs` (extends PetDto):
  - Add `int NextLevelExperience`, `string Description`, `Guid CharacterId`
- [ ] Create `IdleRPG.Application/DTOs/Pet/PetGachaResponseDto.cs`:
  - `List<PetDto> Pets`, `long RemainingCrystal`, `int Cost`
- [ ] Add XML documentation

**Requirements**: [US-1, US-2, US-3, US-4]
**Design Reference**: [API Design - Response DTOs]

---

### 3.3 Create IPetService Interface ⏱️ 20분
- [ ] Create `IdleRPG.Application/Services/IPetService.cs`
- [ ] Define methods:
  - `Task<PetGachaResponseDto> PerformGachaAsync(Guid characterId, int count, CancellationToken cancellationToken = default)`
  - `Task<List<PetDto>> GetCharacterPetsAsync(Guid characterId, CancellationToken cancellationToken = default)`
  - `Task<PetDetailDto> GetPetDetailAsync(Guid petId, Guid characterId, CancellationToken cancellationToken = default)`
  - `Task<PetDto> EquipPetAsync(Guid petId, Guid characterId, bool equip, CancellationToken cancellationToken = default)`
  - `Task<PetDetailDto> LevelUpPetAsync(Guid petId, Guid characterId, int experienceToAdd, CancellationToken cancellationToken = default)`
- [ ] Add XML documentation

**Requirements**: [US-1, US-2, US-3, US-4]
**Design Reference**: [Service Layer Design - IPetService]

---

### 3.4 Implement PetService (Part 1: Gacha & Query) ⏱️ 2시간
- [ ] Create `IdleRPG.Infrastructure/Services/PetService.cs`
- [ ] Inject dependencies:
  - `ICharacterRepository _characterRepository`
  - `ICharacterPetRepository _characterPetRepository`
  - `IPetTemplateRepository _petTemplateRepository`
  - `GachaLogicService _gachaLogicService`
  - `ILogger<PetService> _logger`
- [ ] Implement `PerformGachaAsync`:
  - Validate character exists
  - Calculate cost (1회=100, 10회=900)
  - Validate crystal balance
  - Deduct crystal
  - Loop count times: determine rarity → select pet → create CharacterPet
  - Save to DB (transaction)
  - Log gacha success (Info level)
  - Return response with pets + remaining crystal
- [ ] Implement `GetCharacterPetsAsync`:
  - Query pets with Include(PetTemplate)
  - Map to PetDto (calculate buffs, check isEquipped)
- [ ] Implement `GetPetDetailAsync`:
  - Validate ownership
  - Return PetDetailDto with NextLevelExperience

**Requirements**: [US-1, US-4]
**Design Reference**: [Service Layer Design - PerformGachaAsync, GetCharacterPetsAsync]

---

### 3.5 Implement PetService (Part 2: Equip & LevelUp) ⏱️ 1.5시간
- [ ] Implement `EquipPetAsync` in `PetService.cs`:
  - Validate pet ownership
  - If equip=true: Set character.EquippedPetId = petId (auto-unequip previous)
  - If equip=false: Set character.EquippedPetId = null
  - Save character
  - Log equip/unequip action (Info level)
  - Return PetDto
- [ ] Implement `LevelUpPetAsync`:
  - Validate pet ownership
  - Validate gold balance
  - Validate max level (50)
  - Deduct gold
  - Add experience
  - Loop: Check if level up required (Level * 1000 exp)
    - Increment level
    - Deduct experience
    - Stop at max level
  - Save pet and character
  - Log levelup (Info level)
  - Return PetDetailDto with new level + buffs

**Requirements**: [US-2, US-3]
**Design Reference**: [Service Layer Design - EquipPetAsync, LevelUpPetAsync]

---

## 🌐 Milestone 4: API Layer

### 4.1 Create PetController ⏱️ 30분
- [ ] Create `IdleRPG.API/Controllers/PetController.cs`
- [ ] Add attributes: `[ApiController]`, `[Route("api/pets")]`, `[Authorize]`
- [ ] Inject `IPetService _petService` and `ILogger<PetController> _logger`
- [ ] Add XML documentation for controller

**Requirements**: [All]
**Design Reference**: [API Layer - Controllers]

---

### 4.2 Implement POST /api/pets/gacha Endpoint ⏱️ 45분
- [ ] Add `[HttpPost("gacha")]` method in PetController
- [ ] Validate request: count must be 1 or 10
- [ ] Call `_petService.PerformGachaAsync`
- [ ] Handle exceptions:
  - `InvalidOperationException` → 400 Bad Request
  - `NotFoundException` → 404 Not Found
  - `Exception` → 500 Internal Server Error
- [ ] Add Swagger documentation (ProducesResponseType)

**Requirements**: [US-1]
**Design Reference**: [API Design - POST /api/pets/gacha]

---

### 4.3 Implement GET /api/pets and GET /api/pets/{id} Endpoints ⏱️ 45분
- [ ] Add `[HttpGet]` method with `characterId` query parameter:
  - Call `_petService.GetCharacterPetsAsync`
  - Return 200 OK with pet list
- [ ] Add `[HttpGet("{id}")]` method:
  - Extract `characterId` from JWT claims (or query param)
  - Call `_petService.GetPetDetailAsync`
  - Return 404 if not found
  - Return 403 if ownership mismatch
- [ ] Add Swagger documentation

**Requirements**: [US-4]
**Design Reference**: [API Design - GET /api/pets, GET /api/pets/{id}]

---

### 4.4 Implement PUT /api/pets/{id}/equip and PUT /api/pets/{id}/levelup Endpoints ⏱️ 1시간
- [ ] Add `[HttpPut("{id}/equip")]` method:
  - Parse request body (`PetEquipRequestDto`)
  - Extract `characterId` from JWT
  - Call `_petService.EquipPetAsync`
  - Handle 403 Forbidden
- [ ] Add `[HttpPut("{id}/levelup")]` method:
  - Parse request body (`PetLevelUpRequestDto`)
  - Validate `experienceToAdd > 0`
  - Extract `characterId` from JWT
  - Call `_petService.LevelUpPetAsync`
  - Handle 400 (gold insufficient, max level)
- [ ] Add Swagger documentation

**Requirements**: [US-2, US-3]
**Design Reference**: [API Design - PUT /api/pets/{id}/equip, PUT /api/pets/{id}/levelup]

---

## 🗃️ Milestone 5: Database

### 5.1 Create Database Migration ⏱️ 1시간
- [ ] Add migration to `IdleRPG.Infrastructure/migration.sql`
- [ ] Use `DO $EF$ BEGIN ... END $EF$` pattern (idempotent)
- [ ] **Create PetTemplates Table**:
  ```sql
  CREATE TABLE "PetTemplates" (
      "Id" serial PRIMARY KEY,
      "Name" varchar(100) NOT NULL,
      "Rarity" integer NOT NULL,
      "BaseAttack" integer NOT NULL,
      "BaseDefense" integer NOT NULL,
      "Description" text,
      "CreatedAt" timestamp NOT NULL DEFAULT now(),
      "UpdatedAt" timestamp NOT NULL DEFAULT now()
  );
  CREATE INDEX "IX_PetTemplates_Rarity" ON "PetTemplates" ("Rarity");
  ```
- [ ] **Create CharacterPets Table**:
  ```sql
  CREATE TABLE "CharacterPets" (
      "Id" uuid PRIMARY KEY,
      "PetTemplateId" integer NOT NULL,
      "CharacterId" uuid NOT NULL,
      "Level" integer NOT NULL DEFAULT 1 CHECK ("Level" >= 1 AND "Level" <= 50),
      "Experience" integer NOT NULL DEFAULT 0 CHECK ("Experience" >= 0),
      "CreatedAt" timestamp NOT NULL DEFAULT now(),
      "UpdatedAt" timestamp NOT NULL DEFAULT now(),
      CONSTRAINT "FK_CharacterPets_PetTemplates" FOREIGN KEY ("PetTemplateId") REFERENCES "PetTemplates" ("Id") ON DELETE RESTRICT,
      CONSTRAINT "FK_CharacterPets_Characters" FOREIGN KEY ("CharacterId") REFERENCES "Characters" ("Id") ON DELETE CASCADE
  );
  CREATE INDEX "IX_CharacterPets_CharacterId" ON "CharacterPets" ("CharacterId");
  CREATE INDEX "IX_CharacterPets_PetTemplateId" ON "CharacterPets" ("PetTemplateId");
  ```
- [ ] **Alter Characters Table**:
  ```sql
  ALTER TABLE "Characters" ADD COLUMN IF NOT EXISTS "EquippedPetId" uuid;
  ALTER TABLE "Characters" ADD CONSTRAINT "FK_Characters_CharacterPets" FOREIGN KEY ("EquippedPetId") REFERENCES "CharacterPets" ("Id") ON DELETE SET NULL;
  CREATE INDEX IF NOT EXISTS "IX_Characters_EquippedPetId" ON "Characters" ("EquippedPetId");
  ```
- [ ] Test migration locally (apply to test DB)

**Requirements**: [All]
**Design Reference**: [Migration Plan]

---

### 5.2 Create PetTemplateSeeder ⏱️ 45분
- [ ] Create `IdleRPG.Infrastructure/Seeders/PetTemplateSeeder.cs`
- [ ] Add `SeedAsync(GameDBContext context)` method
- [ ] Check if data exists: `if (await context.PetTemplates.AnyAsync()) return;`
- [ ] Add sample pet templates (8개):
  - **Common** (2개): 슬라임 (Attack=5, Defense=3), 고블린 (5, 3)
  - **Rare** (2개): 아이스 캣 (10, 6), 썬더 울프 (10, 6)
  - **Epic** (2개): 파이어 독 (20, 12), 다크 팬서 (20, 12)
  - **Legendary** (2개): 드래곤 해츨링 (40, 24), 페닉스 (40, 24)
- [ ] Save to DB

**Requirements**: [US-1]
**Design Reference**: [Data Seeding - PetTemplateSeeder]

---

### 5.3 Register Dependencies in DI Container ⏱️ 15분
- [ ] Open `IdleRPG.API/Program.cs`
- [ ] Register repositories:
  - `builder.Services.AddScoped<IPetTemplateRepository, PetTemplateRepository>();`
  - `builder.Services.AddScoped<ICharacterPetRepository, CharacterPetRepository>();`
- [ ] Register service:
  - `builder.Services.AddScoped<IPetService, PetService>();`
- [ ] Note: `GachaLogicService` already registered (reused)

**Requirements**: [All]
**Design Reference**: [Architecture Overview - DI]

---

## 🧪 Milestone 6: Testing & Documentation

### 6.1 Create GachaLogicService Pet Tests ⏱️ 1시간
- [ ] Create `IdleRPG.Tests/Domain/Services/GachaLogicServicePetTests.cs`
- [ ] Mock `IRandomProvider`
- [ ] Test cases:
  - `DeterminePetRarity_RandomValue0_ReturnsLegendary`: rand=0 → Legendary
  - `DeterminePetRarity_RandomValue5_ReturnsEpic`: rand=5 → Epic
  - `DeterminePetRarity_RandomValue20_ReturnsRare`: rand=20 → Rare
  - `DeterminePetRarity_RandomValue50_ReturnsCommon`: rand=50 → Common
  - `SelectRandomPet_ValidRarity_ReturnsRandomPet`: 해당 등급 펫 반환
  - `SelectRandomPet_NoAvailablePets_ThrowsArgumentException`: 예외 발생
- [ ] Use AAA pattern and FluentAssertions

**Requirements**: [US-1]
**Design Reference**: [Testing Strategy - Unit Tests]

---

### 6.2 Create CharacterPet Entity Method Tests ⏱️ 45분
- [ ] Create `IdleRPG.Tests/Domain/Entities/CharacterPetTests.cs`
- [ ] Test cases:
  - `CalculateAttackBuff_CommonLevel1_Returns5`: Common Lv1 → 5
  - `CalculateAttackBuff_LegendaryLevel10_Returns184`: Legendary Lv10 → 184
  - `CalculateDefenseBuff_EpicLevel5_Returns28`: Epic Lv5 → 28
  - `CalculateAttackBuff_RareLevel50_Returns206`: Rare Lv50 → 206
- [ ] Achieve 100% coverage for calculation methods

**Requirements**: [US-2, US-3]
**Design Reference**: [Testing Strategy - Entity Methods]

---

### 6.3 Create PetService Unit Tests ⏱️ 2시간
- [ ] Create `IdleRPG.Tests/Application/Services/PetServiceTests.cs`
- [ ] Mock all dependencies (repositories, GachaLogicService, logger)
- [ ] Test cases for `PerformGachaAsync`:
  - `PerformGachaAsync_SufficientCrystal_ReturnsOnePet`: 성공 시나리오
  - `PerformGachaAsync_InsufficientCrystal_ThrowsException`: 크리스탈 부족
  - `PerformGachaAsync_Count10_Returns10Pets`: 10회 가챠
- [ ] Test cases for `EquipPetAsync`:
  - `EquipPetAsync_ValidPet_UpdatesEquippedPetId`: 장착 성공
  - `EquipPetAsync_OtherCharacterPet_ThrowsForbiddenException`: 타인 펫
- [ ] Test cases for `LevelUpPetAsync`:
  - `LevelUpPetAsync_SufficientGold_IncreasesLevel`: 레벨업 성공
  - `LevelUpPetAsync_MaxLevel_ThrowsException`: 최대 레벨
- [ ] Achieve 80%+ coverage

**Requirements**: [US-1, US-2, US-3]
**Design Reference**: [Testing Strategy - Application Services]

---

### 6.4 Create Unity Documentation ⏱️ 1.5시간
- [ ] Create folder: `../IdleRPGClient/Docs/unity/pet-system/`
- [ ] Create `API_SPEC.md`:
  - Copy content from design.md (lines 1025-1065)
  - Document all 5 endpoints with examples
- [ ] Create `DTOs.cs`:
  - Copy content from design.md (lines 1071-1151)
  - Ensure `[JsonProperty]` attributes correct
  - Test with Newtonsoft.Json
- [ ] Create `README.md`:
  - Copy content from design.md (lines 1157-1193)
  - Add Unity implementation guide
  - Add API usage examples with UnityWebRequest
- [ ] Update `../IdleRPGClient/Docs/unity/README.md`:
  - Add index entry: "Pet System - 펫 가챠, 장착, 육성 API"

**Requirements**: [All]
**Design Reference**: [Unity Client Integration]
**참고**: `docs/unity/UNITY_DOCUMENTATION_GUIDE.md`

---

## 🚀 Post-Implementation

### ✅ Completion Checklist
- [ ] All 27 tasks completed and tested
- [ ] Unit tests passing (90%+ coverage for Domain, 80%+ for Application)
- [ ] Integration tests passing (optional, but recommended)
- [ ] Migration applied via Jenkins CI/CD
- [ ] Unity documentation complete and reviewed
- [ ] PetTemplateSeeder executed successfully (8 pet templates)
- [ ] Code review completed by team
- [ ] Git commit with message: "feat: 펫 시스템 구현 (가챠, 장착, 육성) 🎮"
- [ ] Feature tested end-to-end (가챠 → 장착 → 레벨업 → 전투 버프 적용)

---

## 📝 Notes

### Blockers
<!-- 작업 중 발생한 장애 요소 기록 -->

### Decisions Made
<!-- 구현 중 추가로 내린 결정 기록 -->

**아키텍처 결정 (Requirements/Design에서 완료)**:
1. ✅ PetTemplate/CharacterPet 분리 (Template-Instance 패턴)
2. ✅ 스탯 버프 계산은 Entity 메서드 (`CharacterPet.CalculateAttackBuff()`)
3. ✅ 장착 상태는 `Character.EquippedPetId` (Nullable FK)
4. ✅ 가챠 로직은 `GachaLogicService` 재사용 (DRY 원칙)

### Future Improvements
- [ ] 펫 진화 시스템 (Epic → Legendary 진화)
- [ ] 펫 스킬 시스템 (펫별 고유 스킬)
- [ ] 펫 도감 (수집률 통계)
- [ ] 펫 합성 시스템 (중복 펫 → 강화 재료)
- [ ] 펫 장착 슬롯 확장 (VIP 기능)

---

**시작일**: YYYY-MM-DD
**완료일**: YYYY-MM-DD
**총 소요 시간**: ~18.5시간 (예상)
**Design 추적성**: [design.md의 모든 컴포넌트 커버 완료]
**Requirements 추적성**: [US-1, US-2, US-3, US-4 모두 Task에 매핑 완료]
