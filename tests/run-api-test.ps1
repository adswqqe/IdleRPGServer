# IdleRPG API 자동화 테스트 스크립트
# 사용법: .\tests\run-api-test.ps1

$baseUrl = "https://localhost:7122"
$username = "testuser_$(Get-Date -Format 'yyyyMMddHHmmss')"
$password = "Test123!"
$email = "test_$(Get-Date -Format 'yyyyMMddHHmmss')@example.com"

Write-Host "=== IdleRPG API Automated Test ===" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan

# SSL 인증서 검증 우회 (로컬 개발용)
if (-not ([System.Management.Automation.PSTypeName]'ServerCertificateValidationCallback').Type) {
    Add-Type @"
        using System.Net;
        using System.Security.Cryptography.X509Certificates;
        public class ServerCertificateValidationCallback {
            public static void Ignore() {
                ServicePointManager.ServerCertificateValidationCallback +=
                    delegate { return true; };
            }
        }
"@
}
[ServerCertificateValidationCallback]::Ignore()

# 1. 회원가입
Write-Host "`n1️⃣  회원가입..." -ForegroundColor Yellow
$registerBody = @{
    username = $username
    email = $email
    password = $password
    confirmPassword = $password
} | ConvertTo-Json

try {
    $registerResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/register" `
        -Method Post `
        -ContentType "application/json" `
        -Body $registerBody
    Write-Host "   ✅ 회원가입 성공: $username" -ForegroundColor Green
} catch {
    Write-Host "   ❌ 회원가입 실패: $_" -ForegroundColor Red
    exit 1
}

# 2. 로그인
Write-Host "`n2️⃣  로그인..." -ForegroundColor Yellow
$loginBody = @{
    username = $username
    password = $password
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" `
        -Method Post `
        -ContentType "application/json" `
        -Body $loginBody
    $token = $loginResponse.accessToken
    Write-Host "   ✅ 로그인 성공! JWT 토큰 획득" -ForegroundColor Green
} catch {
    Write-Host "   ❌ 로그인 실패: $_" -ForegroundColor Red
    exit 1
}

# 헤더 설정
$headers = @{
    Authorization = "Bearer $token"
    "Content-Type" = "application/json"
}

# 3. 캐릭터 생성
Write-Host "`n3. 캐릭터 생성..." -ForegroundColor Yellow
$characterBody = @{
    name = "Hero_$(Get-Date -Format 'HHmmss')"
} | ConvertTo-Json

try {
    $characterResponse = Invoke-RestMethod -Uri "$baseUrl/api/character/Create" `
        -Method Post `
        -Headers $headers `
        -Body $characterBody
    $characterId = $characterResponse.id
    Write-Host "   OK 캐릭터 생성 성공! ID: $characterId" -ForegroundColor Green
} catch {
    Write-Host "   ERR 캐릭터 생성 실패: $_" -ForegroundColor Red
    exit 1
}

# 4. 캐릭터 조회 (N+1 확인!)
Write-Host "`n4️⃣  캐릭터 조회 (N+1 확인)..." -ForegroundColor Yellow
try {
    $getCharacter = Invoke-RestMethod -Uri "$baseUrl/api/characters/$characterId" `
        -Method Get `
        -Headers $headers
    Write-Host "   ✅ 캐릭터 조회 성공!" -ForegroundColor Green
    Write-Host "   📊 MiniProfiler: $baseUrl/profiler/results-index" -ForegroundColor Magenta
} catch {
    Write-Host "   ❌ 캐릭터 조회 실패: $_" -ForegroundColor Red
}

# 5. 스킬 가챠
Write-Host "`n5️⃣  스킬 가챠..." -ForegroundColor Yellow
$skillGachaBody = @{
    characterId = $characterId
} | ConvertTo-Json

try {
    $skillResponse = Invoke-RestMethod -Uri "$baseUrl/api/skills/gacha" `
        -Method Post `
        -Headers $headers `
        -Body $skillGachaBody
    Write-Host "   ✅ 스킬 획득: $($skillResponse.skillName)" -ForegroundColor Green
} catch {
    Write-Host "   ⚠️  스킬 가챠 실패 (크리스탈 부족?): $_" -ForegroundColor Yellow
}

# 6. 펫 가챠
Write-Host "`n6️⃣  펫 가챠..." -ForegroundColor Yellow
$petGachaBody = @{
    characterId = $characterId
    count = 1
} | ConvertTo-Json

try {
    $petResponse = Invoke-RestMethod -Uri "$baseUrl/api/pets/gacha" `
        -Method Post `
        -Headers $headers `
        -Body $petGachaBody
    $petId = $petResponse.pets[0].id
    Write-Host "   ✅ 펫 획득: $($petResponse.pets[0].templateName)" -ForegroundColor Green
} catch {
    Write-Host "   ⚠️  펫 가챠 실패 (크리스탈 부족?): $_" -ForegroundColor Yellow
}

# 7. 장착된 펫 조회 (N+1 확인!)
Write-Host "`n7️⃣  장착된 펫 조회 (N+1 확인)..." -ForegroundColor Yellow
try {
    $equippedPets = Invoke-RestMethod -Uri "$baseUrl/api/pets/equipped/$characterId" `
        -Method Get `
        -Headers $headers
    Write-Host "   ✅ 펫 조회 성공!" -ForegroundColor Green
    Write-Host "   📊 MiniProfiler에서 쿼리 개수 확인!" -ForegroundColor Magenta
} catch {
    Write-Host "   ❌ 펫 조회 실패: $_" -ForegroundColor Red
}

# 8. PVP 랭킹 조회 (N+1 확인!)
Write-Host "`n8️⃣  PVP 랭킹 조회 (N+1 확인)..." -ForegroundColor Yellow
try {
    $pvpRankings = Invoke-RestMethod -Uri "$baseUrl/api/pvp/rankings?seasonId=1&page=1&pageSize=20" `
        -Method Get `
        -Headers $headers
    Write-Host "   ✅ 랭킹 조회 성공!" -ForegroundColor Green
    Write-Host "   📊 MiniProfiler에서 쿼리 개수 확인!" -ForegroundColor Magenta
} catch {
    Write-Host "   ⚠️  랭킹 조회 실패: $_" -ForegroundColor Yellow
}

# 완료
Write-Host "`n================================" -ForegroundColor Cyan
Write-Host "✅ 테스트 완료!" -ForegroundColor Green
Write-Host "`n📊 MiniProfiler 결과 확인:" -ForegroundColor Cyan
Write-Host "   $baseUrl/profiler/results-index" -ForegroundColor White
Write-Host "`n👉 각 요청의 SQL 쿼리 개수를 확인하세요!" -ForegroundColor Yellow
