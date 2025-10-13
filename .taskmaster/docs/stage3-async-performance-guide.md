# Stage 3: Async Performance - IHostedService & BackgroundService

## 📚 학습 목표
ASP.NET Core에서 백그라운드 작업을 처리하는 방법을 학습하고, Unity의 Coroutine과 비교하여 서버 사이드 비동기 패턴을 이해합니다.

---

## 🎯 Unity vs ASP.NET Core 패러다임 비교

### Unity: Coroutine 패턴
```csharp
// Unity - MonoBehaviour 기반
public class AutoHuntingManager : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(AutoHuntingRoutine());
    }

    private IEnumerator AutoHuntingRoutine()
    {
        while (true)
        {
            // 사냥 실행
            ExecuteHunting();

            // 1분 대기
            yield return new WaitForSeconds(60f);
        }
    }
}
```

**Unity 특징:**
- `MonoBehaviour` 의존
- 게임 오브젝트 생명주기에 종속
- 씬 로드 시 코루틴 중단
- 단일 스레드 실행

---

### ASP.NET Core: IHostedService 패턴
```csharp
// ASP.NET Core - IHostedService 기반
public class IdleProgressService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // 사냥 실행
            await ExecuteHuntingAsync();

            // 1분 대기
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
```

**ASP.NET Core 특징:**
- 서버 생명주기에 연동
- 앱 시작/종료 시 자동 관리
- 다중 스레드 가능
- Graceful Shutdown 지원

---

## 📖 IHostedService 기초

### 개념
`IHostedService`는 ASP.NET Core 애플리케이션의 생명주기와 함께 실행되는 백그라운드 작업을 정의하는 인터페이스입니다.

### 인터페이스 정의
```csharp
public interface IHostedService
{
    // 서비스 시작
    Task StartAsync(CancellationToken cancellationToken);

    // 서비스 중지
    Task StopAsync(CancellationToken cancellationToken);
}
```

### 생명주기
```
서버 시작 → StartAsync() 호출
          ↓
      백그라운드 실행
          ↓
서버 종료 → StopAsync() 호출
```

---

## 🛠️ BackgroundService 클래스

### 개념
`BackgroundService`는 `IHostedService`를 구현한 추상 클래스로, 장기 실행 작업을 더 쉽게 구현할 수 있습니다.

### 기본 구조
```csharp
public abstract class BackgroundService : IHostedService
{
    // 하위 클래스에서 구현할 메서드
    protected abstract Task ExecuteAsync(CancellationToken stoppingToken);

    // StartAsync/StopAsync는 이미 구현됨
    public virtual Task StartAsync(CancellationToken cancellationToken);
    public virtual Task StopAsync(CancellationToken cancellationToken);
}
```

### 사용 예시
```csharp
public class MyBackgroundService : BackgroundService
{
    private readonly ILogger<MyBackgroundService> _logger;

    public MyBackgroundService(ILogger<MyBackgroundService> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Background Service 시작");

        while (!stoppingToken.IsCancellationRequested)
        {
            // 작업 수행
            await DoWorkAsync();

            // 대기
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }

        _logger.LogInformation("Background Service 종료");
    }

    private async Task DoWorkAsync()
    {
        // 실제 작업 로직
        _logger.LogInformation("작업 실행 중...");
        await Task.CompletedTask;
    }
}
```

---

## ⏰ 주기적 작업 패턴

### 1. Task.Delay 패턴 (기본)
```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    while (!stoppingToken.IsCancellationRequested)
    {
        await DoWorkAsync();
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
    }
}
```

**장점:** 간단하고 직관적
**단점:** 작업 실행 시간에 따라 간격이 불규칙

---

### 2. PeriodicTimer 패턴 (C# 10+) - 추천!
```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

    while (!stoppingToken.IsCancellationRequested
           && await timer.WaitForNextTickAsync(stoppingToken))
    {
        await DoWorkAsync();
    }
}
```

**장점:** 정확한 주기 보장, 메모리 효율적
**단점:** .NET 6+ 필요

---

### 3. Timer 패턴 (콜백 기반)
```csharp
private Timer _timer;

protected override Task ExecuteAsync(CancellationToken stoppingToken)
{
    _timer = new Timer(
        callback: async _ => await DoWorkAsync(),
        state: null,
        dueTime: TimeSpan.Zero,
        period: TimeSpan.FromMinutes(1)
    );

    return Task.CompletedTask;
}

public override Task StopAsync(CancellationToken stoppingToken)
{
    _timer?.Dispose();
    return base.StopAsync(stoppingToken);
}
```

**장점:** 비블로킹, 독립 실행
**단점:** 콜백 패턴이 복잡, 에러 처리 어려움

---

## 💉 Dependency Injection 패턴

### 문제: Scoped 서비스 주입 불가
```csharp
// ❌ 잘못된 방법 - BackgroundService는 Singleton
public class IdleProgressService : BackgroundService
{
    private readonly ICharacterRepository _repository; // Scoped!

    public IdleProgressService(ICharacterRepository repository)
    {
        _repository = repository; // 에러 발생!
    }
}
```

**에러:**
```
Cannot consume scoped service 'ICharacterRepository'
from singleton 'IdleProgressService'.
```

---

### 해결: IServiceScopeFactory 사용
```csharp
// ✅ 올바른 방법
public class IdleProgressService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<IdleProgressService> _logger;

    public IdleProgressService(
        IServiceScopeFactory scopeFactory,
        ILogger<IdleProgressService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // 매 실행마다 새로운 Scope 생성
            using (var scope = _scopeFactory.CreateScope())
            {
                var repository = scope.ServiceProvider
                    .GetRequiredService<ICharacterRepository>();

                await ProcessCharactersAsync(repository);
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task ProcessCharactersAsync(ICharacterRepository repository)
    {
        var characters = await repository.GetActiveCharactersAsync();
        // 처리 로직...
    }
}
```

**핵심:**
- `IServiceScopeFactory`는 Singleton
- 매 작업마다 `CreateScope()`로 새 Scope 생성
- `using` 블록으로 Scope 자동 해제
- Scope 내에서 Scoped 서비스 주입

---

## 🎮 IdleRPG 실전 예제

### IdleProgressService 구현
```csharp
namespace IdleRPG.Infrastructure.Services;

public class IdleProgressService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<IdleProgressService> _logger;

    public IdleProgressService(
        IServiceScopeFactory scopeFactory,
        ILogger<IdleProgressService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("IdleProgressService 시작됨");

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

        while (!stoppingToken.IsCancellationRequested
               && await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await ProcessIdleProgressAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "오프라인 진행 처리 중 에러 발생");
                // 에러가 나도 서비스는 계속 실행
            }
        }

        _logger.LogInformation("IdleProgressService 종료됨");
    }

    private async Task ProcessIdleProgressAsync()
    {
        using var scope = _scopeFactory.CreateScope();

        var characterRepository = scope.ServiceProvider
            .GetRequiredService<ICharacterRepository>();
        var monsterRepository = scope.ServiceProvider
            .GetRequiredService<IMonsterRepository>();
        var battleService = scope.ServiceProvider
            .GetRequiredService<IBattleService>();

        // 최근 1시간 이내 로그인한 캐릭터 조회
        var cutoffTime = DateTime.UtcNow.AddHours(-1);
        var activeCharacters = await characterRepository
            .GetByLastLoginAfterAsync(cutoffTime);

        _logger.LogInformation(
            "활성 캐릭터 {Count}명 자동 진행 처리 시작",
            activeCharacters.Count);

        foreach (var character in activeCharacters)
        {
            try
            {
                // 캐릭터 레벨 기준 랜덤 몬스터 선택
                var minLevel = Math.Max(1, character.Level - 2);
                var maxLevel = character.Level + 2;
                var monsters = await monsterRepository
                    .GetByLevelRangeAsync(minLevel, maxLevel);

                if (monsters.Any())
                {
                    var randomMonster = monsters
                        .OrderBy(_ => Guid.NewGuid())
                        .First();

                    // 자동 전투 시뮬레이션
                    var result = await battleService.SimulateBattleAsync(
                        character.Id,
                        randomMonster.Id);

                    _logger.LogInformation(
                        "캐릭터 {CharacterName} 자동 전투 완료 - 승리: {IsVictory}",
                        character.Name,
                        result.IsVictory);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "캐릭터 {CharacterId} 자동 진행 처리 실패",
                    character.Id);
            }
        }
    }
}
```

---

## 📝 Program.cs 등록

### 서비스 등록
```csharp
// Program.cs

var builder = WebApplication.CreateBuilder(args);

// ... 다른 서비스들 ...

// Background Service 등록
builder.Services.AddHostedService<IdleProgressService>();

var app = builder.Build();

// ... 미들웨어 설정 ...

app.Run();
```

**등록 메서드:**
- `AddHostedService<T>()` - IHostedService 등록
- `AddSingleton<IHostedService, T>()` - 동일한 효과
- 여러 개 등록 가능 (모두 병렬 실행)

---

## 🛡️ 에러 처리 패턴

### 패턴 1: Try-Catch in Loop
```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    while (!stoppingToken.IsCancellationRequested)
    {
        try
        {
            await DoWorkAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "작업 실패");
            // 에러가 나도 계속 실행
        }

        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
    }
}
```

**장점:** 에러가 나도 서비스 계속 실행
**단점:** 심각한 에러도 무시될 수 있음

---

### 패턴 2: Graceful Failure
```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    try
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await DoWorkAsync();
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
    catch (Exception ex)
    {
        _logger.LogCritical(ex, "BackgroundService 치명적 에러");
        throw; // 서비스 중단
    }
}
```

**장점:** 치명적 에러 시 서비스 중단
**단점:** 재시도 로직 없음

---

### 패턴 3: Polly Retry Policy (고급)
```csharp
private readonly IAsyncPolicy _retryPolicy;

public MyService()
{
    _retryPolicy = Policy
        .Handle<Exception>()
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
            onRetry: (exception, timespan, retryCount, context) =>
            {
                _logger.LogWarning(
                    "재시도 {RetryCount}: {Exception}",
                    retryCount,
                    exception.Message);
            });
}

protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    while (!stoppingToken.IsCancellationRequested)
    {
        await _retryPolicy.ExecuteAsync(async () =>
        {
            await DoWorkAsync();
        });

        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
    }
}
```

---

## 🧪 테스트 패턴

### 단위 테스트 예제
```csharp
public class IdleProgressServiceTests
{
    [Fact]
    public async Task ExecuteAsync_Should_Process_Active_Characters()
    {
        // Arrange
        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        var mockLogger = new Mock<ILogger<IdleProgressService>>();

        var service = new IdleProgressService(
            mockScopeFactory.Object,
            mockLogger.Object);

        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(5)); // 5초 후 중단

        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(TimeSpan.FromSeconds(6));
        await service.StopAsync(cts.Token);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("시작됨")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
```

---

## 📊 성능 최적화

### 1. 병렬 처리
```csharp
// ❌ 순차 처리 (느림)
foreach (var character in characters)
{
    await ProcessCharacterAsync(character);
}

// ✅ 병렬 처리 (빠름)
var tasks = characters.Select(c => ProcessCharacterAsync(c));
await Task.WhenAll(tasks);
```

---

### 2. ConfigureAwait(false)
```csharp
// API 호출에는 필요 없음
await DoWorkAsync(); // OK

// Library 코드에서는 사용
await DoWorkAsync().ConfigureAwait(false); // 더 효율적
```

---

### 3. 취소 토큰 전파
```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    while (!stoppingToken.IsCancellationRequested)
    {
        // 모든 비동기 메서드에 토큰 전달
        await DoWorkAsync(stoppingToken);
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
    }
}

private async Task DoWorkAsync(CancellationToken cancellationToken)
{
    var data = await repository.GetDataAsync(cancellationToken);
    // ...
}
```

---

## 🎓 학습 체크리스트

- [ ] IHostedService vs BackgroundService 차이 이해
- [ ] Unity Coroutine vs ASP.NET BackgroundService 비교
- [ ] IServiceScopeFactory를 사용한 DI 패턴 이해
- [ ] PeriodicTimer를 사용한 주기적 작업 구현
- [ ] CancellationToken 사용법 이해
- [ ] try-catch를 이용한 에러 처리 패턴 이해
- [ ] 로깅을 통한 디버깅 방법 이해
- [ ] Program.cs에 서비스 등록 방법 이해

---

## 📚 참고 자료

### 공식 문서
- [ASP.NET Core Background tasks](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services)
- [IHostedService interface](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.hosting.ihostedservice)
- [BackgroundService class](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.hosting.backgroundservice)

### 추가 학습
- Task Parallel Library (TPL)
- async/await 심화
- CancellationToken 고급 사용법
- Polly Resilience 라이브러리

---

## 🚀 다음 단계

1. **Task 31-33 구현** - Monster Entity, Character 업데이트
2. **Task 34-36 구현** - Battle System
3. **Task 37-38 구현** - Offline Rewards + **IdleProgressService** 🎯
4. **Task 39-40 구현** - 문서화 및 테스트

Week 2를 완료하면 **IHostedService 마스터**가 됩니다! 💪

---

**작성일:** 2025-10-13
**대상:** Unity 개발자 → ASP.NET Core 전환
**난이도:** 중급 (Unity Coroutine 경험 필수)