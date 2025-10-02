# 개발 가이드라인 및 설계 패턴

## ⚠️ 중요: Task Master 사용 안 함

**Task Master의 모든 태스크는 deferred(보류) 상태입니다.**
- 앞으로 Task Master를 참조하지 않음
- Serena 메모리, GDD, CLAUDE.md만으로 진행
- 진행 상황은 Serena 메모리에 기록

---

## Clean Architecture 개발 워크플로우

### 새 기능 추가 시 순서
1. **Domain Layer**: 엔티티 및 비즈니스 룰 정의
2. **Application Layer**: 서비스 인터페이스 및 DTO 생성
3. **Infrastructure Layer**: 데이터 레포지토리 및 서비스 구현
4. **API Layer**: 컨트롤러 및 엔드포인트 생성

### 예시: 새 기능 "아이템 시스템" 구현
```
1. Domain/Entities/Item.cs 생성
2. Application/DTOs/Items/ItemDto.cs 생성
3. Application/Services/IItemService.cs 인터페이스 생성
4. Infrastructure/Repositories/ItemRepository.cs 구현
5. Infrastructure/Services/ItemService.cs 구현
6. API/Controllers/ItemController.cs 생성
```

## 주요 설계 패턴

### 1. Repository Pattern
- **목적**: 데이터 액세스 추상화
- **위치**: Domain에 인터페이스, Infrastructure에 구현
- **예시**: `IPlayerRepository` → `PlayerRepository`

### 2. Service Pattern (현재 사용 중)
- **목적**: 비즈니스 로직 캡슐화
- **위치**: Application에 인터페이스, Infrastructure에 구현
- **예시**: `IAuthService` → `AuthService`, `ICharacterService` → `CharacterService`
- **사용 이유**: MediatR/CQRS 패턴은 Week 0 전략에 따라 보류

### 3. Dependency Injection
- **설정 위치**: `Program.cs`
- **라이프사이클**: 대부분 `Scoped` (HTTP 요청 당 인스턴스)
- **인터페이스 기반**: 항상 인터페이스를 통한 의존성 주입

### 4. DTO Pattern
- **목적**: 계층 간 데이터 전송, API 응답 구조화
- **위치**: `Application/DTOs/` 폴더
- **네이밍**: `[엔티티명]Dto.cs` (예: `PlayerDto.cs`)

## 중요한 개발 원칙

### 1. 보안 우선
- **절대 평문 비밀번호 저장 금지**
- **JWT 토큰 적절한 만료 시간 설정**
- **SQL Injection 방지 (EF Core 사용)**
- **민감 정보 로그 출력 금지**

### 2. 비동기 프로그래밍
- **데이터베이스 작업**: 모든 DB 작업은 `async/await`
- **HTTP 클라이언트**: 외부 API 호출 시 비동기
- **메서드 네이밍**: 비동기 메서드는 `Async` 접미사

### 3. 에러 핸들링
- **예외 타입별 적절한 HTTP 상태 코드**
- **사용자 친화적 에러 메시지**
- **상세 에러는 로그에만 기록**

### 4. 데이터베이스 설계
- **GUID 기본키 사용**: 분산 환경 대비
- **Navigation Properties**: 지연 로딩 고려
- **Index 설정**: 쿼리 성능 최적화

## 방치형 게임 특화 패턴

### 1. 오프라인 진행 계산
```csharp
public TimeSpan GetOfflineTime()
{
    return DateTime.UtcNow - LastLogin;
}
```

### 2. 플레이어-캐릭터 관계
- **1:N 관계**: 한 플레이어가 여러 캐릭터 보유
- **캐릭터별 독립적 진행**: 각 캐릭터마다 별도 스탯

### 3. 자동 진행 시스템 (향후 구현)
- **Background Service**: 정기적 계산
- **Redis 캐싱**: 자주 계산되는 데이터 캐시
- **SignalR**: 실시간 업데이트 (준비됨)

## API 설계 원칙

### 1. RESTful 설계
- **리소스 기반 URL**: `/api/players/{id}`
- **HTTP 메서드 의미**: GET, POST, PUT, DELETE
- **상태 코드 일관성**: 200, 201, 400, 401, 404, 500

### 2. 버전 관리 (향후)
- **URL 기반**: `/api/v1/players`
- **Header 기반**: `Accept-Version: v1`

### 3. 페이징 및 필터링 (향후)
- **쿼리 파라미터**: `?page=1&size=20&sort=level`
- **응답 메타데이터**: 총 개수, 페이지 정보

## Week 0 인프라 전략

### 설치되어 있지만 아직 설정 안 된 도구들
- **MediatR**: 설치됨, 설정 안 됨 → Service 패턴 우선 사용
- **AutoMapper**: 설치됨, 설정 안 됨 → 필요할 때 설정
- **FluentValidation**: 설치됨, 설정 안 됨 → 필요할 때 설정
- **Serilog**: 설치됨, 설정 안 됨 → 디버깅 어려울 때 설정

### 추가 타이밍
- 게임 기능 우선 구현
- 코드 반복이 많아지거나 디버깅이 어려울 때 인프라 추가
- 사용자가 필요성을 느낄 때 자연스럽게 제안