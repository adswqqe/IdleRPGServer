# System Patterns

## System Architecture

### Clean Architecture 구조
프로젝트는 Clean Architecture 원칙을 따라 4개 레이어로 구성됩니다:

1. **IdleRPG.API** (Presentation Layer)
   - Web API Controllers
   - Middleware 및 Configuration
   - Swagger/OpenAPI 문서화
   - 의존성: Application → Domain

2. **IdleRPG.Application** (Application Layer)
   - Business Logic Services
   - CQRS Commands/Queries (MediatR)
   - DTOs (Data Transfer Objects)
   - FluentValidation 규칙
   - AutoMapper 프로필
   - 의존성: Domain만 참조

3. **IdleRPG.Domain** (Core Domain Layer)
   - Entity 모델 (Player, Character, Inventory 등)
   - Repository 인터페이스
   - 비즈니스 규칙 및 도메인 로직
   - 의존성: 없음 (순수 도메인)

4. **IdleRPG.Infrastructure** (Infrastructure Layer)
   - Entity Framework Core DbContext
   - Repository 구현체
   - 데이터베이스 마이그레이션
   - 외부 서비스 통합
   - 의존성: Application, Domain

### 의존성 방향
```
API → Application → Domain
         ↑
   Infrastructure
```

## Key Technical Decisions

### 1. CQRS with MediatR
- Command와 Query 분리로 읽기/쓰기 최적화
- MediatR를 통한 Request/Response 패턴
- 비즈니스 로직의 명확한 분리

### 2. Repository Pattern
- 데이터 액세스 추상화
- 단위 테스트 용이성
- 인터페이스: `IRepository<T>`, `IPlayerRepository`, `ICharacterRepository`
- 구현체: Infrastructure 레이어에서 EF Core 기반 구현

### 3. Entity Framework Core
- Code-First 접근 방식
- PostgreSQL 데이터베이스
- Migration 기반 스키마 관리

### 4. Dependency Injection
- ASP.NET Core 내장 DI 컨테이너 활용
- 인터페이스 기반 의존성 주입
- 레이어 간 느슨한 결합

## Design Patterns in Use

### 1. Repository Pattern
- **위치**: Domain/Repositories (인터페이스), Infrastructure/Repositories (구현)
- **목적**: 데이터 액세스 로직 캡슐화
- **주요 인터페이스**:
  - `IRepository<T>`: 제네릭 CRUD 작업
  - `IPlayerRepository`: 플레이어 특화 작업
  - `ICharacterRepository`: 캐릭터 특화 작업

### 2. CQRS (Command Query Responsibility Segregation)
- **구현**: MediatR 라이브러리
- **Command**: 데이터 변경 작업
- **Query**: 데이터 조회 작업
- **Handler**: 각 Command/Query별 처리 로직

### 3. DTO (Data Transfer Object) Pattern
- **목적**: 레이어 간 데이터 전송
- **매핑**: AutoMapper 활용
- **검증**: FluentValidation 통합

### 4. Dependency Injection Pattern
- **컨테이너**: ASP.NET Core 내장 DI
- **등록**: Program.cs에서 서비스 등록
- **해결**: 생성자 주입 방식

### 5. Entity Pattern
- **Base Entity**: `BaseEntity` 클래스로 공통 속성 관리
- **Domain Entities**: Player, Character, PlayerStats, PlayerInventory 등
- **관계 매핑**: EF Core Fluent API 활용

## 프로젝트 현재 상태

### 완료된 구성 요소
1. ✅ Clean Architecture 프로젝트 구조
2. ✅ Entity Framework Core 설정
3. ✅ PostgreSQL 연결 구성
4. ✅ 기본 Domain Entities 정의
5. ✅ Repository 인터페이스 정의
6. ✅ 초기 마이그레이션 생성
7. ✅ Docker 개발 환경 구성

### 개발 예정 구성 요소
1. 🔄 MediatR Commands/Queries 구현
2. 🔄 AutoMapper 프로필 설정
3. 🔄 FluentValidation 규칙 정의
4. 🔄 API Controllers 구현
5. 🔄 JWT Authentication 구현
6. 🔄 게임 비즈니스 로직 구현
