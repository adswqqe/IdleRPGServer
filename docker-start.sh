#!/bin/bash
# ======================================
# Docker Compose 전체 환경 시작
# ======================================

echo "🐳 Starting Idle RPG with Docker Compose..."
echo ""

# HTTPS 인증서 확인
if [ ! -f "$HOME/.aspnet/https/aspnetapp.pfx" ]; then
    echo "⚠️  HTTPS certificate not found!"
    echo "   Run setup script first:"
    echo "   - Windows: .\setup-https-cert.ps1"
    echo "   - Linux/Mac: ./setup-https-cert.sh"
    echo ""
    read -p "Continue without HTTPS? (y/n) " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        exit 1
    fi
fi

# 모든 서비스 시작 (API 포함)
docker-compose up -d

echo ""
echo "✅ All services started:"
echo "   📊 PostgreSQL: localhost:5432"
echo "   🔴 Redis: localhost:6379"
echo "   🌐 pgAdmin: http://localhost:8082"
echo "   🚀 API Swagger: http://localhost:5172/swagger"
echo "   🔒 API Swagger (HTTPS): https://localhost:7122/swagger"
echo ""
echo "📋 View logs: docker-compose logs -f api"
echo "🛑 Stop all: docker-compose down"
echo ""
