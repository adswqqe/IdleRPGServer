# 🍄 버섯커키우기 완전판 - 역기획 문서

> **.NET 서버 학습 + Unity 네트워크 통신 학습**을 위한 프로젝트
> **목표**: 버섯커키우기의 대부분의 기능을 구현하며 풀스택 게임 개발 마스터

---

## 📋 프로젝트 개요

### 목표
- **주목적**: .NET Web API + Unity 네트워크 통신 **완전 마스터**
- **부목적**: 상용 수준의 방치형 RPG 제작
- **참고 게임**: 버섯커키우기 (거의 모든 기능 구현)

### 학습 포커스
1. RESTful API 설계 및 구현
2. JWT 인증 및 보안
3. Unity-서버 통신
4. 데이터베이스 설계 (EF Core)
5. 실시간 통신 (SignalR)
6. 게임 로직의 서버 검증
7. Redis 캐싱 및 랭킹
8. 복잡한 관계형 데이터 모델링
9. 트랜잭션 및 동시성 처리
10. 게임 밸런싱 및 경제 시스템

---

## 🎮 게임 컨셉

### 장르
**방치형 MMORPG (Idle MMORPG)**

### 핵심 게임플레이
1. **캐릭터 육성**: 레벨업, 스탯 자동 성장, 다중 캐릭터
2. **자동 전투**: 오프라인에서도 자동으로 몬스터 사냥
3. **장비 & 펫**: 무기, 방어구, 펫 육성
4. **스킬 시스템**: 스킬 습득 및 강화
5. **던전 & 보스**: 일반 던전 + 보스 레이드
6. **PVP 아레나**: 다른 플레이어와 대전
7. **소셜**: 친구, 길드, 채팅
8. **수익화**: 가챠, VIP, 상점
9. **일일 컨텐츠**: 미션, 출석, 이벤트

### 게임 루프
```
던전 입장 → 자동 전투 → 보상 획득 → 캐릭터/장비/스킬 강화
→ PVP 도전 → 길드 활동 → 보스 레이드 → 다음 던전
```

---

## 🎯 전체 시스템 목록 (20개 시스템)

### 기본 시스템 (Week 1-6)
1. ✅ **인증 시스템** - JWT 인증
2. ✅ **캐릭터 성장** - 레벨업, 스탯
3. **인벤토리 & 장비** - 아이템 관리
4. **전투 시스템** - 자동 전투
5. **오프라인 보상** - 시간 기반 보상
6. **던전 시스템** - 스테이지 진행
7. **장비 강화** - 확률 기반 강화

### 확장 시스템 (Week 7-15)
8. **스킬 시스템** - 스킬 습득/강화
9. **펫 시스템** - 펫 육성
10. **PVP 아레나** - 플레이어 대전
11. **친구 시스템** - 친구 추가/관리
12. **길드 시스템** - 길드 생성/관리
13. **채팅 시스템** - 실시간 채팅 (SignalR)
14. **보스 레이드** - 협동 보스 전투
15. **퀘스트 & 업적** - 목표 달성

### 고급 시스템 (Week 16-20)
16. **일일 미션 & 출석** - 일일 컨텐츠
17. **가챠 시스템** - 뽑기
18. **상점 & VIP** - 수익화
19. **랭킹 시스템** - Redis 기반
20. **우편함 & 이벤트** - 보상 전달

---

## 📚 시스템별 상세 설계

### 1️⃣ 캐릭터 성장 시스템 ✅

**게임 메커니즘**:
- 전투로 경험치 획득 → 레벨업
- 레벨업 시 스탯 자동 증가
- 다중 캐릭터 시스템 (슬롯별 캐릭터)
- 골드 획득 및 사용

**학습 포인트**:
- 서버: Character 엔티티 설계, 레벨업 로직
- Unity: 캐릭터 UI, 경험치 바 애니메이션

**API**:
```
GET    /api/character              - 캐릭터 목록 조회
GET    /api/character/{id}         - 캐릭터 상세 조회
POST   /api/character              - 캐릭터 생성
DELETE /api/character/{id}         - 캐릭터 삭제
POST   /api/character/{id}/exp     - 경험치 추가
POST   /api/character/{id}/select  - 메인 캐릭터 선택
```

---

### 2️⃣ 자동 전투 시스템

**게임 메커니즘**:
- 던전 입장 → 몬스터와 자동 전투
- 턴제 시뮬레이션 (서버에서 계산)
- 스킬 자동 사용
- 승리 시: 경험치, 골드, 아이템 드롭

**학습 포인트**:
- 서버: 전투 시뮬레이션, 데미지 계산, 보상 로직
- Unity: 전투 애니메이션, 데미지 텍스트

**API**:
```
POST   /api/combat/start          - 전투 시작 { dungeonId, characterId }
GET    /api/combat/{id}/status    - 전투 진행 상황
POST   /api/combat/{id}/simulate  - 전투 시뮬레이션 (서버)
GET    /api/combat/{id}/result    - 전투 결과 조회
POST   /api/combat/{id}/claim     - 보상 수령
```

**전투 로직**:
```csharp
// 턴제 전투 시뮬레이션
int turn = 0;
while (player.HP > 0 && monster.HP > 0 && turn < 100) {
    // 플레이어 공격
    bool isCrit = Random.value < player.CritRate;
    int damage = CalculateDamage(player.Attack, monster.Defense, isCrit, player.CritDamage);
    monster.HP -= damage;

    if (monster.HP <= 0) break;

    // 스킬 사용 확률
    if (turn % 3 == 0 && player.Skills.Count > 0) {
        UseSkill(player, monster);
    }

    // 몬스터 반격
    player.HP -= CalculateDamage(monster.Attack, player.Defense, false, 1.0f);
    turn++;
}
return new CombatResult {
    Victory = player.HP > 0,
    Turns = turn,
    Rewards = GenerateRewards(monster)
};
```

---

### 3️⃣ 인벤토리 & 장비 시스템

**게임 메커니즘**:
- 무기, 방어구, 장신구 관리
- 장비 착용 → 스탯 증가
- 장비 강화 (골드 소모)
- 장비 판매 → 골드 획득

**학습 포인트**:
- 서버: 1:N 관계, LINQ, 트랜잭션
- Unity: ScrollView, 아이템 클릭 이벤트

**API**:
```
GET    /api/inventory                  - 인벤토리 조회
POST   /api/inventory/equip            - 장비 착용 { itemId, slot }
POST   /api/inventory/unequip          - 장비 해제 { slot }
POST   /api/inventory/sell             - 아이템 판매 { itemId }
POST   /api/inventory/sort             - 정렬 { sortType }
GET    /api/inventory/equipment-stats  - 장착 장비 스탯 합계
```

---

### 4️⃣ 던전 시스템

**게임 메커니즘**:
- 스테이지별 던전 (1-100+)
- 난이도 증가 (Normal, Hard, Hell)
- 클리어 조건: 보스 처치
- 보상: 경험치, 골드, 장비

**학습 포인트**:
- 서버: 마스터 데이터, Seed Data, 진행도 관리
- Unity: 던전 선택 UI, 잠금/해제 표시

**API**:
```
GET    /api/dungeon                    - 던전 목록
GET    /api/dungeon/{id}               - 던전 상세 정보
POST   /api/dungeon/{id}/enter         - 던전 입장
GET    /api/dungeon/progress           - 진행도 조회
POST   /api/dungeon/{id}/clear         - 던전 클리어 처리
GET    /api/dungeon/{id}/leaderboard   - 던전 클리어 순위
```

---

### 5️⃣ 오프라인 보상 시스템

**게임 메커니즘**:
- 마지막 로그인 시간 기록
- 재접속 시 경과 시간만큼 보상
- VIP 레벨에 따라 최대 시간 증가
- 일반: 12시간, VIP1: 24시간, VIP3: 48시간

**학습 포인트**:
- 서버: DateTime 처리, 보상 계산 로직
- Unity: 보상 팝업, 타임 포맷팅

**API**:
```
GET    /api/offline-reward/check      - 오프라인 보상 확인
POST   /api/offline-reward/claim      - 오프라인 보상 수령
GET    /api/auto-hunt/status          - 자동 사냥 상태
POST   /api/auto-hunt/start           - 자동 사냥 시작 { dungeonId }
POST   /api/auto-hunt/stop            - 자동 사냥 중지
```

**보상 계산**:
```csharp
TimeSpan offlineTime = DateTime.Now - character.LastLoginTime;
int maxHours = character.VIPLevel == 0 ? 12 : (character.VIPLevel >= 3 ? 48 : 24);
double hours = Math.Min(offlineTime.TotalHours, maxHours);

var dungeon = character.CurrentDungeon;
int gold = (int)(hours * dungeon.GoldPerHour);
int exp = (int)(hours * dungeon.ExpPerHour);
List<Item> items = GenerateRandomItems(hours, dungeon.ItemDropRate);
```

---

### 6️⃣ 장비 강화 시스템

**게임 메커니즘**:
- 골드/강화석 소모하여 장비 강화
- 강화 레벨에 따라 성공 확률 감소
- 실패 시: 골드만 소모 (장비 파괴 없음)
- +10 이상: 실패 시 레벨 하락 가능

**학습 포인트**:
- 서버: 확률 계산, 트랜잭션 (성공/실패 처리)
- Unity: 강화 UI, 성공/실패 연출

**API**:
```
POST   /api/equipment/{id}/upgrade     - 장비 강화 시도
GET    /api/equipment/{id}/upgrade-info - 강화 정보 (확률, 비용)
POST   /api/equipment/{id}/safe-upgrade - 안전 강화 (100% 성공, 비용 2배)
```

**강화 로직**:
```csharp
float GetUpgradeSuccessRate(int currentLevel) {
    if (currentLevel < 3) return 1.0f;      // +0~+2: 100%
    if (currentLevel < 6) return 0.8f;      // +3~+5: 80%
    if (currentLevel < 9) return 0.5f;      // +6~+8: 50%
    if (currentLevel < 12) return 0.3f;     // +9~+11: 30%
    return 0.1f;                            // +12~: 10%
}

int GetUpgradeCost(int currentLevel) {
    return 1000 * (int)Math.Pow(2, currentLevel / 3); // 지수 증가
}

bool TryUpgrade(Equipment equipment, bool useSafeMode = false) {
    int cost = GetUpgradeCost(equipment.Level) * (useSafeMode ? 2 : 1);
    if (character.Gold < cost) return false;

    character.Gold -= cost;

    if (useSafeMode) {
        equipment.Level++;
        return true;
    }

    float successRate = GetUpgradeSuccessRate(equipment.Level);
    bool success = Random.value < successRate;

    if (success) {
        equipment.Level++;
    } else if (equipment.Level >= 10) {
        equipment.Level--; // +10 이상 실패 시 하락
    }

    return success;
}
```

---

### 7️⃣ 스킬 시스템

**게임 메커니즘**:
- 레벨업 시 스킬 포인트 획득
- 스킬 습득 및 레벨 업그레이드
- 스킬 타입: 공격, 버프, 힐, 디버프
- 전투 중 자동 사용 (쿨다운)

**학습 포인트**:
- 서버: 복잡한 데이터 모델, 스킬 효과 계산
- Unity: 스킬 트리 UI, 이펙트

**API**:
```
GET    /api/skill                     - 전체 스킬 목록 (마스터 데이터)
GET    /api/character/{id}/skills     - 캐릭터 보유 스킬
POST   /api/character/{id}/skill/learn - 스킬 습득 { skillId }
POST   /api/character/{id}/skill/upgrade - 스킬 레벨업 { skillId }
POST   /api/character/{id}/skill/equip - 스킬 장착 { skillId, slot }
```

**스킬 데이터 예시**:
```csharp
public class Skill {
    public int Id { get; set; }
    public string Name { get; set; }
    public SkillType Type { get; set; } // Attack, Buff, Heal, Debuff
    public int BaseDamage { get; set; }
    public float DamageMultiplier { get; set; } // 레벨당 증가
    public int Cooldown { get; set; } // 턴
    public int ManaCost { get; set; }
    public string Effect { get; set; } // "Stun", "Burn", "Shield" 등
}

// 사용 예시
public class FireBall : Skill {
    Name = "파이어볼";
    Type = SkillType.Attack;
    BaseDamage = 50;
    DamageMultiplier = 1.2f; // 레벨당 20% 증가
    Cooldown = 3;
    ManaCost = 20;
    Effect = "Burn"; // 2턴간 DoT
}
```

---

### 8️⃣ 펫 시스템

**게임 메커니즘**:
- 가챠/던전에서 펫 획득
- 펫 레벨업 (경험치/재화)
- 펫 스탯이 캐릭터에 추가
- 펫 스킬 (전투 중 자동 발동)

**학습 포인트**:
- 서버: N:M 관계 (캐릭터-펫), 펫 스탯 합산
- Unity: 펫 UI, 펫 애니메이션

**API**:
```
GET    /api/pet                       - 전체 펫 목록 (마스터)
GET    /api/character/{id}/pets       - 보유 펫 목록
POST   /api/character/{id}/pet/summon - 펫 소환 (가챠)
POST   /api/character/{id}/pet/equip  - 펫 장착 { petId }
POST   /api/character/{id}/pet/level-up - 펫 레벨업 { petId }
POST   /api/character/{id}/pet/evolve - 펫 진화 { petId }
```

---

### 9️⃣ PVP 아레나 시스템

**게임 메커니즘**:
- 플레이어 vs 플레이어 (AI 대전)
- 상대 캐릭터 데이터 스냅샷 사용
- 승리 시: 아레나 포인트, 보상
- 랭킹 시스템 연동

**학습 포인트**:
- 서버: 스냅샷 데이터, 매칭 시스템, ELO 레이팅
- Unity: PVP UI, 리플레이 기능

**API**:
```
GET    /api/arena/opponents           - 대전 가능 상대 목록 (3명)
POST   /api/arena/battle              - PVP 전투 시작 { opponentId }
GET    /api/arena/history             - 전투 기록
GET    /api/arena/ranking             - 아레나 랭킹
POST   /api/arena/claim-reward        - 시즌 보상 수령
GET    /api/arena/my-rank             - 내 순위 조회
```

**매칭 시스템**:
```csharp
// MMR (Match Making Rating) 기반 매칭
public List<Character> GetOpponents(Character player) {
    int minMMR = player.MMR - 200;
    int maxMMR = player.MMR + 200;

    var opponents = _context.Characters
        .Where(c => c.MMR >= minMMR && c.MMR <= maxMMR && c.Id != player.Id)
        .OrderBy(x => Guid.NewGuid()) // 랜덤
        .Take(3)
        .ToList();

    return opponents;
}

// 승패 시 MMR 변화
public void UpdateMMR(Character winner, Character loser) {
    int K = 32; // ELO K-factor
    float expectedWin = 1.0f / (1.0f + Math.Pow(10, (loser.MMR - winner.MMR) / 400.0f));

    int winnerChange = (int)(K * (1 - expectedWin));
    int loserChange = (int)(K * (0 - (1 - expectedWin)));

    winner.MMR += winnerChange;
    loser.MMR += loserChange;
}
```

---

### 🔟 친구 시스템

**게임 메커니즘**:
- 친구 추가/삭제
- 친구 요청/수락/거절
- 친구 목록 조회
- 친구에게 하트 선물 (일일 1회)

**학습 포인트**:
- 서버: M:N 자기 참조 관계
- Unity: 친구 목록 UI, 알림

**API**:
```
GET    /api/friend                    - 친구 목록
GET    /api/friend/requests           - 친구 요청 목록
POST   /api/friend/request            - 친구 요청 { targetUserId }
POST   /api/friend/accept             - 친구 수락 { requestId }
POST   /api/friend/reject             - 친구 거절 { requestId }
DELETE /api/friend/{friendId}         - 친구 삭제
POST   /api/friend/{friendId}/gift    - 선물 보내기
GET    /api/friend/search             - 유저 검색 { username }
```

**데이터 모델**:
```csharp
public class Friendship {
    public int Id { get; set; }
    public int UserId { get; set; }
    public int FriendId { get; set; }
    public FriendshipStatus Status { get; set; } // Pending, Accepted
    public DateTime CreatedAt { get; set; }
    public DateTime? LastGiftSentAt { get; set; }

    public User User { get; set; }
    public User Friend { get; set; }
}
```

---

### 1️⃣1️⃣ 길드 시스템

**게임 메커니즘**:
- 길드 생성/가입/탈퇴
- 길드 마스터/부마스터/멤버 권한
- 길드 레벨/경험치
- 길드 스킬 (전체 멤버 버프)
- 길드 레이드 (협동 보스 전투)

**학습 포인트**:
- 서버: 복잡한 권한 관리, 길드 로직
- Unity: 길드 UI, 채팅 연동

**API**:
```
GET    /api/guild                     - 길드 목록/검색
GET    /api/guild/{id}                - 길드 상세 정보
POST   /api/guild/create              - 길드 생성 { name, description }
POST   /api/guild/{id}/join           - 길드 가입 신청
POST   /api/guild/{id}/leave          - 길드 탈퇴
POST   /api/guild/{id}/kick           - 멤버 추방 { memberId }
POST   /api/guild/{id}/promote        - 직책 승급 { memberId, role }
GET    /api/guild/{id}/members        - 멤버 목록
POST   /api/guild/{id}/donate         - 길드 기부 { gold }
POST   /api/guild/{id}/skill/upgrade  - 길드 스킬 업그레이드
GET    /api/guild/{id}/raid           - 길드 레이드 정보
POST   /api/guild/{id}/raid/join      - 레이드 참여
```

**길드 데이터 모델**:
```csharp
public class Guild {
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Level { get; set; }
    public int Experience { get; set; }
    public int MasterUserId { get; set; }
    public int MaxMembers { get; set; } = 30;
    public DateTime CreatedAt { get; set; }

    public List<GuildMember> Members { get; set; }
    public List<GuildSkill> Skills { get; set; }
}

public class GuildMember {
    public int Id { get; set; }
    public int GuildId { get; set; }
    public int UserId { get; set; }
    public GuildRole Role { get; set; } // Master, SubMaster, Member
    public int ContributionPoints { get; set; }
    public DateTime JoinedAt { get; set; }
}

public enum GuildRole {
    Member = 0,
    SubMaster = 1,
    Master = 2
}
```

---

### 1️⃣2️⃣ 채팅 시스템 (SignalR)

**게임 메커니즘**:
- 전체 채팅
- 길드 채팅
- 귓속말 (1:1)
- 채팅 금지 시스템 (신고/관리)

**학습 포인트**:
- 서버: SignalR Hub, 실시간 통신
- Unity: SignalR Client, 채팅 UI

**SignalR Hub**:
```csharp
public class ChatHub : Hub {
    // 전체 채팅
    public async Task SendGlobalMessage(string message) {
        var user = GetCurrentUser();
        await Clients.All.SendAsync("ReceiveGlobalMessage", user.Username, message);
    }

    // 길드 채팅
    public async Task SendGuildMessage(int guildId, string message) {
        var user = GetCurrentUser();
        await Clients.Group($"Guild_{guildId}").SendAsync("ReceiveGuildMessage", user.Username, message);
    }

    // 귓속말
    public async Task SendWhisper(string targetUsername, string message) {
        var user = GetCurrentUser();
        var targetConnectionId = GetConnectionId(targetUsername);
        await Clients.Client(targetConnectionId).SendAsync("ReceiveWhisper", user.Username, message);
    }

    // 길드 그룹 가입
    public async Task JoinGuildChannel(int guildId) {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Guild_{guildId}");
    }
}
```

**Unity SignalR Client**:
```csharp
// ChatManager.cs
private HubConnection _connection;

public async Task ConnectToChat() {
    _connection = new HubConnectionBuilder()
        .WithUrl("https://localhost:7122/chatHub", options => {
            options.AccessTokenProvider = () => Task.FromResult(NetworkManager.Instance.GetAccessToken());
        })
        .Build();

    _connection.On<string, string>("ReceiveGlobalMessage", (username, message) => {
        UnityMainThreadDispatcher.Instance().Enqueue(() => {
            OnGlobalMessageReceived?.Invoke(username, message);
        });
    });

    await _connection.StartAsync();
}
```

---

### 1️⃣3️⃣ 보스 레이드 시스템

**게임 메커니즘**:
- 길드 전용 보스 (주간 1회)
- 멤버 협동 전투 (누적 데미지)
- 보스 HP 공유
- 보상: 기여도에 따라 차등 지급

**학습 포인트**:
- 서버: 공유 상태 관리, 동시성 처리
- Unity: 협동 UI, 실시간 업데이트

**API**:
```
GET    /api/raid/current              - 현재 레이드 정보
POST   /api/raid/{id}/attack          - 보스 공격 { damage }
GET    /api/raid/{id}/ranking         - 기여도 랭킹
POST   /api/raid/{id}/claim-reward    - 보상 수령
GET    /api/raid/history              - 레이드 히스토리
```

**레이드 데이터**:
```csharp
public class Raid {
    public int Id { get; set; }
    public int GuildId { get; set; }
    public string BossName { get; set; }
    public long BossMaxHP { get; set; }
    public long BossCurrentHP { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public RaidStatus Status { get; set; } // Active, Completed, Failed

    public List<RaidParticipation> Participants { get; set; }
}

public class RaidParticipation {
    public int Id { get; set; }
    public int RaidId { get; set; }
    public int UserId { get; set; }
    public long TotalDamage { get; set; }
    public int AttackCount { get; set; }
    public bool RewardClaimed { get; set; }
}
```

---

### 1️⃣4️⃣ 퀘스트 & 업적 시스템

**게임 메커니즘**:
- 메인 퀘스트 (스토리 진행)
- 일일 퀘스트 (매일 리셋)
- 업적 (영구 목표)
- 보상: 골드, 아이템, 칭호

**학습 포인트**:
- 서버: 이벤트 트래킹, 진행도 계산
- Unity: 퀘스트 UI, 알림 시스템

**API**:
```
GET    /api/quest                     - 퀘스트 목록
GET    /api/quest/{id}/progress       - 퀘스트 진행도
POST   /api/quest/{id}/complete       - 퀘스트 완료
POST   /api/quest/{id}/claim-reward   - 보상 수령
GET    /api/achievement               - 업적 목록
GET    /api/achievement/{id}/progress - 업적 진행도
```

**퀘스트 타입**:
```csharp
public enum QuestType {
    KillMonster,      // 몬스터 처치
    ClearDungeon,     // 던전 클리어
    UpgradeEquipment, // 장비 강화
    ReachLevel,       // 레벨 달성
    EarnGold,         // 골드 획득
    WinPvP,           // PVP 승리
}

public class Quest {
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public QuestType Type { get; set; }
    public int TargetValue { get; set; } // 목표 수치
    public List<QuestReward> Rewards { get; set; }
}
```

---

### 1️⃣5️⃣ 일일 미션 & 출석 체크

**게임 메커니즘**:
- 일일 미션 (매일 00시 리셋)
- 출석 체크 (28일 사이클)
- 연속 출석 보너스
- 누적 출석 보상

**학습 포인트**:
- 서버: 일일 리셋 로직, DateTime 처리
- Unity: 캘린더 UI

**API**:
```
GET    /api/daily/missions            - 오늘의 미션 목록
POST   /api/daily/mission/{id}/claim  - 미션 보상 수령
GET    /api/attendance/status         - 출석 현황
POST   /api/attendance/check-in       - 출석 체크
GET    /api/attendance/rewards        - 출석 보상 목록
```

**출석 시스템**:
```csharp
public class AttendanceRecord {
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime LastCheckInDate { get; set; }
    public int ConsecutiveDays { get; set; }
    public int TotalDays { get; set; }
    public int CurrentCycle { get; set; } // 28일 사이클
}

// 출석 보상 예시
Day 1: Gold 1000
Day 3: 강화석 x5
Day 7: 레어 장비 상자
Day 14: 스킬 포인트 x10
Day 28: 전설 장비 상자
```

---

### 1️⃣6️⃣ 가챠 시스템

**게임 메커니즘**:
- 다이아몬드(유료재화)로 뽑기
- 장비/펫/스킬 획득
- 등급: 일반, 레어, 에픽, 전설
- 확률 공개 (법적 요구사항)
- 보장 시스템 (10연차, 천장)

**학습 포인트**:
- 서버: 확률 계산, 보장 시스템
- Unity: 가챠 연출, 결과 UI

**API**:
```
GET    /api/gacha/banners             - 가챠 배너 목록
POST   /api/gacha/{bannerId}/pull     - 단일 뽑기
POST   /api/gacha/{bannerId}/pull-10  - 10연 뽑기
GET    /api/gacha/{bannerId}/rates    - 확률 정보
GET    /api/gacha/history             - 뽑기 기록
```

**가챠 로직**:
```csharp
public List<Item> PullGacha(int bannerId, int count) {
    var banner = GetBanner(bannerId);
    var results = new List<Item>();

    for (int i = 0; i < count; i++) {
        // 보장 시스템 확인
        if (character.GachaPityCounter >= 90) {
            // 천장: 전설 확정
            results.Add(GetRandomItem(Rarity.Legendary));
            character.GachaPityCounter = 0;
            continue;
        }

        float roll = Random.value;
        Rarity rarity;

        if (roll < 0.006f) {        // 0.6% 전설
            rarity = Rarity.Legendary;
            character.GachaPityCounter = 0;
        } else if (roll < 0.056f) { // 5% 에픽
            rarity = Rarity.Epic;
        } else if (roll < 0.256f) { // 20% 레어
            rarity = Rarity.Rare;
        } else {                    // 74.4% 일반
            rarity = Rarity.Common;
        }

        character.GachaPityCounter++;
        results.Add(GetRandomItem(rarity));
    }

    return results;
}
```

---

### 1️⃣7️⃣ 상점 & VIP 시스템

**게임 메커니즘**:
- 일반 상점: 골드로 아이템 구매
- 다이아 상점: 유료 재화 구매
- VIP 시스템: 누적 결제 금액으로 레벨 상승
- VIP 혜택: 오프라인 보상 시간, 일일 다이아, 스탯 버프

**학습 포인트**:
- 서버: 결제 시스템 (모의), VIP 혜택 계산
- Unity: 상점 UI, VIP UI

**API**:
```
GET    /api/shop/items                - 상점 아이템 목록 { shopType }
POST   /api/shop/buy                  - 아이템 구매 { itemId, quantity }
POST   /api/shop/buy-diamond          - 다이아 구매 { packageId }
GET    /api/vip/info                  - VIP 정보
GET    /api/vip/benefits              - VIP 혜택 목록
POST   /api/vip/claim-daily           - VIP 일일 보상 수령
```

**VIP 혜택**:
```csharp
public class VIPBenefits {
    public static Dictionary<int, VIPLevel> Levels = new Dictionary<int, VIPLevel> {
        { 1, new VIPLevel {
            RequiredPayment = 10000,    // 1만원
            OfflineHours = 24,
            DailyDiamond = 100,
            StatBonus = 5               // 모든 스탯 +5%
        }},
        { 3, new VIPLevel {
            RequiredPayment = 50000,    // 5만원
            OfflineHours = 48,
            DailyDiamond = 300,
            StatBonus = 10              // 모든 스탯 +10%
        }},
        { 5, new VIPLevel {
            RequiredPayment = 100000,   // 10만원
            OfflineHours = 72,
            DailyDiamond = 500,
            StatBonus = 15              // 모든 스탯 +15%
        }},
    };
}
```

---

### 1️⃣8️⃣ 랭킹 시스템 (Redis)

**게임 메커니즘**:
- 레벨 랭킹
- PVP 랭킹
- 길드 랭킹
- 보스 레이드 랭킹
- 실시간 업데이트

**학습 포인트**:
- 서버: Redis Sorted Set
- Unity: 랭킹 UI, 자동 갱신

**API**:
```
GET    /api/ranking/level             - 레벨 랭킹 { page, size }
GET    /api/ranking/pvp               - PVP 랭킹
GET    /api/ranking/guild             - 길드 랭킹
GET    /api/ranking/raid              - 레이드 랭킹 { raidId }
GET    /api/ranking/my-rank           - 내 순위 조회 { rankingType }
```

**Redis 구현**:
```csharp
public class RankingService {
    private readonly IConnectionMultiplexer _redis;

    // 랭킹 업데이트
    public async Task UpdateLevelRanking(int userId, int level) {
        var db = _redis.GetDatabase();
        await db.SortedSetAddAsync("ranking:level", userId, level);
    }

    // 랭킹 조회 (상위 100명)
    public async Task<List<RankingEntry>> GetTopLevelRanking(int count = 100) {
        var db = _redis.GetDatabase();
        var entries = await db.SortedSetRangeByRankWithScoresAsync(
            "ranking:level",
            0,
            count - 1,
            Order.Descending
        );

        return entries.Select((e, index) => new RankingEntry {
            Rank = index + 1,
            UserId = (int)e.Element,
            Score = (int)e.Score
        }).ToList();
    }

    // 내 순위 조회
    public async Task<int?> GetMyRank(int userId) {
        var db = _redis.GetDatabase();
        var rank = await db.SortedSetRankAsync("ranking:level", userId, Order.Descending);
        return rank.HasValue ? (int)rank.Value + 1 : null;
    }
}
```

---

### 1️⃣9️⃣ 우편함 시스템

**게임 메커니즘**:
- 시스템 메일 (운영자 발송)
- 보상 메일 (이벤트, 보상)
- 친구 선물
- 유효 기간 (7일/30일)

**학습 포인트**:
- 서버: 메일 발송/수신 로직
- Unity: 우편함 UI

**API**:
```
GET    /api/mail                      - 우편 목록 { page, size }
GET    /api/mail/{id}                 - 우편 상세
POST   /api/mail/{id}/claim           - 보상 수령
POST   /api/mail/{id}/delete          - 우편 삭제
POST   /api/mail/claim-all            - 일괄 수령
POST   /api/mail/delete-all           - 일괄 삭제
```

**메일 데이터**:
```csharp
public class Mail {
    public int Id { get; set; }
    public int UserId { get; set; }
    public MailType Type { get; set; } // System, Reward, Gift
    public string Subject { get; set; }
    public string Body { get; set; }
    public bool IsRead { get; set; }
    public bool IsClaimed { get; set; }
    public DateTime SentAt { get; set; }
    public DateTime ExpireAt { get; set; }

    public List<MailAttachment> Attachments { get; set; }
}

public class MailAttachment {
    public int Id { get; set; }
    public int MailId { get; set; }
    public AttachmentType Type { get; set; } // Gold, Diamond, Item, Pet
    public int ItemId { get; set; }
    public int Quantity { get; set; }
}
```

---

### 2️⃣0️⃣ 이벤트 시스템

**게임 메커니즘**:
- 기간 한정 이벤트
- 이벤트 던전 (특별 보상)
- 경험치 2배 이벤트
- 출석 이벤트

**학습 포인트**:
- 서버: 이벤트 스케줄링, 동적 설정
- Unity: 이벤트 배너 UI

**API**:
```
GET    /api/event/active              - 진행 중 이벤트 목록
GET    /api/event/{id}                - 이벤트 상세
POST   /api/event/{id}/participate    - 이벤트 참여
GET    /api/event/{id}/ranking        - 이벤트 랭킹
```

---

## 🗓️ 20주 완전 학습 로드맵

### **Phase 1: 기초 (Week 1-6)**

#### Week 1: 인프라 + 인증 ✅
- ASP.NET Core, JWT, EF Core
- NetworkManager, AuthAPI, CharacterAPI

#### Week 2: 인벤토리 & 장비
- 1:N 관계, LINQ, 트랜잭션
- Item, Inventory, EquipmentAPI

#### Week 3: 전투 시스템
- 게임 로직 검증, 데미지 계산
- CombatAPI, 전투 UI

#### Week 4: 오프라인 보상
- DateTime 처리, 보상 계산
- OfflineRewardAPI, AutoHuntAPI

#### Week 5: 던전 시스템
- 마스터 데이터, Seed Data
- DungeonAPI, 던전 선택 UI

#### Week 6: 장비 강화
- 확률 계산, 트랜잭션
- UpgradeAPI, 강화 UI

---

### **Phase 2: 확장 (Week 7-12)**

#### Week 7: 스킬 시스템
- 복잡한 데이터 모델, 스킬 효과
- SkillAPI, 스킬 트리 UI

#### Week 8: 펫 시스템
- N:M 관계, 펫 스탯 합산
- PetAPI, 펫 UI

#### Week 9: PVP 아레나
- 스냅샷, 매칭, ELO 레이팅
- ArenaAPI, PVP UI

#### Week 10: 친구 시스템
- M:N 자기 참조 관계
- FriendAPI, 친구 UI

#### Week 11: 길드 시스템 (1/2)
- 복잡한 권한 관리
- GuildAPI (기본), 길드 UI

#### Week 12: 길드 시스템 (2/2)
- 길드 스킬, 길드 레이드
- GuildAPI (고급), 레이드 UI

---

### **Phase 3: 고급 (Week 13-18)**

#### Week 13: 채팅 시스템 (SignalR)
- SignalR Hub, 실시간 통신
- ChatHub, Unity SignalR Client

#### Week 14: 보스 레이드
- 공유 상태, 동시성 처리
- RaidAPI, 협동 UI

#### Week 15: 퀘스트 & 업적
- 이벤트 트래킹, 진행도 계산
- QuestAPI, AchievementAPI

#### Week 16: 일일 미션 & 출석
- 일일 리셋, DateTime 처리
- DailyMissionAPI, AttendanceAPI

#### Week 17: 가챠 시스템
- 확률 계산, 보장 시스템
- GachaAPI, 가챠 연출

#### Week 18: 상점 & VIP
- 결제 시스템(모의), VIP 혜택
- ShopAPI, VIPAPI

---

### **Phase 4: 완성 (Week 19-20)**

#### Week 19: 랭킹 시스템 (Redis)
- Redis Sorted Set
- RankingAPI, 랭킹 UI

#### Week 20: 우편함 & 이벤트
- 메일 시스템, 이벤트 스케줄링
- MailAPI, EventAPI

---

## 🗄️ 완전 데이터베이스 설계

### ER 다이어그램

```
┌─────────────┐
│    User     │
├─────────────┤
│ Id          │ PK
│ Username    │
│ Email       │
│ PasswordHash│
│ VIPLevel    │
│ VIPPoints   │
│ TotalPayment│
└─────────────┘
      │ 1
      ├─────────────┐
      │ N           │ N
┌─────────────┐ ┌──────────────┐
│  Character  │ │  Friendship  │
├─────────────┤ ├──────────────┤
│ Id          │ │ UserId       │ FK
│ UserId      │ │ FriendId     │ FK
│ Level       │ │ Status       │
│ Experience  │ │ CreatedAt    │
│ Gold        │ └──────────────┘
│ Diamond     │
│ Attack      │       1 │
│ Defense     │         │ N
│ MaxHealth   │   ┌──────────────┐
│ CritRate    │   │ GuildMember  │
│ CritDamage  │   ├──────────────┤
│ Evasion     │   │ GuildId      │ FK
│ AttackSpeed │   │ UserId       │ FK
│ MMR         │   │ Role         │
│ LastLogin   │   │ Contribution │
└─────────────┘   └──────────────┘
      │ 1                │ N
      │                  │ 1
      ├──────────┐  ┌─────────────┐
      │ N        │  │    Guild    │
┌─────────────┐  │  ├─────────────┤
│  Inventory  │  │  │ Id          │ PK
├─────────────┤  │  │ Name        │
│ Id          │  │  │ Level       │
│ CharacterId │ FK │  │ Experience  │
│ ItemId      │ FK │  │ MasterUserId│ FK
│ IsEquipped  │  │  │ MaxMembers  │
│ EquipSlot   │  │  └─────────────┘
│ Level       │  │
└─────────────┘  │
      │ N        │
      │ 1        │ N
┌─────────────┐  │  ┌──────────────┐
│    Item     │  │  │CharacterSkill│
├─────────────┤  │  ├──────────────┤
│ Id          │ PK │  │ CharacterId  │ FK
│ Name        │  │  │ SkillId      │ FK
│ Type        │  │  │ Level        │
│ Rarity      │  │  │ EquipSlot    │
│ BaseAttack  │  │  └──────────────┘
│ BaseDefense │  │        │ N
│ BasePrice   │  │        │ 1
└─────────────┘  │  ┌─────────────┐
                 │  │    Skill    │
      1 │        │  ├─────────────┤
        │ N      │  │ Id          │ PK
┌─────────────┐  │  │ Name        │
│CharacterPet │  │  │ Type        │
├─────────────┤  │  │ BaseDamage  │
│ CharacterId │ FK │  │ Cooldown    │
│ PetId       │ FK │  │ ManaCost    │
│ Level       │  │  └─────────────┘
│ IsEquipped  │  │
└─────────────┘  │  ┌─────────────┐
      │ N        │  │   Dungeon   │
      │ 1        │  ├─────────────┤
┌─────────────┐  │  │ Id          │ PK
│     Pet     │  │  │ Name        │
├─────────────┤  │  │ Difficulty  │
│ Id          │ PK │  │ RequiredLvl │
│ Name        │  │  │ RewardExp   │
│ Type        │  │  │ RewardGold  │
│ BaseAttack  │  │  └─────────────┘
│ BaseDefense │  │        │ 1
│ BaseHP      │  │        │ N
│ Rarity      │  │  ┌─────────────┐
└─────────────┘  │  │   Monster   │
                 │  ├─────────────┤
      1 │        │  │ Id          │ PK
        │ N      │  │ DungeonId   │ FK
┌──────────────┐ │  │ Name        │
│CombatSession │ │  │ HP          │
├──────────────┤ │  │ Attack      │
│ Id           │ PK │  │ Defense     │
│ CharacterId  │ FK │  └─────────────┘
│ MonsterId    │ FK
│ Result       │    ┌─────────────┐
│ TotalTurns   │    │    Quest    │
│ ExpGained    │    ├─────────────┤
│ GoldGained   │    │ Id          │ PK
└──────────────┘    │ Name        │
                    │ Type        │
┌──────────────┐    │ TargetValue │
│  ArenaBattle │    │ RewardGold  │
├──────────────┤    │ RewardExp   │
│ Id           │ PK └─────────────┘
│ AttackerId   │ FK      │ 1
│ DefenderId   │ FK      │ N
│ WinnerId     │ FK ┌──────────────┐
│ MMRChange    │    │QuestProgress │
│ BattleDate   │    ├──────────────┤
└──────────────┘    │ CharacterId  │ FK
                    │ QuestId      │ FK
┌──────────────┐    │ CurrentValue │
│     Mail     │    │ IsCompleted  │
├──────────────┤    │ IsClaimed    │
│ Id           │ PK └──────────────┘
│ UserId       │ FK
│ Type         │    ┌──────────────┐
│ Subject      │    │Attendance    │
│ Body         │    ├──────────────┤
│ IsRead       │    │ Id           │ PK
│ IsClaimed    │    │ UserId       │ FK
│ SentAt       │    │ LastCheckIn  │
│ ExpireAt     │    │ Consecutive  │
└──────────────┘    │ TotalDays    │
      │ 1           └──────────────┘
      │ N
┌──────────────┐    ┌──────────────┐
│MailAttachment│    │  GachaHistory│
├──────────────┤    ├──────────────┤
│ Id           │ PK │ Id           │ PK
│ MailId       │ FK │ UserId       │ FK
│ Type         │    │ BannerId     │
│ ItemId       │    │ ItemId       │
│ Quantity     │    │ Rarity       │
└──────────────┘    │ PulledAt     │
                    └──────────────┘
┌──────────────┐
│     Raid     │    ┌──────────────┐
├──────────────┤    │    Event     │
│ Id           │ PK ├──────────────┤
│ GuildId      │ FK │ Id           │ PK
│ BossName     │    │ Name         │
│ BossMaxHP    │    │ Type         │
│ BossCurrentHP│    │ StartDate    │
│ StartTime    │    │ EndDate      │
│ EndTime      │    │ IsActive     │
│ Status       │    └──────────────┘
└──────────────┘
      │ 1
      │ N
┌──────────────────┐
│RaidParticipation │
├──────────────────┤
│ Id               │ PK
│ RaidId           │ FK
│ UserId           │ FK
│ TotalDamage      │
│ AttackCount      │
│ RewardClaimed    │
└──────────────────┘
```

---

## 🎨 UI 화면 구성 (완전판)

### 1. 로그인 화면
```
┌──────────────────────────────────┐
│     🍄 버섯 키우기 RPG            │
├──────────────────────────────────┤
│                                  │
│  Username: [______________]      │
│  Password: [______________]      │
│                                  │
│        [로그인]  [회원가입]        │
│                                  │
│  이벤트: 신규 유저 7일 보상!       │
└──────────────────────────────────┘
```

### 2. 메인 화면 (확장)
```
┌──────────────────────────────────┐
│ [🏠] [친구] [길드] [우편💌] [⚙️]  │
├──────────────────────────────────┤
│  🍄 버섯전사 Lv.45  VIP 3         │
│  HP: [████████] 1250/1250        │
│  EXP: [██████░░░░] 60%           │
│  Gold: 125,450  💎: 850          │
├──────────────────────────────────┤
│                                  │
│       [캐릭터 + 펫 스프라이트]     │
│                                  │
├──────────────────────────────────┤
│ [던전] [인벤토리] [스킬] [펫]      │
│ [PVP] [상점] [가챠] [이벤트]      │
└──────────────────────────────────┘
```

### 3. PVP 아레나 화면
```
┌──────────────────────────────────┐
│          PVP 아레나                │
│  내 순위: #127 (MMR: 1450)        │
├──────────────────────────────────┤
│  대전 상대 선택:                   │
│                                  │
│  🍄 슬라임킬러 Lv.43 #150         │
│  전투력: 2,350  승률: 65%         │
│  [도전하기]                        │
│                                  │
│  ⚔️ 검성 Lv.47 #89               │
│  전투력: 2,890  승률: 78%         │
│  [도전하기]                        │
│                                  │
│  🛡️ 탱크왕 Lv.44 #135            │
│  전투력: 2,450  승률: 58%         │
│  [도전하기]                        │
├──────────────────────────────────┤
│  [전적] [랭킹] [보상]              │
└──────────────────────────────────┘
```

### 4. 길드 화면
```
┌──────────────────────────────────┐
│  길드: 🍄 버섯 연합               │
│  Lv.15  멤버: 28/30               │
├──────────────────────────────────┤
│  마스터: 슬라임킬러                │
│  공지: 레이드 참여 필수!           │
├──────────────────────────────────┤
│  멤버 목록:                        │
│  👑 슬라임킬러 Lv.50 (마스터)      │
│  ⭐ 검성 Lv.47 (부마스터)          │
│  🍄 버섯전사 Lv.45 (접속 중)       │
│  💤 탱크왕 Lv.44 (2시간 전)        │
├──────────────────────────────────┤
│  [채팅] [기부] [스킬] [레이드]     │
└──────────────────────────────────┘
```

### 5. 가챠 화면
```
┌──────────────────────────────────┐
│       🎁 프리미엄 가챠            │
│  천장까지: 23회 남음              │
├──────────────────────────────────┤
│                                  │
│      [빛나는 가챠 상자 이미지]     │
│                                  │
│  확률 정보:                       │
│  ⭐⭐⭐⭐⭐ 전설: 0.6%            │
│  ⭐⭐⭐⭐ 에픽: 5.0%              │
│  ⭐⭐⭐ 레어: 20.0%               │
│  ⭐⭐ 일반: 74.4%                │
├──────────────────────────────────┤
│  보유 다이아: 💎 850              │
│  [1회 뽑기] 100💎  [10연차] 900💎 │
│  [확률 보기] [히스토리]            │
└──────────────────────────────────┘
```

### 6. 채팅 화면
```
┌──────────────────────────────────┐
│  [전체] [길드] [귓속말]            │
├──────────────────────────────────┤
│  [전체] 검성: 레이드 ㄱㄱ          │
│  [길드] 슬라임킬러: 오늘 8시!      │
│  [전체] 탱크왕: 힐러 구함          │
│  [귓속말] 버섯전사 → 검성:         │
│         참여할게요!                │
├──────────────────────────────────┤
│  [메시지 입력______________] [전송]│
└──────────────────────────────────┘
```

---

## 📐 게임 밸런싱 (완전판)

### 레벨업 경험치 곡선
```csharp
int GetRequiredExp(int level) {
    return level * 100 + (int)Math.Pow(level - 1, 2) * 10;
}
// Lv.1 → 2: 100
// Lv.2 → 3: 210
// Lv.10 → 11: 1,900
// Lv.50 → 51: 29,010
```

### 스탯 성장 (VIP 보너스 포함)
```csharp
public void UpdateStats(Character character) {
    float vipBonus = 1.0f + (character.VIPLevel * 0.05f);

    character.Attack = (int)((10 + character.Level * 3) * vipBonus);
    character.Defense = (int)((5 + character.Level * 2) * vipBonus);
    character.MaxHealth = (int)((100 + character.Level * 20) * vipBonus);
    character.CritRate = Math.Min(0.5f, 0.05f + character.Level * 0.002f);
    character.CritDamage = 1.5f + character.Level * 0.01f;
}
```

### 데미지 계산 공식
```csharp
public int CalculateDamage(int attack, int defense, bool isCrit, float critDamage) {
    // 기본 데미지
    int baseDamage = Math.Max(1, attack - defense);

    // 랜덤 변동 (±10%)
    float randomFactor = Random.Range(0.9f, 1.1f);

    // 크리티컬
    float critMultiplier = isCrit ? critDamage : 1.0f;

    return (int)(baseDamage * randomFactor * critMultiplier);
}
```

### 아이템 드롭 확률
```csharp
public Rarity GetDropRarity(int dungeonLevel) {
    float roll = Random.value;

    // 던전 레벨에 따라 확률 조정
    float legendaryRate = Math.Min(0.05f, 0.001f + dungeonLevel * 0.0001f);
    float epicRate = 0.05f + dungeonLevel * 0.001f;
    float rareRate = 0.2f;

    if (roll < legendaryRate) return Rarity.Legendary;
    if (roll < legendaryRate + epicRate) return Rarity.Epic;
    if (roll < legendaryRate + epicRate + rareRate) return Rarity.Rare;
    return Rarity.Common;
}
```

### PVP MMR 계산 (ELO)
```csharp
public void UpdatePvPRating(Character winner, Character loser) {
    int K = 32; // K-factor

    float expectedWin = 1.0f / (1.0f + (float)Math.Pow(10, (loser.MMR - winner.MMR) / 400.0f));
    float expectedLose = 1.0f - expectedWin;

    winner.MMR += (int)(K * (1 - expectedWin));
    loser.MMR += (int)(K * (0 - expectedLose));
}
```

---

## 🔧 기술 스택 (완전판)

### 서버 (.NET)
- **프레임워크**: ASP.NET Core 8.0 Web API
- **아키텍처**: Clean Architecture (Domain, Application, Infrastructure, Presentation)
- **ORM**: Entity Framework Core 8.0
- **데이터베이스**: SQL Server (LocalDB → Production)
- **인증**: JWT Bearer Token + Refresh Token
- **캐싱**: Redis (랭킹, 세션)
- **실시간**: SignalR (채팅)
- **로깅**: Serilog
- **유닛 테스트**: xUnit, Moq
- **API 문서**: Swagger/OpenAPI

### 클라이언트 (Unity)
- **Unity**: 2022 LTS
- **언어**: C# 10
- **비동기**: UniTask
- **JSON**: Newtonsoft.Json
- **UI**: TextMeshPro
- **리소스**: Addressables
- **테스트**: Unity Test Framework (PlayMode + EditMode)
- **SignalR**: Microsoft.AspNetCore.SignalR.Client

### DevOps (선택)
- **CI/CD**: GitHub Actions
- **컨테이너**: Docker (서버)
- **모니터링**: Application Insights

---

## 🎯 개발 범위 정리

### 필수 구현 (MVP - Week 1-10)
1. ✅ 인증 시스템
2. ✅ 캐릭터 성장
3. 인벤토리 & 장비
4. 전투 시스템
5. 오프라인 보상
6. 던전 시스템
7. 장비 강화
8. 스킬 시스템
9. 펫 시스템
10. PVP 아레나

### 권장 구현 (Week 11-16)
11. 친구 시스템
12. 길드 시스템
13. 채팅 시스템
14. 보스 레이드
15. 퀘스트 & 업적
16. 일일 미션 & 출석

### 선택 구현 (Week 17-20)
17. 가챠 시스템
18. 상점 & VIP
19. 랭킹 시스템
20. 우편함 & 이벤트

---

## 📝 개발 원칙

### 1. 학습 목표 우선
- **서버 기술 학습**이 1순위
- 각 시스템마다 새로운 개념 배우기
- 코드 품질 > 기능 완성도

### 2. 서버 검증
- 중요 로직은 **서버에서 계산**
- 치팅 방지 (경험치, 골드, 전투 결과, 가챠)
- 클라이언트는 UI + 요청

### 3. 점진적 개선
- MVP 먼저 완성
- 기능 하나씩 확실히
- 리팩토링 주기적으로

### 4. 테스트 작성
- 단위 테스트 (서버)
- 통합 테스트 (API)
- E2E 테스트 (Unity)

### 5. 문서화
- API 문서 (Swagger)
- 주석 (복잡한 로직)
- README (각 시스템별)

---

## 🚀 시작 가이드

### 현재 상태
- ✅ Week 1 완료
- ⏳ Week 2 준비 중

### 다음 단계 (Week 2)
1. **서버**: Item, Inventory 엔티티 설계
2. **서버**: InventoryAPI 구현
3. **Unity**: 인벤토리 UI 구현
4. **Unity**: 장비 착용 시스템

### 학습 목표 (Week 2)
- 1:N 관계 이해
- LINQ 쿼리 작성
- 트랜잭션 처리
- Unity ScrollView 사용

---

**작성일**: 2025-10-15
**버전**: 2.0 (완전판)
**목적**: .NET + Unity 학습을 위한 **버섯커키우기 완전 구현**
**목표 기간**: 20주 (약 5개월)
**최종 목표**: 풀스택 게임 개발자 역량 확보
