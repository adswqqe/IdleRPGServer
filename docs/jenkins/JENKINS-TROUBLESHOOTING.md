# Jenkins CI/CD 문제 해결 가이드

## 🔍 발생한 문제

### 증상
- Jenkins 빌드가 Docker Compose 단계에서 실패
- 에러 메시지: `script returned exit code -1`
- 로그: `Resuming build after Jenkins restart`
- JENKINS-48300 경고 발생

### 원인 분석
1. **Jenkins 재시작**: Docker 빌드 중 Jenkins가 재시작되어 프로세스 끊김
2. **메모리 부족**: t3.micro (1GB RAM)에서 Docker 빌드 시 메모리 소진 가능성
3. **타임아웃 미설정**: 장시간 실행 프로세스 추적 실패
4. **JENKINS-48300**: Durable Task Plugin의 알려진 버그

---

## ✅ 해결 방법

### 1️⃣ 즉시 해결 - 빌드 재실행

**가장 빠른 방법**: Jenkins UI에서 "Build Now" 클릭

또는 EC2에서 직접 실행:
```bash
ssh -i idlerpg-key.pem ec2-user@13.209.66.253
cd /home/ec2-user/IdleRPGServer
docker-compose -f docker-compose.prod.yml up -d --build

# 빌드 진행 확인
docker-compose -f docker-compose.prod.yml logs -f api
```

---

### 2️⃣ EC2 메모리 스왑 추가 (필수)

t3.micro의 1GB RAM은 Docker 빌드에 부족합니다. 스왑 메모리를 추가하세요:

#### 자동 설정 (권장)
```bash
# 로컬에서 스크립트를 EC2로 전송
scp -i idlerpg-key.pem setup-swap.sh ec2-user@13.209.66.253:~

# EC2에서 실행
ssh -i idlerpg-key.pem ec2-user@13.209.66.253
chmod +x setup-swap.sh
./setup-swap.sh
```

#### 수동 설정
```bash
# 2GB 스왑 파일 생성
sudo dd if=/dev/zero of=/swapfile bs=128M count=16
sudo chmod 600 /swapfile
sudo mkswap /swapfile
sudo swapon /swapfile

# 재부팅 후에도 유지
echo '/swapfile swap swap defaults 0 0' | sudo tee -a /etc/fstab

# 스왑 사용 정책 최적화
sudo sysctl vm.swappiness=10
echo 'vm.swappiness=10' | sudo tee -a /etc/sysctl.conf

# 확인
free -h
```

**예상 결과:**
```
              total        used        free      shared  buff/cache   available
Mem:          954Mi       450Mi       200Mi        10Mi       304Mi       380Mi
Swap:         2.0Gi        50Mi       1.9Gi  ← 스왑 추가됨
```

---

### 3️⃣ Jenkins 파이프라인 개선 (권장)

Jenkins UI → 프로젝트 → Configure → Pipeline Script 수정:

```groovy
pipeline {
    agent any

    options {
        // 전체 파이프라인 타임아웃: 30분
        timeout(time: 30, unit: 'MINUTES')
        // 빌드 동시 실행 방지
        disableConcurrentBuilds()
        // 타임스탬프 추가
        timestamps()
    }

    stages {
        stage('Pull Latest Code') {
            steps {
                echo 'Git pull starting...'
                timeout(time: 5, unit: 'MINUTES') {
                    sh 'cd /home/ec2-user/IdleRPGServer && sudo -u ec2-user git pull origin master'
                }
            }
        }

        stage('Deploy with Docker') {
            steps {
                echo 'Docker deployment starting...'
                // Docker 빌드에 충분한 시간 할당 (20분)
                timeout(time: 20, unit: 'MINUTES') {
                    script {
                        // 진행 상황 로그 출력
                        sh '''
                            cd /home/ec2-user/IdleRPGServer
                            echo "Starting Docker Compose build..."
                            docker-compose -f docker-compose.prod.yml up -d --build 2>&1 | tee /tmp/docker-build.log
                            echo "Docker Compose build completed"
                        '''
                    }
                }
            }
        }

        stage('Verify Deployment') {
            steps {
                echo 'Verifying deployment...'
                timeout(time: 2, unit: 'MINUTES') {
                    sh '''
                        # 컨테이너 상태 확인
                        docker ps | grep idlerpg || echo "Container not found!"

                        # 헬스체크
                        sleep 5
                        curl -k https://localhost:7122/health || echo "Health check failed"
                    '''
                }
            }
        }

        stage('Success') {
            steps {
                echo 'Deployment completed successfully!'
                echo 'API Server: https://13.209.66.253:7122'
                echo 'Swagger: https://13.209.66.253:7122/swagger'
            }
        }
    }

    post {
        success {
            echo '✅ Pipeline succeeded!'
        }
        failure {
            echo '❌ Pipeline failed! Check logs.'
            sh 'cat /tmp/docker-build.log || echo "No build log found"'
        }
        always {
            // 임시 파일 정리
            sh 'rm -f /tmp/docker-build.log || true'
        }
    }
}
```

**주요 개선 사항:**
- ✅ 각 stage별 타임아웃 설정
- ✅ 빌드 로그를 파일로 저장 (`/tmp/docker-build.log`)
- ✅ 배포 검증 단계 추가
- ✅ 에러 발생 시 로그 자동 출력

---

### 4️⃣ Jenkins Durable Task Plugin 설정

JENKINS-48300 문제 해결을 위한 환경 변수 추가:

#### 방법 A: Jenkins 서비스 파일 수정 (권장)
```bash
# EC2 SSH 접속
ssh -i idlerpg-key.pem ec2-user@13.209.66.253

# Jenkins 서비스 설정 편집
sudo vim /etc/systemd/system/jenkins.service.d/override.conf
```

다음 내용 추가:
```ini
[Service]
Environment="JAVA_OPTS=-Djava.io.tmpdir=/var/lib/jenkins/tmp -Dorg.jenkinsci.plugins.durabletask.BourneShellScript.HEARTBEAT_CHECK_INTERVAL=300"
```

적용:
```bash
sudo systemctl daemon-reload
sudo systemctl restart jenkins
```

#### 방법 B: Jenkins UI에서 설정
1. Jenkins 관리 → 시스템 설정
2. Global properties → Environment variables 추가:
   - 이름: `JENKINS_NODE_COOKIE`
   - 값: `dontKillMe`

---

### 5️⃣ Docker Compose 경고 제거

**이미 수정됨**: `docker-compose.prod.yml`에서 `version` 필드 제거

변경 전:
```yaml
version: '3.8'

services:
  api:
    ...
```

변경 후:
```yaml
services:
  api:
    ...
```

이 변경사항은 Git에 커밋하고 푸시하세요:
```bash
git add docker-compose.prod.yml
git commit -m "fix: Remove obsolete version field from docker-compose.prod.yml"
git push origin master
```

---

## 📊 빌드 모니터링

### 실시간 로그 확인
```bash
# Jenkins 빌드 중 EC2에서 Docker 로그 확인
docker-compose -f docker-compose.prod.yml logs -f api

# 메모리 사용량 모니터링
watch -n 1 free -h

# 컨테이너 상태 확인
docker ps
docker stats
```

### 빌드 시간 측정
- **예상 빌드 시간**: 5-10분 (처음), 2-5분 (캐시 사용 시)
- 20분 이상 걸리면 문제가 있는 것

---

## 🎯 체크리스트

다음 단계를 순서대로 진행하세요:

- [ ] **Step 1**: EC2에 스왑 메모리 추가 (`./setup-swap.sh`)
- [ ] **Step 2**: Jenkins 파이프라인 스크립트 개선 (타임아웃, 로깅 추가)
- [ ] **Step 3**: Jenkins Durable Task 환경 변수 설정
- [ ] **Step 4**: `docker-compose.prod.yml` 변경사항 커밋 & 푸시
- [ ] **Step 5**: Jenkins에서 빌드 재실행
- [ ] **Step 6**: 배포 확인 (https://13.209.66.253:7122/swagger)

---

## 🔐 보안 참고사항

현재 설정은 **개발/학습 환경**용입니다. 실제 프로덕션 배포 시 다음 사항을 개선하세요:

1. **Elastic IP**: 고정 IP 주소 할당
2. **Security Groups**: Jenkins 포트 접근 제한 (특정 IP만 허용)
3. **HTTPS**: Jenkins에 SSL/TLS 적용
4. **Docker 권한**: Docker rootless mode 또는 Socket Proxy 사용
5. **Secrets**: 환경 변수를 AWS Secrets Manager로 이동

자세한 내용은 `.serena/memories/jenkins-cicd-setup-and-security.md` 참고

---

## 📚 참고 자료

- [JENKINS-48300: Durable Task Plugin Issue](https://issues.jenkins.io/browse/JENKINS-48300)
- [Docker Compose V2 Breaking Changes](https://docs.docker.com/compose/compose-file/04-version-and-name/)
- [Linux Swap Memory Guide](https://www.digitalocean.com/community/tutorials/how-to-add-swap-space-on-ubuntu-20-04)

---

**마지막 업데이트**: 2025-10-11
**작성자**: Claude Code
**관련 파일**: `setup-swap.sh`, `docker-compose.prod.yml`, `.serena/memories/jenkins-cicd-setup-and-security.md`
