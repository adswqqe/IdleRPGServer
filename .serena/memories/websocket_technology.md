# 웹소켓 기술 계획

## SignalR 준비 상태
현재 프로젝트에는 이미 SignalR을 위한 준비가 되어 있습니다:

### Program.cs에서 확인된 SignalR 설정
```csharp
// SignalR을 위한 토큰 추출 (쿼리 파라미터에서)
options.Events = new JwtBearerEvents
{
    OnMessageReceived = context =>
    {
        var accessToken = context.Request.Query["access_token"];
        var path = context.HttpContext.Request.Path;
        
        if (!string.IsNullOrEmpty(accessToken) && 
            path.StartsWithSegments("/gamehub"))
        {
            context.Token = accessToken;
        }
        return Task.CompletedTask;
    }
};
```

## 방치형 RPG에서의 웹소켓 활용 계획

### 1. 실시간 업데이트
- **경험치/레벨업 알림**: 캐릭터 성장 실시간 전송
- **아이템 획득 알림**: 드롭된 아이템 즉시 알림
- **길드/친구 시스템**: 실시간 채팅, 상태 업데이트

### 2. 게임 상태 동기화
- **오프라인 보상 계산 완료**: 계산 결과 실시간 전송
- **이벤트 알림**: 게임 내 특별 이벤트 즉시 전파
- **서버 점검 알림**: 사전 공지 및 카운트다운

### 3. 필요한 추가 설정

#### NuGet 패키지 추가 (아직 미설치)
```bash
dotnet add package Microsoft.AspNetCore.SignalR
```

#### Hub 클래스 생성 예정
```csharp
// IdleRPG.API/Hubs/GameHub.cs
public class GameHub : Hub
{
    // 실시간 게임 이벤트 처리
}
```

#### Program.cs 추가 설정 필요
```csharp
builder.Services.AddSignalR();
app.MapHub<GameHub>("/gamehub");
```

## Unity 클라이언트 연동
- **SignalR Client for Unity** 사용 예정
- **실시간 데이터 동기화**: 서버 계산 결과를 Unity UI에 즉시 반영
- **백그라운드 연결 유지**: 게임이 비활성화되어도 중요 알림 수신

## 향후 구현 우선순위
1. **기본 SignalR Hub 설정**
2. **레벨업/경험치 실시간 알림**
3. **아이템 획득 실시간 전송**
4. **오프라인 보상 계산 완료 알림**
5. **게임 이벤트 브로드캐스팅**