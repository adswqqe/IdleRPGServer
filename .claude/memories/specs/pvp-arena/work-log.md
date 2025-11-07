# Work Log: PVP Arena

> 이 문서는 PVP Arena 기능 개발 과정의 작업 로그입니다.

---

## 2025-11-07 17:23

### Task Completed
- [x] 4.5 Create PvpController - Season Endpoints

### Files Changed
- IdleRPG.API/Controllers/PvpController.cs (modified - 2개 엔드포인트 추가, +140 lines)

### Key Decisions
- **2개 엔드포인트 구현**:
  1. **GET /api/pvp/seasons/current** (Public, AllowAnonymous)
     - 현재 활성 시즌 조회
     - DaysRemaining 계산: (EndDate - DateTime.UtcNow).TotalDays
     - 404: 활성 시즌 없음
  2. **POST /api/pvp/seasons/{seasonId}/rewards** (인증 필수)
     - 시즌 보상 수령
     - PvpSeasonService.ClaimSeasonRewardAsync 호출
     - 400: 시즌 진행 중, 이미 수령함
     - 404: 시즌 없음, 랭킹 없음
- **IPvpSeasonService 의존성 추가**: PvpController에 주입
- **GetUserCharacterIdAsync 재사용**: ClaimSeasonReward에서 첫 번째 캐릭터 조회
- **예외 처리 전략**:
  - InvalidOperationException → 400 (시즌 진행 중, 중복 수령)
  - KeyNotFoundException → 404 (시즌 없음, 랭킹 없음)

### Notes
- **DaysRemaining 계산**: TotalDays를 int로 캐스팅 (소수점 버림)
- **AllowAnonymous**: 현재 시즌 조회는 Public (리더보드 표시 등에 사용)
- **Swagger 주석**: 티어별 보상 상세 명시 (Bronze: 100, Gold: 500 + 전설 상자 등)
- **API Layer 100% 완료**: 5개 엔드포인트 모두 구현 완료 🎉

---

## 2025-11-07 16:00

### Task Completed
- [x] 4.4 Create PvpController - Ranking Endpoints

### Files Changed
- IdleRPG.API/Controllers/PvpController.cs (modified - GetRankings 엔드포인트 추가, 250+ lines 추가)
- IdleRPG.Infrastructure/Repositories/PvpRankingRepository.cs (modified - ThenInclude Player 추가)

### Key Decisions
- **3가지 조회 모드 구현**:
  1. Top N (top 파라미터): Redis 우선 → PostgreSQL Fallback
  2. 내 주변 (nearMe=true, range): Redis 우선 → PostgreSQL Fallback, 인증 필수
  3. 티어별 (tier): PostgreSQL 직접 조회 (Redis는 복잡한 필터링 불가)
- **Redis Fallback 전략**:
  - **Top N**: Redis GetTopRankingsAsync → null이면 PostgreSQL GetTopRankingsAsync
  - **내 주변**: Redis GetRankingsAroundMeAsync + GetMyRankAsync → null이면 PostgreSQL GetRankingsAroundAsync (내 레이팅 기준)
  - **티어별**: PostgreSQL 직접 (Redis는 Enum 필터링 불가)
  - **Fallback 정책**: Graceful Degradation (Redis 장애 시 PostgreSQL로 대체, 성능 저하 허용)
- **Rank 계산 전략**:
  - Redis 성공 시: Rating DESC 정렬 순서로 1, 2, 3... 부여
  - PostgreSQL Fallback 시: Rating DESC 정렬 후 1, 2, 3... 부여 (근사값)
  - 티어별 조회: 페이지 오프셋 고려 (startRank = (page-1) * pageSize + 1)
- **N+1 문제 해결**: PvpRankingRepository에 ThenInclude(c => c.Player) 추가
  - GetByIdAsync: FindAsync → FirstOrDefaultAsync 변경 (Include 지원)
  - 모든 조회 메서드: Character.Player.UserName 조회 가능
- **AllowAnonymous 적용**: Top N, 티어별 조회는 Public, nearMe만 인증 필요

### Notes
- **Redis vs PostgreSQL 전략 근거 (Swagger 주석에 명시)**:
  - Top N: Redis Sorted Set (O(log N + count), 고속)
  - 내 주변: Redis ZREVRANK + ZREVRANGE (O(log N + 2*range))
  - 티어별: PostgreSQL Generated Column (복잡한 필터링, 페이징 필요)
- **GetUserCharacterIdAsync**: GetByPlayerIdAsync 사용 → FirstOrDefault()로 첫 번째 캐릭터 조회
- **Redis 장애 시 로깅**: LogWarning으로 Fallback 기록 (Redis miss 추적)
- **Helper 메서드 분리**: GetTopNRankingsAsync, GetRankingsAroundMeAsync, GetRankingsByTierAsync, CreateRankingDto

---

## 2025-11-07 15:45

### Task Completed
- [x] 4.3 Create PvpController - Match Endpoints

### Files Changed
- IdleRPG.API/Controllers/PvpController.cs (new file, 160 lines)
- IdleRPG.Infrastructure/Repositories/PvpMatchRepository.cs (modified - ThenInclude 추가)

### Key Decisions
- **BaseController 상속**: GetCurrentUserId() 공통 메서드 재사용
- **예외 처리 전략**:
  - UnauthorizedAccessException → 400 Bad Request (CharacterId 소유권 없음)
  - InvalidOperationException → 404 Not Found (활성 시즌 없음)
  - KeyNotFoundException → 404 Not Found (리소스 없음)
- **DTO 변환 로직**: Controller에서 Repository 결과를 PvpMatchHistoryDto로 변환
  - isAttacker 계산: m.AttackerId == request.CharacterId
  - isVictory 계산: m.WinnerId == request.CharacterId
  - 레이팅 변화량 계산: MyRatingAfter - MyRatingBefore
- **N+1 문제 해결**: PvpMatchRepository에 ThenInclude(c => c.Player) 추가
  - Attacker.Player, Defender.Player 조회 (UserName 표시용)
  - Season 조회 (SeasonNumber 표시용)
- **Pagination 응답 구조**: totalCount, page, pageSize, totalPages 포함

### Notes
- **Character.Name 없음**: Player.UserName 사용 (프로젝트 구조)
- **PvpMatch.Season**: 네비게이션 프로퍼티 이름이 PvpSeason이 아닌 Season
- **CreatedAtAction**: StartMatch에서 201 Created 응답 시 GetMatchHistory 링크 반환
- **Swagger 주석**: 모든 엔드포인트에 상세한 XML 문서화 주석 작성 (remarks, response codes)
- **로깅**: 중요 이벤트(매치 시작, 에러)에 대한 구조화된 로그 작성

---

## 2025-11-07 15:32

### Task Completed
- [x] 4.2 Create PVP Request Validators

### Files Changed
- IdleRPG.Application/Validators/PvpMatchRequestValidator.cs (new file)
- IdleRPG.Application/Validators/GetRankingsRequestValidator.cs (new file)
- IdleRPG.Application/Validators/GetMatchHistoryRequestValidator.cs (new file)
- IdleRPG.Application/DTOs/Pvp/GetRankingsRequest.cs (new file, 추가 생성)
- IdleRPG.Application/DTOs/Pvp/GetMatchHistoryRequest.cs (new file, 추가 생성)

### Key Decisions
- **Query Parameter용 Request DTO 생성**: ASP.NET Core 모범 사례에 따라 FromQuery 바인딩을 위한 DTO 추가 생성
- **FluentValidation 통합**: AbstractValidator<T> 상속으로 선언적 검증 규칙 작성
- **조건부 검증 패턴 적용**: When(x => x.HasValue)로 nullable 필드 처리
- **BeValidGuid 재사용**: PvpMatchRequestValidator와 GetMatchHistoryRequestValidator에서 공통 검증 로직 사용 (Guid.Empty 체크)
- **검증 범위 결정**:
  - Top: 1~1000 (대규모 랭킹 조회 지원)
  - Range: 1~50 (내 주변 랭킹 조회)
  - PageSize: 1~50 (히스토리), 1~100 (랭킹)

### Notes
- GetRankingsRequest와 GetMatchHistoryRequest는 Task 4.2에 명시되지 않았지만 Validator 작성을 위해 필수 생성
- 모든 Validator에 명확한 에러 메시지 작성 (WithMessage())
- PvpTier Enum 검증: IsInEnum() 사용
- Validators 폴더 신규 생성 (프로젝트 최초 Validator)

---

## 2025-11-07 15:29

### Task Completed
- [x] 4.1 Create PVP Request/Response DTOs

### Files Changed
- IdleRPG.Application/DTOs/Pvp/PvpMatchRequestDto.cs (new file)
- IdleRPG.Application/DTOs/Pvp/PvpMatchResponseDto.cs (이미 완성)
- IdleRPG.Application/DTOs/Pvp/MatchOpponentDto.cs (이미 완성, = OpponentDto)
- IdleRPG.Application/DTOs/Pvp/PvpRankingDto.cs (new file)
- IdleRPG.Application/DTOs/Pvp/PvpMatchHistoryDto.cs (new file)
- IdleRPG.Application/DTOs/Pvp/PvpSeasonDto.cs (new file)
- IdleRPG.Application/DTOs/Pvp/SeasonRewardDto.cs (이미 완성)

### Key Decisions
- **DTO 7개 모두 완성**: Request 1개, Response 6개
- **XML 문서화 주석 전체 작성**: Swagger UI 자동 생성 및 클라이언트 개발 가이드 제공
- **WinRate 계산 로직 명시**: (Wins / (Wins + Losses)) * 100, 전적 없으면 0
- **DaysRemaining 계산 로직 명시**: (EndDate - DateTime.UtcNow).Days, 음수면 시즌 종료됨
- **PvpMatchResponseDto에 CombatLog 포함**: 향후 리플레이 시스템 확장 고려 (Phase 3)

### Notes
- PvpMatchResponseDto와 SeasonRewardDto는 이미 완성되어 있었음 (3.7 Task에서 생성)
- MatchOpponentDto는 OpponentDto 역할 (이름만 다름, Task 명세와 일치)
- 모든 DTO에 example 값 추가 (Swagger UI 문서 품질 향상)
- 복잡한 타입(Dictionary, Enum)에 대한 remarks 상세 설명 추가

---

## 2025-11-07 11:37

### Task Completed
- [x] 2.5 Create IPvpMatchRepository Interface

### Files Changed
- IdleRPG.Domain/Repositories/IPvpMatchRepository.cs (new file)

### Key Decisions
- GetMatchHistoryAsync에서 seasonId를 nullable로 설계 (전체 시즌 조회 지원)
- 페이징 지원을 위해 튜플 반환 (List<PvpMatch> matches, int totalCount) 채택
- 기존 Repository 패턴과 일관성 유지: XML 문서화 주석, CancellationToken 기본값 default

### Notes
- PvpMatch는 Attacker/Defender 양방향 조회가 필요하므로 GetMatchHistoryAsync에서 OR 조건 필요 (구현은 2.6에서)
- totalCount는 페이징 UI 구현을 위해 필수 (Unity 클라이언트에서 전체 페이지 수 계산)

---

## 2025-11-07 11:39

### Task Completed
- [x] 2.6 Create PvpMatchRepository Implementation

### Files Changed
- IdleRPG.Infrastructure/Repositories/PvpMatchRepository.cs (new file)

### Key Decisions
- OR 조건 구현: WHERE (AttackerId = ? OR DefenderId = ?) - Attacker/Defender 양방향 조회
- seasonId nullable 처리: if (seasonId.HasValue) - LINQ에서 동적 필터링
- Include() 2회 호출: Attacker, Defender 네비게이션 프로퍼티 Eager Loading (N+1 방지)
- totalCount 먼저 조회 후 페이징 쿼리 실행 (2번의 쿼리, 성능 고려)

### Notes
- EF Core는 OR 조건을 WHERE절에서 자동 최적화
- Winner 네비게이션은 GetByIdAsync에서만 Include (히스토리 조회 시 불필요)
- CreatedAt 내림차순 정렬 (최신 매치 먼저 표시)

---

## 2025-11-07 11:45

### Task Completed
- [x] 2.7 Create PvpSeason EF Core Configuration

### Files Changed
- IdleRPG.Infrastructure/Configurations/PvpSeasonConfiguration.cs (new file, 74 lines)

### Key Decisions
- Table name: "PvpSeason" (단수형, 도메인 모델명과 일치)
- Primary Key: Id (int, ValueGeneratedOnAdd - auto-increment)
- SeasonNumber: Unique Index (중복 방지)
- IsActive: Default false (새 시즌은 비활성 상태로 생성)
- Timestamp 컬럼 타입: "timestamp without time zone" (PostgreSQL 표준)
- Navigation OnDelete: Restrict (시즌 삭제 시 랭킹 및 매치 기록 보존)

### Notes
- IX_PvpSeason_IsActive: 활성 시즌 조회 성능 최적화 (WHERE IsActive = true 빈번)
- IX_PvpSeason_SeasonNumber (Unique): 시즌 번호 중복 방지 및 검색 최적화
- CreatedAt/UpdatedAt: BaseEntity에서 상속받지만 명시적 설정 (CURRENT_TIMESTAMP 기본값)
- OnDelete Restrict: 시즌 데이터는 히스토리 보존을 위해 랭킹/매치 삭제 방지

---

## 2025-11-07 11:52

### Task Completed
- [x] 2.8 Create PvpRanking EF Core Configuration

### Files Changed
- IdleRPG.Infrastructure/Configurations/PvpRankingConfiguration.cs (new file, 93 lines)

### Key Decisions (TODO(human) 해소 - 재검토 후 수정)
- **Composite Primary Key**: (SeasonId, CharacterId) - 시즌별 캐릭터 당 하나의 랭킹 레코드
- **Tier 저장 전략**: HasConversion<string>() + ValueGeneratedOnAddOrUpdate()
  - Rating 변경 시 Tier 자동 갱신 (DB Computed Column과 유사)
  - 쿼리 필터링 가능 (WHERE Tier = 'Gold')
- **OnDelete 정책 (최종 결정)**:
  - **SeasonId → Restrict**: 시즌은 히스토리 마스터 데이터, 과거 랭킹 기록 보존 필요
    - 과거 시즌 랭킹은 명예의 전당, 통계 분석에 필수
    - 관리자 실수로 시즌 삭제 시 수천 개 레코드 손실 방지
  - **CharacterId → Cascade**: GDPR 준수, 캐릭터 삭제 시 개인정보 종속 데이터 완전 제거
    - PvpRanking은 CharacterId 없이 조회/표시 불가능 (복합키 구조)
- **복합 인덱스**: IX_PvpRanking_SeasonId_Rating_DESC
  - SeasonId ASC, Rating DESC (ORDER BY Rating DESC 최적화)
  - 시즌별 Top 랭킹 조회 쿼리에 특화

### Notes
- **학습 과정**: 초기 "양쪽 모두 Cascade" 결정 → 비판적 재검토 → SeasonId를 Restrict로 수정
- **판단 기준 학습**: 마스터 데이터(Restrict) vs 개인정보 종속(Cascade) 구분의 중요성
- ValueGeneratedOnAddOrUpdate: EF Core가 INSERT/UPDATE 시 Tier 값을 자동 계산
- IsDescending(false, true): 첫 번째 컬럼 ASC, 두 번째 컬럼 DESC
- **학습 포인트**: 복합키 설정, Enum 저장 전략, OnDelete 정책 결정 과정 + 자기 결정 검증

---

## 2025-11-07 12:01

### Task Completed
- [x] 2.9 Create PvpMatch EF Core Configuration
- [x] 추가: GameDBContext에 PVP Arena DbSet 등록

### Files Changed
- IdleRPG.Infrastructure/Configurations/PvpMatchConfiguration.cs (new file, 106 lines)
- IdleRPG.Infrastructure/Data/GameDBContext.cs (modified: DbSet 3개 추가)

### Key Decisions
- **다중 FK 관계 (4개)**: SeasonId, AttackerId, DefenderId, WinnerId
  - EF Core는 WithMany()로 역방향 네비게이션 없는 단방향 관계 구분
  - Character 엔티티에 PvpMatch 역방향 컬렉션 없음 (단방향 설계)
- **모두 Restrict 채택**:
  - **SeasonId → Restrict**: PvpMatch는 히스토리 데이터, 시즌 삭제 시에도 매치 기록 보존
  - **AttackerId/DefenderId/WinnerId → Restrict**: 캐릭터 삭제 시에도 과거 매치 기록 보존
  - **근거**: PvpMatch는 Immutable 히스토리 데이터, 통계/분석에 필수
- **복합 인덱스 전략**:
  - `IX_PvpMatch_AttackerId_CreatedAt`: 공격자 히스토리 조회 (WHERE AttackerId = ? ORDER BY CreatedAt DESC)
  - `IX_PvpMatch_DefenderId_CreatedAt`: 방어자 히스토리 조회 (WHERE DefenderId = ? ORDER BY CreatedAt DESC)
  - Repository의 GetMatchHistoryAsync는 OR 조건으로 양쪽 인덱스 활용
- **GameDBContext 업데이트**:
  - DbSet<PvpSeason>, DbSet<PvpRanking>, DbSet<PvpMatch> 추가
  - ApplyConfigurationsFromAssembly()가 자동으로 3개 Configuration 적용

### Notes
- **PvpMatch vs PvpRanking 차이점**:
  - PvpMatch: Immutable 히스토리, 모든 FK → Restrict (영구 보존)
  - PvpRanking: Mutable 집계 데이터, CharacterId → Cascade (개인정보 삭제)
- **다중 FK 처리**: 같은 엔티티(Character)에 대한 3개의 관계를 WithMany()로 구분
- **인덱스 컬럼 순서**: 필터링 컬럼(CharacterId) → 정렬 컬럼(CreatedAt)
- **Configuration 자동 적용**: OnModelCreating의 ApplyConfigurationsFromAssembly()가 새 Configuration 자동 인식
- **학습 포인트**: 히스토리 데이터의 OnDelete 정책, 다중 FK 설정, 복합 인덱스 설계, DbSet 등록 필수성

---

## 2025-11-07 14:25

### Task Completed
- [x] 3.1 Create IRedisCacheService Interface

### Files Changed
- IdleRPG.Application/Services/IRedisCacheService.cs (new file, 95 lines)

### Key Decisions
- **Redis Sorted Set 활용**: Key = "pvp:ranking:season:{seasonId}", Score = Rating, Member = CharacterId
- **Write-Through 전략**: PostgreSQL 업데이트 성공 후 Redis 갱신 (트랜잭션 외부, Best Effort)
- **Fallback 설계**: Redis 장애 시 null 반환 → Controller에서 PostgreSQL 직접 조회
- **메서드 5개 정의**:
  1. `UpdateRankingCacheAsync`: ZADD (레이팅 갱신, O(log N))
  2. `GetTopRankingsAsync`: ZREVRANGE (Top N 조회, O(log N + count))
  3. `GetMyRankAsync`: ZREVRANK (내 순위 조회, O(log N), 1-based)
  4. `GetRankingsAroundMeAsync`: ZREVRANK + ZREVRANGE (내 주변 ±range 조회)
  5. `ClearRankingCacheAsync`: DEL (시즌 종료 시 캐시 초기화, O(1))
- **반환 타입**: Dictionary<Guid, int>? (실패/데이터 없음 시 null)

### Notes
- Redis 명령어 시간 복잡도를 XML 주석에 명시 (성능 예측 가능)
- GetMyRankAsync는 0-based → 1-based 변환 필요 (1등 = 1)
- GetRankingsAroundMeAsync는 2번의 Redis 명령 실행 (ZREVRANK → ZREVRANGE)
- Redis 예외는 상위 계층(Infrastructure)에서 처리, Application은 인터페이스만 의존 (DIP)
- **학습 포인트**: Redis Sorted Set 자료구조, Write-Through 캐싱 전략, Fallback 설계

---

## 2025-11-07 14:35

### Task Completed
- [x] 3.2 Create RedisCacheService Implementation

### Files Changed
- IdleRPG.Infrastructure/Services/RedisCacheService.cs (new file, 213 lines)
- IdleRPG.Infrastructure/IdleRPG.Infrastructure.csproj (modified: StackExchange.Redis 2.9.32 추가)

### Key Decisions
- **TODO(human) 해소 - Best Effort 전략 채택**:
  - **Rollback 전략 (기각)**: Redis 실패 시 PostgreSQL 트랜잭션 롤백 → 전체 매치 실패, 가용성 저하
  - **Best Effort 전략 (채택)**: Redis 실패 시 로깅만 하고 계속 진행 → 성능만 저하, 시스템 계속 작동
  - **근거**: PostgreSQL이 Source of Truth, Redis는 캐시 (성능 최적화용)
  - **Graceful Degradation**: Redis 장애 시에도 PostgreSQL Fallback으로 시스템 가용성 유지
- **IConnectionMultiplexer DI**: Singleton으로 등록 (StackExchange.Redis 권장)
- **GetRankingKey() 헬퍼**: Key 형식 통일 (`pvp:ranking:season:{seasonId}`)
- **0-based → 1-based 변환**: GetMyRankAsync에서 rank.Value + 1 (1등 = 1)
- **GetRankingsAroundMeAsync 2단계**:
  1. ZREVRANK로 내 순위 조회 (없으면 null 반환)
  2. ZREVRANGE로 내 순위 ± range 조회 (startRank = Max(0, myRank - range))
- **에러 처리 일관성**: 모든 메서드에서 RedisException catch → LogWarning + null/throw

### Notes
- **StackExchange.Redis 2.9.32 설치**: NuGet 패키지 추가 완료
- **SortedSetRangeByRankWithScoresAsync**: ZREVRANGE WITHSCORES 명령 (레이팅 값도 함께 조회)
- **Order.Descending**: Redis ZREVRANGE (높은 Score → 낮은 Score, 레이팅 내림차순)
- **Guid.TryParse**: Redis에 저장된 characterId 문자열을 Guid로 파싱 (실패 시 무시)
- **UpdateRankingCacheAsync throw**: 트랜잭션 외부에서 호출되므로 예외를 throw (상위에서 처리)
- **ClearRankingCacheAsync throw**: 시즌 관리 로직에서 Redis 장애 인지 필요
- **로깅 레벨**: Debug (성공), Warning (실패), Information (중요 작업)
- **학습 포인트**: Best Effort 전략, Graceful Degradation, Redis Sorted Set 명령어, 0-based/1-based 변환

---

## 2025-11-07 14:50

### Task Completed
- [x] 3.3 Create IPvpMatchmakingService Interface

### Files Changed
- IdleRPG.Application/Services/IPvpMatchmakingService.cs (new file, 55 lines)
- IdleRPG.Application/DTOs/Pvp/MatchOpponentDto.cs (new file, 42 lines)

### Key Decisions
- **매칭 범위**: ±200 레이팅 (Design 문서 준수)
- **후보 제한**: 최대 100명 (성능 고려)
- **NPC 봇 생성 조건**: 후보 없을 시 (타임아웃 대응)
- **NPC 봇 스펙**:
  - CharacterId: Guid.Empty (봇 식별용)
  - Name: "Bot_1234" 형식 (Random(1000, 9999))
  - Rating: 내 레이팅 ± Random(-100, 100)
  - IsBot: true
- **첫 매칭 처리**: 초기 레이팅 1000으로 PvpRanking 자동 생성
- **MatchOpponentDto 4개 필드**: CharacterId, Name, Rating, IsBot

### Notes
- Application Service 채택 (Repository 의존, 외부 데이터 조회 필요)
- XML 문서화 주석에 매칭 프로세스, NPC 봇 생성 규칙, 성능 목표 명시
- MatchOpponentDto는 Pvp 폴더에 생성 (DTO 조직화)
- IsBot 플래그로 실제 플레이어와 NPC 봇 구분
- 매칭 타임아웃 30초는 구현 단계에서 처리 (3.4에서)
- **학습 포인트**: Application Service vs Domain Service 구분, 매칭 알고리즘 설계

---

## 2025-11-07 15:05

### Task Completed
- [x] 3.4 Create PvpMatchmakingService Implementation

### Files Changed
- IdleRPG.Infrastructure/Services/PvpMatchmakingService.cs (new file, 176 lines)

### Key Decisions
- **TODO(human) 해소 - 즉시 NPC 봇 생성 전략 채택**:
  - **동기 대기 30초 (기각)**: HTTP 타임아웃 위험, 사용자 대기 시간 길어짐
  - **즉시 NPC 봇 생성 (채택)**: 후보 없으면 즉시 봇 반환, 대기 0초, 사용자 경험 우선
  - **근거**: MVP 단순화, HTTP 타임아웃 회피, 항상 매칭 성공 보장
  - **Phase 3 개선 계획**: Redis Queue + 백그라운드 워커로 비동기 매칭 도입
- **상수 정의**: InitialRating(1000), MatchingRange(200), MaxCandidates(100), BotRatingVariance(100)
- **첫 매칭 처리**: PvpRanking 없으면 초기 레이팅 1000 생성 후 SaveChanges
- **후보 조회 로직**: GetRankingsAroundAsync(seasonId, myRating, ±200) → 자기 자신 제외 → 최대 100명
- **랜덤 선택**: _randomProvider.Next(candidates.Count)
- **NPC 봇 생성 헬퍼**: CreateNpcBot(myRating) 메서드 분리
  - 레이팅: myRating + Random(-100, 100), 최소 0 보장
  - 이름: "Bot_" + Random(1000, 9999)
  - CharacterId: Guid.Empty (봇 식별용)

### Notes
- **Infrastructure Layer 배치**: 기존 프로젝트 패턴 준수 (Application Service 구현체)
- **UnitOfWork 패턴**: IUnitOfWork로 Repository 접근 (트랜잭션 관리)
- **IRandomProvider 활용**: 테스트 가능성 확보 (Mock 가능)
- **자기 자신 제외**: candidates.Where(r => r.CharacterId != characterId)
- **안전장치**: 후보 캐릭터 정보 없으면 NPC 봇으로 대체 (이론적으로 발생하지 않지만)
- **로깅**: Debug (후보 조회), Information (매칭 성공/봇 생성), Warning (에러)
- **에러 처리**: 캐릭터 없으면 KeyNotFoundException
- **학습 포인트**: 동기 vs 비동기 매칭 전략, MVP 단순화 원칙, 사용자 경험 우선 설계

---

## 2025-11-07 14:31

### Task Completed
- [x] 3.5 Create IPvpSeasonService Interface

### Files Changed
- IdleRPG.Application/DTOs/Pvp/SeasonRewardDto.cs (new file, 63 lines)
- IdleRPG.Application/Services/IPvpSeasonService.cs (new file, 75 lines)

### Key Decisions
- **시즌 보상 지급 메서드**: ClaimSeasonRewardAsync
  - 시즌 종료 확인 (IsActive = false) 필수
  - 중복 수령 방지 (IsRewardClaimed = false)
  - 티어별 차등 보상 (Bronze: Crystal 100 ~ Diamond: Crystal 2000 + 신화 상자)
  - 트랜잭션: Character 보상 지급 + PvpRanking.IsRewardClaimed = true
- **새 시즌 시작 메서드**: StartNewSeasonAsync
  - 기존 활성 시즌 비활성화
  - Soft Reset 적용: 새 레이팅 = (기존 레이팅 + 1000) / 2
  - Redis 랭킹 캐시 초기화
  - 트랜잭션: 기존 시즌 비활성화 + 새 시즌 생성 + 전체 PvpRanking Soft Reset
- **SeasonRewardDto 설계**:
  - SeasonNumber, Tier, FinalRating, FinalRank 포함
  - Rewards는 Dictionary<string, int> (아이템 이름 → 수량)
  - AlreadyClaimed 플래그로 중복 수령 여부 표시

### Notes
- XML 문서화 주석에 비즈니스 규칙, 트랜잭션 경계, 예외 타입 상세 명시
- 티어별 보상 테이블은 AI가 제공 (학습자는 게임 밸런스 고민 없이 구조 학습 집중)
- Soft Reset 공식 예시 명시 (2000점 → 1500점, 800점 → 900점)
- StartNewSeasonAsync는 관리자/스케줄러가 호출 (사용자 직접 호출 아님)
- ClaimSeasonRewardAsync는 사용자가 보상 수령 버튼 클릭 시 호출
- **학습 포인트**: Application Service Interface 설계, 트랜잭션 경계 명시, 비즈니스 규칙 문서화

---

## 2025-11-07 14:52

### Task Completed
- [x] 3.6 Create PvpSeasonService Implementation

### Files Changed
- IdleRPG.Application/Services/PvpSeasonService.cs (new file, 270 lines)

### Key Decisions
- **TODO(human) 해소 - Soft Reset vs Hard Reset 전략**:
  - **Soft Reset 채택**: 새 레이팅 = (기존 레이팅 + 1000) / 2
  - **근거**: 고랭커 유지로 플레이어 경험 개선, 시즌마다 완전 초기화는 동기 저하 우려
  - **예시**: 2000점 → 1500점, 800점 → 900점
- **ClaimSeasonRewardAsync 구현**:
  - 시즌 종료 확인 (IsActive = false)
  - 중복 수령 방지 (IsRewardClaimed = false)
  - 티어별 보상 계산 헬퍼 메서드 분리 (CalculateTierRewards)
  - 트랜잭션: Character 보상 지급 + PvpRanking.IsRewardClaimed = true
  - Redis → PostgreSQL Fallback으로 최종 순위 조회 (GetFinalRankAsync)
- **StartNewSeasonAsync 구현**:
  - 기존 활성 시즌 비활성화
  - 새 시즌 생성
  - 첫 시즌 vs 이후 시즌 분기:
    - 첫 시즌 (activeSeason == null): Soft Reset 없음, 플레이어가 첫 매치 시 초기 레이팅 1000 생성
    - 이후 시즌: foreach로 모든 플레이어 랭킹 복사 + Soft Reset 적용
  - Redis 캐시 초기화 (Best Effort)
- **foreach + AddAsync vs AddRangeAsync 선택**:
  - **foreach 채택**: Repository에 AddRangeAsync 추가하지 않음 (YAGNI 원칙)
  - **성능 문제 없음**: AddAsync는 메모리 작업, SaveChangesAsync가 일괄 INSERT
- **학습자 버그 발견 및 수정**:
  - `await _unitOfWork.PvpRankings.AddAsync(new PvpRanking(), ...)` → `AddAsync(newRanking, ...)`
  - 변수명 개선: `beSeason` → `previousSeasonRankings`
  - 로깅 추가: Soft Reset 플레이어 수, 첫 시즌 안내

### Notes
- **Application Layer 배치**: Clean Architecture에서 Application Service 구현체는 Application Layer에 배치
  - Infrastructure Layer는 외부 시스템 연동 (RedisCacheService, PvpMatchmakingService)
  - Application Layer는 비즈니스 흐름 오케스트레이션 (PvpSeasonService)
- **트랜잭션 경계**: Service 계층에서 UnitOfWork로 여러 Repository 작업을 단일 트랜잭션으로 관리
- **장비 상자 보상**: 현재 시스템 미구현으로 로그만 기록 (TODO 주석 추가)
- **Redis 장애 처리**: Best Effort 전략 (실패 시 로그만, 트랜잭션은 계속 진행)
- **학습 포인트**:
  - foreach 루프로 컬렉션 순회
  - 조건부 로직 (첫 시즌 vs 이후 시즌)
  - 트랜잭션 경계 설정 (Service 계층)
  - EF Core Change Tracker 동작 방식 (AddAsync는 메모리 작업)
  - YAGNI 원칙 (필요 없는 AddRangeAsync 추가하지 않음)

---

## 2025-11-07 14:59

### Task Completed
- [x] 3.7 Create IPvpService Interface & Implementation

### Files Changed
- IdleRPG.Application/Services/IPvpService.cs (new file, 96 lines)
- IdleRPG.Application/Services/PvpService.cs (new file, 312 lines)
- IdleRPG.Application/DTOs/Pvp/PvpMatchResponseDto.cs (new file, 83 lines)

### Key Decisions
- **TODO(human) 해소 - 트랜잭션 경계 설정 근거**:
  - **Service 계층에서 UnitOfWork로 트랜잭션 관리 채택**
  - **근거**: 여러 Repository 작업(PvpMatch 생성, PvpRanking 업데이트 x2, Character 보상)을 하나의 원자적 단위로 묶어야 함
  - **Redis는 트랜잭션 외부**: Best Effort, Redis 실패가 전체 매치를 롤백시키면 안 됨 (Graceful Degradation)
  - **PostgreSQL이 Source of Truth**: Redis는 성능 최적화용 캐시, 실패해도 PostgreSQL에서 조회 가능
- **전투 시뮬레이션 방식**:
  - CombatService 재사용 불가 (Character vs Monster 전용)
  - **간단한 전투력 비교 로직 구현**: 공격력 + 방어력 + HP/10 + 랜덤(±10%)
  - Phase 2에서 CombatService와 통합 예정
- **NPC 봇 처리**:
  - DefenderId: NPC 봇은 Guid.Empty 저장
  - 전투력: 레이팅 = 전투력 (간단한 공식)
  - 랭킹 업데이트: IsBot = false인 경우만 상대 랭킹 업데이트
- **보상 계산 (AI 제공)**:
  - 승리: Gold 100 + Crystal 10 + Experience 50
  - 패배: Gold 50 + Experience 25
- **에러 처리**:
  - UnauthorizedAccessException: CharacterId 소유권 없음
  - InvalidOperationException: 활성 시즌 없음
  - KeyNotFoundException: 캐릭터 또는 상대 정보 없음

### Notes
- **Service 오케스트레이션**: PvpService는 Facade 패턴으로 여러 서비스 조율
  - PvpMatchmakingService: 상대 찾기
  - EloRatingService: 레이팅 계산
  - UnitOfWork: 트랜잭션 관리
  - RedisCacheService: 랭킹 갱신
- **Application Layer 배치**: 비즈니스 흐름 오케스트레이션은 Application Layer
- **트랜잭션 흐름**:
  1. PvpMatch 생성 (매치 기록)
  2. PvpRanking 업데이트 (공격자/방어자, IsBot = false만)
  3. Character 보상 지급 (승리/패배 차등)
  4. SaveChangesAsync() 커밋
  5. Redis 갱신 (트랜잭션 외부, Best Effort)
- **CombatLog**: 간단한 전투 로그 생성 (향후 리플레이 시스템 확장 가능)
- **상대 랭킹 조회**: NPC 봇이 아닌 경우에만 DB에서 조회 및 업데이트
- **학습 포인트**:
  - Service 오케스트레이션 패턴
  - 트랜잭션 경계 설정 (여러 Repository 작업을 하나로)
  - Best Effort 전략 (Redis 실패 허용)
  - 에러 처리 및 로깅
  - DTO 설계 (PvpMatchResponseDto)

---
