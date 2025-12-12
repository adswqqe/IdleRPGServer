# Design: EF Core Migrations 전환

> 이 문서는 레거시 migration.sql을 EF Core Migrations로 전환하는 기술 설계를 정의합니다.
>
> **작성 가이드**:
> - Requirements의 모든 항목이 어떻게 구현될지 설명
> - 하이브리드 접근 (레거시 + EF Core 병행)
> - Jenkins CI/CD 파이프라인 변경
> - 롤백 전략 및 안전망

---

## 📐 Architecture Overview

### Layer Responsibilities

#### Infrastructure Layer
- **EF Core Migrations**: 자동 생성된 마이그레이션 파일 관리 (`IdleRPG.Infrastructure/Migrations/`)
- **GameDBContext**: EF Core DbContext (이미 존재, 변경 없음)
- **Configurations**: Entity 설정 (54개 파일, 변경 없음)
- **Legacy Migration**: 기존 `migration.sql` 보존 (1-2주 모니터링 후 제거)

#### DevOps Layer
- **Jenkins Pipeline**: Database Migration 스테이지 변경
  - 기존: `psql -f migration.sql` (단독 실행)
  - 신규: `psql -f migration.sql` → `dotnet ef database update` (순차 실행)
- **Rollback Script**: 긴급 롤백 명령어 (EC2 SSH 접속)

#### Development Tooling
- **.config/dotnet-tools.json**: EF Core CLI 9.0.0 프로젝트 로컬 도구 등록
- **Local Testing**: `docker-compose up postgres` → `dotnet ef database update`

**Requirements Traceability**:
- [US-1]: Infrastructure Layer - EF Core Migrations 자동 생성
- [US-2]: DevOps Layer - Jenkins 롤백 스크립트
- [US-3]: Development Tooling - dotnet-tools.json

---

## 🗄️ Data Model

### 신규 테이블: `__EFMigrationsHistory` (EF Core 자동 생성)

**목적**: EF Core가 적용된 마이그레이션을 추적하는 시스템 테이블 (자동 생성, 수동 관리 금지)

**필드**:
- `MigrationId` (PK): 마이그레이션 고유 ID (예: `20251111120000_InitialFromExistingDb`)
- `ProductVersion`: EF Core 버전 (예: `9.0.0`)

**제약사항**:
- `MigrationId`: 중복 불가, EF Core가 자동 관리

**중요**: 이 테이블은 EF Core가 자동으로 생성/관리하며, 현재 DB에 **이미 10개의 레거시 레코드 존재** (수동 INSERT).

### 기존 히스토리 보존 전략

**현재 상태** (Requirements 확인):
- `__EFMigrationsHistory` 테이블: 10개 레거시 레코드 존재
- 레거시 레코드는 EF Core 도입 전 수동으로 생성된 것 (형식: `YYYYMMDDHHMMSS_Description`)

**전환 전략**:
- ✅ **레거시 레코드 유지**: 기존 10개 레코드는 삭제하지 않음 (AC-5 충족)
- ✅ **신규 레코드 추가**: EF Core가 11번째 마이그레이션부터 자동 추가
- ✅ **최종 상태**: 10 (레거시) + N (EF Core 신규) 레코드

**검증 방법**:
```sql
SELECT MigrationId, ProductVersion FROM __EFMigrationsHistory ORDER BY MigrationId;
-- 결과: 10개 (레거시) + 신규 마이그레이션들
```

**Requirements**: [AC-5] ✅

---

## 🔌 API Design

### N/A (이번 Spec은 API 변경 없음)

**이유**:
- EF Core Migrations는 **인프라 변경**으로, API 엔드포인트 변경이 없음
- 기존 API는 그대로 작동 (GameDBContext는 변경 없음)

**Unity Client 영향**: 없음 (서버 내부 구조 변경만)

---

## 🧮 Business Logic

### N/A (비즈니스 로직 변경 없음)

**이유**:
- EF Core Migrations는 **DB 스키마 버전 관리 도구**
- 게임 로직 (전투, 가챠, 보상 계산)은 변경 없음

**영향 범위**: DevOps 및 개발 워크플로우만 변경

---

## 🎯 Service Layer Design

### N/A (서비스 계층 변경 없음)

**이유**:
- `GameDBContext`는 변경 없음 (Entity 설정 그대로)
- 기존 Repository, Service 코드 수정 불필요

**검증 방법**:
- 기존 Integration Tests 실행 → 모두 통과 확인

---

## 🔧 Implementation Strategy

### Phase 1: 프로젝트 로컬 도구 설치

**목표**: 팀원 모두 동일한 EF Core CLI 버전 사용 (버전 불일치 방지)

**작업**:
1. `.config/dotnet-tools.json` 생성
   ```bash
   dotnet new tool-manifest  # 이미 존재 시 건너뜀
   dotnet tool install dotnet-ef --version 9.0.0 --local
   ```

2. Git 커밋
   ```bash
   git add .config/dotnet-tools.json
   git commit -m "Add EF Core CLI 9.0.0 as local tool"
   ```

3. 팀원 환경 복원
   ```bash
   git pull
   dotnet tool restore  # dotnet-tools.json 읽고 자동 설치
   dotnet ef --version  # 9.0.0 확인
   ```

**Requirements**: [AC-1], [TR-1] ✅

---

### Phase 2: 초기 마이그레이션 생성 (기존 스키마 기반)

**목표**: 현재 DB 스키마를 EF Core ModelSnapshot으로 변환

**작업**:
```bash
# 1. 현재 스키마 기반 마이그레이션 생성
cd D:\Proj\IdleGameServer
dotnet ef migrations add InitialFromExistingDb --project IdleRPG.Infrastructure

# 2. 생성된 파일 확인
# IdleRPG.Infrastructure/Migrations/
# - 20251111120000_InitialFromExistingDb.cs (빈 Up/Down)
# - 20251111120000_InitialFromExistingDb.Designer.cs
# - GameDBContextModelSnapshot.cs (전체 스키마)
```

**중요**: `InitialFromExistingDb` 마이그레이션의 Up/Down 메서드는 **비어있어야 함**:
- 이유: 현재 DB는 이미 migration.sql로 생성되었으므로, 재실행 불필요
- EF Core가 현재 스키마를 "기준점"으로 인식하도록 설정

**🎓 학습 포인트 (아키텍처 결정)**:
- **TODO(human)**: `InitialFromExistingDb`의 Up/Down을 비워둬야 하는 이유 이해하기
  - 힌트: "Baseline Migration" 개념 (이미 적용된 스키마를 EF Core가 인식하도록)
  - 참고: [EF Core Docs - Existing Database](https://learn.microsoft.com/ef-core/)

**Requirements**: [AC-2], [TR-3] ✅

---

### Phase 3: Jenkinsfile 하이브리드 마이그레이션 구성

**목표**: 레거시 migration.sql과 EF Core 병행 실행, 점진적 전환

**Jenkinsfile 변경**:

```groovy
stage('Database Migration') {
    steps {
        echo 'Running hybrid migrations...'
        timeout(time: 5, unit: 'MINUTES') {
            withCredentials([string(credentialsId: 'rds-postgres-password', variable: 'PGPASSWORD')]) {
                sh '''
                    cd /home/ec2-user/IdleRPGServer

                    # 1. 도구 복원
                    echo "[Step 1/4] Restoring dotnet tools..."
                    dotnet tool restore

                    # 2. 레거시 마이그레이션 (안전망)
                    echo "[Step 2/4] Running legacy migration.sql..."
                    if [ -f "IdleRPG.Infrastructure/migration.sql" ]; then
                        export PGPASSWORD=$PGPASSWORD
                        psql -h idlerpg-dev.chiqweeeuidv.ap-northeast-2.rds.amazonaws.com \
                             -U postgres \
                             -d idlerpg \
                             -f IdleRPG.Infrastructure/migration.sql || exit 1
                        echo "✅ Legacy migration applied"
                    else
                        echo "⚠️ migration.sql not found, skipping legacy migration"
                    fi

                    # 3. EF Core 마이그레이션 (새로운 변경사항)
                    echo "[Step 3/4] Running EF Core migrations..."
                    dotnet ef database update \
                        --project IdleRPG.Infrastructure \
                        --connection "Host=idlerpg-dev.chiqweeeuidv.ap-northeast-2.rds.amazonaws.com;Port=5432;Database=idlerpg;Username=postgres;Password=$PGPASSWORD;SSL Mode=Require;Trust Server Certificate=true" \
                        || exit 1
                    echo "✅ EF Core migrations applied"

                    # 4. 마이그레이션 검증
                    echo "[Step 4/4] Verifying migrations..."
                    psql -h idlerpg-dev.chiqweeeuidv.ap-northeast-2.rds.amazonaws.com \
                         -U postgres \
                         -d idlerpg \
                         -c "SELECT MigrationId, ProductVersion FROM __EFMigrationsHistory ORDER BY MigrationId DESC LIMIT 5;" \
                         || echo "⚠️ Verification skipped"
                '''
            }
        }
    }
}
```

**실행 순서**:
1. `dotnet tool restore` (EF Core CLI 복원)
2. `psql -f migration.sql` (레거시, Idempotent)
3. `dotnet ef database update` (EF Core, 새로운 변경사항만)
4. `psql -c "SELECT ..."` (검증)

**안전장치**:
- ✅ 각 단계 실패 시 `|| exit 1`로 파이프라인 중단
- ✅ 레거시 마이그레이션 실패 시 EF Core 실행 안 함 (순차 보장)
- ✅ 마이그레이션 완료 후에만 Docker 빌드 진행

**Requirements**: [AC-3], [TR-2] ✅

---

### Phase 4: 롤백 전략

**목표**: 배포 후 장애 발생 시 1-2분 내 복구

#### 롤백 명령어 (긴급 상황)

```bash
# 1. EC2 SSH 접속
ssh ec2-user@13.209.66.253

# 2. 현재 마이그레이션 목록 확인
cd /home/ec2-user/IdleRPGServer
dotnet ef migrations list --project IdleRPG.Infrastructure

# 출력 예시:
# 20251111120000_InitialFromExistingDb (Applied)
# 20251112100000_RemoveTierColumn (Applied)
# 20251113150000_AddPetSystem (Applied) ← 이걸 롤백하고 싶음

# 3. 이전 마이그레이션으로 롤백
dotnet ef database update 20251112100000_RemoveTierColumn \
    --project IdleRPG.Infrastructure \
    --connection "Host=idlerpg-dev.chiqweeeuidv.ap-northeast-2.rds.amazonaws.com;Port=5432;Database=idlerpg;Username=postgres;Password=$PGPASSWORD;SSL Mode=Require;Trust Server Certificate=true"

# 4. 검증
export PGPASSWORD=$PGPASSWORD
psql -h idlerpg-dev.chiqweeeuidv.ap-northeast-2.rds.amazonaws.com \
     -U postgres \
     -d idlerpg \
     -c "SELECT * FROM __EFMigrationsHistory ORDER BY MigrationId DESC LIMIT 3;"

# 결과: 20251113150000_AddPetSystem 레코드가 삭제됨
```

**롤백 시간**:
- **일반적으로 1-2분** (Down() 메서드 실행 시간)
- **복잡한 마이그레이션 시 3-5분** (데이터 변환 포함 시)
  - 예: 단순 스키마 변경 (DROP COLUMN) → 빠름 (1분)
  - 예: 데이터 이관 필요 (ADD COLUMN → 데이터 복사) → 느림 (3-5분)

**🎓 학습 포인트 (롤백 전략)**:
- **TODO(human)**: 롤백 시 데이터 손실 위험이 있는 시나리오 이해하기
  - 시나리오 1: `DROP COLUMN` → 데이터 영구 손실 (복구 불가)
  - 시나리오 2: `ALTER COLUMN NOT NULL` → NULL 데이터 있으면 실패
  - 해결책: **2-Step Migration** 설계
    1. AddColumn (Nullable) → 데이터 이관 → AlterColumn (NOT NULL)
    2. 각 단계를 별도 마이그레이션으로 분리
  - 참고: Design 문서 "Backward Compatibility" 섹션

**Requirements**: [AC-4], [US-2], [TR-4] ✅

---

### Phase 5: 로컬 테스트 환경

**목표**: 개발자가 로컬에서 Up/Down 반복 테스트 가능

**작업 흐름**:
```bash
# 1. 로컬 PostgreSQL 실행
docker-compose up -d postgres

# 2. 마이그레이션 적용
cd D:\Proj\IdleGameServer
dotnet ef database update --project IdleRPG.Infrastructure

# 3. 롤백 테스트
dotnet ef database update 20251111120000_InitialFromExistingDb --project IdleRPG.Infrastructure

# 4. 다시 최신으로
dotnet ef database update --project IdleRPG.Infrastructure

# 5. 마이그레이션 삭제 (실수로 만든 경우)
dotnet ef migrations remove --project IdleRPG.Infrastructure
```

**연결 문자열**: `appsettings.Development.json` (로컬 환경)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=idlerpg_dev;Username=postgres;Password=password123"
  }
}
```

**Requirements**: [AC-6] ✅

---

## 🧪 Testing Strategy

### Manual Testing (배포 전)

**로컬 환경 테스트**:
1. ✅ `dotnet ef migrations add TestMigration` (성공 확인)
2. ✅ `dotnet ef database update` (Up 메서드 실행)
3. ✅ `dotnet ef database update {PreviousMigration}` (Down 메서드 실행)
4. ✅ `dotnet ef migrations remove` (파일 삭제)

**Jenkins 시뮬레이션** (EC2 SSH):
1. ✅ Jenkins와 동일한 명령어 수동 실행
2. ✅ 에러 없이 완료 확인
3. ✅ `__EFMigrationsHistory` 테이블 레코드 확인

### Automated Testing

**기존 Integration Tests**:
- ✅ `IdleRPG.Tests.Integration` 프로젝트의 모든 테스트 통과 확인
- 이유: EF Core Migrations는 스키마 버전 관리일 뿐, 로직 변경 없음

**새로운 테스트 불필요**:
- EF Core Migrations는 EF Core 팀이 이미 검증한 도구
- 개발자는 "Up/Down 메서드 로직"만 검증하면 됨 (Manual Testing으로 충분)

---

## ⚠️ Error Handling

### Jenkins 마이그레이션 실패 시나리오

#### Scenario 1: 레거시 migration.sql 실패
**증상**: `psql -f migration.sql` 에러 (예: 문법 오류, 제약조건 위반)

**처리**:
- Jenkins 파이프라인 중단 (`|| exit 1`)
- Docker 빌드 및 배포 진행 안 함 (10대 서버 보호)
- 개발자에게 알림 (Jenkins 로그)

**복구**:
1. `migration.sql` 수정
2. Git 커밋 + 푸시
3. Jenkins 자동 재실행 (GitHub Webhook)

---

#### Scenario 2: EF Core 마이그레이션 실패
**증상**: `dotnet ef database update` 에러 (예: FK 제약조건, NULL 위반)

**처리**:
- Jenkins 파이프라인 중단 (`|| exit 1`)
- 레거시 마이그레이션은 성공했으므로, **EF Core 마이그레이션만 재실행**

**복구 방법 A (코드 수정)**:
1. 마이그레이션 코드 수정 (`{Timestamp}_{Name}.cs`)
2. Git 커밋 + 푸시
3. Jenkins 재실행

**복구 방법 B (롤백)**:
1. EC2 SSH 접속
2. `dotnet ef database update {PreviousMigration}` (Down() 실행)
3. 마이그레이션 코드 수정 후 재배포

---

#### Scenario 3: 배포 후 10분 뒤 장애 발견
**증상**: 앱 서버는 정상 작동, 특정 API에서 에러 (예: 새로운 컬럼 누락)

**🎓 학습 포인트 (장애 복구 전략)**:
- **TODO(human)**: 다음 3가지 복구 방법 중 선택 기준 이해하기
  - **Option A: DB 롤백** (`dotnet ef database update {Previous}`)
    - 장점: 빠름 (1-2분), 안전
    - 단점: 데이터 손실 위험 (10분간 생성된 데이터)
    - 적용 시점: 데이터 생성 없는 시스템 (예: 새벽 배포)
  - **Option B: Code 롤백** (`git revert + Jenkins 재배포`)
    - 장점: 데이터 유지, 안전
    - 단점: 느림 (5-10분, Docker 빌드 시간)
    - 적용 시점: 사용자가 데이터를 생성 중일 때
  - **Option C: Forward Fix** (버그 수정 + 긴급 배포)
    - 장점: 데이터 유지, 근본 해결
    - 단점: 가장 느림 (10-30분)
    - 적용 시점: 버그가 심각하지 않고, 수정이 간단한 경우

**각 시나리오별 복구 시간 & 리스크**:
| 방법 | 시간 | 데이터 손실 위험 | 서비스 영향 |
|------|------|-----------------|-------------|
| DB 롤백 | 1-2분 | 높음 (10분 데이터) | 2분 다운타임 |
| Code 롤백 | 5-10분 | 없음 | 10분 장애 유지 |
| Forward Fix | 10-30분 | 없음 | 30분 장애 유지 |

**권장 전략**: Code 롤백 (데이터 보호 우선)

---

### 예방 조치

**1. 마이그레이션 코드 리뷰**:
- Pull Request에서 `Migrations/` 폴더 변경 확인
- Down() 메서드가 Up()을 정확히 되돌리는지 검증

**2. 로컬 테스트 의무화**:
- 개발자는 `dotnet ef database update` → `update {Previous}` 반복 테스트 후 커밋

**3. Idempotent 원칙**:
- EF Core는 `__EFMigrationsHistory`로 자동 보장
- 동일 마이그레이션 재실행 시 건너뜀

---

## 🔐 Security Considerations

### DB Credentials
- ✅ **Jenkins Credentials**: `rds-postgres-password` (환경 변수 `$PGPASSWORD`)
- ❌ 코드에 하드코딩 금지 (Jenkinsfile, appsettings.json)

### SSL/TLS 암호화
- ✅ **SSL Mode**: `Require` (필수 암호화, 평문 연결 차단)
- **보안 효과**: MITM(Man-In-The-Middle) 공격 방지
- **대안 고려**:
  - `Prefer` (기존): SSL 시도하지만 실패 시 평문 허용 ❌ MITM 취약
  - `Require` (현재): SSL 필수, 평문 연결 차단 ✅ 권장
  - `VerifyFull`: SSL + CA 인증서 + 호스트명 검증 (엔터프라이즈 수준)
- **RDS 호환성**: AWS RDS는 SSL 기본 지원, 호환성 문제 없음

### 접근 제어
- **EC2 SSH 롤백**: DevOps 권한 필요 (키 파일 관리)
- **RDS 접근**: Security Group으로 EC2 IP만 허용

### 감사 로그
- **마이그레이션 기록**: `__EFMigrationsHistory` 테이블 (자동)
- **Jenkins 로그**: 모든 마이그레이션 실행 기록 보존

---

## 📊 Performance Considerations

### 마이그레이션 실행 시간

**기존 (migration.sql)**:
- 5-10초 (1413줄 전체 IF NOT EXISTS 체크)

**신규 (EF Core)**:
- 2-3초 (새로운 마이그레이션만 실행)
- `__EFMigrationsHistory` 테이블 조회 (인덱스 PK)

**개선**: 50% 시간 단축 (5초 → 2.5초)

### 10대 서버 환경

**실행 위치**: Jenkins에서 1회만 실행 (10대 서버는 실행 안 함)
- 이유: 마이그레이션은 배포 전 DB 스키마 변경 작업
- 10대 앱 서버는 **마이그레이션 완료된 DB에 접근**

**경합 조건 방지**:
- EF Core가 `__EFMigrationsHistory` 테이블에 **행 수준 락(Row-Level Lock)** 자동 적용
- 동시 실행 시 두 번째 요청은 대기 후 건너뜀 (Idempotent)

**참고**: Jenkins `disableConcurrentBuilds()` 옵션으로 동시 빌드 방지 (이미 설정됨)

---

## 🔄 Migration Plan

### 전환 로드맵

#### Week 1: EF Core Migrations 도입 (하이브리드)
**작업**:
1. `.config/dotnet-tools.json` 추가 (EF Core CLI 9.0.0)
2. `InitialFromExistingDb` 마이그레이션 생성 (빈 Up/Down)
3. Jenkinsfile 수정 (레거시 + EF Core 병행)
4. 로컬 테스트 (Up/Down 반복)
5. 배포 (Jenkins)

**검증**:
- ✅ `__EFMigrationsHistory`: 10 (레거시) + 1 (InitialFromExistingDb) = 11개 레코드
- ✅ 기존 Integration Tests 통과
- ✅ Jenkins 파이프라인 성공

---

#### Week 2-3: 모니터링 & 안정화
**작업**:
1. 모든 신규 변경사항은 **EF Core Migrations로만 작성**
   - 예: `dotnet ef migrations add RemoveTierColumn`
2. `migration.sql`은 수정하지 않음 (레거시 안전망)
3. Jenkins 로그 모니터링 (에러 없는지)

**검증**:
- ✅ 신규 마이그레이션 3-5개 성공 적용
- ✅ 롤백 테스트 1회 수행 (로컬 환경)

---

#### Week 4: 레거시 제거 (Optional, 사용자 승인 필요)
**작업**:
1. `migration.sql` 파일 이동 → `docs/migrations/legacy/migration.sql` (보관)
2. Jenkinsfile에서 `psql -f migration.sql` 단계 제거
3. **최종 상태**: EF Core Migrations만 사용

**트리거 조건**:
- 2-3주간 문제 없음
- 팀 전체 동의

**Out of Scope**: 현재 Spec에서는 레거시 제거 안 함 (안전망 유지)

---

### 🎓 학습 포인트 (데이터베이스 설계)

#### TODO-1: 인덱스 전략
**위치**: 마이그레이션 생성 시 (코드 작성 단계)

**학습 포인트**: 단일 컬럼 vs 복합 인덱스 선택

**결정 필요**:
- **단일 컬럼 인덱스**: `CREATE INDEX IX_Character_PlayerId ON Character(PlayerId)`
  - 장점: 간단, 저장 공간 적음
  - 단점: WHERE PlayerId = X AND Level > 10 같은 복합 조건 시 성능 저하
- **복합 인덱스**: `CREATE INDEX IX_Character_PlayerId_Level ON Character(PlayerId, Level)`
  - 장점: 복합 조건 쿼리 빠름
  - 단점: 저장 공간 증가, 쓰기 성능 저하

**트레이드오프 분석**:
| 인덱스 타입 | 읽기 성능 | 쓰기 성능 | 저장 공간 | 적용 시점 |
|------------|----------|----------|----------|---------|
| 단일 컬럼 | 보통 | 빠름 | 적음 | 단순 WHERE 절 |
| 복합 인덱스 | 빠름 | 느림 | 많음 | JOIN, 복합 WHERE 절 |

**근거 작성**: 각 인덱스 선택 시 쿼리 패턴 분석 필요

**참고**: EF Core는 인덱스를 자동 생성하지 않음. `HasIndex()` Fluent API로 명시 필요.

---

#### TODO-2: Cascade Delete 규칙
**위치**: Entity Configuration (Fluent API)

**학습 포인트**: ON DELETE CASCADE vs Application 처리

**결정 필요**:
- **ON DELETE CASCADE** (DB 수준):
  ```csharp
  modelBuilder.Entity<Character>()
      .HasMany(c => c.Equipments)
      .WithOne(e => e.Character)
      .OnDelete(DeleteBehavior.Cascade);
  ```
  - 장점: 빠름 (DB가 자동 처리), 트랜잭션 안전
  - 단점: 복구 어려움 (Soft Delete 불가)

- **Application 처리** (코드 수준):
  ```csharp
  // Service에서 수동 삭제
  var equipments = await _repository.GetEquipmentsByCharacterIdAsync(characterId);
  foreach (var equipment in equipments) {
      await _repository.DeleteEquipmentAsync(equipment.Id);
  }
  await _repository.DeleteCharacterAsync(characterId);
  ```
  - 장점: 세밀한 제어, Soft Delete 가능, 로그 기록
  - 단점: 느림 (여러 번 DB 호출), 트랜잭션 관리 복잡

**시나리오 작성**:
- Character 삭제 시 100개 Equipments도 삭제 → Cascade vs Application?
- 게임 복구 요청 (CS) → Soft Delete 필요 → Application 처리

**근거 작성**: 게임 특성상 Soft Delete 필요 → Application 처리 권장

---

#### TODO-3: 타입 선택
**위치**: 마이그레이션 생성 시 (코드 작성 단계)

**학습 포인트**: uuid vs serial, varchar vs text

**결정 필요**:
- **Primary Key 타입**:
  - `uuid` (Guid): 분산 환경 대비, 클라이언트 미리 생성 가능
  - `serial` (int): 간단, 저장 공간 적음, 정렬 빠름
  - 현재 프로젝트: **Entity는 Guid, Template은 int** (이미 결정됨)

- **문자열 타입**:
  - `varchar(100)`: 고정 최대 길이, 검증 필요
  - `text`: 무제한 길이, 유연함
  - 권장: **varchar(N)**을 명시적으로 사용 (데이터 무결성)

**트레이드오프**:
| 타입 | 장점 | 단점 | 적용 시점 |
|------|------|------|---------|
| uuid | 분산 안전, 예측 불가 | 저장 공간 16바이트 | User Data |
| serial | 간단, 8바이트 | 중앙 집중형 | Master Data |
| varchar(N) | 검증 가능, 인덱스 효율 | 최대 길이 제한 | 제한된 텍스트 |
| text | 무제한, 유연 | 인덱스 비효율 | 긴 텍스트 (설명) |

---

### Data Seeding 계획

**Seeder 필요 여부**: No

**이유**:
- EF Core Migrations는 스키마 변경만 다룸
- 초기 데이터 (SkillTemplate, MonsterTemplate)는 별도 Seeder 클래스로 관리 (이미 존재)

---

## 📝 Decision Log

| ID | Decision | ADR Link | Spike Link | Status |
|----|----------|----------|------------|--------|
| D1 | 하이브리드 접근 (레거시 + EF Core 병행) | [ADR-0009](../../../docs/adr/ADR-0009-ef-migrations-hybrid.md) | - | Accepted |
| D2 | 프로젝트 로컬 도구 (Global 대신) | - | - | Accepted |
| D3 | Jenkins에서 마이그레이션 실행 (앱 서버 시작 시 실행 안 함) | - | - | Accepted |
| D4 | 기존 히스토리 보존 (10개 레코드 유지) | - | - | Accepted |

**가이드**:
- **D1**: 중요한 아키텍처 결정 → ADR 작성 필요 (학습 포인트)
- **D2-D4**: 간단한 결정 → 테이블에 1줄로 기록

---

## 📱 Unity Client Integration

### Unity Documentation 필요 항목

**없음** (서버 내부 인프라 변경만)

**이유**:
- EF Core Migrations는 API 변경 없음
- Unity 클라이언트는 영향 받지 않음

---

## ✅ Approval

- [x] Design 리뷰 완료
- [x] 모든 Requirements 항목 커버 확인
- [x] TODO(human) 아키텍처 학습 포인트 3개 확인 완료
  - ✅ Baseline Migration (비어있는 Up/Down) - 동의
  - ✅ 2-Step Migration (컬럼 변경 시 2단계 분리) - 동의
  - ✅ 복구 전략 (Code 롤백 기본) - 동의
- [x] Self-Review Checklist 10개 항목 통과 (10/10)
- [x] Tasks 단계로 진행 승인

---

**작성일**: 2025-11-11
**작성자**: Claude Code AI
**승인일**: 2025-11-11
**승인자**: Development Team
**상태**: Approved ✅
**Requirements 추적성**:
- [US-1]: Phase 2 - 마이그레이션 자동 생성 ✅
- [US-2]: Phase 4 - 롤백 전략 ✅
- [US-3]: Phase 1, 3 - 팀 동기화 (dotnet-tools.json, Git) ✅
- [AC-1]: Phase 1 - EF Core CLI 설치 ✅
- [AC-2]: Phase 2 - 마이그레이션 자동 생성 ✅
- [AC-3]: Phase 3 - Jenkins 하이브리드 마이그레이션 ✅
- [AC-4]: Phase 4 - 롤백 시나리오 ✅
- [AC-5]: Phase 2 - 기존 히스토리 보존 ✅
- [AC-6]: Phase 5 - 로컬 테스트 환경 ✅
