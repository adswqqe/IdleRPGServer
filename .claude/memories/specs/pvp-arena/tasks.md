# Tasks: PVP Arena

> 이 문서는 PVP Arena 기능의 구현 작업 목록입니다.
>
> **작성 가이드**:
> - Design 문서의 모든 컴포넌트를 구현 가능한 작업으로 분해
> - 각 작업은 독립적으로 완료 및 테스트 가능해야 함
> - 순서는 의존성을 고려하여 정렬 (Domain → Infrastructure → Application → API)

---

## 📊 Progress Overview

**전체 진행률**: 27/33 (81.8%)

| Milestone | 작업 수 | 완료 | 진행률 |
|-----------|---------|------|--------|
| Domain Layer | 6 | 6 | 100% |
| Infrastructure Layer | 9 | 9 | 100% |
| Application Layer | 7 | 7 | 100% |
| API Layer | 5 | 5 | 100% |
| Database | 3 | 0 | 0% |
| Testing & Documentation | 3 | 0 | 0% |

**예상 총 소요 시간**: ~24시간

---

## 🏗️ Milestone 1: Domain Layer

### 1.1 Create PvpSeason Entity ⏱️ 30분 ✅
- [x] Create `IdleRPG.Domain/Entities/PvpSeason.cs`
- [x] Add properties: Id (int), SeasonNumber, StartDate, EndDate, IsActive, CreatedAt, UpdatedAt
- [x] Implement BaseEntity inheritance (CreatedAt, UpdatedAt)
- [x] Add validation: StartDate < EndDate

**Requirements**: [US-4]
**Design Reference**: [Data Model - PvpSeason]

---

### 1.2 Create PvpRanking Entity ⏱️ 45분 ✅
- [x] Create `IdleRPG.Domain/Entities/PvpRanking.cs`
- [x] Add properties: SeasonId, CharacterId (복합키), Rating, Wins, Losses, WinStreak, Tier, IsRewardClaimed, LastMatchAt, UpdatedAt
- [x] Add computed property: `public PvpTier Tier { get; }` (Rating 기반 계산)
- [x] Add navigation properties: Character, PvpSeason

**Requirements**: [US-2, US-4]
**Design Reference**: [Data Model - PvpRanking]

---

### 1.3 Create PvpMatch Entity ⏱️ 30분 ✅
- [x] Create `IdleRPG.Domain/Entities/PvpMatch.cs`
- [x] Add properties: Id (Guid), SeasonId, AttackerId, DefenderId, WinnerId, AttackerRatingBefore, AttackerRatingAfter, DefenderRatingBefore, DefenderRatingAfter, CreatedAt
- [x] Add navigation properties: Attacker (Character), Defender (Character), Winner (Character), PvpSeason
- [x] Add validation: AttackerId != DefenderId

**Requirements**: [US-1, US-3]
**Design Reference**: [Data Model - PvpMatch]

---

### 1.4 Create PvpTier Enum ⏱️ 15분 ✅
- [x] Create `IdleRPG.Domain/Enums/PvpTier.cs`
- [x] Define enum values: Bronze, Silver, Gold, Platinum, Diamond
- [x] Add XML documentation comments

**Requirements**: [US-2]
**Design Reference**: [Domain Layer - Enums]

---

### 1.5 Create PvpMatchResult Enum ⏱️ 15분 ✅
- [x] Create `IdleRPG.Domain/Enums/PvpMatchResult.cs`
- [x] Define enum values: Victory, Defeat
- [x] Add XML documentation comments

**Requirements**: [US-1]
**Design Reference**: [Domain Layer - Enums]

---

### 1.6 Create EloRatingService Domain Service ⏱️ 1시간 ✅
- [x] Create `IdleRPG.Domain/Services/EloRatingService.cs`
- [x] Implement `(int, int) CalculateNewRatings(int winnerRating, int loserRating, int kFactor = 32)` method
  - 승자/패자 기대 승률 계산 (ELO 표준 알고리즘)
  - 레이팅 변화 계산
  - 최소값 0 보장 (음수 방지)
- [x] Add input validation (레이팅 >= 0)
- [x] Add XML documentation
- [x] **🎓 TODO(human)**: K-Factor 관리 전략 결정
  - 하드코딩 32 vs Configuration (appsettings.json) vs Database 테이블
  - **결정: Option A (하드코딩 32) 채택** - MVP 단순성 우선, Phase 2에서 Configuration 도입

**Requirements**: [US-1]
**Design Reference**: [Business Logic - ELO 레이팅 계산]

> 💡 **학습 가이드**: ELO 계산식은 AI가 제공합니다. **Domain Service 계층 분리**와 **순수 비즈니스 로직 작성**에 집중하세요.

---

## 🔧 Milestone 2: Infrastructure Layer

### 2.1 Create IPvpSeasonRepository Interface ⏱️ 30분 ✅
- [x] Create `IdleRPG.Domain/Repositories/IPvpSeasonRepository.cs`
- [x] Define methods:
  - `Task<PvpSeason?> GetByIdAsync(int id, CancellationToken cancellationToken = default)`
  - `Task<PvpSeason?> GetActiveSeasonAsync(CancellationToken cancellationToken = default)`
  - `Task<PvpSeason?> GetBySeasonNumberAsync(int seasonNumber, CancellationToken cancellationToken = default)`
  - `Task AddAsync(PvpSeason season, CancellationToken cancellationToken = default)`
  - `Task UpdateAsync(PvpSeason season, CancellationToken cancellationToken = default)`

**Requirements**: [US-4]
**Design Reference**: [Infrastructure Layer - Repositories]

---

### 2.2 Create PvpSeasonRepository Implementation ⏱️ 45분 ✅
- [x] Create `IdleRPG.Infrastructure/Repositories/PvpSeasonRepository.cs`
- [x] Implement IPvpSeasonRepository
- [x] Implement GetActiveSeasonAsync: `WHERE IsActive = true` (단일 레코드)
- [x] Implement GetBySeasonNumberAsync: `WHERE SeasonNumber = ?`
- [x] Add AsNoTracking() for read queries

**Requirements**: [US-4]
**Design Reference**: [Infrastructure Layer - Repositories]

---

### 2.3 Create IPvpRankingRepository Interface ⏱️ 45분 ✅
- [x] Create `IdleRPG.Domain/Repositories/IPvpRankingRepository.cs`
- [x] Define methods:
  - `Task<PvpRanking?> GetByIdAsync(int seasonId, Guid characterId, CancellationToken cancellationToken = default)`
  - `Task<List<PvpRanking>> GetTopRankingsAsync(int seasonId, int count, CancellationToken cancellationToken = default)`
  - `Task<List<PvpRanking>> GetRankingsAroundAsync(int seasonId, int rating, int range, CancellationToken cancellationToken = default)`
  - `Task<List<PvpRanking>> GetByTierAsync(int seasonId, PvpTier tier, int page, int pageSize, CancellationToken cancellationToken = default)`
  - `Task AddAsync(PvpRanking ranking, CancellationToken cancellationToken = default)`
  - `Task UpdateAsync(PvpRanking ranking, CancellationToken cancellationToken = default)`

**Requirements**: [US-2, US-4]
**Design Reference**: [Infrastructure Layer - Repositories]

---

### 2.4 Create PvpRankingRepository Implementation ⏱️ 1시간 ✅
- [x] Create `IdleRPG.Infrastructure/Repositories/PvpRankingRepository.cs`
- [x] Implement IPvpRankingRepository
- [x] Implement GetTopRankingsAsync: `WHERE SeasonId = ? ORDER BY Rating DESC LIMIT ?`
- [x] Implement GetRankingsAroundAsync: 레이팅 기준 ±range 조회
- [x] Implement GetByTierAsync: `WHERE SeasonId = ? AND Tier = ? ORDER BY Rating DESC` (페이징)
- [x] Add Include() for Character navigation property (N+1 방지)

**Requirements**: [US-2]
**Design Reference**: [Infrastructure Layer - Repositories]

---

### 2.5 Create IPvpMatchRepository Interface ⏱️ 30분 ✅
- [x] Create `IdleRPG.Domain/Repositories/IPvpMatchRepository.cs`
- [x] Define methods:
  - `Task<PvpMatch?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)`
  - `Task<(List<PvpMatch> matches, int totalCount)> GetMatchHistoryAsync(Guid characterId, int? seasonId, int page, int pageSize, CancellationToken cancellationToken = default)`
  - `Task AddAsync(PvpMatch match, CancellationToken cancellationToken = default)`

**Requirements**: [US-1, US-3]
**Design Reference**: [Infrastructure Layer - Repositories]

---

### 2.6 Create PvpMatchRepository Implementation ⏱️ 1시간 ✅
- [x] Create `IdleRPG.Infrastructure/Repositories/PvpMatchRepository.cs`
- [x] Implement IPvpMatchRepository
- [x] Implement GetMatchHistoryAsync: `WHERE (AttackerId = ? OR DefenderId = ?) AND (SeasonId = ? OR ? IS NULL) ORDER BY CreatedAt DESC` (페이징)
- [x] Add Include() for Character navigation properties (Attacker, Defender)
- [x] Return totalCount for pagination

**Requirements**: [US-3]
**Design Reference**: [Infrastructure Layer - Repositories]

---

### 2.7 Create PvpSeason EF Core Configuration ⏱️ 30분 ✅
- [x] Create `IdleRPG.Infrastructure/Configurations/PvpSeasonConfiguration.cs`
- [x] Implement IEntityTypeConfiguration<PvpSeason>
- [x] Configure table name: "PvpSeason"
- [x] Configure primary key: Id (int, auto-increment)
- [x] Configure properties: SeasonNumber (required, unique), StartDate/EndDate (required), IsActive (default false)
- [x] Configure indexes:
  - `IX_PvpSeason_IsActive`
  - `IX_PvpSeason_SeasonNumber` (unique)

**Requirements**: [US-4]
**Design Reference**: [Data Model - EF Core Configuration]

---

### 2.8 Create PvpRanking EF Core Configuration ⏱️ 45분 ✅
- [x] Create `IdleRPG.Infrastructure/Configurations/PvpRankingConfiguration.cs`
- [x] Implement IEntityTypeConfiguration<PvpRanking>
- [x] Configure table name: "PvpRanking"
- [x] Configure composite primary key: `HasKey(pr => new { pr.SeasonId, pr.CharacterId })`
- [x] Configure Tier enum: `Property(pr => pr.Tier).HasConversion<string>().ValueGeneratedOnAddOrUpdate()`
- [x] Configure relationships:
  - `HasOne(PvpSeason).WithMany().HasForeignKey(pr => pr.SeasonId).OnDelete(DeleteBehavior.Restrict)`
  - `HasOne(Character).WithMany().HasForeignKey(pr => pr.CharacterId).OnDelete(DeleteBehavior.Cascade)`
- [x] Configure indexes:
  - `IX_PvpRanking_SeasonId_Rating_DESC` (복합 인덱스, DESC)
- [x] **🎓 TODO(human)**: Cascade Delete vs Restrict 전략 근거 작성
  - **최종 결정 (재검토 후 수정)**:
  - PvpRanking.SeasonId → **Restrict** (시즌은 히스토리 마스터 데이터, 과거 랭킹 기록 보존)
  - PvpRanking.CharacterId → **Cascade** (GDPR 준수, 개인정보 완전 삭제)

**Requirements**: [US-2]
**Design Reference**: [Data Model - EF Core Configuration]

> 💡 **학습 가이드**: 복합키 설정과 외래 키 Cascade 전략이 핵심입니다. **데이터 무결성**과 **삭제 정책**을 고민하세요.

---

### 2.9 Create PvpMatch EF Core Configuration ⏱️ 45분 ✅
- [x] Create `IdleRPG.Infrastructure/Configurations/PvpMatchConfiguration.cs`
- [x] Implement IEntityTypeConfiguration<PvpMatch>
- [x] Configure table name: "PvpMatch"
- [x] Configure primary key: Id (Guid)
- [x] Configure properties: 모든 Rating 필드 required
- [x] Configure relationships (다중 FK):
  - `HasOne(Character).WithMany().HasForeignKey(pm => pm.AttackerId).OnDelete(DeleteBehavior.Restrict)`
  - `HasOne(Character).WithMany().HasForeignKey(pm => pm.DefenderId).OnDelete(DeleteBehavior.Restrict)`
  - `HasOne(Character).WithMany().HasForeignKey(pm => pm.WinnerId).OnDelete(DeleteBehavior.Restrict)`
  - `HasOne(PvpSeason).WithMany().HasForeignKey(pm => pm.SeasonId).OnDelete(DeleteBehavior.Restrict)`
- [x] Configure indexes:
  - `IX_PvpMatch_AttackerId_CreatedAt` (복합)
  - `IX_PvpMatch_DefenderId_CreatedAt` (복합)
  - `IX_PvpMatch_SeasonId`

**Requirements**: [US-1, US-3]
**Design Reference**: [Data Model - EF Core Configuration]

---

## 📦 Milestone 3: Application Layer

### 3.1 Create IRedisCacheService Interface ⏱️ 30분 ✅
- [x] Create `IdleRPG.Application/Services/IRedisCacheService.cs`
- [x] Define methods:
  - `Task UpdateRankingCacheAsync(int seasonId, Guid characterId, int rating, CancellationToken cancellationToken = default)`
  - `Task<Dictionary<Guid, int>?> GetTopRankingsAsync(int seasonId, int count, CancellationToken cancellationToken = default)`
  - `Task<int?> GetMyRankAsync(int seasonId, Guid characterId, CancellationToken cancellationToken = default)`
  - `Task<Dictionary<Guid, int>?> GetRankingsAroundMeAsync(int seasonId, Guid characterId, int range, CancellationToken cancellationToken = default)`
  - `Task ClearRankingCacheAsync(int seasonId, CancellationToken cancellationToken = default)`

**Requirements**: [US-2]
**Design Reference**: [Infrastructure Layer - External Services]

---

### 3.2 Create RedisCacheService Implementation ⏱️ 1.5시간 ✅
- [x] Create `IdleRPG.Infrastructure/Services/RedisCacheService.cs`
- [x] Implement IRedisCacheService
- [x] Use Redis Sorted Set: Key = `pvp:ranking:season:{seasonId}`
- [x] Implement UpdateRankingCacheAsync: ZADD (Write-Through)
- [x] Implement GetTopRankingsAsync: ZREVRANGE (O(log N + count))
- [x] Implement GetMyRankAsync: ZREVRANK
- [x] Implement GetRankingsAroundMeAsync: ZREVRANGE with offset
- [x] Add error handling (RedisException → log + return null)
- [x] **🎓 TODO(human)**: Redis 장애 시 처리 전략
  - **최종 결정: Best Effort 전략 채택**
  - **근거**: PostgreSQL이 Source of Truth, Redis는 성능 최적화용 캐시
  - **동작**: Redis 실패 시 null 반환 → Controller에서 PostgreSQL Fallback
  - **장점**: 가용성 우선 (Graceful Degradation), 시스템 계속 작동 (성능만 저하)

**Requirements**: [US-2]
**Design Reference**: [Infrastructure Layer - Redis Caching]

> 💡 **학습 가이드**: Redis Sorted Set 자료구조와 Write-Through 캐싱 전략을 학습하세요.

---

### 3.3 Create IPvpMatchmakingService Interface ⏱️ 30분 ✅
- [x] Create `IdleRPG.Application/Services/IPvpMatchmakingService.cs`
- [x] Define method:
  - `Task<MatchOpponentDto> FindOpponentAsync(Guid characterId, int seasonId, CancellationToken cancellationToken = default)`
- [x] Create `IdleRPG.Application/DTOs/Pvp/MatchOpponentDto.cs`

**Requirements**: [US-1]
**Design Reference**: [Service Layer - PvpMatchmakingService]

---

### 3.4 Create PvpMatchmakingService Implementation ⏱️ 1.5시간 ✅
- [x] Create `IdleRPG.Infrastructure/Services/PvpMatchmakingService.cs`
- [x] Implement IPvpMatchmakingService
- [x] Inject IUnitOfWork, IRandomProvider
- [x] Implement FindOpponentAsync:
  - 내 레이팅 조회 (없으면 초기 레이팅 1000 생성)
  - ±200 레이팅 범위 내 후보 조회 (최대 100명)
  - 후보 있으면 Random.Next() 선택
  - 후보 없으면 NPC 봇 생성 (레이팅: 내 레이팅 ± Random(-100, 100), 이름: "Bot_" + Random(1000, 9999))
- [x] Return MatchOpponentDto (CharacterId, Name, Rating, IsBot)
- [x] **🎓 TODO(human)**: 매칭 타임아웃 처리 전략
  - **최종 결정: 즉시 NPC 봇 생성 (동기 처리) 채택**
  - **근거**: MVP 단순화, HTTP 타임아웃 위험 회피, 사용자 경험 우선
  - **동작**: 후보 없으면 즉시 NPC 봇 반환 (대기 시간 0초)
  - **Phase 3 개선**: Redis Queue + 백그라운드 워커로 비동기 매칭 도입

**Requirements**: [US-1]
**Design Reference**: [Business Logic - 매칭 알고리즘]

> 💡 **학습 가이드**: Application Service는 Repository를 조율합니다. **계층 분리 (Application vs Domain)**에 집중하세요.

---

### 3.5 Create IPvpSeasonService Interface ⏱️ 30분 ✅
- [x] Create `IdleRPG.Application/Services/IPvpSeasonService.cs`
- [x] Define methods:
  - `Task<SeasonRewardDto> ClaimSeasonRewardAsync(int seasonId, Guid characterId, CancellationToken cancellationToken = default)`
  - `Task<PvpSeason> StartNewSeasonAsync(int seasonNumber, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)`

**Requirements**: [US-4]
**Design Reference**: [Service Layer - PvpSeasonService]

---

### 3.6 Create PvpSeasonService Implementation ⏱️ 1.5시간 ✅
- [x] Create `IdleRPG.Application/Services/PvpSeasonService.cs`
- [x] Implement IPvpSeasonService
- [x] Inject IPvpSeasonRepository, IPvpRankingRepository, ICharacterRepository, IRedisCacheService
- [x] Implement ClaimSeasonRewardAsync:
  - SeasonId 검증 및 시즌 종료 확인 (IsActive = false)
  - PvpRanking 조회 (SeasonId, CharacterId)
  - 중복 수령 방지 (IsRewardClaimed = false)
  - 티어별 보상 계산 (Bronze: Crystal 100, Silver: 300, Gold: 500 + 전설 장비 상자 1개, Platinum: 1000 + 전설 3개, Diamond: 2000 + 신화 1개)
  - 트랜잭션: Character 보상 지급, PvpRanking.IsRewardClaimed = true
- [x] Implement StartNewSeasonAsync:
  - 기존 활성 시즌 비활성화 (IsActive = false)
  - 새 시즌 생성
  - Soft Reset 적용 (새 레이팅 = (기존 레이팅 + 1000) / 2)
  - Redis 캐시 초기화
- [x] **🎓 TODO(human)**: Soft Reset vs Hard Reset 전략
  - **최종 결정: Soft Reset 채택** (고랭커 유지, 플레이어 경험 개선)

**Requirements**: [US-4]
**Design Reference**: [Service Layer - PvpSeasonService]

> 💡 **학습 가이드**: 시즌 보상 계산 로직은 AI가 제공합니다. **트랜잭션 경계 설정**을 학습하세요.

---

### 3.7 Create IPvpService Interface & Implementation ⏱️ 2시간 ✅
- [x] Create `IdleRPG.Application/Services/IPvpService.cs` (interface)
- [x] Create `IdleRPG.Application/Services/PvpService.cs` (implementation)
- [x] Create `IdleRPG.Application/DTOs/Pvp/PvpMatchResponseDto.cs` (DTO)
- [x] Inject dependencies: IPvpMatchmakingService, UnitOfWork, EloRatingService, IRedisCacheService
- [x] Implement `StartMatchAsync(Guid characterId, Guid userId, CancellationToken cancellationToken)`:
  - CharacterId 소유권 검증 (userId와 Character.OwnerId 일치)
  - 현재 활성 시즌 조회
  - 매칭 실행 (PvpMatchmakingService)
  - 전투 시뮬레이션 (간단한 전투력 비교, Phase 2에서 CombatService 통합)
  - 레이팅 계산 (EloRatingService)
  - **트랜잭션 시작** (UnitOfWork):
    - PvpMatch 생성
    - PvpRanking 업데이트 (Rating, Wins/Losses, WinStreak)
    - Character 보상 지급 (Gold, Crystal, Experience)
    - 트랜잭션 커밋
  - **Redis 랭킹 갱신** (트랜잭션 외부, Best Effort)
  - 응답 DTO 생성 및 반환
- [x] Add error handling: UnauthorizedAccessException, InvalidOperationException, KeyNotFoundException
- [x] Add logging
- [x] **🎓 TODO(human)**: 트랜잭션 경계 설정 근거 (인터페이스 XML 주석에 명시)
  - **최종 결정: Service 계층에서 UnitOfWork로 트랜잭션 관리**
  - **근거**: 여러 Repository 작업(PvpMatch 생성, PvpRanking 업데이트, Character 보상)을 하나의 원자적 단위로 묶어야 함
  - **Redis는 트랜잭션 외부**: Best Effort, Graceful Degradation

**Requirements**: [US-1]
**Design Reference**: [Service Layer - PvpService]

> 💡 **학습 가이드**: PvpService는 **전체 흐름 오케스트레이션**을 담당합니다. **트랜잭션 관리**와 **에러 처리**가 핵심입니다.

---

## 🌐 Milestone 4: API Layer

### 4.1 Create PVP Request/Response DTOs ⏱️ 1시간 ✅
- [x] Create `IdleRPG.Application/DTOs/Pvp/PvpMatchRequestDto.cs`
  - Properties: CharacterId (Guid)
- [x] Create `IdleRPG.Application/DTOs/Pvp/PvpMatchResponseDto.cs`
  - Properties: MatchId, Opponent (OpponentDto), Result, MyRatingBefore, MyRatingAfter, OpponentRatingChange, Rewards, CombatLog
- [x] Create `IdleRPG.Application/DTOs/Pvp/OpponentDto.cs`
  - Properties: CharacterId, Name, Rating, IsBot
- [x] Create `IdleRPG.Application/DTOs/Pvp/PvpRankingDto.cs`
  - Properties: Rank, CharacterId, CharacterName, Rating, Wins, Losses, WinRate, Tier
- [x] Create `IdleRPG.Application/DTOs/Pvp/PvpMatchHistoryDto.cs`
  - Properties: MatchId, SeasonNumber, OpponentCharacterId, OpponentName, Result, MyRatingChange, OpponentRatingChange, CreatedAt
- [x] Create `IdleRPG.Application/DTOs/Pvp/PvpSeasonDto.cs`
  - Properties: SeasonId, SeasonNumber, StartDate, EndDate, DaysRemaining, IsActive
- [x] Create `IdleRPG.Application/DTOs/Pvp/SeasonRewardDto.cs`
  - Properties: SeasonNumber, Tier, FinalRating, FinalRank, Rewards, AlreadyClaimed
- [x] Add XML documentation to all DTOs

**Requirements**: [US-1, US-2, US-3, US-4]
**Design Reference**: [API Design - DTOs]

---

### 4.2 Create PVP Request Validators ⏱️ 45분 ✅
- [x] Create `IdleRPG.Application/Validators/PvpMatchRequestValidator.cs`
  - RuleFor(x => x.CharacterId).NotEmpty().Must(BeValidGuid)
- [x] Create `IdleRPG.Application/Validators/GetRankingsRequestValidator.cs`
  - RuleFor(x => x.SeasonId).GreaterThan(0).When(x => x.SeasonId.HasValue)
  - RuleFor(x => x.Top).InclusiveBetween(1, 1000).When(x => x.Top.HasValue)
  - RuleFor(x => x.Range).InclusiveBetween(1, 50).When(x => x.Range.HasValue)
  - RuleFor(x => x.Tier).IsInEnum().When(x => x.Tier.HasValue)
- [x] Create `IdleRPG.Application/Validators/GetMatchHistoryRequestValidator.cs`
  - RuleFor(x => x.CharacterId).NotEmpty().Must(BeValidGuid)
  - RuleFor(x => x.PageSize).InclusiveBetween(1, 50)

**Requirements**: [US-1, US-2, US-3]
**Design Reference**: [API Design - Validation Rules]

---

### 4.3 Create PvpController - Match Endpoints ⏱️ 1.5시간 ✅
- [x] Create `IdleRPG.API/Controllers/PvpController.cs`
- [x] Add [ApiController], [Route("api/pvp")], [Authorize] attributes
- [x] Inject IPvpService dependency
- [x] Implement `POST /api/pvp/matches` endpoint:
  - Extract userId from JWT claims
  - Validate PvpMatchRequestDto
  - Call PvpService.StartMatchAsync
  - Return 201 Created with PvpMatchResponseDto
  - Handle exceptions: 400 (CharacterId 소유권 없음), 404 (활성 시즌 없음)
- [x] Implement `GET /api/pvp/matches/history` endpoint:
  - Query parameters: characterId, seasonId, page, pageSize
  - Validate query parameters
  - Call PvpMatchRepository.GetMatchHistoryAsync
  - Return 200 OK with pagination metadata
- [x] Add Swagger XML comments

**Requirements**: [US-1, US-3]
**Design Reference**: [API Design - Endpoints 1, 3]

---

### 4.4 Create PvpController - Ranking Endpoints ⏱️ 1.5시간 ✅
- [x] Add `GET /api/pvp/rankings` endpoint to PvpController:
  - Query parameters: seasonId, top, nearMe, range, tier, page, pageSize
  - Validate query parameters
  - Top N 조회: Redis 시도 → PostgreSQL Fallback
  - 내 주변 조회 (nearMe=true): Redis 시도 → PostgreSQL Fallback
  - 티어별 조회: PostgreSQL 직접 조회
  - Character 이름 조회 (N+1 방지)
  - Return 200 OK with PvpRankingDto list
  - Handle exceptions: 404 (시즌 없음), 401 (nearMe 시 인증 필요)
- [x] Add Swagger XML comments
- [x] **🎓 TODO(human)**: Redis vs PostgreSQL 전략 근거 작성
  - Top N은 Redis 우선, 티어별 필터링은 PostgreSQL
  - Redis 장애 시 PostgreSQL Fallback (성능 저하 허용)

**Requirements**: [US-2]
**Design Reference**: [API Design - Endpoint 2]

> 💡 **학습 가이드**: Redis Fallback 전략이 핵심입니다. **캐시 장애 처리**와 **성능 최적화**를 고민하세요.

---

### 4.5 Create PvpController - Season Endpoints ⏱️ 1시간 ✅
- [x] Add `GET /api/pvp/seasons/current` endpoint to PvpController:
  - No authentication required (Public)
  - Call PvpSeasonRepository.GetActiveSeasonAsync
  - Calculate DaysRemaining
  - Return 200 OK with PvpSeasonDto
  - Handle exception: 404 (활성 시즌 없음)
- [x] Add `POST /api/pvp/seasons/{seasonId}/rewards` endpoint to PvpController:
  - Extract userId from JWT, CharacterId 조회
  - Call PvpSeasonService.ClaimSeasonRewardAsync
  - Return 200 OK with SeasonRewardDto
  - Handle exceptions: 400 (시즌 진행 중, 이미 수령함), 404 (시즌 없음, 랭킹 없음)
- [x] Add Swagger XML comments

**Requirements**: [US-4]
**Design Reference**: [API Design - Endpoints 4, 5]

---

## 🗃️ Milestone 5: Database

### 5.1 Create Database Migration ⏱️ 1.5시간
- [ ] Add migration to `IdleRPG.Infrastructure/migration.sql`
- [ ] Use DO $EF$ BEGIN ... END $EF$ pattern (idempotent)
- [ ] Add CREATE TABLE for PvpSeason:
  - Id INT PRIMARY KEY (auto-increment)
  - SeasonNumber INT UNIQUE NOT NULL
  - StartDate, EndDate TIMESTAMP NOT NULL
  - IsActive BOOLEAN DEFAULT FALSE
  - CreatedAt, UpdatedAt TIMESTAMP
- [ ] Add CREATE TABLE for PvpRanking:
  - PRIMARY KEY (SeasonId, CharacterId)
  - Rating INT DEFAULT 1000, Wins INT DEFAULT 0, Losses INT DEFAULT 0, WinStreak INT DEFAULT 0
  - Tier VARCHAR(20) GENERATED ALWAYS AS (...) STORED (Rating 기반 계산)
  - IsRewardClaimed BOOLEAN DEFAULT FALSE
  - LastMatchAt, UpdatedAt TIMESTAMP
- [ ] Add CREATE TABLE for PvpMatch:
  - Id UUID PRIMARY KEY
  - SeasonId, AttackerId, DefenderId, WinnerId (FK)
  - AttackerRatingBefore, AttackerRatingAfter, DefenderRatingBefore, DefenderRatingAfter INT
  - CreatedAt TIMESTAMP
- [ ] Add CREATE INDEX statements:
  - IX_PvpSeason_IsActive, IX_PvpSeason_SeasonNumber (UNIQUE)
  - IX_PvpRanking_SeasonId_Rating_DESC (복합, DESC)
  - IX_PvpMatch_AttackerId_CreatedAt, IX_PvpMatch_DefenderId_CreatedAt (복합)
  - IX_PvpMatch_SeasonId
- [ ] Add FK constraints with OnDelete rules (Restrict vs Cascade)
- [ ] **🎓 TODO(human)**: 인덱스 전략 근거 작성
  - 복합 인덱스 컬럼 순서 (필터링 → 정렬)
  - GENERATED COLUMN (Tier) 장점: 데이터 정합성 보장, 쿼리 필터링 최적화

**Requirements**: [All]
**Design Reference**: [Migration Plan]
**참고**: `CLAUDE.md - Database Migration`

> 💡 **학습 가이드**: GENERATED COLUMN과 복합 인덱스 설계가 핵심입니다. **인덱싱 전략**과 **데이터 정합성**을 학습하세요.

---

### 5.2 Create PvpSeasonSeeder ⏱️ 30분
- [ ] Create `IdleRPG.Infrastructure/Seeders/PvpSeasonSeeder.cs`
- [ ] Add Season 1 초기 데이터:
  - SeasonNumber: 1
  - StartDate: 2025-11-01
  - EndDate: 2026-02-01
  - IsActive: true
- [ ] Check if Season 1 already exists (중복 방지)
- [ ] Register in `Program.cs` or DbContext seeding

**Requirements**: [US-4]
**Design Reference**: [Data Seeding 계획]

---

### 5.3 Register Dependencies in DI Container ⏱️ 30분
- [ ] Open `IdleRPG.API/Program.cs`
- [ ] Register repositories:
  - `AddScoped<IPvpSeasonRepository, PvpSeasonRepository>()`
  - `AddScoped<IPvpRankingRepository, PvpRankingRepository>()`
  - `AddScoped<IPvpMatchRepository, PvpMatchRepository>()`
- [ ] Register services:
  - `AddScoped<IPvpService, PvpService>()`
  - `AddScoped<IPvpMatchmakingService, PvpMatchmakingService>()`
  - `AddScoped<IPvpSeasonService, PvpSeasonService>()`
  - `AddScoped<IRedisCacheService, RedisCacheService>()`
- [ ] Register domain services:
  - `AddScoped<EloRatingService>()`
- [ ] Add FluentValidation validators:
  - `AddValidatorsFromAssemblyContaining<PvpMatchRequestValidator>()`

**Requirements**: [All]
**Design Reference**: [Architecture Overview]

---

## 🧪 Milestone 6: Testing & Documentation

### 6.1 Create EloRatingService & PvpService Unit Tests ⏱️ 2.5시간
- [ ] Create `IdleRPG.Tests/Domain/Services/EloRatingServiceTests.cs`
  - Test case: 동점 매칭 (1500 vs 1500) → 승자 +16, 패자 -16
  - Test case: 고랭커 vs 저랭커 (2000 vs 1000) → 승자 +3, 패자 -29
  - Test case: 저랭커 vs 고랭커 (1000 vs 2000) → 승자 +29, 패자 -3
  - Test case: 최소 레이팅 0 보장 (50 vs 1500, 패배 시 0으로 클램핑)
  - Use FluentAssertions
  - Achieve 95%+ code coverage
- [ ] Create `IdleRPG.Tests/Application/Services/PvpServiceTests.cs`
  - Mock: IPvpMatchmakingService, IPvpSeasonRepository, IPvpMatchRepository, IPvpRankingRepository, ICharacterRepository, ICombatService, EloRatingService, IRedisCacheService
  - Test case: 정상 매칭 → 전투 → 레이팅 업데이트 (전체 흐름)
  - Test case: CharacterId 소유권 없음 (UnauthorizedAccessException)
  - Test case: 활성 시즌 없음 (InvalidOperationException)
  - Test case: Redis 장애 시 PostgreSQL Fallback
  - Test case: 트랜잭션 롤백 (보상 지급 실패)
  - Achieve 85%+ code coverage

**Requirements**: [US-1]
**Design Reference**: [Testing Strategy - Unit Tests]

---

### 6.2 Create PvpController Integration Tests ⏱️ 2시간
- [ ] Create `IdleRPG.Tests/API/Controllers/PvpControllerTests.cs`
- [ ] Setup: Use WebApplicationFactory, in-memory database or test container
- [ ] Test `POST /api/pvp/matches`:
  - 정상 매칭 및 전투 (201 Created)
  - 소유권 없는 CharacterId (401 Unauthorized)
  - 활성 시즌 없음 (404 Not Found)
- [ ] Test `GET /api/pvp/rankings`:
  - Top 100 조회 (200 OK)
  - 내 주변 ±10등 조회 (200 OK)
  - 티어별 필터링 (200 OK)
- [ ] Test `GET /api/pvp/matches/history`:
  - 최근 20경기 조회 (200 OK, 페이징)
  - 시즌 필터링 (200 OK)
- [ ] Test `POST /api/pvp/seasons/{seasonId}/rewards`:
  - 시즌 보상 수령 (200 OK)
  - 중복 수령 방지 (400 Bad Request)
- [ ] Achieve 75%+ controller coverage

**Requirements**: [US-1, US-2, US-3, US-4]
**Design Reference**: [Testing Strategy - Integration Tests]

---

### 6.3 Create Unity Documentation ⏱️ 1.5시간
- [ ] Create `../IdleRPGClient/Docs/unity/pvp-arena/` folder
- [ ] Create `API_SPEC.md`:
  - 5개 엔드포인트 명세 (URL, Method, Headers, Request/Response)
  - Unity C# 사용 예시 (UnityWebRequest)
  - 에러 코드 목록
- [ ] Create `DTOs.cs`:
  - Unity-compatible C# DTOs (7개: PvpMatchRequestDto, PvpMatchResponseDto, OpponentDto, PvpRankingDto, PvpMatchHistoryDto, PvpSeasonDto, SeasonRewardDto)
  - Use [JsonProperty] attributes (Newtonsoft.Json)
  - Add example values in comments
- [ ] Create `README.md`:
  - PVP Arena 기능 개요
  - 주요 흐름: 매칭 → 전투 → 랭킹 조회 → 시즌 보상
  - 사용 예시 (코드 스니펫)
- [ ] Update `../IdleRPGClient/Docs/unity/README.md` main index (pvp-arena 추가)

**Requirements**: [All]
**Design Reference**: [Unity Client Integration]
**참고**: `docs/unity/UNITY_DOCUMENTATION_GUIDE.md`

---

## 🚀 Post-Implementation

### ✅ Completion Checklist
- [ ] All tasks completed and tested
- [ ] Unit tests passing (Domain: 95%+, Application: 85%+)
- [ ] Integration tests passing (Controller: 75%+)
- [ ] Migration applied via Jenkins (DO NOT run `dotnet ef database update` locally!)
- [ ] Unity documentation complete (API_SPEC.md, DTOs.cs, README.md)
- [ ] Code review completed
- [ ] Git commit with descriptive message
- [ ] Feature merged to main branch

---

## 📝 Notes

### Blockers
<!-- 작업 중 발생한 장애 요소 기록 -->

### Decisions Made
<!-- TODO(human) 해소 과정에서 내린 결정 기록 -->
- K-Factor 관리: 하드코딩 32 (MVP), Phase 2에서 Configuration 도입
- Redis 장애 처리: Best Effort (PostgreSQL이 Source of Truth)
- 매칭 타임아웃: 동기 대기 (30초), Phase 3에서 비동기 큐 도입
- Soft Reset 채택: (기존 레이팅 + 1000) / 2
- **OnDelete 정책 (재검토 후 수정)**:
  - PvpRanking.SeasonId → Restrict (히스토리 마스터 데이터 보존)
  - PvpRanking.CharacterId → Cascade (GDPR 준수, 개인정보 완전 삭제)

### Future Improvements
<!-- 나중에 개선할 사항 -->
- Redis 비동기 큐로 매칭 시스템 확장
- K-Factor Configuration 테이블 도입 (런타임 조정)
- 시즌 종료 자동화 (IHostedService)
- 리플레이 시스템 (BattleLog 저장 및 재생)

---

**시작일**: 2025-11-06
**완료일**: -
**총 소요 시간**: ~24시간
**Design 추적성**: design.md의 모든 컴포넌트 커버 확인 (3 Entities, 2 Enums, 1 Domain Service, 3 Repositories, 3 Application Services, 1 Redis Service, 7 DTOs, 5 Endpoints, Migration, Seeder, Tests, Unity Docs)
