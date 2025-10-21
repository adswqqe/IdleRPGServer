#!/bin/bash

# Claude Code SessionStart Hook: 자동 세션 시작
# 새 세션 시작 시 자동으로 오늘 날짜의 세션 파일 생성

# 입력 JSON 파싱
INPUT=$(cat)
SESSION_ID=$(echo "$INPUT" | grep -o '"session_id":"[^"]*"' | cut -d'"' -f4)

# 날짜 확인
TODAY=$(date +%Y-%m-%d)
SESSION_FILE=".claude/memories/2-session/daily-$TODAY.md"

# 이미 오늘 세션 파일이 있으면 종료 (재개 시)
if [ -f "$SESSION_FILE" ]; then
    echo "✅ 기존 세션 재개: $TODAY" >&2
    echo '{"decision": "allow"}'
    exit 0
fi

# Week/Day 계산 (roadmap.md에서 추출)
ROADMAP_FILE=".claude/memories/1-current/roadmap.md"
# "**현재 진행**: Week 3 Day 2 (2025-10-20)" 형식에서 Week, Day 추출
CURRENT_WEEK=$(grep "^\*\*현재 진행\*\*:" "$ROADMAP_FILE" 2>/dev/null | sed -n 's/.*Week \([0-9]\+\).*/\1/p' | head -1)
CURRENT_DAY=$(grep "^\*\*현재 진행\*\*:" "$ROADMAP_FILE" 2>/dev/null | sed -n 's/.*Day \([0-9]\+\).*/\1/p' | head -1)

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

cp "$TEMPLATE_FILE" "$SESSION_FILE"

# 임시 파일 사용 (Windows Git Bash 호환)
TEMP_FILE="$SESSION_FILE.tmp"

# 변수 치환
sed "s/{DATE}/$TODAY/g" "$SESSION_FILE" > "$TEMP_FILE" && mv "$TEMP_FILE" "$SESSION_FILE"
sed "s/{TIME}/$CURRENT_TIME/g" "$SESSION_FILE" > "$TEMP_FILE" && mv "$TEMP_FILE" "$SESSION_FILE"
sed "s/{X}/$CURRENT_WEEK/g" "$SESSION_FILE" > "$TEMP_FILE" && mv "$TEMP_FILE" "$SESSION_FILE"
sed "s/{Y}/$CURRENT_DAY/g" "$SESSION_FILE" > "$TEMP_FILE" && mv "$TEMP_FILE" "$SESSION_FILE"

# 목표 삽입 (템플릿의 placeholder를 실제 목표로 교체)
# "## 🎯 오늘의 목표" 섹션 다음에 목표 삽입
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

# 로그 기록
echo "[$(date +"%Y-%m-%d %H:%M:%S")] 새 세션 시작: Week $CURRENT_WEEK Day $CURRENT_DAY" >> ".claude/hooks/session-start.log"

# Claude에게 알림 메시지 (stderr로 출력하면 Claude가 볼 수 있음)
echo "
✅ 세션 자동 시작: $TODAY
📅 진행도: Week $CURRENT_WEEK Day $CURRENT_DAY
📂 파일: 2-session/daily-$TODAY.md

🎯 오늘의 목표:
$GOALS
" >&2

# 항상 allow
echo '{"decision": "allow"}'
exit 0
