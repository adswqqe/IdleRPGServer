# /spec-status - 특정 Spec 상태 조회

**목적**: 특정 feature의 상세 상태를 조회합니다.

---

## 실행 절차

### 1. 인자 확인
- `$ARGS[0]`: feature-name

### 2. Spec 존재 확인
`.claude/memories/specs/{feature-name}/` 폴더가 존재하는지 확인

### 3. 각 문서 상태 파악
- **requirements.md**: 승인 여부, 승인 날짜
- **design.md**: 승인 여부, TODO(human) 개수
- **tasks.md**: 전체/완료 작업 수, 현재 진행 중인 task
- **work-log.md**: 마지막 엔트리 날짜, 총 엔트리 수

### 4. 파일 목록 수집
`tasks.md`에서 생성/수정된 파일 목록 추출

### 5. 출력 형식
```
📊 {Feature Name} - Status

**Phase**: {1-4 or Completed}

**Requirements**:
- Status: ✅ Approved / 📋 Draft
- Approved by: {이름}
- Date: {날짜}

**Design**:
- Status: ✅ Approved / 📋 Draft
- TODO(human): {개수}개
- Approved by: {이름}
- Date: {날짜}

**Tasks**:
- Total: {전체} tasks
- Completed: {완료} tasks ({진행률}%)
- In Progress: Task {번호} - {제목}
- Next: {다음 task 번호} - {제목}

**Files Created/Modified**:
- {파일 경로 1}
- {파일 경로 2}
...

**Work Log**:
- Last entry: {날짜}
- Total entries: {개수}

**Next Command**:
/spec-execute {feature-name} {next-task}
```

---

## 주의사항

- 이 명령어는 **조회만** 하며, 변경하지 않음
- Spec이 존재하지 않으면 에러 메시지 출력

---

## 예시

### 입력
```
/spec-status pet-system
```

### 출력 (성공 케이스)
```
📊 Pet System - Status

**Phase**: 4 (Execution)

**Requirements**:
- Status: ✅ Approved
- Approved by: Development Team
- Date: 2025-10-20

**Design**:
- Status: ✅ Approved
- TODO(human): 0개 (모두 해소됨)
- Approved by: Development Team
- Date: 2025-10-21

**Tasks**:
- Total: 20 tasks
- Completed: 8 tasks (40%)
- In Progress: Task 2.3 - Create PetService Implementation
- Next: 2.3 - Create PetService Implementation

**Files Created/Modified**:
- IdleRPG.Domain/Entities/Pet.cs
- IdleRPG.Domain/Entities/PetTemplate.cs
- IdleRPG.Domain/Enums/PetRarity.cs
- IdleRPG.Domain/Services/PetStatBuffService.cs
- IdleRPG.Domain/Repositories/IPetRepository.cs
- IdleRPG.Infrastructure/Repositories/PetRepository.cs
- IdleRPG.Infrastructure/Configurations/PetConfiguration.cs
- IdleRPG.Application/DTOs/Pet/PetDto.cs

**Work Log**:
- Last entry: 2025-10-23 14:30
- Total entries: 8

**Next Command**:
/spec-execute pet-system 2.3
```

### 출력 (에러 케이스)
```
❌ Spec not found: unknown-feature

Available specs:
- pet-system
- skill-equip
- chat-hub

Usage:
/spec-status {feature-name}

Example:
/spec-status pet-system
```

### 출력 (초기 상태)
```
📊 Pet System - Status

**Phase**: 1 (Requirements)

**Requirements**:
- Status: 📋 Draft
- Approved by: (not yet approved)
- Date: (pending)

**Design**:
- Status: ⏸️ Waiting (not created)

**Tasks**:
- Status: ⏸️ Waiting (not created)

**Next Steps**:
1. Review .claude/memories/specs/pet-system/requirements.md
2. Decide TODO(human) items (game balance)
3. Mark approval section
4. Run: /spec-design pet-system
```
