# EC2 배포 가이드

## 📋 사전 요구사항

### EC2 인스턴스 설정
1. **인스턴스 타입**: t3.small 이상 권장 (t2.micro는 메모리 부족 가능)
2. **OS**: Ubuntu 22.04 LTS
3. **필수 소프트웨어**:
   - Docker
   - Docker Compose
   - Git

### 보안 그룹 설정 (Security Group)
다음 인바운드 규칙 추가:

| 타입 | 프로토콜 | 포트 범위 | 소스 |
|------|---------|----------|------|
| SSH | TCP | 22 | My IP |
| Custom TCP | TCP | 5172 | 0.0.0.0/0 |
| Custom TCP | TCP | 8082 | My IP (선택) |
| PostgreSQL | TCP | 5432 | Security Group 자체 |

## 🚀 배포 방법

### 1. EC2 인스턴스 접속

```bash
ssh -i your-key.pem ubuntu@YOUR_EC2_PUBLIC_IP
```

### 2. Docker 설치 (처음 한 번만)

```bash
# Docker 설치
sudo apt update
sudo apt install -y docker.io docker-compose git

# Docker 권한 설정
sudo usermod -aG docker $USER
newgrp docker

# Docker 서비스 시작
sudo systemctl enable docker
sudo systemctl start docker
```

### 3. 프로젝트 클론

```bash
cd ~
git clone YOUR_REPOSITORY_URL
cd IdleRPGServer
```

### 4. 프로덕션 배포

**방법 A: 자동 스크립트 사용 (권장)**
```bash
chmod +x deploy-ec2.sh
./deploy-ec2.sh
```

**방법 B: 수동 배포**
```bash
# 빌드 및 시작
docker-compose -f docker-compose.production.yml up -d --build

# 로그 확인
docker logs -f idlerpg-api
```

### 5. 배포 확인

```bash
# 컨테이너 상태 확인
docker ps

# API 헬스체크
curl http://localhost:5172/swagger

# 외부에서 접속
# http://YOUR_EC2_PUBLIC_IP:5172/swagger
```

## 📊 서비스 관리

### 로그 확인
```bash
# API 로그
docker logs -f idlerpg-api

# 모든 서비스 로그
docker-compose -f docker-compose.production.yml logs -f
```

### 재시작
```bash
# API만 재시작
docker restart idlerpg-api

# 모든 서비스 재시작
docker-compose -f docker-compose.production.yml restart
```

### 중지
```bash
docker-compose -f docker-compose.production.yml down
```

### 업데이트 배포
```bash
# 코드 업데이트
git pull origin main

# 재배포
./deploy-ec2.sh
```

## 🔒 HTTPS 설정 (선택사항)

현재는 HTTP만 사용하도록 설정되어 있습니다. HTTPS가 필요한 경우:

### 옵션 1: Let's Encrypt + Nginx (무료, 권장)

```bash
# Nginx 설치
sudo apt install -y nginx certbot python3-certbot-nginx

# SSL 인증서 발급 (도메인 필요)
sudo certbot --nginx -d yourdomain.com

# Nginx 리버스 프록시 설정
sudo nano /etc/nginx/sites-available/idlerpg
```

`/etc/nginx/sites-available/idlerpg` 내용:
```nginx
server {
    listen 80;
    server_name yourdomain.com;
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl;
    server_name yourdomain.com;

    ssl_certificate /etc/letsencrypt/live/yourdomain.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/yourdomain.com/privkey.pem;

    location / {
        proxy_pass http://localhost:5172;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

```bash
# Nginx 설정 활성화
sudo ln -s /etc/nginx/sites-available/idlerpg /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

### 옵션 2: AWS Application Load Balancer + ACM
1. AWS Certificate Manager에서 SSL 인증서 발급
2. Application Load Balancer 생성
3. Target Group에 EC2 인스턴스 추가
4. HTTPS 리스너 설정

## ⚠️ 트러블슈팅

### 문제 1: "Cannot connect to the Docker daemon"
```bash
sudo systemctl start docker
sudo usermod -aG docker $USER
newgrp docker
```

### 문제 2: 메모리 부족
```bash
# Swap 메모리 추가 (t2.micro용)
sudo dd if=/dev/zero of=/swapfile bs=128M count=16
sudo chmod 600 /swapfile
sudo mkswap /swapfile
sudo swapon /swapfile
echo '/swapfile swap swap defaults 0 0' | sudo tee -a /etc/fstab
```

### 문제 3: 포트 접속 안 됨
- AWS 보안 그룹에서 포트 5172가 열려있는지 확인
- `sudo ufw status` - 방화벽 상태 확인
- `docker ps` - 컨테이너가 실행 중인지 확인

### 문제 4: Database connection error
```bash
# PostgreSQL 컨테이너 로그 확인
docker logs idlerpg-postgres

# 컨테이너 간 네트워크 확인
docker network inspect idlerpg-network
```

## 📈 모니터링

### 리소스 사용량 확인
```bash
# Docker 리소스 사용량
docker stats

# 시스템 리소스
htop
```

### 데이터베이스 접속
```bash
# psql 접속
docker exec -it idlerpg-postgres psql -U gamedev -d idlerpgdb

# 쿼리 예제
SELECT * FROM "Players" LIMIT 10;
```

## 🔄 백업

### 데이터베이스 백업
```bash
# 백업 생성
docker exec idlerpg-postgres pg_dump -U gamedev idlerpgdb > backup_$(date +%Y%m%d).sql

# 백업 복원
docker exec -i idlerpg-postgres psql -U gamedev idlerpgdb < backup_20250101.sql
```

## 📞 지원

문제가 발생하면:
1. `docker logs idlerpg-api` - API 로그 확인
2. `docker ps` - 컨테이너 상태 확인
3. Security Group 설정 재확인
4. 진단 스크립트 실행: `./diagnose-https.sh`
