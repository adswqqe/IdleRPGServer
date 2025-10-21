# Week 3 Day 2 - Unity Documentation Synchronization (2025-10-17)

## 🎯 Completed Tasks

### Unity Client Documentation 완전 동기화
**목표**: Gemini 2.5 Pro의 1M+ 토큰 컨텍스트를 활용하여 서버 코드와 Unity 문서 완벽 동기화

**검증 범위**:
- 6개 Controller (Auth, Character, Equipment, Battle, Monster, Reward)
- 24개 API 엔드포인트
- 모든 DTO 필드 일치 여부

---

### Monster API 확장 (오후 작업)
**목표**: Monster 데이터를 Unity 클라이언트가 초기 로딩 시 캐싱할 수 있도록 GET /api/monster/all 엔드포인트 추가

#### 설계 결정: API vs CSV
**질문**: Monster 테이블 데이터를 CSV로 제공 vs API로 제공?

**선택**: Option B (API 방식) ⭐⭐⭐⭐⭐

**이유**:
1. **실시간 밸런스 패치**: 앱 재배포 없이 서버 DB만 수정
2. **보안**: CSV는 디컴파일 시 모든 밸런스 노출
3. **확장성**: 신규 몬스터 추가 시 서버만 업데이트
4. **캐싱 전략**: Unity에서 24시간 PlayerPrefs 캐싱

**Trade-off**:
- 초기 로딩 약간 증가 (네트워크 요청)
- 서버 부하 미미 (게임 시작 시 1회만 호출)

---

## 📝 Modified Files

### 1. Equipment DTO 완전 재작성
**파일**: `E:\StudyGameProj\IdleRPGClient\Docs\unity\equipment\EquipmentDTO.cs`

**변경 전 문제점**:
- 필드 불완전 (15개 중 일부만 존재)
- Enum 정의 없음
- JsonProperty 속성 누락

**변경 후**:
```csharp
// ✅ 15개 필드 완전 반영
public class EquipmentDTO
{
    [JsonProperty("id")] public string Id { get; set; }
    [JsonProperty("name")] public string Name { get; set; }
    [JsonProperty("slot")] public EquipmentSlot Slot { get; set; }
    [JsonProperty("rarity")] public EquipmentRarity Rarity { get; set; }
    [JsonProperty("ownerId")] public string OwnerId { get; set; }
    [JsonProperty("characterId")] public string CharacterId { get; set; }
    [JsonProperty("enhancementLevel")] public int EnhancementLevel { get; set; }
    [JsonProperty("baseAttack")] public int BaseAttack { get; set; }
    [JsonProperty("baseDefense")] public int BaseDefense { get; set; }
    [JsonProperty("baseHp")] public int BaseHp { get; set; }
    [JsonProperty("totalAttack")] public int TotalAttack { get; set; }
    [JsonProperty("totalDefense")] public int TotalDefense { get; set; }
    [JsonProperty("totalHp")] public int TotalHp { get; set; }
    [JsonProperty("createdAt")] public string CreatedAt { get; set; }
    [JsonProperty("updatedAt")] public string UpdatedAt { get; set; }
}

// ✅ Enum 정의 추가
[JsonConverter(typeof(StringEnumConverter))]
public enum EquipmentSlot { Weapon = 1, Helmet = 2, Armor = 3, Gloves = 4, Boots = 5 }

[JsonConverter(typeof(StringEnumConverter))]
public enum EquipmentRarity { Common = 1, Uncommon = 2, Rare = 3, Epic = 4, Legendary = 5 }
```

**핵심 개선사항**:
- 서버 DTO와 100% 일치
- StringEnumConverter로 JSON 가독성 향상 (`"Weapon"` > `1`)
- Nullable 타입 지원 (CharacterId)

---

### 2. Battle DTOs JsonProperty 속성 추가
**파일**: `E:\StudyGameProj\IdleRPGClient\Docs\unity\battle\DTOs.cs`

**변경 이유**: Unity에서 Newtonsoft.Json 사용 시 camelCase 매핑 필수

**변경 내용**:
```csharp
using Newtonsoft.Json; // ✅ 추가

[Serializable]
public class StartBattleRequest
{
    [JsonProperty("characterId")] // ✅ 추가
    public string characterId;

    [JsonProperty("monsterId")] // ✅ 추가
    public string monsterId;
}

// BattleResult, BattleStatistics, BattleLogData, BattleLogsData, BattleStatsData
// 모든 클래스의 모든 필드에 JsonProperty 속성 추가
```

**적용 클래스 (6개)**:
1. StartBattleRequest (2개 필드)
2. BattleResult (4개 필드)
3. BattleStatistics (5개 필드)
4. BattleLogData (9개 필드)
5. BattleLogsData (5개 필드)
6. BattleStatsData (8개 필드)

**총 33개 필드에 JsonProperty 추가**

---

### 3. Monster API 구현 (신규 추가)

#### 서버 구현
**파일**: `IdleRPG.API/Controllers/MonsterController.cs`

```csharp
/// <summary>
/// 전체 몬스터 목록 조회
/// </summary>
[HttpGet("all")]
[Authorize]
[ProducesResponseType(typeof(object), 200)]
[ProducesResponseType(typeof(object), 500)]
public async Task<IActionResult> GetAllMonsters()
{
    try
    {
        var monsters = await _monsterService.GetAllMonstersAsync();
        return Ok(new { monsters });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "전체 몬스터 목록 조회 중 오류 발생");
        return StatusCode(500, new { message = "몬스터 목록 조회 중 오류가 발생했습니다." });
    }
}
```

**파일**: `IdleRPG.Application/Interfaces/IMonsterService.cs`
```csharp
/// <summary>
/// 모든 몬스터 목록을 조회합니다.
/// Unity 클라이언트의 초기 로딩 시 사용됩니다.
/// </summary>
Task<List<MonsterDto>> GetAllMonstersAsync();
```

**파일**: `IdleRPG.Infrastructure/Service/MonsterService.cs`
```csharp
public async Task<List<MonsterDto>> GetAllMonstersAsync()
{
    var monsters = await _unitOfWork.Monsters.GetAllAsync();

    return monsters.Select(m => new MonsterDto
    {
        Id = m.Id,
        Name = m.Name,
        Level = m.Level,
        MaxHealth = m.MaxHealth,
        Attack = m.Attack,
        Defense = m.Defense,
        AttackSpeed = m.AttackSpeed,
        CritRate = m.CritRate,
        CritDamage = m.CritDamage,
        Evasion = m.Evasion,
        ExperienceReward = m.ExperienceReward,
        GoldReward = m.GoldReward
    }).ToList();
}
```

#### Unity 문서 업데이트
**파일**: `E:\StudyGameProj\IdleRPGClient\Docs\unity\monster\DTOs.cs`

```csharp
/// <summary>
/// Represents complete monster data with all stats.
/// </summary>
[Serializable]
public class MonsterData
{
    [JsonProperty("id")]
    public string Id; // Guid is serialized as string

    [JsonProperty("name")]
    public string Name;

    [JsonProperty("level")]
    public int Level;

    [JsonProperty("maxHealth")]
    public int MaxHealth;

    [JsonProperty("attack")]
    public int Attack;

    [JsonProperty("defense")]
    public int Defense;

    [JsonProperty("attackSpeed")]
    public float AttackSpeed;

    [JsonProperty("critRate")]
    public float CritRate;

    [JsonProperty("critDamage")]
    public float CritDamage;

    [JsonProperty("evasion")]
    public float Evasion;

    [JsonProperty("experienceReward")]
    public int ExperienceReward;

    [JsonProperty("goldReward")]
    public int GoldReward;
}

/// <summary>
/// Wrapper for the all monsters response.
/// </summary>
[Serializable]
public class AllMonstersResponseWrapper
{
    [JsonProperty("monsters")]
    public MonsterData[] Monsters;
}
```

**파일**: `E:\StudyGameProj\IdleRPGClient\Docs\unity\monster\API_SPEC.md`

```markdown
## Get All Monsters

Retrieves all monster data from the database. Unity client should call this once during initial loading and cache the results.

- **Endpoint:** `GET /api/monster/all`
- **Method:** `GET`
- **Authentication:** Required (JWT Bearer token)

### Use Cases
- Download monster data during game initialization
- Display monster encyclopedia UI
- Client-side caching (recommended: 24 hours validity)

### Response (200 OK)
{
  "monsters": [
    {
      "id": "a1b2c3d4-e5f6-7890-1234-567890abcdef",
      "name": "슬라임",
      "level": 1,
      "maxHealth": 100,
      "attack": 10,
      "defense": 5,
      "attackSpeed": 1.0,
      "critRate": 0.05,
      "critDamage": 1.5,
      "evasion": 0.05,
      "experienceReward": 10,
      "goldReward": 5
    }
  ]
}
```

---

## ✅ Verification Results

### AI Multi-Model Consensus 검증
**사용 모델**: Gemini 2.5 Pro, GPT-5 Pro

**Gemini 초기 판단**:
- ❌ Equipment API_SPEC.md 전체 누락 (잘못된 판단)

**실제 상황**:
- ✅ `Equipment-API.md` 파일 존재 (507줄 완벽 문서)
- Gemini가 `API_SPEC.md` 패턴만 검색하여 놓침
- Glob 도구로 `**/equipment/**/*.md` 검색하여 발견

**교훈**: 
- AI 검증은 보조 수단, 파일 시스템 직접 검색 필수
- 네이밍 컨벤션 예외 케이스 고려 필요

---

### 최종 동기화 상태 (100% 완료)

#### 6개 Controller 검증 완료
| Controller | 엔드포인트 | 문서 파일 | 상태 |
|-----------|-----------|----------|------|
| AuthController | 5개 | `unity/auth/API_SPEC.md` | ✅ |
| CharacterController | 5개 | `unity/character/API_SPEC.md` | ✅ |
| EquipmentController | 7개 | `unity/equipment/Equipment-API.md` | ✅ |
| BattleController | 4개 | `unity/battle/API_SPEC.md` | ✅ |
| MonsterController | 2개 | `unity/monster/API_SPEC.md` | ✅ **업데이트** |
| RewardController | 2개 | `unity/offline-reward/API_SPEC.md` | ✅ |
| **합계** | **25개** | **6개 문서** | **100%** |

**변경사항**:
- MonsterController: 1개 → 2개 (GET /api/monster/all 추가)
- 전체 엔드포인트: 24개 → 25개

#### DTO 검증 완료
- ✅ Equipment DTO (15개 필드, 2개 Enum)
- ✅ Battle DTOs (6개 클래스, 33개 필드)
- ✅ **Monster DTOs (2개 클래스 추가: MonsterData, AllMonstersResponseWrapper)**
- ✅ Character, Auth, Reward DTOs (기존 일치)

---

## 💡 Battle Client Implementation Strategy Discussion

### 질문: Unity에서 전투 UI 구현 시 서버 시뮬레이션 코드를 알아야 하는가?

**답변**: **NO** - 로직은 알 필요 없음, 이벤트만 알면 됨

### 세 가지 접근 방식 비교

#### Option A: 결과만 표시 (현재 구현) ✅
```json
{ "isVictory": true, "reward": { "gold": 50, "experience": 100 } }
```

**평가**:
- 구현: ★★★★★ (최간단)
- 보안: ★★★★★ (완벽)
- UX: ★☆☆☆☆ (지루함)
- 유지보수: ★★★★★ (완벽 분리)
- **적합성**: 스킵 기능으로만 사용

---

#### Option B: 서버 로직 복제 ❌ **절대 비추천**
```csharp
// 서버 C# 전투 로직 → Unity C# 복사
```

**치명적 문제**:
- ❌ 유지보수 지옥 (양쪽 코드 동기화 필요)
- ❌ 정보 유출 (디컴파일 시 모든 공식 노출)
- ❌ 코드 중복 (서버가 이미 계산함)
- ❌ 결정성 문제 (float 연산 차이)

**역사적 교훈**:
- 2000년대 MMORPG (리니지, 메이플스토리) 해킹 사례
- 클라이언트 로직 노출 → 스피드핵, 패킷 조작

---

#### Option C: 하이브리드 (권장) ⭐⭐⭐⭐⭐
```json
{
  "isVictory": true,
  "battleLogs": [
    { "turn": 1, "eventType": "Attack", "source": "player", "damage": 150, "isCritical": false },
    { "turn": 1, "eventType": "Attack", "source": "monster", "damage": 30 },
    { "turn": 2, "eventType": "SkillUsed", "skillId": "fireball", "damage": 300, "isCritical": true },
    { "turn": 2, "eventType": "Victory" }
  ],
  "reward": { "gold": 50, "experience": 100 }
}
```

**클라이언트 구현**:
```csharp
public async Task PlayBattle(List<BattleEventLog> logs)
{
    foreach (var log in logs)
    {
        switch (log.EventType)
        {
            case "Attack":
                await PlayAttackAnimation(log.Source, log.Target, log.Damage, log.IsCritical);
                break;
            case "SkillUsed":
                await PlaySkillAnimation(log.SkillId, log.Damage);
                break;
        }
        await Task.Delay(battleSpeed); // 2배속, 4배속 조절 가능
    }
}
```

**평가**:
- 구현: ★★★☆☆ (중간)
- 보안: ★★★★☆ (로직 노출 없음)
- UX: ★★★★★ (전투 과정 시각화)
- 유지보수: ★★★★☆ (서버만 수정)
- **적합성**: Idle 게임 최적

**핵심 장점**:
- 성장 체감 극대화 (스킬 이펙트, 크리티컬 연출)
- 2배속, 4배속, 스킵 기능 구현 용이
- 서버 로직 변경해도 클라이언트 수정 불필요

---

## 🎯 Implementation Roadmap

### Phase 1: 현재 (Option A) ✅
```csharp
// 이미 구현됨
var result = await BattleAPI.Start(characterId, monsterId);
if (result.isVictory)
    ShowVictoryUI(result.reward);
```

**이유**: 학습 프로젝트, 빠른 진행 우선

---

### Phase 2: Week 4-5 (Option C 확장) - 선택사항

#### 서버 수정
```csharp
// BattleResultResponse에 추가
public class BattleResultResponse
{
    public bool IsVictory { get; set; }
    public RewardDto Reward { get; set; }
    public BattleStatisticsDto Statistics { get; set; }
    
    // ✨ 새로 추가
    public List<BattleEventLog> BattleLogs { get; set; }
}

public class BattleEventLog
{
    public int Turn { get; set; }
    public string EventType { get; set; } // "Attack", "Skill", "Critical", "Evade", "BuffApplied"
    public string SourceId { get; set; }
    public string TargetId { get; set; }
    public int Damage { get; set; }
    public bool IsCritical { get; set; }
    public string SkillId { get; set; }
}
```

#### Unity 리플레이 엔진
```csharp
public class BattleReplayEngine
{
    private float battleSpeed = 1f; // 1배속 기본
    
    public async Task PlayReplay(List<BattleEventLog> logs)
    {
        foreach (var log in logs)
        {
            await PlayEvent(log);
            await Task.Delay((int)(500 / battleSpeed)); // 속도 조절
        }
    }
    
    public void SetSpeed(float speed) // 1배속, 2배속, 4배속
    {
        battleSpeed = speed;
    }
    
    public void Skip() // 즉시 완료
    {
        battleSpeed = 999f;
    }
}
```

---

## 🔑 Key Technical Learnings

### 1. JsonProperty의 중요성
```csharp
// ❌ 없으면: C# 속성 이름 그대로 사용 (PascalCase)
public string CharacterId { get; set; }
// JSON: { "CharacterId": "..." } ← 서버와 불일치

// ✅ 있으면: 명시적 매핑 (camelCase)
[JsonProperty("characterId")]
public string CharacterId { get; set; }
// JSON: { "characterId": "..." } ← 서버와 일치
```

**원칙**: Newtonsoft.Json 사용 시 모든 필드에 JsonProperty 필수

---

### 2. StringEnumConverter의 장점
```csharp
[JsonConverter(typeof(StringEnumConverter))]
public enum EquipmentSlot { Weapon = 1, Helmet = 2 }

// ❌ 기본 직렬화: { "slot": 1 } - 숫자, 의미 불명확
// ✅ StringEnum: { "slot": "Weapon" } - 문자열, 가독성 높음
```

**장점**:
- JSON 가독성 향상
- Enum 순서 변경에도 안전
- 디버깅 용이

---

### 3. Authoritative Server Pattern
**핵심 원칙**: 서버만 "무엇이 일어났는지" 결정, 클라이언트는 "어떻게 보여줄지"만 결정

**구조**:
```
Server: 전투 시뮬레이션 → BattleEventLog 생성
        ↓
Client: BattleEventLog 수신 → 애니메이션 재생
```

**현대 게임 사례**:
- 리그 오브 레전드: 모든 게임 로직 서버, 클라이언트는 이벤트 재생만
- 발로란트: Authoritative Server + Client-Side Prediction (Option C 하이브리드)

---

### 4. AI 검증의 한계
**문제**: Gemini가 `API_SPEC.md` 패턴만 검색 → `Equipment-API.md` 놓침

**해결**:
```bash
# AI 검증만 믿지 말고 직접 검색
glob "**/equipment/**/*.md"
```

**교훈**:
- AI는 보조 도구, 최종 확인은 도구로
- 네이밍 컨벤션 예외 케이스 고려
- Multiple verification (Gemini + GPT + Direct Search)

---

### 5. 데이터 배포 전략: API vs CSV
**실무 권장**: 정적 데이터도 API로 제공

**이유**:
1. **실시간 업데이트**: 몬스터 밸런스 조정 시 서버 DB만 수정 → 클라이언트 즉시 반영
2. **A/B 테스트**: 특정 유저 그룹에만 다른 밸런스 적용 가능
3. **보안**: 디컴파일로부터 게임 밸런스 보호
4. **버전 관리**: 클라이언트 버전별로 다른 데이터 제공 가능

**Trade-off**:
- 초기 로딩 증가 (네트워크 요청)
- 해결책: 클라이언트 캐싱 (24시간 PlayerPrefs)

**적용 사례**:
- Pokemon GO: 모든 포켓몬 스탯 API로 제공
- 클래시 로얄: 카드 밸런스 서버 제어

---

## 📊 Week 3 Progress

**Core Vertical Slice (Week 1-3): 85% (6/7 시스템)**
- ✅ Authentication System
- ✅ Character Growth System
- ✅ Combat System
- ✅ Offline Rewards
- ✅ Equipment & Inventory
- ✅ **Unity Documentation Sync** ← **Today!**
- ✅ **Monster API 확장** ← **Today!**
- ⏳ Dungeon System (Next)

---

## 📚 Documentation Status

### Unity Client Docs (v1.7)
| 시스템 | API 문서 | DTO 파일 | 상태 |
|-------|---------|---------|------|
| Auth | ✅ API_SPEC.md | ✅ DTOs.cs | 동기화 |
| Character | ✅ API_SPEC.md | ✅ DTOs.cs | 동기화 |
| Equipment | ✅ Equipment-API.md | ✅ EquipmentDTO.cs | **수정** |
| Battle | ✅ API_SPEC.md | ✅ DTOs.cs | **수정** |
| Monster | ✅ API_SPEC.md | ✅ DTOs.cs | **수정 + 확장** |
| Offline Reward | ✅ API_SPEC.md | ✅ DTOs.cs | 동기화 |

**총 라인 수**: 약 2,100줄 (6개 API 명세 + 6개 DTO 파일)

---

## 🎯 Next Steps

### Immediate (Week 3 Day 3)
1. **Docker Desktop 시작 후 API 테스트** (GET /api/monster/all)
2. Dungeon System 구현 시작
3. Equipment 드랍 로직 추가

### Week 4-5 (선택사항)
1. BattleEventLog 시스템 구현
2. Unity 리플레이 엔진 구현
3. 2배속, 4배속, 스킵 기능

### Long-term
- Gacha System (10연차 장비)
- Combat System에 Equipment 스탯 통합
- Inventory UI (Unity)

---

## 🛏️ End of Day Summary

**오늘의 성과**:
1. ✅ Unity 문서 100% 동기화 (Equipment, Battle DTO 수정)
2. ✅ Monster API 확장 (GET /api/monster/all)
3. ✅ 데이터 배포 전략 결정 (API 방식)
4. ✅ 전투 UI 구현 전략 논의 (Option C 하이브리드)

**테스트 대기 항목**:
- GET /api/monster/all (Docker Desktop 실행 필요)

**다음 세션 우선순위**:
1. API 테스트 완료
2. Dungeon System 시작
