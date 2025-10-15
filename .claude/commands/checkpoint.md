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

### 5. Unity 문서 자동 업데이트 (기능별 분리 구조)

> ⚠️ **문서 구조 변경**: API 문서와 DTO를 기능별로 분리하여 관리합니다.

#### 5.1 기능별 API 문서 업데이트

**문서 구조**:
```
../IdleRPGClient/Docs/unity/
├── API-Overview.md              # API 전체 개요 및 목차 (구현 상태 테이블)
├── Common-Specs.md              # 공통 사양 (인증, 응답 형식, 에러 처리)
├── api/
│   ├── Auth-API.md              # 인증 API (5개 엔드포인트)
│   ├── Character-API.md         # 캐릭터 API (5개 엔드포인트)
│   ├── Battle-API.md            # 전투 API (4개 엔드포인트)
│   ├── Monster-API.md           # 몬스터 API (1개 엔드포인트)
│   └── Reward-API.md            # 보상 API (2개 엔드포인트)
└── dto/
    ├── AuthDTO.cs               # 인증 관련 DTO
    ├── CharacterDTO.cs          # 캐릭터 관련 DTO
    ├── BattleDTO.cs             # 전투 관련 DTO
    ├── MonsterDTO.cs            # 몬스터 관련 DTO
    ├── RewardDTO.cs             # 보상 관련 DTO
    └── CommonDTO.cs             # 공통 DTO (ErrorResponse 등)
```

**업데이트 절차**:
1. **API-Overview.md**: 전체 API 구현 상태 테이블 업데이트
2. **기능별 API 문서**: 변경된 기능에 해당하는 파일만 업데이트
   - 예: Character API 변경 시 `api/Character-API.md`만 수정
   - JSON 응답 예시, Unity C# 예시 코드 갱신
   - 각 파일의 "마지막 업데이트" 날짜 변경
3. **Common-Specs.md**: 공통 사양 변경 시에만 업데이트

**장점**:
- 🎯 필요한 파일만 수정 (변경 범위 최소화)
- 📝 Git diff가 명확해짐 (전체 문서 변경 방지)
- 🚀 Unity 개발자가 필요한 API만 빠르게 참조
- 🔧 병렬 개발 시 문서 충돌 최소화

#### 5.2 기능별 DTO 파일 동기화 (중요!)

**서버 DTO 구조 변경 시 반드시 동기화 필요:**
- `IdleRPG.Application/DTOs/` 폴더의 실제 DTO 클래스 확인
- `../IdleRPGClient/Docs/unity/dto/` 폴더의 해당 DTO 파일과 비교
- 불일치 발견 시:
  - 해당 DTO 파일만 업데이트 (예: `CharacterDTO.cs`)
  - `[Serializable]` 속성 유지 (Unity JsonUtility 필수)
  - XML 주석으로 변경사항 및 BREAKING CHANGE 표시
  - 사용 예시 코드도 함께 수정

**기능별 DTO 매핑**:
| 서버 DTO 경로 | Unity DTO 파일 | 포함 클래스 |
|-------------|---------------|-----------|
| `DTOs/Auth/` | `dto/AuthDTO.cs` | RegisterRequest, LoginRequest, AuthResponseDto, ProfileDto |
| `DTOs/Character/` | `dto/CharacterDTO.cs` | CharacterDto, CharacterStats, AddExperienceRequest |
| `DTOs/Battle/` | `dto/BattleDTO.cs` | BattleRequest, BattleResponseDto, BattleLogDto, BattleStatsDto |
| `DTOs/Monster/` | `dto/MonsterDTO.cs` | RandomMonsterResponse |
| `DTOs/Reward/` | `dto/RewardDTO.cs` | OfflineRewardDto, ClaimOfflineRewardResponseDto |
| 공통 | `dto/CommonDTO.cs` | ErrorResponse, ApiResponse<T> |

#### 5.3 기존 통합 문서 마이그레이션 (1회만)

**첫 체크포인트 시 자동 수행**:
1. 기존 `API_SPEC_FOR_UNITY.md` 읽기
2. 내용을 기능별로 분할하여 새 파일들에 저장
3. 기존 `Unity-DTOs.cs` 읽기
4. DTO를 기능별로 분할하여 `dto/` 폴더에 저장
5. 기존 파일 보관 (`_deprecated/` 폴더로 이동)
6. `API-Overview.md`에 새 구조 안내 추가

**마이그레이션 확인**:
- `../IdleRPGClient/Docs/unity/api/` 폴더 존재 여부 확인
- 존재하지 않으면 마이그레이션 수행
- 존재하면 기능별 업데이트만 수행

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
- 📊 서버 API 구현 상태 요약 (기능별)
- 📝 Unity 문서 업데이트 여부 및 변경 내역
  - **마이그레이션 완료** (첫 실행 시): 기존 통합 문서 → 기능별 분리 완료
  - **API 문서 업데이트**: 변경된 기능별 파일 목록
    - 예: `api/Character-API.md` 업데이트 완료
  - **DTO 동기화**: 변경된 DTO 파일 목록
    - 예: `dto/CharacterDTO.cs` 업데이트 완료
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

### Unity 문서 구조 (기능별 분리)
**신규 구조** (2025-10-15부터 적용):
```
../IdleRPGClient/Docs/unity/
├── API-Overview.md              # 📋 API 전체 개요 (시작점)
├── Common-Specs.md              # 🔧 공통 사양 (인증, 에러 등)
├── api/                         # 📂 기능별 API 문서
│   ├── Auth-API.md
│   ├── Character-API.md
│   ├── Battle-API.md
│   ├── Monster-API.md
│   └── Reward-API.md
├── dto/                         # 📂 기능별 DTO 클래스
│   ├── AuthDTO.cs
│   ├── CharacterDTO.cs
│   ├── BattleDTO.cs
│   ├── MonsterDTO.cs
│   ├── RewardDTO.cs
│   └── CommonDTO.cs
└── _deprecated/                 # 📦 기존 통합 문서 (참고용)
    ├── API_SPEC_FOR_UNITY.md
    └── Unity-DTOs.cs
```

**변경 이유**:
- 기존 `API_SPEC_FOR_UNITY.md`가 1400줄 초과로 관리 어려움
- 기능별 수정 시 전체 문서 변경 방지 (Git diff 최소화)
- Unity 개발자가 필요한 API만 빠르게 참조 가능

**마이그레이션**:
- 첫 `/checkpoint` 실행 시 자동으로 문서 분리 수행
- 기존 파일은 `_deprecated/` 폴더에 보관

### DTO 동기화 필수!
- DTO 구조 변경 시 해당 기능의 DTO 파일을 반드시 업데이트
- 예: Character DTO 변경 → `dto/CharacterDTO.cs` 업데이트
- Unity 개발자가 올바른 클래스 구조를 사용할 수 있도록 보장

## 메모리 저장 위치
- **Serena MCP**: `.serena/memories/` (프로젝트별 관리)
- **이점**:
  - 프로젝트 간 메모리 분리
  - 자동 타임스탬프 관리
  - Serena 도구와 통합
