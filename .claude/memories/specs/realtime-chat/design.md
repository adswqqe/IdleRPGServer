# Design: Realtime Chat System

> 이 문서는 실시간 채팅 시스템의 기술 설계를 정의합니다.
>
> **작성 가이드**:
> - Requirements의 모든 항목이 어떻게 구현될지 설명
> - Clean Architecture 계층별로 구성요소 정의
> - SignalR 실시간 통신 패턴 적용

**Requirements 추적성**: [requirements.md](./requirements.md) (US-1, US-2, US-3, AC-1~AC-5)

---

## 📐 Architecture Overview

### Layer Responsibilities

#### API Layer (`IdleRPG.API`)
- **Controllers**:
  - `ChatController.cs` - REST API (채팅 히스토리 조회, 채팅방 목록)
- **Hubs**:
  - `ChatHub.cs` - SignalR Hub (JoinRoom, LeaveRoom, SendMessage, Typing)
- **DTOs**: Request/Response 구조 (Application Layer에서 정의)

**Requirements**: US-1 (메시지 전송), US-2 (히스토리 조회), AC-1 (브로드캐스팅)

---

#### Application Layer (`IdleRPG.Application`)
- **Service Interface**:
  - `IChatService.cs` - 채팅 비즈니스 로직 인터페이스
- **DTOs**:
  - `ChatMessageDto.cs` - 메시지 DTO (중첩 구조: CharacterSummaryDto)
  - `ErrorDto.cs` - SignalR 에러 응답
- **Validation**:
  - FluentValidation 또는 수동 검증 (메시지 길이, 쿨다운)

**Requirements**: US-1 (검증 로직), AC-2 (Cursor 페이징)

---

#### Domain Layer (`IdleRPG.Domain`)
- **Entities**:
  - `ChatRoom.cs` - 채팅방 Entity
  - `ChatMessage.cs` - 메시지 Entity
- **Enums**:
  - `RoomType.cs` - Global, Guild, Whisper
- **Repository Interfaces**:
  - `IChatRoomRepository.cs`
  - `IChatMessageRepository.cs`

**Requirements**: US-3 (채팅방 타입), AC-3 (인증)

---

#### Infrastructure Layer (`IdleRPG.Infrastructure`)
- **Repositories**:
  - `ChatRoomRepository.cs` - EF Core 구현
  - `ChatMessageRepository.cs` - EF Core 구현 (Cursor 페이징)
- **Services**:
  - `ChatService.cs` - 비즈니스 로직 구현
- **Configurations**:
  - `ChatRoomConfiguration.cs` - Fluent API 설정
  - `ChatMessageConfiguration.cs` - Fluent API 설정
- **Unit of Work**:
  - `IUnitOfWork` 확장 (ChatRooms, ChatMessages 추가)

**Requirements**: AC-4 (N+1 방지), AC-5 (에러 처리)

---

## 🗄️ Data Model

> ⚠️ **개념적 명세만 작성** - SQL DDL은 Implementation 단계에서 작성
> 참고: [CLAUDE.md - Design vs Implementation 경계](../../../CLAUDE.md#design-vs-implementation-경계)

### 신규 테이블: `ChatRooms`

**목적**: 채팅방 정보 저장 (전체, 길드, 귓속말)

**필드**:
- `Id` (PK): GUID, 고유 식별자
- `Type`: Enum (RoomType: Global=1, Guild=2, Whisper=3)
- `Name`: 문자열, 채팅방 이름 (예: "전체 채팅", "길드:드래곤슬레이어")
  - 필수, 최대 100자
- `GuildId` (FK, nullable): GUID, 길드 ID (Guild 타입일 때만 사용)
  - Guild 타입: NOT NULL, 다른 타입: NULL
- `CreatedAt`: DateTime, 생성 시간 (UTC)

**제약사항**:
- `Name`: NOT NULL, 2-100자
- `Type`: NOT NULL, Enum 범위 (1-3)
- `GuildId`: Guild 타입일 때만 NOT NULL, 나머지는 NULL

**인덱스 요구사항**:
- `IX_ChatRooms_Type`: Type별 조회 빈번 (전체/길드/귓속말 필터링)
- `IX_ChatRooms_GuildId`: 길드 채팅방 조회 (Guild 멤버 접속 시)

**관계**:
- `Guilds`와 N:1 관계 (FK: `GuildId`, ON DELETE RESTRICT)
  - Guild 삭제 시 채팅방 보존 (별도 정리 로직 필요)
- `ChatMessages`와 1:N 관계 (역방향)

**Requirements**: US-3 (채팅방 타입별 접근)

---

### 신규 테이블: `ChatMessages`

**목적**: 채팅 메시지 저장 및 히스토리 조회

**필드**:
- `Id` (PK): GUID, 고유 식별자
- `RoomId` (FK): GUID, 채팅방 ID
  - NOT NULL, ChatRooms.Id 참조
- `SenderId` (FK): GUID, 발신자 Character ID
  - NOT NULL, Characters.Id 참조
- `Content`: 문자열, 메시지 내용
  - NOT NULL, 최대 1000자 (Application 검증: 500자)
- `CreatedAt`: DateTime, 전송 시간 (UTC)
  - NOT NULL, Cursor 페이징 기준

**제약사항**:
- `Content`: NOT NULL, 1-1000자
- `RoomId`, `SenderId`: NOT NULL, FK 제약
- `CreatedAt`: NOT NULL, 기본값 UTC_NOW

**인덱스 요구사항** (성능 최적화):
- `IX_ChatMessages_RoomId_CreatedAt`: 복합 인덱스 (RoomId ASC, CreatedAt DESC)
  - 이유: `WHERE RoomId = ? ORDER BY CreatedAt DESC` 쿼리 최적화 (AC-2 Cursor 페이징)
  - **가장 중요한 인덱스** (모든 히스토리 조회에 사용)

**관계**:
- `ChatRooms`와 N:1 관계 (FK: `RoomId`, ON DELETE RESTRICT)
  - 채팅방 삭제 시 메시지 보존 (규정 준수)
- `Characters`와 N:1 관계 (FK: `SenderId`, ON DELETE RESTRICT)
  - 캐릭터 삭제 시 메시지 보존 (익명화 처리 권장)

**Requirements**: US-1 (메시지 저장), US-2 (히스토리 조회), AC-2 (Cursor 페이징), AC-4 (N+1 방지)

---

### Entity Relationships

**ERD 개요**:
```
Player (1) ──< (N) Character (1) ──< (N) ChatMessages
                       │
                       │ (N:1 SenderId)
                       │
                       ▼
Guild (1) ──< (N) ChatRooms (1) ──< (N) ChatMessages
                 (nullable)            (RoomId)
```

**관계 설명**:
- `ChatMessage` → `Character`: N:1 (SenderId)
  - 한 캐릭터가 여러 메시지 전송
- `ChatMessage` → `ChatRoom`: N:1 (RoomId)
  - 한 채팅방에 여러 메시지
- `ChatRoom` → `Guild`: N:1 (GuildId, nullable)
  - Guild 타입 채팅방만 길드 참조

**Cascade 규칙**:
- `ChatRoom` 삭제 시: ChatMessages 보존 (RESTRICT)
- `Character` 삭제 시: ChatMessages 보존 (RESTRICT), 익명화 권장
- `Guild` 삭제 시: ChatRooms 보존 (RESTRICT), 별도 정리 로직

**Requirements**: US-3 (채팅방 타입), AC-4 (N+1 방지)

---

### DTO Design

**ChatMessageDto** (Application Layer):
```
필드:
- Id: Guid
- RoomId: Guid
- Sender: CharacterSummaryDto (중첩)
  - Id: Guid
  - Name: string
  - Level: int
  - AvatarUrl: string? (미래 확장)
- Content: string (1-500자)
- CreatedAt: DateTime (UTC)
- Reactions: List<ReactionDto>? (미래 확장, nullable)
- IsEdited: bool? (미래 확장, nullable)
- ParentMessageId: Guid? (답장 기능, nullable)

용도:
- SignalR ReceiveMessage 이벤트
- REST API 히스토리 조회 응답
```

**ErrorDto** (Application Layer):
```
필드:
- Code: string (예: "INVALID_MESSAGE", "COOLDOWN_ACTIVE")
- Message: string (사용자 표시 메시지)

용도:
- SignalR Error 이벤트
- 비즈니스 로직 에러 전달
```

**Requirements**: US-1 (발신자 정보 포함), AC-5 (에러 처리)

---

### EF Core Configuration 요구사항

**ChatRoomConfiguration.cs**:
- `HasMany(r => r.Messages).WithOne(m => m.Room).HasForeignKey(m => m.RoomId).OnDelete(DeleteBehavior.Restrict)`
- `HasOne(r => r.Guild).WithMany().HasForeignKey(r => r.GuildId).OnDelete(DeleteBehavior.Restrict).IsRequired(false)`
- `Property(r => r.Name).HasMaxLength(100).IsRequired()`
- `Property(r => r.Type).HasConversion<int>()` (Enum → int)

**ChatMessageConfiguration.cs**:
- `HasOne(m => m.Room).WithMany(r => r.Messages).HasForeignKey(m => m.RoomId).OnDelete(DeleteBehavior.Restrict)`
- `HasOne(m => m.Sender).WithMany().HasForeignKey(m => m.SenderId).OnDelete(DeleteBehavior.Restrict)`
- `Property(m => m.Content).HasMaxLength(1000).IsRequired()`
- `HasIndex(m => new { m.RoomId, m.CreatedAt })` (복합 인덱스)

> 💡 **구현 참고**: 구체적인 Fluent API 코드는 `IdleRPG.Infrastructure/Configurations/` 에서 작성

**Requirements**: AC-4 (N+1 방지), Migration Plan

---

## 🔌 API Design

### REST API Endpoints

#### `GET /api/chat/rooms/{roomId}/messages`
**Description**: 채팅방 메시지 히스토리 조회 (Cursor 기반 페이징)

**Auth**: `[Authorize]` (JWT Bearer)

**Path Parameters**:
- `roomId`: Guid, 채팅방 ID

**Query Parameters**:
- `beforeId` (optional): Guid, 이 메시지 이전의 메시지들 조회
  - Cursor 페이징 기준
  - 제공되지 않으면 최신 메시지부터
- `take` (optional): int, 조회 개수
  - 기본값: 50
  - 최소: 10, 최대: 100
  - 범위 초과 시 400 Bad Request

**Request Example**:
```
GET /api/chat/rooms/12345678-1234-1234-1234-123456789abc/messages?beforeId=87654321-4321-4321-4321-cba987654321&take=50
Authorization: Bearer {JWT_TOKEN}
```

**Response (200 OK)**:
```json
[
  {
    "id": "guid-1",
    "roomId": "guid-room",
    "sender": {
      "id": "guid-char",
      "name": "용사123",
      "level": 50,
      "avatarUrl": null
    },
    "content": "안녕하세요!",
    "createdAt": "2025-10-29T10:00:00Z",
    "reactions": null,
    "isEdited": null,
    "parentMessageId": null
  }
]
```

**Errors**:
- `400 Bad Request`: roomId 형식 오류, take 범위 초과
- `401 Unauthorized`: JWT 토큰 없음/만료
- `403 Forbidden`: 채팅방 접근 권한 없음 (Guild/Whisper)
- `404 Not Found`: 채팅방 미존재

**Service 메서드**: `IChatService.GetMessagesAsync(roomId, characterId, beforeId?, take)`

**Requirements**: US-2 (히스토리 조회), AC-2 (Cursor 페이징)

---

#### `GET /api/chat/rooms`
**Description**: 사용자가 접근 가능한 채팅방 목록 조회

**Auth**: `[Authorize]`

**Response (200 OK)**:
```json
[
  {
    "id": "guid",
    "type": "Global",
    "name": "전체 채팅",
    "lastMessage": {
      "content": "최근 메시지...",
      "sender": "용사123",
      "createdAt": "2025-10-29T10:00:00Z"
    }
  },
  {
    "id": "guid",
    "type": "Guild",
    "name": "길드:드래곤슬레이어",
    "lastMessage": null
  }
]
```

**Errors**:
- `401 Unauthorized`: JWT 토큰 없음/만료

**Service 메서드**: `IChatService.GetAccessibleRoomsAsync(characterId)`

**Requirements**: US-3 (채팅방 타입별 접근)

---

### SignalR Hub Methods

#### Hub: `ChatHub` (`/chat`)

**Auth**: `[Authorize]` (JWT Query String: `?access_token={token}`)

**Base Class**: `Hub`

**Dependencies**:
- `IChatService` - 비즈니스 로직
- `ILogger<ChatHub>` - 로깅

---

#### Method 1: `JoinRoom(string roomId)`

**Description**: 채팅방 입장 (SignalR Group 가입)

**Client → Server**:
```csharp
await connection.InvokeAsync("JoinRoom", "roomId-guid");
```

**프로세스 흐름**:
1. JWT에서 CharacterId 추출 (`Context.User.FindFirst("characterId")`)
2. 권한 체크: `await _chatService.CanAccessRoomAsync(characterId, roomId)`
   - 실패 시: `Clients.Caller.SendAsync("Error", ErrorDto { Code="FORBIDDEN" })`
3. SignalR Group 가입: `await Groups.AddToGroupAsync(Context.ConnectionId, roomId)`
4. 브로드캐스트 (선택): `await Clients.Group(roomId).SendAsync("UserJoined", { CharacterId, ConnectionId })`

**에러 조건**:
- roomId 형식 오류: `INVALID_ROOM_ID`
- 권한 없음: `FORBIDDEN`

**Requirements**: US-3 (채팅방 접근 권한), AC-3 (JWT 인증)

---

#### Method 2: `LeaveRoom(string roomId)`

**Description**: 채팅방 퇴장 (SignalR Group 탈퇴)

**Client → Server**:
```csharp
await connection.InvokeAsync("LeaveRoom", "roomId-guid");
```

**프로세스 흐름**:
1. SignalR Group 탈퇴: `await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId)`
2. 브로드캐스트 (선택): `await Clients.Group(roomId).SendAsync("UserLeft", { ConnectionId })`

**Requirements**: US-1 (메시지 수신 중단)

---

#### Method 3: `SendMessage(string roomId, string content)`

**Description**: 메시지 전송 (검증 + 저장 + 브로드캐스팅)

**Client → Server**:
```csharp
await connection.InvokeAsync("SendMessage", "roomId-guid", "메시지 내용");
```

**프로세스 흐름**:
1. JWT에서 SenderId 추출
2. Service 호출: `var dto = await _chatService.SendMessageAsync(roomId, senderId, content)`
   - 내부 로직:
     - 메시지 길이 검증 (500자)
     - 쿨다운 체크 (1초)
     - ChatMessage Entity 저장
     - ChatMessageDto 반환 (Include Sender)
3. 브로드캐스팅: `await Clients.Group(roomId).SendAsync("ReceiveMessage", dto)`
4. 에러 발생 시:
   - `InvalidOperationException` → `Clients.Caller.SendAsync("Error", ErrorDto)`
   - 기타 Exception → 로그 + 일반 에러 응답

**에러 조건**:
- 메시지 길이 초과: `INVALID_MESSAGE` (500자 초과)
- 쿨다운 위반: `COOLDOWN_ACTIVE` (1초 이내 재전송)
- 권한 없음: `FORBIDDEN`
- 서버 에러: `SERVER_ERROR`

**Requirements**: US-1 (메시지 전송 및 수신), AC-1 (브로드캐스팅), AC-5 (에러 처리)

---

#### Method 4: `Typing(string roomId)` (미래 확장)

**Description**: 타이핑 중 알림 (다른 사용자에게 표시)

**Client → Server**:
```csharp
await connection.InvokeAsync("Typing", "roomId-guid");
```

**프로세스 흐름**:
1. CharacterId 추출
2. 나를 제외한 같은 방 사용자: `await Clients.OthersInGroup(roomId).SendAsync("UserTyping", { CharacterId })`

**Requirements**: (미래 확장)

---

### Client Events (Server → Client)

**1. ReceiveMessage** (메시지 수신):
```csharp
connection.On<ChatMessageDto>("ReceiveMessage", (message) => {
    ShowMessage(message);
});
```

**2. Error** (에러 처리):
```csharp
connection.On<ErrorDto>("Error", (error) => {
    switch (error.Code) {
        case "INVALID_MESSAGE": ShowToast(error.Message); break;
        case "COOLDOWN_ACTIVE": ShowCooldownTimer(); break;
        case "FORBIDDEN": RedirectToLobby(); break;
        case "SERVER_ERROR": ShowErrorDialog(error.Message); break;
    }
});
```

**3. UserJoined / UserLeft** (선택):
```csharp
connection.On<object>("UserJoined", (data) => {
    Debug.Log($"사용자 입장: {data.CharacterId}");
});
```

**4. UserTyping** (미래 확장):
```csharp
connection.On<object>("UserTyping", (data) => {
    ShowTypingIndicator(data.CharacterId);
});
```

**Requirements**: US-1 (실시간 수신), AC-5 (에러 처리)

---

## 🧮 Business Logic

### 핵심 알고리즘

#### 메시지 검증 (AI 제안 - 게임 밸런스)

**입력**:
- `content`: string (메시지 내용)
- `senderId`: Guid (발신자 ID)

**출력**:
- 검증 통과: void
- 실패: `InvalidOperationException`

**프로세스 (AI 제안)**:
1. **길이 체크**:
   - `if (content.Length > 500)` → Exception("메시지는 500자 이하여야 합니다.")
   - 이유: 일반 게임 채팅 표준, 스팸 방지
2. **쿨다운 체크** (Redis 또는 메모리):
   - `var lastMessageTime = await _cache.GetAsync($"chat:cooldown:{senderId}")`
   - `if (DateTime.UtcNow - lastMessageTime < TimeSpan.FromSeconds(1))` → Exception("메시지를 너무 빠르게 전송하고 있습니다.")
   - 이유: Discord/Slack 표준 패턴 (1초 쿨다운)
3. **연속 메시지 체크** (선택):
   - 최근 5개 메시지가 동일 내용 → 10초 쿨다운 (스팸 방지)
4. **욕설 필터 (TODO)**:
   - 기본 욕설 목록 50개 → `***` 치환
   - 관리자는 필터 우회

**예외 처리**:
- `InvalidOperationException`: 비즈니스 규칙 위반 (클라이언트 에러)
- `Exception`: 서버 에러 (로그 + 일반 에러 응답)

**🎓 학습 포인트 (아키텍처 결정)**:
- **TODO(human)**: 쿨다운 저장소 선택 (Redis vs MemoryCache vs DB?)
  - Trade-off: Redis (확장성, 추가 인프라) vs MemoryCache (단순, 단일 서버)
- **TODO(human)**: 욕설 필터 구현 위치 (Domain Service vs Application Service?)
  - Domain: 불변 비즈니스 규칙, Application: 외부 API 연동 (Google Perspective API)

> 💡 **학습 가이드**: 계산식과 수치는 AI가 제안합니다. **저장소 선택**, **계층 분리**, **캐싱 전략**에 집중하세요.

**Requirements**: US-1 (메시지 검증), 게임 밸런스 (AI 제안 - 500자, 1초 쿨다운)

---

#### 채팅방 권한 체크

**입력**:
- `characterId`: Guid (접근 요청자)
- `roomId`: Guid (채팅방 ID)

**출력**:
- bool (true: 접근 가능, false: 권한 없음)

**프로세스 (의사코드)**:
```
1. room = await _unitOfWork.ChatRooms.GetByIdAsync(roomId)
2. switch (room.Type):
   case Global:
     return true  // 모든 인증 사용자 접근 가능

   case Guild:
     character = await _unitOfWork.Characters.GetByIdAsync(characterId)
     return character.GuildId == room.GuildId  // 같은 길드만

   case Whisper:
     // TODO: Whisper 참여자 테이블 (미래 확장)
     return false

   default:
     return false
```

**예외 처리**:
- `NotFoundException`: 채팅방 미존재 (404)
- `NotFoundException`: 캐릭터 미존재 (401)

**🎓 학습 포인트 (아키텍처 결정)**:
- **TODO(human)**: Whisper 참여자 저장 (별도 테이블 vs ChatRoom.ParticipantIds JSONB?)
  - Trade-off: 정규화 (조인 쿼리) vs 역정규화 (JSONB, 쿼리 단순)
- **TODO(human)**: 권한 체크 캐싱 (Redis vs 매번 DB 조회?)
  - 고려: 길드 변경 빈도 vs 조회 빈도

> 💡 **학습 가이드**: Enum 기반 비즈니스 로직 분기, Authorization 패턴 학습에 집중하세요.

**Requirements**: US-3 (채팅방 타입별 접근), AC-3 (인증)

---

#### Cursor 기반 페이징 (EF Core)

**입력**:
- `roomId`: Guid
- `beforeId`: Guid? (nullable)
- `take`: int (10-100)

**출력**:
- `List<ChatMessageDto>` (최대 `take`개)

**프로세스 (EF Core 쿼리)**:
```
1. query = _context.ChatMessages
     .Include(m => m.Sender)  // N+1 방지
     .AsNoTracking()  // 읽기 전용
     .Where(m => m.RoomId == roomId)

2. if (beforeId != null):
     beforeMessage = await _context.ChatMessages.FindAsync(beforeId)
     query = query.Where(m => m.CreatedAt < beforeMessage.CreatedAt)

3. messages = await query
     .OrderByDescending(m => m.CreatedAt)  // 최신순
     .Take(take)
     .ToListAsync()

4. return messages.Select(m => MapToDto(m))
```

**인덱스 활용**:
- `IX_ChatMessages_RoomId_CreatedAt` 복합 인덱스 사용
- Execution Plan: Index Seek (Range Scan)

**🎓 학습 포인트 (성능 최적화)**:
- **TODO(human)**: Include vs Select (Projection) 성능 비교
  - Include: Navigation Property 자동 로드 (간편)
  - Select: 필요한 필드만 조회 (성능 우수)
- **TODO(human)**: AsNoTracking 사용 시기 (읽기 전용 vs 수정 가능)

> 💡 **학습 가이드**: EF Core 쿼리 최적화 패턴, 인덱스 활용 전략 학습에 집중하세요.

**Requirements**: US-2 (히스토리 조회), AC-2 (Cursor 페이징), AC-4 (N+1 방지)

---

## 🎯 Service Layer Design

> ⚠️ **메서드 시그니처와 책임만 정의** - 구현 코드는 Implementation 단계에서 작성

### ChatService (`IChatService`)

**책임**: 채팅 비즈니스 로직 (검증, 저장, 권한 체크)

**Dependencies**:
- `IUnitOfWork` - Repository 접근
- `ILogger<ChatService>` - 로깅
- `IMemoryCache` 또는 `IDistributedCache` - 쿨다운 관리

---

#### `SendMessageAsync(roomId, senderId, content)`

**시그니처**:
- 입력: `Guid roomId`, `Guid senderId`, `string content`, `CancellationToken cancellationToken`
- 반환: `Task<ChatMessageDto>` (저장된 메시지 DTO)

**프로세스 흐름**:
1. **Input validation**:
   - content.Length ≤ 500 (ValidationException)
   - roomId, senderId 존재 확인 (NotFoundException)
2. **Business logic**:
   - 쿨다운 체크 (1초)
   - 욕설 필터 적용 (선택)
3. **Domain logic**:
   - ChatMessage Entity 생성
4. **Repository 저장**:
   - `_unitOfWork.ChatMessages.AddAsync(message)`
   - `await _unitOfWork.SaveChangesAsync(cancellationToken)`
5. **Return result**:
   - Include Sender: `await _unitOfWork.ChatMessages.GetByIdWithSenderAsync(message.Id)`
   - MapToDto(message) → ChatMessageDto

**에러 조건**:
- `ValidationException`: 길이 초과, 빈 메시지
- `InvalidOperationException`: 쿨다운 위반
- `NotFoundException`: 채팅방/캐릭터 미존재
- `DbUpdateException`: DB 저장 실패

**🎓 학습 포인트 (트랜잭션 관리)**:
- **TODO(human)**: 트랜잭션 경계 설정 (SaveChangesAsync 호출 시점)
  - Service에서 트랜잭션? Repository에서 자동?
- **TODO(human)**: 낙관적 동시성 제어 (RowVersion) 필요 여부
  - 채팅은 동시성 충돌 낮음, 필요 없을 수도

**Requirements**: US-1 (메시지 전송), AC-1 (브로드캐스팅)

---

#### `GetMessagesAsync(roomId, characterId, beforeId?, take)`

**시그니처**:
- 입력: `Guid roomId`, `Guid characterId`, `Guid? beforeId`, `int take`, `CancellationToken cancellationToken`
- 반환: `Task<List<ChatMessageDto>>` (최대 `take`개)

**프로세스 흐름**:
1. **권한 체크**:
   - `await CanAccessRoomAsync(characterId, roomId)` (false → 403 Forbidden)
2. **Repository 조회**:
   - `await _unitOfWork.ChatMessages.GetByRoomIdAsync(roomId, beforeId, take, cancellationToken)`
   - Cursor 페이징, Include(m => m.Sender)
3. **Return result**:
   - messages.Select(m => MapToDto(m))

**에러 조건**:
- `NotFoundException`: 채팅방 미존재
- `ForbiddenException`: 권한 없음

**Requirements**: US-2 (히스토리 조회), AC-2 (Cursor 페이징)

---

#### `CanAccessRoomAsync(characterId, roomId)`

**시그니처**:
- 입력: `Guid characterId`, `Guid roomId`, `CancellationToken cancellationToken`
- 반환: `Task<bool>` (true: 접근 가능)

**프로세스 흐름**:
1. room = await _unitOfWork.ChatRooms.GetByIdAsync(roomId)
2. switch (room.Type):
   - Global: return true
   - Guild: character.GuildId == room.GuildId
   - Whisper: (TODO: 참여자 체크)

**에러 조건**:
- `NotFoundException`: 채팅방/캐릭터 미존재

**🎓 학습 포인트 (권한 체크 패턴)**:
- **TODO(human)**: Authorization Policy 활용 (ASP.NET Core Policy-based Auth)
  - 장점: 선언적, 재사용 가능
  - 단점: 학습 곡선
- **TODO(human)**: 권한 체크 결과 캐싱 (Redis vs 매번 조회)

**Requirements**: US-3 (채팅방 타입별 접근)

---

#### `GetAccessibleRoomsAsync(characterId)`

**시그니처**:
- 입력: `Guid characterId`, `CancellationToken cancellationToken`
- 반환: `Task<List<ChatRoomDto>>` (접근 가능한 채팅방 목록)

**프로세스 흐름**:
1. character = await _unitOfWork.Characters.GetByIdAsync(characterId)
2. rooms = await _unitOfWork.ChatRooms.GetAccessibleRoomsAsync(characterId, character.GuildId)
   - Global 모두 + Guild (같은 GuildId) + Whisper (참여자)
3. 각 방의 lastMessage 조회 (선택)
4. Return ChatRoomDto 리스트

**Requirements**: US-3 (채팅방 타입별 접근)

---

## 🧪 Testing Strategy

### Unit Tests

**Domain Layer**:
- `ChatMessage` Entity: 필드 제약, Navigation Property
- `RoomType` Enum: 유효한 값 범위

**Application Layer (ChatService)**:
- `SendMessageAsync`:
  - ✅ 정상 메시지 전송 → ChatMessageDto 반환
  - ✅ 길이 초과 (501자) → ValidationException
  - ✅ 쿨다운 위반 → InvalidOperationException
  - ✅ 존재하지 않는 roomId → NotFoundException
- `GetMessagesAsync`:
  - ✅ beforeId 없음 → 최신 50개
  - ✅ beforeId 제공 → 이전 50개
  - ✅ 권한 없음 → ForbiddenException
- `CanAccessRoomAsync`:
  - ✅ Global 채팅방 → true
  - ✅ Guild 채팅방 (같은 길드) → true
  - ✅ Guild 채팅방 (다른 길드) → false

**Mock 대상**:
- `IUnitOfWork`: Repository 동작 시뮬레이션
- `IMemoryCache`: 쿨다운 데이터 시뮬레이션

**핵심 시나리오**:
- N+1 쿼리 방지 검증 (Include vs 여러 번 쿼리)
- Cursor 페이징 정확성 (beforeId 경계 조건)

---

### Integration Tests

**API Layer (ChatHub)**:
- `SendMessage`:
  - ✅ 정상 메시지 → Clients.Group() 호출 확인
  - ✅ 에러 발생 → Clients.Caller.SendAsync("Error") 확인
- `JoinRoom`:
  - ✅ 권한 있음 → Groups.AddToGroupAsync() 확인
  - ✅ 권한 없음 → Error 이벤트

**REST API (ChatController)**:
- `GET /api/chat/rooms/{roomId}/messages`:
  - ✅ 정상 조회 → 200 OK, 메시지 리스트
  - ✅ 권한 없음 → 403 Forbidden
  - ✅ 존재하지 않는 roomId → 404 Not Found

**Database Integration**:
- `ChatMessageRepository.GetByRoomIdAsync()`:
  - ✅ Cursor 페이징 (beforeId 제공)
  - ✅ 인덱스 사용 확인 (Execution Plan)
  - ✅ Include(m => m.Sender) → 단일 쿼리 (N+1 방지)

---

### Test Coverage Goals
- Domain Services: N/A (Domain Service 없음)
- Application Services (ChatService): 85%+
- Controllers & Hubs: 70%+
- Repositories: 80%+

---

## ⚠️ Error Handling

### Exception Types

**Domain/Application Layer**:
- `NotFoundException`: 채팅방/캐릭터 미존재
  - HTTP 404 Not Found
- `ValidationException`: 메시지 길이 초과, 잘못된 파라미터
  - HTTP 400 Bad Request
- `InvalidOperationException`: 쿨다운 위반, 권한 없음
  - HTTP 400 Bad Request (쿨다운) 또는 403 Forbidden (권한)
- `ForbiddenException` (커스텀): 채팅방 접근 권한 없음
  - HTTP 403 Forbidden

**SignalR Hub**:
- Hub 메서드 내 try-catch:
  ```csharp
  try {
      var dto = await _chatService.SendMessageAsync(...);
      await Clients.Group(roomId).SendAsync("ReceiveMessage", dto);
  }
  catch (InvalidOperationException ex) {
      await Clients.Caller.SendAsync("Error", new ErrorDto {
          Code = "INVALID_MESSAGE",
          Message = ex.Message
      });
  }
  catch (Exception ex) {
      _logger.LogError(ex, "SendMessage unexpected error");
      await Clients.Caller.SendAsync("Error", new ErrorDto {
          Code = "SERVER_ERROR",
          Message = "메시지 전송에 실패했습니다."
      });
  }
  ```

---

### Error Response Format

**REST API**:
```json
{
  "message": "채팅방을 찾을 수 없습니다.",
  "statusCode": 404,
  "details": {
    "roomId": "12345678-1234-1234-1234-123456789abc"
  }
}
```

**SignalR Error Event**:
```json
{
  "code": "COOLDOWN_ACTIVE",
  "message": "메시지를 너무 빠르게 전송하고 있습니다. 1초 후 다시 시도하세요."
}
```

**Error Code 목록**:
- `INVALID_MESSAGE`: 메시지 길이 초과 (500자 초과)
- `COOLDOWN_ACTIVE`: 쿨다운 위반 (1초 이내 재전송)
- `FORBIDDEN`: 채팅방 접근 권한 없음
- `INVALID_ROOM_ID`: 채팅방 ID 형식 오류
- `SERVER_ERROR`: 서버 내부 오류

**Requirements**: AC-5 (에러 처리)

---

## 🔐 Security Considerations

### Authentication

**REST API**:
- `[Authorize]` 속성 적용
- JWT Bearer Token (Header: `Authorization: Bearer {token}`)

**SignalR Hub**:
- `[Authorize]` 속성 적용
- JWT Query String 방식: `?access_token={token}`
- `Program.cs` JWT 이벤트 추가:
  ```csharp
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
  ```

**Requirements**: AC-3 (JWT 인증)

---

### Authorization

**채팅방 접근 권한**:
- Global: 모든 인증 사용자
- Guild: 같은 GuildId 소유 캐릭터만
- Whisper: 참여자만 (TODO)

**권한 체크 시점**:
- `JoinRoom()`: Group 가입 전
- `GetMessagesAsync()`: 히스토리 조회 전

**🎓 학습 포인트 (보안 설계)**:
- **TODO(human)**: Policy-based Authorization 사용 여부
  - `[Authorize(Policy = "GuildMemberOnly")]` vs 수동 체크
- **TODO(human)**: Rate Limiting (메시지 전송 속도 제한)
  - AspNetCoreRateLimit 라이브러리 vs 자체 구현

---

### Data Validation

**Input Validation**:
- `content`: 1-500자, NULL 불가, HTML 태그 제거 (XSS 방지)
- `roomId`, `senderId`: GUID 형식 검증
- `take`: 10-100 범위

**SQL Injection Prevention**:
- EF Core 파라미터화 자동 적용
- Raw SQL 사용 금지

**XSS Prevention**:
- Unity 클라이언트는 TextMeshPro (HTML 렌더링 안 함)
- 서버는 `<`, `>` 치환 권장 (선택)

---

## 📊 Performance Considerations

### Database Indexes

**중요도 1순위**:
- `IX_ChatMessages_RoomId_CreatedAt` (복합 인덱스)
  - 이유: 모든 히스토리 조회 쿼리에 사용
  - Covering Index 고려 (Content 포함 시)

**중요도 2순위**:
- `IX_ChatRooms_Type`: 채팅방 목록 필터링
- `IX_ChatRooms_GuildId`: 길드 채팅방 조회

**🎓 학습 포인트 (인덱스 전략)**:
- **TODO(human)**: Covering Index vs 일반 Index
  - Covering: SELECT 컬럼 모두 인덱스에 포함 (Lookup 불필요)
  - 일반: Key 컬럼만 인덱스 (인덱스 크기 작음)
- **TODO(human)**: 인덱스 유지보수 비용
  - INSERT 성능 저하 vs SELECT 성능 향상

**Requirements**: AC-4 (N+1 방지)

---

### Caching

**쿨다운 캐싱**:
- Key: `chat:cooldown:{senderId}`
- Value: DateTime (마지막 메시지 시간)
- TTL: 5초
- 저장소: MemoryCache (단일 서버) 또는 Redis (Scale-out)

**권한 체크 캐싱** (선택):
- Key: `chat:access:{characterId}:{roomId}`
- Value: bool (접근 가능 여부)
- TTL: 60초
- 갱신 조건: 길드 가입/탈퇴 시

**🎓 학습 포인트 (캐싱 전략)**:
- **TODO(human)**: Cache Invalidation 전략
  - TTL 기반 vs Event 기반 (길드 탈퇴 시 즉시 삭제)
- **TODO(human)**: Cache Stampede 방지
  - Lock 사용 vs Stale-While-Revalidate 패턴

---

### Pagination

**Cursor 기반 페이징**:
- Offset 페이징 대비 장점:
  - 안정적인 결과 (중간 삽입/삭제 영향 없음)
  - 인덱스 활용 (WHERE CreatedAt < ?)
- 단점:
  - 임의 페이지 접근 불가 (채팅은 문제 없음)

---

### N+1 Query Prevention

**EF Core Include**:
- `Include(m => m.Sender)` - 메시지 조회 시 발신자 정보 함께 로드
- 단일 LEFT JOIN 쿼리 실행

**AsNoTracking()**:
- 읽기 전용 쿼리 (히스토리 조회)
- Change Tracking 오버헤드 제거

**🎓 학습 포인트 (쿼리 최적화)**:
- **TODO(human)**: Include vs Select (Projection)
  - Include: `Select(m => new ChatMessageDto { Sender = new CharacterSummaryDto { ... } })`
  - Projection: 필요한 필드만 SELECT (성능 우수)
- **TODO(human)**: Lazy Loading 사용 금지 이유
  - N+1 쿼리 발생, 성능 저하

**Requirements**: AC-4 (N+1 방지)

---

## 🔄 Migration Plan

> ⚠️ **마이그레이션 요구사항만 명시** - 실제 SQL은 Implementation 단계에서 작성
> 위치: `IdleRPG.Infrastructure/migration.sql` (Idempotent 패턴)

### Database Migration 요구사항

**신규 테이블**:
1. `ChatRooms`:
   - 목적: 채팅방 정보 저장 (Global, Guild, Whisper)
   - 필드: Id (GUID PK), Type (int), Name (varchar 100), GuildId (GUID nullable FK), CreatedAt (timestamp)
   - 관계: Guilds (N:1, nullable), ChatMessages (1:N)
   - 인덱스: Type, GuildId

2. `ChatMessages`:
   - 목적: 채팅 메시지 저장 및 히스토리
   - 필드: Id (GUID PK), RoomId (GUID FK), SenderId (GUID FK), Content (varchar 1000), CreatedAt (timestamp)
   - 관계: ChatRooms (N:1), Characters (N:1)
   - 인덱스: **복합 인덱스 (RoomId, CreatedAt DESC)** - 가장 중요

**수정 테이블**:
- `Characters`: (선택)
  - 추가 필드: `IsOnline` (bool, 기본 false), `LastSeenAt` (timestamp nullable)
  - 목적: 온라인 상태 추적 (ChatHub Connection Lifecycle)

**인덱스 추가** (우선순위 순):
1. `IX_ChatMessages_RoomId_CreatedAt` (복합, DESC)
   - 이유: Cursor 페이징, 모든 히스토리 조회에 사용
2. `IX_ChatRooms_Type`
   - 이유: 채팅방 목록 필터링 (Global/Guild/Whisper)
3. `IX_ChatRooms_GuildId`
   - 이유: 길드 채팅방 조회

**데이터 마이그레이션**:
- 초기 데이터: Global 채팅방 1개 자동 생성
  - `INSERT INTO ChatRooms (Id, Type, Name, CreatedAt) VALUES (uuid, 1, '전체 채팅', NOW())`
- 기존 데이터 영향: 없음 (신규 기능)

**Idempotent 패턴**:
- `CREATE TABLE IF NOT EXISTS ...`
- `CREATE INDEX IF NOT EXISTS ...`
- 트랜잭션: BEGIN; ... COMMIT;

**백업 권장 여부**: No (신규 테이블, 기존 데이터 영향 없음)

**🎓 학습 포인트 (데이터베이스 설계)**:
- **TODO(human)**: Cascade Delete vs Application 처리
  - ON DELETE CASCADE: DB가 자동 삭제 (빠름)
  - Application: 커스텀 로직 (익명화, 로그)
- **TODO(human)**: GUID vs Serial (Auto-increment)
  - GUID: 분산 환경, 클라이언트 생성 가능
  - Serial: 성능 우수, 순차적
- **TODO(human)**: VARCHAR(1000) vs TEXT
  - VARCHAR: 길이 제한, 인덱스 가능
  - TEXT: 무제한, 인덱스 제약
- **TODO(human)**: CreatedAt 기본값 (UTC_NOW vs Application 설정)
  - DB 기본값: 일관성
  - Application: 테스트 용이성

> 💡 **학습 가이드**: SQL 문법은 AI가 작성합니다. **Cascade 규칙**, **타입 선택**, **인덱스 전략** 같은 설계 결정에 집중하세요.

**Requirements**: Data Model, Entity Relationships

---

### Data Seeding 계획

**Seeder 필요 여부**: Yes

**초기 데이터**:
1. **Global 채팅방**:
   - Type: Global (1)
   - Name: "전체 채팅"
   - GuildId: NULL
   - 목적: 모든 사용자가 사용할 기본 채팅방

**Seeder 클래스**:
- `ChatRoomSeeder.cs` (Infrastructure Layer)
- `ApplicationDbContextSeed.cs`에 통합

**데이터 출처**: 하드코딩 (고정 GUID 사용)

> 💡 **구현 참고**: Seeder 코드는 Implementation 단계에서 작성

**Requirements**: US-1 (전체 채팅)

---

## 📝 Decision Log

> ⚠️ **중요**: L 사이즈 기능은 이 섹션 **필수**. 중요한 아키텍처 결정을 ADR과 연결.

| ID | Decision | ADR Link | Spike Link | Status |
|----|----------|----------|------------|--------|
| D1 | SignalR 도입 (vs Polling, WebSocket Raw) | [ADR-0002](../../../docs/adr/ADR-0002-signalr-adoption.md) | Skipped (프로토타입 검증) | Accepted |
| D2 | Cursor 페이징 채택 (vs Offset) | - | - | Accepted |
| D3 | ChatMessage.Content VARCHAR(1000) (Application 500자 검증) | - | - | Accepted |
| D4 | 복합 인덱스 (RoomId, CreatedAt DESC) | - | - | Accepted |
| D5 | JWT Query String 방식 (SignalR) | - | - | Accepted |
| D6 | MemoryCache 쿨다운 저장 (Redis는 Scale-out 시) | - | - | Accepted |
| D7 | SignalR Error 이벤트 (vs Exception 던지기) | - | - | Accepted |

**가이드**:
- **간단한 결정**: ADR 없이 테이블에 1줄로 기록
- **복잡한 결정**: ADR 작성 후 링크
- **Spike 결과**: Spike 링크 포함

**D1 상세**:
- Context: 실시간 채팅 구현 필요
- Alternatives: Polling (실시간성 부족), WebSocket Raw (복잡도 높음), gRPC (Unity 호환성 불확실)
- Decision: SignalR (ASP.NET Core 내장, Unity 공식 지원, 학습 목적 적합)
- Consequences: ✅ 빠른 프로토타이핑, ⚠️ Scale-out 시 Redis Backplane 필요

**D2 상세**:
- Context: 채팅 히스토리 페이징 전략
- Alternatives: Offset 페이징 (중간 삽입/삭제 시 결과 불안정)
- Decision: Cursor 기반 (beforeId, CreatedAt 기준)
- Consequences: ✅ 안정적 결과, ✅ 인덱스 활용, ⚠️ 임의 페이지 접근 불가 (채팅은 문제 없음)

**D7 상세**:
- Context: SignalR Hub 메서드 에러 처리
- Alternatives: Exception 던지기 (클라이언트 연결 끊김 위험)
- Decision: Clients.Caller.SendAsync("Error", ErrorDto)
- Consequences: ✅ 연결 유지, ✅ 클라이언트 에러 핸들링 가능

---

## 📱 Unity Client Integration

### Unity Documentation 필요 항목

**1. API 명세** (`docs/unity/API_SPEC_FOR_UNITY.md`):
- REST API 추가:
  - `GET /api/chat/rooms/{roomId}/messages` (Cursor 페이징 포함)
  - `GET /api/chat/rooms`
- SignalR Hub 명세 추가:
  - Hub URL: `/chat`
  - Hub Methods: `JoinRoom`, `LeaveRoom`, `SendMessage`, `Typing`
  - Client Events: `ReceiveMessage`, `Error`, `UserJoined`, `UserLeft`, `UserTyping`
  - JWT 인증: Query String 방식 (`?access_token={token}`)

**2. DTO 클래스** (`docs/unity/DTOs/ChatDtos.cs`):
```csharp
// Unity용 C# DTO 클래스 (Newtonsoft.Json 직렬화)
[Serializable]
public class ChatMessageDto {
    public Guid id;
    public Guid roomId;
    public CharacterSummaryDto sender;
    public string content;
    public DateTime createdAt;
}

[Serializable]
public class CharacterSummaryDto {
    public Guid id;
    public string name;
    public int level;
}

[Serializable]
public class ErrorDto {
    public string code;
    public string message;
}
```

**3. SignalR 연동 가이드** (`docs/unity/signalr/SIGNALR_INTEGRATION_GUIDE.md`):
- SignalR Client 패키지 설치: `Microsoft.AspNetCore.SignalR.Client` 6.0+
- Connection 생성 예시:
  ```csharp
  var connection = new HubConnectionBuilder()
      .WithUrl("http://localhost:5172/chat?access_token=" + jwtToken)
      .Build();

  connection.On<ChatMessageDto>("ReceiveMessage", (message) => {
      Debug.Log($"[{message.sender.name}] {message.content}");
  });

  await connection.StartAsync();
  await connection.InvokeAsync("JoinRoom", "room-guid");
  await connection.InvokeAsync("SendMessage", "room-guid", "Hello!");
  ```

**4. 에러 처리 가이드** (`docs/unity/signalr/ERROR_HANDLING.md`):
- Error 이벤트 구독:
  ```csharp
  connection.On<ErrorDto>("Error", (error) => {
      switch (error.code) {
          case "INVALID_MESSAGE": ShowToast(error.message); break;
          case "COOLDOWN_ACTIVE": ShowCooldownTimer(); break;
          case "FORBIDDEN": RedirectToLobby(); break;
          case "SERVER_ERROR": ShowErrorDialog(error.message); break;
      }
  });
  ```

**참고**: `docs/unity/UNITY_DOCUMENTATION_GUIDE.md` (v1.7+ 구조)

**Requirements**: Unity 클라이언트 통합 (US-1, US-2, US-3)

---

## ✅ Approval

- [x] Design 리뷰 완료
- [x] 모든 Requirements 항목 커버 확인
  - [x] US-1: 메시지 전송 및 수신 (SendMessage, ReceiveMessage)
  - [x] US-2: 채팅 히스토리 조회 (GetMessagesAsync, Cursor 페이징)
  - [x] US-3: 채팅방 타입별 접근 (CanAccessRoomAsync)
  - [x] AC-1: 메시지 저장 및 브로드캐스팅
  - [x] AC-2: Cursor 기반 페이징
  - [x] AC-3: JWT 인증 (REST + SignalR Query String)
  - [x] AC-4: N+1 쿼리 방지 (Include, AsNoTracking)
  - [x] AC-5: 에러 처리 표준화 (ErrorDto)
- [x] TODO(human) 아키텍처 학습 포인트 확인 완료
  - [x] 쿨다운 저장소 선택 (Redis vs MemoryCache)
  - [x] 욕설 필터 구현 위치 (Domain vs Application)
  - [x] Whisper 참여자 저장 (별도 테이블 vs JSONB)
  - [x] 권한 체크 캐싱 전략
  - [x] Include vs Select (Projection) 성능
  - [x] Cascade Delete vs Application 처리
  - [x] GUID vs Serial
  - [x] 인덱스 전략 (Covering Index vs 일반)
- [ ] Self-Review Checklist 10개 항목 통과 (9/10 이상)
- [x] 게임 밸런스 AI 제안값 확인
  - [x] 메시지 길이: 500자 (DB: 1000자)
  - [x] 쿨다운: 1초
  - [x] 히스토리: 기본 50개, 최대 100개
- [ ] ADR-0002 검토 완료 (SignalR 도입 결정)
- [ ] Tasks 단계로 진행 승인

---

**작성일**: 2025-10-29
**작성자**: KTS
**상태**: Draft
**Requirements 추적성**: [requirements.md](./requirements.md) - US-1 (메시지 전송), US-2 (히스토리 조회), US-3 (채팅방 타입), AC-1~AC-5 (기술 요구사항)
**ADR**: [ADR-0002 SignalR 도입](../../../docs/adr/ADR-0002-signalr-adoption.md)
