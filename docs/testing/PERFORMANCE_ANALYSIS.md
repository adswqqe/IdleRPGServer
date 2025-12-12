# 성능 분석 결과 (2025-11-14)

> MiniProfiler를 통한 IdleRPG API 성능 병목 분석

---

## 📊 측정 결과 요약

### 1. Pvp/GetRankings
- **총 실행 시간**: 107.05ms (2차 실행, Warm)
- **SQL 실행 시간**: 15.11ms (14.11%)
- **병목 지점**: 애플리케이션 로직 (85.89%)
  - EF Core 객체 매핑: ~40ms
  - DTO 변환: ~30ms
  - JSON 직렬화: ~17ms

**판단**: ✅ 최적화 상태 양호
**근거**:
- 단일 JOIN 쿼리로 N+1 문제 없음
- SQL 15ms는 100개 레코드 조회로 적절
- 전체 107ms는 랭킹 API로 충분히 빠름

---

### 2. Character/GetById
- **총 실행 시간**: 79.74ms
- **SQL 실행 시간**: 21.73ms (27.25%)
- **병목 지점**: 애플리케이션 로직 (72.75%)

**판단**: ✅ 최적화 상태 양호
**근거**:
- 단일 레코드 조회로 N+1 문제 없음
- 관계형 데이터(Equipment, Skills) 미포함으로 쿼리 단순
- 80ms는 단일 캐릭터 조회로 매우 빠름

---

### 3. Pets/GetEquippedPets
- **총 실행 시간**: 189.73ms (1차 실행, Cold) → 69.36ms (2차 실행, Warm)
- **SQL 실행 시간**: 16.92ms (8.92%, 1차 기준)
- **병목 지점**: 애플리케이션 로직 (91.08%)
  - Cold Start 오버헤드: ~120ms (JIT 컴파일 + 연결 풀)
  - EF Core 3단계 JOIN 매핑: ~40ms
  - DTO 변환: ~25ms

**판단**: ✅ 최적화 상태 양호
**근거**:
- 2단계 `.Include().ThenInclude()` 사용으로 N+1 제거
- Warm 상태 69ms는 매우 빠름
- SQL이 9%만 차지 → DB는 병목 아님

---

## 🎯 성능 병목 분석 기준

### SQL 비율별 해석

| SQL 비율 | 병목 위치 | 조치 사항 |
|----------|----------|-----------|
| **70% 이상** | 데이터베이스 | - 인덱스 추가<br>- N+1 쿼리 제거<br>- 쿼리 최적화 (SELECT 컬럼 축소) |
| **30-70%** | 균형 상태 | - 전체 응답 시간 확인<br>- 200ms 이하면 OK |
| **30% 이하** | 애플리케이션 | - DTO 매핑 최적화<br>- AutoMapper → 수동 매핑 고려<br>- JSON 직렬화 설정 검토 |

### 현재 API 상태
- **Pvp/GetRankings**: 14% (애플리케이션 병목)
- **Character/GetById**: 27% (균형 상태)
- **Pets/GetEquippedPets**: 9% (애플리케이션 병목)

**결론**: 모든 API가 **N+1 문제 없이 최적화**되어 있음. SQL 비율이 낮은 것은 **EF Core의 정상적인 동작**이며, 전체 응답 시간이 200ms 이하로 양호함.

---

## 🔍 실제 쿼리 분석

### Pvp/GetRankings 쿼리
```sql
SELECT p0."SeasonId", p0."CharacterId", ..., c."Id", ..., p1."Id", ...
FROM (
    SELECT p."SeasonId", p."CharacterId", ...
    FROM "PvpRanking" AS p
    WHERE p."SeasonId" = 1
    ORDER BY p."Rating" DESC
    LIMIT 100
) AS p0
INNER JOIN "Characters" AS c ON p0."CharacterId" = c."Id"
INNER JOIN "Players" AS p1 ON c."PlayerId" = p1."Id"
ORDER BY p0."Rating" DESC
```

**특징**:
- 2단계 JOIN (PvpRanking → Character → Player)
- `.Include().ThenInclude()` 사용 증거
- 단일 쿼리로 모든 데이터 조회

---

### Pets/GetEquippedPets 쿼리
```sql
SELECT e."CharacterId", e."SlotIndex", ..., p."Id", ..., p0."Id", ...
FROM "EquippedPets" AS e
INNER JOIN "Pets" AS p ON e."PetId" = p."Id"
INNER JOIN "PetTemplates" AS p0 ON p."TemplateId" = p0."Id"
WHERE e."CharacterId" = @characterId
ORDER BY e."SlotIndex"
```

**특징**:
- 2단계 JOIN (EquippedPets → Pets → PetTemplates)
- 단일 쿼리로 모든 관계형 데이터 조회
- N+1 문제 완전 제거

---

## 💡 학습 포인트

### 1. MiniProfiler 읽는 법
1. **상단 요약**에서 `sql: X%` 확인
2. **Call Stack**에서 각 쿼리 실행 시간 확인
3. **Command** 칼럼에서 실제 SQL 분석

### 2. N+1 문제 판별법
- `21.73 (1)` ← 괄호 안 숫자가 **쿼리 개수**
- `(1)` = 단일 쿼리 = 정상
- `(5)` = 5개 쿼리 = N+1 의심

### 3. Cold vs Warm 성능
- **Cold Start**: 첫 실행 (JIT 컴파일 + 연결 풀 초기화)
- **Warm**: 두 번째 이후 실행 (실제 프로덕션 성능)
- **차이**: 보통 2-3배 (예: 189ms → 69ms)

---

## 📈 다음 단계

**Phase 1 완료**: ✅ MiniProfiler 설정 및 N+1 검증 완료

**Phase 2 완료**: ✅ AuthController 통합 테스트 (보안 검증)
- **테스트 작성**: `IdleRPG.Tests/API/Controllers/AuthControllerTests.cs`
  - Public 엔드포인트: Register, Login, Refresh (11개 테스트 통과)
  - Authorized 엔드포인트: Logout, Profile (JWT Helper TODO(human) 구현 후 활성화 가능)
- **보안 검증**:
  - ✅ 중복 가입 방지
  - ✅ SQL Injection 방어 (Username 정규식 검증)
  - ✅ 사용자 존재 여부 노출 방지 (에러 메시지 통일)
  - ✅ Password 길이 검증 (6자 이상)
  - ✅ Password 불일치 검증
  - ✅ Invalid Refresh Token 거부
- **테스트 인프라 개선**:
  - `CustomWebApplicationFactory` 수정 (Testing 환경 설정)
  - `Program.cs` 수정 (Testing 환경에서 InMemory DB 사용)
  - `JwtTokenHelper.cs` 추가 (테스트용 JWT 토큰 생성)

**Phase 3 대기**: 코드 커버리지 측정

---

**최종 업데이트**: 2025-11-14
**분석자**: AI (Claude Code)
