$token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI3ZDdkYTE2OC04NTJkLTQ2N2EtYjdlYy00NzFiODc0ZmFlMTEiLCJqdGkiOiI1MjMzYTBhNi05MDcxLTRkZTctOTlmMS01YThkODVjYTQ1NjYiLCJpYXQiOiIxNzYwODAwNzQ5IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI3ZDdkYTE2OC04NTJkLTQ2N2EtYjdlYy00NzFiODc0ZmFlMTEiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiZHVuZ2VvbnRlc3QiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF5ZXIiLCJleHAiOjE3NjA4MDQzNDksImlzcyI6IklkbGVSUEdTZXJ2ZXIiLCJhdWQiOiJJZGxlUlBHQ2xpZW50In0.DK3dZZrOeKCOApOC5gKE_JzTjO8tYHGRvV-Xzmz-XPg"
$characterId = "fe2394c4-8c2b-4b89-8b53-4ebd4c024265"
$headers = @{ Authorization = "Bearer $token" }

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Dungeon System API Test (After Fix)" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# Test 1: GET /api/dungeons/stages
Write-Host "[Test 1] GET /api/dungeons/stages" -ForegroundColor Yellow
Write-Host "URL: http://13.209.66.253:5172/api/dungeons/stages?characterId=$characterId`n" -ForegroundColor Gray

try {
    $stages = Invoke-RestMethod -Uri "http://13.209.66.253:5172/api/dungeons/stages?characterId=$characterId" -Headers $headers
    Write-Host "✅ Success!" -ForegroundColor Green
    Write-Host "Total stages returned: $($stages.Count)" -ForegroundColor White

    if ($stages.Count -gt 0) {
        Write-Host "`nFirst 3 stages (Normal difficulty):" -ForegroundColor White
        $stages | Where-Object { $_.difficulty -eq 1 } | Select-Object -First 3 | ForEach-Object {
            Write-Host "  - Stage $($_.id): $($_.name) (Lv $($_.requiredLevel)) - Gold: $($_.finalGoldReward), Exp: $($_.finalExpReward)" -ForegroundColor Cyan
        }
    }
} catch {
    Write-Host "❌ Failed!" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n========================================`n" -ForegroundColor Gray

# Test 2: GET /api/dungeons/progress
Write-Host "[Test 2] GET /api/dungeons/progress" -ForegroundColor Yellow
Write-Host "URL: http://13.209.66.253:5172/api/dungeons/progress?characterId=$characterId`n" -ForegroundColor Gray

try {
    $progress = Invoke-RestMethod -Uri "http://13.209.66.253:5172/api/dungeons/progress?characterId=$characterId" -Headers $headers
    Write-Host "✅ Success!" -ForegroundColor Green
    Write-Host "Progress data:" -ForegroundColor White
    Write-Host "  - Normal: Stage $($progress.highestStageClearedNormal)" -ForegroundColor Cyan
    Write-Host "  - Hard: Stage $($progress.highestStageClearedHard)" -ForegroundColor Cyan
    Write-Host "  - Hell: Stage $($progress.highestStageClearedHell)" -ForegroundColor Cyan
} catch {
    Write-Host "❌ Failed!" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n========================================`n" -ForegroundColor Gray

# Test 3: POST /api/dungeons/clear (Stage 1, Normal)
Write-Host "[Test 3] POST /api/dungeons/clear" -ForegroundColor Yellow
Write-Host "Request: Stage 1, Normal difficulty`n" -ForegroundColor Gray

$clearRequest = @{
    characterId = $characterId
    stageId = 1
    difficulty = 1  # Normal
} | ConvertTo-Json

try {
    $clearResult = Invoke-RestMethod -Uri "http://13.209.66.253:5172/api/dungeons/clear" `
        -Method Post `
        -Headers (@{ Authorization = "Bearer $token"; "Content-Type" = "application/json" }) `
        -Body $clearRequest

    if ($clearResult.isSuccess) {
        Write-Host "✅ Clear Success!" -ForegroundColor Green
        Write-Host "Rewards:" -ForegroundColor White
        Write-Host "  - Gold: +$($clearResult.reward.gold)" -ForegroundColor Yellow
        Write-Host "  - Exp: +$($clearResult.reward.experience)" -ForegroundColor Yellow
        Write-Host "  - Level Up: $($clearResult.isLevelUp)" -ForegroundColor Cyan
        Write-Host "  - Current Level: $($clearResult.currentLevel)" -ForegroundColor Cyan
        Write-Host "  - New Highest Stage: $($clearResult.newHighestStage)" -ForegroundColor Magenta
    } else {
        Write-Host "⚠️  Clear Failed (Expected behavior if already cleared)" -ForegroundColor Yellow
        Write-Host "Error: $($clearResult.errorMessage)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "❌ Failed!" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "All Tests Completed!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
