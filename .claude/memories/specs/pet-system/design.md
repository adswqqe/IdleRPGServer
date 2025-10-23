# Design: Pet System

> 이 문서는 펫 시스템 기능의 기술 설계를 정의합니다.
>
> **작성 가이드**:
> - Requirements의 모든 항목이 어떻게 구현될지 설명
> - Clean Architecture 계층별로 구성요소 정의
> - 데이터 모델, API 설계, 에러 처리 포함

---

## 📐 Architecture Overview

### Layer Responsibilities

#### API Layer
- **Controllers**: `PetController` (신규 생성)
- **Endpoints**:
  - `POST /api/pets/gacha` - 펫 가챠 실행 (1회/10회)
  - `GET /api/pets` - 보유 펫 목록 조회
  - `GET /api/pets/{id}` - 펫 상세 정보 조회
  - `PUT /api/pets/{id}/equip` - 펫 장착/해제
  - `PUT /api/pets/{id}/levelup` - 펫 레벨업
- **DTOs**: `PetGachaRequestDto`, `PetGachaResponseDto`, `PetDto`, `PetDetailDto`

**Requirements**: [US-1, US-2, US-3, US-4]

#### Application Layer
- **Services**: `IPetService`, `PetService` (신규 생성)
- **Business Logic**:
  - 펫 가챠 로직 (크리스탈 차감, 확률 계산, 펫 지급)
  - 펫 장착/해제 로직 (기존 펫 교체, 버프 적용)
  - 펫 레벨업 로직 (골드 차감, 경험치 증가, 레벨업 처리)
  - 펫 조회 로직 (목록, 상세)
- **Validation**:
  - 크리스탈/골드 잔액 검증
  - 펫 소유권 검증 (자신의 캐릭터 펫만 조작)
  - 최대 레벨 검증

**Requirements**: [US-1, US-2, US-3, US-4]

#### Domain Layer
- **Entities**:
  - `PetTemplate` (신규): 펫 마스터 데이터 (Id, Name, Rarity, BaseAttack, BaseDefense)
  - `CharacterPet` (신규): 펫 인스턴스 (Id, PetTemplateId, CharacterId, Level, Experience)
  - `Character` (수정): `EquippedPetId` 필드 추가 (Nullable FK)
- **Value Objects**: 없음
- **Domain Services**: `GachaLogicService` (기존 재사용, 펫 가챠 메서드 추가)
- **Enums**: `PetRarity` (신규: Common, Rare, Epic, Legendary)

**Requirements**: [US-1, US-2, US-3]

#### Infrastructure Layer
- **Repositories**:
  - `IPetTemplateRepository`, `PetTemplateRepository` (신규)
  - `ICharacterPetRepository`, `CharacterPetRepository` (신규)
  - `ICharacterRepository` (기존 수정: EquippedPetId 포함)
- **Data Access**: EF Core Configuration (`PetTemplateConfiguration`, `CharacterPetConfiguration`)
- **External Services**: 없음
- **Seeders**: `PetTemplateSeeder` (신규) - 초기 펫 템플릿 데이터

**Requirements**: [US-1, US-2, US-3, US-4]

---

## 🗄️ Data Model

### Database Schema

#### 신규 테이블: `PetTemplates`
```sql
CREATE TABLE IF NOT EXISTS "PetTemplates" (
    "Id" serial PRIMARY KEY,
    "Name" varchar(100) NOT NULL,
    "Rarity" integer NOT NULL, -- 0: Common, 1: Rare, 2: Epic, 3: Legendary
    "BaseAttack" integer NOT NULL,
    "BaseDefense" integer NOT NULL,
    "Description" text,
    "CreatedAt" timestamp NOT NULL DEFAULT now(),
    "UpdatedAt" timestamp NOT NULL DEFAULT now()
);

-- 인덱스: 희귀도별 조회 최적화
CREATE INDEX IF NOT EXISTS "IX_PetTemplates_Rarity" ON "PetTemplates" ("Rarity");
```

#### 신규 테이블: `CharacterPets`
```sql
CREATE TABLE IF NOT EXISTS "CharacterPets" (
    "Id" uuid PRIMARY KEY,
    "PetTemplateId" integer NOT NULL,
    "CharacterId" uuid NOT NULL,
    "Level" integer NOT NULL DEFAULT 1 CHECK ("Level" >= 1 AND "Level" <= 50),
    "Experience" integer NOT NULL DEFAULT 0 CHECK ("Experience" >= 0),
    "CreatedAt" timestamp NOT NULL DEFAULT now(),
    "UpdatedAt" timestamp NOT NULL DEFAULT now(),

    CONSTRAINT "FK_CharacterPets_PetTemplates"
        FOREIGN KEY ("PetTemplateId") REFERENCES "PetTemplates" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_CharacterPets_Characters"
        FOREIGN KEY ("CharacterId") REFERENCES "Characters" ("Id") ON DELETE CASCADE
);

-- 인덱스: 캐릭터별 펫 조회 최적화
CREATE INDEX IF NOT EXISTS "IX_CharacterPets_CharacterId" ON "CharacterPets" ("CharacterId");
-- 인덱스: 템플릿별 펫 조회 최적화 (통계용)
CREATE INDEX IF NOT EXISTS "IX_CharacterPets_PetTemplateId" ON "CharacterPets" ("PetTemplateId");
```

#### 수정 테이블: `Characters`
```sql
-- 장착된 펫 FK 추가
ALTER TABLE "Characters" ADD COLUMN IF NOT EXISTS "EquippedPetId" uuid;

-- FK 제약조건 추가
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_name = 'FK_Characters_CharacterPets'
    ) THEN
        ALTER TABLE "Characters"
        ADD CONSTRAINT "FK_Characters_CharacterPets"
        FOREIGN KEY ("EquippedPetId") REFERENCES "CharacterPets" ("Id") ON DELETE SET NULL;
    END IF;
END $$;

-- 인덱스: 장착된 펫 조회 최적화
CREATE INDEX IF NOT EXISTS "IX_Characters_EquippedPetId" ON "Characters" ("EquippedPetId");
```

### Entity Relationships
```
PetTemplate (1) ──< (N) CharacterPet (N) >── (1) Character
                                               |
                                               └─ EquippedPetId (0..1)
```

**설명**:
- `PetTemplate` 1:N `CharacterPet`: 한 템플릿으로 여러 펫 인스턴스 생성
- `Character` 1:N `CharacterPet`: 한 캐릭터가 여러 펫 소유
- `Character` 0..1 `CharacterPet`: 한 캐릭터가 최대 1개 펫 장착 (EquippedPetId FK)

**Requirements**: [US-1, US-2, US-4]

### EF Core Configuration

#### `PetTemplateConfiguration`
```csharp
public class PetTemplateConfiguration : IEntityTypeConfiguration<PetTemplate>
{
    public void Configure(EntityTypeBuilder<PetTemplate> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Rarity).IsRequired();
        builder.Property(p => p.BaseAttack).IsRequired();
        builder.Property(p => p.BaseDefense).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(500);

        builder.HasIndex(p => p.Rarity);
    }
}
```

#### `CharacterPetConfiguration`
```csharp
public class CharacterPetConfiguration : IEntityTypeConfiguration<CharacterPet>
{
    public void Configure(EntityTypeBuilder<CharacterPet> builder)
    {
        builder.HasKey(p => p.Id);

        builder.HasOne(p => p.PetTemplate)
            .WithMany()
            .HasForeignKey(p => p.PetTemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Character)
            .WithMany(c => c.CharacterPets)
            .HasForeignKey(p => p.CharacterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.CharacterId);
        builder.HasIndex(p => p.PetTemplateId);
    }
}
```

#### `CharacterConfiguration` (수정)
```csharp
// Character.EquippedPetId 설정 추가
builder.HasOne<CharacterPet>()
    .WithMany()
    .HasForeignKey(c => c.EquippedPetId)
    .OnDelete(DeleteBehavior.SetNull);

builder.HasIndex(c => c.EquippedPetId);
```

---

## 🔌 API Design

### Endpoints

#### `POST /api/pets/gacha`
**목적**: 펫 가챠 실행 (1회 또는 10회)

**Request:**
```json
{
  "characterId": "uuid",
  "count": 1  // 1 또는 10
}
```

**Validation Rules**:
- `characterId`: Required, 유효한 GUID
- `count`: Required, 1 또는 10만 허용

**Response (200 OK):**
```json
{
  "pets": [
    {
      "petId": "uuid",
      "petTemplateId": 1,
      "name": "파이어 독",
      "rarity": "Legendary",
      "level": 1,
      "attackBuff": 40,
      "defenseBuff": 24
    }
  ],
  "remainingCrystal": 900,
  "cost": 100
}
```

**Errors:**
- `400 Bad Request`: 크리스탈 부족 (메시지: "크리스탈이 부족합니다. 필요: {cost}, 보유: {current}")
- `400 Bad Request`: 잘못된 count 값 (메시지: "count는 1 또는 10만 가능합니다.")
- `401 Unauthorized`: JWT 인증 실패
- `404 Not Found`: 캐릭터 미존재 (메시지: "캐릭터를 찾을 수 없습니다.")
- `500 Internal Server Error`: 서버 오류

**Requirements**: [US-1]

---

#### `GET /api/pets?characterId={characterId}`
**목적**: 캐릭터가 보유한 펫 목록 조회

**Query Parameters:**
- `characterId` (required): 캐릭터 ID

**Response (200 OK):**
```json
{
  "pets": [
    {
      "petId": "uuid",
      "petTemplateId": 1,
      "name": "파이어 독",
      "rarity": "Epic",
      "level": 5,
      "experience": 3000,
      "attackBuff": 60,
      "defenseBuff": 36,
      "isEquipped": true
    },
    {
      "petId": "uuid-2",
      "petTemplateId": 2,
      "name": "아이스 캣",
      "rarity": "Rare",
      "level": 1,
      "experience": 0,
      "attackBuff": 10,
      "defenseBuff": 6,
      "isEquipped": false
    }
  ],
  "totalCount": 2
}
```

**Errors:**
- `401 Unauthorized`: JWT 인증 실패
- `404 Not Found`: 캐릭터 미존재

**Requirements**: [US-4]

---

#### `GET /api/pets/{id}`
**목적**: 펫 상세 정보 조회 (다음 레벨 요구 경험치 포함)

**Response (200 OK):**
```json
{
  "petId": "uuid",
  "petTemplateId": 1,
  "name": "파이어 독",
  "rarity": "Epic",
  "level": 5,
  "experience": 3000,
  "nextLevelExperience": 5000,
  "attackBuff": 60,
  "defenseBuff": 36,
  "isEquipped": true,
  "description": "불을 다루는 강력한 펫",
  "characterId": "uuid"
}
```

**Errors:**
- `401 Unauthorized`: JWT 인증 실패
- `403 Forbidden`: 다른 캐릭터의 펫 조회 시도
- `404 Not Found`: 펫 미존재

**Requirements**: [US-4]

---

#### `PUT /api/pets/{id}/equip`
**목적**: 펫 장착/해제

**Request:**
```json
{
  "equip": true  // true: 장착, false: 해제
}
```

**Response (200 OK):**
```json
{
  "petId": "uuid",
  "name": "파이어 독",
  "isEquipped": true,
  "attackBuff": 60,
  "defenseBuff": 36,
  "message": "펫이 장착되었습니다."
}
```

**Errors:**
- `401 Unauthorized`: JWT 인증 실패
- `403 Forbidden`: 다른 캐릭터의 펫 조작 시도
- `404 Not Found`: 펫 미존재

**Business Rules**:
- 이미 다른 펫이 장착된 경우 → 기존 펫 자동 해제 후 새 펫 장착
- 해제 요청 시 → EquippedPetId를 NULL로 설정

**Requirements**: [US-2]

---

#### `PUT /api/pets/{id}/levelup`
**목적**: 펫 레벨업 (골드 소모)

**Request:**
```json
{
  "experienceToAdd": 1000  // 추가할 경험치 (골드 1,000 소모)
}
```

**Validation Rules**:
- `experienceToAdd`: Required, 양수 (> 0), 캐릭터 보유 골드 이하

**Response (200 OK):**
```json
{
  "petId": "uuid",
  "name": "파이어 독",
  "previousLevel": 5,
  "currentLevel": 6,
  "experience": 500,
  "nextLevelExperience": 6000,
  "attackBuff": 68,
  "defenseBuff": 40,
  "remainingGold": 49000
}
```

**Errors:**
- `400 Bad Request`: 골드 부족
- `400 Bad Request`: 최대 레벨 도달 (메시지: "펫이 최대 레벨(50)에 도달했습니다.")
- `401 Unauthorized`: JWT 인증 실패
- `403 Forbidden`: 다른 캐릭터의 펫 조작 시도
- `404 Not Found`: 펫 미존재

**Requirements**: [US-3]

---

## 🧮 Business Logic

### 핵심 알고리즘

#### 1. 펫 가챠 확률 계산

**입력:**
- `count`: 가챠 횟수 (1 또는 10)

**출력:**
- `List<CharacterPet>`: 획득한 펫 목록

**프로세스 (AI 제안)**:
1. **크리스탈 차감**:
   - 1회 가챠: 100 크리스탈
   - 10회 가챠: 900 크리스탈 (10% 할인)
   - `character.Crystal -= cost`
2. **각 가챠마다 확률 계산** (GachaLogicService 재사용):
   - `GachaLogicService.DeterminePetRarity()` 호출
   - 확률: Legendary 1%, Epic 9%, Rare 30%, Common 60%
3. **펫 선택**:
   - `GachaLogicService.SelectRandomPet(rarity, availablePets)` 호출
   - 해당 등급의 PetTemplate 중 무작위 선택
4. **펫 인스턴스 생성**:
   - `CharacterPet` 생성 (Level=1, Experience=0)
   - `CharacterPets` 테이블에 INSERT
5. **응답 반환**: 획득한 펫 목록 + 남은 크리스탈

**예외 처리:**
- 크리스탈 부족 → `InvalidOperationException` (400)
- PetTemplate 미존재 → `InvalidOperationException` (500)

**Requirements**: [US-1]

---

#### 2. 펫 스탯 버프 계산 (Entity 메서드)

**CharacterPet.CalculateAttackBuff()** (Entity 메서드):

```csharp
public int CalculateAttackBuff()
{
    var baseAttack = PetTemplate.BaseAttack;
    var attackPerLevel = PetTemplate.Rarity switch
    {
        PetRarity.Common => 2,
        PetRarity.Rare => 4,
        PetRarity.Epic => 8,
        PetRarity.Legendary => 16,
        _ => 0
    };

    return baseAttack + (Level - 1) * attackPerLevel;
}
```

**CharacterPet.CalculateDefenseBuff()** (Entity 메서드):

```csharp
public int CalculateDefenseBuff()
{
    var baseDefense = PetTemplate.BaseDefense;
    var defensePerLevel = PetTemplate.Rarity switch
    {
        PetRarity.Common => 1,
        PetRarity.Rare => 2,
        PetRarity.Epic => 4,
        PetRarity.Legendary => 8,
        _ => 0
    };

    return baseDefense + (Level - 1) * defensePerLevel;
}
```

**공식 (AI 제안)**:
```
Common:    AttackBuff = 5 + (Level - 1) * 2,  DefenseBuff = 3 + (Level - 1) * 1
Rare:      AttackBuff = 10 + (Level - 1) * 4, DefenseBuff = 6 + (Level - 1) * 2
Epic:      AttackBuff = 20 + (Level - 1) * 8, DefenseBuff = 12 + (Level - 1) * 4
Legendary: AttackBuff = 40 + (Level - 1) * 16, DefenseBuff = 24 + (Level - 1) * 8
```

**예시**:
- Legendary 펫, Level 10: Attack = 40 + 9 * 16 = 184, Defense = 24 + 9 * 8 = 96

**🎓 학습 포인트 (아키텍처 결정)**:
- ✅ **결정 완료** (Requirements에서 결정): Entity 메서드 사용
- **이유**: 자신의 속성(Level, Rarity)만으로 계산 가능 → 높은 응집도
- **장점**: 외부 의존성 없음, 테스트 용이, 객체지향 원칙 준수

**Requirements**: [US-2, US-3]

---

#### 3. 펫 레벨업 경험치 계산

**입력:**
- `experienceToAdd`: 추가할 경험치 (골드 소모량과 동일)

**출력:**
- `CharacterPet`: 레벨업된 펫 정보

**프로세스 (AI 제안)**:
1. **골드 차감**:
   - `character.Gold -= experienceToAdd`
   - 부족 시 예외 발생
2. **경험치 추가**:
   - `pet.Experience += experienceToAdd`
3. **레벨업 체크**:
   - `GetRequiredExperience(pet.Level)` 계산
   - 공식 (AI 제안): `RequiredExp = Level * 1000`
     - Level 1→2: 1,000 exp
     - Level 2→3: 2,000 exp
     - Level 49→50: 49,000 exp
   - `pet.Experience >= required` 시 레벨업:
     - `pet.Level++`
     - `pet.Experience -= required`
     - 최대 레벨(50) 도달 시 중단
4. **스탯 버프 재계산**: `CalculateAttackBuff()`, `CalculateDefenseBuff()` 호출
5. **응답 반환**: 새 레벨, 남은 경험치, 스탯 버프

**예외 처리:**
- 골드 부족 → `InvalidOperationException` (400)
- 최대 레벨 도달 → `InvalidOperationException` (400)

**Requirements**: [US-3]

---

#### 4. 펫 장착/해제 로직

**입력:**
- `petId`: 장착할 펫 ID
- `equip`: true (장착) / false (해제)

**출력:**
- `PetDto`: 장착된 펫 정보

**프로세스 (AI 제안)**:
1. **장착 요청 (equip=true)**:
   - 기존 장착 펫 확인: `character.EquippedPetId`
   - 기존 펫이 있으면 자동 해제 (EquippedPetId를 NULL로)
   - 새 펫 장착: `character.EquippedPetId = petId`
2. **해제 요청 (equip=false)**:
   - `character.EquippedPetId = null`
3. **DB 저장**: `_characterRepository.UpdateAsync(character)`
4. **전투 시스템 연동** (향후):
   - 전투 시작 시 `character.EquippedPetId`로 펫 조회
   - 펫 버프를 캐릭터 스탯에 추가

**예외 처리:**
- 펫 소유권 검증: `pet.CharacterId != character.Id` → `ForbiddenException` (403)

**Requirements**: [US-2]

---

## 🎯 Service Layer Design

### `IPetService` / `PetService`

**책임**: 펫 관련 모든 비즈니스 로직 처리

**메서드:**

#### `PerformGachaAsync(characterId, count)`
```csharp
/// <summary>
/// 펫 가챠를 수행합니다.
/// </summary>
/// <param name="characterId">가챠를 수행하는 캐릭터 ID</param>
/// <param name="count">가챠 횟수 (1 또는 10)</param>
/// <returns>획득한 펫 목록</returns>
/// <exception cref="InvalidOperationException">크리스탈 부족 또는 잘못된 count</exception>
public async Task<PetGachaResponseDto> PerformGachaAsync(
    Guid characterId,
    int count,
    CancellationToken cancellationToken = default)
{
    // 1. 캐릭터 조회
    var character = await _characterRepository.GetAsync(characterId, cancellationToken);
    if (character == null) throw new NotFoundException("캐릭터를 찾을 수 없습니다.");

    // 2. 비용 계산
    int cost = count == 1 ? 100 : (count == 10 ? 900 : throw new InvalidOperationException("count는 1 또는 10만 가능"));

    // 3. 크리스탈 검증
    if (character.Crystal < cost)
        throw new InvalidOperationException($"크리스탈이 부족합니다. 필요: {cost}, 보유: {character.Crystal}");

    // 4. 크리스탈 차감
    character.Crystal -= cost;

    // 5. 가챠 수행 (count번 반복)
    var availablePets = await _petTemplateRepository.GetAllAsync(cancellationToken);
    var obtainedPets = new List<CharacterPet>();

    for (int i = 0; i < count; i++)
    {
        var rarity = _gachaLogicService.DeterminePetRarity();
        var petTemplate = _gachaLogicService.SelectRandomPet(rarity, availablePets);

        var characterPet = new CharacterPet
        {
            Id = Guid.NewGuid(),
            PetTemplateId = petTemplate.Id,
            CharacterId = characterId,
            Level = 1,
            Experience = 0
        };

        obtainedPets.Add(characterPet);
        await _characterPetRepository.AddAsync(characterPet, cancellationToken);
    }

    // 6. 트랜잭션 커밋 (Repository SaveChanges)
    await _characterRepository.UpdateAsync(character, cancellationToken);

    // 7. 응답 생성
    return new PetGachaResponseDto
    {
        Pets = obtainedPets.Select(p => MapToPetDto(p)).ToList(),
        RemainingCrystal = character.Crystal,
        Cost = cost
    };
}
```

**Dependencies:**
- `ICharacterRepository`: 캐릭터 조회/수정
- `ICharacterPetRepository`: 펫 추가
- `IPetTemplateRepository`: 펫 템플릿 조회
- `GachaLogicService`: 확률 계산

---

#### `GetCharacterPetsAsync(characterId)`
```csharp
/// <summary>
/// 캐릭터가 보유한 펫 목록을 조회합니다.
/// </summary>
public async Task<List<PetDto>> GetCharacterPetsAsync(
    Guid characterId,
    CancellationToken cancellationToken = default)
{
    var character = await _characterRepository.GetAsync(characterId, cancellationToken);
    if (character == null) throw new NotFoundException("캐릭터를 찾을 수 없습니다.");

    var pets = await _characterPetRepository.GetByCharacterIdAsync(characterId, cancellationToken);

    return pets.Select(p => MapToPetDto(p, character.EquippedPetId == p.Id)).ToList();
}
```

---

#### `EquipPetAsync(petId, characterId, equip)`
```csharp
/// <summary>
/// 펫을 장착하거나 해제합니다.
/// </summary>
public async Task<PetDto> EquipPetAsync(
    Guid petId,
    Guid characterId,
    bool equip,
    CancellationToken cancellationToken = default)
{
    var character = await _characterRepository.GetAsync(characterId, cancellationToken);
    if (character == null) throw new NotFoundException("캐릭터를 찾을 수 없습니다.");

    var pet = await _characterPetRepository.GetAsync(petId, cancellationToken);
    if (pet == null) throw new NotFoundException("펫을 찾을 수 없습니다.");
    if (pet.CharacterId != characterId) throw new ForbiddenException("다른 캐릭터의 펫입니다.");

    if (equip)
    {
        // 장착: 기존 펫 자동 해제
        character.EquippedPetId = petId;
    }
    else
    {
        // 해제
        if (character.EquippedPetId == petId)
            character.EquippedPetId = null;
    }

    await _characterRepository.UpdateAsync(character, cancellationToken);

    return MapToPetDto(pet, equip);
}
```

---

#### `LevelUpPetAsync(petId, characterId, experienceToAdd)`
```csharp
/// <summary>
/// 펫을 레벨업합니다 (골드 소모).
/// </summary>
public async Task<PetDetailDto> LevelUpPetAsync(
    Guid petId,
    Guid characterId,
    int experienceToAdd,
    CancellationToken cancellationToken = default)
{
    var character = await _characterRepository.GetAsync(characterId, cancellationToken);
    if (character == null) throw new NotFoundException("캐릭터를 찾을 수 없습니다.");

    var pet = await _characterPetRepository.GetAsync(petId, cancellationToken);
    if (pet == null) throw new NotFoundException("펫을 찾을 수 없습니다.");
    if (pet.CharacterId != characterId) throw new ForbiddenException("다른 캐릭터의 펫입니다.");

    // 골드 검증
    if (character.Gold < experienceToAdd)
        throw new InvalidOperationException($"골드가 부족합니다. 필요: {experienceToAdd}, 보유: {character.Gold}");

    // 최대 레벨 검증
    if (pet.Level >= 50)
        throw new InvalidOperationException("펫이 최대 레벨(50)에 도달했습니다.");

    // 골드 차감
    character.Gold -= experienceToAdd;

    // 경험치 추가
    int previousLevel = pet.Level;
    pet.Experience += experienceToAdd;

    // 레벨업 체크
    while (pet.Level < 50)
    {
        int required = GetRequiredExperience(pet.Level);
        if (pet.Experience >= required)
        {
            pet.Level++;
            pet.Experience -= required;
        }
        else break;
    }

    // 트랜잭션 커밋
    await _characterRepository.UpdateAsync(character, cancellationToken);
    await _characterPetRepository.UpdateAsync(pet, cancellationToken);

    return MapToPetDetailDto(pet, previousLevel, character.Gold);
}

private int GetRequiredExperience(int level)
{
    return level * 1000;
}
```

---

## 🧪 Testing Strategy

### Unit Tests

#### `GachaLogicService` (펫 가챠 확률)
- `DeterminePetRarity_RandomValue0_ReturnsLegendary`: rand=0 → Legendary
- `DeterminePetRarity_RandomValue5_ReturnsEpic`: rand=5 → Epic
- `DeterminePetRarity_RandomValue20_ReturnsRare`: rand=20 → Rare
- `DeterminePetRarity_RandomValue50_ReturnsCommon`: rand=50 → Common
- `SelectRandomPet_ValidRarity_ReturnsRandomPet`: 해당 등급 펫 무작위 선택

**Mock**: `IRandomProvider`

---

#### `CharacterPet` Entity (스탯 버프 계산)
- `CalculateAttackBuff_CommonLevel1_Returns5`: Common Lv1 → Attack +5
- `CalculateAttackBuff_LegendaryLevel10_Returns184`: Legendary Lv10 → Attack +184
- `CalculateDefenseBuff_EpicLevel5_Returns28`: Epic Lv5 → Defense +28

---

#### `PetService` (비즈니스 로직)
- `PerformGachaAsync_SufficientCrystal_ReturnsOnePet`: 크리스탈 충분 시 펫 지급
- `PerformGachaAsync_InsufficientCrystal_ThrowsException`: 크리스탈 부족 시 예외
- `PerformGachaAsync_Count10_Returns10Pets`: 10회 가챠 시 10개 펫 지급
- `EquipPetAsync_ValidPet_UpdatesCharacterEquippedPetId`: 장착 시 EquippedPetId 업데이트
- `EquipPetAsync_OtherCharacterPet_ThrowsForbiddenException`: 타인 펫 장착 시 예외
- `LevelUpPetAsync_SufficientGold_IncreasesLevel`: 골드 충분 시 레벨업
- `LevelUpPetAsync_MaxLevel_ThrowsException`: 최대 레벨 도달 시 예외

**Mock**: `ICharacterRepository`, `ICharacterPetRepository`, `IPetTemplateRepository`, `GachaLogicService`

---

### Integration Tests

#### API Endpoints
- `POST /api/pets/gacha`: 가챠 수행 후 DB에 펫 저장 확인
- `GET /api/pets?characterId={id}`: 펫 목록 조회
- `PUT /api/pets/{id}/equip`: 펫 장착 후 EquippedPetId 확인
- `PUT /api/pets/{id}/levelup`: 펫 레벨업 후 골드/경험치 확인

**Database**: TestContainers PostgreSQL

---

### Test Coverage Goals
- **Domain Services (GachaLogicService)**: 90%+
- **Application Services (PetService)**: 80%+
- **Controllers (PetController)**: 70%+
- **Entity Methods (CharacterPet)**: 100% (계산 로직은 필수 테스트)

---

## ⚠️ Error Handling

### Exception Types
- `NotFoundException`: 캐릭터/펫 미존재 시 (404)
- `InvalidOperationException`: 크리스탈/골드 부족, 잘못된 입력 (400)
- `ForbiddenException`: 타인 펫 조작 시도 (403)

### Error Response Format
```json
{
  "message": "크리스탈이 부족합니다. 필요: 100, 보유: 50",
  "statusCode": 400
}
```

### 주요 에러 시나리오
1. **크리스탈 부족** (400):
   - `if (character.Crystal < cost) throw new InvalidOperationException(...)`
2. **골드 부족** (400):
   - `if (character.Gold < experienceToAdd) throw new InvalidOperationException(...)`
3. **최대 레벨 도달** (400):
   - `if (pet.Level >= 50) throw new InvalidOperationException(...)`
4. **타인 펫 조작** (403):
   - `if (pet.CharacterId != character.Id) throw new ForbiddenException(...)`
5. **펫 미존재** (404):
   - `if (pet == null) throw new NotFoundException(...)`

---

## 🔐 Security Considerations

### Authentication
- **모든 API**: `[Authorize]` 속성 필수 (JWT Bearer Token)
- `ClaimsPrincipal`에서 `UserId` 추출 → 캐릭터 소유권 검증

### Authorization
- **리소스 소유권 검증**:
  - 펫 조작 시: `pet.CharacterId == character.Id` 확인
  - 다른 캐릭터의 펫 조작 시도 → 403 Forbidden

### Data Validation
- **Request DTO 검증** (FluentValidation):
  - `count`: 1 또는 10만 허용
  - `experienceToAdd`: 양수, 캐릭터 골드 이하
  - `characterId`: 유효한 GUID

### SQL Injection Prevention
- **EF Core 파라미터화**: 모든 쿼리는 EF Core LINQ 사용 (Raw SQL 금지)

---

## 📊 Performance Considerations

### Database Indexes
- `IX_PetTemplates_Rarity`: 희귀도별 펫 조회 최적화 (가챠 시 사용)
- `IX_CharacterPets_CharacterId`: 캐릭터별 펫 목록 조회 최적화
- `IX_CharacterPets_PetTemplateId`: 펫 템플릿별 통계 조회 (선택적)
- `IX_Characters_EquippedPetId`: 장착 펫 조회 최적화

### Logging
- **주요 비즈니스 이벤트 로깅** (Info 레벨):
  - 펫 가챠 성공: `CharacterId`, `PetId`, `Rarity`, `CrystalCost`
  - 펫 장착/해제: `CharacterId`, `PetId`, `Action` (Equip/Unequip)
  - 펫 레벨업: `CharacterId`, `PetId`, `PreviousLevel`, `NewLevel`, `GoldCost`
- **실패 시나리오 로깅** (Warning 레벨):
  - 크리스탈/골드 부족: `CharacterId`, `Required`, `Available`
  - 권한 없음: `UserId`, `CharacterId`, `PetId`
- **민감 정보 제외**: 사용자 이메일, 토큰 로깅 금지

### Caching
- **PetTemplate 캐싱** (향후):
  - 펫 마스터 데이터는 변경 빈도 낮음
  - Redis/In-Memory Cache로 조회 성능 향상
  - 캐시 키: `PetTemplates:All`

### Pagination
- **펫 목록 조회** (향후):
  - 플레이어당 평균 50개 펫 보유 가정
  - 100개 이상 시 페이징 도입 (`Skip`, `Take`)

### N+1 Query Prevention
- **Eager Loading**:
  ```csharp
  await _context.CharacterPets
      .Include(p => p.PetTemplate) // N+1 방지
      .Where(p => p.CharacterId == characterId)
      .ToListAsync();
  ```

---

## 🔄 Migration Plan

### Database Migration (`migration.sql`)

```sql
-- ============================================================
-- Pet System Tables (IdleRPG v1.8)
-- ============================================================

-- 1. PetTemplates 테이블 생성
DO $EF$ BEGIN
    IF NOT EXISTS(SELECT 1 FROM information_schema.tables
                  WHERE table_name = 'PetTemplates') THEN
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

        RAISE NOTICE 'PetTemplates 테이블 생성 완료';
    END IF;
END $EF$;

-- 2. CharacterPets 테이블 생성
DO $EF$ BEGIN
    IF NOT EXISTS(SELECT 1 FROM information_schema.tables
                  WHERE table_name = 'CharacterPets') THEN
        CREATE TABLE "CharacterPets" (
            "Id" uuid PRIMARY KEY,
            "PetTemplateId" integer NOT NULL,
            "CharacterId" uuid NOT NULL,
            "Level" integer NOT NULL DEFAULT 1 CHECK ("Level" >= 1 AND "Level" <= 50),
            "Experience" integer NOT NULL DEFAULT 0 CHECK ("Experience" >= 0),
            "CreatedAt" timestamp NOT NULL DEFAULT now(),
            "UpdatedAt" timestamp NOT NULL DEFAULT now(),

            CONSTRAINT "FK_CharacterPets_PetTemplates"
                FOREIGN KEY ("PetTemplateId") REFERENCES "PetTemplates" ("Id") ON DELETE RESTRICT,
            CONSTRAINT "FK_CharacterPets_Characters"
                FOREIGN KEY ("CharacterId") REFERENCES "Characters" ("Id") ON DELETE CASCADE
        );

        CREATE INDEX "IX_CharacterPets_CharacterId" ON "CharacterPets" ("CharacterId");
        CREATE INDEX "IX_CharacterPets_PetTemplateId" ON "CharacterPets" ("PetTemplateId");

        RAISE NOTICE 'CharacterPets 테이블 생성 완료';
    END IF;
END $EF$;

-- 3. Characters 테이블에 EquippedPetId 컬럼 추가
DO $EF$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'Characters' AND column_name = 'EquippedPetId'
    ) THEN
        ALTER TABLE "Characters" ADD COLUMN "EquippedPetId" uuid;

        -- FK 제약조건 추가
        ALTER TABLE "Characters"
        ADD CONSTRAINT "FK_Characters_CharacterPets"
        FOREIGN KEY ("EquippedPetId") REFERENCES "CharacterPets" ("Id") ON DELETE SET NULL;

        CREATE INDEX "IX_Characters_EquippedPetId" ON "Characters" ("EquippedPetId");

        RAISE NOTICE 'Characters 테이블에 EquippedPetId 컬럼 추가 완료';
    END IF;
END $EF$;
```

---

### Data Seeding (`PetTemplateSeeder`)

```csharp
public class PetTemplateSeeder
{
    public static async Task SeedAsync(GameDBContext context)
    {
        if (await context.PetTemplates.AnyAsync()) return;

        var petTemplates = new List<PetTemplate>
        {
            // Common Pets
            new PetTemplate { Id = 1, Name = "슬라임", Rarity = PetRarity.Common, BaseAttack = 5, BaseDefense = 3, Description = "귀여운 슬라임" },
            new PetTemplate { Id = 2, Name = "고블린", Rarity = PetRarity.Common, BaseAttack = 5, BaseDefense = 3, Description = "작은 고블린" },

            // Rare Pets
            new PetTemplate { Id = 3, Name = "아이스 캣", Rarity = PetRarity.Rare, BaseAttack = 10, BaseDefense = 6, Description = "얼음 고양이" },
            new PetTemplate { Id = 4, Name = "썬더 울프", Rarity = PetRarity.Rare, BaseAttack = 10, BaseDefense = 6, Description = "번개 늑대" },

            // Epic Pets
            new PetTemplate { Id = 5, Name = "파이어 독", Rarity = PetRarity.Epic, BaseAttack = 20, BaseDefense = 12, Description = "불을 다루는 개" },
            new PetTemplate { Id = 6, Name = "다크 팬서", Rarity = PetRarity.Epic, BaseAttack = 20, BaseDefense = 12, Description = "어둠의 표범" },

            // Legendary Pets
            new PetTemplate { Id = 7, Name = "드래곤 해츨링", Rarity = PetRarity.Legendary, BaseAttack = 40, BaseDefense = 24, Description = "어린 드래곤" },
            new PetTemplate { Id = 8, Name = "페닉스", Rarity = PetRarity.Legendary, BaseAttack = 40, BaseDefense = 24, Description = "불사조" }
        };

        await context.PetTemplates.AddRangeAsync(petTemplates);
        await context.SaveChangesAsync();
    }
}
```

**PetRarity Enum 정의**:
```csharp
namespace IdleRPG.Domain.Enums
{
    // Probabilities: Common(60%), Rare(30%), Epic(9%), Legendary(1%)
    public enum PetRarity
    {
        Common = 0,
        Rare = 1,
        Epic = 2,
        Legendary = 3
    }
}
```

---

## 📝 Decision Log

> ⚠️ **중요**: L 사이즈 기능은 이 섹션 **필수**. 중요한 아키텍처 결정을 추적.

| ID | Decision | Rationale | Status |
|----|----------|-----------|--------|
| D1 | PetTemplate/CharacterPet 분리 (Template-Instance 패턴) | 마스터 데이터 중복 방지, 밸런스 패치 용이, SkillTemplate과 일관성 | Accepted |
| D2 | Character-Pet 관계는 1:N | 한 캐릭터가 여러 펫 소유, 펫은 한 캐릭터에만 속함 (일반적인 게임 패턴) | Accepted |
| D3 | 스탯 버프 계산은 Entity 메서드 | 자신의 속성만으로 계산 가능 → 높은 응집도, 테스트 용이 | Accepted |
| D4 | 장착 상태는 Character.EquippedPetId (Nullable FK) | 1:1 관계는 FK 하나로 충분, 최소 복잡도, Equipment 시스템과 일관성 | Accepted |
| D5 | 가챠 로직은 GachaLogicService 재사용 | DRY 원칙, 확률 상수 공유, 스킬/펫 가챠 일관성, 향후 제네릭 리팩토링 가능 | Accepted |

**상세 문서**:
- **ADR 불필요**: 모든 결정이 기존 패턴 재사용 (SkillTemplate, GachaLogicService) → 복잡도 낮음
- **Spike 불필요**: 새 기술 도입 없음, 성능 검증 불필요 (기존 가챠 시스템과 동일)

---

## 📱 Unity Client Integration

### Unity Documentation 필요 항목

**위치**: `../IdleRPGClient/Docs/unity/pet-system/`

#### 1. `API_SPEC.md`
```markdown
# Pet System API Specification

## POST /api/pets/gacha
펫 가챠 수행 (1회 또는 10회)

**Request:**
- CharacterId (Guid): 가챠를 수행하는 캐릭터 ID
- Count (int): 가챠 횟수 (1 또는 10)

**Response:**
- Pets (List<PetDto>): 획득한 펫 목록
- RemainingCrystal (long): 남은 크리스탈
- Cost (int): 소모된 크리스탈

**Errors:**
- 400: 크리스탈 부족 또는 잘못된 count
- 404: 캐릭터 미존재

---

## GET /api/pets?characterId={id}
보유 펫 목록 조회

**Query Parameters:**
- characterId (Guid): 캐릭터 ID

**Response:**
- Pets (List<PetDto>): 펫 목록
- TotalCount (int): 총 펫 개수

---

## PUT /api/pets/{id}/equip
펫 장착/해제

**Request:**
- Equip (bool): true=장착, false=해제

**Response:**
- PetDto: 장착된 펫 정보

---

## PUT /api/pets/{id}/levelup
펫 레벨업 (골드 소모)

**Request:**
- ExperienceToAdd (int): 추가할 경험치 (골드 소모량)

**Response:**
- PetDetailDto: 레벨업된 펫 정보
```

---

#### 2. `DTOs.cs`
```csharp
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace IdleRPGClient.DTOs.Pet
{
    [JsonObject(MemberSerialization.OptIn)]
    public class PetGachaRequestDto
    {
        [JsonProperty("characterId")]
        public Guid CharacterId { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class PetGachaResponseDto
    {
        [JsonProperty("pets")]
        public List<PetDto> Pets { get; set; }

        [JsonProperty("remainingCrystal")]
        public long RemainingCrystal { get; set; }

        [JsonProperty("cost")]
        public int Cost { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class PetDto
    {
        [JsonProperty("petId")]
        public Guid PetId { get; set; }

        [JsonProperty("petTemplateId")]
        public int PetTemplateId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("rarity")]
        public string Rarity { get; set; } // "Common", "Rare", "Epic", "Legendary"

        [JsonProperty("level")]
        public int Level { get; set; }

        [JsonProperty("experience")]
        public int Experience { get; set; }

        [JsonProperty("attackBuff")]
        public int AttackBuff { get; set; }

        [JsonProperty("defenseBuff")]
        public int DefenseBuff { get; set; }

        [JsonProperty("isEquipped")]
        public bool IsEquipped { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class PetDetailDto : PetDto
    {
        [JsonProperty("nextLevelExperience")]
        public int NextLevelExperience { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class PetEquipRequestDto
    {
        [JsonProperty("equip")]
        public bool Equip { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class PetLevelUpRequestDto
    {
        [JsonProperty("experienceToAdd")]
        public int ExperienceToAdd { get; set; }
    }
}
```

---

#### 3. `README.md`
```markdown
# Pet System Unity Documentation

## 개요
펫 시스템은 캐릭터에게 스탯 버프를 제공하는 수집형 시스템입니다.

## 주요 기능
1. **펫 가챠**: 크리스탈을 사용하여 펫 획득
2. **펫 장착**: 1개 펫 장착 가능, 스탯 버프 적용
3. **펫 육성**: 골드로 레벨업, 버프 증가

## Unity 구현 가이드
- **PetManager.cs**: 펫 가챠, 장착/해제, 레벨업 API 호출
- **PetInventoryUI.cs**: 펫 목록 표시, 장착 UI
- **PetGachaUI.cs**: 가챠 연출, 결과 표시

## API 호출 예시
```csharp
// 1. 펫 가챠
var request = new PetGachaRequestDto { CharacterId = characterId, Count = 1 };
var response = await ApiClient.PostAsync<PetGachaResponseDto>("/api/pets/gacha", request);

// 2. 펫 장착
var equipRequest = new PetEquipRequestDto { Equip = true };
await ApiClient.PutAsync<PetDto>($"/api/pets/{petId}/equip", equipRequest);

// 3. 펫 레벨업
var levelUpRequest = new PetLevelUpRequestDto { ExperienceToAdd = 1000 };
await ApiClient.PutAsync<PetDetailDto>($"/api/pets/{petId}/levelup", levelUpRequest);
```
```

---

## ✅ Approval

- [x] Design 리뷰 완료
- [x] 모든 Requirements 항목 커버 확인 (US-1, US-2, US-3, US-4)
- [x] Self-Review Checklist 10개 항목 통과 (10/10)
- [x] **/spec-review 품질 검증 (92/100 - Excellent)**
- [x] Tasks 단계로 진행 승인

---

**작성일**: 2025-10-23
**작성자**: AI + User
**상태**: Approved
**승인일**: 2025-10-23
**Requirements 추적성**:
- US-1 (펫 가챠) → API: POST /api/pets/gacha, Service: PerformGachaAsync
- US-2 (펫 장착) → API: PUT /api/pets/{id}/equip, Service: EquipPetAsync
- US-3 (펫 육성) → API: PUT /api/pets/{id}/levelup, Service: LevelUpPetAsync
- US-4 (펫 목록) → API: GET /api/pets, Service: GetCharacterPetsAsync
