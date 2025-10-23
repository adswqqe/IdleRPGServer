# Design: Skill Gacha System

> 스킬 가챠 시스템의 기술 설계를 정의합니다.
> 
> **설계 원칙**: Clean Architecture, Domain-Driven Design, Test-Driven Development

---

## 📐 Architecture Overview

### Layer Responsibilities

#### API Layer
- **Controllers**: `SkillsController` (가챠 수행, 히스토리 조회)
- **Endpoints**: 
  - `POST /api/skills/gacha` (1회 가챠)
  - `POST /api/skills/gacha/multi` (10연차)
  - `GET /api/skills/gacha/history` (히스토리 조회)
- **DTOs**: `SkillGachaRequestDto`, `SkillGachaResponseDto`, `GachaHistoryDto`

#### Application Layer
- **Services**: `ISkillGachaService` (가챠 오케스트레이션)
- **Business Logic**: 
  - 크리스탈 잔액 검증
  - 스킬 슬롯 확인
  - 트랜잭션 관리 (크리스탈 차감 → 스킬 추가 → 히스토리 기록)
- **Validation**: FluentValidation (CharacterId 필수, 1회/10회 구분)

#### Domain Layer
- **Entities**: `SkillTemplate`, `PlayerSkill`, `GachaHistory`, `Character` (수정), `Player` (수정)
- **Domain Services**: `GachaLogicService` (확률 계산, 랜덤 선택, 천장 시스템)
- **Enums**: `SkillRarity`, `SkillType`
- **Interfaces**: `IRandomProvider` (테스트 가능한 난수 생성)

#### Infrastructure Layer
- **Repositories**: `ISkillTemplateRepository`, `IPlayerSkillRepository`, `IGachaHistoryRepository`
- **Data Access**: EF Core Configurations (SkillTemplateConfiguration, PlayerSkillConfiguration)
- **Services**: `SystemRandomProvider` (IRandomProvider 구현체)

---

## 🗄️ Data Model

### Database Schema

#### 신규 테이블: `SkillTemplates`
```sql
CREATE TABLE "SkillTemplates" (
    "Id" integer PRIMARY KEY,
    "Name" varchar(100) NOT NULL,
    "Description" text,
    "Rarity" integer NOT NULL,           -- 0:Common, 1:Rare, 2:Epic, 3:Legendary
    "SkillType" integer NOT NULL,        -- 0:Passive, 1:Active
    "EffectType" varchar(50),            -- "AttackBoost", "DefenseBoost", etc.
    "EffectValue" integer NOT NULL,
    "CreatedAt" timestamp NOT NULL,
    "UpdatedAt" timestamp NOT NULL
);

CREATE INDEX "IX_SkillTemplates_Rarity" ON "SkillTemplates" ("Rarity");
```

#### 신규 테이블: `PlayerSkills`
```sql
CREATE TABLE "PlayerSkills" (
    "Id" uuid PRIMARY KEY,
    "CharacterId" uuid NOT NULL,
    "SkillTemplateId" integer NOT NULL,
    "AcquiredAt" timestamp NOT NULL,
    "IsEquipped" boolean NOT NULL DEFAULT true,
    "CreatedAt" timestamp NOT NULL,
    "UpdatedAt" timestamp NOT NULL,
    CONSTRAINT "FK_PlayerSkills_Characters" FOREIGN KEY ("CharacterId") REFERENCES "Characters" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_PlayerSkills_SkillTemplates" FOREIGN KEY ("SkillTemplateId") REFERENCES "SkillTemplates" ("Id")
);

CREATE INDEX "IX_PlayerSkills_CharacterId" ON "PlayerSkills" ("CharacterId");
CREATE INDEX "IX_PlayerSkills_SkillTemplateId" ON "PlayerSkills" ("SkillTemplateId");
```

#### 신규 테이블: `GachaHistories`
```sql
CREATE TABLE "GachaHistories" (
    "Id" uuid PRIMARY KEY,
    "CharacterId" uuid NOT NULL,
    "SkillTemplateId" integer NOT NULL,
    "Rarity" integer NOT NULL,
    "PityCountAtDraw" integer NOT NULL,  -- 뽑았을 때의 천장 카운터
    "DrawnAt" timestamp NOT NULL,
    CONSTRAINT "FK_GachaHistories_Characters" FOREIGN KEY ("CharacterId") REFERENCES "Characters" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_GachaHistories_SkillTemplates" FOREIGN KEY ("SkillTemplateId") REFERENCES "SkillTemplates" ("Id")
);

CREATE INDEX "IX_GachaHistories_CharacterId_DrawnAt" ON "GachaHistories" ("CharacterId", "DrawnAt" DESC);
```

#### 수정 테이블: `Characters`
```sql
ALTER TABLE "Characters" ADD COLUMN "PityCount" integer NOT NULL DEFAULT 0;
ALTER TABLE "Characters" ADD COLUMN "SkillSlotCount" integer NOT NULL DEFAULT 4;
```

#### 수정 테이블: `Players`
```sql
ALTER TABLE "Players" ADD COLUMN "Crystal" integer NOT NULL DEFAULT 0;
```

### Entity Relationships
```
Player (1) ──< (N) Character
                    │
                    ├──< (N) PlayerSkill (N) ──> (1) SkillTemplate
                    │
                    └──< (N) GachaHistory (N) ──> (1) SkillTemplate
```

### EF Core Configuration

#### SkillTemplateConfiguration
```csharp
public class SkillTemplateConfiguration : IEntityTypeConfiguration<SkillTemplate>
{
    public void Configure(EntityTypeBuilder<SkillTemplate> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(100);
        builder.Property(s => s.Rarity).IsRequired().HasConversion<int>();
        builder.Property(s => s.SkillType).IsRequired().HasConversion<int>();
        builder.HasIndex(s => s.Rarity).HasDatabaseName("IX_SkillTemplates_Rarity");
    }
}
```

#### PlayerSkillConfiguration
```csharp
public class PlayerSkillConfiguration : IEntityTypeConfiguration<PlayerSkill>
{
    public void Configure(EntityTypeBuilder<PlayerSkill> builder)
    {
        builder.HasKey(ps => ps.Id);
        
        builder.HasOne(ps => ps.Character)
               .WithMany(c => c.PlayerSkills)
               .HasForeignKey(ps => ps.CharacterId)
               .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(ps => ps.SkillTemplate)
               .WithMany()
               .HasForeignKey(ps => ps.SkillTemplateId);
        
        builder.HasIndex(ps => ps.CharacterId);
    }
}
```

---

## 🔌 API Design

### Endpoints

#### `POST /api/skills/gacha`
**Request:**
```json
{
  "characterId": "uuid",
  "drawCount": 1  // 1 또는 10
}
```

**Response (200 OK):**
```json
{
  "skills": [
    {
      "skillTemplateId": 101,
      "name": "강력한 일격",
      "rarity": "Epic",
      "skillType": "Active",
      "effectValue": 50
    }
  ],
  "remainingCrystal": 450,
  "newPityCount": 23
}
```

**Errors:**
- `400 Bad Request`: 크리스탈 부족, 스킬 슬롯 부족
- `401 Unauthorized`: JWT 토큰 없음
- `404 Not Found`: 캐릭터 미존재

---

#### `GET /api/skills/gacha/history?characterId={uuid}`
**Response (200 OK):**
```json
{
  "histories": [
    {
      "id": "uuid",
      "skillName": "강력한 일격",
      "rarity": "Epic",
      "drawnAt": "2025-10-20T14:30:00Z",
      "pityCountAtDraw": 22
    }
  ],
  "totalCount": 47
}
```

---

## 🧮 Business Logic

### 핵심 알고리즘

#### GachaLogicService.DetermineRarity
**책임**: 확률에 따라 스킬 등급 결정 (천장 시스템 포함)

**입력:**
- `pityCount`: 현재 천장 카운터 (0~100)

**출력:**
- `SkillRarity`: Common / Rare / Epic / Legendary

**프로세스:**
```csharp
public SkillRarity DetermineRarity(int pityCount)
{
    // 천장 시스템 (100회 도달)
    if (pityCount >= PITY_THRESHOLD)
        return SkillRarity.Legendary;
    
    int rand = _randomProvider.Next(100); // 0~99
    
    // 누적 확률 방식
    if (rand < 1)                    // 1% (0)
        return SkillRarity.Legendary;
    else if (rand < 10)              // 9% (1~9)
        return SkillRarity.Epic;
    else if (rand < 40)              // 30% (10~39)
        return SkillRarity.Rare;
    else                             // 60% (40~99)
        return SkillRarity.Common;
}
```

**상수:**
```csharp
private const int PITY_THRESHOLD = 100;
private const int LEGENDARY_RATE = 1;
private const int EPIC_RATE = 9;
private const int RARE_RATE = 30;
```

---

#### GachaLogicService.SelectRandomSkill
**책임**: 주어진 등급에서 랜덤하게 스킬 1개 선택

**입력:**
- `availableSkills`: IEnumerable<SkillTemplate>
- `rarity`: SkillRarity

**출력:**
- `SkillTemplate`: 선택된 스킬

**프로세스:**
```csharp
public SkillTemplate SelectRandomSkill(
    IEnumerable<SkillTemplate> availableSkills, 
    SkillRarity rarity)
{
    var skills = availableSkills
        .Where(s => s.Rarity == rarity)
        .ToList();
    
    if (!skills.Any())
        throw new ArgumentException($"'{rarity}' 등급 스킬이 없습니다.");
    
    return skills[_randomProvider.Next(skills.Count)];
}
```

---

#### GachaLogicService.GetPityCountAfterDraw
**책임**: 뽑기 후 천장 카운터 계산

**입력:**
- `currentPityCount`: 현재 카운터
- `drawnRarity`: 뽑은 등급

**출력:**
- `int`: 새로운 카운터 (0 또는 +1)

**프로세스:**
```csharp
public int GetPityCountAfterDraw(int currentPityCount, SkillRarity drawnRarity)
{
    return drawnRarity == SkillRarity.Legendary ? 0 : currentPityCount + 1;
}
```

---

## 🎯 Service Layer Design

### SkillGachaService

**책임**: 가챠 비즈니스 로직 오케스트레이션

**메서드:**

#### `DrawSkillsAsync(characterId, drawCount, cancellationToken)`
```csharp
public async Task<SkillGachaResponseDto> DrawSkillsAsync(
    Guid characterId, 
    int drawCount,  // 1 또는 10
    CancellationToken cancellationToken = default)
{
    // 1. 캐릭터 조회 (Player 포함, Include)
    var character = await _characterRepository
        .GetWithPlayerAsync(characterId, cancellationToken);
    if (character == null)
        throw new NotFoundException("캐릭터를 찾을 수 없습니다.");
    
    // 2. 크리스탈 잔액 검증
    int cost = drawCount == 1 ? 50 : 500;
    if (character.Player.Crystal < cost)
        throw new InvalidOperationException("크리스탈이 부족합니다.");
    
    // 3. 스킬 슬롯 검증
    int currentSkillCount = await _playerSkillRepository
        .CountByCharacterIdAsync(characterId, cancellationToken);
    if (currentSkillCount + drawCount > character.SkillSlotCount)
        throw new InvalidOperationException("스킬 슬롯이 부족합니다.");
    
    // 4. 모든 SkillTemplate 조회 (캐싱 고려)
    var allSkills = await _skillTemplateRepository.GetAllAsync(cancellationToken);
    
    // 5. 트랜잭션 시작
    using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    
    try
    {
        var drawnSkills = new List<SkillTemplate>();
        int pityCount = character.PityCount;
        
        // 6. drawCount만큼 반복
        for (int i = 0; i < drawCount; i++)
        {
            // 6.1. 등급 결정 (Domain Service)
            var rarity = _gachaLogicService.DetermineRarity(pityCount);
            
            // 6.2. 스킬 선택 (Domain Service)
            var skill = _gachaLogicService.SelectRandomSkill(allSkills, rarity);
            drawnSkills.Add(skill);
            
            // 6.3. PlayerSkill 추가
            var playerSkill = new PlayerSkill
            {
                CharacterId = characterId,
                SkillTemplateId = skill.Id,
                AcquiredAt = DateTime.UtcNow
            };
            await _playerSkillRepository.AddAsync(playerSkill, cancellationToken);
            
            // 6.4. GachaHistory 기록
            var history = new GachaHistory
            {
                CharacterId = characterId,
                SkillTemplateId = skill.Id,
                Rarity = rarity,
                PityCountAtDraw = pityCount,
                DrawnAt = DateTime.UtcNow
            };
            await _gachaHistoryRepository.AddAsync(history, cancellationToken);
            
            // 6.5. 천장 카운터 업데이트
            pityCount = _gachaLogicService.GetPityCountAfterDraw(pityCount, rarity);
        }
        
        // 7. 크리스탈 차감 및 천장 카운터 저장
        character.Player.Crystal -= cost;
        character.PityCount = pityCount;
        await _context.SaveChangesAsync(cancellationToken);
        
        // 8. 트랜잭션 커밋
        await transaction.CommitAsync(cancellationToken);
        
        // 9. 응답 DTO 반환
        return new SkillGachaResponseDto
        {
            Skills = drawnSkills.Select(s => new SkillDto { ... }).ToList(),
            RemainingCrystal = character.Player.Crystal,
            NewPityCount = pityCount
        };
    }
    catch
    {
        await transaction.RollbackAsync(cancellationToken);
        throw;
    }
}
```

**Dependencies:**
- `ICharacterRepository`: GetWithPlayerAsync
- `IPlayerSkillRepository`: AddAsync, CountByCharacterIdAsync
- `ISkillTemplateRepository`: GetAllAsync
- `IGachaHistoryRepository`: AddAsync
- `GachaLogicService`: DetermineRarity, SelectRandomSkill, GetPityCountAfterDraw

---

## 🧪 Testing Strategy

### Unit Tests

#### GachaLogicServiceTests (12개 테스트)
**Mock**: `IRandomProvider`

**테스트 케이스:**
1. `DetermineRarity_PityCount100_ReturnsLegendary` (천장 시스템)
2. `DetermineRarity_RandomValue0_ReturnsLegendary` (1% 확률)
3. `DetermineRarity_RandomValue1to9_ReturnsEpic` (9% 확률, Theory 3개)
4. `DetermineRarity_RandomValue10to39_ReturnsRare` (30% 확률, Theory 3개)
5. `DetermineRarity_RandomValue40to99_ReturnsCommon` (60% 확률, Theory 3개)
6. `SelectRandomSkill_FiltersByRarity_ReturnsCorrectSkill`
7. `SelectRandomSkill_NoSkillsForRarity_ThrowsArgumentException`
8. `GetPityCountAfterDraw_Legendary_ResetsToZero`
9. `GetPityCountAfterDraw_OtherRarity_IncrementsCount`

**코드 예시:**
```csharp
[Fact]
public void DetermineRarity_RandomValue0_ReturnsLegendary()
{
    // Arrange
    _mockRandomProvider.Setup(r => r.Next(100)).Returns(0);
    
    // Act
    var result = _service.DetermineRarity(0);
    
    // Assert
    result.Should().Be(SkillRarity.Legendary);
}
```

---

#### SkillGachaServiceTests (8개 테스트)
**Mock**: Repository들, GachaLogicService, DbContext

**테스트 케이스:**
1. `DrawSkillsAsync_ValidRequest_ReturnsSkills`
2. `DrawSkillsAsync_InsufficientCrystal_ThrowsInvalidOperationException`
3. `DrawSkillsAsync_FullSkillSlot_ThrowsInvalidOperationException`
4. `DrawSkillsAsync_CharacterNotFound_ThrowsNotFoundException`
5. `DrawSkillsAsync_10Draw_DrawsTenSkills`
6. `DrawSkillsAsync_UpdatesPityCountCorrectly`
7. `DrawSkillsAsync_SavesGachaHistory`
8. `DrawSkillsAsync_RollbackOnError`

---

### Integration Tests
- `POST /api/skills/gacha` - 실제 DB 연동 테스트
- Transaction 롤백 검증
- 동시 요청 처리 (Concurrency)

### Test Coverage Goals
- **GachaLogicService**: 100% (핵심 비즈니스 로직)
- **SkillGachaService**: 90%+
- **SkillsController**: 80%+

---

## ⚠️ Error Handling

### Exception Types
- `NotFoundException`: 캐릭터 미존재 → 404
- `InvalidOperationException`: 크리스탈 부족, 슬롯 부족 → 400
- `ArgumentException`: 잘못된 파라미터 (drawCount != 1 or 10) → 400

### Error Response Format
```json
{
  "message": "크리스탈이 부족합니다.",
  "statusCode": 400
}
```

---

## 🔐 Security Considerations

- **Authentication**: `[Authorize]` 속성으로 JWT 필수
- **Authorization**: CharacterId가 현재 사용자 소유인지 검증
- **Transaction**: ACID 보장 (크리스탈 차감과 스킬 추가 원자성)
- **SQL Injection Prevention**: EF Core 파라미터화 쿼리

---

## 📊 Performance Considerations

- **Database Indexes**: 
  - `IX_SkillTemplates_Rarity` (등급별 조회)
  - `IX_PlayerSkills_CharacterId` (캐릭터별 스킬 조회)
  - `IX_GachaHistories_CharacterId_DrawnAt` (히스토리 조회 + 정렬)
- **Caching**: SkillTemplate 전체 목록 메모리 캐싱 (IMemoryCache)
- **Eager Loading**: `Include(c => c.Player)` (N+1 방지)
- **Connection Pool**: EF Core 기본 설정 (100 connections)

---

## 🔄 Migration Plan

### Database Migration
```sql
-- migration.sql에 추가
DO $EF$ BEGIN
    -- SkillTemplates 테이블 생성
    IF NOT EXISTS(SELECT 1 FROM information_schema.tables WHERE table_name = 'SkillTemplates') THEN
        CREATE TABLE "SkillTemplates" ( ... );
        CREATE INDEX "IX_SkillTemplates_Rarity" ON "SkillTemplates" ("Rarity");
    END IF;
    
    -- PlayerSkills 테이블 생성
    IF NOT EXISTS(SELECT 1 FROM information_schema.tables WHERE table_name = 'PlayerSkills') THEN
        CREATE TABLE "PlayerSkills" ( ... );
    END IF;
    
    -- GachaHistories 테이블 생성
    IF NOT EXISTS(SELECT 1 FROM information_schema.tables WHERE table_name = 'GachaHistories') THEN
        CREATE TABLE "GachaHistories" ( ... );
    END IF;
    
    -- Characters 테이블 수정
    IF NOT EXISTS(SELECT 1 FROM information_schema.columns 
                  WHERE table_name = 'Characters' AND column_name = 'PityCount') THEN
        ALTER TABLE "Characters" ADD COLUMN "PityCount" integer NOT NULL DEFAULT 0;
        ALTER TABLE "Characters" ADD COLUMN "SkillSlotCount" integer NOT NULL DEFAULT 4;
    END IF;
    
    -- Players 테이블 수정
    IF NOT EXISTS(SELECT 1 FROM information_schema.columns 
                  WHERE table_name = 'Players' AND column_name = 'Crystal') THEN
        ALTER TABLE "Players" ADD COLUMN "Crystal" integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;
```

### Data Seeding
**SkillTemplateSeeder**: 초기 스킬 템플릿 데이터 (50개)
- Common: 30개
- Rare: 15개
- Epic: 4개
- Legendary: 1개

---

## 📱 Unity Client Integration

### Unity Documentation 필요 항목
1. **API_SPEC.md**: 
   - `POST /api/skills/gacha` 명세
   - UnityWebRequest C# 예제
2. **DTOs.cs**: 
   - `SkillGachaRequestDto`, `SkillGachaResponseDto`
   - `[JsonProperty]` 속성 (Newtonsoft.Json)
3. **GachaUI 연동 가이드**:
   - 크리스탈 잔액 표시
   - 천장 카운터 UI
   - 뽑기 애니메이션 타이밍

**참고**: `docs/unity/UNITY_DOCUMENTATION_GUIDE.md`

---

## ✅ Approval

- [x] Design 리뷰 완료 (2025-10-20)
- [x] 모든 Requirements 항목 커버 (US-1~4)
- [x] Gemini 2.5 Pro 검증 완료 (8가지 핵심 질문 해소)
- [x] Zen MCP Code Review 완료 (6개 이슈 수정)
- [x] Tasks 단계로 진행 승인

---

**작성일**: 2025-10-20  
**작성자**: IdleRPG Team (Claude + Human + Gemini)  
**상태**: Approved ✅  
**Requirements 추적성**: US-1 (가챠), US-2 (천장), US-3 (스킬), US-4 (히스토리)
