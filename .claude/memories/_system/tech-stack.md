# 기술 스택

> **참조 목적**: 새 스펙 작성 시 사용 가능한 라이브러리 및 기술 스택을 참조합니다.

---

## 서버 (.NET)

### 사용 중
- **Core**: ASP.NET Core 8.0, C# 12
- **Database**: PostgreSQL 14, EF Core 9.0
- **Auth**: JWT Bearer Token, BCrypt.Net-Next
- **Test**: xUnit, Moq, FluentAssertions
- **Logging**: Serilog (Structured Logging)
- **API Docs**: Swashbuckle.AspNetCore (Swagger)
- **Patterns**: Repository, Service, DTO, ValueObject (DDD)

### 설치됨 (대기 중)
- **MediatR 12.0+**: CQRS (Command/Query 분리) - 복잡도 증가 시 점진적 도입
- **AutoMapper 12.0+**: Entity ↔ DTO 매핑 - 매핑 코드 반복 시 사용
- **FluentValidation 11.0+**: DTO 유효성 검증 - 복잡한 검증 시 사용
- **Redis (StackExchange.Redis)**: 분산 캐싱, 랭킹 시스템 (Sorted Set)
- **SignalR**: 실시간 채팅 (전체, 길드, 귓속말)

---

## 클라이언트 (Unity)

- **Unity 6 (6000.0.59f2)**: C# 10, .NET Standard 2.1
- **UniTask**: 비동기 처리 (async/await)
- **UnityWebRequest**: HTTP 통신
- **SignalR Client**: 실시간 통신
- **Newtonsoft.Json**: JSON 직렬화/역직렬화
- **TextMeshPro**: 텍스트 렌더링

---

## 인프라 & DevOps

### 클라우드 (AWS)
- **EC2 t3.micro**: Ubuntu 22.04, Docker 런타임
- **RDS PostgreSQL**: Multi-AZ 배포
- **Jenkins**: CI/CD 자동 배포 (GitHub Webhook)

### 컨테이너화
- **Docker 24.0+**: 멀티 스테이지 빌드, .NET 8 런타임
- **Docker Compose**: 로컬 개발 (PostgreSQL + Redis + pgAdmin)

---

## 핵심 NuGet Packages

- Npgsql.EntityFrameworkCore.PostgreSQL
- Microsoft.AspNetCore.Authentication.JwtBearer
- Microsoft.Extensions.Caching.StackExchangeRedis
- Newtonsoft.Json

---

## Kiro 스펙 작성 시 참고

### Design 단계에서
- 새로운 라이브러리 도입 전 "설치됨 (대기 중)" 목록 확인
- FluentValidation: 복잡한 DTO 검증 시
- MediatR: Command/Query 분리가 필요할 때 (예: CQRS)
- Redis: 랭킹, 분산 캐싱 필요 시

### Tasks 단계에서
- NuGet 패키지 추가는 별도 Task로 분리
- Unity 호환성 확인 (Newtonsoft.Json 사용)
