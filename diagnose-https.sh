#!/bin/bash
# ======================================
# HTTPS 연결 문제 진단 스크립트
# ======================================

echo "🔍 Diagnosing HTTPS connection issues..."
echo ""

# 1. 포트 리스닝 확인
echo "1️⃣ Checking if ports are listening:"
echo "   HTTP (5172):"
netstat -tuln | grep 5172 || echo "   ❌ Port 5172 not listening"
echo ""
echo "   HTTPS (7122):"
netstat -tuln | grep 7122 || echo "   ❌ Port 7122 not listening"
echo ""

# 2. 방화벽 상태 확인
echo "2️⃣ Checking firewall status (UFW):"
if command -v ufw &> /dev/null; then
    sudo ufw status
else
    echo "   UFW not installed"
fi
echo ""

# 3. 보안 그룹 확인 안내
echo "3️⃣ AWS Security Group checklist:"
echo "   ⚠️  Make sure your EC2 Security Group has these inbound rules:"
echo "   - Type: Custom TCP"
echo "   - Port: 5172"
echo "   - Source: 0.0.0.0/0"
echo ""
echo "   - Type: Custom TCP"
echo "   - Port: 7122"
echo "   - Source: 0.0.0.0/0"
echo ""

# 4. 인증서 확인
echo "4️⃣ Checking HTTPS certificate:"
if [ -f ~/.aspnet/https/aspnetapp.pfx ]; then
    echo "   ✅ Certificate found at ~/.aspnet/https/aspnetapp.pfx"
else
    echo "   ❌ Certificate NOT found"
    echo "   ⚠️  Development certificates won't work on EC2!"
    echo "   You need a proper SSL certificate for production."
fi
echo ""

# 5. Docker 로그 확인
echo "5️⃣ Recent Docker logs from API container:"
docker logs idlerpg-api --tail 20 2>/dev/null || echo "   Container not running"
echo ""

# 6. 추천 사항
echo "📝 Recommendations for EC2 production:"
echo ""
echo "Option 1: Use Let's Encrypt (FREE, recommended)"
echo "   - Install Certbot"
echo "   - Get SSL certificate for your domain"
echo "   - Configure Nginx as reverse proxy"
echo ""
echo "Option 2: Disable HTTPS (development/testing only)"
echo "   - Remove HTTPS URL from appsettings"
echo "   - Remove UseHttpsRedirection() from Program.cs"
echo ""
echo "Option 3: Use AWS Certificate Manager + ALB"
echo "   - Create SSL certificate in ACM"
echo "   - Set up Application Load Balancer"
echo "   - ALB handles SSL termination"
echo ""
