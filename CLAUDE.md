# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Language and Communication

**The user is Korean and all responses should be provided in Korean.** When explaining code, architecture, or providing guidance, translate technical explanations to Korean while keeping code examples and commands in their original form.

## Project Overview

### 🍄 버섯키우기 완전판 - Idle MMORPG

This is a **commercial-scale Idle MMORPG server** project inspired by "버섯키우기" (Mushroom Cultivation Game). Built with ASP.NET Core 8.0 following **Clean Architecture** principles.

**Primary Goal**: Master .NET Web API + Unity Network Communication
**Secondary Goal**: Build production-ready idle RPG with 20+ game systems
**Timeline**: 20 weeks (5 months) development roadmap
**Scale**: 20 major systems from basic character growth to advanced guild raids

### Architecture Layers

- **IdleRPG.API**: Web API layer (Controllers, Middleware, Configuration, SignalR Hubs)
- **IdleRPG.Application**: Business logic layer (Services, Commands/Queries, DTOs) - uses MediatR, AutoMapper, FluentValidation
- **IdleRPG.Domain**: Core domain entities and business rules (20+ entity types)
- **IdleRPG.Infrastructure**: Data access and external services - uses Entity Framework Core with PostgreSQL

The project follows dependency injection pattern where API → Application → Domain, and Infrastructure implements Application interfaces.

### Game Concept

**Genre**: Idle MMORPG (Auto-battle, Offline rewards, Social features)

**Core Gameplay Loop**:
```
Enter Dungeon → Auto Battle → Earn Rewards → Upgrade (Character/Equipment/Skills)
→ PVP Challenge → Guild Activities → Boss Raid → Next Dungeon
```

**20 Major Systems**:
1. ✅ Authentication (JWT)
2. ✅ Character Growth (Auto-stat progression)
3. Inventory & Equipment
4. Combat System (Auto-battle simulation)
5. Offline Rewards
6. Dungeon System
7. Equipment Enhancement
8. Skill System
9. Pet System
10. PVP Arena
11. Friend System
12. Guild System
13. Real-time Chat (SignalR)
14. Boss Raid (Cooperative)
15. Quest & Achievement
16. Daily Mission & Attendance
17. Gacha System
18. Shop & VIP
19. Ranking System (Redis)
20. Mail & Event System

### Deployment Architecture

**For 10k concurrent users, the project uses a monolithic architecture combining API + Scheduler:**
- See **[ARCHITECTURE-GUIDE.md](./ARCHITECTURE-GUIDE.md)** for comprehensive scaling strategy
- Combined API + Background Services in single process (cost-effective)
- Horizontal scaling with load balancer for high availability
- Redis distributed locking for multi-instance coordination
- Separation only needed when reaching 50k+ concurrent users

**Deployment guides:**
- Local/Development: Use `./dev-start.sh` for infrastructure only
- EC2 Production: See **[EC2-DEPLOYMENT.md](./EC2-DEPLOYMENT.md)** for deployment steps

## Development Environment

### Running the Application

**Option 1: Local development (recommended)**
```bash
./dev-start.sh              # Starts infrastructure (PostgreSQL, Redis, pgAdmin)
cd IdleRPG.API
dotnet run                  # Run API locally with hot reload
```

**Option 2: Full Docker environment**
```bash
# First time setup: Generate HTTPS certificate
.\setup-https-cert.ps1      # Windows
./setup-https-cert.sh       # Linux/Mac

# Start all services with Docker
./docker-start.sh           # Starts everything including API
```

**Stop services:**
```bash
docker-compose down         # Stop all Docker containers
```

**Access development tools:**
- API Swagger (HTTP): http://localhost:5172/swagger
- API Swagger (HTTPS): https://localhost:7122/swagger
- pgAdmin: http://localhost:8082 (admin@idlerpg.com / admin123)
- PostgreSQL: localhost:5432 (gamedev / dev123!)
- Redis: localhost:6379

### Building and Testing

**Build the solution:**
```bash
dotnet build IdleRPGServer.sln
```

**Build specific project:**
```bash
dotnet build IdleRPG.API/IdleRPG.API.csproj
```

**Run tests:**
```bash
dotnet test                              # Run all tests
dotnet test --verbosity detailed         # Run with detailed output
cd IdleRPG.Tests && dotnet test         # Run specific test project
```

### Database Operations

**Entity Framework migrations:**
```bash
cd IdleRPG.Infrastructure
dotnet ef migrations add <MigrationName> --startup-project ../IdleRPG.API
# ⚠️ DO NOT run 'dotnet ef database update' manually!
# Jenkins CI/CD automatically applies migrations on git push
```

**⚠️ IMPORTANT: Jenkins CI/CD handles database migrations automatically**

**Production Deployment Flow:**
1. Create EF Core migration locally (as shown above)
2. Commit and push to GitHub
3. Jenkins automatically executes the following pipeline:
   - **Step 1**: Git Pull from GitHub (5 min)
   - **Step 2**: Database Migration to RDS PostgreSQL (5 min) ← **Automatic!**
   - **Step 3**: Docker Build & Deploy to EC2 (20 min)
   - **Step 4**: Verification (2 min)

**Migration Strategy:**
- Migrations are applied via `migration.sql` scripts
- Jenkins executes `psql` commands directly to RDS
- Idempotent migrations with `IF NOT EXISTS` patterns
- **Never run `dotnet ef database update` in production** - it's handled by Jenkins

**Local Development:**
- Use `./dev-start.sh` for local PostgreSQL instance
- Apply migrations locally with `dotnet ef database update` if needed
- Changes to local DB do not affect production

## Key Technologies

- **Framework**: ASP.NET Core 8.0 Web API
- **Database**: PostgreSQL with Entity Framework Core 9.0
- **Caching & Ranking**: Redis (Distributed locking, Ranking system)
- **Real-time Communication**: SignalR (Chat system)
- **Patterns**: CQRS with MediatR, Repository pattern
- **Validation**: FluentValidation
- **Mapping**: AutoMapper
- **Logging**: Serilog (configured in API project)
- **Documentation**: Swagger/OpenAPI
- **Testing**: xUnit, Moq, FluentAssertions
- **Background Services**: IHostedService (Auto-hunt, Daily reset)

### Unity Client Technologies
- **Unity**: 6 (6000.0.59f2) - Latest stable release
- **Language**: C# 10
- **Async**: UniTask
- **JSON**: Newtonsoft.Json
- **SignalR Client**: Microsoft.AspNetCore.SignalR.Client
- **UI**: TextMeshPro
- **Testing**: Unity Test Framework

## Docker Configuration

The project uses docker-compose with the following services:
- **PostgreSQL** - Database on port 5432
- **Redis** - Cache on port 6379
- **pgAdmin** - Database management UI on port 8082
- **API** - ASP.NET Core API on ports 5172 (HTTP) and 7122 (HTTPS)

All services are connected via `idlerpg-network` bridge network.

### HTTPS Configuration

For HTTPS support in Docker, you need to generate a development certificate:

**Windows:**
```powershell
.\setup-https-cert.ps1
```

**Linux/Mac:**
```bash
./setup-https-cert.sh
```

This creates a certificate at `~/.aspnet/https/aspnetapp.pfx` (or `%USERPROFILE%\.aspnet\https\aspnetapp.pfx` on Windows) with password `dev123!`, which is automatically mounted into the API container.

### Environment Variables

The API container uses these key environment variables:
- `ASPNETCORE_ENVIRONMENT=Development`
- `ASPNETCORE_URLS=https://+:7122;http://+:5172`
- `ConnectionStrings__DefaultConnection` - Points to postgres container
- `ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx`
- `ASPNETCORE_Kestrel__Certificates__Default__Password=dev123!`

## Development Progress & Roadmap

### Current Status (Week 1-2 Phase)

**✅ Completed (Week 1)**:
- JWT Bearer authentication system (5 endpoints)
- Character growth system (6 endpoints)
- Auto-stat progression (Level → Stats)
- Monster entities (5 types seeded)
- PostgreSQL + EF Core migrations
- Jenkins CI/CD pipeline
- AWS EC2 + RDS deployment
- Unit tests (17 tests passing)

**🔄 In Progress (Week 2)**:
- Combat system core logic
- Offline reward calculation
- Background service (IHostedService)
- Battle log system

**📋 Next Priority (Week 3-6)**:
- Inventory & Equipment system
- Dungeon progression system
- Equipment enhancement (probability-based)
- Skill system foundation

### 20-Week Development Roadmap

**See**: `docs/MUSHROOM_GAME_PRD.md` for complete system specifications

**Phase 1: Foundation (Week 1-6)** - MVP Systems
1. ✅ Authentication System
2. ✅ Character Growth System
3. ⏳ Inventory & Equipment
4. ⏳ Combat System (Auto-battle)
5. ⏳ Offline Rewards
6. ⏳ Dungeon System
7. 📋 Equipment Enhancement

**Phase 2: Expansion (Week 7-12)** - Core Gameplay
8. 📋 Skill System
9. 📋 Pet System
10. 📋 PVP Arena (ELO rating)
11. 📋 Friend System
12. 📋 Guild System (Part 1: Basic)
13. 📋 Guild System (Part 2: Raids)

**Phase 3: Advanced (Week 13-18)** - Social & Monetization
14. 📋 Real-time Chat (SignalR)
15. 📋 Boss Raid (Cooperative)
16. 📋 Quest & Achievement
17. 📋 Daily Mission & Attendance
18. 📋 Gacha System
19. 📋 Shop & VIP System

**Phase 4: Polish (Week 19-20)** - Live Operations
20. 📋 Ranking System (Redis)
21. 📋 Mail & Event System

### Learning Milestones

- **Week 1-6**: RESTful API, EF Core, JWT, Background Services
- **Week 7-12**: Complex relationships, Game balancing, ELO systems
- **Week 13-18**: SignalR real-time, Redis caching, Monetization
- **Week 19-20**: Live operations, Event systems, Production optimization

### PRD Documents

- **Week 2 PRD**: `.taskmaster/docs/week2-prd.txt` (Idle Game Loop)
- **Complete PRD**: `docs/MUSHROOM_GAME_PRD.md` (20-week roadmap)

## Collaboration Rules (Learning-Oriented Development)

### 🤖 Claude 자동 처리 작업
다음 작업은 Claude가 독립적으로 완료:
- **단순 반복 작업**: CRUD 메서드, Repository 패턴 적용
- **보일러플레이트**: DTO 생성, 엔티티 필드 추가, Configuration
- **마이그레이션**: EF Core 마이그레이션 생성 및 검토
- **문서화**: Unity API 문서, Swagger 주석
- **테스트 코드**: 단위 테스트 작성
- **코드 정리**: 네이밍, 주석, 포맷팅

### 👥 함께 협업하는 작업
다음 작업은 설계/구현 전 논의하고 사용자 의견 반영:
- **데이터 설계**: 엔티티 관계, 필드 타입, 인덱스 전략
- **비즈니스 로직**: 전투 공식, 보상 계산, 밸런싱
- **네트워크 로직**: API 설계, 요청/응답 구조, 에러 핸들링
- **아키텍처 설계**: 계층 분리, 서비스 분할, 의존성 구조
- **성능 최적화**: 쿼리 최적화, 캐싱, 동시성 처리
- **보안 설계**: 인증/인가, 데이터 검증

### 협업 프로세스
1. **설계**: Claude가 초안 제시 → 사용자 피드백 → 최종 결정
2. **구현**: 핵심 로직은 함께 작성 (TODO(human)), 반복 코드는 자동 완성
3. **검토**: 구현 후 주요 변경사항 요약

**상세 규칙**: Serena 메모리 `week2-workflow-and-collaboration-rules` 참조

## Development Guidelines

**When adding new features:**
1. Create domain entities in `IdleRPG.Domain`
2. Define application interfaces and DTOs in `IdleRPG.Application`
3. Implement data repositories in `IdleRPG.Infrastructure`
4. Create API controllers in `IdleRPG.API`
5. Use MediatR for command/query handling
6. Apply FluentValidation for input validation
7. Map between DTOs using AutoMapper
8. Write unit tests in `IdleRPG.Tests`

**Testing guidelines:**
- Write unit tests for business logic in services
- Use **xUnit** as test framework, **Moq** for mocking, **FluentAssertions** for assertions
- Follow **AAA pattern** (Arrange-Act-Assert) in test structure
- Mock repository dependencies to isolate service logic
- Use Theory tests with InlineData for parameterized scenarios
- Test edge cases: null inputs, boundary values, error conditions
- Run tests before committing: `dotnet test`

**Database changes:**
- Always create EF migrations for schema changes
- Use the Infrastructure project for EF context and configurations
- Reference the API project as startup project for migrations

**Authentication:**
- JWT Bearer tokens implemented for user authentication
- Access token (15 min expiry) and Refresh token (7 day expiry)
- BCrypt password hashing with salt
- Game-specific endpoints require [Authorize] attribute

**Infrastructure Adoption Strategy:**
- **Immediate use**: JWT, EF Core, xUnit (Week 1-2)
- **Phase 1 (Week 3-6)**: FluentValidation, AutoMapper, Serilog
- **Phase 2 (Week 7-12)**: MediatR (CQRS), Redis (caching)
- **Phase 3 (Week 13-18)**: SignalR, Redis (ranking), IHostedService optimization
- **Phase 4 (Week 19-20)**: Application Insights, Performance monitoring

### Game Balance Philosophy
- Server-side validation for critical operations (combat, rewards, gacha)
- Client is presentation layer only (UI, animations, input)
- All economy-affecting logic runs on server
- Prevent cheating through server-authoritative design

## Unity Client Documentation

**IMPORTANT: When adding or modifying APIs or DTOs, ALWAYS update Unity documentation files.**

The project maintains Unity client documentation in the **IdleRPGClient** project folder with organized structure:

### Documentation Structure
```
../IdleRPGClient/Docs/
├── README.md                              ← Documentation index
├── Unity-DTOs.cs                          ← Unity C# DTO classes (copy-paste ready)
├── unity/                                 ← Unity client documentation
│   ├── API_SPEC_FOR_UNITY.md             ← Server API specification for Unity
│   ├── Unity-API-Reference.md            ← Detailed API reference with examples
│   ├── Unity-Quick-Reference.md          ← Quick reference guide
│   └── UNITY_PROJECT_CONTEXT.md          ← Unity project context and settings
└── server/                                ← Server integration documentation
    └── SERVER_DEPLOYMENT.md               ← Server deployment guide
```

### Primary Documentation Files (Must Update)
1. **../IdleRPGClient/Docs/unity/API_SPEC_FOR_UNITY.md** - Main API specification for Unity developers
2. **../IdleRPGClient/Docs/Unity-DTOs.cs** - C# DTO classes with [Serializable] attribute
3. **../IdleRPGClient/Docs/unity/Unity-Quick-Reference.md** - Quick lookup tables and examples

### Update Rules

**When creating a new API endpoint:**
1. Add endpoint details to `unity/API_SPEC_FOR_UNITY.md`:
   - Endpoint path and HTTP method
   - Request/Response JSON examples
   - Authentication requirements
   - Unity C# usage example with UnityWebRequest
2. Add corresponding DTO classes to `Unity-DTOs.cs` (root level)
3. Update endpoint summary table in `unity/Unity-Quick-Reference.md`
4. Optionally update `unity/Unity-API-Reference.md` if detailed reference needed

**When adding a new DTO:**
1. Create the DTO in `IdleRPG.Application/DTOs/`
2. Add C# `[Serializable]` version to `Unity-DTOs.cs`
3. Document all properties with XML comments
4. Add usage example in code comments

**When modifying existing endpoints or DTOs:**
1. Update all relevant documentation files simultaneously
2. Mark breaking changes with **⚠️ BREAKING CHANGE** in docs
3. Update version date at bottom of markdown files
4. Consider updating `unity/UNITY_PROJECT_CONTEXT.md` if project structure changes

### Documentation Checklist
Before completing any API-related task, verify:
- [ ] API endpoint documented in `unity/API_SPEC_FOR_UNITY.md`
- [ ] DTO class added/updated in `Unity-DTOs.cs`
- [ ] Quick reference table updated in `unity/Unity-Quick-Reference.md`
- [ ] Example code provided for new features
- [ ] Error responses documented
- [ ] Authentication requirements specified

**Note**: Documentation is stored in the Unity client project folder, not the server folder, to keep client-related files together. The `unity/` subfolder contains Unity-specific documentation, while `server/` contains server deployment guides.

## Task Master AI Instructions
**Import Task Master's development workflow commands and guidelines, treat as if import is in the main CLAUDE.md file.**
@./.taskmaster/CLAUDE.md
