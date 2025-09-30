@echo off
echo GitHub Token 설정 스크립트
echo ====================================
echo.
echo 현재 환경의 GitHub Personal Access Token을 입력하세요.
echo (입력한 내용은 화면에 표시되지 않습니다)
echo.

set /p token="GitHub Token: "

if "%token%"=="" (
    echo.
    echo [오류] 토큰이 입력되지 않았습니다.
    pause
    exit /b 1
)

echo.
echo 환경 변수 설정 중...
setx GITHUB_TOKEN "%token%"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo [성공] GITHUB_TOKEN 환경 변수가 설정되었습니다.
    echo.
    echo 주의: 새로운 환경 변수를 적용하려면 현재 실행 중인
    echo Claude Code 세션을 종료하고 다시 시작해야 합니다.
) else (
    echo.
    echo [오류] 환경 변수 설정에 실패했습니다.
)

echo.
pause