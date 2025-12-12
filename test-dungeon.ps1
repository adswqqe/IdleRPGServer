$token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI3ZDdkYTE2OC04NTJkLTQ2N2EtYjdlYy00NzFiODc0ZmFlMTEiLCJqdGkiOiI3YmE2OTZiNy01YzNjLTQ3MDYtYTEyNi03OWIxYzAwNmJhYTciLCJpYXQiOiIxNzYwNzk4OTA0IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI3ZDdkYTE2OC04NTJkLTQ2N2EtYjdlYy00NzFiODc0ZmFlMTEiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiZHVuZ2VvbnRlc3QiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF5ZXIiLCJleHAiOjE3NjA4MDI1MDQsImlzcyI6IklkbGVSUEdTZXJ2ZXIiLCJhdWQiOiJJZGxlUlBHQ2xpZW50In0.HGtk3Jl4FZlI5xYZPW8AQ_pwnOFv_-BjFX4_FDEKcV4"
$characterId = "fe2394c4-8c2b-4b89-8b53-4ebd4c024265"
$headers = @{ Authorization = "Bearer $token" }

Write-Host "=== Test 1: GET /api/dungeons/stages ===" -ForegroundColor Cyan
$stages = Invoke-RestMethod -Uri "http://13.209.66.253:5172/api/dungeons/stages?characterId=$characterId" -Headers $headers
Write-Host "Total stages: $($stages.Count)"
Write-Host "First 2 stages:" -ForegroundColor Yellow
$stages[0..1] | ConvertTo-Json -Depth 2

Write-Host "`n=== Test 2: GET /api/dungeons/progress ===" -ForegroundColor Cyan
$progress = Invoke-RestMethod -Uri "http://13.209.66.253:5172/api/dungeons/progress?characterId=$characterId" -Headers $headers
$progress | ConvertTo-Json -Depth 2

Write-Host "`n=== Test 3: POST /api/dungeons/clear (Stage 1, Normal) ===" -ForegroundColor Cyan
$clearRequest = @{
    characterId = $characterId
    stageId = 1
    difficulty = 1
} | ConvertTo-Json

$clearResult = Invoke-RestMethod -Uri "http://13.209.66.253:5172/api/dungeons/clear" `
    -Method Post `
    -Headers (@{ Authorization = "Bearer $token"; "Content-Type" = "application/json" }) `
    -Body $clearRequest

$clearResult | ConvertTo-Json -Depth 3

Write-Host "`n=== All tests completed ===" -ForegroundColor Green
