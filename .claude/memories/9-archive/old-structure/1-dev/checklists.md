# 개발 체크리스트 및 명령어

## Unity 문서 업데이트 체크리스트

### ⚠️ CRITICAL: API/DTO 추가/수정 시 반드시 실행

**언제?**
- Controller 추가/수정 시
- DTO 필드 변경 시
- 응답 형식 변경 시

**체크리스트**:
- [ ] **1. 폴더 생성**: `../IdleRPGClient/Docs/unity/{feature}/`
- [ ] **2. API_SPEC.md 작성**
  - [ ] 엔드포인트 목록 (Method, URL, 설명)
  - [ ] Request 예제 (JSON)
  - [ ] Response 예제 (JSON)
  - [ ] Unity C# 코드 예제 (UnityWebRequest)
- [ ] **3. DTOs.cs 작성**
  - [ ] `[Serializable]` 어트리뷰트
  - [ ] `[JsonProperty("fieldName")]` 어트리뷰트
  - [ ] Newtonsoft.Json 기준
- [ ] **4. README.md 업데이트**
  - [ ] 구현 상태 테이블에 엔드포인트 추가
  - [ ] 버전 히스토리 추가

---

## 자주 쓰는 명령어

### 로컬 개발 환경

**개발 서버 시작**:
```bash
# PostgreSQL + Redis + pgAdmin 시작
./dev-start.sh

# API 서버 시작 (Hot Reload)
cd IdleRPG.API
dotnet run

# 또는 Watch 모드
dotnet watch run
```

**접속 주소**:
- Swagger: http://localhost:5172/swagger (HTTP)
- Swagger: https://localhost:7122/swagger (HTTPS)
- pgAdmin: http://localhost:8082
  - Email: admin@idlerpg.com
  - Password: admin123

### 빌드 & 테스트

```bash
# 전체 솔루션 빌드
dotnet build IdleRPGServer.sln

# 특정 프로젝트 빌드
dotnet build IdleRPG.API

# 모든 테스트 실행
dotnet test

# 특정 테스트만 실행
dotnet test --filter FullyQualifiedName~GachaLogicServiceTests

# 테스트 커버리지 (선택)
dotnet test /p:CollectCoverage=true
```

### EF Core 마이그레이션

**⚠️ CRITICAL: 로컬에서만 Migration 생성, 절대 `dotnet ef database update` 금지**

```bash
# Migration 생성 (Infrastructure 폴더에서)
cd IdleRPG.Infrastructure
dotnet ef migrations add <MigrationName> --startup-project ../IdleRPG.API

# 예시
dotnet ef migrations add AddSkillSystem --startup-project ../IdleRPG.API

# migration.sql 생성 (Jenkins 자동 적용용)
# 생성된 Migration 파일 확인 후 수동으로 SQL 작성

# ❌ 절대 금지 (release 브랜치에서)
# dotnet ef database update
```

**Jenkins 배포 워크플로우**:
1. Migration 생성 (로컬)
2. `migration.sql` 작성 (로컬)
3. Git Commit & Push
4. Jenkins가 RDS에 자동 적용

### Git 워크플로우

**커밋**:
```bash
git status
git diff
git add .
git commit -m "feat(skill): Add skill gacha system

- Implement GachaLogicService with probability logic
- Add SkillTemplate and PlayerSkill entities
- Create skill gacha API endpoints

🤖 Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>"
```

**푸시**:
```bash
git push origin master
```

**브랜치 (필요시)**:
```bash
git checkout -b feature/skill-system
git push -u origin feature/skill-system
```

### Docker

**로컬 환경 (dev-start.sh 내부)**:
```bash
docker-compose up -d
docker-compose down
docker-compose logs -f
```

**Production 배포 (Jenkins)**:
```bash
# 로컬에서 실행 금지
docker build -t idlerpg-api .
docker run -d -p 5172:8080 idlerpg-api
```

### NuGet 패키지

**패키지 추가**:
```bash
# 특정 프로젝트에 추가
cd IdleRPG.Infrastructure
dotnet add package Newtonsoft.Json

# 버전 지정
dotnet add package StackExchange.Redis --version 2.6.0
```

**패키지 복원**:
```bash
dotnet restore
```

---

## 작업별 체크리스트

### 새 기능 추가 시

**1. 설계 단계**:
- [ ] 엔티티 관계 확정 (사용자 승인)
- [ ] 비즈니스 규칙 확정 (TODO(human) 구현)
- [ ] API 엔드포인트 설계

**2. 구현 단계**:
- [ ] Domain Entity 생성
- [ ] Application DTO 생성
- [ ] Application Service Interface 정의
- [ ] Infrastructure Repository 구현
- [ ] Infrastructure Service 구현
- [ ] API Controller 생성
- [ ] DI 등록 (`Program.cs`)

**3. Database 단계**:
- [ ] EF Core Configuration 작성
- [ ] Migration 생성
- [ ] `migration.sql` 작성
- [ ] Seeder 작성 (마스터 데이터)

**4. 테스트 단계**:
- [ ] Domain Service 단위 테스트
- [ ] Application Service 단위 테스트
- [ ] Controller 테스트 (선택)
- [ ] 모든 테스트 통과 확인

**5. 문서화 단계**:
- [ ] Unity API_SPEC.md 작성
- [ ] Unity DTOs.cs 작성
- [ ] Unity README.md 업데이트
- [ ] Swagger XML 주석 추가

**6. 배포 단계**:
- [ ] Git Commit (feat/fix/refactor)
- [ ] Git Push
- [ ] Jenkins 배포 확인
- [ ] Production 테스트 (Swagger)

---

## 버그 수정 시

**1. 재현**:
- [ ] 버그 재현 단계 확인
- [ ] 로그 확인 (Serilog)
- [ ] DB 상태 확인 (pgAdmin)

**2. 수정**:
- [ ] 원인 파악
- [ ] 코드 수정
- [ ] 회귀 테스트 추가

**3. 검증**:
- [ ] 로컬 테스트
- [ ] 단위 테스트 통과
- [ ] Production 배포 후 재검증

---

## 리팩토링 시

**Before**:
- [ ] 기존 테스트 모두 통과 확인
- [ ] 변경 범위 파악

**During**:
- [ ] 한 번에 하나씩 변경
- [ ] 테스트 계속 실행
- [ ] Commit 자주 (리팩토링 단위별)

**After**:
- [ ] 모든 테스트 통과
- [ ] 기능 동작 확인
- [ ] 성능 저하 없는지 확인

---

## Production 배포 전 체크리스트

- [ ] 모든 테스트 통과
- [ ] Migration 검증 (`migration.sql` 확인)
- [ ] Unity 문서 최신화
- [ ] appsettings.Production.json 확인
- [ ] 민감 정보 로그 제거
- [ ] Swagger 비활성화 (Production)
- [ ] HTTPS 강제 적용
- [ ] CORS 설정 확인

---

## 주간 회고 체크리스트

**매주 금요일**:
- [ ] 구현 완료된 기능 목록 작성
- [ ] 다음 주 우선순위 확정
- [ ] 메모리 파일 업데이트
  - [ ] `2-current/status.md` (API 목록, 테이블 목록)
  - [ ] `2-current/roadmap.md` (Phase 진행률)
- [ ] 완료 기능은 `9-archive/features/`로 이동
- [ ] 체크포인트 파일 생성 (`9-archive/checkpoints/`)

---

## 긴급 상황 대응

### Production 장애

**1. 롤백**:
```bash
# Jenkins에서 이전 버전으로 롤백
# 또는 Git Revert
git revert <commit-hash>
git push
```

**2. 긴급 수정**:
```bash
# Hotfix 브랜치 생성
git checkout -b hotfix/critical-bug

# 수정 후 즉시 배포
git commit -m "hotfix: Fix critical bug"
git push origin hotfix/critical-bug

# Jenkins 즉시 배포
```

### DB Migration 실패

**1. 확인**:
- Jenkins 로그 확인
- RDS 상태 확인

**2. 수정**:
- `migration.sql` 수정
- Git Commit & Push
- Jenkins 재배포

---

## 개발 환경 초기화

**PostgreSQL 데이터 초기화**:
```bash
docker-compose down -v
./dev-start.sh
```

**NuGet 패키지 재설치**:
```bash
dotnet clean
dotnet restore
dotnet build
```

**Git 상태 초기화**:
```bash
git reset --hard HEAD
git clean -fd
```

---

## VS Code 단축키 (추천)

- `Ctrl+Shift+B`: 빌드
- `F5`: 디버그 시작
- `Ctrl+Shift+P → "OmniSharp: Restart"`: IntelliSense 재시작
- `Ctrl+.`: Quick Fix
- `F12`: Go to Definition
- `Shift+F12`: Find All References
