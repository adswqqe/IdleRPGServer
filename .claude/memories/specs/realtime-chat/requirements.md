# Requirements: Realtime Chat System

**Feature**: realtime-chat
**Size**: L (Large)
**New Technology**: SignalR (실시간 통신)
**Created**: 2025-10-28
**Mode**: 학습 모드 (18개 대화형 질문 완료)

---

## 📋 Overview

방치형 MMORPG 게임에 실시간 채팅 시스템을 추가합니다. SignalR을 사용하여 WebSocket 기반 양방향 통신을 구현하고, 전체 채팅, 길드 채팅, 귓속말 기능을 지원합니다.

**핵심 목표**:
- SignalR 실시간 통신 학습 (프로젝트 첫 도입)
- REST API와 SignalR 하이브리드 패턴
- 확장 가능한 채팅 시스템 설계 (이모티콘, 답장 등 미래 확장)

---

## 👤 User Stories

### US-1: 메시지 전송 및 수신
**WHEN** 사용자가 채팅방에 입장하고 메시지를 전송하면
**IF** 메시지가 유효하고 (500자 이하, 쿨다운 준수) 인증된 사용자라면
**THEN** 시스템은 메시지를 저장하고 같은 채팅방의 모든 사용자에게 실시간으로 브로드캐스팅한다
**AND** 발신자 정보 (이름, 레벨)와 메시지 내용, 전송 시간을 포함한다

### US-2: 채팅 히스토리 조회
**WHEN** 사용자가 채팅방에 입장하면
**IF** 인증된 사용자라면
**THEN** 시스템은 최근 50개의 메시지를 반환한다
**AND** 사용자는 스크롤 업 시 과거 메시지를 추가로 로드할 수 있다 (Cursor 페이징)

### US-3: 채팅방 타입별 접근
**WHEN** 사용자가 채팅방에 접근하려 할 때
**IF** Global 채팅방이라면
**THEN** 모든 인증된 사용자가 접근 가능하다
**ELSE IF** Guild 채팅방이라면
**THEN** 해당 길드 멤버만 접근 가능하다 (TODO: 권한 체크)
**ELSE IF** Whisper(귓속말)라면
**THEN** 1:1 참여자만 접근 가능하다 (TODO: 권한 체크)

---

## ✅ Acceptance Criteria (EARS Format)

### AC-1: 메시지 저장 및 브로드캐스팅
**WHEN** SignalR Hub의 SendMessage() 호출 시
**IF** 메시지 길이 ≤ 500자 AND 쿨다운 준수
**THEN** ChatMessage 테이블에 저장
**AND** Clients.Group(roomId).SendAsync("ReceiveMessage", dto) 호출
**AND** 같은 방의 모든 연결된 클라이언트가 메시지 수신

### AC-2: Cursor 기반 페이징
**WHEN** GET /api/chat/rooms/{roomId}/messages?beforeId={guid}&take=50 호출 시
**IF** beforeId가 제공되면
**THEN** WHERE CreatedAt < (beforeId의 CreatedAt) 쿼리 실행
**AND** 최대 50개 (또는 take 값) 반환
**ELSE** 최신 50개 메시지 반환

### AC-3: JWT 인증 (REST + SignalR)
**WHEN** REST API 또는 SignalR Hub 접근 시
**IF** [Authorize] 속성이 적용된 엔드포인트라면
**THEN** JWT Bearer 토큰 필수
**AND** SignalR은 Query String (access_token) 방식으로 JWT 전달
**AND** Context.User에서 CharacterId 추출

### AC-4: N+1 쿼리 방지
**WHEN** 메시지 조회 시
**IF** Include(m => m.Sender) 또는 Projection 사용
**THEN** 단일 JOIN 쿼리 실행 (N+1 발생 안 함)
**AND** AsNoTracking() 적용 (읽기 전용)

### AC-5: 에러 처리 표준화
**WHEN** Hub 메서드에서 예외 발생 시
**IF** InvalidOperationException (비즈니스 로직 에러)
**THEN** Clients.Caller.SendAsync("Error", ErrorDto) 전송
**AND** ErrorDto { Code, Message } 구조
**AND** 클라이언트는 connection.On<ErrorDto>("Error") 핸들러로 처리

---

## 🗃️ Data Model

### Entity 1: ChatRoom
```csharp
public class ChatRoom {
    public Guid Id { get; set; }
    public RoomType Type { get; set; }       // Enum: Global, Guild, Whisper
    public string Name { get; set; }         // "전체", "길드:드래곤", "DM:user1-user2"
    public Guid? GuildId { get; set; }       // Guild 타입일 때만
    public DateTime CreatedAt { get; set; }

    // Navigation Property
    public List<ChatMessage> Messages { get; set; }
}

public enum RoomType {
    Global = 1,
    Guild = 2,
    Whisper = 3
}
```

**PK**: Guid (프로젝트 일관성)
**FK**: GuildId? → Guilds.Id (nullable, Guild 타입일 때만)

---

### Entity 2: ChatMessage
```csharp
public class ChatMessage {
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }         // FK → ChatRoom
    public Guid SenderId { get; set; }       // FK → Character
    public string Content { get; set; }      // VARCHAR(1000)
    public DateTime CreatedAt { get; set; }

    // Navigation Properties
    public ChatRoom Room { get; set; }
    public Character Sender { get; set; }
}
```

**PK**: Guid (프로젝트 일관성)
**FK**:
- RoomId → ChatRoom.Id (ON DELETE RESTRICT)
- SenderId → Character.Id (ON DELETE RESTRICT)

**Constraints**:
- Content: VARCHAR(1000) NOT NULL
- Application 검증: 500자 제한 (DB는 여유 공간)

**Indexes**:
```sql
CREATE INDEX idx_chatmessages_roomid_createdat
ON ChatMessages(RoomId, CreatedAt DESC);
```
→ WHERE RoomId + ORDER BY CreatedAt 최적화

---

### DTO: ChatMessageDto (Nested)
```csharp
public class ChatMessageDto {
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public CharacterSummaryDto Sender { get; set; }  // 중첩
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }

    // 미래 확장 (Nullable)
    public List<ReactionDto>? Reactions { get; set; }
    public bool? IsEdited { get; set; }
    public Guid? ParentMessageId { get; set; }  // 답장 기능
}

public class CharacterSummaryDto {
    public Guid Id { get; set; }
    public string Name { get; set; }
    public int Level { get; set; }
    public string? AvatarUrl { get; set; }  // 미래 확장
}

public class ErrorDto {
    public string Code { get; set; }     // "INVALID_MESSAGE", "SERVER_ERROR"
    public string Message { get; set; }  // 사용자 표시 메시지
}
```

---

## 🔌 REST API Endpoints

### 1. GET /api/chat/rooms/{roomId}/messages
**Description**: 채팅방 메시지 히스토리 조회 (Cursor 페이징)

**Auth**: `[Authorize]` (JWT Bearer)

**Request**:
```
Query Parameters:
- beforeId (optional): Guid - 이 메시지 이전의 메시지들 조회
- take (optional): int - 조회 개수 (기본 50, 최대 100)
```

**Response** (200 OK):
```json
[
  {
    "id": "guid",
    "roomId": "guid",
    "sender": {
      "id": "guid",
      "name": "용사123",
      "level": 50
    },
    "content": "안녕하세요!",
    "createdAt": "2025-10-28T10:00:00Z"
  }
]
```

**Error Responses**:
- 401 Unauthorized: JWT 토큰 없음/만료
- 403 Forbidden: 채팅방 접근 권한 없음 (TODO: Guild/Whisper 권한 체크)
- 404 Not Found: 채팅방 미존재

---

### 2. GET /api/chat/rooms
**Description**: 채팅방 목록 조회

**Auth**: `[Authorize]`

**Response** (200 OK):
```json
[
  {
    "id": "guid",
    "type": "Global",
    "name": "전체 채팅",
    "lastMessage": {
      "content": "최근 메시지...",
      "createdAt": "2025-10-28T10:00:00Z"
    }
  }
]
```

---

## ⚡ SignalR Hub Methods

### Hub: ChatHub (API Layer)

**Route**: `/chat`
**Auth**: `[Authorize]`

---

### Method 1: JoinRoom
**Description**: 채팅방 입장 (SignalR Group 가입)

**Client → Server**:
```csharp
await connection.InvokeAsync("JoinRoom", "roomId-guid");
```

**Server Logic**:
```csharp
public async Task JoinRoom(string roomId) {
    var characterId = GetCurrentCharacterId();

    // TODO(human): 권한 체크
    // if (!await _chatService.CanAccessRoomAsync(characterId, roomId))
    //     throw new InvalidOperationException("접근 권한이 없습니다.");

    await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

    // 다른 사용자에게 알림 (옵션)
    await Clients.Group(roomId).SendAsync("UserJoined", new {
        CharacterId = characterId,
        ConnectionId = Context.ConnectionId
    });
}
```

---

### Method 2: LeaveRoom
**Description**: 채팅방 퇴장 (SignalR Group 탈퇴)

**Client → Server**:
```csharp
await connection.InvokeAsync("LeaveRoom", "roomId-guid");
```

**Server Logic**:
```csharp
public async Task LeaveRoom(string roomId) {
    await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);

    await Clients.Group(roomId).SendAsync("UserLeft", new {
        ConnectionId = Context.ConnectionId
    });
}
```

---

### Method 3: SendMessage
**Description**: 메시지 전송

**Client → Server**:
```csharp
await connection.InvokeAsync("SendMessage", "roomId-guid", "메시지 내용");
```

**Server Logic**:
```csharp
public async Task SendMessage(string roomId, string content) {
    try {
        var senderId = GetCurrentCharacterId();

        // Service에서 저장 + 검증
        var messageDto = await _chatService.SendMessageAsync(
            Guid.Parse(roomId), senderId, content);

        // 브로드캐스팅
        await Clients.Group(roomId).SendAsync("ReceiveMessage", messageDto);
    }
    catch (InvalidOperationException ex) {
        // 비즈니스 로직 에러
        await Clients.Caller.SendAsync("Error", new ErrorDto {
            Code = "INVALID_MESSAGE",
            Message = ex.Message
        });
    }
    catch (Exception ex) {
        // 서버 에러
        _logger.LogError(ex, "SendMessage unexpected error");
        await Clients.Caller.SendAsync("Error", new ErrorDto {
            Code = "SERVER_ERROR",
            Message = "메시지 전송에 실패했습니다."
        });
    }
}
```

---

### Method 4: Typing (미래 확장)
**Description**: 타이핑 중 알림

**Client → Server**:
```csharp
await connection.InvokeAsync("Typing", "roomId-guid");
```

**Server → Other Clients**:
```csharp
public async Task Typing(string roomId) {
    var characterId = GetCurrentCharacterId();

    // 나를 제외한 같은 방 사용자에게
    await Clients.OthersInGroup(roomId).SendAsync("UserTyping", new {
        CharacterId = characterId
    });
}
```

---

### Client Events (Server → Client)

**1. ReceiveMessage**
```csharp
connection.On<ChatMessageDto>("ReceiveMessage", (message) => {
    ShowMessage(message);
});
```

**2. Error**
```csharp
connection.On<ErrorDto>("Error", (error) => {
    switch (error.Code) {
        case "INVALID_MESSAGE":
            ShowToast(error.Message);
            break;
        case "SERVER_ERROR":
            ShowErrorDialog(error.Message);
            break;
    }
});
```

**3. UserJoined / UserLeft (옵션)**
```csharp
connection.On<object>("UserJoined", (data) => {
    Debug.Log($"사용자 입장: {data.CharacterId}");
});
```

**4. UserTyping (미래 확장)**
```csharp
connection.On<object>("UserTyping", (data) => {
    ShowTypingIndicator(data.CharacterId);
});
```

---

## 🏗️ Architecture & Technical Requirements

### Clean Architecture Layers

**API Layer** (`IdleRPG.API`):
- `Controllers/ChatController.cs` - REST API 엔드포인트
- `Hubs/ChatHub.cs` - SignalR Hub (JoinRoom, SendMessage, ...)

**Application Layer** (`IdleRPG.Application`):
- `Interfaces/IChatService.cs` - Service 인터페이스
- `DTOs/Chat/ChatMessageDto.cs` - DTO 정의

**Infrastructure Layer** (`IdleRPG.Infrastructure`):
- `Services/ChatService.cs` - Service 구현 (비즈니스 로직)
- `Repositories/ChatRoomRepository.cs` - Repository 구현
- `Repositories/ChatMessageRepository.cs` - Repository 구현

**Domain Layer** (`IdleRPG.Domain`):
- `Entities/ChatRoom.cs` - Entity
- `Entities/ChatMessage.cs` - Entity
- `Enums/RoomType.cs` - Enum
- `Repositories/IChatRoomRepository.cs` - Repository 인터페이스
- `Repositories/IChatMessageRepository.cs` - Repository 인터페이스

---

### Unit of Work Pattern

**IUnitOfWork에 추가**:
```csharp
public interface IUnitOfWork : IDisposable {
    // 기존 Repository들...

    IChatRoomRepository ChatRooms { get; }      // 추가
    IChatMessageRepository ChatMessages { get; }  // 추가

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

**UnitOfWork 구현**:
```csharp
public class UnitOfWork : IUnitOfWork {
    private IChatRoomRepository? _chatRooms;
    private IChatMessageRepository? _chatMessages;

    public IChatRoomRepository ChatRooms {
        get {
            if (_chatRooms == null)
                _chatRooms = new ChatRoomRepository(_context);
            return _chatRooms;
        }
    }

    public IChatMessageRepository ChatMessages {
        get {
            if (_chatMessages == null)
                _chatMessages = new ChatMessageRepository(_context);
            return _chatMessages;
        }
    }
}
```

---

### SignalR Configuration (Program.cs)

```csharp
// JWT 인증 설정 (SignalR용)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters { ... };

        // SignalR Query String 토큰
        options.Events = new JwtBearerEvents {
            OnMessageReceived = context => {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/chat")) {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

// SignalR 추가
builder.Services.AddSignalR(options => {
    options.MaximumReceiveMessageSize = 1024 * 100;  // 100KB
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
    options.KeepAliveInterval = TimeSpan.FromSeconds(30);
});

// CORS (Unity 클라이언트용)
builder.Services.AddCors(options => {
    options.AddPolicy("AllowUnity", policy => {
        policy.WithOrigins("http://localhost:*")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();  // SignalR 필수
    });
});

var app = builder.Build();

app.UseCors("AllowUnity");
app.UseAuthentication();
app.UseAuthorization();

// Hub 라우팅
app.MapHub<ChatHub>("/chat").RequireAuthorization();

app.Run();
```

---

### Connection Lifecycle (OnConnectedAsync, OnDisconnectedAsync)

```csharp
[Authorize]
public class ChatHub : Hub {
    private readonly ILogger<ChatHub> _logger;

    public override async Task OnConnectedAsync() {
        var characterId = GetCurrentCharacterId();
        var connectionId = Context.ConnectionId;

        _logger.LogInformation(
            "User {CharacterId} connected with ConnectionId {ConnectionId}",
            characterId, connectionId);

        // TODO(human): 온라인 상태 업데이트
        // await _unitOfWork.Characters.UpdateOnlineStatusAsync(characterId, true);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception) {
        var characterId = GetCurrentCharacterId();

        if (exception != null) {
            _logger.LogWarning(exception,
                "User {CharacterId} disconnected with error", characterId);
        } else {
            _logger.LogInformation(
                "User {CharacterId} disconnected gracefully", characterId);
        }

        // TODO(human): 오프라인 상태 업데이트
        // await _unitOfWork.Characters.UpdateOnlineStatusAsync(characterId, false);

        await base.OnDisconnectedAsync(exception);
    }
}
```

---

## 🎮 Game Balance (AI 제안, 승인됨)

### 1. 메시지 길이 제한
- **Application 검증**: 500자
- **Database 제한**: 1000자 (여유)
- **이유**: 일반 게임 채팅 표준, 스팸 방지

### 2. 채팅 쿨다운 (스팸 방지)
- **일반 사용자**: 1초당 1개 메시지
- **VIP 사용자**: 제한 없음 (미래 확장)
- **연속 5개 초과 시**: 10초 쿨다운
- **이유**: Discord/Slack 표준 패턴

### 3. 채팅 히스토리 조회 제한
- **한 번에 최대**: 100개 메시지
- **최소**: 10개
- **기본값**: 50개
- **이유**: 네트워크 부하 방지

### 4. 욕설 필터 (TODO)
- **기본 욕설 목록**: 한국어 금지어 50개
- **처리 방식**: `***` 치환
- **관리자**: 필터 우회 가능
- **이유**: 게임 커뮤니티 표준

### 5. 채팅방 타입별 기본 설정
- **Global 채팅**: 모든 인증 사용자, 쿨다운 1초
- **Guild 채팅**: 길드 멤버만, 쿨다운 0.5초
- **Whisper (귓속말)**: 1:1, 쿨다운 없음

---

## 🔗 Dependencies

### New Dependencies
- **Microsoft.AspNetCore.SignalR** (ASP.NET Core 8.0 포함)
- **NuGet 추가 패키지 없음** (SignalR은 ASP.NET Core에 내장)

### Future Dependencies (학습 목적)
- **StackExchange.Redis** (Redis Backplane, Scale-out 시)
- **Microsoft.AspNetCore.SignalR.StackExchangeRedis**

---

## 📝 TODO(human) - 아키텍처 학습 포인트

### 1. Content 검증 로직 (Service Layer)
**위치**: `ChatService.SendMessageAsync()`

```csharp
public async Task<ChatMessageDto> SendMessageAsync(
    Guid roomId, Guid senderId, string content)
{
    // TODO(human): 메시지 검증 로직 작성
    // 1. 길이 체크 (500자)
    if (content.Length > 500)
        throw new InvalidOperationException("메시지는 500자 이하여야 합니다.");

    // 2. 욕설 필터
    // content = ApplyProfanityFilter(content);

    // 3. 쿨다운 체크
    // if (!await CheckCooldownAsync(senderId))
    //     throw new InvalidOperationException("메시지를 너무 빠르게 전송하고 있습니다.");

    // 메시지 저장...
}
```

**학습 목표**:
- Application Layer 검증 로직 작성
- 비즈니스 규칙 구현 (쿨다운, 필터)

---

### 2. 채팅방 권한 체크 (Service Layer)
**위치**: `ChatService.CanAccessRoomAsync()` (새 메서드)

```csharp
// TODO(human): 채팅방 타입별 권한 체크 로직
public async Task<bool> CanAccessRoomAsync(Guid characterId, Guid roomId) {
    var room = await _unitOfWork.ChatRooms.GetByIdAsync(roomId);

    switch (room.Type) {
        case RoomType.Global:
            return true;  // 모두 접근 가능

        case RoomType.Guild:
            // 길드 멤버인지 확인
            var character = await _unitOfWork.Characters.GetByIdAsync(characterId);
            return character.GuildId == room.GuildId;

        case RoomType.Whisper:
            // 귓속말 참여자인지 확인
            // return await IsWhisperParticipantAsync(characterId, roomId);
            return false;  // TODO

        default:
            return false;
    }
}
```

**학습 목표**:
- Enum 기반 비즈니스 로직 분기
- 권한 체크 패턴 (Authorization)

---

### 3. 온라인 상태 업데이트 (Hub Lifecycle)
**위치**: `ChatHub.OnConnectedAsync()`, `OnDisconnectedAsync()`

```csharp
// TODO(human): Character.IsOnline 상태 업데이트
public override async Task OnConnectedAsync() {
    var characterId = GetCurrentCharacterId();

    // 온라인 상태로 변경
    // await _unitOfWork.Characters.UpdateOnlineStatusAsync(characterId, true);
    // await _unitOfWork.SaveChangesAsync();

    await base.OnConnectedAsync();
}

public override async Task OnDisconnectedAsync(Exception? exception) {
    var characterId = GetCurrentCharacterId();

    // 오프라인 상태로 변경
    // await _unitOfWork.Characters.UpdateOnlineStatusAsync(characterId, false);
    // await _unitOfWork.SaveChangesAsync();

    await base.OnDisconnectedAsync(exception);
}
```

**필요 시 Character Entity 확장**:
```csharp
public class Character {
    // 기존 프로퍼티들...

    public bool IsOnline { get; set; }          // 추가
    public DateTime? LastSeenAt { get; set; }   // 추가
}
```

**학습 목표**:
- SignalR 연결 생명주기 관리
- Entity 확장 및 마이그레이션

---

## 🚀 Next Steps

### 1. Spike 제안 (SignalR 성능 검증)
**Question**: SignalR로 100명 동시접속 시 메시지 지연 < 200ms?
**Why**: Requirements AC-1 검증 필요 (새 기술 첫 도입)
**Method**:
1. SignalR 테스트 프로젝트 생성
2. 100 가상 클라이언트 연결 시뮬레이션
3. 메시지 전송 → 수신 지연 측정

**Expected**: 30분
**Success Criteria**: 평균 < 200ms → Go, 아니면 No-Go

**실행하시겠습니까?** [Y/n/Skip]

---

### 2. Design 단계
Requirements 승인 후:
```bash
/spec-design realtime-chat
```

**Design 단계에서 작성**:
- Component 설계 (ChatHub, ChatService, Repositories)
- Sequence Diagram (메시지 전송 흐름)
- Database Schema (EF Core Configuration)
- ADR (SignalR vs WebSocket vs Polling 기술 선택)

---

## ✅ Approval

- [ ] **Requirements 검토 완료** (User Stories, AC 확인)
- [ ] **Data Model 승인** (ChatRoom, ChatMessage Entity)
- [ ] **API 설계 승인** (REST + SignalR 하이브리드)
- [ ] **TODO(human) 확인** (3개 학습 포인트)
- [ ] **게임 밸런스 승인** (AI 제안값)
- [ ] **Spike 실행 여부 결정** (SignalR 성능 검증)

---

**Status**: Ready for Review
**Next**: Spike (선택) → `/spec-design realtime-chat`
