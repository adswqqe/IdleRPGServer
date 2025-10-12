#!/bin/bash
# ======================================
# EC2 프로덕션 배포 스크립트
# ======================================

echo "🚀 Deploying IdleRPG Server to EC2..."
echo ""

# 1. 기존 컨테이너 중지
echo "1️⃣ Stopping existing containers..."
docker-compose -f docker-compose.production.yml down

# 2. 최신 코드 빌드
echo ""
echo "2️⃣ Building Docker image..."
docker-compose -f docker-compose.production.yml build --no-cache

# 3. 컨테이너 시작
echo ""
echo "3️⃣ Starting containers..."
docker-compose -f docker-compose.production.yml up -d

# 4. 헬스체크
echo ""
echo "4️⃣ Waiting for services to be healthy..."
sleep 10

# 5. 로그 확인
echo ""
echo "5️⃣ Checking API logs..."
docker logs idlerpg-api --tail 30

echo ""
echo "✅ Deployment completed!"
echo ""
echo "📋 Access information:"
echo "   🌐 API Swagger: http://YOUR_EC2_IP:5172/swagger"
echo "   📊 pgAdmin: http://YOUR_EC2_IP:8082"
echo ""
echo "📝 Useful commands:"
echo "   View logs: docker logs -f idlerpg-api"
echo "   Stop all: docker-compose -f docker-compose.production.yml down"
echo "   Restart API: docker restart idlerpg-api"
echo ""
echo "⚠️  Important: Make sure EC2 Security Group allows:"
echo "   - Port 5172 (HTTP API)"
echo "   - Port 8082 (pgAdmin)"
echo ""
