# Work Log: Pet System

> 이 문서는 Pet System 구현 작업 로그입니다.

---

## 2025-10-27 14:30

### Task Completed
- [x] 1.1 Create Pet Entity

### Files Changed
- `IdleRPG.Domain/Entities/Pet.cs` (new file, 57 lines)

### Key Decisions
- **Entity 설계**:
  - Id: int (AUTO_INCREMENT, 대량 펫 관리에 적합)
  - Level: int (1-50 범위, Default 1)
  - CurrentAttack/CurrentMana: 레벨업 시 동적 계산 후 저장 (성능 우선)
  - Navigation Properties: Character, PetTemplate (EF Core 관계 설정)

### Notes
- Character, PetTemplate 엔티티는 이후 Task에서 참조
- XML 문서화 주석 추가로 코드 가독성 향상
- CreatedAt, UpdatedAt: DateTime.UtcNow로 초기화 (UTC 기준 시간)

---

## 2025-10-27 14:45

### Task Completed
- [x] 1.4 Create Rarity Enum (공용) - 순서 변경하여 우선 처리
- [x] 1.2 Create PetTemplate Entity

### Files Changed
- `IdleRPG.Domain/Enums/Rarity.cs` (new file, 34 lines)
- `IdleRPG.Domain/Entities/PetTemplate.cs` (new file, 40 lines - ImageUrl, Description 제거)

### Key Decisions
- **🎓 아키텍처 결정 변경**: PetRarity 대신 공용 Rarity enum 사용
  - **이유**: 코드 중복 제거, 일관성 향상 (Skill, Equipment, Pet 모두 동일한 희귀도 시스템)
  - **영향 범위**:
    - SkillRarity, EquipmentRarity는 향후 Rarity로 마이그레이션 가능 (리팩토링)
    - 현재는 Pet 시스템만 공용 Rarity 사용
  - **장점**:
    - 새 콘텐츠 추가 시 enum 재정의 불필요
    - 희귀도별 UI/로직 공통화 가능
- **PetTemplate 설계**:
  - Id: int (시퀀스, 마스터 데이터는 서버에서 중앙 관리)
  - Name: UNIQUE 제약 (중복 방지)
  - BaseAttack, BaseMana: 레벨 1 기준 스탯
  - Navigation Property: ICollection<Pet> (1:N 관계)

### Notes
- Task 순서 변경: 1.4 → 1.2 (의존성 해소)
- XML 문서화 주석으로 가챠 확률 명시 (Legendary 1%, Epic 9%, Rare 30%, Common 60%)
- 향후 SkillTemplate, ItemTemplate도 공용 Rarity 사용으로 마이그레이션 고려

### Architecture Refinement (2025-10-27 15:00)
- **🎓 ImageUrl, Description 필드 제거**:
  - **이유**: 서버-클라이언트 관심사 분리 원칙
  - **서버 역할**: 게임 로직과 스탯 데이터만 관리
  - **클라이언트 역할**: 표현(이미지, 텍스트) 관리
  - **매핑 방식**: Unity가 PetTemplate.Id 또는 Name → 리소스 경로 변환
    - 예: `Resources.Load<Sprite>($"Pets/{petTemplate.Id}")`
    - 예: `Resources.Load<Sprite>($"Pets/{ToSnakeCase(petTemplate.Name)}")`
  - **장점**:
    - DB 컬럼 절약 (VARCHAR(255) + TEXT 제거)
    - 결합도 감소 (클라이언트 리소스 구조 변경 시 서버 영향 없음)
    - 표준 게임 개발 패턴 준수

---

## 2025-10-27 15:15

### Task Completed
- [x] 1.3 Create EquippedPets Entity

### Files Changed
- `IdleRPG.Domain/Entities/EquippedPets.cs` (new file, 39 lines)

### Key Decisions
- **Composite PK 설계**:
  - (CharacterId, SlotIndex): 캐릭터당 슬롯별 유일성 보장
  - 한 캐릭터는 슬롯 1, 2, 3에 각각 다른 펫 장착 가능
  - EF Core Configuration에서 `.HasKey(e => new { e.CharacterId, e.SlotIndex })` 설정 필요
- **UNIQUE 제약 (PetId)**:
  - 한 펫은 하나의 슬롯에만 장착 가능 (중복 장착 방지)
  - EF Core Configuration에서 `.HasIndex(e => e.PetId).IsUnique()` 설정 필요
- **슬롯 범위**:
  - SlotIndex: 1-3 (CHECK 제약은 Configuration에서 설정)
  - 게임 확장 시 슬롯 수 증가 가능 (마이그레이션)

### Notes
- Composite PK는 EF Core에서 Fluent API로만 설정 가능 (Data Annotation 불가)
- Navigation Properties: Character, Pet (양방향 관계)
- EquippedAt: 통계/로그 목적 (장착 시간 추적)

---

## 2025-10-27 15:30

### Task Completed
- [x] 1.5 Create PetGachaService Domain Service

### Files Changed
- `IdleRPG.Domain/Services/PetGachaService.cs` (new file, 93 lines)

### Key Decisions
- **Domain Service 패턴 선택**:
  - **이유**: 가챠 확률 계산은 순수 비즈니스 로직 (상태 없음, Entity에 속하지 않음)
  - **장점**: 테스트 용이성, 재사용성, Application Service와 분리
- **Soft Pity 시스템 구현**:
  - Hard Pity: 50회에 Legendary 보장 (100% 확률)
  - Soft Pity: 40회부터 Legendary 확률 증가
    - 40회: 1% → 2%
    - 41회: 1% → 3%
    - ...
    - 49회: 1% → 11%
  - **공식**: `adjustedRate = 1 + (pityCount - 39)` (40회부터 +1%씩 증가)
- **확률 계산 방식**:
  - 누적 확률 방식: rand ∈ [0, 100)
  - Legendary: [0, adjustedRate)
  - Epic: [adjustedRate, adjustedRate+9)
  - Rare: [adjustedRate+9, adjustedRate+39)
  - Common: [adjustedRate+39, 100)
- **Input Validation**:
  - pityCount: 0-50 범위 검증 (ArgumentOutOfRangeException)
  - randomProvider: null 검증 (ArgumentNullException)
- **공용 Rarity enum 사용**:
  - SkillRarity → Rarity로 변경 (Task 1.4 결정 반영)
  - 반환 타입: `Rarity` (Pet, Skill, Equipment 공통)

### Notes
- GachaLogicService (Skill용)와 동일한 구조 유지 (일관성)
- GetPityCountAfterDraw() 메서드 추가 (천장 카운터 관리)
- XML 문서화 주석으로 Soft/Hard Pity 시스템 설명
- IRandomProvider 의존성 주입으로 테스트 가능성 확보

---

## 2025-10-27 15:45

### Task Completed
- [x] 1.6 Update Character Entity

### Files Changed
- `IdleRPG.Domain/Entities/Character.cs` (modified, +7 lines)

### Key Decisions
- **별도 천장 카운터 추가**:
  - `GachaPityCount`: 스킬 가챠용 (100회)
  - `PetGachaCount`: 펫 가챠용 (50회) - **신규 추가**
  - **이유**: 스킬과 펫은 다른 천장 시스템 (100회 vs 50회)
- **Default 값**: 0 (가챠 미실행 상태)
- **XML 문서화**: 각 카운터의 용도와 천장 횟수 명시

### Notes
- 기존 `GachaPityCount`는 스킬 가챠 전용으로 유지
- Migration에서 `ALTER TABLE characters ADD COLUMN pet_gacha_count INT NOT NULL DEFAULT 0` 필요
- CHECK 제약: `pet_gacha_count BETWEEN 0 AND 50` (EF Core Configuration)

---

## 🎉 Milestone 1 Complete: Domain Layer (6/6 tasks, 100%)

### 완료된 작업:
1. ✅ Pet Entity
2. ✅ PetTemplate Entity
3. ✅ EquippedPets Entity
4. ✅ Rarity Enum (공용)
5. ✅ PetGachaService Domain Service
6. ✅ Character Entity Update (PetGachaCount)

### 주요 아키텍처 결정:
- ✅ 공용 Rarity enum 사용 (Skill, Equipment, Pet 공통)
- ✅ ImageUrl, Description 제거 (서버-클라이언트 관심사 분리)
- ✅ Composite PK (CharacterId, SlotIndex) for EquippedPets
- ✅ Domain Service 패턴 (Soft Pity 40회, Hard Pity 50회)
- ✅ 별도 천장 카운터 (스킬 100회, 펫 50회)

### Next Milestone: **Infrastructure Layer** (6 tasks)
- Repository interfaces & implementations
- EF Core Configurations
- DbContext registration

---

## 2025-10-27 16:00

### Task Completed
- [x] 2.1 Create PetRepository

### Files Changed
- `IdleRPG.Domain/Repositories/IPetRepository.cs` (new file, 60 lines)
- `IdleRPG.Infrastructure/Repositories/PetRepository.cs` (new file, 57 lines)

### Key Decisions
- **Interface 메서드 설계**:
  - `GetByIdAsync`: 펫 상세 조회 (Include PetTemplate, Character)
  - `GetByCharacterIdAsync`: 캐릭터 소유 펫 목록 (Include PetTemplate, OrderBy CreatedAt DESC)
  - `GetDuplicateAsync`: 가챠 중복 체크용 (CharacterId + TemplateId)
  - `AddAsync`, `UpdateAsync`, `DeleteAsync`: 기본 CRUD
- **Eager Loading 전략**:
  - GetByIdAsync: PetTemplate + Character 포함 (상세 정보 필요)
  - GetByCharacterIdAsync: PetTemplate만 포함 (목록 조회 성능)
  - GetDuplicateAsync: Include 없음 (존재 여부만 확인)
- **정렬 기준**:
  - GetByCharacterIdAsync: CreatedAt DESC (최근 획득 펫 우선)
- **CancellationToken 지원**:
  - 모든 메서드에 기본값 제공 (default)
  - 비동기 작업 취소 가능

### Notes
- `_context.Set<Pet>()`를 사용하여 DbSet 접근 (명시적)
- UpdateAsync, DeleteAsync는 동기 작업 (EF Core는 메모리에서만 마킹)
- SaveChangesAsync는 UnitOfWork 패턴으로 Application Layer에서 호출
- Navigation Property Include로 N+1 쿼리 문제 방지

---

## 2025-10-27 16:15

### Task Completed
- [x] 2.2 Create PetTemplateRepository

### Files Changed
- `IdleRPG.Domain/Repositories/IPetTemplateRepository.cs` (new file, 34 lines)
- `IdleRPG.Infrastructure/Repositories/PetTemplateRepository.cs` (new file, 42 lines)

### Key Decisions
- **Interface 메서드 설계**:
  - `GetByRarityAsync(Rarity rarity)`: 가챠 풀 구성용 (희귀도별 조회)
  - `GetByIdAsync(int templateId)`: 펫 생성 시 템플릿 정보 조회
  - `GetAllAsync()`: 전체 템플릿 조회 (Admin, 통계용) - **추가 메서드**
- **공용 Rarity enum 사용**:
  - PetRarity → Rarity로 변경 (Task 1.4 결정 반영)
  - SkillTemplate, ItemTemplate과 동일한 enum 사용
- **정렬 기준**:
  - GetByRarityAsync: OrderBy Id (삽입 순서)
  - GetAllAsync: OrderBy Rarity, ThenBy Id (희귀도 우선, ID 보조)
- **Read-Only Repository**:
  - 마스터 데이터이므로 Add/Update/Delete 메서드 없음
  - Seeder에서만 데이터 삽입 (애플리케이션 로직에서 수정 불가)

### Notes
- PetTemplate은 불변 참조 데이터 (마스터 데이터 패턴)
- Include 없음 (PetTemplate 자체가 루트 엔티티, 추가 조회 불필요)
- GetByRarityAsync는 PetGachaService에서 호출 (가챠 풀 구성)
- GetAllAsync는 원래 tasks.md에 없었지만, Admin/통계 기능에 필요하여 추가

---

## 2025-10-27 16:30

### Task Completed
- [x] 2.3 Create Pet EF Core Configuration

### Files Changed
- `IdleRPG.Infrastructure/Configurations/PetConfiguration.cs` (new file, 86 lines)

### Key Decisions
- **Table & Column Naming**:
  - Table: "pets" (snake_case, PostgreSQL 컨벤션)
  - Columns: "id", "character_id", "template_id", "level", etc. (snake_case)
- **Primary Key**:
  - Id: AUTO_INCREMENT (ValueGeneratedOnAdd)
- **Foreign Keys**:
  - CharacterId → characters.id: ON DELETE CASCADE (캐릭터 삭제 시 펫도 삭제)
  - TemplateId → pet_templates.id: ON DELETE RESTRICT (템플릿은 참조 중 삭제 불가)
- **Index 전략**:
  - `idx_pets_character_id`: 단일 컬럼 인덱스 (99% 사용 케이스)
  - 복합 인덱스는 500+ 펫 시 재평가 (현재 불필요)
- **Check Constraints** (PostgreSQL):
  - `level >= 1 AND level <= 50`
  - `current_attack >= 0`
  - `current_mana >= 0`
- **Default Values**:
  - Level: 1
  - CreatedAt, UpdatedAt: CURRENT_TIMESTAMP

### Notes
- DeleteBehavior.Cascade vs Restrict: 데이터 무결성 규칙을 DB 레벨에서 강제
- HasCheckConstraint: EF Core 5.0+ 기능 (PostgreSQL 전용)
- snake_case 컬럼명: C# PascalCase → DB snake_case 자동 매핑
- Navigation Property: Character.WithMany()는 명시적 역참조 없음 (필요 시 추가)

---

## 2025-10-27 16:45

### Task Completed
- [x] 2.4 Create PetTemplate EF Core Configuration

### Files Changed
- `IdleRPG.Infrastructure/Configurations/PetTemplateConfiguration.cs` (new file, 72 lines)

### Key Decisions
- **Table & Column Naming**:
  - Table: "pet_templates" (snake_case)
  - Columns: "id", "name", "rarity", "base_attack", "base_mana"
- **Primary Key**:
  - Id: AUTO_INCREMENT (ValueGeneratedOnAdd)
- **UNIQUE Constraint**:
  - Name: UNIQUE (uk_pet_templates_name) - 중복 펫 이름 방지
- **Index**:
  - `idx_pet_templates_rarity`: 희귀도별 조회 최적화 (가챠 풀 구성)
- **Enum Conversion**:
  - Rarity enum → int: `HasConversion<int>()` (DB에 0,1,2,3 저장)
  - EF Core가 자동으로 enum ↔ int 변환
- **Check Constraints** (PostgreSQL):
  - `rarity >= 0 AND rarity <= 3` (Rarity enum 범위)
  - `base_attack >= 0`, `base_mana >= 0`
- **Relationships**:
  - HasMany(pt => pt.Pets): 1:N 관계 명시
  - OnDelete Restrict: PetTemplate은 참조 중 삭제 불가 (마스터 데이터 보호)

### Notes
- UNIQUE vs Index: Name은 UNIQUE (중복 불가), Rarity는 일반 Index (중복 가능)
- HasConversion<int>: enum을 DB에 int로 저장 (PostgreSQL은 enum 타입 지원하지만 마이그레이션 복잡도 증가)
- MaxLength(50): VARCHAR(50)로 매핑 (이름 길이 제한)
- 마스터 데이터이므로 CreatedAt/UpdatedAt 필드 없음 (Seeder에서만 삽입)

---

## 2025-10-27 17:00

### Task Completed
- [x] 2.5 Create EquippedPets EF Core Configuration

### Files Changed
- `IdleRPG.Infrastructure/Configurations/EquippedPetsConfiguration.cs` (new file, 64 lines)

### Key Decisions
- **Composite Primary Key**:
  - (CharacterId, SlotIndex): Fluent API로만 설정 가능
  - `.HasKey(ep => new { ep.CharacterId, ep.SlotIndex })`
  - **의미**: "캐릭터 A의 슬롯 2"는 본질적으로 유일함
- **UNIQUE Constraint**:
  - PetId: UNIQUE (uk_equipped_pets_pet_id)
  - **의미**: 한 펫은 여러 슬롯에 동시 장착 불가
  - Composite PK와 함께 작동: "한 슬롯에 한 펫" + "한 펫은 한 슬롯에만"
- **Foreign Keys**:
  - CharacterId → characters.id: ON DELETE CASCADE
  - PetId → pets.id: ON DELETE CASCADE
  - **이유**: 캐릭터나 펫이 삭제되면 장착 정보도 의미 없음
- **Check Constraint**:
  - `slot_index >= 1 AND slot_index <= 3` (슬롯 범위 제한)
  - 게임 확장 시 슬롯 수 증가 가능 (마이그레이션으로 CHECK 제약 수정)

### Notes
- Composite PK는 Data Annotation ([Key]) 불가, Fluent API 필수
- UNIQUE(PetId) + Composite PK(CharacterId, SlotIndex):
  - 두 제약이 함께 작동하여 비즈니스 규칙을 DB 레벨에서 강제
  - Race Condition에도 안전 (동시 요청 시 DB가 거부)
- EquippedAt: 타임스탬프 (장착 시간 추적, 통계/로그 용도)
- Navigation Properties: Character, Pet (역참조 WithMany() 없음)

---

## 2025-10-27 17:15

### Task Completed
- [x] 2.6 Register EF Core Configurations in GameDBContext

### Files Changed
- `IdleRPG.Infrastructure/Data/GameDBContext.cs` (modified, +5 lines)

### Key Decisions
- **DbSet 추가**:
  - `DbSet<Pet> Pets`
  - `DbSet<PetTemplate> PetTemplates`
  - `DbSet<EquippedPets> EquippedPets`
- **Configuration 자동 적용**:
  - 기존 `ApplyConfigurationsFromAssembly` 활용
  - PetConfiguration, PetTemplateConfiguration, EquippedPetsConfiguration 자동 탐지
  - **장점**: Configuration 추가 시 GameDBContext 수정 불필요 (확장성)

### Notes
- `ApplyConfigurationsFromAssembly`가 이미 설정되어 있어 별도 `ApplyConfiguration` 호출 불필요
- Assembly 스캔으로 IEntityTypeConfiguration<T> 구현체 자동 적용
- Milestone 2 (Infrastructure Layer) 완료: 6/6 tasks (100%)

---

## 🎉 Milestone 2 Complete: Infrastructure Layer (6/6 tasks, 100%)

### 완료된 작업:
1. ✅ PetRepository (Interface + Implementation)
2. ✅ PetTemplateRepository (Interface + Implementation)
3. ✅ Pet EF Core Configuration
4. ✅ PetTemplate EF Core Configuration
5. ✅ EquippedPets EF Core Configuration
6. ✅ GameDBContext 등록

### 주요 아키텍처 결정:
- ✅ Repository 패턴 (Domain Interface, Infrastructure Implementation)
- ✅ Eager Loading 전략 (Include PetTemplate, Character)
- ✅ Index 전략: `idx_pets_character_id`, `idx_pet_templates_rarity`
- ✅ Cascade Delete: Character → Pet, Pet → EquippedPets
- ✅ Composite PK: (CharacterId, SlotIndex) for EquippedPets
- ✅ Check Constraints: Level (1-50), SlotIndex (1-3), Rarity (0-3)
- ✅ ApplyConfigurationsFromAssembly 활용 (자동 Configuration 적용)

### Next Milestone: **Application Layer** (4 tasks)
- Pet Request/Response DTOs
- PetService Interface & Implementation

---

## 2025-10-27 18:15

### Task Completed
- [x] 3.4 Create PetService Implementation

### Files Changed
- `IdleRPG.Infrastructure/Services/PetService.cs` (modified, +318 lines)
- `IdleRPG.Domain/Repositories/IEquippedPetsRepository.cs` (new file, 35 lines)
- `IdleRPG.Infrastructure/Repositories/EquippedPetsRepository.cs` (new file, 70 lines)
- `IdleRPG.Application/Interfaces/IUnitOfWork.cs` (modified, +5 lines)
- `IdleRPG.Infrastructure/UnitOfWork/UnitOfWork.cs` (modified, +19 lines)

### Key Decisions
- **PetService 구현 완료**:
  1. **DrawPetsAsync**: 가챠 루프 (count 횟수), 중복 체크 → 골드 보상, 천장 시스템 (Legendary 획득 시 0 초기화)
  2. **LevelUpPetAsync**: 비용 계산 `100 * (1.5 ^ (Level - 1))`, 스탯 재계산 `BaseAttack + (Level-1) * 10`
  3. **EquipPetAsync**: 중복 장착 방지, 슬롯 교체 로직, 버프 실시간 계산 (`CurrentAttack * 0.1`)
  4. **UnequipPetAsync**: 슬롯 해제, 슬롯 범위 검증 (1-3)
  5. **GetEquippedPetsAsync**: 장착된 펫 조회, 버프 계산 (10%)
  6. **DeletePetAsync**: 장착 중 삭제 불가 검증

- **EquippedPets Repository 추가** (누락 보완):
  - Interface: `IEquippedPetsRepository` (5개 메서드)
  - Implementation: `EquippedPetsRepository` (EF Core)
  - `GetBySlotAsync`: Composite PK 조회 (CharacterId + SlotIndex)
  - `GetByPetIdAsync`: 중복 장착 체크용
  - `GetByCharacterIdAsync`: Include Pet, PetTemplate (버프 계산)
  - `AddAsync`, `DeleteAsync`: 장착/해제

- **IUnitOfWork 등록**:
  - `IEquippedPetsRepository EquippedPets { get; }` 추가
  - Lazy 초기화 패턴 유지

### Notes
- **트랜잭션 경계**: Service Layer에서 Unit of Work 사용 (DrawPetsAsync, LevelUpPetAsync, EquipPetAsync 모두 트랜잭션 원자성 보장)
- **버프 계산 시점**: 실시간 계산 (데이터 일관성 우선, 성능은 향후 Redis 캐싱 고려)
- **로깅 전략**: 각 주요 작업(가챠, 레벨업, 장착)에 LogInformation 추가, 실패 시 LogWarning
- **소유권 검증**: 모든 쓰기 작업에서 `pet.CharacterId == characterId` 확인 (보안)

### Architecture Alignment
- **design.md Service Layer Design 섹션과 100% 일치**:
  - DrawPetsAsync: design.md Line 568-596
  - LevelUpPetAsync: design.md Line 613-642
  - EquipPetAsync: design.md Line 653-685

---

## 🎉 Milestone 3 Complete: Application Layer (4/4 tasks, 100%)

### 완료된 작업:
1. ✅ Pet Request DTOs (3개: PetGachaRequestDto, PetLevelUpRequestDto, PetEquipRequestDto)
2. ✅ Pet Response DTOs (6개: PetDto, DuplicateRewardDto, PetGachaResponseDto, PetLevelUpResponseDto, EquippedPetDto, PetEquipResponseDto)
3. ✅ PetService Interface (8개 메서드 시그니처)
4. ✅ PetService Implementation (8개 메서드 구현 + EquippedPets Repository 추가)

### 주요 아키텍처 결정:
- ✅ Service Layer에서 Unit of Work 패턴 (트랜잭션 경계)
- ✅ 가챠 중복 처리: 골드 보상 (Common 100 ~ Legendary 10,000)
- ✅ 천장 시스템: Legendary 획득 시 `PetGachaCount` 0 초기화
- ✅ 레벨업 비용: 지수적 증가 `100 * (1.5 ^ (Level-1))`
- ✅ 버프 계산: 실시간 계산 (Attack * 0.1, Mana * 0.1)
- ✅ 장착 로직: 중복 장착 방지 + 슬롯 교체
- ✅ EquippedPets Repository 추가 (Composite PK 지원)

### Next Milestone: **API Layer** (3 tasks)
- PetsController 생성 (8개 엔드포인트)
- Swagger 문서화
- Exception Handling (400, 401, 403, 404)

---

## 2025-10-27 17:30

### Task Completed
- [x] 3.1 Create Pet Request DTOs

### Files Changed
- `IdleRPG.Application/DTOs/Pet/PetGachaRequestDto.cs` (new file, 22 lines)
- `IdleRPG.Application/DTOs/Pet/PetLevelUpRequestDto.cs` (new file, 14 lines)
- `IdleRPG.Application/DTOs/Pet/PetEquipRequestDto.cs` (new file, 25 lines)

### Key Decisions
- **DataAnnotations Validation 패턴 사용**:
  - `[Required]`: 필수 필드 검증 (CharacterId, Count, PetId, SlotIndex)
  - `[Range]`: 값 범위 검증 (Count: 1-10, SlotIndex: 1-3)
  - ErrorMessage: 한국어 에러 메시지 (사용자 친화적)
- **PetGachaRequestDto**:
  - Count: 1 또는 10만 허용 (단건/10연차)
  - Default 값: 1 (단건 가챠 기본)
- **PetLevelUpRequestDto**:
  - CharacterId만 포함 (PetId는 URL 경로에서 추출)
  - 소유권 검증용 (Service Layer에서 Pet.CharacterId == Request.CharacterId 확인)
- **PetEquipRequestDto**:
  - SlotIndex: 1-3 범위 검증
  - PetId: int (Pet Entity의 PK 타입과 일치)
- **XML 문서화**:
  - 각 DTO와 속성에 한국어 설명 추가
  - API 자동 문서화 (Swagger) 지원

### Notes
- 기존 Gacha DTO 패턴 참조 (PerformGachaCommand)
- Request DTO는 입력 데이터만 담음 (비즈니스 로직 없음)
- Validation은 Controller에서 ModelState.IsValid로 자동 검증
- 향후 FluentValidation으로 마이그레이션 고려 (복잡한 검증 규칙 시)

---

## 2025-10-27 17:45

### Task Completed
- [x] 3.2 Create Pet Response DTOs

### Files Changed
- `IdleRPG.Application/DTOs/Pet/PetDto.cs` (new file, 47 lines)
- `IdleRPG.Application/DTOs/Pet/DuplicateRewardDto.cs` (new file, 18 lines)
- `IdleRPG.Application/DTOs/Pet/PetGachaResponseDto.cs` (new file, 34 lines)
- `IdleRPG.Application/DTOs/Pet/PetLevelUpResponseDto.cs` (new file, 36 lines)
- `IdleRPG.Application/DTOs/Pet/EquippedPetDto.cs` (new file, 37 lines)
- `IdleRPG.Application/DTOs/Pet/PetEquipResponseDto.cs` (new file, 30 lines)

### Key Decisions
- **6개 Response DTO 생성**:
  1. **PetDto**: 펫 기본 정보 (가챠, 목록, 상세 조회 공통)
  2. **DuplicateRewardDto**: 중복 펫 골드 보상
  3. **PetGachaResponseDto**: 가챠 결과 (신규 펫 + 중복 보상 + 천장 카운터)
  4. **PetLevelUpResponseDto**: 레벨업 결과 (새 스탯 + 비용 + 남은 골드)
  5. **EquippedPetDto**: 장착된 펫 정보 (슬롯 + 버프)
  6. **PetEquipResponseDto**: 장착 결과 (모든 슬롯 + 총 버프)

- **PetDto 설계**:
  - RarityName: string (Enum을 문자열로 변환, 클라이언트 편의성)
  - ImageUrl: 서버가 TemplateId/Name 기반 경로 생성 (클라이언트 리소스 매핑)
  - IsEquipped: 목록 조회 시 장착 상태 표시 (UI 편의성)

- **PetGachaResponseDto 설계**:
  - Pets: 신규 획득 펫만 포함 (중복은 DuplicateRewards에)
  - TotalGoldFromDuplicates: 중복 보상 합계 (클라이언트가 직접 계산 불필요)
  - CurrentPityCount: 가챠 후 천장 카운터 (UI 프로그레스바: 0/50)

- **EquippedPetDto vs PetDto**:
  - EquippedPetDto: 슬롯 정보 + 버프 계산 결과 포함 (장착 특화)
  - PetDto: 범용 펫 정보 (IsEquipped 플래그만 추가)

- **XML 문서화**:
  - 각 DTO와 속성에 한국어 설명 추가
  - 버프 계산 공식 명시 (CurrentAttack * 0.1)
  - 골드 보상 금액 명시 (Common 100 ~ Legendary 10,000)

### Notes
- 기존 GachaResultDto, SkillDto 패턴 참조
- Response DTO는 읽기 전용 (setter 없음, init-only 고려)
- ImageUrl은 Service Layer에서 생성 (예: `/resources/pets/{templateId}.png`)
- 향후 AutoMapper로 Entity → DTO 매핑 자동화 고려

### Architecture Alignment
- **design.md API Design - Response 섹션과 100% 일치**:
  - PetGachaResponseDto: design.md Line 219-241
  - PetDto: design.md Line 262-275
  - PetLevelUpResponseDto: design.md Line 302-312
  - PetEquipResponseDto: design.md Line 342-356

---

## 2025-10-27 18:00

### Task Completed
- [x] 3.3 Create PetService Interface

### Files Changed
- `IdleRPG.Application/Services/IPetService.cs` (new file, 74 lines)

### Key Decisions
- **8개 메서드 시그니처 정의**:
  1. **DrawPetsAsync**: 가챠 (count: 1 or 10)
  2. **GetPetByIdAsync**: 펫 상세 조회
  3. **GetPetsByCharacterIdAsync**: 캐릭터 소유 펫 목록
  4. **LevelUpPetAsync**: 펫 레벨업 (골드 소모)
  5. **EquipPetAsync**: 펫 장착 (슬롯 1-3)
  6. **UnequipPetAsync**: 펫 해제
  7. **GetEquippedPetsAsync**: 장착된 펫 조회
  8. **DeletePetAsync**: 펫 삭제 (장착 중 불가)

- **메서드 네이밍 패턴**:
  - Async 접미사: 비동기 메서드 (Task<T> 반환)
  - Get/Draw/Equip/Unequip/Delete: 명확한 동사 사용 (CRUD + 도메인 동작)
  
- **매개변수 설계**:
  - characterId: 소유권 검증 필수 (모든 쓰기 작업)
  - petId: int (Pet Entity PK 타입)
  - slotIndex: int (1-3 범위, Validation은 Service 구현에서)
  - cancellationToken: 기본값 default (선택적 취소 지원)

- **반환 타입**:
  - DrawPetsAsync: PetGachaResponseDto (복합 응답)
  - LevelUpPetAsync: PetLevelUpResponseDto (레벨업 결과)
  - EquipPetAsync: PetEquipResponseDto (장착 결과 + 총 버프)
  - UnequipPetAsync: Task (void, 204 No Content)
  - DeletePetAsync: Task (void, 204 No Content)

- **XML 문서화**:
  - 각 메서드의 목적, 매개변수, 반환값 설명
  - 비즈니스 규칙 명시 (예: "장착된 펫은 삭제 불가")

### Notes
- 기존 ISkillService 패턴 참조 (일관성)
- 인터페이스는 계약(Contract)만 정의, 구현은 Task 3.4에서
- design.md Service Layer Design 섹션과 100% 일치
- CancellationToken: 장기 실행 작업 취소 지원 (가챠 10연차 등)

### Architecture Alignment
- **design.md Service Layer Design - PetService 섹션과 완전 일치**:
  - DrawPetsAsync: design.md Line 568-596
  - LevelUpPetAsync: design.md Line 613-642
  - EquipPetAsync: design.md Line 653-685

---

## 2025-10-27 19:00

### Task Completed
- [x] 4.1 Create PetsController
- [x] 4.2 Add Pet Equip/Unequip Endpoints
- [x] 4.3 Add Pet Detail & Delete Endpoints

### Files Changed
- `IdleRPG.API/Controllers/PetsController.cs` (new file, 405 lines)
- `IdleRPG.Infrastructure/Services/PetService.cs` (modified, 오류 수정)
- `IdleRPG.Infrastructure/Repositories/EquippedPetsRepository.cs` (modified, 오류 수정)

### Key Decisions
- **8개 엔드포인트 구현 완료**:
  1. **POST /api/pets/gacha** (`[Authorize]`): 펫 가챠 (1회 또는 10연차)
  2. **GET /api/pets?characterId={id}** (`[AllowAnonymous]`): 펫 목록 조회 (Public)
  3. **POST /api/pets/{petId}/level-up** (`[Authorize]`): 펫 레벨업
  4. **POST /api/pets/equip** (`[Authorize]`): 펫 장착 (슬롯 1-3)
  5. **POST /api/pets/unequip** (`[Authorize]`): 펫 해제
  6. **GET /api/pets/equipped?characterId={id}** (`[AllowAnonymous]`): 장착된 펫 조회 (Public)
  7. **GET /api/pets/{petId}** (`[AllowAnonymous]`): 펫 상세 조회 (Public)
  8. **DELETE /api/pets/{petId}?characterId={id}** (`[Authorize]`): 펫 삭제

- **Public vs Authorize 전략**:
  - **읽기 API는 Public** (`[AllowAnonymous]`): 리더보드, 랭킹, 다른 플레이어 정보 조회 허용
  - **쓰기 API는 인증 필수** (`[Authorize]`): JWT 토큰 검증 필요
  - EquipmentController, DungeonController 패턴 참조

- **예외 처리 전략**:
  - `InvalidOperationException` → 400 Bad Request (비즈니스 규칙 위반)
  - `KeyNotFoundException` → 404 Not Found (리소스 없음)
  - `UnauthorizedAccessException` → 403 Forbidden (소유권 검증 실패)
  - `Exception` → 500 Internal Server Error (예상치 못한 오류)

- **Validation 계층 분리**:
  - Controller: 입력 형식 검증 (count: 1 or 10, slotIndex: 1-3)
  - Service: 비즈니스 규칙 검증 (골드 부족, 최대 레벨, 장착 중 삭제)

- **Swagger 문서화**:
  - `<summary>`, `<param>`, `<returns>`, `<response>` XML 주석 추가
  - ProducesResponseType 특성으로 응답 타입 명시
  - 각 엔드포인트의 HTTP 상태 코드 문서화 (200, 201, 204, 400, 401, 403, 404, 500)

- **Logging 전략**:
  - 성공: `LogInformation` (가챠 결과, 레벨업, 장착)
  - 실패: `LogWarning` (Validation 실패, 리소스 없음)
  - 오류: `LogError` (예상치 못한 예외)

### Notes
- **UnequipPetRequestDto 추가**: Controller 내부 클래스로 정의 (Application Layer에 정의되지 않았으므로)
- **컴파일 오류 해결**:
  1. `GetByIdAsync(characterId, cancellationToken)` → `GetByIdAsync(characterId)` (ICharacterRepository는 CancellationToken 미지원)
  2. `_randomProvider.Next(0, templates.Count)` → `_randomProvider.Next(templates.Count)` (0-based 랜덤)
  3. `pet.Template` → `pet.PetTemplate` (Navigation Property 이름)
  4. `pet.PetTemplateId` → `pet.TemplateId` (Entity 속성 이름)
  5. `UpdateAsync(character, cancellationToken)` → `UpdateAsync(character)` (IRepository<T>는 CancellationToken 미지원)
- **빌드 성공**: 경고 0개, 오류 0개
- **tasks.md 업데이트**: Milestone 4 (API Layer) 완료, 진행률 19/28 (68%)

### Architecture Alignment
- **design.md API Design 섹션과 100% 일치**:
  - POST /api/pets/gacha: design.md Line 165-189
  - GET /api/pets: design.md Line 193-209
  - POST /api/pets/{id}/level-up: design.md Line 283-299
  - POST /api/pets/equip: design.md Line 331-347
  - POST /api/pets/unequip: design.md Line 365-376
  - GET /api/pets/equipped: design.md Line 380-392
  - GET /api/pets/{id}: design.md Line 396-409
  - DELETE /api/pets/{id}: design.md Line 413-431

---

## 🎉 Milestone 4 Complete: API Layer (3/3 tasks, 100%)

### 완료된 작업:
1. ✅ Create PetsController (8개 엔드포인트)
2. ✅ Add Pet Equip/Unequip Endpoints (장착/해제)
3. ✅ Add Pet Detail & Delete Endpoints (상세/삭제)

### 주요 아키텍처 결정:
- ✅ Public/Authorize 전략: 읽기 Public, 쓰기 인증
- ✅ RESTful 설계: POST /gacha (201), GET /pets (200), DELETE (204)
- ✅ 예외 처리 계층화: InvalidOperationException (400), KeyNotFoundException (404), UnauthorizedAccessException (403)
- ✅ Swagger 문서화: XML 주석 + ProducesResponseType
- ✅ Logging: 성공/실패/오류 레벨 분리
- ✅ Controller 책임 분리: Validation (입력 형식), Service (비즈니스 규칙)

### Next Milestone: **Database** (3 tasks)
- Migration 작성 (pets, pet_templates, equipped_pets, character.pet_gacha_count)
- PetTemplate Seeder (11개 펫)
- DI Container 등록

---
