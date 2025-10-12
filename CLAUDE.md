# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Language and Communication

**The user is Korean and all responses should be provided in Korean.** When explaining code, architecture, or providing guidance, translate technical explanations to Korean while keeping code examples and commands in their original form.

## Project Architecture

This is an **Idle RPG Server** project built with ASP.NET Core 8.0 following **Clean Architecture** principles:

- **IdleRPG.API**: Web API layer (Controllers, Middleware, Configuration)
- **IdleRPG.Application**: Business logic layer (Services, Commands/Queries, DTOs) - uses MediatR, AutoMapper, FluentValidation
- **IdleRPG.Domain**: Core domain entities and business rules
- **IdleRPG.Infrastructure**: Data access and external services - uses Entity Framework Core with PostgreSQL

The project follows dependency injection pattern where API → Application → Domain, and Infrastructure implements Application interfaces.

## Development Environment

### Running the Application

**Start development environment:**
```bash
./dev-start.sh    # Starts PostgreSQL, Redis, and pgAdmin containers
```

**Stop development environment:**
```bash
docker-compose down    # Stops all services
```

**Run the API server:**
```bash
cd IdleRPG.API
dotnet run    # Runs on http://localhost:5172, https://localhost:7122
```

**Access development tools:**
- API Documentation: http://localhost:5172/swagger (when running locally)
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

**Run tests:** (when test projects are added)
```bash
dotnet test
```

### Database Operations

**Entity Framework migrations:**
```bash
cd IdleRPG.Infrastructure
dotnet ef migrations add <MigrationName> --startup-project ../IdleRPG.API
dotnet ef database update --startup-project ../IdleRPG.API
```

## Key Technologies

- **Framework**: ASP.NET Core 8.0 Web API
- **Database**: PostgreSQL with Entity Framework Core 9.0
- **Caching**: Redis (prepared for future use)
- **Patterns**: CQRS with MediatR, Repository pattern
- **Validation**: FluentValidation
- **Mapping**: AutoMapper
- **Logging**: Serilog (configured in API project)
- **Documentation**: Swagger/OpenAPI

## Docker Configuration

The project uses docker-compose for development dependencies:
- PostgreSQL database on port 5432
- Redis cache on port 6379
- pgAdmin on port 8082

All services are connected via `idlerpg-network` bridge network.

## Current State

The project is in **active development** with:
- Clean Architecture project structure established
- Docker development environment configured (PostgreSQL, Redis, pgAdmin)
- Core NuGet packages installed (EF Core, MediatR, AutoMapper, FluentValidation, etc.)
- JWT authentication system fully implemented
- Character system fully implemented

**Completed Features:**
- ✅ JWT Bearer authentication (register, login, refresh, logout, profile)
- ✅ Player registration and login system
- ✅ Character entity and CharacterStats value object
- ✅ Character repository with CRUD operations
- ✅ Character service with all business logic
- ✅ Character controller with all endpoints:
  - POST /api/character/Create - 캐릭터 생성
  - GET /api/character/GetCharacters - 캐릭터 목록 조회
  - GET /api/character/{id} - 특정 캐릭터 조회
  - DELETE /api/character/{id} - 캐릭터 삭제
  - POST /api/character/{id}/experience - 경험치 획득
  - PUT /api/character/{id}/stats - 스탯 분배
- ✅ Database migrations (InitialCreate, AddCharacterEntity)
- ✅ Unity documentation updated with all API endpoints

**Current Task:** Week 1 character system complete. Ready for Week 2 features (Idle Game Loop & Progression).

## Development Guidelines

**When adding new features:**
1. Create domain entities in `IdleRPG.Domain`
2. Define application interfaces and DTOs in `IdleRPG.Application`
3. Implement data repositories in `IdleRPG.Infrastructure`
4. Create API controllers in `IdleRPG.API`
5. Use MediatR for command/query handling
6. Apply FluentValidation for input validation
7. Map between DTOs using AutoMapper

**Database changes:**
- Always create EF migrations for schema changes
- Use the Infrastructure project for EF context and configurations
- Reference the API project as startup project for migrations

**Authentication:**
- JWT Bearer tokens implemented for user authentication
- Access token (15 min expiry) and Refresh token (7 day expiry)
- BCrypt password hashing with salt
- Game-specific endpoints require [Authorize] attribute

**Week 0 Infrastructure Strategy:**
- Core game features first, infrastructure tools added when needed
- Serilog, FluentValidation, AutoMapper installed but not yet configured
- Will add logging/validation when debugging becomes difficult
- Focus on Week 1 features (Character system) before infrastructure setup

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
