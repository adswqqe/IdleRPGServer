# /spec-archive - Spec 아카이브

**목적**: 완료된 Spec을 archive로 이동합니다.

---

## 실행 절차

### 1. 인자 확인
- `$ARGS[0]`: feature-name

### 2. 완료 조건 확인
다음 조건 **모두** 만족해야 함:
- ✅ `tasks.md`의 모든 task가 `[x]` 완료
- ✅ Progress Overview가 100%
- ✅ (선택) 테스트 통과 확인
- ✅ (선택) Unity 문서 완성 확인 (API 변경 시)

**조건 불만족 시 에러 메시지 출력 후 중단**

### 3. 메타데이터 추가
`requirements.md` 하단에 완료 정보 추가:
```markdown
---

## Archive Metadata

**Completed**: YYYY-MM-DD
**Duration**: X days
**Total Tasks**: XX
**Files Created**: XX
**Tests**: XX passing
```

### 4. 폴더 이동
```
.claude/memories/specs/{feature-name}/
→ .claude/memories/9-archive/specs/{feature-name}/
```

### 5. 출력
```
✅ {Feature Name} archived successfully

Moved to: .claude/memories/9-archive/specs/{feature-name}/

Summary:
- Total tasks: {개수}
- Duration: {일수} days
- Files created: {개수}
- Tests: {개수} passing

Next feature:
/spec-init {new-feature-name}
```

---

## 주의사항

- ⚠️ **Archive는 되돌릴 수 없음**: 백업 필요 시 git commit 먼저
- ⚠️ **모든 task 완료 필수**: 미완료 시 실행 불가
- ⚠️ **Git 커밋 권장**: Archive 전에 현재 상태 커밋

---

## 예시

### 입력
```
/spec-archive pet-system
```

### 출력 (성공 케이스)
```
✅ Pet System archived successfully

Moved to: .claude/memories/9-archive/specs/pet-system/

Summary:
- Total tasks: 20
- Duration: 5 days (2025-10-20 ~ 2025-10-25)
- Files created: 15
  - Domain: 5 files
  - Infrastructure: 4 files
  - Application: 3 files
  - API: 1 file
  - Tests: 2 files
- Tests: 12 passing (100%)
- Unity Documentation: ✅ Complete

Completed features:
- Pet gacha system with pity counter
- Pet stat buff calculation
- Pet leveling system

To start next feature:
/spec-init skill-equip
```

### 출력 (에러 케이스 1: 미완료 Task)
```
❌ Cannot archive: Tasks not completed

Incomplete tasks:
- 5.3: Create Migration SQL (pending)
- 6.1: Write Unit Tests (pending)
- 6.3: Create Unity Documentation (pending)

Progress: 17/20 (85%)

Complete remaining tasks first:
/spec-execute pet-system 5.3
```

### 출력 (에러 케이스 2: 테스트 실패)
```
❌ Cannot archive: Tests failing

Failed tests:
- PetServiceTests.LevelUpAsync_MaxLevel_ThrowsException
- PetStatBuffServiceTests.CalculateBuff_ZeroLevel_ReturnsBase

Fix tests before archiving.
```

---

## Archive 후 구조

```
.claude/memories/9-archive/specs/
└── pet-system/
    ├── requirements.md (+ Archive Metadata)
    ├── design.md
    ├── tasks.md (모든 task ✅)
    └── work-log.md
```

**조회 방법**:
- Archive된 spec은 `/spec-list`에 표시되지 않음
- 수동으로 `.claude/memories/9-archive/specs/` 폴더 확인
- 참고 자료로 활용 (완료된 사례)
