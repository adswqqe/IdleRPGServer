#!/bin/bash

# ======================================
# IdleRPG Server 배포 스크립트
# ======================================

set -e  # 에러 발생 시 스크립트 중단

# 설정
EC2_HOST="13.125.206.100"
EC2_USER="ec2-user"
SSH_KEY="idlerpg-key.ppk"  # TODO(human): SSH 키 파일 경로 수정 필요
REPO_URL="https://github.com/adswqqe/IdleRPGServer.git"
PROJECT_DIR="IdleRPGServer"

echo "🚀 IdleRPG Server 배포 시작..."

# TODO(human): SSH를 통해 EC2에서 실행할 명령어들
# 힌트 1: ssh -i $SSH_KEY $EC2_USER@$EC2_HOST "명령어들"
# 힌트 2: 여러 명령어를 실행하려면 << EOF ... EOF 사용
# 힌트 3: 필요한 명령어들:
#   - cd ~ (홈 디렉토리로 이동)
#   - git clone 또는 git pull (저장소 복제/업데이트)
#   - cd $PROJECT_DIR (프로젝트 디렉토리 이동)
#   - docker-compose -f docker-compose.prod.yml down (기존 컨테이너 중지)
#   - docker-compose -f docker-compose.prod.yml build (이미지 빌드)
#   - docker-compose -f docker-compose.prod.yml up -d (백그라운드 실행)

ssh -i "$SSH_KEY" "$EC2_USER@$EC2_HOST" << 'EOF'
# 여기에 EC2에서 실행할 명령어들 작성


EOF

echo "✅ 배포 완료!"
echo "🌐 API 서버: http://$EC2_HOST:5172"
echo "📊 Swagger: http://$EC2_HOST:5172/swagger"
