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
