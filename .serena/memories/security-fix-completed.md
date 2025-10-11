# 보안 수정 완료 기록

**날짜**: 2025-10-11
**상태**: ✅ 완료

## 발견된 보안 취약점

### 1. Git에 노출된 시크릿
- `docker-compose.prod.yml`에 하드코딩된 DB 비밀번호
- `docker-compose.prod.yml`에 하드코딩된 JWT Secret
- RDS 호스트 주소 노출
- Redis 비밀번호 없음

### 2. 취약한 비밀번호
- DB 비밀번호: 짧고 단순 (xocjs1547)
- JWT Secret: 예측 가능한 패턴
- Redis: 비밀번호 미설정

## 적용된 해결 방법

### 1. 강력한 비밀번호 생성 (64자)
```powershell
# PowerShell로 생성
-join ((48..57)+(65..90)+(97..122) | Get-Random -Count 64 | %{[char]$_})
```

생성된 비밀번호:
- JWT_SECRET_KEY: ZhvnwESyWx7bz8jA6l4sHXtraTRkgomV3LCqu9Mcd0UeFiGOPQKIpDN21YJ5fB
- RDS_PASSWORD: AZ9ifIytNg5aRcVkxT7nYF0Ob13oS6hjQ4PGXdJ8DKCzqLUmBvreMw2EplWsHu
- REDIS_PASSWORD: zIGQWcD8ghSOVABj20iR97HtKeqMZP3bTvNap5dLxofYs4JnwEFXr16lyUumkC

### 2. 환경변수 분리
**`.env.production` 생성** (Git 추적 안 됨):
```bash
DB_HOST=idlerpg-dev.chiqweeeuidv.ap-northeast-2.rds.amazonaws.com
DB_PORT=5432
DB_NAME=idlerpg
DB_USER=postgres
DB_PASSWORD=AZ9ifIy...

JWT_SECRET_KEY=ZhvnwE...
REDIS_PASSWORD=zIGQW...
```

### 3. docker-compose.prod.yml 템플릿화
환경변수 참조로 변경:
```yaml
environment:
  ConnectionStrings__DefaultConnection: "Host=${DB_HOST};Database=${DB_NAME};..."
  Jwt__SecretKey: "${JWT_SECRET_KEY}"
  Redis__Password: "${REDIS_PASSWORD}"
env_file:
  - .env.production
```

### 4. .gitignore 업데이트
```bash
# Environment variables
.env
.env.*
!.env.example

# Production configurations
docker-compose.prod.yml
*.pem
*.ppk
```

### 5. AWS RDS 비밀번호 변경
- AWS Console에서 RDS 비밀번호 변경
- "Apply immediately" 체크
- 2-5분 대기 후 적용 완료

### 6. RDS Security Group 설정
**추가한 Inbound Rule**:
```
Type: PostgreSQL
Port: 5432
Source: <EC2 Security Group ID> 또는 13.209.66.253/32
Description: Allow EC2 to access RDS
```

## 배포 과정

### 1. PEM 키 권한 수정 (Windows)
```powershell
icacls idlerpg-key.pem /inheritance:r
icacls idlerpg-key.pem /grant:r "$($env:USERNAME):(R)"
```

### 2. 파일 전송
```bash
scp -i idlerpg-key.pem .env.production ec2-user@13.209.66.253:/home/ec2-user/IdleRPGServer/
scp -i idlerpg-key.pem docker-compose.prod.yml ec2-user@13.209.66.253:/home/ec2-user/IdleRPGServer/
```

### 3. EC2에서 배포
```bash
# .env.production을 .env로 복사 (Docker Compose 변수 치환용)
cp .env.production .env

# Docker 재시작
docker-compose -f docker-compose.prod.yml down
docker-compose -f docker-compose.prod.yml up -d
```

## 검증 완료

### 1. 네트워크 연결 테스트
```bash
timeout 5 bash -c 'cat < /dev/null > /dev/tcp/idlerpg-dev...rds.amazonaws.com/5432' && echo "연결 성공!"
# 결과: 연결 성공!
```

### 2. API 기능 테스트
```bash
# Player 등록 - 성공
curl -X POST http://13.209.66.253:5172/api/auth/register

# Player 로그인 - 성공 (새 JWT Secret으로 토큰 생성)
curl -X POST http://13.209.66.253:5172/api/auth/login
# 응답: accessToken, refreshToken, playerId 정상 반환
```

## 보안 개선 요약

| 항목 | 변경 전 | 변경 후 |
|------|---------|---------|
| DB 비밀번호 | 짧고 단순 (Git 노출) | 64자 강력 (환경변수) |
| JWT Secret | 예측 가능 (Git 노출) | 64자 랜덤 (환경변수) |
| Redis 비밀번호 | 없음 | 64자 강력 (requirepass) |
| Redis 포트 | 외부 노출 가능 | 내부 네트워크만 (expose) |
| 시크릿 관리 | 하드코딩 | 환경변수 분리 |
| Git 보안 | 노출됨 | .gitignore 보호 |

## 회사/집 개발 환경

### 로컬 개발 (회사/집)
```bash
# 로컬 Docker PostgreSQL 사용
./dev-start.sh
dotnet run
# localhost:5432 연결
```

### RDS 접속 (회사)
**Rider Database Tools - SSH 터널링 사용**:
```
SSH Tunnel:
  Host: 13.209.66.253
  User: ec2-user
  Key: idlerpg-key.pem
  
PostgreSQL:
  Host: idlerpg-dev...rds.amazonaws.com
  Port: 5432
  User: postgres
  Password: AZ9ifIy...
```

가이드 문서: `docs/development/RIDER-RDS-CONNECTION.md`

## 관련 파일

- `docs/security/SECURITY-FIX-URGENT.md` - 보안 수정 가이드
- `docs/development/RIDER-RDS-CONNECTION.md` - Rider RDS 연결 가이드
- `.env.production` - 프로덕션 환경변수 (EC2에만 존재)
- `docker-compose.prod.yml` - 템플릿 파일 (환경변수 참조)

## 주요 학습 내용

1. **환경변수 분리**: 코드와 시크릿 분리의 중요성
2. **강력한 비밀번호**: 최소 64자 랜덤 생성
3. **Security Group**: AWS 네트워크 보안의 계층적 구조
4. **SSH 터널링**: Bastion Host 패턴
5. **Docker Compose 환경변수**: `.env` vs `env_file`의 차이
6. **.gitignore**: 시크릿 보호의 첫 번째 방어선

## 추가 권장사항 (미실행)

1. Git 히스토리 정리 (BFG Repo-Cleaner) - 선택사항
   - 이유: 새 비밀번호로 변경했으므로 이전 시크릿 무효화됨
2. AWS Secrets Manager 도입 - 향후 고려
3. 정기적 비밀번호 로테이션 정책 수립

## 결과

✅ **모든 보안 취약점 해결 완료**
✅ **API 정상 작동 확인**
✅ **개발 환경 가이드 문서화**
