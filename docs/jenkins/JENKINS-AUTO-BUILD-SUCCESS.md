# Jenkins Auto-Build 설정 완료! 🎉

## ✅ 최종 설정 상태

### GitHub Webhook
- **URL**: `http://13.209.66.253:8080/generic-webhook-trigger/invoke?token=idlerpg-webhook-token`
- **Content type**: `application/json`
- **Events**: Just the push event
- **Status**: ✅ Active

### Jenkins 설정
- **Plugin**: Generic Webhook Trigger
- **Token**: `idlerpg-webhook-token`
- **Pipeline**: Pipeline script from SCM
- **Repository**: `git@github.com:adswqqe/IdleRPGServer.git`
- **Branch**: `*/master`
- **Credentials**: `github-ssh-key` (ED25519)

### EC2 Security Group
- **Port 8080 Inbound Rules**:
  - 나의 IP: `124.55.191.115/32`
  - GitHub IP 범위:
    - `192.30.252.0/22`
    - `185.199.108.0/22`
    - `140.82.112.0/20`
    - `143.55.64.0/20`

---

## 🚀 자동 빌드 흐름

```
1. 로컬에서 git push
   ↓
2. GitHub가 webhook 전송
   ↓
3. Jenkins Generic Webhook Trigger 감지
   ↓
4. Jenkins workspace에 Git clone
   ↓
5. Jenkinsfile 로드 및 실행
   ↓
6. EC2 /home/ec2-user/IdleRPGServer 디렉토리에서 git pull
   ↓
7. Docker Compose 빌드 및 배포
   ↓
8. 컨테이너 검증
   ↓
9. 배포 완료! ✅
```

**소요 시간**: 약 40초~1분

---

## 📊 빌드 로그 확인

### Jenkins UI
```
http://13.209.66.253:8080
프로젝트 클릭 → Build History → 최근 빌드 번호 클릭 → Console Output
```

**성공 시 표시:**
```
Started by Generic Cause
Obtained Jenkinsfile from git
...
Finished: SUCCESS
```

### GitHub Webhook
```
Repository → Settings → Webhooks → Recent Deliveries
```

**성공 시 Response:**
```
Status: 200 OK
Response: "Triggered builds: IdleRPG-Build"
```

---

## 🔧 문제 해결 히스토리

### 문제 1: Jenkins 메모리 부족
- **증상**: Docker 빌드 중 Jenkins 재시작, 빌드 실패
- **해결**: 2GB swap 메모리 추가 (`setup-swap.sh`)

### 문제 2: SSL 인증서 오류
- **증상**: `aspnetapp.pfx` 파일 없음
- **해결**: `docker-compose.prod.yml`에서 HTTPS 설정 제거, HTTP만 사용 (포트 5172)

### 문제 3: GitHub Webhook 연결 실패
- **증상**: "failed to connect to host"
- **해결**: Security Group에 GitHub IP 범위 추가

### 문제 4: Webhook 도착하지만 빌드 안 됨
- **증상**: "Received PushEvent", "Poked IdleRPG-Build" 로그는 보이지만 빌드 시작 안 함
- **원인**: "GitHub hook trigger for GITScm polling"은 polling 방식, 변경사항 감지 실패
- **해결**: Generic Webhook Trigger 플러그인 사용으로 직접 트리거

### 문제 5: EC2 git pull 충돌
- **증상**: "Your local changes would be overwritten by merge"
- **해결**: `git reset --hard HEAD && git clean -fd`

---

## 🎓 핵심 개념

### Pipeline Script vs Pipeline Script from SCM

**Pipeline Script:**
- Jenkinsfile 내용을 Jenkins UI에 직접 입력
- Git repository 정보가 없음
- Webhook이 와도 프로젝트를 매칭할 수 없음 ❌

**Pipeline Script from SCM:**
- Git repository에서 Jenkinsfile을 가져옴
- Repository URL이 명시적으로 설정됨
- Webhook과 프로젝트 매칭 가능 ✅

### GitHub Hook Trigger vs Generic Webhook Trigger

**GitHub hook trigger for GITScm polling:**
- Webhook → SCM polling 활성화 → 변경사항 감지 → 빌드
- Polling이 제대로 작동하지 않으면 빌드 시작 안 됨
- Private repository에서 문제 발생 가능 ⚠️

**Generic Webhook Trigger:**
- Webhook → 즉시 빌드 시작
- Token 기반 프로젝트 식별
- 확실하고 직접적인 트리거 방식 ✅

### SSH Key 인증

**EC2에서 Git 작업:**
- Username: `ec2-user` (Linux 사용자)
- Key: `~/.ssh/id_ed25519`

**Jenkins에서 GitHub 접근:**
- Username: `git` (GitHub SSH 프로토콜 규칙)
- Key: 동일한 `id_ed25519` private key
- Credentials에 `git` username으로 등록

---

## 📝 일상 작업 흐름

### 코드 수정 및 배포

```bash
# 1. 로컬에서 개발
cd E:\StudyGameProj\IdleRPGServer
# ... 코드 수정 ...

# 2. Commit & Push
git add .
git commit -m "feat: 새로운 기능 추가"
git push origin master

# 3. 자동으로 Jenkins 빌드 시작!
# Jenkins UI에서 진행 상황 확인 가능

# 4. 약 1분 후 배포 완료
# API Server: http://13.209.66.253:5172
# Swagger: http://13.209.66.253:5172/swagger
```

### 빌드 확인

```bash
# Jenkins UI
http://13.209.66.253:8080

# API 서버 확인
curl http://13.209.66.253:5172/health

# Swagger UI
http://13.209.66.253:5172/swagger
```

---

## 🔐 보안 참고사항

현재 설정은 **개발/학습 환경**용입니다. 실제 프로덕션 배포 시:

1. **Elastic IP**: 고정 IP 주소 할당
2. **Security Groups**: Jenkins 포트를 특정 IP만 접근 가능하도록 제한
3. **HTTPS**: Jenkins에 SSL/TLS 적용
4. **Webhook Secret**: GitHub webhook에 secret 추가
5. **Secrets Management**: 환경 변수를 AWS Secrets Manager로 이동

---

## 🎉 완료!

이제 GitHub에 코드를 push할 때마다 자동으로:
- ✅ Jenkins 빌드 시작
- ✅ 최신 코드 pull
- ✅ Docker 이미지 빌드
- ✅ 컨테이너 재배포
- ✅ EC2에서 실행

**더 이상 "Build Now" 클릭할 필요 없습니다!** 🚀

---

**작성일**: 2025년 10월 11일
**Jenkins Version**: 2.516.3
**EC2 Instance**: t3.micro (13.209.66.253)
**관련 파일**:
- `Jenkinsfile`
- `docker-compose.prod.yml`
- `JENKINS-TROUBLESHOOTING.md`
- `setup-swap.sh`
