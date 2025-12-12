# 테스트 인프라 구축 로드맵

> **목적**: IdleRPG 서버의 테스트 및 성능 모니터링 인프라 구축
> **시작일**: 2025-11-14
> **컨텍스트**: Context window 제약 극복을 위한 진행 상황 추적 문서

---

## 📊 현재 상황 (2025-11-14)

### 이미 구축된 테스트
- ✅ **단위 테스트**: 130개 이상
  - Domain 계층: GachaLogic, PetGacha, EloRating
  - Application 계층: ChatService, PvpService
  - Infrastructure 계층: Repositories, Services

- ✅ **일부 통합 테스트**:
  - SkillController, ChatController, PvpController

### 부족한 부분
- ✅ **성능 모니터링 도구** (MiniProfiler, EF Core 로깅) - 완료!
- ✅ **보안 테스트** (AuthController 통합 테스트) - 완료!
- ❌ **코드 커버리지 측정** (목표 설정)
- ❌ **부하 테스트** (K6 스크립트)

---

## 🎯 구축 로드맵

### Phase 1: 개발 도구 (가장 우선!)
#### ✅ 1. MiniProfiler 설정 (N+1 쿼리 감지) - 완료
**목표**: 개발 중 실시간으로 N+1 쿼리 감지

**작업 내용**:
- [x] NuGet 패키지 설치
  - `MiniProfiler.AspNetCore.Mvc` ✅
  - `MiniProfiler.EntityFrameworkCore` ✅
- [x] `Program.cs` 설정
  - `AddMiniProfiler()` 서비스 등록 ✅
  - `UseMiniProfiler()` 미들웨어 추가 ✅
- [x] `appsettings.Development.json` EF Core 로깅 활성화 ✅
- [x] 테스트 엔드포인트로 검증
  - 주요 엔드포인트 쿼리 분석 완료 ✅
  - `/profiler/results-index` 접속하여 쿼리 분석 ✅

**학습 포인트**:
- `.Include()` / `.ThenInclude()` 사용법 ✅
- Lazy Loading vs Eager Loading 차이 ✅
- 쿼리 최적화 전략 ✅
- SQL 비율을 통한 성능 병목 분석 ✅

**실제 소요 시간**: 검증 및 분석 포함 45분

**완료 기준**:
- ✅ MiniProfiler UI에서 각 API 호출별 쿼리 개수 확인 가능
- ✅ N+1 문제 검증 완료 (모든 주요 엔드포인트 최적화 상태 확인)

**완료 결과**:
- Pvp/GetRankings: 단일 JOIN 쿼리, SQL 14% (최적화 양호)
- Character/GetById: 단일 쿼리, SQL 27% (최적화 양호)
- Pets/GetEquippedPets: 2단계 JOIN, SQL 9% (최적화 양호)
- 성능 분석 결과: `docs/testing/PERFORMANCE_ANALYSIS.md` 참조

---

### Phase 2: 보안 테스트
#### ✅ 2. AuthController 통합 테스트 - 완료
**목표**: JWT 인증 로직의 안전성 검증

**작업 내용**:
- [x] `IdleRPG.Tests/API/Controllers/AuthControllerTests.cs` 생성 ✅
- [x] 테스트 케이스 작성 (15개 통과): ✅
  - 회원가입 성공 시나리오 ✅
  - 중복 아이디 회원가입 실패 ✅
  - 로그인 성공 및 JWT 토큰 발급 ✅
  - 잘못된 비밀번호 로그인 실패 ✅
  - 인증 없이 접근 시 401 반환 ✅
  - Refresh Token으로 Access Token 갱신 ✅
  - 잘못된 Refresh Token 거부 ✅
  - `[Authorize]` 속성 보호 확인 (Logout, Profile) ✅
  - SQL Injection 방어 검증 ✅
  - 사용자 존재 여부 노출 방지 검증 ✅
  - 비밀번호 검증 (길이, 일치) ✅
- [x] CustomWebApplicationFactory 수정 (Testing 환경 설정) ✅
- [x] JwtTokenHelper 구현 (테스트용 JWT 토큰 생성) ✅
- [x] Program.cs 환경 분기 추가 (Testing 환경 대응) ✅
- [x] 실행 및 통과 확인 (15개 전부 통과) ✅

**학습 포인트**:
- JWT 토큰 구조 (Header, Payload, Signature) ✅
- `Bearer` 인증 스킴 ✅
- Claims 기반 인증 메커니즘 ✅
- Token Expiration 처리 (UtcNow 사용) ✅
- Refresh Token 패턴 ✅
- 보안 테스트 방법론 (SQL Injection, 정보 노출 방지) ✅

**실제 소요 시간**: 2시간 (테스트 인프라 개선 포함)

**완료 기준**:
- ✅ 15개 테스트 케이스 작성 및 통과 (목표 8개 초과 달성)

**완료 결과**:
- Public 엔드포인트: 11개 테스트 통과 (Register, Login, Refresh, 보안 검증)
- Authorized 엔드포인트: 4개 테스트 통과 (Logout, Profile)
- 보안 검증: SQL Injection 방어, 사용자 존재 노출 방지, 비밀번호 검증
- 테스트 인프라: CustomWebApplicationFactory 개선, JwtTokenHelper 추가
- 상세 결과: `docs/testing/PERFORMANCE_ANALYSIS.md` 참조

---

### Phase 3: 품질 측정
#### ⬜ 3. 코드 커버리지 측정
**목표**: 현재 테스트가 코드의 몇 %를 커버하는지 확인

**작업 내용**:
- [ ] Coverlet 설정 확인 (이미 설치됨)
- [ ] 커버리지 측정 실행:
  ```bash
  dotnet test --collect:"XPlat Code Coverage"
  ```
- [ ] 리포트 생성 도구 설치:
  ```bash
  dotnet tool install -g dotnet-reportgenerator-globaltool
  ```
- [ ] HTML 리포트 생성:
  ```bash
  reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coveragereport -reporttypes:Html
  ```
- [ ] 커버리지 목표 설정:
  - Domain: 90% 이상
  - Application: 80% 이상
  - Infrastructure: 70% 이상
  - API: 60% 이상
- [ ] 미달 영역 파악 및 테스트 추가 계획 수립

**학습 포인트**:
- 코드 커버리지의 의미와 한계
- Line Coverage vs Branch Coverage
- 테스트 우선순위 설정 (중요 비즈니스 로직 우선)

**예상 소요 시간**: 30분

**완료 기준**:
- HTML 리포트에서 계층별 커버리지 확인
- 테스트 추가가 필요한 파일 3개 이상 식별

---

### Phase 4: 성능 테스트
#### ⬜ 4. K6 부하 테스트 스크립트
**목표**: 로컬 환경에서 동시 접속 부하 시뮬레이션

**작업 내용**:
- [ ] K6 설치:
  - Windows: `choco install k6` 또는 공식 사이트에서 다운로드
- [ ] 테스트 스크립트 작성 (`tests/load/basic-load-test.js`):
  - 시나리오 1: 캐릭터 조회 (GET `/api/characters/{id}`)
  - 시나리오 2: 스킬 가챠 (POST `/api/skills/gacha`)
  - 시나리오 3: PVP 매칭 (POST `/api/pvp/match`)
- [ ] 부하 단계 설정:
  - Ramp-up: 10명 → 30초
  - Peak: 50명 → 1분
  - Ramp-down: 0명 → 30초
- [ ] 로컬 서버 실행 후 테스트:
  ```bash
  k6 run tests/load/basic-load-test.js
  ```
- [ ] 결과 분석:
  - 평균 응답 시간
  - 95 percentile 응답 시간
  - 에러율
  - RPS (Requests Per Second)
- [ ] 병목 지점 파악 (DB 쿼리, CPU, 메모리)

**학습 포인트**:
- 부하 테스트 시나리오 설계
- VU (Virtual User) 개념
- Throughput vs Latency
- 병목 지점 분석 방법

**예상 소요 시간**: 1.5시간

**완료 기준**:
- 3개 시나리오 스크립트 작성
- 동시 접속 50명 기준 테스트 실행 성공
- 응답 시간 200ms 이하 또는 개선 방안 문서화

---

## 📈 진행 상황

| Phase | 항목 | 상태 | 완료일 | 비고 |
|-------|------|------|--------|------|
| 1 | MiniProfiler 설정 | ✅ 완료 | 2025-11-14 | N+1 문제 없음 확인, 성능 분석 완료 |
| 2 | AuthController 테스트 | ✅ 완료 | 2025-11-14 | 15개 테스트 통과, 보안 검증 완료 |
| 3 | 코드 커버리지 | ⬜ 대기 | - | - |
| 4 | K6 부하 테스트 | ⬜ 대기 | - | - |

---

## 🔗 참고 자료

### MiniProfiler
- 공식 문서: https://miniprofiler.com/dotnet/AspDotNetCore
- EF Core 통합: https://miniprofiler.com/dotnet/HowTo/ProfileEFCore

### K6
- 공식 문서: https://k6.io/docs/
- 설치 가이드: https://k6.io/docs/get-started/installation/

### Coverlet
- GitHub: https://github.com/coverlet-coverage/coverlet
- 리포트 생성기: https://github.com/danielpalme/ReportGenerator

---

## 💡 다음 대화 시작 방법

새로운 대화 시작 시 아래 프롬프트 사용:

```
docs/testing/TESTING_ROADMAP.md를 확인하고,
진행 중이거나 다음 단계의 작업을 이어서 진행해줘.
```

이렇게 하면 컨텍스트를 빠르게 복원하고 바로 작업을 이어갈 수 있습니다.

---

**최종 업데이트**: 2025-11-14 (Phase 1-2 완료: MiniProfiler 설정, AuthController 보안 테스트 완료)
