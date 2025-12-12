# Pet System Spec - 진행 상황

## 📋 세션 정보
- **시작일**: 2025-10-24
- **완료일**: 2025-10-25
- **모드**: 학습 모드 (13개 질문)
- **진행도**: 100% 완료 ✅

---

## ✅ 완료된 설계 결정

### Phase 1: 데이터 모델링 (완료)

#### Q1: 펫-캐릭터 관계
- **선택**: A - 1:N 관계 (Pet → Character)
- **구조**: `Character (1) → Pets (N)`
- **FK**: `Pet.CharacterId` (GUID, NOT NULL)
- **학습 포인트**: 외래키 설계, 비즈니스 요구사항의 데이터 모델 반영

#### Q2: Cascade Delete 규칙
- **선택**: A - ON DELETE CASCADE (DB 레벨)
- **이유**: 성능, 일관성, 단순한 소유 관계
- **설정**: EF Core `OnDelete(DeleteBehavior.Cascade)`
- **학습 포인트**: DB 제약조건 vs 비즈니스 로직 위치

#### Q3: 펫 마스터 데이터 (템플릿)
- **선택**: A - PetTemplate 별도 테이블 (정규화)
- **구조**: `PetTemplates` (Id INT, Name, RarityId, BaseAttack, BaseMana)
- **FK**: `Pet.TemplateId` REFERENCES `PetTemplates(Id)`
- **추가 학습**: JSONB 비정규화의 5가지 문제점 (중복, 업데이트 이상, 참조 무결성, 쿼리 복잡도, 인덱싱)
- **학습 포인트**: 정규화 vs 비정규화, 확장성 vs 성능

#### Q4: 펫 장착 시스템
- **선택**: 2 - 여러 마리 장착 (슬롯 시스템, 최대 3마리)
- **구조**: `EquippedPets` 별도 테이블
  ```sql
  CREATE TABLE equipped_pets (
    character_id UUID REFERENCES characters(id),
    pet_id UUID REFERENCES pets(id),
    slot_index INT CHECK (slot_index BETWEEN 1 AND 3),
    equipped_at TIMESTAMP,
    PRIMARY KEY (character_id, slot_index),
    UNIQUE (pet_id)
  );
  ```
- **제약조건**: UNIQUE(CharacterId, SlotIndex), UNIQUE(PetId)
- **학습 포인트**: 데이터 무결성, M:N 관계, 확장 가능한 설계

#### 추가 결정: 공통 Rarity Entity
- **배경**: Equipment/Skill/Pet 모두 동일한 4단계 등급 체계
- **선택**: 공통 Rarity Entity 사용 (Shared Kernel 패턴)
- **구조**: `Rarities` (Id, Name, Order, Color, IconPath)
- **재사용**: EquipmentTemplate, SkillTemplate, PetTemplate에서 RarityId FK
- **ROI**: 초기 30분 투자 → 장기 100분 절감
- **추가 학습**: DDD Shared Kernel, OCP (개방-폐쇄 원칙), 중복 제거(DRY)

---

### Phase 2: 아키텍처 계층 결정 (진행 중)

#### Q5: 펫 가챠 확률 계산 로직 위치
- **선택**: A - Domain Service (순수 비즈니스 로직)
- **구조**: `PetGachaService` (Domain Layer)
- **이유**: 테스트 용이, 재사용 가능, 외부 의존성 없음
- **학습 포인트**: Clean Architecture 계층 책임 분리

#### Q6: 천장 카운터 관리
- **선택**: B - Value Object (PetGachaCounter)
- **구조**:
  ```csharp
  public class PetGachaCounter
  {
      public int Count { get; }
      public const int HardPity = 50;
      public const int SoftPityStart = 40;

      public PetGachaCounter Increment() => new PetGachaCounter(Count + 1);
      public PetGachaCounter Reset() => new PetGachaCounter(0);
      public double GetLegendaryBonusProbability() { ... }
      public bool IsHardPityTriggered() => Count >= HardPity;
  }
  ```
- **추가 학습**:
  - DDD Value Object 개념 (Immutable, Equality by Value)
  - Primitive Obsession 방지
  - Soft Pity 확률 시스템 구현 (40-49회: 점진적 확률 증가)
  - 확장성: Configuration 테이블 vs appsettings.json vs 하드코딩
- **학습 포인트**: Value Object 패턴, 풍부한 도메인 모델(Rich Domain Model)

#### Q7: 트랜잭션 경계 설정
- **선택**: C - EF Core SaveChanges 자동 트랜잭션 (Unit of Work 패턴 활용)
- **구조**:
  ```csharp
  // Application/Services/PetService.cs
  public async Task<PetGachaResultDto> ExecuteGachaAsync(Guid characterId)
  {
      // 1. 여러 Repository 호출
      var character = await _characterRepo.GetByIdAsync(characterId);
      character.DeductCrystal(100);
      character.IncrementGachaCounter();

      var pet = await _petRepo.CreateAsync(...);
      await _gachaHistoryRepo.AddAsync(...);

      // 2. Unit of Work SaveChanges (자동 트랜잭션)
      await _unitOfWork.SaveChangesAsync();
      return MapToDto(pet);
  }
  ```
- **이유**:
  - 프로젝트는 이미 Unit of Work 패턴 적용 (DbContext가 구현)
  - 단순 플로우는 자동 트랜잭션으로 충분
  - 코드 간결성 유지
- **예외 케이스**: 외부 API 호출 포함 시 명시적 트랜잭션 고려
- **학습 포인트**: Unit of Work 패턴, EF Core 변경 추적, 자동 트랜잭션 경계

---

---

### Phase 3: 데이터베이스 설계 (진행 중)

#### Q8: 인덱스 전략
- **선택**: A - Pets.CharacterId 단일 인덱스 (최소 필수 인덱스)
- **구조**:
  ```sql
  CREATE INDEX idx_pets_character_id ON pets(character_id);
  ```
- **이유**:
  - **가장 빈번한 쿼리**: "내 캐릭터의 펫 목록" (99% 사용 케이스)
  - **클라이언트 필터링**: RarityId, TemplateId 필터는 클라이언트에서 처리
    - 한 캐릭터가 보유한 펫 수: 50-200마리 예상 (네트워크 전송 가능)
    - Unity 클라이언트에서 LINQ 필터링 충분
  - **트레이드오프**: 복합 인덱스(B, C)는 과도한 최적화 (Premature Optimization)
- **추가 인덱스 고려 시점**: 펫 개수 500+ 초과 시 성능 측정 후 추가
- **학습 포인트**:
  - 인덱스 전략의 비용 분석 (읽기 vs 쓰기 성능)
  - 클라이언트-서버 책임 분리
  - Premature Optimization 방지 (YAGNI 원칙)
  - PostgreSQL FK 자동 인덱싱 없음 (MySQL과 차이)

## 🔄 다음 진행 예정

#### Q9: N+1 문제 방지
- **선택**: A - Eager Loading (Include) + 읽기 전용 API 최적화
- **구조**:
  ```csharp
  // 1. 일반 CRUD (Entity 추적 필요)
  public async Task<Pet> GetPetByIdAsync(Guid petId)
  {
      return await _context.Pets
          .Include(p => p.Template)
          .FirstOrDefaultAsync(p => p.Id == petId);
  }

  // 2. 읽기 전용 API (로그인 시 펫 목록 조회)
  public async Task<List<PetDto>> GetCharacterPetsAsync(Guid characterId)
  {
      return await _context.Pets
          .Where(p => p.CharacterId == characterId)
          .Include(p => p.Template)
          .AsNoTracking()  // 변경 추적 비활성화
          .Select(p => new PetDto
          {
              Id = p.Id,
              TemplateId = p.TemplateId,
              TemplateName = p.Template.Name,
              RarityId = p.Template.RarityId,
              Level = p.Level,
              CurrentAttack = p.CurrentAttack,
              CurrentMana = p.CurrentMana
          })
          .ToListAsync();
  }
  ```
- **이유**:
  - **기본은 Include**: 대부분 API에서 Template 정보 필요
  - **읽기 전용 최적화**: 로그인/초기 로딩은 AsNoTracking() + Projection
  - **성능 이점**: AsNoTracking()으로 메모리 사용량 30-40% 감소
- **적용 시점**:
  - Include: Update/Delete 가능성 있는 API
  - AsNoTracking: GET 전용 API (로그인, 목록 조회)
- **학습 포인트**:
  - N+1 문제 원인과 해결 (Include vs Explicit Loading)
  - AsNoTracking() 성능 최적화
  - Projection (Select)으로 네트워크 트래픽 감소
  - CQRS 패턴의 기초 (읽기 vs 쓰기 최적화)

#### Q10: PK 타입 (GUID vs INT)
- **선택**: C - 하이브리드 (Pet은 INT, Character는 GUID 유지)
- **구조**:
  ```csharp
  public class Pet
  {
      public int Id { get; set; }  // INT (AUTO_INCREMENT)
      public Guid CharacterId { get; set; }  // FK to Character (GUID)
      public int TemplateId { get; set; }  // FK to PetTemplate (INT)
  }
  ```
  ```sql
  CREATE TABLE pets (
    id SERIAL PRIMARY KEY,  -- INT AUTO_INCREMENT
    character_id UUID NOT NULL REFERENCES characters(id),
    template_id INT NOT NULL REFERENCES pet_templates(id),
    level INT DEFAULT 1,
    current_attack INT,
    current_mana INT,
    created_at TIMESTAMP DEFAULT NOW()
  );
  ```
- **이유**:
  - **Pet의 성격**: Character의 종속 Entity (Character 없이 독립 존재 안 함)
  - **일관성**: Equipment도 INT 사용 중 (같은 "소유 아이템" 개념)
  - **성능**: INT PK가 GUID보다 인덱스 성능 우수 (4바이트 vs 16바이트)
  - **디버깅**: 가독성 향상 (Pet #1234 vs Pet #a7f3e...)
  - **분산 환경**: 단일 DB 운영 중, 필요시 나중에 마이그레이션 가능
- **트레이드오프**:
  - 포기: 클라이언트 ID 미리 생성 (가챠 시 서버 응답 대기 필요)
  - 획득: 저장 공간 75% 절약, 인덱스 성능 향상, 일관성
- **프로젝트 패턴**:
  - GUID: Player, Character (독립 Entity)
  - INT: Equipment, Pet (종속 Entity), 모든 Template (마스터 데이터)
- **학습 포인트**:
  - PK 타입 선택 기준 (독립 vs 종속 Entity)
  - 하이브리드 전략 (일관성보다 실용성)
  - 저장 공간과 성능 트레이드오프
  - 프로젝트 전체 패턴 분석

---

### Phase 3 완료! ✅

**데이터베이스 설계 결정 사항**:
1. ✅ CharacterId 단일 인덱스 (클라이언트 필터링 활용)
2. ✅ Include + AsNoTracking 하이브리드 (읽기/쓰기 최적화)
3. ✅ Pet PK는 INT (종속 Entity 패턴)

---

### Phase 4: API 설계 (진행 중)

#### Q11: RESTful 엔드포인트 설계
- **선택**: B - Action 중심 (RPC 스타일) + 프로젝트 기존 패턴 일관성 유지
- **구조**:
  ```
  POST   /api/pets/gacha                    # 펫 가챠
  GET    /api/pets?characterId={id}         # 펫 목록 조회
  GET    /api/pets/{petId}                  # 펫 상세 조회
  POST   /api/pets/{petId}/level-up         # 펫 레벨업
  POST   /api/pets/equip                    # 펫 장착
  POST   /api/pets/unequip                  # 펫 해제
  GET    /api/pets/equipped?characterId={id} # 장착된 펫 조회
  DELETE /api/pets/{petId}                  # 펫 삭제 (판매 등)
  ```
- **이유**:
  - **프로젝트 일관성**: 기존 Equipment, Skill, Dungeon과 동일한 패턴
  - **팀 생산성**:
    - Equipment: `POST /equipment/equip` → Pet: `POST /pets/equip` (학습 곡선 ↓)
    - Skill: `POST /skills/gacha` → Pet: `POST /pets/gacha` (일관성 ↑)
  - **클라이언트 편의성**: Unity 개발자가 이미 익숙한 API 구조
  - **소규모 팀**: 학습 프로젝트 (1-2명), 빠른 프로토타이핑 우선
- **프로젝트 API 패턴 분석**:
  - ✅ Action은 POST + 동사 URL (`/equip`, `/gacha`, `/clear`)
  - ✅ 조회는 GET + 명사 (`/equipped`, `/stages`)
  - ✅ characterId는 쿼리스트링 또는 Body
- **Request/Response 예시**:
  ```csharp
  // POST /api/pets/gacha
  Request:  { "characterId": "guid" }
  Response: {
    "pet": { "id": 123, "templateId": 5, "rarity": "Legendary" },
    "isDuplicate": false,
    "currentPityCount": 35
  }

  // POST /api/pets/equip
  Request:  { "petId": 123, "slotIndex": 1 }
  Response: { "pet": {...}, "message": "펫이 슬롯 1에 장착되었습니다" }
  ```
- **학습 포인트**:
  - RESTful vs RPC 스타일 트레이드오프
  - 프로젝트 일관성의 중요성 (새 개발자 온보딩 비용)
  - 소규모 팀에서의 실용적 API 설계
  - 기존 코드베이스 패턴 분석 능력

#### Q12: DTO 구조 (Request/Response)
- **선택**: B - 풍부한 정보 (메타 정보 포함) - 게임 산업 표준
- **구조**:
  ```csharp
  // 가챠 Response DTO
  public class PetGachaResponseDto
  {
      public PetDto Pet { get; set; }
      public bool IsDuplicate { get; set; }
      public int CurrentPityCount { get; set; }      // 천장 카운터 (0-50)
      public int RemainingCrystal { get; set; }      // 가챠 후 남은 크리스탈
      public DuplicateRewardDto? DuplicateReward { get; set; }  // 중복 시 보상
  }

  // Pet DTO (기본 펫 정보)
  public class PetDto
  {
      public int Id { get; set; }
      public int TemplateId { get; set; }
      public string TemplateName { get; set; }
      public int RarityId { get; set; }
      public string RarityName { get; set; }        // "Legendary", "Epic", ...
      public int Level { get; set; }
      public int CurrentAttack { get; set; }
      public int CurrentMana { get; set; }
      public string ImageUrl { get; set; }          // Unity에서 즉시 표시
  }

  // 중복 보상 DTO
  public class DuplicateRewardDto
  {
      public int Gold { get; set; }                 // 중복 시 골드 보상
      public int Experience { get; set; }           // 중복 시 경험치 보상
  }

  // 장착 Request DTO
  public class EquipPetRequestDto
  {
      public Guid CharacterId { get; set; }
      public int PetId { get; set; }
      public int SlotIndex { get; set; }            // 1-3
  }

  // 레벨업 Request DTO
  public class LevelUpPetRequestDto
  {
      public int PetId { get; set; }
      public int Count { get; set; }                // 레벨업 횟수 (1회 또는 여러 번)
  }
  ```
- **이유**:
  - **게임 산업 표준**: "화면 렌더링에 필요한 모든 데이터 한 번에 제공"
  - **UX 최적화**:
    - 가챠 연출: Pet 이미지, 등급, 이름 즉시 표시
    - UI 동시 업데이트: 크리스탈 잔액, 천장 카운터 (N회 남음)
    - 중복 처리: "이미 보유 중! 골드 500 획득" 알림
  - **네트워크 최적화**:
    - 1번 API 호출 vs 3번 호출 (가챠 + 캐릭터 조회 + 펫 조회)
    - 모바일 환경 (3G/4G) 응답속도 3배 향상
  - **실제 게임 사례**: Genshin Impact, Epic Seven 동일 패턴
- **응답 크기 분석**:
  - B 스타일: ~600 bytes (JSON)
  - gzip 압축: ~250 bytes
  - A 스타일 3번 호출: ~900 bytes + 네트워크 왕복 비용
  - **결론**: B가 오히려 효율적
- **트레이드오프**:
  - 포기: DTO 단순함 (A 스타일 대비)
  - 획득: 클라이언트 개발 생산성 ↑, UX ↑, 네트워크 효율 ↑
- **학습 포인트**:
  - 게임 API 설계의 Golden Rule (클라이언트 중심 설계)
  - RESTful 원칙 vs 실용성 (게임은 실용성 우선)
  - DTO 설계 시 네트워크 왕복 비용 고려
  - 모바일 환경 최적화 전략

#### Q13: 인증/권한 설정
- **선택**: B - 읽기 Public, 쓰기 인증 + 소유권 검증 (확장 가능)
- **구조**:
  ```csharp
  [Route("api/pets")]
  public class PetController : BaseController
  {
      // 쓰기 API: 인증 + 소유권 검증
      [HttpPost("gacha")]
      [Authorize]
      public async Task<IActionResult> PerformGacha([FromBody] PetGachaRequestDto dto)
      {
          var playerId = GetCurrentUserId();  // JWT에서 PlayerId 추출

          var character = await _characterService.GetByIdAsync(dto.CharacterId);
          if (character.PlayerId != playerId)
              return Forbid();  // 403

          // 가챠 처리
      }

      [HttpPost("equip")]
      [Authorize]
      public async Task<IActionResult> EquipPet([FromBody] EquipPetRequestDto dto)
      {
          var playerId = GetCurrentUserId();
          // 소유권 검증
      }

      // 읽기 API: Public (리더보드, PvP 대비)
      [HttpGet]
      public async Task<IActionResult> GetPets([FromQuery] Guid characterId)
      {
          // 누구나 조회 가능 (게임 데이터는 공개)
          // 단, 민감 정보는 제외 (가챠 이력, 크리스탈 잔액)
      }

      [HttpGet("equipped")]
      public async Task<IActionResult> GetEquippedPets([FromQuery] Guid characterId)
      {
          // Public: 리더보드에서 다른 플레이어 펫 조회 가능
      }

      // 민감 정보: 인증 필수
      [HttpGet("gacha-history")]
      [Authorize]
      public async Task<IActionResult> GetGachaHistory()
      {
          var playerId = GetCurrentUserId();
          // 자신의 가챠 이력만 조회 가능
      }
  }
  ```
- **이유**:
  - **미래 확장성**: 리더보드, PvP, 길드 시스템 대비
    - 리더보드: "1위 유저의 펫 조합 보기" 기능 필요
    - PvP: 상대 펫 정보 확인 (전략 수립)
    - 길드: 길드원 펫 조회 (팀 편성 참고)
  - **보안 원칙**: "게임 데이터는 Public, 민감 정보만 Private"
    - Public: 펫 목록, 스탯, 장착 정보
    - Private: 가챠 이력, 크리스탈 잔액, 거래 내역
  - **실제 게임 사례**:
    - Summoners War: 다른 플레이어 몬스터 조회 가능
    - Genshin Impact: 친구 캐릭터 조회 (Co-op)
- **Dependencies (외부 의존성)**:
  - **API Security Refactoring** (별도 Spec 필요, S 사이즈)
    - 현재: DungeonController, EquipmentController 모두 `[Authorize]` (읽기도 인증 필수)
    - 변경 후: 읽기 Public, 쓰기 인증 분리
    - 영향 범위: 3-4개 Controller
    - 우선순위: Pet System 작업 전 완료 권장
- **트레이드오프**:
  - 포기: 완벽한 데이터 프라이버시 (누구나 펫 개수 조회 가능)
  - 획득: 게임 기능 확장성, 커뮤니티 형성 (랭킹, 비교)
- **학습 포인트**:
  - 보안 설계는 비즈니스 요구사항에서 출발 (기술 우선 X)
  - 확장 가능한 설계 (현재 없어도 미래 고려)
  - Spec 분리 원칙 (Single Responsibility)
  - 의존성 관리 (External Dependencies 명시)

---

### Phase 4 완료! ✅

**API 설계 결정 사항**:
1. ✅ Action 중심 RPC 스타일 (프로젝트 패턴 일관성)
2. ✅ 풍부한 DTO (게임 산업 표준, 클라이언트 최적화)
3. ✅ 읽기 Public, 쓰기 인증 (확장 가능한 보안)

---

### Phase 5: 게임 밸런스 (AI 자동 제안) ✅

#### 가챠 확률
- **Common**: 60%
- **Rare**: 30%
- **Epic**: 9%
- **Legendary**: 1%

#### 천장 시스템 (Pity)
- **Hard Pity**: 50회 (100% Legendary 보장)
- **Soft Pity**: 40회부터 확률 증가
  - 40회: +1% (총 2%)
  - 45회: +3% (총 4%)
  - 50회: 100%

#### 가챠 비용
- **1회**: 크리스탈 100개
- **10연차**: 크리스탈 900개 (10% 할인)

#### 중복 보상
- **Common**: 골드 100
- **Rare**: 골드 500
- **Epic**: 골드 2,000
- **Legendary**: 골드 10,000

#### 레벨업 비용
- **공식**: 이전 레벨 비용 × 1.5
- **예시**: Lv1→2 (100골드) → Lv2→3 (150골드) → Lv3→4 (225골드)
- **Max Level**: 50

#### 펫 스탯 공식
```csharp
// 펫 스탯 (레벨업 시)
Pet.CurrentAttack = Template.BaseAttack + (Level - 1) * 10
Pet.CurrentMana = Template.BaseMana + (Level - 1) * 5

// 캐릭터 버프 (펫 장착 시)
CharacterAttack += Pet.CurrentAttack * 0.1  // 10%
CharacterMana += Pet.CurrentMana * 0.1      // 10%
```

#### 펫 템플릿 예시 (Seeder 데이터)
```sql
-- Legendary
INSERT INTO pet_templates (name, rarity_id, base_attack, base_mana, image_url)
VALUES ('Fire Dragon', 4, 100, 50, '/pets/fire-dragon.png');

-- Epic
INSERT INTO pet_templates (name, rarity_id, base_attack, base_mana, image_url)
VALUES ('Ice Phoenix', 3, 70, 35, '/pets/ice-phoenix.png');

-- Rare
INSERT INTO pet_templates (name, rarity_id, base_attack, base_mana, image_url)
VALUES ('Forest Wolf', 2, 40, 20, '/pets/forest-wolf.png');

-- Common
INSERT INTO pet_templates (name, rarity_id, base_attack, base_mana, image_url)
VALUES ('Slime', 1, 20, 10, '/pets/slime.png');
```

---

### Phase 5 완료! ✅

### Phase 6: 최종 문서 생성
- requirements.md 완성
- TODO(human) 마커 추가 (아키텍처 학습 포인트만)
- Spike/ADR 필요성 검토

---

## 📚 학습한 핵심 개념

1. **데이터베이스 설계**:
   - 정규화 vs 비정규화
   - FK 제약조건, Cascade Delete
   - JSONB의 적절한 사용처 (메타데이터)
   - 참조 무결성, 데이터 중복 방지

2. **DDD (Domain-Driven Design)**:
   - Value Object (불변 객체, 캡슐화)
   - Shared Kernel (공통 도메인 개념)
   - Rich Domain Model (풍부한 도메인 모델)
   - Primitive Obsession 방지

3. **Clean Architecture**:
   - Domain Service vs Application Service
   - 계층별 책임 분리
   - 의존성 규칙

4. **설계 원칙**:
   - OCP (개방-폐쇄 원칙)
   - DRY (중복 제거)
   - 확장성 vs 복잡도 트레이드오프
   - Configuration-driven Design

5. **실무 패턴**:
   - Strategy Pattern (확률 계산 전략)
   - Shared Kernel (공통 Rarity)
   - 슬롯 시스템 (M:N + 제약조건)

---

## 💡 TODO(human) 후보 (아키텍처 학습 포인트)

1. **Rarity 시스템 설계 검토**:
   - 현재: 공통 Rarity Entity로 통합
   - 향후: 시스템별 독립 등급 필요 시 분리 고려

2. **펫 희귀도 확률 관리 방식** (Q6 확장):
   - Configuration 테이블 vs appsettings.json vs 하드코딩
   - 운영 중 확률 조정 필요성 고려

3. **트랜잭션 관리 전략** (Q7 결정 후):
   - Service Layer 트랜잭션 범위
   - 복잡한 시나리오 대응 방안

---

## 🎯 다음 세션 시작 방법

1. 이 파일(`progress.md`) 읽기
2. Q7부터 이어서 진행
3. Phase 3-6 완료 후 `requirements.md` 최종 생성
