# EF Core Migrations Rollback Guide

> 배포 후 장애 발생 시 1-2분 내 복구 가능하도록 하는 롤백 가이드

**작성일**: 2025-11-11
**대상**: DevOps 엔지니어, 백엔드 개발자
**적용 환경**: IdleRPG Server (EF Core 9.0 + PostgreSQL)

---

## 📋 목차

1. [개요](#개요)
2. [시나리오별 복구 방법](#시나리오별-복구-방법)
   - [Scenario 1: 레거시 migration.sql 실패](#scenario-1-레거시-migrationsql-실패)
   - [Scenario 2: EF Core 마이그레이션 실패](#scenario-2-ef-core-마이그레이션-실패)
   - [Scenario 3: 배포 후 장애 발견 (10분 뒤)](#scenario-3-배포-후-장애-발견-10분-뒤)
3. [롤백 전략 비교](#롤백-전략-비교)
4. [긴급 복구 명령어](#긴급-복구-명령어)
5. [예방 조치](#예방-조치)
6. [장애 대응 체크리스트](#장애-대응-체크리스트)

---

## 개요

### 롤백이 필요한 상황

- ❌ Jenkins 파이프라인 Database Migration 스테이지 실패
- ❌ 배포 후 API 에러 (예: 새로운 컬럼 누락, FK 제약조건 위반)
- ❌ 데이터 정합성 문제 (예: NULL 값 허용 안 함)

### 하이브리드 마이그레이션 구조

Jenkins는 다음 순서로 마이그레이션을 실행합니다:

1. **Step 1**: `dotnet tool restore` (EF Core CLI 복원)
2. **Step 2**: `psql -f migration.sql` (레거시 안전망, Idempotent)
3. **Step 3**: `dotnet ef database update` (EF Core, 새로운 변경사항만)
4. **Step 4**: `psql -c "SELECT ..."` (검증, `__EFMigrationsHistory` 조회)

**롤백 시점**:
- Step 2 실패 → 레거시 migration.sql 문제
- Step 3 실패 → EF Core 마이그레이션 문제
- Step 4 이후 → 배포 완료 후 장애 발견

---

## 시나리오별 복구 방법

### Scenario 1: 레거시 migration.sql 실패

#### 증상
- Jenkins 로그: `psql -f migration.sql` 에러 (예: 문법 오류, 제약조건 위반)
- 파이프라인 상태: `FAILED` (Step 2에서 중단)
- 영향 범위: 10대 서버 배포 안 됨 (안전)

#### 처리 방법
Jenkins 파이프라인이 `|| exit 1`로 자동 중단되므로, **배포가 진행되지 않습니다**.

**복구 절차**:
```bash
# 1. migration.sql 파일 수정 (로컬)
cd D:\Proj\IdleGameServer
# 문제가 되는 SQL 구문 수정

# 2. Git 커밋 + 푸시
git add IdleRPG.Infrastructure/migration.sql
git commit -m "Fix migration.sql syntax error"
git push origin master

# 3. Jenkins 자동 재실행 (GitHub Webhook)
# 또는 Jenkins 콘솔에서 "Build Now" 클릭
```

**복구 시간**: 2-5분 (수정 + Git 푸시 + Jenkins 재실행)

**데이터 손실 위험**: 없음 (배포가 진행되지 않음)

---

### Scenario 2: EF Core 마이그레이션 실패

#### 증상
- Jenkins 로그: `dotnet ef database update` 에러 (예: FK 제약조건, NULL 위반)
- 파이프라인 상태: `FAILED` (Step 3에서 중단)
- 영향 범위: 10대 서버 배포 안 됨 (레거시 마이그레이션은 성공)

#### 처리 방법
레거시 마이그레이션(`migration.sql`)은 성공했으므로, **EF Core 마이그레이션만 수정하면 됩니다**.

**복구 절차 A (코드 수정)**:
```bash
# 1. 마이그레이션 코드 수정 (로컬)
cd D:\Proj\IdleGameServer
# 문제가 되는 마이그레이션 파일 열기 (예: IdleRPG.Infrastructure/Migrations/{Timestamp}_AddPetLevel.cs)

# 2. Up() 또는 Down() 메서드 수정
# 예: AddColumn()에 .IsRequired(false) 추가 (Nullable로 변경)

# 3. Git 커밋 + 푸시
git add IdleRPG.Infrastructure/Migrations/
git commit -m "Fix EF Core migration: Make PetLevel nullable"
git push origin master

# 4. Jenkins 재실행
```

**복구 절차 B (DB 롤백)**:
```bash
# 1. EC2 SSH 접속
ssh ec2-user@13.209.66.253

# 2. 현재 적용된 마이그레이션 확인
cd /home/ec2-user/IdleRPGServer
dotnet ef migrations list --project IdleRPG.Infrastructure --no-build

# 3. 이전 마이그레이션으로 롤백 (Down() 실행)
dotnet ef database update {PreviousMigrationName} --project IdleRPG.Infrastructure --no-build

# 예시:
# dotnet ef database update 20251110120000_AddPvpArena --project IdleRPG.Infrastructure --no-build

# 4. 마이그레이션 히스토리 확인
psql -h idlerpg-dev.chiqweeeuidv.ap-northeast-2.rds.amazonaws.com \
     -U postgres \
     -d idlerpg \
     -c "SELECT MigrationId, ProductVersion FROM __EFMigrationsHistory ORDER BY MigrationId DESC LIMIT 5;"

# 5. 로컬에서 마이그레이션 코드 수정 후 재배포
```

**복구 시간**:
- 코드 수정: 2-5분
- DB 롤백: 1-2분 (+ 코드 수정 + 재배포)

**데이터 손실 위험**: 없음 (배포가 진행되지 않음)

---

### Scenario 3: 배포 후 장애 발견 (10분 뒤)

#### 증상
- Jenkins 파이프라인: `SUCCESS` (모든 스테이지 통과)
- 앱 서버: 정상 작동 (`/health` 엔드포인트 200 OK)
- 특정 API: 에러 발생 (예: 500 Internal Server Error)
  - 로그: `Column 'PetLevel' does not exist` (새로운 컬럼 누락)
  - 로그: `FK constraint violation` (외래 키 제약조건 위반)

#### 처리 방법
배포가 완료되고 **사용자가 데이터를 생성 중**입니다. 다음 3가지 복구 방법 중 선택해야 합니다.

---

#### 📊 롤백 전략 비교

| 방법 | 시간 | 데이터 손실 위험 | 서비스 영향 | 적용 시점 |
|------|------|-----------------|-------------|-----------|
| **A. DB 롤백** | 1-2분 | ⚠️ 높음 (10분 데이터) | 🔴 2분 다운타임 | 데이터 생성 없는 시간대 (새벽 배포) |
| **B. Code 롤백** | 5-10분 | ✅ 없음 | 🟡 10분 장애 유지 | **사용자 활동 중 (권장)** |
| **C. Forward Fix** | 10-30분 | ✅ 없음 | 🔴 30분 장애 유지 | 버그가 심각하지 않고, 수정이 간단한 경우 |

**게임 서비스 특성상 권장 전략**: **B. Code 롤백** (데이터 보호 우선)

---

#### 복구 방법 A: DB 롤백 (1-2분)

⚠️ **주의**: 10분간 생성된 데이터(캐릭터, 아이템, 전투 기록 등)가 손실됩니다!

```bash
# 1. EC2 SSH 접속
ssh ec2-user@13.209.66.253

# 2. 현재 적용된 마이그레이션 확인
cd /home/ec2-user/IdleRPGServer
dotnet ef migrations list --project IdleRPG.Infrastructure --no-build
# 출력 예시:
# 20251110120000_AddPvpArena (Applied)
# 20251111130000_AddPetLevel (Applied) ← 이 마이그레이션이 문제

# 3. 이전 마이그레이션으로 롤백 (Down() 실행)
dotnet ef database update 20251110120000_AddPvpArena --project IdleRPG.Infrastructure --no-build

# 4. 앱 서버 재시작 (새로운 코드가 이전 DB 스키마를 사용하도록)
sudo systemctl restart idlerpg-api

# 5. Health Check (정상 확인)
curl http://localhost:5172/health
# 예상: 200 OK

# 6. 문제 API 테스트
curl -X GET http://localhost:5172/api/pets \
     -H "Authorization: Bearer {TOKEN}"
# 예상: 200 OK (PetLevel 컬럼 없는 상태)
```

**복구 시간**: 1-2분

**데이터 손실**: ⚠️ 10분간 생성된 데이터 손실 (게임 서비스에서는 치명적)

**적용 시점**: 새벽 배포 (사용자 활동 없음)

---

#### 복구 방법 B: Code 롤백 (5-10분) ⭐ 권장

✅ **장점**: 데이터 손실 없음, 안전한 복구

```bash
# 1. 로컬에서 이전 커밋 확인
cd D:\Proj\IdleGameServer
git log --oneline -n 5
# 출력 예시:
# abc1234 Add Pet Level feature (문제가 되는 커밋)
# def5678 Configure hybrid migrations (이전 커밋)

# 2. Git Revert (문제 커밋 되돌리기)
git revert abc1234 --no-edit
# 또는 여러 커밋 되돌리기:
# git revert abc1234^..abc1234

# 3. Git 푸시
git push origin master

# 4. Jenkins 자동 재빌드 (GitHub Webhook)
# 또는 Jenkins 콘솔에서 "Build Now" 클릭

# 5. Jenkins 파이프라인 모니터링 (5-10분 소요)
# - Build → Test → Database Migration → Docker Build → Deploy

# 6. 배포 완료 후 Health Check
curl http://13.209.66.253:5172/health
# 예상: 200 OK

# 7. 문제 API 테스트
curl -X GET http://13.209.66.253:5172/api/pets \
     -H "Authorization: Bearer {TOKEN}"
# 예상: 200 OK (이전 버전으로 복구)
```

**복구 시간**: 5-10분 (Git Revert + Jenkins 재빌드 + 배포)

**데이터 손실**: ✅ 없음 (10분간 생성된 데이터 모두 유지)

**서비스 영향**: 10분간 장애 유지 (사용자는 에러 경험)

**적용 시점**: **사용자 활동 중 (권장)**

---

#### 복구 방법 C: Forward Fix (10-30분)

✅ **장점**: 데이터 유지, 근본 해결 (롤백 후 재배포 불필요)

❌ **단점**: 가장 느림 (버그 수정 + 테스트 + 배포)

```bash
# 1. 버그 수정 (로컬)
cd D:\Proj\IdleGameServer
# 예: PetLevel 컬럼 누락 문제 수정 (마이그레이션 코드 수정)

# 2. 로컬 테스트
dotnet ef database update --project IdleRPG.Infrastructure
# Down/Up 반복 테스트로 검증

# 3. Git 커밋 + 푸시
git add .
git commit -m "Fix: Add missing PetLevel column in migration"
git push origin master

# 4. Jenkins 재배포 (5-10분)

# 5. 배포 완료 후 Health Check
curl http://13.209.66.253:5172/health
# 예상: 200 OK
```

**복구 시간**: 10-30분 (버그 수정 + 테스트 + Jenkins 재배포)

**데이터 손실**: ✅ 없음

**서비스 영향**: 30분간 장애 유지

**적용 시점**: 버그가 심각하지 않고, 수정이 간단한 경우

---

## 긴급 복구 명령어

### RDS 마이그레이션 히스토리 조회

```bash
# EC2 SSH 접속
ssh ec2-user@13.209.66.253

# RDS 쿼리 (마이그레이션 히스토리 확인)
psql -h idlerpg-dev.chiqweeeuidv.ap-northeast-2.rds.amazonaws.com \
     -U postgres \
     -d idlerpg \
     -c "SELECT MigrationId, ProductVersion FROM __EFMigrationsHistory ORDER BY MigrationId DESC LIMIT 10;"

# 출력 예시:
# MigrationId                    | ProductVersion
# -------------------------------|---------------
# 20251111130000_AddPetLevel     | 9.0.0
# 20251110120000_AddPvpArena     | 9.0.0
# 20251109110000_AddGuildSystem  | 9.0.0
```

---

### EF Core 마이그레이션 목록 조회

```bash
# EC2 SSH 접속
ssh ec2-user@13.209.66.253
cd /home/ec2-user/IdleRPGServer

# 마이그레이션 목록 (Applied / Pending 구분)
dotnet ef migrations list --project IdleRPG.Infrastructure --no-build

# 출력 예시:
# 20251109110000_AddGuildSystem (Applied)
# 20251110120000_AddPvpArena (Applied)
# 20251111130000_AddPetLevel (Applied) ← 현재 버전
```

---

### DB 롤백 명령어 (특정 마이그레이션으로)

```bash
# EC2 SSH 접속
ssh ec2-user@13.209.66.253
cd /home/ec2-user/IdleRPGServer

# 이전 마이그레이션으로 롤백 (Down() 실행)
dotnet ef database update {PreviousMigrationName} \
    --project IdleRPG.Infrastructure \
    --no-build

# 예시: AddPvpArena로 롤백 (AddPetLevel 마이그레이션 제거)
dotnet ef database update 20251110120000_AddPvpArena \
    --project IdleRPG.Infrastructure \
    --no-build

# 앱 서버 재시작
sudo systemctl restart idlerpg-api

# Health Check
curl http://localhost:5172/health
```

---

### Git Revert 명령어 (코드 롤백)

```bash
# 로컬에서 이전 커밋 확인
cd D:\Proj\IdleGameServer
git log --oneline -n 10

# 특정 커밋 되돌리기 (Revert)
git revert {CommitHash} --no-edit

# 예시: abc1234 커밋 되돌리기
git revert abc1234 --no-edit

# Git 푸시 (Jenkins 자동 재빌드)
git push origin master

# Jenkins 콘솔에서 파이프라인 모니터링
# http://your-jenkins-url/job/IdleRPGServer/
```

---

## 예방 조치

### 1. 마이그레이션 코드 리뷰 (Pull Request)

**체크리스트**:
- ✅ `Migrations/` 폴더 변경 확인
- ✅ Up() 메서드가 Down()과 정확히 반대 작업 수행하는지 검증
- ✅ 컬럼 변경 시 **2-Step Migration** 적용 확인
  - Step 1: AddColumn (Nullable)
  - Step 2: 데이터 이관 → AlterColumn (NOT NULL)
- ✅ FK 제약조건 추가 시 기존 데이터 정합성 확인

---

### 2. 로컬 테스트 의무화

**개발자는 다음 테스트를 완료한 후에만 커밋**:
```bash
# 1. 마이그레이션 적용 (Up)
dotnet ef database update --project IdleRPG.Infrastructure

# 2. 롤백 테스트 (Down)
dotnet ef database update {PreviousMigration} --project IdleRPG.Infrastructure

# 3. 다시 최신으로 (Up)
dotnet ef database update --project IdleRPG.Infrastructure

# 4. 모든 단계에서 에러 없음 확인
```

---

### 3. Idempotent 원칙 (멱등성)

EF Core는 `__EFMigrationsHistory` 테이블로 자동 보장:
- ✅ 동일 마이그레이션 재실행 시 자동 건너뜀
- ✅ 동시 실행 시 행 수준 락(Row-level lock)으로 경합 조건 방지

**개발자가 해야 할 일**: 없음 (EF Core가 자동 처리)

---

### 4. 배포 시점 선택

**권장 배포 시간대**:
- 🌙 **새벽 배포 (03:00~05:00)**: 사용자 활동 최소화 (데이터 생성 위험 낮음)
- ⚠️ **점심/저녁 시간대 배포 금지**: 사용자 활동 최대 (데이터 손실 위험 높음)

**배포 전 확인 사항**:
- ✅ Jenkins 로그 모니터링 준비 (실시간)
- ✅ EC2 SSH 접속 가능 확인 (롤백 대비)
- ✅ Slack/Discord 알림 채널 준비 (팀원 공유)

---

## 장애 대응 체크리스트

### Phase 1: 장애 감지 (1분)

- [ ] Jenkins 파이프라인 상태 확인 (`SUCCESS` / `FAILED`)
- [ ] 어느 스테이지에서 실패했는지 확인 (Step 2 / Step 3 / Step 4)
- [ ] Jenkins 로그 확인 (에러 메시지 복사)

---

### Phase 2: 영향 범위 파악 (2분)

- [ ] 배포가 진행되었는지 확인 (`Docker Build` 스테이지 통과 여부)
- [ ] 10대 서버에 새로운 코드가 배포되었는지 확인
- [ ] Health Check 엔드포인트 상태 확인 (`/health`)
- [ ] 특정 API 에러 여부 확인 (예: `/api/pets`, `/api/pvp`)

---

### Phase 3: 롤백 전략 선택 (1분)

- [ ] 사용자 활동 여부 확인 (신규 데이터 생성 중인지)
- [ ] 롤백 전략 선택:
  - **새벽 배포 (사용자 없음)**: → **A. DB 롤백** (1-2분)
  - **사용자 활동 중**: → **B. Code 롤백** (5-10분) ⭐ 권장
  - **버그 수정 간단**: → **C. Forward Fix** (10-30분)

---

### Phase 4: 롤백 실행 (1-10분)

- [ ] 선택한 롤백 방법 실행 (위 "긴급 복구 명령어" 참고)
- [ ] Jenkins 파이프라인 재실행 (Code 롤백 시)
- [ ] 앱 서버 재시작 (DB 롤백 시)

---

### Phase 5: 검증 (2분)

- [ ] Health Check 엔드포인트 정상 응답 (`200 OK`)
- [ ] 문제가 되었던 API 정상 동작 확인
- [ ] RDS `__EFMigrationsHistory` 테이블 확인 (마이그레이션 히스토리)
- [ ] 기존 데이터 무결성 확인 (예: 캐릭터 조회, 전투 기록)

---

### Phase 6: 사후 조치 (10분)

- [ ] 팀 채널에 장애 공지 (발생 시간, 원인, 복구 시간)
- [ ] Jenkins 로그 저장 (사고 분석용)
- [ ] 근본 원인 분석 (버그 수정, 테스트 부족, 리뷰 부족)
- [ ] 재발 방지 대책 수립 (코드 리뷰 강화, 테스트 자동화)

---

## 참고 자료

### 프로젝트 내부 문서
- **EF Core Migrations 가이드**: `docs/jenkins/EF_CORE_MIGRATIONS_GUIDE.md`
- **배포 가이드**: `docs/jenkins/DEPLOYMENT_GUIDE.md`
- **Jenkins 파이프라인**: `Jenkinsfile` (Database Migration 스테이지)

### EF Core 공식 문서
- [Managing Migrations](https://learn.microsoft.com/ef-core/managing-schemas/migrations/)
- [Applying Migrations](https://learn.microsoft.com/ef-core/managing-schemas/migrations/applying)
- [Reverting Migrations](https://learn.microsoft.com/ef-core/managing-schemas/migrations/managing?tabs=dotnet-core-cli#remove-a-migration)

### PostgreSQL 공식 문서
- [Backup and Restore](https://www.postgresql.org/docs/current/backup.html)
- [Point-in-Time Recovery](https://www.postgresql.org/docs/current/continuous-archiving.html)

---

## 연락처

**긴급 상황 시 연락처**:
- DevOps 담당자: [담당자 이름/연락처]
- 백엔드 리드: [담당자 이름/연락처]
- Slack 채널: `#idlerpg-alerts`

**Jenkins 접근**:
- Jenkins URL: `http://your-jenkins-url`
- Job 이름: `IdleRPGServer`

**EC2 SSH**:
- Host: `13.209.66.253`
- User: `ec2-user`
- Key: `~/.ssh/idlerpg-key.pem`

**RDS 정보**:
- Host: `idlerpg-dev.chiqweeeuidv.ap-northeast-2.rds.amazonaws.com`
- Database: `idlerpg`
- Username: `postgres`
- Password: Jenkins Credentials (`rds-postgres-password`)

---

**마지막 업데이트**: 2025-11-11
**작성자**: Claude Code AI
**버전**: v1.0
