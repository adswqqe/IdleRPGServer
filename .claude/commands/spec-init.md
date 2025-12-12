# /spec-init - 새로운 Feature Spec 초기화 (사이즈 자동 판단)

**목적**: 사용자의 기능 요청을 분석하여 적절한 Spec 사이즈(S/M/L)를 판단하고, 승인 후 문서를 생성합니다.

---

## 🎯 사이즈 자동 판단 워크플로우 (사용자 자연어 요청 시)

**트리거**: 사용자가 자연어로 기능 요청 (예: "펫 시스템 추가해줘", "캐릭터 이름 변경 기능")

### Step -1: 진행 중인 세션 체크 (Resume Detection)

**목적**: 덮어쓰기 방지 및 세션 이어하기 지원

#### -1.1. progress.md 존재 확인
```bash
.claude/memories/specs/{feature-name}/progress.md
```

#### -1.2. 존재하면 사용자에게 선택지 제공
```markdown
📋 **진행 중인 세션 발견!**

**Feature**: {feature-name}
**진행도**: Phase 2, 7/15 질문 완료
**마지막 질문**: Q7 - 트랜잭션 경계 설정 (답변 대기 중)
**학습 모드**: 활성
**시작일**: 2025-10-24

**다음 작업**:
- Q7부터 이어서 진행
- Phase 3-6 완료 후 requirements.md 생성

---

**어떻게 하시겠습니까?**

[C] **이어하기 (Continue)** - progress.md 읽고 Q7부터 재개
[N] **새로 시작 (New)** - progress.md를 백업하고 처음부터 다시
[X] **취소 (Cancel)** - 아무 작업도 하지 않음

```

#### -1.3. 사용자 선택에 따른 분기
- **[C] 이어하기**:
  1. `progress.md` 파일 읽기
  2. 마지막 완료된 질문 확인 (예: Q6까지 완료)
  3. 다음 질문(Q7)부터 재개
  4. 기존 설계 결정 사항 유지

- **[N] 새로 시작**:
  1. `progress.md` → `progress-backup-{timestamp}.md`로 백업
  2. 기존 폴더 내용 보존 (덮어쓰지 않음)
  3. 사용자에게 경고: "기존 진행 내용이 있습니다. 정말 새로 시작하시겠습니까? [Y/n]"
  4. Y 선택 시 Step 0부터 진행

- **[X] 취소**:
  - 커맨드 종료

#### -1.4. progress.md 없으면
- Step 0으로 이동 (정상 워크플로우)

---

### Step 0: 사이즈 판단 및 사용자 승인

#### 0-1. 기능 분석
다음 항목을 분석하여 사이즈 판단:
- 새 Entity 개수 예상
- API 엔드포인트 개수 예상
- 새 기술 도입 여부 (SignalR, Redis 등)
- 기존 시스템과의 통합 복잡도
- 비즈니스 로직 복잡도

#### 0-2. 리스크 기반 사이즈 판단 (3차원 점수 시스템)

AI는 **복잡도 + 리스크**를 함께 고려하여 사이즈 판단:

##### 1단계: 기본 복잡도 판단
| 사이즈 | Entity | 엔드포인트 | 비즈니스 로직 | 새 기술 |
|--------|--------|------------|---------------|---------|
| **S** | 0개 (수정만) | 1개 | 없음 (단순 CRUD) | 없음 |
| **M** | 1개 이하 | 2-3개 | 간단한 계산/검증 | 없음 |
| **L** | 2개 이상 | 4개 이상 | 복잡한 가챠/매칭/레이드 | 있음 |

##### 2단계: 리스크 점수 계산 (Blast Radius 시스템)

**3가지 리스크 차원** (각 1-5점):

**① Blast Radius (영향 범위)**:
- **1점**: 단일 기능 (별명, 아바타 이미지)
- **2점**: 관련 기능 그룹 (캐릭터 프로필 전체)
- **3점**: 시스템 일부 (전투 시스템 → 스킬, 아이템 영향)
- **4점**: 여러 시스템 (경제 시스템 → 인벤토리, 상점, 거래)
- **5점**: 시스템 전체 (결제, 인증, 데이터베이스)

**② Novelty (새로움/불확실성)**:
- **1점**: 기존 패턴 복사 (CRUD 반복)
- **2점**: 기존 코드 약간 수정
- **3점**: 기존 기술 새 조합 (EF Core + Caching)
- **4점**: 프로젝트 첫 도입 (SignalR, Redis)
- **5점**: 팀 경험 없는 기술 (gRPC, Kafka)

**③ Unknowns (요구사항 불확실성)**:
- **1점**: 요구사항 명확, 구현 경험 있음
- **2점**: 요구사항 명확, 구현 경험 없음
- **3점**: 성능 요구사항 불확실 (테스트 필요)
- **4점**: 비즈니스 로직 불확실 (협의 필요)
- **5점**: 기술 실현 가능성 불확실 (Spike 필수)

##### 3단계: 최종 사이즈 결정

**합산 점수 (3-15점)**:
- **3-5점**: S 사이즈 (저위험)
- **6-9점**: M 사이즈 (중위험)
- **10-15점**: L 사이즈 (고위험)

**강제 승격 규칙**:
- ⚠️ **어느 하나라도 5점 → 무조건 L 사이즈**
- ⚠️ **자동 승격 트리거 감지 → 상위 사이즈** (CLAUDE.md 참조)

#### 0-3. 사용자에게 판단 결과 제시 (리스크 점수 포함)
```markdown
📏 이 기능은 **[L] (Large)** 사이즈로 판단됩니다.

**복잡도 분석**:
- 새 Entity: 3개 (Pet, PetTemplate, CharacterPet)
- API 엔드포인트: 5개 (POST /gacha, GET /pets, PUT /level-up, ...)
- 비즈니스 로직: 복잡함 (가챠 확률, 천장 시스템, 스탯 버프 계산)
- 새 기술 도입: 없음
- 기존 시스템 통합: 필요 (전투 시스템과 스탯 연동)

**리스크 점수** (3차원 분석):
- 📊 Blast Radius: **3점** (전투 시스템 영향)
- 📊 Novelty: **2점** (기존 패턴 활용)
- 📊 Unknowns: **4점** (가챠 밸런스 불확실)
- 🎯 **합산: 9점** → M 사이즈 기준

**최종 판단: L 사이즈**
- 사유: 복잡도(5개 엔드포인트, 3개 Entity)가 L 기준 충족
- 리스크 점수(9점)는 M이지만, 복잡도 우선

**권장 워크플로우**: Full Spec (requirements + design + tasks)

이대로 진행하시겠습니까?
[Y] **L (Large)** 사이즈로 진행
[M] **M (Medium)** 사이즈로 간소화 (리스크 인지 필요)
[S] **S (Small)** 사이즈로 최소화 (권장하지 않음)
[N] 취소
```

#### 0-3.5. 리스크 점수 예시 (학습용)

**예시 1: 캐릭터 별명 추가 (S 사이즈)**
```
복잡도: Entity 0개, 엔드포인트 1개 → S 후보

리스크 점수:
- Blast Radius: 1점 (별명 기능만 영향)
- Novelty: 1점 (기존 CRUD 패턴)
- Unknowns: 1점 (요구사항 명확)
→ 합산: 3점 → S 사이즈 ✅
```

**예시 2: 결제 시스템 통합 (L 사이즈)**
```
복잡도: Entity 2개, 엔드포인트 3개 → M 후보

리스크 점수:
- Blast Radius: 5점 (전체 매출 영향)
- Novelty: 4점 (PG사 SDK 첫 도입)
- Unknowns: 4점 (환불 정책 불확실)
→ 합산: 13점 → L 사이즈 ⚠️
→ Blast Radius 5점 → 강제 L 사이즈 ✅
```

**예시 3: Redis 캐싱 추가 (자동 승격: S → L)**
```
초기 판단: Entity 0개, 엔드포인트 1개 → S

자동 승격 트리거 감지:
⚠️ Redis 첫 도입 (새 기술)

→ 자동 승격: S → L ✅
사유: Spike 필요, ADR 필수, 장애 처리
```

#### 0-4. 사용자 승인 시 분기
- **Y (L 사이즈)**: 아래 "Step 1" 계속 진행 (requirements.md 생성)
- **M (M 사이즈)**: `spec-init-lite` 커맨드 로직 실행 (spec-lite.md 생성)
- **S (S 사이즈)**: 문서 없이 바로 코드 구현
- **N**: 취소

---

## 실행 절차 (L 사이즈 승인된 경우)

### 1. Feature 이름 생성
- 사용자 요청에서 자동 생성: `{feature-name}` (kebab-case)
- 예시: "펫 시스템" → `pet-system`, "스킬 장착" → `skill-equip`

### 2. 폴더 및 파일 생성
```
.claude/memories/specs/{feature-name}/
└── requirements.md
```

### 3. 시스템 문서 참조
다음 문서들을 읽어서 컨텍스트 파악:
- `.claude/memories/_system/architecture.md` - Clean Architecture 패턴
- `.claude/memories/_system/game-design.md` - 게임 시스템 우선순위
- `.claude/memories/_system/tech-stack.md` - 사용 가능한 기술 스택
- `.claude/memories/_system/api-standards.md` - 코딩 컨벤션

### 4. 대화형 Requirements 작성 (백엔드 학습 중심, L 사이즈)

> ⚠️ **학습 프로젝트 특화**: 게임 기획이 아닌 **백엔드 아키텍처, 데이터 모델링, 기술 선택**을 대화로 진행
> 목표: 프론트엔드 개발자가 백엔드 설계를 경험하며 학습

**대화 플로우** (기술/아키텍처 중심):

#### Phase 0: 경험 수준 선택 (유연성 확보)
```
AI: "🎓 경험 수준을 선택하세요."

- [ ] **학습 모드** (권장: 백엔드 학습 중)
  - 10-15개 대화형 질문으로 아키텍처 의사결정 학습
  - 각 선택마다 트레이드오프 설명 제공
  - 예상 시간: 30-40분

- [ ] **표준 모드** (숙련자용)
  - 5-7개 핵심 질문만 (데이터 모델, API, 보안)
  - 트레이드오프 설명 간소화
  - 예상 시간: 15-20분

- [ ] **Fast Track** (전문가용)
  - 대화형 질문 건너뛰기
  - 템플릿 직접 작성 (requirements-template.md 기반)
  - 예상 시간: 5-10분

사용자 선택 → 선택한 모드로 진행
```

> 💡 **Tip**: 첫 번째 기능은 **학습 모드** 권장. 패턴이 익숙해지면 **표준 모드** 사용.

#### Phase 1: 데이터 모델링 (Data Modeling)
> **질문 개수**: 학습 모드 3-4개 | 표준 모드 2개 | Fast Track 0개

```
AI: "📊 [Feature] 데이터 모델부터 설계하겠습니다."

AI: "주요 엔티티 관계를 정의해주세요."

예시 (Pet System):
Q1: "펫과 캐릭터의 관계는?"
  - [ ] 1:N (한 캐릭터가 여러 펫 소유, FK: Pet.CharacterId)
  - [ ] M:N (여러 캐릭터가 펫 공유 가능, Junction Table 필요)
  - [ ] 1:1 (한 캐릭터 한 펫, 제한적)

사용자 선택 → AI: 트레이드오프 설명

Q2: "Cascade Delete 규칙은?"
  - [ ] ON DELETE CASCADE (DB 레벨, 자동 삭제)
  - [ ] Application 처리 (Service에서 명시적 삭제, 로깅 가능)

사용자 선택 → Dependencies 섹션에 기록

Q3: "펫 마스터 데이터는?"
  - [ ] PetTemplates 별도 테이블 (정규화, 확장성 ↑)
  - [ ] Enum (간단, 하드코딩)

사용자 선택 → 새 Entity 추가
```

#### Phase 2: 아키텍처 계층 결정 (Architecture Layers)
> **질문 개수**: 학습 모드 3개 | 표준 모드 1-2개 | Fast Track 0개

```
AI: "🏗️ 비즈니스 로직 배치를 결정하겠습니다."

예시:
Q1: "가챠 확률 계산 로직은 어디에?"
  - [ ] Domain Service (순수 비즈니스 로직, 테스트 용이)
  - [ ] Application Service (외부 의존성 있을 때)

사용자 선택 → AI: "Domain Service 선택 시 IGachaService 인터페이스 필요"

Q2: "천장 카운터 관리는?"
  - [ ] Character Entity 속성 (GachaCount int)
  - [ ] Value Object (GachaCounter, 캡슐화)
  - [ ] 별도 GachaHistory Entity (이력 추적)

사용자 선택 → Data Model 섹션 업데이트

Q3: "트랜잭션 경계는?"
  - [ ] Service Layer (여러 Repository 호출 묶음)
  - [ ] Repository Layer (단일 작업만)

사용자 선택 → TODO(human) 마커 추가
```

#### Phase 3: 데이터베이스 설계 (Database Design)
> **질문 개수**: 학습 모드 3개 | 표준 모드 1개 | Fast Track 0개

```
AI: "💾 데이터베이스 최적화를 고려합니다."

Q1: "PetTemplates 조회 쿼리가 빈번할 텐데, 인덱스는?"
  - [ ] Rarity (희귀도별 필터링)
  - [ ] Name (이름 검색)
  - [ ] 복합 인덱스 (Rarity + Name)

사용자 선택 → Non-Functional Requirements에 기록

Q2: "Pet 목록 조회 시 N+1 문제 방지는?"
  - [ ] Eager Loading (Include(p => p.Template))
  - [ ] Lazy Loading (필요 시 로드)
  - [ ] Projection (Select로 DTO 직접 매핑)

사용자 선택 → Performance 섹션

Q3: "PK 타입은?"
  - [ ] uuid (분산 환경, 충돌 없음)
  - [ ] serial/int (단순, 성능 ↑)

사용자 선택 → Data Model 섹션
```

#### Phase 4: API 설계 (API Design)
> **질문 개수**: 학습 모드 3개 | 표준 모드 1-2개 | Fast Track 0개

```
AI: "🔌 RESTful API를 설계합니다."

Q1: "가챠 API는?"
  - [ ] POST /api/pets/gacha (Resource 중심)
  - [ ] POST /api/gacha/execute (Action 중심)

사용자 선택 → AI: "RESTful 원칙 설명"

Q2: "가챠 결과 응답은?"
  - [ ] PetDto (펫 정보만)
  - [ ] GachaResultDto (펫 + 메타정보: isDuplicate, pityCount)

사용자 선택 → API Design 섹션

Q3: "인증/권한은?"
  - [ ] JWT Bearer Token (모든 API)
  - [ ] 특정 API만 (읽기는 Public)

사용자 선택 → Security 섹션
```

#### Phase 5: 게임 밸런스 (AI 자동 제안, 확인만)
```
AI: "⚖️ 게임 밸런스는 AI가 제안합니다 (학습 초점은 아키텍처)"

AI 자동 제안:
- 가챠 확률: Legendary 1%, Epic 9%, Rare 30%, Common 60%
- 가챠 비용: 크리스탈 100개
- 천장: 50회

이대로 진행하시겠습니까? [Y/수정]
→ Y면 그대로, 수정 요청 시만 조정
```

#### Phase 6: 최종 문서 생성
```
AI: "✅ requirements.md 생성 완료"

대화로 결정된 내용:
✅ Data Model: Pet (1:N) Character, PetTemplates (별도 테이블)
✅ Architecture: 가챠 로직 = Domain Service, 트랜잭션 = Service Layer
✅ Database: Rarity 인덱스, Eager Loading, uuid PK
✅ API: POST /api/pets/gacha, JWT 인증, GachaResultDto
✅ Game Balance: AI 제안 (Legendary 1%, 천장 50회)

확인하시겠습니까? [Y/수정]
```

**대화 특징**:
- 각 질문마다 **트레이드오프 설명** (왜 1:N인가? uuid vs int 차이는?)
- 사용자 선택 → AI가 **학습 포인트** 제공
- 최대 10-15개 질문 (Phase당 3-4개)
- "Skip" 입력 시 AI 자동 작성

### 5. 출력
```
✅ {feature-name} spec initialized

Created:
- .claude/memories/specs/{feature-name}/requirements.md

Next steps:
1. Review requirements.md
2. Decide TODO(human) 아키텍처 학습 포인트 (계층 분리, 설계 패턴)
3. 게임 밸런스는 AI 제안값 확인
4. Check approval section
5. [조건부] Spike 제안 받기 (Requirements 승인 후)
6. Run: /spec-design {feature-name}
```

---

## Spike/ADR 워크플로우 (L 사이즈)

### Requirements 승인 후 Spike 제안
Requirements 승인 후, AI가 **Spike 필요성을 자동 감지**:

**트리거 조건** (하나라도 충족 시):
- ✅ 새 기술/라이브러리 첫 도입 (SignalR, Redis, gRPC)
- ✅ 성능 검증 필요 (측정 필요)
- ✅ 2개 이상 기술 대안 비교
- ✅ 외부 서비스 연동 실제 테스트

**AI 제안 예시**:
```
📋 Spike Proposal

Question: SignalR로 1000명 동시접속 시 응답속도 < 200ms?
Why: requirements.md AC-3 "1000명 동시 채팅" 검증 필요 (새 기술 첫 도입)
Method:
  1. SignalR 테스트 프로젝트 생성
  2. 1000 가상 클라이언트 연결 시뮬레이션
  3. 평균 응답속도 측정
Expected: 30분
Success Criteria: 평균 < 200ms → Go, 아니면 No-Go

실행하시겠습니까? [Y/n/Skip]
```

**사용자 선택**:
- **Y**: Spike 실행 (Go/No-Go 판정) → `docs/spikes/YYYY-MM/spike-xxx.md` 생성
- **n**: Spike 건너뛰고 Design 단계로
- **Skip**: Spike 영구 건너뛰기

**Spike 결과**: Design 단계에서 ADR 작성 시 참조

---

## 주의사항

- ⚠️ **Feature 이름은 kebab-case**: `pet-system` (O), `PetSystem` (X)
- ⚠️ **이미 존재하는 feature는 덮어쓰지 않음**: 확인 후 진행
- ⚠️ **TODO(human)은 아키텍처 학습 포인트만**: 게임 밸런스는 AI 제안

---

## 예시

### 입력
```
/spec-init pet-system
```

### 출력
```
✅ pet-system spec initialized

Created:
- .claude/memories/specs/pet-system/requirements.md

Initial content includes:
- 3 User Stories (Pet 획득, Pet 육성, Pet 스탯 버프)
- EARS format Acceptance Criteria
- 게임 밸런스 (AI 제안): Pet 가챠 확률 (Legendary 1%, Epic 9%, ...), 스탯 버프 공식
- TODO(human): 아키텍처 학습 포인트 (버프 저장 위치: Domain vs Application?)

Next steps:
1. Review .claude/memories/specs/pet-system/requirements.md
2. Decide TODO(human) 아키텍처 결정
3. 게임 밸런스 AI 제안값 확인
4. Mark approval section as complete
5. Run: /spec-design pet-system
```
