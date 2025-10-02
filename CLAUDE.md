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
- JWT authentication system implemented (Player registration, login, token refresh)
- Character system in progress (Entity and Repository completed)

**Completed Features:**
- ✅ JWT Bearer authentication with refresh token
- ✅ Player registration and login system
- ✅ Character entity and CharacterStats value object
- ✅ Character repository with CRUD operations
- ✅ Database migrations (InitialCreate, AddCharacterEntity)

**Current Task:** Task 9.3 - Implementing character creation and validation logic

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

## Task Master AI Instructions
**Import Task Master's development workflow commands and guidelines, treat as if import is in the main CLAUDE.md file.**
@./.taskmaster/CLAUDE.md
