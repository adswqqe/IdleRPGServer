# Requirements: [Feature Name]

> 이 문서는 [Feature Name] 기능의 요구사항을 정의합니다.
>
> **작성 가이드 (학습 프로젝트 특화)**:
> - ✅ **백엔드 학습 중심**: 데이터 모델링, 아키텍처 계층, DB 설계를 **대화로** 결정
> - ✅ **기술적 의사결정**: 1:N vs M:N, Domain vs Application Service, 인덱스 전략 등
> - ❌ **게임 기획 최소화**: 재미, 밸런스는 AI 자동 제안
> - 참고: [/spec-init 대화형 플로우](../../commands/spec-init.md#대화형-requirements-작성)

**대화로 결정한 내용 (L 사이즈)**:
```
Phase 1: 데이터 모델링 (엔티티 관계, Cascade 규칙, 마스터 데이터)
Phase 2: 아키텍처 계층 (로직 배치, Value Object, 트랜잭션)
Phase 3: 데이터베이스 설계 (인덱스, N+1 방지, PK 타입)
Phase 4: API 설계 (RESTful 경로, DTO 구조, 인증)
Phase 5: 게임 밸런스 (AI 제안, 확인만)
```

---

## 📋 Feature Overview

### 목적
<!-- 이 기능이 필요한 이유, 해결하려는 문제 -->

### 성공 기준
<!-- 이 기능이 완료되었다고 판단할 수 있는 기준 -->
- [ ] ...
- [ ] ...

---

## 👤 User Stories

### US-1: [주요 사용자 스토리]
**As a** [역할]  
**I want** [기능]  
**So that** [목적/가치]

**Acceptance Criteria (EARS 형식):**
- **WHEN** [조건] **THEN** system **SHALL** [동작]
- **IF** [상황] **THEN** system **SHALL** [결과]
- **WHILE** [진행 중] system **SHALL** [보장사항]

### US-2: [추가 스토리]
...

---

## 🏗️ Technical Requirements (대화로 결정)

> ⚠️ **백엔드 학습 초점**: 이 섹션은 `/spec-init` 대화 과정에서 결정됩니다

### 데이터 모델 (Data Model)
**엔티티 관계** (대화로 결정):
- [예: Pet (1:N) Character - 한 캐릭터가 여러 펫 소유]
- [예: PetTemplates 별도 테이블 - 정규화, 확장성]

**Cascade 규칙**:
- [예: Application 처리 - 명시적 삭제, 로깅 가능]

**새 Entity**:
- `Entity1`: [목적], [주요 필드]
- `Entity2`: [목적], [주요 필드]

---

### 아키텍처 계층 (Architecture Layers)
**로직 배치** (대화로 결정):
- [예: 가챠 확률 계산 → Domain Service (순수 비즈니스 로직)]
- [예: 천장 카운터 → Value Object (GachaCounter, 캡슐화)]

**트랜잭션 경계**:
- [예: Service Layer - 여러 Repository 호출 묶음]

---

### 데이터베이스 설계 (Database Design)
**인덱스 요구사항** (대화로 결정):
- [예: PetTemplates.Rarity - 희귀도별 필터링 빈번]
- [예: 복합 인덱스 (Entity.Field1 + Field2) - 조합 쿼리]

**N+1 문제 방지**:
- [예: Eager Loading - Include(p => p.Template)]

**PK 타입**:
- [예: uuid - 분산 환경, 충돌 없음]

---

### API 설계 (API Design)
**RESTful 엔드포인트** (대화로 결정):
- [예: POST /api/pets/gacha - Resource 중심]

**DTO 구조**:
- [예: GachaResultDto - 펫 + 메타정보 (isDuplicate, pityCount)]

**인증/권한**:
- [예: JWT Bearer Token - 모든 API]

---

### 게임 밸런스 (AI 자동 제안)
> 💡 **학습 프로젝트**: 게임 밸런스는 AI가 제안합니다. 학습자는 **아키텍처와 DB 설계**에 집중하세요.

**확률/수치 (AI 제안)**:
- [가챠 확률: Legendary 1%, Epic 9%, Rare 30%, Common 60%]
- [재화 비용: 크리스탈 100개]
- [천장: 50회]

---

## 🎓 학습 포인트 (아키텍처 결정)

**TODO(human)**: 다음 아키텍처 결정이 필요합니다:
- [ ] [예: "가챠 결과 저장: Character Entity에 직접? 별도 GachaHistory Entity?"]
- [ ] [예: "천장 카운터 관리: Entity 속성? Value Object?"]
- [ ] [예: "확률 계산 로직: Domain Service? Application Service?"]

> 💡 **학습 가이드**: 게임 밸런스가 아닌, **Clean Architecture 계층 분리**와 **설계 패턴 적용**에 집중하세요.

---

## 🔗 Dependencies

### 기존 시스템 의존성
- [이 기능이 의존하는 기존 엔티티/서비스]

### 새로운 요구사항
- [새로 만들어야 할 엔티티/테이블]

---

## ⚠️ Non-Functional Requirements

### Performance
- [응답 시간, 처리량 요구사항]

### Security
- [인증/인가, 데이터 보호 요구사항]

### Scalability
- [동시 접속자, 데이터 증가 대응]

---

## ✅ Approval

- [ ] Requirements 리뷰 완료
- [ ] 아키텍처 학습 포인트 확인 완료 (TODO(human) 해소)
- [ ] 게임 밸런스 AI 제안값 확인
- [ ] Design 단계로 진행 승인

---

**작성일**: YYYY-MM-DD  
**작성자**: [이름]  
**상태**: Draft / Approved
