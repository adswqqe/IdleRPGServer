# Work Log: PVP Arena

> 이 문서는 PVP Arena 기능 개발 과정의 작업 로그입니다.

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
