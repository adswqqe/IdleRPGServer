# 프로젝트 로드맵

**프로젝트 기간**: 8주 (2025-10-14 ~ 2025-12-06)
**현재 진행**: Week 3 Day 2 (2025-10-20)

---

## Phase 1: Foundation (Week 1-3) - MVP 시스템

**목표**: 완전한 게임 루프 구현 (로그인 → 전투 → 성장)

### 시스템 목록
1. ✅ **인증 시스템** (Week 1)
2. ✅ **캐릭터 성장** (Week 1)
3. ✅ **전투 시스템** (Week 2)
4. ✅ **오프라인 보상** (Week 2)
5. ✅ **인벤토리 & 장비** (Week 3 Day 1)
6. ✅ **던전 시스템** (Week 3 Day 2) ⭐ NEW
7. 📋 **장비 강화** (Week 3-4 예정)

**진행률**: **6/7 완료 (86%)**

### 학습 목표 (T-shaped 전략: 95% 깊이)
- ✅ ASP.NET Core Web API (RESTful 설계)
- ✅ EF Core (Code-First, Migration, 복잡한 관계)
- ✅ PostgreSQL (인덱싱, 쿼리 최적화)
- ✅ JWT 인증 (Access + Refresh Token)
- ✅ Serilog (구조화된 로깅)
- ✅ xUnit + Moq (단위 테스트)
- ✅ IHostedService (Background Service - 개념)
- ✅ Repository Pattern + Clean Architecture
- ✅ ValueObject Pattern (DDD)

---

## Phase 2: Expansion (Week 4-5) - 핵심 게임플레이

**목표**: SignalR 실시간 통신 마스터 + 스킬/펫 시스템

### 시스템 목록
8. 📋 **스킬 시스템** (진행 중)
   - ✅ GachaLogicService 구현 (확률 로직)
   - ✅ SkillTemplate, PlayerSkill Entity 설계
   - TODO: API 엔드포인트, Seeder
9. 📋 **펫 시스템**
10. 📋 **실시간 채팅 (SignalR)** ← 깊게 학습 (90%)
11. 📋 **PVP 아레나**

**진행률**: **0/4 시작 (스킬 시스템 일부 완료)**

### 학습 목표 (SignalR 집중)
- ⭐ **SignalR** (90% 깊이)
  - 실시간 채팅 (전체, 길드, 귓속말)
  - WebSocket 기반 통신
  - 그룹 관리, Connection 관리
  - Unity SignalR Client 연동
- 📋 MediatR (CQRS 기본 - 70%)
- 📋 FluentValidation (DTO 검증 - 70%)
- 📋 AutoMapper (기본 매핑 - 60%)

---

## Phase 3: Social + Redis (Week 6-7) - 소셜 기능

**목표**: Redis 랭킹 시스템 마스터 + 길드 시스템

### 시스템 목록
12. 📋 **친구 시스템**
13. 📋 **길드 시스템 (Basic)** - 생성, 멤버 관리, 권한
14. 📋 **길드 레이드** - 협력 전투, 기여도
15. 📋 **랭킹 시스템 (Redis)** ← 깊게 학습 (90%)
16. 📋 **보스 레이드**

**진행률**: **0/5 미착수**

### 학습 목표 (Redis 집중)
- ⭐ **Redis** (90% 깊이)
  - Sorted Set으로 랭킹 시스템
  - 분산 락 (멀티 인스턴스 동시성)
  - 캐싱 전략 (던전 스테이지, 랭킹)
- 📋 StackExchange.Redis 클라이언트
- 📋 동시성 처리 (Race Condition 방지)

---

## Phase 4: Monetization (Week 8) - 수익화 & 운영

**목표**: 빠른 구현으로 넓은 시스템 커버 (60-70% 깊이)

### 시스템 목록
17. 📋 **퀘스트 & 업적** (70%)
18. 📋 **일일 미션 & 출석** (70%)
19. 📋 **가챠 시스템** (70% - 확률 로직 이미 구현됨)
20. 📋 **상점 & VIP** (60%)
21. 📋 **우편함 & 이벤트** (50%)

**진행률**: **0/5 미착수** (가챠 로직 일부 선행 구현)

### 학습 목표
- 📋 확률 로직 (가챠 - 70%) ← 일부 완료
- 📋 이벤트 스케줄링 (60%)
- 📋 Background Service 스케줄러

---

## 전체 진행률

**완료된 시스템**: 6개 / 21개 (29%)
**진행 중 시스템**: 1개 (스킬 시스템)

**Phase 별**:
- Phase 1 (MVP): 6/7 = 86% ✅
- Phase 2 (핵심): 0/4 = 0%
- Phase 3 (소셜): 0/5 = 0%
- Phase 4 (수익화): 0/5 = 0%

---

## 다음 우선순위

### Immediate (이번 주)
1. **스킬 가챠 API 완성**
   - SkillTemplate Seeder (Common/Rare/Epic/Legendary)
   - API Controller (POST /api/skills/gacha)
   - Unity 문서 업데이트

2. **Drop System**
   - 던전 클리어 시 Equipment 드랍
   - 드랍 확률 테이블 (DropRate by DungeonStage)

3. **Combat-Dungeon 통합**
   - DungeonStage의 Monster 스탯 활용
   - BattleLog DungeonStageId 기록 활용

### Short-term (다음 주)
4. **장비 강화 완성**
   - UI/UX 개선
   - 강화 실패 처리 (레벨 유지 vs 하락)

5. **펫 시스템 시작**
   - Pet Entity, PetTemplate
   - 스탯 버프 계산

### Mid-term (Week 4-5)
6. **SignalR 채팅**
   - Hub 구현 (전체, 길드, 귓속말)
   - Unity SignalR Client 연동
   - Connection 관리

7. **PVP 아레나 기초**
   - ELO 매칭 로직
   - 랭킹 시스템 (Redis 준비)

---

## 학습 전략 (T-shaped Learning)

### 깊게 학습 (95% 깊이)
**Phase 1 완료**:
- ✅ EF Core (Code-First, Migration, 복잡한 관계)
- ✅ PostgreSQL (인덱싱, 쿼리 최적화)
- ✅ Clean Architecture (계층 분리, 의존성 규칙)
- ✅ Repository Pattern
- ✅ ValueObject Pattern (DDD)

**Phase 2-3 예정**:
- 📋 SignalR (90% - Week 4-5)
- 📋 Redis (90% - Week 6-7)

### 넓게 학습 (60-80% 깊이)
- 📋 MediatR (CQRS - 70%)
- 📋 FluentValidation (70%)
- 📋 AutoMapper (60%)
- 📋 가챠 시스템 (70%) ← 일부 완료
- 📋 이벤트 스케줄링 (60%)

### 명시적 제외 (Out of Scope)
- ❌ IAP (In-App Purchase) - 플랫폼 연동 시간 과다
- ❌ FCM (Push Notification) - Firebase 설정 시간
- ❌ Application Insights - Serilog로 대체
- ❌ CDN, Load Balancer - 개념만 이해

---

## 주요 마일스톤

### ✅ Week 1 완료 (2025-10-14)
- JWT 인증 시스템
- 캐릭터 성장 시스템
- Jenkins CI/CD + AWS 배포
- 17 단위 테스트

### ✅ Week 2 완료 (2025-10-16)
- 전투 시스템 핵심 로직
- 오프라인 보상 계산
- 전투 로그 시스템
- Background Service 계획

### ✅ Week 3 Day 1 완료 (2025-10-16)
- Equipment System (7 endpoints)
- 장비 강화 (+0~+10)
- OwnerId + CharacterId 이중 FK
- 계산 속성 패턴

### ✅ Week 3 Day 2 완료 (2025-10-18 ~ 2025-10-20)
- **Dungeon System** (3 endpoints)
- 15 Stages, 3 Difficulties
- Progressive Unlock, First Clear Bonus
- ValueObject Pattern
- **스킬 가챠 로직** (GachaLogicService)
- 확률 시스템 검증 (단위 테스트 12개)

### 📋 Week 3-4 목표
- Drop System
- Combat-Dungeon 통합
- Skill System 완성
- Enhancement System 개선

### 📋 Week 4-5 목표
- SignalR 채팅 구현
- Pet System
- PVP 아레나 기초

### 📋 Week 6-7 목표
- Redis 랭킹 시스템
- 길드 시스템
- 길드 레이드

### 📋 Week 8 목표
- 가챠 시스템 완성
- 일일 미션, 퀘스트
- 우편함, 이벤트
- 최종 통합 테스트

---

## 기술 부채 관리

### Critical (즉시 해결)
- 없음 (현재)

### High (Week 4)
- [ ] N+1 쿼리 최적화 (Include → Projection)
- [ ] Redis 캐싱 도입 (던전 스테이지 목록)

### Medium (Week 5-6)
- [ ] MediatR CQRS 패턴 도입 검토
- [ ] AutoMapper 도입 검토
- [ ] Rate Limiting (API 호출 제한)

### Low (Week 7-8)
- [ ] Swagger Production 비활성화
- [ ] HTTPS 강제 적용
- [ ] Application Insights 연동 검토

---

## 리스크 관리

### 일정 리스크
- **SignalR 학습 곡선**: Week 4-5에 집중 학습 시간 확보
- **Redis 분산 락**: Week 6-7 복잡도 높음, 충분한 테스트 필요

### 기술 리스크
- **EF Core 성능**: N+1 쿼리 문제, Projection으로 해결
- **동시성 처리**: Redis 분산 락 도입 필요

### 완화 전략
- 매주 회고 및 우선순위 조정
- 복잡한 시스템은 MVP → 확장 전략
- 단위 테스트 커버리지 유지 (70%+)

---

## 성공 기준

### Phase 1 (MVP)
- ✅ 완전한 게임 루프 작동
- ✅ Production 배포 완료
- ✅ Unity 클라이언트 기본 연동

### Phase 2 (핵심)
- 📋 SignalR 실시간 채팅 구현
- 📋 스킬/펫 시스템 작동
- 📋 PVP 매칭 기초

### Phase 3 (소셜)
- 📋 Redis 랭킹 실시간 업데이트
- 📋 길드 레이드 협력 플레이
- 📋 친구 시스템

### Phase 4 (수익화)
- 📋 가챠 시스템 완성
- 📋 일일 미션 자동 리셋
- 📋 이벤트 스케줄링

### 최종 목표 (Week 8)
- 20개 시스템 모두 구현
- 단위 테스트 70%+ 커버리지
- Unity 클라이언트 완전 통합
- Production 안정 운영
