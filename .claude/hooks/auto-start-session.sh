#!/bin/bash

# Claude Code SessionStart Hook: 자동 세션 시작
# 새 세션 시작 시 자동으로 오늘 날짜의 세션 파일 생성

# 입력 처리 (입력이 없을 수도 있음)
INPUT=""
if [ -t 0 ]; then
    # 터미널에서 직접 실행 시
    SESSION_ID=""
else
    # Claude Code에서 실행 시
    INPUT=$(cat 2>/dev/null || echo "{}")
    # JSON 파싱을 더 안전하게 처리 (Windows Git Bash 호환)
    SESSION_ID=$(echo "$INPUT" | sed -n 's/.*"session_id"[[:space:]]*:[[:space:]]*"\([^"]*\)".*/\1/p' 2>/dev/null || echo "")
fi

# 날짜 확인
TODAY=$(date +%Y-%m-%d)
SESSION_FILE=".claude/memories/2-session/daily-$TODAY.md"

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

# Week/Day 계산 (roadmap.md에서 추출)
ROADMAP_FILE=".claude/memories/1-current/roadmap.md"
# "**현재 진행**: Week 3 Day 2 (2025-10-20)" 형식에서 Week, Day 추출
if [ -f "$ROADMAP_FILE" ]; then
    CURRENT_WEEK=$(grep "^\*\*현재 진행\*\*:" "$ROADMAP_FILE" 2>/dev/null | sed -n 's/.*Week \([0-9]\+\).*/\1/p' | head -1)
    CURRENT_DAY=$(grep "^\*\*현재 진행\*\*:" "$ROADMAP_FILE" 2>/dev/null | sed -n 's/.*Day \([0-9]\+\).*/\1/p' | head -1)
else
    CURRENT_WEEK=""
    CURRENT_DAY=""
fi

# 기본값 설정 (추출 실패 시)
CURRENT_WEEK=${CURRENT_WEEK:-3}
CURRENT_DAY=${CURRENT_DAY:-2}

# 오늘의 목표 추출 (status.md의 "다음 우선순위" 섹션)
STATUS_FILE=".claude/memories/1-current/status.md"
# "### Immediate (이번 주)" 섹션에서 1., 2., 3.으로 시작하는 줄 추출
GOALS=$(sed -n '/### Immediate/,/^$/p' "$STATUS_FILE" | grep -E "^[0-9]\." | head -3 || echo "")

# 목표가 비어있으면 기본 메시지
if [ -z "$GOALS" ]; then
    GOALS="1. [ ] 진행 중인 작업 완료
2. [ ] 새로운 기능 구현
3. [ ] 테스트 및 문서화"
fi

# 템플릿 복사 및 변수 치환
TEMPLATE_FILE=".claude/templates/session-template.md"
CURRENT_TIME=$(date +"%H:%M")

# 템플릿 파일 확인 및 복사
if [ ! -f "$TEMPLATE_FILE" ]; then
    # 템플릿이 없으면 기본 세션 파일 생성
    cat > "$SESSION_FILE" << 'EOF'
# 📅 {DATE} | Week {X} Day {Y}
> 시작: {TIME} | 종료: --:-- | 세션 시간: -- 시간

## 🎯 오늘의 목표
1. [ ] 진행 중인 작업 완료
2. [ ] 새로운 기능 구현
3. [ ] 테스트 및 문서화

---

## 📝 작업 내용

### 진행한 작업
-

### 발생한 이슈 및 해결
-

### 다음 세션 계획
-

---

## ✍️ 회고 및 메모
-

EOF
else
    cp "$TEMPLATE_FILE" "$SESSION_FILE"
fi

# 임시 파일 사용 (Windows Git Bash 호환)
TEMP_FILE="$SESSION_FILE.tmp"

# 변수 치환 (에러 무시)
sed "s/{DATE}/$TODAY/g" "$SESSION_FILE" > "$TEMP_FILE" 2>/dev/null && mv "$TEMP_FILE" "$SESSION_FILE" 2>/dev/null || true
sed "s/{TIME}/$CURRENT_TIME/g" "$SESSION_FILE" > "$TEMP_FILE" 2>/dev/null && mv "$TEMP_FILE" "$SESSION_FILE" 2>/dev/null || true
sed "s/{X}/$CURRENT_WEEK/g" "$SESSION_FILE" > "$TEMP_FILE" 2>/dev/null && mv "$TEMP_FILE" "$SESSION_FILE" 2>/dev/null || true
sed "s/{Y}/$CURRENT_DAY/g" "$SESSION_FILE" > "$TEMP_FILE" 2>/dev/null && mv "$TEMP_FILE" "$SESSION_FILE" 2>/dev/null || true

# 목표 삽입 (템플릿의 placeholder를 실제 목표로 교체)
# "## 🎯 오늘의 목표" 섹션 다음에 목표 삽입
# awk가 실패해도 무시하고 진행
if command -v awk >/dev/null 2>&1; then
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
    ' "$SESSION_FILE" > "$TEMP_FILE" 2>/dev/null && mv "$TEMP_FILE" "$SESSION_FILE" 2>/dev/null || true
fi

# 로그 기록 (에러 무시)
echo "[$(date +"%Y-%m-%d %H:%M:%S")] 새 세션 시작: Week $CURRENT_WEEK Day $CURRENT_DAY" >> ".claude/hooks/session-start.log" 2>/dev/null || true

# 항상 allow (JSON을 먼저 출력)
echo '{"decision": "allow"}'

# Claude에게 알림 메시지 (JSON 출력 후 stderr로)
echo "✅ 세션 자동 시작: $TODAY | Week $CURRENT_WEEK Day $CURRENT_DAY | 목표: 3개" >&2

exit 0
