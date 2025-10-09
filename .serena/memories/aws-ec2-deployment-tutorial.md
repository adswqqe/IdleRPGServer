# AWS EC2 배포 완전 가이드 (처음부터 끝까지)

## 세션 개요
- 날짜: 2025-01-10
- 목표: IdleRPG 서버를 AWS EC2에 배포
- 결과: ✅ 성공 - http://13.125.206.100:5172/swagger

## Phase 1: Docker 환경 구성

### 1.1 Dockerfile 작성
- Multi-stage build 패턴 사용
- Stage 1 (BUILD): .NET SDK로 컴파일
- Stage 2 (RUNTIME): ASP.NET Runtime으로 실행
- 최종 이미지 크기 최소화 (~200MB)

**핵심 포인트:**
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
RUN dotnet restore
RUN dotnet publish --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "IdleRPG.API.dll"]
```

### 1.2 docker-compose.prod.yml 작성
- API 서버 컨테이너 정의
- Redis 컨테이너 추가 (ElastiCache 대신 무료)
- 환경변수로 RDS 연결 설정

**환경변수 패턴:**
- `ConnectionStrings__DefaultConnection` = appsettings.json의 섹션:키

### 1.3 .gitignore 업데이트
- `*.ppk`, `*.pem` 추가 (SSH 키 보안)
- GitHub에 민감 정보 업로드 방지

## Phase 2: AWS 인프라 설정

### 2.1 Billing Alarm 설정 (과금 방지)
**순서:**
1. CloudWatch (us-east-1 리전) → Alarms → Billing
2. "Create alarm" → Billing → Total Estimated Charge (USD)
3. Threshold: Greater than $5
4. SNS Topic 생성 → 이메일 입력
5. 이메일 확인 링크 클릭 (중요!)

**교훈:** 항상 과금 알람부터 설정!

### 2.2 EC2 인스턴스 생성
**설정 값:**
- Name: idlerpg-server
- AMI: Amazon Linux 2023
- Instance type: t3.micro (서울 리전 프리티어)
- Key pair: idlerpg-key.ppk (새로 생성)
- Security group: idlerpg-sg

**보안 그룹 규칙 (Inbound):**
```
SSH (22): My IP
API HTTP (5172): 0.0.0.0/0 (Anywhere)
API HTTPS (7122): 0.0.0.0/0 (Anywhere)
Redis (6379): My IP (디버깅용)
```

**주의사항:**
- Auto-assign public IP: Enable 필수!
- .ppk 파일 절대 분실 금지

### 2.3 SSH 접속 (PuTTY)
**PuTTY 설정:**
- Host: ec2-user@13.125.206.100
- Port: 22
- SSH → Auth → Credentials: idlerpg-key.ppk

**첫 접속:** "Accept" 클릭

## Phase 3: EC2 서버 환경 설정

### 3.1 시스템 업데이트
```bash
sudo yum update -y
```

### 3.2 Docker 설치
```bash
sudo yum install docker -y
sudo systemctl start docker
sudo systemctl enable docker
sudo usermod -aG docker ec2-user
```

### 3.3 Docker Compose 설치
```bash
sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose
```

### 3.4 Git 설치
```bash
sudo yum install git -y
```

### 3.5 재접속 (docker 그룹 권한 적용)
```bash
exit  # PuTTY 종료 후 재접속
docker ps  # sudo 없이 실행 확인
```

## Phase 4: 배포 프로세스

### 4.1 코드 준비 (로컬)
```bash
# .gitignore에 *.ppk 추가
git add Dockerfile docker-compose.prod.yml .gitignore
git commit -m "feat: Add AWS EC2 deployment configuration"
git push origin master
```

### 4.2 EC2에서 Clone
```bash
cd ~
git clone https://github.com/adswqqe/IdleRPGServer.git
cd IdleRPGServer
```

### 4.3 이미지 빌드
```bash
docker-compose -f docker-compose.prod.yml build
# 또는 캐시 문제 시
docker-compose -f docker-compose.prod.yml build --no-cache
```

**소요 시간:** 첫 빌드 5~10분 (SDK 다운로드)

### 4.4 컨테이너 실행
```bash
docker-compose -f docker-compose.prod.yml up -d
```

### 4.5 상태 확인
```bash
docker-compose -f docker-compose.prod.yml ps
docker-compose -f docker-compose.prod.yml logs api --tail 50
```

**정상 메시지:**
```
Now listening on: http://[::]:5172
Application started.
```

## Phase 5: 트러블슈팅 사례

### 문제 1: Swagger 404 에러
**원인:** Program.cs에서 개발 환경에서만 Swagger 활성화
```csharp
if (app.Environment.IsDevelopment())  // 프로덕션에서 비활성화됨!
```

**해결:**
```csharp
// 테스트용으로 항상 활성화
app.UseSwagger();
app.UseSwaggerUI();
```

**재배포:**
```bash
git pull origin master
docker-compose -f docker-compose.prod.yml build --no-cache
docker-compose -f docker-compose.prod.yml down
docker-compose -f docker-compose.prod.yml up -d
```

### 문제 2: 빌드가 0.0초로 끝남
**원인:** Docker 캐시 사용
**해결:** `--no-cache` 플래그 사용

### 문제 3: 브라우저 404 vs EC2 내부 성공
**원인:** 브라우저 캐시 또는 보안 그룹
**해결:** Ctrl+F5 강력 새로고침

## 핵심 개념 정리

### Docker Multi-stage Build
**목적:** 이미지 크기 최소화
- SDK (700MB) → 빌드만 사용
- Runtime (200MB) → 최종 이미지

### 환경변수 오버라이드
```
appsettings.json → 환경변수 → 최종 설정
```

### Docker Compose 서비스
- api: .NET 애플리케이션
- redis: 캐싱 레이어
- network: 컨테이너 간 통신

### AWS 보안 그룹
- Stateful 방화벽
- Inbound: 외부 → 서버
- Outbound: 서버 → 외부 (기본 All)

## 배포 체크리스트

### 코드 변경 시
- [ ] 로컬에서 git commit & push
- [ ] EC2에 SSH 접속
- [ ] git pull origin master
- [ ] docker-compose build (필요시 --no-cache)
- [ ] docker-compose down
- [ ] docker-compose up -d
- [ ] 로그 확인: docker-compose logs api
- [ ] 브라우저 테스트

### 트러블슈팅 순서
1. `docker-compose ps` - 컨테이너 실행 중?
2. `docker-compose logs api` - 에러 메시지?
3. `curl localhost:5172` - EC2 내부 접속?
4. AWS Security Group - 포트 열림?
5. 브라우저 캐시 - Ctrl+F5

## 학습 성과

### 기술 습득
1. ✅ Docker 개념 및 실습
2. ✅ AWS EC2 인스턴스 관리
3. ✅ 보안 그룹 설정
4. ✅ SSH 키 기반 인증
5. ✅ Git 기반 배포 워크플로우
6. ✅ 환경변수 관리

### 문제 해결 능력
1. ✅ 404 에러 진단 및 해결
2. ✅ Docker 캐시 이슈 해결
3. ✅ 환경 설정 차이 이해
4. ✅ 로그 분석 및 디버깅

### DevOps 경험
1. ✅ 로컬 → 프로덕션 배포 전체 과정
2. ✅ 인프라 코드화 (Dockerfile, docker-compose)
3. ✅ 비용 관리 (Billing Alarm)
4. ✅ 보안 고려사항 (SSH 키, 보안 그룹)

## 다음 단계 로드맵

### 단기 (1주일 내)
1. Unity 클라이언트 연동
2. 실제 회원가입/로그인 테스트
3. Character API 테스트

### 중기 (1개월 내)
1. GitHub Actions CI/CD 구축
2. HTTPS 인증서 설정
3. 환경변수 .env 파일 분리

### 장기 (3개월 내)
1. 도메인 연결 (Route 53)
2. CloudWatch 모니터링
3. Auto Scaling 설정
4. RDS 백업 전략

## 중요 명령어 모음

### EC2 접속
```bash
# PuTTY: ec2-user@13.125.206.100 + idlerpg-key.ppk
```

### 배포
```bash
cd ~/IdleRPGServer
git pull origin master
docker-compose -f docker-compose.prod.yml build --no-cache
docker-compose -f docker-compose.prod.yml down
docker-compose -f docker-compose.prod.yml up -d
```

### 모니터링
```bash
docker-compose -f docker-compose.prod.yml ps
docker-compose -f docker-compose.prod.yml logs api --tail 100
docker-compose -f docker-compose.prod.yml logs api -f  # 실시간
```

### 재시작
```bash
docker-compose -f docker-compose.prod.yml restart api
```

## 리소스 정보

### 접속 정보
- API URL: http://13.125.206.100:5172
- Swagger: http://13.125.206.100:5172/swagger
- EC2 IP: 13.125.206.100
- SSH: ec2-user@13.125.206.100

### AWS 리소스
- EC2: idlerpg-server (t3.micro)
- Security Group: idlerpg-sg
- RDS: idlerpg-dev.chiqweeeuidv.ap-northeast-2.rds.amazonaws.com
- Billing Alarm: BillingAlarm-5USD

### GitHub
- Repository: https://github.com/adswqqe/IdleRPGServer.git
- Branch: master

## 비용 관리

### 프리티어 한도
- EC2 t3.micro: 750시간/월
- RDS db.t2.micro: 750시간/월  
- 데이터 전송: 15GB/월

### 비용 발생 가능성
- ❌ ElastiCache Redis (사용 안 함, Docker 사용)
- ❌ Application Load Balancer (사용 안 함)
- ❌ Elastic IP (사용 안 함, 일반 Public IP)
- ✅ **예상 월 비용: $0** (프리티어 범위 내)

### 알람 설정
- $5 초과 시 이메일 알림
- 이메일: adswqqe@gmail.com

## 보안 고려사항

### 현재 보안 수준
- ✅ SSH 키 기반 인증
- ✅ .gitignore에 민감 정보 제외
- ✅ 보안 그룹으로 포트 제한
- ⚠️ HTTP (HTTPS 미적용)
- ⚠️ Swagger 공개 (테스트용)
- ⚠️ 환경변수 평문 (RDS 비밀번호)

### 개선 필요 사항
1. HTTPS 적용 (Let's Encrypt)
2. Swagger 프로덕션에서 비활성화
3. 환경변수 암호화 (AWS Secrets Manager)
4. 정기적인 보안 업데이트

## 마무리

이 배포 과정을 통해 **로컬 개발에서 클라우드 배포까지의 전체 파이프라인**을 경험했습니다.

**핵심 성과:**
- Docker로 환경 일관성 확보
- Git으로 버전 관리
- AWS로 실제 서비스 운영
- 비용 관리 및 보안 고려

**다음 학습 추천:**
1. GitHub Actions로 자동 배포
2. Kubernetes 기초
3. 모니터링 및 로깅
4. 성능 최적화
