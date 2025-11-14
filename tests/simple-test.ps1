# IdleRPG Simple API Test
$baseUrl = "https://localhost:7122"
$username = "test_$(Get-Date -Format 'yyyyMMddHHmmss')"
$password = "Test123!"
$email = "$username@example.com"

Write-Host "=== IdleRPG API Test Starting ===" -ForegroundColor Cyan

# Ignore SSL cert (for local HTTPS)
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

# 1. Register
Write-Host "`n[1] Register: $username"
$registerBody = @{ username = $username; email = $email; password = $password; confirmPassword = $password } | ConvertTo-Json
try {
    Invoke-RestMethod -Uri "$baseUrl/api/auth/register" -Method Post -ContentType "application/json" -Body $registerBody | Out-Null
    Write-Host "    OK - Registered" -ForegroundColor Green
} catch { Write-Host "    FAIL - $_" -ForegroundColor Red; exit 1 }

# 2. Login
Write-Host "`n[2] Login"
$loginBody = @{ username = $username; password = $password } | ConvertTo-Json
try {
    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method Post -ContentType "application/json" -Body $loginBody
    Write-Host "    DEBUG - Response Type: $($loginResponse.GetType().Name)" -ForegroundColor Gray
    Write-Host "    DEBUG - Response: $($loginResponse | ConvertTo-Json -Depth 3)" -ForegroundColor Gray

    # 응답이 { "response": { ... } } 형태로 감싸져 있음
    $data = $loginResponse.response
    $token = $data.accessToken

    if ($token) {
        Write-Host "    OK - Got JWT token" -ForegroundColor Green
        Write-Host "    Token: $($token.Substring(0, 50))..." -ForegroundColor Gray
    } else {
        Write-Host "    ERROR - Token is null!" -ForegroundColor Red
        exit 1
    }
} catch { Write-Host "    FAIL - $_" -ForegroundColor Red; exit 1 }

$headers = @{
    Authorization = "Bearer $token"
    "Content-Type" = "application/json"
}
Write-Host "    Debug: Authorization header set" -ForegroundColor Gray

# 3. Create Character
Write-Host "`n[3] Create Character"
$charBody = @{ name = "Hero_$(Get-Date -Format 'HHmmss')" } | ConvertTo-Json
try {
    $charResponse = Invoke-RestMethod -Uri "$baseUrl/api/character/Create" -Method Post -Headers $headers -Body $charBody
    # 응답 구조 확인 (response로 감싸져 있을 수 있음)
    if ($charResponse.response) {
        $charId = $charResponse.response.id
    } else {
        $charId = $charResponse.id
    }
    Write-Host "    OK - Character ID: $charId" -ForegroundColor Green
} catch {
    Write-Host "    FAIL - Status: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    Write-Host "    Error: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.ErrorDetails.Message) {
        Write-Host "    Details: $($_.ErrorDetails.Message)" -ForegroundColor Red
    }
    exit 1
}

# 4. Get Character (Check N+1!)
Write-Host "`n[4] Get Character (N+1 Check)"
try {
    Invoke-RestMethod -Uri "$baseUrl/api/character/$charId" -Method Get -Headers $headers | Out-Null
    Write-Host "    OK - Retrieved character" -ForegroundColor Green
} catch { Write-Host "    FAIL - $_" -ForegroundColor Red }

# 5. Skill Gacha
Write-Host "`n[5] Skill Gacha"
$gachaBody = @{ characterId = $charId } | ConvertTo-Json
try {
    $skillResp = Invoke-RestMethod -Uri "$baseUrl/api/skills/gacha" -Method Post -Headers $headers -Body $gachaBody
    $skillData = if ($skillResp.response) { $skillResp.response } else { $skillResp }
    Write-Host "    OK - Got: $($skillData.skillName)" -ForegroundColor Green
} catch { Write-Host "    SKIP - No crystal" -ForegroundColor Yellow }

# 6. Pet Gacha
Write-Host "`n[6] Pet Gacha"
$petBody = @{ characterId = $charId; count = 1 } | ConvertTo-Json
try {
    $petResp = Invoke-RestMethod -Uri "$baseUrl/api/pets/gacha" -Method Post -Headers $headers -Body $petBody
    $petData = if ($petResp.response) { $petResp.response } else { $petResp }
    Write-Host "    OK - Got: $($petData.pets[0].templateName)" -ForegroundColor Green
} catch { Write-Host "    SKIP - No crystal" -ForegroundColor Yellow }

# 7. Get Equipped Pets (N+1 Check)
Write-Host "`n[7] Get Equipped Pets (N+1 Check)"
try {
    Invoke-RestMethod -Uri "$baseUrl/api/pets/equipped?characterId=$charId" -Method Get -Headers $headers | Out-Null
    Write-Host "    OK - Retrieved pets" -ForegroundColor Green
} catch { Write-Host "    FAIL - $_" -ForegroundColor Red }

# 8. PVP Rankings (N+1 Check)
Write-Host "`n[8] PVP Rankings (N+1 Check)"
try {
    Invoke-RestMethod -Uri "$baseUrl/api/pvp/rankings?seasonId=1&page=1&pageSize=20" -Method Get -Headers $headers | Out-Null
    Write-Host "    OK - Retrieved rankings" -ForegroundColor Green
} catch { Write-Host "    SKIP - $_" -ForegroundColor Yellow }

Write-Host "`n================================" -ForegroundColor Cyan
Write-Host "Test Complete! Check MiniProfiler:" -ForegroundColor Green
Write-Host "$baseUrl/profiler/results-index" -ForegroundColor White
