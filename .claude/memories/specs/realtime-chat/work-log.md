# Work Log: Realtime Chat System

> 이 문서는 실시간 채팅 시스템 구현 과정의 작업 기록입니다.

---

## 2025-10-30 15:30

### Task Completed
- [x] 1.1 Create RoomType Enum

### Files Changed
- `IdleRPG.Domain/Enums/RoomType.cs` (new file)

### Key Decisions
- Enum 값을 명시적으로 정수로 설정 (Global=1, Guild=2, Whisper=3)
- DB에서 int로 저장될 예정

### Notes
- XML 문서화 주석 추가 완료
- 각 채팅방 타입의 접근 권한 설명 포함

---

## 2025-10-30 15:45

### Task Completed
- [x] 1.2 Create ChatRoom Entity

### Files Changed
- `IdleRPG.Domain/Entities/ChatRoom.cs` (new file)

### Key Decisions
- BaseEntity 상속 대신 직접 속성 정의 (프로젝트의 기존 Entity 패턴 따름)
- CreatedAt 기본값을 DateTime.UtcNow로 설정 (일관성)
- Navigation Properties를 빈 컬렉션으로 초기화 (NullReferenceException 방지)

### Notes
- Guild, ChatMessage, ChatRoomParticipant는 아직 생성 전이므로 컴파일 에러 발생 예상
- EF Core Configuration에서 관계 설정 필요

---

## 2025-10-30 16:00 (리팩토링)

### Task Completed
- [x] 리팩토링: BaseEntity 개선 및 전체 적용

### Files Changed
- `IdleRPG.Domain/Repositories/BaseEntity.cs` (modified)
  - CreatedAt 속성 추가
  - BaseEntity (Guid 전용) 클래스 추가
- `IdleRPG.Domain/Entities/ChatRoom.cs` (modified)
  - BaseEntity 상속으로 변경
  - Id, CreatedAt 제거 (상속받음)
- `IdleRPG.Domain/Entities/Character.cs` (modified)
  - BaseEntity 상속으로 변경
  - Id, CreatedAt 제거 (상속받음)
  - UpdatedAt은 유지 (Character 전용)

### Key Decisions
- **BaseEntity 패턴 도입 이유**:
  - 중복 코드 제거 (DRY 원칙)
  - 프로젝트 전체 일관성 확보
  - 나중에 Soft Delete, Audit 기능 추가 시 용이
- **BaseEntity<TKey>와 BaseEntity 분리**:
  - 제네릭 버전: 유연성 (나중에 int 키 필요 시)
  - Guid 전용 버전: 편의성 (프로젝트 표준)
- **UpdatedAt은 BaseEntity에 포함하지 않음**:
  - 모든 Entity가 수정 시간을 추적할 필요는 없음
  - ChatRoom은 생성 후 거의 수정 안 됨
  - 필요한 Entity만 개별적으로 추가

### Notes
- 이 리팩토링으로 프로젝트의 모든 Entity가 일관된 패턴 사용
- Clean Architecture 학습 목표와 부합 (공통 속성 추상화)
- 실무 표준 패턴 적용

---

## 2025-10-30 16:10

### Task Completed
- [x] 1.3 Create ChatRoomParticipant Entity

### Files Changed
- `IdleRPG.Domain/Entities/ChatRoomParticipant.cs` (new file)

### Key Decisions
- BaseEntity 상속으로 Id, CreatedAt 자동 포함
- JoinedAt 별도 추가 (참여 시간 추적용)
- Navigation Properties에 null-forgiving operator (!) 사용
  - EF Core가 런타임에 채워줄 것임을 명시
  - null 경고 제거

### Notes
- N:M 중간 테이블 패턴 (ChatRoom ↔ Character)
- Whisper 타입 채팅방에서만 사용
- EF Core Configuration에서 UNIQUE(RoomId, CharacterId) 제약 필요

---

## 2025-10-30 16:15

### Task Completed
- [x] 1.4 Create ChatMessage Entity

### Files Changed
- `IdleRPG.Domain/Entities/ChatMessage.cs` (new file)

### Key Decisions
- BaseEntity 상속으로 Id, CreatedAt 자동 포함
- Content 최대 1000자 (DB), Application 레이어에서 500자 검증 예정
- CreatedAt을 Cursor 페이징 기준으로 활용
- Navigation Properties에 null-forgiving operator 사용

### Notes
- CreatedAt이 메시지 전송 시간으로 사용됨 (비즈니스 의미와 일치)
- EF Core Configuration에서 복합 인덱스 필요: (RoomId, CreatedAt DESC)
- N+1 쿼리 방지를 위해 Select Projection 사용 예정

---

## 2025-10-30 16:20

### Task Completed
- [x] 1.5 Create IChatRoomRepository Interface

### Files Changed
- `IdleRPG.Domain/Repositories/IChatRoomRepository.cs` (new file)

### Key Decisions
- **GetAccessibleRoomsAsync 파라미터 설계**:
  - characterId: 필수 (권한 체크)
  - guildId: nullable (Guild 타입 방 필터링용)
  - 이유: Repository는 비즈니스 로직 없이 데이터 조회만
- **CancellationToken 기본값 = default**:
  - 선택적 매개변수로 설계
  - 호출자가 생략 가능 (편의성)
- **반환 타입**:
  - GetByIdAsync: ChatRoom? (nullable, 없을 수 있음)
  - GetAccessibleRoomsAsync: List<ChatRoom> (빈 리스트 가능)

### Notes
- Domain Layer는 EF Core에 의존하지 않음 (인터페이스만)
- Infrastructure Layer에서 구현 예정 (Task 2.4)
- 3개 메서드만 정의 (YAGNI 원칙)

---

## 2025-10-30 16:30

### Task Completed
- [x] 1.6 Create IChatMessageRepository Interface
- [x] Create ChatMessageDto, CharacterSummaryDto

### Files Changed
- `IdleRPG.Domain/Repositories/IChatMessageRepository.cs` (new file)
- `IdleRPG.Application/DTOs/Chat/ChatMessageDto.cs` (new file)

### Key Decisions
- **아키텍처 트레이드오프: Domain → Application 의존**:
  - 문제: Repository Interface (Domain)가 DTO (Application) 참조
  - Clean Architecture 원칙 위반이지만 실용적 선택
  - 이유: Select Projection 성능 최적화 (N+1 방지)
  - 대안 검토:
    - Entity 반환 → N+1 쿼리 발생 가능
    - DTO를 Domain으로 이동 → DTO는 Application 관심사
  - **결정**: 성능 우선, 프로젝트 규모 고려 (학습용)
- **Cursor 페이징 파라미터**:
  - beforeId: nullable (첫 조회 시 null)
  - take: 10-100 범위 (Application에서 검증)
- **Select Projection 명시**:
  - DTO 반환 = Repository에서 Projection 수행
  - 필요한 컬럼만 조회 (Sender.Id, Name, Level)

### Notes
- 이 설계는 실무에서도 논쟁적 (순수 vs 실용)
- 대규모 프로젝트: CQRS 패턴으로 Read/Write 분리 권장
- 학습 목적: 트레이드오프 이해가 중요

---

## 2025-10-30 16:40 (리팩토링: Clean Architecture 복원)

### Task Completed
- [x] 리팩토링: IChatMessageRepository를 Clean Architecture 준수하도록 수정

### Files Changed
- `IdleRPG.Domain/Repositories/IChatMessageRepository.cs` (modified)

### Key Decisions
- **근본 원인 파악**:
  - Repository Interface가 두 가지 책임을 가짐 (Entity 저장 + DTO 조회)
  - Read(조회 최적화)와 Write(저장)의 관심사 혼재
  - 결과: Domain → Application 의존 (아키텍처 위반)

- **수정 내용**:
  - ❌ 이전: `Task<List<ChatMessageDto>> GetByRoomIdAsync(...)` (DTO 반환)
  - ✅ 수정: `Task<List<ChatMessage>> GetByRoomIdAsync(...)` (Entity 반환)
  - ❌ 이전: `Task<ChatMessageDto?> GetDtoByIdAsync(...)` (DTO 반환)
  - ✅ 수정: `Task<ChatMessage?> GetByIdAsync(...)` (Entity 반환)
  - Application DTO 의존성 제거 → Domain Layer 순수성 복원

- **새로운 책임 분담**:
  - **Repository (Infrastructure)**: Entity 조회, Include로 Eager Loading
  - **Service (Application)**: Entity → DTO 변환 (Mapper 또는 수동)
  - 장점: Clean Architecture 준수, 각 계층의 책임 명확

- **N+1 쿼리 방지 전략**:
  - Repository에서 `.Include(m => m.Sender)` 사용
  - Service에서 DTO 변환 시 이미 로드된 Navigation Property 사용
  - 결과: 단일 쿼리 (LEFT JOIN), N+1 발생 안 함

### Notes
- **학습 포인트**: "성능 최적화"를 핑계로 아키텍처를 깨면 안 됨
- Include + Service 변환 패턴은 실무에서도 일반적
- Select Projection이 필요하면 CQRS 패턴 도입 (나중에 학습)
- ChatMessageDto는 Application Layer에 유지 (올바른 위치)

---

## 2025-10-30 14:00

### Task Completed
- [x] 2.1 Create ChatRoomConfiguration

### Files Changed
- `IdleRPG.Infrastructure/Configurations/ChatRoomConfiguration.cs` (new file, 69 lines)

### Key Decisions
- **OnDelete Behavior 전략**:
  - Messages → Restrict: 채팅방 삭제 시 메시지 보존 (신고 시스템 증거)
  - Participants → Cascade: 채팅방 삭제 시 참여자 정보 함께 삭제 (정리 로직)
  - Guild → Restrict: 길드 삭제 시 채팅방 보존 (별도 정리 로직 필요)

- **Fluent API 사용**: Entity 클래스를 순수하게 유지하고 Infrastructure 계층에 DB 설정 격리 (Clean Architecture 준수)

- **Enum → int 변환**: DB 호환성 향상, enum 이름 변경에도 기존 데이터 영향 없음

### Notes
- 인덱스 2개 설정: IX_ChatRooms_Type (채팅방 타입별 조회), IX_ChatRooms_GuildId (길드 채팅방 조회)
- Navigation Properties 3개 설정: Guild (nullable), Messages, Participants
- 모든 관계 설정 완료: 1:N (Messages, Participants), N:1 (Guild)

---

## 2025-10-30 14:10

### Task Completed
- [x] 2.2 Create ChatRoomParticipantConfiguration

### Files Changed
- `IdleRPG.Infrastructure/Configurations/ChatRoomParticipantConfiguration.cs` (new file, 57 lines)

### Key Decisions
- **복합 Unique Index 설정**:
  - `(RoomId, CharacterId)` 복합 인덱스를 Unique로 설정
  - DB 레벨에서 중복 참여 방지 (Application 검증과 이중 보호)
  - 데이터 무결성 보장

- **OnDelete Behavior 전략**:
  - Room → Cascade: 채팅방 삭제 시 참여자 정보도 함께 삭제
  - Character → Restrict: 캐릭터 삭제 시 참여 기록 보존 (채팅 히스토리 보존)

- **N:M 중간 테이블 패턴**:
  - ChatRoom ↔ Character 간 N:M 관계 표현
  - 나중에 그룹 채팅(3명 이상)으로 확장 가능
  - Whisper(1:1)는 Application 레벨에서 2명 제약 검증

### Notes
- 인덱스 3개 설정: RoomId (방별 참여자 조회), CharacterId (사용자별 참여 방), Unique 복합 인덱스
- Navigation Properties 2개: Room, Character
- Whisper 채팅방에서만 사용되는 테이블 (Global, Guild는 미사용)

---

## 2025-10-30 14:20

### Task Completed
- [x] 2.3 Create ChatMessageConfiguration

### Files Changed
- `IdleRPG.Infrastructure/Configurations/ChatMessageConfiguration.cs` (new file, 54 lines)

### Key Decisions
- **복합 인덱스 최적화**:
  - `(RoomId, CreatedAt)` 복합 인덱스 설정
  - 쿼리 최적화: `WHERE RoomId = ? ORDER BY CreatedAt DESC`
  - Cursor 페이징 성능 향상 (모든 히스토리 조회에 사용)
  - 인덱스 순서 중요: 왼쪽부터 사용 (RoomId 필터 → CreatedAt 정렬)

- **OnDelete Restrict 전략**:
  - Room, Sender 모두 Restrict
  - 메시지 보존 (신고 시스템 증거, 규정 준수, 감사 추적)
  - 별도 정리 로직은 Application Layer에서 처리 (익명화, 아카이빙)

- **Content 길이 제한**:
  - DB: MaxLength 1000자
  - Application: 500자 검증 예정
  - 이중 보호 + DB 여유분 (정책 변경, 시스템 메시지 대응)

### Notes
- 복합 인덱스 1개: RoomId + CreatedAt (가장 중요한 인덱스)
- Navigation Properties 2개: Room, Sender
- AC-4 (N+1 방지) 준비: Include 또는 Select Projection 사용 예정

---

## 2025-10-30 14:30

### Task Completed
- [x] 2.4 Create ChatRoomRepository

### Files Changed
- `IdleRPG.Infrastructure/Repositories/ChatRoomRepository.cs` (new file, 92 lines)

### Key Decisions
- **AsNoTracking 사용**:
  - 모든 읽기 쿼리에 AsNoTracking() 적용
  - Change Tracking 오버헤드 제거 (성능 향상)
  - 반환된 Entity는 수정되지 않으므로 추적 불필요

- **GetAccessibleRoomsAsync 3단계 조회**:
  - Global: 모든 Global 타입 방 조회
  - Guild: guildId가 있을 때만 조회 (같은 길드)
  - Whisper: ChatRoomParticipants 조인하여 참여 방 조회
  - 각 타입별 쿼리 분리 (명시적, 유지보수 용이)
  - 단일 복잡한 쿼리 대신 명확한 3개 쿼리

- **Include(Guild) Eager Loading**:
  - GetByIdAsync에서 Guild를 Eager Loading
  - N+1 쿼리 방지 (LEFT JOIN 사용)
  - Guild는 nullable이지만 Include 사용 가능

### Notes
- GameDBContext 의존성 주입 (프로젝트 표준)
- IChatRoomRepository 인터페이스 구현 완료
- 3개 메서드: GetByIdAsync, GetAccessibleRoomsAsync, AddAsync
- SaveChanges는 UnitOfWork 패턴에서 처리 (Repository에서 호출 안 함)

---

## 2025-10-30 14:40

### Task Completed
- [x] 2.5 Create ChatMessageRepository

### Files Changed
- `IdleRPG.Infrastructure/Repositories/ChatMessageRepository.cs` (new file, 81 lines)

### Key Decisions
- **Entity 반환 (Clean Architecture 복원)**:
  - Repository는 ChatMessage Entity 반환 (DTO 아님)
  - Include(m => m.Sender)로 Eager Loading
  - Service Layer에서 Entity → DTO 변환
  - Domain → Application 의존성 제거

- **Cursor 페이징 구현**:
  - beforeId를 기준으로 CreatedAt < beforeMessage.CreatedAt 조건
  - Offset 페이징보다 안정적 (중간 삽입/삭제에 영향 없음)
  - OrderByDescending(CreatedAt) + Take(take)

- **N+1 쿼리 방지**:
  - Include(m => m.Sender)로 Eager Loading
  - SQL: LEFT JOIN Characters (단일 쿼리)
  - Service에서 이미 로드된 Sender 사용 (추가 쿼리 없음)

- **beforeMessage 조회 최적화**:
  - Select(m => m.CreatedAt)로 CreatedAt만 조회
  - 전체 Entity 로드 불필요 (성능 향상)

### Notes
- 3개 메서드: GetByIdAsync, GetByRoomIdAsync, AddAsync
- AsNoTracking 사용 (읽기 전용 쿼리)
- 복합 인덱스 (RoomId, CreatedAt) 활용 예정
- Service Layer에서 DTO 변환 책임 (AutoMapper 또는 수동)

---

## 2025-10-30 14:50

### Task Completed
- [x] 2.6 Extend IUnitOfWork Interface

### Files Changed
- `IdleRPG.Application/Interfaces/IUnitOfWork.cs` (modified, +8 lines)

### Key Decisions
- **IUnitOfWork 확장**:
  - IChatRoomRepository ChatRooms 프로퍼티 추가
  - IChatMessageRepository ChatMessages 프로퍼티 추가
  - Unit of Work 패턴으로 트랜잭션 관리

### Notes
- 프로젝트 위치: IdleRPG.Application/Interfaces (Domain 아님)
- 다음 Task 2.7에서 UnitOfWork 구현체 업데이트 예정
- 현재 빌드 에러 정상 (구현체 미완성)

---

## 2025-10-30 15:00

### Task Completed
- [x] 2.7 Extend UnitOfWork Implementation

### Files Changed
- `IdleRPG.Infrastructure/UnitOfWork/UnitOfWork.cs` (modified, +32 lines)

### Key Decisions
- **Lazy Initialization 패턴 유지**:
  - 기존 Repository와 동일한 패턴 사용
  - null 체크 후 필요 시 인스턴스 생성
  - 메모리 효율성 (사용하지 않는 Repository는 생성 안 함)

- **프로퍼티 구현**:
  - ChatRooms: `new ChatRoomRepository(_context)`
  - ChatMessages: `new ChatMessageRepository(_context)`
  - 프로젝트 전체 일관성 유지

### Notes
- ✅ Infrastructure Layer 완료 (7/7 tasks, 100%)
- ✅ 빌드 성공 (오류 0개)
- private 필드 추가: `_chatRooms`, `_chatMessages`
- 프로젝트 실제 위치: `IdleRPG.Infrastructure/UnitOfWork/UnitOfWork.cs` (tasks.md와 경로 다름)

---

## 2025-10-30 (Task 3.1)

### Task Completed
- [x] 3.1 Create Chat DTOs

### Files Changed
- `IdleRPG.Application/DTOs/Chat/ChatMessageDto.cs` (modified, +20 lines)
- `IdleRPG.Application/DTOs/Chat/ErrorDto.cs` (new file, 21 lines)
- `IdleRPG.Application/DTOs/Chat/ChatRoomDto.cs` (new file, 69 lines)

### Key Decisions
- **미래 확장 필드 추가**:
  - ChatMessageDto: Reactions, IsEdited, ParentMessageId (모두 nullable)
  - CharacterSummaryDto: AvatarUrl (nullable)
  - ChatRoomDto: LastMessage (LastMessageDto 중첩, nullable)
  - 이유: Unity 클라이언트가 나중에 기능 추가 시 서버 DTO 변경 불필요

- **ReactionDto 설계**:
  - Emoji: 이모지 코드 (":thumbsup:", ":heart:")
  - CharacterIds: 반응한 사용자 목록
  - Count: 반응 횟수 (중복 데이터지만 클라이언트 편의성)

- **ErrorDto 에러 코드 표준**:
  - INVALID_MESSAGE: 메시지 길이/내용 오류
  - COOLDOWN_ACTIVE: 쿨다운 위반 (1초)
  - FORBIDDEN: 권한 없음 (채팅방 접근)
  - INVALID_ROOM_ID: 존재하지 않는 채팅방
  - SERVER_ERROR: 서버 내부 오류

- **LastMessageDto 설계**:
  - ContentPreview: 50자로 자른 내용 (채팅방 목록 UI)
  - UnreadCount: 읽지 않은 메시지 개수 (미래 확장, nullable)

### Notes
- ✅ Application Layer 진행률: 1/4 (25%)
- ✅ 빌드 성공 (오류 0개)
- XML 문서화 주석 완료 (Unity 클라이언트 참고 자료)
- DTO는 직렬화 전용 (비즈니스 로직 없음)
- 모든 DTO가 Application Layer에 위치 (Clean Architecture 준수)

---

## 2025-10-30 (Task 3.2)

### Task Completed
- [x] 3.2 Create IChatService Interface

### Files Changed
- `IdleRPG.Application/Services/IChatService.cs` (new file, 106 lines)

### Key Decisions
- **메서드 5개 정의**:
  1. **SendMessageAsync**: 메시지 전송 + 쿨다운 체크 + 권한 체크
  2. **GetMessagesAsync**: Cursor 페이징 히스토리 조회 (beforeId, take)
  3. **CanAccessRoomAsync**: 채팅방 접근 권한 검증 (Global/Guild/Whisper 분기)
  4. **GetAccessibleRoomsAsync**: 캐릭터가 접근 가능한 전체 채팅방 목록
  5. **CreateWhisperRoomAsync**: 1:1 귓속말 방 생성 또는 재사용

- **Exception 문서화**:
  - ValidationException: 메시지 길이 초과 (501자 이상)
  - InvalidOperationException: 쿨다운 위반 (1초 이내), 자기 자신과 귓속말
  - NotFoundException: 채팅방/캐릭터 미존재
  - ForbiddenException: 권한 없음

- **CancellationToken 기본값**:
  - 모든 메서드에 `CancellationToken cancellationToken = default` 추가
  - 비동기 작업 취소 지원 (서버 리소스 효율화)
  - 호출자가 생략 가능 (편의성)

- **Cursor 페이징 파라미터**:
  - beforeId: nullable (첫 조회 시 null)
  - take: 기본값 50, 최소 10, 최대 100 (Controller에서 검증 예정)

- **Whisper 방 재사용 전략**:
  - CreateWhisperRoomAsync는 기존 방 조회 후 없으면 생성
  - A-B, B-A 동일하게 처리 (순서 무관)
  - 중복 방 생성 방지 (비용 절감)

### Notes
- ✅ Application Layer 진행률: 2/4 (50%)
- ✅ 빌드 성공 (오류 0개)
- Interface만 정의 (구현은 Task 3.3에서)
- XML 문서화 완료 (exception, remarks 포함)
- 모든 메서드가 비동기 (async/await 패턴)
- Service Layer는 비즈니스 로직 처리 (Repository는 데이터 액세스만)

---

## 2025-10-30 (Task 3.3)

### Task Completed
- [x] 3.3 Create ChatService Implementation (3시간 예상)

### Files Changed
- `IdleRPG.Infrastructure/Services/ChatService.cs` (new file, 322 lines)

### Key Decisions
- **쿨다운 캐싱 전략**:
  - 캐시 키: `"chat:cooldown:{senderId}"` (네임스페이스 명확)
  - TTL: 5초 (1초 쿨다운 + 4초 안전 마진)
  - 캐시 값: `true` (단순 플래그, 존재 여부만 체크)
  - IMemoryCache.TryGetValue() 사용 (성능 최적화)

- **Entity → DTO 변환**:
  - 수동 매핑 (프로젝트 일관성 유지, AutoMapper 미사용)
  - MapToDto() private helper 메서드로 분리
  - CharacterSummaryDto 중첩 생성

- **권한 체크 로직 (CanAccessRoomAsync)**:
  - switch 문으로 RoomType별 분기 (Global/Guild/Whisper)
  - Global: 모든 캐릭터 접근 가능 (즉시 true 반환)
  - Guild: TODO로 표시 (Guild 시스템 미구현)
  - Whisper: Participants.Any() 사용 (LINQ)

- **Whisper 방 중복 확인**:
  - 설계 선택: 2번 단순 쿼리 (가독성 우선)
  - GetAccessibleRoomsAsync 활용 후 LINQ 필터링
  - 대안 (성능 우선): 1번 복잡한 JOIN 쿼리 (나중에 최적화 가능)

- **트랜잭션 범위**:
  - CreateWhisperRoomAsync: SaveChangesAsync 1번 호출
  - 방 생성 + 참여자 2명 추가를 단일 트랜잭션으로 처리 (원자성)
  - newRoom.Participants.Add() 사용 (Navigation Property 활용)

- **예외 처리**:
  - ArgumentException: 빈 메시지, 길이 초과 (501자)
  - InvalidOperationException: 쿨다운 위반, 자기 자신과 귓속말
  - UnauthorizedAccessException: 권한 없음
  - KeyNotFoundException: 캐릭터/채팅방 미존재

- **임시 해결책 (TODO)**:
  - Character.Name 미존재 → `"Character-{Id:N}"` 사용
  - Guild 시스템 미구현 → Guild 채팅방 접근 항상 false
  - TODO 주석으로 명시 (나중에 리팩토링 지점 표시)

### Notes
- ✅ Application Layer 진행률: 3/4 (75%)
- ✅ 빌드 성공 (오류 0개, 경고는 기존 코드)
- 5개 메서드 모두 구현 완료:
  1. SendMessageAsync (입력 검증 + 쿨다운 + 권한 + 저장 + 캐싱)
  2. GetMessagesAsync (권한 체크 + Repository 조회 + DTO 변환)
  3. CanAccessRoomAsync (RoomType별 권한 로직)
  4. GetAccessibleRoomsAsync (캐릭터별 접근 가능 방 목록)
  5. CreateWhisperRoomAsync (중복 확인 + 방 생성 + 참여자 추가)
- 로깅: Info, Warning 레벨 적절히 사용
- 보일러플레이트 코드 (DTO 매핑)는 AI가 자동 구현 → 학습 효율 향상
- 설계 논의는 스킵하고 합리적인 기본 설계 사용 → 빠른 진행

---

## 2025-10-30 (Task 3.4)

### Task Completed
- [x] 3.4 Register ChatService in DI Container

### Files Changed
- `IdleRPG.API/Program.cs` (modified, +5 lines)

### Key Decisions
- **DI 등록**:
  - IChatService → ChatService (Scoped 생명주기)
  - IMemoryCache 추가 (쿨다운 관리용, Singleton)
  - 프로젝트 표준 패턴 유지 (AddScoped)

- **SignalR JWT 경로 추가**:
  - OnMessageReceived 이벤트에 `/chat` 경로 추가
  - 기존 `/gamehub`와 동일한 토큰 추출 로직 공유
  - Query String 방식: `?access_token={token}`

### Notes
- ✅ Application Layer 완료 (4/4, 100%)
- ✅ 빌드 성공 (오류 0개)
- 다음 Milestone: API Layer (ChatController, ChatHub 구현)
- ChatHub에서도 JWT 인증 가능 (path.StartsWithSegments("/chat") 조건)

---

## 2025-10-30 (Task 4.1)

### Task Completed
- [x] 4.1 Create ChatController

### Files Changed
- `IdleRPG.API/Controllers/ChatController.cs` (new file, 172 lines)

### Key Decisions
- **REST API 엔드포인트 2개**:
  - `GET /api/chat/rooms/{roomId}/messages`: 메시지 히스토리 조회 (Cursor 페이징)
  - `GET /api/chat/rooms`: 접근 가능한 채팅방 목록 조회

- **JWT에서 CharacterId 추출**:
  - `User.FindFirst(ClaimTypes.NameIdentifier)?.Value`
  - Guid 파싱 실패 시 401 Unauthorized 반환

- **Input Validation**:
  - roomId: Guid.Empty 체크
  - take: 10-100 범위 검증 (기본값 50)
  - beforeId: nullable (첫 조회 시 null)

- **에러 처리 전략**:
  - 400 Bad Request: roomId 형식 오류, take 범위 초과
  - 401 Unauthorized: JWT 토큰 없음/만료
  - 403 Forbidden: 권한 없음 (UnauthorizedAccessException 캐치)
  - 404 Not Found: 채팅방/캐릭터 미존재 (KeyNotFoundException 캐치)
  - 500 Internal Server Error: 예상치 못한 예외

- **Swagger 문서화**:
  - XML 주석 완료 (summary, param, response 태그)
  - ProducesResponseType 어트리뷰트로 응답 코드 명시

### Notes
- ✅ API Layer 진행률: 1/5 (20%)
- ✅ 빌드 성공 (오류 0개)
- [Authorize] 어트리뷰트로 클래스 레벨 인증 강제
- ILogger 의존성 주입으로 로깅 구조화
- try-catch 패턴으로 Service Layer 예외 처리
- 다음 작업: ChatHub 구현 (SignalR 실시간 통신)

---
