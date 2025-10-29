# ADR-0002: SignalR 도입 결정

**Status**: Accepted
**Date**: 2025-10-29
**Decider(s)**: Development Team
**Related Spike**: Skipped (프로토타입에서 검증 예정)

---

## Context (배경)

**문제 상황**: 방치형 MMORPG에 실시간 채팅 시스템(전체, 길드, 귓속말)을 추가해야 합니다. 기존 REST API 폴링 방식은 실시간성이 부족하고 서버 부하가 높습니다.

**현재 상태**:
- 프로젝트에 실시간 통신 기술이 전무
- ASP.NET Core 8.0 기반 REST API만 존재
- 목표: 100명 동시 접속, 메시지 지연 < 500ms

**고려사항**:
- 학습 프로젝트이므로 러닝 커브와 구현 복잡도 중요
- Unity 클라이언트 호환성 (SignalR Client 라이브러리 지원)
- 향후 확장성 (길드 레이드, 실시간 알림)

---

## Decision (결정)

**선택한 방안**: **SignalR** (ASP.NET Core 내장)

**선택 이유**:
1. **ASP.NET Core 8.0 네이티브 지원**: 별도 패키지 설치 불필요, JWT 인증 통합 용이
2. **추상화 수준**: Transport 자동 협상 (WebSocket → Server-Sent Events → Long Polling), 개발자는 비즈니스 로직만 집중
3. **Unity 공식 지원**: `Microsoft.AspNetCore.SignalR.Client` NuGet, `.NET Standard 2.1` 호환
4. **학습 목적 적합**: Connection 관리, Group, Hub 개념 학습 가능
5. **Scale-out 준비**: Redis Backplane 지원 (미래 확장 시)

---

## Alternatives Considered (검토한 대안들)

### Alternative 1: REST Polling
- **장점**:
  - 기존 REST API 지식 재사용
  - 구현 단순 (30분 컷)
- **단점**:
  - 실시간성 부족 (최소 1초 지연)
  - 서버 부하 높음 (100명 × 1초 폴링 = 100 req/s)
  - 배터리 소모 (모바일)
- **기각 이유**: 실시간 채팅 요구사항 미충족 (US-1 "실시간 브로드캐스팅")

### Alternative 2: WebSocket Raw
- **장점**:
  - 최대 성능 (오버헤드 최소)
  - 프로토콜 제어 가능
- **단점**:
  - Connection 관리 복잡 (재연결, Heartbeat 직접 구현)
  - Unity 통합 복잡 (third-party 라이브러리 필요: `BestHTTP/2`)
  - JWT 인증 직접 구현 필요
  - 러닝 커브 높음 (학습 목적 부적합)
- **기각 이유**: 학습 프로젝트에 과도한 복잡도

### Alternative 3: gRPC Streaming
- **장점**:
  - HTTP/2 기반, 양방향 스트리밍
  - Protocol Buffers (성능 우수)
- **단점**:
  - Unity 지원 제한적 (gRPC-Unity 패키지 실험적)
  - HTTP/2 인프라 요구 (현재 EC2 t3.micro는 HTTP/1.1)
  - 학습 곡선 높음 (Protobuf 정의, Code Generation)
- **기각 이유**: Unity 호환성 불확실, 인프라 제약

---

## Consequences (결과 및 영향)

### Positive (긍정적 영향)
- ✅ **실시간 통신 학습**: Hub, Group, Connection Lifecycle 개념 습득
- ✅ **빠른 프로토타이핑**: REST API와 유사한 Hub 메서드 정의, 1일 내 MVP 가능
- ✅ **Unity 통합 간편**: `HubConnectionBuilder`, `On<T>()` 패턴으로 30분 연동
- ✅ **JWT 재사용**: 기존 인증 인프라 활용 (`OnMessageReceived` Query String)
- ✅ **미래 확장 가능**: 길드 레이드, PVP 실시간 알림, 푸시 알림 기반 마련

### Negative (부정적 영향 또는 Trade-off)
- ⚠️ **Scale-out 제약**: 단일 서버 환경에서는 문제 없으나, 멀티 인스턴스 시 Redis Backplane 필수 (추가 학습 필요)
- ⚠️ **디버깅 복잡도**: WebSocket 디버깅은 REST보다 어려움 (Browser DevTools, Fiddler 필요)
- ⚠️ **State 관리**: Connection 상태 추적 필요 (OnConnectedAsync, OnDisconnectedAsync)
- ⚠️ **패킷 크기 제한**: 기본 100KB (대용량 메시지 비지원, 채팅에는 충분)

### Neutral (중립적 변화)
- 🔄 **새 개념 학습**: Hub, Caller, Clients.Group() 등 SignalR 전용 개념
- 🔄 **에러 핸들링 패턴 변경**: REST (Status Code) vs SignalR (Caller.SendAsync("Error"))
- 🔄 **테스트 전략 변화**: Hub 메서드 Unit Test 시 IHubContext<T> Mock 필요

---

## Implementation Notes (구현 시 주의사항)

**필수 조치**:
- [x] `Program.cs`에 `builder.Services.AddSignalR()` 추가
- [x] JWT 인증 Query String 지원 (`OnMessageReceived` 이벤트)
- [x] CORS 설정 (`AllowCredentials()` 필수, SignalR은 Credentials 필요)
- [x] Hub 라우팅 (`app.MapHub<ChatHub>("/chat")`)
- [ ] Unity SignalR Client 패키지 설치 (`Microsoft.AspNetCore.SignalR.Client` 6.0+)

**권장 사항**:
- Connection Timeout 설정 (`ClientTimeoutInterval`, `KeepAliveInterval`)
- 로깅: `_logger.LogInformation()` (Connection/Disconnection 추적)
- 에러 핸들링: try-catch + `Clients.Caller.SendAsync("Error", ErrorDto)`
- 테스트: `IHubContext<ChatHub>` Mock 사용 (Moq)

---

## References (참고 자료)

- [ASP.NET Core SignalR 공식 문서](https://learn.microsoft.com/en-us/aspnet/core/signalr/introduction)
- [SignalR JWT 인증 가이드](https://learn.microsoft.com/en-us/aspnet/core/signalr/authn-and-authz)
- [Unity SignalR Client 패키지](https://www.nuget.org/packages/Microsoft.AspNetCore.SignalR.Client/)
- [Redis Backplane (Scale-out)](https://learn.microsoft.com/en-us/aspnet/core/signalr/redis-backplane)
- Requirements: `.claude/memories/specs/realtime-chat/requirements.md` (US-1, AC-1)

---

⚠️ **단어 수**: ~450단어 (목표: 500단어 이내)
