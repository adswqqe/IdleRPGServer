# 🚨 긴급 보안 조치 가이드

**작성일**: 2025년 10월 11일
**심각도**: 🔴 CRITICAL
**조치 필요**: 즉시!

---

## ❌ 발견된 보안 문제

### 1. **DB 비밀번호와 JWT Secret이 GitHub에 노출됨**
- 파일: `docker-compose.prod.yml`
- 노출된 정보:
  - RDS 호스트: `idlerpg-dev.chiqweeeuidv.ap-northeast-2.rds.amazonaws.com`
  - DB 비밀번호: `xocjs1547`
  - JWT Secret: `IdleRPG-Super-Secret-Key-Min-32-Characters-For-HS256-Algorithm`
- **위험도**: 🔴 CRITICAL

### 2. **Redis가 비밀번호 없이 외부에 노출**
- 포트 6379가 인터넷에 노출될 가능성
- 인증 없음
- **위험도**: 🟠 HIGH

### 3. **JWT Secret이 예측 가능**
- 단순한 패턴의 키
- **위험도**: 🟡 MEDIUM

---

## ✅ 즉시 조치 사항 (순서대로 진행)

### Step 1: 환경 변수 파일 설정 ⚠️ 가장 시급!

#### 1-1. 강력한 비밀번호 생성

PowerShell에서 실행:
```powershell
# JWT Secret (64자리 이상 추천)
-join ((48..57) + (65..90) + (97..122) | Get-Random -Count 64 | ForEach-Object {[char]$_})

# RDS Password (32자리)
-join ((48..57) + (65..90) + (97..122) | Get-Random -Count 32 | ForEach-Object {[char]$_})

# Redis Password (32자리)
-join ((48..57) + (65..90) + (97..122) | Get-Random -Count 32 | ForEach-Object {[char]$_})
```

#### 1-2. `.env.production` 파일 업데이트

생성된 비밀번호로 `.env.production` 파일 수정:
```bash
DB_HOST=idlerpg-dev.chiqweeeuidv.ap-northeast-2.rds.amazonaws.com
DB_PORT=5432
DB_NAME=idlerpg
DB_USER=postgres
DB_PASSWORD=여기에_새로_생성한_RDS_비밀번호

JWT_SECRET_KEY=여기에_새로_생성한_JWT_Secret

REDIS_PASSWORD=여기에_새로_생성한_Redis_비밀번호
```

#### 1-3. docker-compose.prod.yml 백업 및 교체

```bash
# 현재 파일 백업 (삭제 전용)
cp docker-compose.prod.yml docker-compose.prod.yml.OLD

# 템플릿으로 교체
cp docker-compose.prod.template.yml docker-compose.prod.yml
```

---

### Step 2: AWS RDS 비밀번호 변경 🔴 긴급!

AWS Console에서:
1. RDS → Databases → `idlerpg-dev` 선택
2. **Modify** 버튼 클릭
3. **New master password** 입력 (Step 1-1에서 생성한 것)
4. **Continue** → **Apply immediately** 선택 ✅
5. **Modify DB Instance** 클릭

**주의**:
- 비밀번호 변경 후 API 서버가 즉시 재시작될 수 있음
- `.env.production` 파일의 비밀번호와 동일하게 입력!

---

### Step 3: EC2에 .env.production 파일 전송

```bash
# 로컬에서 EC2로 전송 (PowerShell)
scp -i idlerpg-key.pem .env.production ec2-user@13.209.66.253:/home/ec2-user/IdleRPGServer/
```

---

### Step 4: EC2에서 Docker 재배포

```bash
# EC2 SSH 접속
ssh -i idlerpg-key.pem ec2-user@13.209.66.253

# 프로젝트 디렉토리로 이동
cd /home/ec2-user/IdleRPGServer

# 기존 컨테이너 중지
docker-compose -f docker-compose.prod.yml down

# 새 설정으로 재시작
docker-compose -f docker-compose.prod.yml up -d --build

# 로그 확인
docker-compose -f docker-compose.prod.yml logs -f api
```

**예상 에러 확인:**
- ✅ DB 연결 성공 여부
- ✅ Redis 연결 성공 여부
- ✅ JWT 토큰 생성 가능 여부

---

### Step 5: EC2 Security Group 검토

AWS Console → EC2 → Security Groups:

#### 현재 설정 확인:
```
Port 8080: 나의 IP + GitHub IP 범위 ✅
Port 5172: ??? (API 서버)
Port 6379: ??? (Redis)
```

#### 권장 설정:
```
Port 8080 (Jenkins):
  - 나의 IP만 허용
  - GitHub IP 범위 허용 (webhook용)

Port 5172 (API):
  - 0.0.0.0/0 (전체 허용) - Unity 클라이언트 접근용
  - 또는 CloudFront/Load Balancer 사용 추천

Port 6379 (Redis):
  - ❌ 외부 접근 차단!
  - 내부 Docker network만 사용 (이미 템플릿에서 수정됨)
```

#### Redis 포트 확인 명령:
```bash
# EC2에서
sudo netstat -tulpn | grep 6379
```

**만약 0.0.0.0:6379 로 노출되어 있다면:**
→ Security Group에서 6379 인바운드 규칙 삭제!

---

### Step 6: Git 히스토리에서 시크릿 제거

**⚠️ 주의**: 이 작업은 Git 히스토리를 다시 쓰므로 신중하게!

#### 옵션 A: BFG Repo-Cleaner (추천)

```bash
# BFG 다운로드
# https://rtyley.github.io/bfg-repo-cleaner/

# Git clone (mirror)
git clone --mirror https://github.com/adswqqe/IdleRPGServer.git

# 시크릿 제거
java -jar bfg.jar --replace-text passwords.txt IdleRPGServer.git

# passwords.txt 내용:
# xocjs1547
# IdleRPG-Super-Secret-Key-Min-32-Characters-For-HS256-Algorithm

# Git reflog 정리
cd IdleRPGServer.git
git reflog expire --expire=now --all
git gc --prune=now --aggressive

# Force push
git push --force
```

#### 옵션 B: GitHub에서 Repository 삭제 후 재생성 (가장 확실)

1. GitHub에서 repository 삭제
2. 로컬에서:
```bash
# Git 히스토리 삭제
rm -rf .git

# 새 Git 초기화
git init
git add .
git commit -m "Initial commit with secrets removed"

# 새 repository 생성 후 push
git remote add origin https://github.com/adswqqe/IdleRPGServer.git
git branch -M master
git push -u origin master
```

---

### Step 7: appsettings.json JWT Secret 업데이트

`IdleRPG.API/appsettings.json`:
```json
{
  "Jwt": {
    "SecretKey": "USE_ENVIRONMENT_VARIABLE",  // 환경 변수 사용 힌트
    ...
  }
}
```

**또는** 환경 변수가 없을 때만 사용되도록 코드 수정.

---

## ✅ 조치 완료 체크리스트

- [ ] Step 1: 강력한 비밀번호 생성
- [ ] Step 1: `.env.production` 파일 업데이트
- [ ] Step 1: `docker-compose.prod.yml` 템플릿으로 교체
- [ ] Step 2: AWS RDS 비밀번호 변경
- [ ] Step 3: EC2에 `.env.production` 전송
- [ ] Step 4: Docker 재배포 및 동작 확인
- [ ] Step 5: EC2 Security Group 검토 (특히 6379 포트!)
- [ ] Step 6: Git 히스토리 정리 (BFG 또는 Repository 재생성)
- [ ] Step 7: `appsettings.json` 업데이트

---

## 🔐 추가 보안 강화 (선택)

### 1. AWS Secrets Manager 사용

장기적으로 시크릿을 AWS Secrets Manager로 이동:
```csharp
// Program.cs
builder.Configuration.AddSecretsManager();
```

### 2. API Rate Limiting

DoS 공격 방지:
```csharp
builder.Services.AddRateLimiter(options => {
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
        context => RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: partition => new FixedWindowRateLimiterOptions {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }
        )
    );
});
```

### 3. HTTPS 설정

Let's Encrypt SSL 인증서:
```bash
# EC2에서
sudo yum install certbot
sudo certbot certonly --standalone -d your-domain.com
```

### 4. JWT Refresh Token Rotation

한 번 사용한 Refresh Token은 무효화.

---

## 📝 보안 모범 사례

1. **절대 Git에 커밋하지 말 것**:
   - 비밀번호
   - API Keys
   - JWT Secrets
   - Private Keys (.pem, .ppk)
   - `.env` 파일

2. **환경별 설정 분리**:
   - Development: `appsettings.Development.json`
   - Production: 환경 변수 (`.env.production`)

3. **정기적인 비밀번호 변경**:
   - 3개월마다 RDS 비밀번호 변경
   - JWT Secret도 주기적 rotation

4. **최소 권한 원칙**:
   - DB 사용자는 필요한 권한만
   - Security Group은 필요한 포트만 개방

---

## 🆘 긴급 연락처

문제 발생 시:
1. 먼저 모든 서비스 중지: `docker-compose down`
2. Security Group에서 모든 포트 임시 차단
3. RDS 비밀번호 즉시 변경

---

**마지막 업데이트**: 2025-10-11
**다음 검토 예정**: 2025-11-11 (1개월 후)
**책임자**: 개발자 본인
