# Requirements: PVP Arena

> 이 문서는 PVP Arena 기능의 요구사항을 정의합니다.
>
> **작성 방식**: 학습 모드 (대화형 질문 12개를 통해 백엔드 아키텍처 학습)

**대화로 결정한 내용 (L 사이즈)**:
```
Phase 1: 데이터 모델링 (매치 구조, 랭킹 저장, 시즌 관리)
Phase 2: 아키텍처 계층 (ELO 계산, 매칭 로직, Redis 캐싱)
Phase 3: 데이터베이스 설계 (인덱스 전략, 쿼리 최적화, PK 타입)
Phase 4: API 설계 (RESTful 엔드포인트, 페이징 전략)
Phase 5: 게임 밸런스 (AI 제안 수용)
```

---

## 📋 Feature Overview

### 목적
플레이어 간 실시간 PVP 경쟁 시스템을 구현하여 게임의 경쟁적 요소를 강화합니다.

**핵심 가치**:
- ELO 레이팅 기반 공정한 매칭
- Redis 캐싱을 활용한 실시간 랭킹
- 시즌제를 통한 장기 운영

### 성공 기준
- [x] ELO 매칭 알고리즘 구현 (±200 레이팅 범위)
- [x] Redis Sorted Set 기반 랭킹 시스템
- [x] 시즌별 랭킹 분리 및 아카이빙 가능한 구조
- [x] 매치 히스토리 조회 API
- [x] RESTful API 설계 (서버 권위 원칙)

---

## 👤 User Stories

### US-1: PVP 매칭 및 전투
**As a** 플레이어
**I want** 비슷한 실력의 상대와 자동으로 매칭되어 PVP 전투를 하고 싶다
**So that** 공정한 경쟁을 통해 실력을 향상시키고 랭킹을 올릴 수 있다

**Acceptance Criteria (EARS 형식):**
- **WHEN** 플레이어가 매칭 시작을 요청 **THEN** 시스템은 ±200 레이팅 범위 내에서 상대를 찾는다
- **IF** 30초 내에 상대를 찾지 못하면 **THEN** 시스템은 NPC 봇을 매칭한다 (내 레이팅 ±100, 전투력 80%)
- **WHEN** 매칭이 완료되면 **THEN** 시스템은 서버에서 전투를 시뮬레이션하고 결과를 반환한다
- **WHEN** 전투가 종료되면 **THEN** 시스템은 ELO 알고리즘으로 레이팅을 계산하고 PostgreSQL + Redis를 모두 업데이트한다

### US-2: 랭킹 조회
**As a** 플레이어
**I want** 현재 시즌의 Top 100 랭킹과 내 주변 랭킹을 조회하고 싶다
**So that** 내 위치를 파악하고 목표를 설정할 수 있다

**Acceptance Criteria (EARS 형식):**
- **WHEN** Top 100 랭킹 조회 요청 **THEN** 시스템은 Redis에서 O(log N) 시간에 조회한다
- **WHEN** "내 주변 ±10등" 조회 요청 **THEN** 시스템은 내 순위를 기준으로 앞뒤 10명을 반환한다
- **WHEN** 특정 시즌의 랭킹 조회 **THEN** 시스템은 PostgreSQL에서 SeasonId 필터링하여 조회한다
- **IF** Redis 장애 발생 **THEN** 시스템은 PostgreSQL에서 조회한다 (Fallback)

### US-3: 전적 조회
**As a** 플레이어
**I want** 내 PVP 전적(승패, 레이팅 변화)을 조회하고 싶다
**So that** 과거 전투를 복기하고 전략을 개선할 수 있다

**Acceptance Criteria (EARS 형식):**
- **WHEN** 전적 조회 요청 (최근 20경기) **THEN** 시스템은 Offset 기반 페이징으로 반환한다
- **WHEN** 특정 시즌 필터링 **THEN** 시스템은 해당 시즌의 매치만 조회한다
- **WHILE** 매치가 진행 중 **THEN** 시스템은 실시간으로 전적에 반영한다

### US-4: 시즌 종료 및 보상
**As a** 플레이어
**I want** 시즌이 종료되면 최종 티어에 따른 보상을 받고 싶다
**So that** 경쟁에 대한 동기를 유지할 수 있다

**Acceptance Criteria (EARS 형식):**
- **WHEN** 시즌이 종료되면 **THEN** 시스템은 최종 레이팅 기준으로 티어를 산정한다
- **WHEN** 플레이어가 보상 수령 요청 **THEN** 시스템은 티어별 보상(Crystal, 장비 상자)을 지급한다
- **WHEN** 새 시즌이 시작되면 **THEN** 시스템은 레이팅 Soft Reset을 적용한다 (기존 레이팅 + 1000) / 2

---

## 🏗️ Technical Requirements (대화로 결정)

> ⚠️ **백엔드 학습 초점**: 이 섹션은 `/spec-init` 대화 과정에서 결정되었습니다 (12개 질문)

### 데이터 모델 (Data Model)

**엔티티 관계** (Q1-Q3 결정):
- **PvpMatch (1:1) 구조**: AttackerId, DefenderId 단일 테이블 (1:1 매치 고정)
- **리플레이 기능**: 향후 확장 가능하도록 설계 (현재 구현 제외, 전투 로직에서 턴별 로그 생성 구조 미리 준비)
- **PvpRanking**: Redis Sorted Set + PostgreSQL 이중 저장 (Write-Through 전략)
- **시즌 관리**: PvpSeason + 시즌별 PvpRanking (SeasonId, CharacterId 복합키)

**새 Entity**:
- `PvpMatch`: 매치 기록 저장
  - `Id` (uuid, PK)
  - `AttackerId`, `DefenderId`, `WinnerId` (FK → Character)
  - `AttackerRatingBefore/After`, `DefenderRatingBefore/After` (int)
  - `CreatedAt` (timestamp)

- `PvpRanking`: 시즌별 랭킹 정보
  - `SeasonId` (int, FK → PvpSeason)
  - `CharacterId` (uuid, FK → Character)
  - `Rating`, `Wins`, `Losses`, `WinStreak` (int)
  - `Tier` (enum: Bronze, Silver, Gold, Platinum, Diamond)
  - `LastMatchAt` (timestamp)
  - **PRIMARY KEY** (SeasonId, CharacterId)

- `PvpSeason`: 시즌 마스터 데이터
  - `Id` (int, PK, auto-increment)
  - `SeasonNumber` (int)
  - `StartDate`, `EndDate` (timestamp)
  - `IsActive` (bool)

**확장성 고려**:
- 나중에 데이터가 커지면 PvpRanking을 아카이빙 전략 (선택지 B)로 마이그레이션 가능
- 리플레이 기능 추가 시 BattleTurnLog 테이블 추가 가능

---

### 아키텍처 계층 (Architecture Layers)

**로직 배치** (Q4-Q6 결정):

**Domain Service** (순수 비즈니스 로직):
- `EloRatingService`: ELO 레이팅 계산
  ```csharp
  public (int winnerNewRating, int loserNewRating) CalculateNewRatings(
      int winnerRating, int loserRating, int kFactor = 32)
  ```
  - 외부 의존성 없음 (DB, API 호출 없음)
  - 단위 테스트 용이 (Mock 불필요)
  - 재사용 가능 (다른 경쟁 시스템에서도 사용)

**Application Service** (유스케이스 조율):
- `PvpMatchmakingService`: 매칭 로직
  - Repository 호출하여 ±200 레이팅 범위 후보 조회
  - 랜덤 선택 및 타임아웃 처리 (30초)
- `PvpService`: 매치 전체 흐름 관리
  - 매칭 → 전투 시뮬레이션 → 레이팅 업데이트 → 보상 지급

**Infrastructure Service** (외부 통신):
- `RedisCacheService` (IRedisCacheService 인터페이스):
  ```csharp
  Task UpdateRankingCacheAsync(Guid characterId, int rating);
  Task<Dictionary<Guid, int>> GetTopRankingsAsync(int count);
  ```
  - Redis Sorted Set 캡슐화
  - Application은 인터페이스만 의존 (DIP 준수)
  - 테스트 시 Mock 용이

**트랜잭션 경계**:
- Application Service Layer에서 트랜잭션 관리
- 순서: PostgreSQL 업데이트 → Redis 갱신 (Write-Through)

---

### 데이터베이스 설계 (Database Design)

**인덱스 요구사항** (Q7-Q8 결정):

**PvpRanking**:
```sql
-- 복합 인덱스: 현재 시즌 Top 100 조회 최적화
CREATE INDEX idx_pvp_ranking_season_rating
ON PvpRanking (SeasonId, Rating DESC);

-- 쿼리: SELECT * FROM PvpRanking WHERE SeasonId = ? ORDER BY Rating DESC LIMIT 100
-- 성능: O(log N + 100), 100만 건에서도 수 ms 내 조회
```

**PvpMatch**:
```sql
-- 전적 조회 최적화 (AttackerId 기준)
CREATE INDEX idx_pvp_match_attacker
ON PvpMatch (AttackerId, CreatedAt DESC);

-- 전적 조회 최적화 (DefenderId 기준)
CREATE INDEX idx_pvp_match_defender
ON PvpMatch (DefenderId, CreatedAt DESC);

-- 쿼리: SELECT * FROM PvpMatch WHERE AttackerId = ? OR DefenderId = ? ORDER BY CreatedAt DESC LIMIT 20
-- PostgreSQL의 Bitmap Index Scan이 자동으로 두 인덱스를 병합
```

**PK 타입** (Q9 결정):
- **프로젝트 표준 준수**:
  - PvpMatch.Id: `uuid` (Entity, 유저 생성 데이터)
  - PvpRanking: 복합키 `(SeasonId, CharacterId)`
  - PvpSeason.Id: `int` (Template, 마스터 데이터)

---

### API 설계 (API Design)

**RESTful 엔드포인트** (Q10-Q12 결정):

**1. 매칭 시작 (Resource 중심)**:
```http
POST /api/pvp/matches
Authorization: Bearer {token}

Request:
{
  "characterId": "uuid"
}

Response (201 Created):
{
  "matchId": "uuid",
  "opponent": {
    "characterId": "uuid",
    "name": "적캐릭터",
    "rating": 1500
  },
  "result": "Victory",
  "myRatingChange": +25,
  "opponentRatingChange": -23,
  "rewardGold": 500,
  "rewardCrystal": 10,
  "rewardExperience": 200
}
```
**설계 결정**: 매칭 + 전투를 원자적으로 처리 (서버 권위, 치팅 방지)

**2. 랭킹 조회 (쿼리 파라미터 활용)**:
```http
GET /api/pvp/rankings?seasonId=10&top=100
GET /api/pvp/rankings?seasonId=10&nearMe=true&range=10
GET /api/pvp/rankings?seasonId=10&tier=Gold&page=1&pageSize=50

Response (200 OK):
{
  "rankings": [
    {
      "rank": 1,
      "characterId": "uuid",
      "characterName": "최강자",
      "rating": 2500,
      "wins": 150,
      "losses": 30,
      "tier": "Diamond"
    }
  ],
  "totalCount": 1000,
  "myRank": 42  // nearMe=true 시에만
}
```
**설계 결정**: 단일 엔드포인트로 다양한 필터링 지원 (실용성)

**3. 전적 조회 (Offset 페이징)**:
```http
GET /api/pvp/matches/history?characterId={id}&page=1&pageSize=20&seasonId=10

Response (200 OK):
{
  "matches": [
    {
      "matchId": "uuid",
      "opponentName": "상대방",
      "result": "Victory",
      "ratingChange": +25,
      "createdAt": "2025-11-06T10:30:00Z"
    }
  ],
  "totalCount": 150,
  "currentPage": 1,
  "totalPages": 8
}
```
**설계 결정**: Offset 페이징 (단순, 플레이어당 수백 건 수준에서 충분)

**4. 시즌 정보 조회**:
```http
GET /api/pvp/seasons/current

Response (200 OK):
{
  "seasonId": 10,
  "seasonNumber": 10,
  "startDate": "2025-11-01T00:00:00Z",
  "endDate": "2026-02-01T00:00:00Z",
  "daysRemaining": 25
}
```

**5. 시즌 보상 수령**:
```http
POST /api/pvp/seasons/{seasonId}/rewards
Authorization: Bearer {token}

Response (200 OK):
{
  "tier": "Gold",
  "rewards": {
    "crystal": 500,
    "legendaryBoxes": 1
  }
}
```

**인증/권한**:
- 모든 API: JWT Bearer Token 필수
- CharacterId 검증: JWT의 userId와 Character 소유권 확인

---

### 게임 밸런스 (AI 자동 제안)

> 💡 **학습 프로젝트**: 게임 밸런스는 AI가 제안합니다. 학습자는 **아키텍처와 DB 설계**에 집중하세요.

**ELO 레이팅 시스템**:
- 초기 레이팅: 1000점
- K-Factor: 32 (레이팅 변동폭)
- 매칭 범위: ±200 레이팅
- 최소 레이팅: 0 (음수 불가)
- 최대 레이팅: 무제한

**티어 시스템**:
- Bronze: 0 ~ 999
- Silver: 1000 ~ 1499
- Gold: 1500 ~ 1999
- Platinum: 2000 ~ 2499
- Diamond: 2500+

**매치 보상**:
- 승리: Gold 500, Crystal 10, 경험치 200
- 패배: Gold 100, Crystal 2, 경험치 50

**시즌 시스템**:
- 시즌 기간: 3개월 (90일)
- 시즌 종료 시 Soft Reset: 새 레이팅 = (기존 레이팅 + 1000) / 2
- 시즌 보상:
  - Bronze: Crystal 100
  - Silver: Crystal 300
  - Gold: Crystal 500 + 전설 장비 상자 1개
  - Platinum: Crystal 1000 + 전설 장비 상자 3개
  - Diamond: Crystal 2000 + 신화 장비 상자 1개 + 칭호

**매칭 타임아웃**:
- 대기 시간: 최대 30초
- 타임아웃 시: NPC 봇 매칭 (내 레이팅 ±100, 전투력 80%)

---

## 🎓 학습 포인트 (아키텍처 결정)

**TODO(human)**: 다음 아키텍처 결정이 구현 시 필요합니다:

**Redis 학습**:
- [ ] Redis Docker Compose 설정 (docker-compose.yml 업데이트)
- [ ] StackExchange.Redis NuGet 패키지 추가
- [ ] RedisCacheService 인터페이스 및 구현
- [ ] Program.cs DI 등록 (Singleton IConnectionMultiplexer)
- [ ] Write-Through 전략 구현 (PostgreSQL 성공 후 Redis 갱신)

**ELO 알고리즘 학습**:
- [ ] EloRatingService Domain Service 구현
- [ ] K-Factor 상수 관리 (Configuration vs 하드코딩?)
- [ ] 단위 테스트 작성 (다양한 레이팅 차이 케이스)

**시스템 확장성**:
- [ ] 나중에 리플레이 기능 추가 시 BattleTurnLog 테이블 설계
- [ ] 나중에 데이터 증가 시 PvpRanking 아카이빙 전략 고려
- [ ] Redis 장애 시 Fallback 로직 (PostgreSQL 직접 조회)

> 💡 **학습 가이드**: 게임 밸런스가 아닌, **Redis 캐싱 전략**, **ELO 알고리즘 구현**, **시스템 확장 설계**에 집중하세요.

---

## 🔗 Dependencies

### 기존 시스템 의존성
- **Character Entity**: PvpRanking.CharacterId, PvpMatch.AttackerId/DefenderId
- **전투 시스템 (CombatService)**: PVP 전투 시뮬레이션 재사용
  - 기존 턴제 전투 로직 활용
  - PVE와 동일한 스킬, 버프, 데미지 계산
- **인증 시스템 (JWT)**: 모든 PVP API 인증

### 새로운 요구사항
- **Redis**: 랭킹 캐싱용 (StackExchange.Redis)
- **NuGet 패키지**:
  - StackExchange.Redis (최신 버전)

---

## ⚠️ Non-Functional Requirements

### Performance
- **랭킹 조회**: Redis 사용 시 100ms 이하 (99 percentile)
- **매칭 시간**: 평균 5초 이내 (±200 레이팅 범위)
- **전투 시뮬레이션**: 1초 이내 (기존 전투 시스템 성능)

### Security
- **서버 권위**: 모든 전투 계산 서버에서 수행 (클라이언트 조작 불가)
- **JWT 인증**: 모든 API Bearer Token 필수
- **CharacterId 검증**: 소유권 확인 (다른 플레이어 대신 매칭 불가)
- **Rate Limiting**: 매칭 요청 10회/분 제한 (남용 방지)

### Scalability
- **동시 접속자**: 1000명 동시 매칭 처리 가능
- **Redis 용량**: 10만 명 랭킹 데이터 (약 10MB)
- **PostgreSQL 증가**: 시즌당 10만 건 PvpMatch 레코드 예상

### Reliability
- **Redis 장애 대응**: Fallback to PostgreSQL (성능 저하 허용)
- **트랜잭션 안정성**: PostgreSQL 업데이트 실패 시 Redis 갱신 안 함

---

## 🔄 확장 가능성 (Future Enhancements)

### Phase 1 (현재 구현)
- [x] 1:1 매칭 및 전투
- [x] ELO 레이팅 시스템
- [x] Redis 랭킹 캐싱
- [x] 시즌 시스템

### Phase 2 (향후 확장)
- [ ] 리플레이 기능 (BattleTurnLog 테이블 추가)
- [ ] 랭킹 아카이빙 (PvpRankingHistory 테이블)
- [ ] 3v3 팀 매치 (PvpMatchParticipant 테이블)
- [ ] 매칭 큐 시스템 (Redis Queue)

---

## ✅ Approval

- [ ] Requirements 리뷰 완료
- [ ] 아키텍처 학습 포인트 확인 완료 (TODO(human) 해소)
- [ ] 게임 밸런스 AI 제안값 확인
- [ ] Redis 학습 준비 완료
- [ ] Design 단계로 진행 승인

---

**작성일**: 2025-11-06
**작성자**: AI + User (학습 모드 대화형)
**상태**: Draft
**학습 모드**: 활성 (12개 질문 완료)
