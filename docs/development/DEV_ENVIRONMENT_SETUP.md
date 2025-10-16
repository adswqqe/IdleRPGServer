# Development Environment Setup

This guide covers one-time setup for local development environment.

## Running the Application

### Option 1: Local Development (Recommended)

```bash
./dev-start.sh              # Starts infrastructure (PostgreSQL, Redis, pgAdmin)
cd IdleRPG.API
dotnet run                  # Run API locally with hot reload
```

**Why recommended:**
- Faster iteration with hot reload
- Direct debugging in IDE
- Lower resource usage

### Option 2: Full Docker Environment

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

## Access Points

Once running, you can access:

- **API Swagger (HTTP)**: http://localhost:5172/swagger
- **API Swagger (HTTPS)**: https://localhost:7122/swagger
- **pgAdmin**: http://localhost:8082
  - Email: `admin@idlerpg.com`
  - Password: `admin123`
- **PostgreSQL**: `localhost:5432`
  - Username: `gamedev`
  - Password: `dev123!`
- **Redis**: `localhost:6379`

## Docker Configuration

The project uses docker-compose with the following services:

- **PostgreSQL** - Database on port 5432
- **Redis** - Cache on port 6379
- **pgAdmin** - Database management UI on port 8082
- **API** - ASP.NET Core API on ports 5172 (HTTP) and 7122 (HTTPS)

All services are connected via `idlerpg-network` bridge network.

### HTTPS Certificate Setup

For HTTPS support in Docker, generate a development certificate:

**Windows:**
```powershell
.\setup-https-cert.ps1
```

**Linux/Mac:**
```bash
./setup-https-cert.sh
```

This creates a certificate at:
- Windows: `%USERPROFILE%\.aspnet\https\aspnetapp.pfx`
- Linux/Mac: `~/.aspnet/https/aspnetapp.pfx`

Password: `dev123!` (automatically mounted into the API container)

### Environment Variables

The API container uses these key environment variables:

```bash
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=https://+:7122;http://+:5172
ConnectionStrings__DefaultConnection=Host=postgres;Database=idlerpg;Username=gamedev;Password=dev123!
ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx
ASPNETCORE_Kestrel__Certificates__Default__Password=dev123!
```

## Building and Testing

### Build Commands

```bash
# Build entire solution
dotnet build IdleRPGServer.sln

# Build specific project
dotnet build IdleRPG.API/IdleRPG.API.csproj
```

### Test Commands

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --verbosity detailed

# Run specific test project
cd IdleRPG.Tests && dotnet test
```

## Local Database Management

### Applying Migrations Locally

```bash
cd IdleRPG.Infrastructure
dotnet ef database update --startup-project ../IdleRPG.API
```

**Note**: This only affects your local development database. Production migrations are handled automatically by Jenkins CI/CD.

### Reset Local Database

```bash
# Drop and recreate database
dotnet ef database drop --startup-project ../IdleRPG.API --force
dotnet ef database update --startup-project ../IdleRPG.API
```

## Troubleshooting

### Port Already in Use

If you see "port already in use" errors:

```bash
# Check what's using the ports
docker ps
netstat -ano | findstr :5432  # Windows
lsof -i :5432                 # Linux/Mac

# Stop all containers
docker-compose down
```

### Certificate Issues

If HTTPS doesn't work:

1. Verify certificate exists in `~/.aspnet/https/`
2. Re-run certificate setup script
3. Restart Docker containers

### Database Connection Issues

1. Verify PostgreSQL container is running: `docker ps`
2. Check connection string in `appsettings.Development.json`
3. Try connecting via pgAdmin first to verify credentials

---

**Last Updated**: 2025-10-17
