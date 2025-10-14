# EC2 환경 변수 설정 가이드

## 개요
`docker-compose.production.yml`이 환경 변수를 사용하도록 변경되었습니다. EC2에서 `.env` 파일을 생성하여 RDS 연결 정보를 설정해야 합니다.

## EC2에서 설정하기

### 1. EC2에 SSH 접속
```bash
ssh -i your-key.pem ec2-user@13.209.66.253
```

### 2. 프로젝트 디렉토리로 이동
```bash
cd /home/ec2-user/IdleRPGServer
```

### 3. .env.production 파일 생성
```bash
cat > .env.production << 'EOF'
# ======================================
# RDS PostgreSQL 연결 정보
# ======================================
DB_HOST=idlerpg-dev.chiqweeeuidv.ap-northeast-2.rds.amazonaws.com
DB_NAME=idlerpg
DB_USER=postgres
DB_PASSWORD=xocjs1547
DB_PORT=5432

# ======================================
# JWT Secret Key (64자 이상 권장)
# ⚠️ 실제 프로덕션 환경에서는 강력한 비밀번호로 변경!
# ======================================
JWT_SECRET_KEY=ThisIsAVerySecureSecretKeyForJWTTokenGenerationPleaseChangeThis1234567890

# ======================================
# Redis Password (64자 이상 권장)
# ⚠️ 실제 프로덕션 환경에서는 강력한 비밀번호로 변경!
# ======================================
REDIS_PASSWORD=ThisIsAVerySecureRedisPasswordPleaseChangeThisInProduction12345678
EOF
```

**⚠️ 보안 강화:** JWT_SECRET_KEY와 REDIS_PASSWORD는 위 예시 값을 사용하지 말고, 아래 명령어로 강력한 비밀번호를 생성하세요:
```bash
# JWT Secret Key 생성 (64자)
openssl rand -base64 48

# Redis Password 생성 (64자)
openssl rand -base64 48
```

### 4. 파일 권한 설정 (보안)
```bash
chmod 600 .env.production
```

### 5. .env.production 파일 확인
```bash
cat .env.production
```

### 6. Docker Compose에서 환경 변수 사용 확인
`docker-compose.prod.yml` 파일에서 다음과 같이 설정되어 있어야 합니다:
```yaml
env_file:
  - .env.production
environment:
  ConnectionStrings__DefaultConnection: "Host=${DB_HOST};Database=${DB_NAME};Username=${DB_USER};Password=${DB_PASSWORD};Port=${DB_PORT}"
```

### 7. Docker Compose 재시작
Jenkins가 자동으로 배포하지만, 수동으로 재시작하려면:
```bash
cd /home/ec2-user/IdleRPGServer
docker-compose -f docker-compose.production.yml down
docker-compose -f docker-compose.production.yml up -d --build
```

## 검증

### API 컨테이너의 환경 변수 확인
```bash
docker exec idlerpg-api env | grep ConnectionStrings
```

출력 예시:
```
ConnectionStrings__DefaultConnection=Host=idlerpg-dev.chiqweeeuidv.ap-northeast-2.rds.amazonaws.com;Port=5432;...
```

### RDS 연결 테스트
```bash
# API가 RDS에 제대로 연결되었는지 확인
curl http://localhost:5172/health

# Character 조회 테스트
curl http://localhost:5172/api/character/GetCharacters \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

### 데이터베이스 직접 확인
```bash
# RDS PostgreSQL 직접 연결
PGPASSWORD=xocjs1547 psql \
  -h idlerpg-dev.chiqweeeuidv.ap-northeast-2.rds.amazonaws.com \
  -U postgres \
  -d idlerpg \
  -c "SELECT * FROM \"Characters\" LIMIT 5;"
```

## 보안 주의사항

### ⚠️ 중요: .env 파일은 Git에 절대 커밋하지 마세요!

`.gitignore`에 다음 항목이 있는지 확인:
```
.env
.env.local
.env.production
.env.*.local
```

### 현재 보안 상태
- ✅ `docker-compose.production.yml`은 환경 변수 참조만 포함
- ✅ 실제 비밀번호는 EC2의 `.env` 파일에만 존재
- ✅ `.env` 파일 권한은 600 (소유자만 읽기/쓰기)
- ✅ Git 저장소에는 비밀번호가 포함되지 않음

## 트러블슈팅

### 문제: API가 여전히 로컬 PostgreSQL에 연결
**증상**: Rider에서는 데이터가 안 보이는데 EC2 컨테이너에서는 보임

**해결**:
1. `.env` 파일이 제대로 생성되었는지 확인
2. `docker-compose.production.yml`이 최신 버전인지 확인 (git pull)
3. Docker 컨테이너 완전 재시작:
   ```bash
   docker-compose -f docker-compose.production.yml down
   docker-compose -f docker-compose.production.yml up -d --build
   ```

### 문제: "no space left on device"
**해결**:
```bash
# Docker 정리 (주의: 모든 컨테이너와 이미지 삭제)
docker system prune -a -f

# 볼륨은 유지하면서 정리
docker system prune -f
```

### 문제: Jenkins 빌드 후에도 변경사항 반영 안됨
**해결**:
```bash
# Jenkins가 사용하는 docker-compose 파일 확인
cat Jenkinsfile | grep docker-compose

# Jenkinsfile에서 docker-compose.prod.yml을 사용한다면
# 해당 파일도 업데이트 필요
```

## 파일 위치

- **프로덕션 설정**: `/home/ec2-user/IdleRPGServer/docker-compose.production.yml`
- **환경 변수**: `/home/ec2-user/IdleRPGServer/.env`
- **Jenkins 파이프라인**: `/home/ec2-user/IdleRPGServer/Jenkinsfile`

## 다음 단계

1. ✅ `.env` 파일 생성 완료
2. ✅ Jenkins 자동 배포 대기
3. ⏳ API 컨테이너 재시작 확인
4. ⏳ RDS 연결 검증
5. ⏳ Rider에서 데이터 조회 테스트

---

**마지막 업데이트**: 2025-10-14
**관련 커밋**: d5567b0 - refactor: Use environment variable for DB connection
