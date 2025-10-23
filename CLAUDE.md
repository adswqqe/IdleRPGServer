# CLAUDE.md

## Language and Communication
**모든 응답은 한국어로 제공**. 코드와 명령어는 원문 유지.

**Architecture Layers** (Dependency: API → Application → Domain):
- **API**: Controllers, Middleware, SignalR
- **Application**: Services, DTOs (MediatR, FluentValidation, AutoMapper)
- **Domain**: Entities, Business rules
- **Infrastructure**: Repositories (EF Core + PostgreSQL)

## Critical Rules

### 🗃️ Database Migration
**마이그레이션 형식**: `IdleRPG.Infrastructure/migration.sql` (단일 파일, Idempotent 패턴)

**프로세스**:
1. `IdleRPG.Infrastructure/migration.sql` 파일에 새 마이그레이션 추가
2. `DO $EF$ BEGIN IF NOT EXISTS` 패턴 사용 (재실행 안전)
3. Migration ID: `YYYYMMDD000000_FeatureName` 형식
4. **배포**: Jenkins CI/CD가 자동으로 적용 (수동 실행 금지)
5. **로컬 테스트**: `IdleRPG.Infrastructure/Migrations/` 폴더에 별도 `.sql` 파일 생성하여 테스트 가능
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
CRUD, Repository, DTO, Configuration, SQL Migration, Unity 문서, Swagger 주석, 단위 테스트

### 👥 함께 협업
데이터 설계, 비즈니스 로직, API 설계, 아키텍처, 성능 최적화

**프로세스**: 설계 초안 → 피드백 → 구현(핵심 로직은 TODO(human)) → 검토

### TODO(human) 규칙

**사용 시점**:
- 비즈니스 로직이 불확실할 때 (확률, 보상 계산)
- 사용자 선호도가 필요할 때 (강화 실패 시 처리)
- 디자인 결정이 필요할 때 (매칭 알고리즘)
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

### Testing Standards
- **AAA 패턴**: Arrange → Act → Assert
- **Moq**: `_mockRepository.Setup(r => r.GetAsync(id)).ReturnsAsync(entity)`
- **FluentAssertions**: `result.Should().NotBeNull()`, `result.Id.Should().Be(expectedId)`
- **네이밍**: `{MethodName}_{Scenario}_{ExpectedResult}`

### Feature Development Order
Domain Entity → Application (Interface/DTO) → Infrastructure (Repository) → API (Controller) → Tests

## Kiro Workflow (Spec-Driven Development)

### 📋 Overview
**Kiro**는 Amazon의 Spec-Driven Development 방법론으로, 코드 작성 전에 요구사항-설계-작업을 체계적으로 문서화합니다.

**3단계 프로세스**:
1. **Requirements** (무엇을 만들 것인가) → `requirements.md`
2. **Design** (어떻게 만들 것인가) → `design.md`
3. **Tasks** (단계별 구현 작업) → `tasks.md`

### 📁 Spec 위치
`.claude/memories/specs/{feature-name}/`
- `requirements.md` - 사용자 스토리, EARS 형식 수용 기준, 게임 디자인 결정
- `design.md` - Clean Architecture 계층별 설계, API/DB 스키마, 비즈니스 로직
- `tasks.md` - Milestone별 체크리스트 (Domain → Infrastructure → Application → API → DB → Tests)

**예시**: `.claude/memories/specs/skill-gacha/` (완료된 참고 예시)

### 🔄 Workflow 순서

#### Phase 1: Requirements
1. Claude가 `requirements-template.md` 기반으로 초안 작성
2. 사용자 스토리 작성: "As a [역할], I want [기능], so that [목적]"
3. EARS 형식 수용 기준: "WHEN [조건] THEN system SHALL [동작]"
4. **게임 디자인 결정**: 확률, 보상, 밸런스 → TODO(human) 마커 표시
5. **사용자 승인 필수** → "Approved" 상태로 변경

#### Phase 2: Design
1. Claude가 `design-template.md` 기반으로 초안 작성
2. Clean Architecture 계층별 책임 정의 (API/Application/Domain/Infrastructure)
3. 데이터 모델 (Entity, EF Core Configuration, 인덱스)
4. API 설계 (Endpoint, Request/Response DTO)
5. 비즈니스 로직 (알고리즘, 계산식) → 복잡한 부분은 TODO(human)
6. **사용자 승인 필수**

#### Phase 3: Tasks
1. Claude가 `tasks-template.md` 기반으로 초안 작성
2. Milestone별 작업 분류:
   - Milestone 1: Domain Layer (Entity, Enum, Domain Service)
   - Milestone 2: Infrastructure (Repository, EF Config)
   - Milestone 3: Application (DTO, Service)
   - Milestone 4: API (Controller)
   - Milestone 5: Database (Migration, Seeder)
   - Milestone 6: Testing & Documentation (Unit Test, Unity Docs)
3. 각 작업에 체크리스트, 예상 시간, Requirements 추적성 포함
4. **사용자 승인 필수**

#### Phase 4: Implementation
- `tasks.md` 열어서 각 작업 옆 **"Start task"** 버튼 클릭
- Claude가 한 번에 1개 작업만 집중 수행
- 완료 시 체크박스 ✅ 업데이트

### 🎯 Kiro + IdleRPG 통합 규칙

#### Requirements 단계에서
- **게임 밸런스**: 확률, 보상량, 재화 비용 → TODO(human) 명시
- **EARS 형식 필수**: "WHEN 플레이어가 가챠 요청 THEN system SHALL 크리스탈 차감"

#### Design 단계에서
- **Clean Architecture 준수**: Domain은 다른 계층에 의존하지 않음
- **Unity 연동 고려**: DTO는 `[JsonProperty]` 속성 (Newtonsoft.Json 호환)
- **Migration Plan 포함**: Idempotent 패턴 SQL 작성

#### Tasks 단계에서
- **의존성 순서**: Domain → Infrastructure → Application → API
- **Unity 문서 필수**: Milestone 6에 "Create Unity Documentation" 작업 포함
- **테스트 커버리지**: Domain Service 90%+, Application Service 80%+

### 📚 상세 가이드
`.claude/memories/kiro-system-templates/how-kiro-works.md` - Kiro 전체 워크플로우, 승인 프로세스, Best Practices

### ✅ 새 기능 시작 시 체크리스트
- [ ] `.claude/memories/specs/{feature-name}/` 폴더 생성
- [ ] `requirements.md` 작성 및 승인
- [ ] `design.md` 작성 및 승인
- [ ] `tasks.md` 작성 및 승인
- [ ] Task 실행 (Start task 버튼)

## Reference
- **Roadmap**: `docs/learning/PROJECT_ROADMAP.md`
- **Deployment**: `docs/jenkins/DEPLOYMENT_GUIDE.md`
- **Unity Docs**: `docs/unity/UNITY_DOCUMENTATION_GUIDE.md`
- **PRD**: `docs/MUSHROOM_GAME_PRD.md`
- **Kiro Guide**: `.claude/memories/kiro-system-templates/how-kiro-works.md`