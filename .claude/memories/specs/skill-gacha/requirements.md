# Requirements: Skill Gacha System

> 스킬 랜덤 뽑기(가챠) 시스템의 요구사항을 정의합니다.
> 
> **작성 배경**: 강화 시스템 대신 더 흥미로운 성장 콘텐츠 제공

---

## 📋 Feature Overview

### 목적
플레이어에게 랜덤 스킬 획득을 통한 성장 콘텐츠를 제공하고, 방치형 게임의 핵심 루프(던전 → 보상 → 성장)를 완성한다.

### 성공 기준
- [x] 플레이어가 크리스탈을 사용하여 스킬을 랜덤 획득할 수 있다
- [x] 천장 시스템으로 공정한 확률을 보장한다
- [x] 10연차 기능으로 편의성을 제공한다
- [x] 가챠 히스토리를 통해 투명성을 확보한다
- [x] 패시브/액티브 스킬이 캐릭터 성장에 기여한다

---

## 👤 User Stories

### US-1: 스킬 랜덤 획득
**As a** 플레이어  
**I want** 크리스탈을 사용해 스킬을 랜덤으로 획득하고 싶다  
**So that** 캐릭터를 성장시키고 전투력을 높일 수 있다

**Acceptance Criteria (EARS 형식):**
- **WHEN** 플레이어가 1회 가챠를 요청 **THEN** system **SHALL** 크리스탈 50개를 차감하고 1개의 스킬을 부여
- **WHEN** 플레이어가 10연차를 요청 **THEN** system **SHALL** 크리스탈 500개를 차감하고 10개의 스킬을 부여
- **IF** 크리스탈이 부족 **THEN** system **SHALL** 400 Bad Request 에러를 반환
- **WHEN** 스킬 획득 **THEN** system **SHALL** GachaHistory에 기록을 저장

---

### US-2: 천장 시스템
**As a** 플레이어  
**I want** 일정 횟수 이상 가챠 시 전설 등급을 보장받고 싶다  
**So that** 운이 나빠도 공정하게 성장할 수 있다

**Acceptance Criteria:**
- **WHEN** 플레이어가 100회 가챠 후 전설 등급 미획득 **THEN** system **SHALL** 다음 가챠에서 전설 등급 보장
- **WHEN** 전설 등급 획득 **THEN** system **SHALL** 천장 카운터를 0으로 리셋
- **WHILE** 천장 카운터가 진행 중 **THEN** system **SHALL** Character.PityCount를 증가

---

### US-3: 혼합형 스킬 시스템
**As a** 플레이어  
**I want** 패시브 스킬로 스탯을 영구 증가시키고, 액티브 스킬로 전투에서 특수 효과를 발동하고 싶다  
**So that** 다양한 전략으로 캐릭터를 육성할 수 있다

**Acceptance Criteria:**
- **WHEN** 패시브 스킬 획득 **THEN** system **SHALL** 캐릭터 스탯(공격력/방어력/HP)을 영구 증가
- **WHEN** 액티브 스킬 획득 **THEN** system **SHALL** 전투 중 발동 가능한 스킬로 등록
- **IF** 스킬 슬롯이 가득 참 **THEN** system **SHALL** 400 Bad Request 에러 반환 ("스킬 슬롯이 부족합니다")
- **WHILE** 캐릭터 레벨업 **THEN** system **SHALL** 스킬 슬롯 수를 증가 (Lv1: 4개, Lv10: 5개, Lv20: 6개)

---

### US-4: 가챠 히스토리 조회
**As a** 플레이어  
**I want** 내 가챠 기록을 조회하고 싶다  
**So that** 어떤 스킬을 언제 획득했는지 확인할 수 있다

**Acceptance Criteria:**
- **WHEN** 플레이어가 히스토리 조회 요청 **THEN** system **SHALL** 최근 100개 기록을 반환
- **WHEN** 각 기록 **THEN** system **SHALL** 획득 스킬, 등급, 획득 시각, 천장 카운터 정보 포함

---

## 🎮 Game Design Requirements

### 게임 밸런스

#### 스킬 등급별 확률 ✅ (결정 완료)
- **Common**: 60%
- **Rare**: 30%
- **Epic**: 9%
- **Legendary**: 1%

**검증 방법**: 단위 테스트로 10,000회 시뮬레이션 시 ±1% 오차 내 수렴

#### 천장 시스템 ✅ (결정 완료)
- **천장 횟수**: 100회
- **리셋 조건**: 전설 등급 획득 시 즉시

#### 스킬 슬롯 확장 ✅ (결정 완료)
- **기본 슬롯**: 4개 (Lv1)
- **확장 조건**: Lv10 → 5개, Lv20 → 6개
- **최대 슬롯**: 6개

### 재화/보상

#### 가챠 비용 ✅ (결정 완료)
- **1회 가챠**: 크리스탈 50개
- **10연차**: 크리스탈 500개 (할인 없음, 편의 기능)

#### 크리스탈 획득 경로
- 던전 클리어 보상
- 일일 미션 완료
- (향후) 이벤트, 결제 상품

### 플레이어 경험
- **흥미 요소**: 랜덤 보상의 기대감, 천장 시스템의 안정감
- **성장 실감**: 패시브 스킬로 스탯 증가 시각화
- **전략성**: 한정된 슬롯에 어떤 스킬을 장착할지 선택

---

## 🔗 Dependencies

### 기존 시스템 의존성
- **Character Entity**: PityCount, SkillSlotCount 필드 추가 필요
- **Player Entity**: Crystal 재화 추가 필요
- **Combat System**: 액티브 스킬 발동 연동 (Phase 2에서 구현)

### 새로운 요구사항
- **SkillTemplate Entity**: 스킬 마스터 데이터 (Id, Name, Rarity, SkillType, EffectValue)
- **PlayerSkill Entity**: 플레이어가 소유한 스킬 (CharacterId, SkillTemplateId)
- **GachaHistory Entity**: 가챠 기록 (CharacterId, SkillTemplateId, DrawnAt, PityCount)
- **SkillRarity Enum**: Common, Rare, Epic, Legendary
- **SkillType Enum**: Passive, Active

---

## ⚠️ Non-Functional Requirements

### Performance
- **가챠 API 응답 시간**: 200ms 이내 (DB 조회 1회, 삽입 2~3회)
- **히스토리 조회**: 100개 기록 조회 시 100ms 이내 (인덱스 필요)

### Security
- **인증**: JWT 토큰 필수 (`[Authorize]` 속성)
- **권한**: 본인 캐릭터만 가챠 가능 (CharacterId 소유 검증)
- **트랜잭션**: 크리스탈 차감 + 스킬 추가 + 히스토리 기록을 하나의 트랜잭션으로 처리

### Scalability
- **동시 가챠**: 1,000 req/sec 처리 가능 (EF Core connection pool)
- **데이터 증가**: GachaHistory는 월 100만 건 증가 예상 (인덱스 + 파티셔닝 고려)

---

## ✅ Approval

- [x] Requirements 리뷰 완료 (2025-10-20)
- [x] 게임 디자인 결정 완료 (확률, 천장, 비용)
- [x] Gemini 2.5 Pro 협업 완료 (8가지 핵심 질문 해소)
- [x] Design 단계로 진행 승인

---

**작성일**: 2025-10-20  
**작성자**: IdleRPG Team (Claude + Human + Gemini)  
**상태**: Approved ✅  
**협업 방식**: Multi-AI Collaborator (Gemini 2.5 Pro 상담)
