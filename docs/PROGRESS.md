# 프로젝트 진행 상황

> **Last Updated**: 2025-10-21
> **Current Phase**: Week 3 - Core Vertical Slice
> **전체 로드맵**: `docs/learning/PROJECT_ROADMAP.md` 참고

---

## 📊 현재 상태 (Quick View)

| Category | Status | Progress |
|----------|--------|----------|
| **인증/인가** | ✅ 완료 | 5/5 endpoints |
| **캐릭터 시스템** | ✅ 완료 | 6/6 endpoints |
| **장비 시스템** | ✅ 완료 | 7/7 endpoints |
| **전투 시스템** | ⏳ 진행 중 | Core logic 완료 |
| **던전 시스템** | ⏳ 진행 중 | DB/Repository 완료 |
| **오프라인 보상** | ✅ 기본 완료 | Background service |
| **스킬 시스템** | 📋 예정 | 가챠 DTO 작성 완료 |

---

## 🎯 현재 작업 중 (2025-10-21)

### 던전 시스템
- [x] DungeonStage 엔티티 설계
- [x] Repository 구현
- [x] Seeder 작성 (5개 던전, 각 10스테이지)
- [x] DB 마이그레이션 적용
- [ ] **보상 계산 로직 구현** ← 현재 작업

#### 진행 중인 결정 사항
**던전 보상 계산 방식:**
- ✅ 서버 사이드 계산 확정
- 이유: 치팅 방지, 보상 밸런스 중앙 관리
- 대안 고려: 클라이언트 계산 → 보안 문제로 기각

### 스킬 시스템 준비
- [x] 스킬 가챠 DTO 작성 완료
- [ ] 스킬 엔티티 설계
- [ ] 스킬 적용 로직 (전투 시스템 연동)

---

## ✅ 완료된 시스템

### Week 1-2 (2025-01-10 ~ 2025-01-20)

#### 1. 인증/인가 시스템 ✅
- JWT Bearer 토큰 (Access 15분, Refresh 7일)
- BCrypt 비밀번호 해싱
- 5개 엔드포인트: Register, Login, Refresh, Logout, Profile

**주요 결정:**
- Redis 세션 대신 JWT 선택 (복잡도 감소)
- Refresh Token은 DB 저장 (탈취 시 무효화 가능)

#### 2. 캐릭터 시스템 ✅
- 자동 스탯 성장 (레벨별 공식)
- 6개 엔드포인트: Create, Get, List, Delete, LevelUp, Stats
- Unit Test 17개 통과

**기술적 배움:**
- Clean Architecture 계층 분리
- Repository 패턴
- AutoMapper DTO 매핑

#### 3. 장비 시스템 ✅
- 7개 엔드포인트: Create, List, Equip, Unequip, Delete, GetEquipped, GetStats
- Inventory 관리 (OwnerId vs CharacterId 분리)
- 장비 타입: Weapon, Helmet, Armor, Boots, Gloves

**설계 결정:**
- OwnerId: 소유자 (가방에 있음)
- CharacterId: 장착자 (몸에 착용 중)
- Nullable CharacterId로 장착/미장착 구분

#### 4. 전투 시스템 (Core) ✅
- Auto-battle 로직
- 턴제 전투 계산
- Battle log 저장
- Monster 엔티티 (5종 시드 데이터)

**전투 공식:**
```csharp
Damage = Attacker.Attack - Defender.Defense
FinalDamage = Max(Damage, 1) // 최소 1 데미지 보장
```

#### 5. 오프라인 보상 (기본) ✅
- IHostedService 백그라운드 작업
- 접속 시간 기록
- 보상 계산 (시간 * 시간당 보상)

**설계 한계 인식:**
- 현재: 단순 시간 * 보상
- 개선 필요: 던전 진행도 기반 보상 (Week 3-4)

#### 6. DevOps 인프라 ✅
- Jenkins CI/CD 파이프라인
- AWS EC2 + RDS (PostgreSQL)
- Docker 기반 배포
- 자동 마이그레이션 적용

---

## 📋 다음 우선순위

### Week 3 완료 목표
1. **던전 시스템 완성**
   - 보상 계산 로직
   - 스테이지 진행 API
   - Unity 문서화

2. **장비 강화 시스템 설계**
   - 확률 기반 강화
   - 실패 시 패널티 결정 필요

3. **스킬 시스템 기초**
   - 스킬 엔티티
   - 스킬 가챠
   - 전투 적용 (간단한 버프/디버프)

### Week 4-5 예정
- SignalR 실시간 채팅 (Deep Learning 목표)
- MediatR CQRS 패턴 적용
- FluentValidation 전면 도입

---

## 🎓 학습 진행 상황

### 완료한 기술 스택 (95% 이해도)
- ✅ ASP.NET Core 8.0 Web API
- ✅ Entity Framework Core 9.0
- ✅ PostgreSQL (Indexes, Relations)
- ✅ JWT Authentication
- ✅ Clean Architecture
- ✅ Repository Pattern
- ✅ AutoMapper
- ✅ Serilog
- ✅ xUnit + Moq + FluentAssertions
- ✅ Jenkins CI/CD
- ✅ Docker Compose

### 학습 중 (60-80% 이해도)
- ⏳ IHostedService (기본 사용 완료, 고급 패턴 학습 중)
- ⏳ EF Core 고급 (Performance tuning, N+1 문제)

### 예정 학습 (Week 4+)
- 📋 SignalR (Real-time communication)
- 📋 MediatR (CQRS pattern)
- 📋 FluentValidation
- 📋 Redis (Ranking, Distributed locking)

---

## 🔍 중요 설계 결정 히스토리

### 1. JWT vs Redis Session (Week 1)
**결정**: JWT 선택
**이유**:
- 서버 부하 감소 (상태 비저장)
- 수평 확장 용이
- Redis 추가 의존성 불필요 (초기)

**트레이드오프**: Refresh Token은 DB 저장 필요 (탈취 대응)

### 2. Equipment OwnerId vs CharacterId (Week 3)
**결정**: 두 필드 분리
**이유**:
- OwnerId: 소유권 추적
- CharacterId: 장착 상태 (nullable)
- 거래 시스템 확장성

**대안**: IsEquipped boolean → 여러 캐릭터 지원 불가

### 3. 전투 시스템 서버 권위 (Week 2)
**결정**: 모든 전투 계산 서버 실행
**이유**: 치팅 방지 최우선
**트레이드오프**: 네트워크 부하 증가 → 허용 가능 (턴제 게임)

### 4. 던전 보상 계산 (Week 3)
**결정**: 서버 사이드 계산
**이유**: 보상 밸런스 중앙 관리, 치팅 방지
**구현 예정**: RewardCalculator service

---

## 📝 메모/이슈

### 해결된 이슈
- ✅ DungeonStages 마이그레이션 실패 → 수동 SQL 스크립트로 해결
- ✅ Jenkins 자동 마이그레이션 적용 확인 완료
- ✅ HTTPS 인증서 로컬 개발 환경 설정

### 알려진 제한사항
- 현재 오프라인 보상이 단순 시간 기반 (던전 진행도 미반영)
- 장비 강화 시스템 미구현 (Week 3 목표)
- 스킬 시스템 전투 연동 미완성

### 기술 부채
- [ ] N+1 쿼리 최적화 필요 (Character → Equipment)
- [ ] 전투 로그 용량 관리 전략 필요 (파티셔닝 고려)
- [ ] API Rate Limiting 미적용

---

## 🎯 8주 학습 목표 체크

| Week | 목표 | 상태 | 달성률 |
|------|------|------|--------|
| **1-2** | Core Game Loop (6 systems) | ✅ 대부분 완료 | 90% |
| **3** | Combat + Dungeon + Enhancement | ⏳ 진행 중 | 60% |
| **4-5** | SignalR + CQRS + Skills | 📋 예정 | 0% |
| **6-7** | Redis + Social + Raid | 📋 예정 | 0% |
| **8** | Monetization (6 systems) | 📋 예정 | 0% |

**전체 진행률**: 약 25% (20개 시스템 중 5개 완료, 2개 진행 중)

---

## 📚 참고 문서

- **전체 로드맵**: `docs/learning/PROJECT_ROADMAP.md`
- **PRD**: `docs/MUSHROOM_GAME_PRD.md`
- **환경 설정**: `docs/development/DEV_ENVIRONMENT_SETUP.md`
- **배포 가이드**: `docs/jenkins/DEPLOYMENT_GUIDE.md`
- **Unity 문서**: `docs/unity/UNITY_DOCUMENTATION_GUIDE.md`

---

## 🔄 업데이트 규칙

이 문서는 **프로젝트의 현재 상태를 반영하는 살아있는 문서**입니다.

**업데이트 시점:**
- ✅ 새 시스템 완료 시
- ⏳ 중요한 설계 결정 시
- 📋 주간 회고 시 (매주 금요일)
- 🔍 마일스톤 달성 시

**업데이트 담당:**
- 개발자가 직접 수정
- Claude는 작업 완료 시 업데이트 제안

**Git 커밋 메시지 예시:**
```
docs: Update PROGRESS.md - Dungeon system completed
```
