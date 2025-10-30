# Tasks: Realtime Chat System

> 이 문서는 실시간 채팅 시스템의 구현 작업 목록입니다.
>
> **작성 가이드**:
> - Design 문서의 모든 컴포넌트를 구현 가능한 작업으로 분해
> - 각 작업은 독립적으로 완료 및 테스트 가능해야 함
> - 순서는 의존성을 고려하여 정렬 (Domain → Infrastructure → Application → API)

---

## 📊 Progress Overview

**전체 진행률**: 16/31 (52%)

| Milestone | 작업 수 | 완료 | 진행률 |
|-----------|---------|------|--------|
| Domain Layer | 6 | 6 | 100% |
| Infrastructure Layer | 7 | 7 | 100% |
| Application Layer | 4 | 3 | 75% |
| API Layer | 5 | 0 | 0% |
| Database | 3 | 0 | 0% |
| Testing & Documentation | 6 | 0 | 0% |

**예상 총 소요 시간**: ~24.75시간

---

## 🏗️ Milestone 1: Domain Layer

### 1.1 Create RoomType Enum ⏱️ 15분 ✅
- [x] Create `IdleRPG.Domain/Enums/RoomType.cs`
- [x] Define enum values:
  - Global = 1 (전체 채팅)
  - Guild = 2 (길드 채팅)
  - Whisper = 3 (1:1 귓속말)
- [x] Add XML documentation comments

**Requirements**: US-3 (채팅방 타입)
**Design Reference**: [Domain Layer - Enums - RoomType]

---

### 1.2 Create ChatRoom Entity ⏱️ 45분 ✅
- [x] Create `IdleRPG.Domain/Entities/ChatRoom.cs`
- [x] Add properties:
  - Id (Guid, PK)
  - Type (RoomType enum)
  - Name (string, 2-100자)
  - GuildId? (Guid nullable, FK)
  - CreatedAt (DateTime)
- [x] Add navigation properties:
  - Guild (nullable)
  - Messages (ICollection<ChatMessage>)
  - Participants (ICollection<ChatRoomParticipant>)
- [x] Implement BaseEntity inheritance

**Requirements**: US-3 (채팅방 정보)
**Design Reference**: [Data Model - ChatRooms]

---

### 1.3 Create ChatRoomParticipant Entity ⏱️ 30분 ✅
- [x] Create `IdleRPG.Domain/Entities/ChatRoomParticipant.cs`
- [x] Add properties:
  - Id (Guid, PK)
  - RoomId (Guid, FK)
  - CharacterId (Guid, FK)
  - JoinedAt (DateTime)
- [x] Add navigation properties:
  - Room (ChatRoom)
  - Character (Character)

**Requirements**: US-3 (Whisper 참여자 관리)
**Design Reference**: [Data Model - ChatRoomParticipants]

---

### 1.4 Create ChatMessage Entity ⏱️ 45분 ✅
- [x] Create `IdleRPG.Domain/Entities/ChatMessage.cs`
- [x] Add properties:
  - Id (Guid, PK)
  - RoomId (Guid, FK)
  - SenderId (Guid, FK)
  - Content (string, 1-1000자)
  - CreatedAt (DateTime)
- [x] Add navigation properties:
  - Room (ChatRoom)
  - Sender (Character)
- [x] Implement BaseEntity inheritance

**Requirements**: US-1 (메시지 저장)
**Design Reference**: [Data Model - ChatMessages]

---

### 1.5 Create IChatRoomRepository Interface ⏱️ 30분 ✅
- [x] Create `IdleRPG.Domain/Repositories/IChatRoomRepository.cs`
- [x] Define methods:
  - `Task<ChatRoom?> GetByIdAsync(Guid id, CancellationToken cancellationToken)`
  - `Task<List<ChatRoom>> GetAccessibleRoomsAsync(Guid characterId, Guid? guildId, CancellationToken cancellationToken)`
  - `Task AddAsync(ChatRoom room, CancellationToken cancellationToken)`

**Requirements**: US-3 (채팅방 조회)
**Design Reference**: [Domain Layer - Repository Interfaces]

---

### 1.6 Create IChatMessageRepository Interface ⏱️ 45분 ✅
- [x] Create `IdleRPG.Domain/Repositories/IChatMessageRepository.cs`
- [x] Define methods:
  - `Task<ChatMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken)` (Entity 반환, Eager Loading)
  - `Task<List<ChatMessage>> GetByRoomIdAsync(Guid roomId, Guid? beforeId, int take, CancellationToken cancellationToken)` (Cursor 페이징, Entity 반환)
  - `Task AddAsync(ChatMessage message, CancellationToken cancellationToken)`
- [x] Create `IdleRPG.Application/DTOs/Chat/ChatMessageDto.cs`
- [x] Create `IdleRPG.Application/DTOs/Chat/CharacterSummaryDto.cs`
- [x] 리팩토링: Clean Architecture 준수 (Entity 반환으로 변경)

**Requirements**: US-2 (히스토리 조회), AC-2 (Cursor 페이징)
**Design Reference**: [Infrastructure Layer - Repositories]

---

## 🔧 Milestone 2: Infrastructure Layer

### 2.1 Create ChatRoomConfiguration ⏱️ 45분 ✅
- [x] Create `IdleRPG.Infrastructure/Configurations/ChatRoomConfiguration.cs`
- [x] Implement IEntityTypeConfiguration<ChatRoom>
- [x] Configure:
  - Table name: "ChatRooms"
  - Primary key: Id
  - Properties: Name (MaxLength 100, Required), Type (Enum → int)
  - Relationships:
    - HasMany(Messages).WithOne(Room).OnDelete(Restrict)
    - HasMany(Participants).WithOne(Room).OnDelete(Cascade)
    - HasOne(Guild).WithMany().OnDelete(Restrict).IsRequired(false)
  - Indexes:
    - IX_ChatRooms_Type
    - IX_ChatRooms_GuildId

**Requirements**: US-3 (데이터 모델)
**Design Reference**: [Data Model - EF Core Configuration]

---

### 2.2 Create ChatRoomParticipantConfiguration ⏱️ 30분 ✅
- [x] Create `IdleRPG.Infrastructure/Configurations/ChatRoomParticipantConfiguration.cs`
- [x] Implement IEntityTypeConfiguration<ChatRoomParticipant>
- [x] Configure:
  - Table name: "ChatRoomParticipants"
  - Primary key: Id
  - Relationships:
    - HasOne(Room).WithMany(Participants).OnDelete(Cascade)
    - HasOne(Character).WithMany().OnDelete(Restrict)
  - Indexes:
    - IX_ChatRoomParticipants_RoomId
    - IX_ChatRoomParticipants_CharacterId
    - Unique Index: (RoomId, CharacterId)

**Requirements**: US-3 (Whisper 참여자 중복 방지)
**Design Reference**: [Data Model - EF Core Configuration]

---

### 2.3 Create ChatMessageConfiguration ⏱️ 30분 ✅
- [x] Create `IdleRPG.Infrastructure/Configurations/ChatMessageConfiguration.cs`
- [x] Implement IEntityTypeConfiguration<ChatMessage>
- [x] Configure:
  - Table name: "ChatMessages"
  - Primary key: Id
  - Properties: Content (MaxLength 1000, Required)
  - Relationships:
    - HasOne(Room).WithMany(Messages).OnDelete(Restrict)
    - HasOne(Sender).WithMany().OnDelete(Restrict)
  - Indexes:
    - **복합 인덱스**: IX_ChatMessages_RoomId_CreatedAt (RoomId ASC, CreatedAt DESC)

**Requirements**: US-1 (메시지 저장), AC-4 (인덱스 최적화)
**Design Reference**: [Data Model - EF Core Configuration]

---

### 2.4 Create ChatRoomRepository ⏱️ 1시간 ✅
- [x] Create `IdleRPG.Infrastructure/Repositories/ChatRoomRepository.cs`
- [x] Inject ApplicationDbContext
- [x] Implement IChatRoomRepository:
  - `GetByIdAsync`: AsNoTracking, Include(Guild) if needed
  - `GetAccessibleRoomsAsync`:
    - Global: 모든 Global 타입 방
    - Guild: 같은 GuildId를 가진 Guild 타입 방
    - Whisper: ChatRoomParticipants에서 characterId가 참여한 Whisper 방
  - `AddAsync`: DbContext.ChatRooms.AddAsync()

**Requirements**: US-3 (채팅방 접근)
**Design Reference**: [Infrastructure Layer - Repositories]

---

### 2.5 Create ChatMessageRepository ⏱️ 1.5시간 ✅
- [x] Create `IdleRPG.Infrastructure/Repositories/ChatMessageRepository.cs`
- [x] Inject ApplicationDbContext
- [x] Implement IChatMessageRepository:
  - `GetByRoomIdAsync`: **Cursor 페이징** (beforeId 기준)
    - AsNoTracking()
    - Where(RoomId == roomId)
    - if (beforeId != null): Where(CreatedAt < beforeMessage.CreatedAt)
    - OrderByDescending(CreatedAt)
    - Take(take)
    - **Include(Sender)**: Eager Loading으로 N+1 방지
  - `GetByIdAsync`: Include(Sender)로 Entity 반환
  - `AddAsync`: DbContext.ChatMessages.AddAsync()

**Requirements**: US-2 (히스토리 조회), AC-2 (Cursor 페이징), AC-4 (N+1 방지)
**Design Reference**: [Business Logic - Cursor 기반 페이징]

---

### 2.6 Extend IUnitOfWork Interface ⏱️ 15분 ✅
- [x] Open `IdleRPG.Application/Interfaces/IUnitOfWork.cs`
- [x] Add properties:
  - `IChatRoomRepository ChatRooms { get; }`
  - `IChatMessageRepository ChatMessages { get; }`

**Requirements**: All
**Design Reference**: [Infrastructure Layer - Unit of Work]

---

### 2.7 Extend UnitOfWork Implementation ⏱️ 30분 ✅
- [x] Open `IdleRPG.Infrastructure/UnitOfWork/UnitOfWork.cs`
- [x] Inject ChatRoomRepository, ChatMessageRepository
- [x] Implement properties:
  - `public IChatRoomRepository ChatRooms { get; }`
  - `public IChatMessageRepository ChatMessages { get; }`
- [x] Ensure repositories are initialized in constructor (Lazy 초기화)

**Requirements**: All
**Design Reference**: [Infrastructure Layer - Unit of Work]

---

## 📦 Milestone 3: Application Layer

### 3.1 Create Chat DTOs ⏱️ 1시간 ✅
- [x] Create `IdleRPG.Application/DTOs/Chat/ChatMessageDto.cs`:
  - Id (Guid)
  - RoomId (Guid)
  - Sender (CharacterSummaryDto - 중첩)
  - Content (string)
  - CreatedAt (DateTime)
  - Reactions? (List<ReactionDto> - 미래 확장, nullable)
  - IsEdited? (bool - 미래 확장, nullable)
  - ParentMessageId? (Guid - 답장 기능, nullable)
- [x] Create `IdleRPG.Application/DTOs/Chat/CharacterSummaryDto.cs`:
  - Id (Guid)
  - Name (string)
  - Level (int)
  - AvatarUrl? (string - 미래 확장, nullable)
- [x] Create `IdleRPG.Application/DTOs/Chat/ErrorDto.cs`:
  - Code (string - "INVALID_MESSAGE", "COOLDOWN_ACTIVE", etc.)
  - Message (string)
- [x] Create `IdleRPG.Application/DTOs/Chat/ChatRoomDto.cs`:
  - Id (Guid)
  - Type (string - "Global", "Guild", "Whisper")
  - Name (string)
  - LastMessage? (LastMessageDto - nullable)
- [x] Add XML documentation

**Requirements**: US-1 (메시지 DTO), US-3 (채팅방 DTO)
**Design Reference**: [DTO Design]

---

### 3.2 Create IChatService Interface ⏱️ 45분 ✅
- [x] Create `IdleRPG.Application/Services/IChatService.cs`
- [x] Define methods:
  - `Task<ChatMessageDto> SendMessageAsync(Guid roomId, Guid senderId, string content, CancellationToken cancellationToken)`
  - `Task<List<ChatMessageDto>> GetMessagesAsync(Guid roomId, Guid characterId, Guid? beforeId, int take, CancellationToken cancellationToken)`
  - `Task<bool> CanAccessRoomAsync(Guid characterId, Guid roomId, CancellationToken cancellationToken)`
  - `Task<List<ChatRoomDto>> GetAccessibleRoomsAsync(Guid characterId, CancellationToken cancellationToken)`
  - `Task<Guid> CreateWhisperRoomAsync(Guid characterAId, Guid characterBId, CancellationToken cancellationToken)`
- [x] Add XML documentation

**Requirements**: US-1, US-2, US-3
**Design Reference**: [Service Layer Design]

---

### 3.3 Create ChatService Implementation ⏱️ 3시간 ✅
- [x] Create `IdleRPG.Infrastructure/Services/ChatService.cs`
- [x] Inject dependencies:
  - IUnitOfWork
  - ILogger<ChatService>
  - IMemoryCache (쿨다운 관리)
- [x] Implement `SendMessageAsync`:
  - Input validation: content.Length ≤ 500
  - 쿨다운 체크 (1초): IMemoryCache.Get($"chat:cooldown:{senderId}")
  - ChatMessage Entity 생성
  - _unitOfWork.ChatMessages.AddAsync()
  - await _unitOfWork.SaveChangesAsync()
  - IMemoryCache.Set($"chat:cooldown:{senderId}", DateTime.UtcNow, TTL: 5초)
  - Return ChatMessageDto (GetDtoByIdAsync)
- [x] Implement `GetMessagesAsync`:
  - 권한 체크: await CanAccessRoomAsync()
  - Repository 조회: _unitOfWork.ChatMessages.GetByRoomIdAsync()
- [x] Implement `CanAccessRoomAsync`:
  - room = await _unitOfWork.ChatRooms.GetByIdAsync()
  - switch (room.Type):
    - Global: return true
    - Guild: character.GuildId == room.GuildId
    - Whisper: ChatRoomParticipants 체크
- [x] Implement `GetAccessibleRoomsAsync`:
  - character = await _unitOfWork.Characters.GetByIdAsync()
  - rooms = await _unitOfWork.ChatRooms.GetAccessibleRoomsAsync()
  - Return ChatRoomDto 리스트
- [x] Implement `CreateWhisperRoomAsync`:
  - 기존 방 확인 (A-B 또는 B-A 재사용)
  - 새 방 생성: ChatRoom (Type=Whisper)
  - 참여자 2명 추가: ChatRoomParticipant
  - SaveChangesAsync()
- [x] Add logging (Info, Warning, Error)
- [x] Error handling:
  - ArgumentException: 길이 초과, 빈 메시지
  - InvalidOperationException: 쿨다운 위반
  - KeyNotFoundException: 채팅방/캐릭터 미존재
  - UnauthorizedAccessException: 권한 없음

**Requirements**: US-1, US-2, US-3
**Design Reference**: [Service Layer Design - ChatService]

---

### 3.4 Register ChatService in DI Container ⏱️ 15분
- [ ] Open `IdleRPG.API/Program.cs`
- [ ] Register: `builder.Services.AddScoped<IChatService, ChatService>()`
- [ ] Register: `builder.Services.AddMemoryCache()` (쿨다운 캐싱)

**Requirements**: All
**Design Reference**: [Service Layer Design]

---

## 🌐 Milestone 4: API Layer

### 4.1 Create ChatController ⏱️ 1.5시간
- [ ] Create `IdleRPG.API/Controllers/ChatController.cs`
- [ ] Add [Authorize] attribute (JWT Bearer)
- [ ] Inject IChatService dependency
- [ ] Implement `GET /api/chat/rooms/{roomId}/messages`:
  - Path parameter: roomId (Guid)
  - Query parameters: beforeId? (Guid), take? (int, 기본 50, 최소 10, 최대 100)
  - JWT에서 CharacterId 추출
  - Call _chatService.GetMessagesAsync()
  - Return 200 OK: List<ChatMessageDto>
  - Error handling:
    - 400 Bad Request: roomId 형식 오류, take 범위 초과
    - 401 Unauthorized: JWT 토큰 없음/만료
    - 403 Forbidden: 권한 없음
    - 404 Not Found: 채팅방 미존재
- [ ] Implement `GET /api/chat/rooms`:
  - JWT에서 CharacterId 추출
  - Call _chatService.GetAccessibleRoomsAsync()
  - Return 200 OK: List<ChatRoomDto>
- [ ] Add Swagger XML comments

**Requirements**: US-2 (히스토리 조회), US-3 (채팅방 목록)
**Design Reference**: [API Design - REST API Endpoints]

---

### 4.2 Create ChatHub (SignalR Hub) ⏱️ 2시간
- [ ] Create `IdleRPG.API/Hubs/ChatHub.cs`
- [ ] Inherit from `Hub`
- [ ] Add [Authorize] attribute
- [ ] Inject dependencies:
  - IChatService
  - ILogger<ChatHub>
- [ ] Implement `JoinRoom(string roomId)`:
  - JWT에서 CharacterId 추출 (Context.User.FindFirst("characterId"))
  - 권한 체크: await _chatService.CanAccessRoomAsync()
  - Groups.AddToGroupAsync(Context.ConnectionId, roomId)
  - 실패 시: Clients.Caller.SendAsync("Error", ErrorDto { Code="FORBIDDEN" })
- [ ] Implement `LeaveRoom(string roomId)`:
  - Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId)
- [ ] Implement `SendMessage(string roomId, string content)`:
  - JWT에서 SenderId 추출
  - try-catch:
    - var dto = await _chatService.SendMessageAsync()
    - Clients.Group(roomId).SendAsync("ReceiveMessage", dto)
  - catch (InvalidOperationException): Clients.Caller.SendAsync("Error", ErrorDto)
  - catch (Exception): 로그 + 일반 에러 응답
- [ ] Implement `Typing(string roomId)` (미래 확장):
  - Clients.OthersInGroup(roomId).SendAsync("UserTyping", { CharacterId })
- [ ] Add logging

**Requirements**: US-1 (메시지 전송 및 수신), AC-1 (브로드캐스팅), AC-5 (에러 처리)
**Design Reference**: [API Design - SignalR Hub Methods]

---

### 4.3 Configure SignalR in Program.cs ⏱️ 30분
- [ ] Open `IdleRPG.API/Program.cs`
- [ ] Add SignalR service: `builder.Services.AddSignalR()`
- [ ] Map SignalR Hub: `app.MapHub<ChatHub>("/chat")`
- [ ] Configure JWT for SignalR (Query String 방식):
  ```csharp
  builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
      .AddJwtBearer(options => {
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
  ```

**Requirements**: AC-3 (JWT 인증)
**Design Reference**: [Security Considerations - Authentication]

---

### 4.4 Add CORS Configuration for SignalR ⏱️ 15분
- [ ] Open `IdleRPG.API/Program.cs`
- [ ] Update CORS policy to allow SignalR:
  ```csharp
  builder.Services.AddCors(options => {
      options.AddPolicy("AllowUnity", policy => {
          policy.WithOrigins("http://localhost:*")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials(); // SignalR requires credentials
      });
  });
  ```

**Requirements**: AC-1 (SignalR 연동)
**Design Reference**: [API Design - SignalR Hub]

---

### 4.5 Create Global Error Handling Middleware (Optional) ⏱️ 30분
- [ ] Create `IdleRPG.API/Middleware/ErrorHandlingMiddleware.cs`
- [ ] Handle exceptions globally:
  - NotFoundException → 404
  - ValidationException → 400
  - ForbiddenException → 403
  - Exception → 500 (로그)
- [ ] Register in Program.cs: `app.UseMiddleware<ErrorHandlingMiddleware>()`

**Requirements**: AC-5 (에러 처리)
**Design Reference**: [Error Handling]

---

## 🗃️ Milestone 5: Database

### 5.1 Create Database Migration ⏱️ 1.5시간
- [ ] Add migration to `IdleRPG.Infrastructure/migration.sql`
- [ ] Use `DO $EF$ BEGIN ... END $EF$` pattern (idempotent)
- [ ] Add CREATE TABLE:
  - ChatRooms (Id, Type, Name, GuildId nullable, CreatedAt)
  - ChatRoomParticipants (Id, RoomId, CharacterId, JoinedAt)
  - ChatMessages (Id, RoomId, SenderId, Content, CreatedAt)
- [ ] Add CREATE INDEX:
  - IX_ChatRooms_Type
  - IX_ChatRooms_GuildId
  - IX_ChatRoomParticipants_RoomId
  - IX_ChatRoomParticipants_CharacterId
  - IX_ChatRoomParticipants_RoomId_CharacterId (UNIQUE)
  - **IX_ChatMessages_RoomId_CreatedAt** (복합 인덱스, DESC)
- [ ] Add FK constraints:
  - ChatRooms.GuildId → Guilds.Id (ON DELETE RESTRICT)
  - ChatRoomParticipants.RoomId → ChatRooms.Id (ON DELETE CASCADE)
  - ChatRoomParticipants.CharacterId → Characters.Id (ON DELETE RESTRICT)
  - ChatMessages.RoomId → ChatRooms.Id (ON DELETE RESTRICT)
  - ChatMessages.SenderId → Characters.Id (ON DELETE RESTRICT)
- [ ] Test migration locally (create separate .sql file in Migrations/ folder)

**Requirements**: All
**Design Reference**: [Migration Plan]
**참고**: `CLAUDE.md - Database Migration`

---

### 5.2 Create ChatRoomSeeder ⏱️ 30분
- [ ] Create `IdleRPG.Infrastructure/Seeders/ChatRoomSeeder.cs`
- [ ] Add Global 채팅방 초기 데이터:
  - Id: 고정 GUID (예: "00000000-0000-0000-0000-000000000001")
  - Type: Global (1)
  - Name: "전체 채팅"
  - GuildId: NULL
  - CreatedAt: NOW()
- [ ] Register in `ApplicationDbContextSeed.cs` (if applicable)
- [ ] Use idempotent pattern (중복 방지)

**Requirements**: US-1 (전체 채팅)
**Design Reference**: [Data Seeding]

---

### 5.3 Apply Migration via Jenkins ⏱️ 15분
- [ ] Commit migration.sql to git
- [ ] Push to remote repository
- [ ] Trigger Jenkins pipeline (DO NOT run `dotnet ef database update` locally!)
- [ ] Verify migration success in Jenkins logs
- [ ] Check database schema via pgAdmin or psql

**Requirements**: All
**Design Reference**: [Migration Plan]
**참고**: `docs/jenkins/DEPLOYMENT_GUIDE.md`

---

## 🧪 Milestone 6: Testing & Documentation

### 6.1 Create ChatService Unit Tests ⏱️ 2시간
- [ ] Create `IdleRPG.Tests/Application/Services/ChatServiceTests.cs`
- [ ] Mock dependencies: IUnitOfWork, IMemoryCache, ILogger
- [ ] Test `SendMessageAsync`:
  - ✅ 정상 메시지 전송 → ChatMessageDto 반환
  - ✅ 길이 초과 (501자) → ValidationException
  - ✅ 쿨다운 위반 (1초 이내 재전송) → InvalidOperationException
  - ✅ 존재하지 않는 roomId → NotFoundException
- [ ] Test `GetMessagesAsync`:
  - ✅ beforeId 없음 → 최신 50개
  - ✅ beforeId 제공 → 이전 50개
  - ✅ 권한 없음 → ForbiddenException
- [ ] Test `CanAccessRoomAsync`:
  - ✅ Global 채팅방 → true
  - ✅ Guild 채팅방 (같은 길드) → true
  - ✅ Guild 채팅방 (다른 길드) → false
  - ✅ Whisper 채팅방 (참여자) → true
  - ✅ Whisper 채팅방 (비참여자) → false
- [ ] Test `CreateWhisperRoomAsync`:
  - ✅ 새 Whisper 방 생성 → room.Id 반환
  - ✅ 기존 방 존재 (A-B 또는 B-A) → 기존 room.Id 반환
  - ✅ 자기 자신과 Whisper (A==B) → InvalidOperationException
  - ✅ 존재하지 않는 캐릭터 → NotFoundException
- [ ] Use AAA pattern (Arrange-Act-Assert)
- [ ] Use FluentAssertions
- [ ] Achieve 85%+ code coverage

**Requirements**: US-1, US-2, US-3
**Design Reference**: [Testing Strategy - Unit Tests]

---

### 6.2 Create ChatHub Integration Tests ⏱️ 1.5시간
- [ ] Create `IdleRPG.Tests/API/Hubs/ChatHubTests.cs`
- [ ] Setup: TestServer, InMemory Database, JWT Token
- [ ] Test `SendMessage`:
  - ✅ 정상 메시지 → Clients.Group() 호출 확인
  - ✅ 에러 발생 → Clients.Caller.SendAsync("Error") 확인
- [ ] Test `JoinRoom`:
  - ✅ 권한 있음 → Groups.AddToGroupAsync() 확인
  - ✅ 권한 없음 → Error 이벤트
- [ ] Test `LeaveRoom`:
  - ✅ 정상 퇴장 → Groups.RemoveFromGroupAsync() 확인

**Requirements**: US-1, AC-1 (브로드캐스팅)
**Design Reference**: [Testing Strategy - Integration Tests]

---

### 6.3 Create ChatController Integration Tests ⏱️ 1시간
- [ ] Create `IdleRPG.Tests/API/Controllers/ChatControllerTests.cs`
- [ ] Setup: TestServer, InMemory Database, JWT Token
- [ ] Test `GET /api/chat/rooms/{roomId}/messages`:
  - ✅ 정상 조회 → 200 OK, 메시지 리스트
  - ✅ 권한 없음 → 403 Forbidden
  - ✅ 존재하지 않는 roomId → 404 Not Found
- [ ] Test `GET /api/chat/rooms`:
  - ✅ 정상 조회 → 200 OK, 채팅방 리스트
  - ✅ JWT 없음 → 401 Unauthorized

**Requirements**: US-2, US-3
**Design Reference**: [Testing Strategy - Integration Tests]

---

### 6.4 Create ChatMessageRepository N+1 Query Test ⏱️ 45분
- [ ] Create `IdleRPG.Tests/Infrastructure/Repositories/ChatMessageRepositoryTests.cs`
- [ ] Setup: InMemory Database
- [ ] Test `GetByRoomIdAsync`:
  - ✅ Cursor 페이징 (beforeId 제공)
  - ✅ Select Projection → 단일 LEFT JOIN 쿼리 (N+1 방지)
  - ✅ 필요한 컬럼만 SELECT (Id, Name, Level만)
  - ✅ AsNoTracking 사용 확인
- [ ] Use SQL logging to verify query count (1 query expected)

**Requirements**: AC-4 (N+1 방지)
**Design Reference**: [Testing Strategy - Database Integration]

---

### 6.5 Create Unity Documentation ⏱️ 1.5시간
- [ ] Create folder: `D:\Proj\IdleGameClient\Docs\unity\realtime-chat\`
- [ ] Create `API_SPEC.md`:
  - REST API:
    - `GET /api/chat/rooms/{roomId}/messages` (Cursor 페이징 포함)
    - `GET /api/chat/rooms`
  - SignalR Hub:
    - Hub URL: `/chat`
    - Hub Methods: `JoinRoom`, `LeaveRoom`, `SendMessage`, `Typing`
    - Client Events: `ReceiveMessage`, `Error`, `UserJoined`, `UserLeft`, `UserTyping`
    - JWT 인증: Query String 방식 (`?access_token={token}`)
  - Request/Response examples (JSON)
  - Unity C# usage example (UnityWebRequest + SignalR)
- [ ] Create `DTOs.cs`:
  - ChatMessageDto
  - CharacterSummaryDto
  - ErrorDto
  - ChatRoomDto
  - Use [JsonProperty] attributes (Newtonsoft.Json)
- [ ] Create `SIGNALR_INTEGRATION_GUIDE.md`:
  - SignalR Client 패키지 설치
  - Connection 생성 예시
  - Event 구독 예시
  - Error handling 예시
- [ ] Create `PROFANITY_FILTER.md`:
  - 서버 원본 저장 + 클라이언트 필터링 설명
  - Unity 구현 예시 (ProfanityFilter 클래스)
  - 사용자 설정 (PlayerPrefs)
- [ ] Update `D:\Proj\IdleGameClient\Docs\unity\README.md` main index

**Requirements**: All
**Design Reference**: [Unity Client Integration]
**참고**: `docs/unity/UNITY_DOCUMENTATION_GUIDE.md`

---

### 6.6 Create ERROR_HANDLING.md (Unity) ⏱️ 30분
- [ ] Create `D:\Proj\IdleGameClient\Docs\unity\realtime-chat\ERROR_HANDLING.md`
- [ ] Document Error Event 구독:
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
- [ ] Document Error Code 목록:
  - INVALID_MESSAGE, COOLDOWN_ACTIVE, FORBIDDEN, INVALID_ROOM_ID, SERVER_ERROR
- [ ] Add Unity UI 처리 예시 (Toast, Dialog, Redirect)

**Requirements**: AC-5 (에러 처리)
**Design Reference**: [Unity Client Integration - Error Handling]

---

## 🚀 Post-Implementation

### ✅ Completion Checklist
- [ ] All tasks completed and tested
- [ ] Unit tests passing (85%+ coverage)
- [ ] Integration tests passing
- [ ] Migration applied via Jenkins (DO NOT run `dotnet ef database update` locally!)
- [ ] Unity documentation complete (4 files)
- [ ] Code review completed
- [ ] Git commit with descriptive message
- [ ] Feature merged to main branch

---

## 📝 Notes

### Blockers
<!-- 작업 중 발생한 장애 요소 기록 -->

### Decisions Made
<!-- TODO(human) 해소 과정에서 내린 결정 기록 -->

### Future Improvements
<!-- 나중에 개선할 사항 -->
- Redis 도입 (쿨다운 캐싱, 권한 체크 캐싱)
- Policy-based Authorization 전환
- Covering Index 성능 측정 및 적용
- Rate Limiting 추가
- 욕설 필터 외부 API 연동 (선택)
- Typing indicator 기능 완성
- Reactions, 답장, 편집 기능 추가

---

**시작일**: 2025-10-30
**완료일**: -
**총 소요 시간**: ~24.75시간
**Design 추적성**: [design.md의 모든 컴포넌트 커버 확인 완료]
