# IdleRPG Server 프로젝트 체크포인트 (2025-10-12)

## 프로젝트 개요
- **프로젝트명**: IdleRPG Server
- **아키텍처**: ASP.NET Core 8.0 Clean Architecture
- **배포 아키텍처**: Monolithic (API + Background Services 통합)
- **목표 동접**: 10,000명 (확장 가능: ~50,000명)
- **데이터베이스**: PostgreSQL with EF Core 9.0
- **캐싱**: Redis (분산 락, 세션 관리)
- **인증**: JWT Bearer Token
- **패턴**: CQRS with MediatR, Repository Pattern

### 아키텍처 결정 사항
- **동접 1만명 규모에서는 API + 스케줄러 통합이 적합**
- 비용 효율성: 통합 $120/월 vs 분리 $150/월 (20% 절감)
- IHostedService 또는 Hangfire로 백그라운드 작업 처리
- 수평 확장(Load Balancer)으로 고가용성 확보
- Redis 분산 락으로 여러 인스턴스 간 스케줄러 중복 방지
- 상세 내용: **ARCHITECTURE-GUIDE.md** 참조

## 완료된 기능 (Week 0-1)

### 인증 시스템 (100% 완료)
**Controller**: `IdleRPG.API/Controllers/AuthController.cs`

구현된 엔드포인트:
1. `POST /api/auth/register` - 회원가입
2. `POST /api/auth/login` - 로그인
3. `POST /api/auth/refresh` - 토큰 갱신
4. `POST /api/auth/logout` - 로그아웃
5. `GET /api/auth/profile` - 프로필 조회

**특징**:
- BCrypt 패스워드 해싱
- Access Token (15분) + Refresh Token (7일)
- ClaimTypes.NameIdentifier로 사용자 ID 관리

### 캐릭터 시스템 (100% 완료)
**Controller**: `IdleRPG.API/Controllers/CharacterController.cs`
**Service**: `IdleRPG.Application/Character/Services/CharacterService.cs`
**Repository**: `IdleRPG.Infrastructure/Repositories/CharacterRepository.cs`
**Entity**: `IdleRPG.Domain/Entities/Character.cs`
**Tests**: `IdleRPG.Tests/CharacterServiceTests.cs` (17 unit tests)

구현된 엔드포인트:
1. `POST /api/character/Create` - 캐릭터 생성
2. `GET /api/character/GetCharacters` - 전체 목록 조회
3. `GET /api/character/{characterId}` - 특정 캐릭터 조회
4. `DELETE /api/character/{characterId}` - 캐릭터 삭제
5. `POST /api/character/{characterId}/experience` - 경험치 획득
6. `PUT /api/character/{characterId}/stats` - 스탯 분배

**비즈니스 로직**:
- 레벨업 공식: 필요 경험치 = Level * 100
- 레벨업 시 스탯 포인트 +5
- 초기 스탯: Str/Dex/Int/Vit 각 10
- 플레이어당 캐릭터 최대 3개 제한

**Value Object**: CharacterStats
- Strength, Dexterity, Intelligence, Vitality
- 불변 객체로 구현

## 데이터베이스 마이그레이션
- `InitialCreate` - Player 테이블
- `AddCharacterEntity` - Character 테이블

## Unity 문서 동기화 상태
**문서 위치**: `../IdleRPGClient/Docs/unity/API_SPEC_FOR_UNITY.md`

**최근 업데이트 (2025-10-12)**:
- ✅ 모든 API 엔드포인트 구현 상태 ✅로 업데이트
- ✅ logout, profile 엔드포인트 추가 문서화
- ✅ 경로 수정: `/api/character` → `/api/character/Create`
- ✅ 경로 수정: `/api/character` → `/api/character/GetCharacters`
- ✅ 구현 상태 테이블 추가 (✅ 완료 / 🚧 개발 중 / 📋 예정)

## 다음 단계 (Week 2)
**주제**: Idle Game Loop & Progression

예정 기능:
1. 자동 전투 시스템
2. 경험치/골드 자동 획득
3. 오프라인 보상 계산
4. 전투 로그 시스템

## 프로젝트 구조 참고

```
IdleRPGServer/
├── IdleRPG.API/
│   └── Controllers/
│       ├── AuthController.cs (완료)
│       └── CharacterController.cs (완료)
├── IdleRPG.Application/
│   ├── Auth/Services/AuthService.cs (완료)
│   ├── Character/Services/CharacterService.cs (완료)
│   └── DTOs/
│       ├── Auth/ (완료)
│       └── Characters/ (완료)
├── IdleRPG.Domain/
│   └── Entities/
│       ├── Player.cs (완료)
│       └── Character.cs (완료)
├── IdleRPG.Infrastructure/
│   ├── Data/ApplicationDbContext.cs (완료)
│   └── Repositories/
│       ├── PlayerRepository.cs (완료)
│       └── CharacterRepository.cs (완료)
└── IdleRPG.Tests/
    └── CharacterServiceTests.cs (완료 - 17 tests)
```

## 중요 참고사항

### GetCurrentUserId() 패턴
모든 인증이 필요한 Controller에서 사용:
```csharp
private Guid GetCurrentUserId()
{
    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    return Guid.Parse(userIdClaim);
}
```

### 에러 처리 패턴
Controller에서 일관된 에러 처리:
```csharp
try
{
    var response = await _service.MethodAsync();
    return Ok(new { response });
}
catch (InvalidOperationException ex)
{
    return BadRequest(new { message = ex.Message });
}
```

## 개발 환경
- Docker Compose로 PostgreSQL, Redis, pgAdmin 실행
- `./dev-start.sh` - 개발 환경 시작
- `docker-compose down` - 환경 종료
- `dotnet test` - 전체 테스트 실행 (17 unit tests)
- Swagger UI: http://localhost:5172/swagger

## 마지막 업데이트
- **날짜**: 2025-10-12
- **작업자**: Claude Code
- **커밋 상태**: Week 1 character system complete with unit tests (17 passing)
