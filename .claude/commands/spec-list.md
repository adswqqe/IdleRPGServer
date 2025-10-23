# /spec-list - 전체 Spec 목록 조회

**목적**: 진행 중인 모든 Spec의 목록과 상태를 조회합니다.

---

## 실행 절차

### 1. Specs 폴더 스캔
`.claude/memories/specs/` 폴더 내 모든 하위 폴더 검색

### 2. 각 Spec 상태 파악
각 feature에 대해:
- `requirements.md` 존재 및 승인 여부
- `design.md` 존재 및 승인 여부
- `tasks.md` 존재 및 진행 상황 (완료/전체)

### 3. Phase 판단
- **Phase 1**: requirements.md만 존재 (Draft 또는 Approved)
- **Phase 2**: design.md 존재 (Draft 또는 Approved)
- **Phase 3**: tasks.md 존재, 진행 전 (0% 완료)
- **Phase 4**: tasks.md 존재, 실행 중 (1%~99% 완료)
- **Completed**: 모든 tasks 완료 (100%)

### 4. 출력 형식
```
📋 Active Specs ({개수}개)

| Feature | Phase | Requirements | Design | Tasks | Progress |
|---------|-------|--------------|--------|-------|----------|
| pet-system | 4 | ✅ Approved | ✅ Approved | 📋 In Progress | 3/15 (20%) |
| skill-equip | 2 | ✅ Approved | 📋 Draft | ⏸️ Waiting | 0/0 (0%) |
| chat-hub | 1 | 📋 Draft | ⏸️ Waiting | ⏸️ Waiting | 0/0 (0%) |

📊 Overall Progress: 3/15 tasks completed across all specs

Next actions:
- pet-system: /spec-execute pet-system 1.4
- skill-equip: Review design.md and approve
- chat-hub: Review requirements.md and approve
```

---

## 아이콘 범례

### Requirements/Design 상태
- ✅ **Approved**: 승인 완료
- 📋 **Draft**: 작성됨, 승인 대기
- ⏸️ **Waiting**: 아직 작성 안 됨

### Tasks 상태
- 📋 **In Progress**: 실행 중 (1%~99%)
- ✅ **Completed**: 완료 (100%)
- ⏸️ **Waiting**: 아직 시작 안 함

---

## 주의사항

- 이 명령어는 **조회만** 하며, 변경하지 않음
- Archive된 spec은 표시되지 않음 (`.claude/memories/9-archive/specs/` 제외)

---

## 예시

### 입력
```
/spec-list
```

### 출력 (예시 1: 여러 Spec 진행 중)
```
📋 Active Specs (3개)

| Feature | Phase | Requirements | Design | Tasks | Progress |
|---------|-------|--------------|--------|-------|----------|
| pet-system | 4 | ✅ Approved | ✅ Approved | 📋 In Progress | 8/20 (40%) |
| pvp-arena | 3 | ✅ Approved | ✅ Approved | ⏸️ Waiting | 0/25 (0%) |
| guild-system | 2 | ✅ Approved | 📋 Draft | ⏸️ Waiting | 0/0 (0%) |

📊 Overall Progress: 8/45 tasks completed across all specs

Next actions:
- pet-system: /spec-execute pet-system 2.3 (continue)
- pvp-arena: /spec-execute pvp-arena 1.1 (start)
- guild-system: Review design.md → approve → /spec-tasks guild-system
```

### 출력 (예시 2: 빈 상태)
```
📋 Active Specs (0개)

No active specs found.

To start a new feature:
/spec-init {feature-name}

Example:
/spec-init pet-system
```
