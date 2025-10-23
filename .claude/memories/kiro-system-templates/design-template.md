# Design: [Feature Name]

> 이 문서는 [Feature Name] 기능의 기술 설계를 정의합니다.
> 
> **작성 가이드**: 
> - Requirements의 모든 항목이 어떻게 구현될지 설명
> - Clean Architecture 계층별로 구성요소 정의
> - 데이터 모델, API 설계, 에러 처리 포함

---

## 📐 Architecture Overview

### Layer Responsibilities

#### API Layer
- **Controllers**: [어떤 Controller 생성/수정]
- **Endpoints**: [RESTful API 경로]
- **DTOs**: [Request/Response 구조]

#### Application Layer
- **Services**: [어떤 Service 생성/수정]
- **Business Logic**: [주요 로직 설명]
- **Validation**: [입력 검증 규칙]

#### Domain Layer
- **Entities**: [어떤 Entity 생성/수정]
- **Value Objects**: [불변 값 객체]
- **Domain Services**: [도메인 로직 서비스]
- **Enums**: [열거형 정의]

#### Infrastructure Layer
- **Repositories**: [어떤 Repository 생성/수정]
- **Data Access**: [EF Core 설정]
- **External Services**: [외부 연동]

---

## 🗄️ Data Model

### Database Schema

#### 신규 테이블: `TableName`
```sql
CREATE TABLE "TableName" (
    "Id" uuid PRIMARY KEY,
    "Column1" varchar(100) NOT NULL,
    "Column2" integer NOT NULL,
    "CreatedAt" timestamp NOT NULL,
    "UpdatedAt" timestamp NOT NULL
);

CREATE INDEX "IX_TableName_Column1" ON "TableName" ("Column1");
```

#### 수정 테이블: `ExistingTable`
- 추가 컬럼: `NewColumn` (타입, 제약조건)
- 인덱스: `IX_ExistingTable_NewColumn`

### Entity Relationships
```
Player (1) ──< (N) Character (1) ──< (N) Equipment
```

### EF Core Configuration
- [Fluent API 설정 필요사항]
- [관계 설정, Cascade 규칙]

---

## 🔌 API Design

### Endpoints

#### `POST /api/[resource]`
**Request:**
```json
{
  "field1": "value",
  "field2": 100
}
```

**Response (200 OK):**
```json
{
  "id": "uuid",
  "field1": "value",
  "status": "success"
}
```

**Errors:**
- `400 Bad Request`: [조건]
- `401 Unauthorized`: [조건]
- `404 Not Found`: [조건]

#### `GET /api/[resource]/{id}`
...

---

## 🧮 Business Logic

### 핵심 알고리즘

#### [알고리즘 이름]

**입력:**
- [파라미터 목록]

**출력:**
- [반환값]

**프로세스 (AI 제안)**:
1. [단계별 설명 - 확률, 보상 계산식 포함]
2. [예: "Random(0~100) < 1 → Legendary, < 10 → Epic, ..."]
3. [예: "보상 Gold = BaseGold * (1 + Level * 0.1)"]

**예외 처리:**
- [예외 상황과 처리 방식]

**🎓 학습 포인트 (아키텍처 결정)**:
- **TODO(human)**: [예: "이 로직은 Domain Service vs Application Service?"]
- **TODO(human)**: [예: "Random 생성: IRandomProvider 인터페이스로 추상화? (테스트 용이성)"]
- **TODO(human)**: [예: "확률 계산 결과 캐싱 필요? (성능 최적화)"]

> 💡 **학습 가이드**: 계산식은 AI가 제안합니다. **계층 분리**와 **의존성 주입**에 집중하세요.
> 상세 규칙: [CLAUDE.md - 학습 프로젝트 특화 규칙](../../../CLAUDE.md#학습-프로젝트-특화-규칙)

---

## 🎯 Service Layer Design

### [ServiceName]

**책임**: [서비스가 담당하는 역할]

**메서드:**

#### `MethodNameAsync(params)`
```csharp
public async Task<ReturnType> MethodNameAsync(
    ParamType param1,
    CancellationToken cancellationToken = default)
{
    // 1. Input validation
    // 2. Business logic orchestration (AI가 구현)
    // 3. Domain service 호출
    // 4. Repository 저장
    // 5. Return result

    // 🎓 TODO(human):
    // 트랜잭션 경계를 어디에? (여기? Repository?)
    // 에러 핸들링: try-catch vs Result<T> 패턴?
}
```

**Dependencies:**
- `IRepository`: [사용 목적]
- `IDomainService`: [사용 목적]

---

## 🧪 Testing Strategy

### Unit Tests
- [테스트할 컴포넌트 목록]
- [Mock 대상]
- [핵심 시나리오]

### Integration Tests
- [API 엔드포인트 테스트]
- [데이터베이스 연동 테스트]

### Test Coverage Goals
- Domain Services: 90%+
- Application Services: 80%+
- Controllers: 70%+

---

## ⚠️ Error Handling

### Exception Types
- `NotFoundException`: [사용 시점]
- `ValidationException`: [사용 시점]
- `BusinessRuleException`: [사용 시점]

### Error Response Format
```json
{
  "message": "Error description",
  "statusCode": 400,
  "details": {}
}
```

---

## 🔐 Security Considerations

- **Authentication**: [JWT, Role-based 등]
- **Authorization**: [권한 체크 지점]
- **Data Validation**: [입력 검증 규칙]
- **SQL Injection Prevention**: [EF Core 파라미터화]

---

## 📊 Performance Considerations

- **Database Indexes**: [어떤 인덱스 필요]
- **Caching**: [캐싱 전략]
- **Pagination**: [대량 데이터 처리]
- **N+1 Query Prevention**: [Eager Loading 전략]

---

## 🔄 Migration Plan

### Database Migration
```sql
-- migration.sql에 추가할 내용 (AI가 작성)
DO $EF$ BEGIN
    IF NOT EXISTS(SELECT 1 FROM information_schema.tables
                  WHERE table_name = 'TableName') THEN
        CREATE TABLE "TableName" (
            "Id" uuid PRIMARY KEY,
            ...
        );
        CREATE INDEX "IX_TableName_Column1" ON "TableName" ("Column1");
    END IF;
END $EF$;
```

**🎓 학습 포인트 (SQL 작성)**:
- **TODO(human)**: [예: "인덱스 전략: 단일 컬럼 vs 복합 인덱스?"]
- **TODO(human)**: [예: "Cascade Delete: ON DELETE CASCADE vs Application에서 처리?"]
- **TODO(human)**: [예: "JSONB 컬럼 사용 vs 정규화? (스킬 메타데이터 저장)"]

> 💡 **학습 가이드**: SQL 문법은 AI가 작성합니다. **인덱싱 전략**, **정규화 vs 역정규화** 설계 결정에 집중하세요.
> 상세 규칙: [CLAUDE.md - 학습 프로젝트 특화 규칙](../../../CLAUDE.md#학습-프로젝트-특화-규칙)

### Data Seeding
- [초기 데이터 필요 여부]
- [Seeder 클래스 작성 계획]

---

## 📝 Decision Log

> ⚠️ **중요**: L 사이즈 기능은 이 섹션 **필수**. 중요한 아키텍처 결정을 ADR과 연결.
>
> **Spike/ADR 트리거 조건**: [CLAUDE.md - Spike & ADR](../../../CLAUDE.md#spike--adr) 참조

| ID | Decision | ADR Link | Spike Link | Status |
|----|----------|----------|------------|--------|
| D1 | [예: Pet-Character 관계는 1:N] | [ADR-0005](../../docs/adr/ADR-0005-pet-relationship.md) | - | Accepted |
| D2 | [예: 가챠 확률 계산은 Domain Service] | [ADR-0006](../../docs/adr/ADR-0006-gacha-logic-layer.md) | [Spike-001](../../docs/spikes/2025-10/gacha-performance.md) | Accepted |
| D3 | [예: EF Core Include 사용 (Select 대신)] | [ADR-0007](../../docs/adr/ADR-0007-ef-query-strategy.md) | [Spike-002](../../docs/spikes/2025-10/ef-performance.md) | Accepted |

**가이드**:
- **간단한 결정**: ADR 없이 테이블에 1줄로 기록 (예: "단일 컬럼 인덱스 사용")
- **복잡한 결정**: ADR 작성 후 링크
- **Spike 결과**: Spike 링크 포함

---

## 📱 Unity Client Integration

### Unity Documentation 필요 항목
- `API_SPEC.md`: [엔드포인트 명세]
- `DTOs.cs`: [C# DTO 클래스]
- [추가 문서]

**참고**: `docs/unity/UNITY_DOCUMENTATION_GUIDE.md`

---

## ✅ Approval

- [ ] Design 리뷰 완료
- [ ] 모든 Requirements 항목 커버 확인
- [ ] TODO(human) 아키텍처 학습 포인트 확인 완료
- [ ] Self-Review Checklist 10개 항목 통과 (9/10 이상)
- [ ] Tasks 단계로 진행 승인

---

**작성일**: YYYY-MM-DD  
**작성자**: [이름]  
**상태**: Draft / Approved  
**Requirements 추적성**: [requirements.md의 US-1, US-2 등 참조]
