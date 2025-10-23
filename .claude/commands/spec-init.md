# /spec-init - 새로운 Feature Spec 초기화

**목적**: 새로운 기능의 Requirements 문서를 생성합니다.

---

## 실행 절차

### 1. 인자 확인
- `$ARGS[0]`: feature-name (kebab-case)
- 예시: `pet-system`, `skill-equip`, `chat-hub`

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
- Game Design Requirements (게임 밸런스, 재화, 플레이어 경험)
  - **TODO(human)** 마커: 확률, 보상량, 비용 등
- Dependencies (기존 시스템, 새로운 요구사항)
- Non-Functional Requirements (Performance, Security, Scalability)

### 5. 출력
```
✅ {feature-name} spec initialized

Created:
- .claude/memories/specs/{feature-name}/requirements.md

Next steps:
1. Review requirements.md
2. Decide TODO(human) items (game balance, probabilities)
3. Check approval section
4. Run: /spec-design {feature-name}
```

---

## 주의사항

- ⚠️ **Feature 이름은 kebab-case**: `pet-system` (O), `PetSystem` (X)
- ⚠️ **이미 존재하는 feature는 덮어쓰지 않음**: 확인 후 진행
- ⚠️ **TODO(human)은 명확하게 마킹**: 사용자 결정 필요한 부분 강조

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
- TODO(human): Pet 스탯 버프 공식, Pet 가챠 확률

Next steps:
1. Review .claude/memories/specs/pet-system/requirements.md
2. Decide TODO(human) items
3. Mark approval section as complete
4. Run: /spec-design pet-system
```
