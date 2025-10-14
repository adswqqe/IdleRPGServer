# Week 2 Feature 3: Battle Controller & API 완료

## 완료 일시
- 2025-10-14

## 구현 내용

### 1. 생성된 파일

#### StartBattleRequest.cs
- **경로**: `IdleRPG.Application/DTOs/Battle/StartBattleRequest.cs`
- **목적**: 전투 시작 요청 DTO
- **필드**:
  - `CharacterId` (Guid, Required)
  - `MonsterId` (Guid, Required)

#### BattleController.cs
- **경로**: `IdleRPG.API/Controllers/BattleController.cs`
- **엔드포인트**: `POST /api/battle/start`
- **인증**: `[Authorize]` 필요
- **의존성**:
  - IBattleService (전투 시뮬레이션)
  - ICharacterService (경험치 지급, 캐릭터 조회)
  - ICharacterRepository (골드 지급)

### 2. 수정된 파일

#### BattleResultResponse.cs
- **변경 사항**: 불필요한 필드 제거
  - ~~CharacterStats~~ 제거 (클라이언트가 이미 알고 있음)
  - ~~MonsterStats~~ 제거 (클라이언트가 이미 알고 있음)
- **추가된 필드**:
  - `UpdatedCharacter` (CharacterDto?, nullable) - 전투 후 업데이트된 캐릭터 정보

#### CharacterDto.cs
- **추가된 필드**:
  - `Gold` (long) - Feature 4에서 추가된 필드 반영
  - `LastLoginTime` (DateTime) - Feature 4에서 추가된 필드 반영

#### CharacterService.cs (CreateCharacterDto 메서드)
- **추가된 매핑**:
  - `Gold = character.Gold`
  - `LastLoginTime = character.LastLoginTime`

#### BattleService.cs (SimulateCombat 메서드)
- **변경 사항**: 응답 구조 변경
  - CharacterStats, MonsterStats 제거
  - UpdatedCharacter = null (Controller에서 설정)

## API 설계

### POST /api/battle/start

**Request Body**:
```json
{
  "characterId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "monsterId": "550e8400-e29b-41d4-a716-446655440000"
}
```

**Response (200 OK)**:
```json
{
  "response": {
    "isVictory": true,
    "reward": {
      "experience": 250,
      "gold": 50
    },
    "statistics": {
      "totalTurns": 15,
      "totalDamageDealt": 1500,
      "totalDamageTaken": 300,
      "criticalHitCount": 3,
      "evasionCount": 0
    },
    "updatedCharacter": {
      "id": "3fa85f64...",
      "level": 6,
      "experience": 20,
      "gold": 350,
      "stats": { /* ... */ }
    }
  }
}
```

**Error Responses**:
- `400 Bad Request`: 유효성 검증 실패, 몬스터 없음
- `403 Forbidden`: 다른 플레이어의 캐릭터 사용 시도
- `404 Not Found`: 캐릭터 없음

## 전투 흐름

1. **소유권 검증**: 
   - CharacterService로 캐릭터 조회
   - GetCurrentUserId()로 소유권 확인
   - 다른 플레이어 캐릭터면 403 Forbidden 반환

2. **전투 시뮬레이션**:
   - BattleService.SimulateBattleAsync() 호출
   - Priority Queue 기반 Event-driven 전투 실행

3. **보상 자동 지급** (승리 시):
   - **경험치**: CharacterService.AddExperienceAsync() 호출
     - 자동 레벨업 처리 포함
   - **골드**: CharacterRepository로 직접 업데이트
     - character.Gold += reward.Gold
     - SaveChangesAsync()
   - **최종 캐릭터 조회**: 레벨업 반영된 최신 정보 가져오기

4. **응답 반환**:
   - battleResult.UpdatedCharacter에 최신 캐릭터 설정
   - Ok(new { response = battleResult }) 반환

## 설계 결정 사항

### ✅ 채택된 설계

1. **응답 구조 최소화**:
   - CharacterStats, MonsterStats 제거
   - 이유: 클라이언트가 이미 알고 있는 정보, 불필요한 데이터 전송

2. **UpdatedCharacter 포함**:
   - 레벨업 여부를 즉시 알 수 있음
   - 추가 API 호출 불필요 (경험치 획득 + 캐릭터 조회 통합)

3. **골드 직접 업데이트**:
   - CharacterService에 AddGoldAsync 메서드 추가하지 않음
   - Controller에서 Repository 직접 사용
   - 이유: 간단한 구현, 나중에 리팩토링 가능

4. **소유권 검증**:
   - 403 Forbidden 처리 추가
   - CharacterController와 일관된 보안 패턴

### 사용 시나리오

- **보스 스테이지**: 서버 검증 필요
- **랭킹 던전**: 서버 검증 필요
- **PVP 전투**: 서버 검증 필요
- **오프라인 보상**: 서버 계산 필요
- ~~일반 스테이지~~: 클라이언트에서 처리 (서버 호출 없음)

## 빌드 결과

- **Status**: ✅ 성공
- **Errors**: 0
- **Warnings**: 22 (기존 nullable 경고, 비critical)

## 남은 작업

- [ ] Unity API_SPEC_FOR_UNITY.md 업데이트 (v1.3)
- [ ] Unity-DTOs.cs에 Battle DTO 추가
- [ ] Swagger로 API 테스트
- [ ] (Optional) CharacterService에 AddGoldAsync 메서드 추가하여 리팩토링

## 기술적 인사이트

**API 응답 설계 원칙**:
- 클라이언트가 이미 알고 있는 정보는 전송하지 않기
- Idle RPG 특성상 전투 중 HP는 의미 없음 (결과만 중요)
- 레벨업 정보는 즉시 필요하므로 포함
- Battle Log 기능 구현 시 상세 정보는 DB에 저장

**보상 지급 전략**:
- 경험치: CharacterService (자동 레벨업 로직 재사용)
- 골드: Repository 직접 (간단한 덧셈 연산)
- 최종 조회: 레벨업 + 골드 지급 반영된 최신 데이터

**클린 아키텍처 준수**:
- Controller → Application Service → Domain → Infrastructure
- Repository는 Infrastructure 계층이지만 필요 시 Controller에서 DI 가능
- 비즈니스 로직(레벨업)은 CharacterService에 집중
