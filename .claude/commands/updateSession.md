# Update Session Memory

현재 진행 중인 작업을 세션 메모리에 기록합니다.

---

## 사용법

```bash
/updateSession "완료한 작업 또는 의사결정 내용"
```

---

## 작업 순서

### 1. 세션 파일 확인

오늘 날짜의 세션 파일이 있는지 확인:
- `.claude/memories/2-session/daily-{오늘날짜}.md`

**없으면**: `/startSession`을 먼저 실행하라고 안내

### 2. 업데이트 내용 분석

사용자가 입력한 메시지를 분석해서 적절한 섹션에 추가:

**API/DB 변경** (예: "스킬 API 3개 완료"):
→ "⚙️ 진행 중" 섹션 업데이트
```markdown
### 스킬 시스템 - API 구현

**상태**: 🟢 완료

**변경사항**:
- API: POST /api/skills/gacha
- API: GET /api/skills
- API: PUT /api/skills/levelup
```

**의사결정** (예: "SSR 확률 1%로 결정"):
→ "🤝 의사결정 & 협업" 섹션의 "결정 사항"에 추가
```markdown
### 결정 사항
- **SSR 확률**: 1% (Legendary)
```

**TODO(human)** (예: "CalculatePetBuff() 협업 대기"):
→ "🤝 의사결정 & 협업" 섹션의 "TODO(human) 대기 중"에 추가
```markdown
### TODO(human) 대기 중
- [ ] `CalculatePetBuff()` 구현 - 펫 스탯 버프 계산 로직
  - 위치: IdleRPG.Domain/Services/PetService.cs:TODO(human)
```

**이슈 발견** (예: "N+1 쿼리 문제 발견"):
→ "🐛 발견된 이슈" 섹션에 추가

**메모/인사이트** (예: "ValueObject 패턴 유용함"):
→ "💡 메모 & 인사이트" 섹션에 추가

### 3. 코드베이스 스캔 (선택적)

더 정확한 기록을 위해 다음 정보를 자동으로 수집:

- 최근 수정된 파일 (Git diff)
- 새로 추가된 API 엔드포인트 (Controller 스캔)
- TODO(human) 주석 위치 (Grep)

### 4. 세션 통계 업데이트

"📊 세션 통계" 섹션 자동 업데이트:
```markdown
- **API 엔드포인트**: +3개 (24 → 27)
- **Git Commits**: git log --oneline --since="today" 카운트
```

### 5. 확인 메시지

```
✅ 세션 메모리 업데이트 완료

📝 업데이트 내용:
- [섹션명]: {업데이트 내용}

📂 파일: 2-session/daily-{날짜}.md
```

---

## 사용 예시

### 작업 완료 시
```bash
/updateSession "스킬 가챠 API 3개 엔드포인트 구현 완료"
```

### 의사결정 시
```bash
/updateSession "결정: SSR 1%, SR 9%, R 30%, N 60%"
```

### TODO(human) 남긴 후
```bash
/updateSession "TODO(human): CalculateCriticalDamage() 크리티컬 배율 결정 필요"
```

### 이슈 발견 시
```bash
/updateSession "이슈: Equipment Include 쿼리에서 N+1 문제 발견"
```

### 인사이트 기록
```bash
/updateSession "메모: DifficultyMultiplier ValueObject로 던전 난이도 계산이 깔끔해짐"
```

---

## 호출 타이밍 권장사항

### 반드시 호출
- ✅ 시스템 또는 주요 기능 완료 시
- ✅ 중요한 설계 결정 후
- ✅ TODO(human) 남긴 직후
- ✅ Critical 이슈 발견 시

### 선택적 호출
- 🤔 작은 버그 수정
- 🤔 단순 리팩토링
- 🤔 문서 수정

### 불필요
- ❌ 오타 수정
- ❌ 주석 추가
- ❌ 코드 포맷팅

---

## Tip

하루에 3-5번 정도 호출하는 것이 이상적입니다:
1. 오전 작업 완료 후
2. 점심 전
3. 오후 중간
4. 하루 마무리 전 (`/endSession` 전에 마지막 업데이트)
