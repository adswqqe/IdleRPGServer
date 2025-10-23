# /spec-log - Work Log 엔트리 추가

**목적**: work-log.md에 간편하게 로그를 추가합니다.

---

## 실행 절차

### 1. 인자 확인
- `$ARGS[0]`: feature-name
- `$ARGS[1]`: message (따옴표로 감싸기)

### 2. Spec 존재 확인
`.claude/memories/specs/{feature-name}/` 폴더가 존재하는지 확인

### 3. work-log.md 확인/생성
- 파일이 없으면 생성
- 있으면 기존 내용 유지

### 4. 로그 타입 자동 분류
메시지 키워드로 타입 결정:

| 키워드 | 로그 타입 | 포맷 |
|--------|-----------|------|
| "결정", "decision", "선택" | Decision | **Decision**: ... / **Reasoning**: ... |
| "문제", "issue", "에러", "bug" | Issue & Solution | **Issue**: ... / **Solution**: ... |
| "해결", "solution", "fix" | Issue & Solution | **Problem**: ... / **Solution**: ... |
| "완료", "progress", "milestone" | Progress | **Milestone**: ... |
| (기본) | Note | **Note**: ... |

### 5. 타임스탬프 추가
현재 시각으로 자동 추가 (YYYY-MM-DD HH:MM)

### 6. work-log.md에 추가
```markdown
## YYYY-MM-DD HH:MM - {Type}

{Formatted Message}

---
```

### 7. 출력
```
✅ Log entry added to {feature-name}/work-log.md

Timestamp: {YYYY-MM-DD HH:MM}
Type: {Type}

To view: cat .claude/memories/specs/{feature-name}/work-log.md
```

---

## 사용 시점

- ⚠️ **중간 노트 기록**: Task 실행 외 결정사항
- ⚠️ **문제 발견/해결**: 디버깅 과정
- ⚠️ **진행 상황**: Milestone 달성
- ⚠️ `/spec-execute`는 자동으로 work-log 업데이트하므로, 이 명령어는 **수동 기록용**

---

## 예시

### 입력 (Decision)
```
/spec-log pet-system "결정: Pet 버프 공식을 Attack * (1 + 0.05 * PetLevel)로 결정. 레벨당 5% 증가가 밸런스 측면에서 적절"
```

### 출력
```
✅ Log entry added to pet-system/work-log.md

Timestamp: 2025-10-23 15:45
Type: Decision

Entry:
**Decision**: Pet 버프 공식을 Attack * (1 + 0.05 * PetLevel)로 결정
**Reasoning**: 레벨당 5% 증가가 밸런스 측면에서 적절
```

---

### 입력 (Issue & Solution)
```
/spec-log pet-system "문제: PetService.LevelUpAsync에서 MaxLevel 체크 누락. 해결: 레벨 100 초과 시 InvalidOperationException 발생 추가"
```

### 출력
```
✅ Log entry added to pet-system/work-log.md

Timestamp: 2025-10-23 16:20
Type: Issue & Solution

Entry:
**Issue**: PetService.LevelUpAsync에서 MaxLevel 체크 누락
**Solution**: 레벨 100 초과 시 InvalidOperationException 발생 추가
```

---

### 입력 (Note)
```
/spec-log pet-system "PetTemplate에 MaxLevel 필드 추가 고려 중"
```

### 출력
```
✅ Log entry added to pet-system/work-log.md

Timestamp: 2025-10-23 14:15
Type: Note

Entry:
**Note**: PetTemplate에 MaxLevel 필드 추가 고려 중
```

---

### 입력 (Progress)
```
/spec-log pet-system "완료: Domain Layer 전체 작업 완료 (5/5 tasks)"
```

### 출력
```
✅ Log entry added to pet-system/work-log.md

Timestamp: 2025-10-23 17:00
Type: Progress

Entry:
**Milestone**: Domain Layer 전체 작업 완료 (5/5 tasks)
```
