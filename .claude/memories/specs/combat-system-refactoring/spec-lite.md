# Combat System Refactoring - Lite Spec

**Status**: Draft
**Size**: M (Medium)
**Created**: 2025-10-28
**Type**: Refactoring

---

## 1. Intent & Scope

**What**: 전투 시스템의 Controller와 Service를 역할에 따라 분리하여 Clean Architecture 원칙을 준수하고, 코드 재사용성을 향상시킵니다.

**Why**:
- 현재 `DungeonController`는 실제로 메인 스테이지를 담당하지만 명칭이 혼란스러움
- `BattleController`는 전투 시뮬레이션 + 로그 조회를 모두 담당하여 SRP 위반
- 전투 로직이 BattleService에 강결합되어 PVP, 보스 던전 등에서 재사용 불가

**Scope**:
- ✅ In Scope:
  - DungeonController → StageController 리네이밍
  - BattleController → SpecialDungeonController + BattleLogController 분리
  - BattleService → CombatService + BattleLogService 분리
  - 관련 DTO, Interface 업데이트
  - Unity 문서 API 엔드포인트 업데이트
  - 테스트 코드 수정
- ❌ Out of Scope:
  - 비즈니스 로직 변경 (기능은 동일하게 유지)
  - 새로운 특수 던전 컨텐츠 추가 (미래 작업)
  - PVP 시스템 구현 (Phase 2)

---

## 2. User Stories & Acceptance Criteria

### User Story (개발자 관점)
**As a** 백엔드 개발자,
**I want** Controller와 Service의 책임이 명확히 분리되고,
**So that** 새로운 전투 컨텐츠(PVP, 보스 레이드)를 추가할 때 전투 로직을 재사용할 수 있다.

### Acceptance Criteria (EARS 형식)
- **AC-1**: WHEN 메인 스테이지 클리어 요청 시 THEN system SHALL `/api/stages/clear` 엔드포인트로 처리한다
- **AC-2**: WHEN 전투 로그 조회 요청 시 THEN system SHALL `/api/battle-logs` 엔드포인트로 처리한다
- **AC-3**: WHEN 보스 던전 도전 요청 시 THEN system SHALL `/api/dungeons/boss/challenge` 엔드포인트로 처리한다 (미래 확장)
- **AC-4**: WHEN StageService, SpecialDungeonService가 전투 시뮬레이션 필요 시 THEN system SHALL CombatService를 주입받아 사용한다
- **AC-5**: WHEN 기존 클라이언트 요청 시 THEN system SHALL 기존 API 호환성을 유지한다 (Breaking Change 없음)

---

## 3. API Design

### 3-1. Stage API (구 Dungeon API)

#### GET /api/stages
**변경 전**: `GET /api/dungeons/stages`
**변경 후**: `GET /api/stages`
**Authorization**: Anonymous (Public Read)

**Query Parameters**:
```
characterId: Guid (required)
difficulty: DungeonDifficulty (optional)
```

**Response (200)**:
```json
[
  {
    "id": 1,
    "name": "숲의 입구",
    "difficulty": "Normal",
    "requiredLevel": 1,
    "finalGoldReward": 100,
    "finalExpReward": 50,
    "isAvailable": true
  }
]
```

---

#### GET /api/stages/progress
**변경 전**: `GET /api/dungeons/progress`
**변경 후**: `GET /api/stages/progress`
**Authorization**: Anonymous

**Query Parameters**:
```
characterId: Guid (required)
```

**Response (200)**:
```json
{
  "characterId": "guid",
  "normalHighestStage": 5,
  "hardHighestStage": 2,
  "hellHighestStage": 0
}
```

---

#### POST /api/stages/clear
**변경 전**: `POST /api/dungeons/clear`
**변경 후**: `POST /api/stages/clear`
**Authorization**: Required (JWT)

**Request**:
```json
{
  "characterId": "guid",
  "stageId": 5,
  "difficulty": "Normal"
}
```

**Response (200)**:
```json
{
  "isSuccess": true,
  "reward": {
    "gold": 150,
    "experience": 75
  },
  "newHighestStage": 5,
  "isLevelUp": true,
  "currentLevel": 6,
  "droppedEquipments": [],
  "battleStatistics": {
    "totalTurns": 8,
    "totalDamageDealt": 450,
    "totalDamageTaken": 120,
    "criticalHitCount": 2,
    "evasionCount": 1
  }
}
```

**Error Responses**:
- **400 Bad Request**: 레벨 부족, 이전 스테이지 미클리어
- **401 Unauthorized**: JWT 없음/만료
- **403 Forbidden**: 다른 플레이어의 캐릭터
- **404 Not Found**: 캐릭터 미존재

---

### 3-2. Battle Log API (신규 분리)

#### GET /api/battle-logs
**변경 전**: `GET /api/battle/logs`
**변경 후**: `GET /api/battle-logs`
**Authorization**: Required

**Query Parameters**:
```
characterId: Guid (required)
page: int (default: 1)
pageSize: int (default: 20)
```

**Response (200)**:
```json
{
  "logs": [
    {
      "id": "guid",
      "monsterName": "슬라임",
      "monsterLevel": 5,
      "isVictory": true,
      "experienceGained": 50,
      "goldGained": 100,
      "damageDealt": 450,
      "damageTaken": 120,
      "battleDate": "2025-10-28T10:30:00Z"
    }
  ],
  "currentPage": 1,
  "pageSize": 20,
  "totalCount": 150,
  "totalPages": 8
}
```

---

#### GET /api/battle-logs/recent
**변경 전**: `GET /api/battle/logs/recent`
**변경 후**: `GET /api/battle-logs/recent`
**Authorization**: Required

**Query Parameters**:
```
characterId: Guid (required)
count: int (default: 10)
```

**Response (200)**:
```json
[
  {
    "id": "guid",
    "monsterName": "슬라임",
    "isVictory": true,
    "battleDate": "2025-10-28T10:30:00Z"
  }
]
```

---

#### GET /api/battle-logs/stats
**변경 전**: `GET /api/battle/stats`
**변경 후**: `GET /api/battle-logs/stats`
**Authorization**: Required

**Query Parameters**:
```
characterId: Guid (required)
```

**Response (200)**:
```json
{
  "totalBattles": 150,
  "victories": 120,
  "defeats": 30,
  "winRate": 80.0,
  "totalDamageDealt": 67500,
  "totalDamageTaken": 18000
}
```

---

### 3-3. Special Dungeon API (미래 확장용 Placeholder)

#### POST /api/dungeons/boss/challenge
**Authorization**: Required

**Request**:
```json
{
  "characterId": "guid",
  "bossId": "guid"
}
```

**Response (200)**:
```json
{
  "isVictory": true,
  "reward": {
    "gold": 1000,
    "experience": 500
  },
  "battleStatistics": { ... }
}
```

> 💡 **미래 작업**: 보스 던전 시스템 구현 시 SpecialDungeonController에 추가

---

## 4. Data Model

> ⚠️ **데이터 모델 변경 없음** - 이 리팩토링은 코드 구조만 변경하며, DB 스키마는 변경하지 않습니다.

### 영향받는 기존 Entities
- `DungeonStage` (변경 없음) - 스테이지 마스터 데이터
- `CharacterDungeonProgress` (변경 없음) - 스테이지 진행도
- `BattleLog` (변경 없음) - 전투 로그
- `Character` (변경 없음)
- `Monster` (변경 없음)

### Migration 요구사항
- **없음**: DB 스키마 변경 없이 코드 구조만 리팩토링

---

## 5. Business Logic

### 5-1. CombatService (순수 전투 시뮬레이션)

**책임**: 전투 계산만 수행 (보상 지급, DB 저장 제외)

**Core Logic**:
```
1. Load Character stats (Attack, Defense, HP, CritRate, etc.)
2. Load Monster stats
3. Simulate turn-based combat:
   - Calculate damage (with Crit, Evasion)
   - Update HP
   - Check win/lose condition
4. Return CombatResultDto (승패, 통계)
```

**입력**: `characterId`, `monsterId`
**출력**: `CombatResultDto` (승패, 턴수, 데미지, 크리티컬 횟수)

**특징**:
- 보상 계산 없음 (호출자가 책임)
- DB 저장 없음 (호출자가 트랜잭션 관리)
- 재사용 가능 (Stage, SpecialDungeon, PVP에서 공통 사용)

---

### 5-2. BattleLogService (전투 로그 CRUD)

**책임**: 전투 로그 저장 및 조회

**Methods**:
```
- SaveBattleLogAsync(BattleLog log): 로그 저장
- GetLogsAsync(characterId, page, pageSize): 페이징 조회
- GetRecentLogsAsync(characterId, count): 최근 N개 조회
- GetStatsAsync(characterId): 통계 계산
```

**특징**:
- 전투 로직 없음 (순수 CRUD)
- 통계 계산은 DB 쿼리로 처리 (LINQ Aggregation)

---

### 5-3. StageService (구 DungeonService)

**책임**: 메인 스테이지 진행도 관리 + 보상 지급

**ClearStageAsync Logic**:
```
1. Validate stage unlock (레벨, 이전 스테이지 클리어)
2. [Optional] Call CombatService.SimulateCombatAsync()
3. Calculate rewards (Gold, Exp) with difficulty multiplier
4. Transaction:
   - Update Character (Gold, Exp, Level)
   - Update CharacterDungeonProgress
   - Save BattleLog (via BattleLogService)
5. Return StageClearResultDto
```

**의존성**:
- `ICombatService` (전투 시뮬레이션 필요 시)
- `IBattleLogService` (로그 저장)
- `ICharacterRepository`, `IStageProgressRepository`

---

### 5-4. SpecialDungeonService (신규, 미래 확장용)

**책임**: 보스 던전, 일일 던전 등 특수 컨텐츠 처리

**ChallengeBossAsync Logic** (Placeholder):
```
1. Validate boss unlock
2. Call CombatService.SimulateCombatAsync()
3. Calculate special rewards
4. Transaction:
   - Update Character
   - Save BattleLog
5. Return BossChallengeResultDto
```

> 💡 **현재는 Interface만 정의**, 구현은 Phase 3에서 진행

---

### Business Rules (기존 유지)

- **BR-1**: 스테이지는 순차적으로 클리어해야 함 (Stage N 클리어 전에 N+1 도전 불가)
- **BR-2**: 캐릭터 레벨이 `RequiredLevel` 이상이어야 스테이지 도전 가능
- **BR-3**: 난이도별 보상 배수: Normal (1.0x), Hard (1.5x), Hell (2.0x)
- **BR-4**: 전투 로그는 최근 1000개까지 보관 (자동 삭제 로직은 미구현)

---

### 🎓 학습 포인트 (아키텍처 결정)

**TODO(human)**: 다음 아키텍처 결정이 필요합니다. 리팩토링 과정에서 고민해보세요:

#### 1. CombatService의 트랜잭션 경계
- [ ] **질문**: CombatService는 트랜잭션 내부에서 호출되는가? 외부인가?
- **옵션 A**: CombatService는 stateless (트랜잭션 밖)
  - 장점: 재사용성 최대, 테스트 용이
  - 단점: 호출자가 트랜잭션 관리 책임
- **옵션 B**: CombatService가 트랜잭션 포함
  - 장점: 일관성 보장
  - 단점: 재사용 시 중첩 트랜잭션 문제

**가이드**: Clean Architecture에서는 **Domain/Application Service는 stateless**가 원칙입니다. 트랜잭션은 호출자(StageService, SpecialDungeonService)가 관리하는 것이 일반적입니다.

---

#### 2. BattleLog 저장 책임
- [ ] **질문**: BattleLog는 누가 저장하는가?
- **옵션 A**: CombatService가 BattleLog 생성만 (저장은 호출자)
  - 장점: SRP 준수
  - 단점: 호출자가 로그 저장 잊을 수 있음
- **옵션 B**: BattleLogService에 위임
  - 장점: 중앙 관리
  - 단점: 모든 호출자가 BattleLogService 의존성 추가

**가이드**: **옵션 B (BattleLogService)** 추천. 로그는 횡단 관심사(Cross-Cutting Concern)이므로 별도 Service로 분리하는 것이 유지보수에 유리합니다.

---

#### 3. Controller 네이밍 규칙
- [ ] **질문**: RESTful 관점에서 Controller 이름은 리소스 중심인가? 동작 중심인가?
- **현재**:
  - ✅ `StageController` → `/api/stages` (리소스 중심)
  - ✅ `BattleLogController` → `/api/battle-logs` (리소스 중심)
  - ❓ `SpecialDungeonController` → `/api/dungeons` (리소스? 행위?)

**가이드**: RESTful API에서는 **명사형 리소스**가 원칙입니다. "Battle"은 동사적 성격이 강하므로 `BattleController`는 부적절했습니다. `SpecialDungeonController`는 "던전"이라는 리소스를 다루므로 적절합니다.

---

#### 4. Service Interface 위치
- [ ] **질문**: ICombatService는 어느 Layer에 위치하는가?
- **옵션 A**: Application Layer (IStageService와 함께)
  - 장점: 일반적 패턴
  - 단점: Domain 순수성 약화
- **옵션 B**: Domain Layer (Domain Service)
  - 장점: 순수 비즈니스 로직
  - 단점: Infrastructure 의존성 필요 시 문제

**가이드**: CombatService는 **순수 계산 로직**(Character/Monster Entity만 사용)이므로 **Domain Service**로 배치 가능합니다. 하지만 Repository 의존성이 필요하다면 **Application Service**가 적절합니다. 현재는 Repository 의존하므로 **Application Layer** 추천.

---

> 💡 **학습 가이드**: 이 4가지 질문은 **Clean Architecture의 핵심**입니다. 각 레이어의 책임, 의존성 방향, 트랜잭션 경계를 이해하는 것이 이번 리팩토링의 학습 목표입니다.

---

## 6. Constraints & Risks

### Constraints
- **C-1**: 기존 Unity 클라이언트는 API 엔드포인트 변경으로 영향받음 → Unity 문서 업데이트 필수
- **C-2**: 테스트 코드가 Controller/Service 이름에 강결합되어 있음 → 대량 수정 필요
- **C-3**: BattleController의 기존 엔드포인트는 Breaking Change 발생 → 버전 관리 또는 Redirect 필요

### Known Risks

#### Risk 1: Breaking Changes
**설명**: API 엔드포인트 변경으로 기존 Unity 클라이언트가 동작 불가
**Mitigation**:
- **Option A**: 구 엔드포인트 유지 + Redirect (301 Moved Permanently)
- **Option B**: 버전 관리 (`/api/v1/dungeons` → `/api/v2/stages`)
- **Option C** (권장): 개발 단계이므로 Breaking Change 허용, Unity 클라이언트 동시 수정

#### Risk 2: 테스트 코드 대량 수정
**설명**: Controller/Service 이름 변경으로 50+ 테스트 파일 수정 필요
**Mitigation**:
- Refactoring 전 모든 테스트가 통과하는지 확인
- 단계별 커밋 (Controller 리네이밍 → Service 분리 → 테스트 수정)
- 각 단계마다 테스트 실행하여 회귀 방지

#### Risk 3: Service 의존성 순환 참조
**설명**: StageService → CombatService → BattleLogService → StageService (순환 가능성)
**Mitigation**:
- CombatService는 다른 Service 의존하지 않음 (Repository만 사용)
- BattleLogService는 독립적 CRUD만 수행
- StageService만 CombatService + BattleLogService 의존

---

## 7. Test Strategy

### Unit Tests

#### CombatService Tests
- [ ] `SimulateCombatAsync_CharacterWins_ReturnsVictory`
- [ ] `SimulateCombatAsync_CharacterLoses_ReturnsDefeat`
- [ ] `SimulateCombatAsync_CriticalHit_IncreasesCount`
- [ ] `SimulateCombatAsync_Evasion_ReducesDamage`

#### BattleLogService Tests
- [ ] `SaveBattleLogAsync_ValidLog_SavesSuccessfully`
- [ ] `GetLogsAsync_ValidPaging_ReturnsCorrectPage`
- [ ] `GetStatsAsync_ValidCharacter_ReturnsAggregatedStats`

#### StageService Tests
- [ ] `ClearStageAsync_ValidStage_UpdatesProgress`
- [ ] `ClearStageAsync_InsufficientLevel_ThrowsException`
- [ ] `ClearStageAsync_PreviousStageNotCleared_ThrowsException`

---

### Integration Tests

#### StageController Tests
- [ ] `POST /api/stages/clear → 200 OK`
- [ ] `POST /api/stages/clear → 403 Forbidden (다른 플레이어)`
- [ ] `GET /api/stages → 200 OK (스테이지 목록)`

#### BattleLogController Tests
- [ ] `GET /api/battle-logs → 200 OK (페이징)`
- [ ] `GET /api/battle-logs/recent → 200 OK`
- [ ] `GET /api/battle-logs/stats → 200 OK`

---

### Refactoring Validation Tests
- [ ] 모든 기존 테스트가 리팩토링 후에도 통과하는가?
- [ ] API 응답 형식이 변경되지 않았는가? (DTO 호환성)
- [ ] 성능 저하가 없는가? (전투 시뮬레이션 속도 측정)

---

## 8. Implementation Checklist

### Phase 1: Service Layer 분리
- [ ] **Step 1-1**: `ICombatService` 인터페이스 생성 (`Application/Combat/Services/`)
- [ ] **Step 1-2**: `CombatService` 구현 (기존 `BattleService.SimulateBattleAsync` 로직 이동)
- [ ] **Step 1-3**: `IBattleLogService` 인터페이스 생성
- [ ] **Step 1-4**: `BattleLogService` 구현 (기존 `BattleService` 로그 로직 이동)
- [ ] **Step 1-5**: Unit Tests 작성 (CombatService, BattleLogService)

### Phase 2: Controller 리네이밍
- [ ] **Step 2-1**: `DungeonController.cs` → `StageController.cs` 파일 리네이밍
- [ ] **Step 2-2**: 클래스명, Route 속성 변경 (`[Route("api/stages")]`)
- [ ] **Step 2-3**: `IDungeonService` → `IStageService` 리네이밍
- [ ] **Step 2-4**: `DungeonService` → `StageService` 리네이밍
- [ ] **Step 2-5**: DI 등록 업데이트 (`Program.cs`)

### Phase 3: BattleController 분리
- [ ] **Step 3-1**: `BattleLogController.cs` 생성 (`[Route("api/battle-logs")]`)
- [ ] **Step 3-2**: 기존 `BattleController`의 로그 조회 메서드 이동
- [ ] **Step 3-3**: `SpecialDungeonController.cs` Placeholder 생성 (빈 Controller)
- [ ] **Step 3-4**: 기존 `BattleController` 삭제 (또는 Deprecated 주석)

### Phase 4: Service 의존성 주입 업데이트
- [ ] **Step 4-1**: `StageService`에 `ICombatService`, `IBattleLogService` 주입
- [ ] **Step 4-2**: `StageService.ClearStageAsync()` 로직 수정 (CombatService 호출)
- [ ] **Step 4-3**: `SpecialDungeonService` Interface 정의 (빈 메서드)
- [ ] **Step 4-4**: DI 등록 (`Program.cs`)

### Phase 5: DTO 및 응답 구조 확인
- [ ] **Step 5-1**: 기존 DTO 호환성 확인 (Breaking Change 없는지)
- [ ] **Step 5-2**: Swagger 주석 업데이트
- [ ] **Step 5-3**: API 응답 샘플 문서 업데이트

### Phase 6: 테스트 수정
- [ ] **Step 6-1**: Controller 테스트 파일 리네이밍
- [ ] **Step 6-2**: Service 테스트 Mock 업데이트
- [ ] **Step 6-3**: Integration 테스트 엔드포인트 URL 변경
- [ ] **Step 6-4**: 모든 테스트 통과 확인

### Phase 7: Unity 문서 업데이트
- [ ] **Step 7-1**: `docs/unity/API_SPEC_FOR_UNITY.md` 업데이트
  - `/api/dungeons/` → `/api/stages/`
  - `/api/battle/` → `/api/battle-logs/`
- [ ] **Step 7-2**: Unity DTO 클래스 파일 업데이트 (네임스페이스 변경 필요 시)
- [ ] **Step 7-3**: Breaking Changes 문서 작성 (CHANGELOG.md)

### Phase 8: 🎓 TODO(human) 구현
- [ ] **Step 8-1**: CombatService 트랜잭션 경계 결정 및 구현
- [ ] **Step 8-2**: BattleLog 저장 책임 확인 및 테스트
- [ ] **Step 8-3**: Controller 네이밍 규칙 자기 검토
- [ ] **Step 8-4**: Service Layer 위치 결정 검증

### Phase 9: Self-Review
- [ ] 모든 테스트 통과
- [ ] Swagger UI 정상 작동
- [ ] API 호환성 확인 (기존 Unity 클라이언트 영향 최소화)
- [ ] 코드 리뷰 체크리스트 10개 항목 통과

---

## 9. Unity Documentation Plan

### 업데이트 필요 문서
- [ ] `docs/unity/API_SPEC_FOR_UNITY.md`
  - Section: "Battle API" → "Battle Log API"로 제목 변경
  - Section: "Dungeon API" → "Stage API"로 제목 변경
  - 엔드포인트 URL 전체 업데이트
  - Breaking Changes 섹션 추가

### Breaking Changes 안내
```markdown
## ⚠️ Breaking Changes (v1.7 → v1.8)

### Stage API (구 Dungeon API)
- `GET /api/dungeons/stages` → `GET /api/stages`
- `GET /api/dungeons/progress` → `GET /api/stages/progress`
- `POST /api/dungeons/clear` → `POST /api/stages/clear`

### Battle Log API (구 Battle API)
- `GET /api/battle/logs` → `GET /api/battle-logs`
- `GET /api/battle/logs/recent` → `GET /api/battle-logs/recent`
- `GET /api/battle/stats` → `GET /api/battle-logs/stats`

### Response 형식
- **변경 없음**: DTO 구조는 동일하게 유지

### Migration Guide
1. Unity 프로젝트에서 API 베이스 URL 변수 업데이트
2. `ApiEndpoints.cs` 파일의 상수 변경
3. 기존 코드에서 `dungeons` → `stages` 전역 검색/치환
```

---

## 10. Decisions

### Decision 1: Controller 리네이밍 시 기존 엔드포인트 처리
- **Context**: 기존 Unity 클라이언트가 `/api/dungeons/clear`를 사용 중
- **Decision**: Breaking Change 허용 (개발 단계)
- **Rationale**:
  - 현재는 학습 프로젝트이므로 완벽한 하위 호환성보다 올바른 아키텍처 우선
  - Redirect 로직 추가는 코드 복잡도만 증가시킴
  - Unity 클라이언트도 동시에 수정하여 일관성 유지

### Decision 2: BattleService 완전 삭제 vs Deprecated
- **Context**: 기존 BattleService의 역할을 CombatService + BattleLogService로 분리
- **Decision**: BattleService 완전 삭제
- **Rationale**:
  - Deprecated로 남기면 혼란 증가
  - 모든 호출 코드가 이미 리팩토링되므로 삭제 가능
  - Git History로 추적 가능

### Decision 3: SpecialDungeonController 지금 생성 vs 나중에 생성
- **Context**: 보스 던전 시스템은 Phase 3에서 구현 예정
- **Decision**: 지금 Placeholder만 생성
- **Rationale**:
  - 아키텍처 설계 명확성 (전투 컨텐츠 구조 가시화)
  - 미래 작업 시 추가 리팩토링 불필요
  - Placeholder는 빈 Controller + Interface만 (구현 없음)

### Decision 4: CombatService는 Application Layer
- **Context**: ICombatService 인터페이스의 위치 (Domain vs Application)
- **Decision**: Application Layer (`IdleRPG.Application/Combat/Services/`)
- **Rationale**:
  - Repository 의존성 필요 (Character, Monster 조회)
  - Domain Layer는 외부 의존성 없어야 함 (Clean Architecture)
  - 전투 계산 로직은 Application Service로 충분 (복잡한 Domain 규칙 없음)

---

## 11. Approval

### 작성자 체크리스트
- [ ] Intent & Scope 명확한가?
- [ ] TODO(human) 아키텍처 학습 포인트 포함되었는가?
- [ ] API 엔드포인트 변경사항 문서화되었는가?
- [ ] Breaking Changes 영향도 분석되었는가?
- [ ] 테스트 전략이 충분한가?
- [ ] Implementation Checklist 실행 가능한가?

### 승인 전 확인사항
- [ ] `/spec-review combat-system-refactoring` 실행 (목표: 70점 이상)
- [ ] TODO(human) 4개 항목 이해 완료
- [ ] Unity 문서 업데이트 계획 검토

---

**Next Steps**:
1. 이 문서를 검토하고 TODO(human) 항목에 대해 생각해보기
2. `/spec-review combat-system-refactoring` 실행하여 품질 점수 확인
3. 70점 이상 획득 시 "Approval" 섹션에 서명
4. Implementation Checklist를 따라 단계별 구현 시작

---

**Estimated Effort**: 6-10시간 (Service 분리 3h + Controller 리팩토링 2h + 테스트 수정 3h + 문서 2h)
