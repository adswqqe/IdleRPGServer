# realtime-chat Spec Progress

**Feature**: realtime-chat
**Size**: L (Large)
**Mode**: 학습 모드
**Started**: 2025-10-28
**Status**: In Progress

---

## Progress Tracker

**Current Phase**: Phase 7 - 최종 문서 생성
**Current Question**: 완료
**Completed Questions**: 18/18

---

## Phase Overview

- [x] Phase 1: 데이터 모델링 (Q1-Q4) - Completed ✅
- [x] Phase 2: 아키텍처 계층 결정 (Q5-Q7) - Completed ✅
- [x] Phase 3: 데이터베이스 설계 (Q8-Q10) - Completed ✅
- [x] Phase 4: API 설계 (Q11-Q14) - Completed ✅
- [x] Phase 5: SignalR 설계 (Q15-Q18) - Completed ✅
- [x] Phase 6: 게임 밸런스 (AI 자동 제안) - Completed ✅
- [x] Phase 7: 최종 문서 생성 - Completed ✅

---

## Decision Log

### Q1: 메시지 저장 방식 ✅
**선택**: A - 모든 메시지 저장 (ChatMessage 테이블)
**이유**:
- 메시지 히스토리 조회 가능
- 신고 시스템, 감사 추적 지원
- EF Core CRUD, 페이징 학습 기회
**Entity**: ChatMessage (Id, RoomId, SenderId, Content, CreatedAt)

### Q2: 채팅방 타입 설계 ✅
**선택**: A - 단일 ChatRoom 테이블 + RoomType Enum
**이유**:
- 단순하고 확장 가능한 구조
- 타입별 비즈니스 로직 분기 학습
- Party, Trade 등 추가 용이
**Entity**: ChatRoom (Id, Type, Name, GuildId?, CreatedAt)
**Enum**: RoomType (Global, Guild, Whisper)

### Q3: ChatMessage-Character 관계 ✅
**선택**: A - Character 직접 참조 (FK)
**이유**:
- 정규화된 설계 (데이터 무결성)
- Include/Projection으로 N+1 방지 가능
- EF Core Navigation Property 학습
**FK**: SenderId → Character.Id (ON DELETE RESTRICT)
**최적화**: Include + AsNoTracking, 또는 Projection (DTO)

### Q4: ChatRoom 삭제 정책 ✅
**선택**: B - RESTRICT (Application Layer 명시적 처리)
**이유**:
- Application에서 삭제 로직 제어 (로깅, 아카이빙)
- 실수 방지 (DB 레벨 CASCADE 위험)
- Clean Architecture 계층 책임 학습
**FK**: RoomId → ChatRoom.Id (ON DELETE RESTRICT)
**구현**: Service에서 메시지 먼저 삭제 후 채팅방 삭제

---

## Phase 2: 아키텍처 계층 결정

### Q5: SignalR Hub 배치 계층 ✅
**선택**: A - API Layer
**이유**:
- Hub = Controller와 동급 (API 진입점)
- Clean Architecture 준수 (API → Application)
- 비즈니스 로직은 Service에 위임
**위치**: IdleRPG.API/Hubs/ChatHub.cs
**의존성**: IChatService (Application Layer) 주입

### Q6: 메시지 전송/저장 로직 배치 ✅
**선택**: B - Application Service에 위임
**이유**:
- Clean Architecture 준수 (API → Application → Infrastructure)
- 비즈니스 로직 재사용 (REST API, Hub 모두 사용)
- 테스트 용이 (Service만 Unit Test)
- 검증 로직 중앙화 (욕설 필터, 권한 체크)
**Service**: IChatService.SendMessageAsync()
**TODO(human)**: 욕설 필터, 채팅 제한 체크 로직

### Q7: 트랜잭션 경계 설정 ✅
**선택**: C - Unit of Work 패턴 (기존 패턴 준수)
**이유**:
- 프로젝트에 이미 IUnitOfWork 구현되어 있음 (일관성)
- 여러 Repository 작업을 SaveChangesAsync()로 원자적 커밋
- Lazy 초기화 패턴 활용
**구현**: IUnitOfWork에 ChatRooms, ChatMessages 추가
**패턴**: _unitOfWork.ChatMessages.AddAsync() → _unitOfWork.SaveChangesAsync()

---

## Phase 3: 데이터베이스 설계

### Q8: ChatMessage 인덱스 설계 ✅
**선택**: B - 복합 인덱스 (RoomId + CreatedAt DESC)
**이유**:
- WHERE + ORDER BY 동시 최적화
- Index Scan만으로 정렬 (Using filesort 없음)
- 채팅 시스템의 가장 빈번한 쿼리 최적화
**SQL**: CREATE INDEX idx_chatmessages_roomid_createdat ON ChatMessages(RoomId, CreatedAt DESC)
**쿼리 패턴**: WHERE RoomId = ? ORDER BY CreatedAt DESC LIMIT 100

### Q9: ChatMessage PK 타입 ✅
**선택**: A - GUID
**이유**:
- 프로젝트 기존 패턴 일치 (Character, Equipment 등 모두 GUID)
- 코드 일관성 유지
- 분산 환경 안전 (충돌 없음)
- 클라이언트에서 미리 생성 가능
**타입**: public Guid Id { get; set; } = Guid.NewGuid()

### Q10: ChatMessage.Content 타입 및 제약 ✅
**선택**: C - VARCHAR(1000) + Application 500자 검증
**이유**:
- 2단계 검증 패턴 (Application 500자, DB 1000자 여유)
- 유연성 (나중에 제한 완화 시 DB 변경 불필요)
- 스팸 방지 + 안전망
**DB**: Content VARCHAR(1000) NOT NULL
**Application**: 500자 제한 검증, 욕설 필터
**TODO(human)**: Service Layer에서 Content 검증 로직 작성

---

## Phase 4: API 설계

### Q11: REST API vs SignalR 역할 분리 ✅
**선택**: B - REST API 병행 (하이브리드)
**이유**:
- REST API로 초기 데이터 로드 (채팅방 목록, 히스토리)
- SignalR로 실시간 업데이트 (새 메시지)
- Unity WebRequest 지원 (SignalR 없이도 조회 가능)
- Postman 테스트 가능
**REST API**: GET /rooms, GET /messages (Stateless)
**SignalR Hub**: JoinRoom, SendMessage, ReceiveMessage (Stateful)

### Q12: 메시지 히스토리 페이징 방식 ✅
**선택**: B - Cursor 기반 (beforeId 옵션)
**이유**:
- 확장 가능한 설계 (미래 과거 로드 추가 시 API 변경 없음)
- 현재: beforeId 없이 최근 100개만 조회
- 미래: beforeId로 과거 메시지 무한 스크롤
- 인덱스 활용 최적화 (CreatedAt < beforeMessage.CreatedAt)
**API**: GET /api/chat/rooms/{roomId}/messages?beforeId={guid}&take=100
**쿼리**: WHERE RoomId = ? AND CreatedAt < (SELECT ...) ORDER BY CreatedAt DESC LIMIT ?

### Q13: 채팅 API 인증 및 권한 설계 ✅
**선택**: A - JWT Bearer 인증 (모든 API)
**이유**:
- 프로젝트 기존 패턴 일치 (Character API 모두 [Authorize])
- 일관성 있는 보안 정책
- 스팸 방지 (인증된 사용자만)
- 단순하고 명확
**Controller**: [Authorize] 속성
**Hub**: [Authorize] 속성, Context.User에서 CharacterId 추출
**TODO(human)**: 나중에 Guild/Whisper 권한 체크 로직 추가 (CanAccessRoomAsync)

### Q14: ChatMessageDto 응답 구조 ✅
**선택**: B - 발신자 중첩 (Nested DTO)
**이유**:
- 확장 가능 (이모티콘, 답장, 첨부파일 등 추가 용이)
- CharacterSummaryDto 재사용 (다른 API에서도)
- 하위 호환성 (Nullable 필드로 확장)
- 클라이언트 친화적 (UI 필요 정보 제공)
**DTO**: ChatMessageDto { Sender: CharacterSummaryDto, ... }
**미래 확장**: Reactions, IsEdited, Attachments (Nullable 필드)

---

## Phase 5: SignalR 설계

### Q15: SignalR Hub 메서드 설계 ✅
**선택**: B - 완전한 메서드 (JoinRoom, LeaveRoom, SendMessage, Typing)
**이유**:
- 명시적 채팅방 입장/퇴장 (SignalR Group 관리)
- 타입 안정성 (각 메서드 명확한 파라미터)
- 확장 가능 (Typing, 읽음 표시 추가 용이)
- 실무 표준 패턴
**메서드**: JoinRoom, LeaveRoom, SendMessage, Typing
**브로드캐스팅**: Clients.Group(roomId).SendAsync("ReceiveMessage")
**TODO(human)**: JoinRoom에서 권한 체크 로직 (CanAccessRoomAsync)

### Q16: SignalR 연결 생명주기 관리 ✅
**선택**: B - 로깅 + 자동 정리
**이유**:
- ILogger를 통한 연결/해제 추적
- 디버깅 용이 (에러 감지, 통계)
- SignalR 자동 Group 정리 활용
- 단순하면서 실용적
**OnConnectedAsync**: 로깅, TODO(온라인 상태 업데이트)
**OnDisconnectedAsync**: 로깅, 에러 처리, TODO(오프라인 상태)
**TODO(human)**: Character.IsOnline 상태 업데이트 로직

### Q17: SignalR 에러 처리 전략 ✅
**선택**: B - Try-Catch + 표준 에러 응답
**이유**:
- 표준화된 에러 응답 (ErrorDto)
- 클라이언트에서 에러 타입별 처리 가능
- 보안 (민감 정보 노출 방지)
- 명시적 에러 처리 (로깅)
**패턴**: Try-Catch → Clients.Caller.SendAsync("Error", ErrorDto)
**ErrorDto**: Code (string), Message (string)
**클라이언트**: connection.On<ErrorDto>("Error") 이벤트 핸들러

### Q18: SignalR Program.cs 설정 ✅
**선택**: B - JWT 인증 + CORS
**이유**:
- JWT Query String 방식 (OnMessageReceived 이벤트)
- CORS AllowCredentials (Unity 클라이언트 지원)
- 메시지 크기/연결 유지 시간 설정
- 프로덕션 준비
**설정**: JWT Events (Query String 토큰), MaximumReceiveMessageSize (100KB)
**라우팅**: app.MapHub<ChatHub>("/chat").RequireAuthorization()
**미래 학습**: Redis Backplane (Scale-out, 분산 환경)

---

## Phase 6: 게임 밸런스

### 채팅 시스템 밸런스 (AI 제안, 승인됨) ✅
- **메시지 길이**: Application 500자, DB 1000자
- **쿨다운**: 일반 1초/1개, 연속 5개 초과 시 10초
- **히스토리 조회**: 최대 100개, 기본 50개
- **욕설 필터**: TODO(human) - 한국어 금지어 50개, *** 치환
- **채팅방 타입별**: Global (쿨다운 1초), Guild (0.5초), Whisper (없음)
