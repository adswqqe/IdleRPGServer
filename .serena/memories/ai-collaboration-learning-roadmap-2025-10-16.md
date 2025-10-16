# 🤖 AI 협업 학습 로드맵 (2개월 집중)

**프로젝트 성격**: 상용 서비스 ❌ → **기술 스택 학습** ✅  
**개발 방식**: 혼자 개발 ❌ → **AI (Claude, Gemini, GPT) 협업** ✅  
**기간**: 20주 ❌ → **2개월 (8주)** ✅  
**학습 시간**: 매일 4-6시간 집중  
**검증**: Gemini 2.5 Pro 검증 완료 (2025-10-16)

---

## 🎯 학습 전략: T자형 학습

```
┌─────────────────────────────┐
│  핵심 시스템 (6개): 95%     │  ← 깊이 (Depth)
│  인증, 캐릭터, 인벤토리      │
│  전투, 던전, 장비 강화       │
└─────────────────────────────┘
        │
┌───────┴─────────────────────┐
│ 확장 시스템 (14개): 60-80%  │  ← 넓이 (Breadth)
└─────────────────────────────┘
```

**핵심 원칙**:
- **깊이 없는 넓이 = 피상적 이해**
- 핵심 6개는 트랜잭션, 동시성, 성능까지 깊게 파기
- 나머지 14개는 아키텍처 완성도를 위한 넓이

---

## 📅 8주 Phase 기반 로드맵

### 🔵 Phase 1: Core Vertical Slice (Week 1-3)

**목표**: "로그인 → 전투 → 성장" 완전한 사이클 구현

**시스템 (6개 - 깊이 95%)**:
1. ✅ 인증 (JWT)
2. ✅ 캐릭터 성장
3. 인벤토리 & 장비 착용
4. 전투 시스템 (턴제 시뮬레이션)
5. 던전 (1종류)
6. 장비 강화 (확률 기반)

**핵심 학습**:
- Clean Architecture 전체 계층 이해
- 1:N 관계 (Character - Inventory)
- 트랜잭션 처리 (장비 강화)
- 게임 밸런싱 (데미지 계산, 강화 확률)
- Repository + Unit of Work 패턴

**Definition of Done 예시 (인벤토리 80%)**:
- ✅ In-Scope: 아이템 획득/소모, 장비 장착/해제, DB 저장
- ❌ Out-of-Scope: 정렬/필터링, 아이템 잠금, 슬롯 확장, 다중 판매

**AI 협업 프로세스**:
- Day 1-2: 설계 및 아키텍처 (함께)
- Day 2-3: AI가 보일러플레이트 생성
- Day 3-4: 코드 리뷰 및 개념 이해 (학습자)
- Day 4-5: 핵심 로직 함께 작성, 실험

---

### 🟢 Phase 2: Core Features & Real-time (Week 4-5)

**목표**: 핵심 콘텐츠 + 실시간 통신 경험

**시스템 (4개 - 넓이 60-80%)**:
7. 스킬 시스템 (기본 공격 스킬만)
8. 오프라인 보상
9. 퀘스트 (간단한 목표 달성형만)
10. **실시간 채팅 (SignalR)** ← 깊게

**핵심 학습**:
- **SignalR Hub** (전체 채팅, 그룹 채팅)
- Unity SignalR Client
- IHostedService (백그라운드 작업)
- DateTime 처리

**Out-of-Scope**:
- 스킬: 복잡한 효과, 쿨다운 시스템 제외
- 퀘스트: 진행도 트래킹 고급 기능 제외

---

### 🟡 Phase 3: Social & Advanced (Week 6-7)

**목표**: 소셜 기능 + Redis 고급 활용

**시스템 (4개 - 넓이 60-80%)**:
11. 친구 시스템 (M:N 자기 참조)
12. 길드 시스템 (기본 - 생성/가입만)
13. **랭킹 시스템 (Redis Sorted Set)** ← 깊게
14. 보스 레이드 (간단 버전)

**핵심 학습**:
- **Redis Sorted Set** (레벨 랭킹, PVP 랭킹)
- M:N 자기 참조 관계
- 공유 상태 관리 (보스 HP)

**Out-of-Scope**:
- 길드: 길드전, 스킬, 기부 제외
- 레이드: 단순화된 누적 데미지만

---

### 🟣 Phase 4: Monetization & Utility (Week 8)

**목표**: 나머지 시스템 빠르게 훑기

**시스템 (6개 - 넓이 50-70%, 개념 위주)**:
15. 가챠 (확률 시스템)
16. 상점 (재화 구매, IAP 제외)
17. VIP (혜택 계산)
18. 일일 미션
19. 우편함
20. 펫 (간단 스탯 합산만)

**핵심 학습**:
- 확률 계산 (가챠)
- 일일 리셋 로직
- VIP 레벨별 혜택 설계

**제외 항목**:
- IAP 통합 (플랫폼 연동 시간 소모)
- FCM 푸시 알림 (학습 외적)
- Firebase Analytics (Serilog로 대체)

---

## 🛠️ 기술 스택 우선순위 (수정)

### ✅ 첫날부터 필수
1. **Serilog** - 구조화된 로깅
   ```csharp
   // Program.cs - 첫날 설정
   Log.Logger = new LoggerConfiguration()
       .WriteTo.Console()
       .WriteTo.File("logs/idlerpg-.log", rollingInterval: RollingInterval.Day)
       .CreateLogger();
   ```

### Week 1-3 (Phase 1)
2. JWT, EF Core, PostgreSQL
3. xUnit, Moq, FluentAssertions
4. Repository Pattern, Unit of Work

### Week 4-5 (Phase 2)
5. **SignalR** (실시간 채팅)
6. IHostedService (백그라운드)
7. FluentValidation, AutoMapper

### Week 6-7 (Phase 3)
8. **Redis** (캐싱, 랭킹)
9. MediatR (CQRS) - 옵션

### Week 8 (Phase 4)
10. 가챠, VIP 로직 (확률 계산)

### ❌ 제외 또는 개념만
- IAP (Google Play, App Store) - 플랫폼 설정 시간 소모
- FCM - 학습 외적
- Firebase Analytics - Serilog로 충분
- Application Insights - Phase 4 이후

---

## 🎓 AI 협업 학습 원칙

### 1. 당신이 설계, AI가 구현
```
❌ "전투 시스템 만들어줘" (수동적)
✅ "턴제 전투, 이 공식으로 구현해줘" (능동적)
```

### 2. 모든 코드를 이해하려 하지 말 것
```
✅ 이해 필수: 아키텍처, 핵심 로직, 패턴
⚠️ 가볍게: 보일러플레이트, DTO 매핑
❌ 무시: 자동 생성 코드 (마이그레이션)
```

### 3. AI에게 "왜"를 계속 질문
```
"왜 여기서 async/await를 사용했어?"
"MediatR 없이 직접 서비스 주입하는 장단점은?"
"이 트랜잭션 격리 수준을 선택한 이유는?"
```

### 4. 매주 기술 회고
```markdown
Week 1 회고:
- 배운 기술: JWT, EF Core, Repository
- 이해도: JWT (90%), EF Core (70%)
- 다음 주: EF Core 고급 쿼리 심화
```

### 5. Definition of Done 엄수
- 각 시스템 시작 전 In/Out-Scope 명확화
- Out-of-Scope는 과감히 자르기
- 완벽주의 = 실패

---

## ⚠️ 실패 위험 요소

### 1. 인지적 부하 관리 실패
**증상**: "코드는 돌아가는데 왜 그런지 모르겠음"  
**대책**: AI에게 "왜"를 계속 질문, 코드 리뷰 시간 충분히

### 2. 시스템 통합 복잡성 과소평가
**증상**: Week 7-8에 모든 것이 꼬임  
**대책**: Phase 1에서 통합을 미리 경험 (Vertical Slice)

### 3. 완벽주의 함정
**증상**: 한 시스템에 1주 이상 소요  
**대책**: DoD 엄수, 타이머 설정, Out-of-Scope 엄격히

---

## 📊 2개월 후 목표 상태

```
✅ .NET Web API 전반적 이해
✅ PostgreSQL + EF Core 능숙 사용
✅ JWT, Redis, SignalR 실무 활용 가능
✅ Unity 네트워크 통신 구현 능력
✅ CQRS, Repository 패턴 이해
✅ 20개 시스템 경험 (핵심 6개 95%, 나머지 60-80%)
✅ GitHub 포트폴리오: "20개 시스템 Idle RPG"
✅ 강약점 파악 → 2개월 후 심화 학습 방향 결정
```

---

## 🏆 성공 공식

```
성공 = Gemini의 전략 × Claude의 낙관 × 당신의 규율
```

- **Gemini의 전략**: T자형 학습, 의존성 기반 Phase, DoD
- **Claude의 낙관**: AI 협업으로 충분히 가능
- **당신의 규율**: 매일 4-6시간, DoD 엄수, "왜" 질문

---

**최종 업데이트**: 2025-10-16  
**검증**: Gemini 2.5 Pro (신뢰도 8/10)  
**상태**: 2개월 달성 가능 (조건부)
