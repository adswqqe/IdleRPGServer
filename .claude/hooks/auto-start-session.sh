#!/bin/bash

# Claude Code SessionStart Hook: 자동 세션 시작
# 새 세션 시작 시 자동으로 오늘 날짜의 세션 파일 생성

# 디버깅 로그 초기화
DEBUG_LOG=".claude/hooks/session-start-debug.log"
echo "[$(date +"%Y-%m-%d %H:%M:%S")] ========== SessionStart Hook 시작 ==========" >> "$DEBUG_LOG"

# 입력 처리 (입력이 없을 수도 있음)
INPUT=""
if [ -t 0 ]; then
    # 터미널에서 직접 실행 시
    SESSION_ID=""
    echo "[DEBUG] 터미널에서 직접 실행" >> "$DEBUG_LOG"
else
    # Claude Code에서 실행 시
    INPUT=$(cat 2>/dev/null || echo "{}")
    # JSON 파싱을 더 안전하게 처리 (Windows Git Bash 호환)
    SESSION_ID=$(echo "$INPUT" | sed -n 's/.*"session_id"[[:space:]]*:[[:space:]]*"\([^"]*\)".*/\1/p' 2>/dev/null || echo "")
    echo "[DEBUG] Claude Code에서 실행, SESSION_ID: $SESSION_ID" >> "$DEBUG_LOG"
fi

# 날짜 확인
TODAY=$(date +%Y-%m-%d)
SESSION_FILE=".claude/memories/2-session/daily-$TODAY.md"
echo "[DEBUG] TODAY: $TODAY" >> "$DEBUG_LOG"
echo "[DEBUG] SESSION_FILE: $SESSION_FILE" >> "$DEBUG_LOG"

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
echo "[DEBUG] ROADMAP_FILE: $ROADMAP_FILE" >> "$DEBUG_LOG"
echo "[DEBUG] ROADMAP_FILE exists: $([ -f "$ROADMAP_FILE" ] && echo 'YES' || echo 'NO')" >> "$DEBUG_LOG"

# "**현재 진행**: Week 3 Day 2 (2025-10-20)" 형식에서 Week, Day 추출
if [ -f "$ROADMAP_FILE" ]; then
    CURRENT_WEEK=$(grep "^\*\*현재 진행\*\*:" "$ROADMAP_FILE" 2>/dev/null | sed -n 's/.*Week \([0-9]\+\).*/\1/p' | head -1)
    CURRENT_DAY=$(grep "^\*\*현재 진행\*\*:" "$ROADMAP_FILE" 2>/dev/null | sed -n 's/.*Day \([0-9]\+\).*/\1/p' | head -1)
    echo "[DEBUG] CURRENT_WEEK 추출: $CURRENT_WEEK" >> "$DEBUG_LOG"
    echo "[DEBUG] CURRENT_DAY 추출: $CURRENT_DAY" >> "$DEBUG_LOG"
else
    CURRENT_WEEK=""
    CURRENT_DAY=""
    echo "[DEBUG] ROADMAP_FILE이 없음" >> "$DEBUG_LOG"
fi

# 기본값 설정 (추출 실패 시)
CURRENT_WEEK=${CURRENT_WEEK:-3}
CURRENT_DAY=${CURRENT_DAY:-2}
echo "[DEBUG] 최종 CURRENT_WEEK: $CURRENT_WEEK" >> "$DEBUG_LOG"
echo "[DEBUG] 최종 CURRENT_DAY: $CURRENT_DAY" >> "$DEBUG_LOG"

# 오늘의 목표 추출 (status.md의 "다음 우선순위" 섹션)
STATUS_FILE=".claude/memories/1-current/status.md"
echo "[DEBUG] STATUS_FILE: $STATUS_FILE" >> "$DEBUG_LOG"
echo "[DEBUG] STATUS_FILE exists: $([ -f "$STATUS_FILE" ] && echo 'YES' || echo 'NO')" >> "$DEBUG_LOG"

# "### Immediate (이번 주)" 섹션에서 1., 2., 3.으로 시작하는 줄 추출
echo "[DEBUG] sed 명령 실행 중..." >> "$DEBUG_LOG"
GOALS=$(sed -n '/### Immediate/,/^$/p' "$STATUS_FILE" 2>/dev/null | grep -E "^[0-9]\." | head -3 || echo "")
echo "[DEBUG] GOALS content:" >> "$DEBUG_LOG"
echo "$GOALS" >> "$DEBUG_LOG"

# 각 목표를 개별 변수로 분리 (앞의 숫자 제거)
GOAL1=$(echo "$GOALS" | sed -n '1p' | sed 's/^[0-9]\+\.\s*//' || echo "")
GOAL2=$(echo "$GOALS" | sed -n '2p' | sed 's/^[0-9]\+\.\s*//' || echo "")
GOAL3=$(echo "$GOALS" | sed -n '3p' | sed 's/^[0-9]\+\.\s*//' || echo "")

# 목표가 비어있으면 기본 메시지
if [ -z "$GOAL1" ]; then
    echo "[DEBUG] GOAL1이 비어있음 - 기본값 사용" >> "$DEBUG_LOG"
    GOAL1="진행 중인 작업 완료"
fi
if [ -z "$GOAL2" ]; then
    echo "[DEBUG] GOAL2가 비어있음 - 기본값 사용" >> "$DEBUG_LOG"
    GOAL2="새로운 기능 구현"
fi
if [ -z "$GOAL3" ]; then
    echo "[DEBUG] GOAL3이 비어있음 - 기본값 사용" >> "$DEBUG_LOG"
    GOAL3="테스트 및 문서화"
fi

echo "[DEBUG] GOAL1: $GOAL1" >> "$DEBUG_LOG"
echo "[DEBUG] GOAL2: $GOAL2" >> "$DEBUG_LOG"
echo "[DEBUG] GOAL3: $GOAL3" >> "$DEBUG_LOG"

# 템플릿 복사 및 변수 치환
TEMPLATE_FILE=".claude/templates/session-template.md"
CURRENT_TIME=$(date +"%H:%M")

echo "[DEBUG] TEMPLATE_FILE: $TEMPLATE_FILE" >> "$DEBUG_LOG"
echo "[DEBUG] TEMPLATE_FILE exists: $([ -f "$TEMPLATE_FILE" ] && echo 'YES' || echo 'NO')" >> "$DEBUG_LOG"
echo "[DEBUG] CURRENT_TIME: $CURRENT_TIME" >> "$DEBUG_LOG"

# 템플릿 파일 확인 및 복사
if [ ! -f "$TEMPLATE_FILE" ]; then
    echo "[DEBUG] 템플릿 파일이 없음 - 기본 세션 파일 생성" >> "$DEBUG_LOG"
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
    echo "[DEBUG] 템플릿 파일 복사 중..." >> "$DEBUG_LOG"
    cp "$TEMPLATE_FILE" "$SESSION_FILE"
    echo "[DEBUG] 템플릿 파일 복사 완료" >> "$DEBUG_LOG"
fi

echo "[DEBUG] SESSION_FILE 생성 후 존재 여부: $([ -f "$SESSION_FILE" ] && echo 'YES' || echo 'NO')" >> "$DEBUG_LOG"

# 임시 파일 사용 (Windows Git Bash 호환)
TEMP_FILE="$SESSION_FILE.tmp"
echo "[DEBUG] 변수 치환 시작 - TEMP_FILE: $TEMP_FILE" >> "$DEBUG_LOG"

# 변수 치환 (에러 무시)
echo "[DEBUG] {DATE} -> $TODAY 치환 중..." >> "$DEBUG_LOG"
sed "s/{DATE}/$TODAY/g" "$SESSION_FILE" > "$TEMP_FILE" 2>/dev/null && mv "$TEMP_FILE" "$SESSION_FILE" 2>/dev/null || true

echo "[DEBUG] {TIME} -> $CURRENT_TIME 치환 중..." >> "$DEBUG_LOG"
sed "s/{TIME}/$CURRENT_TIME/g" "$SESSION_FILE" > "$TEMP_FILE" 2>/dev/null && mv "$TEMP_FILE" "$SESSION_FILE" 2>/dev/null || true

echo "[DEBUG] {X} -> $CURRENT_WEEK 치환 중..." >> "$DEBUG_LOG"
sed "s/{X}/$CURRENT_WEEK/g" "$SESSION_FILE" > "$TEMP_FILE" 2>/dev/null && mv "$TEMP_FILE" "$SESSION_FILE" 2>/dev/null || true

echo "[DEBUG] {Y} -> $CURRENT_DAY 치환 중..." >> "$DEBUG_LOG"
sed "s/{Y}/$CURRENT_DAY/g" "$SESSION_FILE" > "$TEMP_FILE" 2>/dev/null && mv "$TEMP_FILE" "$SESSION_FILE" 2>/dev/null || true

echo "[DEBUG] 변수 치환 완료" >> "$DEBUG_LOG"

# 목표 삽입 (placeholder를 실제 목표로 교체)
echo "[DEBUG] 목표 삽입 시작" >> "$DEBUG_LOG"

# {목표1}, {목표2}, {목표3} 교체
# sed에서 /가 포함된 경우를 대비해 | 구분자 사용
echo "[DEBUG] {목표1} 교체 중..." >> "$DEBUG_LOG"
sed "s|{목표1}|$GOAL1|g" "$SESSION_FILE" > "$TEMP_FILE" 2>/dev/null && mv "$TEMP_FILE" "$SESSION_FILE" 2>/dev/null || true

echo "[DEBUG] {목표2} 교체 중..." >> "$DEBUG_LOG"
sed "s|{목표2}|$GOAL2|g" "$SESSION_FILE" > "$TEMP_FILE" 2>/dev/null && mv "$TEMP_FILE" "$SESSION_FILE" 2>/dev/null || true

echo "[DEBUG] {목표3} 교체 중..." >> "$DEBUG_LOG"
sed "s|{목표3}|$GOAL3|g" "$SESSION_FILE" > "$TEMP_FILE" 2>/dev/null && mv "$TEMP_FILE" "$SESSION_FILE" 2>/dev/null || true

echo "[DEBUG] 목표 삽입 완료" >> "$DEBUG_LOG"

echo "[DEBUG] 최종 SESSION_FILE 내용 (첫 30줄):" >> "$DEBUG_LOG"
head -30 "$SESSION_FILE" >> "$DEBUG_LOG" 2>/dev/null || true

# 로그 기록 (에러 무시)
echo "[$(date +"%Y-%m-%d %H:%M:%S")] 새 세션 시작: Week $CURRENT_WEEK Day $CURRENT_DAY" >> ".claude/hooks/session-start.log" 2>/dev/null || true

echo "[DEBUG] ========== SessionStart Hook 완료 ==========" >> "$DEBUG_LOG"
echo "" >> "$DEBUG_LOG"

# 항상 allow (JSON을 먼저 출력)
echo '{"decision": "allow"}'

# Claude에게 알림 메시지 (JSON 출력 후 stderr로)
echo "✅ 세션 자동 시작: $TODAY | Week $CURRENT_WEEK Day $CURRENT_DAY | 목표: 3개" >&2
echo "🔍 디버그 로그: .claude/hooks/session-start-debug.log" >&2

exit 0
