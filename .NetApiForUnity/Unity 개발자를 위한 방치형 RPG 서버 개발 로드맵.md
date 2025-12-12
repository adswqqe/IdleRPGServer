# 🎮 Unity 개발자를 위한 방치형 RPG 서버 개발 로드맵

## 📋 프로젝트 개요
**프로젝트명**: 미니 방치형 RPG 서버
**목표**: 1000명 동시 접속 + 상용 서비스 수준 품질
**기간**: 8주 완성 코스
**기술 스택**: [[.NET 8]] + [[ASP.NET Core]] + [[PostgreSQL]] + [[Docker]] + [[Unity]]

---

## 🗓️ 주차별 학습 계획

### [[Week 1 - Domain 모델링과 Clean Architecture]]
- **핵심 목표**: 방치형 RPG의 데이터 구조 설계 및 [[Entity Framework Core]] 마스터
- **주요 내용**: 
  - [[Player Entity]] 설계
  - [[Character System]] 구현  
  - [[Offline Reward System]] 기초
  - [[Repository Pattern]] 적용
- **산출물**: 완전한 도메인 모델 + 데이터베이스 마이그레이션
- **연결 기술**: [[Entity Framework Core]], [[PostgreSQL]]

### [[Week 2 - Authentication과 Authorization]]
- **핵심 목표**: [[JWT]] 기반 인증 시스템 구축
- **주요 내용**:
  - [[JWT Token]] 생성 및 검증
  - [[Role-based Access Control]]
  - [[Unity Login Integration]]
  - API 보안 강화
- **산출물**: 완전한 인증/인가 시스템
- **연결 기술**: [[JWT]], [[ASP.NET Core Identity]]

### [[Week 3 - Core Game Logic]]
- **핵심 목표**: 방치형 게임의 핵심 메커니즘 구현
- **주요 내용**:
  - [[Offline Reward Calculation]]
  - [[Auto Battle System]]
  - [[Stage Progression]]
  - [[Item Drop System]]
- **산출물**: 게임 로직 API 완성
- **연결 기술**: [[Background Services]], [[Hangfire]]

### [[Week 4 - Performance Optimization]]
- **핵심 목표**: 고성능 서버 최적화
- **주요 내용**:
  - [[Redis Caching]] 구현
  - [[Database Indexing]]
  - [[Connection Pooling]]
  - [[Query Optimization]]
- **산출물**: 1000명 동접 처리 가능한 서버
- **연결 기술**: [[Redis]], [[pgBouncer]]

### [[Week 5 - Real-time Features]]
- **핵심 목표**: [[SignalR]]을 통한 실시간 통신
- **주요 내용**:
  - [[Guild Chat System]]
  - [[Real-time Notifications]]
  - [[Live Leaderboard]]
  - [[Unity SignalR Integration]]
- **산출물**: 실시간 멀티플레이어 기능
- **연결 기술**: [[SignalR]], [[WebSocket]]

### [[Week 6 - Security Hardening과 Monitoring]]
- **핵심 목표**: 보안 강화 및 모니터링 시스템
- **주요 내용**:
  - [[Rate Limiting]]
  - [[SQL Injection Prevention]]
  - [[Application Monitoring]]
  - [[Error Tracking]]
- **산출물**: 보안이 강화된 모니터링 시스템
- **연결 기술**: [[Serilog]], [[Application Insights]]

### [[Week 7 - Load Testing과 Optimization]]
- **핵심 목표**: 부하 테스트 및 성능 튜닝
- **주요 내용**:
  - [[k6 Load Testing]]
  - [[Performance Profiling]]
  - [[Bottleneck Analysis]]
  - [[Scalability Testing]]
- **산출물**: 성능 검증된 서버
- **연결 기술**: [[k6]], [[BenchmarkDotNet]]

### [[Week 8 - Production Deployment와 CI/CD]]
- **핵심 목표**: 실제 서비스 배포 및 자동화
- **주요 내용**:
  - [[Docker Containerization]]
  - [[GitHub Actions CI/CD]]
  - [[Blue-Green Deployment]]
  - [[Health Checks]]
- **산출물**: 완전 자동화된 배포 파이프라인
- **연결 기술**: [[Docker]], [[Kubernetes]], [[GitHub Actions]]

---

## 🔗 기술 스택 연결 그래프

### Core Technologies
- [[.NET 8]] ← [[ASP.NET Core]] ← [[Entity Framework Core]]
- [[PostgreSQL]] ← [[pgBouncer]] ← [[Database Optimization]]
- [[Redis]] ← [[Caching Strategy]] ← [[Performance]]
- [[Docker]] ← [[Containerization]] ← [[Deployment]]

### Game-Specific Features
- [[Idle Game Mechanics]] ← [[Offline Rewards]] ← [[Background Processing]]
- [[Unity Integration]] ← [[API Client]] ← [[Real-time Sync]]
- [[Authentication]] ← [[JWT]] ← [[Security]]

### DevOps & Monitoring
- [[CI/CD Pipeline]] ← [[GitHub Actions]] ← [[Automated Testing]]
- [[Monitoring]] ← [[Logging]] ← [[Performance Metrics]]
- [[Load Testing]] ← [[Scalability]] ← [[Optimization]]

---

## 📊 학습 진행 상황

### 완료된 주차
- [ ] [[Week 1 - Domain 모델링과 Clean Architecture]]
- [ ] [[Week 2 - Authentication과 Authorization]]
- [ ] [[Week 3 - Core Game Logic]]
- [ ] [[Week 4 - Performance Optimization]]
- [ ] [[Week 5 - Real-time Features]]
- [ ] [[Week 6 - Security Hardening과 Monitoring]]
- [ ] [[Week 7 - Load Testing과 Optimization]]
- [ ] [[Week 8 - Production Deployment와 CI/CD]]

### 현재 진행 중
**Current**: [[Week 1 - Domain 모델링과 Clean Architecture]]

---

## 🎯 최종 목표

### 기술적 목표
- ✅ 1000명 동시 접속 처리
- ✅ API 응답시간 100ms 이하
- ✅ 99.9% 서버 가동률
- ✅ 완전 자동화된 배포

### 학습 목표  
- ✅ Unity → 서버 개발 완전 전환
- ✅ 실무 수준 아키텍처 설계 능력
- ✅ 성능 최적화 및 모니터링 스킬
- ✅ DevOps 파이프라인 구축 능력

---

## 📚 관련 자료

### 외부 링크
- [ASP.NET Core 공식 문서](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core 가이드](https://docs.microsoft.com/ef/core)
- [PostgreSQL 성능 튜닝](https://wiki.postgresql.org/wiki/Performance_Optimization)

### 내부 노트
- [[Unity와 서버 개념 매핑]]
- [[실무 개발 팁과 트러블슈팅]]
- [[성능 최적화 체크리스트]]

---

#게임서버개발 #Unity #ASP.NET #Entity-Framework #방치형RPG #서버아키텍처