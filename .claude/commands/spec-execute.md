# /spec-execute - Task 실행

**목적**: 하나의 Task를 실행하고 완료 처리합니다.

---

## 실행 절차

### 1. 인자 확인
- `$ARGS[0]`: feature-name
- `$ARGS[1]`: task-number (예: `1.1`, `2.3`)

### 2. 전제 조건 확인
- `tasks.md`가 존재하는지 확인
- Tasks 승인 여부 확인
- 이전 task들이 완료되었는지 확인 (`[x]`)

### 3. Task 내용 파싱
`tasks.md`에서 해당 task 번호 찾기:
```markdown
### 1.1 Create Pet.cs Entity ⏱️ 30분
- [ ] Create IdleRPG.Domain/Entities/Pet.cs
- [ ] Add properties: Id, Name, Level, OwnerId
- [ ] Implement BaseEntity inheritance

**Requirements**: [US-1]
**Design Reference**: [Data Model - Pet]
```

### 4. TodoWrite로 진행 상황 추적
```json
{
  "content": "Task {task-number}: {task-title}",
  "status": "in_progress",
  "activeForm": "Task {task-number} 실행 중"
}
```

### 5. Task 실행
**체크리스트 항목별로 순차 실행**:
- 파일 생성/수정 (Write, Edit 도구 사용)
- TODO(human) 발견 시:
  - "● Learn by Doing" 형식으로 사용자 입력 요청
  - 사용자가 구현할 때까지 대기
  - 구현 완료 후 통합
- 코드 작성 완료

### 6. tasks.md 업데이트
해당 task의 체크박스를 `[x]`로 변경:
```markdown
### 1.1 Create Pet.cs Entity ⏱️ 30분 ✅
- [x] Create IdleRPG.Domain/Entities/Pet.cs
- [x] Add properties: Id, Name, Level, OwnerId
- [x] Implement BaseEntity inheritance
```

### 7. work-log.md 자동 생성/업데이트
`.claude/memories/specs/{feature-name}/work-log.md`에 자동 추가:
```markdown
## YYYY-MM-DD HH:MM

### Task Completed
- [x] {task-number} {task-title}

### Files Changed
- {파일 경로} (new file / modified)

### Key Decisions
- {결정 사항}

### Notes
- {특이 사항}

---
```

### 8. TodoWrite 완료 처리
```json
{
  "content": "Task {task-number}: {task-title}",
  "status": "completed"
}
```

### 9. 출력
```
✅ Task {task-number} completed

📝 work-log.md updated

Files changed:
- IdleRPG.Domain/Entities/Pet.cs (new file)

Next task: {next-task-number} - {next-task-title}

Ready to continue? Use: /spec-execute {feature-name} {next-task-number}
```

---

## TODO(human) 처리 흐름

### 발견 시
```
● Learn by Doing

**Context**: {현재 상황 설명}

**Your Task**: {파일명}에서 {메서드/클래스} 구현
Look for TODO(human).

**Guidance**:
- Option A: {옵션 1 설명}
- Option B: {옵션 2 설명}
{결정 가이드}
```

### 사용자 구현 후
- 코드 통합
- work-log.md에 결정사항 기록
- Task 계속 진행

---

## 주의사항

- ⚠️ **하나씩만 실행**: 다음 task는 이전 task 완료 후
- ⚠️ **순서 준수**: 1.1 → 1.2 → 2.1 (건너뛰기 금지)
- ⚠️ **TODO(human) 필수 해소**: 결정 없이 진행 불가
- ⚠️ **테스트 실패 시**: 수정 후 완료 처리

---

## 예시

### 입력
```
/spec-execute pet-system 1.1
```

### 출력
```
✅ Task 1.1 completed

Created Pet.cs entity with:
- Properties: Id (Guid), Name (string), Level (int), OwnerId (Guid)
- BaseEntity inheritance (CreatedAt, UpdatedAt)
- Navigation: Owner → Player

📝 work-log.md updated:
- Task 1.1 completed
- Decision: Pet Level default value = 1, MaxLevel = 100

Files changed:
- IdleRPG.Domain/Entities/Pet.cs (new file, 45 lines)

Next task: 1.2 - Create PetTemplate.cs Entity

Ready to continue? Use: /spec-execute pet-system 1.2
```

### TODO(human) 발견 케이스
```
⏸️ Task 1.4 paused - User input required

● Learn by Doing

**Context**: Pet 시스템의 스탯 버프 계산 로직이 필요합니다.

**Your Task**: PetStatBuffService.cs의 CalculateStatBuff() 메서드 구현
Look for TODO(human) in line 23.

**Guidance**:
- Option A: Attack * (1 + 0.05 * PetLevel) [레벨당 5% 증가]
- Option B: Attack + (10 * PetLevel) [레벨당 고정 10 증가]

게임 밸런스를 고려하여 선택하거나 직접 공식을 작성하세요.

[Waiting for your implementation...]
```

### 사용자 구현 후
```
✅ Implementation integrated

Your choice: Option A (Attack * (1 + 0.05 * PetLevel))

Task 1.4 completed

📝 work-log.md updated:
- Decision: Pet buff formula = Attack * (1 + 0.05 * PetLevel)
- Reasoning: 5% per level provides balanced growth (max 500% at Lv100)

Next task: 1.5 - Create IRandomProvider Interface
```
