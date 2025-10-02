# 프로젝트 진행 상황 (최신 업데이트: 2025-10-02)

## 현재 상태

### ✅ 완료된 작업

#### 인증 시스템
- JWT 기반 인증 시스템 구현 완료
- Player 엔티티 및 RefreshToken 엔티티
- AuthController, AuthService, JwtTokenService 구현
- 회원가입, 로그인, 토큰 갱신 API 완료
- PlayerRepository 구현 (IRepository<Player> 상속)

#### 캐릭터 시스템 (완료)
- ✅ Task 9.1: Character 엔티티 및 Value Objects 정의 완료
  - Character.cs 엔티티 생성
  - CharacterStats Value Object 정의
  - Player와 1:N 관계 설정
  
- ✅ Task 9.2: Character Repository 구현 완료
  - ICharacterRepository 인터페이스 정의 (IRepository<Character> 상속)
  - CharacterRepository 구현체 작성 (CountByPlayerIdAsync, SaveChangesAsync 포함)
  - GameDBContext에 Character DbSet 추가

- ✅ Task 9.3: 캐릭터 CRUD 서비스 및 API 구현 완료
  - **DTOs 구현**:
    - CharacterDto.cs (CharacterStats 평탄화, 응답용)
    - CreateCharacterDto.cs (현재 빈 DTO, 모든 데이터는 JWT/서버에서 생성)
  
  - **Service Pattern 구현** (MediatR 대신 Service 사용):
    - ICharacterService 인터페이스 정의 (Application 레이어)
    - CharacterService 구현 (Infrastructure 레이어)
    - 기능: Create, GetById, GetAll (플레이어별), Delete
    - 플레이어당 최대 3개 캐릭터 제한 검증
    - 초기 스탯: STR 5, DEX 5, INT 5, VIT 10
  
  - **RESTful API 구현**:
    - CharacterController 작성
    - 명시적 Route: `[Route("api/character")]` (소문자)
    - 엔드포인트:
      - POST /api/character - 캐릭터 생성
      - GET /api/character/{id} - 단건 조회
      - GET /api/character - 플레이어의 전체 캐릭터 목록
      - DELETE /api/character/{id} - 캐릭터 삭제
    - JWT 기반 인증 적용 (GetCurrentUserId() 헬퍼 메서드)

#### Repository 패턴 리팩토링 (완료)
- **IRepository<T> 공통 인터페이스**:
  - GetAllAsync(), AddAsync(), UpdateAsync(), FindAsync() 정의
  
- **PlayerRepository 구현**:
  - IRepository<Player> 상속
  - Player 전용 메서드: GetByUsername, IsUsernameAvailable 등
  - SaveChangesAsync 포함
  
- **CharacterRepository 구현**:
  - IRepository<Character> 상속
  - Character 전용 메서드: GetByPlayerId, CountByPlayerId 등
  - SaveChangesAsync 포함

- **AuthService 리팩토링**:
  - DbContext 직접 사용 → PlayerRepository 사용으로 변경
  - 모든 SaveChanges를 Repository를 통해 수행

#### API 테스트 환경
- test-api.http 파일 생성 (Rider HTTP Client용)
- 회원가입, 로그인, 캐릭터 CRUD 테스트 완료
- JWT 토큰 인증 동작 확인

#### 데이터베이스
- PostgreSQL 연동 완료
- EF Core 마이그레이션 완료
  - InitialCreate
  - AddCharacterEntity

#### 빌드 상태
- ✅ 빌드 성공 (경고 27개, 에러 0개)
- ✅ API 테스트 성공

### 📝 다음 단계

**아이템 시스템 기초 구현**
- Item 엔티티 설계 (아이템 타입, 등급, 스탯 등)
- ItemRepository 구현 (Repository 패턴)
- 아이템 데이터 시딩
- 아이템 조회 API

### 🗂️ 현재 프로젝트 구조

```
IdleRPG.Domain/
├── Entities/
│   ├── Player.cs
│   ├── Character.cs ✅
│   └── RefreshToken.cs
├── ValueObjects/
│   └── CharacterStats.cs ✅
└── Repositories/
    ├── IRepository.cs ✅
    ├── IPlayerRepository.cs ✅ (IRepository<Player> 상속)
    └── ICharacterRepository.cs ✅ (IRepository<Character> 상속)

IdleRPG.Application/
├── Auth/Services/
│   └── IAuthService.cs
├── Character/Services/
│   └── ICharacterService.cs ✅
├── DTOs/Auth/
│   ├── RegisterDto.cs
│   ├── LoginDto.cs
│   ├── RefreshTokenDto.cs
│   └── AuthResponseDto.cs
├── DTOs/Characters/
│   ├── CharacterDto.cs ✅
│   └── CreateCharacterDto.cs ✅
└── Tokens/Services/
    └── IJwtTokenService.cs

IdleRPG.Infrastructure/
├── Data/
│   └── GameDBContext.cs
├── Repositories/
│   ├── PlayerRepository.cs ✅ (IRepository<Player> 구현)
│   └── CharacterRepository.cs ✅ (IRepository<Character> 구현)
├── Service/
│   ├── AuthService.cs ✅ (PlayerRepository 사용)
│   ├── CharacterService.cs ✅
│   └── JwtTokenService.cs
└── Migrations/
    ├── InitialCreate
    └── AddCharacterEntity ✅

IdleRPG.API/
├── Controllers/
│   ├── AuthController.cs ✅ ([Route("api/auth")])
│   └── CharacterController.cs ✅ ([Route("api/character")])
└── Program.cs (DI 등록 완료)
```

### 🎯 Week 0 인프라 전략

**기본 방침**: 게임 기능 우선, 인프라는 필요할 때 추가
- Service Pattern 사용 (MediatR/CQRS는 복잡도 증가 시 도입)
- Repository Pattern으로 데이터 접근 추상화 (일관성 확보)
- Unit of Work는 DbContext가 기본 제공 (SaveChangesAsync 활용)
- Serilog, FluentValidation, AutoMapper 등은 나중에 추가
- Week 1 게임 기능(캐릭터 시스템)부터 집중
- 디버깅 어려울 때, 검증 로직 복잡해질 때 인프라 추가 고려

### 💡 개발 워크플로우

**새 기능 추가 시 순서:**
1. Domain Layer: 엔티티 정의
2. Application Layer: DTO & Service 인터페이스
3. Infrastructure Layer: Repository & Service 구현
4. API Layer: Controller 생성
5. HTTP Client로 테스트

**RESTful API 설계 원칙:**
- URL은 자원(Resource), HTTP 메서드는 행위(Action)
- 예: POST /api/character (Create 동사 X)
- 컨트롤러 메서드명은 간결하게 (Create, GetById, GetAll, Delete)
- Route는 소문자로 명시적 지정: `[Route("api/character")]`

**Repository 패턴 일관성:**
- 모든 Repository는 IRepository<T> 상속
- 공통 메서드: GetAllAsync, AddAsync, UpdateAsync, FindAsync
- 엔티티별 전용 메서드 추가
- SaveChangesAsync 포함

### 📚 참고 문서
- GDD: `.taskmaster/docs/prd.txt`
- 학습 가이드: `STAGE_1_WEB_FOUNDATIONS.md`, `STAGE_2_DATABASE_MASTERY.md`
- Task Master 태스크: `.taskmaster/tasks/tasks.json` (현재 전체 deferred 상태)
- 협업 방식: `collaboration_workflow` 메모리 참조
- API 테스트: `test-api.http` (Rider HTTP Client)