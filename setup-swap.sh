#!/bin/bash

# ======================================
# EC2 스왑 메모리 설정 스크립트
# t3.micro (1GB RAM) 환경에서 Docker 빌드 안정성 향상
# ======================================

set -e

echo "🔧 스왑 메모리 설정 시작..."

# 현재 메모리 상태 확인
echo "📊 현재 메모리 상태:"
free -h

# 기존 스왑 파일이 있는지 확인
if [ -f /swapfile ]; then
    echo "⚠️  기존 스왑 파일이 존재합니다. 제거합니다..."
    sudo swapoff /swapfile 2>/dev/null || true
    sudo rm -f /swapfile
fi

# 2GB 스왑 파일 생성 (128MB × 16 = 2GB)
echo "📦 2GB 스왑 파일 생성 중... (약 30초 소요)"
sudo dd if=/dev/zero of=/swapfile bs=128M count=16 status=progress

# 파일 권한 설정 (보안상 중요)
echo "🔒 스왑 파일 권한 설정..."
sudo chmod 600 /swapfile

# 스왑 영역 생성
echo "⚙️  스왑 영역 생성..."
sudo mkswap /swapfile

# 스왑 활성화
echo "✅ 스왑 활성화..."
sudo swapon /swapfile

# 재부팅 후에도 스왑 유지하도록 fstab 설정
if ! grep -q '/swapfile' /etc/fstab; then
    echo "💾 재부팅 후에도 스왑 유지하도록 설정..."
    echo '/swapfile swap swap defaults 0 0' | sudo tee -a /etc/fstab
else
    echo "ℹ️  /etc/fstab에 이미 스왑 설정이 존재합니다."
fi

# 스왑 사용 정책 최적화 (swappiness 설정)
echo "⚡ 스왑 사용 정책 최적화..."
sudo sysctl vm.swappiness=10  # RAM을 우선 사용, 메모리 부족 시에만 스왑 사용

if ! grep -q 'vm.swappiness' /etc/sysctl.conf; then
    echo 'vm.swappiness=10' | sudo tee -a /etc/sysctl.conf
fi

# 최종 메모리 상태 확인
echo ""
echo "🎉 스왑 메모리 설정 완료!"
echo "📊 최종 메모리 상태:"
free -h
echo ""
echo "✨ 이제 Docker 빌드 시 메모리 부족 문제가 크게 줄어듭니다."
echo "💡 스왑 사용량 모니터링: watch -n 1 free -h"
