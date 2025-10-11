# Jenkins CI/CD 설치 및 학습 (2025-01-10)

## 🎯 목표
- Jenkins를 활용한 IdleRPG 서버 자동 빌드 및 EC2 배포 시스템 구축
- 실무 CI/CD 경험 학습 (회사에서 Unity 빌드에 Jenkins 사용 중)

## ✅ 완료된 작업

### 1. Java & Jenkins 환경 구축
**로컬 PC (Windows):**
- Java JDK 17 설치 (winget)
- Jenkins LTS 설치 (MSI 인스톨러)
- 초기 설정 완료 (플러그인, 관리자 계정)
- 접속 URL: http://localhost:8080

**EC2 (Amazon Linux):**
- Java 17 설치 (Amazon Corretto)
- Jenkins 설치 (yum)
- 보안 그룹 설정 (8080 포트)
- **문제 발견**: t3.micro (1GB RAM) 부족 → Jenkins 실행 시 EC2 크래시
- **결정**: EC2 Jenkins 중지, 로컬 Jenkins로 전환

### 2. 로컬 Jenkins 빌드 자동화 성공
**Job: IdleRPG-Local-Build**
```groovy
pipeline {
    agent any
    stages {
        stage('Build') {
            steps {
                dir('E:\\StudyGameProj\\IdleRPGServer') {
                    bat 'dotnet build IdleRPGServer.sln --configuration Release'
                }
            }
        }
    }
}
```

**결과:**
- ✅ .NET 빌드 성공
- ✅ 빌드 시간: 약 2초
- ✅ Git clone 불필요 (로컬 경로 직접 접근)

### 3. EC2 배포 Pipeline 구축 (95% 완료)
**Job: IdleRPG-Deploy-to-EC2**

**목표 워크플로우:**
```
로컬 Jenkins 빌드 → SSH로 EC2 접속 → git pull → docker-compose up -d --build
```

**현재 상태:**
- ✅ 로컬 빌드 성공
- ✅ Pipeline 구조 완성
- ❌ SSH 연결 문제 (미해결)

**시도한 방법들:**
1. ❌ `sshagent` 플러그인 → Windows SSH Agent 서비스 실행 안 됨
2. ❌ OpenSSH (`ssh.exe`) → Connection timed out
3. ❌ PuTTY `plink.exe` → Connection timed out (Jenkins System 계정 권한 문제 추정)

**PuTTY GUI는 접속 가능, 커맨드라인 도구는 실패**

## 📊 아키텍처 결정

### EC2 Master vs 로컬 Master

**초기 계획:**
```
EC2 Jenkins Master + 로컬 Agent
→ EC2가 관리, 로컬 PC가 빌드 실행
```

**변경된 계획:**
```
로컬 Jenkins Master
→ 로컬에서 빌드 & 배포 명령 전송
→ EC2는 API 서버 전용
```

**이유:**
- EC2 t3.micro (1GB RAM) 부족: Jenkins Master + API 서버 동시 실행 불가
- t3.small (2GB, $7/월) 업그레이드 가능하지만 학습용으로는 과함
- 로컬 Jenkins가 빠르고 안정적 (16GB+ RAM, NVMe SSD)
- Unity 빌드도 로컬에서 수행 예정

### 장점
✅ 비용 $0 (EC2는 기존 t3.micro 유지)  
✅ 빌드 속도 빠름 (로컬 PC 성능)  
✅ Unity Personal 라이선스 사용 가능  
✅ 실무와 유사한 패턴 (빌드 서버 분리)  

### 단점
❌ 외부(회사)에서 Jenkins 접속 불가  
❌ 로컬 PC 꺼지면 Jenkins 중지  
→ 학습 목적으로는 문제없음

## 🐛 트러블슈팅 경험

### 1. EC2 리소스 부족
**증상:**
- Jenkins 빌드 시작 → EC2 먹통
- PuTTY SSH 접속 불가
- 웹 UI 접속 불가

**원인:**
```
t3.micro 총 1GB RAM
- Linux OS: 150-200MB
- Docker: 50-100MB
- API 서버: 100-200MB
- Jenkins: 500-900MB
- 빌드 시 스파이크: +200MB
→ 총 1200MB+ → OOM Killer 작동
```

**해결:**
- EC2 재부팅
- Jenkins 중지 및 자동 시작 비활성화
- 로컬 Jenkins로 전환

### 2. Windows bat 명령어 줄바꿈 문제
**증상:**
```groovy
bat """
    ssh -i "key.pem" user@host "command"
"""
```
→ 각 줄이 별도 명령어로 실행됨

**해결:**
- 모든 명령어를 한 줄로 작성
- 또는 PowerShell 사용

### 3. Git 보안 에러 (dubious ownership)
**증상:**
```
fatal: detected dubious ownership in repository
```

**원인:**
- Jenkins가 NT AUTHORITY/SYSTEM 계정으로 실행
- Git 저장소는 사용자 계정 소유

**해결:**
- Git 체크 단계 제거 (로컬에 이미 코드 있음)

### 4. SSH 연결 문제 (미해결)
**증상:**
- PuTTY GUI: 접속 O
- ssh.exe: Connection timed out
- plink.exe: Connection timed out (Jenkins에서)

**추정 원인:**
- Windows Defender 방화벽이 ssh.exe/plink.exe 차단
- Jenkins System 계정의 네트워크 권한 제약

**시도한 해결책:**
- PEM → PPK 변환
- SSH Agent 서비스 활성화 시도 (실패)
- plink.exe 사용 (여전히 timed out)

## 📝 Pipeline 코드

### 최종 작동 버전 (빌드만)
```groovy
pipeline {
    agent any
    stages {
        stage('Build') {
            steps {
                echo '.NET 빌드 시작...'
                dir('E:\\StudyGameProj\\IdleRPGServer') {
                    bat 'dotnet build IdleRPGServer.sln --configuration Release'
                }
            }
        }
        stage('Success') {
            steps {
                echo '빌드 성공! 🎉'
            }
        }
    }
    post {
        success {
            echo 'Pipeline 완료!'
        }
        failure {
            echo 'Pipeline 실패!'
        }
    }
}
```

### 배포 Pipeline (SSH 부분 미작동)
```groovy
pipeline {
    agent any
    stages {
        stage('Build') {
            steps {
                dir('E:\\StudyGameProj\\IdleRPGServer') {
                    bat 'dotnet build IdleRPGServer.sln --configuration Release'
                }
            }
        }
        stage('Deploy to EC2') {
            steps {
                bat '"C:\\Program Files\\PuTTY\\plink.exe" -i "E:\\StudyGameProj\\IdleRPGServer\\idlerpg-key.ppk" -batch ec2-user@13.125.206.100 "cd ~/IdleRPGServer && git pull origin master && docker-compose -f docker-compose.prod.yml up -d --build"'
            }
        }
    }
}
```

## 🎓 핵심 학습 내용

### Jenkins 아키텍처
**Master-Agent 패턴:**
- Master: 스케줄링, UI, 작업 관리
- Agent: 실제 빌드 실행
- 역할 분리로 리소스 최적화

**실무 적용:**
- 회사: Jenkins Master (서버) + Unity Agent (Windows PC)
- 학습: 로컬 Master (빌드+관리 통합)

### Pipeline as Code
- Jenkinsfile: 빌드 과정을 코드로 정의
- 버전 관리 가능 (Git)
- 단계별 실행 (stage)
- 조건부 실행 (post)

### Windows Jenkins 특성
- `bat` vs `powershell`: PowerShell이 더 안정적
- 줄바꿈 처리: bat는 각 줄을 개별 명령어로 인식
- System 계정: LocalSystem으로 실행, 권한 제약 있음
- SSH: OpenSSH 대신 PuTTY 도구 사용 권장

### 리소스 관리
- Jenkins Master 최소 요구사항: 2GB RAM
- t3.micro (1GB): 단독 사용 OK, 다른 서비스와 함께 X
- 로컬 빌드의 장점: 고성능, 무제한 리소스, Unity Personal OK

## 🔧 다음 단계 (미완료)

### SSH 연결 문제 해결 방법

**옵션 1: Jenkins 서비스 계정 변경**
- Windows Services → Jenkins
- 로그온 계정: LocalSystem → 현재 사용자 계정
- 장점: 사용자 권한으로 SSH 실행
- 단점: 비밀번호 관리 필요

**옵션 2: 수동 배포 스크립트**
```powershell
# deploy.ps1
dotnet build --configuration Release
ssh -i "key.ppk" ec2-user@13.125.206.100 "cd ~/IdleRPGServer && git pull && docker-compose up -d --build"
```
- Jenkins는 빌드만
- 배포는 수동 스크립트 실행

**옵션 3: GitHub Actions 활용**
```yaml
# .github/workflows/deploy.yml
on:
  push:
    branches: [master]
jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: appleboy/ssh-action@master
        with:
          host: 13.125.206.100
          username: ec2-user
          key: ${{ secrets.EC2_SSH_KEY }}
          script: |
            cd ~/IdleRPGServer
            git pull
            docker-compose up -d --build
```
- Git push → 자동 배포
- GitHub이 SSH 실행 (로컬 Jenkins 불필요)

## 📂 파일 위치

**Jenkins:**
- 설치 경로: `C:\Program Files\Jenkins`
- 작업 디렉토리: `C:\ProgramData\Jenkins\.jenkins`
- Workspace: `C:\ProgramData\Jenkins\.jenkins\workspace\<JobName>`

**SSH 키:**
- PPK 파일: `E:\StudyGameProj\IdleRPGServer\idlerpg-key.ppk` (PuTTY용)
- PEM 파일: `E:\StudyGameProj\IdleRPGServer\idlerpg-key.pem` (생성 완료)

**프로젝트:**
- 서버: `E:\StudyGameProj\IdleRPGServer`
- 클라이언트: `E:\StudyGameProj\IdleRPGClient`

## 🌟 실무 연결 포인트

### 회사 Jenkins와 비교
**공통점:**
- Windows 빌드 서버
- Unity 빌드 자동화
- Pipeline 기반

**차이점:**
- 회사: 전용 빌드 서버 (24시간 가동)
- 학습: 개인 PC (필요시만 실행)

### 배운 실무 스킬
1. **리소스 분석**: EC2 크래시 → 원인 분석 → 해결책 도출
2. **아키텍처 변경**: Master-Agent → 로컬 통합
3. **트러블슈팅**: Windows/Linux, 권한, 네트워크 문제 해결
4. **Pipeline 작성**: Groovy, bat/PowerShell, 단계별 실행

## 💡 중요한 깨달음

**"Jenkins는 설정 도구가 아니라 문제 해결 과정"**

- 초기 계획대로 안 됨 (EC2 리소스 부족)
- 여러 방법 시도 (SSH Agent, OpenSSH, PuTTY)
- 지속적인 최적화 필요

**실무에서도 동일:**
- 완벽한 설정은 없음
- 환경에 맞게 조정
- 시행착오를 통한 학습

## ⏰ 작업 시간
- 총 소요 시간: 약 4~5시간
- EC2 Jenkins: 1시간
- 로컬 Jenkins: 2시간
- 배포 Pipeline 시도: 1~2시간

## 📊 성과 평가

**목표 달성도: 80%**
- ✅ Jenkins 설치 및 이해
- ✅ 로컬 빌드 자동화
- ✅ Pipeline 코드 작성
- ✅ 실무 문제 해결 경험
- ⏳ EC2 자동 배포 (95% 완료, SSH만 남음)

**다음 세션 목표:**
- SSH 연결 문제 해결
- 또는 대안 방법 구현 (GitHub Actions)
- Unity 빌드 자동화 추가

---

**작업 일시**: 2025-01-10  
**다음 작업 시**: SSH 문제부터 재개 또는 대안 선택
