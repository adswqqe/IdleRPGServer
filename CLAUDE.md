# CLAUDE.md

## Language and Communication
**모든 응답은 한국어로 제공**. 코드와 명령어는 원문 유지.

## Project Overview
**버섯키우기 완전판** - ASP.NET Core 8.0 Clean Architecture 학습 프로젝트. 20개 게임 시스템, 8주 완성 목표.

**Architecture Layers** (Dependency: API → Application → Domain):
- **API**: Controllers, Middleware, SignalR
- **Application**: Services, DTOs (MediatR, FluentValidation, AutoMapper)
- **Domain**: Entities, Business rules
- **Infrastructure**: Repositories (EF Core + PostgreSQL)

**상세**: `docs/learning/PROJECT_ROADMAP.md`

## Quick Start
```bash
./dev-start.sh && cd IdleRPG.API && dotnet run    # Local dev
dotnet ef migrations add <Name> --startup-project ../IdleRPG.API  # Migration
```
**환경 설정**: `docs/development/DEV_ENVIRONMENT_SETUP.md`

## Critical Rules

### ⚠️ Production Deployment
**NEVER** `dotnet ef database update` on release branches. Jenkins CI/CD auto-applies migrations.
**상세**: `docs/jenkins/DEPLOYMENT_GUIDE.md`

### 🔄 Unity Documentation
**CRITICAL**: API/DTO 추가/수정 시 **반드시** Unity 문서 업데이트.

**체크리스트**:
1. `../IdleRPGClient/Docs/unity/{feature}/` 폴더 생성
2. `API_SPEC.md` 작성 (Request/Response + Unity C# 예제)
3. `DTOs.cs` 작성 (JsonProperty, Newtonsoft.Json)
4. `unity/README.md` 메인 인덱스 업데이트

**상세**: `docs/unity/UNITY_DOCUMENTATION_GUIDE.md`

## Collaboration Rules

### 🤖 Claude 자동 처리
CRUD, Repository, DTO, Configuration, EF Core Migration, Unity 문서, Swagger 주석, 단위 테스트

### 👥 함께 협업
데이터 설계, 비즈니스 로직, API 설계, 아키텍처, 성능 최적화

**프로세스**: 설계 초안 → 피드백 → 구현(핵심 로직은 TODO(human)) → 검토

### TODO(human) 규칙

**사용 시점**:
- 비즈니스 로직이 불확실할 때 (확률, 보상 계산)
- 사용자 선호도가 필요할 때 (강화 실패 시 처리)
- 디자인 결정이 필요할 때 (매칭 알고리즘)

**작성 형식**:
```csharp
// TODO(human): 크리티컬 확률 몇 %로 설정할까요?
public bool IsCritical()
{
    throw new NotImplementedException();
}
```

**제거 시점**: 사용자가 결정 후 구현 완료 시

## Development Standards

### Code Conventions
**Naming**:
- PascalCase: 클래스, 메서드, 프로퍼티 (`PlayerService`, `GetCharacterAsync`)
- camelCase: 로컬 변수, 파라미터 (`var characterId`, `string username`)
- _camelCase: Private 필드 (`private readonly IPlayerRepository _repository`)

**File Structure**:
- `Domain/Entities/{Entity}.cs`
- `Application/DTOs/{Feature}/{Feature}Dto.cs`
- `Infrastructure/Repositories/{Entity}Repository.cs`
- `API/Controllers/{Entity}Controller.cs`

**Comments**:
- XML 문서 주석: Public API 필수
- TODO(human): 사용자와 결정 필요한 비즈니스 로직

### Security Principles
- **비밀번호**: BCrypt (WorkFactor 12)
- **JWT**: Access 15분, Refresh 7일
- **SQL Injection**: EF Core 파라미터화, Raw SQL 직접 조합 금지
- **로깅**: 비밀번호, 토큰 등 민감 정보 로깅 금지

### API Design
- **RESTful**: `GET /api/characters`, `POST /api/characters`, `PUT /api/characters/{id}`, `DELETE /api/characters/{id}`
- **동작 엔드포인트**: `POST /api/characters/{id}/experience`, `POST /api/equipment/enhance`
- **HTTP 상태 코드**: 200(OK), 201(Created), 400(Bad Request), 401(Unauthorized), 404(Not Found), 500(Server Error)
- **응답 형식**: JSON DTO, 에러는 `{ "message": "...", "statusCode": 404 }`

### Error Handling
```csharp
try {
    var entity = await _service.GetAsync(id);
    if (entity == null) return NotFound(new { Message = "리소스를 찾을 수 없습니다." });
    return Ok(entity);
} catch (Exception ex) {
    _logger.LogError(ex, "Error getting entity {Id}", id);
    return StatusCode(500, new { Message = "서버 오류가 발생했습니다." });
}
```

### Testing Standards
- **AAA 패턴**: Arrange → Act → Assert
- **Moq**: `_mockRepository.Setup(r => r.GetAsync(id)).ReturnsAsync(entity)`
- **FluentAssertions**: `result.Should().NotBeNull()`, `result.Id.Should().Be(expectedId)`
- **네이밍**: `{MethodName}_{Scenario}_{ExpectedResult}`

### Git Workflow
```
type(scope): subject

body

🤖 Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```
**Types**: feat, fix, refactor, test, docs, chore

### Async Programming
- 모든 DB 작업: `async/await` 필수
- 메서드명: `{Name}Async` 접미사
- CancellationToken 전달 (장기 작업)

### Feature Development Order
Domain Entity → Application (Interface/DTO) → Infrastructure (Repository) → API (Controller) → Tests

## Reference
- **Roadmap**: `docs/learning/PROJECT_ROADMAP.md`
- **Deployment**: `docs/jenkins/DEPLOYMENT_GUIDE.md`
- **Unity Docs**: `docs/unity/UNITY_DOCUMENTATION_GUIDE.md`
- **PRD**: `docs/MUSHROOM_GAME_PRD.md`