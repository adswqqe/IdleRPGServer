# AWS EC2 배포 완료! (2025-01-10)

## 🎉 성공적으로 완료된 작업

### 배포된 서버 정보

**접속 주소:**
- Swagger UI: http://13.125.206.100:5172/swagger
- API Base URL: http://13.125.206.100:5172

**서버 구성:**
- EC2: t3.micro (프리티어)
- Docker: API 서버 + Redis
- Database: RDS PostgreSQL (기존)

### 전체 워크플로우

1. ✅ Dockerfile 작성 (Multi-stage build)
2. ✅ docker-compose.prod.yml 작성
3. ✅ AWS Billing Alarm 설정 ($5 초과 시 알림)
4. ✅ EC2 t3.micro 인스턴스 생성
5. ✅ 보안 그룹 설정 (SSH, API 포트)
6. ✅ SSH 키 생성 및 접속
7. ✅ Docker, Git 설치
8. ✅ GitHub에서 코드 clone
9. ✅ Docker 이미지 빌드
10. ✅ 컨테이너 실행 및 검증

### 배포 프로세스 (수동)

```bash
# EC2 접속
ssh -i idlerpg-key.ppk ec2-user@13.125.206.100

# 최신 코드 받기
cd ~/IdleRPGServer
git pull origin master

# 이미지 빌드 (필요시 --no-cache)
docker-compose -f docker-compose.prod.yml build

# 컨테이너 재시작
docker-compose -f docker-compose.prod.yml down
docker-compose -f docker-compose.prod.yml up -d

# 상태 확인
docker-compose -f docker-compose.prod.yml ps
docker-compose -f docker-compose.prod.yml logs api --tail 50
```

### 학습한 개념들

1. **Docker Multi-stage Build**
   - SDK stage: 빌드 전용
   - Runtime stage: 실행 전용
   - 최종 이미지 크기 최소화

2. **Environment Variables**
   - appsettings.json 값을 환경변수로 덮어쓰기
   - `ConnectionStrings__DefaultConnection` 패턴

3. **Docker Compose**
   - 서비스 정의 (api, redis)
   - 네트워크 구성
   - 볼륨 관리

4. **AWS 보안**
   - Security Groups (방화벽)
   - Billing Alarms (비용 관리)
   - SSH 키 기반 인증

5. **배포 워크플로우**
   - Git → Docker Build → Container Deploy
   - 캐시 관리의 중요성 (--no-cache)

### 다음 단계 제안

1. **배포 자동화**
   - .ppk → .pem 변환
   - Bash 스크립트 완성
   - GitHub Actions CI/CD

2. **보안 강화**
   - .env 파일로 민감 정보 분리
   - Swagger 프로덕션에서 비활성화
   - HTTPS 인증서 설정 (Let's Encrypt)

3. **모니터링**
   - CloudWatch Logs 연동
   - 애플리케이션 로그 수집
   - 성능 모니터링

4. **Unity 클라이언트 연동**
   - API 문서 업데이트
   - 실제 서버 URL로 테스트

### 중요 파일들

- `Dockerfile`: 이미지 빌드 정의
- `docker-compose.prod.yml`: 프로덕션 설정
- `deploy.sh`: 배포 스크립트 (미완성)
- `idlerpg-key.ppk`: SSH 접속 키 (.gitignore 필수!)

### 비용 관리

**프리티어 사용 현황:**
- EC2 t3.micro: 750시간/월 (1대 24시간 가동 가능)
- RDS db.t2.micro: 750시간/월
- 데이터 전송: 15GB/월

**Billing Alarm:** $5 초과 시 adswqqe@gmail.com으로 알림

### 배운 교훈

1. Docker 캐시는 편리하지만, 때론 --no-cache 필요
2. 환경변수는 ASPNETCORE_ENVIRONMENT로 개발/프로덕션 분리
3. 보안 그룹 설정이 중요 (0.0.0.0/0 vs My IP)
4. Swagger는 개발용, 프로덕션에서는 비활성화 권장
5. Git 기반 배포는 버전 관리와 롤백에 유리
