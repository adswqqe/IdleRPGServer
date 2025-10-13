# Battle System & PvP Architecture Design (2025-10-13)

## 📋 개요

Week 2 Battle System 구현 전, 아키텍처 설계에 대한 심층 논의 내용 정리.
PvP 및 복잡한 전투 시스템 구현 시 참고용.

---

## 🎯 핵심 아키텍처 결정

### 1. **서버 권위 모델 (Server Authority)**

**결정: 서버에서 전투 시뮬레이션 수행 ✅**

```csharp
// 클라이언트 요청
POST /api/battle/start
{
    "characterId": "...",
    "monsterId": "..."
}

// 서버 처리
- Monster 데이터 서버 DB에서 조회
- 전투 시뮬레이션 실행
- 보상 계산 (100% 서버)
- 캐릭터 상태 업데이트
- 결과 반환

// 응답
{
    "isVictory": true,
    "expGained": 100,  // ← 서버가 계산
    "goldGained": 50,  // ← 서버가 계산
    "timeline": [...]
}
```

**이유:**
- ✅ 보안: 클라이언트 조작 불가
- ✅ 단순성: Week 2 학습에 적합
- ✅ 확장성: 나중에 Unity로 이동 가능

---

## 🎮 PvE vs PvP 아키텍처

### **PvE (Monster 전투)**

**Monster 데이터:**
- 위치: 서버 DB (PostgreSQL)
- 권위: 100% 서버
- 보상: 서버 계산 (Monster.ExperienceReward, Monster.GoldReward)

```csharp
// ✅ 올바른 방식
var monster = await _monsterRepo.GetByIdAsync(monsterId);
var expGained = monster.ExperienceReward;  // 서버 데이터
character.Experience += expGained;
```

```csharp
// ❌ 절대 안 되는 방식
POST /api/battle/result
{
    "expGained": 999999  // 클라이언트가 보낸 값 - 조작 가능!
}
```

### **PvP (Player 전투)**

**종류:**
1. **실시간 PvP** (LoL, 배그)
   - 서버: 게임 루프 실행 (60 tick/sec)
   - 모든 입력을 서버가 처리
   - Client-Side Prediction + Server Reconciliation
   
2. **턴제 PvP** (하스스톤, 체스)
   - 각 행동마다 서버 검증
   - 서버가 게임 상태 관리
   
3. **비동기 PvP** (방치형 RPG) ⭐ **추천**
   - 상대방 캐릭터 "스냅샷" 사용
   - 서버에서 시뮬레이션
   - 실시간 접속 불필요

**비동기 PvP 구현 패턴:**
```csharp
// 1. 상대방 스냅샷 조회
var opponent = await _characterRepo.GetByIdAsync(opponentId);

// 2. 서버 시뮬레이션
var result = await _battleService.SimulatePvPAsync(
    myCharacter, 
    opponent  // 스냅샷
);

// 3. 보상 계산 (서버!)
if (result.IsVictory)
{
    var reward = CalculateArenaReward(
        myCharacter.RankingPoints,
        opponent.RankingPoints
    );
    
    myCharacter.RankingPoints += reward.Points;
    myCharacter.Gold += reward.Gold;  // ← 절대 클라이언트 값 안 믿음!
}

// 4. 상대방에게 알림 (비동기)
await _notificationService.CreateAsync(new Notification
{
    UserId = opponent.PlayerId,
    Type = "ArenaDefeat",
    Message = $"{myCharacter.Name}에게 공격받음"
});
```

---

## 🔐 보안 & 치팅 방지

### **Golden Rule: Never Trust the Client**

```csharp
// ❌ 위험: 클라이언트 결과 신뢰
[HttpPost("battle/result")]
public IActionResult SubmitResult(BattleResult clientResult)
{
    if (clientResult.IsVictory)  // 조작 가능!
    {
        character.Gold += clientResult.GoldGained;  // 💀
    }
}

// ✅ 안전: 서버가 계산
[HttpPost("battle/start")]
public IActionResult StartBattle(StartBattleRequest request)
{
    var serverResult = SimulateBattle(request.CharacterId, request.MonsterId);
    
    if (serverResult.IsVictory)
    {
        var monster = GetMonster(request.MonsterId);
        character.Gold += monster.GoldReward;  // 서버 데이터
    }
}
```

### **검증 전략**

#### **1. 즉시 밴 금지 (로그 수집)**
```csharp
if (result.ExpGained > maxPossible)
{
    // 일단 허용하되 로그 기록
    await _suspiciousActivityLog.CreateAsync(new SuspiciousActivity
    {
        CharacterId = characterId,
        ActivityType = "BattleRewardExceeded",
        ExpectedMax = maxPossible,
        ActualValue = result.ExpGained
    });
    
    // 반복 시 플래그 (수동 검토)
    if (await CountRecentSuspicious(characterId) > 10)
    {
        await FlagForReview(characterId);
    }
}
```

#### **2. Server Reconciliation (재계산)**
```csharp
// 의심스러운 결과 → 서버가 재계산
if (clientResult.ExpGained > maxPossible * 1.2)
{
    var serverResult = await SimulateBattleAsync(characterId, monsterId);
    return serverResult;  // 서버 결과 사용
}
```

#### **3. Deterministic Simulation (시드 기반)**
```csharp
// 서버가 시드 발급
var seed = GenerateSecureRandomSeed();
var battle = new Battle
{
    Seed = seed,
    ExpiresAt = DateTime.UtcNow.AddMinutes(5)
};

// 클라이언트: 동일 시드로 시뮬레이션
var clientResult = SimulateBattle(character, monster, seed);

// 서버: 검증
var serverResult = SimulateBattle(character, monster, battle.Seed);
if (serverResult.IsVictory != clientResult.IsVictory)
    return Unauthorized();
```

### **False Positive 방지**

- ❌ 즉시 밴: 정상 유저 피해
- ✅ 로그 수집 → 패턴 분석 → 수동 검토
- ✅ 여유있는 범위 설정 (크리티컬, RNG 고려)

---

## ⚙️ 결정론적 시뮬레이션

### **방치형 게임의 특성**

**가능한 이유:**
1. 전투 시작 전 모든 변수 확정
2. 실시간 플레이어 입력 없음
3. 단순한 계산 (복잡한 물리 없음)

**복잡한 시스템도 가능:**
- ✅ 스킬 (조건부 자동 발동)
- ✅ 버프/디버프 (시간/턴 기반)
- ✅ 펫 (AI 패턴 고정)
- ✅ 장비 효과 (패시브)
- ✅ 포메이션 (전투 전 확정)

**불가능한 경우:**
- ❌ 실시간 스킬 선택 (플레이어 입력)
- ❌ 조준/에임 (실시간 위치)
- ❌ 타이밍 의존 게임

### **구현 패턴**

#### **Type 1: 완전 결정론 (RNG 없음)**
```csharp
var characterTimeToKill = monster.MaxHealth / characterDps;
var monsterTimeToKill = character.MaxHealth / monsterDps;

return new BattleResult
{
    IsVictory = characterTimeToKill < monsterTimeToKill
};
```

#### **Type 2: 시드 기반 결정론**
```csharp
public BattleResult SimulateBattle(Character c, Monster m, int seed)
{
    var rng = new Random(seed);  // 시드 고정
    
    while (c.Hp > 0 && m.Hp > 0)
    {
        var damage = c.Attack - m.Defense;
        var isCritical = rng.Next(100) < 10;  // 재현 가능
        if (isCritical) damage *= 2;
        
        m.Hp -= damage;
    }
    
    return result;
}

// 같은 시드 → 같은 결과
```

#### **Type 3: 서버 전용 시뮬레이션** ⭐ **추천**
```csharp
public async Task<BattleResult> SimulateBattleAsync(Guid characterId, Guid monsterId)
{
    var rng = new Random();  // 서버에서만 생성
    
    // 전투 시뮬레이션
    var result = SimulateCombat(character, monster, rng);
    
    // 보상 지급
    if (result.IsVictory)
    {
        character.Experience += monster.ExperienceReward;
        await SaveChanges();
    }
    
    return result;
}
```

---

## ⏱️ 시간 처리

### **Game Time vs Real Time**

```csharp
// ❌ 잘못된 이해
while (gameTime < 30f)
{
    await Task.Delay(1000);  // 1초 기다림 ❌
    gameTime += 1f;
}
// → 실제로 30초 걸림!

// ✅ 올바른 구현
while (gameTime < 30f)
{
    gameTime += 0.1f;  // 게임 시간 증가 (즉시!)
    ProcessTurn(gameTime);
}
// → 실제 처리: ~1ms
```

**30초 PvP 예시:**
```csharp
const float BATTLE_DURATION = 30f;  // 게임 시간
var gameTime = 0f;

while (gameTime < BATTLE_DURATION)
{
    gameTime += 0.1f;  // 0.1초씩 증가 (즉시!)
    
    // 각 캐릭터 행동 처리
    ProcessActions(gameTime);
    
    if (IsBattleOver()) break;
}

// 실제 처리 시간: 2ms
// 게임 내 시간: 30초
```

### **시간 제한 구현**

**방치형 RPG 방식 (추상화):**
```csharp
// 1. 턴 제한
while (turn < MAX_TURNS) { ... }

// 2. 전투 횟수 제한
for (int i = 0; i < MAX_BATTLES; i++) { ... }

// 3. 일일 제한
if (todayCount >= dailyLimit) return Error;

// 4. 시즌 제한
if (DateTime.UtcNow > season.EndTime) return Error;
```

**실시간 타이머는 사용 안 함!**

---

## 🎨 투사체 & 비주얼 처리

### **게임 로직 vs 시각 효과**

```csharp
// === Server (로직) ===
public void ProcessAttack(Character attacker, Monster target)
{
    var damage = CalculateDamage(attacker, target);
    target.Hp -= damage;  // 즉시!
    
    timeline.Add(new BattleEvent
    {
        Time = currentTime,
        Type = "Attack",
        Damage = damage
        // 투사체 정보 없음!
    });
}

// === Client (비주얼) ===
public async Task PlayAttackAnimation(BattleEvent evt)
{
    // 1. 공격 모션
    attacker.PlayAnimation("Attack");
    
    // 2. 투사체 생성 (시각 효과만!)
    var projectile = Instantiate(ProjectilePrefab);
    
    // 3. 투사체 이동 애니메이션
    await MoveProjectileToTarget(projectile, target, 0.5f);
    
    // 4. 데미지 표시 (이미 계산된 값!)
    target.ShowDamageText(evt.Damage);
}
```

### **투사체 이동 시간 처리**

**Level 1: 즉시 명중 (Week 2 권장)**
```csharp
var damage = CalculateDamage(c, m);
m.Hp -= damage;  // 즉시
```

**Level 2: 고정 딜레이 (Week 5+ 선택)**
```csharp
const float PROJECTILE_TRAVEL = 0.5f;

timeline.Add(new AttackEvent { Time = time });
timeline.Add(new DamageEvent { Time = time + PROJECTILE_TRAVEL });
```

**Level 3: 거리 계산 (고급)**
```csharp
var distance = Vector3.Distance(attacker.Position, target.Position);
var travelTime = distance / projectileSpeed;
```

---

## 📦 게임 데이터 관리

### **데이터 분류**

#### **1. Static GameData (정적 마스터 데이터)**
- 위치: **서버 + 클라이언트 모두**
- 형식: JSON 파일
- 버전 관리 필요

```
데이터:
- 스킬 템플릿
- 아이템 템플릿
- 몬스터 템플릿
- 레벨 테이블
- 스탯 공식

저장:
Server: IdleRPG.Infrastructure/Data/GameData/Skills.json
Client: Unity/Resources/GameData/Skills.json
```

#### **2. Dynamic PlayerData (동적 플레이어 데이터)**
- 위치: **서버만** (PostgreSQL)
- 권위: 100% 서버

```
데이터:
- 캐릭터 레벨, 경험치
- 보유 골드
- 인벤토리
- 장비

저장:
Server: PostgreSQL (Characters, Inventories, Equipments)
Client: 서버에서 받아서 표시만
```

#### **3. Calculation Logic (계산 로직)**
- 중요 계산: **서버만** (보안)
- 표시용: 클라이언트 복사 가능 (예상치)
- 실제 적용: **항상 서버!**

```csharp
// Server (권위)
public int CalculateSkillDamage(Character c, Skill s)
{
    var baseDamage = skillTemplate.BaseDamage;
    var scaledDamage = c.GetStat(scalingStat) * scalingRatio;
    return baseDamage + scaledDamage;
}

// Client (예상치 표시)
public int EstimateSkillDamage(Character c, string skillId)
{
    // 같은 로직 복사
    // UI에 "예상 데미지: 150" 표시
}

// 실제 데미지는 서버가 결정!
```

### **데이터 동기화**

```json
// GameDataVersion.json
{
  "version": "1.2.3",
  "skills": {
    "version": "1.2.0",
    "hash": "abc123..."
  }
}
```

```csharp
// 클라이언트 버전 체크
var serverVersion = await GetServerGameDataVersion();
if (serverVersion != localVersion)
{
    await DownloadGameData(serverVersion);
}
```

---

## 🏗️ Week 2 구현 계획

### **현재 구조**
```
✅ Monster Entity (DB)
✅ MonsterRepository
✅ Character Entity (Gold, LastLoginTime 추가 완료)
```

### **구현할 것**
```
→ BattleService (Application Layer)
  - SimulateBattleAsync(characterId, monsterId)
  - 단순 턴제 전투 (Strength, Vitality 사용)
  - 서버 시뮬레이션
  - 보상 지급

→ BattleController (API Layer)
  - POST /api/battle/start
  - 서버 권위 모델
```

### **나중에 (Week 5+)**
```
→ 스킬 시스템
→ GameData/ 폴더 구조
→ Unity 비주얼 연동
→ 타임라인 기반 이벤트
```

---

## 📋 체크리스트

### **PvP 구현 시 확인사항**

- [ ] 비동기 PvP (스냅샷) 방식 사용
- [ ] 서버 시뮬레이션 (보안)
- [ ] 보상은 100% 서버 계산
- [ ] 상대방에게 알림 (비동기)
- [ ] 일일 전투 제한 체크
- [ ] 랭킹 포인트 계산 (레벨 차이 기반)
- [ ] False Positive 방지 (로그 수집)

### **보안 체크리스트**

- [ ] 클라이언트 보상 값 절대 신뢰 안 함
- [ ] Monster/Skill 데이터 서버에서 조회
- [ ] 검증 실패 시 로그 기록 (즉시 밴 금지)
- [ ] 여유있는 범위 설정 (RNG 고려)
- [ ] 수동 검토 프로세스 구축

### **데이터 관리**

- [ ] Static GameData 버전 동기화
- [ ] 중요 계산 로직 서버만
- [ ] 클라이언트 예상치는 표시용만
- [ ] 실제 적용은 항상 서버!

---

## 🔗 참고 자료

### **실제 게임 패턴**
- AFK Arena: 비동기 PvP, 서버 시뮬레이션
- Idle Heroes: 15라운드 제한, 시드 기반
- Clash of Clans: 리플레이 시스템

### **관련 메모리**
- `jenkins-deployment-pipeline-2025-10-13.md`: CI/CD 파이프라인
- `week2-progress-checkpoint-2025-10-13.md`: Week 2 진행 상황
- `week2-workflow-and-collaboration-rules.md`: 협업 규칙

---

**작성일:** 2025-10-13
**컨텍스트:** Week 2 Battle System 구현 전 아키텍처 설계 논의
**다음 단계:** BattleService 구현 시작
