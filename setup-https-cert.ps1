# ======================================
# HTTPS 개발 인증서 생성 스크립트
# ======================================
# Docker에서 HTTPS를 사용하기 위한 개발 인증서 생성

Write-Host "🔐 ASP.NET Core HTTPS 개발 인증서 설정 중..." -ForegroundColor Cyan

# 기존 개발 인증서 제거 (선택사항)
Write-Host "기존 인증서 확인 중..." -ForegroundColor Yellow
dotnet dev-certs https --clean

# 새로운 개발 인증서 생성 및 신뢰
Write-Host "새 개발 인증서 생성 중..." -ForegroundColor Yellow
dotnet dev-certs https -ep $env:USERPROFILE\.aspnet\https\aspnetapp.pfx -p "dev123!"
dotnet dev-certs https --trust

Write-Host "✅ HTTPS 개발 인증서 생성 완료!" -ForegroundColor Green
Write-Host "   - 인증서 위치: $env:USERPROFILE\.aspnet\https\aspnetapp.pfx" -ForegroundColor Gray
Write-Host "   - 비밀번호: dev123!" -ForegroundColor Gray
Write-Host ""
Write-Host "이제 docker-compose를 사용하여 애플리케이션을 시작할 수 있습니다:" -ForegroundColor Green
Write-Host "   docker-compose up -d" -ForegroundColor White
