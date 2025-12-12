# 🍄 Idle RPG Server

> **Clean Architecture 기반 방치형 RPG 게임 서버**
> ASP.NET Core 8.0 + Entity Framework Core + PostgreSQL

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-14-336791?logo=postgresql)](https://www.postgresql.org/)
[![Docker](https://img.shields.io/badge/Docker-24.0+-2496ED?logo=docker)](https://www.docker.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

---

## 📋 프로젝트 소개

Unity 클라이언트와 연동되는 방치형 RPG 게임의 백엔드 서버입니다.
**Clean Architecture** 패턴을 적용하여 계층 간 의존성을 명확히 분리하고, 테스트 가능한 구조로 설계했습니다.

### 주요 특징
- **Clean Architecture**: Domain → Application → Infrastructure → API 계층 분리
- **JWT 인증**: Access Token + Refresh Token 기반 보안 인증
- **EF Core Migrations**: Code-First 방식의 데이터베이스 스키마 관리
- **Docker 배포**: 컨테이너 기반 CI/CD 파이프라인 (Jenkins)

---

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                        API Layer                             │
│              (Controllers, Middleware, Hubs)                 │
├─────────────────────────────────────────────────────────────┤
│                    Application Layer                         │
│                 (Services, DTOs, Interfaces)                 │
├─────────────────────────────────────────────────────────────┤
│                      Domain Layer                            │
│            (Entities, ValueObjects, Enums)                   │
├─────────────────────────────────────────────────────────────┤
│                   Infrastructure Layer                       │
│          (Repositories, DbContext, Configurations)           │
└─────────────────────────────────────────────────────────────┘
```

**Dependency Flow**: `API → Application → Domain ← Infrastructure`

---

## 🛠️ Tech Stack

| Category | Technology |
|----------|------------|
| **Framework** | ASP.NET Core 8.0, C# 12 |
| **Database** | PostgreSQL 14, EF Core 9.0 |
| **Authentication** | JWT Bearer Token, BCrypt |
| **Real-time** | SignalR (채팅, 실시간 알림) |
| **Caching** | Redis (랭킹, 세션) |
| **Testing** | xUnit, Moq, FluentAssertions |
| **Logging** | Serilog (Structured Logging) |
| **Documentation** | Swagger/OpenAPI |
| **DevOps** | Docker, Jenkins, AWS EC2/RDS |

---

## 📁 Project Structure

```
IdleRPGServer/
├── IdleRPG.API/                 # API Layer
│   ├── Controllers/             # REST API 엔드포인트
│   ├── Middleware/              # 요청 파이프라인
│   └── Hubs/                    # SignalR Hubs
│
├── IdleRPG.Application/         # Application Layer
│   ├── DTOs/                    # 데이터 전송 객체
│   └── Services/                # 서비스 인터페이스
│
├── IdleRPG.Domain/              # Domain Layer
│   ├── Entities/                # 도메인 엔티티
│   ├── ValueObjects/            # DDD 값 객체
│   └── Repositories/            # 레포지토리 인터페이스
│
├── IdleRPG.Infrastructure/      # Infrastructure Layer
│   ├── Data/                    # DbContext
│   ├── Repositories/            # 레포지토리 구현
│   ├── Configurations/          # EF Core 설정
│   └── Migrations/              # 데이터베이스 마이그레이션
│
└── IdleRPG.Tests/               # 단위/통합 테스트
```

---

## 🎮 주요 기능

### 구현 완료
| 시스템 | 설명 | 주요 기술 |
|--------|------|-----------|
| **인증** | JWT 기반 회원가입/로그인 | BCrypt, Refresh Token |
| **캐릭터** | 다중 캐릭터 생성/관리, 레벨업 | EF Core Relationships |
| **장비** | 장비 착용/해제, 강화 시스템 | 확률 기반 로직, 트랜잭션 |
| **가챠** | 확률 뽑기, 천장 시스템 | 가중치 랜덤, 서버 검증 |
| **던전** | 스테이지 진행, 보상 시스템 | ValueObject 패턴 |
| **펫** | 펫 육성 및 장착 | N:M 관계 |
| **PVP** | ELO 레이팅 기반 매칭 | MMR 알고리즘 |
| **채팅** | 실시간 채팅 (전체/길드/귓속말) | SignalR Hub |

### API 예시
```
POST   /api/auth/register         # 회원가입
POST   /api/auth/login            # 로그인
GET    /api/character             # 캐릭터 목록
POST   /api/equipment/{id}/upgrade # 장비 강화
POST   /api/gacha/{bannerId}/pull  # 가챠 뽑기
GET    /api/arena/opponents       # PVP 상대 매칭
```

---

## 🚀 Getting Started

### Prerequisites
- .NET 8.0 SDK
- PostgreSQL 14+
- Docker (선택)

### Local Development

```bash
# 1. Clone
git clone https://github.com/your-username/IdleRPGServer.git
cd IdleRPGServer

# 2. Database Setup (Docker)
docker-compose up -d postgres

# 3. Apply Migrations
dotnet ef database update --project IdleRPG.Infrastructure --startup-project IdleRPG.API

# 4. Run
dotnet run --project IdleRPG.API
```

### Configuration
`appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=idlerpg;Username=postgres;Password=postgres"
  },
  "Jwt": {
    "Key": "your-secret-key-at-least-32-characters",
    "Issuer": "IdleRPG",
    "Audience": "IdleRPGClient"
  }
}
```

### API Documentation
서버 실행 후 Swagger UI: `https://localhost:7122/swagger`

---

## 🧪 Testing

```bash
# 전체 테스트 실행
dotnet test

# 특정 프로젝트 테스트
dotnet test IdleRPG.Tests
```

---

## 📊 Database Schema (주요 엔티티)

```
Player (1) ──── (N) Character
                    │
                    ├── (N) Equipment
                    ├── (N) CharacterSkill
                    └── (N) CharacterPet

Guild (1) ──── (N) GuildMember ──── (1) Player
```

---

## 🔒 Security Features

- **Password Hashing**: BCrypt with salt
- **JWT Validation**: 토큰 만료, 서명 검증
- **Server-side Validation**: 모든 게임 로직 서버에서 검증 (치팅 방지)
- **Rate Limiting**: API 호출 제한
- **Input Sanitization**: SQL Injection, XSS 방지

---

## 📝 License

This project is licensed under the MIT License.
