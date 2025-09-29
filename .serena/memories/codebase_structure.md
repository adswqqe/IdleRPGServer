# 코드베이스 구조 상세

## 프로젝트 계층별 구조

### 1. IdleRPG.API (Web API Layer)
```
IdleRPG.API/
├── Controllers/
│   └── AuthController.cs        # 인증 관련 API (가입/로그인)
├── Properties/
│   └── launchSettings.json      # 개발 서버 설정
├── Program.cs                   # 애플리케이션 진입점, DI 설정
├── appsettings.json            # 애플리케이션 설정
├── IdleRPG.API.csproj          # 프로젝트 파일
└── Dockerfile                  # Docker 이미지 빌드
```

### 2. IdleRPG.Application (Business Logic Layer)
```
IdleRPG.Application/
├── DTOs/
│   ├── Auth/                   # 인증 관련 DTO
│   │   ├── AuthResponseDto.cs  # 인증 응답 DTO
│   │   ├── LoginDto.cs         # 로그인 요청 DTO
│   │   ├── RefreshTokenDto.cs  # 토큰 갱신 DTO
│   │   └── RegisterDto.cs      # 가입 요청 DTO
│   └── Contents/
│       └── PlayerDto.cs        # 플레이어 정보 DTO
├── Services/
│   ├── IAuthService.cs         # 인증 서비스 인터페이스
│   └── IJwtTokenService.cs     # JWT 토큰 서비스 인터페이스
└── IdleRPG.Application.csproj
```

### 3. IdleRPG.Domain (Domain Layer)
```
IdleRPG.Domain/
├── Entities/                   # 도메인 엔티티
│   ├── Character.cs            # 게임 캐릭터 엔티티
│   ├── ItemTemplate.cs         # 아이템 템플릿
│   ├── OfflineReward.cs        # 오프라인 보상
│   ├── Player.cs               # 플레이어 엔티티
│   ├── PlayerInventory.cs      # 플레이어 인벤토리
│   ├── PlayerStats.cs          # 플레이어 스탯
│   └── RefreshToken.cs         # 리프레시 토큰
├── Repositories/               # 레포지토리 인터페이스
│   ├── BaseEntity.cs           # 기본 엔티티
│   ├── ICharacterRepository.cs # 캐릭터 레포지토리 인터페이스
│   ├── IPlayerRepository.cs    # 플레이어 레포지토리 인터페이스
│   └── IRepository.cs          # 기본 레포지토리 인터페이스
└── IdleRPG.Domain.csproj
```

### 4. IdleRPG.Infrastructure (Data Access Layer)
```
IdleRPG.Infrastructure/
├── Authentication/
│   ├── JwtSettings.cs          # JWT 설정 클래스
│   └── JwtTokenService.cs      # JWT 토큰 서비스 구현
├── Data/
│   └── GameDBContext.cs        # Entity Framework DB 컨텍스트
├── Migrations/                 # EF Core 마이그레이션 파일
├── Repositories/
│   └── PlayerRepository.cs     # 플레이어 레포지토리 구현
├── Service/
│   └── AuthService.cs          # 인증 서비스 구현
└── IdleRPG.Infrastructure.csproj
```

## 핵심 파일 설명

### Program.cs (애플리케이션 구성)
- JWT 인증 설정
- Entity Framework 설정
- Dependency Injection 등록
- Swagger 설정

### GameDBContext.cs
- Entity Framework DB 컨텍스트
- 모든 엔티티의 DbSet 정의

### AuthService.cs
- 회원가입, 로그인, 토큰 갱신 로직
- BCrypt 비밀번호 해싱
- JWT 토큰 생성

## Clean Architecture 의존성 규칙
- **API** → Application (DTO, Services 인터페이스 사용)
- **Application** → Domain (엔티티 참조)
- **Infrastructure** → Application (인터페이스 구현)
- **Infrastructure** → Domain (엔티티 사용)

## 현재 구현된 기능
1. **플레이어 가입/로그인** 시스템
2. **JWT 토큰 기반 인증**
3. **Entity Framework Core** 데이터 액세스
4. **Clean Architecture** 구조