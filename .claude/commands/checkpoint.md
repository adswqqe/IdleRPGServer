# Checkpoint: Save Progress & Update Unity Docs

현재 작업 진행사항을 Serena 메모리에 저장하고 Unity 문서를 자동으로 업데이트합니다.

## ⚠️ 중요: Serena MCP 메모리 사용 필수

**이 명령어는 반드시 Serena MCP의 `mcp__serena__write_memory` 도구를 사용해야 합니다.**

- ❌ **사용 금지**: `Edit`, `Write` 도구로 `memory-bank/` 폴더 직접 수정
- ✅ **사용 필수**: `mcp__serena__write_memory` 도구로 메모리 저장

## 수행 단계

### 1. 현재 작업 진행사항 Serena 메모리 저장

**도구 사용**: `mcp__serena__write_memory`

**메모리 이름**: `checkpoint-progress-YYYYMMDD-HHMM` (예: `checkpoint-progress-20251014-2050`)

**저장할 내용**:
- CLAUDE.md의 "Current State" 섹션에서 현재 작업 상태 확인
- Week 2 진행 상황 (완료된 Feature, 진행 중인 Feature)
- 최근 완성된 작업 (APIs, 마이그레이션, 문서화)
- 다음 단계 (Next Steps)
- 현재 차단된 이슈 (Blocked Issues)
- 기술 부채 (Tech Debt)

**형식**:
```markdown
# Checkpoint Progress - YYYY-MM-DD HH:MM KST

## Current Phase
Week 2 - Idle Game Loop & Progression System

## Completed Features
- ✅ Feature 1: ...
- ✅ Feature 2: ...

## In Progress
- 🚧 Feature 4: ...

## Recent Achievements
- ...

## Next Steps
1. ...
2. ...

## Blocked Issues
- ...

## Tech Debt
- ...
```

### 2. 서버 API 구현 상태 확인
다음 API Controller 파일들을 확인하여 구현 여부 파악:
- `IdleRPG.API/Controllers/AuthController.cs` - 인증 API
- `IdleRPG.API/Controllers/CharacterController.cs` - 캐릭터 API

각 엔드포인트의 구현 상태를 다음 기준으로 분류:
- ✅ **완료**: Controller에 엔드포인트가 구현되고 로직이 완성됨
- 🚧 **개발 중**: Controller에 엔드포인트가 있지만 로직이 미완성
- 📋 **예정**: Controller에 엔드포인트가 없음

### 3. Unity 문서 현재 상태 확인
`../IdleRPGClient/Docs/unity/API_SPEC_FOR_UNITY.md` 파일을 읽어서:
- "구현 상태" 섹션의 API별 구현 상태 테이블 확인
- 각 API의 현재 상태 (✅ 완료, 🚧 개발 중, 📋 예정) 파악

### 4. 서버 vs Unity 문서 비교
서버의 실제 구현 상태와 Unity 문서의 상태를 비교:
- 차이가 있는 API 엔드포인트 식별
- 업데이트가 필요한 항목 리스트 작성

### 5. Unity 문서 자동 업데이트

#### 5.1 API_SPEC_FOR_UNITY.md 업데이트
차이가 있는 경우:
- `API_SPEC_FOR_UNITY.md`의 "구현 상태" 테이블 업데이트
- JSON 응답 예시 갱신 (DTO 구조 변경 시)
- "마지막 업데이트" 날짜를 오늘 날짜로 변경
- "업데이트 이력" 섹션에 변경사항 기록

차이가 없는 경우:
- "Unity 문서가 최신 상태입니다" 메시지 출력

#### 5.2 Unity-DTOs.cs 동기화 (중요!)
**서버 DTO 구조 변경 시 반드시 동기화 필요:**
- `IdleRPG.Application/DTOs/` 폴더의 실제 DTO 클래스 확인
- `../IdleRPGClient/Docs/Unity-DTOs.cs` 파일의 DTO 클래스와 비교
- 불일치 발견 시:
  - Unity-DTOs.cs의 해당 클래스 업데이트
  - `[Serializable]` 속성 유지 (Unity JsonUtility 필수)
  - XML 주석으로 변경사항 및 BREAKING CHANGE 표시
  - 사용 예시 코드도 함께 수정

**동기화 대상 DTO:**
- AuthResponse, ProfileResponse
- CharacterData, CharacterStats
- AddExperienceRequest
- 기타 요청/응답 DTO 클래스

### 6. API 구현 상태 Serena 메모리 저장

**도구 사용**: `mcp__serena__write_memory`

**메모리 이름**: `checkpoint-api-status-YYYYMMDD-HHMM`

**저장할 내용**:
```markdown
# API Implementation Status - YYYY-MM-DD HH:MM KST

## Authentication API
- ✅ POST /api/auth/register - 완료
- ✅ POST /api/auth/login - 완료
- ✅ POST /api/auth/refresh - 완료
- ✅ POST /api/auth/logout - 완료
- ✅ GET /api/auth/profile - 완료

## Character API
- ✅ POST /api/character/Create - 완료
- ✅ GET /api/character/GetCharacters - 완료
- ✅ GET /api/character/{id} - 완료
- ✅ DELETE /api/character/{id} - 완료
- ✅ POST /api/character/{id}/experience - 완료
- ⚠️ PUT /api/character/{id}/stats - 삭제됨 (자동 성장 시스템)

## Battle API
- ✅ POST /api/battle/start - 완료

## Summary
- Total Endpoints: 11
- Completed: 10
- Removed: 1 (stats allocation)
- In Progress: 0
```

### 7. 결과 보고
다음 정보를 사용자에게 보고:
- 💾 **Serena 메모리에 저장 완료**
  - `checkpoint-progress-YYYYMMDD-HHMM`: 진행사항 요약
  - `checkpoint-api-status-YYYYMMDD-HHMM`: API 구현 상태
- 📊 서버 API 구현 상태 요약
- 📝 Unity 문서 업데이트 여부 및 변경 내역
  - API_SPEC_FOR_UNITY.md 변경사항
  - Unity-DTOs.cs 동기화 여부 및 변경된 클래스 목록
- 📌 다음 작업 제안 (CLAUDE.md의 "Current Task" 기반)

## 사용 예시
```bash
/checkpoint
```

## 메모리 확인 방법
저장된 메모리를 확인하려면:
```bash
# Serena 메모리 목록 조회
mcp__serena__list_memories

# 특정 메모리 읽기
mcp__serena__read_memory --memory_file_name="checkpoint-progress-20251014-2050"
```

## 참고
- ⚠️ **이 명령어는 Serena MCP의 `write_memory` 도구만 사용합니다**
- `memory-bank/` 폴더는 직접 수정하지 않습니다
- 이 명령어는 작업 세션 종료 전이나 주요 기능 완성 후 실행하세요
- Unity 문서 경로:
  - API 스펙: `../IdleRPGClient/Docs/unity/API_SPEC_FOR_UNITY.md`
  - DTO 클래스: `../IdleRPGClient/Docs/Unity-DTOs.cs` ⚠️ 동기화 필수!
- DTO 구조 변경 시 Unity-DTOs.cs를 반드시 함께 업데이트해야 Unity 개발자가 올바른 클래스를 사용할 수 있습니다

## 메모리 저장 위치
- **Serena MCP**: `.serena/memories/` (프로젝트별 관리)
- **이점**:
  - 프로젝트 간 메모리 분리
  - 자동 타임스탬프 관리
  - Serena 도구와 통합
