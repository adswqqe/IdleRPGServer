# Work Log: PVP Arena

> 이 문서는 PVP Arena 기능 구현 과정에서 발생한 작업 기록을 시간순으로 기록합니다.

---

## 2025-11-06 17:06

### Task Completed
- [x] 1.1 Create PvpSeason Entity

### Files Changed
- IdleRPG.Domain/Entities/PvpSeason.cs (new file, 58 lines)

### Key Decisions
- **BaseEntity<int> 상속**: PvpSeason은 마스터 데이터이므로 int 타입 Id 사용 (BaseEntity<int> 상속)
- **IsValid() 메서드**: 시즌 기간 검증 (StartDate < EndDate)을 Domain Entity에서 제공하여 비즈니스 규칙 명시
- **Navigation Properties 추가**: Rankings, Matches를 미리 정의 (EF Core 관계 설정 대비)

### Notes
- CreatedAt, UpdatedAt은 BaseEntity<int>에서 자동 제공
- IsActive 기본값은 false (시즌 생성 시 수동으로 활성화 필요)
- 활성 시즌은 최대 1개 제약은 Application Layer에서 검증 예정

---

## 2025-11-06 17:08

### Task Completed
- [x] 1.2 Create PvpRanking Entity

### Files Changed
- IdleRPG.Domain/Enums/PvpTier.cs (new file, 32 lines)
- IdleRPG.Domain/Entities/PvpRanking.cs (new file, 92 lines)

### Key Decisions
- **복합키 패턴**: (SeasonId, CharacterId) 복합키 사용으로 시즌별 캐릭터 랭킹 유일 식별 - BaseEntity 상속 안 함
- **Computed Property**: Tier는 Rating 기반 계산 속성 (get-only), PostgreSQL GENERATED COLUMN과 동기화 예정
- **PvpTier Enum**: Bronze(0-999), Silver(1000-1499), Gold(1500-1999), Platinum(2000-2499), Diamond(2500+)
- **기본값 설정**: Rating=1000 (초기 레이팅), Wins/Losses/WinStreak=0, IsRewardClaimed=false

### Notes
- 복합키는 EF Core Configuration에서 `HasKey(pr => new { pr.SeasonId, pr.CharacterId })`로 설정 예정
- LastMatchAt은 Nullable (첫 매칭 전에는 null)
- Tier는 PostgreSQL에서 GENERATED COLUMN (STORED)으로 구현 예정 (데이터 정합성 보장)

---

## 2025-11-06 17:13

### Task Completed
- [x] 1.3 Create PvpMatch Entity

### Files Changed
- IdleRPG.Domain/Entities/PvpMatch.cs (new file, 93 lines)

### Key Decisions
- **BaseEntity 상속**: PvpMatch는 Guid 타입 Id 사용 (사용자 생성 데이터, 유일 식별자)
- **다중 FK 패턴**: AttackerId, DefenderId, WinnerId가 모두 Character 테이블 참조 (자기참조 관계)
- **레이팅 변화 추적**: Before/After 레이팅 저장으로 ELO 계산 결과 감사 가능
- **IsValid() 메서드**: AttackerId != DefenderId, WinnerId는 둘 중 하나 검증

### Notes
- CreatedAt만 사용 (BaseEntity 제공), UpdatedAt 불필요 (Immutable 히스토리 데이터)
- Navigation Properties 4개: Season, Attacker, Defender, Winner
- EF Core Configuration에서 3개의 Character FK 관계 명시 필요 (다중 FK 충돌 방지)

---

## 2025-11-06 17:15

### Task Completed
- [x] 1.4 Create PvpTier Enum (completed in Task 1.2)
- [x] 1.5 Create PvpMatchResult Enum

### Files Changed
- IdleRPG.Domain/Enums/PvpMatchResult.cs (new file, 17 lines)

### Key Decisions
- **Victory/Defeat only**: 현재 전투 시스템이 무승부를 지원하지 않으므로 두 값만 정의
- **명시적 값 할당**: Victory=0, Defeat=1 (기본값 명시, Unity 클라이언트 호환성)

### Notes
- PvpTier Enum은 Task 1.2에서 PvpRanking Entity와 함께 생성됨 (의존성 해결)
- PvpMatchResult는 클라이언트 UI에서 승리/패배 표시 및 보상 분기 처리에 사용
- 향후 무승부(Draw) 추가 시 Enum 확장 필요 (WinnerId Nullable 변경 동반)

---

## 2025-11-06 17:22

### Task Completed
- [x] 1.6 Create EloRatingService Domain Service

### Files Changed
- IdleRPG.Domain/Services/EloRatingService.cs (new file, 98 lines)

### Key Decisions
- **K-Factor 관리 전략: Option A (하드코딩 32) 채택**
  - Reasoning: MVP 단계에서 K-Factor 변경 계획 없음, 단순성 및 테스트 용이성 우선
  - Future Plan: Phase 2에서 appsettings.json Configuration으로 이동 예정
- **Domain Service 패턴**: 외부 의존성 없는 순수 비즈니스 로직 (Repository, Infrastructure 의존 금지)
- **ELO 표준 알고리즘 구현**: 기대 승률 계산 → 레이팅 변화 계산 → 최소값 0 보장

### Notes
- CalculateNewRatings() 메서드: (winnerNewRating, loserNewRating) 튜플 반환
- Input Validation: 레이팅 음수 검증, K-Factor 양수 검증
- CalculateExpectedScore() private 메서드: ELO 표준 공식 `1 / (1 + 10^((R_B - R_A) / 400))`
- 단위 테스트 시나리오:
  - 동점 매칭 (1500 vs 1500) → 승자 +16, 패자 -16
  - 고랭커 vs 저랭커 (2000 vs 1000) → 승자 +3, 패자 -29
  - 저랭커 vs 고랭커 (1000 vs 2000) → 승자 +29, 패자 -3
  - 최소 레이팅 0 보장 (50 vs 1500, 패배 시 0으로 클램핑)

---

## 2025-11-06 17:28

### Task Completed
- [x] 2.1 Create IPvpSeasonRepository Interface

### Files Changed
- IdleRPG.Domain/Repositories/IPvpSeasonRepository.cs (new file, 52 lines)

### Key Decisions
- **Repository 인터페이스 위치**: Domain Layer에 정의 (의존성 역전 원칙 DIP)
- **메서드 시그니처**: CancellationToken 기본 매개변수로 추가 (비동기 취소 지원)
- **Nullable 반환**: 단일 엔티티 조회는 `PvpSeason?` 반환 (없을 수 있음)
- **5개 메서드 정의**: GetByIdAsync, GetActiveSeasonAsync, GetBySeasonNumberAsync, AddAsync, UpdateAsync

### Notes
- GetActiveSeasonAsync: IsActive = true 조회 (시스템 전체 최대 1개)
- GetBySeasonNumberAsync: SeasonNumber 유니크 제약 활용
- IRepository<T> 제네릭 인터페이스는 상속하지 않음 (PvpSeason은 마스터 데이터, 특화 메서드만 필요)
- 구현은 Task 2.2 (PvpSeasonRepository)에서 진행 예정

---

## 2025-11-06 17:33

### Task Completed
- [x] 2.2 Create PvpSeasonRepository Implementation

### Files Changed
- IdleRPG.Infrastructure/Repositories/PvpSeasonRepository.cs (new file, 68 lines)

### Key Decisions
- **AsNoTracking() 사용**: 모든 읽기 쿼리에 적용 (변경 추적 비활성화, 성능 최적화)
- **FirstOrDefaultAsync() 사용**: 단일 레코드 조회, 없으면 null 반환
- **CancellationToken 전달**: EF Core 메서드에 cancellationToken 전달 (HTTP 요청 취소 시 쿼리도 취소)
- **UpdateAsync 패턴**: _context.Update() 호출 후 await Task.CompletedTask (명시적 비동기 시그니처 유지)

### Notes
- GameDBContext 의존성 주입 (생성자 주입)
- GetActiveSeasonAsync: `WHERE IsActive = true` 쿼리 (인덱스 활용 예정)
- GetBySeasonNumberAsync: `WHERE SeasonNumber = ?` 쿼리 (Unique 인덱스 활용 예정)
- AddAsync/UpdateAsync는 DbContext.SaveChangesAsync() 호출 안 함 (UnitOfWork 패턴, Application Layer에서 트랜잭션 관리)

---

## 2025-11-06 17:35

### Task Completed
- [x] 2.3 Create IPvpRankingRepository Interface

### Files Changed
- IdleRPG.Domain/Repositories/IPvpRankingRepository.cs (new file, 73 lines)

### Key Decisions
- **복합키 조회**: GetByIdAsync는 (seasonId, characterId) 두 매개변수 사용
- **6개 메서드 정의**: GetByIdAsync, GetTopRankingsAsync, GetRankingsAroundAsync, GetByTierAsync, AddAsync, UpdateAsync
- **페이징 지원**: GetByTierAsync는 page/pageSize로 대용량 데이터 처리
- **List 반환**: 다중 레코드 조회는 `List<PvpRanking>` 반환 (빈 리스트 가능)

### Notes
- GetTopRankingsAsync: Redis 캐싱 미스 시 PostgreSQL Fallback 용도
- GetRankingsAroundAsync: 특정 레이팅 기준 ±range등 조회 (내 주변 랭킹)
- GetByTierAsync: 티어별 필터링 + 페이징 (PostgreSQL 전용, Redis 사용 안 함)
- 복합키 특성상 GetByIdAsync는 두 매개변수 필수 (SeasonId, CharacterId)
- 구현은 Task 2.4 (PvpRankingRepository)에서 진행 예정

---

## 2025-11-06 17:37

### Task Completed
- [x] 2.4 Create PvpRankingRepository Implementation

### Files Changed
- IdleRPG.Infrastructure/Repositories/PvpRankingRepository.cs (new file, 105 lines)

### Key Decisions
- **복합키 FindAsync 사용**: `FindAsync(new object[] { seasonId, characterId })` 패턴
- **Include(Character) 적용**: 모든 읽기 쿼리에 Character 포함 (N+1 문제 방지)
- **GetRankingsAroundAsync 로직**: 레이팅 범위 ±(range * 10) 계산 (range=10 → ±100 레이팅)
- **페이징 계산**: `Skip((page - 1) * pageSize).Take(pageSize)` (1-based 페이지)

### Notes
- GetByIdAsync: FindAsync는 Primary Key 기반 최적화 쿼리 (캐싱 활용)
- GetTopRankingsAsync: `OrderByDescending(Rating).Take(count)` + AsNoTracking()
- GetRankingsAroundAsync: 레이팅 범위 필터링 + Take(range * 2)로 ±range등 조회
- GetByTierAsync: Tier 필터링 + Skip/Take 페이징
- 모든 읽기 쿼리에 Include(Character) 추가 (캐릭터 이름 조회 필요)
- 복합 인덱스 `IX_PvpRanking_SeasonId_Rating_DESC` 활용 예정 (쿼리 성능 최적화)

---
