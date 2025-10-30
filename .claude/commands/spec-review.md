# /spec-review - Spec 품질 검증 및 상세 보고서

**목적**: 특정 feature의 Spec 품질을 자동 검증하고 상세 보고서를 생성합니다.

**검증 방식**:
- **Claude (구조적 검증)**: 형식, 완성도, 추적성 체크
- **Gemini Pro 2.5 (내용 품질 평가)**: 설계 적절성, 비즈니스 로직, 아키텍처 품질
- **통합 분석**: 두 관점을 종합하여 최종 점수 산출

---

## 실행 절차 (3단계 프로세스)

### 1. 인자 확인 및 Spec 타입 확인
- `$ARGS[0]`: feature-name (필수)
- `$ARGS[1]`: --format (선택, "summary" | "detailed", 기본값: "detailed")
- L 사이즈: `.claude/memories/specs/{feature-name}/` 확인
- M 사이즈: `.claude/memories/specs/{feature-name}/spec-lite.md` 확인

---

### STEP 1: Claude Self-Review (구조적 검증)

**목적**: 문서의 형식적 완성도와 추적성 검증

#### 3.1 Self-Review Checklist 검증 (10개 항목)
**Self-Review Checklist 위치**: `.claude/memories/kiro-system-templates/self-review-checklist.md`

**L 사이즈 (design.md)**:
1. ✅/❌ 요구사항 추적성: 모든 US-X가 Design에 참조되는가?
2. ✅/❌ Clean Architecture: Domain이 다른 계층에 의존하지 않는가?
3. ✅/❌ API 계약 정의: 엔드포인트, Request/Response DTO 명확한가?
4. ✅/❌ 데이터 모델 정의: Entity, 관계, 제약조건, 인덱스 명시되었는가?
5. ✅/❌ 인증 및 권한: 각 API의 인증 여부와 권한 명시되었는가?
6. ✅/❌ 유효성 검사: Request DTO 유효성 규칙이 있는가?
7. ✅/❌ 에러 처리: 실패 시나리오와 에러 메시지가 정의되었는가?
8. ✅/❌ 트랜잭션 경계: 원자적 작업이 식별되었는가?
9. ✅/❌ 비기능적 요구사항: 로깅, 성능, 보안 고려되었는가?
10. ✅/❌ Unity 문서화 계획: API_SPEC.md, DTOs.cs 계획이 있는가?

**M 사이즈 (spec-lite.md)**:
동일한 10개 항목, 단일 파일에서 검증

#### 3.2 Requirements 추적성 검증
- requirements.md의 모든 **US-X**, **AC-X** 추출
- design.md/tasks.md에서 각 항목이 참조되는지 확인
- 누락된 항목 리스트업

#### 3.3 TODO(human) 완성도 체크
- design.md, tasks.md에서 모든 `TODO(human)` 검색
- 해소 여부 확인 (체크박스 `[ ]` vs `[x]`)
- 미해소 TODO 위치와 내용 리포트

#### 3.4 Decision Log 검증 (L 사이즈만)
- ADR 링크가 실제 파일로 연결되는가?
- Spike 링크가 실제 파일로 연결되는가?

#### 3.5 API-Unity 문서 일관성 (선택적)
- `docs/unity/{feature-name}/API_SPEC.md` 존재 여부
- Design의 API와 Unity 문서의 엔드포인트 일치 여부

#### 3.6 리스크 재평가 (구현 중 변화 추적)
초기 리스크 점수와 현재 상태 비교:

**초기 리스크 점수 추출**:
- requirements.md에서 초기 리스크 점수 추출 (Blast Radius, Novelty, Unknowns)

**현재 리스크 재평가**:
- **Blast Radius**: design.md의 실제 영향 범위 분석
  - API 개수, 참조하는 시스템 개수
  - 기존 판단 vs 실제 설계 비교
- **Novelty**: design.md/tasks.md의 실제 기술 스택
  - 새 라이브러리/패턴 도입 여부
  - ADR 개수로 새로움 측정
- **Unknowns**: TODO(human) 개수와 Spike 결과
  - 미해소 TODO가 많을수록 불확실성 증가
  - Spike No-Go 결과는 불확실성 증가

**변화 감지**:
- 점수 증가 (+2점 이상): ⚠️ 리스크 증가 경고
- 점수 감소 (-2점 이상): ✅ 리스크 완화 확인
- 사이즈 재판단: 현재 리스크가 다른 사이즈 기준 초과 시 경고

---

### STEP 2: Gemini External Review (내용 품질 평가)

**목적**: 설계의 적절성, 비즈니스 로직 품질, 아키텍처 일관성 평가

#### 2.1 Zen MCP Clink를 통한 Gemini 호출

**Tool**: `mcp__zen__clink`
- `cli_name`: "gemini"
- `role`: "codereviewer"

**Prompt Template** (Gemini에게 전달):
```
You are reviewing a Clean Architecture-based ASP.NET Core backend design document.

**Context**:
- Project: {project-name}
- Feature: {feature-name}
- Size: {L/M}
- Tech Stack: ASP.NET Core 8.0, EF Core, PostgreSQL, SignalR (if applicable)

**Documents to Review**:
{L 사이즈인 경우}
- Requirements: [requirements.md 전체 내용]
- Design: [design.md 전체 내용]
- Tasks: [tasks.md 전체 내용]

{M 사이즈인 경우}
- Spec: [spec-lite.md 전체 내용]

**Review Criteria** (각 항목 1-10점 평가):

1. **Architecture Quality** (아키텍처 품질)
   - Clean Architecture 원칙 준수 (의존성 방향, 계층 분리)
   - Domain 순수성 (외부 의존성 없음)
   - Service 책임 명확성 (Application vs Domain Service)
   - 점수: ?/10
   - 이유: [구체적 근거]

2. **Data Modeling** (데이터 모델링)
   - Entity 관계 설계의 적절성 (1:N, N:M)
   - 정규화 vs 역정규화 선택 적절성
   - 인덱스 전략의 효율성 (복합 인덱스, Covering Index)
   - Cascade Delete 규칙의 타당성
   - 점수: ?/10
   - 이유: [구체적 근거]

3. **Business Logic** (비즈니스 로직)
   - 게임 밸런스 수치의 합리성 (확률, 쿨다운, 제한 등)
   - 검증 로직의 충분성 (Input Validation, Business Rules)
   - 에러 처리의 완전성 (모든 실패 시나리오 커버)
   - 점수: ?/10
   - 이유: [구체적 근거]

4. **Performance Considerations** (성능 고려)
   - N+1 쿼리 방지 전략 (Include, Select Projection)
   - 페이징 전략의 적절성 (Cursor vs Offset)
   - 캐싱 전략의 합리성 (MemoryCache, Redis)
   - 인덱스 활용 최적화
   - 점수: ?/10
   - 이유: [구체적 근거]

5. **Security & Authorization** (보안 및 권한)
   - 인증 방식의 적절성 (JWT Bearer, Query String for SignalR)
   - 권한 체크 완전성 (모든 API 엔드포인트)
   - SQL Injection, XSS 방지
   - 민감 정보 보호 (Password 제외, Select Projection)
   - 점수: ?/10
   - 이유: [구체적 근거]

**Output Format**:
```json
{
  "overall_score": ?/50 (5개 항목 합산),
  "architecture_quality": { "score": ?/10, "reason": "...", "suggestions": ["..."] },
  "data_modeling": { "score": ?/10, "reason": "...", "suggestions": ["..."] },
  "business_logic": { "score": ?/10, "reason": "...", "suggestions": ["..."] },
  "performance": { "score": ?/10, "reason": "...", "suggestions": ["..."] },
  "security": { "score": ?/10, "reason": "...", "suggestions": ["..."] },
  "critical_issues": ["issue 1", "issue 2"],
  "strengths": ["strength 1", "strength 2"],
  "improvement_priority": {
    "high": ["action 1", "action 2"],
    "medium": ["action 3"],
    "low": ["action 4"]
  }
}
```

**Important**:
- 한국어로 응답하되, 기술 용어는 영어 유지
- 구체적인 코드 위치(design.md:123) 또는 섹션명 인용
- AI가 제안한 게임 밸런스 수치도 합리성 평가
- TODO(human) 항목은 아키텍처 학습 포인트이므로 긍정 평가
```

#### 2.2 Gemini 응답 파싱
- JSON 응답 파싱
- 각 항목 점수 추출 (총 50점 만점)
- Critical Issues, Strengths 추출
- Improvement Priority 추출

---

### STEP 3: Synthesis (결과 통합 및 최종 점수 산출)

#### 3.1 점수 통합

**Claude 점수 (50점 만점)**:
- Self-Review Checklist: 30점 (각 항목 3점)
- Requirements 추적성: 10점 (100% 추적 → 10점)
- TODO(human) 완성도: 5점 (100% 해소 → 5점)
- Decision Log: 2.5점 (모든 링크 유효 → 2.5점, L 사이즈만)
- Unity 문서: 2.5점 (존재 → 2.5점)

**Gemini 점수 (50점 만점)**:
- Architecture Quality: 10점
- Data Modeling: 10점
- Business Logic: 10점
- Performance: 10점
- Security: 10점

**최종 점수** = Claude 점수 + Gemini 점수 (100점 만점)

**등급**:
- 🏆 **Excellent**: 90점 이상
- ✅ **Good**: 70-89점
- ⚠️ **Needs Improvement**: 50-69점
- ❌ **Poor**: 50점 미만

#### 3.2 Recommendations 통합
- Claude의 구조적 문제 (HIGH 우선순위)
- Gemini의 Critical Issues (HIGH 우선순위)
- Gemini의 Improvement Priority (MEDIUM/LOW)
- 중복 제거 및 우선순위 재정렬

---

### 4. 출력 형식

#### 4.1 Summary 모드 (`--format summary`)
```
📊 Spec Review: {Feature Name}

═══════════════════════════════════════════════════
                  OVERALL SCORE
═══════════════════════════════════════════════════

🏆 {최종점수}/100 - {등급}

Progress Bar: [████████░░] {점수}%

Claude (구조적 검증): {Claude점수}/50
Gemini (내용 품질): {Gemini점수}/50

═══════════════════════════════════════════════════

**Claude Quick Status**:
✅ Self-Review: 9/10 passed
⚠️ Requirements Traceability: 15/18 items tracked (83%)
❌ TODO(human): 3 items remaining
✅ Decision Log: All links valid
✅ Unity Docs: Present

**Gemini Quick Status**:
✅ Architecture Quality: 8/10
⚠️ Data Modeling: 7/10
✅ Business Logic: 9/10
✅ Performance: 8/10
❌ Security: 6/10 (권한 체크 불충분)

**Critical Issues**: {개수}개
**Strengths**: {개수}개

**Top Recommendation**: {가장 중요한 개선사항 1줄}

For detailed report, run:
/spec-review {feature-name} --format detailed
```

#### 4.2 Detailed 모드 (기본값)
```
📊 Spec Quality Review: {Feature Name}

═══════════════════════════════════════════════════
                  OVERALL SCORE
═══════════════════════════════════════════════════

🏆 {최종점수}/100 - {등급}

Progress Bar: [████████░░] {점수}%

Claude (구조적 검증): {Claude점수}/50 ({Claude등급})
Gemini (내용 품질): {Gemini점수}/50 ({Gemini등급})

═══════════════════════════════════════════════════
          PART 1: CLAUDE STRUCTURAL REVIEW
═══════════════════════════════════════════════════

Score: {Claude점수}/50

─────────────────────────────────────────────────
  1.1 SELF-REVIEW CHECKLIST (30점)
─────────────────────────────────────────────────

Score: {점수}/30 ({통과 개수}/10 passed)

1. ✅ 요구사항 추적성
   → All US-1, US-2 referenced in Design

2. ✅ Clean Architecture
   → Domain layer has no external dependencies

3. ❌ API 계약 정의
   → Missing: Response DTO for GET /api/pets/{id}
   → Location: design.md:85

4. ✅ 데이터 모델 정의
   → All entities, relationships, indexes defined

5. ⚠️ 인증 및 권한
   → Authorization mentioned but role not specified for POST /api/pets
   → Location: design.md:120

... (10개 항목 모두 체크)

═══════════════════════════════════════════════════
      REQUIREMENTS TRACEABILITY (20점)
═══════════════════════════════════════════════════

Score: {점수}/20 (15/18 items tracked = 83%)

✅ Tracked Requirements:
- US-1: Player can adopt pet → Referenced in design.md:45, tasks.md:23
- US-2: Pet provides stat buffs → Referenced in design.md:67, tasks.md:35
- AC-1: System validates pet rarity → Referenced in tasks.md:52

❌ Missing Traceability:
- US-3: Pet evolution system (mentioned in requirements.md:78)
  → NOT found in design.md or tasks.md
  → Action: Add to design.md Business Logic section

- AC-4: System logs pet adoption events (requirements.md:92)
  → NOT found in tasks.md
  → Action: Add logging task in tasks.md Milestone 5

═══════════════════════════════════════════════════
         TODO(HUMAN) COMPLETION (10점)
═══════════════════════════════════════════════════

Score: {점수}/10 (5/8 resolved = 62%)

✅ Resolved TODOs (5개):
- [x] Pet-Character relationship (design.md:123)
- [x] Index strategy for PetId (design.md:234)
...

❌ Unresolved TODOs (3개):
1. [ ] Domain Service vs Application Service for buff calculation
   → Location: design.md:145
   → Impact: Architecture decision needed before implementation

2. [ ] IRandomProvider abstraction for gacha
   → Location: tasks.md:62
   → Impact: Affects testing strategy

3. [ ] Cascade Delete strategy
   → Location: design.md:267
   → Impact: Data integrity decision

═══════════════════════════════════════════════════
          DECISION LOG VALIDATION (5점)
═══════════════════════════════════════════════════

Score: {점수}/5

✅ Valid Links:
- ADR-0015: Pet storage in separate table (docs/adr/ADR-0015-pet-storage.md)
- Spike-003: Pet stat calculation performance (docs/spikes/2025-10/spike-003.md)

❌ Broken Links:
- ADR-0016: Pet evolution trigger (docs/adr/ADR-0016-evolution.md)
  → File not found
  → Action: Create ADR or remove link

─────────────────────────────────────────────────
  1.5 UNITY DOCUMENTATION (2.5점)
─────────────────────────────────────────────────

Score: {점수}/2.5

✅ Present:
- docs/unity/pet-system/API_SPEC.md (exists)
- docs/unity/pet-system/DTOs.cs (exists)

⚠️ Inconsistencies:
- API_SPEC.md shows GET /api/pets but design.md has GET /api/characters/{id}/pets
  → Action: Update Unity docs to match design.md

═══════════════════════════════════════════════════
          PART 2: GEMINI QUALITY REVIEW
═══════════════════════════════════════════════════

Score: {Gemini점수}/50

─────────────────────────────────────────────────
  2.1 ARCHITECTURE QUALITY (10점)
─────────────────────────────────────────────────

Score: {점수}/10

**평가 근거**:
{Gemini의 architecture_quality.reason}

**개선 제안**:
- {suggestion 1}
- {suggestion 2}

─────────────────────────────────────────────────
  2.2 DATA MODELING (10점)
─────────────────────────────────────────────────

Score: {점수}/10

**평가 근거**:
{Gemini의 data_modeling.reason}

**개선 제안**:
- {suggestion 1}
- {suggestion 2}

─────────────────────────────────────────────────
  2.3 BUSINESS LOGIC (10점)
─────────────────────────────────────────────────

Score: {점수}/10

**평가 근거**:
{Gemini의 business_logic.reason}

**개선 제안**:
- {suggestion 1}
- {suggestion 2}

─────────────────────────────────────────────────
  2.4 PERFORMANCE CONSIDERATIONS (10점)
─────────────────────────────────────────────────

Score: {점수}/10

**평가 근거**:
{Gemini의 performance.reason}

**개선 제안**:
- {suggestion 1}
- {suggestion 2}

─────────────────────────────────────────────────
  2.5 SECURITY & AUTHORIZATION (10점)
─────────────────────────────────────────────────

Score: {점수}/10

**평가 근거**:
{Gemini의 security.reason}

**개선 제안**:
- {suggestion 1}
- {suggestion 2}

═══════════════════════════════════════════════════
        PART 3: INTEGRATED ANALYSIS
═══════════════════════════════════════════════════

─────────────────────────────────────────────────
  CRITICAL ISSUES (반드시 수정 필요)
─────────────────────────────────────────────────

{Gemini의 critical_issues 리스트}

예:
1. [Gemini] 권한 체크 누락: POST /api/pets에 Authorization 없음 (design.md:120)
2. [Claude] Requirements 추적성: US-3 (Pet evolution) 미포함
3. [Gemini] 인덱스 전략: Pets.CharacterId 인덱스 누락 (design.md:234)

─────────────────────────────────────────────────
  STRENGTHS (잘된 점)
─────────────────────────────────────────────────

{Gemini의 strengths 리스트}

예:
1. Clean Architecture 의존성 방향 완벽 준수
2. Cursor 페이징 전략 우수 (Offset 대비 안정성)
3. Select Projection으로 N+1 쿼리 방지
4. TODO(human) 학습 포인트 명확히 정의

─────────────────────────────────────────────────
  RISK RE-ASSESSMENT (참고 정보)
─────────────────────────────────────────────────

**초기 리스크 점수** (requirements.md):
- Blast Radius: 3점 (전투 시스템 영향)
- Novelty: 2점 (기존 패턴 활용)
- Unknowns: 4점 (가챠 밸런스 불확실)
→ 합산: 9/15 (중위험, M 사이즈 기준)

**현재 리스크 재평가** (구현 중):
- Blast Radius: 4점 → +1점 ⚠️
  → 이유: 실제로 인벤토리 시스템까지 영향 (design.md:156)
  → 추가 영향: Item 드랍 시스템과 통합 필요

- Novelty: 3점 → +1점 ⚠️
  → 이유: ADR-0015에서 별도 Pet 스토리지 도입 (새 패턴)
  → 추가 복잡도: PetRepository 캐싱 전략 추가

- Unknowns: 5점 → +1점 ⚠️
  → 이유: 3개 미해소 TODO(human) (design.md:145, 234, 267)
  → 추가 불확실성: Spike-003 결과 "추가 검증 필요"

→ **재평가 합산: 12/15 (고위험)** ⚠️

**리스크 변화 분석**:
📈 Risk Escalation: 9점 → 12점 (+3점, +33% 증가)

⚠️ **경고**:
- 초기 판단: M 사이즈 리스크 (6-9점)
- 현재 상태: L 사이즈 리스크 (10-15점)
- 실제 사이즈: L (복잡도 기준으로 이미 L)

**권장사항**:
1. 인벤토리 시스템 통합 영향 재검토 (Blast Radius 완화)
2. TODO(human) 3개 조기 해소 (Unknowns 완화)
3. Spike-003 후속 검증 실행 고려

**긍정적 측면**:
✅ 복잡도 기준으로 이미 L 사이즈였으므로 워크플로우는 적절
✅ Full Spec 프로세스로 리스크 조기 발견 가능했음

═══════════════════════════════════════════════════
              INTEGRATED RECOMMENDATIONS
═══════════════════════════════════════════════════

**Priority: CRITICAL** (점수 향상 +15점 이상)
1. [Gemini] {critical_issue_1}
   → Action: {구체적 해결 방법}
2. [Claude] {구조적 문제 1}
   → Action: {구체적 해결 방법}

**Priority: HIGH** (점수 향상 +10점)
3. [Gemini] {high_priority_1}
   → Action: {구체적 해결 방법}
4. [Claude] {구조적 문제 2}
   → Action: {구체적 해결 방법}

**Priority: MEDIUM** (점수 향상 +5점)
5. [Gemini] {medium_priority_1}
   → Action: {구체적 해결 방법}

**Priority: LOW** (점수 향상 +2점)
6. [Gemini] {low_priority_1}
   → Action: {구체적 해결 방법}

예시:
Priority: CRITICAL
1. [Gemini] Security: POST /api/pets에 [Authorize] 속성 누락 (design.md:120)
   → Action: API Design 섹션에 [Authorize] 명시 및 권한 체크 로직 추가
2. [Claude] Requirements: US-3 (Pet evolution) 추적 누락
   → Action: design.md Business Logic 섹션에 진화 메커니즘 추가

Priority: HIGH
3. [Gemini] Data Modeling: Pets.CharacterId 인덱스 누락 (design.md:234)
   → Action: Migration Plan에 CREATE INDEX IX_Pets_CharacterId 추가
4. [Claude] TODO(human) 3개 미해소 (design.md:145, 234, 267)
   → Action: Domain Service vs Application Service 결정 후 체크박스 완료

═══════════════════════════════════════════════════
              NEXT STEPS
═══════════════════════════════════════════════════

**점수 향상 전략**:
1. Address CRITICAL priority (expected +{점수}점) → Target: {목표등급}
2. Address HIGH priority (expected +{점수}점) → Target: {목표등급}
3. Address MEDIUM priority (expected +{점수}점) → Target: {목표등급}

**Current**: {현재점수}/100 ({현재등급})
**Target (CRITICAL only)**: {예상점수}/100 ({예상등급})
**Target (CRITICAL + HIGH)**: {예상점수}/100 ({예상등급})

**Commands to run**:
- Edit design.md: `claude edit .claude/memories/specs/{feature-name}/design.md`
- Create missing ADR: `/adr-create {번호} {제목}`
- Update Unity docs: `cd D:/Proj/IdleGameClient/Docs/unity && edit {feature-name}/API_SPEC.md`
- Re-run review: `/spec-review {feature-name}`

**Gemini가 제안한 추가 조치**:
{Gemini의 improvement_priority 통합}
```

---

## 사용 예시

### 예시 1: Summary 모드
```bash
/spec-review pet-system --format summary
```

### 예시 2: Detailed 모드 (기본값)
```bash
/spec-review pet-system
```

### 예시 3: M 사이즈 Spec 검증
```bash
/spec-review character-rename
# spec-lite.md를 자동 감지하여 검증
```

---

## 주의사항

### 검증 방식의 특성

**Claude (구조적 검증)**:
- ✅ **강점**: 형식적 완성도, 추적성, 문서 구조
- ⚠️ **한계**: 내용의 적절성, 비즈니스 로직 품질은 제한적
- **방법**: 키워드 매칭, 파일 존재 확인, 체크리스트 항목 검색

**Gemini (내용 품질 평가)**:
- ✅ **강점**: 설계 적절성, 아키텍처 일관성, 비즈니스 로직 합리성
- ⚠️ **한계**: 프로젝트 컨텍스트 이해 제한 (템플릿 기반 평가)
- **방법**: 전체 문서 분석, 아키텍처 패턴 평가, 모범 사례 비교

### Gemini 연동 관련

**성공 조건**:
- Zen MCP 서버가 실행 중이어야 함
- Gemini CLI가 정상 구성되어 있어야 함 (zen:clink 동작 확인)
- 문서 크기가 Gemini 컨텍스트 제한 내여야 함 (~1M tokens)

**실패 시 대응**:
- Gemini 연동 실패 시: Claude 점수만으로 평가 (50점 만점, 90점 목표 = 45점 필요)
- 경고 메시지: "⚠️ Gemini review failed. Showing Claude-only score ({점수}/50). Please check Zen MCP server."

### 개선을 위한 피드백
- 검증 로직은 지속적으로 개선 가능
- False positive/negative 발견 시 리포트
- Gemini 프롬프트 개선 제안 환영

---

## 관련 명령어

- `/spec-status {feature}`: 기본 상태 조회
- `/spec-list`: 전체 Spec 목록
- `/spec-design {feature}`: Design 재생성 (문제 수정 후)
- `/spec-tasks {feature}`: Tasks 재생성 (문제 수정 후)

---

## 팁

### 점수 향상 전략

**90점 이상 달성 (Excellent)**:
- **Claude 45점 이상** (50점 만점):
  - Self-Review 10개 항목 모두 통과 (30점)
  - 모든 Requirements 추적 완료 (10점)
  - TODO(human) 100% 해소 (5점)
- **Gemini 45점 이상** (50점 만점):
  - Architecture Quality 9점 이상 (Clean Architecture 완벽 준수)
  - Data Modeling 9점 이상 (인덱스 전략, 관계 설계 우수)
  - Business Logic 9점 이상 (검증 로직 충분, 에러 처리 완전)
  - Performance 9점 이상 (N+1 방지, 페이징 최적화)
  - Security 9점 이상 (권한 체크 완전, 민감 정보 보호)

**빠른 개선 (70점 → 90점)**:
1. **CRITICAL 우선순위 해결** (+15점):
   - Gemini가 지적한 Critical Issues (보안, 아키텍처)
   - Claude가 발견한 구조적 누락 (Requirements 추적)
2. **HIGH 우선순위 해결** (+10점):
   - 인덱스 누락, TODO(human) 미해소
3. **MEDIUM 우선순위** (+5점):
   - Unity 문서 일관성, Decision Log 링크

**지속적 검증 워크플로우**:
1. **Requirements 단계**: Complexity 체크만 (리스크 평가)
2. **Design 단계**: `/spec-review {feature} --format summary` (70점 목표)
   - 구조적 완성도 확인
   - Gemini 피드백으로 설계 개선
3. **Tasks 생성 후**: `/spec-review {feature}` (90점 목표)
   - 전체 커버리지 확인
   - Implementation 전 최종 검증
4. **Implementation 전**: 최종 90점 달성 확인

**Gemini 피드백 활용 팁**:
- **Architecture Quality 낮음** (7점 이하):
  → Domain 의존성 재확인, Service 책임 분리
- **Data Modeling 낮음** (7점 이하):
  → 인덱스 전략 재검토, Cascade 규칙 점검
- **Performance 낮음** (7점 이하):
  → N+1 쿼리 확인, Select Projection 적용
- **Security 낮음** (7점 이하):
  → [Authorize] 누락 확인, 권한 체크 로직 추가
