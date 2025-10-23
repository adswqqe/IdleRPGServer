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
**TODO(human)**: [디자인 결정 필요 - 확률, 보상 계산식 등]

**입력:**
- [파라미터 목록]

**출력:**
- [반환값]

**프로세스:**
1. [단계별 설명]
2. ...

**예외 처리:**
- [예외 상황과 처리 방식]

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
    // TODO(human): 비즈니스 로직 디자인 결정
    // [의사코드 또는 프로세스 설명]
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
-- migration.sql에 추가할 내용
DO $EF$ BEGIN
    IF NOT EXISTS(SELECT 1 FROM information_schema.tables 
                  WHERE table_name = 'TableName') THEN
        -- CREATE TABLE ...
    END IF;
END $EF$;
```

### Data Seeding
- [초기 데이터 필요 여부]
- [Seeder 클래스 작성 계획]

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
- [ ] TODO(human) 비즈니스 로직 결정 완료
- [ ] Tasks 단계로 진행 승인

---

**작성일**: YYYY-MM-DD  
**작성자**: [이름]  
**상태**: Draft / Approved  
**Requirements 추적성**: [requirements.md의 US-1, US-2 등 참조]
