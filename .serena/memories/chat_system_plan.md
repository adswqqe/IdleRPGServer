# 채팅 시스템 구현 계획

## 채팅 시스템 아키텍처

### SignalR 기반 실시간 채팅
방치형 RPG에서 채팅은 커뮤니티 형성에 핵심적인 역할을 합니다.

### 예상 채팅 유형

#### 1. 전체 채팅 (Global Chat)
- **모든 온라인 플레이어** 참여 가능
- **레벨 제한**: 스팸 방지를 위한 최소 레벨 요구
- **쿨다운**: 메시지 간격 제한 (예: 3초)

#### 2. 길드 채팅 (Guild Chat)
- **길드 멤버 전용** 채팅
- **길드 공지**: 길드장/부길드장 전용 공지 기능
- **길드 이벤트**: 보스 레이드, 길드 전쟁 등

#### 3. 개인 채팅 (Private Message)
- **1:1 대화** 시스템
- **친구 목록** 연동
- **오프라인 메시지** 저장

#### 4. 시스템 채팅 (System Chat)
- **서버 공지사항**
- **이벤트 알림** (예: "플레이어 XXX가 레전더리 아이템을 획득했습니다!")
- **점검 알림**

## 데이터베이스 설계 (예정)

### ChatMessage Entity
```csharp
public class ChatMessage
{
    public Guid Id { get; set; }
    public Guid SenderId { get; set; }
    public Player Sender { get; set; }
    public string Content { get; set; }
    public ChatType Type { get; set; }
    public Guid? ChannelId { get; set; } // 길드ID 또는 개인채팅 상대방ID
    public DateTime CreatedAt { get; set; }
    public bool IsDeleted { get; set; }
}

public enum ChatType
{
    Global,
    Guild,
    Private,
    System
}
```

### ChatChannel Entity (길드/개인채팅용)
```csharp
public class ChatChannel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public ChatChannelType Type { get; set; }
    public List<ChatMessage> Messages { get; set; }
    public List<Player> Members { get; set; }
}
```

## SignalR Hub 설계

### GameHub 확장 계획
```csharp
public class GameHub : Hub
{
    // 채팅 그룹 입장/퇴장
    public async Task JoinGlobalChat()
    public async Task JoinGuildChat(string guildId)
    
    // 메시지 전송
    public async Task SendGlobalMessage(string message)
    public async Task SendGuildMessage(string guildId, string message)
    public async Task SendPrivateMessage(string targetUserId, string message)
    
    // 연결 관리
    public override async Task OnConnectedAsync()
    public override async Task OnDisconnectedAsync(Exception exception)
}
```

## 클라이언트 연동 계획

### Unity SignalR Client
- **실시간 메시지 수신**: 채팅창 자동 업데이트
- **타이핑 인디케이터**: "XXX님이 입력 중..." 표시
- **알림 배지**: 읽지 않은 메시지 개수 표시

## 보안 및 관리 기능

### 스팸 방지
- **레이트 리미팅**: 메시지 전송 빈도 제한
- **욕설 필터**: 부적절한 언어 자동 차단
- **신고 시스템**: 사용자 신고 및 관리자 검토

### 관리자 기능
- **채팅 로그**: 모든 채팅 기록 저장
- **사용자 제재**: 채팅 금지, 계정 정지
- **실시간 모니터링**: 관리자 채팅 감시 도구

## 추가 구현 예정 기능

### 이모지 및 스티커
- **기본 이모지**: 텍스트 기반 이모지 (:smile:, :heart: 등)
- **게임 전용 스티커**: 캐릭터/아이템 관련 스티커

### 채팅 명령어
- **/who**: 온라인 플레이어 수 확인
- **/guild**: 길드 정보 조회  
- **/friend**: 친구 목록 확인
- **/block**: 특정 사용자 차단

## 성능 고려사항
- **메시지 페이징**: 오래된 메시지 분할 로딩
- **Redis 캐싱**: 최근 채팅 메시지 캐시
- **Connection Scaling**: 대량 동접 시 SignalR 스케일링