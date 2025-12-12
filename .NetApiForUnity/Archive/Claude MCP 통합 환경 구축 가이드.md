 [[🎮 방치형 RPG 서버 개발 완전 가이드 - Week 1]]# Claude MCP 통합 환경 구축 가이드

> 🎯 **목표**: Claude Desktop ↔ Claude Code를 Memory Bank 중심으로 완벽 연계하여 집↔회사 개발 환경 동기화

## 📋 개요

Unity 클라이언트 개발자가 서버 개발을 학습하면서 Claude Desktop과 Claude Code를 효율적으로 연계 사용하는 환경을 구축하는 가이드입니다.

### 핵심 아이디어
- **Claude Desktop**: 질문, 기획, 설계, 학습 → Memory Bank에 저장
- **Claude Code**: Memory Bank에서 컨텍스트 불러와서 → 바로 구현
- **Memory Bank**: 두 환경 간의 연결고리 역할

## 🛠 구축된 MCP 서버 목록

### 1. **Memory Bank MCP** (핵심!)
```json
"memory-bank": {
  "command": "npx",
  "args": ["-y", "memory-bank-mcp"],
  "env": {
    "MEMORY_BANK_PATH": "${PROJECT_ROOT}/.memory-bank"
  },
  "description": "프로젝트 Desktop↔Code 연계 핵심!"
}
```
- **역할**: 프로젝트 상태, 진행사항, 결정사항을 파일로 저장/관리
- **연계 방식**: Desktop에서 저장 → Git으로 동기화 → Code에서 불러오기

### 2. **Sequential Thinking MCP**
```json
"sequential-thinking": {
  "command": "npx", 
  "args": ["-y", "@modelcontextprotocol/server-sequential-thinking"],
  "description": "복잡한 문제를 단계별로 해결"
}
```
- **활용**: ASP.NET Core 아키텍처 설계, 복잡한 디버깅

### 3. **Context7 MCP**
```json
"context7": {
  "command": "npx",
  "args": ["-y", "context7-mcp"],
  "description": "최신 라이브러리 문서 조회"
}
```
- **활용**: .NET 8, Unity 2023.3 최신 문서 참조

### 4. **Filesystem MCP**
```json
"filesystem": {
  "command": "npx",
  "args": ["-y", "@modelcontextprotocol/server-filesystem"],
  "env": {
    "ACCESS_DIRS": "${PROJECT_ROOT},${PROJECT_ROOT}/Assets,${PROJECT_ROOT}/ServerCode,${PROJECT_ROOT}/Documentation"
  },
  "description": "프로젝트 파일 시스템 관리"
}
```
- **활용**: Unity Assets, ASP.NET Core 서버 코드 직접 관리

### 5. **Obsidian MCP** (uvx 기반)
```json
"mcp-obsidian": {
  "command": "uvx",
  "args": ["mcp-obsidian"],
  "env": {
    "OBSIDIAN_API_KEY": "cb4acf0367656419acf85c6f64eed186f3978f3a937bae28bf2a3483d83fcc85",
    "OBSIDIAN_HOST": "https://127.0.0.1",
    "OBSIDIAN_PORT": "27124"
  },
  "description": "학습 노트 관리"
}
```
- **활용**: 이 노트들을 Claude에서 직접 검색/수정
- **요구사항**: Python + uvx + Obsidian Local REST API 플러그인

### 6. **Unity MCP**
```json
"unity": {
  "command": "npx",
  "args": ["-y", "unity-mcp"],
  "env": {
    "UNITY_PROJECT_PATH": "${PROJECT_ROOT}"
  },
  "description": "Unity 프로젝트 관리"
}
```
- **활용**: Unity 프로젝트 설정, 스크립트 관리

### 7. **GitHub MCP**
```json
"github": {
  "command": "npx",
  "args": ["-y", "@modelcontextprotocol/server-github"],
  "env": {
    "GITHUB_PERSONAL_ACCESS_TOKEN": "${GITHUB_TOKEN}"
  },
  "description": "GitHub 저장소 관리"
}
```
- **활용**: 이슈 관리, PR 리뷰, 코드 검색

## 🔄 환경 동기화 시스템

### 폴더 구조
```
프로젝트루트/
├── claude-configs/           # Claude 설정 전용 폴더
│   ├── unified-mcp-config.json  # 통합 MCP 설정
│   ├── sync.sh                  # 환경 전환 스크립트
│   ├── .env.home               # 집 환경 변수
│   └── .env.work               # 회사 환경 변수
├── .memory-bank/              # Memory Bank 데이터
├── .mcp.json                  # 프로젝트 전용 MCP 설정
└── README.md                  # 사용법 가이드
```

### 환경 전환 방법
```bash
# 집에서 개발할 때
cd claude-configs
./sync.sh home

# 회사에서 개발할 때  
cd claude-configs
./sync.sh work
```

### 환경 변수 설정
```bash
# .env.home / .env.work 공통
PROJECT_ROOT="/path/to/project"
GITHUB_TOKEN="your_github_token"
OBSIDIAN_API_KEY="cb4acf0367656419acf85c6f64eed186f3978f3a937bae28bf2a3483d83fcc85"
OBSIDIAN_HOST="https://127.0.0.1"
OBSIDIAN_PORT="27124"
```

## 🎯 실제 사용 워크플로우

### 1. **질문/학습 단계 (Claude Desktop)**
```
"ASP.NET Core Web API에서 JWT 인증을 구현하려고 하는데, 
Unity 클라이언트에서 어떻게 사용하는지도 함께 설계해줘.
이 내용을 Memory Bank에 저장해줘."
```

### 2. **구현 단계 (Claude Code)**
```bash
claude "Memory Bank에서 JWT 인증 설계 내용을 확인하고, 
실제 ASP.NET Core 컨트롤러 코드를 구현해줘"
```

### 3. **학습 노트 정리 (Obsidian + Claude)**
```
"방금 구현한 JWT 인증 내용을 Obsidian의 '인증 시스템 학습' 노트에 
정리해서 추가해줘"
```

## 📝 Memory Bank 활용 패턴

### 프로젝트 상태 저장
- **설계 결정사항**: 아키텍처 선택 이유, 트레이드오프
- **구현 계획**: 다음에 해야 할 작업들
- **학습 내용**: 새로 배운 개념, 실수했던 부분
- **이슈 및 해결책**: 발생한 문제와 해결 과정

### 연계 명령어 예시
```bash
# Desktop에서
"현재 프로젝트 진행 상황을 Memory Bank에 정리해줘"
"오늘 학습한 ASP.NET Core 내용을 Memory Bank에 저장해줘"

# Code에서  
claude "Memory Bank에서 다음 작업 계획을 확인해줘"
claude "Memory Bank의 설계 내용을 바탕으로 코드를 생성해줘"
```

## ⚠️ 현재 알려진 이슈

### 1. **Memory Bank Git 동기화 문제**
- **문제**: `.memory-bank/`가 `.gitignore`에 포함되어 실제 동기화 안 됨
- **임시해결**: 중요한 내용은 수동으로 README나 문서에 정리
- **근본해결 필요**: Memory Bank를 Git에 포함하거나 클라우드 폴더 사용

### 2. **MCP 서버 응답 지연**
- **문제**: 일부 MCP 서버(특히 memory-bank-mcp)에서 무한 로딩 발생
- **대처**: 서버 재시작 후 재시도

### 3. **Obsidian MCP 요구사항**  
- **필요**: Python + uvx + Obsidian Local REST API 플러그인
- **설치**: `pip install uvx`, Obsidian 플러그인 활성화

## 🚀 자동 설치 스크립트

프로젝트 루트에서 실행:
```bash
curl -sSL https://raw.githubusercontent.com/user/repo/main/setup-claude-mcp.sh | bash
```

스크립트 기능:
- ✅ MCP 서버 통합 설정 생성
- ✅ 환경별 설정 파일 생성  
- ✅ 동기화 스크립트 생성
- ✅ 필요한 패키지 설치
- ✅ Git 설정 업데이트

## 🔮 향후 개선 계획

### 1. **Memory Bank 동기화 개선**
```bash
# 옵션 A: 선별적 Git 포함
.memory-bank/secrets/    # 제외
.memory-bank/cache/      # 제외  
.memory-bank/*.md        # 포함

# 옵션 B: 클라우드 동기화
MEMORY_BANK_PATH="~/Dropbox/DevProjects/.memory-bank"
```

### 2. **개발환경별 MCP 설정**
```json
// 개발용 - 모든 MCP 활성화
// 운영용 - 보안 관련 MCP만
// 학습용 - 문서/노트 관련 MCP 중심
```

### 3. **자동화 확장**
- CI/CD 파이프라인에 Claude Code 통합
- 코드 리뷰 자동화
- 문서 자동 생성

---

## 📚 관련 문서

- [[Unity 서버개발 학습 일지]] - 전체 학습 진행상황
- [[Week 1 Day 0 - 환경구축 완료]] - 초기 환경 설정
- [ASP.NET Core Web API 가이드](/) - 서버 개발 학습 자료

## 🏷 태그

#Claude #MCP #환경구축 #Unity #ASP.NET #서버개발 #자동화 #Memory-Bank

---

**최종 업데이트**: 2025년 9월 26일  
**작성자**: 3년차 Unity 개발자
**상태**: 구축 완료, 개선 진행 중
