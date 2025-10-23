# Requirements: [Feature Name]

> 이 문서는 [Feature Name] 기능의 요구사항을 정의합니다.
> 
> **작성 가이드**: 
> - 사용자 관점에서 "무엇을" 만들지 정의 (How는 Design에서)
> - EARS 형식으로 검증 가능한 기준 작성
> - 비즈니스 로직 결정이 필요한 부분은 명시

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

## 🎮 Game Design Requirements

<!-- IdleRPG 게임 특화: 밸런스, 보상, 경제 시스템 -->
<!-- 학습 프로젝트이므로 AI가 합리적인 기본값을 제안합니다 -->

### 게임 밸런스 (AI 제안)
> **학습 프로젝트 원칙**: 게임 밸런스는 AI가 일반적인 Idle RPG 관례에 따라 제안합니다.
> 학습자는 **아키텍처와 코드 구조**에 집중하세요.

**확률/수치 (AI 제안 예시)**:
- [가챠 확률: Legendary 1%, Epic 9%, Rare 30%, Common 60%]
- [재화 비용: 크리스탈 100개, Gold 1000개]
- [보상량: 경험치 100~500, Gold 50~200]

### 재화/보상
- [어떤 재화를 사용/획득하는지]
- [드랍률, 보상 테이블 (AI가 제안)]

### 플레이어 경험
- [이 기능이 플레이어에게 주는 경험]
- [플레이 타임, 리텐션 목표]

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
