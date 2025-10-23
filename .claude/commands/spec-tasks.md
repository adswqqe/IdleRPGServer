# /spec-tasks - Tasks 문서 생성

**목적**: 승인된 Design을 실행 가능한 Task들로 분해합니다.

---

## 실행 절차

### 1. 전제 조건 확인
- `design.md`가 존재하는지 확인
- Design 승인 여부 확인:
  ```markdown
  ## Approval
  - [x] Design 리뷰 완료
  - [x] 모든 Requirements 항목 커버
  - [x] TODO(human) 비즈니스 로직 결정 완료
  **Approved by**: Development Team
  **Date**: YYYY-MM-DD
  ```
- 승인되지 않았으면 에러 메시지 출력 후 중단

### 2. 템플릿 기반 초안 작성
`.claude/memories/kiro-system-templates/tasks-template.md`를 기반으로 `tasks.md` 생성:

**Milestone 구조** (의존성 순서):
1. **Milestone 1: Domain Layer**
   - Entity, Enum, Domain Service, Value Object 생성
2. **Milestone 2: Infrastructure Layer**
   - Repository 구현, EF Core Configuration
3. **Milestone 3: Application Layer**
   - DTO, Service Interface, Service 구현
4. **Milestone 4: API Layer**
   - Controller, Endpoint 구현
5. **Milestone 5: Database**
   - Migration SQL (Idempotent), Seeder, DI 등록
6. **Milestone 6: Testing & Documentation**
   - Unit Tests, Integration Tests, Unity Documentation

### 3. 각 Task 작성 규칙
**Task 형식**:
```markdown
### X.Y Task 제목 ⏱️ 예상시간
- [ ] 구체적인 체크리스트 항목 1
- [ ] 구체적인 체크리스트 항목 2

**Requirements**: [US-X]
**Design Reference**: [섹션명]
```

**Task 분해 원칙**:
- ✅ 독립적으로 완료 및 테스트 가능
- ✅ 명확한 결과물 정의 (파일 생성, 메서드 구현 등)
- ✅ 예상 시간 산정 (15분~2시간)
- ✅ Requirements 추적성 (US-X 매핑)
- ❌ "시스템 이해하기", "조사하기" 같은 모호한 작업 금지

### 4. Progress Overview 생성
```markdown
## 📊 Progress Overview

**전체 진행률**: 0/25 (0%)

| Milestone | 작업 수 | 완료 | 진행률 |
|-----------|---------|------|--------|
| Domain Layer | 7 | 0 | 0% |
| Infrastructure Layer | 5 | 0 | 0% |
| ...

**예상 총 소요 시간**: ~XX시간
```

### 5. Tasks 품질 검증 (간소화된 Self-Review)

**검증 항목**:
1. ✅ **Design 커버리지**: design.md의 모든 컴포넌트가 Task로 변환되었는가?
   - Entity, Enum, Service, Repository, Controller, Migration, Seeder, Tests, Unity Docs
2. ✅ **의존성 순서**: Domain → Infrastructure → Application → API → Database → Tests 순서 준수?
3. ✅ **Task 명확성**: 각 Task가 명확한 결과물을 정의하는가? (파일명, 메서드명)
4. ✅ **시간 산정**: 각 Task가 15분~2시간 범위 내인가? (2시간 초과 시 분해)
5. ✅ **Requirements 추적성**: 각 Task에 US-X 참조가 포함되었는가?

**통과 기준**: 5/5 항목 모두 통과

### 6. 출력
```
✅ {feature-name} tasks created

Created:
- .claude/memories/specs/{feature-name}/tasks.md

Tasks breakdown:
- Total: 25 tasks
- Milestone 1 (Domain): 7 tasks (~3.5h)
- Milestone 2 (Infrastructure): 5 tasks (~2.5h)
- Milestone 3 (Application): 4 tasks (~3h)
- Milestone 4 (API): 3 tasks (~2h)
- Milestone 5 (Database): 3 tasks (~2h)
- Milestone 6 (Tests & Docs): 3 tasks (~3h)

Estimated total: ~16 hours

📋 Tasks 품질 검증: 5/5 통과 ✅
- #1 Design 커버리지: ✅ 모든 컴포넌트 포함
- #2 의존성 순서: ✅ Domain → Infra → App → API 순서 준수
- #3 Task 명확성: ✅ 모든 Task가 명확한 결과물 정의
- #4 시간 산정: ✅ 모든 Task가 15분~2시간 범위 내
- #5 Requirements 추적성: ✅ 모든 Task에 US-X 참조 포함

✨ 사용자 승인 준비 완료!

Next steps:
1. Review tasks.md (check order, dependencies)
2. Adjust estimates if needed
3. Mark approval
4. Run: /spec-execute {feature-name} 1.1
```

---

## 주의사항

- ⚠️ **Design 승인 필수**: 승인되지 않으면 실행 중단
- ⚠️ **Task는 순차 실행**: 1.1 → 1.2 → 2.1 (병렬 실행 금지)
- ⚠️ **의존성 순서**: Domain → Infrastructure → Application → API
- ⚠️ **Unity 문서 필수**: Milestone 6에 포함
- ⚠️ **품질 검증 필수**: 5개 항목 모두 통과해야 승인 가능

---

## 예시

### 입력
```
/spec-tasks pet-system
```

### 성공 케이스
```
✅ pet-system tasks created

Created:
- .claude/memories/specs/pet-system/tasks.md

Tasks breakdown:
- Milestone 1: Domain Layer (5 tasks, ~2.5h)
  - 1.1 Create Pet.cs entity (30min)
  - 1.2 Create PetTemplate.cs entity (30min)
  - 1.3 Create PetRarity enum (15min)
  - 1.4 Create PetStatBuffService (1h)
  - 1.5 Create IRandomProvider (15min)
  
- Milestone 2-6: ... (20 tasks, ~13.5h)

Total: 25 tasks, ~16 hours

📋 Tasks 품질 검증: 5/5 통과 ✅
- Design 커버리지, 의존성 순서, Task 명확성, 시간 산정, Requirements 추적성 모두 통과

Next steps:
1. Review .claude/memories/specs/pet-system/tasks.md
2. Verify task order and dependencies
3. **[필수] Run: /spec-review pet-system (목표: 90점 이상 - 최종 검증)**
4. Mark approval
5. Start execution: /spec-execute pet-system 1.1
```
