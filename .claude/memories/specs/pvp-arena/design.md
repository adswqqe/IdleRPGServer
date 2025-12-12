# Design: PVP Arena

> 이 문서는 PVP Arena 기능의 기술 설계를 정의합니다.
>
> **Requirements 추적성**: [requirements.md](./requirements.md)의 US-1 ~ US-4 기반

---

## 📐 Architecture Overview

### Layer Responsibilities

#### API Layer
- **Controllers**: `PvpController` 신규 생성
- **Endpoints**:
  - `POST /api/pvp/matches` (매칭 시작 및 전투)
  - `GET /api/pvp/rankings` (랭킹 조회)
  - `GET /api/pvp/matches/history` (전적 조회)
  - `GET /api/pvp/seasons/current` (현재 시즌 정보)
  - `POST /api/pvp/seasons/{seasonId}/rewards` (시즌 보상 수령)
- **DTOs**: `PvpMatchRequestDto`, `PvpMatchResponseDto`, `PvpRankingDto`, `PvpMatchHistoryDto`

#### Application Layer
- **Services**:
  - `IPvpService` (신규): PVP 전체 흐름 관리 (매칭 → 전투 → 레이팅 업데이트 → 보상)
  - `IPvpMatchmakingService` (신규): 매칭 로직 (±200 레이팅 범위, 30초 타임아웃)
  - `IPvpSeasonService` (신규): 시즌 관리 (시즌 종료, 보상 지급, Soft Reset)
- **Business Logic**:
  - 매칭 알고리즘: ±200 레이팅 범위 내 후보 조회 → 랜덤 선택
  - 레이팅 계산: `EloRatingService` 호출 (Domain Service)
  - 보상 계산: 승패별 보상 (Gold, Crystal, 경험치)
- **Validation**:
  - CharacterId 소유권 검증 (JWT userId와 일치)
  - 시즌 활성 여부 검증
  - 보상 수령 중복 방지

#### Domain Layer
- **Entities**:
  - `PvpMatch` (신규): 매치 기록
  - `PvpRanking` (신규): 시즌별 랭킹 정보
  - `PvpSeason` (신규): 시즌 마스터 데이터
- **Value Objects**: 없음
- **Domain Services**:
  - `EloRatingService` (신규): ELO 레이팅 계산 (순수 비즈니스 로직)
  - **메서드**: `(int, int) CalculateNewRatings(int winnerRating, int loserRating, int kFactor = 32)`
- **Enums**:
  - `PvpTier`: Bronze, Silver, Gold, Platinum, Diamond
  - `PvpMatchResult`: Victory, Defeat

#### Infrastructure Layer
- **Repositories**:
  - `IPvpMatchRepository` (신규): PvpMatch CRUD, 전적 조회 (페이징)
  - `IPvpRankingRepository` (신규): PvpRanking CRUD, 랭킹 조회 (Top 100, 내 주변)
  - `IPvpSeasonRepository` (신규): PvpSeason CRUD, 현재 시즌 조회
- **External Services**:
  - `IRedisCacheService` (신규 인터페이스): Redis Sorted Set 캐싱
  - **구현**: `RedisCacheService` (Infrastructure)
  - **메서드**:
    - `Task UpdateRankingCacheAsync(int seasonId, Guid characterId, int rating)`
    - `Task<Dictionary<Guid, int>> GetTopRankingsAsync(int seasonId, int count)`
    - `Task<int?> GetMyRankAsync(int seasonId, Guid characterId)`
    - `Task<Dictionary<Guid, int>> GetRankingsAroundMeAsync(int seasonId, Guid characterId, int range)`
- **Data Access**: EF Core Configuration (복합키, 인덱스)

---

## 🗄️ Data Model

> ⚠️ **개념적 명세만 작성** - SQL DDL, C# 코드는 Implementation 단계에서 작성

### 신규 테이블: `PvpMatch`

**목적**: 플레이어 간 PVP 매치 기록 저장 (전적 조회, 통계, 리플레이 기반 데이터)

**요구사항**: [US-1] PVP 매칭 및 전투, [US-3] 전적 조회

**필드**:
- `Id` (PK): uuid (Entity, 유저 생성 데이터)
- `SeasonId`: int (FK → PvpSeason, Not Null)
- `AttackerId`: uuid (FK → Character, Not Null, 공격자)
- `DefenderId`: uuid (FK → Character, Not Null, 방어자)
- `WinnerId`: uuid (FK → Character, Not Null, 승자)
- `AttackerRatingBefore`: int (매치 전 공격자 레이팅)
- `AttackerRatingAfter`: int (매치 후 공격자 레이팅)
- `DefenderRatingBefore`: int (매치 전 방어자 레이팅)
- `DefenderRatingAfter`: int (매치 후 방어자 레이팅)
- `CreatedAt`: timestamp (매치 발생 시각)

**제약사항**:
- `AttackerId != DefenderId` (자기 자신과 매칭 불가)
- `WinnerId IN (AttackerId, DefenderId)` (승자는 둘 중 하나)
- 모든 Rating 필드: 0 이상 정수

**비즈니스 규칙 - 무승부 정책**:
- ✅ **무승부 불가**: 현재 전투 시스템(CombatService)이 무승부를 지원하지 않으므로, 항상 승자가 존재합니다.
- `WinnerId`는 `Not Null` (현재 설계 유지)
- 향후 무승부 도입 시: `WinnerId` Nullable 변경 + 레이팅 변화 없음 로직 추가

**인덱스 요구사항**:
- `IX_PvpMatch_AttackerId_CreatedAt`: AttackerId 기준 전적 조회 (최신순)
- `IX_PvpMatch_DefenderId_CreatedAt`: DefenderId 기준 전적 조회 (최신순)
- `IX_PvpMatch_SeasonId`: 시즌별 필터링

**관계**:
- `PvpMatch` → `Character` (AttackerId, DefenderId, WinnerId): N:1
- `PvpMatch` → `PvpSeason` (SeasonId): N:1

---

### 신규 테이블: `PvpRanking`

**목적**: 시즌별 플레이어 PVP 랭킹 정보 저장 (PostgreSQL + Redis 이중 저장)

**요구사항**: [US-2] 랭킹 조회, [US-4] 시즌 종료 및 보상

**필드**:
- `SeasonId` (PK 일부): int (FK → PvpSeason, Not Null)
- `CharacterId` (PK 일부): uuid (FK → Character, Not Null)
- `Rating`: int (현재 레이팅, Default 1000)
- `Wins`: int (승리 횟수, Default 0)
- `Losses`: int (패배 횟수, Default 0)
- `WinStreak`: int (연승 횟수, Default 0)
- `Tier`: enum (Bronze, Silver, Gold, Platinum, Diamond)
- `IsRewardClaimed`: bool (시즌 보상 수령 여부, Default false)
- `LastMatchAt`: timestamp (마지막 매치 시각, Nullable)
- `UpdatedAt`: timestamp (업데이트 시각)

**PRIMARY KEY**: (SeasonId, CharacterId) 복합키

**제약사항**:
- `Rating >= 0` (최소 레이팅 0)
- `Wins >= 0`, `Losses >= 0`, `WinStreak >= 0`
- `Tier` 자동 계산: Rating 기반 (Bronze 0-999, Silver 1000-1499, Gold 1500-1999, Platinum 2000-2499, Diamond 2500+)

**인덱스 요구사항**:
- `IX_PvpRanking_SeasonId_Rating_DESC`: 현재 시즌 Top 100 조회 최적화 (복합 인덱스)
- **쿼리 패턴**: `SELECT * FROM PvpRanking WHERE SeasonId = ? ORDER BY Rating DESC LIMIT 100`
- **성능**: O(log N + 100), 100만 건에서도 수 ms 내 조회

**관계**:
- `PvpRanking` → `Character` (CharacterId): N:1
- `PvpRanking` → `PvpSeason` (SeasonId): N:1

**🎓 학습 포인트 (데이터베이스 설계)**:
- **TODO(human)**: 복합키 vs Surrogate Key (Id + Unique Constraint)?
  - **복합키 장점**: 자연키, 중복 데이터 방지
  - **Surrogate Key 장점**: EF Core 관계 설정 단순화
  - **권장**: 복합키 (프로젝트 표준 준수, SeasonId + CharacterId가 자연스러운 식별자)

---

### 신규 테이블: `PvpSeason`

**목적**: PVP 시즌 마스터 데이터 관리 (시즌 기간, 활성 여부)

**요구사항**: [US-4] 시즌 종료 및 보상

**필드**:
- `Id` (PK): int (Template, 마스터 데이터, Auto-increment)
- `SeasonNumber`: int (시즌 번호, 1부터 시작, Unique, Not Null)
- `StartDate`: timestamp (시즌 시작 시각, Not Null)
- `EndDate`: timestamp (시즌 종료 시각, Not Null)
- `IsActive`: bool (현재 활성 시즌 여부, Default false)
- `CreatedAt`: timestamp
- `UpdatedAt`: timestamp

**제약사항**:
- `SeasonNumber` Unique (중복 불가)
- `StartDate < EndDate` (시작일이 종료일보다 앞)
- 활성 시즌은 최대 1개 (`SELECT COUNT(*) FROM PvpSeason WHERE IsActive = true` ≤ 1)

**인덱스 요구사항**:
- `IX_PvpSeason_IsActive`: 현재 시즌 조회 (`SELECT * FROM PvpSeason WHERE IsActive = true`)
- `IX_PvpSeason_SeasonNumber`: 시즌 번호 검색

**관계**:
- `PvpSeason` → `PvpRanking` (1:N)
- `PvpSeason` → `PvpMatch` (1:N)

---

### Entity Relationships

**ERD 개요**:
```
Player (1) ──< (N) Character
                ↓ (1:N)
            PvpRanking (PK: SeasonId, CharacterId)
                ↓ (N:1)
            PvpSeason (1) ──< (N) PvpMatch
                ↑ (N:1)        ↓ (N:1)
            Character ←───────┘
```

**관계 설명**:
- `Character` → `PvpRanking`: 1:N (한 캐릭터가 여러 시즌 랭킹 보유)
- `PvpSeason` → `PvpRanking`: 1:N (한 시즌이 여러 캐릭터 랭킹 포함)
- `PvpSeason` → `PvpMatch`: 1:N (한 시즌이 여러 매치 포함)
- `Character` → `PvpMatch` (Attacker/Defender/Winner): 1:N (한 캐릭터가 여러 매치 참여)

**Cascade 규칙**:
- `PvpRanking.CharacterId` → Character 삭제 시: Cascade Delete (캐릭터 삭제 시 랭킹도 삭제)
- `PvpMatch.AttackerId/DefenderId/WinnerId` → Character 삭제 시: Restrict (매치 기록 보존)
- `PvpRanking.SeasonId` → PvpSeason 삭제 시: Restrict (시즌 기록 보존)

**🎓 학습 포인트 (데이터베이스 설계)**:
- **TODO(human)**: Cascade Delete vs Restrict 전략?
  - **Cascade**: 캐릭터 삭제 시 관련 데이터 자동 삭제 (단순, GDPR 준수)
  - **Restrict**: 데이터 무결성 보존, 통계 유지 (히스토리 보존)
  - **권장**: PvpRanking은 Cascade (랭킹은 캐릭터에 종속), PvpMatch는 Restrict (전적 기록은 보존)

---

### EF Core Configuration 요구사항

**Fluent API 필요 항목**:

#### `PvpRanking` Configuration
- **복합키 설정**: `HasKey(pr => new { pr.SeasonId, pr.CharacterId })`
- **Enum 변환**: `Property(pr => pr.Tier).HasConversion<string>()`
- **외래 키 관계**:
  - `PvpSeason` → `PvpRanking` (1:N):
    - `HasOne(PvpSeason).WithMany().HasForeignKey(pr => pr.SeasonId).OnDelete(DeleteBehavior.Restrict)`
    - **동작**: 시즌 삭제 시 Restrict (시즌 기록 보존, 히스토리 유지)
    - **이유**: 시즌 삭제는 관리자 실수일 가능성이 높음, 랭킹 데이터는 통계/분석에 필요
  - `Character` → `PvpRanking` (1:N):
    - `HasOne(Character).WithMany().HasForeignKey(pr => pr.CharacterId).OnDelete(DeleteBehavior.Cascade)`
    - **동작**: 캐릭터 삭제 시 Cascade Delete (랭킹 자동 삭제)
    - **이유**: 랭킹은 캐릭터에 종속, GDPR 준수 (개인 데이터 삭제)

#### `PvpMatch` Configuration
- **외래 키 관계**:
  - `Character` → `PvpMatch` (AttackerId, 1:N):
    - `HasOne(Character).WithMany().HasForeignKey(pm => pm.AttackerId).OnDelete(DeleteBehavior.Restrict)`
    - **동작**: 공격자 캐릭터 삭제 시 Restrict (매치 기록 보존)
    - **이유**: 전적 기록은 히스토리 데이터, 통계 및 분석에 필요
  - `Character` → `PvpMatch` (DefenderId, 1:N):
    - `HasOne(Character).WithMany().HasForeignKey(pm => pm.DefenderId).OnDelete(DeleteBehavior.Restrict)`
    - **동작**: 방어자 캐릭터 삭제 시 Restrict (매치 기록 보존)
    - **이유**: 전적 기록은 히스토리 데이터, 통계 및 분석에 필요
  - `Character` → `PvpMatch` (WinnerId, 1:N):
    - `HasOne(Character).WithMany().HasForeignKey(pm => pm.WinnerId).OnDelete(DeleteBehavior.Restrict)`
    - **동작**: 승자 캐릭터 삭제 시 Restrict (매치 기록 보존)
    - **이유**: 승패 기록은 통계 데이터, 삭제 시 데이터 무결성 훼손
  - `PvpSeason` → `PvpMatch` (1:N):
    - `HasOne(PvpSeason).WithMany().HasForeignKey(pm => pm.SeasonId).OnDelete(DeleteBehavior.Restrict)`
    - **동작**: 시즌 삭제 시 Restrict (매치 기록 보존)
    - **이유**: 시즌 종료 후 매치 기록은 아카이빙 대상

#### `PvpSeason` Configuration
- **인덱스**: `HasIndex(ps => ps.IsActive)`
- **Unique 인덱스**: `HasIndex(ps => ps.SeasonNumber).IsUnique()`

**FK 제약 조건 요약**:
| Entity | FK Column | Referenced Table | OnDelete Behavior | 이유 |
|--------|-----------|------------------|-------------------|------|
| PvpRanking | SeasonId | PvpSeason | Restrict | 시즌 기록 보존 (통계 유지) |
| PvpRanking | CharacterId | Character | Cascade | 캐릭터 종속 데이터 (GDPR) |
| PvpMatch | AttackerId | Character | Restrict | 전적 히스토리 보존 |
| PvpMatch | DefenderId | Character | Restrict | 전적 히스토리 보존 |
| PvpMatch | WinnerId | Character | Restrict | 승패 통계 보존 |
| PvpMatch | SeasonId | PvpSeason | Restrict | 시즌별 매치 아카이빙 |

> 💡 **구현 참고**: 구체적인 Fluent API 코드는 `IdleRPG.Infrastructure/Configurations/` 에서 작성
>
> ⚠️ **주의**: Restrict 제약이 있는 FK는 삭제 시 예외 발생. 캐릭터 삭제 API는 PvpMatch 존재 여부 확인 필요.

---

## 🔌 API Design

> ⚠️ **Requirements 추적성**:
> - US-1: Endpoint 1
> - US-2: Endpoint 2
> - US-3: Endpoint 3
> - US-4: Endpoint 4, 5

### Endpoint 1: 매칭 시작 및 전투 [US-1]

#### `POST /api/pvp/matches`

**목적**: 플레이어 매칭 → 전투 시뮬레이션 → 레이팅 업데이트를 원자적으로 처리 (서버 권위, 치팅 방지)

**Authorization**: Bearer Token 필수 (JWT)

**Request:**
```json
{
  "characterId": "uuid"
}
```

**Validation Rules** (FluentValidation):
```csharp
RuleFor(x => x.CharacterId)
  .NotEmpty().WithMessage("CharacterId is required")
  .Must(BeValidGuid).WithMessage("CharacterId must be a valid UUID");
```

**Response (201 Created):**
```json
{
  "matchId": "uuid",
  "opponent": {
    "characterId": "uuid",
    "name": "적캐릭터",
    "rating": 1500,
    "isBot": false
  },
  "result": "Victory",
  "myRatingBefore": 1450,
  "myRatingAfter": 1475,
  "opponentRatingChange": -23,
  "rewards": {
    "gold": 500,
    "crystal": 10,
    "experience": 200
  },
  "combatLog": {
    "isVictory": true,
    "totalDamageDealt": 1500,
    "totalDamageTaken": 800,
    "totalTurns": 12
  }
}
```

**Errors:**
- `400 Bad Request`: "Character not found" (CharacterId 존재하지 않음)
- `400 Bad Request`: "Character does not belong to you" (CharacterId 소유권 없음)
- `401 Unauthorized`: "Invalid or missing token" (JWT 인증 실패)
- `404 Not Found`: "No active season" (활성 시즌 없음)

**Business Logic**:
1. CharacterId 소유권 검증 (JWT userId와 Character.OwnerId 일치)
2. 현재 활성 시즌 조회 (`PvpSeason WHERE IsActive = true`)
3. 매칭 실행 (`PvpMatchmakingService`)
   - ±200 레이팅 범위 내 후보 조회
   - 랜덤 선택
   - 30초 타임아웃 시 NPC 봇 매칭
4. 전투 시뮬레이션 (`CombatService.SimulateCombatAsync` 재사용)
5. 레이팅 계산 (`EloRatingService.CalculateNewRatings`)
6. **트랜잭션**:
   - PvpMatch 생성
   - PvpRanking 업데이트 (Rating, Wins/Losses, WinStreak)
   - Character 보상 지급 (Gold, Crystal, Experience)
   - Redis 랭킹 갱신 (Write-Through)
7. 응답 반환

---

### Endpoint 2: 랭킹 조회 [US-2]

#### `GET /api/pvp/rankings`

**목적**: 현재 시즌 랭킹 조회 (Top 100, 내 주변, 티어별 필터링)

**Query Parameters**:
- `seasonId` (optional, int): 시즌 ID (기본값: 현재 활성 시즌)
- `top` (optional, int): Top N 랭킹 조회 (예: `top=100`, 기본값 100)
- `nearMe` (optional, bool): 내 주변 ±10등 조회 (`nearMe=true`)
- `range` (optional, int): nearMe 시 범위 (기본값 10)
- `tier` (optional, enum): 티어 필터링 (Bronze, Silver, Gold, Platinum, Diamond)
- `page` (optional, int): 페이지 번호 (tier 필터링 시 사용, 기본값 1)
- `pageSize` (optional, int): 페이지 크기 (기본값 50)

**Validation Rules** (FluentValidation):
```csharp
RuleFor(x => x.SeasonId)
  .GreaterThan(0).When(x => x.SeasonId.HasValue)
  .WithMessage("SeasonId must be a positive integer");

RuleFor(x => x.Top)
  .InclusiveBetween(1, 1000).When(x => x.Top.HasValue)
  .WithMessage("Top must be between 1 and 1000");

RuleFor(x => x.Range)
  .InclusiveBetween(1, 50).When(x => x.Range.HasValue)
  .WithMessage("Range must be between 1 and 50");

RuleFor(x => x.Page)
  .GreaterThan(0).When(x => x.Page.HasValue)
  .WithMessage("Page must be a positive integer");

RuleFor(x => x.PageSize)
  .InclusiveBetween(1, 100).When(x => x.PageSize.HasValue)
  .WithMessage("PageSize must be between 1 and 100");

RuleFor(x => x.Tier)
  .IsInEnum().When(x => x.Tier.HasValue)
  .WithMessage("Tier must be one of: Bronze, Silver, Gold, Platinum, Diamond");
```

**Authorization**: Bearer Token 필수 (내 주변 조회 시)

**Response (200 OK):**
```json
{
  "seasonId": 10,
  "seasonNumber": 10,
  "rankings": [
    {
      "rank": 1,
      "characterId": "uuid",
      "characterName": "최강자",
      "rating": 2500,
      "wins": 150,
      "losses": 30,
      "winRate": 83.33,
      "tier": "Diamond"
    },
    {
      "rank": 2,
      "characterId": "uuid",
      "characterName": "2등",
      "rating": 2450,
      "wins": 140,
      "losses": 35,
      "winRate": 80.0,
      "tier": "Diamond"
    }
  ],
  "totalCount": 1000,
  "myRank": 42,
  "myRating": 1800
}
```

**Errors:**
- `404 Not Found`: "Season not found" (SeasonId 존재하지 않음)
- `401 Unauthorized`: "Invalid or missing token" (nearMe 조회 시 인증 필요)

**Business Logic**:
1. SeasonId 검증 (없으면 현재 활성 시즌)
2. **Top N 조회** (`top` 파라미터):
   - **Step 1**: Redis Sorted Set 조회 시도
     ```
     try {
       result = await _redisCacheService.GetTopRankingsAsync(seasonId, top);
       if (result != null && result.Count > 0) {
         return 200 OK with Redis data;
       }
     } catch (RedisException ex) {
       _logger.LogWarning("Redis 조회 실패, PostgreSQL Fallback: {Error}", ex.Message);
       // Step 2로 진행
     }
     ```
   - **Step 2**: PostgreSQL Fallback
     ```
     result = await _pvpRankingRepository.GetTopRankingsAsync(seasonId, top);
     if (result == null || result.Count == 0) {
       return 404 Not Found: "No rankings found for season";
     }
     return 200 OK with PostgreSQL data;
     ```
   - **에러 처리**:
     - Redis 장애: Warning 로그 + PostgreSQL 조회 (성능 저하 허용)
     - PostgreSQL 장애: 500 Internal Server Error
     - 데이터 없음: 404 Not Found

3. **내 주변 조회** (`nearMe=true`):
   - **Step 1**: Redis에서 내 순위 조회 시도
     ```
     try {
       myRank = await _redisCacheService.GetMyRankAsync(seasonId, characterId);
       if (myRank.HasValue) {
         result = await _redisCacheService.GetRankingsAroundMeAsync(seasonId, characterId, range);
         return 200 OK with Redis data;
       }
     } catch (RedisException ex) {
       _logger.LogWarning("Redis 조회 실패, PostgreSQL Fallback: {Error}", ex.Message);
       // Step 2로 진행
     }
     ```
   - **Step 2**: PostgreSQL Fallback
     ```
     myRanking = await _pvpRankingRepository.GetByCharacterIdAsync(seasonId, characterId);
     if (myRanking == null) {
       return 404 Not Found: "No ranking data for character";
     }
     // PostgreSQL에서 OFFSET/LIMIT로 내 주변 조회
     result = await _pvpRankingRepository.GetRankingsAroundAsync(seasonId, myRanking.Rating, range);
     return 200 OK with PostgreSQL data;
     ```
   - **에러 처리**:
     - Redis 장애: Warning 로그 + PostgreSQL 조회
     - 캐릭터 랭킹 없음: 404 Not Found

4. **티어별 조회** (`tier` 파라미터):
   - PostgreSQL 직접 조회 (`SELECT * FROM PvpRanking WHERE SeasonId = ? AND Tier = ? ORDER BY Rating DESC LIMIT ? OFFSET ?`)
   - Redis 사용 안 함 (복잡한 필터링은 PostgreSQL이 효율적)

5. Character 이름 조회 (별도 쿼리로 N+1 방지)
6. 응답 반환

**🎓 학습 포인트 (성능 최적화)**:
- **TODO(human)**: Redis vs PostgreSQL 전략?
  - **Redis 장점**: O(log N) 조회, 100ms 이하, 실시간 랭킹
  - **PostgreSQL 장점**: 복잡한 필터링 (티어별, 페이징)
  - **권장**: Top N은 Redis 우선, 티어별 필터링은 PostgreSQL

---

### Endpoint 3: 전적 조회 [US-3]

#### `GET /api/pvp/matches/history`

**목적**: 특정 캐릭터의 PVP 전적 조회 (최근 20경기, 페이징)

**Query Parameters**:
- `characterId` (required, uuid): 조회할 캐릭터 ID
- `seasonId` (optional, int): 시즌 필터링 (기본값: 전체)
- `page` (optional, int): 페이지 번호 (기본값 1)
- `pageSize` (optional, int): 페이지 크기 (기본값 20, 최대 50)

**Validation Rules** (FluentValidation):
```csharp
RuleFor(x => x.CharacterId)
  .NotEmpty().WithMessage("CharacterId is required")
  .Must(BeValidGuid).WithMessage("CharacterId must be a valid UUID");

RuleFor(x => x.SeasonId)
  .GreaterThan(0).When(x => x.SeasonId.HasValue)
  .WithMessage("SeasonId must be a positive integer");

RuleFor(x => x.Page)
  .GreaterThan(0).When(x => x.Page.HasValue)
  .WithMessage("Page must be a positive integer");

RuleFor(x => x.PageSize)
  .InclusiveBetween(1, 50).When(x => x.PageSize.HasValue)
  .WithMessage("PageSize must be between 1 and 50");
```

**Authorization**: Bearer Token 필수

**Response (200 OK):**
```json
{
  "matches": [
    {
      "matchId": "uuid",
      "seasonNumber": 10,
      "opponentCharacterId": "uuid",
      "opponentName": "상대방",
      "result": "Victory",
      "myRatingChange": +25,
      "opponentRatingChange": -23,
      "createdAt": "2025-11-06T10:30:00Z"
    },
    {
      "matchId": "uuid",
      "seasonNumber": 10,
      "opponentCharacterId": "uuid",
      "opponentName": "다른상대",
      "result": "Defeat",
      "myRatingChange": -18,
      "opponentRatingChange": +20,
      "createdAt": "2025-11-06T09:15:00Z"
    }
  ],
  "totalCount": 150,
  "currentPage": 1,
  "totalPages": 8
}
```

**Errors:**
- `400 Bad Request`: "Character not found"
- `401 Unauthorized`: "Invalid or missing token"

**Business Logic**:
1. CharacterId 검증
2. PvpMatch 조회:
   - `SELECT * FROM PvpMatch WHERE (AttackerId = ? OR DefenderId = ?) AND (SeasonId = ? OR ? IS NULL) ORDER BY CreatedAt DESC LIMIT ? OFFSET ?`
   - 인덱스 활용: `IX_PvpMatch_AttackerId_CreatedAt`, `IX_PvpMatch_DefenderId_CreatedAt`
   - PostgreSQL Bitmap Index Scan 자동 병합
3. 상대방 정보 조회 (Character 이름)
4. 결과 매핑 (Victory/Defeat 판정)
5. 응답 반환

**🎓 학습 포인트 (쿼리 최적화)**:
- **TODO(human)**: OR 조건 vs UNION ALL?
  - **OR**: 단순한 쿼리, Bitmap Index Scan 자동 병합
  - **UNION ALL**: 명시적 인덱스 사용, 더 예측 가능한 성능
  - **권장**: OR 조건 사용 (PostgreSQL 옵티마이저 신뢰, 쿼리 단순화)

---

### Endpoint 4: 현재 시즌 정보 조회 [US-4]

#### `GET /api/pvp/seasons/current`

**목적**: 현재 활성 시즌 정보 조회 (시즌 번호, 시작/종료일, 남은 기간)

**Authorization**: 없음 (Public)

**Response (200 OK):**
```json
{
  "seasonId": 10,
  "seasonNumber": 10,
  "startDate": "2025-11-01T00:00:00Z",
  "endDate": "2026-02-01T00:00:00Z",
  "daysRemaining": 87,
  "isActive": true
}
```

**Errors:**
- `404 Not Found`: "No active season" (활성 시즌 없음)

**Business Logic**:
1. 현재 활성 시즌 조회 (`SELECT * FROM PvpSeason WHERE IsActive = true`)
2. 남은 기간 계산 (`(EndDate - DateTime.UtcNow).Days`)
3. 응답 반환

---

### Endpoint 5: 시즌 보상 수령 [US-4]

#### `POST /api/pvp/seasons/{seasonId}/rewards`

**목적**: 시즌 종료 후 최종 티어에 따른 보상 수령

**Authorization**: Bearer Token 필수

**Request**: Body 없음 (CharacterId는 JWT에서 추출)

**Response (200 OK):**
```json
{
  "seasonNumber": 10,
  "tier": "Gold",
  "finalRating": 1850,
  "finalRank": 123,
  "rewards": {
    "crystal": 500,
    "legendaryEquipmentBox": 1
  },
  "alreadyClaimed": false
}
```

**Errors:**
- `400 Bad Request`: "Season is still active" (시즌이 아직 진행 중)
- `400 Bad Request`: "Reward already claimed" (이미 수령함)
- `401 Unauthorized`: "Invalid or missing token"
- `404 Not Found`: "Season not found"
- `404 Not Found`: "No ranking data for this season" (해당 시즌 참여 기록 없음)

**Business Logic**:
1. SeasonId 검증 및 시즌 종료 확인 (`IsActive = false`)
2. CharacterId 추출 (JWT userId → Character 조회)
3. PvpRanking 조회 (`SeasonId, CharacterId`)
4. 중복 수령 방지 (`IsRewardClaimed = false`)
5. 티어별 보상 계산:
   - Bronze: Crystal 100
   - Silver: Crystal 300
   - Gold: Crystal 500 + 전설 장비 상자 1개
   - Platinum: Crystal 1000 + 전설 장비 상자 3개
   - Diamond: Crystal 2000 + 신화 장비 상자 1개 (+ 칭호, 추후 확장)
6. **트랜잭션**:
   - Character 보상 지급 (Crystal, 장비 상자)
   - PvpRanking 업데이트 (`IsRewardClaimed = true`)
7. 응답 반환

---

## 🧮 Business Logic

> ⚠️ **학습 프로젝트**: 게임 밸런스 (확률, 보상)는 AI가 제안합니다. 아키텍처에 집중하세요.

### 핵심 알고리즘 1: ELO 레이팅 계산

#### `EloRatingService.CalculateNewRatings` (Domain Service)

**입력:**
- `winnerRating` (int): 승자의 현재 레이팅
- `loserRating` (int): 패자의 현재 레이팅
- `kFactor` (int, optional): K-Factor (기본값 32)

**출력:**
- `(int winnerNewRating, int loserNewRating)`: 승자와 패자의 새 레이팅

**프로세스 (ELO 표준 알고리즘)**:
1. 승자의 기대 승률 계산:
   ```
   expectedWinner = 1 / (1 + 10^((loserRating - winnerRating) / 400))
   ```
2. 패자의 기대 승률 계산:
   ```
   expectedLoser = 1 / (1 + 10^((winnerRating - loserRating) / 400))
   ```
3. 레이팅 변화 계산:
   ```
   winnerChange = K * (1 - expectedWinner)  // 실제 결과(1) - 기대 승률
   loserChange = K * (0 - expectedLoser)    // 실제 결과(0) - 기대 승률
   ```
4. 새 레이팅 계산:
   ```
   winnerNewRating = winnerRating + winnerChange
   loserNewRating = loserRating + loserChange
   ```
5. 최소값 보장 (0 이상):
   ```
   winnerNewRating = Math.Max(0, winnerNewRating)
   loserNewRating = Math.Max(0, loserNewRating)
   ```

**예외 처리:**
- K-Factor는 항상 양수 (검증 불필요, 상수 사용)
- 레이팅은 음수 불가 (0으로 클램핑)

**🎓 학습 포인트 (아키텍처 결정)**:
- **TODO(human)**: ELO 계산 로직은 Domain Service vs Application Service?
  - **Domain Service**: 순수 비즈니스 로직, 외부 의존성 없음, 재사용 가능, 단위 테스트 용이
  - **Application Service**: 유스케이스 조율, Repository/Infrastructure 의존
  - **권장**: Domain Service (`IdleRPG.Domain/Services/EloRatingService.cs`)
    - 이유: 외부 의존성 없음, 다른 경쟁 시스템 (길드 랭킹, 리더보드)에서 재사용 가능
- **TODO(human)**: K-Factor 관리 전략?
  - **하드코딩**: 상수 32 (단순, 대부분의 ELO 시스템 표준)
  - **Configuration**: appsettings.json (유연, 운영 중 조정 가능)
  - **Database**: Configuration 테이블 (런타임 변경 가능)
  - **권장**: 하드코딩 → Configuration (MVP는 하드코딩, Phase 2에서 Configuration 테이블 도입)

> 💡 **학습 가이드**: ELO 계산식은 표준 알고리즘이므로 AI가 구현합니다. **계층 분리 (Domain vs Application)**와 **K-Factor 관리 전략**에 집중하세요.

---

### 핵심 알고리즘 2: 매칭 알고리즘

#### `PvpMatchmakingService.FindOpponentAsync` (Application Service)

**입력:**
- `characterId` (Guid): 매칭 요청 캐릭터
- `seasonId` (int): 현재 시즌

**출력:**
- `MatchOpponentDto`: 매칭된 상대 (CharacterId, Name, Rating, IsBot)

**프로세스 (AI 제안)**:
1. 내 레이팅 조회 (`PvpRanking WHERE SeasonId = ? AND CharacterId = ?`)
2. ±200 레이팅 범위 내 후보 조회:
   ```sql
   SELECT CharacterId, Rating FROM PvpRanking
   WHERE SeasonId = ?
     AND CharacterId != ?  -- 자기 자신 제외
     AND Rating BETWEEN (myRating - 200) AND (myRating + 200)
   LIMIT 100  -- 최대 100명
   ```
3. 랜덤 선택:
   - 후보가 있으면: `Random.Next(0, candidates.Count)`
   - 후보가 없으면 (30초 타임아웃 가정): NPC 봇 생성
4. **NPC 봇 생성** (후보 없을 시):
   - 레이팅: 내 레이팅 ± Random(-100, 100)
   - 전투력: 내 전투력 * 0.8 (80%)
   - 이름: "Bot_" + Random(1000, 9999)
   - IsBot: true
5. 상대방 정보 반환

**예외 처리:**
- 내 레이팅이 없으면 (첫 매칭): 초기 레이팅 1000으로 PvpRanking 생성
- 후보 조회 실패 시: NPC 봇 매칭 (타임아웃 대응)

**🎓 학습 포인트 (아키텍처 결정)**:
- **TODO(human)**: 매칭 로직은 Application Service vs Domain Service?
  - **Application Service**: Repository 의존, 외부 데이터 조회
  - **Domain Service**: 순수 비즈니스 로직만
  - **권장**: Application Service (`IdleRPG.Application/Services/PvpMatchmakingService.cs`)
    - 이유: Repository 의존 (PvpRankingRepository), Infrastructure 조회 필요
- **TODO(human)**: 타임아웃 처리 전략?
  - **동기 대기**: 30초 대기 후 NPC 매칭 (단순, HTTP Timeout 고려 필요)
  - **비동기 큐**: Redis Queue로 매칭 요청 저장, 백그라운드 처리 (복잡, 확장성 좋음)
  - **권장**: 동기 대기 (MVP는 단순화, Phase 3에서 비동기 큐 도입)

> 💡 **학습 가이드**: 매칭 알고리즘 (레이팅 범위, 랜덤 선택)은 AI가 구현합니다. **Application vs Domain 계층 선택**과 **타임아웃 처리 전략**에 집중하세요.

---

### 핵심 알고리즘 3: 티어 산정

#### `PvpRanking.Tier` 자동 계산 (Domain Entity Property)

**입력:**
- `Rating` (int): 현재 레이팅

**출력:**
- `PvpTier` (enum): Bronze, Silver, Gold, Platinum, Diamond

**프로세스 (AI 제안)**:
```csharp
// Domain Entity 계산 속성
public PvpTier Tier
{
    get
    {
        return Rating switch
        {
            >= 2500 => PvpTier.Diamond,
            >= 2000 => PvpTier.Platinum,
            >= 1500 => PvpTier.Gold,
            >= 1000 => PvpTier.Silver,
            _       => PvpTier.Bronze
        };
    }
}
```

**🎓 학습 포인트 (데이터 모델링)**:
- **결정**: Tier는 **PostgreSQL GENERATED COLUMN**으로 구현 (Stored)
  - **이유**: DB 레벨에서 데이터 정합성 보장, Application 로직 실수 방지
  - **장점**:
    - 쿼리 필터링 최적화 (`WHERE Tier = 'Gold'`)
    - Rating 변경 시 자동 갱신 (Application 코드 불필요)
    - EF Core 모델과 DB 일관성 자동 유지
  - **구현**: Migration에 GENERATED COLUMN 추가 (아래 Migration Plan 참조)
  - **EF Core 매핑**: `Property(pr => pr.Tier).HasConversion<string>().ValueGeneratedOnAddOrUpdate()`

> 💡 **학습 가이드**: 티어 구간 (Bronze 0-999, Silver 1000-1499 등)은 AI가 제안합니다. **Computed vs Stored Column 전략**에 집중하세요.

---

## 🎯 Service Layer Design

> ⚠️ **메서드 시그니처와 책임만 정의** - 구현 코드는 Implementation 단계에서 작성

### `IPvpService` (신규)

**책임**: PVP 전체 흐름 관리 (매칭 → 전투 → 레이팅 업데이트 → 보상 지급)

**메서드:**

#### `StartMatchAsync(Guid characterId, Guid userId, CancellationToken cancellationToken)`

**시그니처**:
- 입력: `Guid characterId`, `Guid userId` (JWT에서 추출), `CancellationToken cancellationToken`
- 반환: `Task<PvpMatchResponseDto>` (매치 결과, 레이팅 변화, 보상)

**프로세스 흐름**:
1. CharacterId 소유권 검증 (userId와 Character.OwnerId 일치)
   - 실패 시: `throw new UnauthorizedAccessException("Character does not belong to you")`
2. 현재 활성 시즌 조회 (`PvpSeasonRepository.GetActiveSeasonAsync`)
   - 실패 시: `throw new InvalidOperationException("No active season")`
3. 매칭 실행 (`PvpMatchmakingService.FindOpponentAsync`)
   - 30초 타임아웃 시 NPC 봇 매칭
4. 전투 시뮬레이션 (`CombatService.SimulateCombatAsync` 재사용)
   - PVE 전투 로직 재사용 (Character vs Character)
5. 레이팅 계산 (`EloRatingService.CalculateNewRatings`)
   - 승자, 패자 새 레이팅 계산
6. **트랜잭션 시작** (UnitOfWork):
   ```csharp
   using var transaction = await _unitOfWork.BeginTransactionAsync();
   try {
     // 6.1 PvpMatch 생성 및 저장
     await _pvpMatchRepository.AddAsync(pvpMatch);

     // 6.2 PvpRanking 업데이트 (승자, 패자 모두)
     await _pvpRankingRepository.UpdateRankingAsync(winnerId, newWinnerRating, ...);
     await _pvpRankingRepository.UpdateRankingAsync(loserId, newLoserRating, ...);

     // 6.3 Character 보상 지급 (Gold, Crystal, Experience)
     await _characterRepository.AddRewardsAsync(characterId, rewards);

     // 6.4 트랜잭션 커밋 (PostgreSQL)
     await _unitOfWork.CommitAsync();
     await transaction.CommitAsync();

   } catch (Exception ex) {
     await transaction.RollbackAsync();
     _logger.LogError("PVP 매치 저장 실패: {Error}", ex.Message);
     throw;
   }
   ```
7. **Redis 랭킹 갱신** (트랜잭션 외부, Best Effort):
   ```csharp
   try {
     await _redisCacheService.UpdateRankingCacheAsync(seasonId, winnerId, newWinnerRating);
     await _redisCacheService.UpdateRankingCacheAsync(seasonId, loserId, newLoserRating);
   } catch (RedisException ex) {
     // Redis 갱신 실패 시 로깅만 하고 계속 진행 (PostgreSQL이 Source of Truth)
     _logger.LogWarning("Redis 랭킹 갱신 실패 (SeasonId: {SeasonId}): {Error}", seasonId, ex.Message);
     // 응답은 정상적으로 반환 (클라이언트에 영향 없음)
   }
   ```
8. 응답 DTO 생성 및 반환 (201 Created)

**에러 조건**:
- `UnauthorizedAccessException`: CharacterId 소유권 없음
- `InvalidOperationException`: 활성 시즌 없음
- `NotFoundException`: Character 또는 상대 캐릭터 없음

**🎓 학습 포인트 (아키텍처 결정)**:
- **TODO(human)**: 트랜잭션 경계는 Service vs Repository?
  - **Service**: Application Service에서 UnitOfWork 패턴으로 트랜잭션 관리
  - **Repository**: 각 Repository 메서드에서 개별 트랜잭션
  - **권장**: Service (Application Service에서 여러 Repository 호출을 하나의 트랜잭션으로 묶음)
- **TODO(human)**: Redis 갱신 실패 시 처리?
  - **Rollback**: PostgreSQL 트랜잭션 롤백 (일관성 우선)
  - **Best Effort**: PostgreSQL 커밋, Redis 갱신 실패 로깅 (가용성 우선)
  - **권장**: Best Effort (Redis는 캐시, PostgreSQL이 Source of Truth)

---

### `IPvpMatchmakingService` (신규)

**책임**: 매칭 로직 (±200 레이팅 범위, 30초 타임아웃, NPC 봇 매칭)

**메서드:**

#### `FindOpponentAsync(Guid characterId, int seasonId, CancellationToken cancellationToken)`

**시그니처**:
- 입력: `Guid characterId`, `int seasonId`, `CancellationToken cancellationToken`
- 반환: `Task<MatchOpponentDto>` (상대 정보: CharacterId, Name, Rating, IsBot)

**프로세스 흐름**:
1. 내 레이팅 조회 (`PvpRankingRepository`)
2. ±200 레이팅 범위 내 후보 조회 (최대 100명)
3. 후보가 있으면 랜덤 선택, 없으면 NPC 봇 생성
4. 상대방 정보 반환

**에러 조건**:
- `NotFoundException`: CharacterId 레이팅 정보 없음 (첫 매칭 시 초기 레이팅 생성)

**Dependencies:**
- `IPvpRankingRepository`: 레이팅 조회, 후보 조회

---

### `IPvpSeasonService` (신규)

**책임**: 시즌 관리 (시즌 종료, 보상 지급, Soft Reset)

**메서드:**

#### `ClaimSeasonRewardAsync(int seasonId, Guid characterId, CancellationToken cancellationToken)`

**시그니처**:
- 입력: `int seasonId`, `Guid characterId`, `CancellationToken cancellationToken`
- 반환: `Task<SeasonRewardDto>` (티어, 보상 내역)

**프로세스 흐름**:
1. SeasonId 검증 및 시즌 종료 확인 (`IsActive = false`)
2. PvpRanking 조회 (SeasonId, CharacterId)
3. 중복 수령 방지 (`IsRewardClaimed = false`)
4. 티어별 보상 계산 (Bronze ~ Diamond)
5. **트랜잭션**:
   - Character 보상 지급 (Crystal, 장비 상자)
   - PvpRanking 업데이트 (`IsRewardClaimed = true`)
6. 응답 반환

**에러 조건**:
- `InvalidOperationException`: 시즌이 아직 진행 중
- `InvalidOperationException`: 이미 보상 수령함
- `NotFoundException`: SeasonId 또는 CharacterId 레이팅 정보 없음

**Dependencies:**
- `IPvpSeasonRepository`: 시즌 조회
- `IPvpRankingRepository`: 랭킹 조회, 업데이트
- `ICharacterRepository`: 보상 지급

---

#### `StartNewSeasonAsync(int seasonNumber, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)`

**시그니처**:
- 입력: `int seasonNumber`, `DateTime startDate`, `DateTime endDate`, `CancellationToken cancellationToken`
- 반환: `Task<PvpSeason>` (새 시즌 정보)

**프로세스 흐름**:
1. 기존 활성 시즌 비활성화 (`IsActive = false`)
2. 새 시즌 생성 (`PvpSeason`)
3. Soft Reset 적용 (모든 PvpRanking):
   - 새 레이팅 = (기존 레이팅 + 1000) / 2
   - Wins, Losses, WinStreak 초기화
   - IsRewardClaimed = false
4. Redis 랭킹 캐시 초기화
5. 응답 반환

**에러 조건**:
- `InvalidOperationException`: SeasonNumber 중복
- `ArgumentException`: StartDate >= EndDate

**🎓 학습 포인트 (시스템 확장성)**:
- **TODO(human)**: Soft Reset vs Hard Reset?
  - **Soft Reset**: (기존 레이팅 + 1000) / 2 (고랭커 유지, 낮은 경기 수)
  - **Hard Reset**: 모두 1000으로 초기화 (평등, 초반 혼란)
  - **권장**: Soft Reset (일반적인 경쟁 게임 관례, 플레이어 경험 개선)
- **TODO(human)**: 시즌 종료 자동화?
  - **수동**: 관리자가 API 호출하여 시즌 종료
  - **자동**: Background Service (IHostedService)로 EndDate 체크, 자동 종료
  - **권장**: 수동 (MVP는 단순화, Phase 3에서 자동화 도입)

---

## 🧪 Testing Strategy

### Unit Tests

**Domain Service**:
- `EloRatingService.CalculateNewRatings`:
  - 동점 매칭 (1500 vs 1500) → 승자 +16, 패자 -16
  - 고랭커 vs 저랭커 (2000 vs 1000) → 승자 +3, 패자 -29
  - 저랭커 vs 고랭커 (1000 vs 2000) → 승자 +29, 패자 -3
  - 최소 레이팅 0 보장 (50 vs 1500, 패배 시 0으로 클램핑)

**Application Service**:
- `PvpMatchmakingService.FindOpponentAsync`:
  - 후보 있을 때 랜덤 선택
  - 후보 없을 때 NPC 봇 생성
  - 첫 매칭 시 초기 레이팅 1000 생성

**Mock 대상**:
- `IPvpRankingRepository`: 레이팅 조회, 후보 조회
- `IPvpSeasonRepository`: 시즌 조회
- `ICombatService`: 전투 시뮬레이션
- `IRedisCacheService`: Redis 캐싱

**핵심 시나리오**:
- 매칭 → 전투 → 레이팅 업데이트 (전체 흐름)
- Redis 장애 시 PostgreSQL Fallback
- 트랜잭션 롤백 (보상 지급 실패 시)

---

### Integration Tests

**API Endpoint**:
- `POST /api/pvp/matches`:
  - 정상 매칭 및 전투
  - 소유권 없는 CharacterId (401)
  - 활성 시즌 없음 (404)
- `GET /api/pvp/rankings`:
  - Top 100 조회 (Redis 활용)
  - 내 주변 ±10등 조회
  - 티어별 필터링 (PostgreSQL)
- `GET /api/pvp/matches/history`:
  - 최근 20경기 조회 (페이징)
  - 시즌 필터링
- `POST /api/pvp/seasons/{seasonId}/rewards`:
  - 시즌 보상 수령
  - 중복 수령 방지 (400)

**Database 연동**:
- PvpMatch 생성 및 조회
- PvpRanking 업데이트 (Rating, Wins/Losses)
- Redis Sorted Set 캐싱 (Top 100 조회)

---

### Test Coverage Goals
- Domain Services: 95%+ (EloRatingService는 핵심 로직)
- Application Services: 85%+ (PvpService, PvpMatchmakingService)
- Controllers: 75%+ (PvpController)

---

## ⚠️ Error Handling

### Exception Types
- `NotFoundException`: Character, PvpSeason, PvpRanking 미존재
- `UnauthorizedAccessException`: CharacterId 소유권 없음
- `InvalidOperationException`: 활성 시즌 없음, 시즌 보상 중복 수령, 시즌 진행 중
- `ArgumentException`: SeasonNumber 중복, StartDate >= EndDate
- `TimeoutException`: 매칭 타임아웃 (30초, NPC 봇 매칭으로 대응)

### Error Response Format
```json
{
  "message": "Character not found",
  "statusCode": 404,
  "details": {
    "characterId": "uuid"
  }
}
```

**HTTP 상태 코드 매핑**:
- `NotFoundException` → 404 Not Found
- `UnauthorizedAccessException` → 401 Unauthorized
- `InvalidOperationException` → 400 Bad Request
- `ArgumentException` → 400 Bad Request
- `TimeoutException` → 408 Request Timeout (내부 처리, NPC 봇 매칭)

---

## 🔐 Security Considerations

- **Authentication**: JWT Bearer Token 필수 (모든 API)
- **Authorization**:
  - CharacterId 소유권 검증 (JWT userId와 Character.OwnerId 일치)
  - 다른 플레이어 대신 매칭 요청 불가
- **Rate Limiting**: 매칭 요청 10회/분 제한 (남용 방지, Middleware 구현)
- **Server Authority**: 모든 전투 계산 서버에서 수행 (클라이언트 조작 불가)
- **Data Validation**:
  - CharacterId: UUID 형식 검증
  - Rating: 0 이상 정수
  - SeasonId: 존재하는 시즌
- **SQL Injection Prevention**: EF Core 파라미터화 (모든 쿼리)
- **Logging**:
  - 로그 대상: 매치 생성, 레이팅 변화, 보상 지급, Redis 갱신 실패
  - 민감 정보 로깅 금지: JWT Token

---

## 📊 Performance Considerations

- **Database Indexes**:
  - `IX_PvpRanking_SeasonId_Rating_DESC`: Top 100 조회 최적화 (O(log N + 100))
  - `IX_PvpMatch_AttackerId_CreatedAt`: 전적 조회 최적화
  - `IX_PvpMatch_DefenderId_CreatedAt`: 전적 조회 최적화
  - `IX_PvpSeason_IsActive`: 현재 시즌 조회 (단일 레코드)
- **Caching**: Redis Sorted Set (Top 100 랭킹, 100ms 이하)
  - Key 구조: `pvp:ranking:season:{seasonId}`
  - Write-Through: PostgreSQL 업데이트 → Redis 갱신
  - Fallback: Redis 장애 시 PostgreSQL 직접 조회 (성능 저하 허용)
- **Pagination**: Offset 기반 페이징 (전적 조회, 최대 50건/페이지)
- **N+1 Query Prevention**:
  - EF Core Include: `PvpMatch.Include(pm => pm.Attacker).Include(pm => pm.Defender)`
  - 또는: 별도 조회 후 DTO 매핑 (Character 이름만 필요)
- **Query Optimization**:
  - 매칭 후보 조회: `LIMIT 100` (최대 100명)
  - Redis ZREVRANGE: O(log N + M) (M = 반환 개수)
  - PostgreSQL Bitmap Index Scan: OR 조건 자동 병합

**🎓 학습 포인트 (성능 최적화)**:
- **TODO(human)**: Redis vs PostgreSQL 선택 전략?
  - **Redis**: 실시간 랭킹 (Top 100), 빠른 조회 (100ms)
  - **PostgreSQL**: 복잡한 필터링 (티어별, 시즌별), 데이터 일관성
  - **권장**: 하이브리드 (Top N은 Redis, 필터링은 PostgreSQL)
- **TODO(human)**: Character 이름 조회 전략?
  - **EF Core Include**: 한 번에 조회 (N+1 방지)
  - **별도 조회**: Character 이름만 필요, 불필요한 데이터 로드 방지
  - **권장**: 별도 조회 (PvpMatch는 CharacterId만 저장, 이름은 응답 시 조회)

---

## 🔄 Migration Plan

> ⚠️ **마이그레이션 요구사항만 명시** - 실제 SQL은 Implementation 단계에서 작성
> 위치: `IdleRPG.Infrastructure/migration.sql` (Idempotent 패턴)

### Database Migration 요구사항

**신규 테이블**:
- `PvpSeason`: 시즌 마스터 데이터 (5개 필드, INT PK, IsActive 인덱스)
- `PvpRanking`: 시즌별 랭킹 정보 (10개 필드, 복합키 SeasonId + CharacterId, 복합 인덱스)
  - **중요**: `Tier` 컬럼은 **GENERATED COLUMN** (Rating 기반 자동 계산)
- `PvpMatch`: 매치 기록 (9개 필드, UUID PK, AttackerId/DefenderId 인덱스)

**Enum 테이블**:
- `PvpTier` Enum: Bronze, Silver, Gold, Platinum, Diamond

**GENERATED COLUMN 요구사항** (PvpRanking.Tier):
```sql
-- PvpRanking 테이블 생성 시 Tier 컬럼을 GENERATED COLUMN으로 정의
ALTER TABLE "PvpRanking" ADD COLUMN "Tier" VARCHAR(20)
GENERATED ALWAYS AS (
  CASE
    WHEN "Rating" >= 2500 THEN 'Diamond'
    WHEN "Rating" >= 2000 THEN 'Platinum'
    WHEN "Rating" >= 1500 THEN 'Gold'
    WHEN "Rating" >= 1000 THEN 'Silver'
    ELSE 'Bronze'
  END
) STORED;
```
- **동작**: Rating 값이 변경되면 Tier가 자동으로 갱신
- **장점**: Application 로직 실수 방지, 데이터 정합성 보장
- **쿼리 최적화**: `WHERE Tier = 'Gold'` 필터링 가능 (인덱스 활용)

**인덱스 추가**:
- `IX_PvpRanking_SeasonId_Rating_DESC`: 랭킹 조회 최적화 (복합, DESC)
- `IX_PvpMatch_AttackerId_CreatedAt`: 전적 조회 최적화 (복합)
- `IX_PvpMatch_DefenderId_CreatedAt`: 전적 조회 최적화 (복합)
- `IX_PvpMatch_SeasonId`: 시즌별 필터링
- `IX_PvpSeason_IsActive`: 현재 시즌 조회
- `IX_PvpSeason_SeasonNumber`: Unique 제약

**데이터 마이그레이션**:
- 없음 (신규 기능)

**백업 권장**:
- 아니오 (신규 테이블, 기존 데이터 영향 없음)

**🎓 학습 포인트 (데이터베이스 설계)**:
- **TODO(human)**: Idempotent 마이그레이션 패턴?
  - **CREATE IF NOT EXISTS**: 중복 실행 시 에러 없음
  - **DROP IF EXISTS**: 테이블 재생성 (데이터 손실 주의)
  - **권장**: CREATE IF NOT EXISTS (안전, 배포 자동화 용이)
- **TODO(human)**: 인덱스 타입 선택 (B-Tree vs Hash)?
  - **B-Tree**: 범위 쿼리 (`BETWEEN`, `ORDER BY`) 지원
  - **Hash**: 정확 일치 쿼리만 (`=`) 지원, 빠름
  - **권장**: B-Tree (대부분의 쿼리가 범위 검색, PostgreSQL 기본값)
- **TODO(human)**: 복합 인덱스 컬럼 순서?
  - **원칙**: 필터링 컬럼 → 정렬 컬럼 순서
  - **예**: `IX_PvpRanking_SeasonId_Rating_DESC` (SeasonId로 필터링 → Rating으로 정렬)
  - **권장**: 쿼리 패턴 기반 결정 (`WHERE SeasonId = ? ORDER BY Rating DESC`)

---

### Data Seeding 계획

**Seeder 필요 여부**: Yes

**초기 데이터**:
- `PvpSeason`: 시즌 1 (SeasonNumber: 1, StartDate: 2025-11-01, EndDate: 2026-02-01, IsActive: true)
- `PvpRanking`: 없음 (플레이어 첫 매칭 시 자동 생성)
- `PvpMatch`: 없음 (매치 발생 시 생성)

**Seeder 클래스**: `PvpSeasonSeeder.cs` (Infrastructure Layer)

**Seeder 실행 시점**:
- 애플리케이션 시작 시 (`Program.cs`에서 `await seeder.SeedAsync()`)

> 💡 **구현 참고**: Seeder 코드는 Implementation 단계에서 작성 (`IdleRPG.Infrastructure/Seeders/PvpSeasonSeeder.cs`)

---

## 📝 Decision Log

> ⚠️ **중요**: L 사이즈 기능은 이 섹션 **필수**. 중요한 아키텍처 결정을 ADR과 연결.

| ID | Decision | Rationale | ADR Link | Spike Link | Status |
|----|----------|-----------|----------|------------|--------|
| D1 | Redis Sorted Set 채택 (랭킹 캐싱) | Top 100 랭킹 O(log N) 조회 (100ms 이하), Write-Through 전략 | - | - | Accepted |
| D2 | ELO 레이팅 시스템 채택 | 표준 알고리즘, 공정한 매칭, K-Factor 32 (업계 표준) | - | - | Accepted |
| D3 | PvpRanking 복합키 (SeasonId, CharacterId) | 자연키, 중복 데이터 방지, 시즌별 랭킹 분리 | - | - | Accepted |
| D4 | CombatService 재사용 (PVP 전투) | 기존 턴제 전투 로직 재사용, 중복 코드 방지 | - | - | Accepted |
| D5 | 매칭 알고리즘: ±200 레이팅 범위, 랜덤 선택 | 단순, 빠른 매칭 (평균 5초), 타임아웃 30초 → NPC 봇 | - | - | Accepted |
| D6 | Soft Reset ((기존 레이팅 + 1000) / 2) | 고랭커 유지, 플레이어 경험 개선 (일반적인 경쟁 게임 관례) | - | - | Accepted |
| D7 | OR 조건 사용 (전적 조회) | PostgreSQL Bitmap Index Scan 자동 병합, 단순한 쿼리 | - | - | Accepted |
| D8 | Tier는 Computed Property + Stored Column | DB 저장 (티어별 필터링), 계산 속성 (일관성) | - | - | Accepted |

**가이드**:
- **간단한 결정**: ADR 없이 테이블에 1줄로 기록
- **복잡한 결정**: ADR 작성 후 링크 (예: D1 Redis 채택)
- **Spike 결과**: Spike 링크 포함 (예: Redis vs PostgreSQL 성능 비교)

**Spike/ADR 트리거 조건**: 이 기능은 다음 조건에 해당하므로 ADR 없이 진행 가능
- ✅ 새 기술 도입 (Redis): 간단한 결정, ADR 불필요 (표준 캐싱 전략)
- ✅ 데이터 모델 변경 (신규 테이블): 기존 데이터 영향 없음, ADR 불필요
- ❌ 인프라 변경 (Redis 서버 추가): Docker Compose 수정만, ADR 불필요
- ❌ 보안/성능/크로스컷팅 영향: 기존 시스템 영향 없음, ADR 불필요

---

## 📱 Unity Client Integration

### Unity Documentation 필요 항목
- **API_SPEC.md**: 5개 엔드포인트 명세
  - `POST /api/pvp/matches`
  - `GET /api/pvp/rankings`
  - `GET /api/pvp/matches/history`
  - `GET /api/pvp/seasons/current`
  - `POST /api/pvp/seasons/{seasonId}/rewards`
- **DTOs.cs**: C# DTO 클래스 (Newtonsoft.Json 호환)
  - `PvpMatchRequestDto`
  - `PvpMatchResponseDto`
  - `PvpRankingDto`
  - `PvpMatchHistoryDto`
  - `PvpSeasonDto`
  - `SeasonRewardDto`
- **README.md**: PVP Arena 기능 개요, 사용 예시

**Unity 문서 경로** (프로젝트 외부):
- `../IdleRPGClient/Docs/unity/pvp-arena/API_SPEC.md`
- `../IdleRPGClient/Docs/unity/pvp-arena/DTOs.cs`
- `../IdleRPGClient/Docs/unity/pvp-arena/README.md`

**참고**: `docs/unity/UNITY_DOCUMENTATION_GUIDE.md`

---

## ✅ Approval

- [x] Design 리뷰 완료
- [x] 모든 Requirements 항목 커버 확인 (US-1 ~ US-4)
- [x] TODO(human) 아키텍처 학습 포인트 확인 완료 (8개)
- [x] Self-Review Checklist 10개 항목 통과 (목표: 9/10 이상)
- [x] Tasks 단계로 진행 승인

**Approved by**: Development Team
**Date**: 2025-11-06

---

**작성일**: 2025-11-06
**작성자**: AI (Claude)
**상태**: Approved
**Requirements 추적성**: [requirements.md](./requirements.md) US-1 (매칭 및 전투), US-2 (랭킹 조회), US-3 (전적 조회), US-4 (시즌 보상)
