# Start Daily Session

하루 작업을 시작하고 일일 세션 메모리를 생성합니다.

---

## 작업 순서

### 1. 오늘 날짜 확인

현재 날짜를 `YYYY-MM-DD` 형식으로 확인하세요.

### 2. 세션 파일 생성

`.claude/memories/2-session/daily-{날짜}.md` 파일을 생성하세요.

**템플릿**: `.claude/templates/session-template.md` 복사 후 다음 항목 채우기:

- `{DATE}`: 오늘 날짜 (2025-10-21)
- `{TIME}`: 현재 시간 (14:30)
- `{X}`, `{Y}`: Week 번호, Day 번호 (roadmap.md 참조)

### 3. 오늘의 목표 설정

`.claude/memories/1-current/status.md`의 "다음 우선순위" 섹션에서 상위 3개 항목을 복사하여 "🎯 오늘의 목표"에 작성하세요.

**예시**:
```markdown
## 🎯 오늘의 목표

1. [ ] 스킬 가챠 API 구현 (POST /api/skills/gacha)
2. [ ] SkillTemplate Seeder 작성
3. [ ] Unity 문서 업데이트 (skills/)
```

### 4. 현재 상태 요약

다음 정보를 출력하세요:

```
✅ 세션 시작: {날짜} {시간}
📂 세션 파일: 2-session/daily-{날짜}.md
📅 진행도: Week {X} Day {Y}

🎯 오늘의 목표 (3개):
1. {목표1}
2. {목표2}
3. {목표3}

💡 Tip: 작업 진행하면서 중요한 결정이나 TODO(human)은 자동으로 세션 메모리에 기록됩니다.
```

---

## 중요 원칙

1. **하루 1 세션**: 같은 날짜 파일이 이미 존재하면 기존 파일 사용
2. **자동 업데이트**: 작업 중 중요 이벤트 발생 시 세션 파일 자동 업데이트
3. **하루 종료**: `/endSession` 명령어로 Current 메모리에 반영

---

## 참고 파일

- 템플릿: `.claude/templates/session-template.md`
- 목표 소스: `.claude/memories/1-current/status.md`
- Week/Day 정보: `.claude/memories/1-current/roadmap.md`
