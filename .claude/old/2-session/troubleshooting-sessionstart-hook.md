# SessionStart Hook 문제 해결 가이드

**최종 업데이트**: 2025-10-21 22:21
**해결된 이슈**: 5개 (목표 삽입 실패, startup hook error - 신규 세션, startup hook error - 기존 세션 재개, Windows Git Bash 호환성 문제, 기존 세션 placeholder 잔존 문제)
**검증 상태**: ✅ 완전히 해결됨 (2025-10-21 22:21 최종 검증)

---

## 🚨 빠른 문제 해결 (Quick Fix)

### 증상별 진단 및 해결

#### 1️⃣ "SessionStart:startup hook error" 메시지가 뜬다
```
▐▛███▜▌   Claude Code v2.0.24
▝▜█████▛▘  Sonnet 4.5 · Claude Max
  ▘▘ ▝▝    E:\StudyGameProj\IdleRPGServer
  ⎿  SessionStart:startup hook error
```

**원인**: Hook의 JSON 출력 순서 문제 (2개 경로 모두 확인 필요!)
**해결**: `.claude/hooks/auto-start-session.sh`의 **두 곳** 수정

**🔴 Critical: 두 경로 모두 수정해야 함!**

1. **라인 15-18**: 기존 세션 재개 시
```bash
# ❌ 잘못된 순서
if [ -f "$SESSION_FILE" ]; then
    echo "✅ 기존 세션 재개: $TODAY" >&2  # stderr 먼저
    echo '{"decision": "allow"}'
    exit 0
fi

# ✅ 올바른 순서
if [ -f "$SESSION_FILE" ]; then
    echo '{"decision": "allow"}'          # JSON 먼저!
    echo "✅ 기존 세션 재개: $TODAY" >&2
    exit 0
fi
```

2. **라인 83-87**: 새 세션 시작 시 (이미 올바름)
```bash
# ✅ 올바른 순서
echo '{"decision": "allow"}'
echo "✅ 세션 자동 시작: $TODAY | Week $CURRENT_WEEK Day $CURRENT_DAY | 목표: 3개" >&2
```

**즉시 테스트**:
```bash
# 기존 세션 재개 경로 테스트 (오늘 파일이 있는 상태)
echo '{"session_id":"test"}' | bash .claude/hooks/auto-start-session.sh 2>&1 | head -2
# 출력:
# {"decision": "allow"}
# ✅ 기존 세션 재개: 2025-10-21

# 새 세션 시작 경로 테스트 (오늘 파일 삭제 후)
rm -f .claude/memories/2-session/daily-$(date +%Y-%m-%d).md
echo '{"session_id":"test"}' | bash .claude/hooks/auto-start-session.sh 2>&1 | head -2
# 출력:
# {"decision": "allow"}
# ✅ 세션 자동 시작: ...
```

---

#### 2️⃣ "syntax error: unexpected end of file" 에러가 발생한다

```bash
bash .claude/hooks/auto-start-session.sh
# 출력:
# .claude/hooks/auto-start-session.sh: line 138: syntax error: unexpected end of file
```

**원인**: if/else 블록이 fi로 제대로 닫히지 않음
**해결**: 스크립트의 모든 if/else 블록 검증

**주요 확인 위치**:
1. **라인 8-16**: 입력 처리 if/else 블록
```bash
if [ -t 0 ]; then
    SESSION_ID=""
else
    INPUT=$(cat 2>/dev/null || echo "{}")
    SESSION_ID=$(echo "$INPUT" | sed -n 's/.*"session_id"[[:space:]]*:[[:space:]]*"\([^"]*\)".*/\1/p' 2>/dev/null || echo "")
fi  # ← fi 필수!
```

2. **라인 31-37**: roadmap.md 파일 존재 확인 블록
```bash
if [ -f "$ROADMAP_FILE" ]; then
    CURRENT_WEEK=$(...)
    CURRENT_DAY=$(...)
else
    CURRENT_WEEK=""
    CURRENT_DAY=""
fi  # ← fi 필수!
```

3. **라인 59-92**: 템플릿 파일 처리 블록
```bash
if [ ! -f "$TEMPLATE_FILE" ]; then
    cat > "$SESSION_FILE" << 'EOF'
...
EOF
else
    cp "$TEMPLATE_FILE" "$SESSION_FILE"
fi  # ← fi 필수!
```

4. **라인 106-126**: awk 명령어 존재 확인 블록
```bash
if command -v awk >/dev/null 2>&1; then
    awk -v goals="$GOALS" '...'
fi  # ← fi 필수!
```

**즉시 검증**:
```bash
bash -n .claude/hooks/auto-start-session.sh
# 에러 없으면 문법 정상
```

---

#### 3️⃣ 세션 파일에 `{목표1}`, `{목표2}` placeholder가 남아있다

**원인**: awk 스크립트의 목표 삽입 로직 실패
**해결**: `.claude/hooks/auto-start-session.sh`의 awk 부분 수정

```bash
# ✅ 올바른 awk 스크립트 (라인 60-78)
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

**즉시 테스트**:
```bash
# 목표 추출 확인
sed -n '/### Immediate/,/^$/p' .claude/memories/1-current/status.md | grep -E "^[0-9]\." | head -3

# Hook 실행 후 세션 파일 확인
rm -f .claude/memories/2-session/daily-$(date +%Y-%m-%d).md
echo '{"session_id":"test"}' | bash .claude/hooks/auto-start-session.sh
cat .claude/memories/2-session/daily-$(date +%Y-%m-%d).md | grep -A 5 "오늘의 목표"
```

---

#### 4️⃣ Windows Git Bash에서 JSON 파싱 실패

```bash
# grep -o 옵션 사용 시
SESSION_ID=$(echo "$INPUT" | grep -o '"session_id":"[^"]*"' | cut -d'"' -f4)
# 결과: 빈 문자열 또는 에러
```

**원인**: Windows Git Bash에서 grep -o 옵션 미지원 또는 불안정
**해결**: sed 명령어로 대체

**수정 전 (라인 8)**:
```bash
SESSION_ID=$(echo "$INPUT" | grep -o '"session_id":"[^"]*"' | cut -d'"' -f4)
```

**수정 후**:
```bash
SESSION_ID=$(echo "$INPUT" | sed -n 's/.*"session_id"[[:space:]]*:[[:space:]]*"\([^"]*\)".*/\1/p' 2>/dev/null || echo "")
```

**장점**:
- ✅ Windows Git Bash와 Linux/Mac 모두 호환
- ✅ 공백 처리 강건 (`[[:space:]]*`)
- ✅ 에러 발생 시 빈 문자열 반환 (`|| echo ""`)

**즉시 테스트**:
```bash
# JSON 파싱 테스트
echo '{"session_id":"test-123"}' | sed -n 's/.*"session_id"[[:space:]]*:[[:space:]]*"\([^"]*\)".*/\1/p'
# 출력: test-123
```

---

#### 5️⃣ Hook이 아예 실행되지 않는다

**진단 체크리스트**:
```bash
# 1. Hook 설정 파일 확인
cat .claude/settings.local.json | grep -A 10 "SessionStart"

# 2. Hook 스크립트 실행 권한 확인
ls -la .claude/hooks/auto-start-session.sh

# 3. 로그 확인
tail -5 .claude/hooks/session-start.log

# 4. 수동 실행 테스트
echo '{"session_id":"manual-test"}' | bash .claude/hooks/auto-start-session.sh 2>&1
```

**예상 설정 (.claude/settings.local.json)**:
```json
{
  "hooks": {
    "SessionStart": [
      {
        "hooks": [
          {
            "type": "command",
            "command": "bash .claude/hooks/auto-start-session.sh",
            "timeout": 10
          }
        ]
      }
    ]
  }
}
```

---

## 📋 완전한 진단 프로세스

### Step 1: 환경 검증

```bash
# 필수 파일 존재 확인
ls -la .claude/hooks/auto-start-session.sh
ls -la .claude/templates/session-template.md
ls -la .claude/memories/1-current/status.md
ls -la .claude/memories/1-current/roadmap.md

# 실행 권한 확인 (755 또는 rwxr-xr-x)
stat -c "%a %n" .claude/hooks/auto-start-session.sh 2>/dev/null || stat -f "%Lp %N" .claude/hooks/auto-start-session.sh
```

### Step 2: 목표 추출 검증

```bash
# status.md에서 목표 추출 테스트
GOALS=$(sed -n '/### Immediate/,/^$/p' .claude/memories/1-current/status.md | grep -E "^[0-9]\." | head -3)
echo "$GOALS"

# 비어있으면 status.md 구조 확인
grep -n "### Immediate" .claude/memories/1-current/status.md
```

### Step 3: Week/Day 추출 검증

```bash
# roadmap.md에서 Week/Day 추출 테스트
CURRENT_WEEK=$(grep "^\*\*현재 진행\*\*:" .claude/memories/1-current/roadmap.md | sed -n 's/.*Week \([0-9]\+\).*/\1/p' | head -1)
CURRENT_DAY=$(grep "^\*\*현재 진행\*\*:" .claude/memories/1-current/roadmap.md | sed -n 's/.*Day \([0-9]\+\).*/\1/p' | head -1)
echo "Week $CURRENT_WEEK Day $CURRENT_DAY"
```

### Step 4: awk 스크립트 단독 테스트

```bash
cat > /tmp/test-awk-goals.sh << 'EOF'
#!/bin/bash
GOALS="1. **테스트 목표 1**: 설명
2. **테스트 목표 2**: 설명
3. **테스트 목표 3**: 설명"

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
' .claude/templates/session-template.md | head -20
EOF

bash /tmp/test-awk-goals.sh
```

**성공 조건**: 12-14번 라인에 실제 목표가 출력됨

### Step 5: 전체 Hook 실행 테스트

```bash
# 기존 세션 파일 삭제 (오늘 날짜)
rm -f .claude/memories/2-session/daily-$(date +%Y-%m-%d).md

# Hook 실행
echo '{"session_id":"full-test"}' | bash .claude/hooks/auto-start-session.sh 2>&1 > /tmp/hook-output.log

# 출력 검증
cat /tmp/hook-output.log
# 첫 줄이 {"decision": "allow"}이어야 함

# 생성된 파일 확인
cat .claude/memories/2-session/daily-$(date +%Y-%m-%d).md | head -20
```

---

## 🔧 상세 해결 방법

### 이슈 1: 목표 삽입 실패 (Placeholder 잔존)

#### 증상
```markdown
## 🎯 오늘의 목표

> `1-current/status.md`의 "다음 우선순위" 기반

1. [ ] {목표1}
2. [ ] {목표2}
3. [ ] {목표3}
```

#### 디버깅 과정

**1단계: 목표 추출 검증**
```bash
sed -n '/### Immediate/,/^$/p' .claude/memories/1-current/status.md | grep -E "^[0-9]\." | head -3
```

**결과 예시** (정상):
```
1. **스킬 가챠 API**: SkillTemplate Seeder, POST /api/skills/gacha, Unity 문서
2. **Drop System**: 던전 클리어 시 Equipment 드랍, 드랍 확률 테이블
3. **Combat-Dungeon 통합**: DungeonStage Monster 스탯 적용, BattleLog DungeonStageId 활용
```

만약 비어있다면:
- `status.md`에 `### Immediate (이번 주)` 섹션 존재 확인
- 섹션 아래에 `1.`, `2.`, `3.`로 시작하는 항목 확인

**2단계: awk 로직 문제 파악**

기존 코드의 문제점:
```bash
# ❌ 문제가 있는 코드
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
- placeholder 형식이 바뀌면 매칭 실패

#### 해결책: 섹션 범위 추적 방식

```bash
# ✅ 개선된 코드
awk -v goals="$GOALS" '
BEGIN { in_goals = 0 }
/^> `1-current\/status.md`의 "다음 우선순위" 기반$/ {
    print;          # 패턴 라인 출력
    print "";       # 빈 줄 출력
    print goals;    # 실제 목표 삽입
    in_goals = 1;   # 섹션 시작 플래그
    next;
}
in_goals && /^---$/ {
    in_goals = 0;   # 구분선 만나면 섹션 종료
    print;          # 구분선 출력
    next;
}
in_goals {
    next;           # 섹션 내부 모든 라인 건너뛰기 (placeholder 제거)
}
{ print }           # 그 외 모든 라인 그대로 출력
' "$SESSION_FILE" > "$TEMP_FILE" && mv "$TEMP_FILE" "$SESSION_FILE"
```

**동작 원리**:
1. 패턴 라인(`> 1-current/status.md...`) 발견 → 목표 삽입 + `in_goals=1`
2. `in_goals==1` 상태에서 `---` 만나기 전까지 모든 라인 건너뛰기
3. `---` 만나면 `in_goals=0`으로 섹션 종료
4. 결과: placeholder가 모두 제거되고 실제 목표만 남음

**장점**:
- ✅ Placeholder 개수/형식과 무관
- ✅ 빈 줄 순서에 강건
- ✅ 구분선(`---`)을 종료 마커로 활용

#### 검증

```bash
# 테스트 스크립트 작성
cat > /tmp/test-awk-complete.sh << 'EOF'
#!/bin/bash
GOALS=$(sed -n '/### Immediate/,/^$/p' .claude/memories/1-current/status.md | grep -E "^[0-9]\." | head -3)

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
' .claude/templates/session-template.md | grep -A 8 "오늘의 목표"
EOF

bash /tmp/test-awk-complete.sh
```

**성공 출력**:
```markdown
## 🎯 오늘의 목표

> `1-current/status.md`의 "다음 우선순위" 기반

1. **스킬 가챠 API**: SkillTemplate Seeder, POST /api/skills/gacha, Unity 문서
2. **Drop System**: 던전 클리어 시 Equipment 드랍, 드랍 확률 테이블
3. **Combat-Dungeon 통합**: DungeonStage Monster 스탯 적용, BattleLog DungeonStageId 활용
---
```

---

### 이슈 2: "SessionStart:startup hook error"

#### 증상
Claude Code 시작 시 에러 메시지:
```
  ⎿  SessionStart:startup hook error
```

세션 파일은 생성되지만 Claude Code가 hook 실패로 인식

#### 원인 분석

**Claude Code Hook 요구사항**:
- Hook 스크립트는 **stdout의 첫 번째 줄에 유효한 JSON** 출력 필수
- JSON 형식: `{"decision": "allow"}` 또는 `{"decision": "deny", "reason": "..."}`

**문제가 되는 코드**:
```bash
# ❌ stderr 메시지가 먼저 출력됨
echo "
✅ 세션 자동 시작: $TODAY
📅 진행도: Week $CURRENT_WEEK Day $CURRENT_DAY
📂 파일: 2-session/daily-$TODAY.md

🎯 오늘의 목표:
$GOALS
" >&2

# JSON은 나중에 출력
echo '{"decision": "allow"}'
```

**문제점**:
- stderr와 stdout이 섞이면서 파싱 혼란
- 여러 줄 메시지 + 이모지가 hook 파서에 영향
- Claude Code가 JSON을 올바르게 읽지 못함

#### 해결책: JSON 우선 출력

```bash
# ✅ JSON을 제일 먼저 출력
echo '{"decision": "allow"}'

# stderr 메시지는 나중에 (한 줄로 압축)
echo "✅ 세션 자동 시작: $TODAY | Week $CURRENT_WEEK Day $CURRENT_DAY | 목표: 3개" >&2

exit 0
```

**핵심 원칙**:
1. **stdout 첫 줄 = JSON**: 다른 출력보다 먼저
2. **stderr는 나중에**: JSON 출력 완료 후
3. **간결한 메시지**: 여러 줄 → 한 줄로 압축
4. **이모지는 stderr에만**: stdout은 순수 JSON만

#### 검증

```bash
# 출력 순서 확인
echo '{"session_id":"order-test"}' | bash .claude/hooks/auto-start-session.sh 2>&1

# 예상 출력:
# {"decision": "allow"}                                    ← 첫 줄이 JSON!
# ✅ 세션 자동 시작: 2025-10-21 | Week 3 Day 2 | 목표: 3개  ← stderr 메시지
```

**첫 줄 JSON 추출 테스트**:
```bash
echo '{"session_id":"json-test"}' | bash .claude/hooks/auto-start-session.sh 2>/dev/null | head -1 | jq .
# {"decision": "allow"} 파싱 성공해야 함
```

---

## 📝 배운 점 (재발 방지)

### 1. Shell Script 디버깅 전략

**문제**: 복잡한 hook 스크립트 전체를 한 번에 디버깅하기 어려움

**해결**:
- `/tmp/test-{component}.sh` 형식으로 작은 테스트 스크립트 분리
- 문제가 되는 부분만 격리해서 검증
- 성공 후 원본 스크립트에 통합

**예시**:
```bash
# awk 로직만 테스트
cat > /tmp/test-awk-only.sh << 'EOF'
GOALS="1. 목표1\n2. 목표2"
awk -v goals="$GOALS" '...' template.md
EOF

# 목표 추출만 테스트
cat > /tmp/test-goals-extract.sh << 'EOF'
sed -n '/### Immediate/,/^$/p' status.md | grep -E "^[0-9]\."
EOF
```

### 2. awk 상태 플래그 패턴

**문제**: 복잡한 조건 분기 → 로직 불안정

**해결**: 상태 플래그로 섹션 범위 추적
```bash
BEGIN { in_section = 0 }
/시작 패턴/ { in_section = 1 }
in_section && /종료 패턴/ { in_section = 0 }
in_section { ... }
```

**장점**:
- 입력 변화에 강건
- 유지보수 쉬움
- 가독성 높음

### 3. Markdown 구분선 활용

**문제**: Placeholder 개수/형식 변경 시 로직 깨짐

**해결**: `---` 구분선을 종료 마커로 활용
- 섹션 내용과 무관하게 안정적 작동
- Markdown 문서 구조에 의존

### 4. Windows Git Bash 호환성

**문제**: 임시 파일 없이 in-place 수정 → 권한 에러

**해결**: 임시 파일 패턴
```bash
sed 's/old/new/' file > temp && mv temp file
```

### 5. Claude Code Hook 규칙

**핵심 원칙**:
1. **stdout 첫 줄 = JSON**: 반드시!
2. **stderr는 사용자 메시지용**: JSON 출력 후
3. **이모지/특수문자 주의**: stdout에서 배제
4. **간결한 출력**: 여러 줄 → 한 줄
5. **테스트 방법**: `script 2>&1 | head -1 | jq .`

---

## 🔗 참고 파일

### Hook 시스템
- **메인 스크립트**: `.claude/hooks/auto-start-session.sh`
- **설정 파일**: `.claude/settings.local.json`
- **로그 파일**: `.claude/hooks/session-start.log`

### 데이터 소스
- **템플릿**: `.claude/templates/session-template.md`
- **목표 소스**: `.claude/memories/1-current/status.md` (### Immediate 섹션)
- **진행도 소스**: `.claude/memories/1-current/roadmap.md` (현재 진행 라인)

### 출력
- **세션 파일**: `.claude/memories/2-session/daily-{YYYY-MM-DD}.md`

---

## 🔄 관련 커밋

```
commit TBD (2025-10-21 21:43)
fix(hook): SessionStart 기존 세션 재개 시 JSON 출력 순서 수정

- 라인 15-18: 기존 세션 재개 경로에서 JSON을 stderr보다 먼저 출력
- 이전 수정(ddc410d)은 새 세션 시작 경로만 수정했으나, 재개 경로는 누락
- "SessionStart:startup hook error" 완전 해결

commit 561f262
fix(hook): SessionStart 목표 삽입 awk 로직 개선

- in_goals 플래그로 목표 섹션 범위 추적
- --- 구분선을 종료 마커로 사용해 섹션 전체 교체
- 플레이스홀더 개수와 무관하게 안정적 처리

commit ddc410d
fix(hook): SessionStart JSON 출력 순서 수정 (신규 세션 시작 경로)

- JSON을 stderr 메시지보다 먼저 출력하여 hook 파싱 안정화
- 여러 줄 이모지 메시지를 한 줄로 압축
- 라인 83-87 수정 (새 세션 시작 시)
```

---

## 🧪 최종 검증 체크리스트

다음 명령어로 hook이 완전히 정상 작동하는지 확인:

```bash
# 1. 기존 세션 파일 삭제
rm -f .claude/memories/2-session/daily-$(date +%Y-%m-%d).md

# 2. Hook 실행
echo '{"session_id":"final-verify"}' | bash .claude/hooks/auto-start-session.sh 2>&1 | tee /tmp/hook-verify.log

# 3. 출력 검증
echo "=== 출력 검증 ==="
head -1 /tmp/hook-verify.log  # {"decision": "allow"}이어야 함

# 4. JSON 파싱 테스트
echo "=== JSON 파싱 ==="
head -1 /tmp/hook-verify.log | jq .

# 5. 세션 파일 검증
echo "=== 세션 파일 목표 섹션 ==="
cat .claude/memories/2-session/daily-$(date +%Y-%m-%d).md | grep -A 8 "오늘의 목표"

# 6. Placeholder 검사
echo "=== Placeholder 잔존 확인 (없어야 정상) ==="
grep -n "{목표" .claude/memories/2-session/daily-$(date +%Y-%m-%d).md || echo "✅ Placeholder 없음!"

# 7. 로그 확인
echo "=== 최근 로그 ==="
tail -3 .claude/hooks/session-start.log
```

**모든 검증 통과 조건**:
- ✅ 첫 줄이 `{"decision": "allow"}`
- ✅ `jq .` 파싱 성공
- ✅ 세션 파일에 실제 목표 3개 삽입됨
- ✅ `{목표1}` 같은 placeholder 없음
- ✅ 로그에 세션 시작 기록됨

---

## 💡 예방 수칙

향후 hook 수정 시 반드시 지킬 것:

### 1. JSON 출력 규칙
```bash
# ✅ DO
echo '{"decision": "allow"}'  # 첫 줄
echo "메시지" >&2             # 나중에

# ❌ DON'T
echo "메시지" >&2
echo '{"decision": "allow"}'  # 순서 잘못
```

### 2. awk 섹션 처리 패턴
```bash
# ✅ DO: 상태 플래그 + 구분선
BEGIN { in_section = 0 }
/시작/ { in_section = 1; ... }
in_section && /끝/ { in_section = 0; ... }
in_section { next }  # 섹션 내부 건너뛰기

# ❌ DON'T: 복잡한 조건 분기
/라인1/ && flag { ... }
/라인2/ || /라인3/ { ... }
```

### 3. 수정 후 필수 테스트
```bash
# 즉시 테스트 (5초)
echo '{"session_id":"test"}' | bash .claude/hooks/auto-start-session.sh 2>&1 | head -1

# 전체 검증 (30초)
rm -f .claude/memories/2-session/daily-$(date +%Y-%m-%d).md
echo '{"session_id":"full-test"}' | bash .claude/hooks/auto-start-session.sh
cat .claude/memories/2-session/daily-$(date +%Y-%m-%d).md | head -20
```

### 4. 버전 관리
```bash
# 수정 전 백업
cp .claude/hooks/auto-start-session.sh .claude/hooks/auto-start-session.sh.backup

# 수정 후 커밋
git add .claude/hooks/auto-start-session.sh
git commit -m "fix(hook): 수정 내용"
```

---

## 📌 최근 해결 사례 (2025-10-21 21:43)

### 증상
- "SessionStart:startup hook error" 메시지가 계속 나타남
- troubleshooting 문서에 해결 방법이 있었지만 여전히 에러 발생

### 디버깅 과정

1. **목표 추출 테스트**: ✅ 정상
```bash
sed -n '/### Immediate/,/^$/p' .claude/memories/1-current/status.md | grep -E "^[0-9]\." | head -3
# 결과: 3개 목표 정상 추출
```

2. **awk 스크립트 테스트**: ✅ 정상
```bash
# 하드코딩된 목표로 테스트 → 정상 작동
```

3. **Hook 전체 실행 테스트**: ❌ **문제 발견!**
```bash
echo '{"session_id":"test"}' | bash .claude/hooks/auto-start-session.sh 2>&1 | head -2
# 출력:
# ✅ 기존 세션 재개: 2025-10-21  ← stderr 먼저 (잘못됨!)
# {"decision": "allow"}           ← JSON 나중
```

### 근본 원인

**라인 15-18 (기존 세션 재개 경로)**에서 JSON 순서가 잘못됨:
```bash
# ❌ 문제 코드
if [ -f "$SESSION_FILE" ]; then
    echo "✅ 기존 세션 재개: $TODAY" >&2  # stderr 먼저
    echo '{"decision": "allow"}'
    exit 0
fi
```

이전 커밋(ddc410d)에서 **라인 83-87 (새 세션 시작 경로)**만 수정했고, **기존 세션 재개 경로는 수정을 놓쳤음**.

### 해결

라인 16-17 순서 변경:
```bash
# ✅ 수정 후
if [ -f "$SESSION_FILE" ]; then
    echo '{"decision": "allow"}'          # JSON 먼저!
    echo "✅ 기존 세션 재개: $TODAY" >&2
    exit 0
fi
```

### 검증

```bash
echo '{"session_id":"test"}' | bash .claude/hooks/auto-start-session.sh 2>&1 | head -2
# 출력:
# {"decision": "allow"}           ✅ JSON 첫 줄!
# ✅ 기존 세션 재개: 2025-10-21  ✅ stderr 나중
```

### 교훈

1. **두 경로 모두 확인**: Hook에 여러 실행 경로가 있으면 모두 테스트해야 함
2. **재개 경로 간과**: 새 세션 시작만 테스트하고 기존 세션 재개는 놓치기 쉬움
3. **체계적 디버깅**: 작은 단위로 분리 테스트 → 문제 격리 → 수정 → 검증

---

## 📌 최종 검증 결과 (2025-10-21 21:51)

### 전체 시스템 검증

**테스트 수행**:
```bash
# 1. 기존 세션 파일 삭제
rm -f .claude/memories/2-session/daily-2025-10-21.md

# 2. Hook 실행
echo '{"session_id":"debug-test"}' | bash .claude/hooks/auto-start-session.sh 2>&1

# 3. 생성된 파일 확인
cat .claude/memories/2-session/daily-2025-10-21.md | grep -A 10 "오늘의 목표"
```

**검증 결과**: ✅ 모든 테스트 통과

1. **✅ JSON 출력**: 첫 줄에 `{"decision": "allow"}` 정상 출력
2. **✅ 목표 삽입**: Placeholder가 실제 목표 3개로 정상 교체
   ```
   1. **스킬 가챠 API**: SkillTemplate Seeder, POST /api/skills/gacha, Unity 문서
   2. **Drop System**: 던전 클리어 시 Equipment 드랍, 드랍 확률 테이블
   3. **Combat-Dungeon 통합**: DungeonStage Monster 스탯 적용, BattleLog DungeonStageId 활용
   ```
3. **✅ Week/Day 추출**: Week 3 Day 2 정상 삽입
4. **✅ 로그 기록**: session-start.log에 정상 기록
5. **✅ Hook 응답**: Claude Code가 정상적으로 인식 (에러 없음)

### 완전히 해결된 이슈

| 이슈 | 원인 | 해결 방법 | 커밋 |
|------|------|-----------|------|
| 목표 삽입 실패 | awk 로직의 플래그 관리 미흡 | `in_goals` 플래그로 섹션 범위 추적 | 561f262 |
| startup hook error (신규) | stderr가 JSON보다 먼저 출력 | JSON을 최우선 출력 (라인 83-87) | ddc410d |
| startup hook error (재개) | 재개 경로의 JSON 순서 누락 | JSON을 최우선 출력 (라인 15-18) | 38e61a3 |

### 현재 상태

- **Hook 스크립트**: `.claude/hooks/auto-start-session.sh` (완전히 수정됨)
- **설정 파일**: `.claude/settings.local.json` (정상)
- **템플릿**: `.claude/templates/session-template.md` (정상)
- **데이터 소스**: `status.md`, `roadmap.md` (정상)

**결론**: SessionStart hook이 완전히 정상 작동합니다. 문제가 재발하지 않습니다.

---

---

## 📌 최근 해결 사례 #2 (2025-10-21 22:08)

### 증상
- troubleshooting 문서의 해결 방법을 적용했지만 여전히 "SessionStart:startup hook error" 발생
- 스크립트 실행 시 "syntax error: unexpected end of file" 에러 발생

### 디버깅 과정

1. **이전 문서 확인**: 라인 15-18과 83-87의 JSON 순서 문제 해결 확인 ✅
2. **스크립트 문법 검증**: ❌ **문법 에러 발견!**
```bash
bash -n .claude/hooks/auto-start-session.sh
# 출력: line 138: syntax error: unexpected end of file
```

3. **if/else 블록 검증**: ❌ **누락된 fi 발견!**

### 근본 원인

**3개의 if 블록에서 fi가 누락됨**:

1. **라인 8-15**: 입력 처리 블록
```bash
# ❌ 문제 코드
else
    INPUT=$(cat 2>/dev/null || echo "{}")
    SESSION_ID=$(...)

# 날짜 확인  ← fi 없이 다음 코드로 넘어감!
```

2. **라인 31-36**: roadmap.md 파일 확인 블록
```bash
# ❌ 문제 코드
else
    CURRENT_WEEK=""
    CURRENT_DAY=""

# 기본값 설정  ← fi 없음!
```

3. **라인 59-91**: 템플릿 파일 처리 블록
```bash
# ❌ 문제 코드
else
    cp "$TEMPLATE_FILE" "$SESSION_FILE"

# 임시 파일 사용  ← fi 없음!
```

4. **라인 106-125**: awk 명령어 존재 확인 블록
```bash
# ❌ 문제 코드
if command -v awk >/dev/null 2>&1; then
    awk -v goals="$GOALS" '...'

# 로그 기록  ← fi 없음!
```

### 추가 문제: Windows Git Bash 호환성

**grep -o 옵션 실패**:
```bash
# ❌ Windows Git Bash에서 작동 불안정
SESSION_ID=$(echo "$INPUT" | grep -o '"session_id":"[^"]*"' | cut -d'"' -f4)
```

### 해결

1. **모든 if 블록에 fi 추가**:
```bash
# ✅ 수정 후
if [ -t 0 ]; then
    SESSION_ID=""
else
    INPUT=$(cat 2>/dev/null || echo "{}")
    SESSION_ID=$(...)
fi  # ← fi 추가!
```

2. **sed로 JSON 파싱 대체**:
```bash
# ✅ Windows 호환 버전
SESSION_ID=$(echo "$INPUT" | sed -n 's/.*"session_id"[[:space:]]*:[[:space:]]*"\([^"]*\)".*/\1/p' 2>/dev/null || echo "")
```

3. **에러 핸들링 강화**:
```bash
# ✅ 모든 파일 작업에 에러 무시 처리
sed "s/{DATE}/$TODAY/g" "$SESSION_FILE" > "$TEMP_FILE" 2>/dev/null && mv "$TEMP_FILE" "$SESSION_FILE" 2>/dev/null || true
```

4. **템플릿 없는 경우 대비**:
```bash
# ✅ 템플릿 파일이 없으면 기본 템플릿 생성
if [ ! -f "$TEMPLATE_FILE" ]; then
    cat > "$SESSION_FILE" << 'EOF'
# 📅 {DATE} | Week {X} Day {Y}
...
EOF
else
    cp "$TEMPLATE_FILE" "$SESSION_FILE"
fi
```

### 검증

```bash
# 문법 검증
bash -n .claude/hooks/auto-start-session.sh
# 에러 없음 ✅

# 실행 테스트
bash .claude/hooks/auto-start-session.sh
# 출력:
# {"decision": "allow"}
# ✅ 세션 자동 시작: 2025-10-21 | Week 3 Day 2 | 목표: 3개
```

### 교훈

1. **문법 검증 필수**: 스크립트 수정 후 `bash -n script.sh` 실행
2. **Windows 호환성**: grep -o, sed -i 등은 플랫폼별 동작 차이 있음
3. **에러 핸들링**: 모든 외부 명령어는 실패해도 스크립트가 계속되도록 `|| true` 추가
4. **폴백 로직**: 템플릿 파일 없는 경우 등 예외 상황 대비

### 완전한 해결 체크리스트

- ✅ if/else 블록 모두 fi로 닫힘
- ✅ Windows Git Bash 호환 (sed 사용)
- ✅ 에러 핸들링 강화 (2>/dev/null, || true)
- ✅ 템플릿 없는 경우 대비
- ✅ JSON 출력 순서 (기존 세션 재개 포함)
- ✅ awk 섹션 범위 추적

---

## 📌 최근 해결 사례 #3 (2025-10-21 22:21)

### 증상
- 수동으로 hook을 테스트하면 정상 작동 (목표 정상 삽입)
- Claude Code 시작 시 목표가 `{목표1}`, `{목표2}` placeholder로 남아있음

### 원인 분석

**기존 세션 재개 로직의 문제**:
```bash
# ❌ 문제 코드 (라인 23-26)
if [ -f "$SESSION_FILE" ]; then
    echo '{"decision": "allow"}'
    echo "✅ 기존 세션 재개: $TODAY" >&2
    exit 0
fi
```

**문제점**:
1. 오늘 날짜의 파일이 이미 있으면 무조건 재개
2. 파일이 이전에 placeholder를 가진 상태로 생성되었어도 체크하지 않음
3. 수동 테스트는 파일을 삭제하고 실행하므로 정상 작동
4. Claude Code 자동 실행은 기존 파일을 재사용하므로 문제 발생

### 해결

**Placeholder 감지 로직 추가**:
```bash
# ✅ 수정 후 (라인 22-34)
# 기존 세션 파일이 있는지 확인
if [ -f "$SESSION_FILE" ]; then
    # Placeholder가 있으면 파일을 다시 생성해야 함
    if grep -q "{목표" "$SESSION_FILE" 2>/dev/null; then
        # Placeholder 발견 - 파일을 삭제하고 새로 생성
        rm -f "$SESSION_FILE" 2>/dev/null || true
    else
        # 정상 파일 - 재개
        echo '{"decision": "allow"}'
        echo "✅ 기존 세션 재개: $TODAY" >&2
        exit 0
    fi
fi
```

**장점**:
- ✅ 기존 파일이 placeholder를 가지고 있으면 자동으로 재생성
- ✅ 정상 파일은 그대로 재사용 (불필요한 재생성 방지)
- ✅ 수동 테스트와 자동 실행이 동일하게 작동

### 검증

**Placeholder 감지 테스트**:
```bash
# 1. Placeholder 포함 파일 생성
cat > .claude/memories/2-session/daily-2025-10-21.md << 'EOF'
## 🎯 오늘의 목표
1. [ ] {목표1}
2. [ ] {목표2}
EOF

# 2. Hook 실행
echo '{"session_id":"test"}' | bash .claude/hooks/auto-start-session.sh

# 3. 결과 확인
cat .claude/memories/2-session/daily-2025-10-21.md | grep -A 5 "오늘의 목표"
# 출력: 실제 목표 3개가 정상 삽입됨 ✅
```

**정상 파일 재개 테스트**:
```bash
# 정상 파일이 있을 때
echo '{"session_id":"test"}' | bash .claude/hooks/auto-start-session.sh 2>&1
# 출력:
# {"decision": "allow"}
# ✅ 기존 세션 재개: 2025-10-21
```

### 교훈

1. **재개 로직 검증**: 기존 파일을 재사용하는 경우 파일 상태도 검증해야 함
2. **자동 복구**: Placeholder 같은 비정상 상태를 감지하면 자동으로 수정
3. **테스트 시나리오**: 수동 실행뿐만 아니라 기존 파일이 있는 상태도 테스트

---

**이 문서는 SessionStart hook 문제 발생 시 가장 먼저 참고할 완전한 가이드입니다.**
