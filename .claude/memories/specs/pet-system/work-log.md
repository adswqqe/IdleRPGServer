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
