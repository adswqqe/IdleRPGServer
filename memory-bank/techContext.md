# Tech Context

## Core Technologies Used

### Backend Framework
- **ASP.NET Core 8.0**: Web API 프레임워크
- **C# .NET 8**: 개발 언어
- **Swagger/OpenAPI**: API 문서화

### Database & ORM
- **PostgreSQL**: 주 데이터베이스
- **Entity Framework Core 9.0**: ORM 및 데이터 액세스
- **EF Core Migrations**: 데이터베이스 스키마 관리

### Caching & Performance
- **Redis**: 대기 상태 (추후 사용 예정)
- **In-Memory Caching**: ASP.NET Core 내장

### Architecture & Patterns
- **Clean Architecture**: 레이어 분리 아키텍처
- **CQRS with MediatR**: 명령/조회 분리 패턴
- **Repository Pattern**: 데이터 액세스 추상화
- **Dependency Injection**: ASP.NET Core DI 컨테이너

### Validation & Mapping
- **FluentValidation**: 입력 데이터 유효성 검사
- **AutoMapper**: 도메인-DTO 간 매핑

### Logging & Monitoring
- **Serilog**: 구조화된 로깅
- **ASP.NET Core Logging**: 기본 로깅 인프라

### Development & DevOps
- **Docker & Docker Compose**: 컨테이너화 및 로컬 개발 환경
- **pgAdmin**: PostgreSQL 데이터베이스 관리 도구

## Development Setup

### 필수 설치 도구
- **.NET 8.0 SDK**: 개발 프레임워크
- **Docker & Docker Compose**: 로컬 개발 환경
- **JetBrains Rider / Visual Studio**: IDE 옵션

### 로컬 개발 환경 시작
```bash
# 데이터베이스 및 도구 시작
./dev-start.sh

# API 서버 실행
cd IdleRPG.API
dotnet run
```

### 접속 정보
- **API Server**: http://localhost:5172, https://localhost:7122
- **Swagger UI**: http://localhost:5172/swagger
- **pgAdmin**: http://localhost:8082 (admin@idlerpg.com / admin123)
- **PostgreSQL**: localhost:5432 (gamedev / dev123!)
- **Redis**: localhost:6379

### 마이그레이션 관리
```bash
# 마이그레이션 생성
cd IdleRPG.Infrastructure
dotnet ef migrations add <MigrationName> --startup-project ../IdleRPG.API

# 마이그레이션 적용
dotnet ef database update --startup-project ../IdleRPG.API
```

## Key Dependencies

### Core NuGet Packages

#### API 레이어
- `Microsoft.AspNetCore.OpenApi`
- `Swashbuckle.AspNetCore`
- `Serilog.AspNetCore`

#### Application 레이어
- `MediatR`: CQRS 패턴 구현
- `FluentValidation.AspNetCore`: 입력 유효성 검사
- `AutoMapper.Extensions.Microsoft.DependencyInjection`: 객체 매핑

#### Infrastructure 레이어
- `Microsoft.EntityFrameworkCore`: ORM 프레임워크
- `Microsoft.EntityFrameworkCore.Design`: EF 디자인 도구
- `Npgsql.EntityFrameworkCore.PostgreSQL`: PostgreSQL 프로바이더
- `Microsoft.EntityFrameworkCore.Tools`: EF 마이그레이션 도구

#### 미래 계획 패키지
- `Microsoft.Extensions.Caching.Redis`: Redis 캐싱
- `Microsoft.AspNetCore.Authentication.JwtBearer`: JWT 인증
- `SignalR`: 리얼타임 통신

## Performance Considerations

### 데이터베이스 최적화
- **Connection Pooling**: EF Core 기본 커넥션 풀링
- **Query Optimization**: LINQ 쿼리 최적화 예정
- **Indexing Strategy**: 주요 쿼리 경로 인덱싱

### 캐싱 전략
- **Level 1**: In-Memory Caching (ASP.NET Core)
- **Level 2**: Redis 분산 캐싱 (계획 중)
- **Level 3**: 데이터베이스 쿼리 최적화

### 스케일링 고려사항
- **Horizontal Scaling**: 상태비저장 API 디자인
- **Database Scaling**: Read Replicas 및 샤딩 고려
- **Background Jobs**: Hangfire 또는 Quartz.NET 도입 계획
