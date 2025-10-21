#!/bin/bash

# Claude Code Stop Hook: 자동 세션 메모리 업데이트
# Claude가 응답 완료 시마다 자동으로 세션 파일 업데이트

# 입력 JSON 파싱 (stdin으로 전달됨)
INPUT=$(cat)
SESSION_ID=$(echo "$INPUT" | grep -o '"session_id":"[^"]*"' | cut -d'"' -f4)
TRANSCRIPT_PATH=$(echo "$INPUT" | grep -o '"transcript_path":"[^"]*"' | cut -d'"' -f4)

# 날짜 확인
TODAY=$(date +%Y-%m-%d)
SESSION_FILE=".claude/memories/2-session/daily-$TODAY.md"

# 세션 파일이 없으면 종료 (아직 /startSession 안 함)
if [ ! -f "$SESSION_FILE" ]; then
    echo '{"decision": "allow"}' # 세션 없으면 그냥 통과
    exit 0
fi

# Transcript 분석 (마지막 응답에서 중요 정보 추출)
LAST_RESPONSE=$(tail -n 50 "$TRANSCRIPT_PATH" 2>/dev/null || echo "")

# 중요 이벤트 감지
SHOULD_UPDATE=false

# 1. API/Controller 언급 검사
if echo "$LAST_RESPONSE" | grep -qiE "(Controller|API|endpoint|HttpPost|HttpGet)"; then
    SHOULD_UPDATE=true
    EVENT_TYPE="API 변경"
fi

# 2. Entity/DTO 언급 검사
if echo "$LAST_RESPONSE" | grep -qiE "(Entity|DTO|DbSet|Migration)"; then
    SHOULD_UPDATE=true
    EVENT_TYPE="DB/DTO 변경"
fi

# 3. 의사결정 키워드 검사
if echo "$LAST_RESPONSE" | grep -qiE "(결정|decision|확률|percentage|%로 설정)"; then
    SHOULD_UPDATE=true
    EVENT_TYPE="의사결정"
fi

# 4. TODO(human) 검사
if echo "$LAST_RESPONSE" | grep -qiE "TODO\(human\)"; then
    SHOULD_UPDATE=true
    EVENT_TYPE="TODO(human) 추가"
fi

# 업데이트 필요하면 세션 파일에 기록
if [ "$SHOULD_UPDATE" = true ]; then
    TIMESTAMP=$(date +"%H:%M:%S")
    
    # 세션 파일의 "⚙️ 진행 중" 섹션에 자동 추가
    # (간단하게 타임스탬프만 추가)
    echo "" >> "$SESSION_FILE"
    echo "<!-- Auto-updated at $TIMESTAMP: $EVENT_TYPE detected -->" >> "$SESSION_FILE"
    
    # 로그 (디버그용, 삭제 가능)
    echo "[$TIMESTAMP] Session updated: $EVENT_TYPE" >> ".claude/hooks/session-update.log"
fi

# 항상 allow (블로킹하지 않음)
echo '{"decision": "allow"}'
exit 0
