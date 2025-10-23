# /spec-review - Spec 품질 검증 및 상세 보고서

**목적**: 특정 feature의 Spec 품질을 자동 검증하고 상세 보고서를 생성합니다.

---

## 실행 절차

### 1. 인자 확인
- `$ARGS[0]`: feature-name (필수)
- `$ARGS[1]`: --format (선택, "summary" | "detailed", 기본값: "detailed")

### 2. Spec 존재 및 타입 확인
- L 사이즈: `.claude/memories/specs/{feature-name}/` 확인
- M 사이즈: `.claude/memories/specs/{feature-name}/spec-lite.md` 확인

### 3. 품질 검증 실행

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

### 4. 품질 점수 계산

**점수 체계** (100점 만점):
- Self-Review Checklist: 60점 (각 항목 6점)
- Requirements 추적성: 20점 (100% 추적 → 20점)
- TODO(human) 완성도: 10점 (100% 해소 → 10점)
- Decision Log: 5점 (모든 링크 유효 → 5점, L 사이즈만)
- Unity 문서: 5점 (존재 → 5점)

**등급**:
- 🏆 **Excellent**: 90점 이상
- ✅ **Good**: 70-89점
- ⚠️ **Needs Improvement**: 50-69점
- ❌ **Poor**: 50점 미만

### 5. 출력 형식

#### 5.1 Summary 모드 (`--format summary`)
```
📊 Spec Review: {Feature Name}

**Overall Score**: {점수}/100 ({등급})

**Quick Status**:
✅ Self-Review: 9/10 passed
⚠️ Requirements Traceability: 15/18 items tracked (83%)
❌ TODO(human): 3 items remaining
✅ Decision Log: All links valid
✅ Unity Docs: Present

**Recommendation**: {개선 권장사항 1줄}

For detailed report, run:
/spec-review {feature-name} --format detailed
```

#### 5.2 Detailed 모드 (기본값)
```
📊 Spec Quality Review: {Feature Name}

═══════════════════════════════════════════════════
                  OVERALL SCORE
═══════════════════════════════════════════════════

🏆 {점수}/100 - {등급}

Progress Bar: [████████░░] {점수}%

═══════════════════════════════════════════════════
          SELF-REVIEW CHECKLIST (60점)
═══════════════════════════════════════════════════

Score: {점수}/60 ({통과 개수}/10 passed)

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

═══════════════════════════════════════════════════
         UNITY DOCUMENTATION (5점)
═══════════════════════════════════════════════════

Score: {점수}/5

✅ Present:
- docs/unity/pet-system/API_SPEC.md (exists)
- docs/unity/pet-system/DTOs.cs (exists)

⚠️ Inconsistencies:
- API_SPEC.md shows GET /api/pets but design.md has GET /api/characters/{id}/pets
  → Action: Update Unity docs to match design.md

═══════════════════════════════════════════════════
              RECOMMENDATIONS
═══════════════════════════════════════════════════

Priority: HIGH
1. Define Response DTO for GET /api/pets/{id} in design.md
2. Resolve 3 unresolved TODO(human) items (especially Domain vs Application Service decision)
3. Fix broken ADR link (ADR-0016)

Priority: MEDIUM
4. Add traceability for US-3 (Pet evolution) in design.md
5. Specify authorization role for POST /api/pets
6. Update Unity API_SPEC.md to match endpoint paths

Priority: LOW
7. Add AC-4 logging task to tasks.md

═══════════════════════════════════════════════════
              NEXT STEPS
═══════════════════════════════════════════════════

To improve your score:
1. Address HIGH priority recommendations (expected +15 points)
2. Resolve remaining TODO(human) items (expected +5 points)
3. Fix Unity doc inconsistencies (expected +3 points)

Target Score: {현재점수 + 개선예상} → {등급}

Commands to run:
- Edit design.md: claude edit design.md
- Create missing ADR: /spec-design pet-system (re-run to update)
- Update Unity docs: cd ../IdleRPGClient/Docs && edit API_SPEC.md
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

### 자동 검증의 한계
- **Self-Review**는 구조적 검증만 가능 (내용의 적절성은 인간 판단 필요)
- **Traceability**는 키워드 매칭 기반 (예: "US-1", "AC-2")
- **TODO(human)**는 체크박스 형식만 감지

### 개선을 위한 피드백
- 검증 로직은 지속적으로 개선 가능
- False positive/negative 발견 시 리포트

---

## 관련 명령어

- `/spec-status {feature}`: 기본 상태 조회
- `/spec-list`: 전체 Spec 목록
- `/spec-design {feature}`: Design 재생성 (문제 수정 후)
- `/spec-tasks {feature}`: Tasks 재생성 (문제 수정 후)

---

## 팁

### 점수 향상 전략
1. **90점 이상 달성**:
   - Self-Review 10개 항목 모두 통과
   - 모든 Requirements 추적 완료
   - TODO(human) 100% 해소

2. **빠른 개선**:
   - HIGH priority 권장사항 먼저 처리
   - 구조적 문제(API 정의, 데이터 모델) 우선

3. **지속적 검증**:
   - Design 단계에서 1회 검증
   - Tasks 생성 후 1회 검증
   - Implementation 전 최종 검증
