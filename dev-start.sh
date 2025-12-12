#!/bin/bash
# ======================================
# 개발 환경 시작 스크립트
# ======================================

echo "🚀 Starting Idle RPG Development Environment..."
echo ""

# DB와 인프라 서비스만 시작 (API는 로컬에서 실행)
docker-compose up -d postgres redis pgadmin

echo ""
echo "✅ Infrastructure services started:"
echo "   📊 PostgreSQL: localhost:5432"
echo "   🔴 Redis: localhost:6379"
echo "   🌐 pgAdmin: http://localhost:8082 (admin@idlerpg.com / admin123)"
echo ""
echo "🔧 To start API server locally:"
echo "   cd IdleRPG.API && dotnet run"
echo ""
echo "   HTTP:  http://localhost:5172/swagger"
echo "   HTTPS: https://localhost:7122/swagger"
echo ""
echo "🐳 To start everything with Docker (including API):"
echo "   ./docker-start.sh"
echo ""