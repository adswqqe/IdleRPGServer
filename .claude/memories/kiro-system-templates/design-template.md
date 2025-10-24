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

> ⚠️ **개념적 명세만 작성** - SQL DDL, C# 코드는 Implementation 단계에서 작성
> 참고: [CLAUDE.md - Design vs Implementation 경계](../../../CLAUDE.md#design-vs-implementation-경계)

### 신규 테이블: `TableName`

**목적**: [테이블의 역할과 저장할 데이터 설명]

**필드**:
- `Id` (PK): 고유 식별자 (타입 결정은 구현 시)
- `Column1`: [설명] (필수/선택, 제약조건)
- `Column2`: [설명] (범위, 기본값)
- `CreatedAt`, `UpdatedAt`: 감사 필드

**제약사항**:
- `Column1`: [예: 중복 불가, 2-50자 제한]
- `Column2`: [예: 0 이상 정수]

**인덱스 요구사항**:
- `Column1`: [예: 이름 검색 쿼리 빈번, 단일 컬럼 인덱스]
- 복합 인덱스: [필요 시]

**관계**:
- `ExistingEntity`와 1:N 관계 (FK: `ExistingEntityId`)

---

### 수정 테이블: `ExistingTable`

**변경사항**:
- 추가 필드: `NewColumn` ([설명], 필수/선택)
- 제약조건: [새로운 제약]
- 인덱스 추가: `NewColumn` (이유: [쿼리 패턴])

---

### Entity Relationships

**ERD 개요**:
```
Player (1) ──< (N) Character (1) ──< (N) Equipment
                ↓
              (1:N)
                ↓
           TableName
```

**관계 설명**:
- `TableName` → `Character`: 1:N (한 캐릭터가 여러 TableName 소유)
- Cascade 규칙: [예: 캐릭터 삭제 시 TableName도 삭제]

---

### EF Core Configuration 요구사항

**Fluent API 필요 항목**:
- `TableName`: [예: HasMany-WithOne 관계 설정, Cascade Delete]
- 복합 키: [필요 시]
- Value Object 매핑: [필요 시]

> 💡 **구현 참고**: 구체적인 Fluent API 코드는 `IdleRPG.Infrastructure/Configurations/` 에서 작성

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

> ⚠️ **메서드 시그니처와 책임만 정의** - 구현 코드는 Implementation 단계에서 작성

### [ServiceName]

**책임**: [서비스가 담당하는 역할]

**메서드:**

#### `MethodNameAsync(params)`

**시그니처**:
- 입력: `ParamType param1`, `CancellationToken cancellationToken`
- 반환: `Task<ReturnType>` (성공 시 [설명])

**프로세스 흐름**:
1. Input validation (검증 규칙: [명시])
2. Business logic orchestration (AI가 구현: [계산식/로직 설명])
3. Domain service 호출 (어떤 도메인 로직?)
4. Repository 저장 (트랜잭션 필요 여부)
5. Return result

**에러 조건**:
- `ValidationException`: [조건]
- `NotFoundException`: [조건]
- `BusinessRuleException`: [조건]

**🎓 학습 포인트 (아키텍처 결정)**:
- **TODO(human)**: 트랜잭션 경계 설정 (Service vs Repository?)
- **TODO(human)**: 에러 핸들링 전략 (try-catch vs Result<T> 패턴?)
- **TODO(human)**: 비동기 패턴 선택 (Task vs ValueTask?)

> 💡 **학습 가이드**: 메서드 구현은 AI가 작성합니다. **트랜잭션 관리**, **에러 핸들링 패턴**, **의존성 주입** 설계에 집중하세요.

---

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

> ⚠️ **마이그레이션 요구사항만 명시** - 실제 SQL은 Implementation 단계에서 작성
> 위치: `IdleRPG.Infrastructure/migration.sql` (Idempotent 패턴)

### Database Migration 요구사항

**신규 테이블**:
- `TableName`: [목적], [필드 수], [관계]

**수정 테이블**:
- `ExistingTable`: [변경 내용]

**인덱스 추가**:
- `IX_TableName_Column1`: [이유]

**데이터 마이그레이션**:
- [기존 데이터 변환 필요 여부]
- [백업 권장 여부]

**🎓 학습 포인트 (데이터베이스 설계)**:
- **TODO(human)**: 인덱스 전략 (단일 컬럼 vs 복합 인덱스?)
- **TODO(human)**: Cascade Delete (ON DELETE CASCADE vs Application 처리?)
- **TODO(human)**: 정규화 vs 역정규화 (JSONB vs 별도 테이블?)
- **TODO(human)**: 타입 선택 (uuid vs serial, varchar vs text?)

> 💡 **학습 가이드**: SQL 문법은 AI가 작성합니다. **인덱싱**, **정규화**, **타입 선택**, **Cascade 규칙** 같은 설계 결정에 집중하세요.

---

### Data Seeding 계획

**Seeder 필요 여부**: [Yes/No]

**초기 데이터**:
- `TableName`: [예: 마스터 데이터 10개 (Common 5개, Rare 3개, Epic 2개)]
- 데이터 출처: [하드코딩 vs JSON 파일 vs 외부 API]

**Seeder 클래스**: `TableNameSeeder.cs` (Infrastructure Layer)

> 💡 **구현 참고**: Seeder 코드는 Implementation 단계에서 작성

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
