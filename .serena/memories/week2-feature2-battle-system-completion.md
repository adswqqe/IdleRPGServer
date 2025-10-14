# Week 2 Feature 2: Battle System Core Logic 완료 (2025-10-14)

## 🎯 완료된 작업

### 1. DTO 계층 구축
- ✅ **RewardDto** (DTOs/Rewards/): `int` → `long` 타입 변경, 방치형 게임 특성 반영
- ✅ **BattleStatsDto** (DTOs/Battle/): `int` → `long` 변경, `AttackSpeed` 필드 추가
- ✅ **BattleResultResponse**: 기존 파일 활용, Rewards.RewardDto 참조
- ✅ **BattleStatisticsDto**: 전투 통계 (턴 수, 데미지, 크리티컬, 회피)

### 2. 서비스 계층 구현
- ✅ **IBattleService** (Application/Interfaces/): 전투 시뮬레이션 인터페이스
- ✅ **BattleService** (Infrastructure/Service/): Priority Queue 기반 Event-driven 구현

### 3. 설정 및 테스트
- ✅ **Program.cs**: DI 등록 완료 (IBattleService → BattleService)
- ✅ **CharacterServiceTests.cs**: AttackSpeed 매개변수 추가
- ✅ **빌드 성공**: 오류 0개, 경고 21개 (비치명적)

## 🏗️ 핵심 아키텍처: Priority Queue Event-Driven

### 설계 결정 배경
**문제**: 초기 0.1초 고정 틱 방식은 10초 전투에서 100번 체크 필요 (비효율)
**해결**: Priority Queue로 실제 공격 이벤트만 처리 (10-20번) → **CPU 80-90% 절감**

**참고 자료**:
- Melvor Idle: 0.05s 틱 + Event-driven 방식 사용
- Discrete Event Simulation: 업계 표준 패턴

### 구현 코드 구조
```csharp
// 1. 초기 공격 이벤트 스케줄링
float characterAttackInterval = 1.0f / character.Stats.AttackSpeed;
float monsterAttackInterval = 1.0f / monster.AttackSpeed;

var eventQueue = new PriorityQueue<CombatEvent, float>();
eventQueue.Enqueue(characterEvent, characterAttackInterval);
eventQueue.Enqueue(monsterEvent, monsterAttackInterval);

// 2. Event-driven 루프 (고정 틱 없음)
while (characterHP > 0 && monsterHP > 0)
{
    eventQueue.TryDequeue(out var event, out float time);
    currentTime = time;
    
    // 공격 처리 후 다음 이벤트 스케줄링
    if (target.HP > 0)
        eventQueue.Enqueue(nextEvent, currentTime + attackInterval);
}
```

## 💥 데미지 계산 로직

**계산 순서** (사용자 명세):
1. **회피 체크 (우선)**: `Random < evasion` → 0 데미지 리턴
2. **기본 데미지**: `Max(1, Attack - Defense)`
3. **크리티컬 체크**: `Random < critRate` → 데미지 * CritDamage

**구현 메서드**:
```csharp
private long CalculateDamage(
    long attack, long defense,
    float critRate, float critDamage, float evasion,
    out bool isCritical, out bool isEvaded)
{
    // 1. 회피 체크
    if (_random.NextDouble() < evasion)
    {
        isEvaded = true;
        return 0;
    }
    
    // 2. 기본 데미지
    long baseDamage = Math.Max(1, attack - defense);
    
    // 3. 크리티컬 체크
    if (_random.NextDouble() < critRate)
    {
        isCritical = true;
        return (long)(baseDamage * critDamage);
    }
    
    return baseDamage;
}
```

## 🔧 해결한 기술적 이슈

### Issue 1: Monster 엔티티 구조 불일치
- **문제**: Character는 `Stats` Value Object 사용, Monster는 직접 속성
- **원인**: 설계 일관성 부족
- **해결**: BattleService에서 `monster.Attack`, `monster.Defense` 등 직접 접근
- **향후**: MonsterStats Value Object 리팩토링 고려

### Issue 2: 타입 불일치 (int vs long)
- **문제**: BattleStatsDto `int`, CharacterStats `long` 혼용
- **원인**: 방치형 게임 특성 고려 부족
- **해결**: 모든 전투 스탯을 `long`으로 통일

### Issue 3: DTO 중복
- **문제**: Battle 폴더에 RewardDto 중복 생성
- **원인**: 기존 Rewards 폴더 구조 미확인
- **해결**: Rewards/RewardDto.cs (공용) 사용, 중복 파일 삭제

## 📝 코드 품질

**타입 일관성**:
- Character, Monster, BattleStatsDto 모두 `long` 타입 사용
- AttackSpeed는 `float` (초당 공격 횟수)

**네이밍 컨벤션**:
- 이벤트 클래스: `CombatEvent` (내부 private class)
- 메서드: `SimulateCombat`, `CalculateDamage`, `CalculateReward`

**보상 계산** (임시 구현):
```csharp
Experience = monster.Level * 50  // TODO: 데이터 테이블
Gold = monster.Level * 10        // TODO: 데이터 테이블
```

## 🎮 게임 규칙 (사용자 명세)

**전투 종료 조건**:
- PVE: HP 0 도달 시 (시간 제한 없음, maxBattleDuration=300f는 안전장치)
- PVP: 30초 시간 제한 또는 HP 0 (향후 구현)

**실시간 전투**:
- 턴제 ❌
- 실시간 RPG ✅ (버섯커키우기 스타일)
- 양측 AttackSpeed에 따라 독립적으로 공격

## 📊 성능 비교

| 방식 | 10초 전투 계산 횟수 | CPU 사용 |
|------|-------------------|---------|
| 고정 틱 (0.1s) | 100번 체크 | 100% |
| Event-driven | 10-20번 이벤트 | 10-20% |
| **절감율** | **80-90% 감소** | **80-90% 절감** |

## 📁 파일 구조

```
IdleRPG.Application/
├── DTOs/
│   ├── Battle/
│   │   ├── BattleResultResponse.cs      ✅ 기존 활용
│   │   ├── BattleStatsDto.cs            ✅ long + AttackSpeed
│   │   ├── BattleStatisticsDto.cs       ✅ 기존 활용
│   │   └── StartBattleRequest.cs        ✅ 기존 활용
│   └── Rewards/
│       └── RewardDto.cs                  ✅ int→long 변경
├── Interfaces/
│   └── IBattleService.cs                 ✅ 신규 생성

IdleRPG.Infrastructure/
└── Service/
    └── BattleService.cs                  ✅ Priority Queue 구현

IdleRPG.API/
└── Program.cs                            ✅ DI 등록

IdleRPG.Tests/
└── CharacterServiceTests.cs              ✅ AttackSpeed 추가
```

## ✅ 다음 단계: Week 2 Feature 3

**Battle Controller & API 구현**:
- POST /api/battle/start - 전투 시작 API
- BattleController 생성
- 경험치/골드 자동 지급 로직
- Swagger 문서 추가
- Unity API 문서 업데이트

## 💡 학습 포인트

1. **Event-Driven vs Tick-Based**: 이벤트만 처리하여 불필요한 계산 제거
2. **Priority Queue 활용**: .NET 6+ 내장 클래스로 효율적 이벤트 관리
3. **타입 선택의 중요성**: 방치형 게임은 숫자 급증 → `long` 타입 필수
4. **DTO 재사용**: Rewards DTO는 전투/퀘스트/던전 공용 가능
5. **실시간 전투 시뮬레이션**: 공격 간격 = 1.0 / AttackSpeed
