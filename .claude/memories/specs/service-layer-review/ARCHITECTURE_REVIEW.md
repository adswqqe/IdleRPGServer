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

**Last Updated**: 2025-11-10
**Severity**: 🔴 CRITICAL
**Priority**: HIGH
**Estimated Time**: 1-2 hours
**Recommendation**: **Option 1 - 전체 리팩토링 즉시 실행**
