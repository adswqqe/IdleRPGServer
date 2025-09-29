# Stage 1: 웹 서버 기초 - Unity 개발자를 위한 입문 (5-7일 과정)

## 🎯 학습 목표
Unity 개발자가 웹 서버 개발의 핵심 개념을 이해하고, IdleRPG 프로젝트를 통해 실제 구현해보는 것

## 📚 사전 준비사항
- Unity C# 경험 (코루틴, MonoBehaviour 생명주기 이해)
- 기본적인 객체지향 프로그래밍 지식
- HTTP 요청/응답에 대한 기본 이해

---

## Day 1: 분산 시스템과 클라이언트-서버 모델의 이론적 기초

### 🌅 오전 세션 (3시간): 컴퓨팅 패러다임의 근본적 이해

#### 1.1 컴퓨터 시스템 아키텍처의 진화와 철학 (45분)

**서론: 단일 프로세스에서 분산 시스템으로의 패러다임 변화**

현대 소프트웨어 개발에서 가장 중요한 개념 중 하나는 **분산 시스템(Distributed System)**의 이해이다. Unity 개발자가 웹 서버 개발로 전환할 때 겪는 가장 큰 어려움은 단순히 문법이나 도구의 차이가 아니라, **컴퓨팅 패러다임 자체의 근본적 차이**에 있다.

**1.1.1 단일 머신 컴퓨팅의 한계와 분산의 필연성**

1940년대 폰 노이만 아키텍처가 확립된 이후, 컴퓨터는 오랫동안 **순차적 실행(Sequential Execution)** 모델을 기반으로 발전해왔다. 이 모델에서 프로그램은 하나의 처리기(processor)에서 명령어를 순서대로 실행하며, 모든 데이터는 단일 메모리 공간에 존재한다.

**Unity의 실행 모델**: Unity는 이러한 전통적인 단일 머신 컴퓨팅 모델의 현대적 구현체이다.

- **중앙집중식 상태 관리**: 모든 게임 객체와 상태가 하나의 메모리 공간에 존재
- **동기적 실행**: 게임 루프는 순차적으로 모든 객체를 업데이트
- **결정론적 행동**: 같은 입력에 대해 항상 같은 결과 보장
- **단일 실패점**: 프로그램이 크래시하면 모든 것이 정지

이러한 모델은 **제어의 단순성**과 **예측 가능성**이라는 장점을 제공하지만, 근본적인 확장성 한계를 가진다:

1. **수직적 확장의 한계**: 단일 머신의 CPU, 메모리, 디스크 성능에 의존
2. **가용성 문제**: 시스템 장애 시 전체 서비스 중단
3. **동시 사용자 제약**: 하나의 게임 인스턴스는 제한된 수의 플레이어만 지원

**분산 시스템의 등장 배경**

1960년대 ARPANET 프로젝트와 함께 시작된 분산 컴퓨팅은 다음 문제들을 해결하고자 했다:

- **자원 공유**: 지리적으로 분산된 컴퓨팅 자원의 효율적 활용
- **내결함성**: 부분적 장애가 전체 시스템에 미치는 영향 최소화
- **확장성**: 수평적 확장을 통한 선형적 성능 향상
- **지연 시간**: 사용자와 가까운 곳에서 서비스 제공

**1.1.2 이벤트 기반 아키텍처의 이론적 기초**

웹 서버는 **이벤트 기반 아키텍처(Event-Driven Architecture)**를 기반으로 한다. 이는 **액터 모델(Actor Model)** 이론과 **반응형 시스템(Reactive Systems)** 설계 원칙에 뿌리를 둔다.

**액터 모델의 핵심 원리** (Carl Hewitt, 1973):

1. **캡슐화**: 각 액터는 독립적인 상태를 가지며 직접적인 접근을 허용하지 않음
2. **메시지 전달**: 액터 간 통신은 오직 비동기 메시지를 통해서만 가능
3. **동시성**: 여러 액터가 동시에 메시지를 처리할 수 있음
4. **위치 투명성**: 액터의 물리적 위치는 통신에 영향을 주지 않음

```
전통적 모델 (Unity):
[메인 스레드] → [게임 루프] → [모든 객체 순차 업데이트]
                     ↓
               [렌더링 파이프라인]

액터 모델 (웹 서버):
[HTTP 요청] → [요청 큐] → [워커 스레드 풀] → [비즈니스 로직 처리]
                            ↓
                      [응답 생성 및 반환]
```

**반응형 시스템의 4가지 특성** (Reactive Manifesto):

1. **응답성(Responsive)**: 적절한 시간 내에 응답
2. **복원력(Resilient)**: 장애 상황에서도 응답성 유지
3. **탄력성(Elastic)**: 작업량 변화에 따른 자원 조정
4. **메시지 기반(Message Driven)**: 비동기 메시지 전달을 통한 느슨한 결합

**1.1.3 상태 관리의 패러다임 차이: 가변성 vs 불변성**

Unity와 웹 서버의 가장 근본적인 차이는 **상태(State)를 다루는 철학**에 있다.

**Unity의 가변 상태 모델**:
- **공유 가변 상태(Shared Mutable State)**: 모든 컴포넌트가 동일한 메모리 공간의 데이터를 직접 수정
- **즉시 일관성(Immediate Consistency)**: 상태 변경이 즉시 모든 곳에 반영
- **순차적 실행**: 상태 변경이 예측 가능한 순서로 발생

**웹 서버의 불변성 지향 모델**:
- **격리된 상태**: 각 요청은 독립적인 실행 컨텍스트에서 처리
- **최종 일관성(Eventual Consistency)**: 분산 환경에서 상태 동기화는 시간이 걸림
- **함수형 접근**: 상태 변경보다는 새로운 상태 생성을 선호

이러한 차이는 **CAP 정리(CAP Theorem)**로 설명할 수 있다:
- **일관성(Consistency)**: 모든 노드가 같은 시간에 같은 데이터를 봄
- **가용성(Availability)**: 시스템이 항상 작동함
- **분할 내성(Partition Tolerance)**: 네트워크 분할 상황에서도 동작

Unity는 단일 노드 환경에서 일관성과 가용성을 보장하지만, 웹 서버는 분산 환경에서 분할 내성을 위해 일관성과 가용성 사이의 트레이드오프를 선택해야 한다.

**1.1.4 시간의 개념: 논리적 시간 vs 물리적 시간**

**Unity의 물리적 시간 모델**:
```csharp
// Unity에서 시간은 게임 루프와 동기화됨
void Update()
{
    float deltaTime = Time.deltaTime; // 마지막 프레임 이후 경과 시간
    transform.position += velocity * deltaTime; // 물리적 시간 기반 계산
}
```

Unity에서 시간은 **물리적이고 연속적**이다. 모든 객체가 동일한 시간축에서 동기적으로 업데이트되며, `Time.deltaTime`을 통해 정확한 시간 계산이 가능하다.

**분산 시스템의 논리적 시간 모델**:

분산 시스템에서는 **물리적 시간의 동기화가 불가능**하다. 이를 해결하기 위해 Leslie Lamport가 제안한 **논리적 시계(Logical Clock)** 개념을 사용한다.

- **벡터 시계(Vector Clock)**: 각 노드가 자신과 다른 노드들의 이벤트 순서를 추적
- **인과 관계(Causality)**: 이벤트 간의 선후 관계를 시간이 아닌 의존성으로 판단
- **동시성 감지**: 물리적으로 동시에 발생한 이벤트도 논리적으로는 순서가 있을 수 있음

```
Unity 시간 모델:
T0 → T1 → T2 → T3 (선형적, 전역적 시간)

분산 시스템 시간 모델:
Node A: A1 → A2 → A3
Node B: B1 → B2 → B3
Node C: C1 → C2 → C3
(각 노드마다 독립적인 시간축, 이벤트 간 인과관계로만 순서 결정)
```

**1.1.5 실습: 패러다임 차이 체험하기**

다음 시나리오를 통해 두 패러다임의 차이를 체험해보자:

**시나리오**: "플레이어가 레벨업할 때 다른 플레이어들에게 알림을 보내는 시스템"

**Unity 접근법 (중앙집중식)**:

```csharp
public class GameManager : MonoBehaviour
{
    // 모든 플레이어 정보가 메모리에 상주
    public List<Player> allPlayers = new List<Player>();

    public void PlayerLevelUp(Player player, int newLevel)
    {
        // 즉시 상태 변경
        player.level = newLevel;

        // 동기적으로 모든 플레이어에게 알림
        foreach(Player otherPlayer in allPlayers)
        {
            if(otherPlayer != player)
            {
                otherPlayer.ShowNotification($"{player.name}님이 {newLevel}레벨이 되었습니다!");
            }
        }

        // 즉시 UI 업데이트
        UpdateLeaderboard();
        SaveGameData(); // 즉시 저장
    }
}
```

**웹 서버 접근법 (분산 이벤트 기반)**:

```csharp
public class PlayerService : IPlayerService
{
    public async Task<LevelUpResult> ProcessLevelUpAsync(Guid playerId)
    {
        // 1. 독립적 트랜잭션에서 레벨업 처리
        using var transaction = await _dbContext.BeginTransactionAsync();

        var player = await _playerRepository.GetByIdAsync(playerId);
        player.Level++;
        await _playerRepository.UpdateAsync(player);

        // 2. 이벤트 생성 (다른 플레이어들에게는 나중에 전파)
        var levelUpEvent = new PlayerLevelUpEvent
        {
            PlayerId = playerId,
            PlayerName = player.Name,
            NewLevel = player.Level,
            Timestamp = DateTime.UtcNow
        };

        // 3. 이벤트 큐에 비동기로 발행
        await _eventPublisher.PublishAsync(levelUpEvent);
        await transaction.CommitAsync();

        return new LevelUpResult { Success = true, NewLevel = player.Level };
    }
}

// 별도 서비스에서 비동기로 알림 처리
public class NotificationService : IEventHandler<PlayerLevelUpEvent>
{
    public async Task HandleAsync(PlayerLevelUpEvent @event)
    {
        // 나중에, 다른 시점에 실행됨
        var onlinePlayers = await _playerRepository.GetOnlinePlayersAsync();

        foreach(var onlinePlayer in onlinePlayers)
        {
            if(onlinePlayer.Id != @event.PlayerId)
            {
                await _pushNotificationService.SendAsync(onlinePlayer.Id,
                    $"{@event.PlayerName}님이 {@event.NewLevel}레벨이 되었습니다!");
            }
        }
    }
}
```

이 예제에서 볼 수 있듯이:
- Unity는 **즉시성**과 **동기성**을 보장하지만 **확장성**에 한계가 있다
- 웹 서버는 **확장성**과 **내결함성**을 위해 **일관성**과 **즉시성**을 포기한다

---

#### 1.2 HTTP 프로토콜의 설계 철학과 통신 이론 (45분)

**1.2.1 통신 프로토콜의 역사적 발전**

**OSI 7계층 모델의 이론적 기반**

1984년 국제표준화기구(ISO)가 제정한 OSI(Open Systems Interconnection) 7계층 모델은 네트워크 통신의 이론적 기반을 제공한다. 각 계층은 **관심사의 분리(Separation of Concerns)** 원칙에 따라 설계되었다:

1. **물리 계층**: 전기적 신호의 전송
2. **데이터 링크 계층**: 인접한 노드 간의 신뢰성 있는 전송
3. **네트워크 계층**: 경로 설정과 패킷 라우팅 (IP)
4. **전송 계층**: 종단 간 신뢰성 있는 데이터 전송 (TCP/UDP)
5. **세션 계층**: 연결의 설정, 관리, 종료
6. **표현 계층**: 데이터 암호화, 압축, 형식 변환
7. **응용 계층**: 사용자 애플리케이션 인터페이스 (HTTP)

**HTTP의 설계 철학**

HTTP(HyperText Transfer Protocol)는 1989년 Tim Berners-Lee가 설계했으며, 다음 원칙들을 기반으로 한다:

**무상태성(Statelessness)의 철학적 기반**:

Roy Fielding의 박사 논문에서 제시된 **REST(Representational State Transfer)** 아키텍처의 핵심 제약사항이다.

```
상태가 있는 통신 (Unity의 TCP 연결):
Client ←---[지속적 연결]---→ Server
       [상태 정보 공유]

상태가 없는 통신 (HTTP):
Client → [요청] → Server
Client ← [응답] ← Server
[매번 독립적인 트랜잭션]
```

**무상태성의 장점**:
- **확장성**: 서버는 클라이언트 상태를 기억할 필요가 없음
- **신뢰성**: 중간에 연결이 끊어져도 다음 요청에 영향 없음
- **캐싱**: 동일한 요청은 동일한 응답을 보장

**무상태성의 단점**:
- **오버헤드**: 매 요청마다 컨텍스트 정보를 전송해야 함
- **복잡성**: 클라이언트가 상태 관리 책임을 져야 함

**1.2.2 요청-응답 모델의 수학적 모델링**

HTTP는 **동기적 요청-응답 모델**을 기반으로 한다. 이는 **원격 프로시저 호출(RPC, Remote Procedure Call)** 패러다임의 구현체이다.

**수학적 모델**:

```
함수형 관점에서의 HTTP:
Response = Server(Request)

여기서:
- Request = (Method, URI, Headers, Body)
- Response = (StatusCode, Headers, Body)
- Server는 순수 함수(Pure Function)처럼 동작
```

**순수 함수의 특성**:
1. **결정론적**: 같은 입력에 대해 항상 같은 출력
2. **부수 효과 없음**: 함수 호출이 외부 상태를 변경하지 않음 (이론적으로)
3. **참조 투명성**: 함수 호출을 결과값으로 대체 가능

**Unity vs HTTP의 호출 모델 비교**:

```csharp
// Unity: 직접 호출 (로컬)
public class GameManager : MonoBehaviour
{
    public void AttackMonster(Monster monster, int damage)
    {
        monster.health -= damage; // 즉시 상태 변경
        if(monster.health <= 0)
        {
            DestroyMonster(monster); // 즉시 실행
        }
    }
}

// HTTP: 원격 호출 (네트워크)
[HttpPost("api/combat/attack")]
public async Task<ActionResult<AttackResult>> AttackMonster(AttackRequest request)
{
    // 네트워크 지연, 직렬화/역직렬화 오버헤드 존재
    var result = await _combatService.ProcessAttackAsync(request.MonsterId, request.Damage);
    return Ok(result); // 클라이언트가 결과를 받아야 상태 변경 확인 가능
}
```

**1.2.3 메시지 형식과 직렬화 이론**

**구조적 데이터의 직렬화 문제**

Unity에서는 객체가 메모리에서 직접 참조되지만, 네트워크 통신에서는 **직렬화(Serialization)**가 필요하다.

**직렬화의 이론적 배경**:
- **마샬링(Marshaling)**: 메모리상의 객체를 바이트 스트림으로 변환
- **언마샬링(Unmarshaling)**: 바이트 스트림을 메모리상의 객체로 복원
- **스키마 진화(Schema Evolution)**: 데이터 구조의 변경에 대한 호환성 유지

**JSON의 설계 철학**:

Douglas Crockford가 설계한 JSON(JavaScript Object Notation)은 다음 원칙을 따른다:

1. **인간 가독성**: 사람이 읽고 쓸 수 있는 텍스트 형식
2. **단순성**: 최소한의 문법으로 구조적 데이터 표현
3. **언어 독립성**: 프로그래밍 언어에 종속되지 않음
4. **자기 서술적**: 스키마 없이도 데이터 구조 파악 가능

```json
// JSON의 재귀적 정의
JSONValue = Object | Array | String | Number | Boolean | null
Object = "{" (String ":" JSONValue)* "}"
Array = "[" JSONValue* "]"
```

**Unity의 메모리 참조 vs HTTP의 값 전달**:

```csharp
// Unity: 메모리 참조 (포인터)
public class Inventory
{
    public List<Item> items; // 메모리 주소를 통한 직접 참조

    public void AddItem(Item item)
    {
        items.Add(item); // O(1) 연산, 메모리 주소만 복사
    }
}

// HTTP: 값 전달 (직렬화)
[HttpPost("api/inventory/add")]
public async Task<ActionResult> AddItem(AddItemRequest request)
{
    // request는 네트워크를 통해 전송된 값의 복사본
    // 원본 클라이언트 객체와는 완전히 별개의 메모리 공간에 존재
    var item = new Item
    {
        Id = request.ItemId,
        Quantity = request.Quantity
        // 모든 필드를 명시적으로 복사해야 함
    };

    await _inventoryService.AddItemAsync(request.PlayerId, item);
    return Ok(); // 응답도 직렬화되어 클라이언트로 전송
}
```

**1.2.4 캐싱 이론과 성능 최적화**

**시간적 지역성과 공간적 지역성**

컴퓨터 시스템에서 캐싱은 **지역성 원리(Principle of Locality)**에 기반한다:

- **시간적 지역성(Temporal Locality)**: 최근에 접근한 데이터는 가까운 시일 내에 다시 접근될 가능성이 높음
- **공간적 지역성(Spatial Locality)**: 접근한 데이터 주변의 데이터도 함께 접근될 가능성이 높음

**HTTP 캐싱 계층**:

```
Browser Cache ← Client-Side Caching
      ↓
CDN Cache ← Geographic Distribution
      ↓
Load Balancer Cache ← Request Distribution
      ↓
Application Cache ← Memory-based Fast Access
      ↓
Database Cache ← Disk I/O Reduction
      ↓
Database ← Persistent Storage
```

**캐시 무효화 문제**:

Phil Karlton의 유명한 말: "컴퓨터 과학에서 어려운 문제는 단 두 가지뿐이다: 캐시 무효화와 이름 짓기"

```
캐시 일관성 문제:
Time 0: Database = A, Cache = A (일관됨)
Time 1: Database = B, Cache = A (불일치!)
Time 2: Cache Invalidated, Cache = B (일관됨 복구)
```

**HTTP 캐싱 헤더의 설계 철학**:

- `Cache-Control: max-age=3600`: **만료 기반 캐싱** (시간적 제어)
- `ETag: "version123"`: **검증 기반 캐싱** (변경 감지)
- `Last-Modified`: **시간 기반 검증** (마지막 수정 시간)

Unity에서는 캐싱이 **투명**하지만(개발자가 신경 쓸 필요 없음), 웹 서버에서는 캐싱이 **명시적**이다(개발자가 직접 제어해야 함).

---

#### 1.3 RESTful 아키텍처의 이론적 기초와 설계 원칙 (45분)

**1.3.1 REST의 철학적 배경: Roy Fielding의 아키텍처 스타일**

**아키텍처 스타일 vs 아키텍처 패턴**

REST는 단순한 API 설계 방법론이 아니라, **분산 하이퍼미디어 시스템을 위한 아키텍처 스타일**이다. Roy Fielding이 2000년 박사 논문에서 제시한 REST는 다음 6가지 제약사항을 정의한다:

**1. 클라이언트-서버 (Client-Server)**
- **관심사의 분리**: 사용자 인터페이스와 데이터 저장의 분리
- **진화의 독립성**: 클라이언트와 서버가 독립적으로 진화 가능
- **이동성**: 클라이언트 컴포넌트의 이식성 향상

```
Unity 모델 (결합도 높음):
[UI] ←직접참조→ [GameLogic] ←직접참조→ [DataStorage]

REST 모델 (결합도 낮음):
[Client] ←HTTP→ [Server API] ←Internal→ [Business Logic] ←Internal→ [Database]
```

**2. 무상태성 (Statelessness)**
- **확장성**: 서버 상태 저장 부담 제거
- **신뢰성**: 부분적 장애에 대한 복원력
- **모니터링**: 각 요청을 독립적으로 추적 가능

**3. 캐시 가능성 (Cacheability)**
- **성능**: 네트워크 통신 비용 감소
- **확장성**: 서버 부하 분산
- **사용자 경험**: 응답 시간 개선

**4. 계층화 시스템 (Layered System)**
- **추상화**: 각 계층은 인접한 계층만 알면 됨
- **보안**: 중간 계층에서 보안 정책 적용
- **로드 밸런싱**: 부하 분산 및 성능 향상

**5. 코드 온 디멘드 (Code-on-Demand) - 선택사항**
- **확장성**: 클라이언트 기능의 동적 확장
- **단순성**: 클라이언트 복잡성 감소

**6. 통합 인터페이스 (Uniform Interface)**
- **단순성**: 시스템 아키텍처 단순화
- **상호운용성**: 독립적 진화 가능

**1.3.2 자원 지향 아키텍처 (Resource-Oriented Architecture)**

**자원(Resource)의 철학적 개념**

REST에서 자원은 **정보의 추상화**이다. 이는 **플라톤의 이데아 이론**과 유사한 개념적 틀을 제공한다:

- **자원**: 개념적 실체 (플레이어, 게임, 점수)
- **표현**: 자원의 구체적 형태 (JSON, XML, HTML)
- **식별자**: 자원의 고유한 이름 (URI)

```
Unity의 객체 지향 vs REST의 자원 지향:

Unity (객체 지향):
class Player {
    int level;
    void LevelUp() { level++; }  // 행동이 객체에 속함
}

REST (자원 지향):
GET /players/123        // 자원의 현재 상태 조회
PUT /players/123/level  // 자원의 특정 속성 수정
POST /players/123/actions/levelup  // 자원에 대한 행동 요청
```

**URI 설계의 이론적 원칙**:

Tim Berners-Lee가 제시한 **웹 아키텍처 원칙**:

1. **단순성**: URI는 단순하고 직관적이어야 함
2. **직교성**: 서로 다른 개념은 서로 다른 URI로 표현
3. **확장성**: 새로운 자원 타입 추가가 용이해야 함

```
좋은 URI 설계:
/players/123/characters/456/equipment/789
└─ 계층구조가 자원 간의 포함관계를 명확히 표현

나쁜 URI 설계:
/getPlayerCharacterEquipment?playerId=123&charId=456&equipId=789
└─ 행동(get) 중심, 쿼리 파라미터 남용
```

**1.3.3 HTTP 메서드의 의미론 (Semantics)**

HTTP 메서드는 단순한 CRUD 매핑이 아니라, **의미론적 제약**을 갖는다:

**멱등성(Idempotence)의 수학적 정의**:

```
함수 f가 멱등적 ⟺ f(f(x)) = f(x) for all x

HTTP 메서드의 멱등성:
GET /players/123    → 몇 번 호출해도 같은 결과
PUT /players/123    → 몇 번 호출해도 최종 상태 동일
DELETE /players/123 → 이미 삭제된 자원을 다시 삭제해도 결과 동일
POST /players       → 호출할 때마다 새로운 플레이어 생성 (멱등적 아님)
```

**안전성(Safety)의 개념**:

안전한 메서드는 **부수 효과(Side Effect)**를 발생시키지 않는다:

```
안전한 메서드: GET, HEAD, OPTIONS
→ 서버 상태를 변경하지 않음 (읽기 전용)

안전하지 않은 메서드: POST, PUT, DELETE, PATCH
→ 서버 상태를 변경할 수 있음
```

**Unity의 즉시 실행 vs REST의 의도 표현**:

```csharp
// Unity: 즉시 실행
player.LevelUp(); // 즉시 레벨 증가

// REST: 의도 표현
POST /players/123/actions/levelup
Content-Type: application/json
{
    "experienceGained": 1000
}
// 서버가 해당 의도를 해석하고 적절히 처리
```

**1.3.4 하이퍼미디어와 HATEOAS (Hypermedia As The Engine Of Application State)**

**하이퍼미디어의 이론적 기초**

하이퍼미디어는 **유한 상태 기계(Finite State Machine)** 이론에 기반한다:

```
게임 상태 전이 (Unity):
MainMenu → GamePlay → GameOver → MainMenu

REST API 상태 전이:
{
    "player": {...},
    "links": {
        "self": "/players/123",
        "levelup": "/players/123/actions/levelup",  // 가능한 다음 행동
        "inventory": "/players/123/inventory"
    }
}
```

**발견 가능성(Discoverability)**:

HATEOAS를 통해 API는 **자기 문서화**된다:

```json
{
    "player": {
        "id": 123,
        "name": "GamerX",
        "level": 10,
        "experience": 1500,
        "nextLevelExp": 2000
    },
    "_links": {
        "self": { "href": "/players/123" },
        "levelup": {
            "href": "/players/123/actions/levelup",
            "method": "POST",
            "condition": "experience >= nextLevelExp"
        },
        "characters": { "href": "/players/123/characters" },
        "inventory": { "href": "/players/123/inventory" }
    }
}
```

클라이언트는 하드코딩된 URL 대신 **링크 관계**를 따라 네비게이션할 수 있다. 이는 **웹 브라우저가 링크를 따라 페이지를 이동하는 것**과 같은 원리다.

---

#### 1.4 데이터 지속성과 ACID 속성의 이론적 기초 (45분)

**1.4.1 데이터베이스 이론의 역사적 발전**

**계층형 → 네트워크형 → 관계형 모델의 진화**

**1960년대 계층형 모델**:
- IBM의 IMS (Information Management System)
- 트리 구조로 데이터 표현
- 부모-자식 관계만 표현 가능

**1970년대 네트워크형 모델**:
- CODASYL DBTG 표준
- 그래프 구조로 다대다 관계 표현
- 복잡한 포인터 기반 네비게이션

**1970년 관계형 모델의 혁명**:

Edgar F. Codd의 논문 "A Relational Model of Data for Large Shared Data Banks"는 데이터베이스 이론에 수학적 기초를 제공했다.

**관계형 모델의 수학적 기초**:

- **관계(Relation)**: 수학적 집합론의 카르테시안 곱
- **튜플(Tuple)**: 관계의 원소
- **속성(Attribute)**: 도메인(정의역)의 이름
- **스키마(Schema)**: 관계의 구조적 정의

```
Unity의 계층적 데이터 vs 관계형 데이터:

Unity GameObject 계층:
Player
├── Character1
│   ├── Equipment1
│   └── Equipment2
└── Character2
    └── Equipment3

관계형 모델:
Players(id, name, level)
Characters(id, playerId, name, class)  // playerId는 Players.id를 참조
Equipment(id, characterId, itemType, stats)  // characterId는 Characters.id를 참조
```

**1.4.2 ACID 속성의 이론적 배경**

**원자성(Atomicity): 모든 것 또는 아무것도**

원자성은 **그리스 철학의 원자론**에서 유래된 개념이다. 데이터베이스에서 트랜잭션은 **더 이상 분할할 수 없는 최소 단위**여야 한다.

```
Unity의 즉시 실행 vs 데이터베이스의 원자성:

Unity (원자성 보장 안됨):
player.gold -= 1000;  // 1단계: 골드 차감
player.AddItem(sword); // 2단계: 아이템 추가
// 만약 2단계에서 에러 발생 시, 골드만 차감되고 아이템은 추가 안됨

데이터베이스 (원자성 보장):
BEGIN TRANSACTION;
    UPDATE players SET gold = gold - 1000 WHERE id = @playerId;
    INSERT INTO inventory (player_id, item_id) VALUES (@playerId, @itemId);
COMMIT; -- 모든 작업이 성공하거나 모든 작업이 실패
```

**일관성(Consistency): 불변 조건의 유지**

일관성은 **불변 조건(Invariant)**의 개념에 기반한다. 시스템의 상태가 변하더라도 특정 조건들은 항상 참이어야 한다.

```
게임의 불변 조건 예시:
- 플레이어의 골드는 항상 0 이상이어야 함
- 인벤토리의 아이템 수는 최대 용량을 초과할 수 없음
- 캐릭터의 총 스탯 포인트는 레벨에 따른 최대치를 넘을 수 없음

데이터베이스 제약조건으로 구현:
ALTER TABLE players ADD CONSTRAINT chk_positive_gold CHECK (gold >= 0);
ALTER TABLE inventory ADD CONSTRAINT chk_inventory_limit
    CHECK ((SELECT COUNT(*) FROM inventory WHERE player_id = player_id) <= max_capacity);
```

**격리성(Isolation): 동시 실행의 환상**

격리성은 **직렬화 이론(Serializability Theory)**에 기반한다. 여러 트랜잭션이 동시에 실행되어도, 결과는 순차적으로 실행한 것과 동일해야 한다.

**격리 수준의 이론적 분류**:

1. **Read Uncommitted**: 다른 트랜잭션의 미완료 변경사항 읽기 가능
2. **Read Committed**: 커밋된 데이터만 읽기 가능
3. **Repeatable Read**: 같은 트랜잭션 내에서 같은 읽기 결과 보장
4. **Serializable**: 완전한 격리, 순차 실행과 동일한 결과

```
Unity vs 데이터베이스의 동시성:

Unity (동시성 문제 없음 - 단일 스레드):
void Update() {
    // 모든 객체가 순차적으로 업데이트
    foreach(var obj in gameObjects) {
        obj.Update();
    }
}

데이터베이스 (동시성 제어 필요):
Transaction A: SELECT gold FROM players WHERE id = 1; (gold = 1000)
Transaction B: SELECT gold FROM players WHERE id = 1; (gold = 1000)
Transaction A: UPDATE players SET gold = gold - 500 WHERE id = 1;
Transaction B: UPDATE players SET gold = gold - 600 WHERE id = 1;
// Lost Update 문제: B의 업데이트가 A의 업데이트를 덮어씀
```

**지속성(Durability): 영속성의 보장**

지속성은 **비휘발성 저장소**의 개념과 관련된다. 트랜잭션이 커밋되면, 시스템 장애가 발생하더라도 결과가 보존되어야 한다.

**Unity의 휘발적 저장 vs 데이터베이스의 영속적 저장**:

```csharp
// Unity: 메모리 기반 (휘발적)
public class PlayerData : MonoBehaviour
{
    public int gold = 1000;

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
            PlayerPrefs.SetInt("Gold", gold); // 명시적 저장 필요
    }
}

// 데이터베이스: 디스크 기반 (영속적)
public async Task<bool> UpdatePlayerGoldAsync(int playerId, int goldChange)
{
    using var transaction = await _context.Database.BeginTransactionAsync();
    try
    {
        var player = await _context.Players.FindAsync(playerId);
        player.Gold += goldChange;

        await _context.SaveChangesAsync(); // 디스크에 영구 저장
        await transaction.CommitAsync();   // WAL(Write-Ahead Log)로 내구성 보장
        return true;
    }
    catch
    {
        await transaction.RollbackAsync();
        return false;
    }
}
```

**1.4.3 동시성 제어 이론**

**Two-Phase Locking (2PL) 프로토콜**

동시성 제어를 위한 고전적 알고리즘:

1. **성장 단계(Growing Phase)**: 락을 획득만 하고 해제하지 않음
2. **수축 단계(Shrinking Phase)**: 락을 해제만 하고 획득하지 않음

```
Unity의 단순한 실행 vs 데이터베이스의 복잡한 동시성 제어:

Unity:
// 순차적 실행, 동시성 문제 없음
player1.gold += 100;
player2.gold -= 100;

데이터베이스:
// 복잡한 락 관리
BEGIN TRANSACTION;
    SELECT * FROM players WHERE id IN (1, 2) FOR UPDATE; -- 락 획득
    UPDATE players SET gold = gold + 100 WHERE id = 1;
    UPDATE players SET gold = gold - 100 WHERE id = 2;
COMMIT; -- 락 해제
```

**데드락과 라이브락**

**데드락**: 두 개 이상의 트랜잭션이 서로가 가진 자원을 기다리며 무한히 대기하는 상황

```
Transaction A: Lock(Player1) → Wait for Lock(Player2)
Transaction B: Lock(Player2) → Wait for Lock(Player1)
→ 데드락 발생
```

**라이브락**: 데드락을 회피하려다 지속적으로 상태만 변경하고 진전이 없는 상황

Unity에서는 이런 문제가 발생하지 않지만, 멀티플레이어 게임이나 웹 서버에서는 핵심적인 고려사항이다.

---

### 🌆 오후 세션 (3시간): 실전 적용과 구현

이론을 바탕으로 실제 IdleRPG 프로젝트에 적용해보는 시간이다.

#### 2.1 HTTP API 설계 실습 (45분)

**2.1.1 자원 모델링과 URI 설계**

IdleRPG의 도메인을 REST 자원으로 모델링:

```
도메인 분석:
- Player (플레이어): 게임의 주체
- Character (캐릭터): 플레이어가 소유한 게임 캐릭터
- Item (아이템): 게임 내 도구나 장비
- Battle (전투): 게임의 주요 액션
- Guild (길드): 플레이어들의 집단

자원 계층 구조:
/players/{playerId}
├── /characters/{characterId}
│   ├── /equipment
│   ├── /stats
│   └── /skills/{skillId}
├── /inventory
│   └── /items/{itemId}
├── /battles/{battleId}
└── /guild
```

**2.1.2 OpenAPI 명세 작성**

```yaml
openapi: 3.0.0
info:
  title: IdleRPG API
  description: Unity 개발자를 위한 RESTful 게임 서버 API
  version: 1.0.0

paths:
  /players/{playerId}:
    get:
      summary: 플레이어 정보 조회
      parameters:
        - name: playerId
          in: path
          required: true
          schema:
            type: string
            format: uuid
      responses:
        '200':
          description: 플레이어 정보
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/Player'
              example:
                id: "123e4567-e89b-12d3-a456-426614174000"
                name: "GamerX"
                level: 15
                experience: 12500
                gold: 5000
                _links:
                  self:
                    href: "/players/123e4567-e89b-12d3-a456-426614174000"
                  characters:
                    href: "/players/123e4567-e89b-12d3-a456-426614174000/characters"
                  inventory:
                    href: "/players/123e4567-e89b-12d3-a456-426614174000/inventory"

components:
  schemas:
    Player:
      type: object
      properties:
        id:
          type: string
          format: uuid
        name:
          type: string
          example: "GamerX"
        level:
          type: integer
          minimum: 1
          example: 15
        experience:
          type: integer
          minimum: 0
          example: 12500
        gold:
          type: integer
          minimum: 0
          example: 5000
        _links:
          $ref: '#/components/schemas/Links'
```

#### 2.2 데이터베이스 설계와 마이그레이션 (45분)

**2.2.1 정규화 이론 적용**

**제1정규형 (1NF)**: 원자값 저장
```sql
-- 위반 사례 (배열 저장)
CREATE TABLE players_bad (
    id UUID PRIMARY KEY,
    name VARCHAR(100),
    skills TEXT  -- "Fireball,Healing,Shield" - 원자적이지 않음
);

-- 올바른 설계
CREATE TABLE players (
    id UUID PRIMARY KEY,
    name VARCHAR(100) NOT NULL
);

CREATE TABLE player_skills (
    player_id UUID REFERENCES players(id),
    skill_id INTEGER,
    level INTEGER DEFAULT 1,
    PRIMARY KEY (player_id, skill_id)
);
```

**제2정규형 (2NF)**: 부분적 함수 종속 제거
```sql
-- 위반 사례
CREATE TABLE character_equipment_bad (
    character_id UUID,
    equipment_slot VARCHAR(20),
    item_id INTEGER,
    item_name VARCHAR(100),    -- item_id에만 종속적 (부분적 함수 종속)
    item_attack_power INTEGER  -- item_id에만 종속적
    PRIMARY KEY (character_id, equipment_slot)
);

-- 올바른 설계
CREATE TABLE items (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    attack_power INTEGER DEFAULT 0
);

CREATE TABLE character_equipment (
    character_id UUID,
    equipment_slot VARCHAR(20),
    item_id INTEGER REFERENCES items(id),
    PRIMARY KEY (character_id, equipment_slot)
);
```

**제3정규형 (3NF)**: 이행적 함수 종속 제거
```sql
-- 위반 사례
CREATE TABLE characters_bad (
    id UUID PRIMARY KEY,
    player_id UUID,
    level INTEGER,
    total_stat_points INTEGER,  -- level에 종속적 (이행적 함수 종속)
    class_id INTEGER,
    class_name VARCHAR(50)      -- class_id에 종속적 (이행적 함수 종속)
);

-- 올바른 설계
CREATE TABLE character_classes (
    id SERIAL PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE characters (
    id UUID PRIMARY KEY,
    player_id UUID REFERENCES players(id),
    level INTEGER DEFAULT 1,
    class_id INTEGER REFERENCES character_classes(id)
);

-- total_stat_points는 계산으로 도출
CREATE VIEW character_stats AS
SELECT
    c.id,
    c.level,
    (c.level - 1) * 5 AS total_stat_points  -- 레벨업마다 5포인트
FROM characters c;
```

**2.2.2 인덱스 전략과 쿼리 최적화**

**B-Tree 인덱스의 이론적 배경**:

```
B-Tree 인덱스 구조 (간단화):
                [Level=10|Level=20]
               /                    \
    [Level=5|Level=8]              [Level=15|Level=25]
   /       |        \             /         |         \
[1,2,3] [6,7,8] [9,10,11]  [12,13,14] [18,19,20] [22,23,24]

검색 복잡도: O(log n) vs 전체 스캔 O(n)
```

Unity 메모리 접근과 데이터베이스 디스크 접근의 차이:

```csharp
// Unity: 메모리 접근 (나노초 단위)
var player = players.Find(p => p.id == targetId); // O(n) but very fast per operation

// 데이터베이스: 디스크 접근 (밀리초 단위)
SELECT * FROM players WHERE id = @targetId;  -- 인덱스 없으면 테이블 스캔
```

**인덱스 설계 전략**:

```sql
-- 1. 기본 키 인덱스 (자동 생성)
CREATE TABLE players (
    id UUID PRIMARY KEY  -- 자동으로 고유 인덱스 생성
);

-- 2. 조회 패턴 기반 인덱스
CREATE INDEX idx_players_level ON players(level);  -- 레벨별 조회
CREATE INDEX idx_players_last_login ON players(last_login);  -- 활성 사용자 조회

-- 3. 복합 인덱스 (쿼리 패턴 고려)
CREATE INDEX idx_characters_player_level ON characters(player_id, level);
-- WHERE player_id = ? AND level > ? 같은 쿼리에 최적

-- 4. 부분 인덱스 (조건부)
CREATE INDEX idx_active_players ON players(last_login)
WHERE last_login > NOW() - INTERVAL '30 days';  -- 활성 사용자만
```

#### 2.3 비즈니스 로직 구현 (45분)

**2.3.1 도메인 주도 설계 (DDD) 적용**

**Unity의 MonoBehaviour vs DDD의 Entity**:

```csharp
// Unity: 상태와 행동이 결합된 MonoBehaviour
public class Player : MonoBehaviour
{
    [SerializeField] private int level = 1;
    [SerializeField] private long experience = 0;

    public void GainExperience(long amount)
    {
        experience += amount;
        CheckLevelUp();
        UpdateUI();  // UI 업데이트까지 책임
    }
}

// DDD: 순수한 도메인 로직
public class Player  // Entity
{
    public Guid Id { get; private set; }
    public PlayerName Name { get; private set; }  // Value Object
    public Level Level { get; private set; }      // Value Object
    public Experience Experience { get; private set; }  // Value Object

    public LevelUpResult GainExperience(ExperiencePoints points)
    {
        // 순수한 비즈니스 로직만 담당
        var newExperience = Experience.Add(points);
        var requiredExp = Level.GetRequiredExperienceForNext();

        if (newExperience.IsEnoughFor(requiredExp))
        {
            var newLevel = Level.Increment();
            var remainingExp = newExperience.Subtract(requiredExp);

            // 도메인 이벤트 발행
            var levelUpEvent = new PlayerLevelUpEvent(Id, Level, newLevel);
            DomainEvents.Raise(levelUpEvent);

            return LevelUpResult.Success(newLevel, remainingExp);
        }

        return LevelUpResult.NoLevelUp(newExperience);
    }
}
```

**Value Objects의 불변성**:

```csharp
// Unity: 가변 값
public class PlayerStats
{
    public int strength;
    public int agility;

    public void AddBonus(int str, int agi)
    {
        strength += str;  // 직접 수정
        agility += agi;
    }
}

// DDD: 불변 값 객체
public class PlayerStats : ValueObject
{
    public int Strength { get; }
    public int Agility { get; }

    public PlayerStats(int strength, int agility)
    {
        Strength = strength;
        Agility = agility;
    }

    public PlayerStats AddBonus(int strengthBonus, int agilityBonus)
    {
        // 새로운 인스턴스 반환 (불변성)
        return new PlayerStats(
            Strength + strengthBonus,
            Agility + agilityBonus
        );
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Strength;
        yield return Agility;
    }
}
```

**2.3.2 CQRS (Command Query Responsibility Segregation) 패턴**

**읽기와 쓰기의 분리**:

```csharp
// Command: 상태 변경
public class GainExperienceCommand : IRequest<LevelUpResult>
{
    public Guid PlayerId { get; set; }
    public long ExperienceAmount { get; set; }
}

public class GainExperienceHandler : IRequestHandler<GainExperienceCommand, LevelUpResult>
{
    public async Task<LevelUpResult> Handle(GainExperienceCommand request, CancellationToken cancellationToken)
    {
        // 1. 도메인 객체 로드
        var player = await _playerRepository.GetByIdAsync(request.PlayerId);

        // 2. 비즈니스 로직 실행
        var result = player.GainExperience(new ExperiencePoints(request.ExperienceAmount));

        // 3. 변경사항 저장
        await _playerRepository.SaveAsync(player);

        return result;
    }
}

// Query: 데이터 조회 (최적화된 읽기 모델)
public class GetPlayerStatsQuery : IRequest<PlayerStatsDto>
{
    public Guid PlayerId { get; set; }
}

public class GetPlayerStatsHandler : IRequestHandler<GetPlayerStatsQuery, PlayerStatsDto>
{
    public async Task<PlayerStatsDto> Handle(GetPlayerStatsQuery request, CancellationToken cancellationToken)
    {
        // 읽기에 최적화된 쿼리 (JOIN 사용)
        var stats = await _dbContext.Players
            .Where(p => p.Id == request.PlayerId)
            .Select(p => new PlayerStatsDto
            {
                Id = p.Id,
                Name = p.Name,
                Level = p.Level,
                Experience = p.Experience,
                TotalPlayTime = p.Statistics.TotalPlayTime,
                // 복잡한 계산도 미리 수행
                NextLevelExperience = CalculateRequiredExperience(p.Level + 1),
                RankPosition = _dbContext.Players.Count(other => other.Level > p.Level) + 1
            })
            .FirstOrDefaultAsync();

        return stats;
    }
}
```

#### 2.4 통합 테스트와 API 문서화 (45분)

**2.4.1 테스트 전략**

**Unity의 PlayMode vs 웹 API의 통합 테스트**:

```csharp
// Unity: PlayMode Test
[UnityTest]
public IEnumerator Player_GainsExperience_LevelsUp()
{
    // Arrange
    var player = CreateTestPlayer();
    var initialLevel = player.level;

    // Act
    player.GainExperience(1000);
    yield return null;  // 한 프레임 대기

    // Assert
    Assert.AreEqual(initialLevel + 1, player.level);
}

// 웹 API: 통합 테스트
[Test]
public async Task GainExperience_SufficientAmount_PlayerLevelsUp()
{
    // Arrange
    var player = await CreateTestPlayerAsync();
    var initialLevel = player.Level;

    var request = new GainExperienceRequest
    {
        PlayerId = player.Id,
        ExperienceAmount = 1000
    };

    // Act
    var response = await _client.PostAsJsonAsync("/api/players/experience", request);

    // Assert
    response.EnsureSuccessStatusCode();
    var result = await response.Content.ReadFromJsonAsync<LevelUpResult>();
    Assert.True(result.LeveledUp);
    Assert.AreEqual(initialLevel + 1, result.NewLevel);

    // 데이터베이스에서도 확인
    var updatedPlayer = await _playerRepository.GetByIdAsync(player.Id);
    Assert.AreEqual(initialLevel + 1, updatedPlayer.Level);
}
```

**테스트 데이터베이스 관리**:

```csharp
public class ApiTestFixture : IAsyncLifetime
{
    private readonly DockerContainer _postgreSqlContainer;

    public async Task InitializeAsync()
    {
        // Docker를 통한 격리된 테스트 환경
        _postgreSqlContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15")
            .WithDatabase("idlerpg_test")
            .WithUsername("test")
            .WithPassword("test123")
            .Build();

        await _postgreSqlContainer.StartAsync();

        // 스키마 초기화
        await MigrateDatabase();
    }

    public async Task DisposeAsync()
    {
        await _postgreSqlContainer.StopAsync();
    }
}
```

**2.4.2 API 문서화 자동화**

```csharp
// Swagger/OpenAPI 설정
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "IdleRPG API",
                Version = "v1",
                Description = "Unity 개발자를 위한 RESTful 게임 서버 API",
                Contact = new OpenApiContact
                {
                    Name = "개발팀",
                    Email = "dev@idlerpg.com"
                }
            });

            // XML 문서 주석 포함
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);

            // JWT 인증 스키마
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });
        });
    }
}

// 컨트롤러에서 상세 문서화
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PlayersController : ControllerBase
{
    /// <summary>
    /// 플레이어 경험치 추가 및 레벨업 처리
    /// </summary>
    /// <param name="request">경험치 추가 요청 정보</param>
    /// <returns>레벨업 결과</returns>
    /// <response code="200">경험치 추가 성공</response>
    /// <response code="400">잘못된 요청 데이터</response>
    /// <response code="404">존재하지 않는 플레이어</response>
    [HttpPost("experience")]
    [ProducesResponseType(typeof(LevelUpResult), 200)]
    [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<LevelUpResult>> GainExperience(
        [FromBody] GainExperienceRequest request)
    {
        var result = await _mediator.Send(new GainExperienceCommand
        {
            PlayerId = request.PlayerId,
            ExperienceAmount = request.ExperienceAmount
        });

        return Ok(result);
    }
}
```

---

### 📋 Day 1 완료 체크리스트

- [ ] **이론적 기초 이해**: 분산 시스템과 클라이언트-서버 모델의 차이점 설명 가능
- [ ] **HTTP 프로토콜**: 요청-응답 모델과 무상태성의 의미 이해
- [ ] **REST 아키텍처**: 6가지 제약사항과 HATEOAS 개념 설명 가능
- [ ] **데이터베이스 이론**: ACID 속성과 동시성 제어 메커니즘 이해
- [ ] **실습 완료**: IdleRPG API 기본 엔드포인트 설계 및 구현
- [ ] **테스트 작성**: 통합 테스트 케이스 작성 및 실행
- [ ] **문서화**: OpenAPI 명세서 작성 및 Swagger UI 확인

### 🚀 Day 2 Preview: 데이터베이스 심화

내일은 Entity Framework Core를 통한 ORM 매핑, 복잡한 쿼리 최적화, 그리고 데이터베이스 마이그레이션 전략에 대해 학습할 예정이다. Unity의 ScriptableObject와 PlayerPrefs에서 벗어나 진정한 엔터프라이즈급 데이터 관리 방법을 익혀보자.
{
    private float fixedTimeStep = 0.02f; // 50Hz 물리 업데이트
    private List<Player> allPlayers = new();
    private NetworkManager networkManager;

    void Update()
    {
        // 매 프레임 실행 (1/60초마다)
        HandlePlayerInput();
        UpdateGameLogic();
        UpdateUI();
        RenderFrame();

        // 메모리에 모든 플레이어 데이터가 계속 존재
        foreach (var player in allPlayers)
        {
            player.UpdateIdleProgress(Time.deltaTime);
        }
    }

    void FixedUpdate()
    {
        // 고정 간격으로 실행 (물리 계산용)
        foreach (var player in allPlayers)
        {
            player.UpdatePhysics(fixedTimeStep);
        }
    }

    // Unity의 문제점: 플레이어가 1만명이면 매 프레임마다 1만번 업데이트
    // 메모리 사용량: 지속적으로 모든 데이터를 RAM에 보관
    // CPU 사용량: 아무도 게임을 하지 않아도 계속 실행
}

// 웹 서버: 이벤트 기반 처리 (요청이 올 때만 실행)
[ApiController]
[Route("api/[controller]")]
public class PlayersController : ControllerBase
{
    private readonly IPlayerService _playerService;
    private readonly ILogger<PlayersController> _logger;

    public PlayersController(IPlayerService playerService, ILogger<PlayersController> logger)
    {
        _playerService = playerService;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PlayerDto>> GetPlayer(Guid id)
    {
        // 요청이 올 때만 실행 (1초에 0번일 수도, 1000번일 수도 있음)
        var startTime = DateTime.UtcNow;

        try
        {
            // 1. 데이터베이스에서 플레이어 정보 로드
            var player = await _playerService.GetPlayerAsync(id);
            if (player == null)
            {
                _logger.LogWarning($"Player {id} not found");
                return NotFound($"Player with ID {id} was not found");
            }

            // 2. 마지막 접속 이후 방치 보상 계산
            var idleReward = await _playerService.CalculateIdleRewardAsync(id);

            // 3. DTO로 변환하여 반환
            var playerDto = new PlayerDto
            {
                Id = player.Id,
                Nickname = player.Nickname,
                Level = player.Level,
                Experience = player.Experience,
                IdleReward = idleReward
            };

            var processingTime = DateTime.UtcNow - startTime;
            _logger.LogInformation($"Player {id} loaded in {processingTime.TotalMilliseconds}ms");

            return Ok(playerDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error loading player {id}");
            return StatusCode(500, "Internal server error");
        }

        // 요청 처리 완료 후 메모리에서 모든 데이터 해제
        // 다음 요청까지 아무것도 실행하지 않음
    }
}
```

**메모리 관리 방식의 차이점 심화 분석**

```csharp
// Unity: 지속적 메모리 점유 방식
public class UnityPlayerManager : MonoBehaviour
{
    // 게임이 실행되는 동안 계속 메모리에 존재
    private Dictionary<int, PlayerData> activePlayers = new();
    private Queue<NetworkMessage> messageQueue = new();
    private List<IdleReward> pendingRewards = new();

    void Start()
    {
        // 게임 시작 시 모든 데이터 로드
        LoadAllPlayerData();
        StartCoroutine(ProcessIdleRewards());
        StartCoroutine(SavePlayerDataPeriodically());
    }

    IEnumerator ProcessIdleRewards()
    {
        while (true)
        {
            // 5초마다 모든 플레이어의 방치 보상 계산
            foreach (var player in activePlayers.Values)
            {
                CalculateIdleReward(player);
            }
            yield return new WaitForSeconds(5f);
        }
    }

    IEnumerator SavePlayerDataPeriodically()
    {
        while (true)
        {
            // 1분마다 모든 플레이어 데이터 저장
            foreach (var player in activePlayers.Values)
            {
                SavePlayerData(player);
            }
            yield return new WaitForSeconds(60f);
        }
    }
}

// 웹 서버: 요청별 메모리 할당/해제 방식
public class PlayerService : IPlayerService
{
    private readonly IPlayerRepository _playerRepository;
    private readonly IMemoryCache _cache;
    private readonly ILogger<PlayerService> _logger;

    public PlayerService(
        IPlayerRepository playerRepository,
        IMemoryCache cache,
        ILogger<PlayerService> logger)
    {
        _playerRepository = playerRepository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Player> GetPlayerAsync(Guid id)
    {
        // 1단계: 캐시에서 확인 (메모리에 임시 저장된 데이터)
        var cacheKey = $"player:{id}";
        if (_cache.TryGetValue(cacheKey, out Player cachedPlayer))
        {
            _logger.LogInformation($"Player {id} loaded from cache");
            return cachedPlayer;
        }

        // 2단계: 데이터베이스에서 로드
        var player = await _playerRepository.GetByIdAsync(id);
        if (player == null)
        {
            return null;
        }

        // 3단계: 캐시에 5분간 저장 (자주 접근하는 데이터 최적화)
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
            SlidingExpiration = TimeSpan.FromMinutes(2),
            Priority = CacheItemPriority.Normal
        };
        _cache.Set(cacheKey, player, cacheOptions);

        _logger.LogInformation($"Player {id} loaded from database and cached");
        return player;

        // 메서드 종료 시 지역 변수들은 가비지 컬렉션으로 자동 해제
    }

    public async Task<IdleRewardDto> CalculateIdleRewardAsync(Guid playerId)
    {
        // 요청할 때만 계산 (Unity처럼 계속 실행하지 않음)
        var player = await GetPlayerAsync(playerId);
        var currentTime = DateTime.UtcNow;
        var offlineTime = currentTime - player.LastLogin;

        // 복잡한 보상 계산 로직
        var baseExpRate = CalculateBaseExperienceRate(player.Level);
        var baseGoldRate = CalculateBaseGoldRate(player.Level);

        // 캐릭터별 보너스 계산
        var characters = await _playerRepository.GetPlayerCharactersAsync(playerId);
        var totalExpBonus = characters.Sum(c => c.GetExpBonus());
        var totalGoldBonus = characters.Sum(c => c.GetGoldBonus());

        // 장비 보너스 계산
        var equipmentBonus = await CalculateEquipmentBonusAsync(characters);

        var reward = new IdleRewardDto
        {
            ExperienceGained = (long)(baseExpRate * totalExpBonus * offlineTime.TotalMinutes),
            GoldGained = (int)(baseGoldRate * totalGoldBonus * offlineTime.TotalMinutes),
            ItemsFound = await CalculateItemDropsAsync(player, offlineTime),
            OfflineTimeMinutes = Math.Min(offlineTime.TotalMinutes, 480) // 최대 8시간
        };

        return reward;
        // 계산 완료 후 모든 임시 데이터는 메모리에서 해제
    }
}
```

#### 1.2 상태 관리 패러다임의 근본적 차이 (45분)

**Unity의 GameObject 기반 vs 웹 서버의 Entity 기반 상태 관리**

```csharp
// Unity: GameObject와 Component 시스템을 통한 상태 관리
public class PlayerData : MonoBehaviour
{
    [Header("기본 정보")]
    public string playerName;
    public int level = 1;
    public long experience = 0;

    [Header("게임 상태")]
    public bool isOnline = true;
    public Vector3 lastPosition;
    public Quaternion lastRotation;

    [Header("인벤토리")]
    public List<InventoryItem> inventory = new();
    public int maxInventorySize = 100;

    [Header("캐릭터 상태")]
    public PlayerStats stats;
    public List<ActiveSkill> activeSkills = new();
    public List<PassiveSkill> passiveSkills = new();

    // Unity의 생명주기
    void Awake()
    {
        // 씬 전환 시에도 유지
        DontDestroyOnLoad(this.gameObject);

        // 컴포넌트 초기화
        stats = GetComponent<PlayerStats>();
        if (stats == null)
            stats = gameObject.AddComponent<PlayerStats>();
    }

    void Start()
    {
        // 게임 시작 시 데이터 로드
        LoadPlayerData();
        StartCoroutine(AutoSave());
    }

    void Update()
    {
        // 매 프레임 상태 업데이트
        UpdateIdleProgress();
        UpdateSkillCooldowns();
        UpdateBuffsAndDebuffs();
    }

    void OnApplicationPause(bool pauseStatus)
    {
        // 앱 일시정지 시 데이터 저장
        if (pauseStatus)
        {
            SavePlayerData();
            CalculateOfflineProgress();
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        // 앱 포커스 잃을 때/얻을 때 처리
        if (!hasFocus)
            SavePlayerData();
        else
            LoadOfflineRewards();
    }

    IEnumerator AutoSave()
    {
        while (true)
        {
            yield return new WaitForSeconds(30f);
            SavePlayerData();
        }
    }

    public void SavePlayerData()
    {
        // Unity PlayerPrefs를 통한 저장
        PlayerPrefs.SetString("PlayerName", playerName);
        PlayerPrefs.SetInt("Level", level);
        PlayerPrefs.SetString("Experience", experience.ToString());
        PlayerPrefs.SetFloat("PosX", transform.position.x);
        PlayerPrefs.SetFloat("PosY", transform.position.y);
        PlayerPrefs.SetFloat("PosZ", transform.position.z);

        // JSON으로 복잡한 데이터 저장
        string inventoryJson = JsonUtility.ToJson(new SerializableList<InventoryItem>(inventory));
        PlayerPrefs.SetString("Inventory", inventoryJson);

        PlayerPrefs.Save();
    }

    public void LoadPlayerData()
    {
        playerName = PlayerPrefs.GetString("PlayerName", "NewPlayer");
        level = PlayerPrefs.GetInt("Level", 1);
        experience = long.Parse(PlayerPrefs.GetString("Experience", "0"));

        Vector3 savedPos = new Vector3(
            PlayerPrefs.GetFloat("PosX", 0),
            PlayerPrefs.GetFloat("PosY", 0),
            PlayerPrefs.GetFloat("PosZ", 0)
        );
        transform.position = savedPos;

        string inventoryJson = PlayerPrefs.GetString("Inventory", "{}");
        if (!string.IsNullOrEmpty(inventoryJson))
        {
            var loadedInventory = JsonUtility.FromJson<SerializableList<InventoryItem>>(inventoryJson);
            inventory = loadedInventory.items;
        }
    }
}

// 웹 서버: Entity와 Repository를 통한 상태 관리
public class Player
{
    public Guid Id { get; set; }
    public string Nickname { get; set; }
    public int Level { get; set; } = 1;
    public long Experience { get; set; } = 0;

    // 위치 정보 (3D 좌표를 별도 엔티티로 관리)
    public PlayerPosition? CurrentPosition { get; set; }

    // 게임 상태
    public bool IsOnline { get; set; }
    public DateTime LastLogin { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // 게임 진행 상태
    public GameProgress GameProgress { get; set; } = new();

    // 관계형 데이터 (별도 테이블로 관리)
    public List<Character> Characters { get; set; } = new();
    public List<InventoryItem> Inventory { get; set; } = new();
    public List<PlayerSkill> Skills { get; set; } = new();
    public List<PlayerBuff> ActiveBuffs { get; set; } = new();

    // 통계 데이터
    public PlayerStatistics Statistics { get; set; } = new();

    // 설정 데이터
    public PlayerSettings Settings { get; set; } = new();

    // 비즈니스 로직 메서드
    public bool CanLevelUp()
    {
        var requiredExp = CalculateRequiredExperience(Level + 1);
        return Experience >= requiredExp;
    }

    public LevelUpResult LevelUp()
    {
        if (!CanLevelUp())
            return LevelUpResult.Failed("Not enough experience");

        var oldLevel = Level;
        var requiredExp = CalculateRequiredExperience(Level + 1);

        Level++;
        Experience -= requiredExp;

        var newStats = CalculateStatsForLevel(Level);
        var rewards = CalculateLevelUpRewards(Level);

        return new LevelUpResult
        {
            Success = true,
            OldLevel = oldLevel,
            NewLevel = Level,
            NewStats = newStats,
            Rewards = rewards
        };
    }

    private long CalculateRequiredExperience(int targetLevel)
    {
        // 복잡한 경험치 공식
        return (long)(100 * Math.Pow(targetLevel, 2.1) + 50 * targetLevel);
    }

    private PlayerStats CalculateStatsForLevel(int level)
    {
        return new PlayerStats
        {
            Health = 100 + (level * 25),
            Mana = 50 + (level * 15),
            Attack = 10 + (level * 5),
            Defense = 5 + (level * 3),
            Speed = 10 + (level * 2)
        };
    }

    private List<LevelUpReward> CalculateLevelUpRewards(int level)
    {
        var rewards = new List<LevelUpReward>();

        // 기본 보상
        rewards.Add(new LevelUpReward
        {
            Type = RewardType.Gold,
            Amount = level * 100
        });

        // 특별 레벨 보상
        if (level % 5 == 0)
        {
            rewards.Add(new LevelUpReward
            {
                Type = RewardType.Item,
                ItemId = GetLevelMilestoneReward(level)
            });
        }

        if (level % 10 == 0)
        {
            rewards.Add(new LevelUpReward
            {
                Type = RewardType.SkillPoint,
                Amount = 3
            });
        }

        return rewards;
    }
}

// 복잡한 관련 엔티티들
public class PlayerPosition
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public Player Player { get; set; }

    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
    public float RotationX { get; set; }
    public float RotationY { get; set; }
    public float RotationZ { get; set; }
    public float RotationW { get; set; }

    public string ZoneName { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class GameProgress
{
    public int CurrentStage { get; set; } = 1;
    public int CurrentWave { get; set; } = 1;
    public DateTime LastIdleCalculation { get; set; } = DateTime.UtcNow;
    public long TotalIdleTime { get; set; } = 0; // 총 방치 시간 (분)
    public int ConsecutivePlayDays { get; set; } = 1;
    public DateTime LastDailyReward { get; set; }
}

public class PlayerStatistics
{
    public long TotalExperienceGained { get; set; } = 0;
    public long TotalGoldEarned { get; set; } = 0;
    public int TotalMonstersKilled { get; set; } = 0;
    public int TotalItemsFound { get; set; } = 0;
    public int TotalDeaths { get; set; } = 0;
    public TimeSpan TotalPlayTime { get; set; } = TimeSpan.Zero;
    public DateTime FirstLogin { get; set; } = DateTime.UtcNow;
    public int LoginStreak { get; set; } = 1;
}

public class PlayerSettings
{
    public bool SoundEnabled { get; set; } = true;
    public bool MusicEnabled { get; set; } = true;
    public float SoundVolume { get; set; } = 1.0f;
    public float MusicVolume { get; set; } = 0.8f;
    public bool NotificationsEnabled { get; set; } = true;
    public string PreferredLanguage { get; set; } = "en";
    public bool AutoBattleEnabled { get; set; } = true;
    public bool AutoSkillEnabled { get; set; } = false;
}
```

#### 1.3 데이터 지속성과 동시성 처리 (45분)

**Unity의 단일 사용자 vs 웹 서버의 다중 사용자 동시성 처리**

```csharp
// Unity: 단일 사용자 환경 (동시성 문제 없음)
public class InventoryManager : MonoBehaviour
{
    [SerializeField] private List<InventoryItem> items = new();
    [SerializeField] private int maxCapacity = 100;

    public bool AddItem(ItemData itemData, int quantity = 1)
    {
        // 단일 플레이어이므로 동시성 걱정 없음
        if (items.Count >= maxCapacity)
        {
            ShowMessage("인벤토리가 가득 참!");
            return false;
        }

        var existingItem = items.FirstOrDefault(i => i.ItemId == itemData.Id);
        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
        }
        else
        {
            items.Add(new InventoryItem
            {
                ItemId = itemData.Id,
                Quantity = quantity,
                AcquiredAt = DateTime.Now
            });
        }

        SaveInventory(); // 즉시 저장
        UpdateUI(); // UI 업데이트
        return true;
    }

    public bool RemoveItem(int itemId, int quantity = 1)
    {
        var item = items.FirstOrDefault(i => i.ItemId == itemId);
        if (item == null || item.Quantity < quantity)
            return false;

        item.Quantity -= quantity;
        if (item.Quantity <= 0)
            items.Remove(item);

        SaveInventory();
        UpdateUI();
        return true;
    }
}

// 웹 서버: 다중 사용자 동시성 처리가 필요
public class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IItemRepository _itemRepository;
    private readonly ILogger<InventoryService> _logger;
    private readonly IDistributedLock _distributedLock; // Redis 기반 분산 락

    public async Task<AddItemResult> AddItemAsync(Guid playerId, int itemId, int quantity)
    {
        // 플레이어별 인벤토리 락 (동시에 같은 플레이어가 아이템을 조작하는 것 방지)
        var lockKey = $"inventory_lock:{playerId}";
        var lockTimeout = TimeSpan.FromSeconds(30);

        using (var lockHandle = await _distributedLock.AcquireLockAsync(lockKey, lockTimeout))
        {
            if (lockHandle == null)
            {
                return AddItemResult.Failed("다른 작업이 진행 중입니다. 잠시 후 다시 시도해주세요.");
            }

            try
            {
                // 1단계: 현재 인벤토리 상태 확인
                var currentInventory = await _inventoryRepository.GetPlayerInventoryAsync(playerId);
                var inventoryCapacity = await GetInventoryCapacityAsync(playerId);

                if (currentInventory.Count >= inventoryCapacity)
                {
                    return AddItemResult.Failed("인벤토리가 가득 참!");
                }

                // 2단계: 아이템 정보 확인
                var itemData = await _itemRepository.GetByIdAsync(itemId);
                if (itemData == null)
                {
                    return AddItemResult.Failed("존재하지 않는 아이템입니다.");
                }

                // 3단계: 기존 아이템 확인 (스택 가능한 아이템인지)
                var existingItem = currentInventory.FirstOrDefault(i => i.ItemId == itemId);

                if (existingItem != null && itemData.IsStackable)
                {
                    // 스택 가능한 아이템 - 수량 증가
                    var maxStack = itemData.MaxStackSize;
                    var canAdd = Math.Min(quantity, maxStack - existingItem.Quantity);

                    if (canAdd > 0)
                    {
                        existingItem.Quantity += canAdd;
                        await _inventoryRepository.UpdateItemAsync(existingItem);

                        _logger.LogInformation($"Player {playerId} added {canAdd} of item {itemId} to existing stack");

                        return new AddItemResult
                        {
                            Success = true,
                            AddedQuantity = canAdd,
                            RemainingQuantity = quantity - canAdd
                        };
                    }
                    else
                    {
                        return AddItemResult.Failed("아이템 스택이 가득 참!");
                    }
                }
                else
                {
                    // 새로운 아이템 슬롯 생성
                    var newInventoryItem = new InventoryItem
                    {
                        Id = Guid.NewGuid(),
                        PlayerId = playerId,
                        ItemId = itemId,
                        Quantity = quantity,
                        AcquiredAt = DateTime.UtcNow,
                        SlotIndex = GetNextAvailableSlot(currentInventory)
                    };

                    await _inventoryRepository.AddItemAsync(newInventoryItem);

                    _logger.LogInformation($"Player {playerId} added {quantity} of new item {itemId}");

                    return new AddItemResult
                    {
                        Success = true,
                        AddedQuantity = quantity,
                        NewItem = newInventoryItem
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding item {itemId} to player {playerId} inventory");
                return AddItemResult.Failed("아이템 추가 중 오류가 발생했습니다.");
            }
        }
    }

    public async Task<RemoveItemResult> RemoveItemAsync(Guid playerId, int itemId, int quantity)
    {
        var lockKey = $"inventory_lock:{playerId}";
        var lockTimeout = TimeSpan.FromSeconds(30);

        using (var lockHandle = await _distributedLock.AcquireLockAsync(lockKey, lockTimeout))
        {
            if (lockHandle == null)
            {
                return RemoveItemResult.Failed("다른 작업이 진행 중입니다.");
            }

            try
            {
                // 데이터베이스 트랜잭션 시작
                using (var transaction = await _inventoryRepository.BeginTransactionAsync())
                {
                    var inventoryItem = await _inventoryRepository.GetPlayerItemAsync(playerId, itemId);
                    if (inventoryItem == null)
                    {
                        await transaction.RollbackAsync();
                        return RemoveItemResult.Failed("해당 아이템이 인벤토리에 없습니다.");
                    }

                    if (inventoryItem.Quantity < quantity)
                    {
                        await transaction.RollbackAsync();
                        return RemoveItemResult.Failed("요청한 수량보다 적은 아이템을 보유하고 있습니다.");
                    }

                    inventoryItem.Quantity -= quantity;

                    if (inventoryItem.Quantity <= 0)
                    {
                        await _inventoryRepository.RemoveItemAsync(inventoryItem.Id);
                        _logger.LogInformation($"Player {playerId} completely removed item {itemId}");
                    }
                    else
                    {
                        await _inventoryRepository.UpdateItemAsync(inventoryItem);
                        _logger.LogInformation($"Player {playerId} removed {quantity} of item {itemId}, {inventoryItem.Quantity} remaining");
                    }

                    await transaction.CommitAsync();

                    return new RemoveItemResult
                    {
                        Success = true,
                        RemovedQuantity = quantity,
                        RemainingQuantity = Math.Max(0, inventoryItem.Quantity)
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing item {itemId} from player {playerId} inventory");
                return RemoveItemResult.Failed("아이템 제거 중 오류가 발생했습니다.");
            }
        }
    }

    // 동시에 여러 플레이어가 같은 아이템을 획득하려 할 때의 처리
    public async Task<List<AddItemResult>> AddItemsToMultiplePlayersAsync(
        List<Guid> playerIds,
        int itemId,
        int quantityPerPlayer)
    {
        var results = new List<AddItemResult>();
        var tasks = new List<Task<AddItemResult>>();

        // 병렬 처리로 성능 향상
        foreach (var playerId in playerIds)
        {
            tasks.Add(AddItemAsync(playerId, itemId, quantityPerPlayer));
        }

        var taskResults = await Task.WhenAll(tasks);
        results.AddRange(taskResults);

        _logger.LogInformation($"Added item {itemId} to {taskResults.Count(r => r.Success)} out of {playerIds.Count} players");

        return results;
    }

    private async Task<int> GetInventoryCapacityAsync(Guid playerId)
    {
        // 플레이어 레벨에 따른 인벤토리 용량 계산
        var player = await _playerRepository.GetByIdAsync(playerId);
        var baseCapacity = 50;
        var bonusCapacity = (player.Level / 10) * 5; // 레벨 10마다 슬롯 5개 추가
        var premiumBonus = await GetPremiumInventoryBonusAsync(playerId);

        return baseCapacity + bonusCapacity + premiumBonus;
    }

    private int GetNextAvailableSlot(List<InventoryItem> currentInventory)
    {
        // 사용 중인 슬롯 인덱스 찾기
        var usedSlots = currentInventory.Select(i => i.SlotIndex).OrderBy(s => s).ToList();

        // 첫 번째 빈 슬롯 찾기
        for (int i = 0; i < usedSlots.Count; i++)
        {
            if (usedSlots[i] != i)
                return i;
        }

        return usedSlots.Count; // 마지막 슬롯 다음
    }
}

// 동시성 처리를 위한 결과 클래스들
public class AddItemResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public int AddedQuantity { get; set; }
    public int RemainingQuantity { get; set; } // 스택 제한으로 추가되지 못한 수량
    public InventoryItem NewItem { get; set; }

    public static AddItemResult Failed(string message) => new() { Success = false, ErrorMessage = message };
}

public class RemoveItemResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public int RemovedQuantity { get; set; }
    public int RemainingQuantity { get; set; }

    public static RemoveItemResult Failed(string message) => new() { Success = false, ErrorMessage = message };
}
```

#### 1.4 실전 연습: Unity 사고방식을 웹 서버로 전환 (45분)

**실습 과제: Unity 스킬 쿨타임 시스템을 웹 서버 API로 변환**

```csharp
// Unity: 실시간 스킬 쿨타임 관리
public class SkillSystem : MonoBehaviour
{
    [System.Serializable]
    public class Skill
    {
        public int skillId;
        public string name;
        public float cooldownTime;
        public float damage;
        public float manaCost;

        [System.NonSerialized]
        public float remainingCooldown;

        public bool IsReady => remainingCooldown <= 0;

        public void Use()
        {
            if (!IsReady) return;
            remainingCooldown = cooldownTime;
        }
    }

    public List<Skill> playerSkills = new();
    public float mana = 100f;
    public float maxMana = 100f;

    void Update()
    {
        // 매 프레임 쿨타임 감소
        foreach (var skill in playerSkills)
        {
            if (skill.remainingCooldown > 0)
            {
                skill.remainingCooldown -= Time.deltaTime;
            }
        }

        // 마나 자동 회복
        if (mana < maxMana)
        {
            mana += Time.deltaTime * 2f; // 초당 2 마나 회복
            mana = Mathf.Min(mana, maxMana);
        }
    }

    public bool UseSkill(int skillId)
    {
        var skill = playerSkills.FirstOrDefault(s => s.skillId == skillId);
        if (skill == null || !skill.IsReady || mana < skill.manaCost)
            return false;

        skill.Use();
        mana -= skill.manaCost;

        // 스킬 효과 적용
        ApplySkillEffect(skill);
        return true;
    }
}

// 웹 서버: 계산 기반 스킬 쿨타임 관리
[ApiController]
[Route("api/skills")]
public class SkillController : ControllerBase
{
    private readonly ISkillService _skillService;

    [HttpPost("use")]
    public async Task<ActionResult<SkillUseResult>> UseSkill(UseSkillRequest request)
    {
        var result = await _skillService.UseSkillAsync(request.PlayerId, request.SkillId);

        if (!result.Success)
            return BadRequest(result.ErrorMessage);

        return Ok(result);
    }

    [HttpGet("player/{playerId}/cooldowns")]
    public async Task<ActionResult<PlayerSkillsDto>> GetPlayerSkills(Guid playerId)
    {
        var skills = await _skillService.GetPlayerSkillsWithCooldownsAsync(playerId);
        return Ok(skills);
    }
}

public class SkillService : ISkillService
{
    public async Task<SkillUseResult> UseSkillAsync(Guid playerId, int skillId)
    {
        using (var lockHandle = await _distributedLock.AcquireLockAsync($"player_action:{playerId}", TimeSpan.FromSeconds(10)))
        {
            if (lockHandle == null)
                return SkillUseResult.Failed("다른 액션이 처리 중입니다.");

            // 1. 플레이어와 스킬 정보 로드
            var player = await _playerRepository.GetByIdAsync(playerId);
            var playerSkill = await _skillRepository.GetPlayerSkillAsync(playerId, skillId);
            var skillTemplate = await _skillRepository.GetSkillTemplateAsync(skillId);

            if (playerSkill == null || skillTemplate == null)
                return SkillUseResult.Failed("해당 스킬을 보유하고 있지 않습니다.");

            // 2. 쿨타임 확인 (실시간 계산)
            var currentTime = DateTime.UtcNow;
            var timeSinceLastUse = currentTime - playerSkill.LastUsedAt;
            var cooldownDuration = TimeSpan.FromSeconds(skillTemplate.CooldownSeconds);

            if (timeSinceLastUse < cooldownDuration)
            {
                var remainingCooldown = cooldownDuration - timeSinceLastUse;
                return SkillUseResult.Failed($"스킬이 아직 쿨타임 중입니다. ({remainingCooldown.TotalSeconds:F1}초 남음)");
            }

            // 3. 마나 확인 및 소모
            var playerStats = await _playerRepository.GetPlayerStatsAsync(playerId);
            if (playerStats.CurrentMana < skillTemplate.ManaCost)
                return SkillUseResult.Failed("마나가 부족합니다.");

            // 4. 스킬 사용 처리
            playerSkill.LastUsedAt = currentTime;
            playerSkill.TotalUseCount++;

            playerStats.CurrentMana -= skillTemplate.ManaCost;

            // 5. 스킬 효과 계산
            var skillEffect = await CalculateSkillEffectAsync(player, playerSkill, skillTemplate);

            // 6. 데이터베이스 업데이트
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    await _skillRepository.UpdatePlayerSkillAsync(playerSkill);
                    await _playerRepository.UpdatePlayerStatsAsync(playerStats);

                    // 스킬 효과 적용 (데미지, 힐링, 버프 등)
                    await ApplySkillEffectAsync(playerId, skillEffect);

                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }

            return new SkillUseResult
            {
                Success = true,
                SkillEffect = skillEffect,
                RemainingMana = playerStats.CurrentMana,
                NextUseAvailableAt = currentTime.Add(cooldownDuration)
            };
        }
    }

    public async Task<PlayerSkillsDto> GetPlayerSkillsWithCooldownsAsync(Guid playerId)
    {
        var playerSkills = await _skillRepository.GetPlayerSkillsAsync(playerId);
        var currentTime = DateTime.UtcNow;

        var skillsWithCooldowns = new List<SkillWithCooldownDto>();

        foreach (var playerSkill in playerSkills)
        {
            var skillTemplate = await _skillRepository.GetSkillTemplateAsync(playerSkill.SkillId);
            var timeSinceLastUse = currentTime - playerSkill.LastUsedAt;
            var cooldownDuration = TimeSpan.FromSeconds(skillTemplate.CooldownSeconds);

            var dto = new SkillWithCooldownDto
            {
                SkillId = playerSkill.SkillId,
                Name = skillTemplate.Name,
                Level = playerSkill.Level,
                IsReady = timeSinceLastUse >= cooldownDuration,
                RemainingCooldownSeconds = Math.Max(0, (cooldownDuration - timeSinceLastUse).TotalSeconds),
                ManaCost = skillTemplate.ManaCost,
                Damage = CalculateDamage(skillTemplate.BaseDamage, playerSkill.Level),
                LastUsedAt = playerSkill.LastUsedAt
            };

            skillsWithCooldowns.Add(dto);
        }

        return new PlayerSkillsDto
        {
            PlayerId = playerId,
            Skills = skillsWithCooldowns,
            CurrentMana = (await _playerRepository.GetPlayerStatsAsync(playerId)).CurrentMana,
            MaxMana = (await _playerRepository.GetPlayerStatsAsync(playerId)).MaxMana,
            ManaRegenRate = 2.0f // 초당 마나 회복량
        };
    }

    private async Task<SkillEffect> CalculateSkillEffectAsync(Player player, PlayerSkill playerSkill, SkillTemplate template)
    {
        var baseDamage = template.BaseDamage;
        var skillLevel = playerSkill.Level;
        var playerStats = await _playerRepository.GetPlayerStatsAsync(player.Id);

        // 복잡한 데미지 계산
        var finalDamage = baseDamage * (1 + skillLevel * 0.1f) * (1 + playerStats.Attack / 100f);

        // 크리티컬 계산
        var criticalChance = 0.05f + playerStats.Luck / 1000f;
        var isCritical = new Random().NextDouble() < criticalChance;
        if (isCritical)
            finalDamage *= 2.0f;

        return new SkillEffect
        {
            Damage = (int)finalDamage,
            IsCritical = isCritical,
            EffectType = template.EffectType,
            Duration = TimeSpan.FromSeconds(template.EffectDurationSeconds)
        };
    }
}
```

#### 🎯 실습 1: IdleRPG 플레이어 데이터 모델링

기존 WEB_SERVER_MASTERY_GUIDE.md의 Player 엔티티를 확장해보겠습니다:

```csharp
// IdleRPG.Domain/Entities/Player.cs - Unity 개발자가 이해하기 쉬운 구조
public class Player
{
    public Guid Id { get; set; }
    public string Nickname { get; set; }
    public int Level { get; set; } = 1;
    public long Experience { get; set; } = 0;

    // Unity의 PlayerPrefs와 유사한 개념
    public int Gold { get; set; } = 100;
    public int Gems { get; set; } = 0;

    // Unity의 게임 오브젝트 활성/비활성과 유사
    public bool IsOnline { get; set; }
    public DateTime LastLogin { get; set; }
    public DateTime CreatedAt { get; set; }

    // Unity의 인벤토리 시스템
    public List<Item> Inventory { get; set; } = new();
    public List<Character> Characters { get; set; } = new();
}
```

### 🌆 오후 세션 (3시간): HTTP와 RESTful API 기초

#### 1.2 HTTP 프로토콜 - Unity개발자 관점에서 이해하기

**Unity의 UnityWebRequest vs ASP.NET Core Controller**

```csharp
// Unity 클라이언트 측 (이미 익숙한 코드)
public class NetworkManager : MonoBehaviour
{
    public async Task<PlayerData> GetPlayerData(string playerId)
    {
        string url = $"https://api.idlerpg.com/players/{playerId}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                return JsonUtility.FromJson<PlayerData>(request.downloadHandler.text);
            }
            return null;
        }
    }
}

// 웹 서버 측 (새로 배워야 할 코드)
[ApiController]
[Route("api/[controller]")]
public class PlayersController : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<ActionResult<Player>> GetPlayer(Guid id)
    {
        var player = await _playerRepository.GetByIdAsync(id);
        if (player == null)
            return NotFound();

        return Ok(player);
    }
}
```

#### 🎯 실습 2: IdleRPG API 엔드포인트 설계

REST API 설계 원칙을 Unity 개발자가 이해하기 쉽게 설명:

```csharp
// IdleRPG.API/Controllers/PlayersController.cs
[ApiController]
[Route("api/[controller]")]
public class PlayersController : ControllerBase
{
    private readonly IPlayerService _playerService;

    public PlayersController(IPlayerService playerService)
    {
        _playerService = playerService;
    }

    // Unity의 Save 기능과 유사
    [HttpPost]
    public async Task<ActionResult<Player>> CreatePlayer(CreatePlayerRequest request)
    {
        var player = await _playerService.CreatePlayerAsync(request.Nickname);
        return CreatedAtAction(nameof(GetPlayer), new { id = player.Id }, player);
    }

    // Unity의 Load 기능과 유사
    [HttpGet("{id}")]
    public async Task<ActionResult<Player>> GetPlayer(Guid id)
    {
        var player = await _playerService.GetPlayerAsync(id);
        return player == null ? NotFound() : Ok(player);
    }

    // Unity의 실시간 업데이트와 유사
    [HttpPut("{id}/experience")]
    public async Task<ActionResult> AddExperience(Guid id, AddExperienceRequest request)
    {
        await _playerService.AddExperienceAsync(id, request.Amount);
        return NoContent();
    }
}
```

---

## Day 2: 데이터베이스 - Unity의 영구 저장과 비교

### 🌅 오전 세션 (3시간): 관계형 데이터베이스 이해

#### 2.1 Unity 저장 방식 vs 데이터베이스

**Unity PlayerPrefs vs SQL Database**

```csharp
// Unity: PlayerPrefs (단순하지만 제한적)
public void SavePlayerData()
{
    PlayerPrefs.SetString("PlayerName", playerName);
    PlayerPrefs.SetInt("PlayerLevel", level);
    PlayerPrefs.SetFloat("PlayerExp", experience);
    PlayerPrefs.Save();
}

// 데이터베이스: 구조화된 데이터와 관계
public class ApplicationDbContext : DbContext
{
    public DbSet<Player> Players { get; set; }
    public DbSet<Character> Characters { get; set; }
    public DbSet<Item> Items { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 플레이어와 캐릭터 간의 관계 (1:N)
        modelBuilder.Entity<Character>()
            .HasOne(c => c.Owner)
            .WithMany(p => p.Characters)
            .HasForeignKey(c => c.OwnerId);
    }
}
```

#### 🎯 실습 3: IdleRPG 데이터베이스 스키마 설계

```csharp
// IdleRPG.Domain/Entities/Character.cs
public class Character
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public CharacterClass Class { get; set; }

    // Unity의 스탯 시스템과 유사
    public int Strength { get; set; } = 10;
    public int Agility { get; set; } = 10;
    public int Intelligence { get; set; } = 10;
    public int Vitality { get; set; } = 10;

    // 플레이어와의 관계
    public Guid OwnerId { get; set; }
    public Player Owner { get; set; }

    // Unity의 장비 시스템
    public List<Equipment> Equipments { get; set; } = new();
}

public enum CharacterClass
{
    Warrior = 1,
    Mage = 2,
    Archer = 3,
    Rogue = 4
}
```

### 🌆 오후 세션 (3시간): Entity Framework Core 기초

#### 2.2 Unity의 ScriptableObject vs EF Core Repository

```csharp
// Unity: ScriptableObject를 통한 데이터 관리
[CreateAssetMenu(fileName = "PlayerConfig", menuName = "IdleRPG/PlayerConfig")]
public class PlayerConfig : ScriptableObject
{
    public int startingLevel = 1;
    public int startingGold = 100;
    public List<StartingItem> startingItems = new();
}

// 웹 서버: Repository 패턴을 통한 데이터 접근
public interface IPlayerRepository
{
    Task<Player> GetByIdAsync(Guid id);
    Task<Player> CreateAsync(Player player);
    Task UpdateAsync(Player player);
    Task DeleteAsync(Guid id);
}

public class PlayerRepository : IPlayerRepository
{
    private readonly ApplicationDbContext _context;

    public PlayerRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Player> GetByIdAsync(Guid id)
    {
        return await _context.Players
            .Include(p => p.Characters)
            .Include(p => p.Inventory)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}
```

#### 🎯 실습 4: IdleRPG Repository 구현

```bash
# 데이터베이스 마이그레이션 생성 (Unity의 Build Settings와 유사한 과정)
cd IdleRPG.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../IdleRPG.API
dotnet ef database update --startup-project ../IdleRPG.API
```

---

## Day 3: 비즈니스 로직 - Unity의 Game Manager 패턴 적용

### 🌅 오전 세션 (3시간): 서비스 레이어 설계

#### 3.1 Unity GameManager vs 웹 서버 Service Layer

```csharp
// Unity: GameManager를 통한 게임 로직
public class GameManager : MonoBehaviour
{
    public PlayerManager playerManager;
    public InventoryManager inventoryManager;
    public BattleManager battleManager;

    public void LevelUp(Player player)
    {
        player.level++;
        player.experience = 0;
        playerManager.UpdatePlayerStats(player);
        inventoryManager.GiveReward(player, GetLevelUpReward(player.level));
    }
}

// 웹 서버: Service를 통한 비즈니스 로직
public interface IPlayerService
{
    Task<Player> CreatePlayerAsync(string nickname);
    Task<Player> GetPlayerAsync(Guid id);
    Task AddExperienceAsync(Guid playerId, long amount);
    Task<bool> LevelUpAsync(Guid playerId);
}

public class PlayerService : IPlayerService
{
    private readonly IPlayerRepository _playerRepository;
    private readonly IInventoryService _inventoryService;

    public async Task<bool> LevelUpAsync(Guid playerId)
    {
        var player = await _playerRepository.GetByIdAsync(playerId);
        if (player == null) return false;

        var expRequired = CalculateRequiredExperience(player.Level);
        if (player.Experience < expRequired) return false;

        player.Level++;
        player.Experience -= expRequired;

        // Unity의 이벤트 시스템과 유사
        await _inventoryService.AddLevelUpRewardAsync(playerId, player.Level);
        await _playerRepository.UpdateAsync(player);

        return true;
    }
}
```

### 🌆 오후 세션 (3시간): Clean Architecture 적용

#### 3.2 Unity Project Structure vs Clean Architecture

```
Unity 프로젝트 구조:
Assets/
├── Scripts/
│   ├── Managers/     (GameManager, PlayerManager)
│   ├── Data/         (ScriptableObjects)
│   ├── UI/          (Canvas, Panels)
│   └── Gameplay/    (Player, Character, Item)

웹 서버 Clean Architecture:
IdleRPGServer/
├── IdleRPG.Domain/      (Entities, Enums) - Unity의 Core Gameplay Scripts
├── IdleRPG.Application/ (Services, DTOs) - Unity의 Managers
├── IdleRPG.Infrastructure/ (Database, External APIs) - Unity의 Data
└── IdleRPG.API/        (Controllers, Middleware) - Unity의 UI
```

#### 🎯 실습 5: 경험치 시스템 구현

```csharp
// IdleRPG.Application/Services/ExperienceService.cs
public class ExperienceService : IExperienceService
{
    public long CalculateRequiredExperience(int level)
    {
        // Unity에서 흔히 사용하는 경험치 공식
        return (long)(100 * Math.Pow(level, 1.5));
    }

    public int CalculateLevel(long totalExperience)
    {
        int level = 1;
        while (CalculateRequiredExperience(level) <= totalExperience)
        {
            level++;
        }
        return level - 1;
    }

    public LevelUpResult ProcessExperienceGain(Player player, long expGain)
    {
        var result = new LevelUpResult
        {
            OldLevel = player.Level,
            ExperienceGained = expGain
        };

        player.Experience += expGain;
        var newLevel = CalculateLevel(player.Experience);

        if (newLevel > player.Level)
        {
            result.NewLevel = newLevel;
            result.LeveledUp = true;
            player.Level = newLevel;
        }
        else
        {
            result.NewLevel = player.Level;
        }

        return result;
    }
}
```

---

## Day 4: API 디자인과 테스팅

### 🌅 오전 세션 (3시간): RESTful API 설계 원칙

#### 4.1 Unity SceneManager vs API Routing

```csharp
// Unity: 씬 전환을 통한 화면 관리
public class SceneController : MonoBehaviour
{
    public void LoadMainMenu() => SceneManager.LoadScene("MainMenu");
    public void LoadBattle() => SceneManager.LoadScene("Battle");
    public void LoadInventory() => SceneManager.LoadScene("Inventory");
}

// 웹 서버: 라우팅을 통한 엔드포인트 관리
[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    [HttpGet("player/{id}/status")]
    public async Task<PlayerStatusDto> GetPlayerStatus(Guid id) { }

    [HttpPost("battle/start")]
    public async Task<BattleResultDto> StartBattle(StartBattleRequest request) { }

    [HttpGet("inventory/{playerId}")]
    public async Task<InventoryDto> GetInventory(Guid playerId) { }
}
```

#### 🎯 실습 6: IdleRPG 게임 액션 API 설계

```csharp
// IdleRPG.API/Controllers/GameActionsController.cs
[ApiController]
[Route("api/game-actions")]
public class GameActionsController : ControllerBase
{
    [HttpPost("idle-reward/claim")]
    public async Task<ActionResult<IdleRewardDto>> ClaimIdleReward(Guid playerId)
    {
        var reward = await _gameService.CalculateIdleRewardAsync(playerId);
        await _gameService.ClaimIdleRewardAsync(playerId, reward);
        return Ok(reward);
    }

    [HttpPost("equipment/upgrade")]
    public async Task<ActionResult<EquipmentDto>> UpgradeEquipment(UpgradeEquipmentRequest request)
    {
        var result = await _equipmentService.UpgradeAsync(request.EquipmentId, request.UpgradeLevel);
        return result.Success ? Ok(result.Equipment) : BadRequest(result.ErrorMessage);
    }
}
```

### 🌆 오후 세션 (3시간): 테스팅과 검증

#### 4.2 Unity PlayMode Tests vs Integration Tests

```csharp
// Unity: PlayMode Test (이미 익숙한 패턴)
[UnityTest]
public IEnumerator PlayerLevelUp_ShouldIncreaseStats()
{
    var player = new GameObject().AddComponent<Player>();
    player.experience = 1000;

    player.LevelUp();
    yield return null;

    Assert.AreEqual(2, player.level);
    Assert.Greater(player.maxHealth, 100);
}

// 웹 서버: Integration Test
[Test]
public async Task AddExperience_ShouldLevelUpPlayer_WhenExperienceExceedsThreshold()
{
    // Arrange
    var player = await CreateTestPlayerAsync();
    var initialLevel = player.Level;
    var expToAdd = CalculateRequiredExperience(initialLevel + 1);

    // Act
    await _playerService.AddExperienceAsync(player.Id, expToAdd);

    // Assert
    var updatedPlayer = await _playerRepository.GetByIdAsync(player.Id);
    Assert.That(updatedPlayer.Level, Is.EqualTo(initialLevel + 1));
}
```

---

## Day 5-7: 실전 프로젝트 - IdleRPG 핵심 기능 구현

### 🎯 최종 프로젝트: 완전한 IdleRPG API 구현

#### 5.1 필수 구현 기능 목록

1. **플레이어 관리 시스템**
   - 플레이어 생성/조회/업데이트
   - 경험치 및 레벨업 시스템

2. **캐릭터 관리 시스템**
   - 캐릭터 생성 및 클래스 선택
   - 스탯 관리 및 성장 시스템

3. **아이템 및 인벤토리 시스템**
   - 아이템 획득/사용/삭제
   - 인벤토리 용량 관리

4. **방치형 게임 핵심 시스템**
   - 오프라인 보상 계산
   - 자동 사냥 및 경험치 획득

#### 🎯 실습 7: 통합 시스템 구현

```csharp
// IdleRPG.Application/Services/IdleGameService.cs
public class IdleGameService : IIdleGameService
{
    public async Task<IdleRewardDto> CalculateIdleRewardAsync(Guid playerId)
    {
        var player = await _playerRepository.GetByIdAsync(playerId);
        var offlineTime = DateTime.UtcNow - player.LastLogin;

        // Unity 개발자가 익숙한 계산 방식
        var baseExpPerMinute = player.Level * 10;
        var baseGoldPerMinute = player.Level * 5;

        var maxOfflineHours = 8; // 최대 8시간까지만 보상
        var effectiveMinutes = Math.Min(offlineTime.TotalMinutes, maxOfflineHours * 60);

        return new IdleRewardDto
        {
            ExperienceGained = (long)(baseExpPerMinute * effectiveMinutes),
            GoldGained = (int)(baseGoldPerMinute * effectiveMinutes),
            OfflineTimeMinutes = effectiveMinutes
        };
    }
}
```

### 📋 학습 완료 체크리스트

- [ ] Unity와 웹 서버의 핵심 차이점 이해
- [ ] HTTP 프로토콜과 RESTful API 설계 원칙 숙지
- [ ] Entity Framework Core를 통한 데이터베이스 조작
- [ ] Clean Architecture를 적용한 비즈니스 로직 구현
- [ ] API 테스팅 방법론 습득
- [ ] IdleRPG 프로젝트 핵심 기능 구현 완료

### 🚀 다음 단계 준비
Stage 2: 데이터베이스 심화 학습을 위한 준비사항 점검

---

## 📖 참고 자료
- [ASP.NET Core 공식 문서](https://docs.microsoft.com/aspnet/core/)
- [Entity Framework Core 가이드](https://docs.microsoft.com/ef/core/)
- [RESTful API 설계 가이드](https://restfulapi.net/)
- [Clean Architecture 패턴](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)

**다음 파일: STAGE_2_DATABASE_MASTERY.md**