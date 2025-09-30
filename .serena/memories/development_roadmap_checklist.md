# IdleRPG 서버 개발 로드맵 체크리스트

## 📚 우선순위별 학습 및 구현 계획

### ✅ 완료된 항목
- [x] Clean Architecture 프로젝트 구조 설정
- [x] JWT 인증 시스템 구현 (가입/로그인)
- [x] Entity Framework Core + PostgreSQL 연동
- [x] Docker 개발 환경 구축
- [x] Stage 1: 웹 서버 기초 학습

---

### 🎯 Phase 1: 핵심 게임 시스템 구현 (우선순위 높음)

#### 1️⃣ Database Mastery + 캐릭터 시스템
- [ ] **Stage 2: Database Mastery 학습**
  - EF Core 심화, 복잡한 쿼리 최적화
  - 마이그레이션 전략, ORM vs Raw SQL
- [ ] **캐릭터 생성 시스템 구현**
  - Character Entity CRUD API
  - 플레이어당 다중 캐릭터 관리
- [ ] **캐릭터 스탯 및 레벨링 시스템**
  - 경험치, 레벨업 로직
  - 스탯 자동 계산

#### 2️⃣ Async Performance + 방치형 핵심 로직
- [ ] **Stage 3: Async Performance 학습**
  - C# async/await vs Unity Coroutines
  - .NET GC 최적화, 동시성 처리
- [ ] **오프라인 보상 시스템 구현**
  - 로그아웃 시간 계산
  - 오프라인 진행 보상 계산 로직
  - Background Service 구현

---

### 🔐 Phase 2: 보안 및 아이템 시스템 (우선순위 중상)

#### 3️⃣ Security 심화 + 아이템 시스템
- [ ] **Stage 4: Security & JWT 심화 학습**
  - OWASP Top 10, Rate limiting
  - 게임 치팅 방지 전략
- [ ] **아이템 및 인벤토리 시스템 구현**
  - ItemTemplate, PlayerInventory Entity
  - 아이템 획득/사용/판매 API
  - 장비 착용/해제 시스템

---

### ⚡ Phase 3: 성능 최적화 및 소셜 기능 (우선순위 중)

#### 4️⃣ Redis + 캐싱 적용
- [ ] **Stage 5: Caching & Redis 학습**
  - 캐싱 전략, 분산 캐싱
  - 세션 관리, 상태 저장
- [ ] **Redis 캐싱 적용**
  - 플레이어 세션 캐싱
  - 자주 조회되는 게임 데이터 캐싱
  - 캐시 무효화 전략 구현

#### 5️⃣ 길드 시스템
- [ ] **길드 시스템 구현**
  - 길드 생성/가입/탈퇴
  - 길드 멤버 관리
  - 길드 레벨 및 혜택 시스템

---

### 💬 Phase 4: 실시간 통신 (우선순위 중)

#### 6️⃣ Monitoring + SignalR 채팅
- [ ] **Stage 6: Monitoring & Logging 학습**
  - Serilog 구조화 로깅
  - APM, 성능 모니터링
- [ ] **SignalR 실시간 통신 구현**
  - GameHub 설정
  - 연결/재연결 관리
- [ ] **채팅 시스템 구현**
  - 전체 채팅 (레벨 제한, 쿨다운)
  - 길드 채팅
  - 개인 채팅 (1:1, 오프라인 메시지)
  - 욕설 필터, 신고 시스템

---

### 🧪 Phase 5: 테스트 및 배포 (우선순위 중하)

#### 7️⃣ Testing + Deployment
- [ ] **Stage 7: Testing & Deployment 학습**
  - 단위/통합/E2E 테스트 전략
  - CI/CD 파이프라인
- [ ] **테스트 프로젝트 작성**
  - xUnit 테스트 프로젝트 생성
  - 핵심 비즈니스 로직 단위 테스트
  - API 통합 테스트
- [ ] **Docker 배포 설정**
  - Dockerfile 최적화
  - docker-compose 프로덕션 설정
  - CI/CD 파이프라인 구축 (GitHub Actions)

---

### 🚀 Phase 6: 확장성 및 고급 패턴 (우선순위 낮음)

#### 8️⃣ Scalability + 성능 최적화
- [ ] **Stage 8: Scalability & Architecture 학습**
  - 마이크로서비스, 로드밸런싱
  - 메시지 큐, 이벤트 기반 아키텍처
- [ ] **성능 최적화**
  - 데이터베이스 쿼리 최적화
  - N+1 문제 해결
  - 프로파일링 및 병목 지점 개선

#### 9️⃣ Advanced Patterns
- [ ] **Stage 9: Advanced Patterns 학습**
  - CQRS, Event Sourcing
  - DDD 심화
- [ ] **MediatR CQRS 패턴 적용**
  - Command/Query 분리
  - Pipeline Behavior 구현
  - FluentValidation 통합

---

## 🎓 학습 자료 구조

각 Stage는 **5-7일 과정** (Day당 오전 3시간 + 오후 3시간):
- 이론적 기초 → 실습 → 프로젝트 적용
- Unity 개발자 관점에서 패러다임 비교
- 전문 서적 수준의 깊이

---

## 📊 진행 상황 추적

### 우선순위 가이드
- **높음**: Phase 1 (핵심 게임 시스템) - 즉시 시작
- **중상**: Phase 2 (보안 및 아이템) - Phase 1 완료 후
- **중**: Phase 3-4 (최적화 및 소셜) - 기본 기능 완성 후
- **중하**: Phase 5 (테스트 및 배포) - 주요 기능 완성 후
- **낮음**: Phase 6 (확장성 및 고급) - 서비스 안정화 후

### 현재 위치
- **완료**: 프로젝트 초기 설정 및 인증 시스템
- **진행 중**: Phase 1 준비
- **다음 단계**: Stage 2 Database Mastery 학습 + 캐릭터 시스템 구현

---

## 🔄 업데이트 이력
- 2025-09-30: 초기 로드맵 작성, Phase 1-6 우선순위 정리