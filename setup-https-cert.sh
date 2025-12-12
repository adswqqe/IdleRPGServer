#!/bin/bash
# ======================================
# HTTPS 개발 인증서 생성 스크립트 (Linux/Mac)
# ======================================

echo "🔐 Setting up ASP.NET Core HTTPS development certificate..."

# 기존 개발 인증서 제거
echo "Cleaning existing certificates..."
dotnet dev-certs https --clean

# 새로운 개발 인증서 생성
echo "Generating new development certificate..."
mkdir -p ~/.aspnet/https
dotnet dev-certs https -ep ~/.aspnet/https/aspnetapp.pfx -p "dev123!"
dotnet dev-certs https --trust

echo ""
echo "✅ HTTPS development certificate created successfully!"
echo "   - Certificate location: ~/.aspnet/https/aspnetapp.pfx"
echo "   - Password: dev123!"
echo ""
echo "You can now start the application with Docker:"
echo "   ./docker-start.sh"
