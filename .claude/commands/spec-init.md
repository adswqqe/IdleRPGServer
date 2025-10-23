# /spec-init - 새로운 Feature Spec 초기화 (사이즈 자동 판단)

**목적**: 사용자의 기능 요청을 분석하여 적절한 Spec 사이즈(S/M/L)를 판단하고, 승인 후 문서를 생성합니다.

---

## 🎯 사이즈 자동 판단 워크플로우 (사용자 자연어 요청 시)

**트리거**: 사용자가 자연어로 기능 요청 (예: "펫 시스템 추가해줘", "캐릭터 이름 변경 기능")

### Step 0: 사이즈 판단 및 사용자 승인

#### 0-1. 기능 분석
다음 항목을 분석하여 사이즈 판단:
- 새 Entity 개수 예상
- API 엔드포인트 개수 예상
- 새 기술 도입 여부 (SignalR, Redis 등)
- 기존 시스템과의 통합 복잡도
- 비즈니스 로직 복잡도

#### 0-2. 사이즈 판단 기준
| 사이즈 | Entity | 엔드포인트 | 비즈니스 로직 | 새 기술 |
|--------|--------|------------|---------------|---------|
| **S** | 0개 (수정만) | 1개 | 없음 (단순 CRUD) | 없음 |
| **M** | 1개 이하 | 2-3개 | 간단한 계산/검증 | 없음 |
| **L** | 2개 이상 | 4개 이상 | 복잡한 가챠/매칭/레이드 | 있음 |

#### 0-3. 사용자에게 판단 결과 제시
```markdown
📏 이 기능은 **[L] (Large)** 사이즈로 판단됩니다.

**근거**:
- 새 Entity: 3개 (Pet, PetTemplate, CharacterPet)
- API 엔드포인트: 5개 (POST /gacha, GET /pets, PUT /level-up, ...)
- 비즈니스 로직: 복잡함 (가챠 확률, 천장 시스템, 스탯 버프 계산)
- 새 기술 도입: 없음
- 기존 시스템 통합: 필요 (전투 시스템과 스탯 연동)

**권장 워크플로우**: Full Spec (requirements + design + tasks)

이대로 진행하시겠습니까?
[Y] **L (Large)** 사이즈로 진행
[M] **M (Medium)** 사이즈로 간소화
[S] **S (Small)** 사이즈로 최소화
[N] 취소
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

### 4. 템플릿 기반 초안 작성
`.claude/memories/kiro-system-templates/requirements-template.md`를 기반으로 `requirements.md` 생성:

**포함 내용**:
- Feature Overview (목적, 성공 기준)
- User Stories (As a ... I want ... So that ...)
- Acceptance Criteria (EARS 형식: WHEN ... THEN system SHALL ...)
- Game Design Requirements:
  - **게임 밸런스 (AI 제안)**: 확률, 보상량, 비용 등 AI가 합리적인 기본값 제안
  - **학습 포인트 (아키텍처 결정)**: TODO(human) 마커 - 계층 분리, 설계 패턴 선택 등
- Dependencies (기존 시스템, 새로운 요구사항)
- Non-Functional Requirements (Performance, Security, Scalability)

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
