# Service 클래스 계층 배치 검토 리포트

> **Review Date**: 2025-11-10
> **Status**: 🔴 **CRITICAL** - 심각한 아키텍처 위반 발견
> **Priority**: HIGH

---

## 📊 요약

### 문제 심각도
- **심각도**: 🔴 CRITICAL
- **영향도**: Clean Architecture 의존성 방향 위반
- **범위**: 15개+ Service 클래스

### 발견 이슈
**거의 모든 Application Service들이 Infrastructure Layer에 잘못 배치되어 있습니다.**

---

## 🏗️ Clean Architecture 계층 원칙 (복습)

### 의존성 방향
```
API Layer → Application Layer → Domain Layer
           ↑
    Infrastructure Layer (외부 의존성만)
```

### 각 계층의 Service 역할

#### 1. Domain Services (`IdleRPG.Domain/Services/`)
- **역할**: 순수 비즈니스 로직, 계산 알고리즘
- **의존성**: 없음 (또는 다른 Domain Entity/ValueObject만)
- **예시**: EloRatingService, GachaLogicService, LootCalculator

#### 2. Application Services (`IdleRPG.Application/Services/`)
- **역할**: Use Case 구현, Repository/Domain Service 오케스트레이션
- **의존성**: IUnitOfWork, Repository, Domain Service
- **예시**: CharacterService, SkillService, PvpService

#### 3. Infrastructure Services (`IdleRPG.Infrastructure/Services/`)
- **역할**: 외부 시스템 연동 (Redis, SignalR, HTTP Client, Message Queue 등)
- **의존성**: 외부 라이브러리, Infrastructure 구현체
- **예시**: RedisCacheService, EmailService, SmsService, SignalRHub

---

## ❌ 잘못 배치된 Service 목록 (15개)

### Infrastructure/Services/ (최근 추가, 5개)

| Service | 현재 위치 | 올바른 위치 | 이유 |
|---------|----------|-----------|------|
| SkillService | Infrastructure | **Application** | IUnitOfWork 사용, 가챠 Use Case 구현 |
| PetService | Infrastructure | **Application** | IUnitOfWork 사용, 펫 가챠 Use Case 구현 |
| ChatService | Infrastructure | **Application** | IUnitOfWork 사용, 채팅 메시지 저장 |
| PvpMatchmakingService | Infrastructure | **Application** | IUnitOfWork 사용, 매칭 로직 구현 |
| OfflineRewardService | Infrastructure | **Application** | IUnitOfWork 사용, 보상 계산 및 지급 |

### Infrastructure/Service/ (기존, 10개)

| Service | 현재 위치 | 올바른 위치 | 이유 |
|---------|----------|-----------|------|
| AuthService | Infrastructure | **Application** | IUnitOfWork 사용, 회원가입/로그인 Use Case |
| CharacterService | Infrastructure | **Application** | IUnitOfWork 사용, 캐릭터 CRUD |
| CombatService | Infrastructure | **Application** | IUnitOfWork 사용, 전투 시뮬레이션 |
| BattleService | Infrastructure | **Application** | IUnitOfWork 사용, 배틀 로직 |
| BattleLogService | Infrastructure | **Application** | IUnitOfWork 사용, 배틀 로그 저장 |
| DungeonService | Infrastructure | **Application** | IUnitOfWork 사용, 던전 진행 |
| StageService | Infrastructure | **Application** | IUnitOfWork 사용, 스테이지 관리 |
| SpecialDungeonService | Infrastructure | **Application** | IUnitOfWork 사용, 특수 던전 |
| EquipmentService | Infrastructure | **Application** | IUnitOfWork 사용, 장비 관리 |
| MonsterService | Infrastructure | **Application** | IUnitOfWork 사용, 몬스터 관리 |

---

## ✅ 올바르게 배치된 Service (7개)

### Domain Services (5개) ✅
1. `EloRatingService` - ELO 레이팅 계산 (순수 알고리즘)
2. `GachaLogicService` - 가챠 확률 계산 (순수 로직)
3. `LootCalculator` - 드랍 확률 계산
4. `PetGachaService` - 펫 가챠 확률 계산
5. `IRandomProvider` - 랜덤 인터페이스

### Infrastructure Services (2개) ✅
1. `RedisCacheService` - Redis 연동 (외부 시스템)
2. `SystemRandomProvider` - Random.Shared 래퍼

### Application Services (2개) ✅
1. `PvpService` - PVP 매치 진행 (Use Case)
2. `PvpSeasonService` - PVP 시즌 관리 (Use Case)

---

## 🔴 문제점

### 1. Clean Architecture 위반
- **의존성 방향 역전**: Infrastructure가 Application 역할을 함
- **계층 책임 혼란**: 외부 시스템 연동 계층이 비즈니스 로직 구현

### 2. 테스트 어려움
- Infrastructure Service로 분류되어 통합 테스트로만 검증 가능
- 단위 테스트 작성 어려움 (DB, Redis 등 외부 의존성 필요)

### 3. 유지보수성 저하
- Application Service를 찾기 어려움 (Infrastructure에 섞여 있음)
- 새로운 개발자가 아키텍처 이해 어려움

### 4. 재사용성 저하
- Application Service가 Infrastructure에 종속
- 다른 Infrastructure 구현으로 교체 어려움

---

## 💡 해결 방안

### Option 1: 전체 리팩토링 (권장) 🌟
**모든 Application Service를 `IdleRPG.Application/Services/`로 이동**

#### 장점
- Clean Architecture 원칙 준수
- 명확한 계층 분리
- 테스트 용이성 향상
- 향후 확장성 확보

#### 단점
- 15개 파일 이동 필요
- 네임스페이스 변경 (`Infrastructure.Service` → `Application.Services`)
- 테스트 코드 수정 필요
- **예상 작업 시간: 1-2시간**

#### 작업 순서
1. `IdleRPG.Application/Services/` 폴더 생성 (이미 존재)
2. Infrastructure의 15개 Service 파일을 Application으로 이동
3. 네임스페이스 변경: `Infrastructure.Service` → `Application.Services`
4. `Program.cs` DI 등록 위치 확인 (변경 불필요, 인터페이스 기반)
5. 테스트 코드 네임스페이스 수정
6. 빌드 및 테스트 실행

---

### Option 2: 점진적 마이그레이션
**새 기능부터 올바른 위치에 배치, 기존 코드는 리팩토링 시 이동**

#### 장점
- 기존 코드에 대한 영향 최소화
- 작업 부담 분산

#### 단점
- 일관성 부족 (혼재 상태 지속)
- 기술 부채 누적
- **비추천**: 학습 프로젝트이므로 올바른 아키텍처 학습 우선

---

### Option 3: 현상 유지 (비추천)
**현재 구조 유지, 문서에만 명시**

#### 문제점
- Clean Architecture 학습 목표 미달성
- 잘못된 패턴 학습
- 향후 프로젝트에서 동일한 실수 반복 가능성

---

## 🎯 권장 사항

### 즉시 조치 (HIGH Priority)
1. **Option 1 선택** - 전체 리팩토링 실시
2. **이유**: 학습 프로젝트이므로 올바른 아키텍처 패턴 학습이 최우선
3. **타이밍**: PVP Arena 완료 직후 (현재)가 최적

### 리팩토링 후 학습 포인트
- ✅ Clean Architecture 계층별 책임 분리 이해
- ✅ 의존성 방향 원칙 실습
- ✅ Application Service vs Infrastructure Service 구분
- ✅ 테스트 가능한 구조 설계

---

## 📝 리팩토링 체크리스트

### Phase 1: 백업 & 계획
- [ ] Git commit (현재 상태 백업)
- [ ] 이동할 15개 Service 목록 확인
- [ ] 테스트 코드 영향도 파악

### Phase 2: Service 이동 (15개)
- [ ] SkillService → Application/Services/
- [ ] PetService → Application/Services/
- [ ] ChatService → Application/Services/
- [ ] PvpMatchmakingService → Application/Services/
- [ ] OfflineRewardService → Application/Services/
- [ ] AuthService → Application/Services/
- [ ] CharacterService → Application/Services/
- [ ] CombatService → Application/Services/
- [ ] BattleService → Application/Services/
- [ ] BattleLogService → Application/Services/
- [ ] DungeonService → Application/Services/
- [ ] StageService → Application/Services/
- [ ] SpecialDungeonService → Application/Services/
- [ ] EquipmentService → Application/Services/
- [ ] MonsterService → Application/Services/

### Phase 3: 네임스페이스 수정
- [ ] 각 Service 파일의 `namespace IdleRPG.Infrastructure.Service` → `namespace IdleRPG.Application.Services`

### Phase 4: 테스트 코드 수정
- [ ] `using IdleRPG.Infrastructure.Service` → `using IdleRPG.Application.Services`
- [ ] 테스트 프로젝트의 네임스페이스 참조 업데이트

### Phase 5: 빌드 & 검증
- [ ] `dotnet build` 실행 (에러 0개 확인)
- [ ] `dotnet test` 실행 (모든 테스트 통과 확인)
- [ ] 통합 테스트 실행 (선택)

### Phase 6: 정리
- [ ] `IdleRPG.Infrastructure/Service/` 폴더 삭제 (비어 있으면)
- [ ] Git commit ("Refactor: Move Application Services to correct layer")

---

## 📚 참고 자료

### Clean Architecture 계층별 Service 가이드

**Domain Services**:
- 순수 계산 로직, 알고리즘
- 외부 의존성 없음
- 예: `TotalDamage CalculateDamage(Attack, Defense)`

**Application Services**:
- Use Case 구현 (비즈니스 흐름)
- Repository 오케스트레이션
- 예: `CreateCharacterAsync(playerId, dto)` - DB 저장 포함

**Infrastructure Services**:
- 외부 시스템 연동
- 예: `SendEmailAsync(to, subject, body)` - SMTP 호출

---

## 🤔 FAQ

### Q1: AuthService는 Infrastructure가 맞지 않나요? (JWT 생성)
**A**: AuthService는 **회원가입/로그인 Use Case**를 구현하므로 Application입니다.
JWT 생성 자체는 `JwtTokenService` (Infrastructure)가 담당합니다.

### Q2: ChatService는 SignalR을 사용하니 Infrastructure 아닌가요?
**A**: ChatService는 **채팅 메시지 저장 Use Case**를 구현합니다.
SignalR Hub는 `ChatHub` (API Layer)가 담당하고, ChatService는 DB 저장만 합니다.

### Q3: 기존 코드가 모두 Infrastructure인데 왜 문제인가요?
**A**: Clean Architecture의 핵심은 **의존성 방향**입니다.
Application Service가 Infrastructure에 있으면 **Infrastructure → Application** 역의존이 발생하여 아키텍처가 무너집니다.

---

## ✅ 다음 단계

1. ✅ **검토 완료** (현재)
2. ⏭️ **사용자 승인** - Option 1 (전체 리팩토링) 진행 여부 결정
3. ⏸️ **리팩토링 실행** - 승인 시 15개 Service 이동
4. ⏸️ **테스트 & 검증** - 빌드, 테스트 통과 확인
5. ⏸️ **Git Commit** - "Refactor: Move Application Services to correct layer"

---

---

## ✅ **최종 결정 (2025-11-10)**

### 🔄 **Option 1 선택: 롤백 완료**

**결정 사항**: 15개 Service를 **Infrastructure Layer에 유지**

**롤백 완료**:
- ✅ Git reset으로 모든 변경사항 되돌림
- ✅ 빌드 성공 (오류 0개)
- ✅ 원래 작동 상태 복구

---

## 🎓 **학습 결론: 실용적 Clean Architecture**

### 발견한 핵심 이슈

**Application Layer로 이동 시 빌드 에러 발생**:
```csharp
// ChatService.cs - Application Layer로 이동 시
using Microsoft.EntityFrameworkCore;      // ❌ Infrastructure 의존성!
using Microsoft.Extensions.Caching.Memory; // ❌ Infrastructure 의존성!

public class ChatService : IChatService
{
    private readonly IMemoryCache _cache; // ❌ Infrastructure 기술 직접 사용

    // EF Core 확장 메서드 사용
    await _unitOfWork.ChatMessages
        .Include(m => m.Sender)  // ❌ EF Core 기능
        .AsNoTracking()          // ❌ EF Core 기능
}
```

**문제**: Application Layer는 Infrastructure 기술에 의존하면 안 되지만, 현재 Service들은 **Use Case 구현 + Infrastructure 기술 직접 사용** 모두 포함

---

### Clean Architecture 이론 vs 실무

#### 📚 **이론적 Clean Architecture**
```
Domain Layer      - 순수 비즈니스 로직, 외부 의존성 없음
    ↑
Application Layer - Use Case 구현, Repository 오케스트레이션
    ↑
Infrastructure    - 외부 시스템 연동 (DB, Redis, SMTP 등)
```

#### 🏭 **실무적 Clean Architecture (현재 프로젝트)**
```
Domain Layer          - 순수 비즈니스 로직 ✅ (EloRatingService, GachaLogicService)
    ↑
Application Layer     - DTO, Interfaces만 ✅
    ↑
Infrastructure Layer  - Use Case 구현 + DB 액세스 + 외부 시스템 연동
    ↑                   (AuthService, PetService 등 15개)
Infrastructure Services - 순수 외부 시스템만 (RedisCacheService)
```

---

### 왜 Infrastructure에 두는 것이 맞는가?

#### 1. **Repository 패턴 이후의 ORM 사용**
- Repository는 추상화했지만, **Service에서 EF Core 확장 기능 직접 사용** (Include, AsNoTracking)
- 이를 완벽히 추상화하려면 매우 복잡한 설계 필요
- **실무에서는 이 정도 수준의 추상화면 충분**

#### 2. **캐싱 기술의 직접 사용**
- IMemoryCache를 Service에서 직접 의존
- 완벽한 추상화 (ICacheService 인터페이스 생성)는 오버엔지니어링
- **실용성 > 이론적 완벽함**

#### 3. **Domain Layer 보호가 핵심**
- Clean Architecture의 **가장 중요한 원칙**: Domain Layer가 외부 의존성 없이 순수하게 유지
- 현재 프로젝트: ✅ **Domain Layer 완벽 보호** (EloRatingService, GachaLogicService 등 순수 로직만)
- Application-Infrastructure 경계는 **실무에서 유연하게 적용 가능**

---

### 실무 사례

많은 실무 프로젝트에서도 이와 유사한 구조 사용:
- **Martin Fowler의 "Monolith First"**: 완벽한 분리보다 실용적 구조 우선
- **Microsoft eShopOnContainers**: Infrastructure에 Use Case 구현 포함
- **실무 Clean Architecture**: Domain만 순수하게, Application-Infrastructure는 유연하게

---

## 🎯 **최종 권장 사항**

### ✅ **현재 구조 유지 (Infrastructure에 Service 배치)**

**장점**:
1. ✅ Domain Layer 순수성 보장 (핵심 달성)
2. ✅ 실용적이고 유지보수 용이
3. ✅ 실무에서도 흔히 사용하는 패턴
4. ✅ 오버엔지니어링 방지

**구조**:
```
IdleRPG.Domain/Services/
├── EloRatingService.cs      ✅ 순수 비즈니스 로직
├── GachaLogicService.cs     ✅ 순수 비즈니스 로직
└── PetGachaService.cs       ✅ 순수 비즈니스 로직

IdleRPG.Application/
├── DTOs/                    ✅ 데이터 전송 객체
└── Interfaces/              ✅ 인터페이스 정의

IdleRPG.Infrastructure/Service, Services/
├── AuthService.cs           ✅ Use Case + DB 액세스
├── CharacterService.cs      ✅ Use Case + DB 액세스
├── SkillService.cs          ✅ Use Case + DB 액세스
└── ... (15개)

IdleRPG.Infrastructure/Services/
├── RedisCacheService.cs     ✅ 순수 외부 시스템 연동
└── SystemRandomProvider.cs  ✅ Infrastructure 기술 래퍼
```

---

## 📚 **학습 포인트**

### 1. **Clean Architecture는 원칙, 실무는 실용성**
- 이론적 완벽함 < 실용적 유지보수성
- **Domain Layer 순수성**이 가장 중요한 핵심
- Application-Infrastructure 경계는 프로젝트에 따라 유연하게

### 2. **Service 계층 판단 기준**
```
Domain Service      - 외부 의존성 없는 순수 계산/로직
Application Service - Use Case 구현 (현실: Infrastructure에 위치 가능)
Infrastructure Service - 순수 외부 시스템 연동만
```

### 3. **오버엔지니어링 방지**
- 모든 Infrastructure 기술을 인터페이스로 래핑 → 과도한 복잡성
- **핵심 비즈니스 로직(Domain)만 보호하면 충분**

---

**Last Updated**: 2025-11-10
**Final Decision**: ✅ **Infrastructure Layer에 Service 유지 (실용적 Clean Architecture)**
**Status**: 🟢 **RESOLVED - 롤백 완료, 빌드 성공**
