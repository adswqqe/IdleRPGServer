# Week 0 인프라 추가 타이밍 가이드

## 전략: 필요할 때 점진적으로 추가

사용자는 Week 0 인프라를 스킵하고 Week 1부터 시작하기로 결정했습니다.
Claude는 **적절한 타이밍**에 인프라 추가를 제안해야 합니다.

---

## 📍 추가 시점 트리거

### 🔥 즉시 추가 권장 시점

#### 1. Serilog 구조화 로깅
**트리거:**
- 사용자가 "왜 안 되는지 모르겠어" 말할 때
- API 호출 실패 원인을 찾기 어려울 때
- 3번 이상 같은 문제로 디버깅할 때
- Week 2 (인벤토리) 시작 전

**추천 멘트:**
```
디버깅이 어려우시죠? 지금이 Serilog를 추가하기 딱 좋은 타이밍입니다.
로그만 추가하면 문제를 10배 빠르게 찾을 수 있어요.
30분만 투자하시겠어요?
```

**구현 내용:**
```csharp
// Program.cs
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/idlerpg-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// 사용 예시 (Controller에서)
_logger.LogInformation("Character created: {CharacterId}, Player: {PlayerId}", 
    character.Id, character.PlayerId);
```

---

#### 2. Global Exception Handler
**트리거:**
- 서버가 예외로 인해 크래시할 때
- "Internal Server Error" 500 에러가 나올 때
- Week 2 (장비 트랜잭션) 시작 전

**추천 멘트:**
```
서버가 예외로 죽는 것을 방지하기 위해 Global Exception Handler를 추가하시는 게 좋겠습니다.
10분이면 끝나고, 서버 안정성이 크게 향상됩니다.
```

**구현 내용:**
```csharp
// Middleware/GlobalExceptionHandlerMiddleware.cs
public class GlobalExceptionHandlerMiddleware : IMiddleware
{
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            
            var response = new
            {
                error = "An internal server error occurred",
                message = ex.Message,
                traceId = context.TraceIdentifier
            };
            
            await context.Response.WriteAsJsonAsync(response);
        }
    }
}

// Program.cs에 등록
builder.Services.AddTransient<GlobalExceptionHandlerMiddleware>();
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
```

---

#### 3. FluentValidation
**트리거:**
- Week 2 (인벤토리/장비) 시작할 때
- 잘못된 입력 데이터로 인한 버그가 생길 때
- "null 체크를 계속 해야 하네" 라고 말할 때

**추천 멘트:**
```
입력 검증 로직이 늘어나고 있네요. 
FluentValidation을 추가하면 검증 코드가 깔끔해지고, 
에러 메시지도 자동으로 만들어집니다. 추가할까요?
```

**구현 내용:**
```csharp
// NuGet 설치
dotnet add package FluentValidation.AspNetCore

// Characters/Validators/CreateCharacterValidator.cs
public class CreateCharacterValidator : AbstractValidator<CreateCharacterRequest>
{
    public CreateCharacterValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("캐릭터 이름은 필수입니다")
            .Length(2, 20).WithMessage("이름은 2-20자여야 합니다")
            .Matches("^[a-zA-Z0-9가-힣]+$").WithMessage("특수문자는 사용할 수 없습니다");
    }
}

// Program.cs
builder.Services.AddValidatorsFromAssemblyContaining<CreateCharacterValidator>();
builder.Services.AddFluentValidationAutoValidation();
```

---

#### 4. AutoMapper
**트리거:**
- Week 1 중반 (Entity ↔ DTO 매핑이 반복될 때)
- "이 매핑 코드가 너무 많네요" 라고 말할 때

**추천 멘트:**
```
Entity와 DTO 변환 코드가 반복되고 있네요.
AutoMapper를 추가하면 보일러플레이트 코드가 90% 줄어듭니다.
```

**구현 내용:**
```csharp
// NuGet 설치
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection

// Characters/MappingProfiles/CharacterProfile.cs
public class CharacterProfile : Profile
{
    public CharacterProfile()
    {
        CreateMap<Character, CharacterDto>();
        CreateMap<CreateCharacterRequest, Character>();
    }
}

// Program.cs
builder.Services.AddAutoMapper(typeof(Program).Assembly);

// 사용
var characterDto = _mapper.Map<CharacterDto>(character);
```

---

#### 5. Request/Response Logging Middleware
**트리거:**
- Week 4 (자동 사냥) 시작 전
- "API 응답 시간이 느린 것 같은데" 말할 때
- 성능 측정이 필요할 때

**추천 멘트:**
```
이제 성능을 측정할 시점입니다.
Request Logging을 추가하면 모든 API의 응답 시간이 자동으로 기록됩니다.
```

**구현 내용:**
```csharp
// Middleware/RequestLoggingMiddleware.cs
public class RequestLoggingMiddleware : IMiddleware
{
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var sw = Stopwatch.StartNew();
        
        await next(context);
        
        sw.Stop();
        
        _logger.LogInformation(
            "API {Method} {Path} - {StatusCode} ({ElapsedMs}ms)",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            sw.ElapsedMilliseconds
        );
    }
}

// Program.cs
app.UseMiddleware<RequestLoggingMiddleware>();
```

---

## 📅 주차별 추가 권장 타이밍

| Week | 추천 인프라 | 이유 |
|------|-----------|------|
| Week 1 중반 | AutoMapper | Entity↔DTO 변환 반복 |
| Week 1 후반 | Serilog | 디버깅 필요성 체감 |
| Week 2 시작 | FluentValidation | 복잡한 입력 검증 |
| Week 2 중반 | Global Exception Handler | 트랜잭션 에러 처리 |
| Week 4 시작 | Request Logging | 성능 측정 필요 |
| Week 6 시작 | 모두 완료 확인 | 이벤트 기반 아키텍처 전 |

---

## 🎯 Claude의 행동 지침

### 추천 타이밍 판단 기준
1. **사용자가 어려움을 겪을 때**
   - 디버깅이 어렵다
   - 같은 문제 반복
   - 코드가 지저분해진다

2. **자연스러운 시점**
   - 새 Week 시작 전
   - 비슷한 코드가 3번 이상 반복될 때
   - 복잡도가 급증할 때

3. **추천 방식**
   - 강요하지 않음
   - "지금 추가하면 좋은 이유" 명확히 설명
   - "10분이면 끝" 등 시간 명시
   - 사용자가 거절하면 존중

### 추천 멘트 템플릿
```
[상황 공감]
→ "디버깅이 어려우시죠?"

[해결책 제시]
→ "Serilog를 추가하면 문제를 10배 빠르게 찾을 수 있어요."

[비용 명시]
→ "30분만 투자하시겠어요?"

[선택권 부여]
→ "지금 할까요, 아니면 나중에 필요할 때 할까요?"
```

---

## ⚠️ 주의사항

### 절대 하지 말아야 할 것
- ❌ 사용자가 원하지 않는데 강요
- ❌ "이거 안 하면 안 됩니다" 식의 압박
- ❌ 한 번에 여러 인프라 추천
- ❌ 인프라 추가를 막는 행동

### 해야 할 것
- ✅ 적절한 타이밍 판단
- ✅ 명확한 이득 설명
- ✅ 코드 예시 제공
- ✅ 사용자 결정 존중

---

## 📝 추가 기록

### 추가된 인프라 체크리스트
- [ ] Serilog
- [ ] Global Exception Handler
- [ ] FluentValidation
- [ ] AutoMapper
- [ ] Request Logging Middleware

이 체크리스트를 업데이트하면서 진행 상황 추적.
