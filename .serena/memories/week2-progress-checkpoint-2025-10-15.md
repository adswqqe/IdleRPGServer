# Week 2 Progress Checkpoint - 2025-10-15

## 📊 전체 진행 상태

**Week 2 전체 진행률: 약 60%** (9개 Feature 중 4개 완료, 2개 부분 완료, 3개 미구현)

---

## ✅ 완료된 Feature (4/9)

### Feature 1: Monster Entity & Repository ✅
- Monster 엔티티 생성 완료
- MonsterConfiguration (EF Core) 완료
- 5종 몬스터 시딩 완료 (슬라임, 고블린, 오크, 트롤, 드래곤)
- 마이그레이션 적용 및 EC2 배포 완료

### Feature 2: Battle System Core Logic ✅ (PRD 초과 달성!)
**위치**: `IdleRPG.Infrastructure/Service/BattleService.cs`

**구현 내용** (PRD보다 훨씬 고급):
- ✅ `SimulateBattleAsync(characterId, monsterId)` 메서드
- ✅ Priority Queue 기반 Event-driven 전투 시뮬레이션
- ✅ 크리티컬 히트 시스템 (CritRate, CritDamage)
- ✅ 회피 시스템 (Evasion)
- ✅ 공격 속도 시스템 (AttackSpeed)
- ✅ 보상 자동 지급 (경험치 + 골드)
- ✅ 자동 레벨업 처리 (CharacterService 재사용)

**전투 공식**:
```csharp
// 1. 회피 체크
if (Random < Evasion) return 0;

// 2. 기본 데미지
baseDamage = Max(1, Attack - Defense);

// 3. 크리티컬 체크
if (Random < CritRate) return baseDamage * CritDamage;

return baseDamage;
```

**PRD와 차이점**:
- PRD: 단순 턴제 전투 (공격 속도 고려 안함)
- 실제: Event-driven 전투 (공격 속도, 크리티컬, 회피 모두 구현)

### Feature 4: Character Schema Update ✅
- Gold 필드 추가 완료
- LastLoginTime 필드 추가 완료
- 마이그레이션 적용 완료

### Feature 8: Unity Documentation Update ✅
**파일**: `IdleRPGClient/Docs/unity/API_SPEC_FOR_UNITY.md`

- ✅ v1.3 버전 업데이트 완료
- ✅ 전투 API 문서 추가 (`POST /api/battle/start`)
- ✅ BattleResultResponse, BattleStatisticsDto 문서화
- ✅ Unity C# 예시 코드 추가

---

## ⚠️ 부분 완료 Feature (2/9)

### Feature 3: Battle Controller & API (부분 완료)
**위치**: `IdleRPG.API/Controllers/BattleController.cs`

**구현됨**:
- ✅ POST /api/battle/start - 전투 시뮬레이션 및 보상 지급
- ✅ 캐릭터 소유권 검증
- ✅ Swagger 문서 주석

**미구현** (PRD에 명시됨):
- ❌ GET /api/battle/random-monster?level={level} - 레벨 기준 랜덤 몬스터 선택
- ❌ GET /api/battle/logs/{characterId} - 전투 기록 조회

### Feature 9: Unit Tests (부분 완료)
**위치**: `IdleRPG.Tests/CharacterServiceTests.cs`

**현재 테스트 수: 13개** (CharacterService만 테스트됨)

**테스트 항목**:
- ✅ AddExperience 시나리오 (8개 테스트)
  - 경험치 부족 시 레벨업 안함
  - 정확히 100 경험치로 레벨업
  - 초과 경험치 이월
  - 다중 레벨업 (Theory 5개 케이스)
  - 캐릭터 없음 예외 처리
- ✅ CreateCharacter 시나리오 (2개 테스트)
  - 3개 미만일 때 생성 성공
  - 3개 초과 시 예외 발생
- ✅ DeleteCharacter 시나리오 (2개 테스트)
  - 캐릭터 존재 시 삭제 성공
  - 캐릭터 없음 예외 처리

**미구현** (PRD 요구: 최소 20개 테스트):
- ❌ BattleServiceTests (7개 시나리오 필요)
  - 캐릭터 승리
  - 캐릭터 패배
  - 경험치/골드 정확히 지급
  - 레벨업 자동 처리
  - 전투 로그 저장
  - 존재하지 않는 캐릭터 예외
  - 존재하지 않는 몬스터 예외
- ❌ OfflineRewardServiceTests (6개 시나리오 필요)

---

## ❌ 미구현 Feature (3/9)

### Feature 5: Offline Reward System ❌
**상태**: 마이그레이션에 `OfflineRewards` 테이블이 있으나, Domain 엔티티 파일 없음

**필요 작업**:
- [ ] OfflineReward 엔티티 생성 (또는 다른 방식 결정)
- [ ] OfflineRewardService 구현
  - CalculateOfflineRewardsAsync()
  - ClaimOfflineRewardsAsync()
- [ ] RewardController API
  - GET /api/rewards/offline/{characterId}
  - POST /api/rewards/offline/{characterId}/claim

**보상 공식** (PRD 기준):
```
분당 경험치 = 캐릭터 레벨 * 2
분당 골드 = 캐릭터 레벨 * 1
최대 누적 시간 = 8시간 (480분)
```

### Feature 6: Idle Progress Background Service ❌
**필요 작업**:
- [ ] IdleProgressService 생성 (BackgroundService 상속)
- [ ] 매 1분마다 실행
- [ ] 최근 1시간 이내 로그인 캐릭터 자동 전투
- [ ] Program.cs 등록: `builder.Services.AddHostedService<IdleProgressService>()`

### Feature 7: Battle Log System ❌
**필요 작업**:
- [ ] BattleLog 엔티티 생성
  - Id, CharacterId, MonsterId, IsVictory, ExperienceGained, GoldGained, DamageDealt, DamageTaken, BattleDate
- [ ] BattleLogRepository 구현
- [ ] BattleService에서 전투 후 로그 저장
- [ ] API 엔드포인트: GET /api/battle/logs/{characterId}?count=10

---

## 🎯 다음 작업 우선순위

### 1순위: Feature 3 완성 (random-monster, battle logs API)
- **작업량**: 2-3시간
- **목적**: 전투 시스템 완성
- **연관성**: Feature 7 (BattleLog)과 함께 구현

### 2순위: Feature 7 (Battle Log System)
- **작업량**: 3-4시간
- **목적**: 전투 기록 시스템 구축
- **연관성**: Feature 3의 logs API와 함께 구현

### 3순위: Feature 5 (Offline Reward System)
- **작업량**: 4-5시간
- **목적**: 방치형 RPG 핵심 기능
- **복잡도**: 보상 계산 로직, LastLoginTime 활용

### 4순위: Feature 6 (Idle Progress Background Service)
- **작업량**: 4-5시간
- **목적**: 자동 진행 시스템
- **복잡도**: BackgroundService, 주기적 실행

### 5순위: Feature 9 (Unit Tests 추가)
- **작업량**: 3-4시간
- **목적**: 테스트 커버리지 확보 (20+ 테스트)
- **항목**: BattleServiceTests, OfflineRewardServiceTests

---

## 📈 주요 성과

### 1. 고급 전투 시스템 구현
PRD는 기본 턴제 전투만 요구했으나, 실제로는 **Event-driven 전투 시스템**을 구현:
- Priority Queue 기반 이벤트 스케줄링
- 크리티컬/회피/공격속도 시스템
- 향후 PVP, 던전 시스템 확장 가능

### 2. 자동 보상 지급 시스템
전투 승리 시 서버가 자동으로:
- 경험치 지급 → 자동 레벨업
- 골드 지급
- 업데이트된 캐릭터 정보 반환

### 3. Unity 문서 체계화
API 문서 v1.3 업데이트로 Unity 개발자가 바로 사용 가능한 상태

---

## 🚧 기술적 이슈

### Issue 1: OfflineReward 엔티티 부재
- **현상**: 마이그레이션에는 테이블이 있으나, Domain/Entities에 파일 없음
- **추정**: 제거되었거나 다른 방식으로 구현 예정
- **해결 방안**: 
  1. 엔티티 재생성
  2. 또는 Character.LastLoginTime 기반으로 직접 계산하는 방식

### Issue 2: BattleLog 미구현
- **영향**: Feature 3의 logs API 구현 불가
- **해결**: Feature 7과 함께 구현 권장

---

## 📝 협업 규칙 준수 여부

✅ **Claude 자동 처리 항목**:
- BattleService 구현 (보일러플레이트)
- DTO 생성 (BattleResultResponse, BattleStatisticsDto)
- Unity 문서 업데이트

⚠️ **협업 필요 항목** (아직 논의 안됨):
- Feature 5: 오프라인 보상 공식 밸런싱
- Feature 6: Background Service 실행 주기 (1분? 5분?)
- Feature 7: BattleLog 저장 범위 (모든 전투? 중요한 전투만?)

---

## 🔄 다음 세션 체크리스트

- [ ] Feature 3 완성 (random-monster API 추가)
- [ ] Feature 7 구현 (BattleLog 엔티티 + API)
- [ ] Feature 5 설계 논의 (보상 공식 확정)
- [ ] Feature 9 테스트 추가 (BattleServiceTests)

---

**마지막 업데이트**: 2025-10-15  
**다음 Checkpoint 예정일**: Feature 3, 7 완료 후
