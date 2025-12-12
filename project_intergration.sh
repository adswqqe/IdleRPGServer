#!/bin/bash
# 기존 프로젝트에 Claude Desktop + Code 통합 환경 추가
# 사용법: 기존 프로젝트 루트에서 실행

set -e

PROJECT_ROOT="$(pwd)"
CLAUDE_DIR="$PROJECT_ROOT/claude-configs"

echo "🚀 기존 프로젝트에 Claude 통합 환경 추가"
echo "📁 프로젝트 경로: $PROJECT_ROOT"

# 1. Claude 설정 폴더 생성
setup_claude_directory() {
    echo "📁 Claude 설정 폴더 생성 중..."
    
    mkdir -p "$CLAUDE_DIR"
    cd "$CLAUDE_DIR"
    
    echo "✅ Claude 설정 폴더 생성: $CLAUDE_DIR"
}

# 2. 프로젝트 특화 MCP 설정 생성 (claude-continuity 제거)
create_project_mcp_config() {
    echo "⚙️  프로젝트 특화 MCP 설정 생성 중..."
    
    # claude-continuity 제거된 MCP 설정
    cat > "$CLAUDE_DIR/unified-mcp-config.json" << EOF
{
  "mcpServers": {
    "sequential-thinking": {
      "command": "npx",
      "args": ["-y", "@modelcontextprotocol/server-sequential-thinking"],
      "description": "복잡한 문제를 단계별로 해결"
    },
    "filesystem": {
      "command": "npx",
      "args": ["-y", "@modelcontextprotocol/server-filesystem"],
      "env": {
        "ACCESS_DIRS": "\${PROJECT_ROOT},\${PROJECT_ROOT}/Assets,\${PROJECT_ROOT}/ServerCode,\${PROJECT_ROOT}/Documentation"
      },
      "description": "프로젝트 파일 시스템 관리"
    },
    "memory-bank": {
      "command": "npx",
      "args": ["-y", "memory-bank-mcp"], 
      "env": {
        "MEMORY_BANK_PATH": "\${PROJECT_ROOT}/.memory-bank"
      },
      "description": "프로젝트 Desktop↔Code 연계 핵심!"
    },
    "mcp-obsidian": {
      "command": "uvx",
      "args": [
        "mcp-obsidian"
      ],
      "env": {
        "OBSIDIAN_API_KEY": "\${OBSIDIAN_API_KEY}",
        "OBSIDIAN_HOST": "\${OBSIDIAN_HOST}",
        "OBSIDIAN_PORT": "\${OBSIDIAN_PORT}"
      },
      "description": "학습 노트 관리"
    },
    "unity": {
      "command": "npx", 
      "args": ["-y", "unity-mcp"],
      "env": {
        "UNITY_PROJECT_PATH": "\${PROJECT_ROOT}"
      },
      "description": "이 Unity 프로젝트 관리"
    },
    "github": {
      "command": "npx",
      "args": ["-y", "@modelcontextprotocol/server-github"],
      "env": {
        "GITHUB_PERSONAL_ACCESS_TOKEN": "\${GITHUB_TOKEN}"
      },
      "description": "이 프로젝트 GitHub 관리"
    }
  }
}
EOF

    echo "✅ 프로젝트 특화 MCP 설정 생성 완료"
}

# 3. 환경 변수 템플릿 생성 (프로젝트 기반)
create_project_env_templates() {
    echo "🔧 프로젝트 환경 변수 생성 중..."
    
    # .env.home (집 환경)
    cat > "$CLAUDE_DIR/.env.home" << EOF
# 프로젝트 환경 설정 - 집
PROJECT_ROOT="$PROJECT_ROOT"

# Obsidian MCP 설정
OBSIDIAN_API_KEY="cb4acf0367656419acf85c6f64eed186f3978f3a937bae28bf2a3483d83fcc85"
OBSIDIAN_HOST="https://127.0.0.1"
OBSIDIAN_PORT="27124"

# API 키 (실제 값으로 수정!)
GITHUB_TOKEN="your_github_token_here"

# 프로젝트 특화 설정
PROJECT_NAME="$(basename "$PROJECT_ROOT")"
UNITY_VERSION="2023.3"
ASPNET_ENVIRONMENT="Development"
EOF

    # .env.work (회사 환경)  
    cat > "$CLAUDE_DIR/.env.work" << EOF
# 프로젝트 환경 설정 - 회사
PROJECT_ROOT="$PROJECT_ROOT"

# Obsidian MCP 설정 (집과 동일)
OBSIDIAN_API_KEY="cb4acf0367656419acf85c6f64eed186f3978f3a937bae28bf2a3483d83fcc85"
OBSIDIAN_HOST="https://127.0.0.1"
OBSIDIAN_PORT="27124"

# API 키 (집과 동일)
GITHUB_TOKEN="your_github_token_here"

# 프로젝트 특화 설정
PROJECT_NAME="$(basename "$PROJECT_ROOT")"
UNITY_VERSION="2023.3"
ASPNET_ENVIRONMENT="Development"
EOF

    echo "✅ 환경 변수 템플릿 생성 완료"
}

# 4. 프로젝트 동기화 스크립트 생성
create_project_sync_script() {
    echo "📱 프로젝트 동기화 스크립트 생성 중..."
    
    cat > "$CLAUDE_DIR/sync.sh" << 'EOF'
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
EOF

    chmod +x "$CLAUDE_DIR/sync.sh"
    
    echo "✅ 동기화 스크립트 생성 완료"
}

# 5. 프로젝트 .gitignore 업데이트 (claude-continuity 관련 제거)
update_project_gitignore() {
    echo "🔒 프로젝트 .gitignore 업데이트 중..."
    
    # 기존 .gitignore에 Claude 관련 항목 추가
    if [[ -f "$PROJECT_ROOT/.gitignore" ]]; then
        # Claude 설정이 이미 있는지 확인
        if ! grep -q "# Claude MCP" "$PROJECT_ROOT/.gitignore"; then
            cat >> "$PROJECT_ROOT/.gitignore" << 'EOF'

# Claude MCP 보안 설정
claude-configs/.env.home
claude-configs/.env.work
claude-configs/secrets/
claude-configs/*.key
claude-configs/*token*

# Claude 작업 데이터
.memory-bank/
EOF
            echo "  ✅ 기존 .gitignore에 Claude 항목 추가"
        else
            echo "  ✅ .gitignore에 Claude 항목이 이미 존재함"
        fi
    else
        # 새 .gitignore 생성
        cat > "$PROJECT_ROOT/.gitignore" << 'EOF'
# Claude MCP 보안 설정
claude-configs/.env.home
claude-configs/.env.work
claude-configs/secrets/
claude-configs/*.key
claude-configs/*token*

# Claude 작업 데이터
.memory-bank/

# Unity
[Ll]ibrary/
[Tt]emp/
[Oo]bj/
[Bb]uild/
[Bb]uilds/
Assets/AssetStoreTools*

# ASP.NET Core
bin/
obj/
*.user
*.userprefs
.vs/

# 시스템 파일
.DS_Store
Thumbs.db
EOF
        echo "  ✅ 새 .gitignore 생성 완료"
    fi
}

# 6. 프로젝트 전용 MCP 설정 (.mcp.json) - claude-continuity 제거
create_project_mcp_json() {
    echo "🎯 프로젝트 전용 MCP 설정 생성 중..."
    
    cat > "$PROJECT_ROOT/.mcp.json" << 'EOF'
{
  "name": "Unity ASP.NET Core 프로젝트 MCP 설정",
  "mcpServers": {
    "project-filesystem": {
      "command": "npx",
      "args": ["-y", "@modelcontextprotocol/server-filesystem"],
      "env": {
        "ACCESS_DIRS": "./Assets,./ServerCode,./Documentation,./claude-configs"
      },
      "description": "프로젝트 파일 접근"
    },
    "project-memory": {
      "command": "npx",
      "args": ["-y", "memory-bank-mcp"],
      "env": {
        "MEMORY_BANK_PATH": "./.memory-bank"
      },
      "description": "프로젝트 상태 관리"
    },
    "project-unity": {
      "command": "npx",
      "args": ["-y", "unity-mcp"],
      "env": {
        "UNITY_PROJECT_PATH": "."
      },
      "description": "Unity 프로젝트 관리"
    }
  }
}
EOF

    echo "✅ 프로젝트 전용 .mcp.json 생성 완료"
}

# 7. MCP 패키지 설치 (mcp-obsidian은 uvx 사용)
install_mcp_packages() {
    echo "📦 MCP 패키지 설치 중..."
    
    local packages=(
        "@modelcontextprotocol/server-sequential-thinking"
        "@modelcontextprotocol/server-filesystem"
        "memory-bank-mcp"
        "unity-mcp"
        "@modelcontextprotocol/server-github"
    )
    
    for package in "${packages[@]}"; do
        echo "  📥 $package 설치 중..."
        npm install -g "$package" &>/dev/null || echo "    ⚠️  $package 설치 실패 (이미 설치됨)"
    done
    
    echo "  📝 mcp-obsidian은 uvx로 실행되므로 별도 설치 불필요"
    echo "  💡 Python과 uvx가 설치되어 있는지 확인하세요"
    
    echo "✅ MCP 패키지 설치 완료"
}

# 8. README 업데이트
update_project_readme() {
    echo "📖 프로젝트 README 업데이트 중..."
    
    # Claude 섹션을 README에 추가 (있으면 스킵)
    if [[ -f "$PROJECT_ROOT/README.md" ]]; then
        if ! grep -q "Claude 통합 환경" "$PROJECT_ROOT/README.md"; then
            cat >> "$PROJECT_ROOT/README.md" << 'EOF'

## 🤖 Claude 통합 개발 환경

이 프로젝트는 Claude Desktop ↔ Claude Code 연계 환경을 지원합니다.

### 환경 설정
```bash
# 집에서 개발할 때
cd claude-configs
./sync.sh home

# 회사에서 개발할 때  
cd claude-configs
./sync.sh work
```

### 사용 방법
1. **Claude Desktop**: 질문, 기획, 설계 → Memory Bank 저장
2. **Claude Code**: Memory Bank에서 불러와서 → 코드 구현

### 연계 테스트
- Desktop: "이 프로젝트 상태를 Memory Bank에 저장해줘"  
- Code: `claude "Memory Bank에서 프로젝트 상태 확인해줘"`

EOF
            echo "  ✅ README에 Claude 섹션 추가"
        else
            echo "  ✅ README에 Claude 섹션이 이미 존재함"
        fi
    fi
}

# 9. Git 커밋 준비
prepare_git_commit() {
    echo "📡 Git 커밋 준비 중..."
    
    cd "$PROJECT_ROOT"
    
    # 변경사항 추가 (보안 파일 제외)
    git add claude-configs/
    git add .gitignore
    git add .mcp.json
    
    if [[ -f README.md ]]; then
        git add README.md
    fi
    
    echo "✅ Git 커밋 준비 완료"
    echo "💡 다음 명령으로 커밋하세요:"
    echo "    git commit -m 'Add Claude Desktop+Code integration to project'"
}

# 10. 완료 메시지
show_completion_message() {
    echo ""
    echo "=========================================="
    echo "🎉 프로젝트 Claude 통합 환경 추가 완료!"
    echo "=========================================="
    
    cat << EOF

📁 추가된 파일들:
  📂 claude-configs/           # Claude 설정 폴더
  ├── unified-mcp-config.json  # MCP 설정
  ├── sync.sh                  # 환경 동기화 스크립트
  ├── .env.home                # 집 환경 변수 (수정 필요!)
  └── .env.work                # 회사 환경 변수 (수정 필요!)
  
  📄 .mcp.json                 # 프로젝트 전용 MCP
  📄 .gitignore                # 보안 파일 제외
  📄 README.md                 # 사용법 추가

🔧 다음 단계:
1. API 키 설정:
   cd claude-configs
   # .env.home과 .env.work 파일에서 GitHub 토큰 입력
   # Obsidian API 키는 이미 설정됨 (필요시 수정)

2. Python과 uvx 설치 확인 (mcp-obsidian용):
   pip install uvx  # 또는 pipx install uvx

3. 환경 동기화:
   ./sync.sh home  # 집에서
   ./sync.sh work  # 회사에서

3. 연계 테스트:
   - Desktop: "이 $(basename "$PROJECT_ROOT") 프로젝트를 분석하고 Memory Bank에 저장해줘"
   - Code: claude "Memory Bank에서 프로젝트 상태를 확인하고 다음 작업 계획을 세워줘"
   - Obsidian: "개발 노트를 Obsidian에서 검색해줘"

4. Git 커밋:
   git commit -m "Add Claude Desktop+Code integration to project"
   git push

🎯 이제 이 프로젝트에서:
  ✅ Desktop에서 질문 → Memory Bank 저장
  ✅ Code에서 Memory Bank → 바로 구현
  ✅ 집↔회사 환경 완벽 동기화
  ✅ 프로젝트 컨텍스트 유지
  ✅ Obsidian 노트 연동 (uvx 기반)

💡 Obsidian MCP 사용 전 확인:
  - Python 설치되어 있는지 확인
  - uvx 설치: pip install uvx
  - Obsidian Local REST API 플러그인 활성화

EOF
}

# 메인 실행 함수
main() {
    # 현재 디렉토리가 Git 저장소인지 확인
    if [[ ! -d ".git" ]]; then
        echo "❌ 현재 디렉토리가 Git 저장소가 아닙니다"
        echo "💡 프로젝트 루트 디렉토리에서 실행하세요"
        exit 1
    fi
    
    setup_claude_directory
    create_project_mcp_config
    create_project_env_templates
    create_project_sync_script
    update_project_gitignore
    create_project_mcp_json
    install_mcp_packages
    update_project_readme
    prepare_git_commit
    show_completion_message
}

# 스크립트 실행
main "$@"