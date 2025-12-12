# Dungeon System Application Layer 구현 완료 (Week 3, Day 2 - 2025-10-18)

## 📋 구현 완료 항목

### 1. Application Layer - DTOs

**생성된 DTO (4개)**:

1. **DungeonStageDto** (`IdleRPG.Application/DTOs/Dungeon/DungeonStageDto.cs`)
   - 스테이지 정보 + 난이도별 보상 계산 결과
   - Properties:
     - Id, Name, Description
     - Difficulty (DungeonDifficulty enum)
     - RequiredLevel
     - BossMonsterId, BossMonsterName
     - BaseGoldReward, BaseExpReward
     - FinalGoldReward, FinalExpReward (난이도 배수 적용 후)
     - IsAvailable (도전 가능 여부)

2. **CharacterDungeonProgressDto** (`IdleRPG.Application/DTOs/Dungeon/CharacterDungeonProgressDto.cs`)
   - 난이도별 최고 클리어 스테이지
   - Properties:
     - Id, CharacterId
     - HighestStageClearedNormal
     - HighestStageClearedHard
     - HighestStageClearedHell (Entity는 Nightmare, DTO는 Hell 매핑)
     - UpdatedAt

3. **DungeonClearRequestDto** (`IdleRPG.Application/DTOs/Dungeon/DungeonClearRequestDto.cs`)
   - 최소한의 클라이언트 요청 (서버 중심 설계)
   - Properties:
     - StageId (int)
     - Difficulty (DungeonDifficulty enum)

4. **DungeonClearResultDto** (`IdleRPG.Application/DTOs/Dungeon/DungeonClearResultDto.cs`)
   - 서버 검증 + 보상 결과
   - Properties:
     - IsSuccess (bool)
     - Reward (RewardDto: Gold, Experience)
     - NewHighestStage (int)
     - ErrorMessage (string?, 실패 시)
     - IsLevelUp (bool)
     - CurrentLevel (int)

### 2. Application Layer - Service Interface

**IDungeonService** (`IdleRPG.Application/Interfaces/IDungeonService.cs`)

```csharp
Task<List<DungeonStageDto>> GetAvailableStagesAsync(Guid characterId, DungeonDifficulty? difficulty = null);
Task<CharacterDungeonProgressDto> GetProgressAsync(Guid characterId);
Task<DungeonClearResultDto> ClearStageAsync(Guid characterId, DungeonClearRequestDto request);
```

### 3. Infrastructure Layer - Service Implementation

**DungeonService** (`IdleRPG.Infrastructure/Service/DungeonService.cs`)

#### 핵심 기능

1. **GetAvailableStagesAsync**
   - 캐릭터 레벨 + 진행도 기반 필터링
   - 난이도별 DifficultyMultiplier 적용
   - IsAvailable 플래그 계산 (레벨 + 이전 스테이지 클리어 여부)

2. **GetProgressAsync**
   - 진행도 조회 (없으면 자동 생성)
   - Nightmare → Hell 매핑

3. **ClearStageAsync** (사용자 구현 + 코드 리뷰 개선)
   - **동시성 제어**: SemaphoreSlim 기반 캐릭터별 잠금
   - **서버 검증**: 레벨, 스테이지 존재, 이전 클리어 여부
   - **트랜잭션 처리**: 진행도 + 보상 + 레벨업 원자성
   - **DTO 기반 에러**: IsSuccess + ErrorMessage 패턴
   - **헬퍼 메서드 활용**: GetHighestStageCleared, SetHighestStageCleared

### 4. DI 등록

**Program.cs** (`IdleRPG.API/Program.cs:68`)
```csharp
builder.Services.AddScoped<IDungeonService, DungeonService>();
```

---

## 🔧 코드 리뷰 개선 (Gemini 2.5 Pro)

### Critical Issues 수정

1. **Line 192 Copy-Paste Bug**
   - 문제: Nightmare 난이도에서 Hard 진행도를 참조
   - 수정: `HighestStageClearedHard` → `HighestStageClearedNightmare`

2. **Race Condition 방지**
   - 추가: `ConcurrentDictionary<Guid, SemaphoreSlim>` 캐릭터별 잠금
   - 패턴: `WaitAsync()` + `try-finally Release()`

3. **Transaction Management**
   - 변경: `AddExperienceWithResultAsync` → `ProcessExperienceGain`
   - 이유: SaveChanges를 DungeonService에서 한 번만 호출 (원자성)

### Important Issues 수정

4. **Error Handling**
   - 변경: Exception throw → DTO 기반 에러 반환
   - 장점: 클라이언트 친화적, 명확한 에러 메시지

5. **Switch Statement 중복 제거**
   - 추가: CharacterDungeonProgress 헬퍼 메서드
     - `GetHighestStageCleared(DungeonDifficulty)`
     - `SetHighestStageCleared(DungeonDifficulty, int)`
   - 효과: 3개 Switch 문 → 헬퍼 메서드 호출로 간소화

### 추가 개선

6. **Nullable 경고 제거**
   - 제거: `GetOrCreateByCharacterIdAsync` null 체크
   - 이유: 반환 타입이 non-nullable이므로 항상 값 반환

---

## 📁 생성/수정된 파일 목록

### 생성된 파일 (6개)

**Application Layer (5)**:
1. IdleRPG.Application/DTOs/Dungeon/DungeonStageDto.cs
2. IdleRPG.Application/DTOs/Dungeon/CharacterDungeonProgressDto.cs
3. IdleRPG.Application/DTOs/Dungeon/DungeonClearRequestDto.cs
4. IdleRPG.Application/DTOs/Dungeon/DungeonClearResultDto.cs
5. IdleRPG.Application/Interfaces/IDungeonService.cs

**Infrastructure Layer (1)**:
6. IdleRPG.Infrastructure/Service/DungeonService.cs

### 수정된 파일 (4개)

1. **IdleRPG.Domain/Entities/CharacterDungeonProgress.cs**
   - 헬퍼 메서드 추가: GetHighestStageCleared, SetHighestStageCleared
   - 네임스페이스 충돌 해결: `Enums.DungeonDifficulty` 명시

2. **IdleRPG.Application/Character/Services/ICharacterService.cs**
   - 메서드 추가: `AddExperienceWithResultAsync`
   - 메서드 노출: `ProcessExperienceGain`

3. **IdleRPG.Infrastructure/Service/CharacterService.cs**
   - 구현: `AddExperienceWithResultAsync`
   - 수정: `ProcessExperienceGain` 반환값 (IsLevelUp, LevelUps)

4. **IdleRPG.API/Program.cs**
   - DI 등록: IDungeonService → DungeonService

---

## 🎯 설계 패턴 및 베스트 프랙티스

### 1. Server-Authoritative Design
- 클라이언트는 최소 정보만 전송 (StageId, Difficulty)
- 모든 검증/계산은 서버에서 수행
- 치트 방지: 보상 계산, 진행도 검증

### 2. Concurrency Control (캐릭터별 잠금)
```csharp
private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> _characterLocks = new();

var lockObj = _characterLocks.GetOrAdd(characterId, _ => new SemaphoreSlim(1, 1));
await lockObj.WaitAsync();
try {
    // Critical section
} finally {
    lockObj.Release();
}
```

### 3. Transaction Atomicity
- 진행도 업데이트 + Gold 지급 + 경험치/레벨업을 **하나의 트랜잭션**으로
- ProcessExperienceGain (SaveChanges 없음) + DungeonService에서 SaveChanges 1회

### 4. DTO-based Error Handling
```csharp
// Bad: throw new Exception("레벨 부족")
// Good:
return new DungeonClearResultDto
{
    IsSuccess = false,
    ErrorMessage = "레벨이 부족합니다 (필요 레벨: 5)"
};
```

### 5. Helper Methods for DRY
- Switch 중복 제거
- Domain Entity에 비즈니스 로직 캡슐화
- UpdatedAt 자동 갱신

---

## 🔜 다음 작업 (API Layer)

### 1. Controller 구현

**DungeonController** (`IdleRPG.API/Controllers/DungeonController.cs`)

필요한 엔드포인트:
```csharp
[HttpGet("stages")]
Task<IActionResult> GetAvailableStages([FromQuery] DungeonDifficulty? difficulty = null);

[HttpGet("progress")]
Task<IActionResult> GetMyProgress();

[HttpPost("clear")]
Task<IActionResult> ClearStage([FromBody] DungeonClearRequestDto request);
```

### 2. Seed Data 생성

**초기 던전 스테이지 데이터**:
- 10~20개 스테이지
- RequiredLevel: 1, 5, 10, 15, 20, ...
- 기존 Monster 연동 (MonsterId)
- BaseGold, BaseExperience 균형 조정

**Seed 방법**:
- Seeder 클래스 생성 (Infrastructure/Data/Seeders/DungeonStageSeeder.cs)
- Program.cs에서 초기화 시 실행

### 3. Unity 문서 업데이트

**업데이트 파일**:
- `../IdleRPGClient/Docs/unity/API_SPEC_FOR_UNITY.md`
  - GET /api/dungeons/stages
  - GET /api/dungeons/progress
  - POST /api/dungeons/clear

- `../IdleRPGClient/Docs/Unity-DTOs.cs`
  - DungeonStageDto
  - CharacterDungeonProgressDto
  - DungeonClearRequestDto
  - DungeonClearResultDto

- `../IdleRPGClient/Docs/unity/Unity-Quick-Reference.md`
  - 던전 시스템 API 요약 테이블 추가

---

## 💡 학습 포인트

### Concurrency Control
- **Global Lock**: 전체 서버 성능 저하
- **Character-Level Lock**: 캐릭터별 독립적 처리 (최적)
- **SemaphoreSlim**: async/await 호환 경량 잠금

### Transaction Management
- **Anti-Pattern**: 서브 서비스에서 SaveChanges 호출
- **Best Practice**: 최상위 서비스에서 1회 SaveChanges
- **Atomicity**: 부분 성공 방지 (진행도만 업데이트, 보상 실패 등)

### DTO vs Exception
- **Exception**: 로그 스택, HTTP 500, 클라이언트 파싱 어려움
- **DTO**: 구조화된 에러, HTTP 200, IsSuccess 체크

### DRY (Don't Repeat Yourself)
- Switch 중복 → Helper Method
- Domain Entity에 로직 캡슐화
- Maintenance 비용 감소

---

## ✅ 체크리스트

- [x] DTOs 생성
- [x] IDungeonService 인터페이스 정의
- [x] DungeonService 구현
- [x] 코드 리뷰 (Gemini) + 5개 이슈 수정
- [x] CharacterService 확장 (AddExperienceWithResultAsync)
- [x] DI 등록
- [x] 빌드 검증 (0 errors, 1 warning)
- [ ] Controller 구현
- [ ] Seed Data 생성
- [ ] Unity 문서 업데이트
- [ ] 통합 테스트 (Postman/Swagger)

---

## 📌 중요 노트

1. **코드 품질**: Gemini 코드 리뷰로 6/10 → 9/10 수준으로 향상
2. **빌드 상태**: 경고 28개 → 1개 (27개 제거)
3. **다음 세션**: Controller + Seed Data + Unity 문서 업데이트
