# 코드 스타일 및 컨벤션

## 프로젝트 설정
- **Target Framework**: .NET 8.0
- **Nullable**: 활성화 (`<Nullable>enable</Nullable>`)
- **Implicit Usings**: 활성화 (`<ImplicitUsings>enable</ImplicitUsings>`)
- **InvariantGlobalization**: 활성화 (국제화 최적화)

## C# 코딩 컨벤션

### 네이밍 규칙
- **클래스명**: PascalCase (예: `PlayerService`, `AuthController`)
- **메서드명**: PascalCase (예: `RegisterAsync`, `GetPlayerStats`)
- **프로퍼티명**: PascalCase (예: `UserName`, `CreatedAt`)
- **필드명**: camelCase with underscore prefix (예: `_context`, `_logger`)
- **파라미터명**: camelCase (예: `dto`, `playerId`)

### 타입 및 어노테이션
- **Nullable 타입 적극 활용**: `string?`, `Player?`
- **Data Annotations 사용**: `[Required]`, `[MaxLength(50)]`, `[EmailAddress]`
- **async/await 패턴**: 비동기 메서드는 `Async` 접미사 사용

### 파일 및 네임스페이스
- **파일명 = 클래스명**: `Player.cs`, `AuthService.cs`
- **네임스페이스**: 프로젝트 구조 반영
  - `IdleRPG.Domain.Entities`
  - `IdleRPG.Application.Services`
  - `IdleRPG.Infrastructure.Data`

## Clean Architecture 패턴

### 의존성 방향
```
API → Application → Domain
Infrastructure → Application (인터페이스만)
```

### 폴더 구조
- **Controllers**: API 엔드포인트
- **DTOs**: 데이터 전송 객체
- **Services**: 비즈니스 로직 인터페이스
- **Entities**: 도메인 엔티티
- **Repositories**: 데이터 액세스 인터페이스

## Entity Framework 컨벤션
- **Primary Key**: `Guid Id { get; set; } = Guid.NewGuid()`
- **Navigation Properties**: 적절한 `Include()` 사용
- **Context 이름**: `GameDBContext`
- **DbSet 이름**: 복수형 (예: `Players`, `Characters`)

## JWT 및 보안
- **비밀번호**: 절대 평문 저장 금지, BCrypt 해싱 필수
- **토큰**: Access Token (짧은 수명) + Refresh Token
- **Claims**: 사용자 ID, 이름 등 최소 정보만

## 로깅 패턴
- **Serilog** 구조화 로깅 사용
- **로그 레벨**: Information, Warning, Error
- **민감 정보**: 로그에 비밀번호, 토큰 등 포함 금지