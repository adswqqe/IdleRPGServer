# AWS EC2 배포 환경 설정 완료 (2025-01-10)

## 완료된 작업

### 1. Docker 환경 구성
- **Dockerfile** 작성 완료
  - Multi-stage build (SDK → Runtime)
  - 베이스 이미지: mcr.microsoft.com/dotnet/sdk:8.0 & aspnet:8.0
  - 포트: 5172 (HTTP), 7122 (HTTPS)
  
- **docker-compose.prod.yml** 작성 완료
  - API 서버 컨테이너
  - Redis 컨테이너 (캐싱용)
  - RDS 연결 설정

### 2. AWS 설정

#### Billing Alarm
- CloudWatch Alarm 생성: BillingAlarm-5USD
- SNS Topic: billing-alarm-topic
- 알림 이메일: adswqqe@gmail.com (확인 완료)
- 임계값: $5 초과 시 알림

#### EC2 인스턴스
- **인스턴스 정보**
  - Name: idlerpg-server
  - Instance Type: t3.micro (프리티어)
  - AMI: Amazon Linux 2023
  - Region: ap-northeast-2 (서울)
  - Public IP: **13.125.206.100**
  
- **보안 그룹 (idlerpg-sg)**
  - SSH (22): My IP
  - API HTTP (5172): Anywhere IPv4
  - API HTTPS (7122): Anywhere IPv4
  - Redis (6379): My IP
  
- **SSH 키**
  - Key name: idlerpg-key
  - File: idlerpg-key.ppk (다운로드 완료)

#### RDS 정보 (기존)
- Endpoint: idlerpg-dev.chiqweeeuidv.ap-northeast-2.rds.amazonaws.com
- Database: idlerpg
- Username: postgres
- Port: 5432

### 3. EC2 서버 환경 설정 완료

설치된 도구:
```bash
# Docker
Docker version 25.x.x

# Docker Compose
Docker Compose version v2.x.x

# Git
git version 2.x.x
```

사용자 권한:
- ec2-user가 docker 그룹에 추가됨
- sudo 없이 docker 명령 실행 가능

### 4. 다음 단계 (예정)

1. 배포 스크립트 작성
   - 로컬에서 EC2로 코드 전송
   - Docker 이미지 빌드 및 실행
   
2. GitHub Actions CI/CD 파이프라인
   - 자동 빌드 및 배포
   
3. 실제 배포 테스트

## 중요 정보

**EC2 접속 명령어 (PuTTY)**
- Host: ec2-user@13.125.206.100
- Port: 22
- Auth: idlerpg-key.ppk

**환경변수 (docker-compose.prod.yml)**
- ASPNETCORE_ENVIRONMENT: Production
- ASPNETCORE_URLS: http://+:5172
- ConnectionStrings__DefaultConnection: RDS 연결 문자열
- Jwt__SecretKey: 보안키

## 보안 고려사항

⚠️ docker-compose.prod.yml에 RDS 비밀번호 평문 포함
   → 추후 .env 파일 또는 AWS Secrets Manager로 이동 필요

⚠️ Public IP는 EC2 재시작 시 변경됨
   → 고정 IP 필요 시 Elastic IP 사용 (프리티어 1개 무료)
