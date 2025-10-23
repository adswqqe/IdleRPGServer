# /spec-design - Design 문서 생성

**목적**: 승인된 Requirements를 기반으로 기술 설계 문서를 생성합니다.

---

## 실행 절차

### 1. 전제 조건 확인
- `requirements.md`가 존재하는지 확인
- Requirements 승인 여부 확인:
  ```markdown
  ## Approval
  - [x] Requirements reviewed by team
  - [x] Business value confirmed
  **Approved by**: Development Team
  **Date**: YYYY-MM-DD
  ```
- 승인되지 않았으면 에러 메시지 출력 후 중단

### 2. 시스템 문서 참조
다음 문서들을 읽어서 설계 컨텍스트 파악:
- `.claude/memories/_system/architecture.md` - Clean Architecture 계층
- `.claude/memories/_system/game-design.md` - 서버 권위 원칙, 엔티티 관계
- `.claude/memories/_system/tech-stack.md` - 사용 가능한 라이브러리
- `.claude/memories/_system/api-standards.md` - API 설계 표준

### 3. 템플릿 기반 초안 작성
`.claude/memories/kiro-system-templates/design-template.md`를 기반으로 `design.md` 생성:

**포함 내용**:
- **Architecture Overview**: API/Application/Domain/Infrastructure 계층별 책임
- **Data Model**: 
  - Database Schema (CREATE TABLE, indexes)
  - Entity Relationships (ERD)
  - EF Core Configuration
- **API Design**: 
  - Endpoints (RESTful)
  - Request/Response DTO
  - HTTP 상태 코드
- **Business Logic**: 
  - 핵심 알고리즘 (의사코드)
  - **TODO(human)**: 복잡한 비즈니스 로직 디자인 결정
- **Service Layer Design**: 메서드 시그니처, Dependencies
- **Testing Strategy**: Unit/Integration 테스트 계획
- **Error Handling**: Exception Types, Error Response Format
- **Security Considerations**: Authentication, Authorization
- **Performance Considerations**: Indexes, Caching, N+1 Prevention
- **Migration Plan**: Idempotent SQL, Data Seeding
- **Unity Client Integration**: Unity 문서화 필요 항목

### 4. Requirements 추적성 확인
- 모든 User Story (US-1, US-2 등)가 Design에 매핑되었는지 확인
- 각 섹션에 `**Requirements**: [US-X]` 표기

### 5. 출력
```
✅ {feature-name} design created

Created:
- .claude/memories/specs/{feature-name}/design.md

Design includes:
- 4 layers (API/Application/Domain/Infrastructure)
- 3 database tables (Entity, Template, History)
- 2 API endpoints
- TODO(human): 1개 (Business logic decision needed)

Requirements coverage:
- US-1: ✅ Covered in API/Data Model
- US-2: ✅ Covered in Business Logic
- US-3: ✅ Covered in Service Layer

Next steps:
1. Review design.md
2. Decide TODO(human) items (business logic, algorithms)
3. Check approval section
4. Run: /spec-tasks {feature-name}
```

---

## 주의사항

- ⚠️ **Requirements 승인 필수**: 승인되지 않으면 실행 중단
- ⚠️ **모든 Requirements 커버**: US-1, US-2 등 모두 Design에 반영
- ⚠️ **TODO(human)은 비즈니스 결정만**: 기술 구현은 Claude가 처리
- ⚠️ **Unity 문서화 계획 포함**: API/DTO 변경 시 필수

---

## 예시

### 입력
```
/spec-design pet-system
```

### 에러 케이스
```
❌ Requirements not approved

Please approve requirements.md first:
1. Review .claude/memories/specs/pet-system/requirements.md
2. Decide all TODO(human) items
3. Mark approval section:
   - [x] Requirements reviewed by team
   **Approved by**: Your Name
   **Date**: 2025-10-23

Then run: /spec-design pet-system
```

### 성공 케이스
```
✅ pet-system design created

Created:
- .claude/memories/specs/pet-system/design.md

Design includes:
- Database: Pet, PetTemplate, PetOwnership tables
- API: POST /api/pets/gacha, GET /api/pets
- Business Logic: PetStatBuffService
- TODO(human): Pet stat buff calculation formula

Next steps:
1. Review design.md
2. Decide: Pet buff formula (Attack * 1.05 or Attack + 10?)
3. Mark approval
4. Run: /spec-tasks pet-system
```
