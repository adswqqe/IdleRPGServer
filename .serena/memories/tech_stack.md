# 🍄 IdleRPG Server 기술 스택 (완전판)

## 서버 (.NET)

### Core Framework
- **ASP.NET Core 8.0 Web API**
  - RESTful API 설계
  - Minimal API (필요 시)
  - HTTPS 지원
  - CORS 설정

### Database & ORM
- **PostgreSQL 14+**
  - 관계형 데이터베이스
  - RDS 배포 (AWS)
  - 20+ 엔티티
  - 복잡한 관계 (1:N, M:N, 자기 참조)
  
- **Entity Framework Core 9.0**
  - Code-First 접근
  - Fluent API Configuration
  - Migration 관리
  - LINQ 쿼리
  - 트랜잭션 지원

### 인증 & 보안
- **JWT Bearer Token**
  - Access Token (15분)
  - Refresh Token (7일)
  - Token Rotation
  
- **BCrypt.Net-Next**
  - 비밀번호 해싱
  - Salt 자동 생성

### 캐싱 & 성능
- **StackExchange.Redis**
  - 분산 캐싱
  - 세션 관리
  - Sorted Set (랭킹 시스템)
  - 분산 락 (멀티 인스턴스)

### 실시간 통신
- **SignalR**
  - 실시간 채팅 (전체, 길드, 귓속말)
  - WebSocket 기반
  - 그룹 관리
  - Connection 관리

### 패턴 & 아키텍처
- **MediatR 12.0+**
  - CQRS 패턴
  - Command/Query 분리
  - Pipeline Behaviors
  
- **Repository Pattern**
  - 데이터 액세스 추상화
  - Unit of Work

### 유효성 검증 & 매핑
- **FluentValidation 11.0+**
  - DTO 유효성 검증
  - Custom Validators
  - 에러 메시지 커스터마이징
  
- **AutoMapper 12.0+**
  - Entity ↔ DTO 매핑
  - Projection
  - Value Converters

### 로깅 & 모니터링
- **Serilog**
  - 구조화된 로깅
  - File/Console Sink
  - 로그 레벨 관리
  - Request 로깅

- **Application Insights** (선택)
  - 성능 모니터링
  - 에러 트래킹
  - 사용자 분석

### Background Services
- **IHostedService / BackgroundService**
  - 자동 사냥 시스템
  - 일일 미션 리셋
  - 랭킹 갱신
  - 이벤트 스케줄링

### 테스팅
- **xUnit 2.5+**
  - 단위 테스트 프레임워크
  - Theory/InlineData
  - AAA 패턴
  
- **Moq 4.20+**
  - Mock 객체 생성
  - Repository Mocking
  - Service Mocking
  
- **FluentAssertions 6.0+**
  - 가독성 높은 Assertion
  - Should() 문법

### API 문서화
- **Swashbuckle.AspNetCore 6.5+**
  - Swagger UI
  - OpenAPI 3.0
  - XML 주석 지원
  - Bearer Token 인증 UI

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
  - Entities 1.0 지원

### Unity 6 주요 개선사항
- **렌더링**: 향상된 URP (Universal Render Pipeline)
- **성능**: GPU Resident Drawer (대량 오브젝트)
- **Multiplayer**: Netcode for GameObjects 통합
- **AI**: ML-Agents 2.0 네이티브 지원
- **Web**: WebGPU 지원

### 비동기 처리
- **UniTask**
  - async/await (Unity용)
  - CancellationToken
  - 성능 최적화된 Task

### 네트워크 통신
- **UnityWebRequest**
  - HTTP 요청
  - Bearer Token 헤더
  - JSON 통신
  
- **Microsoft.AspNetCore.SignalR.Client**
  - SignalR 클라이언트
  - 실시간 채팅 연동
  - 자동 재연결

### JSON 처리
- **Newtonsoft.Json for Unity**
  - JSON 직렬화/역직렬화
  - DTO 매핑
  - [Serializable] 지원

### UI & UX
- **TextMeshPro**
  - 고품질 텍스트 렌더링
  - 다국어 지원
  - 이모지 지원
  - Unity 6에서 기본 통합

- **DOTween** (선택)
  - UI 애니메이션
  - Tween 시스템

### 리소스 관리
- **Addressables**
  - 동적 리소스 로딩
  - 메모리 최적화
  - 업데이트 지원

### 테스팅
- **Unity Test Framework**
  - PlayMode Tests
  - EditMode Tests
  - Integration Tests

---

## 인프라 & DevOps

### 클라우드 (AWS)
- **EC2 t3.micro**
  - Ubuntu 22.04 LTS
  - Docker 런타임
  - Nginx Reverse Proxy (선택)
  
- **RDS PostgreSQL**
  - Multi-AZ 배포 (Production)
  - 자동 백업
  - ap-northeast-2 (서울 리전)

### CI/CD
- **Jenkins**
  - EC2 자체 호스팅
  - GitHub Webhook 연동
  - 파이프라인:
    1. Git Pull
    2. Database Migration (psql)
    3. Docker Build & Deploy
    4. Verification
  
- **GitHub Actions** (미래 전환 고려)
  - 관리형 서비스
  - 더 나은 스케일링

### 컨테이너화
- **Docker 24.0+**
  - 멀티 스테이지 빌드
  - .NET 8 런타임
  
- **Docker Compose**
  - 로컬 개발 환경
  - PostgreSQL + Redis + pgAdmin
  - 네트워크 구성

### 버전 관리
- **Git**
  - GitHub 원격 저장소
  - Feature Branch 전략
  - Conventional Commits

---

## 개발 도구

### IDE
- **Visual Studio 2022** (Windows)
  - .NET 8 SDK
  - ReSharper (선택)
  
- **JetBrains Rider** (선택)
  - Cross-platform
  - Unity 통합

- **Visual Studio Code**
  - 가벼운 편집
  - C# Extension

### Database Tools
- **pgAdmin 4**
  - PostgreSQL GUI
  - 쿼리 실행
  - 스키마 관리
  
- **DBeaver** (선택)
  - Universal DB Tool

### API Testing
- **Postman**
  - API 테스팅
  - Collection 관리
  - Environment 변수
  
- **Swagger UI**
  - 통합 API 문서
  - Try-it-out 기능

### Redis Management
- **RedisInsight**
  - Redis GUI
  - Key 조회/수정
  - 성능 모니터링

---

## 학습 로드맵 (기술별)

### Phase 1 (Week 1-6): Foundation
- ✅ ASP.NET Core Web API
- ✅ EF Core (Migrations, LINQ)
- ✅ JWT 인증
- ⏳ PostgreSQL (고급 쿼리)
- ⏳ xUnit 단위 테스트
- 📋 IHostedService
- 📋 FluentValidation
- 📋 AutoMapper

### Phase 2 (Week 7-12): Expansion
- 📋 MediatR (CQRS)
- 📋 Redis (캐싱)
- 📋 Serilog (로깅)
- 📋 복잡한 데이터 관계
- 📋 트랜잭션 관리
- 📋 동시성 처리

### Phase 3 (Week 13-18): Advanced
- 📋 SignalR (실시간 통신)
- 📋 Redis (랭킹, 분산 락)
- 📋 Unity SignalR Client
- 📋 성능 최적화
- 📋 보안 강화

### Phase 4 (Week 19-20): Production
- 📋 Application Insights
- 📋 성능 모니터링
- 📋 로그 분석
- 📋 배포 자동화 최적화

---

## 기술 선택 이유

### Unity 6 선택 이유
- **최신 안정 버전**: 2024년 릴리스, 장기 지원
- **성능 개선**: GPU Resident Drawer로 대량 오브젝트 렌더링 최적화
- **WebGPU 지원**: 웹 빌드 성능 향상
- **향상된 URP**: 모바일 최적화
- **C# 10 지원**: 최신 언어 기능

### PostgreSQL vs MySQL
- **PostgreSQL 선택**
  - 더 강력한 JSON 지원
  - 복잡한 쿼리 성능
  - 확장성 (PostGIS 등)

### SignalR vs Socket.IO
- **SignalR 선택**
  - .NET 네이티브 통합
  - 자동 재연결
  - Unity Client 존재

### Redis vs Memcached
- **Redis 선택**
  - Sorted Set (랭킹 시스템)
  - 다양한 데이터 구조
  - Pub/Sub 지원

### MediatR vs 직접 구현
- **MediatR 선택**
  - CQRS 패턴 표준화
  - Pipeline Behaviors
  - 코드 구조화

### xUnit vs NUnit
- **xUnit 선택**
  - 최신 .NET 표준
  - Parallel 실행
  - Theory/InlineData

---

## 프로덕션 준비 (Week 19-20)

### 필수 항목
- [ ] HTTPS 인증서 (Let's Encrypt)
- [ ] 환경 변수 분리 (Development/Production)
- [ ] 에러 처리 미들웨어
- [ ] Rate Limiting (DDoS 방지)
- [ ] CORS 정책
- [ ] DB 인덱스 최적화
- [ ] 로그 레벨 조정 (Production: Warning+)

### 권장 항목
- [ ] CDN (정적 리소스)
- [ ] Load Balancer (다중 인스턴스)
- [ ] 모니터링 대시보드
- [ ] 자동 스케일링
- [ ] 데이터베이스 복제

---

**최종 업데이트**: 2025-10-15
**Unity 버전**: 6 (6000.0.59f2)
**대상 프로젝트**: 버섯키우기 완전판 (20주 로드맵)
