# 🍄 IdleRPG Server 기술 스택 - AI 협업 학습 (2개월/8주)

> **프로젝트 성격**: 상용 프로젝트가 아닌 AI 협업 학습 프로젝트  
> **학습 목표**: 2개월 안에 기술 스택 이해 (완벽한 구현 X)  
> **학습 전략**: T-shaped Learning (6개 핵심 95% + 14개 60-80%)  
> **AI 협업**: Claude Code, Gemini, GPT와 함께 개발

---

## 🎯 학습 우선순위 (Gemini 검증 결과)

### ✅ 첫날부터 필수 (Day 1 부터)
- **Serilog** - 구조화된 로깅, 디버깅 시간 단축 (학습 프로젝트에 필수!)
- **ASP.NET Core 8.0 Web API** - RESTful API 기초
- **Entity Framework Core 9.0** - Code-First, Migration
- **PostgreSQL** - 관계형 데이터베이스 기본
- **JWT Bearer Token** - 인증/인가 기초
- **xUnit + Moq + FluentAssertions** - 단위 테스트

### 🔥 깊게 학습 (90-95% 깊이)
**Week 1-3 (Core Vertical Slice):**
- **EF Core 고급** - 복잡한 관계, 트랜잭션, 성능 최적화
- **PostgreSQL 고급** - 인덱싱, 쿼리 최적화
- **IHostedService** - Background Services (자동 사냥, 리셋)
- **Clean Architecture** - 계층 분리 (Domain → Application → Infrastructure → API)

**Week 4-5 (SignalR 집중):**
- **SignalR** - 실시간 채팅, WebSocket, Connection 관리, 그룹 관리 (90% 깊이)
- **Unity SignalR Client** - 클라이언트 연동

**Week 6-7 (Redis 집중):**
- **Redis** - Sorted Set 랭킹, 분산 락, 캐싱 전략 (90% 깊이)
- **StackExchange.Redis** - .NET Redis 클라이언트

### 📚 넓게 학습 (60-80% 깊이)
**Week 3-5:**
- **MediatR** - CQRS 패턴 기초 (70%)
- **FluentValidation** - DTO 검증 (70%)
- **AutoMapper** - Entity ↔ DTO 매핑 (60%)

**Week 8:**
- **가챠 시스템** - 확률 로직 (70%)
- **상점 & VIP** - 개념 이해 (60%)
- **우편 & 이벤트** - 기본 구조 (60%)

### ⚠️ 제외 또는 개념만 (Out of Scope)
- **IAP (In-App Purchase)** - Apple/Google 플랫폼 연동 시간 소모
- **FCM (Push Notification)** - 플랫폼 설정 시간 소모
- **Firebase Analytics** - 학습 외적인 설정 작업
- **Application Insights** - 프로덕션 모니터링 (개념만)
- **CDN / Load Balancer** - 인프라 운영 (개념만)

---

## 서버 (.NET)

### Core Framework
- **ASP.NET Core 8.0 Web API**
  - RESTful API 설계
  - Minimal API (선택)
  - HTTPS 지원
  - CORS 설정

### Database & ORM
- **PostgreSQL 14+**
  - 관계형 데이터베이스
  - RDS 배포 (AWS)
  - 20+ 엔티티
  - 복잡한 관계 (1:N, M:N, 자기 참조)
  - **학습 깊이**: 95% (인덱싱, 쿼리 최적화, 트랜잭션)
  
- **Entity Framework Core 9.0**
  - Code-First 접근
  - Fluent API Configuration
  - Migration 관리
  - LINQ 쿼리
  - 트랜잭션 지원
  - **학습 깊이**: 95% (복잡한 관계, 성능 최적화)

### 인증 & 보안
- **JWT Bearer Token**
  - Access Token (15분)
  - Refresh Token (7일)
  - Token Rotation
  - **학습 깊이**: 90%
  
- **BCrypt.Net-Next**
  - 비밀번호 해싱
  - Salt 자동 생성

### 캐싱 & 성능
- **StackExchange.Redis**
  - 분산 캐싱
  - 세션 관리
  - **Sorted Set (랭킹 시스템)** ← 깊게 학습 (90%)
  - **분산 락 (멀티 인스턴스)** ← 깊게 학습 (90%)
  - **학습 시기**: Week 6-7

### 실시간 통신
- **SignalR**
  - **실시간 채팅 (전체, 길드, 귓속말)** ← 깊게 학습 (90%)
  - WebSocket 기반
  - 그룹 관리
  - Connection 관리
  - **학습 시기**: Week 4-5

### 패턴 & 아키텍처
- **MediatR 12.0+**
  - CQRS 패턴
  - Command/Query 분리
  - Pipeline Behaviors
  - **학습 깊이**: 70% (기본 패턴, 간단한 Behavior)
  
- **Repository Pattern**
  - 데이터 액세스 추상화
  - Unit of Work
  - **학습 깊이**: 95%

### 유효성 검증 & 매핑
- **FluentValidation 11.0+**
  - DTO 유효성 검증
  - Custom Validators
  - 에러 메시지 커스터마이징
  - **학습 깊이**: 70%
  
- **AutoMapper 12.0+**
  - Entity ↔ DTO 매핑
  - Projection
  - Value Converters
  - **학습 깊이**: 60% (기본 매핑만)

### 로깅 & 모니터링
- **Serilog** ⭐ **첫날부터 필수!**
  - 구조화된 로깅 (Structured Logging)
  - File/Console Sink
  - 로그 레벨 관리
  - Request 로깅
  - **학습 깊이**: 80%
  - **중요성**: 디버깅 시간 극적 단축, 학습 프로젝트에 필수

- **Application Insights** ❌ **제외**
  - 프로덕션 모니터링 (개념만 이해)

### Background Services
- **IHostedService / BackgroundService**
  - 자동 사냥 시스템
  - 일일 미션 리셋
  - 랭킹 갱신
  - 이벤트 스케줄링
  - **학습 깊이**: 90%

### 테스팅
- **xUnit 2.5+**
  - 단위 테스트 프레임워크
  - Theory/InlineData
  - AAA 패턴
  - **학습 깊이**: 85%
  
- **Moq 4.20+**
  - Mock 객체 생성
  - Repository Mocking
  - Service Mocking
  - **학습 깊이**: 80%
  
- **FluentAssertions 6.0+**
  - 가독성 높은 Assertion
  - Should() 문법
  - **학습 깊이**: 80%

### API 문서화
- **Swashbuckle.AspNetCore 6.5+**
  - Swagger UI
  - OpenAPI 3.0
  - XML 주석 지원
  - Bearer Token 인증 UI
  - **학습 깊이**: 70%

### 기타 NuGet Packages
- **Newtonsoft.Json** - JSON 직렬화
- **Npgsql.EntityFrameworkCore.PostgreSQL** - PostgreSQL Provider
- **Microsoft.Extensions.Caching.StackExchangeRedis** - Redis 캐싱

---

## 클라이언트 (Unity)

### Unity Engine
- **Unity 6 (6000.0.59f2)**
  - Latest stable release (2024)
  - C# 10
  - .NET Standard 2.1
  - IL2CPP 빌드
  - **학습 깊이**: 70% (네트워크 통신 위주)

### 비동기 처리
- **UniTask**
  - async/await (Unity용)
  - CancellationToken
  - 성능 최적화된 Task
  - **학습 깊이**: 80%

### 네트워크 통신
- **UnityWebRequest**
  - HTTP 요청
  - Bearer Token 헤더
  - JSON 통신
  - **학습 깊이**: 85%
  
- **Microsoft.AspNetCore.SignalR.Client**
  - SignalR 클라이언트
  - 실시간 채팅 연동
  - 자동 재연결
  - **학습 깊이**: 90% (Week 4-5 집중)

### JSON 처리
- **Newtonsoft.Json for Unity**
  - JSON 직렬화/역직렬화
  - DTO 매핑
  - [Serializable] 지원
  - **학습 깊이**: 80%

### UI & UX
- **TextMeshPro**
  - 고품질 텍스트 렌더링
  - 다국어 지원
  - Unity 6에서 기본 통합
  - **학습 깊이**: 60%

- **DOTween** (선택)
  - UI 애니메이션
  - Tween 시스템
  - **학습 깊이**: 50%

### 리소스 관리
- **Addressables** (선택)
  - 동적 리소스 로딩
  - **학습 깊이**: 개념만 (40%)

### 테스팅
- **Unity Test Framework**
  - PlayMode Tests
  - EditMode Tests
  - **학습 깊이**: 60%

---

## 인프라 & DevOps

### 클라우드 (AWS)
- **EC2 t3.micro**
  - Ubuntu 22.04 LTS
  - Docker 런타임
  - **학습 깊이**: 70%
  
- **RDS PostgreSQL**
  - Multi-AZ 배포 (개념만)
  - 자동 백업
  - **학습 깊이**: 60%

### CI/CD
- **Jenkins**
  - EC2 자체 호스팅
  - GitHub Webhook 연동
  - 파이프라인: Git Pull → Migration → Docker Deploy
  - **학습 깊이**: 70%

### 컨테이너화
- **Docker 24.0+**
  - 멀티 스테이지 빌드
  - .NET 8 런타임
  - **학습 깊이**: 75%
  
- **Docker Compose**
  - 로컬 개발 환경
  - PostgreSQL + Redis + pgAdmin
  - **학습 깊이**: 80%

---

## 🎓 8주 학습 로드맵 (Phase 기반)

### Phase 1: Core Vertical Slice (Week 1-3) - 6개 시스템, 95% 깊이
**필수 기술:**
- ✅ ASP.NET Core Web API (RESTful)
- ✅ EF Core (Code-First, Migration, 복잡한 관계)
- ✅ PostgreSQL (인덱싱, 쿼리 최적화)
- ✅ JWT 인증
- ⭐ **Serilog** (첫날부터!)
- ✅ xUnit + Moq (단위 테스트)
- ✅ IHostedService (Background Service)
- ✅ Repository Pattern + Unit of Work

**시스템:**
1. Authentication (JWT)
2. Character Growth
3. Combat System (Auto-battle)
4. Inventory & Equipment
5. Offline Rewards
6. Dungeon System

**학습 목표:**
- 완전한 게임 루프 (로그인 → 전투 → 성장) 구현
- Clean Architecture 계층 구조 체득
- 디버깅을 위한 Serilog 활용

---

### Phase 2: Core Features + SignalR (Week 4-5) - 4개 시스템, SignalR 90% 깊이
**필수 기술:**
- ⭐ **SignalR** (90% 깊이 - 실시간 채팅, WebSocket, 그룹 관리)
- Unity SignalR Client (Unity 통합)
- MediatR (CQRS 기본 - 70%)
- FluentValidation (DTO 검증 - 70%)
- AutoMapper (기본 매핑 - 60%)

**시스템:**
7. Equipment Enhancement
8. Skill System
9. Pet System
10. **Real-time Chat (SignalR)** ← 깊게 학습

**학습 목표:**
- SignalR 실시간 통신 마스터
- Unity ↔ Server SignalR 연동
- CQRS 패턴 이해

---

### Phase 3: Social + Redis (Week 6-7) - 4개 시스템, Redis 90% 깊이
**필수 기술:**
- ⭐ **Redis** (90% 깊이 - Sorted Set 랭킹, 분산 락, 캐싱)
- StackExchange.Redis
- 동시성 처리 (Race condition 방지)

**시스템:**
11. Friend System
12. Guild System (Basic + Raid)
13. **Ranking System (Redis Sorted Set)** ← 깊게 학습
14. Boss Raid (Cooperative)

**학습 목표:**
- Redis Sorted Set으로 랭킹 시스템 구현
- 분산 락으로 멀티 인스턴스 동시성 처리
- Guild Raid 협력 메커니즘

---

### Phase 4: Monetization (Week 8) - 6개 시스템, 50-70% 깊이
**필수 기술:**
- 확률 로직 (가챠 - 70%)
- 이벤트 스케줄링 (60%)

**시스템:**
15. Quest & Achievement (70%)
16. Daily Mission & Attendance (70%)
17. Gacha System (70% - 확률 로직)
18. Shop & VIP (60%)
19. Mail System (60%)
20. Event System (50%)

**학습 목표:**
- 빠른 구현으로 넓은 시스템 커버
- 가챠 확률 로직 이해
- 이벤트 스케줄링 개념

---

## 🚫 명시적 제외 항목 (Out of Scope)

### 플랫폼 연동 (학습 외적 작업 소모)
- ❌ **IAP (In-App Purchase)** - Apple/Google 플랫폼 설정, 정책 학습 시간 과다
- ❌ **FCM (Push Notification)** - Firebase 설정, 플랫폼별 인증서 작업
- ❌ **Firebase Analytics** - 플랫폼 연동 설정 시간

### 프로덕션 운영 (개념만)
- ❌ **Application Insights** - 실제 모니터링 대신 Serilog 로그 분석으로 대체
- ❌ **CDN** - 개념 이해만
- ❌ **Load Balancer** - 개념 이해만
- ❌ **Multi-AZ RDS** - 개념 이해만

### 고급 Unity 기능 (최소화)
- ⚠️ **Addressables** - 개념만 (40%)
- ⚠️ **DOTween** - 선택 사항

---

## 🎯 기술 선택 이유 (학습 관점)

### Serilog - 왜 첫날부터?
- **디버깅 시간 단축**: 구조화된 로그로 문제 추적 시간 80% 감소
- **학습 가속화**: 에러 원인을 빠르게 파악 → 더 많은 기능 학습 가능
- **실무 필수 스킬**: 프로덕션 환경에서 로깅 없이 운영 불가능

### SignalR - 왜 깊게?
- **실시간 통신 핵심**: 채팅, 알림, 실시간 이벤트의 기초
- **Unity 통합**: .NET 네이티브 지원으로 학습 곡선 완만
- **차별화 기술**: WebSocket 이해는 실무에서 고급 스킬

### Redis - 왜 깊게?
- **랭킹 시스템 필수**: Sorted Set은 랭킹의 사실상 표준
- **분산 락**: 멀티 인스턴스 환경 이해 (확장성 학습)
- **범용 기술**: 캐싱, 세션, 메시지 큐 등 다양한 용도

### PostgreSQL vs MySQL
- **복잡한 쿼리 학습**: PostgreSQL이 더 강력한 쿼리 기능 제공
- **JSON 지원**: NoSQL처럼 유연한 데이터 처리 가능
- **확장성**: PostGIS 등 다양한 확장 모듈

### IAP/FCM 제외 이유
- **학습 목표와 무관**: 플랫폼별 설정은 기술 이해가 아닌 행정 작업
- **시간 대비 효율 낮음**: Apple Developer, Google Play Console 설정에 2-3일 소모
- **대체 가능**: 개념 이해만으로 충분, 실제 연동은 프로덕션에서 진행

---

## 📊 Definition of Done (80% 완성 기준)

### 시스템별 완성도
- **95% 시스템** (6개): 단위 테스트 90%+, 엣지 케이스 처리, 성능 최적화
- **70-80% 시스템** (8개): 핵심 기능 구현, 단위 테스트 60%+, 기본 에러 처리
- **50-60% 시스템** (6개): 기본 CRUD, 개념 이해, 간단한 테스트

### Out of Scope
- 완벽한 UI/UX (Unity는 최소 기능만)
- 프로덕션 최적화 (n+1 쿼리는 이해만)
- 100% 테스트 커버리지 (핵심 로직만)

---

**최종 업데이트**: 2025-10-16 (Gemini 2.5 Pro 검증 완료)  
**학습 기간**: 2개월 (8주)  
**학습 전략**: T-shaped (6개 95% + 14개 60-80%)  
**AI 협업**: Claude Code + Gemini + GPT  
**핵심 원칙**: Serilog 첫날, SignalR/Redis 깊게, IAP/FCM 제외
