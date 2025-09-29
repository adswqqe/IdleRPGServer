# 필수 개발 명령어 가이드

## 개발 환경 시작/정지

### 개발 환경 시작
```bash
# PostgreSQL, Redis, pgAdmin 컨테이너 시작
./dev-start.sh
```

### 개발 환경 정지
```bash
# 모든 서비스 정지
docker-compose down
```

## 애플리케이션 실행

### API 서버 실행
```bash
cd IdleRPG.API
dotnet run
# 실행 후 접속 URL:
# - HTTP: http://localhost:5172
# - HTTPS: https://localhost:7122
# - Swagger: http://localhost:5172/swagger
```

## 빌드 명령어

### 전체 솔루션 빌드
```bash
dotnet build IdleRPGServer.sln
```

### 특정 프로젝트 빌드
```bash
dotnet build IdleRPG.API/IdleRPG.API.csproj
dotnet build IdleRPG.Application/IdleRPG.Application.csproj
dotnet build IdleRPG.Domain/IdleRPG.Domain.csproj
dotnet build IdleRPG.Infrastructure/IdleRPG.Infrastructure.csproj
```

## 데이터베이스 마이그레이션

### 새 마이그레이션 생성
```bash
cd IdleRPG.Infrastructure
dotnet ef migrations add <마이그레이션명> --startup-project ../IdleRPG.API
```

### 데이터베이스 업데이트
```bash
cd IdleRPG.Infrastructure
dotnet ef database update --startup-project ../IdleRPG.API
```

## 테스트 (추후 추가 시)
```bash
dotnet test
```

## Windows 시스템 유틸리티
- `dir`: 디렉토리 목록 보기 (ls 대신)
- `cd`: 디렉토리 이동
- `type`: 파일 내용 보기 (cat 대신)
- `findstr`: 텍스트 검색 (grep 대신)
- `where`: 파일 찾기 (find 대신)

## Docker 관련 명령어
```bash
# 컨테이너 상태 확인
docker ps

# 로그 확인
docker-compose logs postgres
docker-compose logs pgadmin

# 컨테이너 재시작
docker-compose restart postgres
```

## 개발 도구 접속 정보
- **API 문서**: http://localhost:5172/swagger
- **pgAdmin**: http://localhost:8082 (admin@idlerpg.com / admin123)
- **PostgreSQL**: localhost:5432 (gamedev / dev123!)
- **Redis**: localhost:6379