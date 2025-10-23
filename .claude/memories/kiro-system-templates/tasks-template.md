# Tasks: [Feature Name]

> 이 문서는 [Feature Name] 기능의 구현 작업 목록입니다.
> 
> **작성 가이드**: 
> - Design 문서의 모든 컴포넌트를 구현 가능한 작업으로 분해
> - 각 작업은 독립적으로 완료 및 테스트 가능해야 함
> - 순서는 의존성을 고려하여 정렬 (Domain → Infrastructure → Application → API)

---

## 📊 Progress Overview

**전체 진행률**: 0/25 (0%)

| Milestone | 작업 수 | 완료 | 진행률 |
|-----------|---------|------|--------|
| Domain Layer | 7 | 0 | 0% |
| Infrastructure Layer | 5 | 0 | 0% |
| Application Layer | 4 | 0 | 0% |
| API Layer | 3 | 0 | 0% |
| Database | 3 | 0 | 0% |
| Testing & Documentation | 3 | 0 | 0% |

**예상 총 소요 시간**: ~XX시간

---

## 🏗️ Milestone 1: Domain Layer

### 1.1 Create [EntityName] Entity ⏱️ 30분
- [ ] Create `IdleRPG.Domain/Entities/[EntityName].cs`
- [ ] Add properties: Id, Name, ... (from design.md)
- [ ] Implement BaseEntity inheritance (CreatedAt, UpdatedAt)
- [ ] Add navigation properties for relationships

**Requirements**: [US-1]  
**Design Reference**: [Data Model - EntityName]

---

### 1.2 Create [EnumName] Enum ⏱️ 15분
- [ ] Create `IdleRPG.Domain/Enums/[EnumName].cs`
- [ ] Define enum values: Value1, Value2, ...
- [ ] Add XML documentation comments

**Requirements**: [US-1]  
**Design Reference**: [Domain Layer - Enums]

---

### 1.3 Create [DomainServiceName] Domain Service ⏱️ 1시간
- [ ] Create `IdleRPG.Domain/Services/[DomainServiceName].cs`
- [ ] Implement [MethodName](...) method (AI가 비즈니스 로직 구현)
- [ ] Add input validation
- [ ] Add XML documentation
- [ ] **🎓 TODO(human)**: 학습 포인트 구현
  - [예: "IRandomProvider 인터페이스 추상화 (테스트 용이성)"]
  - [예: "Domain Service vs Application Service 선택 근거 작성"]

**Requirements**: [US-2]
**Design Reference**: [Business Logic - 알고리즘]

> 💡 **학습 가이드**: 확률 계산식은 AI가 제공합니다. **계층 분리**와 **의존성 주입** 구현에 집중하세요.

---

### 1.4 Create [ValueObjectName] Value Object ⏱️ 45분
- [ ] Create `IdleRPG.Domain/ValueObjects/[ValueObjectName].cs`
- [ ] Implement immutability (readonly properties)
- [ ] Override Equals/GetHashCode (value equality)
- [ ] Add factory method

**Requirements**: [US-1]  
**Design Reference**: [Domain Layer - Value Objects]

---

## 🔧 Milestone 2: Infrastructure Layer

### 2.1 Create [RepositoryName] Repository ⏱️ 45분
- [ ] Create `IdleRPG.Domain/Repositories/I[RepositoryName].cs` (interface)
- [ ] Create `IdleRPG.Infrastructure/Repositories/[RepositoryName].cs` (implementation)
- [ ] Implement CRUD methods: GetAsync, AddAsync, UpdateAsync, DeleteAsync
- [ ] Add specialized query methods (e.g., GetByUserIdAsync)

**Requirements**: [US-1]  
**Design Reference**: [Infrastructure Layer - Repositories]

---

### 2.2 Create [EntityName] EF Core Configuration ⏱️ 30분
- [ ] Create `IdleRPG.Infrastructure/Configurations/[EntityName]Configuration.cs`
- [ ] Implement IEntityTypeConfiguration<[EntityName]>
- [ ] Configure table name, primary key
- [ ] Configure properties (required, max length, column types)
- [ ] Configure relationships (HasMany/WithOne, cascade rules)
- [ ] Configure indexes

**Requirements**: [US-1]  
**Design Reference**: [Data Model - EF Core Configuration]

---

### 2.3 Create [ServiceName] Application Service ⏱️ 1.5시간
- [ ] Create `IdleRPG.Application/Services/I[ServiceName].cs` (interface)
- [ ] Create `IdleRPG.Infrastructure/Services/[ServiceName].cs` (implementation)
- [ ] Inject dependencies (repositories, domain services)
- [ ] Implement [MainMethodAsync] method (AI가 오케스트레이션 로직 구현)
  - Input validation
  - Business logic orchestration
  - Error handling
- [ ] Add logging
- [ ] **🎓 TODO(human)**: 학습 포인트 구현
  - [예: "트랜잭션 경계 설정 (TransactionScope? DbContext?)"]
  - [예: "에러 핸들링 패턴: try-catch vs Result<T>?"]

**Requirements**: [US-2]
**Design Reference**: [Service Layer - ServiceName]

> 💡 **학습 가이드**: Application Service는 **워크플로우 오케스트레이션**만 담당. Pure 비즈니스 로직은 Domain에!

---

## 📦 Milestone 3: Application Layer (DTOs)

### 3.1 Create [FeatureName] Request DTOs ⏱️ 30분
- [ ] Create `IdleRPG.Application/DTOs/[FeatureName]/[Action]RequestDto.cs`
- [ ] Add properties matching API request
- [ ] Add FluentValidation rules (if needed)
- [ ] Add XML documentation

**Requirements**: [US-1]  
**Design Reference**: [API Design - Request]

---

### 3.2 Create [FeatureName] Response DTOs ⏱️ 30분
- [ ] Create `IdleRPG.Application/DTOs/[FeatureName]/[Action]ResponseDto.cs`
- [ ] Add properties matching API response
- [ ] Add AutoMapper profile (if needed)
- [ ] Add XML documentation

**Requirements**: [US-1]  
**Design Reference**: [API Design - Response]

---

## 🌐 Milestone 4: API Layer

### 4.1 Create [ControllerName] Controller ⏱️ 1시간
- [ ] Create `IdleRPG.API/Controllers/[ControllerName]Controller.cs`
- [ ] Add [Authorize] attribute (if authentication required)
- [ ] Inject I[ServiceName] dependency
- [ ] Implement POST /api/[resource] endpoint
  - Validate request DTO
  - Call service method
  - Return response DTO
  - Handle exceptions (try-catch)
- [ ] Add Swagger XML comments

**Requirements**: [US-1, US-2]  
**Design Reference**: [API Design - Endpoints]

---

### 4.2 Add GET /api/[resource]/{id} Endpoint ⏱️ 30분
- [ ] Implement GET endpoint in [ControllerName]Controller
- [ ] Validate id parameter
- [ ] Return 404 if not found
- [ ] Add Swagger documentation

**Requirements**: [US-1]  
**Design Reference**: [API Design - GET Endpoint]

---

## 🗃️ Milestone 5: Database

### 5.1 Create Database Migration ⏱️ 45분
- [ ] Add migration to `IdleRPG.Infrastructure/migration.sql` (AI가 SQL 작성)
- [ ] Use DO $EF$ BEGIN ... END $EF$ pattern (idempotent)
- [ ] Add CREATE TABLE for new entities
- [ ] Add ALTER TABLE for modified entities
- [ ] Add CREATE INDEX for performance
- [ ] **🎓 TODO(human)**: 학습 포인트 구현
  - [예: "인덱스 전략: 단일 vs 복합 인덱스 선택 근거 작성"]
  - [예: "Cascade Delete: ON DELETE CASCADE vs Application 코드에서 처리?"]
- [ ] Test migration locally (create separate .sql file in Migrations/ folder)

**Requirements**: [All]
**Design Reference**: [Migration Plan]
**참고**: `CLAUDE.md - Database Migration`

> 💡 **학습 가이드**: SQL 문법은 AI가 작성합니다. **인덱싱 전략**, **정규화 vs 역정규화** 설계 결정을 경험하세요.

---

### 5.2 Create [EntityName] Seeder ⏱️ 30분
- [ ] Create `IdleRPG.Infrastructure/Seeders/[EntityName]Seeder.cs`
- [ ] Add sample/template data (AI가 밸런스 값 제안)
  - [예: "스킬 템플릿 24개, Legendary 3개, Epic 6개, Rare 9개, Common 6개"]
  - [예: "공격력: Legendary 100~150, Epic 70~100, ..."]
- [ ] Register in Program.cs or DbContext seeding

**Requirements**: [US-1]
**Design Reference**: [Data Seeding]

> 💡 **학습 가이드**: Seeder 데이터는 AI가 제공합니다. **Seeding 패턴 구현**과 **데이터 중복 방지 로직**에 집중하세요.

---

### 5.3 Register Dependencies in DI Container ⏱️ 15분
- [ ] Open `IdleRPG.API/Program.cs`
- [ ] Register repositories: `AddScoped<I[Repository], [Repository]>()`
- [ ] Register services: `AddScoped<I[Service], [Service]>()`
- [ ] Register domain services: `AddScoped<[DomainService]>()`

**Requirements**: [All]  
**Design Reference**: [Architecture Overview]

---

## 🧪 Milestone 6: Testing & Documentation

### 6.1 Create [DomainServiceName] Unit Tests ⏱️ 2시간
- [ ] Create `IdleRPG.Tests/Domain/Services/[DomainServiceName]Tests.cs`
- [ ] Setup: Mock dependencies (IRandomProvider, etc.)
- [ ] Test cases:
  - Happy path scenarios (최소 3개)
  - Edge cases (경계값, 0, null 등)
  - Error cases (예외 발생)
- [ ] Use AAA pattern (Arrange-Act-Assert)
- [ ] Use FluentAssertions for readable assertions
- [ ] Achieve 90%+ code coverage

**Requirements**: [US-2]  
**Design Reference**: [Testing Strategy - Unit Tests]

---

### 6.2 Create [ServiceName] Unit Tests ⏱️ 1.5시간
- [ ] Create `IdleRPG.Tests/Application/Services/[ServiceName]Tests.cs`
- [ ] Mock repositories and domain services
- [ ] Test business logic orchestration
- [ ] Test error handling
- [ ] Achieve 80%+ code coverage

**Requirements**: [US-1, US-2]  
**Design Reference**: [Testing Strategy]

---

### 6.3 Create Unity Documentation ⏱️ 1시간
- [ ] Create `../IdleRPGClient/Docs/unity/[feature-name]/` folder
- [ ] Create `API_SPEC.md`:
  - Endpoint details
  - Request/Response examples
  - Unity C# usage example (UnityWebRequest)
- [ ] Create `DTOs.cs`:
  - Unity-compatible C# DTOs
  - Use [JsonProperty] attributes
  - Newtonsoft.Json compatible
- [ ] Update `unity/README.md` main index

**Requirements**: [All]  
**Design Reference**: [Unity Client Integration]  
**참고**: `docs/unity/UNITY_DOCUMENTATION_GUIDE.md`

---

## 🚀 Post-Implementation

### ✅ Completion Checklist
- [ ] All tasks completed and tested
- [ ] Unit tests passing (90%+ coverage)
- [ ] Integration tests passing
- [ ] Migration applied via Jenkins (DO NOT run `dotnet ef database update` locally!)
- [ ] Unity documentation complete
- [ ] Code review completed
- [ ] Git commit with descriptive message
- [ ] Feature merged to main branch

---

## 📝 Notes

### Blockers
<!-- 작업 중 발생한 장애 요소 기록 -->

### Decisions Made
<!-- TODO(human) 해소 과정에서 내린 결정 기록 -->

### Future Improvements
<!-- 나중에 개선할 사항 -->

---

**시작일**: YYYY-MM-DD  
**완료일**: YYYY-MM-DD  
**총 소요 시간**: XX시간  
**Design 추적성**: [design.md의 모든 컴포넌트 커버 확인]
