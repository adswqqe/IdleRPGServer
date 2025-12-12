# /spec-init-lite - Medium 사이즈 Spec 생성

**목적**: M (Medium) 사이즈 기능의 spec-lite.md (단일 파일 Spec)를 생성합니다.

---

## 트리거

- 사이즈 판단 결과 **M (Medium)**으로 승인된 경우
- 또는 사용자가 직접 `/spec-init-lite {feature-name}` 호출

---

## 실행 절차

### 1. Feature 이름 확인
- 사용자 요청에서 자동 생성: `{feature-name}` (kebab-case)
- 예시: "캐릭터 이름 변경" → `character-rename`

### 2. 파일 생성
```
.claude/memories/specs/{feature-name}/
└── spec-lite.md
```

### 3. 시스템 문서 참조
다음 문서들을 읽어서 컨텍스트 파악:
- `.claude/memories/_system/architecture.md` - Clean Architecture 패턴
- `.claude/memories/_system/game-design.md` - 게임 시스템 우선순위
- `.claude/memories/_system/api-standards.md` - 코딩 컨벤션

### 3.5. 작성 모드 선택 (유연성 확보)
```
AI: "📝 spec-lite 작성 방식을 선택하세요."

- [ ] **표준 모드** (권장: M 사이즈 기본)
  - AI가 템플릿 기반으로 spec-lite.md 초안 작성
  - 사용자는 TODO(human) 결정 및 검토
  - 예상 시간: 10-15분

- [ ] **Fast Track** (숙련자용)
  - AI가 전체 spec-lite.md 자동 작성 (TODO(human) 최소화)
  - 사용자는 최종 검토만
  - 예상 시간: 5분

사용자 선택 → 선택한 모드로 진행
```

> 💡 **Tip**: M 사이즈는 이미 간소화된 버전이므로 **표준 모드**로 충분합니다.

### 4. 템플릿 기반 초안 작성
`.claude/memories/kiro-system-templates/spec-lite-template.md`를 기반으로 `spec-lite.md` 생성:

> ⚠️ **중요**: spec-lite도 **개념적 명세만** 작성 (SQL DDL, C# 코드 ❌)
> 참고: [CLAUDE.md - Design vs Implementation 경계](../../../CLAUDE.md#design-vs-implementation-경계)

**포함 내용** (개념적 명세):
1. **Intent & Scope**: 목적, 범위 (In/Out Scope)
2. **User Stories & Acceptance Criteria**: EARS 형식
3. **API Design**: 엔드포인트, Request/Response 구조, 에러 조건
4. **Data Model**:
   - ✅ 필드 목록, 제약사항, 관계, 인덱스 요구사항
   - ❌ SQL DDL 코드
5. **Business Logic**:
   - ✅ 핵심 로직 흐름 (AI가 계산식 제안)
   - ❌ 구체적 메서드 구현
   - **TODO(human)**: 아키텍처 결정 (로직 위치, 패턴 선택)
6. **Constraints & Risks**: 제약사항, 리스크 및 대응 방안
7. **Test Strategy**: Unit/Integration 테스트 계획
8. **Implementation Checklist**: 구현 단계 체크리스트
9. **Unity Documentation Plan**: API_SPEC.md, DTOs.cs
10. **Decisions** (선택적): 간단한 결정은 직접 기록, 복잡한 결정은 ADR 링크

### 5. [조건부] Spike 제안 (트리거 충족 시)
AI가 Spike 필요성 감지 → **Spike Question 승인 요청** (M 사이즈는 최대 0-1개):

**트리거**: 새 라이브러리 도입, 간단한 성능 검증, 2가지 설계 대안 선택

**Spike 제안 예시**:
```
📋 Spike Proposal (M 사이즈)

Question: 이름 변경 이력 저장 시 성능 영향 < 50ms?
Why: 쿨다운 체크 성능 검증 필요
Method: 10만 건 데이터로 쿼리 성능 측정
Expected: 15분

실행하시겠습니까? [Y/n/Skip]
```

**Spike 결과**:
- `docs/spikes/YYYY-MM/spike-xxx.md` 생성 (최대 1개)
- spec-lite.md "10. Decisions" 섹션에 기록 또는 별도 ADR

### 6. 출력
```
✅ {feature-name} spec-lite created (M 사이즈)

Created:
- .claude/memories/specs/{feature-name}/spec-lite.md
- [조건부] docs/spikes/YYYY-MM/spike-xxx.md (Spike 실행 시)

Spec includes:
- 1-2 User Stories
- 2-3 API endpoints
- 간단한 비즈니스 로직 (AI 제안)
- TODO(human): 아키텍처 학습 포인트 (예: 트랜잭션 경계 설정)
- [조건부] Decisions: 1개 결정 기록 (간단한 경우) 또는 ADR 링크

Spike/ADR:
- [조건부] Spike-001: ✅ Go (성능 요구사항 충족)
- [조건부] Decision: 이름 변경 이력 저장 (Character 속성으로 결정)

Next steps:
1. Review spec-lite.md
2. [조건부] Review Spike 결과 및 Decisions 섹션
3. Decide TODO(human) 아키텍처 결정
4. 게임 밸런스 AI 제안값 확인
5. Mark approval section
6. 구현 시작 (Implementation Checklist 따라 진행)
```

---

## Spike/ADR 워크플로우 (M 사이즈)

### M 사이즈 특징
- **Spike**: 최대 0-1개 (선택적, 트리거 충족 시만)
- **ADR**: 간단한 결정은 spec-lite.md "10. Decisions" 섹션에 기록
- **복잡한 결정**: 별도 ADR 작성 후 spec-lite.md에 링크

### Decisions 섹션 사용 예시

**간단한 결정** (spec-lite.md 내):
```markdown
## 10. Decisions

### Decision 1: 이름 변경 이력 저장
- **Context**: 쿨다운 체크를 위해 마지막 변경 시각 필요
- **Decision**: Character Entity에 NameChangedAt 속성 추가
- **Rationale**: 별도 History Entity는 과도함 (M 사이즈)

### Decision 2: 쿨다운 체크 위치
- **Context**: 30일 쿨다운 검증 로직 위치
- **Decision**: Application Service (비즈니스 규칙)
- **Rationale**: 순수 비즈니스 로직이지만 외부 시계 의존 (DateTime.Now)
```

**복잡한 결정** (별도 ADR):
```markdown
## 10. Decisions

### Decision 1: FluentValidation 첫 도입
- **Context**: 이름 검증 규칙 (길이, 특수문자 제한)
- **Decision**: FluentValidation 도입
- **ADR**: [ADR-0008](../../docs/adr/ADR-0008-fluentvalidation.md)
```

---

## 주의사항

- ⚠️ **M 사이즈 기준 준수**: 새 Entity 1개 이하, 2-3개 엔드포인트
- ⚠️ **단일 파일**: spec-lite.md 하나로 모든 내용 포함
- ⚠️ **TODO(human)은 아키텍처 학습 포인트만**: 게임 밸런스는 AI 제안
- ⚠️ **Self-Review 필수**: 10개 항목 체크리스트 통과

---

## 예시

### 입력
```
사용자: "캐릭터 이름 변경 기능 추가해줘"

AI 사이즈 판단:
📏 이 기능은 **[M] (Medium)** 사이즈로 판단됩니다.

**근거**:
- 새 Entity: 0개 (Character 수정만)
- API 엔드포인트: 1개 (PUT /api/characters/{id}/name)
- 비즈니스 로직: 간단함 (이름 중복 검증, 20자 제한)
- 새 기술 도입: 없음
- 기존 시스템 통합: 불필요

**권장 워크플로우**: Spec Lite (단일 파일)

사용자: Y
```

### 출력
```
✅ character-rename spec-lite created (M 사이즈)

Created:
- .claude/memories/specs/character-rename/spec-lite.md

Spec includes:
- 1 User Story: "As a player, I want to change my character name"
- 1 API endpoint: PUT /api/characters/{id}/name
- Business Logic (AI 제안):
  - 이름 중복 검증 (사용자별)
  - 길이 제한: 2-20자
  - 쿨다운: 30일 (변경 후 재변경 불가)
- TODO(human):
  - "이름 변경 이력 저장? (별도 History Entity vs Character 속성?)"
  - "쿨다운 체크: Domain Service vs Application Service?"

Next steps:
1. Review .claude/memories/specs/character-rename/spec-lite.md
2. Decide TODO(human) items (이름 변경 이력 저장 방식)
3. **[필수] Run: /spec-review character-rename (목표: 70점 이상)**
4. Mark approval
5. 구현 시작:
   - Domain: Character.ChangeName() 메서드 추가
   - Infrastructure: EF Configuration 수정
   - Application: CharacterService.ChangeNameAsync()
   - API: CharactersController.ChangeName()
   - Tests: Unit + Integration
   - Unity: API_SPEC.md, DTOs.cs
```

---

## M vs L 사이즈 비교

| 항목 | M (Medium) | L (Large) |
|------|------------|-----------|
| 문서 | spec-lite.md (단일) | requirements + design + tasks |
| Entity | 0-1개 | 2개 이상 |
| 엔드포인트 | 2-3개 | 4개 이상 |
| 로직 | 간단한 검증/계산 | 복잡한 가챠/매칭/레이드 |
| 소요 시간 | 2-4시간 | 1-3일 |
| 승인 프로세스 | 1회 (spec-lite 승인) | 3회 (requirements, design, tasks) |
| **Spike** | 0-1개 (선택적) | 최대 3개 (권장) |
| **ADR** | spec-lite.md "10. Decisions" 또는 별도 ADR | design.md "Decision Log" + 별도 ADR (필수) |
| **Decisions 기록** | 간단: 1-3문장, 복잡: ADR 링크 | 테이블 형식 + ADR 링크 |

