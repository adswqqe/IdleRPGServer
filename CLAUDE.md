# CLAUDE.md

This file provides guidance to Claude Code when working with code in this repository.

## Language and Communication

**The user is Korean and all responses should be provided in Korean.** When explaining code, architecture, or providing guidance, translate technical explanations to Korean while keeping code examples and commands in their original form.

---

## Project Overview

### 🍄 버섯키우기 완전판 - Idle MMORPG

**AI-collaborative learning project** for mastering .NET backend development. Built with ASP.NET Core 8.0 following **Clean Architecture** principles.

- **Project Type**: Learning project (not commercial)
- **Goal**: Master 20+ modern .NET technologies in 8 weeks
- **Strategy**: T-shaped learning (6 core systems at 95% depth + 14 systems at 60-80% breadth)

**Roadmap & Scope**: See `docs/learning/PROJECT_ROADMAP.md` for full 8-week plan and current progress.

### Architecture Layers

- **IdleRPG.API**: Web API layer (Controllers, Middleware, SignalR Hubs)
- **IdleRPG.Application**: Business logic (Services, Commands/Queries, DTOs) - MediatR, AutoMapper, FluentValidation
- **IdleRPG.Domain**: Core domain entities and business rules (20+ entity types)
- **IdleRPG.Infrastructure**: Data access and external services - EF Core + PostgreSQL

**Dependency Flow**: API → Application → Domain, Infrastructure implements Application interfaces.

### Game Concept

**Genre**: Idle MMORPG (Auto-battle, Offline rewards, Social features)

**Core Loop**: Enter Dungeon → Auto Battle → Earn Rewards → Upgrade → PVP → Guild → Boss Raid

**20 Major Systems**: Authentication, Character, Equipment, Combat, Offline Rewards, Dungeon, Enhancement, Skills, Pets, PVP, Friends, Guild, Chat (SignalR), Boss Raid, Quest, Daily Mission, Gacha, Shop, Ranking (Redis), Mail/Events

---

## Development Environment

### Quick Start

```bash
# Local development (recommended)
./dev-start.sh              # Starts PostgreSQL, Redis, pgAdmin
cd IdleRPG.API
dotnet run                  # Run with hot reload

# Full Docker environment
./docker-start.sh           # Starts everything including API
```

**Access Points**:
- Swagger: http://localhost:5172/swagger (HTTP) or https://localhost:7122/swagger (HTTPS)
- pgAdmin: http://localhost:8082 (admin@idlerpg.com / admin123)
- PostgreSQL: localhost:5432 (gamedev / dev123!)

**Detailed Setup**: See `docs/development/DEV_ENVIRONMENT_SETUP.md`

### Building and Testing

```bash
dotnet build IdleRPGServer.sln      # Build solution
dotnet test                          # Run all tests
```

---

## Database Migrations

### Creating Migrations

```bash
cd IdleRPG.Infrastructure
dotnet ef migrations add <MigrationName> --startup-project ../IdleRPG.API
```

### ⚠️ CRITICAL: Production Deployment

**Jenkins CI/CD handles migrations automatically**.

**NEVER** run `dotnet ef database update` on release branches.

**Workflow**:
1. Create migration locally (command above)
2. Commit and push to GitHub
3. Jenkins automatically applies `migration.sql` to RDS

**Details**: See `docs/jenkins/DEPLOYMENT_GUIDE.md`

---

## Key Technologies

- **Framework**: ASP.NET Core 8.0 Web API
- **Database**: PostgreSQL with Entity Framework Core 9.0
- **Cache**: Redis (Distributed locking, Ranking system)
- **Real-time**: SignalR (Chat system)
- **Patterns**: CQRS with MediatR, Repository pattern
- **Validation**: FluentValidation
- **Mapping**: AutoMapper
- **Logging**: Serilog
- **Testing**: xUnit, Moq, FluentAssertions
- **Background**: IHostedService

---

## Collaboration Rules

### 🤖 Claude 자동 처리 작업

Claude가 독립적으로 완료:
- CRUD 메서드, Repository 패턴
- DTO 생성, Configuration
- EF Core 마이그레이션
- Unity API 문서, Swagger 주석
- 단위 테스트 작성

### 👥 함께 협업하는 작업

설계/구현 전 논의:
- 데이터 설계 (엔티티 관계, 인덱스)
- 비즈니스 로직 (전투 공식, 보상 계산)
- API 설계 (요청/응답, 에러 핸들링)
- 아키텍처 설계 (계층 분리, 의존성)
- 성능 최적화 (쿼리, 캐싱, 동시성)

### 협업 프로세스

1. **설계**: Claude 초안 제시 → 사용자 피드백 → 최종 결정
2. **구현**: 핵심 로직은 TODO(human), 반복 코드는 자동 완성
3. **검토**: 구현 후 주요 변경사항 요약

---

## Development Guidelines

### Adding New Features

1. Create domain entities in `IdleRPG.Domain`
2. Define interfaces and DTOs in `IdleRPG.Application`
3. Implement repositories in `IdleRPG.Infrastructure`
4. Create API controllers in `IdleRPG.API`
5. Use MediatR for CQRS
6. Apply FluentValidation
7. Map with AutoMapper
8. Write unit tests in `IdleRPG.Tests`

### Testing Guidelines

- Use **xUnit**, **Moq**, **FluentAssertions**
- Follow **AAA pattern** (Arrange-Act-Assert)
- Mock repository dependencies
- Test edge cases: null inputs, boundaries, errors
- Run before committing: `dotnet test`

### Authentication

- JWT Bearer tokens (Access: 15 min, Refresh: 7 days)
- BCrypt password hashing
- Game endpoints require `[Authorize]` attribute

### Game Balance Philosophy

- Server-side validation for critical operations
- Client is presentation only (UI, animations)
- All economy logic runs on server
- Server-authoritative design prevents cheating

---

## Unity Client Documentation

### Policy

**CRITICAL**: When adding/modifying APIs or DTOs, **ALWAYS** update Unity documentation.

### Documentation Structure (v1.7+)

Unity 문서는 **기능별 폴더 구조**로 관리됩니다:

```
../IdleRPGClient/Docs/unity/
├── README.md                    # 메인 인덱스 (구현 상태 테이블)
├── auth/
│   ├── API_SPEC.md
│   └── DTOs.cs
├── character/
│   ├── API_SPEC.md
│   └── DTOs.cs
├── {feature}/                   # 새 기능 추가 시
│   ├── API_SPEC.md              # API 명세서
│   └── DTOs.cs                  # C# DTO 클래스
```

### Standard Checklist (새 기능 추가 시)

1. **폴더 생성**: `../IdleRPGClient/Docs/unity/{feature}/`
2. **API 명세 작성**: `{feature}/API_SPEC.md`
   - 엔드포인트별 Request/Response 예제
   - Unity C# 코드 예제 포함
3. **DTO 작성**: `{feature}/DTOs.cs`
   - JsonProperty 어트리뷰트 사용
   - Newtonsoft.Json 기준
4. **메인 인덱스 업데이트**: `unity/README.md`
   - 구현 상태 테이블에 엔드포인트 추가
   - 빠른 시작 가이드 업데이트 (필요시)
   - 버전 히스토리 추가

### File Naming Convention

- API 명세: `API_SPEC.md` (표준) 또는 `{Feature}-API.md`
- DTO 클래스: `DTOs.cs` (표준) 또는 `{Feature}DTO.cs`

**Detailed Guide**: See `docs/unity/UNITY_DOCUMENTATION_GUIDE.md`

---

## Reference Documentation

- **Project Roadmap**: `docs/learning/PROJECT_ROADMAP.md` - 8-week learning plan, current status
- **Environment Setup**: `docs/development/DEV_ENVIRONMENT_SETUP.md` - Docker, HTTPS, local dev
- **Deployment Guide**: `docs/jenkins/DEPLOYMENT_GUIDE.md` - CI/CD pipeline, migrations
- **Unity Documentation**: `docs/unity/UNITY_DOCUMENTATION_GUIDE.md` - API docs update rules
- **Complete PRD**: `docs/MUSHROOM_GAME_PRD.md` - 20-system specification

---