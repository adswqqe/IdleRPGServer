#!/bin/bash

# Claude Code Stop Hook V2: 전체 중요 대화 저장
# 서브에이전트 제외, 메인 대화만 저장

INPUT=$(cat)
# Windows Git Bash 호환: sed 사용
SESSION_ID=$(echo "$INPUT" | sed -n 's/.*"session_id"[[:space:]]*:[[:space:]]*"\([^"]*\)".*/\1/p' 2>/dev/null || echo "")
TRANSCRIPT_PATH=$(echo "$INPUT" | sed -n 's/.*"transcript_path"[[:space:]]*:[[:space:]]*"\([^"]*\)".*/\1/p' 2>/dev/null || echo "")

TODAY=$(date +%Y-%m-%d)
SESSION_FILE=".claude/memories/2-session/daily-$TODAY.md"

# 디버깅: 입력 로그 기록
echo "[$(date +"%Y-%m-%d %H:%M:%S")] Stop Hook 실행" >> ".claude/hooks/stop-hook-debug.log"
echo "  SESSION_ID: $SESSION_ID" >> ".claude/hooks/stop-hook-debug.log"
echo "  TRANSCRIPT_PATH: $TRANSCRIPT_PATH" >> ".claude/hooks/stop-hook-debug.log"
echo "  SESSION_FILE exists: $([ -f "$SESSION_FILE" ] && echo 'YES' || echo 'NO')" >> ".claude/hooks/stop-hook-debug.log"

# 세션 파일 없으면 종료
if [ ! -f "$SESSION_FILE" ]; then
    echo "  조기 종료: 세션 파일 없음" >> ".claude/hooks/stop-hook-debug.log"
    echo '{"decision": "allow"}'
    exit 0
fi

# Windows 경로를 Unix 형식으로 변환 (Git Bash vs WSL 자동 감지)
# 1. 백슬래시를 슬래시로 변환
# 2. 이중 슬래시 제거
# 3. 드라이브 문자 변환: C: → /c/ (Git Bash) 또는 /mnt/c (WSL)
if [ -d "/mnt/c" ]; then
    # WSL 환경
    TRANSCRIPT_PATH_UNIX=$(echo "$TRANSCRIPT_PATH" | sed 's|\\|/|g' | sed 's|//|/|g' | sed -E 's|^([A-Za-z]):|/mnt/\L\1|')
    echo "  환경: WSL" >> ".claude/hooks/stop-hook-debug.log"
else
    # Git Bash 환경
    TRANSCRIPT_PATH_UNIX=$(echo "$TRANSCRIPT_PATH" | sed 's|\\|/|g' | sed 's|//|/|g' | sed -E 's|^([A-Za-z]):|/\L\1|')
    echo "  환경: Git Bash" >> ".claude/hooks/stop-hook-debug.log"
fi
echo "  Transcript 경로 변환: $TRANSCRIPT_PATH → $TRANSCRIPT_PATH_UNIX" >> ".claude/hooks/stop-hook-debug.log"

# Hook 실행 환경 정보
echo "  Hook PWD: $PWD" >> ".claude/hooks/stop-hook-debug.log"
echo "  Hook HOME: $HOME" >> ".claude/hooks/stop-hook-debug.log"
echo "  Hook SHELL: $SHELL" >> ".claude/hooks/stop-hook-debug.log"

# ls로 경로 확인
LS_RESULT=$(ls "$TRANSCRIPT_PATH_UNIX" 2>&1)
LS_EXIT=$?
echo "  ls 테스트 종료코드: $LS_EXIT" >> ".claude/hooks/stop-hook-debug.log"
echo "  ls 결과: $LS_RESULT" >> ".claude/hooks/stop-hook-debug.log"

# Transcript에서 마지막 assistant 메시지 추출 (JSONL 형식)
# type="assistant"인 entry 찾기 (message.role이 아닌 최상위 type 사용)
echo "  Transcript 읽기 시도 (마지막 줄 확인)..." >> ".claude/hooks/stop-hook-debug.log"

# 마지막 줄 읽기
LAST_ENTRY=$(tail -n 1 "$TRANSCRIPT_PATH_UNIX" 2>&1)
TAIL_EXIT_CODE=$?

if [ $TAIL_EXIT_CODE -ne 0 ]; then
    echo "  tail 실패: $LAST_ENTRY" >> ".claude/hooks/stop-hook-debug.log"
    echo '{"decision": "allow"}'
    exit 0
fi

echo "  LAST_ENTRY length: ${#LAST_ENTRY}" >> ".claude/hooks/stop-hook-debug.log"
echo "  LAST_ENTRY preview: ${LAST_ENTRY:0:200}..." >> ".claude/hooks/stop-hook-debug.log"

# 빈 줄이면 종료
if [ -z "$LAST_ENTRY" ]; then
    echo "  조기 종료: transcript 마지막 줄이 비어있음" >> ".claude/hooks/stop-hook-debug.log"
    echo '{"decision": "allow"}'
    exit 0
fi

# jq 사용 가능 여부 확인 (프로젝트 폴더 우선)
JQ_CMD=""
if [ -x ".claude/bin/jq" ]; then
    JQ_CMD=".claude/bin/jq"
    echo "  jq available: YES (project bin)" >> ".claude/hooks/stop-hook-debug.log"
elif command -v jq &> /dev/null; then
    JQ_CMD="jq"
    echo "  jq available: YES (system)" >> ".claude/hooks/stop-hook-debug.log"
else
    echo "  jq available: NO" >> ".claude/hooks/stop-hook-debug.log"
fi

if [ -n "$JQ_CMD" ]; then
    # jq로 파싱
    echo "  jq 실행 테스트: $("$JQ_CMD" --version 2>&1 | head -1)" >> ".claude/hooks/stop-hook-debug.log"
    echo "  LAST_ENTRY preview: ${LAST_ENTRY:0:200}..." >> ".claude/hooks/stop-hook-debug.log"
    TYPE=$(echo "$LAST_ENTRY" | "$JQ_CMD" -r '.type' 2>&1)
    JQ_EXIT=$?
    echo "  jq exit code: $JQ_EXIT" >> ".claude/hooks/stop-hook-debug.log"
    echo "  TYPE: $TYPE" >> ".claude/hooks/stop-hook-debug.log"
    if [ "$TYPE" != "assistant" ]; then
        echo "  조기 종료: TYPE이 assistant가 아님 ($TYPE)" >> ".claude/hooks/stop-hook-debug.log"
        echo '{"decision": "allow"}'
        exit 0
    fi

    # 서브에이전트 응답인지 확인 (Task tool 사용 여부)
    IS_AGENT=$(echo "$LAST_ENTRY" | "$JQ_CMD" -r '.message.content[] | select(.type == "tool_use") | select(.name == "Task")' 2>/dev/null)
    echo "  IS_AGENT check: $([ -n "$IS_AGENT" ] && echo 'YES (서브에이전트)' || echo 'NO (메인 대화)')" >> ".claude/hooks/stop-hook-debug.log"
    if [ -n "$IS_AGENT" ]; then
        # 서브에이전트 작업이면 저장 안 함
        echo "  조기 종료: 서브에이전트 응답" >> ".claude/hooks/stop-hook-debug.log"
        echo '{"decision": "allow"}'
        exit 0
    fi

    # 텍스트 내용 추출
    MESSAGE_TEXT=$(echo "$LAST_ENTRY" | "$JQ_CMD" -r '.message.content[] | select(.type == "text") | .text' 2>/dev/null)
    echo "  MESSAGE_TEXT length: ${#MESSAGE_TEXT}" >> ".claude/hooks/stop-hook-debug.log"
else
    # jq 없으면 Python으로 파싱 (python3 또는 python 시도)
    PYTHON_CMD=""
    if command -v python &> /dev/null; then
        PYTHON_CMD="python"
    elif command -v python3 &> /dev/null; then
        PYTHON_CMD="python3"
    fi

    if [ -n "$PYTHON_CMD" ]; then
        echo "  Python 명령: $PYTHON_CMD" >> ".claude/hooks/stop-hook-debug.log"
        TYPE=$(echo "$LAST_ENTRY" | $PYTHON_CMD -c "import sys, json; data = json.loads(sys.stdin.read()); print(data.get('type', ''))" 2>&1)
        PYTHON_EXIT=$?
        echo "  Python exit code: $PYTHON_EXIT" >> ".claude/hooks/stop-hook-debug.log"
        echo "  TYPE (python): $TYPE" >> ".claude/hooks/stop-hook-debug.log"

        if [ $PYTHON_EXIT -ne 0 ]; then
            echo "  Python 파싱 실패: $TYPE" >> ".claude/hooks/stop-hook-debug.log"
            echo '{"decision": "allow"}'
            exit 0
        fi

        if [ "$TYPE" != "assistant" ]; then
            echo "  조기 종료: TYPE이 assistant가 아님 ($TYPE)" >> ".claude/hooks/stop-hook-debug.log"
            echo '{"decision": "allow"}'
            exit 0
        fi

        # Task tool 사용 여부 체크
        HAS_TASK=$(echo "$LAST_ENTRY" | $PYTHON_CMD -c "import sys, json; data = json.loads(sys.stdin.read()); print(any(c.get('name') == 'Task' for c in data.get('message', {}).get('content', []) if c.get('type') == 'tool_use'))" 2>/dev/null || echo "False")
        echo "  HAS_TASK: $HAS_TASK" >> ".claude/hooks/stop-hook-debug.log"

        if [ "$HAS_TASK" = "True" ]; then
            echo "  조기 종료: 서브에이전트 응답 (Task tool 사용)" >> ".claude/hooks/stop-hook-debug.log"
            echo '{"decision": "allow"}'
            exit 0
        fi

        # 텍스트 내용 추출
        MESSAGE_TEXT=$(echo "$LAST_ENTRY" | $PYTHON_CMD -c "import sys, json; data = json.loads(sys.stdin.read()); print('\\n'.join([c.get('text', '') for c in data.get('message', {}).get('content', []) if c.get('type') == 'text']))" 2>/dev/null || echo "")
    else
        # Python도 없으면 포기하고 전체 허용
        echo "  경고: jq와 python 모두 없음 - 파싱 불가" >> ".claude/hooks/stop-hook-debug.log"
        echo '{"decision": "allow"}'
        exit 0
    fi
fi

# 빈 메시지면 종료
if [ -z "$MESSAGE_TEXT" ] || [ "$MESSAGE_TEXT" = "null" ]; then
    echo "  조기 종료: 빈 메시지 (MESSAGE_TEXT=$MESSAGE_TEXT)" >> ".claude/hooks/stop-hook-debug.log"
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
    echo "  조기 종료: 필터링됨 (SHOULD_SAVE=false)" >> ".claude/hooks/stop-hook-debug.log"
    echo '{"decision": "allow"}'
    exit 0
fi

echo "  ✅ 저장 실행: CATEGORY=$CATEGORY" >> ".claude/hooks/stop-hook-debug.log"

# 대화 내용 저장
TIMESTAMP=$(date +"%H:%M:%S")

# Insight 블록 제거 (★ Insight로 시작하는 박스형 텍스트)
# 패턴: `★ Insight ─...` ~ `─...` 사이의 모든 내용 (Python 정규식)
if [ -n "$PYTHON_CMD" ]; then
    MESSAGE_TEXT_FILTERED=$(echo "$MESSAGE_TEXT" | $PYTHON_CMD -c "import sys, re; content = sys.stdin.read(); print(re.sub(r'\`★ Insight ─+\`.*?\`─+\`', '', content, flags=re.DOTALL).strip())" 2>/dev/null || echo "$MESSAGE_TEXT")
    echo "  Insight 제거 완료 (길이: ${#MESSAGE_TEXT} → ${#MESSAGE_TEXT_FILTERED})" >> ".claude/hooks/stop-hook-debug.log"
else
    MESSAGE_TEXT_FILTERED="$MESSAGE_TEXT"
    echo "  Insight 제거 건너뜀 (Python 없음)" >> ".claude/hooks/stop-hook-debug.log"
fi

echo "" >> "$SESSION_FILE"
echo "### $CATEGORY [$TIMESTAMP]" >> "$SESSION_FILE"
echo "" >> "$SESSION_FILE"
echo "$MESSAGE_TEXT_FILTERED" >> "$SESSION_FILE"
echo "" >> "$SESSION_FILE"

# 파일 크기 및 토큰 수 계산 (Windows Git Bash 호환)
FILE_SIZE=$(stat -c%s "$SESSION_FILE" 2>/dev/null || stat -f%z "$SESSION_FILE" 2>/dev/null || wc -c < "$SESSION_FILE" 2>/dev/null || echo "0")
FILE_SIZE_MB=$(awk "BEGIN {printf \"%.2f\", $FILE_SIZE / 1048576}")

# 토큰 수 추정 (1 token ≈ 4 characters for English, ≈ 2 for Korean)
# 한글/영어 혼합이므로 평균 3 characters per token으로 계산
CHAR_COUNT=$(wc -m < "$SESSION_FILE" 2>/dev/null || echo "0")
TOKEN_COUNT=$(awk "BEGIN {printf \"%.0f\", $CHAR_COUNT / 3}")
TOKEN_COUNT_K=$(awk "BEGIN {printf \"%.1f\", $TOKEN_COUNT / 1000}")

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
