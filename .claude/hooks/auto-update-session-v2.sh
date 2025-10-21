#!/bin/bash

# Claude Code Stop Hook V2: 전체 대화 컨텍스트 저장
# 모든 중요 대화를 세션 파일에 append (LLM 요약은 SessionEnd에서)

INPUT=$(cat)
SESSION_ID=$(echo "$INPUT" | grep -o '"session_id":"[^"]*"' | cut -d'"' -f4)
TRANSCRIPT_PATH=$(echo "$INPUT" | grep -o '"transcript_path":"[^"]*"' | cut -d'"' -f4)

# 날짜 확인
TODAY=$(date +%Y-%m-%d)
SESSION_FILE=".claude/memories/2-session/daily-$TODAY.md"

# 세션 파일이 없으면 종료
if [ ! -f "$SESSION_FILE" ]; then
    echo '{"decision": "allow"}'
    exit 0
fi

# Transcript에서 마지막 대화 추출 (사용자 메시지 + Claude 응답)
# JSONL 파싱: 마지막 2개 entry (user + assistant)
LAST_MESSAGES=$(tail -n 2 "$TRANSCRIPT_PATH" 2>/dev/null | jq -r '.content[] | select(.type == "text") | .text' 2>/dev/null || echo "")

# 빈 응답이면 종료
if [ -z "$LAST_MESSAGES" ]; then
    echo '{"decision": "allow"}'
    exit 0
fi

# 중요 대화 필터링 (키워드 기반)
SHOULD_SAVE=false
CATEGORY=""

# 1. API/Controller 관련
if echo "$LAST_MESSAGES" | grep -qiE "(Controller|API|endpoint|HttpPost|HttpGet)"; then
    SHOULD_SAVE=true
    CATEGORY="API 변경"
fi

# 2. Entity/DTO/DB
if echo "$LAST_MESSAGES" | grep -qiE "(Entity|DTO|DbSet|Migration|EF Core)"; then
    SHOULD_SAVE=true
    CATEGORY="DB/DTO 변경"
fi

# 3. 의사결정
if echo "$LAST_MESSAGES" | grep -qiE "(결정|decision|확률|percentage|%로|설계|design)"; then
    SHOULD_SAVE=true
    CATEGORY="의사결정"
fi

# 4. TODO(human)
if echo "$LAST_MESSAGES" | grep -qiE "TODO\(human\)"; then
    SHOULD_SAVE=true
    CATEGORY="TODO(human)"
fi

# 5. 구현 완료
if echo "$LAST_MESSAGES" | grep -qiE "(완료|completed|finished|구현|implemented)"; then
    SHOULD_SAVE=true
    CATEGORY="작업 완료"
fi

# 6. 오류/이슈
if echo "$LAST_MESSAGES" | grep -qiE "(오류|error|bug|issue|문제|problem)"; then
    SHOULD_SAVE=true
    CATEGORY="이슈"
fi

# 중요 대화가 아니면 저장 안 함 (노이즈 제거)
if [ "$SHOULD_SAVE" = false ]; then
    echo '{"decision": "allow"}'
    exit 0
fi

# 세션 파일에 대화 내용 추가
TIMESTAMP=$(date +"%H:%M:%S")

echo "" >> "$SESSION_FILE"
echo "---" >> "$SESSION_FILE"
echo "" >> "$SESSION_FILE"
echo "### 💬 [$TIMESTAMP] $CATEGORY" >> "$SESSION_FILE"
echo "" >> "$SESSION_FILE"

# 대화 내용 저장 (전체)
echo "$LAST_MESSAGES" >> "$SESSION_FILE"

# 파일 크기 체크 (10MB 초과 시 경고)
FILE_SIZE=$(stat -f%z "$SESSION_FILE" 2>/dev/null || stat -c%s "$SESSION_FILE" 2>/dev/null || echo "0")
if [ "$FILE_SIZE" -gt 10485760 ]; then
    echo "⚠️  세션 파일이 10MB를 초과했습니다. SessionEnd에서 LLM 요약이 권장됩니다." >&2
fi

# 로그 기록
echo "[$TIMESTAMP] $CATEGORY: 대화 저장됨 (${#LAST_MESSAGES} chars)" >> ".claude/hooks/session-update.log"

echo '{"decision": "allow"}'
exit 0
