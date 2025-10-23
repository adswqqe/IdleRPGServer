# Tasks: Skill Gacha System

> 스킬 가챠 시스템의 구현 작업 목록입니다.
> 
> **작업 기간**: 2025-10-20 ~ 2025-10-22 (완료)  
> **실제 소요 시간**: ~19.5시간 (예상과 동일)

---

## 📊 Progress Overview

**전체 진행률**: 25/25 (100%) ✅

| Milestone | 작업 수 | 완료 | 진행률 |
|-----------|---------|------|--------|
| Domain Layer | 7 | 7 | 100% ✅ |
| Infrastructure Layer | 5 | 5 | 100% ✅ |
| Application Layer | 4 | 4 | 100% ✅ |
| API Layer | 1 | 1 | 100% ✅ |
| Database | 3 | 3 | 100% ✅ |
| Testing & Documentation | 5 | 5 | 100% ✅ |

**총 소요 시간**: ~19.5시간

---

## 🏗️ Milestone 1: Domain Layer

### 1.1 Create SkillRarity Enum ⏱️ 15분 ✅
- [x] Create `IdleRPG.Domain/Enums/SkillRarity.cs`
- [x] Define enum values: Common = 0, Rare = 1, Epic = 2, Legendary = 3
- [x] Add XML documentation comments

**Requirements**: [US-1]  
**Design Reference**: [Domain Layer - Enums]

---

### 1.2 Create SkillType Enum ⏱️ 15분 ✅
- [x] Create `IdleRPG.Domain/Enums/SkillType.cs`
- [x] Define enum values: Passive = 0, Active = 1
- [x] Add XML documentation comments

**Requirements**: [US-3]  
**Design Reference**: [Domain Layer - Enums]

---

### 1.3 Create SkillTemplate Entity ⏱️ 30분 ✅
- [x] Create `IdleRPG.Domain/Entities/SkillTemplate.cs`
- [x] Add properties: Id (int), Name, Description, Rarity, SkillType, EffectType, EffectValue
- [x] Implement BaseEntity inheritance (CreatedAt, UpdatedAt)
- [x] Add XML documentation

**Requirements**: [US-1, US-3]  
**Design Reference**: [Data Model - SkillTemplates]

**Note**: 코드 리뷰에서 Rarity 속성 누락 발견 → 수정 완료

---

### 1.4 Create PlayerSkill Entity ⏱️ 30분 ✅
- [x] Create `IdleRPG.Domain/Entities/PlayerSkill.cs`
- [x] Add properties: Id (Guid), CharacterId, SkillTemplateId, AcquiredAt, IsEquipped
- [x] Add navigation properties: Character, SkillTemplate
- [x] Implement BaseEntity inheritance

**Requirements**: [US-1, US-3]  
**Design Reference**: [Data Model - PlayerSkills]

---

### 1.5 Create GachaHistory Entity ⏱️ 30분 ✅
- [x] Create `IdleRPG.Domain/Entities/GachaHistory.cs`
- [x] Add properties: Id (Guid), CharacterId, SkillTemplateId, Rarity, PityCountAtDraw, DrawnAt
- [x] Add navigation properties: Character, SkillTemplate

**Requirements**: [US-4]  
**Design Reference**: [Data Model - GachaHistories]

---

### 1.6 Update Character Entity ⏱️ 15분 ✅
- [x] Add `PityCount` property (int, default 0)
- [x] Add `SkillSlotCount` property (int, default 4)
- [x] Add navigation property: `ICollection<PlayerSkill> PlayerSkills`

**Requirements**: [US-2, US-3]  
**Design Reference**: [Data Model - Characters 수정]

---

### 1.7 Create GachaLogicService Domain Service ⏱️ 2시간 ✅
- [x] Create `IdleRPG.Domain/Services/GachaLogicService.cs`
- [x] Inject `IRandomProvider` dependency
- [x] Implement `DetermineRarity(int pityCount)` method
  - [x] 천장 시스템 (pityCount >= 100 → Legendary)
  - [x] 누적 확률 방식 (1%, 9%, 30%, 60%)
  - [x] 상수 정의 (PITY_THRESHOLD, LEGENDARY_RATE, EPIC_RATE, RARE_RATE)
- [x] Implement `SelectRandomSkill(IEnumerable<SkillTemplate>, SkillRarity)` method
  - [x] Rarity 필터링
  - [x] ArgumentException 처리 (해당 등급 없을 때)
- [x] Implement `GetPityCountAfterDraw(int, SkillRarity)` method
  - [x] Legendary → 0, 그 외 → +1
- [x] Add XML documentation

**Requirements**: [US-1, US-2]  
**Design Reference**: [Business Logic - GachaLogicService]

**Critical Issues Fixed**:
- ✅ 확률 계산 로직 오류 수정 (Rare 50% → 30%)
- ✅ Random.Shared 직접 사용 → IRandomProvider DI 적용

---

### 1.8 Create IRandomProvider Interface ⏱️ 15분 ✅
- [x] Create `IdleRPG.Domain/Services/IRandomProvider.cs`
- [x] Define method: `int Next(int maxValue)`
- [x] Add XML documentation (테스트 가능성 명시)

**Requirements**: [US-1]  
**Design Reference**: [Domain Layer - Interfaces]

---

## 🔧 Milestone 2: Infrastructure Layer

### 2.1 Create SystemRandomProvider ⏱️ 15분 ✅
- [x] Create `IdleRPG.Infrastructure/Services/SystemRandomProvider.cs`
- [x] Implement IRandomProvider
- [x] Use `Random.Shared.Next(maxValue)`

**Requirements**: [US-1]  
**Design Reference**: [Infrastructure Layer - Services]

---

### 2.2 Create SkillTemplate EF Core Configuration ⏱️ 30분 ✅
- [x] Create `IdleRPG.Infrastructure/Configurations/SkillTemplateConfiguration.cs`
- [x] Implement IEntityTypeConfiguration<SkillTemplate>
- [x] Configure primary key (Id)
- [x] Configure properties (Name: required, max 100 / Rarity, SkillType: enum to int conversion)
- [x] Configure index: IX_SkillTemplates_Rarity

**Requirements**: [US-1]  
**Design Reference**: [EF Core Configuration - SkillTemplate]

---

### 2.3 Create PlayerSkill EF Core Configuration ⏱️ 30분 ✅
- [x] Create `IdleRPG.Infrastructure/Configurations/PlayerSkillConfiguration.cs`
- [x] Implement IEntityTypeConfiguration<PlayerSkill>
- [x] Configure relationships:
  - [x] Character (1) → PlayerSkills (N), OnDelete Cascade
  - [x] SkillTemplate (1) → PlayerSkills (N)
- [x] Configure indexes: IX_PlayerSkills_CharacterId, IX_PlayerSkills_SkillTemplateId

**Requirements**: [US-1, US-3]  
**Design Reference**: [EF Core Configuration - PlayerSkill]

---

### 2.4 Create GachaHistory EF Core Configuration ⏱️ 30분 ✅
- [x] Create `IdleRPG.Infrastructure/Configurations/GachaHistoryConfiguration.cs`
- [x] Implement IEntityTypeConfiguration<GachaHistory>
- [x] Configure relationships (Character, SkillTemplate)
- [x] Configure composite index: IX_GachaHistories_CharacterId_DrawnAt (DESC)

**Requirements**: [US-4]  
**Design Reference**: [EF Core Configuration - GachaHistory]

---

### 2.5 Create Repositories ⏱️ 1.5시간 ✅
- [x] Create `IdleRPG.Domain/Repositories/ISkillTemplateRepository.cs` (interface)
  - [x] `GetAllAsync()` (캐싱 고려)
  - [x] `GetByRarityAsync(SkillRarity rarity)`
- [x] Create `IdleRPG.Infrastructure/Repositories/SkillTemplateRepository.cs` (implementation)
- [x] Create `IdleRPG.Domain/Repositories/IPlayerSkillRepository.cs` (interface)
  - [x] `AddAsync(PlayerSkill)`, `CountByCharacterIdAsync(Guid)`
- [x] Create `IdleRPG.Infrastructure/Repositories/PlayerSkillRepository.cs` (implementation)
- [x] Create `IdleRPG.Domain/Repositories/IGachaHistoryRepository.cs` (interface)
  - [x] `AddAsync(GachaHistory)`, `GetByCharacterIdAsync(Guid, int limit)`
- [x] Create `IdleRPG.Infrastructure/Repositories/GachaHistoryRepository.cs` (implementation)

**Requirements**: [US-1, US-4]  
**Design Reference**: [Infrastructure Layer - Repositories]

---

## 📦 Milestone 3: Application Layer

### 3.1 Create SkillGacha Request/Response DTOs ⏱️ 45분 ✅
- [x] Create `IdleRPG.Application/DTOs/SkillGacha/SkillGachaRequestDto.cs`
  - [x] Properties: CharacterId (Guid), DrawCount (int)
  - [x] Validation: DrawCount = 1 or 10
- [x] Create `IdleRPG.Application/DTOs/SkillGacha/SkillGachaResponseDto.cs`
  - [x] Properties: Skills (List<SkillDto>), RemainingCrystal, NewPityCount
- [x] Create `IdleRPG.Application/DTOs/SkillGacha/SkillDto.cs`
  - [x] Properties: SkillTemplateId, Name, Rarity, SkillType, EffectValue
- [x] Add XML documentation

**Requirements**: [US-1]  
**Design Reference**: [API Design - Request/Response]

---

### 3.2 Create GachaHistory DTOs ⏱️ 30분 ✅
- [x] Create `IdleRPG.Application/DTOs/SkillGacha/GachaHistoryResponseDto.cs`
  - [x] Properties: Histories (List<GachaHistoryDto>), TotalCount
- [x] Create `IdleRPG.Application/DTOs/SkillGacha/GachaHistoryDto.cs`
  - [x] Properties: Id, SkillName, Rarity, DrawnAt, PityCountAtDraw
- [x] Add XML documentation

**Requirements**: [US-4]  
**Design Reference**: [API Design - GET Endpoint]

---

### 3.3 Create ISkillGachaService Interface ⏱️ 30분 ✅
- [x] Create `IdleRPG.Application/Services/ISkillGachaService.cs`
- [x] Define methods:
  - [x] `Task<SkillGachaResponseDto> DrawSkillsAsync(Guid characterId, int drawCount, CancellationToken)`
  - [x] `Task<GachaHistoryResponseDto> GetHistoryAsync(Guid characterId, int limit, CancellationToken)`
- [x] Add XML documentation

**Requirements**: [US-1, US-4]  
**Design Reference**: [Service Layer - SkillGachaService]

---

### 3.4 Implement SkillGachaService ⏱️ 3시간 ✅
- [x] Create `IdleRPG.Infrastructure/Services/SkillGachaService.cs`
- [x] Inject dependencies: ICharacterRepository, IPlayerSkillRepository, ISkillTemplateRepository, IGachaHistoryRepository, GachaLogicService, GameDBContext
- [x] Implement `DrawSkillsAsync`:
  - [x] 캐릭터 조회 (Include Player)
  - [x] 크리스탈 잔액 검증 (1회: 50, 10회: 500)
  - [x] 스킬 슬롯 검증
  - [x] 트랜잭션 시작
  - [x] For loop (drawCount):
    - [x] DetermineRarity 호출
    - [x] SelectRandomSkill 호출
    - [x] PlayerSkill 추가
    - [x] GachaHistory 기록
    - [x] PityCount 업데이트
  - [x] 크리스탈 차감
  - [x] SaveChanges + Commit
  - [x] 에러 시 Rollback
- [x] Implement `GetHistoryAsync`:
  - [x] GachaHistoryRepository.GetByCharacterIdAsync 호출
  - [x] DTO 변환
- [x] Add logging

**Requirements**: [US-1, US-2, US-4]  
**Design Reference**: [Service Layer - DrawSkillsAsync]

---

## 🌐 Milestone 4: API Layer

### 4.1 Create SkillsController ⏱️ 1.5시간 ✅
- [x] Create `IdleRPG.API/Controllers/SkillsController.cs`
- [x] Add `[Authorize]` attribute
- [x] Inject ISkillGachaService
- [x] Implement `POST /api/skills/gacha` endpoint
  - [x] Validate request DTO
  - [x] Call DrawSkillsAsync
  - [x] Return 200 OK with response DTO
  - [x] Handle exceptions:
    - [x] NotFoundException → 404
    - [x] InvalidOperationException → 400
  - [x] Add Swagger XML comments
- [x] Implement `GET /api/skills/gacha/history?characterId={uuid}` endpoint
  - [x] Validate characterId
  - [x] Call GetHistoryAsync
  - [x] Return 200 OK
  - [x] Add Swagger XML comments

**Requirements**: [US-1, US-4]  
**Design Reference**: [API Design - Endpoints]

---

## 🗃️ Milestone 5: Database

### 5.1 Create Database Migration ⏱️ 1시간 ✅
- [x] Add migration to `IdleRPG.Infrastructure/migration.sql`
- [x] Use DO $EF$ BEGIN ... END $EF$ pattern (idempotent)
- [x] CREATE TABLE "SkillTemplates"
- [x] CREATE TABLE "PlayerSkills" with foreign keys
- [x] CREATE TABLE "GachaHistories" with foreign keys
- [x] ALTER TABLE "Characters" ADD COLUMN "PityCount", "SkillSlotCount"
- [x] ALTER TABLE "Players" ADD COLUMN "Crystal"
- [x] CREATE INDEX "IX_SkillTemplates_Rarity"
- [x] CREATE INDEX "IX_PlayerSkills_CharacterId"
- [x] CREATE INDEX "IX_GachaHistories_CharacterId_DrawnAt"
- [x] Test migration locally (separate .sql file in Migrations/ folder)

**Requirements**: [All]  
**Design Reference**: [Migration Plan]  
**참고**: `CLAUDE.md - Database Migration`

---

### 5.2 Create SkillTemplateSeeder ⏱️ 1시간 ✅
- [x] Create `IdleRPG.Infrastructure/Seeders/SkillTemplateSeeder.cs`
- [x] Add 50개 스킬 템플릿 데이터:
  - [x] Common: 30개 (패시브 15개, 액티브 15개)
  - [x] Rare: 15개 (패시브 8개, 액티브 7개)
  - [x] Epic: 4개 (패시브 2개, 액티브 2개)
  - [x] Legendary: 1개 (액티브)
- [x] Register in Program.cs seeding logic
- [x] 밸런스 조정: EffectValue (Common: 5~10, Rare: 15~25, Epic: 30~50, Legendary: 100)

**Requirements**: [US-1, US-3]  
**Design Reference**: [Data Seeding]

---

### 5.3 Register Dependencies in DI Container ⏱️ 20분 ✅
- [x] Open `IdleRPG.API/Program.cs`
- [x] Register repositories:
  - [x] `AddScoped<ISkillTemplateRepository, SkillTemplateRepository>()`
  - [x] `AddScoped<IPlayerSkillRepository, PlayerSkillRepository>()`
  - [x] `AddScoped<IGachaHistoryRepository, GachaHistoryRepository>()`
- [x] Register services:
  - [x] `AddScoped<ISkillGachaService, SkillGachaService>()`
- [x] Register domain services:
  - [x] `AddScoped<GachaLogicService>()`
- [x] Register singletons:
  - [x] `AddSingleton<IRandomProvider, SystemRandomProvider>()`

**Requirements**: [All]  
**Design Reference**: [Architecture Overview]

---

## 🧪 Milestone 6: Testing & Documentation

### 6.1 Create GachaLogicServiceTests ⏱️ 2시간 ✅
- [x] Create `IdleRPG.Tests/Domain/Services/GachaLogicServiceTests.cs`
- [x] Setup: Mock<IRandomProvider>, GachaLogicService instance
- [x] Test cases (12개):
  - [x] DetermineRarity_PityCount100_ReturnsLegendary
  - [x] DetermineRarity_RandomValue0_ReturnsLegendary
  - [x] DetermineRarity_RandomValue1to9_ReturnsEpic (Theory 3개)
  - [x] DetermineRarity_RandomValue10to39_ReturnsRare (Theory 3개)
  - [x] DetermineRarity_RandomValue40to99_ReturnsCommon (Theory 3개)
  - [x] SelectRandomSkill_FiltersByRarity_ReturnsCorrectSkill
  - [x] SelectRandomSkill_NoSkillsForRarity_ThrowsArgumentException
  - [x] SelectRandomSkill_EmptyList_ThrowsArgumentException
  - [x] GetPityCountAfterDraw_Legendary_ResetsToZero
  - [x] GetPityCountAfterDraw_OtherRarity_IncrementsCount
- [x] Use AAA pattern, FluentAssertions
- [x] Achieve 100% code coverage

**Requirements**: [US-1, US-2]  
**Design Reference**: [Testing Strategy - Unit Tests]

**Insight**: Mock으로 확률 제어 → "Legendary 100% 뽑기" 시나리오 작성 가능

---

### 6.2 Create SkillGachaServiceTests ⏱️ 2.5시간 ✅
- [x] Create `IdleRPG.Tests/Application/Services/SkillGachaServiceTests.cs`
- [x] Mock repositories, GachaLogicService, DbContext
- [x] Test cases (8개):
  - [x] DrawSkillsAsync_ValidRequest_ReturnsSkills
  - [x] DrawSkillsAsync_InsufficientCrystal_ThrowsInvalidOperationException
  - [x] DrawSkillsAsync_FullSkillSlot_ThrowsInvalidOperationException
  - [x] DrawSkillsAsync_CharacterNotFound_ThrowsNotFoundException
  - [x] DrawSkillsAsync_10Draw_DrawsTenSkills
  - [x] DrawSkillsAsync_UpdatesPityCountCorrectly
  - [x] DrawSkillsAsync_SavesGachaHistory
  - [x] DrawSkillsAsync_RollbackOnError
- [x] Achieve 90%+ code coverage

**Requirements**: [US-1, US-2, US-4]  
**Design Reference**: [Testing Strategy]

---

### 6.3 Create Integration Tests ⏱️ 1.5시간 ✅
- [x] Create `IdleRPG.Tests/Integration/SkillsControllerIntegrationTests.cs`
- [x] Setup WebApplicationFactory, In-Memory DB
- [x] Test cases:
  - [x] POST /api/skills/gacha - 성공 케이스
  - [x] POST /api/skills/gacha - 크리스탈 부족 (400)
  - [x] POST /api/skills/gacha - 인증 없음 (401)
  - [x] GET /api/skills/gacha/history - 히스토리 조회

**Requirements**: [US-1, US-4]  
**Design Reference**: [Testing Strategy - Integration Tests]

---

### 6.4 Update DbContext Seeding ⏱️ 30분 ✅
- [x] Modify `GameDBContext.OnModelCreating`
- [x] Call SkillTemplateSeeder.Seed() in seeding logic
- [x] Ensure idempotency (IF NOT EXISTS check)

**Requirements**: [US-1]  
**Design Reference**: [Data Seeding]

---

### 6.5 Create Unity Documentation ⏱️ 1시간 ✅
- [x] Create `../IdleRPGClient/Docs/unity/skill-gacha/` folder
- [x] Create `API_SPEC.md`:
  - [x] POST /api/skills/gacha endpoint details
  - [x] Request/Response JSON 예제
  - [x] Unity C# UnityWebRequest 예제
  - [x] JWT 토큰 헤더 추가 방법
- [x] Create `DTOs.cs`:
  - [x] SkillGachaRequestDto, SkillGachaResponseDto
  - [x] Use `[JsonProperty]` attributes (Newtonsoft.Json)
  - [x] Unity-compatible C# code
- [x] Update `../IdleRPGClient/Docs/unity/README.md` main index

**Requirements**: [All]  
**Design Reference**: [Unity Client Integration]  
**참고**: `docs/unity/UNITY_DOCUMENTATION_GUIDE.md`

---

## 🚀 Post-Implementation

### ✅ Completion Checklist
- [x] All tasks completed and tested
- [x] Unit tests passing (100% coverage for GachaLogicService)
- [x] Integration tests passing
- [x] Migration applied via Jenkins (**NOT** `dotnet ef database update` locally!)
- [x] Unity documentation complete
- [x] Zen MCP Code Review completed (6개 이슈 수정)
- [x] Git commit with descriptive message
- [x] Feature merged to main branch

---

## 📝 Notes

### Blockers (해소 완료)
1. ✅ **확률 계산 로직 오류** (Critical) - 수정 완료
   - 문제: Rare 30% → 50% 잘못된 확률
   - 해결: 누적 확률 방식 적용 (0-based index)
2. ✅ **Random.Shared 테스트 불가** (High) - 수정 완료
   - 문제: Mock 불가능
   - 해결: IRandomProvider 인터페이스 DI

### Decisions Made
1. ✅ **천장 시스템**: 100회 (Gemini 제안 채택)
2. ✅ **확률 분포**: Common 60%, Rare 30%, Epic 9%, Legendary 1%
3. ✅ **가챠 비용**: 1회 50크리스탈, 10연차 500크리스탈 (할인 없음)
4. ✅ **스킬 슬롯**: Lv1(4개), Lv10(5개), Lv20(6개)
5. ✅ **트랜잭션 처리**: EF Core Transaction (크리스탈 차감 + 스킬 추가 원자성)

### Future Improvements
- [ ] **캐싱**: SkillTemplate 전체 목록 IMemoryCache 적용
- [ ] **10연차 이벤트**: 첫 번째 뽑기 확률 업 (향후 기획)
- [ ] **스킬 레벨업**: 중복 스킬 획득 시 레벨업 시스템 (Phase 2)
- [ ] **StatCalculationService**: 패시브 스킬 스탯 적용 중앙화 (Gemini 제안)
- [ ] **액티브 스킬 발동**: Combat System 연동 (Phase 2)

---

## 🎓 Key Insights (작업 중 배운 점)

### 1. 확률 계산의 함정
- **0-based vs 1-based**: `Random.Next(100)` → 0~99, `+1` 하면 범위 틀어짐
- **누적 확률 방식이 정확**: `if (rand < 1)` (1%), `else if (rand < 10)` (9%), ...
- **단위 테스트 필수**: 확률 버그는 게임 경제 붕괴로 이어짐

### 2. 테스트 가능한 설계
- **정적 의존성(Random.Shared)** → Mock 불가능
- **인터페이스 추상화(IRandomProvider)** → 확정적 테스트 가능
- **IRandomProvider 덕분에**: "Legendary 100% 뽑기" 시나리오 작성 가능

### 3. Clean Architecture의 가치
- **Domain Layer 독립성**: GachaLogicService는 EF Core, ASP.NET 의존 없음
- **테스트 용이성**: Domain Service 단위 테스트 12개 작성 (100% 커버리지)
- **의존성 역전**: IRandomProvider 인터페이스를 Domain에 정의 → Infrastructure에서 구현

### 4. 협업 도구의 힘
- **Gemini 2.5 Pro 협업**: 8가지 핵심 질문 → StatCalculationService 통찰 획득
- **Zen MCP Code Review**: 6개 이슈 발견 (Critical 2개 수정으로 게임 경제 보호)
- **Multi-AI 접근**: Claude + Gemini = 설계 품질 향상

---

**시작일**: 2025-10-20  
**완료일**: 2025-10-22  
**총 소요 시간**: ~19.5시간  
**Design 추적성**: 모든 컴포넌트 구현 완료 ✅  
**코드 품질**: Critical 이슈 0개, Unit Test 100% 커버리지 ✅
