# Jenkins vs GitHub Actions 비교 및 CI/CD 전략 (2025-01-10)

## 논의 배경

**상황:**
- IdleRPG 서버 AWS EC2 배포 완료
- 자동화 필요성 대두
- **회사에서 Jenkins 사용 중** (Unity 빌드 → 검수 제출 자동화)
- 실무 학습 필요

**결론: Jenkins 학습 우선 추진**

## Jenkins vs GitHub Actions 상세 비교

### 1. 작동 방식

**GitHub Actions:**
- 클라우드 기반 (GitHub 제공 Runner)
- 매번 깨끗한 환경
- 설정 없이 바로 시작

**Jenkins:**
- 자체 서버 기반 (EC2 or 로컬)
- 환경 유지 (캐시 활용)
- 완전한 제어 가능

### 2. Unity 빌드 비교

**GitHub Actions의 문제:**
```
- Unity Personal License: CI 사용 불가 (약관 위반)
- Unity Plus/Pro 필요: 월 $40~185
- 매번 Unity 설치: 10~20분 소요
- 빌드 시간 제한: 무료 2,000분/월
```

**Jenkins의 장점:**
```
- Unity 한 번만 설치
- Personal License 사용 가능 (로컬)
- 빌드 캐시로 5~10분
- 무제한 빌드
```

### 3. .NET 서버 배포 비교

**GitHub Actions:**
```yaml
# 간단한 YAML 설정
- 설정 5분
- 무료
- GitHub Secrets로 키 관리
```

**Jenkins:**
```groovy
// Groovy 스크립트
- 설정 30분~1시간
- 복잡한 로직 가능
- 플러그인 풍부
```

### 4. 비용 비교

**GitHub Actions:**
```
Public Repo: 무료 무제한
Private Repo: 2,000분/월 무료
초과: $0.008/분

예상 사용:
.NET 배포: 3분 × 10회/일 = 900분/월 → 무료
Unity 빌드: 30분 × 3회/일 = 2,700분/월 → $5.6/월
```

**Jenkins:**
```
로컬 PC:
- 비용: $0 (전기세만)
- Unity Personal OK
- PC 24시간 가동 필요

EC2 t3.small:
- 월 $15~20
- Unity Plus/Pro 필요: +$40/월
- 총 $55~60/월
```

## 실무 Jenkins 파이프라인 예시

### 회사 워크플로우 (추정)

```
Git Push (release 브랜치)
    ↓
Jenkins 자동 감지
    ↓
Unity 멀티플랫폼 빌드
    ├─ Windows (Standalone)
    ├─ Android (APK/AAB)
    └─ iOS (IPA)
    ↓
자동 테스트 실행
    ├─ Unit Tests
    └─ Integration Tests
    ↓
빌드 아카이브 (버전별 저장)
    ↓
검수 제출 자동화
    ├─ Steam (SteamCMD)
    ├─ Google Play (fastlane)
    └─ App Store (fastlane)
    ↓
Slack/Discord 알림
```

### IdleRPG 적용 파이프라인

```groovy
pipeline {
    agent any
    
    stages {
        stage('Checkout') {
            steps {
                git 'https://github.com/adswqqe/IdleRPGServer.git'
            }
        }
        
        stage('Build .NET Server') {
            steps {
                bat 'dotnet build --configuration Release'
            }
        }
        
        stage('Test') {
            steps {
                bat 'dotnet test'
            }
        }
        
        stage('Deploy to EC2') {
            steps {
                sshagent(['ec2-credentials']) {
                    bat '''
                        ssh ec2-user@13.125.206.100 << EOF
                        cd ~/IdleRPGServer
                        git pull
                        docker-compose -f docker-compose.prod.yml up -d --build
                        EOF
                    '''
                }
            }
        }
        
        stage('Build Unity Client') {
            steps {
                bat '''
                    "C:\\Program Files\\Unity\\Hub\\Editor\\2022.3.x\\Editor\\Unity.exe" ^
                    -quit -batchmode -nographics ^
                    -projectPath "E:\\StudyGameProj\\IdleRPGClient" ^
                    -buildWindows64Player "Build\\IdleRPG.exe" ^
                    -logFile unity_build.log
                '''
            }
        }
    }
    
    post {
        success {
            echo 'Build and Deploy Success!'
        }
        failure {
            echo 'Build Failed!'
        }
    }
}
```

## 학습 로드맵

### Phase 1: Jenkins 기초 (1~2일)
- [ ] Java JDK 설치
- [ ] Jenkins 로컬 설치
- [ ] 첫 Job 생성 (.NET 빌드)
- [ ] Jenkinsfile 문법 학습

### Phase 2: .NET 자동 배포 (2~3일)
- [ ] EC2 SSH 연동
- [ ] 자동 배포 파이프라인
- [ ] Git 트리거 설정
- [ ] 실패 시 알림 설정

### Phase 3: Unity 빌드 자동화 (1주)
- [ ] Unity 커맨드라인 빌드
- [ ] Jenkins Unity 플러그인
- [ ] 멀티플랫폼 빌드
- [ ] 빌드 아카이브

### Phase 4: 고급 기능 (실무 수준)
- [ ] 병렬 빌드 (Parallel stages)
- [ ] 조건부 배포 (브랜치별)
- [ ] 검수 제출 자동화
- [ ] 슬랙 알림 연동

## 추천 학습 전략

### 개인 프로젝트 활용
```
1. IdleRPG 서버 자동 배포로 Jenkins 기초 학습
2. 간단한 Unity 빌드 자동화 추가
3. 회사 파이프라인 이해 향상
4. 포트폴리오에 CI/CD 경험 추가
```

### 회사 시스템 이해
```
1. Jenkins 기본 개념 습득
2. Groovy 문법 익히기
3. 플러그인 생태계 탐색
4. 회사 Jenkinsfile 분석 가능
```

## 실무 연결 포인트

### 학습 효과
1. ✅ 빌드 프로세스 이해
2. ✅ 파이프라인 트러블슈팅
3. ✅ 워크플로우 개선 제안 가능
4. ✅ 팀 생산성 향상 기여

### 포트폴리오 가치
```
"Jenkins를 활용한 CI/CD 파이프라인 구축"
- .NET 서버 자동 배포
- Unity 멀티플랫폼 빌드 자동화
- Docker 기반 배포 전략
- 실무 경험과 직접 연결
```

## 다음 액션 아이템

1. **즉시 시작 가능:** Jenkins 로컬 설치
2. **학습 목표:** .NET 자동 배포 파이프라인 완성
3. **예상 시간:** 2~3시간
4. **산출물:** Jenkinsfile + 동작하는 파이프라인

## 참고 자료

### Jenkins 공식 문서
- https://www.jenkins.io/doc/
- https://www.jenkins.io/doc/book/pipeline/

### Unity CI/CD
- https://docs.unity3d.com/Manual/CommandLineArguments.html
- Unity Build Automation 플러그인

### 실무 예시
- https://github.com/jenkinsci/pipeline-examples

## 결론

**회사에서 Jenkins 사용 → Jenkins 학습 필수**

GitHub Actions보다 설정이 복잡하지만:
- 실무 직결
- Unity 빌드에 최적
- 완전한 제어 가능
- 장기적으로 더 강력

**개인 프로젝트로 Jenkins 마스터하기!**
