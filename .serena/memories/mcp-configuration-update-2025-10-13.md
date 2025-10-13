# MCP Configuration Update (2025-10-13)

## 변경 사항
**project-memory MCP 비활성화**

### Before
```json
{
  "mcpServers": {
    "project-filesystem": { ... },
    "project-memory": {           // ← 제거됨
      "type": "stdio",
      "command": "npx",
      "args": ["-y", "memory-bank-mcp"],
      "env": {
        "MEMORY_BANK_PATH": "./.memory-bank"
      },
      "description": "프로젝트 상태 관리"
    },
    "context7": { ... },
    "github": { ... },
    "sequential-thinking": { ... },
    "task-master-ai": { ... },
    "serena": { ... }
  }
}
```

### After
```json
{
  "mcpServers": {
    "project-filesystem": { ... },
    "context7": { ... },
    "github": { ... },
    "sequential-thinking": { ... },
    "task-master-ai": { ... },
    "serena": { ... }              // ← Serena만 메모리 관리
  }
}
```

## 이유
1. **중복 제거**: project-memory와 memory-bank는 같은 `memory-bank/` 폴더를 사용하여 중복
2. **명확한 역할**: Serena가 코드베이스 메모리 관리 전담
3. **토큰 절약**: activeContext.md의 불필요한 누적 방지
4. **혼란 방지**: 메모리 업데이트 시 어느 시스템을 사용할지 명확

## 활성화된 MCP 서버 (6개)
1. **project-filesystem** - 프로젝트 파일 접근
2. **context7** - 최신 기술 문서 및 API 참조
3. **github** - GitHub 저장소 관리
4. **sequential-thinking** - 단계별 사고 지원
5. **task-master-ai** - 작업 관리
6. **serena** - 코드베이스 메모리 및 분석 (메모리 전담)

## 메모리 사용 정책
### Serena (Primary)
- 모든 프로젝트 컨텍스트 저장
- 구조화된 메모리 관리
- 26개 메모리 파일 활용

### memory-bank 폴더 (Passive)
- MCP 비활성화되었지만 폴더는 유지
- activeContext.md, progress.md는 수동 관리
- 필요 시 Bash 명령어로 직접 수정 가능

## 재시작 필요 여부
Claude Code를 재시작하면 새 MCP 설정이 적용됩니다.
현재 세션에서는 기존 MCP가 계속 활성 상태일 수 있습니다.

## 확인 방법
다음 세션에서:
```bash
# project-memory 도구가 더 이상 표시되지 않아야 함
# Serena 메모리만 사용
mcp__serena__list_memories
```
