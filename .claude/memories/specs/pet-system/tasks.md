# Tasks: Pet System

> 이 문서는 Pet System 기능의 구현 작업 목록입니다.
>
> **작성 방식**: Design 승인 후, 의존성 순서로 작업 분해
> **학습 초점**: 순차 구현, 테스트 주도 개발, Clean Architecture 실습

**Design 추적성**: [design.md](./design.md) - 모든 컴포넌트 커버

---

## 📊 Progress Overview

**전체 진행률**: 24/28 (86%)

| Milestone | 작업 수 | 완료 | 진행률 |
|-----------|---------|------|--------|
| Domain Layer | 6 | 6 | 100% |
| Infrastructure Layer | 6 | 6 | 100% |
| Application Layer | 4 | 4 | 100% |
| API Layer | 3 | 3 | 100% |
| Database | 3 | 3 | 100% |
| Testing & Documentation | 6 | 2 | 33% |

**예상 총 소요 시간**: ~18.5시간

---

## 🏗️ Milestone 1: Domain Layer

### 1.1 Create Pet Entity ⏱️ 45분 ✅
- [x] Create `IdleRPG.Domain/Entities/Pet.cs`
- [x] Add properties: Id (int), CharacterId (Guid), TemplateId (int), Level (int), CurrentAttack (int), CurrentMana (int)
- [x] Add CreatedAt, UpdatedAt (timestamp)
- [x] Add navigation properties: Character, PetTemplate

**Requirements**: [US-1, US-2]
**Design Reference**: Data Model - pets 테이블

---

### 1.2 Create PetTemplate Entity ⏱️ 30분 ✅
- [x] Create `IdleRPG.Domain/Entities/PetTemplate.cs`
- [x] Add properties: Id (int), Name (string), Rarity (공용 enum), BaseAttack (int), BaseMana (int)
- [x] ~~ImageUrl, Description 제거~~ (서버-클라이언트 관심사 분리)
- [x] Add navigation property: ICollection<Pet>

**Requirements**: [US-1]
**Design Reference**: Data Model - pet_templates 테이블
**🎓 아키텍처 개선**: ImageUrl, Description 제거 (클라이언트가 Id/Name 기반 리소스 매핑)

---

### 1.3 Create EquippedPets Entity ⏱️ 30분 ✅
- [x] Create `IdleRPG.Domain/Entities/EquippedPets.cs`
- [x] Add properties: CharacterId (Guid, PK), SlotIndex (int, PK), PetId (int, FK), EquippedAt (DateTime)
- [x] Add navigation properties: Character, Pet
- [x] **Note**: Composite PK (CharacterId, SlotIndex)

**Requirements**: [US-3]
**Design Reference**: Data Model - equipped_pets 테이블

---

### 1.4 Create Rarity Enum (공용) ⏱️ 15분 ✅
- [x] Create `IdleRPG.Domain/Enums/Rarity.cs` (PetRarity → 공용 Rarity로 변경)
- [x] Define enum values: Common = 0, Rare = 1, Epic = 2, Legendary = 3
- [x] Add XML documentation comments (Legendary 1%, Epic 9%, Rare 30%, Common 60%)

**Requirements**: [US-1]
**Design Reference**: Domain Layer - Enums
**🎓 아키텍처 결정**: PetRarity 대신 공용 Rarity enum 사용 (Skill, Equipment, Pet 공통)

---

### 1.5 Create PetGachaService Domain Service ⏱️ 1.5시간 ✅
- [x] Create `IdleRPG.Domain/Services/PetGachaService.cs`
- [x] Implement `DrawPet(int pityCount, IRandomProvider randomProvider)` method:
  - Hard Pity: pityCount >= 50 → return Legendary
  - Soft Pity: pityCount >= 40 → adjustedRate = 1 + (pityCount - 39)
  - Random 값 생성 (0-100)
  - 확률 구간 판정: [0, adjustedRate): Legendary, [adjustedRate, adjustedRate+9): Epic, ...
- [x] Add input validation (pityCount 0-50 범위)
- [x] Add XML documentation
- [x] Implement `GetPityCountAfterDraw()` helper method

**Requirements**: [US-1]
**Design Reference**: Business Logic - 펫 가챠 확률 계산

**🎓 결정 완료**: Domain Service 선택 (순수 비즈니스 로직, 재사용성)

---

### 1.6 Update Character Entity ⏱️ 15분 ✅
- [x] Open `IdleRPG.Domain/Entities/Character.cs`
- [x] Add property: `public int PetGachaCount { get; set; } = 0` (천장 카운터)
- [x] Add XML documentation (구분: 스킬 가챠 100회, 펫 가챠 50회)

**Requirements**: [US-1]
**Design Reference**: Data Model - characters 테이블 수정

---

## 🔧 Milestone 2: Infrastructure Layer

### 2.1 Create PetRepository ⏱️ 45분 ✅
- [x] Create `IdleRPG.Domain/Repositories/IPetRepository.cs` (interface)
  - `Task<Pet?> GetByIdAsync(int petId, CancellationToken cancellationToken = default)`
  - `Task<List<Pet>> GetByCharacterIdAsync(Guid characterId, CancellationToken cancellationToken = default)`
  - `Task<Pet?> GetDuplicateAsync(Guid characterId, int templateId, CancellationToken cancellationToken = default)`
  - `Task AddAsync(Pet pet, CancellationToken cancellationToken = default)`
  - `Task UpdateAsync(Pet pet, CancellationToken cancellationToken = default)`
  - `Task DeleteAsync(Pet pet, CancellationToken cancellationToken = default)`
- [x] Create `IdleRPG.Infrastructure/Repositories/PetRepository.cs` (implementation)
- [x] Implement all methods using EF Core (Include PetTemplate, Character)

**Requirements**: [US-1, US-2]
**Design Reference**: Infrastructure Layer - Repositories

---

### 2.2 Create PetTemplateRepository ⏱️ 30분 ✅
- [x] Create `IdleRPG.Domain/Repositories/IPetTemplateRepository.cs`
  - `Task<List<PetTemplate>> GetByRarityAsync(Rarity rarity, CancellationToken cancellationToken = default)` (공용 Rarity)
  - `Task<PetTemplate?> GetByIdAsync(int templateId, CancellationToken cancellationToken = default)`
  - `Task<List<PetTemplate>> GetAllAsync(CancellationToken cancellationToken = default)` (추가)
- [x] Create `IdleRPG.Infrastructure/Repositories/PetTemplateRepository.cs`

**Requirements**: [US-1]
**Design Reference**: Infrastructure Layer - Repositories

---

### 2.3 Create Pet EF Core Configuration ⏱️ 45분 ✅
- [x] Create `IdleRPG.Infrastructure/Configurations/PetConfiguration.cs`
- [x] Implement IEntityTypeConfiguration<Pet>
- [x] Configure:
  - Table name: "pets"
  - Primary key: Id (int, AUTO_INCREMENT)
  - FK: CharacterId (ON DELETE CASCADE)
  - FK: TemplateId (ON DELETE RESTRICT)
  - Index: `idx_pets_character_id` on CharacterId
  - Property constraints: Level (1-50), CurrentAttack/Mana (Min 0)
  - Check constraints (PostgreSQL)

**Requirements**: [US-1, US-2]
**Design Reference**: Data Model - EF Core Configuration (PetConfiguration.cs)

**🎓 결정 완료**: 단일 컬럼 인덱스 (99% 사용 케이스, 500+ 펫 시 재평가)

---

### 2.4 Create PetTemplate EF Core Configuration ⏱️ 30분 ✅
- [x] Create `IdleRPG.Infrastructure/Configurations/PetTemplateConfiguration.cs`
- [x] Implement IEntityTypeConfiguration<PetTemplate>
- [x] Configure:
  - Table name: "pet_templates"
  - Primary key: Id (int, SEQUENCE)
  - UNIQUE: Name (uk_pet_templates_name)
  - Index: `idx_pet_templates_rarity` on Rarity
  - Property constraints: Name (MaxLength 50), Rarity (Range 0-3)
  - Enum conversion: Rarity (HasConversion<int>)
  - Check constraints (PostgreSQL)

**Requirements**: [US-1]
**Design Reference**: Data Model - EF Core Configuration (PetTemplateConfiguration.cs)

---

### 2.5 Create EquippedPets EF Core Configuration ⏱️ 45분 ✅
- [x] Create `IdleRPG.Infrastructure/Configurations/EquippedPetsConfiguration.cs`
- [x] Implement IEntityTypeConfiguration<EquippedPets>
- [x] Configure:
  - Table name: "equipped_pets"
  - **Composite PK**: (CharacterId, SlotIndex) - Fluent API 필수
  - UNIQUE: PetId (uk_equipped_pets_pet_id) - 중복 장착 방지
  - FK: CharacterId (ON DELETE CASCADE)
  - FK: PetId (ON DELETE CASCADE)
  - Property constraint: SlotIndex (Range 1-3)
  - Check constraint (PostgreSQL)

**Requirements**: [US-3]
**Design Reference**: Data Model - EF Core Configuration (EquippedPetsConfiguration.cs)

---

### 2.6 Register EF Core Configurations in GameDBContext ⏱️ 15분 ✅
- [x] Open `IdleRPG.Infrastructure/Data/GameDBContext.cs`
- [x] Add DbSet<Pet>, DbSet<PetTemplate>, DbSet<EquippedPets>
- [x] Add `modelBuilder.ApplyConfiguration(new PetConfiguration())`
- [x] Add `modelBuilder.ApplyConfiguration(new PetTemplateConfiguration())`
- [x] Add `modelBuilder.ApplyConfiguration(new EquippedPetsConfiguration())`

**Requirements**: [All]
**Design Reference**: Infrastructure Layer

---

## 📦 Milestone 3: Application Layer

### 3.1 Create Pet Request DTOs ⏱️ 45분 ✅
- [x] Create `IdleRPG.Application/DTOs/Pet/PetGachaRequestDto.cs`
  - Properties: CharacterId (Guid), Count (int, 1 or 10)
- [x] Create `IdleRPG.Application/DTOs/Pet/PetLevelUpRequestDto.cs`
  - Properties: CharacterId (Guid)
- [x] Create `IdleRPG.Application/DTOs/Pet/PetEquipRequestDto.cs`
  - Properties: CharacterId (Guid), PetId (int), SlotIndex (int)
- [x] Add XML documentation

**Requirements**: [US-1, US-2, US-3]
**Design Reference**: API Design - Request

---

### 3.2 Create Pet Response DTOs ⏱️ 45분 ✅
- [x] Create `IdleRPG.Application/DTOs/Pet/PetDto.cs`
  - Properties: Id, TemplateName, RarityName, Level, CurrentAttack, CurrentMana, ImageUrl
- [x] Create `IdleRPG.Application/DTOs/Pet/PetGachaResponseDto.cs`
  - Properties: List<PetDto> Pets, List<DuplicateRewardDto> DuplicateRewards, CurrentPityCount, RemainingCrystal, TotalGoldFromDuplicates
- [x] Create `IdleRPG.Application/DTOs/Pet/DuplicateRewardDto.cs`
  - Properties: PetTemplateName, GoldReward
- [x] Create `IdleRPG.Application/DTOs/Pet/PetLevelUpResponseDto.cs`
  - Properties: PetId, NewLevel, NewAttack, NewMana, CostGold, RemainingGold
- [x] Create `IdleRPG.Application/DTOs/Pet/PetEquipResponseDto.cs`
  - Properties: CharacterId, List<EquippedPetDto> EquippedPets, TotalBuffAttack, TotalBuffMana
- [x] Create `IdleRPG.Application/DTOs/Pet/EquippedPetDto.cs` (helper DTO)
  - Properties: SlotIndex, PetId, PetName, Level, BuffAttack, BuffMana

**Requirements**: [US-1, US-2, US-3]
**Design Reference**: API Design - Response

---

### 3.3 Create PetService Interface ⏱️ 30분 ✅
- [x] Create `IdleRPG.Application/Services/IPetService.cs`
- [x] Define method signatures:
  - `Task<PetGachaResponseDto> DrawPetsAsync(Guid characterId, int count, CancellationToken cancellationToken)`
  - `Task<PetDto> GetPetByIdAsync(int petId, CancellationToken cancellationToken)`
  - `Task<List<PetDto>> GetPetsByCharacterIdAsync(Guid characterId, CancellationToken cancellationToken)`
  - `Task<PetLevelUpResponseDto> LevelUpPetAsync(int petId, Guid characterId, CancellationToken cancellationToken)`
  - `Task<PetEquipResponseDto> EquipPetAsync(Guid characterId, int petId, int slotIndex, CancellationToken cancellationToken)`
  - `Task UnequipPetAsync(Guid characterId, int slotIndex, CancellationToken cancellationToken)`
  - `Task<List<EquippedPetDto>> GetEquippedPetsAsync(Guid characterId, CancellationToken cancellationToken)`
  - `Task DeletePetAsync(int petId, Guid characterId, CancellationToken cancellationToken)`

**Requirements**: [US-1, US-2, US-3]
**Design Reference**: Service Layer Design

---

### 3.4 Create PetService Implementation ⏱️ 3시간 ✅
- [x] Create `IdleRPG.Infrastructure/Services/PetService.cs`
- [x] Inject dependencies: IPetRepository, IPetTemplateRepository, ICharacterRepository, PetGachaService, IRandomProvider, IUnitOfWork
- [x] Implement `DrawPetsAsync`:
  - Validation: count == 1 or 10, character.Crystal >= cost (100 per draw, 900 for 10)
  - Transaction: Crystal 차감 → Loop (count 횟수):
    - Call PetGachaService.DrawPet(character.PetGachaCount, randomProvider) → Rarity
    - GetByRarityAsync (PetTemplate 조회)
    - 중복 체크 (GetDuplicateAsync)
    - 중복: 골드 보상 (Common 100, Rare 500, Epic 2,000, Legendary 10,000)
    - 중복 아님: Pet Entity 생성 (Level 1, CurrentAttack/Mana = Base)
    - character.PetGachaCount += 1 (Legendary 획득 시 0 초기화)
  - SaveChangesAsync
  - Return PetGachaResponseDto
- [x] Implement `LevelUpPetAsync`:
  - Validation: pet.CharacterId == characterId, pet.Level < 50
  - 비용 계산: `(int)(100 * Math.Pow(1.5, pet.Level - 1))`
  - Character 골드 차감
  - 스탯 계산: `pet.CurrentAttack = template.BaseAttack + (pet.Level - 1) * 10` (Mana +5)
  - pet.Level += 1
  - SaveChangesAsync
  - Return PetLevelUpResponseDto
- [x] Implement `EquipPetAsync`:
  - Validation: slotIndex (1-3), pet.CharacterId == characterId
  - 기존 장착 상태 확인 (EquippedPets WHERE PetId) → 있으면 삭제
  - 슬롯 점유 확인 (EquippedPets WHERE CharacterId, SlotIndex) → 있으면 삭제
  - EquippedPets Entity 생성
  - SaveChangesAsync
  - 버프 계산: GetEquippedPetsAsync (Include Pet, Template) → Sum(Pet.CurrentAttack * 0.1)
  - Return PetEquipResponseDto
- [x] Implement other methods (CRUD)
- [x] Add logging (가챠 결과, 레벨업, 장착)

**Requirements**: [US-1, US-2, US-3]
**Design Reference**: Service Layer Design - PetService

**🎓 결정 완료**:
- 트랜잭션 경계: Service Layer (Unit of Work)
- 버프 계산 시점: 실시간 계산 (데이터 일관성 우선)

**🎓 추가 구현**:
- EquippedPets Repository 추가 (누락 보완)
- IUnitOfWork에 EquippedPets 등록

---

## 🌐 Milestone 4: API Layer

### 4.1 Create PetsController ⏱️ 2시간 ✅
- [x] Create `IdleRPG.API/Controllers/PetsController.cs`
- [x] Inject IPetService dependency
- [x] Implement `POST /api/pets/gacha`:
  - `[Authorize]` attribute
  - Extract PlayerId from JWT
  - Validate request DTO
  - Call PetService.DrawPetsAsync
  - Return 201 Created
  - Handle exceptions: 400 (Validation), 401 (Unauthorized), 404 (Not Found)
  - Swagger documentation
- [x] Implement `GET /api/pets?characterId={id}`:
  - **Public** (no [Authorize])
  - Query parameter: characterId (required), rarity (optional), sortBy (optional)
  - Call PetService.GetPetsByCharacterIdAsync
  - Return 200 OK
- [x] Implement `POST /api/pets/{petId}/level-up`:
  - `[Authorize]`
  - Validate request DTO
  - Call PetService.LevelUpPetAsync
  - Return 200 OK
  - Handle exceptions: 400 (Max level, Insufficient gold), 403 (Forbidden), 404 (Not Found)

**Requirements**: [US-1, US-2]
**Design Reference**: API Design - Endpoints (POST /api/pets/gacha, GET /api/pets, POST /api/pets/{id}/level-up)

---

### 4.2 Add Pet Equip/Unequip Endpoints ⏱️ 45분 ✅
- [x] Implement `POST /api/pets/equip`:
  - `[Authorize]`
  - Call PetService.EquipPetAsync
  - Return 200 OK
  - Handle exceptions: 400 (Invalid slot), 403, 404
- [x] Implement `POST /api/pets/unequip`:
  - `[Authorize]`
  - Call PetService.UnequipPetAsync
  - Return 204 No Content
  - Handle exceptions: 404 (No pet in slot)
- [x] Implement `GET /api/pets/equipped?characterId={id}`:
  - **Public**
  - Call PetService.GetEquippedPetsAsync
  - Return 200 OK

**Requirements**: [US-3]
**Design Reference**: API Design - Endpoints (POST /api/pets/equip, unequip, GET /api/pets/equipped)

---

### 4.3 Add Pet Detail & Delete Endpoints ⏱️ 30분 ✅
- [x] Implement `GET /api/pets/{petId}`:
  - **Public**
  - Call PetService.GetPetByIdAsync
  - Return 200 OK, 404 if not found
- [x] Implement `DELETE /api/pets/{petId}`:
  - `[Authorize]`
  - Validation: 장착된 펫은 삭제 불가 (400)
  - Call PetService.DeletePetAsync
  - Return 204 No Content
  - Handle exceptions: 400 (Equipped pet), 403, 404

**Requirements**: [US-1]
**Design Reference**: API Design - Endpoints (GET /api/pets/{id}, DELETE /api/pets/{id})

---

## 🗃️ Milestone 5: Database

### 5.1 Create Database Migration ⏱️ 1.5시간 ✅
- [x] Add migration to `IdleRPG.Infrastructure/migration.sql`
- [x] Use `DO $EF$ BEGIN ... END $EF$` pattern (idempotent):
  - **CREATE TABLE pets**:
    - id SERIAL PRIMARY KEY
    - character_id UUID NOT NULL REFERENCES characters(id) ON DELETE CASCADE
    - template_id INT NOT NULL REFERENCES pet_templates(id) ON DELETE RESTRICT
    - level INT NOT NULL DEFAULT 1 CHECK (level BETWEEN 1 AND 50)
    - current_attack INT NOT NULL CHECK (current_attack >= 0)
    - current_mana INT NOT NULL CHECK (current_mana >= 0)
    - created_at TIMESTAMP NOT NULL DEFAULT NOW()
    - updated_at TIMESTAMP NOT NULL DEFAULT NOW()
  - **CREATE INDEX idx_pets_character_id** ON pets(character_id)
  - **CREATE TABLE pet_templates**:
    - id SERIAL PRIMARY KEY
    - name VARCHAR(50) NOT NULL UNIQUE
    - rarity INT NOT NULL CHECK (rarity BETWEEN 0 AND 3)
    - base_attack INT NOT NULL CHECK (base_attack >= 0)
    - base_mana INT NOT NULL CHECK (base_mana >= 0)
    - ~~image_url VARCHAR(255)~~ (제거 - 서버-클라이언트 관심사 분리)
    - ~~description TEXT~~ (제거 - 서버-클라이언트 관심사 분리)
  - **CREATE INDEX idx_pet_templates_rarity** ON pet_templates(rarity)
  - **CREATE TABLE equipped_pets**:
    - character_id UUID NOT NULL REFERENCES characters(id) ON DELETE CASCADE
    - slot_index INT NOT NULL CHECK (slot_index BETWEEN 1 AND 3)
    - pet_id INT NOT NULL UNIQUE REFERENCES pets(id) ON DELETE CASCADE
    - equipped_at TIMESTAMP NOT NULL DEFAULT NOW()
    - PRIMARY KEY (character_id, slot_index)
  - **ALTER TABLE characters** ADD COLUMN pet_gacha_count INT NOT NULL DEFAULT 0 CHECK (pet_gacha_count BETWEEN 0 AND 50)
- [x] Migration ID: `20251027120000_AddPetSystem` (7 DO 블록)

**Requirements**: [All]
**Design Reference**: Migration Plan
**참고**: CLAUDE.md - Database Migration (Idempotent 패턴)

**🎓 결정 완료**:
- 인덱스 전략: 단일 컬럼 (character_id) - 500+ 펫 시 재평가
- Cascade Delete: ON DELETE CASCADE (DB 레벨, 성능/일관성 우선)
- ImageUrl, Description 제거: 서버-클라이언트 관심사 분리 (Task 1.2 결정 반영)

---

### 5.2 Create PetTemplate Seeder ⏱️ 45분 ✅
- [x] Create `IdleRPG.Infrastructure/Data/Seeders/PetTemplateSeeder.cs`
- [x] Add 11개 펫 템플릿 데이터 (AI 제안 - 판타지 테마):
  - **Common (5개)**: Slime (Attack 50, Mana 25), Wolf (60, 20), Bat (55, 30), Goblin (65, 15), Rabbit (45, 35)
  - **Rare (3개)**: Fire Fox (100, 50), Ice Wolf (110, 45), Thunder Eagle (105, 55)
  - **Epic (2개)**: Dark Dragon (200, 100), Light Phoenix (190, 110)
  - **Legendary (1개)**: Ancient Guardian (350, 200)
- [x] Implement idempotent seeding (`AnyAsync()` check)
- [x] Register in `Program.cs` (Seeder 등록 완료)

**Requirements**: [US-1]
**Design Reference**: Data Seeding 계획

---

### 5.3 Register Dependencies in DI Container ⏱️ 15분 ✅
- [x] Open `IdleRPG.API/Program.cs`
- [x] ~~Register repositories~~ (UnitOfWork 내부에서 Lazy 초기화, 별도 등록 불필요)
- [x] Register services:
  - `builder.Services.AddScoped<IPetService, PetService>()`
- [x] Register domain services:
  - `builder.Services.AddScoped<PetGachaService>()`

**Requirements**: [All]
**Design Reference**: Architecture Overview

**Note**:
- `IRandomProvider`는 이미 Singleton으로 등록되어 있음 (확인 완료)
- Repositories는 UnitOfWork 패턴으로 관리 (별도 DI 등록 불필요)

---

## 🧪 Milestone 6: Testing & Documentation

### 6.1 Create PetGachaService Unit Tests ⏱️ 2시간 ✅
- [x] Create `IdleRPG.Tests/Domain/Services/PetGachaServiceTests.cs`
- [x] Setup: Mock IRandomProvider
- [x] Test cases:
  - `DrawPet_PityCount50_ReturnsLegendary` (Hard Pity)
  - `DrawPet_PityCount45_IncreasesLegendaryRate` (Soft Pity, 6% → 7%)
  - `DrawPet_PityCount0_Roll0_ReturnsLegendary` (Mock Random 0 → Legendary 1%)
  - `DrawPet_PityCount0_Roll1to9_ReturnsEpic` (Mock Random 1-9 → Epic 9%)
  - `DrawPet_PityCount0_Roll10to39_ReturnsRare` (Mock Random 10-39 → Rare 30%)
  - `DrawPet_PityCount0_Roll40to99_ReturnsCommon` (Mock Random 40-99 → Common 60%)
- [x] Use AAA pattern (Arrange-Act-Assert)
- [x] Use FluentAssertions: `result.Should().Be(Rarity.Legendary)`
- [x] Achieve 95%+ code coverage (17 tests, all passed)

**Requirements**: [US-1]
**Design Reference**: Testing Strategy - Unit Tests (PetGachaServiceTests)

---

### 6.2 Create PetService Unit Tests ⏱️ 2시간
- [ ] Create `IdleRPG.Tests/Application/Services/PetServiceTests.cs`
- [ ] Mock: IPetRepository, IPetTemplateRepository, ICharacterRepository, PetGachaService, IRandomProvider
- [ ] Test cases:
  - `DrawPetsAsync_InsufficientCrystal_ThrowsValidationException`
  - `DrawPetsAsync_InvalidCount_ThrowsValidationException` (count != 1 and != 10)
  - `DrawPetsAsync_DuplicatePet_ReturnsGoldReward` (Mock GetDuplicateAsync returns existing pet)
  - `DrawPetsAsync_LegendaryDrawn_ResetsPityCount` (Mock PetGachaService returns Legendary)
  - `LevelUpPetAsync_MaxLevel_ThrowsValidationException` (pet.Level == 50)
  - `LevelUpPetAsync_InsufficientGold_ThrowsValidationException`
  - `LevelUpPetAsync_Success_UpdatesStats` (Check CurrentAttack = BaseAttack + (Level-1) * 10)
  - `EquipPetAsync_InvalidSlot_ThrowsValidationException` (slotIndex < 1 or > 3)
  - `EquipPetAsync_SlotOccupied_ReplacesExistingPet`
- [ ] Use Moq: `_mockRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(pet)`
- [ ] Use FluentAssertions
- [ ] Achieve 85%+ code coverage

**Requirements**: [US-1, US-2, US-3]
**Design Reference**: Testing Strategy - Unit Tests (PetServiceTests)

---

### 6.3 Create PetsController Integration Tests ⏱️ 1.5시간 (선택)
- [ ] Create `IdleRPG.Tests/API/Controllers/PetsControllerTests.cs`
- [ ] Setup: WebApplicationFactory (in-memory database)
- [ ] Test cases:
  - `POST_PetsGacha_Unauthorized_Returns401` (No JWT token)
  - `POST_PetsGacha_InsufficientCrystal_Returns400`
  - `POST_PetsGacha_Success_Returns201` (With JWT, verify DB changes)
  - `GET_Pets_Public_Returns200` (No auth required)
  - `POST_PetsLevelUp_Success_Returns200`
  - `POST_PetsLevelUp_MaxLevel_Returns400`
  - `POST_PetsEquip_Success_Returns200`
  - `DELETE_Pets_EquippedPet_Returns400` (Cannot delete equipped pet)
- [ ] Use HttpClient: `var response = await _client.PostAsync("/api/pets/gacha", content)`
- [ ] Verify status codes and response body
- [ ] Achieve 75%+ controller coverage

**Requirements**: [US-1, US-2, US-3]
**Design Reference**: Testing Strategy - Integration Tests

**Note**: 선택적 작업. Domain/Application 레이어 Unit Tests 완료로 핵심 로직 검증 완료. Manual E2E Testing (Task 6.6)으로 대체 가능.

---

### 6.4 Create PetGachaService Property-Based Tests (선택) ⏱️ 1시간
- [ ] Install FsCheck NuGet package (Property-Based Testing)
- [ ] Create `IdleRPG.Tests/Domain/Services/PetGachaServicePropertyTests.cs`
- [ ] Property tests:
  - `PityCount_AlwaysBetween0And50_ReturnsValidRarity`
  - `SoftPity_IncreasesLegendaryRate_AsCountIncreases` (pityCount 40-49 → Legendary 확률 증가 검증)
  - `RandomDistribution_Matches_ExpectedProbabilities` (1000회 시행 → 통계적 검증)
- [ ] Use FsCheck.Xunit: `[Property]` attribute

**Requirements**: [US-1]
**Design Reference**: Testing Strategy (Advanced)

**Note**: 선택적 작업 (학습 목적, 시간 여유 시)

---

### 6.5 Create Unity Documentation ⏱️ 1.5시간 ✅
- [x] Create `../IdleRPGClient/Docs/unity/pet-system/` folder
- [x] Create `API_SPEC.md`:
  - 8개 엔드포인트 상세 명세 (POST /api/pets/gacha, GET /api/pets, ...)
  - Request/Response JSON 예시
  - HTTP 상태 코드 및 에러 메시지
  - Unity C# 코드 예시 (UnityWebRequest):
    ```csharp
    var request = UnityWebRequest.Post("http://server/api/pets/gacha", jsonBody);
    request.SetRequestHeader("Authorization", "Bearer " + token);
    await request.SendWebRequest();
    ```
- [x] Create `DTOs.cs`:
  - Unity-compatible C# DTOs (PetGachaRequestDto, PetGachaResponseDto, PetDto, ...)
  - Use `[JsonProperty]` attributes (Newtonsoft.Json):
    ```csharp
    [JsonObject(MemberSerialization.OptIn)]
    public class PetDto {
        [JsonProperty("id")] public int Id;
        [JsonProperty("templateName")] public string TemplateName;
        // ...
    }
    ```
- [x] Create `INTEGRATION_GUIDE.md`:
  - 펫 UI 연동 (가챠 버튼, 장착 슬롯, 인벤토리)
  - 버프 계산 클라이언트 표시 (서버 검증 필수)
  - 천장 카운터 UI (Progress Bar: 0/50)
- [x] ~~Update `../IdleRPGClient/Docs/unity/README.md`~~ (README.md 파일 없음, 불필요)

**Requirements**: [All]
**Design Reference**: Unity Client Integration
**참고**: `docs/unity/UNITY_DOCUMENTATION_GUIDE.md`

---

### 6.6 Manual End-to-End Testing ⏱️ 1시간
- [ ] 로컬 서버 실행 (Jenkins 배포 전)
- [ ] Postman/Swagger 테스트:
  - 펫 가챠 (크리스탈 100 소모)
  - 펫 목록 조회 (Public API)
  - 펫 레벨업 (골드 소모, 스탯 증가 확인)
  - 펫 장착 (슬롯 1-3)
  - 장착된 펫 조회 (버프 계산 확인)
  - 중복 펫 가챠 (골드 보상 확인)
  - 천장 시스템 (50회 가챠 → Legendary 보장)
  - 펫 삭제 (장착 중 삭제 불가 확인)
- [ ] 데이터베이스 검증:
  - `SELECT * FROM pets WHERE character_id = '...'`
  - `SELECT * FROM equipped_pets WHERE character_id = '...'`
  - `SELECT pet_gacha_count FROM characters WHERE id = '...'`

**Requirements**: [All]
**Design Reference**: Testing Strategy

---

## 🚀 Post-Implementation

### ✅ Completion Checklist
- [ ] All tasks completed and tested
- [ ] Unit tests passing (90%+ Domain, 80%+ Application)
- [ ] Integration tests passing (75%+ Controller)
- [ ] Migration applied via Jenkins (**DO NOT** run `dotnet ef database update` locally!)
- [ ] Seeder executed successfully (11 pet templates)
- [ ] Unity documentation complete
- [ ] Manual E2E testing complete
- [ ] Code review completed
- [ ] Git commit with message: "feat(pet-system): Add pet gacha, level-up, equip system"
- [ ] Feature merged to main branch
- [ ] Jenkins deployment successful

---

## 📝 Notes

### 🎓 학습 포인트 복습

**완료한 아키텍처 결정**:
1. ✅ **가챠 로직 계층**: Domain Service (순수 비즈니스 로직, 재사용성)
2. ✅ **Random 추상화**: IRandomProvider (DI) (테스트 용이성)
3. ✅ **버프 계산 시점**: 실시간 계산 (데이터 일관성 우선)
4. ✅ **트랜잭션 경계**: Service Layer (Unit of Work) (비즈니스 로직 원자성)
5. ✅ **인덱스 전략**: 단일 컬럼 (character_id) (99% 사용 케이스, 500+ 펫 시 재평가)

### Blockers
<!-- 작업 중 발생한 장애 요소 기록 -->

### Decisions Made During Implementation
<!-- TODO(human) 해소 과정에서 내린 결정 기록 -->

### Future Improvements
- [ ] PetGachaCounter Value Object 도입 (Soft Pity 로직 캡슐화, Immutable 설계)
- [ ] Redis 캐싱 (버프 계산 결과 캐싱, 500+ 펫 시)
- [ ] 복합 인덱스 (character_id, rarity) 추가 (500+ 펫, Rarity 필터 빈번 시)
- [ ] GachaHistory Entity 추가 (가챠 이력 추적, 통계, GDPR 대응)
- [ ] 펫 진화 시스템 (Pet Evolution, 별도 Spec)

---

**시작일**: 2025-10-27
**완료일**: TBD
**예상 총 소요 시간**: ~18.5시간
**Design 추적성**: ✅ design.md의 모든 컴포넌트 커버 확인 완료
