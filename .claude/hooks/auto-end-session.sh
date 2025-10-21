#!/bin/bash

# Claude Code SessionEnd Hook: 자동 세션 종료
# Claude Code 종료 시 자동으로 세션 내용을 Current 메모리에 반영하고 아카이브

# 입력 JSON 파싱
INPUT=$(cat)
SESSION_ID=$(echo "$INPUT" | grep -o '"session_id":"[^"]*"' | cut -d'"' -f4)

# 날짜 확인
TODAY=$(date +%Y-%m-%d)
SESSION_FILE=".claude/memories/2-session/daily-$TODAY.md"

# 세션 파일이 없으면 종료 (오늘 작업 안 함)
if [ ! -f "$SESSION_FILE" ]; then
    echo "ℹ️  오늘 세션 파일 없음 (작업 없음)" >&2
    echo '{"decision": "allow"}'
    exit 0
fi

# 1. 코드베이스 스캔 (API 엔드포인트 수)
API_COUNT=$(find IdleRPG.API/Controllers -name "*Controller.cs" 2>/dev/null | xargs grep -hE "(HttpGet|HttpPost|HttpPut|HttpDelete|HttpPatch)" | wc -l || echo "0")

# 2. DB 테이블 수
DB_COUNT=$(grep -cE "DbSet<" IdleRPG.Infrastructure/Data/GameDBContext.cs 2>/dev/null || echo "0")

# 3. Git Commits (오늘)
GIT_COMMITS=$(git log --oneline --since="$TODAY 00:00" --until="$TODAY 23:59" 2>/dev/null | wc -l || echo "0")

# 4. Week/Day 정보
ROADMAP_FILE=".claude/memories/1-current/roadmap.md"
CURRENT_WEEK=$(grep -oP '현재 진행: Week \K[0-9]+' "$ROADMAP_FILE" 2>/dev/null || echo "3")
CURRENT_DAY=$(grep -oP 'Week [0-9]+ Day \K[0-9]+' "$ROADMAP_FILE" 2>/dev/null || echo "3")

# 5. 세션 통계 업데이트 (세션 파일 끝에 추가)
echo "" >> "$SESSION_FILE"
echo "---" >> "$SESSION_FILE"
echo "" >> "$SESSION_FILE"
echo "## 🏁 세션 종료 (자동)" >> "$SESSION_FILE"
echo "" >> "$SESSION_FILE"
echo "**종료 시간**: $(date +"%H:%M:%S")" >> "$SESSION_FILE"
echo "" >> "$SESSION_FILE"
echo "**최종 통계**:" >> "$SESSION_FILE"
echo "- API 엔드포인트: $API_COUNT개" >> "$SESSION_FILE"
echo "- Database Tables: $DB_COUNT개" >> "$SESSION_FILE"
echo "- Git Commits (오늘): $GIT_COMMITS개" >> "$SESSION_FILE"

# 6. 아카이브 폴더 생성
ARCHIVE_DIR=".claude/memories/9-archive/checkpoints"
mkdir -p "$ARCHIVE_DIR"

# 7. 세션 파일을 아카이브로 이동
ARCHIVE_FILE="$ARCHIVE_DIR/week$CURRENT_WEEK-day$CURRENT_DAY.md"
mv "$SESSION_FILE" "$ARCHIVE_FILE"

# 8. roadmap.md 업데이트 (다음 날로 진행)
# Day 증가 (간단하게 +1, 실제로는 주/월 경계 고려 필요)
NEXT_DAY=$((CURRENT_DAY + 1))
sed -i "s/현재 진행: Week $CURRENT_WEEK Day $CURRENT_DAY/현재 진행: Week $CURRENT_WEEK Day $NEXT_DAY/" "$ROADMAP_FILE" 2>/dev/null

# 9. status.md 업데이트 (업데이트 날짜만 변경)
STATUS_FILE=".claude/memories/1-current/status.md"
sed -i "s/\*\*업데이트\*\*: [0-9]\{4\}-[0-9]\{2\}-[0-9]\{2\}/**업데이트**: $TODAY/" "$STATUS_FILE" 2>/dev/null

# 10. 로그 기록
echo "[$(date +"%Y-%m-%d %H:%M:%S")] 세션 종료: Week $CURRENT_WEEK Day $CURRENT_DAY | API: $API_COUNT | DB: $DB_COUNT | Commits: $GIT_COMMITS" >> ".claude/hooks/session-end.log"

# Claude에게 종료 메시지
echo "
✅ 세션 자동 종료: $TODAY

📊 최종 통계:
- API 엔드포인트: $API_COUNT개
- Database Tables: $DB_COUNT개
- Git Commits: $GIT_COMMITS개

📂 아카이브: 9-archive/checkpoints/week$CURRENT_WEEK-day$CURRENT_DAY.md
📅 다음 세션: Week $CURRENT_WEEK Day $NEXT_DAY

🎉 오늘도 수고하셨습니다!
" >&2

# 항상 allow
echo '{"decision": "allow"}'
exit 0
