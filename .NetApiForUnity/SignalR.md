# SignalR

## 🎯 개념 이해
실시간 양방향 통신을 위한 ASP.NET Core 라이브러리 - Unity의 Network 시스템과 유사

## 🔗 연결 관계
- [[Week 5 - Real-time Features]]
- [[Guild Chat System]]
- [[Real-time Notifications]]
- [[Unity SignalR Integration]]

## 💻 핵심 구성
### 서버 측 (Hub)
```csharp
public class GameHub : Hub
{
    public async Task SendMessage(string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", message);
    }
}
```

### Unity 클라이언트
```csharp
connection = new HubConnectionBuilder()
    .WithUrl("https://gameserver.com/gamehub")
    .Build();

await connection.StartAsync();
```

## 🎮 게임 활용 사례
- **길드 채팅**: 실시간 메시지 전송
- **라이브 리더보드**: 순위 변동 실시간 업데이트  
- **푸시 알림**: 게임 이벤트 알림
- **멀티플레이어**: 실시간 플레이어 액션 동기화

## ⚡ 성능 최적화
- **Connection Pooling**: 연결 재사용
- **Group Management**: 그룹별 메시지 전송
- **Backplane**: Redis로 스케일 아웃

## 🔄 연결 관리
```csharp
// Unity에서 연결 상태 관리
private async void HandleConnectionLost()
{
    while (connection.State != HubConnectionState.Connected)
    {
        await Task.Delay(5000);
        try 
        {
            await connection.StartAsync();
        }
        catch { }
    }
}
```

## 📚 관련 학습
- [[WebSocket]]
- [[Real-time Notifications]]
- [[Unity Network]]

---

#SignalR #RealTime #WebSocket #실시간통신