# 🍄 IdleRPG Server 프로젝트 개요 (버섯키우기 완전판)

## 프로젝트 목적
**Primary Goal**: .NET Web API + Unity 네트워크 통신 완전 마스터
**Secondary Goal**: 상용 수준의 방치형 MMORPG 제작
**참고 게임**: 버섯키우기 (거의 모든 기능 구현)
**개발 기간**: 20주 (약 5개월)

## 프로젝트 스케일
- **20개 주요 시스템**: 인증부터 길드 레이드, SignalR 채팅, Redis 랭킹까지
- **Clean Architecture**: 4계층 구조 (API, Application, Domain, Infrastructure)
- **상용급 인프라**: AWS EC2, RDS PostgreSQL, Jenkins CI/CD, Docker
- **20+ 엔티티**: Player, Character, Monster, Equipment, Skill, Pet, Guild, Raid, Mail 등
- **100+ API 엔드포인트** 예상

## 게임 장르 및 핵심 메커니즘

**장르**: 방치형 MMORPG (Idle MMORPG)

**핵심 게임플레이 루프**:
```
던전 입장 → 자동 전투 → 보상 획득 → 캐릭터/장비/스킬 강화
→ PVP 도전 → 길드 활동 → 보스 레이드 → 다음 던전
```

**주요 특징**:
1. **방치형 게임플레이**: 오프라인에서도 자동 사냥, 로그인 시 보상 수령
2. **자동 전투 시스템**: 서버에서 턴제 전투 시뮬레이션
3. **소셜 기능**: 친구, 길드, 실시간 채팅 (SignalR)
4. **협동 컨텐츠**: 길드 레이드 (멤버들이 협력하여 보스 처치)
5. **경쟁 컨텐츠**: PVP 아레나 (ELO 매칭), 랭킹 시스템
6. **수익화**: 가챠, VIP 시스템, 상점
7. **라이브 운영**: 일일 미션, 출석 체크, 이벤트, 우편함

## 20개 시스템 목록

### Phase 1: Foundation (Week 1-6) - MVP 시스템
1. ✅ **인증 시스템** - JWT Bearer Token (Access + Refresh)
2. ✅ **캐릭터 성장** - 레벨업, 자동 스탯 증가, 다중 캐릭터
3. ⏳ **인벤토리 & 장비** - 아이템 관리, 장착, 스탯 적용
4. ⏳ **전투 시스템** - 서버 기반 자동 전투 시뮬레이션
5. ⏳ **오프라인 보상** - 시간 기반 보상 계산 (최대 12-48시간)
6. 📋 **던전 시스템** - 스테이지 진행, 난이도별 보상
7. 📋 **장비 강화** - 확률 기반 강화 (+0 ~ +15)

### Phase 2: Expansion (Week 7-12) - 핵심 게임플레이
8. 📋 **스킬 시스템** - 스킬 습득, 레벨업, 전투 중 자동 사용
9. 📋 **펫 시스템** - 펫 육성, 스탯 버프, 펫 스킬
10. 📋 **PVP 아레나** - ELO 매칭, 랭킹, 시즌 보상
11. 📋 **친구 시스템** - 친구 추가, 선물 보내기
12. 📋 **길드 시스템 (1부)** - 길드 생성, 멤버 관리, 권한 시스템
13. 📋 **길드 시스템 (2부)** - 길드 스킬, 길드 레이드

### Phase 3: Advanced (Week 13-18) - 소셜 & 수익화
14. 📋 **실시간 채팅** - SignalR Hub (전체, 길드, 귓속말)
15. 📋 **보스 레이드** - 협동 전투, 기여도 랭킹
16. 📋 **퀘스트 & 업적** - 메인 퀘스트, 일일 퀘스트, 업적
17. 📋 **일일 미션 & 출석** - 일일 리셋, 연속 출석 보상
18. 📋 **가챠 시스템** - 확률 공개, 천장 시스템
19. 📋 **상점 & VIP** - 유료 재화, VIP 혜택

### Phase 4: Polish (Week 19-20) - 라이브 운영
20. 📋 **랭킹 시스템** - Redis Sorted Set (레벨, PVP, 길드 랭킹)
21. 📋 **우편함 & 이벤트** - 시스템 메일, 기간 한정 이벤트

## 아키텍처 패턴
**Clean Architecture** 4계층 구조:
1. **API Layer** (`IdleRPG.API`): Controllers, Middleware, SignalR Hubs
2. **Application Layer** (`IdleRPG.Application`): Services, DTOs, Interfaces
3. **Domain Layer** (`IdleRPG.Domain`): Entities, Value Objects, Business Rules
4. **Infrastructure Layer** (`IdleRPG.Infrastructure`): EF Core, Repositories, External Services

**의존성 규칙**: API → Application → Domain ← Infrastructure

## 기술 스택

### 서버 (.NET)
- **Framework**: ASP.NET Core 8.0 Web API
- **ORM**: Entity Framework Core 9.0
- **Database**: PostgreSQL (RDS)
- **Caching**: Redis (랭킹, 분산 락, 세션)
- **Real-time**: SignalR (채팅)
- **Patterns**: CQRS (MediatR), Repository
- **Auth**: JWT Bearer Token
- **Validation**: FluentValidation
- **Mapping**: AutoMapper
- **Logging**: Serilog
- **Testing**: xUnit, Moq, FluentAssertions
- **Background**: IHostedService

### 클라이언트 (Unity)
- **Unity**: 2022 LTS
- **Language**: C# 10
- **Async**: UniTask
- **JSON**: Newtonsoft.Json
- **SignalR**: Microsoft.AspNetCore.SignalR.Client
- **UI**: TextMeshPro
- **Testing**: Unity Test Framework

### 인프라 (AWS)
- **EC2**: t3.micro (API 서버)
- **RDS**: PostgreSQL 14 (ap-northeast-2)
- **Jenkins**: CI/CD 파이프라인
- **Docker**: 컨테이너화 배포

## 현재 진행 상황

**✅ Week 1 완료**:
- JWT 인증 시스템 (5 endpoints)
- 캐릭터 성장 시스템 (6 endpoints)
- Monster 엔티티 (5종 시딩)
- Jenkins CI/CD + AWS 배포
- 단위 테스트 (17 tests)

**🔄 Week 2 진행 중**:
- 전투 시스템 핵심 로직
- 오프라인 보상 계산
- Background Service (IHostedService)
- 전투 로그 시스템

**📋 다음 우선순위 (Week 3-6)**:
- 인벤토리 & 장비 시스템
- 던전 진행 시스템
- 장비 강화 시스템
- 스킬 시스템 기초

## 학습 목표 (단계별)

**Phase 1 (Week 1-6)**: 
- RESTful API 설계
- EF Core, 마이그레이션
- JWT 인증/인가
- Background Services
- 게임 로직 서버 검증

**Phase 2 (Week 7-12)**:
- 복잡한 데이터 관계 (M:N, 자기 참조)
- 게임 밸런싱 (전투 공식, 확률)
- ELO 레이팅 시스템
- 트랜잭션 및 동시성

**Phase 3 (Week 13-18)**:
- SignalR 실시간 통신
- Redis 고급 활용 (캐싱, 랭킹)
- 수익화 시스템 설계
- 치팅 방지 (서버 권위 설계)

**Phase 4 (Week 19-20)**:
- 라이브 운영 시스템
- 이벤트 스케줄링
- 성능 모니터링
- Production 최적화

## 개발 원칙

1. **서버 권위 설계**: 중요 로직은 서버에서 계산 (전투, 보상, 가챠)
2. **클라이언트는 UI**: Unity는 표현 계층, 입력 전달만
3. **학습 우선**: 코드 품질 > 기능 완성도
4. **점진적 개선**: MVP → 확장 → 고급 → 완성
5. **테스트 작성**: 단위 테스트 (서버), E2E (Unity)
6. **문서화**: API 문서 (Swagger), Unity 통합 가이드

## 프로젝트 최종 목표

- **기술적 목표**: 풀스택 게임 개발 역량 확보
- **학습 목표**: .NET + Unity 네트워크 통신 완전 마스터
- **결과물**: 상용 수준의 방치형 MMORPG (포트폴리오급)
- **기간**: 20주 (2025년 10월 ~ 2026년 3월)
