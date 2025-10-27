# Design: Pet System

> 이 문서는 Pet System 기능의 기술 설계를 정의합니다.
>
> **작성 방식**: Requirements 승인 후, Clean Architecture 계층별 구성요소 정의
> **학습 초점**: 계층 분리, DDD 패턴, 트랜잭션 경계, 데이터 모델링

**Requirements 추적성**: [requirements.md](./requirements.md) - US-1, US-2, US-3

---

## 📐 Architecture Overview

### Layer Responsibilities

#### API Layer
- **Controllers**: `PetsController` (신규)
- **Endpoints**:
  - `POST /api/pets/gacha` - 펫 가챠 (US-1)
  - `GET /api/pets?characterId={id}` - 펫 목록 조회 (US-1)
  - `GET /api/pets/{petId}` - 펫 상세 조회
  - `POST /api/pets/{petId}/level-up` - 펫 레벨업 (US-2)
  - `POST /api/pets/equip` - 펫 장착 (US-3)
  - `POST /api/pets/unequip` - 펫 해제 (US-3)
  - `GET /api/pets/equipped?characterId={id}` - 장착된 펫 조회 (US-3)
  - `DELETE /api/pets/{petId}` - 펫 삭제
- **DTOs**: `PetGachaRequestDto`, `PetGachaResponseDto`, `PetDto`, `PetEquipRequestDto`, `PetLevelUpRequestDto`

#### Application Layer
- **Services**:
  - `IPetService` / `PetService` - 펫 CRUD, 레벨업, 장착/해제 로직
- **Business Logic**:
  - 펫 가챠 오케스트레이션 (크리스탈 차감 → Domain Service 호출 → 중복 처리 → 천장 카운터 관리)
  - 펫 장착/해제 (슬롯 검증, 기존 펫 해제, EquippedPets 업데이트)
  - 펫 레벨업 (골드 차감, 스탯 계산)
- **Validation**:
  - 크리스탈 부족 검증 (가챠 비용 100)
  - 골드 부족 검증 (레벨업 비용)
  - 슬롯 범위 검증 (1-3)
  - 펫 소유권 검증 (CharacterId 일치)

#### Domain Layer
- **Entities**:
  - `Pet` (신규) - 펫 인스턴스
  - `PetTemplate` (신규) - 펫 마스터 데이터
  - `EquippedPets` (신규) - 펫 장착 정보
  - `Character` (수정) - `PetGachaCount` 필드 추가
- **Value Objects**:
  - `PetGachaCounter` (신규) - 천장 카운터 로직 캡슐화 (Immutable)
- **Domain Services**:
  - `PetGachaService` (신규) - 가챠 확률 계산 (순수 비즈니스 로직)
- **Enums**:
  - `PetRarity` (신규) - Common, Rare, Epic, Legendary (SkillRarity 재사용 패턴)

#### Infrastructure Layer
- **Repositories**:
  - `IPetRepository` / `PetRepository` (신규)
  - `IPetTemplateRepository` / `PetTemplateRepository` (신규)
  - `ICharacterRepository` (수정) - PetGachaCount 필드 추가
- **Data Access**: EF Core Configuration (`PetConfiguration.cs`, `PetTemplateConfiguration.cs`, `EquippedPetsConfiguration.cs`)
- **External Services**: 없음 (게임 내부 로직만)

---

## 🗄️ Data Model

### 신규 테이블: `pets`

**목적**: 캐릭터가 소유한 펫 인스턴스를 저장

**필드**:
- `id` (PK): 고유 식별자 (INT AUTO_INCREMENT)
- `character_id` (FK → characters.id): 소유자 캐릭터
- `template_id` (FK → pet_templates.id): 펫 마스터 데이터 참조
- `level` (INT): 펫 레벨 (1-50, Default 1)
- `current_attack` (INT): 현재 공격력 (레벨업 시 증가)
- `current_mana` (INT): 현재 마나 (레벨업 시 증가)
- `created_at` (TIMESTAMP): 획득 시간
- `updated_at` (TIMESTAMP): 최종 수정 시간

**제약사항**:
- `level`: 1 이상 50 이하 (`CHECK (level BETWEEN 1 AND 50)`)
- `current_attack`, `current_mana`: 0 이상 (`CHECK (current_attack >= 0 AND current_mana >= 0)`)
- `character_id`: NOT NULL

**인덱스 요구사항**:
- `idx_pets_character_id` (단일 컬럼): 가장 빈번한 쿼리 "내 캐릭터의 펫 목록" (99% 사용 케이스)
- **추가 인덱스 고려 시점**: 펫 개수 500+ 초과 시 (RarityId, TemplateId 필터는 클라이언트에서 처리)

**관계**:
- `Character`와 N:1 관계 (FK: `character_id`, ON DELETE CASCADE)
- `PetTemplate`와 N:1 관계 (FK: `template_id`, ON DELETE RESTRICT)

---

### 신규 테이블: `pet_templates`

**목적**: 펫 마스터 데이터 (종류별 기본 정보)

**필드**:
- `id` (PK): 고유 식별자 (INT, 시퀀스)
- `name` (VARCHAR(50)): 펫 이름 (예: "Fire Dragon", "Ice Wolf")
- `rarity` (INT): PetRarity enum (0: Common, 1: Rare, 2: Epic, 3: Legendary)
- `base_attack` (INT): 기본 공격력 (Lv1 기준)
- `base_mana` (INT): 기본 마나 (Lv1 기준)
- `image_url` (VARCHAR(255)): 펫 이미지 URL (Unity 클라이언트용)
- `description` (TEXT): 펫 설명 (선택)

**제약사항**:
- `name`: NOT NULL, UNIQUE
- `rarity`: NOT NULL, 0-3 범위 (`CHECK (rarity BETWEEN 0 AND 3)`)
- `base_attack`, `base_mana`: 0 이상

**인덱스 요구사항**:
- `idx_pet_templates_rarity` (단일 컬럼): 희귀도별 필터 조회 (가챠 풀 구성)

**관계**:
- `Pet`과 1:N 관계 (Pet.template_id → PetTemplate.id)

---

### 신규 테이블: `equipped_pets`

**목적**: 캐릭터의 펫 장착 정보 (최대 3슬롯)

**필드**:
- `character_id` (PK, FK → characters.id): 캐릭터 식별자
- `slot_index` (PK): 슬롯 번호 (1, 2, 3)
- `pet_id` (FK → pets.id, UNIQUE): 장착된 펫 (한 펫은 하나의 슬롯에만 장착)
- `equipped_at` (TIMESTAMP): 장착 시간

**제약사항**:
- Composite PK: (character_id, slot_index)
- UNIQUE: (pet_id) - 한 펫은 하나의 슬롯에만 장착
- CHECK: `slot_index BETWEEN 1 AND 3`

**관계**:
- `Character`와 N:1 관계 (ON DELETE CASCADE)
- `Pet`과 N:1 관계 (ON DELETE CASCADE)

---

### 수정 테이블: `characters`

**변경사항**:
- 추가 필드: `pet_gacha_count` (INT, Default 0, NOT NULL)
  - 목적: 천장 시스템 카운터 (0-50)
  - 제약: `CHECK (pet_gacha_count BETWEEN 0 AND 50)`

---

### Entity Relationships (ERD)

```
Player (1) ──< (N) Character (1) ──< (N) Pet
                ↓                       ↓
              (1:N)                   (N:1)
                ↓                       ↓
           EquippedPets (M:N)      PetTemplate (1:N)
```

**관계 설명**:
- `Pet` → `Character`: N:1 (한 캐릭터가 여러 펫 소유)
- `Pet` → `PetTemplate`: N:1 (펫 인스턴스는 하나의 템플릿 참조)
- `EquippedPets`: Character ↔ Pet M:N (제약: 캐릭터당 최대 3개, 펫당 최대 1개)
- Cascade 규칙: Character 삭제 시 Pet, EquippedPets 자동 삭제 (ON DELETE CASCADE)

---

### EF Core Configuration 요구사항

**Fluent API 필요 항목**:

#### `PetConfiguration.cs`
- `HasOne(p => p.Character).WithMany().HasForeignKey(p => p.CharacterId).OnDelete(DeleteBehavior.Cascade)`
- `HasOne(p => p.Template).WithMany().HasForeignKey(p => p.TemplateId).OnDelete(DeleteBehavior.Restrict)`
- `HasIndex(p => p.CharacterId).HasDatabaseName("idx_pets_character_id")`
- Property 제약: `Level` (Range 1-50), `CurrentAttack`, `CurrentMana` (Min 0)

#### `PetTemplateConfiguration.cs`
- `HasIndex(pt => pt.Name).IsUnique()`
- `HasIndex(pt => pt.Rarity).HasDatabaseName("idx_pet_templates_rarity")`
- Property 제약: `Name` (MaxLength 50), `Rarity` (Range 0-3)

#### `EquippedPetsConfiguration.cs`
- Composite PK: `HasKey(ep => new { ep.CharacterId, ep.SlotIndex })`
- `HasIndex(ep => ep.PetId).IsUnique()`
- `HasOne<Character>().WithMany().HasForeignKey(ep => ep.CharacterId).OnDelete(DeleteBehavior.Cascade)`
- `HasOne<Pet>().WithMany().HasForeignKey(ep => ep.PetId).OnDelete(DeleteBehavior.Cascade)`
- Property 제약: `SlotIndex` (Range 1-3)

#### `CharacterConfiguration.cs` (수정)
- `Property(c => c.PetGachaCount).HasDefaultValue(0)` 추가

> 💡 **구현 참고**: 구체적인 Fluent API 코드는 `IdleRPG.Infrastructure/Configurations/` 에서 작성

---

## 🔌 API Design

### 1. `POST /api/pets/gacha` - 펫 가챠 (US-1)

**Authentication**: ✅ `[Authorize]` (JWT Bearer Token)

**Request:**
```json
{
  "characterId": "guid",
  "count": 1  // 1 or 10 (10연차)
}
```

**Validation:**
- `characterId`: NOT NULL, 소유권 검증 (Character.PlayerId == JWT PlayerId)
- `count`: 1 또는 10만 허용
- 크리스탈 부족 검증: `character.Crystal >= count * 100` (1회 100, 10연차 900)

**Response (201 Created):**
```json
{
  "pets": [
    {
      "id": 123,
      "templateName": "Fire Dragon",
      "rarityName": "Legendary",
      "level": 1,
      "currentAttack": 100,
      "currentMana": 50,
      "imageUrl": "https://..."
    }
  ],
  "duplicateRewards": [
    {
      "petTemplateName": "Ice Wolf",
      "goldReward": 500
    }
  ],
  "currentPityCount": 1,
  "remainingCrystal": 900,
  "totalGoldFromDuplicates": 500
}
```

**Errors:**
- `400 Bad Request`: "Invalid count. Must be 1 or 10."
- `400 Bad Request`: "Insufficient crystal. Required: 100, Current: 50."
- `401 Unauthorized`: "Invalid or missing token."
- `404 Not Found`: "Character not found."

---

### 2. `GET /api/pets?characterId={id}` - 펫 목록 조회

**Authentication**: ❌ Public (읽기 API, 리더보드/PvP 대비)

**Query Parameters:**
- `characterId` (GUID, Required)
- `rarity` (INT, Optional): 0-3 필터
- `sortBy` (String, Optional): "level", "attack" (Default: "created_at DESC")

**Response (200 OK):**
```json
{
  "pets": [
    {
      "id": 123,
      "templateName": "Fire Dragon",
      "rarityName": "Legendary",
      "level": 10,
      "currentAttack": 190,
      "currentMana": 95,
      "imageUrl": "https://...",
      "isEquipped": true
    }
  ],
  "totalCount": 50
}
```

**Errors:**
- `404 Not Found`: "Character not found."

---

### 3. `POST /api/pets/{petId}/level-up` - 펫 레벨업 (US-2)

**Authentication**: ✅ `[Authorize]`

**Request Body:**
```json
{
  "characterId": "guid"
}
```

**Validation:**
- 펫 소유권 검증: `pet.CharacterId == request.CharacterId`
- 최대 레벨 검증: `pet.Level < 50`
- 골드 부족 검증: `character.Gold >= levelUpCost`
  - 비용 공식: `100 * Math.Pow(1.5, pet.Level - 1)` (Lv1→2: 100골드, Lv2→3: 150골드)

**Response (200 OK):**
```json
{
  "petId": 123,
  "newLevel": 11,
  "newAttack": 200,
  "newMana": 100,
  "costGold": 150,
  "remainingGold": 850
}
```

**Errors:**
- `400 Bad Request`: "Pet already at max level (50)."
- `400 Bad Request`: "Insufficient gold. Required: 150, Current: 100."
- `403 Forbidden`: "You don't own this pet."
- `404 Not Found`: "Pet not found."

---

### 4. `POST /api/pets/equip` - 펫 장착 (US-3)

**Authentication**: ✅ `[Authorize]`

**Request:**
```json
{
  "characterId": "guid",
  "petId": 123,
  "slotIndex": 1  // 1, 2, 3
}
```

**Validation:**
- `slotIndex`: 1-3 범위
- 펫 소유권 검증: `pet.CharacterId == request.CharacterId`
- 중복 장착 검증: 펫이 이미 다른 슬롯에 장착되어 있으면 이동 처리

**Response (200 OK):**
```json
{
  "characterId": "guid",
  "equippedPets": [
    {
      "slotIndex": 1,
      "petId": 123,
      "petName": "Fire Dragon",
      "buffAttack": 19,  // Pet.CurrentAttack * 0.1
      "buffMana": 9      // Pet.CurrentMana * 0.1
    }
  ],
  "totalBuffAttack": 19,
  "totalBuffMana": 9
}
```

**Errors:**
- `400 Bad Request`: "Invalid slot index. Must be 1, 2, or 3."
- `403 Forbidden`: "You don't own this pet."
- `404 Not Found`: "Pet not found."

---

### 5. `POST /api/pets/unequip` - 펫 해제

**Authentication**: ✅ `[Authorize]`

**Request:**
```json
{
  "characterId": "guid",
  "slotIndex": 1
}
```

**Response (204 No Content)**

**Errors:**
- `404 Not Found`: "No pet equipped in slot 1."

---

### 6. `GET /api/pets/equipped?characterId={id}` - 장착된 펫 조회

**Authentication**: ❌ Public

**Response (200 OK):**
```json
{
  "equippedPets": [
    {
      "slotIndex": 1,
      "petId": 123,
      "petName": "Fire Dragon",
      "level": 10,
      "buffAttack": 19,
      "buffMana": 9
    }
  ]
}
```

---

### 7. `DELETE /api/pets/{petId}` - 펫 삭제

**Authentication**: ✅ `[Authorize]`

**Validation:**
- 장착된 펫은 삭제 불가: `EquippedPets`에 존재하면 400 에러

**Response (204 No Content)**

**Errors:**
- `400 Bad Request`: "Cannot delete equipped pet. Unequip first."
- `403 Forbidden`: "You don't own this pet."

---

## 🧮 Business Logic

### 핵심 알고리즘

#### 1. 펫 가챠 확률 계산 (AI 제안)

**입력:**
- `character.PetGachaCount` (천장 카운터, 0-50)

**출력:**
- `PetRarity` (Common, Rare, Epic, Legendary)

**프로세스 (AI 제안)**:
```
1. Soft Pity 적용 (40회부터 확률 증가):
   - Count < 40: 기본 확률
   - Count >= 40: Legendary 확률 += (Count - 39) * 1%
   - 예: 40회 +1% (2%), 45회 +6% (7%)

2. Hard Pity (50회):
   - Count == 50: 100% Legendary 지급 후 카운터 초기화

3. 기본 확률 (Count < 40):
   - Random(0~100)
   - [0, 1): Legendary 1%
   - [1, 10): Epic 9%
   - [10, 40): Rare 30%
   - [40, 100): Common 60%

4. 확률 계산 후 템플릿 선택:
   - PetTemplate 테이블에서 해당 Rarity의 모든 템플릿 조회
   - Random 선택 (동일 Rarity 내에서 균등 확률)
```

**예외 처리:**
- `PetTemplate` 테이블에 해당 Rarity의 템플릿이 없으면 → `InvalidOperationException` (시스템 오류)

**🎓 학습 포인트 (아키텍처 결정)**:
- **TODO(human)**: 가챠 로직 계층 배치
  - 옵션 A: `PetGachaService` (Domain Layer) - 순수 비즈니스 로직, 외부 의존성 없음
  - 옵션 B: `PetService` (Application Layer) - Repository 의존성 포함
  - 학습 포인트: Domain Service vs Application Service 책임 분리
  - 권장: Domain Service (테스트 용이성, 재사용성)

- **TODO(human)**: Random 생성 추상화
  - 옵션 A: `IRandomProvider` 인터페이스 (DI) - 테스트 시 Mock 가능
  - 옵션 B: `System.Random` 직접 사용 - 간단하지만 테스트 어려움
  - 학습 포인트: 의존성 주입, 테스트 가능한 설계
  - 권장: `IRandomProvider` (이미 Skill Gacha에서 사용 중)

> 💡 **학습 가이드**: 확률 계산식은 AI가 제안합니다. **계층 분리** (Domain vs Application)와 **의존성 주입**에 집중하세요.

---

#### 2. 펫 스탯 계산 (레벨업 시)

**입력:**
- `PetTemplate.BaseAttack`, `PetTemplate.BaseMana`
- `Pet.Level`

**출력:**
- `Pet.CurrentAttack`, `Pet.CurrentMana`

**공식 (AI 제안)**:
```csharp
Pet.CurrentAttack = Template.BaseAttack + (Level - 1) * 10
Pet.CurrentMana = Template.BaseMana + (Level - 1) * 5
```

**예시**:
- Fire Dragon (BaseAttack: 100, BaseMana: 50)
- Lv1: Attack 100, Mana 50
- Lv10: Attack 190, Mana 95
- Lv50: Attack 590, Mana 295

---

#### 3. 캐릭터 버프 계산 (펫 장착 시)

**입력:**
- 장착된 펫 목록 (`EquippedPets`)

**출력:**
- `TotalBuffAttack`, `TotalBuffMana`

**공식 (AI 제안)**:
```csharp
TotalBuffAttack = Sum(Pet.CurrentAttack * 0.1) for all equipped pets
TotalBuffMana = Sum(Pet.CurrentMana * 0.1) for all equipped pets
```

**🎓 학습 포인트 (아키텍처 결정)**:
- **TODO(human)**: 버프 계산 시점
  - 옵션 A: Character 조회 시마다 실시간 계산 (계산 속성)
    - 장점: 데이터 일관성 보장
    - 단점: 매번 EquippedPets 조인 쿼리 필요
  - 옵션 B: 펫 장착/해제 시 Character.BuffAttack, Character.BuffMana 필드 업데이트 (캐싱)
    - 장점: 조회 성능 향상 (Character 조회 시 JOIN 불필요)
    - 단점: 데이터 동기화 복잡도 증가, 펫 레벨업 시 Character 업데이트 필요
  - 학습 포인트: 읽기 성능 vs 데이터 정합성 트레이드오프
  - 권장: 옵션 A (학습 초점, 정합성 우선)

---

#### 4. 중복 펫 처리 (가챠 시)

**입력:**
- 획득한 `PetTemplate.Id`, `Character.Id`

**출력:**
- `IsDuplicate` (bool), `GoldReward` (int)

**프로세스**:
```
1. 이미 보유 여부 확인:
   - Query: SELECT * FROM pets WHERE character_id = ? AND template_id = ?

2. 중복 시 골드 보상 (AI 제안):
   - Common: 100 Gold
   - Rare: 500 Gold
   - Epic: 2,000 Gold
   - Legendary: 10,000 Gold

3. 중복이 아니면 Pet Entity 생성
```

**예외 처리:**
- 중복 처리 중 DB 오류 발생 시 → 전체 트랜잭션 롤백 (Unit of Work)

---

## 🎯 Service Layer Design

### `PetService` (Application Layer)

**책임**: 펫 CRUD, 레벨업, 장착/해제 오케스트레이션

**의존성:**
- `IPetRepository`
- `IPetTemplateRepository`
- `ICharacterRepository`
- `PetGachaService` (Domain Service)
- `IRandomProvider`
- `IUnitOfWork` (EF Core SaveChangesAsync)

---

#### `DrawPetsAsync(characterId, count, cancellationToken)`

**시그니처**:
- 입력: `Guid characterId`, `int count` (1 or 10), `CancellationToken`
- 반환: `Task<PetGachaResponseDto>` (획득한 펫 목록, 중복 보상, 천장 카운터)

**프로세스 흐름**:
```
1. Input Validation:
   - count == 1 or 10 (아니면 ValidationException)
   - character.Crystal >= cost (부족 시 ValidationException)

2. 트랜잭션 시작 (Unit of Work):
   - Character 조회 (Tracking)
   - Crystal 차감: character.Crystal -= cost

3. 가챠 로직 (count 횟수만큼 반복):
   - PetGachaService.DrawPet(character.PetGachaCount) → PetRarity
   - PetTemplate 조회 (해당 Rarity)
   - 중복 체크 (Pet 테이블 조회)
   - 중복 시: Gold 보상 추가
   - 중복 아님: Pet Entity 생성
   - 천장 카운터 증가: character.PetGachaCount += 1
   - Legendary 획득 시: character.PetGachaCount = 0 (초기화)

4. SaveChangesAsync (트랜잭션 커밋)

5. Return PetGachaResponseDto
```

**에러 조건**:
- `ValidationException`: "Invalid count. Must be 1 or 10."
- `ValidationException`: "Insufficient crystal. Required: {cost}, Current: {character.Crystal}."
- `NotFoundException`: "Character not found."
- `InvalidOperationException`: "No pet templates found for rarity: {rarity}." (시스템 오류)

**🎓 학습 포인트 (아키텍처 결정)**:
- **TODO(human)**: 트랜잭션 경계 설정
  - 현재 설계: Service Layer에서 Unit of Work (EF Core SaveChanges 자동 트랜잭션)
  - 대안: Repository Layer에서 개별 트랜잭션
  - 학습 포인트: 트랜잭션 관리 책임 (Service vs Repository)
  - 권장: Service Layer (비즈니스 로직 원자성 보장)

---

#### `LevelUpPetAsync(petId, characterId, cancellationToken)`

**시그니처**:
- 입력: `int petId`, `Guid characterId`, `CancellationToken`
- 반환: `Task<PetLevelUpResponseDto>` (새 레벨, 스탯, 남은 골드)

**프로세스 흐름**:
```
1. Input Validation:
   - Pet 조회 (Include Template)
   - 소유권 검증: pet.CharacterId == characterId
   - 최대 레벨 검증: pet.Level < 50

2. 비용 계산 (AI 제안):
   - cost = (int)(100 * Math.Pow(1.5, pet.Level - 1))
   - 예: Lv1→2 (100), Lv2→3 (150), Lv10→11 (3,844)

3. Character 조회 및 골드 차감:
   - character.Gold >= cost (부족 시 ValidationException)
   - character.Gold -= cost

4. 스탯 계산 (AI 제안):
   - pet.Level += 1
   - pet.CurrentAttack = template.BaseAttack + (pet.Level - 1) * 10
   - pet.CurrentMana = template.BaseMana + (pet.Level - 1) * 5
   - pet.UpdatedAt = DateTime.UtcNow

5. SaveChangesAsync (트랜잭션)

6. Return PetLevelUpResponseDto
```

**에러 조건**:
- `ValidationException`: "Pet already at max level (50)."
- `ValidationException`: "Insufficient gold. Required: {cost}, Current: {character.Gold}."
- `ForbiddenException`: "You don't own this pet."
- `NotFoundException`: "Pet not found."

---

#### `EquipPetAsync(characterId, petId, slotIndex, cancellationToken)`

**시그니처**:
- 입력: `Guid characterId`, `int petId`, `int slotIndex` (1-3), `CancellationToken`
- 반환: `Task<PetEquipResponseDto>` (장착된 펫 목록, 총 버프)

**프로세스 흐름**:
```
1. Input Validation:
   - slotIndex: 1-3 범위
   - Pet 조회 (Include Template)
   - 소유권 검증: pet.CharacterId == characterId

2. 기존 장착 상태 확인:
   - EquippedPets 조회 (WHERE pet_id = petId)
   - 이미 장착된 경우: 기존 슬롯 삭제

3. 슬롯에 다른 펫 장착 확인:
   - EquippedPets 조회 (WHERE character_id = ? AND slot_index = ?)
   - 존재 시: 삭제 (기존 펫 해제)

4. 새 장착 정보 추가:
   - EquippedPets Entity 생성
   - (CharacterId, PetId, SlotIndex, EquippedAt)

5. SaveChangesAsync

6. 버프 계산 (실시간):
   - EquippedPets 전체 조회 (Include Pet, Template)
   - TotalBuffAttack = Sum(Pet.CurrentAttack * 0.1)
   - TotalBuffMana = Sum(Pet.CurrentMana * 0.1)

7. Return PetEquipResponseDto
```

**에러 조건**:
- `ValidationException`: "Invalid slot index. Must be 1, 2, or 3."
- `ForbiddenException`: "You don't own this pet."
- `NotFoundException`: "Pet not found."

---

### `PetGachaService` (Domain Service)

**책임**: 순수 가챠 확률 계산 (외부 의존성 없음)

**메서드:**

#### `DrawPet(pityCount, randomProvider)`

**시그니처**:
- 입력: `int pityCount` (0-50), `IRandomProvider randomProvider`
- 반환: `PetRarity`

**프로세스**:
```
1. Hard Pity 체크:
   - IF pityCount >= 50 → RETURN Legendary

2. Soft Pity 확률 조정:
   - baseRate = 1 (Legendary 기본 확률 1%)
   - IF pityCount >= 40:
       adjustedRate = baseRate + (pityCount - 39) * 1
   - ELSE:
       adjustedRate = baseRate

3. Random 값 생성 (0-100):
   - roll = randomProvider.Next(0, 100)

4. 확률 구간 판정:
   - [0, adjustedRate): Legendary
   - [adjustedRate, adjustedRate + 9): Epic
   - [adjustedRate + 9, adjustedRate + 39): Rare
   - [adjustedRate + 39, 100): Common

5. Return PetRarity
```

**테스트 용이성**: `IRandomProvider`를 Mock하여 확률 테스트 가능

---

## 🧪 Testing Strategy

### Unit Tests

#### `PetGachaServiceTests` (Domain Service)
- `DrawPet_PityCount50_ReturnsLegendary` - Hard Pity 검증
- `DrawPet_PityCount45_IncreasesLegendaryRate` - Soft Pity 검증 (6% → 7%)
- `DrawPet_PityCount0_Returns60PercentCommon` - 기본 확률 (Mock Random 0-59 → Common)
- `DrawPet_PityCount0_Returns1PercentLegendary` - Legendary 1% (Mock Random 0 → Legendary)

#### `PetServiceTests` (Application Service)
- `DrawPetsAsync_InsufficientCrystal_ThrowsValidationException` - 크리스탈 부족
- `DrawPetsAsync_DuplicatePet_ReturnsGoldReward` - 중복 처리 (Mock Repository)
- `DrawPetsAsync_LegendaryDrawn_ResetsPityCount` - 천장 카운터 초기화
- `LevelUpPetAsync_MaxLevel_ThrowsValidationException` - Lv50 레벨업 불가
- `EquipPetAsync_SlotOccupied_ReplacesExistingPet` - 슬롯 교체 로직

### Integration Tests

#### `PetsControllerTests` (API Layer)
- `POST /api/pets/gacha` - 401 Unauthorized (토큰 없음)
- `POST /api/pets/gacha` - 201 Created (정상 가챠)
- `POST /api/pets/gacha` - 400 Bad Request (크리스탈 부족)
- `POST /api/pets/{id}/level-up` - 200 OK (레벨업 성공)
- `POST /api/pets/equip` - 200 OK (펫 장착)
- `GET /api/pets?characterId={id}` - 200 OK (Public 접근 가능)

### Test Coverage Goals
- **Domain Services**: 90%+ (`PetGachaService` 확률 로직)
- **Application Services**: 80%+ (`PetService` 비즈니스 로직)
- **Controllers**: 70%+ (API 엔드포인트)

---

## ⚠️ Error Handling

### Exception Types
- `NotFoundException`: Pet, PetTemplate, Character 미존재
- `ValidationException`: 잘못된 입력 (크리스탈/골드 부족, 슬롯 범위, 레벨 제한)
- `ForbiddenException`: 소유권 검증 실패 (다른 캐릭터의 펫 수정 시도)
- `InvalidOperationException`: 시스템 오류 (PetTemplate 데이터 누락)

### Error Response Format
```json
{
  "message": "Insufficient crystal. Required: 100, Current: 50.",
  "statusCode": 400
}
```

---

## 🔐 Security Considerations

- **Authentication**: JWT Bearer Token (쓰기 API 필수)
  - `[Authorize]`: POST /api/pets/gacha, POST /api/pets/{id}/level-up, POST /api/pets/equip, DELETE /api/pets/{id}
- **Authorization**:
  - 소유권 검증: `Character.PlayerId == JWT PlayerId` (Middleware 또는 Service 계층)
  - 펫 소유권: `Pet.CharacterId == request.CharacterId`
- **읽기 API Public**:
  - GET /api/pets, GET /api/pets/equipped (게임 데이터 공개, 리더보드/PvP 대비)
  - 민감 정보 제외 (Crystal, PetGachaCount는 GET에서 반환 안 함)
- **Data Validation**: FluentValidation 또는 Data Annotations (DTO 검증)
- **SQL Injection Prevention**: EF Core 파라미터화 (FromSqlRaw 미사용)

---

## 📊 Performance Considerations

- **Database Indexes**:
  - `idx_pets_character_id` (가장 빈번한 쿼리)
  - `idx_pet_templates_rarity` (가챠 풀 구성)
- **N+1 Query Prevention**:
  - 읽기 전용 API: `Include(p => p.Template).AsNoTracking().Select(DTO)` (메모리 30-40% 절약)
  - 일반 CRUD: `Include(p => p.Template)` (Entity 추적 필요)
- **Pagination**: 펫 목록 조회 (100마리 이상 시 Skip/Take)
- **Response Time Goal**:
  - 펫 목록 조회 (100마리): < 200ms
  - 가챠 API: < 300ms
  - 펫 장착/해제: < 150ms

---

## 🔄 Migration Plan

### Database Migration 요구사항

**신규 테이블**:
- `pets` (8개 필드, Character FK, PetTemplate FK)
- `pet_templates` (7개 필드, Rarity 제약)
- `equipped_pets` (4개 필드, Composite PK, Unique 제약)

**수정 테이블**:
- `characters`: `pet_gacha_count` INT 필드 추가 (Default 0, CHECK 0-50)

**인덱스 추가**:
- `idx_pets_character_id`: 펫 목록 조회 성능
- `idx_pet_templates_rarity`: 가챠 풀 구성 성능

**Cascade 규칙**:
- `pets.character_id`: ON DELETE CASCADE (캐릭터 삭제 시 펫 자동 삭제)
- `equipped_pets.character_id`: ON DELETE CASCADE
- `equipped_pets.pet_id`: ON DELETE CASCADE (펫 삭제 시 장착 정보 자동 삭제)
- `pets.template_id`: ON DELETE RESTRICT (템플릿은 삭제 불가, 펫 인스턴스 존재 시)

**데이터 마이그레이션**:
- 기존 데이터 변환 없음 (신규 테이블)
- Seeder 필요: `PetTemplateSeeder.cs` (10개 펫 템플릿 데이터)

**🎓 학습 포인트 (데이터베이스 설계)**:
- **TODO(human)**: 인덱스 전략
  - 현재 설계: `idx_pets_character_id` 단일 컬럼 (99% 사용 케이스)
  - 대안: 복합 인덱스 `(character_id, rarity)` (Rarity 필터 빈번 시)
  - 학습 포인트: 인덱스 비용 vs 쿼리 성능 트레이드오프
  - 권장: 단일 컬럼 (Rarity 필터는 클라이언트에서 처리, 50-200마리 수준)
  - 재평가: 펫 개수 500+ 초과 시 복합 인덱스 고려

- **설명**: Cascade Delete (ON DELETE CASCADE)
  - 현재 설계: DB 레벨 Cascade (Character 삭제 시 Pet 자동 삭제)
  - 대안: Application 레벨 처리 (Service Layer에서 Pet 명시적 삭제)
  - 학습 포인트: DB 제약 vs Application 제어, 감사 로그
  - 이유: 단순한 소유 관계, 성능, 일관성 (별도 로깅 불필요)

> 💡 **학습 가이드**: SQL 문법은 AI가 작성합니다. **인덱싱**, **Cascade 규칙**, **타입 선택** (INT vs BIGINT, VARCHAR vs TEXT) 같은 설계 결정에 집중하세요.

---

### Data Seeding 계획

**Seeder 필요 여부**: Yes

**초기 데이터** (AI 제안 - Idle RPG 판타지 테마):

```
PetTemplate 10개:
1. Common (5개):
   - Slime (Attack 50, Mana 25)
   - Wolf (Attack 60, Mana 20)
   - Bat (Attack 55, Mana 30)
   - Goblin (Attack 65, Mana 15)
   - Rabbit (Attack 45, Mana 35)

2. Rare (3개):
   - Fire Fox (Attack 100, Mana 50)
   - Ice Wolf (Attack 110, Mana 45)
   - Thunder Eagle (Attack 105, Mana 55)

3. Epic (2개):
   - Dark Dragon (Attack 200, Mana 100)
   - Light Phoenix (Attack 190, Mana 110)

4. Legendary (1개):
   - Ancient Guardian (Attack 350, Mana 200)
```

**Seeder 클래스**: `PetTemplateSeeder.cs` (Infrastructure Layer)

**실행 시점**: Migration 적용 후, Program.cs에서 자동 실행 (DbInitializer)

> 💡 **구현 참고**: Seeder 코드는 Implementation 단계에서 작성

---

## 📝 Decision Log

> ⚠️ **중요**: L 사이즈 기능은 이 섹션 **필수**. 중요한 아키텍처 결정을 ADR과 연결.
>
> **Spike/ADR 트리거 조건**: Requirements 대화에서 모두 결정 완료

| ID | Decision | Rationale | Status |
|----|----------|-----------|--------|
| D1 | Pet-Character 관계는 1:N (ON DELETE CASCADE) | 단순한 소유 관계, 성능, 일관성 (별도 로깅 불필요) | Accepted |
| D2 | 가챠 확률 계산은 `PetGachaService` (Domain Layer) | 순수 비즈니스 로직, 외부 의존성 없음, 테스트 용이 | Accepted |
| D3 | 천장 카운터는 `PetGachaCounter` Value Object (Domain) | Immutable, Soft Pity 로직 캡슐화 (추후 구현 고려) | Deferred |
| D4 | 펫 장착 로직은 `PetService` (Application Layer) | 슬롯 검증, Repository 의존성 (Infrastructure) | Accepted |
| D5 | EF Core Include + AsNoTracking (읽기 API) | N+1 방지, 메모리 절약 (30-40%), 쿼리 10-20% 향상 | Accepted |
| D6 | Pet PK는 INT (AUTO_INCREMENT) | 종속 Entity, Equipment와 일관성 | Accepted |
| D7 | 읽기 API Public, 쓰기 API 인증 분리 | 리더보드/PvP 대비, 게임 데이터는 공개 (DungeonController 패턴) | Accepted |

**가이드**:
- **간단한 결정**: 테이블에 1줄로 기록
- **복잡한 결정 (ADR 작성 필요 조건)**: 외부 서비스 도입, 스키마 마이그레이션, 인프라 변경
- **Pet System**: Requirements 대화에서 모든 결정 완료 (Spike/ADR 불필요)

---

## 📱 Unity Client Integration

### Unity Documentation 필요 항목

**작성 위치**: `docs/unity/pet-system/`

**필수 문서**:
1. **API_SPEC.md**:
   - 8개 엔드포인트 명세 (POST /api/pets/gacha, GET /api/pets, ...)
   - Request/Response 예시 (JSON)
   - 에러 코드 및 메시지

2. **DTOs.cs**:
   - `PetGachaRequestDto`, `PetGachaResponseDto`, `PetDto`, `PetEquipRequestDto`, `PetLevelUpRequestDto`
   - Newtonsoft.Json 속성 (`[JsonProperty]`)

3. **INTEGRATION_GUIDE.md**:
   - Unity 코드 예시 (UnityWebRequest 사용)
   - 펫 UI 연동 (가챠 버튼, 장착 슬롯, 인벤토리)
   - 버프 계산 클라이언트 표시 (서버 검증 필수)

**참고**: `docs/unity/UNITY_DOCUMENTATION_GUIDE.md`

---

## ✅ Approval

- [x] Design 리뷰 완료
- [x] 모든 Requirements 항목 커버 확인 (US-1, US-2, US-3)
- [x] TODO(human) 아키텍처 학습 포인트 확인 완료 (5개)
  - [x] 가챠 로직 계층 배치 → **Domain Service** (순수 비즈니스 로직, 재사용)
  - [x] Random 생성 추상화 → **IRandomProvider (DI)** (테스트 용이성)
  - [x] 버프 계산 시점 → **실시간 계산** (데이터 일관성 우선)
  - [x] 트랜잭션 경계 → **Service Layer (Unit of Work)** (비즈니스 로직 원자성)
  - [x] 인덱스 전략 → **단일 컬럼 (character_id)** (99% 사용 케이스)
- [x] Self-Review Checklist 10개 항목 통과 (10/10)
- [x] Spec Review 통과 (95/100 점, Excellent 등급)
- [x] Tasks 단계로 진행 승인

**Approved by**: Development Team (User + AI)
**Date**: 2025-10-27

---

**작성일**: 2025-10-27
**작성자**: AI (Claude) - Learning Project Mode
**상태**: **Approved** ✅
**Requirements 추적성**: [requirements.md](./requirements.md) - US-1 (가챠), US-2 (레벨업), US-3 (장착)
