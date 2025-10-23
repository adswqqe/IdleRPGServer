# Requirements: Pet System

> 이 문서는 펫 시스템 기능의 요구사항을 정의합니다.
>
> **작성 가이드**:
> - 사용자 관점에서 "무엇을" 만들지 정의 (How는 Design에서)
> - EARS 형식으로 검증 가능한 기준 작성
> - 비즈니스 로직 결정이 필요한 부분은 명시

---

## 📋 Feature Overview

### 목적
펫 시스템은 캐릭터에게 추가 스탯 버프를 제공하고, 수집 요소를 통해 게임의 깊이를 더하는 기능입니다. 플레이어는 가챠를 통해 다양한 등급의 펫을 획득하고, 육성을 통해 더 강력한 버프를 얻을 수 있습니다.

### 성공 기준
- [ ] 플레이어가 크리스탈을 사용하여 펫 가챠를 뽑을 수 있음
- [ ] 획득한 펫을 캐릭터에 장착하여 스탯 버프를 받을 수 있음
- [ ] 펫을 레벨업하여 버프 효과를 증가시킬 수 있음
- [ ] 펫 인벤토리에서 보유 중인 펫 목록을 확인할 수 있음
- [ ] 전투 시스템에서 펫 버프가 정상적으로 적용됨

---

## 👤 User Stories

### US-1: 펫 가챠를 통한 펫 획득
**As a** 플레이어
**I want** 크리스탈을 사용하여 펫 가챠를 뽑아 새로운 펫을 획득하고 싶다
**So that** 다양한 펫을 수집하고 캐릭터를 강화할 수 있다

**Acceptance Criteria (EARS 형식):**
- **WHEN** 플레이어가 펫 가챠를 1회 실행할 때 **THEN** system **SHALL** 크리스탈 100개를 차감한다
- **WHEN** 플레이어가 펫 가챠를 10회 실행할 때 **THEN** system **SHALL** 크리스탈 900개를 차감한다 (10% 할인)
- **WHEN** 가챠 실행 시 **THEN** system **SHALL** 서버에서 등급별 확률에 따라 펫을 추첨한다
- **IF** 플레이어의 크리스탈이 부족할 때 **THEN** system **SHALL** 400 Bad Request를 반환한다
- **WHEN** 가챠 결과 펫을 획득할 때 **THEN** system **SHALL** 펫을 캐릭터의 펫 인벤토리에 추가한다
- **WHEN** 플레이어가 동일한 펫을 중복으로 획득할 때 **THEN** system **SHALL** 별도 인스턴스로 저장한다 (향후 강화 재료로 사용 가능)

### US-2: 펫 장착을 통한 스탯 버프
**As a** 플레이어
**I want** 보유 중인 펫을 캐릭터에 장착하여 스탯 버프를 받고 싶다
**So that** 전투력을 향상시킬 수 있다

**Acceptance Criteria (EARS 형식):**
- **WHEN** 플레이어가 펫을 캐릭터에 장착할 때 **THEN** system **SHALL** 해당 펫의 스탯 버프를 캐릭터에 적용한다
- **WHEN** 캐릭터가 이미 펫을 장착한 상태에서 새 펫을 장착할 때 **THEN** system **SHALL** 기존 펫을 해제하고 새 펫을 장착한다
- **WHEN** 플레이어가 펫을 해제할 때 **THEN** system **SHALL** 스탯 버프를 제거한다
- **WHEN** 전투 시작 시 **THEN** system **SHALL** 장착된 펫의 버프를 캐릭터 스탯에 반영한다
- **IF** 플레이어가 다른 캐릭터의 펫을 장착하려고 할 때 **THEN** system **SHALL** 403 Forbidden을 반환한다

### US-3: 펫 육성을 통한 성장
**As a** 플레이어
**I want** 펫을 레벨업하여 더 강력한 버프를 받고 싶다
**So that** 더 높은 난이도의 던전을 클리어할 수 있다

**Acceptance Criteria (EARS 형식):**
- **WHEN** 플레이어가 펫 레벨업을 실행할 때 **THEN** system **SHALL** 골드를 차감하고 펫 경험치를 증가시킨다
- **WHEN** 펫 경험치가 레벨업 요구치에 도달할 때 **THEN** system **SHALL** 펫 레벨을 1 증가시킨다
- **WHEN** 펫이 레벨업할 때 **THEN** system **SHALL** 스탯 버프량을 증가시킨다
- **WHEN** 펫이 최대 레벨에 도달할 때 **THEN** system **SHALL** 레벨업을 제한한다
- **IF** 골드가 부족할 때 **THEN** system **SHALL** 400 Bad Request를 반환한다

### US-4: 펫 인벤토리 관리
**As a** 플레이어
**I want** 보유 중인 펫 목록을 확인하고 관리하고 싶다
**So that** 어떤 펫을 육성하고 장착할지 결정할 수 있다

**Acceptance Criteria (EARS 형식):**
- **WHEN** 플레이어가 펫 목록을 조회할 때 **THEN** system **SHALL** 해당 캐릭터가 보유한 모든 펫을 반환한다
- **WHEN** 펫 목록 조회 시 **THEN** system **SHALL** 펫의 등급, 레벨, 스탯 버프 정보를 포함한다
- **WHEN** 플레이어가 특정 펫 상세 정보를 조회할 때 **THEN** system **SHALL** 펫의 모든 정보와 다음 레벨 요구치를 반환한다

---

## 🎮 Game Design Requirements

<!-- IdleRPG 게임 특화: 밸런스, 보상, 경제 시스템 -->
<!-- 학습 프로젝트이므로 AI가 합리적인 기본값을 제안합니다 -->

### 게임 밸런스 (AI 제안)
> 💡 **학습 프로젝트**: 게임 밸런스 수치는 AI가 제안하며, 학습자는 **아키텍처와 코드 구조**에 집중하세요.
>
> 상세 규칙: [CLAUDE.md - 학습 프로젝트 특화 규칙](../../../CLAUDE.md#학습-프로젝트-특화-규칙)

**가챠 확률 (AI 제안)**:
```
Legendary: 1%    (가장 희귀, 최고 스탯 버프)
Epic:      9%    (희귀, 높은 스탯 버프)
Rare:     30%    (일반적, 중간 스탯 버프)
Common:   60%    (흔함, 기본 스탯 버프)
```

**가챠 비용 (AI 제안)**:
- 1회 가챠: 크리스탈 100개
- 10회 가챠: 크리스탈 900개 (10% 할인)

**펫 레벨업 비용 (AI 제안)**:
```
레벨 1→2: 골드 1,000
레벨 2→3: 골드 2,000
...
공식: 레벨 * 1,000 골드
최대 레벨: 50
```

**펫 스탯 버프 공식 (AI 제안)**:
```
Common:
  - 기본 공격력 +5, 레벨당 +2
  - 기본 방어력 +3, 레벨당 +1

Rare:
  - 기본 공격력 +10, 레벨당 +4
  - 기본 방어력 +6, 레벨당 +2

Epic:
  - 기본 공격력 +20, 레벨당 +8
  - 기본 방어력 +12, 레벨당 +4

Legendary:
  - 기본 공격력 +40, 레벨당 +16
  - 기본 방어력 +24, 레벨당 +8
```

### 재화/보상
- **사용 재화**: 크리스탈 (가챠), 골드 (레벨업)
- **획득 방법**: 던전 첫 클리어, 일일 미션, 유료 구매 (크리스탈)

### 플레이어 경험
- **수집 욕구**: 다양한 등급/종류의 펫 수집
- **육성 깊이**: 펫 레벨업을 통한 장기 목표 제공
- **전략적 선택**: 어떤 펫을 우선 육성할지 고민
- **리텐션**: 매일 접속하여 펫 육성 (골드 획득 필요)

---

## 🎓 학습 포인트 (아키텍처 결정)

**TODO(human)**: 다음 아키텍처 결정이 필요합니다:

### 1. Entity 설계
- [x] **PetTemplate vs CharacterPet 분리**
  - PetTemplate: 마스터 데이터 (등급, 기본 스탯, 이름)
  - CharacterPet: 인스턴스 데이터 (레벨, 경험치, 소유자)
  - 질문: 두 엔티티로 분리할 것인가, 단일 엔티티로 관리할 것인가?
  - **✅ 결정: 두 엔티티로 분리**
    - **이유**: Template-Instance 패턴 (게임 개발 표준)
    - **장점**: 마스터 데이터 중복 방지, 밸런스 패치 용이 (1개 레코드만 UPDATE), 기존 시스템(MonsterTemplate, SkillTemplate)과 일관성
    - **구현**: `PetTemplate` (Id, Name, Rarity, BaseAttack, BaseDefense), `CharacterPet` (Id, PetTemplateId FK, CharacterId FK, Level, Experience)

### 2. 관계 설정
- [x] **Character-Pet 관계**
  - 1:N: 캐릭터는 여러 펫 소유, 1개만 장착
  - M:N: 펫을 여러 캐릭터가 공유? (일반적으로는 1:N)
  - 질문: 1:N 관계로 충분한가?
  - **✅ 결정: 1:N 관계**
    - **이유**: 한 캐릭터가 여러 펫 소유, 펫은 한 캐릭터에만 속함 (일반적인 수집형 게임 패턴)
    - **구현**: `CharacterPet.CharacterId` (FK → Character), `Character.CharacterPets` (Navigation Property)

### 3. 스탯 버프 계산 위치
- [x] **버프 계산 로직 위치**
  - Domain Service: 펫 스탯 계산 로직을 Domain Layer에 배치
  - Application Service: Application Layer에서 계산
  - Entity 메서드: CharacterPet.CalculateBuff() 메서드
  - 질문: Clean Architecture 관점에서 어디에 배치해야 하는가?
  - **✅ 결정: Entity 메서드 (`CharacterPet.CalculateAttackBuff()`, `CalculateDefenseBuff()`)**
    - **이유**: 자신의 속성(Level, Rarity)만으로 계산 가능 → 높은 응집도 (데이터+행동 함께 위치)
    - **장점**: 외부 의존성 없음, 테스트 용이, 객체지향 원칙 준수
    - **원칙**: Entity 혼자 할 수 있으면 Entity 메서드, 여러 Entity 조정 필요 시 Domain Service

### 4. 장착 상태 저장 방식
- [x] **캐릭터-펫 장착 관계**
  - Character.EquippedPetId (FK): Character 엔티티에 FK 추가
  - CharacterPet.IsEquipped (bool): CharacterPet 엔티티에 플래그
  - 별도 중간 테이블: CharacterPetEquipment
  - 질문: 어떤 방식이 더 명확하고 유지보수하기 쉬운가?
  - **✅ 결정: `Character.EquippedPetId` (Nullable FK)**
    - **이유**: 1:1 관계는 FK 하나로 충분, 최소 복잡도, 데이터 정합성 보장 (NULL = 미장착, UUID = 장착)
    - **장점**: 쿼리 간단 (JOIN 불필요), 기존 Equipment 시스템과 일관성 (Equipment.CharacterId와 동일 패턴)
    - **중간 테이블 불필요**: M:N 관계나 추가 메타데이터(장착 시각, 슬롯 번호) 필요 시에만 사용 (YAGNI)

### 5. 가챠 로직 배치
- [x] **가챠 확률 계산**
  - Domain Service: GachaLogicService (스킬 가챠와 유사)
  - Application Service: PetGachaService
  - 질문: 기존 GachaLogicService를 재사용할 것인가, 별도로 만들 것인가?
  - **✅ 결정: `GachaLogicService` 재사용 (펫 전용 메서드 추가)**
    - **이유**: 확률 상수 재사용 (DRY 원칙), Domain Service 위치 유지 (Clean Architecture 준수), 기존 코드 영향 없음
    - **구현**: `DeterminePetRarity()`, `SelectRandomPet(PetRarity, IEnumerable<PetTemplate>)` 메서드 추가
    - **장점**: 스킬/펫 가챠 일관성, IRandomProvider 공유 (테스트 용이), 향후 제네릭 리팩토링 가능

> 💡 **학습 가이드**: 게임 밸런스가 아닌, **Clean Architecture 계층 분리**와 **설계 패턴 적용**에 집중하세요.
>
> 참고 문서:
> - `.claude/memories/_system/architecture.md` - Repository Pattern, Service Pattern
> - `.claude/memories/_system/game-design.md` - 엔티티 관계 예시

---

## 🔗 Dependencies

### 기존 시스템 의존성
- **Character**: 펫의 소유자, 펫 장착 대상
- **Player**: 크리스탈 재화 차감 (가챠 비용)
- **Combat System**: 전투 시 펫 버프 적용

### 새로운 요구사항
- **PetTemplate** (마스터 데이터): 펫 종류, 등급, 기본 스탯
- **CharacterPet** (인스턴스 데이터): 소유한 펫, 레벨, 경험치
- **PetGachaHistory** (선택적): 가챠 이력 로그

---

## ⚠️ Non-Functional Requirements

### Performance
- **API 응답 시간**:
  - GET /api/pets (펫 목록): < 200ms
  - POST /api/pets/gacha (가챠): < 500ms
  - PUT /api/pets/{id}/equip (장착): < 200ms
- **동시 가챠 처리**: 100 TPS (Transactions Per Second)

### Security
- **인증**: 모든 펫 관련 API는 JWT 인증 필수 (`[Authorize]`)
- **권한**: 플레이어는 자신의 캐릭터 펫만 조회/수정 가능
- **서버 권위**: 가챠 확률 계산, 스탯 버프 계산은 **반드시 서버에서 수행**

### Scalability
- **데이터 증가**: 플레이어당 평균 50개 펫 보유 가정
- **인덱스**: CharacterId, PetTemplateId에 인덱스 필요
- **페이징**: 펫 목록 조회 시 페이징 고려 (향후)

---

## ✅ Approval

- [x] Requirements 리뷰 완료
- [x] 아키텍처 학습 포인트 확인 완료 (TODO(human) 해소)
  - #1 Entity 설계: PetTemplate/CharacterPet 분리 ✅
  - #2 관계 설정: Character-Pet 1:N ✅
  - #3 스탯 버프 계산: Entity 메서드 ✅
  - #4 장착 상태: Character.EquippedPetId FK ✅
  - #5 가챠 로직: GachaLogicService 재사용 ✅
- [x] 게임 밸런스 AI 제안값 확인 (가챠 확률, 비용, 레벨업 비용, 스탯 버프 공식)
- [x] Design 단계로 진행 승인

---

**작성일**: 2025-10-23
**작성자**: User + AI
**상태**: Approved
**승인일**: 2025-10-23
