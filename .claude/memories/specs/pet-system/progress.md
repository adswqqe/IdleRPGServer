# Pet System Spec - 진행 상황

## 📋 세션 정보
- **시작일**: 2025-10-24
- **모드**: 학습 모드 (10-15개 질문)
- **진행도**: 7/15 질문 완료 (Phase 2 진행 중)

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

#### Q7: 트랜잭션 경계 설정 (현재 진행 중)
- **질문**: Service Layer vs Repository Layer vs EF Core 자동 트랜잭션
- **옵션**:
  - A: Service Layer에서 명시적 트랜잭ション 관리
  - B: Repository Layer 트랜잭션 (잘못된 설계)
  - C: EF Core SaveChanges 자동 트랜잭션
- **다음 답변 대기 중**

---

## 🔄 다음 진행 예정

### Phase 2 남은 질문 (예상 2-3개)
- Q7 완료 후: Database 인덱싱, N+1 문제 방지

### Phase 3: 데이터베이스 설계 (예상 2-3개)
- 인덱스 전략 (PetTemplates.RarityId, Pets.CharacterId)
- N+1 문제 방지 (Eager Loading vs Lazy Loading)
- PK 타입 (GUID vs INT)

### Phase 4: API 설계 (예상 3개)
- RESTful 엔드포인트 설계
- DTO 구조 (Request/Response)
- 인증/권한 설정

### Phase 5: 게임 밸런스 (AI 자동 제안)
- 가챠 확률 (AI 제안: Legendary 1%, Epic 9%, Rare 30%, Common 60%)
- 가챠 비용 (AI 제안: 크리스탈 100개)
- 천장 (AI 제안: 50회)
- 펫 스탯 버프 공식

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
