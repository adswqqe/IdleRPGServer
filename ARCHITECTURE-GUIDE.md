# IdleRPG 아키텍처 가이드

## 현재 구조: 모놀리식 (API + 스케줄러 통합)

동접 10,000명 규모에서는 **모놀리식 구조가 적합**합니다.

### 왜 통합 구조를 선택했는가?

#### 1. 규모 적합성
- 동접 1만명은 **중소 규모**
- 단일 서버로 처리 가능한 부하
- 수평 확장(여러 인스턴스)으로 충분히 대응 가능

#### 2. 비용 효율성
```
통합 구조:
├── API 엔드포인트 처리 (Kestrel)
├── 백그라운드 작업 (IHostedService)
├── 스케줄 작업 (Hangfire/Quartz.NET)
└── 공통 서비스 공유 (DB, Redis, Logger)

단일 인스턴스 비용: $60/월 (t3.medium)
2개 인스턴스 (Load Balancer): $120/월
```

#### 3. 개발 생산성
- 하나의 솔루션에서 모든 기능 개발
- 공통 로직 재사용
- 로컬 메서드 호출 (빠름)

---

## 구현 패턴

### 1. API와 백그라운드 작업 분리 (코드 레벨)

**Program.cs에서 백그라운드 서비스 등록:**
```csharp
// API 컨트롤러
builder.Services.AddControllers();

// 백그라운드 스케줄러 서비스
builder.Services.AddHostedService<IdleRewardScheduler>();
builder.Services.AddHostedService<AutoBattleScheduler>();

// 또는 Hangfire 사용
builder.Services.AddHangfire(config =>
    config.UsePostgreSqlStorage(connectionString));
builder.Services.AddHangfireServer();
```

**백그라운드 서비스 예시:**
```csharp
public class IdleRewardScheduler : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<IdleRewardScheduler> _logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();
            var rewardService = scope.ServiceProvider.GetRequiredService<IRewardService>();

            await rewardService.ProcessIdleRewardsAsync();

            // 10분마다 실행
            await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
        }
    }
}
```

### 2. 리소스 격리 (선택적)

**CPU/메모리 사용량 제어:**
```csharp
// 백그라운드 작업에 우선순위 낮게 설정
public class IdleRewardScheduler : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // 스레드 우선순위 낮춤 (API 요청 우선)
        Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;

        // 작업 실행...
    }
}
```

### 3. 수평 확장 (스케일 아웃)

**여러 인스턴스 실행 시 스케줄러 중복 방지:**

**옵션 A: Redis Lock 사용**
```csharp
public class IdleRewardScheduler : BackgroundService
{
    private readonly IDistributedLockFactory _lockFactory;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Redis 분산 락 획득
            await using var lockHandle = await _lockFactory.AcquireLockAsync(
                "scheduler:idle-reward",
                TimeSpan.FromMinutes(10));

            if (lockHandle != null)
            {
                // 이 인스턴스만 작업 실행
                await ProcessRewardsAsync();
            }

            await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
        }
    }
}
```

**옵션 B: Hangfire 사용 (자동 분산 처리)**
```csharp
// Hangfire는 자동으로 여러 인스턴스 중 하나만 실행
RecurringJob.AddOrUpdate(
    "process-idle-rewards",
    () => ProcessIdleRewardsAsync(),
    "*/10 * * * *" // 10분마다
);
```

---

## 확장 전략

### Phase 1: 동접 ~10,000명 (현재)
```
┌─────────────────────────────────┐
│   EC2 Instance (t3.medium)      │
│  ┌──────────────────────────┐   │
│  │  ASP.NET Core API        │   │
│  │  + Background Services   │   │
│  └──────────────────────────┘   │
│         ▼                        │
│  ┌──────────────────────────┐   │
│  │  PostgreSQL + Redis      │   │
│  └──────────────────────────┘   │
└─────────────────────────────────┘

비용: ~$120/월 (인스턴스 2개 + DB)
```

### Phase 2: 동접 10,000~50,000명
```
         ┌─────────────┐
         │     ALB     │
         └──────┬──────┘
                │
    ┌───────────┴───────────┐
    ▼                       ▼
┌─────────┐           ┌─────────┐
│Instance1│           │Instance2│
│API+BG   │           │API+BG   │
└────┬────┘           └────┬────┘
     │                     │
     └──────────┬──────────┘
                ▼
      ┌──────────────────┐
      │   RDS + Redis    │
      │   (스케일 업)     │
      └──────────────────┘

비용: ~$500/월 (인스턴스 4개 + RDS m5.large)
```

### Phase 3: 동접 50,000명 이상 (분리 고려 시점)
```
         ┌─────────────┐
         │     ALB     │
         └──────┬──────┘
                │
    ┌───────────┴───────────┐
    ▼                       ▼
┌─────────┐           ┌─────────┐
│API Only │           │API Only │  ← 스케일 아웃 쉬움
└────┬────┘           └────┬────┘
     │                     │
     └──────────┬──────────┘
                │
    ┌───────────┴───────────┐
    ▼                       ▼
┌──────────┐         ┌──────────┐
│Scheduler │         │Scheduler │  ← 독립 스케일링
│Server    │         │Server    │
└────┬─────┘         └────┬─────┘
     │                     │
     └──────────┬──────────┘
                ▼
      ┌──────────────────┐
      │  RDS Aurora +    │
      │  ElastiCache     │
      └──────────────────┘

비용: ~$1,500/월 (전용 인스턴스 + Aurora)
```

---

## 분리가 필요한 시점 (Phase 3)

### 신호들:
1. **API 응답 속도 저하**
   - 백그라운드 작업이 API에 영향
   - CPU 사용률 지속적으로 80% 이상

2. **독립적인 스케일링 필요**
   - API는 더 많은 인스턴스 필요
   - 스케줄러는 소수 인스턴스로 충분

3. **배포 전략 분리 필요**
   - 스케줄러 로직 변경이 API에 영향
   - 각각 다른 배포 주기 필요

### 분리 시 고려사항:
```csharp
// 분리 후에도 공통 라이브러리 공유
IdleRPG.Common         // 공통 DTO, 유틸리티
IdleRPG.Domain         // 도메인 엔티티
IdleRPG.Application    // 비즈니스 로직

IdleRPG.API            // API 서버
IdleRPG.Scheduler      // 스케줄러 서버
```

---

## 실제 사례

### 통합 구조 사용 사례:
- **Instagram 초기** (5천만 사용자까지 모놀리식)
- **Slack** (1만명 동접까지 단일 서버)
- **Stack Overflow** (300만 사용자, 서버 9대)

### 핵심 교훈:
> "Make it work, make it right, make it fast - in that order."
>
> 먼저 **작동하게** 만들고,
> 그 다음 **제대로** 만들고,
> 마지막에 **빠르게** 만들어라.

---

## 권장사항

### 동접 1만명 단계:
✅ **통합 구조 유지**
- API + 백그라운드 서비스 하나의 프로세스
- IHostedService 또는 Hangfire 사용
- 수평 확장으로 대응

### 모니터링할 지표:
```csharp
// Application Insights 또는 Prometheus
- API 응답 시간 (목표: < 200ms)
- CPU 사용률 (목표: < 70%)
- 메모리 사용률 (목표: < 80%)
- 동시 요청 수
- 백그라운드 작업 큐 길이
```

### 분리 결정 체크리스트:
- [ ] API 응답 시간이 지속적으로 300ms 이상
- [ ] CPU 사용률이 지속적으로 80% 이상
- [ ] 백그라운드 작업이 API 성능에 영향
- [ ] 독립적인 배포 주기가 필요
- [ ] 팀이 5명 이상으로 역할 분리 가능
- [ ] 운영 복잡도를 감당할 수 있는 인프라 팀 존재

**위 항목 중 3개 이상 해당되면 분리 고려**

---

## 결론

**동접 1만명 규모에서는:**
- ✅ API + 스케줄러 통합이 **정상적**이고 **효율적**
- ✅ 비용, 개발속도, 운영 복잡도 측면에서 유리
- ✅ 수평 확장으로 10만명까지도 대응 가능
- ⚠️ 분리는 **명확한 필요성이 생긴 후**에 진행

**"Premature optimization is the root of all evil."** - Donald Knuth
