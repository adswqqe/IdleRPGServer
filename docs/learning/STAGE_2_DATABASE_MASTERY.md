# Stage 2: Database Mastery - Unity 개발자를 위한 데이터베이스 심화 (7일 과정)

## 🎯 학습 목표
Unity 개발자가 관계형 데이터베이스와 ORM의 이론적 기초를 깊이 이해하고, Entity Framework Core를 통해 실무에 적용하는 능력을 갖추는 것

## 📚 사전 준비사항
- STAGE_1 완료 (웹 서버 기초 이해)
- Unity C# 경험 (직렬화, 컴포넌트 시스템 이해)
- 기본적인 SQL 문법 지식
- IdleRPG 프로젝트의 Clean Architecture 구조 이해

## 🔑 핵심 질문
이 과정을 통해 다음 질문들에 답할 수 있게 됩니다:
- 왜 관계형 데이터베이스가 70년대에 등장해서 지금까지 지배적인가?
- 객체지향과 관계형 모델은 왜 근본적으로 다른가?
- ORM은 어떻게 이 차이를 극복하는가?
- Entity Framework Core는 내부적으로 어떻게 동작하는가?
- 쿼리 성능 문제를 어떻게 진단하고 해결하는가?
- 트랜잭션과 동시성 문제를 어떻게 다루는가?

---

## Day 1: 데이터베이스 이론의 역사적 기초와 관계형 모델의 탄생

### 🌅 오전 세션 (3시간): 데이터 관리의 패러다임 진화

#### 1.1 컴퓨터 데이터 관리의 역사적 발전 (45분)

**서론: 파일 시스템에서 데이터베이스로의 패러다임 전환**

현대 소프트웨어 개발에서 데이터베이스는 필수적인 요소이다. Unity 개발자가 서버 개발로 전환할 때 가장 큰 도전 중 하나는 **데이터 관리 패러다임의 근본적 차이**를 이해하는 것이다. Unity에서는 모든 데이터가 메모리에 상주하며 즉시 접근 가능하지만, 서버 환경에서는 데이터가 영구 저장소에 존재하며 복잡한 관리 시스템이 필요하다.

**1.1.1 1960년대: 파일 시스템과 초기 데이터 관리**

컴퓨터가 상업적으로 활용되기 시작한 1960년대, 데이터는 **순차 파일(Sequential File)**과 **색인 순차 파일(Indexed Sequential File, ISAM)**로 관리되었다. 이는 Unity의 `PlayerPrefs`나 `JSON` 파일 저장과 유사한 개념이다.

**파일 시스템의 한계**:
1. **데이터 중복성(Data Redundancy)**: 같은 정보가 여러 파일에 중복 저장됨
2. **데이터 불일치(Data Inconsistency)**: 중복된 데이터 간 동기화 문제
3. **데이터 종속성(Data Dependency)**: 응용 프로그램이 파일 구조에 강하게 결합됨
4. **동시성 제어 부재**: 여러 프로그램이 동시에 파일을 수정할 때 충돌 발생
5. **무결성 제약 부재**: 잘못된 데이터 입력을 막을 방법이 없음

Unity 비유:
```csharp
// Unity의 파일 기반 데이터 저장 (1960년대 방식과 유사)
public class PlayerDataManager : MonoBehaviour
{
    // 각 저장 파일이 독립적으로 관리됨 (중복 문제)
    public void SavePlayerStats(PlayerStats stats)
    {
        string json = JsonUtility.ToJson(stats);
        File.WriteAllText("player_stats.json", json); // 파일 1
    }

    public void SavePlayerInventory(PlayerInventory inventory)
    {
        string json = JsonUtility.ToJson(inventory);
        File.WriteAllText("player_inventory.json", json); // 파일 2
    }

    // 문제: player_stats와 player_inventory의 PlayerId가 다르면?
    // 문제: 두 파일을 동시에 업데이트하다가 크래시 나면?
    // 문제: 다른 프로그램이 동시에 같은 파일을 수정하면?
}
```

**1.1.2 1960년대 후반: 계층형 데이터베이스 (IMS)**

IBM이 1968년 개발한 **IMS(Information Management System)**는 최초의 상용 데이터베이스 시스템이다. Apollo 프로그램의 자재 관리를 위해 개발되었으며, **계층형 모델(Hierarchical Model)**을 사용했다.

**계층형 모델의 구조**:
```
회사
├── 부서1
│   ├── 직원1
│   ├── 직원2
│   └── 직원3
└── 부서2
    ├── 직원4
    └── 직원5
```

Unity의 GameObject 계층 구조와 놀랍도록 유사하다:
```
Scene
├── Player
│   ├── Head
│   ├── Body
│   └── Legs
└── Enemy
    ├── Weapon
    └── Shield
```

**계층형 모델의 한계**:
1. **M:N 관계 표현 불가**: 한 직원이 여러 부서에 속할 수 없음
2. **경직된 구조**: 계층을 변경하려면 전체 재구성 필요
3. **복잡한 쿼리**: 부모-자식 관계 외의 검색이 어려움
4. **데이터 중복**: M:N을 구현하려면 데이터를 복사해야 함

Unity에서도 동일한 문제가 발생한다:
```csharp
// Unity의 계층 구조 한계
public class GameManager : MonoBehaviour
{
    // 한 Player가 여러 Team에 속하려면?
    public GameObject team1;
    public GameObject team2;

    // 방법 1: Player를 복제 (데이터 중복!)
    GameObject player1InTeam1 = Instantiate(player);
    GameObject player1InTeam2 = Instantiate(player);

    // 방법 2: 참조만 저장 (계층 구조 깨짐)
    public List<GameObject> playerReferences;
}
```

**1.1.3 1970년대 초반: 네트워크 데이터베이스 (CODASYL)**

CODASYL(Conference on Data Systems Languages)이 1971년 제안한 **네트워크 모델(Network Model)**은 계층형 모델의 한계를 극복하려 했다.

**네트워크 모델의 구조**:
```
직원1 ──┬── 프로젝트A
        └── 프로젝트B

직원2 ──┬── 프로젝트A
        └── 프로젝트C
```

**네트워크 모델의 한계**:
1. **복잡한 포인터 구조**: 프로그래머가 직접 포인터를 관리해야 함
2. **절차적 쿼리**: "어떻게" 탐색할지를 명시해야 함 (선언적이 아님)
3. **높은 복잡도**: 데이터 구조 변경 시 모든 응용 프로그램 수정 필요

Unity의 컴포넌트 참조 시스템과 유사한 복잡성:
```csharp
// Unity에서 복잡한 참조 관리 (네트워크 모델과 유사)
public class ComplexReferenceManager : MonoBehaviour
{
    // 수동 포인터 관리
    public Player player;
    public List<Quest> activeQuests; // Player → Quest
    public List<Item> inventory;     // Player → Item
    public Guild guild;              // Player → Guild

    // Quest도 역참조가 필요하면?
    // Item도 소유자를 알아야 하면?
    // 순환 참조 문제는?

    void UpdateQuestProgress(Quest quest)
    {
        // 양방향 참조 유지 (에러 가능성 높음!)
        quest.participants.Add(player);
        player.activeQuests.Add(quest);
    }
}
```

**1.1.4 패러다임의 전환이 필요한 이유**

1960년대 데이터 관리 시스템들의 공통적 문제점:
1. **응용 프로그램과 데이터의 강한 결합**: 데이터 구조 변경 시 모든 프로그램 수정 필요
2. **절차적 접근**: "무엇을" 원하는지가 아닌 "어떻게" 가져올지를 명시
3. **물리적 구조 노출**: 프로그래머가 저장 구조를 알아야 함
4. **데이터 무결성 보장 어려움**: 응용 프로그램이 직접 검증해야 함

이러한 문제들은 **데이터 독립성(Data Independence)**의 부재에서 비롯된다. 새로운 패러다임이 필요했고, 그것이 바로 **관계형 모델**이다.

---

#### 1.2 E.F. Codd의 관계형 모델 혁명 (45분)

**1.2.1 1970년: 패러다임을 바꾼 논문**

1970년 6월, IBM 연구원 **Edgar F. Codd**는 "A Relational Model of Data for Large Shared Data Banks"라는 논문을 발표했다. 이 논문은 데이터베이스 역사상 가장 영향력 있는 논문 중 하나로, 컴퓨터 과학에 **튜링상(1981)**을 안겨준 업적이다.

**Codd의 핵심 통찰**:
1. **데이터를 수학적 관계(Relation)로 모델링**: 집합론과 1차 논리학 기반
2. **물리적 저장과 논리적 구조의 분리**: 데이터 독립성 달성
3. **선언적 쿼리 언어**: "무엇을" 원하는지만 명시, "어떻게"는 시스템이 결정

**관계형 모델의 수학적 기초**:

Codd는 데이터베이스를 **집합론(Set Theory)**과 **1차 술어 논리(First-Order Predicate Logic)**로 정의했다.

**정의 1: 도메인(Domain)**
도메인 D는 원자적(atomic) 값들의 집합이다.
```
D_Name = {"Alice", "Bob", "Charlie", ...}
D_Age = {0, 1, 2, ..., 150}
D_Email = {"alice@example.com", "bob@example.com", ...}
```

Unity 비유:
```csharp
// Unity의 타입 시스템 = 도메인
public enum CharacterClass // 도메인: 가능한 직업들의 집합
{
    Warrior,
    Mage,
    Archer
}

public struct Level // 도메인: 1~100 사이의 정수
{
    private int value;
    public int Value
    {
        get => value;
        set => this.value = Mathf.Clamp(value, 1, 100);
    }
}
```

**정의 2: 관계(Relation)**
관계 R은 도메인들의 카르테시안 곱(Cartesian Product)의 부분집합이다.

수학적 정의:
```
R ⊆ D₁ × D₂ × ... × Dₙ

여기서 × 는 카르테시안 곱 연산자
```

예시:
```
Player 관계 = ID × Name × Level × Class

Player = {
    (1, "Alice", 50, "Warrior"),
    (2, "Bob", 30, "Mage"),
    (3, "Charlie", 45, "Archer")
}
```

**정의 3: 튜플(Tuple)**
관계의 각 원소를 튜플(또는 행, row)이라 한다.
```
t = (1, "Alice", 50, "Warrior") ∈ Player
```

**정의 4: 속성(Attribute)**
각 도메인에 부여된 이름을 속성이라 한다.
```
Player.ID : D_ID
Player.Name : D_Name
Player.Level : D_Level
Player.Class : D_CharacterClass
```

**Unity와의 근본적 차이**:

Unity의 객체 모델:
```csharp
// Unity: 객체는 정체성(identity)을 가진 독립적 존재
public class Player : MonoBehaviour
{
    public int ID;
    public string Name;
    public int Level;
    public CharacterClass Class;

    // 객체는 "무언가"이다 (존재론적)
    // 메모리 주소로 식별됨
    // 가변적(mutable) - 속성 변경 가능
}

Player player1 = new Player();
Player player2 = new Player();
// player1 != player2 (다른 객체)
// 설령 모든 속성 값이 같아도 다른 존재
```

관계형 모델:
```sql
-- 관계형: 튜플은 값들의 조합일 뿐
-- "무언가에 대한 사실"을 표현 (인식론적)
-- 불변적(immutable) - 튜플 자체는 변경 불가
-- 속성 값들의 조합으로만 식별됨

INSERT INTO Player (ID, Name, Level, Class)
VALUES (1, 'Alice', 50, 'Warrior');

-- 같은 값을 가진 튜플은 같은 튜플
-- (1, 'Alice', 50, 'Warrior') = (1, 'Alice', 50, 'Warrior')
```

**1.2.2 데이터 독립성의 철학적 의미**

Codd의 가장 중요한 기여 중 하나는 **데이터 독립성(Data Independence)** 개념이다.

**물리적 데이터 독립성(Physical Data Independence)**:
- 데이터의 물리적 저장 방식이 변경되어도 논리적 스키마는 불변
- 예: B-Tree 인덱스 추가/제거해도 쿼리는 동일

**논리적 데이터 독립성(Logical Data Independence)**:
- 논리적 스키마가 변경되어도 응용 프로그램은 최소한의 수정
- 예: 테이블에 컬럼 추가해도 기존 쿼리는 동작

Unity에는 이러한 독립성이 없다:
```csharp
// Unity: 물리적/논리적 구조가 응용 로직과 강하게 결합
public class GameManager : MonoBehaviour
{
    public List<Player> players; // List를 Array로 바꾸면? (물리적 변경)

    void AttackMonster()
    {
        // Player 클래스에 새 필드가 추가되면? (논리적 변경)
        int damage = player.Strength * 2; // Strength → PhysicalPower로 이름 변경하면?
    }
}
```

관계형 모델:
```sql
-- 응용 프로그램은 논리적 관계만 알면 됨
SELECT Name, Level FROM Player WHERE Class = 'Warrior';

-- 내부적으로 인덱스가 추가되든, 테이블이 파티셔닝되든
-- 쿼리는 변경 불필요 (물리적 독립성)

-- Player 테이블에 Email 컬럼이 추가되어도
-- 기존 쿼리는 계속 동작 (논리적 독립성)
```

**1.2.3 선언적 vs 절차적 패러다임**

Codd는 데이터 조작을 위해 **선언적 언어(Declarative Language)**를 제안했다. 이는 Unity의 **명령형 프로그래밍(Imperative Programming)**과 근본적으로 다르다.

**절차적 접근 (Unity, 네트워크 DB)**:
```csharp
// Unity: "어떻게" 할지를 명시
List<Player> warriors = new List<Player>();
foreach(Player player in allPlayers) // 반복 순서 명시
{
    if(player.Class == CharacterClass.Warrior) // 조건 검사 순서 명시
    {
        if(player.Level >= 30) // 중첩된 조건
        {
            warriors.Add(player); // 결과 저장 방법 명시
        }
    }
}
// 정렬까지 하려면?
warriors.Sort((p1, p2) => p2.Level.CompareTo(p1.Level)); // 정렬 알고리즘까지 명시
```

**선언적 접근 (SQL)**:
```sql
-- SQL: "무엇을" 원하는지만 명시
SELECT * FROM Player
WHERE Class = 'Warrior' AND Level >= 30
ORDER BY Level DESC;

-- "어떻게" 실행할지는 데이터베이스가 결정:
-- - 인덱스를 사용할지 말지
-- - 어떤 조인 알고리즘을 쓸지
-- - 정렬을 메모리에서 할지 디스크에서 할지
```

이러한 선언적 특성은 **쿼리 최적화(Query Optimization)**를 가능하게 한다. 데이터베이스는 같은 결과를 내는 여러 실행 방법 중 가장 효율적인 것을 선택할 수 있다.

---

#### 1.3 관계 대수의 수학적 기초 (45분)

**1.3.1 관계 대수 연산자**

Codd는 관계를 조작하기 위한 **관계 대수(Relational Algebra)**를 정의했다. 이는 SQL의 이론적 기반이다.

**기본 연산자**:

**1. Selection (σ - sigma)**
조건을 만족하는 튜플들을 선택한다.

수학적 정의:
```
σ_조건(R) = {t | t ∈ R ∧ 조건(t) = true}
```

예시:
```
σ_Level≥30(Player) = {
    (1, "Alice", 50, "Warrior"),
    (3, "Charlie", 45, "Archer")
}
```

SQL 대응:
```sql
SELECT * FROM Player WHERE Level >= 30;
```

Unity 대응:
```csharp
var result = players.Where(p => p.Level >= 30);
```

**2. Projection (π - pi)**
특정 속성들만 추출한다.

수학적 정의:
```
π_A₁,A₂,...,Aₙ(R) = {(t.A₁, t.A₂, ..., t.Aₙ) | t ∈ R}
```

예시:
```
π_Name,Class(Player) = {
    ("Alice", "Warrior"),
    ("Bob", "Mage"),
    ("Charlie", "Archer")
}
```

SQL 대응:
```sql
SELECT Name, Class FROM Player;
```

**중요**: Projection은 중복을 제거한다 (집합 의미론):
```
π_Class(Player) = {"Warrior", "Mage", "Archer"}
-- 중복된 "Warrior"는 하나만
```

**3. Cartesian Product (×)**
두 관계의 모든 튜플 조합을 생성한다.

수학적 정의:
```
R × S = {(r, s) | r ∈ R ∧ s ∈ S}
```

예시:
```
Player = {(1, "Alice"), (2, "Bob")}
Quest = {("Q1", "Kill Slime"), ("Q2", "Find Treasure")}

Player × Quest = {
    ((1, "Alice"), ("Q1", "Kill Slime")),
    ((1, "Alice"), ("Q2", "Find Treasure")),
    ((2, "Bob"), ("Q1", "Kill Slime")),
    ((2, "Bob"), ("Q2", "Find Treasure"))
}
```

SQL 대응:
```sql
SELECT * FROM Player CROSS JOIN Quest;
-- 또는
SELECT * FROM Player, Quest;
```

**4. Join (⋈)**
조건을 만족하는 튜플들만 결합한다.

수학적 정의:
```
R ⋈_조건 S = σ_조건(R × S)
```

Natural Join (공통 속성으로 자동 조인):
```
R ⋈ S
```

예시:
```
Player = {(1, "Alice", 50), (2, "Bob", 30)}
PlayerQuest = {(1, "Q1"), (1, "Q2"), (2, "Q1")}

Player ⋈ PlayerQuest = {
    (1, "Alice", 50, "Q1"),
    (1, "Alice", 50, "Q2"),
    (2, "Bob", 30, "Q1")
}
```

SQL 대응:
```sql
SELECT * FROM Player
JOIN PlayerQuest ON Player.ID = PlayerQuest.PlayerID;
```

**5. Union, Intersection, Difference (∪, ∩, −)**
집합 연산자들

Union (합집합):
```
R ∪ S = {t | t ∈ R ∨ t ∈ S}
```

Intersection (교집합):
```
R ∩ S = {t | t ∈ R ∧ t ∈ S}
```

Difference (차집합):
```
R − S = {t | t ∈ R ∧ t ∉ S}
```

SQL 대응:
```sql
SELECT * FROM Player WHERE Class = 'Warrior'
UNION
SELECT * FROM Player WHERE Level >= 50;
```

**1.3.2 관계 대수의 완전성(Completeness)**

Codd는 이 5개 기본 연산자 {σ, π, ×, ∪, −}가 **관계적으로 완전(Relationally Complete)**하다고 증명했다. 즉, 모든 쿼리를 이 연산자들의 조합으로 표현할 수 있다.

예시: "30레벨 이상인 전사들의 이름"
```
π_Name(σ_Class='Warrior' ∧ Level≥30(Player))
```

이는 SQL의 이론적 기반이 된다:
```sql
SELECT Name FROM Player
WHERE Class = 'Warrior' AND Level >= 30;
```

**1.3.3 Unity LINQ와의 비교**

Unity C#의 LINQ는 관계 대수에서 영감을 받았다:

```csharp
// LINQ는 관계 대수를 C#에 적용한 것
var result = players
    .Where(p => p.Class == CharacterClass.Warrior && p.Level >= 30) // σ (Selection)
    .Select(p => p.Name); // π (Projection)

// Join도 가능
var playerQuests = players
    .Join(quests,
          p => p.ID,
          q => q.PlayerID,
          (p, q) => new { p.Name, q.QuestName }); // ⋈ (Join)
```

**차이점**:
1. **LINQ는 메모리에서 동작**: 모든 데이터가 RAM에 로드됨
2. **SQL은 디스크 기반**: 데이터가 영구 저장소에 있음
3. **LINQ는 지연 실행**: 열거할 때 실행됨
4. **SQL은 즉시 실행**: 쿼리를 보내면 바로 실행됨

---

#### 1.4 Unity Asset 관리 vs 관계형 데이터 관리의 패러다임 차이 (45분)

**1.4.1 데이터 생명주기의 차이**

**Unity의 임시적 데이터 모델**:
```csharp
public class GameSession : MonoBehaviour
{
    // 데이터는 세션 동안만 존재
    public List<Player> activePlayers;
    public Dictionary<int, Monster> monsters;

    void OnApplicationQuit()
    {
        // 게임 종료 시 모든 데이터 소멸 (명시적 저장 필요)
        SaveGameData(); // 수동으로 저장해야 함
    }
}
```

**관계형 DB의 영구적 데이터 모델**:
```sql
-- 데이터는 기본적으로 영구 저장
INSERT INTO Player (Name, Level) VALUES ('Alice', 50);
-- 전원이 꺼져도, 프로그램이 크래시해도 데이터는 안전

-- 트랜잭션으로 원자성 보장
BEGIN TRANSACTION;
UPDATE Player SET Level = Level + 1 WHERE ID = 1;
UPDATE PlayerStats SET Experience = 0 WHERE PlayerID = 1;
COMMIT; -- 두 작업 모두 성공하거나 모두 실패
```

**1.4.2 데이터 접근 패턴의 차이**

**Unity: 탐색 기반(Navigation-based)**
```csharp
// 객체 그래프를 따라 탐색
Player player = GameObject.Find("Player").GetComponent<Player>();
Weapon weapon = player.equippedWeapon; // 참조를 따라감
int damage = weapon.attackPower; // 또 참조를 따라감

// 문제: 깊은 계층 구조일수록 복잡
Character character = player.character;
Inventory inventory = character.inventory;
Item item = inventory.items[0];
// NullReferenceException의 위험!
```

**관계형: 값 기반(Value-based)**
```sql
-- 값으로 검색
SELECT Weapon.AttackPower
FROM Player
JOIN EquippedWeapon ON Player.ID = EquippedWeapon.PlayerID
JOIN Weapon ON EquippedWeapon.WeaponID = Weapon.ID
WHERE Player.Name = 'Alice';

-- 모든 관계는 값(외래 키)으로 표현
-- 포인터 탐색이 아닌 조인 연산
```

**1.4.3 데이터 구조 변경의 영향**

**Unity: 강한 결합**
```csharp
// Player 클래스 변경
public class Player : MonoBehaviour
{
    // public int Strength; // 이전
    public Stats stats; // 새 구조

    // 문제: 모든 코드에서 player.Strength를 찾아서 수정해야 함!
}

// 영향받는 코드들
void CalculateDamage()
{
    // int damage = player.Strength * 2; // 에러!
    int damage = player.stats.Strength * 2; // 수동 수정 필요
}
```

**관계형: 느슨한 결합 (데이터 독립성)**
```sql
-- Player 테이블 구조 변경
ALTER TABLE Player ADD COLUMN Email VARCHAR(255);

-- 기존 쿼리는 계속 동작
SELECT Name, Level FROM Player; -- 문제없음

-- View를 사용한 호환성 유지
CREATE VIEW OldPlayerView AS
SELECT ID, Name, Level,
       (SELECT Strength FROM PlayerStats WHERE PlayerID = Player.ID) as Strength
FROM Player;

-- 응용 프로그램은 OldPlayerView를 사용하면 변경 불필요
```

**1.4.4 동시성 모델의 차이**

**Unity: 단일 스레드 가정**
```csharp
// Unity는 기본적으로 단일 스레드
void Update()
{
    player.gold += 100; // 동시성 문제 없음
    player.experience += 50; // 순차적 실행
}

// 멀티스레드를 쓰려면 명시적 동기화 필요
private object lockObj = new object();
void IncrementGold()
{
    lock(lockObj) // 수동 잠금
    {
        player.gold += 100;
    }
}
```

**관계형: 멀티유저 동시성이 기본**
```sql
-- 여러 사용자가 동시에 같은 데이터 수정
-- 트랜잭션 1 (사용자 A)
BEGIN TRANSACTION;
UPDATE Player SET Gold = Gold + 100 WHERE ID = 1;
COMMIT;

-- 트랜잭션 2 (사용자 B - 동시에 실행)
BEGIN TRANSACTION;
UPDATE Player SET Gold = Gold + 50 WHERE ID = 1;
COMMIT;

-- 데이터베이스가 자동으로 동시성 제어
-- 격리 수준(Isolation Level)에 따라 다른 결과
```

**1.4.5 검증과 무결성의 차이**

**Unity: 응용 로직에서 검증**
```csharp
public class Player : MonoBehaviour
{
    private int level;

    public int Level
    {
        get => level;
        set
        {
            // 응용 코드가 직접 검증
            if(value < 1 || value > 100)
                throw new ArgumentException("Invalid level");
            level = value;
        }
    }

    // 문제: 다른 방법으로 level을 수정하면?
    // 문제: 직렬화/역직렬화 시 검증 우회?
}
```

**관계형: 스키마 제약으로 보장**
```sql
CREATE TABLE Player (
    ID INT PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    Level INT CHECK (Level BETWEEN 1 AND 100), -- 데이터베이스가 강제
    Email VARCHAR(255) UNIQUE, -- 중복 불가
    GuildID INT REFERENCES Guild(ID) -- 외래 키 제약
);

-- 제약 위반 시 데이터베이스가 거부
INSERT INTO Player (ID, Name, Level) VALUES (1, 'Alice', 150);
-- 에러: Level CHECK 제약 위반

-- 응용 프로그램은 검증 로직 불필요
```

**요약: Unity vs 관계형 DB**

| 측면 | Unity | 관계형 DB |
|------|-------|-----------|
| 데이터 생명주기 | 임시적 (세션 동안) | 영구적 (디스크 저장) |
| 접근 방법 | 탐색 기반 (포인터) | 값 기반 (조인) |
| 결합도 | 강한 결합 | 느슨한 결합 (데이터 독립성) |
| 동시성 | 단일 스레드 (기본) | 멀티유저 (기본) |
| 무결성 | 응용 로직 | 스키마 제약 |
| 쿼리 스타일 | 명령형 | 선언형 |
| 최적화 | 수동 | 자동 (쿼리 옵티마이저) |

---

### 🌆 오후 세션 (3시간): 개념적 모델링과 ER 모델

#### 1.5 Peter Chen의 Entity-Relationship 모델 (45분)

**1.5.1 ER 모델의 탄생 배경**

관계형 모델이 이론적 기초를 제공했지만, 실제 세계를 데이터베이스로 변환하는 과정은 여전히 어려웠다. 1976년 **Peter Chen**이 발표한 **Entity-Relationship Model**은 이 간극을 메웠다.

Chen의 통찰: "데이터베이스 설계는 두 단계로 나뉜다"
1. **개념적 설계(Conceptual Design)**: 실제 세계를 모델링
2. **논리적 설계(Logical Design)**: 개념적 모델을 관계형 스키마로 변환

**1.5.2 ER 모델의 기본 요소**

**엔티티(Entity)**:
- 독립적으로 존재하는 객체
- Unity의 GameObject와 유사하지만 더 추상적

```
[Player]
- ID
- Name
- Level
- Email
```

**관계(Relationship)**:
- 엔티티들 간의 연관
- Unity의 참조와 유사하지만 양방향 의미 포함

```
Player ─── Owns ─── Item
```

**속성(Attribute)**:
- 엔티티나 관계의 특성
- Unity의 필드와 유사

```
Player
├── ID (Key Attribute - 밑줄)
├── Name
├── Level
└── Email
```

**1.5.3 카디널리티(Cardinality)**

관계의 수적 제약을 표현한다.

**일대일 (1:1)**:
```
Player ──1:1── Account
```
- 한 플레이어는 정확히 하나의 계정을 가짐
- 한 계정은 정확히 하나의 플레이어를 가짐

Unity 비유:
```csharp
public class Player : MonoBehaviour
{
    public Account account; // 1:1 관계
}

public class Account
{
    public Player player; // 양방향 참조 (선택적)
}
```

**일대다 (1:N)**:
```
Player ──1:N── Character
```
- 한 플레이어는 여러 캐릭터를 가질 수 있음
- 한 캐릭터는 정확히 하나의 플레이어를 가짐

Unity 비유:
```csharp
public class Player : MonoBehaviour
{
    public List<Character> characters; // 1:N 관계
}

public class Character
{
    public Player owner; // 역참조
}
```

**다대다 (M:N)**:
```
Player ──M:N── Quest
```
- 한 플레이어는 여러 퀘스트를 수행
- 한 퀘스트는 여러 플레이어가 수행

Unity에서의 문제:
```csharp
// Unity에서 M:N은 복잡함
public class Player : MonoBehaviour
{
    public List<Quest> activeQuests; // Player → Quest
}

public class Quest
{
    public List<Player> participants; // Quest → Player

    // 문제: 양쪽을 동기화해야 함!
}
```

관계형에서의 해결:
```sql
-- 중간 테이블(Junction Table)로 해결
CREATE TABLE PlayerQuest (
    PlayerID INT REFERENCES Player(ID),
    QuestID INT REFERENCES Quest(ID),
    StartDate DATE,
    Status VARCHAR(20),
    PRIMARY KEY (PlayerID, QuestID)
);
```

**1.5.4 참여 제약(Participation Constraint)**

**전체 참여(Total Participation)**: 모든 엔티티가 관계에 참여해야 함
```
Player ==|| Owns ||== Inventory
(이중선: 모든 플레이어는 반드시 인벤토리를 가져야 함)
```

**부분 참여(Partial Participation)**: 일부만 참여 가능
```
Player ──|| JoinedTo ||── Guild
(단일선: 플레이어는 길드에 가입하지 않을 수 있음)
```

SQL로 표현:
```sql
CREATE TABLE Player (
    ID INT PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    InventoryID INT NOT NULL REFERENCES Inventory(ID), -- 전체 참여 (NOT NULL)
    GuildID INT REFERENCES Guild(ID) -- 부분 참여 (NULL 허용)
);
```

**1.5.5 약한 엔티티(Weak Entity)**

독립적으로 존재할 수 없는 엔티티

```
[Player] ──── [PlayerStats]
(강한 엔티티)  (약한 엔티티 - 이중 네모)
```

PlayerStats는 Player 없이 의미가 없다:
```sql
CREATE TABLE PlayerStats (
    PlayerID INT,
    Strength INT,
    Dexterity INT,
    PRIMARY KEY (PlayerID),
    FOREIGN KEY (PlayerID) REFERENCES Player(ID) ON DELETE CASCADE
);
```

Unity 비유:
```csharp
public class Player : MonoBehaviour
{
    public Stats stats; // Player의 일부 (약한 엔티티)
}

// Stats는 독립적으로 존재할 수 없음
```

---

#### 1.6 개념적/논리적/물리적 모델링의 3단계 (45분)

**1.6.1 개념적 모델링(Conceptual Modeling)**

**목적**: 실제 세계의 요구사항을 추상화
**도구**: ER 다이어그램
**특징**: 구현 기술에 독립적

예시: IdleRPG의 개념적 모델
```
[Player] ──1:N── [Character] ──M:N── [Quest]
   |                  |
  1:1               1:N
   |                  |
[Account]        [Equipment]
```

Unity 설계 단계와 유사:
```
게임 설계 문서 작성
  ↓
GameObject 계층 구조 스케치
  ↓
컴포넌트 관계 정의
```

**1.6.2 논리적 모델링(Logical Modeling)**

**목적**: 개념적 모델을 관계형 스키마로 변환
**도구**: 관계형 스키마(테이블, 제약조건)
**특징**: 특정 DBMS에 독립적

변환 규칙:

**1. 엔티티 → 테이블**
```
[Player]        →    CREATE TABLE Player (
- ID                     ID INT PRIMARY KEY,
- Name                   Name VARCHAR(100),
- Level                  Level INT
                     );
```

**2. 1:1 관계 → 외래 키**
```
Player ──1:1── Account

방법 1: 한 쪽에 외래 키
CREATE TABLE Player (
    ID INT PRIMARY KEY,
    AccountID INT UNIQUE REFERENCES Account(ID)
);

방법 2: 테이블 병합
CREATE TABLE PlayerAccount (
    ID INT PRIMARY KEY,
    -- Player 속성
    -- Account 속성
);
```

**3. 1:N 관계 → 외래 키 (N 쪽에)**
```
Player ──1:N── Character

CREATE TABLE Character (
    ID INT PRIMARY KEY,
    Name VARCHAR(100),
    PlayerID INT NOT NULL REFERENCES Player(ID) -- N 쪽에 외래 키
);
```

**4. M:N 관계 → 중간 테이블**
```
Player ──M:N── Quest

CREATE TABLE PlayerQuest (
    PlayerID INT REFERENCES Player(ID),
    QuestID INT REFERENCES Quest(ID),
    Status VARCHAR(20),
    StartDate DATE,
    PRIMARY KEY (PlayerID, QuestID)
);
```

**1.6.3 물리적 모델링(Physical Modeling)**

**목적**: 성능 최적화를 위한 구현 세부사항
**도구**: 인덱스, 파티셔닝, 클러스터링
**특징**: 특정 DBMS에 의존적

PostgreSQL 구현 예시:
```sql
-- 인덱스 추가 (쿼리 최적화)
CREATE INDEX idx_player_level ON Player(Level);
CREATE INDEX idx_character_player ON Character(PlayerID);

-- 파티셔닝 (대용량 데이터 분할)
CREATE TABLE PlayerLog (
    ID BIGSERIAL,
    PlayerID INT,
    Action VARCHAR(100),
    CreatedAt TIMESTAMP
) PARTITION BY RANGE (CreatedAt);

CREATE TABLE PlayerLog_2024_01 PARTITION OF PlayerLog
FOR VALUES FROM ('2024-01-01') TO ('2024-02-01');

-- 제약조건 추가
ALTER TABLE Player ADD CONSTRAINT chk_level
    CHECK (Level BETWEEN 1 AND 100);
```

Unity에는 이러한 단계 구분이 없다:
```csharp
// Unity: 개념 설계와 구현이 동시에
public class Player : MonoBehaviour // 설계 = 구현
{
    public int ID;
    public string Name;
    public int Level; // 검증 로직도 여기 포함
}
```

**1.6.4 정규화(Normalization) 개요**

논리적 모델링의 중요한 기법 (Day 2에서 상세히)

**목적**: 데이터 중복 제거, 이상 현상 방지

**비정규화된 테이블**:
```sql
CREATE TABLE PlayerCharacter (
    CharacterID INT,
    CharacterName VARCHAR(100),
    PlayerID INT,
    PlayerName VARCHAR(100),  -- 중복!
    PlayerEmail VARCHAR(255), -- 중복!
    CharacterLevel INT
);
```

**문제점**:
1. **갱신 이상**: PlayerName 변경 시 모든 행 수정 필요
2. **삽입 이상**: 캐릭터 없는 플레이어 저장 불가
3. **삭제 이상**: 마지막 캐릭터 삭제 시 플레이어 정보도 삭제

**정규화된 테이블**:
```sql
CREATE TABLE Player (
    PlayerID INT PRIMARY KEY,
    PlayerName VARCHAR(100),
    PlayerEmail VARCHAR(255)
);

CREATE TABLE Character (
    CharacterID INT PRIMARY KEY,
    CharacterName VARCHAR(100),
    PlayerID INT REFERENCES Player(PlayerID),
    CharacterLevel INT
);
```

Unity에서도 유사한 원칙:
```csharp
// 나쁜 설계: 중복 데이터
public class Character : MonoBehaviour
{
    public int characterID;
    public string characterName;
    public int playerID;
    public string playerName; // 중복!
    public string playerEmail; // 중복!
}

// 좋은 설계: 참조로 분리
public class Character : MonoBehaviour
{
    public int characterID;
    public string characterName;
    public Player player; // 참조로 중복 제거
}
```

---

#### 1.7 Unity의 Component 관계 vs Database의 Foreign Key 철학 (45분)

**1.7.1 참조 vs 값의 근본적 차이**

**Unity의 참조 모델**:
```csharp
public class Player : MonoBehaviour
{
    public Weapon weapon; // 메모리 주소를 가리킴
}

// 참조의 특성:
// 1. 포인터: weapon은 메모리 주소
// 2. Null 가능: weapon이 삭제되면 null
// 3. 양방향 탐색 가능 (설계에 따라)
// 4. 캐스팅: weapon을 Sword로 다운캐스트 가능
```

**Database의 값 모델**:
```sql
CREATE TABLE Player (
    ID INT PRIMARY KEY,
    WeaponID INT REFERENCES Weapon(ID) -- 값(정수)을 저장
);

-- 값의 특성:
-- 1. 스칼라: WeaponID는 그냥 정수
-- 2. Null 가능 (제약에 따라)
-- 3. 조인으로만 연결
-- 4. 타입 안전성 (정수는 정수)
```

**왜 값인가?**

관계형 모델은 **데이터 독립성**을 위해 물리적 위치(포인터)가 아닌 논리적 값(키)을 사용한다.

```
Unity (포인터):
Player.weapon → [메모리 0x1234]에 있는 Weapon 객체

만약 Weapon이 메모리 재배치되면?
→ 포인터가 깨질 수 있음 (물리적 의존성)

Database (값):
Player.WeaponID → 5 (논리적 키)

Weapon 테이블이 재구성되어도?
→ ID=5인 무기를 찾으면 됨 (물리적 독립성)
```

**1.7.2 생명주기 관리의 차이**

**Unity의 수동 생명주기**:
```csharp
public class InventoryManager : MonoBehaviour
{
    public List<Item> items;

    public void RemoveItem(Item item)
    {
        items.Remove(item);
        Destroy(item.gameObject); // 수동 삭제

        // 문제: 다른 곳에서 참조하고 있으면?
        // → NullReferenceException 발생 가능
    }
}

public class Player : MonoBehaviour
{
    public Item favoriteItem; // 삭제된 item을 참조하면?

    void UseItem()
    {
        if(favoriteItem != null) // null 체크 필수
        {
            favoriteItem.Use();
        }
    }
}
```

**Database의 선언적 생명주기**:
```sql
CREATE TABLE Player (
    ID INT PRIMARY KEY,
    FavoriteItemID INT REFERENCES Item(ID) ON DELETE SET NULL
    -- ON DELETE SET NULL: Item 삭제 시 자동으로 NULL로 설정
    -- ON DELETE CASCADE: Item 삭제 시 Player도 삭제
    -- ON DELETE RESTRICT: Item을 참조하는 Player가 있으면 삭제 거부
);

-- Item 삭제
DELETE FROM Item WHERE ID = 5;

-- 데이터베이스가 자동으로:
-- 1. Player.FavoriteItemID를 NULL로 설정 (SET NULL)
-- 또는 2. Player도 함께 삭제 (CASCADE)
-- 또는 3. 삭제 거부 (RESTRICT)
```

**1.7.3 일관성 보장의 차이**

**Unity: 응용 로직이 책임**:
```csharp
public class Player : MonoBehaviour
{
    public Guild guild;

    public void JoinGuild(Guild newGuild)
    {
        // 응용 로직이 직접 양방향 일관성 유지
        if(guild != null)
        {
            guild.members.Remove(this); // 이전 길드에서 제거
        }

        guild = newGuild;

        if(newGuild != null)
        {
            newGuild.members.Add(this); // 새 길드에 추가
        }

        // 문제: 이 로직을 빠뜨리면 불일치 발생!
        // 문제: 멀티스레드 환경에서 경합 조건 발생 가능!
    }
}
```

**Database: 시스템이 보장**:
```sql
-- 외래 키 제약이 자동으로 일관성 보장
CREATE TABLE Player (
    ID INT PRIMARY KEY,
    GuildID INT REFERENCES Guild(ID)
);

-- 유효하지 않은 Guild는 삽입 불가
INSERT INTO Player (ID, GuildID) VALUES (1, 999);
-- 에러: Guild(999)가 존재하지 않음

-- Guild 삭제 시 자동 처리
DELETE FROM Guild WHERE ID = 5;
-- ON DELETE CASCADE면 해당 Guild의 모든 Player 삭제
-- ON DELETE SET NULL이면 Player.GuildID를 NULL로 설정
-- ON DELETE RESTRICT면 삭제 거부
```

**1.7.4 다형성의 차이**

**Unity: 객체지향 다형성**:
```csharp
public abstract class Item : MonoBehaviour
{
    public abstract void Use();
}

public class Potion : Item
{
    public override void Use()
    {
        // 체력 회복
    }
}

public class Weapon : Item
{
    public override void Use()
    {
        // 장착
    }
}

public class Inventory : MonoBehaviour
{
    public List<Item> items; // 다형성: Potion과 Weapon 모두 저장

    public void UseItem(int index)
    {
        items[index].Use(); // 런타임 다형성
    }
}
```

**Database: 상속 매핑 전략**:

관계형 데이터베이스는 상속을 직접 지원하지 않는다. 여러 전략으로 우회:

**전략 1: Table Per Hierarchy (TPH)**
```sql
-- 모든 타입을 하나의 테이블에
CREATE TABLE Item (
    ID INT PRIMARY KEY,
    ItemType VARCHAR(20), -- 'Potion' or 'Weapon'
    -- Potion 속성
    HealAmount INT,
    -- Weapon 속성
    AttackPower INT,
    WeaponType VARCHAR(20)
    -- 문제: Null이 많음 (Potion은 AttackPower가 NULL)
);
```

**전략 2: Table Per Type (TPT)**
```sql
-- 타입별로 테이블 분리
CREATE TABLE Item (
    ID INT PRIMARY KEY,
    Name VARCHAR(100)
);

CREATE TABLE Potion (
    ID INT PRIMARY KEY REFERENCES Item(ID),
    HealAmount INT
);

CREATE TABLE Weapon (
    ID INT PRIMARY KEY REFERENCES Item(ID),
    AttackPower INT,
    WeaponType VARCHAR(20)
);

-- 문제: 조회 시 JOIN 필요
SELECT * FROM Item
LEFT JOIN Potion ON Item.ID = Potion.ID
LEFT JOIN Weapon ON Item.ID = Weapon.ID;
```

**전략 3: Table Per Concrete Type (TPC)**
```sql
-- 구체 타입별로 완전히 분리
CREATE TABLE Potion (
    ID INT PRIMARY KEY,
    Name VARCHAR(100),
    HealAmount INT
);

CREATE TABLE Weapon (
    ID INT PRIMARY KEY,
    Name VARCHAR(100),
    AttackPower INT,
    WeaponType VARCHAR(20)
);

-- 문제: "모든 아이템" 쿼리가 복잡
SELECT ID, Name, 'Potion' as Type FROM Potion
UNION ALL
SELECT ID, Name, 'Weapon' as Type FROM Weapon;
```

Entity Framework Core의 매핑:
```csharp
// TPH 설정
modelBuilder.Entity<Item>()
    .HasDiscriminator<string>("ItemType")
    .HasValue<Potion>("Potion")
    .HasValue<Weapon>("Weapon");

// TPT 설정
modelBuilder.Entity<Potion>().ToTable("Potion");
modelBuilder.Entity<Weapon>().ToTable("Weapon");
```

---

#### 1.8 실습: ER 다이어그램 그리기 (45분)

**1.8.1 IdleRPG의 Week 1 요구사항 분석**

Week 1 PRD 요구사항:
```
- 플레이어는 최대 3개의 캐릭터를 가질 수 있다
- 각 캐릭터는 이름, 레벨, 경험치를 가진다
- 캐릭터는 스탯(Strength, Dexterity, Intelligence, Vitality)을 가진다
- 레벨업 시 스탯 포인트를 받는다
```

**1.8.2 엔티티 식별**

```
[Player]
- ID (PK)
- Username
- Email
- PasswordHash

[Character]
- ID (PK)
- PlayerID (FK)
- Name
- Level
- Experience
- StatPoints
- Gold
- CreatedAt

[CharacterStats] (약한 엔티티 또는 Owned Entity)
- CharacterID (PK, FK)
- Strength
- Dexterity
- Intelligence
- Vitality
```

**1.8.3 관계 식별**

```
Player ──1:N── Character
(한 플레이어는 최대 3개 캐릭터, 한 캐릭터는 한 플레이어에게만 속함)

Character ──1:1── CharacterStats
(한 캐릭터는 정확히 하나의 스탯 세트)
```

**1.8.4 ER 다이어그램**

```
┌─────────┐
│ Player  │
├─────────┤
│ ID (PK) │
│ Username│
│ Email   │
│ PwdHash │
└────┬────┘
     │
     │ 1
     │
     │ N (최대 3)
     │
┌────┴────────┐
│  Character  │
├─────────────┤
│ ID (PK)     │
│ PlayerID(FK)│
│ Name        │
│ Level       │
│ Experience  │
│ StatPoints  │
│ Gold        │
│ CreatedAt   │
└──────┬──────┘
       │
       │ 1
       │
       │ 1
       │
┌──────┴──────────┐
│ CharacterStats  │
├─────────────────┤
│ CharacterID(PK) │
│ Strength        │
│ Dexterity       │
│ Intelligence    │
│ Vitality        │
└─────────────────┘
```

**1.8.5 논리적 스키마로 변환**

```sql
CREATE TABLE Player (
    ID UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Username VARCHAR(100) NOT NULL UNIQUE,
    Email VARCHAR(255) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    LastLoginAt TIMESTAMP
);

CREATE TABLE Character (
    ID UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    PlayerID UUID NOT NULL REFERENCES Player(ID) ON DELETE CASCADE,
    Name VARCHAR(100) NOT NULL,
    Level INT NOT NULL DEFAULT 1 CHECK (Level BETWEEN 1 AND 100),
    Experience INT NOT NULL DEFAULT 0 CHECK (Experience >= 0),
    StatPoints INT NOT NULL DEFAULT 0 CHECK (StatPoints >= 0),
    Gold INT NOT NULL DEFAULT 0 CHECK (Gold >= 0),
    CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT unique_character_name UNIQUE (Name),
    CONSTRAINT max_3_characters_per_player CHECK (
        (SELECT COUNT(*) FROM Character WHERE PlayerID = Character.PlayerID) <= 3
    )
);

CREATE TABLE CharacterStats (
    CharacterID UUID PRIMARY KEY REFERENCES Character(ID) ON DELETE CASCADE,
    Strength INT NOT NULL DEFAULT 10 CHECK (Strength >= 0),
    Dexterity INT NOT NULL DEFAULT 10 CHECK (Dexterity >= 0),
    Intelligence INT NOT NULL DEFAULT 10 CHECK (Intelligence >= 0),
    Vitality INT NOT NULL DEFAULT 10 CHECK (Vitality >= 0)
);

-- 인덱스 (물리적 모델링)
CREATE INDEX idx_character_player ON Character(PlayerID);
CREATE INDEX idx_character_level ON Character(Level);
```

**Unity 대응 구조**:
```csharp
// Unity에서 동일한 구조 (메모리 기반)
public class Player
{
    public Guid ID { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public List<Character> Characters { get; set; } // 1:N

    public bool CanCreateCharacter()
    {
        return Characters.Count < 3; // 응용 로직으로 제약
    }
}

public class Character
{
    public Guid ID { get; set; }
    public Guid PlayerID { get; set; } // 외래 키
    public Player Player { get; set; } // 역참조 (선택적)
    public string Name { get; set; }
    public int Level { get; set; }
    public int Experience { get; set; }
    public CharacterStats Stats { get; set; } // 1:1
}

public class CharacterStats // Value Object 패턴
{
    public int Strength { get; set; }
    public int Dexterity { get; set; }
    public int Intelligence { get; set; }
    public int Vitality { get; set; }
}
```

**1.8.6 설계 결정 사항**

**Q: CharacterStats를 별도 테이블로 분리해야 하는가?**

옵션 1: 별도 테이블 (현재 설계)
```sql
-- 장점: 정규화, 확장 용이
-- 단점: JOIN 필요, 복잡도 증가
SELECT c.Name, cs.Strength
FROM Character c
JOIN CharacterStats cs ON c.ID = cs.CharacterID;
```

옵션 2: Character 테이블에 포함
```sql
CREATE TABLE Character (
    ID UUID PRIMARY KEY,
    -- ... 기타 컬럼
    Strength INT,
    Dexterity INT,
    Intelligence INT,
    Vitality INT
);

-- 장점: 쿼리 단순, 성능 향상
-- 단점: 테이블이 넓어짐, 비정규화
```

**결정**: Week 1에서는 옵션 2 (Character에 포함)가 더 실용적
- 스탯은 항상 캐릭터와 함께 조회됨
- 스탯만 독립적으로 쿼리할 일이 없음
- JOIN 오버헤드 제거

Entity Framework의 **Owned Entity** 패턴으로 구현 가능 (Day 4에서 상세히)

---

## Day 1 요약 및 다음 단계 Preview

### 📝 오늘 배운 핵심 개념

1. **데이터베이스의 역사적 발전**
   - 파일 시스템 → 계층형 → 네트워크 → 관계형
   - 각 단계의 한계와 극복 과정

2. **E.F. Codd의 관계형 모델**
   - 집합론과 관계 대수 기반
   - 데이터 독립성의 철학
   - 선언적 쿼리 언어

3. **관계 대수**
   - Selection, Projection, Join, Union, Difference
   - SQL의 이론적 기반
   - 관계적 완전성

4. **Peter Chen의 ER 모델**
   - 엔티티, 관계, 속성
   - 카디널리티와 참여 제약
   - 개념적/논리적/물리적 모델링

5. **Unity vs Database 패러다임**
   - 탐색 vs 값 기반
   - 참조 vs 외래 키
   - 객체지향 vs 관계형

### 🎯 내일 배울 내용 (Day 2)

**오전**: 관계 대수와 SQL의 수학적 기초
- 관계 대수 연산자의 수학적 증명
- SQL의 선언형 패러다임 심화
- 3-valued logic과 NULL의 철학
- 튜링 완전성과 SQL의 한계

**오후**: 정규화 이론의 수학적 기초
- 함수 종속성과 Armstrong's Axioms
- 1NF, 2NF, 3NF, BCNF의 정의와 증명
- 다치 종속성과 4NF, 5NF
- 정규화 vs 비정규화 트레이드오프

### ✅ 학습 점검 체크리스트

- [ ] 관계형 모델이 왜 혁명적이었는지 설명할 수 있다
- [ ] 관계, 튜플, 속성을 수학적으로 정의할 수 있다
- [ ] 데이터 독립성의 개념을 이해하고 설명할 수 있다
- [ ] 관계 대수의 기본 연산자를 사용할 수 있다
- [ ] ER 다이어그램을 그리고 관계형 스키마로 변환할 수 있다
- [ ] Unity의 객체 모델과 관계형 모델의 차이를 이해한다

### 📚 추가 학습 자료

**필독 논문**:
- E.F. Codd (1970): "A Relational Model of Data for Large Shared Data Banks"
- Peter Chen (1976): "The Entity-Relationship Model - Toward a Unified View of Data"

**권장 도서**:
- "Database System Concepts" - Silberschatz, Korth, Sudarshan (6장: ER Model)
- "An Introduction to Database Systems" - C.J. Date (5장: Relational Algebra)

**Unity 개발자를 위한 추천**:
- Martin Fowler: "Patterns of Enterprise Application Architecture" (3장: Mapping to Relational Databases)

---

## Day 2: 관계 대수, SQL의 수학적 기초, 정규화 이론

### 🌅 오전 세션 (3시간): 관계 대수와 SQL의 이론적 기초

#### 2.1 관계 대수 연산자의 수학적 증명 (45분)

**2.1.1 관계 대수의 표현력(Expressive Power)**

Day 1에서 배운 5개 기본 연산자 {σ, π, ×, ∪, −}가 정말 모든 쿼리를 표현할 수 있을까? 이를 증명하기 위해 **관계 대수의 완전성(Relational Completeness)**을 이해해야 한다.

**정리 1: 관계 대수의 완전성**
관계 대수는 **1차 술어 논리(First-Order Predicate Logic)**의 안전한 부분집합과 표현력이 동등하다.

수학적 정의:
```
관계 대수로 표현 가능한 쿼리 ≡ 안전한 1차 논리 쿼리

안전한(safe) 쿼리: 유한한 결과를 보장하는 쿼리
```

예시: "30레벨 이상인 전사들"

1차 논리:
```
{p | Player(p) ∧ p.Class = 'Warrior' ∧ p.Level ≥ 30}
```

관계 대수:
```
σ_Class='Warrior' ∧ Level≥30(Player)
```

**2.1.2 조인의 결합 법칙과 교환 법칙**

조인은 교환 법칙과 결합 법칙을 만족한다. 이는 쿼리 최적화의 이론적 기반이다.

**정리 2: 조인의 교환 법칙**
```
R ⋈ S = S ⋈ R
```

증명:
```
R ⋈ S
= {(r, s) | r ∈ R ∧ s ∈ S ∧ r.A = s.A}  (조인의 정의)
= {(s, r) | s ∈ S ∧ r ∈ R ∧ s.A = r.A}  (논리곱의 교환 법칙)
= S ⋈ R
```

Unity 비유:
```csharp
// LINQ에서도 동일한 성질
var result1 = players.Join(guilds, p => p.GuildID, g => g.ID, (p, g) => new {p, g});
var result2 = guilds.Join(players, g => g.ID, p => p.GuildID, (g, p) => new {p, g});
// result1과 result2는 같은 데이터 (순서만 다를 수 있음)
```

**정리 3: 조인의 결합 법칙**
```
(R ⋈ S) ⋈ T = R ⋈ (S ⋈ T)
```

이는 쿼리 옵티마이저가 조인 순서를 자유롭게 변경할 수 있는 이론적 근거다.

```sql
-- 다음 두 실행 계획은 결과가 같다
-- 계획 1: (Player ⋈ Character) ⋈ Quest
-- 계획 2: Player ⋈ (Character ⋈ Quest)

-- 데이터베이스는 비용이 낮은 계획을 선택
SELECT * FROM Player
JOIN Character ON Player.ID = Character.PlayerID
JOIN Quest ON Character.ID = Quest.CharacterID;
```

**2.1.3 Selection의 분할과 이동**

Selection 연산은 여러 최적화 규칙을 가진다.

**규칙 1: Selection 분할**
```
σ_조건1∧조건2(R) = σ_조건1(σ_조건2(R))
```

**규칙 2: Selection 교환**
```
σ_조건1(σ_조건2(R)) = σ_조건2(σ_조건1(R))
```

**규칙 3: Selection 푸시다운(Push-down)**
```
σ_조건(R ⋈ S) = (σ_조건(R)) ⋈ S  (조건이 R에만 관련될 때)
```

이는 **가능한 한 일찍 필터링**하는 최적화 기법의 이론적 기반이다.

예시:
```sql
-- 비효율적: 모든 조인 후 필터링
SELECT * FROM Player
JOIN Character ON Player.ID = Character.PlayerID
WHERE Player.Level >= 50;

-- 효율적: 조인 전에 필터링 (옵티마이저가 자동 수행)
-- σ_Level≥50(Player ⋈ Character)
-- = (σ_Level≥50(Player)) ⋈ Character
```

Unity에서의 유사한 최적화:
```csharp
// 비효율적: 모든 조합 후 필터
var result = players
    .SelectMany(p => characters.Where(c => c.PlayerID == p.ID), (p, c) => new {p, c})
    .Where(x => x.p.Level >= 50);

// 효율적: 먼저 필터링
var result = players
    .Where(p => p.Level >= 50)
    .SelectMany(p => characters.Where(c => c.PlayerID == p.ID), (p, c) => new {p, c});
```

**2.1.4 Projection의 성질**

Projection은 멱등성(Idempotence)을 가진다.

**정리 4: Projection의 멱등성**
```
π_A(π_A(R)) = π_A(R)
```

증명:
```
π_A(π_A(R))
= {t.A | t ∈ π_A(R)}
= {t.A | t ∈ {s.A | s ∈ R}}
= {t.A | t.A ∈ {s.A | s ∈ R}}
= {s.A | s ∈ R}
= π_A(R)
```

하지만 다른 속성 집합으로 Projection을 반복하면 정보가 손실된다:
```
π_A(π_A,B(R)) ≠ π_A,B(π_A(R))  (일반적으로)
```

SQL 예시:
```sql
-- 멱등성
SELECT Name FROM (SELECT Name FROM Player);
-- = SELECT Name FROM Player;

-- 정보 손실
SELECT Name FROM (SELECT Name, Level FROM Player);
-- Level 정보가 손실됨 (복구 불가)
```

---

#### 2.2 SQL의 선언형 패러다임 심화 (45분)

**2.2.1 명령형 vs 선언형의 근본적 차이**

**명령형(Imperative) - Unity C#**:
```csharp
// "어떻게(How)" 할지를 명시
List<string> highLevelWarriors = new List<string>();
foreach(var player in allPlayers)  // 1. 반복 방법 명시
{
    if(player.Class == "Warrior")   // 2. 검사 순서 명시
    {
        if(player.Level >= 50)      // 3. 중첩 조건
        {
            highLevelWarriors.Add(player.Name);  // 4. 저장 방법 명시
        }
    }
}
// 5. 정렬도 명시적 알고리즘 필요
highLevelWarriors.Sort();
```

**선언형(Declarative) - SQL**:
```sql
-- "무엇을(What)" 원하는지만 명시
SELECT Name FROM Player
WHERE Class = 'Warrior' AND Level >= 50
ORDER BY Name;

-- "어떻게"는 옵티마이저가 결정:
-- - 인덱스를 사용할지?
-- - 어떤 조인 알고리즘?
-- - 메모리 정렬 vs 디스크 정렬?
-- - 병렬 실행 여부?
```

**선언형의 장점**:
1. **최적화 가능**: 실행 방법을 시스템이 선택
2. **간결함**: 의도가 명확
3. **병렬화 용이**: 실행 순서가 고정되지 않음
4. **유지보수성**: 구현 세부사항에 독립적

**2.2.2 SQL의 평가 순서**

SQL은 선언적이지만, 논리적 평가 순서는 정해져 있다.

```sql
SELECT Name, AVG(Level) as AvgLevel    -- 5. 최종 프로젝션
FROM Player                             -- 1. 데이터 소스
WHERE Class = 'Warrior'                 -- 2. 행 필터링
GROUP BY GuildID                        -- 3. 그룹화
HAVING AVG(Level) >= 30                 -- 4. 그룹 필터링
ORDER BY AvgLevel DESC                  -- 6. 정렬
LIMIT 10;                               -- 7. 제한
```

**논리적 평가 순서** (중요!):
```
FROM → WHERE → GROUP BY → HAVING → SELECT → ORDER BY → LIMIT
```

Unity에는 이런 명확한 순서가 없다:
```csharp
// Unity: 직접 순서를 제어
var result = players
    .Where(p => p.Class == "Warrior")     // 먼저 필터
    .GroupBy(p => p.GuildID)              // 그룹화
    .Select(g => new {
        GuildID = g.Key,
        AvgLevel = g.Average(p => p.Level)
    })
    .Where(x => x.AvgLevel >= 30)         // 그룹 필터
    .OrderByDescending(x => x.AvgLevel)   // 정렬
    .Take(10);                            // 제한
```

**2.2.3 집합 의미론 vs Multiset 의미론**

관계 대수는 **집합(Set)** 의미론을 가지지만, SQL은 **다중집합(Multiset, Bag)** 의미론을 가진다.

**집합 의미론** (관계 대수):
```
π_Class(Player) = {'Warrior', 'Mage', 'Archer'}
-- 중복 자동 제거
```

**Multiset 의미론** (SQL):
```sql
SELECT Class FROM Player;
-- 결과:
-- Warrior
-- Warrior
-- Mage
-- Warrior
-- Archer
-- 중복 허용!

-- 집합 의미론을 원하면 DISTINCT 명시
SELECT DISTINCT Class FROM Player;
```

**왜 Multiset인가?**

성능 때문이다. 중복 제거는 정렬 또는 해싱이 필요하므로 비용이 크다.

```sql
-- 중복 제거 없음 (빠름)
SELECT Name FROM Player;  -- O(n)

-- 중복 제거 (느림)
SELECT DISTINCT Name FROM Player;  -- O(n log n) 또는 O(n) with hash
```

Unity LINQ도 동일:
```csharp
// 중복 허용 (빠름)
var names = players.Select(p => p.Name);

// 중복 제거 (느림)
var uniqueNames = players.Select(p => p.Name).Distinct();
```

**2.2.4 NULL의 3-valued Logic**

SQL의 가장 논란이 많은 특징 중 하나가 **NULL**이다.

**전통적 2-valued Logic**:
```
True, False
```

**SQL의 3-valued Logic**:
```
True, False, Unknown (NULL)
```

**진리표**:

AND 연산:
```
AND     | True    | False   | Unknown
--------|---------|---------|--------
True    | True    | False   | Unknown
False   | False   | False   | False
Unknown | Unknown | False   | Unknown
```

OR 연산:
```
OR      | True    | False   | Unknown
--------|---------|---------|--------
True    | True    | True    | True
False   | True    | False   | Unknown
Unknown | True    | Unknown | Unknown
```

NOT 연산:
```
NOT True    = False
NOT False   = True
NOT Unknown = Unknown
```

**실무 예시**:
```sql
-- Player.GuildID가 NULL인 경우
SELECT * FROM Player
WHERE GuildID = 5 OR GuildID <> 5;

-- 예상: 모든 플레이어 반환?
-- 실제: GuildID가 NULL인 플레이어는 제외됨!
-- NULL <> 5 → Unknown
-- Unknown OR False → Unknown
-- Unknown은 WHERE 절에서 False처럼 처리됨
```

**올바른 NULL 처리**:
```sql
-- NULL을 명시적으로 처리
SELECT * FROM Player
WHERE GuildID = 5 OR GuildID <> 5 OR GuildID IS NULL;

-- 또는 COALESCE 사용
SELECT * FROM Player
WHERE COALESCE(GuildID, -1) = 5;
```

Unity에서는 NULL이 명확하다:
```csharp
// C#의 Nullable<T>
int? guildID = null;

if(guildID == 5)  // false
if(guildID != 5)  // false
// null은 어떤 값과도 같지 않음 (명확한 의미론)

// Null 체크
if(guildID == null)  // true
if(guildID.HasValue) // false
```

**Tony Hoare의 "Billion Dollar Mistake"**:

NULL의 발명자 Tony Hoare는 2009년 다음과 같이 말했다:
> "I call it my billion-dollar mistake. It was the invention of the null reference in 1965... This has led to innumerable errors, vulnerabilities, and system crashes, which have probably caused a billion dollars of pain and damage in the last forty years."

---

#### 2.3 튜링 완전성과 SQL의 한계 (45분)

**2.3.1 튜링 완전성의 정의**

**튜링 완전(Turing Complete)**: 튜링 머신으로 계산 가능한 모든 것을 계산할 수 있는 시스템

예시:
- C#: 튜링 완전 (반복, 조건, 함수 등)
- SQL: 튜링 불완전 (표준 SQL-92 기준)

**2.3.2 SQL이 튜링 불완전한 이유**

**1. 반복문 부재**

표준 SQL에는 `while`, `for` 같은 반복문이 없다.

```sql
-- SQL로 1부터 10까지 합을 구하려면?
-- 불가능! (재귀 CTE 없이는)

-- C#은 간단
int sum = 0;
for(int i = 1; i <= 10; i++)
{
    sum += i;
}
```

**2. 가변 변수 부재**

SQL에는 할당문이 없다 (표준 SQL 기준).

```sql
-- SQL에서는 불가능
-- SET @x = @x + 1;  (T-SQL, PL/SQL 확장)

-- C#은 당연히 가능
int x = 0;
x = x + 1;
```

**3. 종료 조건 없는 반복 불가**

튜링 머신의 핵심은 **무한 반복**과 **조건부 종료**다. SQL은 이를 보장하지 않는다.

**2.3.3 재귀 CTE로 일부 극복**

SQL:1999부터 **재귀 공통 테이블 식(Recursive CTE)**을 지원하여 일부 반복 계산이 가능해졌다.

```sql
-- 1부터 10까지의 합
WITH RECURSIVE Numbers AS (
    SELECT 1 as n, 1 as sum    -- 기저 사례
    UNION ALL
    SELECT n + 1, sum + (n + 1) -- 재귀 사례
    FROM Numbers
    WHERE n < 10                -- 종료 조건
)
SELECT sum FROM Numbers WHERE n = 10;
```

**하지만 여전히 제한적**:
1. 재귀 깊이 제한 (무한 재귀 방지)
2. 복잡한 알고리즘 표현 어려움
3. 성능 문제

**2.3.4 PL/pgSQL, T-SQL 등의 확장**

각 DBMS는 튜링 완전한 **프로시저 언어**를 제공한다.

**PostgreSQL의 PL/pgSQL**:
```sql
CREATE FUNCTION calculate_fibonacci(n INT) RETURNS INT AS $$
DECLARE
    a INT := 0;
    b INT := 1;
    temp INT;
    i INT;
BEGIN
    FOR i IN 1..n LOOP
        temp := a + b;
        a := b;
        b := temp;
    END LOOP;
    RETURN a;
END;
$$ LANGUAGE plpgsql;
```

**하지만 이는 표준 SQL이 아니다**:
- DBMS마다 문법이 다름
- 이식성 저하
- 일반적으로 응용 로직으로 처리하는 것이 권장됨

Unity C#과 비교:
```csharp
// Unity: 어떤 알고리즘이든 표현 가능
public int Fibonacci(int n)
{
    int a = 0, b = 1;
    for(int i = 0; i < n; i++)
    {
        int temp = a + b;
        a = b;
        b = temp;
    }
    return a;
}
```

**2.3.5 SQL의 의도적 제한**

SQL이 튜링 불완전한 것은 **버그가 아니라 설계 의도**다.

**이유**:
1. **종료 보장**: 모든 쿼리는 유한 시간 내에 종료됨 (이론적으로)
2. **최적화 가능**: 선언적이므로 옵티마이저가 자유롭게 최적화
3. **보안**: 무한 루프로 서버 다운시키기 어려움
4. **병렬화**: 실행 순서가 고정되지 않아 병렬 실행 용이

**교훈**:
- 복잡한 알고리즘은 응용 계층에서 (C#, Unity)
- 데이터 조작만 SQL에서 (선택, 조인, 집계)

```csharp
// 좋은 설계: 역할 분리
public class CharacterService
{
    // C#: 비즈니스 로직
    public void LevelUp(Character character)
    {
        character.Level++;
        character.Experience = 0;
        int statPoints = CalculateStatPoints(character.Level); // 복잡한 계산
        character.StatPoints += statPoints;

        // SQL: 데이터 저장만
        _repository.Update(character);
    }

    private int CalculateStatPoints(int level)
    {
        // 복잡한 피보나치 기반 계산 (C#에서)
        return Fibonacci(level) / 10;
    }
}
```

---

#### 2.4 LINQ to Objects vs LINQ to Entities (45분)

**2.4.1 IEnumerable vs IQueryable의 근본적 차이**

Unity 개발자가 가장 혼란스러워하는 부분이다.

**IEnumerable<T>** (LINQ to Objects):
```csharp
// Unity의 메모리 컬렉션
List<Player> players = GetAllPlayers(); // 모든 데이터 로드됨!

IEnumerable<Player> query = players
    .Where(p => p.Level >= 50); // 메모리에서 필터링

// 실행 시점
foreach(var player in query) // 여기서 실제 반복
{
    Console.WriteLine(player.Name);
}
```

**IQueryable<T>** (LINQ to Entities):
```csharp
// EF Core의 데이터베이스 쿼리
IQueryable<Player> query = _dbContext.Players
    .Where(p => p.Level >= 50); // 아직 실행 안됨!

// SQL 생성
// SELECT * FROM Player WHERE Level >= 50

foreach(var player in query) // 여기서 SQL 실행
{
    Console.WriteLine(player.Name);
}
```

**핵심 차이**:

| 특성 | IEnumerable | IQueryable |
|------|-------------|------------|
| 데이터 위치 | 메모리 | 데이터베이스 |
| 실행 시점 | 즉시 | 지연 (Deferred) |
| 필터링 위치 | 클라이언트 (C#) | 서버 (SQL) |
| Expression | 델리게이트 | Expression Tree |
| 성능 | 모든 데이터 로드 | 필요한 데이터만 |

**2.4.2 Expression Tree의 마법**

IQueryable은 **Expression Tree**를 사용하여 C# 코드를 SQL로 변환한다.

```csharp
// IEnumerable: 델리게이트 (컴파일된 코드)
IEnumerable<Player> enumerable = players;
var result1 = enumerable.Where(p => p.Level >= 50);
// Func<Player, bool> predicate = (컴파일된 IL 코드)

// IQueryable: Expression Tree (코드의 구조)
IQueryable<Player> queryable = _dbContext.Players;
var result2 = queryable.Where(p => p.Level >= 50);
// Expression<Func<Player, bool>> expression = (코드의 추상 구문 트리)
```

**Expression Tree 구조**:
```
BinaryExpression (>=)
├─ MemberExpression (p.Level)
└─ ConstantExpression (50)
```

EF Core는 이 트리를 순회하며 SQL을 생성:
```csharp
// 내부적으로 이런 과정
void TranslateToSql(Expression expr)
{
    if(expr is BinaryExpression binary)
    {
        if(binary.NodeType == ExpressionType.GreaterThanOrEqual)
        {
            sql += TranslateMember(binary.Left);
            sql += " >= ";
            sql += TranslateConstant(binary.Right);
        }
    }
}

// 결과: "Level >= 50"
```

**2.4.3 지연 실행(Deferred Execution)의 함정**

```csharp
// 함정 1: 의도치 않은 다중 실행
public IEnumerable<Player> GetHighLevelPlayers()
{
    return _dbContext.Players.Where(p => p.Level >= 50);
    // 쿼리 정의만, 실행 안됨
}

void PrintPlayers()
{
    var players = GetHighLevelPlayers();

    Console.WriteLine($"Count: {players.Count()}"); // 쿼리 실행 1
    foreach(var p in players) // 쿼리 실행 2 (다시!)
    {
        Console.WriteLine(p.Name);
    }
    // DB에 같은 쿼리 2번!
}

// 해결: ToList()로 즉시 실행
public List<Player> GetHighLevelPlayers()
{
    return _dbContext.Players
        .Where(p => p.Level >= 50)
        .ToList(); // 여기서 쿼리 실행, 결과를 메모리에
}
```

**함정 2: Client Evaluation**

```csharp
// 위험: 복잡한 메서드는 SQL로 변환 불가
var players = _dbContext.Players
    .Where(p => ComplexCalculation(p.Level) > 100) // C# 메서드!
    .ToList();

// EF Core 3.0 이전: 모든 Player를 메모리로 가져와서 필터링 (느림!)
// EF Core 3.0 이후: 예외 발생 (안전)

// 해결: SQL로 변환 가능한 코드만 사용
var players = _dbContext.Players
    .AsEnumerable() // 명시적으로 클라이언트 평가로 전환
    .Where(p => ComplexCalculation(p.Level) > 100)
    .ToList();
```

Unity에는 이런 문제가 없다:
```csharp
// Unity: 모든 것이 메모리에서 즉시 실행
var players = allPlayers
    .Where(p => ComplexCalculation(p.Level) > 100); // 문제없음

foreach(var p in players) // 즉시 필터링됨
{
    // ...
}
```

**2.4.4 최적화 전략**

**전략 1: 프로젝션 먼저**
```csharp
// 나쁨: 전체 엔티티 로드
var players = _dbContext.Players
    .Where(p => p.Level >= 50)
    .ToList();
// SELECT * FROM Player WHERE Level >= 50

// 좋음: 필요한 컬럼만
var playerNames = _dbContext.Players
    .Where(p => p.Level >= 50)
    .Select(p => p.Name)
    .ToList();
// SELECT Name FROM Player WHERE Level >= 50
```

**전략 2: AsNoTracking**
```csharp
// 기본: 변경 추적 활성화 (느림)
var players = _dbContext.Players.ToList();

// 읽기 전용: 변경 추적 비활성화 (빠름)
var players = _dbContext.Players
    .AsNoTracking()
    .ToList();
```

**전략 3: 적절한 시점에 ToList()**
```csharp
// 너무 이르게 ToList() (나쁨)
var players = _dbContext.Players.ToList() // 모든 플레이어 로드!
    .Where(p => p.Level >= 50); // 메모리에서 필터링

// 적절한 시점 (좋음)
var players = _dbContext.Players
    .Where(p => p.Level >= 50) // SQL에서 필터링
    .ToList(); // 필터링된 결과만 로드
```

---

### 🌆 오후 세션 (3시간): 정규화 이론의 수학적 기초

#### 2.5 함수 종속성(Functional Dependency)과 Armstrong's Axioms (45분)

**2.5.1 함수 종속성의 정의**

**정의**: 속성 집합 X가 속성 집합 Y를 **함수적으로 결정(functionally determine)**한다는 것은, X 값이 같은 튜플들은 반드시 Y 값도 같다는 의미다.

표기: `X → Y`

수학적 정의:
```
R에서 X → Y ⟺ ∀t₁, t₂ ∈ R, t₁[X] = t₂[X] ⇒ t₁[Y] = t₂[Y]
```

예시:
```
Player(PlayerID, Username, Email, Level)

함수 종속성:
PlayerID → Username  (PlayerID가 같으면 Username도 같음)
PlayerID → Email
PlayerID → Level
Username → PlayerID  (Username이 유일하므로)
Email → PlayerID
```

Unity 비유:
```csharp
public class Player
{
    public Guid PlayerID { get; set; }  // Key
    public string Username { get; set; }
    public string Email { get; set; }
    public int Level { get; set; }
}

// PlayerID를 알면 나머지를 결정할 수 있음
Player GetPlayer(Guid playerID)
{
    // playerID → username, email, level
    return FindPlayer(playerID);
}
```

**2.5.2 완전 함수 종속성 vs 부분 함수 종속성**

**완전 함수 종속성(Full Functional Dependency)**:
```
X → Y이고, X의 어떤 부분집합도 Y를 결정하지 않음
```

**부분 함수 종속성(Partial Functional Dependency)**:
```
X → Y이지만, X의 일부만으로도 Y를 결정할 수 있음
```

예시:
```
CharacterQuest(CharacterID, QuestID, CharacterName, QuestName, Status)

Primary Key: (CharacterID, QuestID)

함수 종속성:
(CharacterID, QuestID) → Status  (완전 종속)
CharacterID → CharacterName      (부분 종속!)
QuestID → QuestName              (부분 종속!)
```

**문제**: CharacterName은 CharacterID만으로 결정되므로, 복합키의 일부에만 종속된다.

**2.5.3 이행적 함수 종속성(Transitive Functional Dependency)**

**정의**:
```
X → Y이고 Y → Z일 때, X → Z (이행 종속)
단, Y → X는 아님 (비자명)
```

예시:
```
Player(PlayerID, GuildID, GuildName, Level)

PlayerID → GuildID
GuildID → GuildName
∴ PlayerID → GuildName (이행 종속)
```

**문제**: GuildName은 PlayerID를 통해 간접적으로만 결정된다.

**2.5.4 Armstrong's Axioms**

William Armstrong (1974)이 증명한 함수 종속성의 추론 규칙:

**기본 공리(Axioms)**:

**1. 반사성(Reflexivity)**:
```
Y ⊆ X ⇒ X → Y
```
예: `(PlayerID, Username) → PlayerID`

**2. 증가성(Augmentation)**:
```
X → Y ⇒ XZ → YZ
```
예: `PlayerID → Level ⇒ (PlayerID, GuildID) → (Level, GuildID)`

**3. 이행성(Transitivity)**:
```
X → Y ∧ Y → Z ⇒ X → Z
```
예: `PlayerID → GuildID ∧ GuildID → GuildName ⇒ PlayerID → GuildName`

**파생 규칙(Derived Rules)**:

**4. 합집합(Union)**:
```
X → Y ∧ X → Z ⇒ X → YZ
```

증명:
```
X → Y         (주어짐)
⇒ X → XY      (증가성: X로 증가)
X → Z         (주어짐)
⇒ XY → YZ     (증가성: Y로 증가)
X → XY ∧ XY → YZ ⇒ X → YZ (이행성)
```

**5. 분해(Decomposition)**:
```
X → YZ ⇒ X → Y ∧ X → Z
```

증명:
```
X → YZ        (주어짐)
YZ → Y        (반사성)
X → YZ ∧ YZ → Y ⇒ X → Y (이행성)
(X → Z도 동일하게 증명)
```

**2.5.5 함수 종속성 추론 예제**

주어진 종속성:
```
F = {A → B, B → C, CD → E}
```

**질문**: `A → C`를 추론할 수 있는가?

추론:
```
1. A → B        (주어짐)
2. B → C        (주어짐)
3. A → C        (1, 2에 이행성 적용)
```

**질문**: `A → E`를 추론할 수 있는가?

추론:
```
1. A → B        (주어짐)
2. B → C        (주어짐)
3. A → C        (1, 2에 이행성)
4. CD → E       (주어짐)
5. A → AC       (3에 증가성, A로 증가)
6. AC → CE      (4에 증가성, C로 증가) - 틀림!

실제로는:
CD → E에서 C만으로는 E를 추론 불가
따라서 A → E 추론 불가
```

Unity에서의 유사 개념:
```csharp
// 함수 종속성 = 함수 호출 체인
public class Player
{
    public Guild GetGuild() { ... }  // Player → Guild
}

public class Guild
{
    public string GetGuildName() { ... }  // Guild → GuildName
}

// 이행성: Player → Guild → GuildName
string guildName = player.GetGuild().GetGuildName();
```

---

#### 2.6 정규형의 수학적 정의 (45분)

**2.6.1 제1정규형 (1NF)**

**정의**: 모든 속성이 **원자적(atomic)** 값만을 가진다.

**위반 예**:
```
Player(PlayerID, Name, Skills)

튜플:
(1, "Alice", "Fireball, Ice Storm, Teleport")  -- Skills가 리스트!
```

Unity에서도 비슷한 문제:
```csharp
// 나쁨: 배열을 문자열로 저장
public class Player
{
    public string Skills { get; set; } // "Fireball,Ice Storm,Teleport"
}

// 좋음: 별도 테이블
public class Player
{
    public List<Skill> Skills { get; set; }
}
```

**1NF 변환**:
```
Player(PlayerID, Name)
PlayerSkill(PlayerID, SkillID)
Skill(SkillID, SkillName)
```

**2.6.2 제2정규형 (2NF)**

**정의**: 1NF이고, **부분 함수 종속성이 없음**

즉, 모든 비키 속성이 기본키 전체에 완전 함수 종속이어야 한다.

**위반 예**:
```
CharacterQuest(CharacterID, QuestID, CharacterName, QuestName, Status)

Primary Key: (CharacterID, QuestID)

CharacterID → CharacterName  (부분 종속!)
QuestID → QuestName          (부분 종속!)
```

**문제점**:
1. **갱신 이상**: CharacterName 변경 시 모든 행 수정 필요
2. **삽입 이상**: Quest를 시작하지 않은 Character는 저장 불가
3. **삭제 이상**: 마지막 Quest 삭제 시 Character 정보도 삭제

**2NF 변환**:
```
Character(CharacterID, CharacterName)
Quest(QuestID, QuestName)
CharacterQuest(CharacterID, QuestID, Status)
```

수학적 증명:
```
2NF 조건:
∀ 비키 속성 A, ∀ 키 K의 부분집합 X,
X → A ⇒ X = K (완전 종속)

변환 후:
Character: CharacterID → CharacterName (완전, 키가 단일 속성)
Quest: QuestID → QuestName (완전)
CharacterQuest: (CharacterID, QuestID) → Status (완전)
```

**2.6.3 제3정규형 (3NF)**

**정의**: 2NF이고, **이행적 함수 종속성이 없음**

**위반 예**:
```
Player(PlayerID, GuildID, GuildName, Level)

PlayerID → GuildID (직접 종속)
GuildID → GuildName (직접 종속)
∴ PlayerID → GuildName (이행 종속!)
```

**문제점**:
1. **갱신 이상**: 길드명 변경 시 모든 멤버 행 수정
2. **삽입 이상**: 멤버 없는 길드 저장 불가
3. **삭제 이상**: 마지막 멤버 탈퇴 시 길드 정보 삭제

**3NF 변환**:
```
Player(PlayerID, GuildID, Level)
Guild(GuildID, GuildName)
```

수학적 정의:
```
3NF 조건:
∀ 함수 종속성 X → A,
다음 중 하나를 만족:
1. X가 슈퍼키이거나
2. A가 키의 일부이거나
3. 이행 종속이 아님
```

Unity 설계 원칙과 유사:
```csharp
// 나쁨: 중복 데이터
public class Player
{
    public Guid GuildID { get; set; }
    public string GuildName { get; set; } // 중복!
    public string GuildDescription { get; set; } // 중복!
}

// 좋음: 참조로 분리
public class Player
{
    public Guid GuildID { get; set; }
    public Guild Guild { get; set; } // Navigation Property
}

public class Guild
{
    public Guid GuildID { get; set; }
    public string GuildName { get; set; }
    public string Description { get; set; }
}
```

**2.6.4 Boyce-Codd 정규형 (BCNF)**

**정의**: 모든 함수 종속성 `X → Y`에 대해, X가 슈퍼키이다.

3NF보다 엄격하다.

**3NF이지만 BCNF가 아닌 예**:
```
CourseInstructor(Course, Instructor, Room)

함수 종속성:
(Course, Instructor) → Room
Instructor → Room  (강사는 고정 강의실 사용)
```

분석:
```
Instructor → Room에서
Instructor는 슈퍼키가 아님! (BCNF 위반)

하지만 Room은 키의 일부가 아니므로 3NF는 만족
```

**BCNF 변환**:
```
CourseInstructor(Course, Instructor)
InstructorRoom(Instructor, Room)
```

**문제**: 종속성 손실 가능!
```
원래: (Course, Instructor) → Room
분해 후: Instructor → Room만 보존됨
Course와 Room의 관계가 손실될 수 있음
```

이것이 **무손실 분해(Lossless Decomposition)**의 중요성이다.

---

#### 2.7 고급 정규형과 다치 종속성 (45분)

**2.7.1 다치 종속성(Multivalued Dependency)**

함수 종속성보다 일반적인 개념이다.

**정의**: `X ↠ Y` (X가 Y를 다치 결정)
```
X의 각 값에 대해, Y 값들의 집합이 Z 값들과 독립적으로 결정됨
```

예시:
```
Instructor(InstructorID, Course, Hobby)

한 강사는:
- 여러 과목을 가르칠 수 있고
- 여러 취미를 가질 수 있다
- 과목과 취미는 서로 독립적

InstructorID ↠ Course
InstructorID ↠ Hobby
```

데이터:
```
InstructorID | Course  | Hobby
-------------|---------|--------
I1           | DB      | Tennis
I1           | DB      | Hiking
I1           | AI      | Tennis
I1           | AI      | Hiking
```

**문제**: 불필요한 중복!
- I1이 새 과목을 맡으면? 모든 취미마다 행 추가
- I1이 새 취미를 가지면? 모든 과목마다 행 추가

**2.7.2 제4정규형 (4NF)**

**정의**: BCNF이고, **비자명 다치 종속성이 없음**

즉, 모든 다치 종속성 `X ↠ Y`에 대해 X가 슈퍼키이거나 종속성이 자명해야 한다.

**4NF 변환**:
```
InstructorCourse(InstructorID, Course)
InstructorHobby(InstructorID, Hobby)
```

이제 독립적:
```
InstructorCourse:
I1 | DB
I1 | AI

InstructorHobby:
I1 | Tennis
I1 | Hiking

새 과목 추가: InstructorCourse에만 INSERT
새 취미 추가: InstructorHobby에만 INSERT
```

**2.7.3 제5정규형 (5NF, PJNF)**

**정의**: 4NF이고, **조인 종속성**을 만족

매우 희귀한 경우에만 적용된다.

예시 (3개 이상 관계):
```
ProjectAssignment(Project, Part, Supplier)

제약:
- 프로젝트 P는 부품 A를 사용
- 부품 A는 공급자 S가 공급
- 공급자 S는 프로젝트 P에 납품
⇒ (P, A, S) 튜플 존재
```

이 경우 3개 테이블로 무손실 분해 가능:
```
ProjectPart(Project, Part)
PartSupplier(Part, Supplier)
SupplierProject(Supplier, Project)
```

**실무에서는 3NF/BCNF까지만**:
- 4NF, 5NF는 희귀
- 성능상 비정규화가 필요할 수 있음
- 응용 로직으로 제약 보장 가능

---

#### 2.8 정규화 vs 비정규화 트레이드오프 (45분)

**2.8.1 정규화의 장점**

1. **데이터 무결성**
```sql
-- 정규화: 길드명 변경이 한 곳에서
UPDATE Guild SET GuildName = 'NewName' WHERE GuildID = 1;

-- 비정규화: 모든 플레이어 행 수정 필요
UPDATE Player SET GuildName = 'NewName' WHERE GuildID = 1;
```

2. **저장 공간 절약**
```
비정규화:
Player(PlayerID, Name, GuildID, GuildName, GuildDescription, ...)
1000명 멤버 → GuildName과 Description이 1000번 중복

정규화:
Player(PlayerID, Name, GuildID)
Guild(GuildID, GuildName, Description)
1000명 멤버 → GuildName과 Description은 1번만
```

3. **갱신 이상 방지**

**2.8.2 정규화의 단점**

1. **조인 오버헤드**
```sql
-- 정규화: 조인 필요 (느림)
SELECT p.Name, g.GuildName
FROM Player p
JOIN Guild g ON p.GuildID = g.GuildID;

-- 비정규화: 조인 불필요 (빠름)
SELECT Name, GuildName FROM Player;
```

2. **복잡한 쿼리**
```sql
-- 정규화: 여러 테이블 조인
SELECT p.Name, g.GuildName, c.CharacterName, i.ItemName
FROM Player p
JOIN Guild g ON p.GuildID = g.GuildID
JOIN Character c ON p.PlayerID = c.PlayerID
JOIN Inventory inv ON c.CharacterID = inv.CharacterID
JOIN Item i ON inv.ItemID = i.ItemID;

-- 비정규화: 단순 SELECT
SELECT * FROM PlayerFullView;
```

**2.8.3 게임 서버에서의 비정규화 전략**

**전략 1: 읽기 전용 비정규화**
```sql
-- 정규화된 원본 (쓰기용)
CREATE TABLE Player (
    PlayerID UUID PRIMARY KEY,
    GuildID UUID REFERENCES Guild(GuildID)
);

CREATE TABLE Guild (
    GuildID UUID PRIMARY KEY,
    GuildName VARCHAR(100)
);

-- 비정규화된 뷰 (읽기용)
CREATE MATERIALIZED VIEW PlayerWithGuild AS
SELECT p.PlayerID, p.Name, g.GuildName
FROM Player p
LEFT JOIN Guild g ON p.GuildID = g.GuildID;

-- 주기적 갱신
REFRESH MATERIALIZED VIEW PlayerWithGuild;
```

**전략 2: 계산된 컬럼 비정규화**
```sql
-- 정규화: 매번 계산
SELECT PlayerID,
       (SELECT COUNT(*) FROM Character WHERE PlayerID = Player.PlayerID) as CharCount
FROM Player;

-- 비정규화: 미리 저장
CREATE TABLE Player (
    PlayerID UUID PRIMARY KEY,
    CharacterCount INT DEFAULT 0  -- 비정규화!
);

-- 트리거로 동기화
CREATE TRIGGER update_character_count
AFTER INSERT ON Character
FOR EACH ROW
UPDATE Player SET CharacterCount = CharacterCount + 1
WHERE PlayerID = NEW.PlayerID;
```

Unity에서의 유사 패턴:
```csharp
public class Player : MonoBehaviour
{
    public List<Character> Characters { get; set; }

    // 계산 속성 (정규화)
    public int CharacterCount => Characters.Count;

    // 캐시 필드 (비정규화)
    private int _cachedCharacterCount;
    public int CachedCharacterCount
    {
        get
        {
            if(_cachedCharacterCount == 0)
                _cachedCharacterCount = Characters.Count;
            return _cachedCharacterCount;
        }
    }

    public void AddCharacter(Character c)
    {
        Characters.Add(c);
        _cachedCharacterCount++; // 동기화
    }
}
```

**전략 3: 선택적 비정규화 (Hot Data)**
```sql
-- 자주 조회되는 데이터만 비정규화
CREATE TABLE Player (
    PlayerID UUID PRIMARY KEY,
    Name VARCHAR(100),
    -- 자주 조회됨: 비정규화
    CurrentGuildName VARCHAR(100),
    -- 가끔 조회됨: 정규화 유지
    GuildID UUID REFERENCES Guild(GuildID)
);
```

**2.8.4 정규화 의사결정 가이드**

**정규화를 유지해야 할 때**:
- 쓰기가 많은 데이터
- 마스터 데이터 (길드, 아이템 템플릿 등)
- 일관성이 중요한 데이터 (재화, 레벨 등)

**비정규화를 고려할 때**:
- 읽기가 압도적으로 많음 (90% 이상)
- 조인 비용이 너무 큼
- 실시간 성능이 중요함 (랭킹, 리더보드 등)

**IdleRPG Week 1 결정**:
```sql
-- Character와 Stats를 분리할까?

-- 옵션 1: 정규화 (별도 테이블)
CREATE TABLE Character (...);
CREATE TABLE CharacterStats (
    CharacterID UUID PRIMARY KEY REFERENCES Character(ID),
    Strength INT,
    ...
);

-- 옵션 2: 비정규화 (통합)
CREATE TABLE Character (
    ID UUID PRIMARY KEY,
    Name VARCHAR(100),
    Level INT,
    Strength INT,  -- Stats를 Character에 포함
    Dexterity INT,
    ...
);

-- 결정: 옵션 2 (비정규화)
-- 이유:
-- 1. Stats는 항상 Character와 함께 조회됨
-- 2. Stats만 독립적으로 조회할 일 없음
-- 3. 1:1 관계이므로 중복 없음
-- 4. 조인 오버헤드 제거
```

---

## Day 2 요약 및 다음 단계 Preview

### 📝 오늘 배운 핵심 개념

1. **관계 대수의 수학적 증명**
   - 조인의 교환/결합 법칙
   - Selection 푸시다운
   - 쿼리 최적화의 이론적 기반

2. **SQL의 선언형 패러다임**
   - 명령형 vs 선언형
   - 평가 순서
   - 집합 vs Multiset 의미론
   - NULL의 3-valued logic

3. **튜링 완전성**
   - SQL이 튜링 불완전한 이유
   - 재귀 CTE로 일부 극복
   - 프로시저 언어 확장

4. **LINQ to Objects vs LINQ to Entities**
   - IEnumerable vs IQueryable
   - Expression Tree
   - 지연 실행의 함정
   - Client Evaluation

5. **함수 종속성**
   - 함수 종속성의 정의
   - Armstrong's Axioms
   - 완전/부분/이행 종속성

6. **정규형**
   - 1NF: 원자성
   - 2NF: 부분 종속성 제거
   - 3NF: 이행 종속성 제거
   - BCNF: 모든 결정자가 슈퍼키
   - 4NF: 다치 종속성 제거

7. **정규화 vs 비정규화**
   - 트레이드오프
   - 게임 서버 전략
   - 의사결정 가이드

### 🎯 내일 배울 내용 (Day 3)

**오전**: Object-Relational Impedance Mismatch
- 객체 정체성 vs 값 동등성
- 객체 그래프 탐색 vs 조인 연산
- 상속 vs 테이블 매핑
- 캡슐화 vs 데이터 노출

**오후**: ORM의 역사와 철학적 논쟁
- 1990년대 EJB의 실패
- Hibernate의 등장
- Active Record vs Data Mapper
- Ted Neward의 "Vietnam of Computer Science"
- 언제 ORM을 쓰고, 언제 Raw SQL을 써야 하는가

### ✅ 학습 점검 체크리스트

- [ ] 관계 대수 연산자의 수학적 성질을 설명할 수 있다
- [ ] SQL의 논리적 평가 순서를 이해한다
- [ ] NULL의 3-valued logic을 다룰 수 있다
- [ ] IEnumerable과 IQueryable의 차이를 안다
- [ ] Expression Tree의 개념을 이해한다
- [ ] 함수 종속성을 식별하고 Armstrong's Axioms를 적용할 수 있다
- [ ] 1NF, 2NF, 3NF, BCNF로 정규화할 수 있다
- [ ] 정규화와 비정규화의 트레이드오프를 이해한다

### 📚 추가 학습 자료

**필독 논문**:
- William Armstrong (1974): "Dependency Structures of Data Base Relationships"
- Ronald Fagin (1977): "Multivalued Dependencies and a New Normal Form for Relational Databases" (4NF)

**권장 도서**:
- "Database System Concepts" - Silberschatz (7장: Relational Database Design)
- "An Introduction to Database Systems" - C.J. Date (11장: Functional Dependencies)

---

**다음**: Day 3 - ORM의 역사와 Object-Relational Impedance Mismatch

---
