# Troubleshooting: SessionStart Hook 목표 삽입 에러

**날짜**: 2025-10-21
**이슈**: SessionStart hook이 실행되지만 세션 파일에 placeholder가 남아있음
**상태**: ✅ 해결 완료

---

## 🔍 문제 발견

### 증상
- `.claude/hooks/auto-start-session.sh` 실행 시 세션 파일 생성됨
- 하지만 목표 섹션에 `{목표1}`, `{목표2}`, `{목표3}` placeholder가 그대로 남음

### 예상 동작
```markdown
## 🎯 오늘의 목표

> `1-current/status.md`의 "다음 우선순위" 기반

1. **스킬 가챠 API**: SkillTemplate Seeder, POST /api/skills/gacha, Unity 문서
2. **Drop System**: 던전 클리어 시 Equipment 드랍, 드랍 확률 테이블
3. **Combat-Dungeon 통합**: DungeonStage Monster 스탯 적용, BattleLog DungeonStageId 활용
```

### 실제 결과
```markdown
## 🎯 오늘의 목표

> `1-current/status.md`의 "다음 우선순위" 기반

1. [ ] {목표1}
2. [ ] {목표2}
3. [ ] {목표3}
```

---

## 🧪 디버깅 과정

### 1단계: 목표 추출 검증
```bash
sed -n '/### Immediate/,/^$/p' .claude/memories/1-current/status.md | grep -E "^[0-9]\." | head -3
```

**결과**: ✅ 목표 추출은 정상 작동
```
1. **스킬 가챠 API**: SkillTemplate Seeder, POST /api/skills/gacha, Unity 문서
2. **Drop System**: 던전 클리어 시 Equipment 드랍, 드랍 확률 테이블
3. **Combat-Dungeon 통합**: DungeonStage Monster 스탯 적용, BattleLog DungeonStageId 활용
```

### 2단계: awk 스크립트 로직 분석

**기존 코드 (문제)**:
```bash
awk -v goals="$GOALS" '
/^> `1-current\/status.md`의 "다음 우선순위" 기반$/ {
    print;
    print "";
    print goals;
    next;
}
/^$/ && prev_goals { prev_goals=0; next; }
/^1\. \[ \] {목표1}$/ || /^2\. \[ \] {목표2}$/ || /^3\. \[ \] {목표3}$/ {
    prev_goals=1;
    next;
}
{ print }
'
```

**문제점**:
- `prev_goals` 플래그 로직이 복잡하고 불안정
- 빈 줄과 placeholder 라인의 순서에 민감
- placeholder 라인을 정확히 매칭하지 못하면 제거 실패

### 3단계: 테스트 스크립트 작성

```bash
cat > /tmp/test-awk2.sh << 'EOF'
#!/bin/bash
GOALS="1. **스킬 가챠 API**: SkillTemplate Seeder, POST /api/skills/gacha, Unity 문서
2. **Drop System**: 던전 클리어 시 Equipment 드랍, 드랍 확률 테이블
3. **Combat-Dungeon 통합**: DungeonStage Monster 스탯 적용, BattleLog DungeonStageId 활용"

awk -v goals="$GOALS" '
BEGIN { in_goals = 0 }
/^> `1-current\/status.md`의 "다음 우선순위" 기반$/ {
    print;
    print "";
    print goals;
    in_goals = 1;
    next;
}
in_goals && /^---$/ {
    in_goals = 0;
    print;
    next;
}
in_goals {
    next;
}
{ print }
' .claude/templates/session-template.md
EOF
bash /tmp/test-awk2.sh | grep -A 8 "오늘의 목표"
```

**결과**: ✅ 새로운 로직으로 정상 작동 확인

---

## ✅ 해결 방법

### 개선된 awk 스크립트

```bash
awk -v goals="$GOALS" '
BEGIN { in_goals = 0 }
/^> `1-current\/status.md`의 "다음 우선순위" 기반$/ {
    print;
    print "";
    print goals;
    in_goals = 1;
    next;
}
in_goals && /^---$/ {
    in_goals = 0;
    print;
    next;
}
in_goals {
    next;
}
{ print }
' "$SESSION_FILE" > "$TEMP_FILE" && mv "$TEMP_FILE" "$SESSION_FILE"
```

### 핵심 개선 사항

1. **섹션 범위 추적**: `in_goals` 플래그로 목표 섹션 시작부터 `---` 구분선까지 범위 명확히 추적
2. **무조건 건너뛰기**: `in_goals` 상태일 때 모든 라인(빈 줄 + placeholder)을 건너뛰고 실제 목표만 삽입
3. **안정성**: 플레이스홀더 개수, 형식, 순서와 무관하게 작동

### 동작 원리

```
1. `> 1-current/status.md...` 라인 만나면:
   - 해당 라인 출력
   - 빈 줄 출력
   - 실제 목표 출력
   - in_goals = 1 (섹션 시작)

2. in_goals == 1 상태에서:
   - `---` 만나면: in_goals = 0, `---` 출력 (섹션 종료)
   - 그 외 모든 라인: 건너뛰기 (placeholder 제거)

3. in_goals == 0 상태에서:
   - 모든 라인 그대로 출력
```

---

## 🧪 검증

### 테스트 실행
```bash
rm -f .claude/memories/2-session/daily-2025-10-21.md
echo '{"session_id":"test123"}' | bash .claude/hooks/auto-start-session.sh
```

### 결과 확인
```bash
cat .claude/memories/2-session/daily-2025-10-21.md | head -15
```

**출력**:
```markdown
# Daily Session: 2025-10-21

**시작 시간**: 21:23
**Week 3 Day 2**

---

## 🎯 오늘의 목표

> `1-current/status.md`의 "다음 우선순위" 기반

1. **스킬 가챠 API**: SkillTemplate Seeder, POST /api/skills/gacha, Unity 문서
2. **Drop System**: 던전 클리어 시 Equipment 드랍, 드랍 확률 테이블
3. **Combat-Dungeon 통합**: DungeonStage Monster 스탯 적용, BattleLog DungeonStageId 활용
---
```

✅ **성공**: Placeholder 없이 실제 목표만 정상 삽입됨

---

## 📝 배운 점 (Lessons Learned)

### 1. Shell Script 디버깅 전략
- **문제**: Hook 스크립트 전체를 한 번에 디버깅하기 어려움
- **해결**: 작은 테스트 스크립트(`/tmp/test-awk.sh`)로 문제가 되는 부분만 분리해서 검증
- **효과**: awk 로직의 문제를 빠르게 파악하고 수정 가능

### 2. awk 상태 플래그 패턴
- **문제**: 복잡한 조건 분기로 인한 로직 불안정
- **해결**: `in_goals` 같은 상태 플래그로 섹션 범위를 명확히 추적
- **효과**: 입력 변화에 강건하고 유지보수 쉬운 코드

### 3. 구분선 활용
- **문제**: Placeholder 개수나 형식이 변경되면 로직 깨짐
- **해결**: Markdown 구분선(`---`)을 종료 마커로 활용
- **효과**: 섹션 내용과 무관하게 안정적으로 작동

### 4. Windows Git Bash 호환성
- **기존**: 임시 파일 없이 in-place 수정 시도 → 권한 에러
- **개선**: `$TEMP_FILE` 사용 → `sed ... > TEMP && mv TEMP ...` 패턴
- **효과**: Windows 환경에서도 안정적 동작

---

## 🔄 관련 커밋

```
commit 561f262
fix(hook): SessionStart 목표 삽입 awk 로직 개선

- in_goals 플래그로 목표 섹션 범위 추적
- --- 구분선을 종료 마커로 사용해 섹션 전체 교체
- 플레이스홀더 개수와 무관하게 안정적 처리
```

---

## 🔗 참고 파일

- **Hook 스크립트**: `.claude/hooks/auto-start-session.sh`
- **템플릿**: `.claude/templates/session-template.md`
- **목표 소스**: `.claude/memories/1-current/status.md` (### Immediate 섹션)
- **로그**: `.claude/hooks/session-start.log`
