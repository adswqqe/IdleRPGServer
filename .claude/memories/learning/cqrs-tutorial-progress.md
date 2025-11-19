# CQRS 학습 진행 상황

> **학습 철학**: "작게 시작 → 즉시 실행 → 점진적 확장"
>
> 이 문서는 **진행 상황 추적**을 위한 파일입니다.
> 컨텍스트가 초기화되어도 체크리스트만 보면 이어서 진행할 수 있습니다.

---

## 📖 학습 방식 (AI 세션 간 일관성 유지용)

> **중요**: 모든 AI 세션에서 이 방식을 따라야 합니다!
>
> 교육학적 분석 결과 (Gemini 3 Pro 자문, 2025-11-19):
> - ❌ **완전 가이드 방식 (Option A)**: 능력의 착각 발생 → Step 7에서 막막함
> - ❌ **완전 직접 구현 (Option B)**: 인지 과부하 → 좌절 위험
> - ✅ **점진적 비계 제거 (Scaffolding Fading)**: 처음엔 시범 → 점차 독립

### 🎯 핵심 원칙

**"Boilerplate는 AI가, Business Logic은 학습자가"**

- **AI 담당**: 파일 생성, 네임스페이스, DI 등록, Repository 인터페이스, Entity/DTO 껍데기
- **학습자 담당**: `Handle()` 메서드 내부, 검증 규칙(`RuleFor()`), 도메인 이벤트 발행 로직

### 📊 Phase별 전략 (Step별 가이드)

| Step | Phase | 전략 | AI 역할 | 학습자 역할 | TODO(human) 사용 |
|------|-------|------|---------|-------------|-----------------|
| **1-2** | 🎬 모델링 | 가이드 중심 | 전체 코드 작성 + 상세 설명 | 실행 + 구조 이해 + 질문 | ❌ 사용 안 함 |
| **3-4** | ✏️ 빈칸 채우기 | Fill-in-the-blank | Entity/Repo/Controller 제공 | `Handle()` 핵심 로직 작성 (5-10줄) | ✅ TODO(human) 사용 |
| **5-6** | 💪 적극 참여 | Active Learning | 힌트만 제공 | Validator, Event Handler 직접 구현 | ✅ TODO(human) 사용 |
| **7** | 🚀 완전 독립 | Self-Implementation | 막힐 때만 조언 | 전체 기능 스스로 구현 | ✅ TODO(human) 사용 |

### 🔧 구체적 실행 가이드 (각 Step별)

#### **Step 1-2: 모델링 단계** 🎬
- **접근**: AI가 모든 코드 작성
- **이유**: MediatR 연결 방식, 폴더 구조 자체가 낯설기 때문
- **학습자 활동**:
  1. Swagger에서 직접 테스트
  2. 코드 읽고 `IRequest`, `IRequestHandler` 흐름 이해
  3. "왜 이 파일이 이 폴더에 있는지" 질문하며 구조 파악
- **TODO(human) 사용**: ❌ 없음

#### **Step 3-4: 빈칸 채우기** ✏️
- **접근**: Fill-in-the-blank (뼈대는 AI, 핵심은 학습자)
- **이유**: DB 연결과 Query 분리는 코드가 많아서 전체를 짜면 지침
- **AI 제공**:
  - Entity, Repository 인터페이스, Configuration
  - Controller, DTO 껍데기
  - Handler 클래스 구조 (생성자, 의존성 주입)
- **학습자 작성** (TODO(human) 위치):
  - `AcceptQuestCommandHandler`의 `Handle()` 메서드 내부 (5-7줄)
    ```csharp
    // TODO(human): Quest 객체 생성 및 저장
    // 힌트: new Quest(...), _unitOfWork.Quests.AddAsync(), SaveChangesAsync()
    ```
  - `GetActiveQuestsQueryHandler`의 조회 로직 (3-5줄)
    ```csharp
    // TODO(human): 활성 퀘스트 조회 및 DTO 변환
    // 힌트: Where(q => q.Status == QuestStatus.InProgress), Select(q => new QuestDto {...})
    ```
- **Learn by Doing 형식 사용**: ✅ 예

#### **Step 5-6: 적극 참여** 💪
- **접근**: Active Learning (힌트만 제공, 대부분 직접 구현)
- **이유**: Validation과 Domain Events는 핵심 패턴 체득이 목표
- **Step 5 (FluentValidation)**:
  - AI: `ValidationBehavior` 클래스는 제공 (보일러플레이트)
  - 학습자: `AcceptQuestCommandValidator` 전체 작성
    ```csharp
    // TODO(human): AcceptQuestCommandValidator.cs 전체 구현
    // Guidance:
    // - RuleFor(x => x.CharacterId).NotEmpty()
    // - RuleFor(x => x.QuestTemplateId).GreaterThan(0)
    // - 어떤 규칙이 필요한지 스스로 판단해보세요
    ```
- **Step 6 (Domain Events)**: 🎯 **가장 중요!**
  - AI: `QuestAcceptedEvent` 클래스만 정의
  - 학습자: Handler 2개 전체 작성
    ```csharp
    // TODO(human): QuestAcceptedEventHandler.cs 전체 구현
    // Context: Quest 수락 시 업적 확인 필요
    // Guidance:
    // - INotificationHandler<QuestAcceptedEvent> 구현
    // - Handle() 메서드에서 업적 확인 로직 (로그 출력)
    // - 느슨한 결합 체감하기

    // TODO(human): QuestAcceptedNotificationHandler.cs 전체 구현
    // Context: 퀘스트 수락 알림 전송
    // Guidance: 마찬가지로 INotificationHandler 구현
    ```
- **Learn by Doing 형식 사용**: ✅ 필수

#### **Step 7: 완전 독립** 🚀
- **접근**: Self-Implementation (온전히 학습자 몫)
- **AI 역할**: 막히는 부분만 힌트 제공, 코드 작성 안 함
- **학습자**: 옵션 A/B/C 중 1-2개 선택하여 처음부터 끝까지 구현

### ⚠️ AI에게 주는 지침 (모든 세션에서 준수)

1. **Step 1-2**: TODO(human) 사용 금지, 모든 코드 작성 후 상세 설명
2. **Step 3-4**:
   - 먼저 구조 파일 모두 생성 (Entity, Repository, Controller 등)
   - Handler의 `Handle()` 메서드에만 TODO(human) 표시
   - "Learn by Doing" 형식으로 요청
   - 5-10줄 정도의 핵심 로직만 요청
3. **Step 5-6**:
   - 보일러플레이트만 제공
   - Validator와 Event Handler는 학습자가 직접 작성
   - "Learn by Doing" 형식 필수
   - 막히면 힌트 제공하되 정답 코드는 주지 않음
4. **Step 7**:
   - 코드 작성 절대 금지
   - 질문에만 답변, 막히는 부분만 가이드
5. **"Learn by Doing" 요청 후**:
   - 즉시 멈추고 사용자 입력 대기
   - 추가 작업 진행하지 않음
   - 사용자가 코드 작성 후 계속 진행

---

## 📊 현재 상태

**시작일**: [미시작]
**마지막 업데이트**: [미시작]

**현재 진행 중인 Step**: 없음
**완료한 Step**: 없음
**현재 Git 태그**: [없음] (Step 완료 시 태그 생성)
**다음 할 일**: Step 1 시작

**총 진행률**: 0/7 Steps (0%)

> 💡 **중요**: 각 Step 완료 시 반드시 Git 커밋 + 태그를 생성하세요!
> 이는 컨텍스트 초기화 후 정확한 코드 상태 파악을 위한 필수 조건입니다.

---

## 🐛 문제 해결 기록

> **목적**: 학습 중 발생한 문제와 해결 방법을 기록하여 같은 실수 반복 방지
>
> 새 AI 세션에서 비슷한 문제 발생 시 즉시 참고 가능

### 문제 기록 템플릿

```markdown
### Step X 진행 중
- **문제 (날짜)**: [문제 설명]
- **증상**: [에러 메시지 또는 증상]
- **원인**: [근본 원인]
- **해결**: [해결 방법]
- **소요 시간**: [X분]
```

### 현재 기록

**[아직 문제 없음]**

---

## 🎯 학습 목표 (완료 시 체크)

이 튜토리얼을 완료하면 다음을 **코드로 이해**하게 됩니다:

- [ ] MediatR 기본 (Command/Query 패턴)
- [ ] CQRS 구조 (읽기/쓰기 분리)
- [ ] Pipeline Behavior (횡단 관심사 자동화)
- [ ] FluentValidation (검증 로직 분리)
- [ ] Domain Events (이벤트 기반 아키텍처)
- [ ] Clean Architecture 통합

---

## ✅ 진행 체크리스트

### **Step 1: MediatR Hello World** ⏱️ 15분

**목표**: MediatR가 동작하는지 확인

**작업 체크리스트**:
- [ ] MediatR NuGet 패키지 설치 (2개)
- [ ] `IdleRPG.Application/AssemblyMarker.cs` 생성
- [ ] `Program.cs`에 MediatR 등록
- [ ] `PingCommand.cs` 작성
- [ ] `PingCommandHandler.cs` 작성
- [ ] `PingController.cs` 작성
- [ ] Swagger에서 `POST /api/ping` 테스트 성공

**생성할 파일**:
```
IdleRPG.Application/
  └── AssemblyMarker.cs
  └── Commands/
      └── Ping/
          ├── PingCommand.cs
          └── PingCommandHandler.cs

IdleRPG.API/
  └── Controllers/
      └── PingController.cs
```

**수정할 파일**:
- `IdleRPG.API/Program.cs` (MediatR 등록)

**실행할 명령어**:
```bash
cd IdleRPG.Application
dotnet add package MediatR --version 12.4.0
dotnet add package MediatR.Extensions.Microsoft.DependencyInjection --version 11.1.0
```

**테스트 방법**:
1. `dotnet run --project IdleRPG.API`
2. Swagger 열기: http://localhost:5000/swagger
3. `POST /api/ping` 실행
4. 응답: `{ "message": "Pong! MediatR is working! 🎉" }`

**✅ 완료 조건**:
1. [ ] "Pong!" 메시지 응답 확인
2. [ ] 모든 변경사항 Git 커밋
   ```bash
   git add .
   git commit -m "feat: Complete Step 1 - MediatR Hello World

   - Install MediatR packages
   - Add AssemblyMarker
   - Implement PingCommand and Handler
   - Add PingController
   - Test via Swagger (Success)"
   ```
3. [ ] Git 태그 생성
   ```bash
   git tag step-1-complete
   ```
4. [ ] 문서 "현재 상태" 업데이트 (AI에게 요청: "Step 1 완료했어. 문서 업데이트해줘")

**학습 포인트**:
- `IRequest<TResponse>`: Command/Query 인터페이스
- `IRequestHandler<TRequest, TResponse>`: Handler 인터페이스
- `IMediator.Send()`: Command/Query 전송

---

### **Step 2: 첫 번째 실전 Command** ⏱️ 30분

**목표**: 퀘스트 수락 기능 (DB 없이 로직만)

**작업 체크리스트**:
- [ ] `Quest.cs` Entity 작성 (QuestStatus enum 포함)
- [ ] `AcceptQuestCommand.cs` 작성
- [ ] `AcceptQuestCommandHandler.cs` 작성 (로그만 출력)
- [ ] `QuestController.cs` 작성
- [ ] Swagger에서 테스트 (로그 확인)

**생성할 파일**:
```
IdleRPG.Domain/
  └── Entities/
      └── Quest.cs

IdleRPG.Application/
  └── Quests/
      └── Commands/
          └── AcceptQuest/
              ├── AcceptQuestCommand.cs
              └── AcceptQuestCommandHandler.cs

IdleRPG.API/
  └── Controllers/
      └── QuestController.cs
```

**테스트 방법**:
1. Swagger에서 `/api/auth/login` (JWT 토큰 받기)
2. "Authorize" 버튼 → Bearer 토큰 입력
3. `POST /api/quest/accept` 실행:
   ```json
   {
     "characterId": "기존-캐릭터-ID",
     "questTemplateId": 1
   }
   ```
4. 로그 확인: "Quest 1 accepted by Character ..."

**✅ 완료 조건**:
1. [ ] 로그에 퀘스트 수락 메시지 출력
2. [ ] Git 커밋
   ```bash
   git add .
   git commit -m "feat: Complete Step 2 - First Command Implementation

   - Add Quest Entity and QuestStatus enum
   - Implement AcceptQuestCommand and Handler (log only)
   - Add QuestController
   - Test via Swagger (Success)"
   ```
3. [ ] Git 태그 생성: `git tag step-2-complete`
4. [ ] 문서 "현재 상태" 업데이트

**학습 포인트**:
- Command와 Handler의 분리
- Controller는 얇게 (MediatR만 호출)
- record 타입으로 불변 객체

---

### **Step 3: DB 연동** ⏱️ 30분

**목표**: 실제로 퀘스트를 DB에 저장

**작업 체크리스트**:
- [ ] `IQuestRepository.cs` 인터페이스 작성
- [ ] `IUnitOfWork.cs`에 `Quests` 프로퍼티 추가
- [ ] `QuestRepository.cs` 구현
- [ ] `UnitOfWork.cs`에 QuestRepository 추가
- [ ] `GameDbContext.cs`에 `DbSet<Quest>` 추가
- [ ] `QuestConfiguration.cs` 작성 (EF Core 매핑)
- [ ] Migration 생성
- [ ] Migration 실행
- [ ] `AcceptQuestCommandHandler.cs` 수정 (DB 저장 로직 추가)
- [ ] Swagger 테스트 → DB 확인

**생성할 파일**:
```
IdleRPG.Domain/
  └── Interfaces/
      └── IQuestRepository.cs

IdleRPG.Infrastructure/
  └── Repositories/
      └── QuestRepository.cs
  └── Data/
      └── Configurations/
          └── QuestConfiguration.cs
```

**수정할 파일**:
- `IdleRPG.Domain/Interfaces/IUnitOfWork.cs`
- `IdleRPG.Infrastructure/UnitOfWork.cs`
- `IdleRPG.Infrastructure/Data/GameDbContext.cs`
- `IdleRPG.Application/Quests/Commands/AcceptQuest/AcceptQuestCommandHandler.cs`

**실행할 명령어**:
```bash
dotnet ef migrations add AddQuestEntity --project IdleRPG.Infrastructure --startup-project IdleRPG.API
dotnet ef database update --project IdleRPG.Infrastructure --startup-project IdleRPG.API
```

**테스트 방법**:
1. Swagger에서 `POST /api/quest/accept` 실행
2. DB 확인:
   ```sql
   SELECT * FROM "Quests" ORDER BY "AcceptedAt" DESC;
   ```
3. 데이터 확인: CharacterId, QuestTemplateId, Status = 0

**✅ 완료 조건**:
1. [ ] DB에 Quest 레코드 저장 확인
2. [ ] Git 커밋
   ```bash
   git add .
   git commit -m "feat: Complete Step 3 - DB Integration

   - Add IQuestRepository and implementation
   - Update UnitOfWork and GameDbContext
   - Add QuestConfiguration (EF Core mapping)
   - Create and apply migration (AddQuestEntity)
   - Update AcceptQuestCommandHandler with DB logic
   - Test: Quest data persisted in DB"
   ```
3. [ ] Git 태그 생성: `git tag step-3-complete`
4. [ ] 문서 "현재 상태" 업데이트

**학습 포인트**:
- Repository 패턴 + UnitOfWork
- EF Core Configuration (Fluent API)
- CQRS는 아키텍처 계층과 독립적

---

### **Step 4: Query 추가 (CQRS 분리)** ⏱️ 20분

**목표**: 진행 중인 퀘스트 목록 조회 (읽기 전용)

**작업 체크리스트**:
- [ ] `GetActiveQuestsQuery.cs` 작성
- [ ] `QuestDto.cs` 작성
- [ ] `GetActiveQuestsQueryHandler.cs` 작성
- [ ] `QuestController.cs`에 GET 엔드포인트 추가
- [ ] Swagger 테스트

**생성할 파일**:
```
IdleRPG.Application/
  └── Quests/
      └── Queries/
          └── GetActiveQuests/
              ├── GetActiveQuestsQuery.cs
              └── GetActiveQuestsQueryHandler.cs
```

**수정할 파일**:
- `IdleRPG.API/Controllers/QuestController.cs` (GET 메서드 추가)

**테스트 방법**:
1. Swagger에서 `GET /api/quest/active/{characterId}` 실행
2. Step 3에서 생성한 Quest 확인

**✅ 완료 조건**:
1. [ ] 진행 중인 퀘스트 목록 조회 성공
2. [ ] Git 커밋
   ```bash
   git add .
   git commit -m "feat: Complete Step 4 - Add Query (CQRS Separation)

   - Add GetActiveQuestsQuery and Handler
   - Add QuestDto
   - Update QuestController with GET endpoint
   - Test: Query returns active quests"
   ```
3. [ ] Git 태그 생성: `git tag step-4-complete`
4. [ ] 문서 "현재 상태" 업데이트

**학습 포인트**:
- **Command**: 쓰기 작업, 상태 변경
- **Query**: 읽기 작업, 상태 변경 없음
- 책임의 명확한 분리

---

### **Step 5: FluentValidation 추가** ⏱️ 20분

**목표**: 검증 로직을 Handler에서 분리

**작업 체크리스트**:
- [ ] FluentValidation NuGet 패키지 설치 (2개)
- [ ] `Program.cs`에 FluentValidation 등록
- [ ] `AcceptQuestCommandValidator.cs` 작성
- [ ] `ValidationBehavior.cs` 작성 (Pipeline)
- [ ] `Program.cs`에 ValidationBehavior 등록
- [ ] Swagger 테스트 (잘못된 요청 → 자동 검증)

**생성할 파일**:
```
IdleRPG.Application/
  └── Quests/
      └── Commands/
          └── AcceptQuest/
              └── AcceptQuestCommandValidator.cs
  └── Common/
      └── Behaviors/
          └── ValidationBehavior.cs
```

**수정할 파일**:
- `IdleRPG.API/Program.cs` (FluentValidation + Behavior 등록)

**실행할 명령어**:
```bash
cd IdleRPG.Application
dotnet add package FluentValidation --version 11.9.0
dotnet add package FluentValidation.DependencyInjectionExtensions --version 11.9.0
```

**테스트 방법**:
1. Swagger에서 잘못된 요청:
   ```json
   {
     "characterId": "00000000-0000-0000-0000-000000000000",
     "questTemplateId": -1
   }
   ```
2. 에러 응답 확인: "CharacterId is required", "QuestTemplateId must be greater than 0"

**✅ 완료 조건**:
1. [ ] Validation 에러 자동 발생 확인
2. [ ] Git 커밋
   ```bash
   git add .
   git commit -m "feat: Complete Step 5 - FluentValidation Integration

   - Install FluentValidation packages
   - Add AcceptQuestCommandValidator
   - Implement ValidationBehavior (Pipeline)
   - Register in Program.cs
   - Test: Auto validation on invalid requests"
   ```
3. [ ] Git 태그 생성: `git tag step-5-complete`
4. [ ] 문서 "현재 상태" 업데이트

**학습 포인트**:
- **Pipeline Behavior**: 모든 Request에 자동 적용
- 횡단 관심사(Cross-Cutting Concerns) 처리
- 검증 로직 완전 분리

---

### **Step 6: Domain Event** ⏱️ 30분 🎯 **핵심!**

**목표**: 퀘스트 수락 시 업적 확인 (느슨한 결합 체험)

**작업 체크리스트**:
- [ ] `QuestAcceptedEvent.cs` 작성
- [ ] `AcceptQuestCommandHandler.cs` 수정 (이벤트 발행)
- [ ] `QuestAcceptedEventHandler.cs` 작성 (업적 확인)
- [ ] `QuestAcceptedNotificationHandler.cs` 작성 (알림 전송)
- [ ] Swagger 테스트 → 로그 확인 (2개 Handler 독립 실행)

**생성할 파일**:
```
IdleRPG.Domain/
  └── Events/
      └── QuestAcceptedEvent.cs

IdleRPG.Application/
  └── Quests/
      └── EventHandlers/
          ├── QuestAcceptedEventHandler.cs
          └── QuestAcceptedNotificationHandler.cs
```

**수정할 파일**:
- `IdleRPG.Application/Quests/Commands/AcceptQuest/AcceptQuestCommandHandler.cs`
  (IMediator 주입, 이벤트 발행 코드 추가)

**테스트 방법**:
1. Swagger에서 `POST /api/quest/accept` 실행
2. 로그 확인 (순서대로 출력):
   ```
   Quest {QuestId} accepted, event published
   🏆 [Achievement] Checking achievements ...
   ✅ Achievement check completed ...
   📬 [Notification] Sending notification ...
   ```

**✅ 완료 조건**:
1. [ ] 하나의 Command 실행 시 두 개의 독립적인 Handler 자동 실행 확인
2. [ ] 로그에서 이벤트 흐름 확인 (Quest accepted → Achievement check → Notification)
3. [ ] Git 커밋
   ```bash
   git add .
   git commit -m "feat: Complete Step 6 - Domain Events (Event-Driven Architecture)

   - Add QuestAcceptedEvent
   - Update AcceptQuestCommandHandler to publish event
   - Implement QuestAcceptedEventHandler (Achievement check)
   - Implement QuestAcceptedNotificationHandler
   - Test: Multiple handlers respond to single event"
   ```
4. [ ] Git 태그 생성: `git tag step-6-complete`
5. [ ] 문서 "현재 상태" 업데이트

**학습 포인트 (가장 중요!)**:
- **느슨한 결합**: Command Handler는 Event Handler를 몰라도 됨
- **확장성**: 새 Handler 추가해도 기존 코드 수정 불필요
- **단일 책임**: 각 Handler가 하나의 일만
- **테스트 용이성**: Handler 각각 독립 테스트 가능

---

### **Step 7: 확장 & 리팩토링** ⏱️ 30-60분 (선택)

**목표**: 배운 패턴을 직접 적용해보기

**📋 TODO(human) - 직접 구현 (1-2개 선택)**:

**옵션 A: CompleteQuestCommand**
- [ ] `CompleteQuestCommand.cs` 작성
- [ ] `CompleteQuestCommandHandler.cs` 작성
  - Quest.Status = Completed로 변경
  - CompletedAt = DateTime.UtcNow
  - DB 저장
- [ ] `QuestCompletedEvent.cs` 작성
- [ ] `QuestCompletedEventHandler.cs` 작성 (보상 지급)
- [ ] Controller에 엔드포인트 추가
- [ ] 테스트

**옵션 B: GetCompletedQuestsQuery**
- [ ] `GetCompletedQuestsQuery.cs` 작성 (페이징 포함)
- [ ] `CompletedQuestDto.cs` 작성
- [ ] `GetCompletedQuestsQueryHandler.cs` 작성
- [ ] Repository에 메서드 추가
- [ ] Controller에 엔드포인트 추가
- [ ] 테스트

**옵션 C: Quest 중복 수락 방지**
- [ ] `AcceptQuestCommandValidator.cs` 수정
- [ ] Repository에 `IsQuestInProgressAsync()` 메서드 추가
- [ ] 비동기 Validation 구현
- [ ] 테스트 (중복 수락 시도 → 에러)

**✅ 완료 조건**:
1. [ ] 선택한 옵션 1-2개 구현 및 동작 확인
2. [ ] Git 커밋
   ```bash
   git add .
   git commit -m "feat: Complete Step 7 - Self Implementation

   - Implement [선택한 옵션 이름]
   - [구현한 내용 요약]
   - Test: [테스트 결과]"
   ```
3. [ ] Git 태그 생성: `git tag step-7-complete`
4. [ ] 문서 "현재 상태" 업데이트
5. [ ] CQRS 학습 완료! 🎉

**학습 포인트**:
- Step 1-6에서 배운 패턴 스스로 적용
- 막히는 부분은 기존 코드 참고
- "이렇게 하면 어떻게 될까?" 실험 환영!

---

## 📝 생성/수정 파일 전체 목록

> 컨텍스트 초기화 후 재개 시 참고

### **생성된 파일** (Step별로 업데이트)

**Step 1 완료 후**:
- `IdleRPG.Application/AssemblyMarker.cs`
- `IdleRPG.Application/Commands/Ping/PingCommand.cs`
- `IdleRPG.Application/Commands/Ping/PingCommandHandler.cs`
- `IdleRPG.API/Controllers/PingController.cs`

**Step 2 완료 후**:
- `IdleRPG.Domain/Entities/Quest.cs`
- `IdleRPG.Application/Quests/Commands/AcceptQuest/AcceptQuestCommand.cs`
- `IdleRPG.Application/Quests/Commands/AcceptQuest/AcceptQuestCommandHandler.cs`
- `IdleRPG.API/Controllers/QuestController.cs`

**Step 3 완료 후**:
- `IdleRPG.Domain/Interfaces/IQuestRepository.cs`
- `IdleRPG.Infrastructure/Repositories/QuestRepository.cs`
- `IdleRPG.Infrastructure/Data/Configurations/QuestConfiguration.cs`
- Migration: `YYYYMMDDHHMMSS_AddQuestEntity.cs`

**Step 4 완료 후**:
- `IdleRPG.Application/Quests/Queries/GetActiveQuests/GetActiveQuestsQuery.cs`
- `IdleRPG.Application/Quests/Queries/GetActiveQuests/GetActiveQuestsQueryHandler.cs`

**Step 5 완료 후**:
- `IdleRPG.Application/Quests/Commands/AcceptQuest/AcceptQuestCommandValidator.cs`
- `IdleRPG.Application/Common/Behaviors/ValidationBehavior.cs`

**Step 6 완료 후**:
- `IdleRPG.Domain/Events/QuestAcceptedEvent.cs`
- `IdleRPG.Application/Quests/EventHandlers/QuestAcceptedEventHandler.cs`
- `IdleRPG.Application/Quests/EventHandlers/QuestAcceptedNotificationHandler.cs`

### **수정된 파일**

**Step 1**:
- `IdleRPG.API/Program.cs` (MediatR 등록)

**Step 3**:
- `IdleRPG.Domain/Interfaces/IUnitOfWork.cs` (Quests 추가)
- `IdleRPG.Infrastructure/UnitOfWork.cs` (QuestRepository 추가)
- `IdleRPG.Infrastructure/Data/GameDbContext.cs` (DbSet<Quest> 추가)
- `IdleRPG.Application/Quests/Commands/AcceptQuest/AcceptQuestCommandHandler.cs` (DB 저장 로직)

**Step 4**:
- `IdleRPG.API/Controllers/QuestController.cs` (GET 메서드 추가)

**Step 5**:
- `IdleRPG.API/Program.cs` (FluentValidation + ValidationBehavior 등록)

**Step 6**:
- `IdleRPG.Application/Quests/Commands/AcceptQuest/AcceptQuestCommandHandler.cs` (이벤트 발행)

---

## 💡 재개 가이드 (컨텍스트 초기화 시)

### **새 세션에서 이어하기**

1. **이 파일(`cqrs-tutorial-progress.md`) 읽기**
2. **"현재 상태" 섹션 확인**
   - 어느 Step까지 완료했는지
   - 다음 할 일이 무엇인지
3. **해당 Step의 체크리스트 확인**
   - 어디까지 체크되어 있는지
   - 다음 체크할 항목 보기
4. **"생성/수정 파일 목록" 확인**
   - 어떤 파일들이 이미 만들어졌는지
   - 코드 확인 필요 시 파일 열어보기
5. **이어서 진행**

### **예시**

```
[새 세션 시작]

사용자: "CQRS 학습 이어서 해줘"

Claude: (cqrs-tutorial-progress.md 읽기)

  "Step 3 진행 중이시네요!

   체크리스트를 보니:
   - ✅ IQuestRepository, QuestRepository, QuestConfiguration 완성
   - ⬜ Migration 생성 ← 다음 할 일

   이 명령어 실행하시면 됩니다:

   dotnet ef migrations add AddQuestEntity --project IdleRPG.Infrastructure --startup-project IdleRPG.API

   실행해볼까요?"
```

---

## 📖 Quick Reference (핵심 개념)

### **CQRS 핵심**

**Command (쓰기)**:
- 상태를 변경하는 작업
- 예: AcceptQuestCommand, CompleteQuestCommand
- 반환: Result DTO (성공/실패)

**Query (읽기)**:
- 상태를 변경하지 않는 조회
- 예: GetActiveQuestsQuery
- 반환: DTO 또는 List<DTO>

**분리 이점**:
- 읽기/쓰기 최적화 가능
- 책임 명확
- 테스트 용이

### **MediatR 핵심 인터페이스**

```csharp
// Command/Query
public record MyCommand : IRequest<MyResult>;

// Handler
public class MyCommandHandler : IRequestHandler<MyCommand, MyResult>
{
    public Task<MyResult> Handle(MyCommand request, CancellationToken ct)
    {
        // 비즈니스 로직
    }
}

// Event
public record MyEvent : INotification;

// Event Handler
public class MyEventHandler : INotificationHandler<MyEvent>
{
    public Task Handle(MyEvent notification, CancellationToken ct)
    {
        // 이벤트 처리
    }
}

// Pipeline Behavior
public class MyBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        // 전처리
        var response = await next();
        // 후처리
        return response;
    }
}
```

---

## 🎓 학습 완료 체크리스트

**CQRS 기본**:
- [ ] MediatR IRequest, IRequestHandler 이해
- [ ] Command와 Query의 차이 체감
- [ ] Mediator 패턴의 장점 이해

**Pipeline Behavior**:
- [ ] ValidationBehavior 동작 원리
- [ ] 횡단 관심사 처리 방법
- [ ] IPipelineBehavior 인터페이스 이해

**Domain Events**:
- [ ] INotification 인터페이스
- [ ] 여러 Handler가 하나의 Event 처리
- [ ] 느슨한 결합의 실제 이점 체감

**Clean Architecture 통합**:
- [ ] CQRS가 계층 구조를 깨지 않음
- [ ] Domain → Application → API 흐름 유지
- [ ] Repository 패턴과 CQRS의 조화

---

## 📚 다음 단계 (심화)

Step 1-7 완료 후 도전:

- [ ] **TransactionBehavior 추가** - 모든 Command에 자동 트랜잭션
- [ ] **LoggingBehavior 추가** - Request/Response 자동 로깅
- [ ] **AuthorizationBehavior 추가** - CharacterId 소유권 자동 검증
- [ ] **다른 시스템에 적용** - 길드, 업적, 일일 미션 등

---

## 💭 메모 & 학습 노트

> 학습하면서 배운 점, 어려웠던 점, 궁금한 점을 자유롭게 적어보세요.

```
[여기에 자유롭게 작성]

예:
- MediatR Pipeline이 Spring AOP랑 비슷한 것 같다
- Domain Event를 언제 써야 할지 감이 잘 안 온다
- ValidationBehavior에서 에러 처리를 Global Exception Handler랑 연동하면 좋을 듯





```

---

## 🔍 트러블슈팅

**자주 발생하는 문제**:

1. **MediatR Handler가 자동 등록 안 됨**
   - 원인: AssemblyMarker가 없거나 잘못된 Assembly 지정
   - 해결: `typeof(Application.AssemblyMarker).Assembly` 확인

2. **Migration 생성 실패**
   - 원인: DbContext에 DbSet 추가 안 함
   - 해결: `GameDbContext.cs`에 `public DbSet<Quest> Quests => Set<Quest>();` 추가

3. **Validation 자동 적용 안 됨**
   - 원인: ValidationBehavior 등록 안 함
   - 해결: `Program.cs`에 `AddBehavior(...)` 확인

4. **Event Handler 실행 안 됨**
   - 원인: `_mediator.Publish()` 호출 안 함
   - 해결: Command Handler에서 이벤트 발행 코드 추가

---

**마지막 업데이트**: 2025-11-19 (초기 생성)
**완료 Step**: 0/7
**다음 세션 시작 시**: "현재 상태" 섹션부터 읽기
