# 기술 스택 및 주요 라이브러리

## 핵심 프레임워크
- **ASP.NET Core 8.0**: Web API 프레임워크
- **.NET 8.0**: 런타임 환경
- **Entity Framework Core 9.0**: ORM (PostgreSQL 연결)

## 데이터베이스
- **PostgreSQL 16**: 주 데이터베이스
- **Redis**: 캐싱 (준비됨, 향후 사용)

## 인증 및 보안
- **JWT Bearer Authentication**: JSON Web Token 기반 인증
- **BCrypt.Net-Next 4.0.3**: 비밀번호 해싱
- **System.IdentityModel.Tokens.Jwt 8.14.0**: JWT 토큰 처리

## 아키텍처 패턴 라이브러리
- **MediatR**: CQRS 패턴 구현 (설정됨, 사용 준비)
- **AutoMapper**: 객체 매핑 (설정됨, 사용 준비)
- **FluentValidation**: 입력 유효성 검증 (설정됨, 사용 준비)

## 로깅 및 문서화
- **Serilog.AspNetCore 9.0.0**: 구조화된 로깅
- **Swashbuckle.AspNetCore 6.4.0**: OpenAPI/Swagger 문서
- **Microsoft.AspNetCore.OpenApi 8.0.1**: OpenAPI 지원

## 개발 도구
- **Docker & Docker Compose**: 개발 환경 컨테이너화
- **pgAdmin**: PostgreSQL 관리 도구

## 프로젝트 종속성 구조
```
API → Application → Domain
Infrastructure → Application (인터페이스 구현)
```

## NuGet 패키지 (주요)
- `Microsoft.EntityFrameworkCore.Design 9.0.9`
- `Microsoft.AspNetCore.Authentication.JwtBearer 8.0.20`
- `AspNetCoreModules 0.2.0`