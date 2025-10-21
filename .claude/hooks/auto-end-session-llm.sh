#!/bin/bash

# Claude Code SessionEnd Hook V2: LLM 기반 하루 요약
# 세션 파일 전체를 LLM에 전달해서 요약 생성

INPUT=$(cat)
SESSION_ID=$(echo "$INPUT" | grep -o '"session_id":"[^"]*"' | cut -d'"' -f4)

TODAY=$(date +%Y-%m-%d)
SESSION_FILE=".claude/memories/2-session/daily-$TODAY.md"

# 세션 파일 없으면 종료
if [ ! -f "$SESSION_FILE" ]; then
    echo "ℹ️  세션 파일 없음" >&2
    echo '{"decision": "allow"}'
    exit 0
fi

# 1. 코드베이스 최종 스캔
API_COUNT=$(find IdleRPG.API/Controllers -name "*Controller.cs" 2>/dev/null | xargs grep -hE "(HttpGet|HttpPost|HttpPut|HttpDelete)" | wc -l || echo "0")
DB_COUNT=$(grep -cE "DbSet<" IdleRPG.Infrastructure/Data/GameDBContext.cs 2>/dev/null || echo "0")
GIT_COMMITS=$(git log --oneline --since="$TODAY 00:00" --until="$TODAY 23:59" 2>/dev/null | wc -l || echo "0")

# 2. Week/Day 정보
ROADMAP_FILE=".claude/memories/1-current/roadmap.md"
CURRENT_WEEK=$(grep -oP '현재 진행: Week \K[0-9]+' "$ROADMAP_FILE" 2>/dev/null || echo "3")
CURRENT_DAY=$(grep -oP 'Week [0-9]+ Day \K[0-9]+' "$ROADMAP_FILE" 2>/dev/null || echo "3")

# 3. 세션 파일 크기 확인
FILE_SIZE=$(stat -c%s "$SESSION_FILE" 2>/dev/null || stat -f%z "$SESSION_FILE" 2>/dev/null || echo "0")
FILE_SIZE_MB=$((FILE_SIZE / 1048576))

echo "" >> "$SESSION_FILE"
echo "---" >> "$SESSION_FILE"
echo "" >> "$SESSION_FILE"
echo "## 🏁 세션 종료 (자동)" >> "$SESSION_FILE"
echo "" >> "$SESSION_FILE"
echo "**종료 시간**: $(date +"%H:%M:%S")" >> "$SESSION_FILE"
echo "**세션 파일 크기**: ${FILE_SIZE_MB}MB" >> "$SESSION_FILE"
echo "" >> "$SESSION_FILE"
echo "**최종 통계**:" >> "$SESSION_FILE"
echo "- API 엔드포인트: $API_COUNT개" >> "$SESSION_FILE"
echo "- Database Tables: $DB_COUNT개" >> "$SESSION_FILE"
echo "- Git Commits: $GIT_COMMITS개" >> "$SESSION_FILE"
echo "" >> "$SESSION_FILE"

# 4. LLM 요약 (파일 크기가 1MB 이상일 때만)
if [ "$FILE_SIZE" -gt 1048576 ]; then
    echo "🤖 **LLM 요약 생성 중...**" >> "$SESSION_FILE"
    echo "" >> "$SESSION_FILE"
    
    # Zen MCP를 통해 GPT-4로 요약 생성
    # 세션 파일 내용을 읽어서 요약 요청
    SESSION_CONTENT=$(cat "$SESSION_FILE")
    
    # 요약 프롬프트
    SUMMARY_PROMPT="다음은 오늘 하루 동안의 개발 세션 기록입니다. 다음 형식으로 요약해주세요:

## 📝 하루 요약

### 주요 성과
- (구현한 기능 나열)

### 기술적 결정
- (내린 의사결정 나열)

### 발견한 이슈
- (문제점 나열)

### 다음 작업
- (남은 TODO 나열)

---
세션 내용:
$SESSION_CONTENT
"
    
    # 실제 LLM 호출은 여기서 (예: curl로 OpenAI API 또는 Zen MCP)
    # 간단한 예시: echo로 placeholder
    echo "### 📊 AI 요약" >> "$SESSION_FILE"
    echo "" >> "$SESSION_FILE"
    echo "> 세션 파일이 ${FILE_SIZE_MB}MB로 커서 LLM 요약을 생성해야 합니다." >> "$SESSION_FILE"
    echo "> SessionEnd 이후 수동으로 \`/summarizeSession $TODAY\` 명령어를 실행하세요." >> "$SESSION_FILE"
    echo "" >> "$SESSION_FILE"
    
    # 요약 요청 플래그 파일 생성 (나중에 처리)
    echo "$TODAY" > ".claude/memories/2-session/.needs-summary"
fi

# 5. 아카이브
ARCHIVE_DIR=".claude/memories/9-archive/checkpoints"
mkdir -p "$ARCHIVE_DIR"
ARCHIVE_FILE="$ARCHIVE_DIR/week$CURRENT_WEEK-day$CURRENT_DAY.md"
cp "$SESSION_FILE" "$ARCHIVE_FILE"
rm "$SESSION_FILE"

# 6. roadmap.md 업데이트
NEXT_DAY=$((CURRENT_DAY + 1))
sed -i "s/현재 진행: Week $CURRENT_WEEK Day $CURRENT_DAY/현재 진행: Week $CURRENT_WEEK Day $NEXT_DAY/" "$ROADMAP_FILE" 2>/dev/null

# 7. status.md 업데이트
STATUS_FILE=".claude/memories/1-current/status.md"
sed -i "s/\*\*업데이트\*\*: [0-9]\{4\}-[0-9]\{2\}-[0-9]\{2\}/**업데이트**: $TODAY/" "$STATUS_FILE" 2>/dev/null

# 8. 로그
echo "[$(date +"%Y-%m-%d %H:%M:%S")] 세션 종료: Week $CURRENT_WEEK Day $CURRENT_DAY | Size: ${FILE_SIZE_MB}MB | API: $API_COUNT | DB: $DB_COUNT | Commits: $GIT_COMMITS" >> ".claude/hooks/session-end.log"

# 종료 메시지
echo "
✅ 세션 자동 종료: $TODAY

📊 최종 통계:
- 세션 파일 크기: ${FILE_SIZE_MB}MB
- API 엔드포인트: $API_COUNT개
- Database Tables: $DB_COUNT개
- Git Commits: $GIT_COMMITS개

📂 아카이브: week$CURRENT_WEEK-day$CURRENT_DAY.md
📅 다음 세션: Week $CURRENT_WEEK Day $NEXT_DAY

🎉 오늘도 수고하셨습니다!
" >&2

echo '{"decision": "allow"}'
exit 0
