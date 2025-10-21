#!/bin/bash

# Claude Code Stop Hook V2: 전체 중요 대화 저장
# 서브에이전트 제외, 메인 대화만 저장

INPUT=$(cat)
SESSION_ID=$(echo "$INPUT" | grep -o '"session_id":"[^"]*"' | cut -d'"' -f4)
TRANSCRIPT_PATH=$(echo "$INPUT" | grep -o '"transcript_path":"[^"]*"' | cut -d'"' -f4)

TODAY=$(date +%Y-%m-%d)
SESSION_FILE=".claude/memories/2-session/daily-$TODAY.md"

# 세션 파일 없으면 종료
if [ ! -f "$SESSION_FILE" ]; then
    echo '{"decision": "allow"}'
    exit 0
fi

# Transcript 마지막 entry 추출 (JSONL 형식)
LAST_ENTRY=$(tail -n 1 "$TRANSCRIPT_PATH" 2>/dev/null)

# role이 assistant인지 확인 (Claude의 응답만 저장)
ROLE=$(echo "$LAST_ENTRY" | jq -r '.role' 2>/dev/null)
if [ "$ROLE" != "assistant" ]; then
    echo '{"decision": "allow"}'
    exit 0
fi

# 서브에이전트 응답인지 확인 (Task tool 사용 여부)
IS_AGENT=$(echo "$LAST_ENTRY" | jq -r '.content[] | select(.type == "tool_use") | select(.name == "Task")' 2>/dev/null)
if [ -n "$IS_AGENT" ]; then
    # 서브에이전트 작업이면 저장 안 함
    echo '{"decision": "allow"}'
    exit 0
fi

# 텍스트 내용 추출
MESSAGE_TEXT=$(echo "$LAST_ENTRY" | jq -r '.content[] | select(.type == "text") | .text' 2>/dev/null)

# 빈 메시지면 종료
if [ -z "$MESSAGE_TEXT" ] || [ "$MESSAGE_TEXT" = "null" ]; then
    echo '{"decision": "allow"}'
    exit 0
fi

# 중요 키워드 필터링
SHOULD_SAVE=false
CATEGORY="일반 대화"

if echo "$MESSAGE_TEXT" | grep -qiE "(Controller|API|endpoint|HttpPost|HttpGet|HttpPut|HttpDelete)"; then
    SHOULD_SAVE=true
    CATEGORY="🔧 API 변경"
fi

if echo "$MESSAGE_TEXT" | grep -qiE "(Entity|DTO|DbSet|Migration|EF Core|Database)"; then
    SHOULD_SAVE=true
    CATEGORY="💾 DB/DTO"
fi

if echo "$MESSAGE_TEXT" | grep -qiE "(결정|decision|확률|percentage|%로|설계|design|선택|choice)"; then
    SHOULD_SAVE=true
    CATEGORY="🤝 의사결정"
fi

if echo "$MESSAGE_TEXT" | grep -qiE "TODO\(human\)"; then
    SHOULD_SAVE=true
    CATEGORY="👤 TODO(human)"
fi

if echo "$MESSAGE_TEXT" | grep -qiE "(완료|completed|finished|구현|implemented|추가|added)"; then
    SHOULD_SAVE=true
    CATEGORY="✅ 작업 완료"
fi

if echo "$MESSAGE_TEXT" | grep -qiE "(오류|error|bug|issue|문제|problem|실패|failed)"; then
    SHOULD_SAVE=true
    CATEGORY="🐛 이슈"
fi

if echo "$MESSAGE_TEXT" | grep -qiE "(Hook|자동화|automation|SessionStart|SessionEnd)"; then
    SHOULD_SAVE=true
    CATEGORY="⚙️ 시스템"
fi

# 중요 대화가 아니면 저장 안 함
if [ "$SHOULD_SAVE" = false ]; then
    echo '{"decision": "allow"}'
    exit 0
fi

# 대화 내용 저장
TIMESTAMP=$(date +"%H:%M:%S")

echo "" >> "$SESSION_FILE"
echo "### $CATEGORY [$TIMESTAMP]" >> "$SESSION_FILE"
echo "" >> "$SESSION_FILE"
echo "$MESSAGE_TEXT" >> "$SESSION_FILE"
echo "" >> "$SESSION_FILE"

# 파일 크기 및 토큰 수 계산
FILE_SIZE=$(stat -c%s "$SESSION_FILE" 2>/dev/null || stat -f%z "$SESSION_FILE" 2>/dev/null || echo "0")
FILE_SIZE_MB=$(echo "scale=2; $FILE_SIZE / 1048576" | bc 2>/dev/null || echo "0")

# 토큰 수 추정 (1 token ≈ 4 characters for English, ≈ 2 for Korean)
# 한글/영어 혼합이므로 평균 3 characters per token으로 계산
CHAR_COUNT=$(wc -m < "$SESSION_FILE" 2>/dev/null || echo "0")
TOKEN_COUNT=$(echo "scale=0; $CHAR_COUNT / 3" | bc 2>/dev/null || echo "0")
TOKEN_COUNT_K=$(echo "scale=1; $TOKEN_COUNT / 1000" | bc 2>/dev/null || echo "0")

# 경고 메시지
if [ "$FILE_SIZE" -gt 10485760 ]; then
    echo "⚠️  세션 파일 ${FILE_SIZE_MB}MB (약 ${TOKEN_COUNT_K}k tokens) - LLM 요약 권장" >&2
fi

# Claude에게 토큰 정보 출력
echo "📊 세션 메모리: ${FILE_SIZE_MB}MB | ~${TOKEN_COUNT_K}k tokens" >&2

# 로그 기록
echo "[$TIMESTAMP] $CATEGORY (${#MESSAGE_TEXT} chars) | Total: ${FILE_SIZE_MB}MB, ~${TOKEN_COUNT_K}k tokens" >> ".claude/hooks/session-update.log"

echo '{"decision": "allow"}'
exit 0
