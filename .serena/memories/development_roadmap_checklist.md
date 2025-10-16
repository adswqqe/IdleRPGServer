# 🤖 AI 협업 학습 로드맵 - 2개월(8주) 집중

**프로젝트 타입**: 기술 스택 학습 프로젝트 (상용 출시 아님)  
**개발 방식**: AI (Claude Sonnet 4.5, Gemini 2.5 Pro, o3, Grok-4) 협업  
**기간**: 2개월 (8주), 매일 4-6시간  
**학습 전략**: T자형 (핵심 6개 깊게 95% + 나머지 14개 넓게 60-80%)  
**최종 합의**: 4개 AI 모델 Consensus (2025-10-16)

---

## 📊 전체 진행률: 10% (2/20 시스템 완료)

**완료**: Auth (JWT), Character Growth  
**진행 중**: Week 3 Equipment 준비  
**다음**: Week 4 Gacha, Week 5 Enhancement

---

## 🔵 Phase 1: Core Vertical Slice (Week 1-3)

**목표**: "로그인 → 전투 → 장비 → 성장" 완전한 게임 루프 구현  
**전략**: Item Axis 완성 우선 (Equipment → Gacha → Enhancement)

### Week 1: 인증 & 캐릭터 ✅ 완료

**시스템 1: 인증 (JWT)** ✅ 깊이: 95%
- [x] Player 엔티티
- [x] RefreshToken 엔티티
- [x] JWT 토큰 서비스 (Access + Refresh)
- [x] AuthService (회원가입, 로그인, 토큰 갱신)
- [x] AuthController (5개 엔드포인트)
- [x] BCrypt 비밀번호 해싱
- [x] Jenkins CI/CD 파이프라인
- [x] AWS EC2 + RDS 배포

**시스템 2: 캐릭터 성장** ✅ 깊이: 95%
- [x] Character 엔티티
- [x] CharacterStats (Value Object)
- [x] 자동 스탯 성장 시스템
- [x] CharacterService (CRUD, 경험치, 레벨업)
- [x] CharacterController (6개 엔드포인트)
- [x] Monster 엔티티 (5종 시딩)
- [x] 단위 테스트 (17 tests)
- [x] Unity API 문서 작성
- [x] **Serilog 설정** (첫날부터 필수)

**학습 성과**:
- ✅ RESTful API 설계
- ✅ EF Core 마이그레이션
- ✅ JWT 인증/인가
- ✅ Clean Architecture 구조
- ✅ xUnit 단위 테스트
- ✅ **구조화된 로깅 (Serilog)**

---

### Week 2: 전투 시스템 완성 ✅ 완료

**시스템 4: 전투 시스템** ✅ 깊이: 95%
- [x] Monster 엔티티
- [x] BattleService (턴제 전투 시뮬레이션)
- [x] 데미지 계산 공식 (서버 검증)
- [x] 크리티컬/회피 처리
- [x] BattleResultDto
- [x] BattleController (3개 엔드포인트)
- [x] 경험치/골드 보상 지급
- [x] **BattleLog 시스템** (전투 기록)
- [x] **OfflineReward 시스템** (시간 기반 보상)
- [x] 단위 테스트

**시스템 5: 오프라인 보상** ✅ 깊이: 70%
- [x] OfflineRewardType 엔티티
- [x] OfflineRewardService (시간 기반 계산)
- [x] RewardController (조회, 수령)
- [x] LastLoginTime 기반 계산
- [x] MaxMinutes 제한 적용

**시스템 7: 전투 로그** ✅ 깊이: 70%
- [x] BattleLog 엔티티
- [x] BattleLogRepository
- [x] 전투 결과 자동 저장
- [x] 페이징 조회

**학습 성과**:
- ✅ 게임 밸런싱 (전투 공식)
- ✅ DateTime 처리
- ✅ Unit of Work 패턴
- ✅ 버섯키우기 스타일 분석 (Direct-equip)

---

### Week 3: Equipment System 🔥 **우선순위 10/10**

**목표**: Item Axis 시작 - Equipment 기반 구축  
**합의**: 4/4 모델 만장일치 (Claude, Gemini, o3, Grok-4)

**시스템 3: Equipment & Combat Integration** 🔄 깊이: 95%

**Day 1-2: Equipment 엔티티 (Domain Layer)**
- [ ] Equipment 엔티티
  - EquipmentSlot enum (Weapon, Helmet, Armor, Gloves, Boots)
  - EquipmentRarity enum (Common, Rare, Epic, Legendary)
  - AttackBonus, DefenseBonus fields
  - **Immutable Value Object 설계**
- [ ] CharacterEquipment 엔티티 (1:N 관계)
  - CharacterId, EquipmentId (FK)
  - EquipmentSlot, EquippedAt
- [ ] EF Core Configuration
- [ ] Migration 생성

**Day 3-4: Direct-Equip 로직 (Application Layer)**
- [ ] IEquipmentRepository (Repository Pattern)
- [ ] EquipItemUseCase
  - 현재 장비 조회
  - 스탯 비교 (ATK + DEF 합산)
  - 더 좋으면 자동 교체, 나쁘면 자동 판매
- [ ] UnequipItemUseCase
- [ ] SellEquipmentUseCase (골드 지급)
- [ ] GetEquippedItemsUseCase
- [ ] Unit of Work 통합

**Day 5-6: Combat Integration (Infrastructure Layer)**
- [ ] BattleService 리팩토링
  - Character.GetTotalAttack() - 장비 스탯 합산
  - Character.GetTotalDefense() - 장비 스탯 합산
  - 전투 시뮬레이션에 장비 효과 반영
- [ ] 전투 테스트 (장비 유무 차이 검증)

**Day 7: API & Testing**
- [ ] EquipmentController
  - POST /api/equipment/equip (자동 비교 로직)
  - POST /api/equipment/sell
  - GET /api/equipment/equipped/{characterId}
- [ ] 단위 테스트 (10+ tests)
- [ ] Unity API 문서 업데이트
- [ ] 통합 테스트

**Definition of Done (80%)**:
- ✅ In-Scope: Equipment 엔티티, Direct-equip, 스탯 합산, 자동 판매
- ❌ Out-of-Scope: Stash (보관함), 정렬/필터링, 아이템 잠금

**학습 목표**:
- 1:N 관계 (Character - CharacterEquipment)
- Value Object 패턴
- Repository Pattern
- **버섯키우기 Direct-equip UX**

---

## 🟢 Phase 2: Item Progression Loop (Week 4-5)

**목표**: Item Axis 완성 (Acquire → Upgrade)  
**전략**: RNG 서비스 재사용으로 학습 효율 극대화

### Week 4: Gacha System 🎰 **우선순위 9/10**

**합의**: 4/4 모델 만장일치  
**핵심**: IProbabilityService 설계 (Clean Architecture 학습의 정점)

**시스템 17: Gacha System** 🔄 깊이: 70%

**Day 1-2: RNG 서비스 설계 ⭐ 최우선**
- [ ] **IProbabilityService (Domain Interface)**
  ```csharp
  public interface IProbabilityService
  {
      T WeightedRandom<T>(Dictionary<T, double> weightTable);
      bool RollProbability(double successRate);
  }
  ```
- [ ] **ProbabilityService (Infrastructure Implementation)**
  - 가중치 기반 추첨 로직
  - Thread-safe Random 처리
- [ ] 단위 테스트 (확률 검증 10,000회 시뮬레이션)

**Day 3-4: Gacha 엔티티 및 로직**
- [ ] GachaPool 엔티티 (마스터 데이터)
  - Name, CostGold, IsActive
- [ ] DropTable 엔티티
  - GachaPoolId (FK)
  - EquipmentRarity, Weight (확률)
- [ ] PullGachaUseCase
  - IProbabilityService 주입 (DI)
  - WeightedRandom<EquipmentRarity> 호출
  - Equipment 생성 및 지급
- [ ] GachaRepository

**Day 5-6: Pity System & Currency**
- [ ] Character.GachaPullCount field 추가
- [ ] Simple Pity (10연차 보장)
- [ ] 골드 소모 로직
- [ ] GachaHistory 엔티티 (감사 로그)

**Day 7: API & UX Polish**
- [ ] GachaController
  - POST /api/gacha/pull (단발)
  - POST /api/gacha/pull-ten (10연)
  - GET /api/gacha/pools (풀 목록)
- [ ] Unity API 문서
- [ ] 단위 테스트 (10+ tests)

**Definition of Done (70%)**:
- ✅ In-Scope: 단발/10연, 확률, Pity 기본, 골드 소모
- ❌ Out-of-Scope: 천장 시스템, 유료 재화, 복잡한 Pity

**학습 목표**:
- **Interface Segregation (SOLID)**
- **Dependency Injection 고급**
- 확률 시스템 설계
- 마스터 데이터 관리

---

### Week 5: Enhancement System 🔨 **우선순위 8/10**

**합의**: 3/4 모델 (Gemini, o3, Claude)  
**핵심**: IProbabilityService 재사용, 트랜잭션 학습

**시스템 7: Enhancement System** 🔄 깊이: 95%

**Day 1-2: Enhancement 로직 설계**
- [ ] **IProbabilityService 재사용** ⭐
- [ ] EnhancementConfig 엔티티 (마스터 데이터)
  - EnhancementLevel (+0 ~ +10)
  - SuccessRate, FailureRate (레벨 유지)
  - GoldCost
- [ ] EnhanceEquipmentUseCase
  - 현재 장비 조회
  - RollProbability(successRate) 호출
  - 성공/실패 처리

**Day 3-4: 트랜잭션 처리 (핵심 학습)**
- [ ] **Unit of Work 패턴 활용**
- [ ] 트랜잭션 시나리오:
  - 성공: EnhancementLevel++, 골드 차감, SaveChanges
  - 실패: 레벨 유지, 골드 차감, SaveChanges
  - 에러: 롤백
- [ ] EnhancementMaterial 엔티티 (옵션)
- [ ] EnhancementHistory 엔티티 (로그)

**Day 5-6: Enhancement API**
- [ ] EnhancementController
  - POST /api/enhancement/enhance
  - GET /api/enhancement/config (확률 테이블)
  - GET /api/enhancement/history/{characterId}
- [ ] 단위 테스트 (15+ tests, 트랜잭션 시나리오)

**Day 7: 밸런싱 테스트**
- [ ] 확률 시뮬레이션 (10,000회)
- [ ] 골드 소모 밸런싱
- [ ] Unity API 문서

**Definition of Done (80%)**:
- ✅ In-Scope: +0~+10 강화, 확률, 트랜잭션, 골드 소모
- ❌ Out-of-Scope: 파괴 시스템, 안전 강화, +10 이상

**학습 목표**:
- **트랜잭션 처리 (깊게)**
- **RNG 서비스 재사용 (패턴 학습)**
- Unit of Work 고급
- 밸런싱 시뮬레이션

---

## 🟡 Phase 3: Combat Diversity & Content (Week 6-7)

**목표**: Skill Axis 추가 + Dungeon Content

### Week 6: Skill System 🎯 **우선순위 9/10**

**합의**: 3/4 모델 (Gemini, o3, Claude)

**시스템 8: Skill System** 🔄 깊이: 60%

**Day 1-3: Skill 엔티티 및 설계**
- [ ] Skill 엔티티 (마스터 데이터)
  - SkillType enum (Active/Passive)
  - TargetType enum (Single/Multi)
  - DamageMultiplier, ManaCost, Cooldown
- [ ] CharacterSkill 엔티티 (M:N)
  - CharacterId, SkillId
  - SkillLevel (기본 1)
- [ ] SkillEffect 설계
  - 단순 데미지 증가만
  - 버프/디버프 제외

**Day 4-6: Battle Integration**
- [ ] BattleService에 스킬 실행 추가
  - 전투 시작 시 스킬 확인
  - 턴마다 스킬 자동 사용 (확률)
  - 쿨다운 관리 (간단)
- [ ] 2-3개 샘플 스킬 구현
  - 강타 (1.5배 데미지)
  - 연타 (2회 공격)
  - 치명타 확률 증가 (Passive)
- [ ] Mana 시스템 (기본)

**Day 7: Skill API**
- [ ] SkillController
  - GET /api/skill/available (습득 가능)
  - POST /api/skill/learn
  - GET /api/skill/character/{id} (보유 스킬)
- [ ] 단위 테스트 (10+ tests)

**Definition of Done (60%)**:
- ✅ In-Scope: Active 스킬 2-3개, 자동 사용, 기본 쿨다운
- ❌ Out-of-Scope: 버프/힐/디버프, 스킬 레벨업, 스킬 트리

**학습 목표**:
- M:N 관계 기초
- 전투 시스템 확장
- 게임 밸런싱 (스킬 배율)

---

### Week 7: Dungeon System 🏰 **우선순위 7/10**

**합의**: 3/4 모델 (Gemini, o3, Claude)  
**핵심**: Content Feature - 시스템 완성 후 구현

**시스템 6: Dungeon System** 🔄 깊이: 95%

**Day 1-3: Dungeon 엔티티**
- [ ] Dungeon 엔티티 (마스터 데이터)
  - Name, RequiredLevel, StaminaCost
  - Difficulty enum (Normal만)
- [ ] DungeonStage 엔티티
  - DungeonId (FK), StageNumber
  - MonsterId (FK) - 기존 Monster 재사용
  - DropTableId (FK) - Gacha DropTable 재사용 ⭐
- [ ] DungeonProgress 엔티티
  - CharacterId, DungeonId
  - CurrentStage, ClearedAt

**Day 4-5: Dungeon 진행 로직**
- [ ] EnterDungeonUseCase
  - 입장 조건 검증 (레벨, 스태미나)
  - 스태미나 소모
- [ ] BattleDungeonStageUseCase
  - **BattleService 재사용** ⭐
  - 승리 시 다음 스테이지 진행
  - 패배 시 진행 중단
- [ ] CompleteDungeonUseCase
  - 보상 지급 (DropTable 사용)
  - 경험치/골드 지급

**Day 6-7: Dungeon API & SignalR 시작**
- [ ] DungeonController
  - POST /api/dungeon/enter
  - POST /api/dungeon/battle
  - GET /api/dungeon/progress/{characterId}
  - GET /api/dungeon/list (던전 목록)
- [ ] SignalR 기초 설정
  - ChatHub 생성
  - 연결 관리 테스트
- [ ] 단위 테스트 (8+ tests)

**Definition of Done (80%)**:
- ✅ In-Scope: 단일 던전, Normal 난이도, 스테이지 진행, 보상
- ❌ Out-of-Scope: 여러 난이도, 일일 제한, 랭킹

**학습 목표**:
- Combat 로직 재사용
- DropTable 재사용 (패턴 학습)
- 진행 상태 관리
- SignalR 기초

---

## 🟣 Phase 4: Social & Polish (Week 8)

**목표**: SignalR 완성 + 보조 시스템

### Week 8: SignalR & Secondary Features 📱 **우선순위 7/10**

**합의**: 4/4 모델 합의 (Phase 4 축소)

**시스템 13: 실시간 채팅 (SignalR)** 🔄 **깊이: 90%** ⭐

**Day 1-3: SignalR Chat (90% Depth)**
- [ ] ChatHub (SignalR Hub)
  - OnConnectedAsync, OnDisconnectedAsync
  - SendMessage (전체 채팅)
  - SendGroupMessage (그룹 채팅)
- [ ] ConnectionManager (연결 관리)
- [ ] ChatMessage 엔티티 (히스토리)
- [ ] Unity SignalR Client 통합 테스트
- [ ] 재연결 로직

**Day 4-5: Shop System (기본)**
- [ ] Shop 엔티티 (마스터 데이터)
  - ItemType (골드, 장비, 재료)
  - Price (골드)
- [ ] PurchaseItemUseCase
  - 골드 차감
  - 아이템 지급
- [ ] ShopController (3개 엔드포인트)

**Day 6-7: Quest & Polish**
- [ ] Quest 엔티티 (간단)
  - QuestType enum (Kill, Level)
  - Target, Reward
- [ ] QuestProgress 엔티티
- [ ] CompleteQuestUseCase
- [ ] 전체 시스템 통합 테스트
- [ ] Unity API 문서 최종 업데이트

**제외 시스템 (개념만)**:
- ❌ VIP System (50% depth로 축소 또는 제외)
- ❌ Mail System (skeleton만)
- ❌ Event System (제외)
- ❌ Pet System (제외)
- ❌ Daily Mission (기본만)

**학습 목표**:
- **SignalR Hub 구현 (깊게)**
- **실시간 통신 (깊게)**
- Unity SignalR Client
- 빠른 시스템 구현 경험

---

## 🚀 핵심 학습 포인트 (Gemini 강조)

### **IProbabilityService 패턴** ⭐⭐⭐

```csharp
// Domain Layer
public interface IProbabilityService
{
    T WeightedRandom<T>(Dictionary<T, double> weightTable);
    bool RollProbability(double successRate);
}

// Infrastructure Layer
public class ProbabilityService : IProbabilityService
{
    // Implementation
}
```

**재사용 예시**:
- **Gacha (Week 4)**: `WeightedRandom<EquipmentRarity>`
- **Enhancement (Week 5)**: `RollProbability(enhanceSuccessRate)`
- **Dungeon (Week 7)**: `WeightedRandom<Equipment>` (DropTable)

**학습 가치**: Clean Architecture의 핵심 (Interface Segregation, DI)

---

## 📈 학습 마일스톤

### Phase 1 완료 시 (Week 3)
- ✅ Equipment 시스템 완성
- ✅ 1:N 관계 마스터
- ✅ Direct-equip UX 이해
- ✅ Value Object 패턴

### Phase 2 완료 시 (Week 5)
- ✅ **IProbabilityService 설계 (핵심)**
- ✅ RNG 시스템 재사용
- ✅ 트랜잭션 처리 (깊게)
- ✅ Item Progression Loop 완성

### Phase 3 완료 시 (Week 7)
- ✅ Skill System (Combat Diversity)
- ✅ Dungeon Content
- ✅ 코드 재사용 경험
- ✅ SignalR 기초

### Phase 4 완료 시 (Week 8)
- ✅ SignalR 90% Depth
- ✅ 20개 시스템 개념 경험
- ✅ 핵심 6개: 95% 이해
- ✅ 포트폴리오 완성

---

## 🎯 2개월 후 최종 목표

**기술 역량**:
- ✅ .NET Web API 전반 이해
- ✅ PostgreSQL + EF Core 실무 활용
- ✅ **Clean Architecture 깊은 이해** (Interface, DI)
- ✅ 확률 시스템, 트랜잭션 처리
- ✅ SignalR 실시간 통신
- ✅ Unity 네트워크 통신 구현

**포트폴리오**:
- ✅ GitHub: "20개 시스템 Idle MMORPG"
- ✅ 핵심 6개 시스템 깊은 이해
  - Equipment, Gacha, Enhancement (95%)
  - Combat, Skill, Dungeon (90%)
- ✅ SignalR 90% depth
- ✅ 나머지 시스템 아키텍처 경험

**다음 단계 준비**:
- ✅ 강약점 파악
- ✅ 2개월 후 심화 학습 방향 결정
- ✅ 실무 투입 가능 수준 도달

---

## 📝 AI Consensus 이력

**2025-10-16 (4개 모델 합의)**:
- Claude Sonnet 4.5 (초기 9/10 → 조정 8/10)
- Gemini 2.5 Pro (추정 9/10) - IProbabilityService 강조
- o3 (8/10) - Gacha → Enhancement 순서 강조
- Grok-4 (8/10) - Skill 우선 의견 (소수)

**주요 변경사항**:
1. Week 3: Gacha 제외, Equipment + Combat만
2. Week 4: Gacha + RNG 서비스 설계
3. Week 5: Enhancement (Gacha 직후, 코드 재사용)
4. Week 6: Skill (Combat Diversity)
5. Week 7: Dungeon (Content Feature)
6. Week 8: SignalR 90% + 보조 시스템

**핵심 논리**:
- **Item Axis 완성 우선** (Equipment → Gacha → Enhancement)
- **코드 재사용 극대화** (IProbabilityService)
- **학습 효율** (RNG, 트랜잭션 패턴 집중 학습)
- **Phase 4 축소** (VIP/Mail/Event 제외)

---

**최종 업데이트**: 2025-10-16  
**합의 모델**: Claude, Gemini, o3, Grok-4 (4/4)  
**전략**: T자형 학습 + Item Axis 완성 우선  
**다음 작업**: Week 3 Equipment System 구현 시작
