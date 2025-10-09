# ======================================
# Stage 1: BUILD - 애플리케이션 빌드
# ======================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 프로젝트 파일 복사 (의존성 복원을 위해)
COPY ["IdleRPG.API/IdleRPG.API.csproj", "IdleRPG.API/"]
COPY ["IdleRPG.Application/IdleRPG.Application.csproj", "IdleRPG.Application/"]
COPY ["IdleRPG.Domain/IdleRPG.Domain.csproj", "IdleRPG.Domain/"]
COPY ["IdleRPG.Infrastructure/IdleRPG.Infrastructure.csproj", "IdleRPG.Infrastructure/"]

# NuGet 패키지 복원
RUN dotnet restore IdleRPG.API/IdleRPG.API.csproj 

# 전체 소스코드 복사
COPY . .

# 애플리케이션 빌드 및 배포 파일 생성
WORKDIR /src/IdleRPG.API
RUN dotnet publish --configuration Release --no-restore --output /app/publish 

# ======================================
# Stage 2: RUNTIME - 실행 환경
# ======================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# 빌드 단계에서 생성된 배포 파일 복사
COPY --from=build /app/publish .

# 컨테이너가 사용할 포트 노출
EXPOSE 5172 7122

# 컨테이너 시작 시 실행할 명령어
ENTRYPOINT ["dotnet", "IdleRPG.API.dll"]
