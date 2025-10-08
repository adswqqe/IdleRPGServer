# AWS RDS 마이그레이션 완료

## 작업 일시
2025-10-09

## 배경
- 집과 회사에서 모두 개발하는 환경
- 로컬 Docker DB는 데이터 동기화 불가능
- 마이그레이션 스크립트만 Git 공유, 실제 데이터는 동기화 안 됨
- Task 9.3 (캐릭터 생성 및 검증 로직) 작업 중 DB 문제 발견

## 완료된 작업

### 1. AWS RDS PostgreSQL 인스턴스 생성
- **리전**: 서울 (ap-northeast-2)
- **인스턴스 타입**: db.t3.micro (Free Tier)
- **스토리지**: 20GB
- **엔드포인트**: `idlerpg-dev.chiqweeeuidv.ap-northeast-2.rds.amazonaws.com`
- **포트**: 5432
- **데이터베이스 이름**: idlerpg
- **마스터 사용자**: postgres

### 2. 보안 그룹 설정
- VPC 보안 그룹: `idlerpg-db-sg`
- 인바운드 규칙: PostgreSQL (5432) 포트, 사용자 IP만 허용
- 퍼블릭 액세스: 활성화 (개발용)

### 3. 보안 설정 (Git)
**변경 전:**
- `appsettings.json`에 실제 DB 비밀번호 포함
- Git 추적되어 보안 위험 존재

**변경 후:**
- `appsettings.json`: 샘플값으로 변경 (Git 추적 유지)
  ```json
  "DefaultConnection": "Host=localhost;Database=idlerpg;Username=postgres;Password=CHANGE_ME"
  ```
- `appsettings.Development.json`: 실제 RDS 연결 정보 포함 (Git 무시)
  ```json
  "DefaultConnection": "Host=idlerpg-dev.chiqweeeuidv.ap-northeast-2.rds.amazonaws.com;Database=idlerpg;Username=postgres;Password=xocjs1547;Port=5432"
  ```
- `.gitignore`에 `appsettings.Development.json` 추가
- `git rm --cached IdleRPG.API/appsettings.Development.json` 실행하여 Git 추적 제거

### 4. 마이그레이션 적용
**명령어:**
```bash
cd IdleRPG.Infrastructure
dotnet ef database update --startup-project ../IdleRPG.API
```

**적용된 마이그레이션:**
1. `20251001072028_InitialCreate` - Player, RefreshToken 테이블 생성
2. `20251001082640_AddCharacterEntity` - Character 테이블 생성
3. `20251002064144_AddStatPointsToCharacter` - StatPoints 컬럼 추가

**결과:** RDS에 모든 테이블 생성 완료

### 5. 연결 확인
- Rider Database 도구로 RDS 연결 성공
- 테이블 구조 확인 완료
- 데이터 조회 가능 확인

## 프로젝트 구조 변경 사항

### 설정 파일 계층
ASP.NET Core는 다음 순서로 설정을 로드하여 덮어씀:
1. `appsettings.json` (Git 추적, 샘플값)
2. `appsettings.Development.json` (Git 무시, 실제 RDS 정보) ← 개발 환경에서 우선
3. User Secrets
4. 환경 변수

### 다른 PC에서 설정 방법
회사 PC나 다른 환경에서도 동일하게:
1. Git pull로 최신 코드 받기
2. `IdleRPG.API/appsettings.Development.json` 파일 생성
3. RDS 연결 정보 입력 (동일한 엔드포인트, 비밀번호)
4. `dotnet run` 실행하면 자동으로 RDS 사용

## 장점
✅ 집과 회사에서 동일한 데이터 접근
✅ Player, Character 데이터 자동 동기화
✅ 마이그레이션 한 번만 적용하면 모든 환경에 반영
✅ 실무 AWS 경험 학습
✅ 보안 설정 실습 (Git에 비밀번호 노출 방지)

## 주의사항
⚠️ RDS는 12개월 Free Tier 이후 유료 (~$15-20/월)
⚠️ 보안 그룹에 새 IP 추가 필요 시 AWS 콘솔에서 수동 설정
⚠️ `appsettings.Development.json`은 각 PC에서 개별 생성 필요
⚠️ Git History에 이미 커밋된 비밀번호는 별도 정리 필요할 수 있음

## 다음 작업
Task 9.3 - 캐릭터 생성 및 검증 로직 구현으로 복귀:
1. Character 엔티티에 Name 필드 추가
2. CharacterDto, CreateCharacterDto에 Name 필드 추가
3. CharacterRepository에 이름 중복 체크 메서드 추가
4. CharacterService에 이름 중복 검증 로직 추가
5. 마이그레이션 생성 및 RDS 적용
6. Unity 문서 업데이트
