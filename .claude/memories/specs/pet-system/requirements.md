# Requirements: Pet System

> 이 문서는 Pet System 기능의 요구사항을 정의합니다.
>
> **작성 방식**: `/spec-init` 대화형 플로우 (학습 모드, 13개 질문)
> **학습 초점**: 데이터 모델링, Clean Architecture 계층 분리, DB 최적화, 게임 API 설계

**대화로 결정한 내용**:
```
Phase 1: 데이터 모델링 (Pet-Character 1:N, PetTemplate 정규화, 장착 슬롯 시스템, 공통 Rarity Entity)
Phase 2: 아키텍처 계층 (가챠 로직 Domain Service, 천장 카운터 Value Object, Unit of Work 트랜잭션)
Phase 3: 데이터베이스 설계 (CharacterId 인덱스, Include + AsNoTracking, Pet PK는 INT)
Phase 4: API 설계 (Action 중심 RPC, 풍부한 DTO, 읽기 Public/쓰기 인증)
Phase 5: 게임 밸런스 (AI 제안: Legendary 1%, 천장 50회, 크리스탈 100개)
```

---

## 📋 Feature Overview

### 목적
캐릭터가 **펫(Pet)**을 가챠로 획득하고, 육성하고, 장착하여 **전투력을 강화**하는 컬렉션 시스템을 제공합니다. 방치형 게임의 핵심 재화 소모처이자 과금 유도 메커니즘으로 작동합니다.

### 성공 기준
- [x] 캐릭터가 크리스탈로 펫 가챠를 수행할 수 있다
- [x] 펫을 레벨업하여 스탯을 강화할 수 있다
- [x] 펫을 장착(최대 3마리)하여 캐릭터 스탯에 버프를 제공한다
- [x] 천장 시스템(50회)으로 Legendary 펫을 보장한다
- [x] 중복 펫 획득 시 골드 보상으로 전환한다

---

## 👤 User Stories

### US-1: 펫 가챠로 새로운 펫 획득
**As a** 플레이어
**I want** 크리스탈을 소모하여 펫 가챠를 수행
**So that** 다양한 희귀도의 펫을 수집하고 전투력을 강화할 수 있다

**Acceptance Criteria (EARS 형식):**
- **WHEN** 캐릭터가 크리스탈 100개를 소모하여 가챠 수행 **THEN** system **SHALL** 확률에 따라 펫을 지급하고 천장 카운터를 1 증가
- **IF** 이미 보유한 펫을 획득 **THEN** system **SHALL** 희귀도별 골드 보상으로 전환 (Common 100, Legendary 10,000)
- **WHILE** 천장 카운터가 50회 도달 **THEN** system **SHALL** 100% Legendary 펫 지급 및 카운터 초기화

### US-2: 펫 육성 및 스탯 강화
**As a** 플레이어
**I want** 골드를 소모하여 펫을 레벨업
**So that** 펫의 공격력/마나를 증가시켜 캐릭터 버프를 강화할 수 있다

**Acceptance Criteria:**
- **WHEN** 골드를 소모하여 펫 레벨업 **THEN** system **SHALL** 펫 스탯 증가 (Lv당 공격력 +10, 마나 +5)
- **IF** 최대 레벨(Lv50) 도달 **THEN** system **SHALL** 레벨업 불가 메시지 반환

### US-3: 펫 장착으로 캐릭터 버프 적용
**As a** 플레이어
**I want** 펫을 슬롯(최대 3개)에 장착
**So that** 펫 스탯의 10%가 캐릭터에 버프로 적용된다

**Acceptance Criteria:**
- **WHEN** 펫을 슬롯 1-3에 장착 **THEN** system **SHALL** 캐릭터 공격력/마나에 펫 스탯의 10% 추가
- **IF** 슬롯에 이미 다른 펫 장착 **THEN** system **SHALL** 기존 펫을 인벤토리로 이동 후 새 펫 장착
- **WHILE** 펫 장착 중 **THEN** system **SHALL** 해당 펫을 다른 슬롯이나 삭제 작업에서 제외

---

## 🏗️ Technical Requirements (대화로 결정)

### 데이터 모델 (Data Model)

**엔티티 관계** (Phase 1 결정):
- **Pet (1:N) Character**: 한 캐릭터가 여러 펫 소유 (`Pet.CharacterId` FK)
- **Pet (N:1) PetTemplate**: 펫 마스터 데이터 정규화 (`Pet.TemplateId` FK)
- **Pet (N:1) Rarity**: 공통 Rarity Entity 재사용 (Shared Kernel 패턴)
- **EquippedPets (M:N with constraints)**: Character ↔ Pet 장착 관계
  - PK: (CharacterId, SlotIndex)
  - UNIQUE: (PetId) - 한 펫은 하나의 슬롯에만 장착
  - CHECK: SlotIndex BETWEEN 1 AND 3

**Cascade 규칙**:
- **ON DELETE CASCADE (DB 레벨)**: Character 삭제 시 소유 Pet 자동 삭제
- **이유**: 성능, 일관성, 단순한 소유 관계 (별도 로깅 불필요)

**새 Entity**:
- `Pet`: 캐릭터가 소유한 펫 인스턴스 (Id, CharacterId, TemplateId, Level, CurrentAttack, CurrentMana)
- `PetTemplate`: 펫 마스터 데이터 (Id, Name, RarityId, BaseAttack, BaseMana, ImageUrl)
- `EquippedPets`: 펫 장착 정보 (CharacterId, PetId, SlotIndex, EquippedAt)

---

### 아키텍처 계층 (Architecture Layers)

**로직 배치** (Phase 2 결정):
- **가챠 확률 계산**: `PetGachaService` (Domain Layer)
  - 순수 비즈니스 로직, 외부 의존성 없음, 테스트 용이
- **천장 카운터**: `PetGachaCounter` (Value Object, Domain Layer)
  - Immutable, 캡슐화 (Soft Pity 확률 계산 포함)
  - `Character.PetGachaCount` int 필드 추가
- **펫 장착 로직**: `PetService` (Application Layer)
  - 슬롯 검증, 기존 펫 해제, EquippedPets 업데이트

**트랜잭션 경계**:
- **Unit of Work 패턴 (EF Core SaveChanges 자동 트랜잭션)**
  - Service Layer에서 여러 Repository 호출 후 `_unitOfWork.SaveChangesAsync()`
  - 단순 플로우는 자동 트랜잭션으로 충분 (크리스탈 차감 + 펫 생성 + 카운터 증가)

---

### 데이터베이스 설계 (Database Design)

**인덱스 요구사항** (Phase 3 결정):
- `CREATE INDEX idx_pets_character_id ON pets(character_id)`
  - 가장 빈번한 쿼리: "내 캐릭터의 펫 목록" (99% 사용 케이스)
  - RarityId, TemplateId 필터는 클라이언트에서 처리 (50-200마리 수준)
  - 추가 인덱스 고려 시점: 펫 개수 500+ 초과 시 성능 측정

**N+1 문제 방지**:
- **일반 CRUD**: `Include(p => p.Template)` (Entity 추적 필요)
- **읽기 전용 API**: `Include().AsNoTracking().Select(DTO)` (로그인 시 펫 목록)
  - AsNoTracking()으로 메모리 30-40% 절약, 쿼리 10-20% 향상

**PK 타입**:
- **Pet**: `int` (AUTO_INCREMENT) - 종속 Entity, Equipment와 일관성
- **PetTemplate**: `int` - 마스터 데이터
- **EquippedPets**: Composite PK (CharacterId, SlotIndex)

---

### API 설계 (API Design)

**RESTful 엔드포인트** (Phase 4 결정 - Action 중심 RPC 스타일):
```
POST   /api/pets/gacha                    # 펫 가챠
GET    /api/pets?characterId={id}         # 펫 목록 조회
GET    /api/pets/{petId}                  # 펫 상세 조회
POST   /api/pets/{petId}/level-up         # 펫 레벨업
POST   /api/pets/equip                    # 펫 장착
POST   /api/pets/unequip                  # 펫 해제
GET    /api/pets/equipped?characterId={id} # 장착된 펫 조회
DELETE /api/pets/{petId}                  # 펫 삭제
```

**DTO 구조** (게임 산업 표준 - 풍부한 정보):
```csharp
// 가챠 Response
public class PetGachaResponseDto
{
    public PetDto Pet { get; set; }
    public bool IsDuplicate { get; set; }
    public int CurrentPityCount { get; set; }      // 천장 카운터 (0-50)
    public int RemainingCrystal { get; set; }      // 가챠 후 남은 크리스탈
    public DuplicateRewardDto? DuplicateReward { get; set; }
}

// Pet DTO
public class PetDto
{
    public int Id { get; set; }
    public string TemplateName { get; set; }
    public string RarityName { get; set; }
    public int Level { get; set; }
    public int CurrentAttack { get; set; }
    public int CurrentMana { get; set; }
    public string ImageUrl { get; set; }
}
```

**인증/권한** (Phase 4 결정):
- **읽기 API**: Public (리더보드, PvP 대비)
  - `GET /api/pets`, `GET /api/pets/equipped`
- **쓰기 API**: `[Authorize]` + 소유권 검증
  - `POST /api/pets/gacha`, `POST /api/pets/equip`, `POST /api/pets/{id}/level-up`
  - JWT에서 PlayerId 추출 → Character.PlayerId 검증
- **민감 정보**: `[Authorize]` 필수
  - `GET /api/pets/gacha-history` (자신의 가챠 이력만)

---

### 게임 밸런스 (AI 자동 제안)

> 💡 **학습 프로젝트**: 게임 밸런스는 AI가 제안합니다. 학습자는 **아키텍처와 DB 설계**에 집중하세요.

**가챠 확률**:
- Common: 60%, Rare: 30%, Epic: 9%, Legendary: 1%

**천장 시스템**:
- Hard Pity: 50회 (100% Legendary)
- Soft Pity: 40회부터 확률 증가 (40회 +1%, 45회 +3%)

**가챠 비용**:
- 1회: 크리스탈 100개
- 10연차: 크리스탈 900개 (10% 할인)

**중복 보상**:
- Common: 골드 100, Rare: 500, Epic: 2,000, Legendary: 10,000

**레벨업 비용**:
- 공식: 이전 레벨 비용 × 1.5
- 예시: Lv1→2 (100골드), Lv2→3 (150골드), Max Lv50

**펫 스탯 공식**:
```csharp
// 레벨업 시 펫 스탯
Pet.CurrentAttack = Template.BaseAttack + (Level - 1) * 10
Pet.CurrentMana = Template.BaseMana + (Level - 1) * 5

// 장착 시 캐릭터 버프
CharacterAttack += Pet.CurrentAttack * 0.1
CharacterMana += Pet.CurrentMana * 0.1
```

---

## 🎓 학습 포인트 (아키텍처 결정)

**TODO(human)**: 다음 아키텍처 결정이 필요합니다 (Design 단계에서 구현):

1. **[ ] 펫 스탯 버프 계산 시점**
   - 옵션 A: Character 조회 시마다 실시간 계산 (계산 속성)
   - 옵션 B: 펫 장착/해제 시 캐릭터 스탯 업데이트 (캐싱)
   - 학습 포인트: 읽기 성능 vs 데이터 정합성 트레이드오프

2. **[ ] 가챠 확률 설정 저장 위치**
   - 옵션 A: appsettings.json (간단, 배포 필요)
   - 옵션 B: GachaConfig 테이블 (운영 중 조정 가능)
   - 옵션 C: Domain 하드코딩 (const, 변경 없음)
   - 학습 포인트: Configuration-driven Design

3. **[ ] 가챠 이력 저장 여부**
   - 옵션 A: GachaHistory Entity (이력 추적, 통계)
   - 옵션 B: 저장 안 함 (최소 구현)
   - 학습 포인트: 감사(Audit) 로그, GDPR 대응

> 💡 **학습 가이드**: 게임 밸런스가 아닌, **Clean Architecture 계층 분리**와 **DDD 패턴 적용**에 집중하세요.

---

## 🔗 Dependencies

### 기존 시스템 의존성
- **Character Entity**: PetGachaCount 필드 추가 (int, default 0)
- **Character Entity**: Crystal 필드 (가챠 비용 차감)
- **Rarity Entity**: 공통 Rarity 재사용 (Shared Kernel)

### 새로운 요구사항
- **Pet Entity**: 펫 인스턴스 (CharacterId, TemplateId, Level, CurrentAttack, CurrentMana)
- **PetTemplate Entity**: 펫 마스터 데이터 (Name, RarityId, BaseAttack, BaseMana, ImageUrl)
- **EquippedPets Entity**: 펫 장착 정보 (CharacterId, PetId, SlotIndex)

### External Dependencies (별도 Spec 필요)
- **API Security Refactoring** (S 사이즈)
  - 현재: 모든 API `[Authorize]` (DungeonController 패턴)
  - 변경 후: 읽기 Public, 쓰기 인증 분리
  - 영향 범위: DungeonController, EquipmentController, CharacterController
  - 우선순위: Pet System 작업 전 완료 권장

---

## ⚠️ Non-Functional Requirements

### Performance
- 펫 목록 조회 (100마리): < 200ms (AsNoTracking + Projection)
- 가챠 API: < 300ms (확률 계산 + DB 트랜잭션)
- 펫 장착/해제: < 150ms (단순 UPDATE)

### Security
- JWT 인증: 쓰기 API 필수 (`[Authorize]`)
- 소유권 검증: Character.PlayerId == JWT PlayerId
- 읽기 API Public: 게임 데이터는 공개, 민감 정보 제외

### Scalability
- 캐릭터당 펫 보유 상한: 500마리 (초과 시 판매/삭제 유도)
- 가챠 동시 요청: 1000 req/s (DB 커넥션 풀 50)

---

## ✅ Approval

- [x] Requirements 리뷰 완료 (13개 질문 대화 완료)
- [ ] 아키텍처 학습 포인트 확인 완료 (TODO(human) 3개)
- [x] 게임 밸런스 AI 제안값 확인
- [ ] Design 단계로 진행 승인

---

**작성일**: 2025-10-25
**작성자**: AI (Claude) + User (대화형 결정)
**상태**: Draft → Design 준비 완료
