#!/bin/bash
# 프로젝트 Claude 환경 동기화 스크립트

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
ENVIRONMENT=${1:-home}

echo "🔄 프로젝트 Claude 환경 동기화: $ENVIRONMENT"
echo "📁 프로젝트: $(basename "$PROJECT_ROOT")"

# 환경 변수 로드
if [[ -f "$SCRIPT_DIR/.env.$ENVIRONMENT" ]]; then
    source "$SCRIPT_DIR/.env.$ENVIRONMENT"
else
    echo "❌ 환경 파일이 없습니다: .env.$ENVIRONMENT"
    exit 1
fi

# Claude 설정 경로 확인
if [[ "$OSTYPE" == "darwin"* ]]; then
    DESKTOP_CONFIG="$HOME/Library/Application Support/Claude/claude_desktop_config.json"
    CODE_CONFIG="$HOME/.claude.json"
else
    DESKTOP_CONFIG="$HOME/.config/claude/claude_desktop_config.json"  
    CODE_CONFIG="$HOME/.claude.json"
fi

# 필요한 디렉토리 생성
mkdir -p "$(dirname "$DESKTOP_CONFIG")"
mkdir -p "$(dirname "$CODE_CONFIG")"
mkdir -p "$PROJECT_ROOT/.memory-bank"

# 환경 변수 치환하여 설정 적용
envsubst < "$SCRIPT_DIR/unified-mcp-config.json" > "$DESKTOP_CONFIG"
envsubst < "$SCRIPT_DIR/unified-mcp-config.json" > "$CODE_CONFIG"

echo "✅ $ENVIRONMENT 환경 동기화 완료"
echo "💡 Claude Desktop과 Code 재시작 후 테스트하세요"

# 연계 테스트 가이드
cat << TESTEOF

🧪 연계 테스트 방법:
1. Claude Desktop에서:
   "이 $(basename "$PROJECT_ROOT") 프로젝트 상태를 Memory Bank에 저장해줘"

2. Claude Code에서:
   claude "Memory Bank에서 프로젝트 상태 확인하고 다음 작업 알려줘"

3. Obsidian 테스트:
   "Obsidian에서 Unity 관련 노트를 검색해줘"

TESTEOF
