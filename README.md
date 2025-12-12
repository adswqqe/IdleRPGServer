# Idle RPG Server

ASP.NET Core 8.0 기반 방치형 RPG 게임 서버 (학습 프로젝트)

## 프로젝트 소개

Unity 클라이언트와 연동되는 방치형 RPG 게임 서버입니다.
**Clean Architecture** 패턴을 학습하고 적용하기 위해 진행한 개인 프로젝트입니다.

## 기술 스택

- **Backend**: ASP.NET Core 8.0, C# 12
- **Database**: PostgreSQL 14, Entity Framework Core 9.0
- **Authentication**: JWT (Access Token + Refresh Token)
- **Real-time**: SignalR (채팅)
- **Cache**: Redis (PVP 랭킹)
- **DevOps**: Docker, Jenkins CI/CD, AWS EC2/RDS

## 프로젝트 구조

```
IdleRPGServer/
├── IdleRPG.API/              # Controllers, SignalR Hub
├── IdleRPG.Application/      # DTOs, Service Interfaces
├── IdleRPG.Domain/           # Entities, Repository Interfaces
├── IdleRPG.Infrastructure/   # Repository 구현, DbContext
└── IdleRPG.Tests/            # xUnit 테스트
```

**의존성 방향**: API → Application → Domain ← Infrastructure

## 구현된 기능

| 기능 | 설명 |
|------|------|
| 인증 | 회원가입, 로그인, JWT 토큰 갱신 |
| 캐릭터 | 생성, 조회, 레벨업, 스탯 관리 |
| 장비 | 착용/해제, 강화 (확률 기반) |
| 스킬 | 스킬 가챠, 장착 |
| 펫 | 펫 가챠, 육성, 장착 |
| 던전 | 스테이지 진행, 보상 획득 |
| PVP | 매칭, ELO 레이팅, 랭킹 (Redis) |
| 채팅 | 실시간 채팅 (SignalR) |

## 실행 방법

```bash
# 1. Clone
git clone https://github.com/adswqqe/IdleRPGServer.git

# 2. Database (Docker)
docker-compose up -d postgres

# 3. Migration
dotnet ef database update --project IdleRPG.Infrastructure --startup-project IdleRPG.API

# 4. Run
dotnet run --project IdleRPG.API
```

## API 문서

서버 실행 후: `https://localhost:7122/swagger`

## 학습 내용

- Clean Architecture 계층 분리
- Repository 패턴, DI(의존성 주입)
- EF Core Code-First Migration
- JWT 인증 흐름
- SignalR 실시간 통신
- Docker 컨테이너화 및 CI/CD
