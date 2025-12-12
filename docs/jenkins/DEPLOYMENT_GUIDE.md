# Deployment Guide

## Production Deployment Flow

### Overview

Production deployments are **fully automated** via Jenkins CI/CD pipeline. Manual deployment commands should never be run on release branches.

### Automated Pipeline

When you push to GitHub, Jenkins automatically executes:

1. **Git Pull** (5 min)
   - Fetches latest code from GitHub

2. **Database Migration** (5 min) ← **Automatic!**
   - Applies `migration.sql` to RDS PostgreSQL
   - Uses `psql` commands directly

3. **Docker Build & Deploy** (20 min)
   - Builds Docker image
   - Deploys to EC2 instance

4. **Verification** (2 min)
   - Health checks
   - Smoke tests

**Total Time**: ~32 minutes

## Migration Strategy

### How Migrations Work

```bash
# 1. Create migration locally
cd IdleRPG.Infrastructure
dotnet ef migrations add <MigrationName> --startup-project ../IdleRPG.API

# 2. This generates migration.sql

# 3. Commit and push to GitHub
git add .
git commit -m "Add migration for <feature>"
git push

# 4. Jenkins automatically applies it to RDS
```

### Migration Best Practices

**Idempotent Patterns** - Always use `IF NOT EXISTS`:

```sql
-- ✅ Good
CREATE TABLE IF NOT EXISTS "Equipments" (...);

-- ❌ Bad
CREATE TABLE "Equipments" (...);
```

**Script Location**:
- `IdleRPG.Infrastructure/migration.sql`

**Jenkins Execution**:
```bash
psql -h <RDS_HOST> -U gamedev -d idlerpg -f migration.sql
```

### ⚠️ Critical Rules

**NEVER run these commands on release branches:**
```bash
# ❌ DON'T DO THIS
dotnet ef database update  # Jenkins handles this!
```

**Local Development Only:**
```bash
# ✅ OK for local dev
dotnet ef database update --startup-project ../IdleRPG.API
```

## Deployment Architecture

### For 10k Concurrent Users

**Monolithic Architecture** (API + Background Services):
- Combined process (cost-effective)
- Horizontal scaling with load balancer
- Redis distributed locking for multi-instance coordination

**When to Separate**:
- Only needed at 50k+ concurrent users
- See **[ARCHITECTURE-GUIDE.md](../ARCHITECTURE-GUIDE.md)** for details

### Infrastructure

- **API**: AWS EC2
- **Database**: AWS RDS PostgreSQL
- **Cache**: AWS ElastiCache Redis
- **CI/CD**: Jenkins (self-hosted)
- **Source**: GitHub

## Deployment Guides by Environment

### Local Development
See **[DEV_ENVIRONMENT_SETUP.md](../development/DEV_ENVIRONMENT_SETUP.md)**

### EC2 Production
See **[EC2-DEPLOYMENT.md](../../EC2-DEPLOYMENT.md)** (root level)

### Jenkins Setup
- **[JENKINS-AUTO-BUILD-SUCCESS.md](./JENKINS-AUTO-BUILD-SUCCESS.md)** - Initial setup
- **[JENKINS-TROUBLESHOOTING.md](./JENKINS-TROUBLESHOOTING.md)** - Common issues

## Rollback Procedure

If deployment fails:

1. **Check Jenkins logs** for error details
2. **Identify failing step** (Git, Migration, Build, or Deploy)
3. **Fix locally** and push again
4. **If critical**: Manually rollback RDS migration
   ```bash
   # Connect to RDS
   psql -h <RDS_HOST> -U gamedev -d idlerpg

   # Rollback specific migration
   # (Write rollback SQL based on what was applied)
   ```

## Monitoring

**Post-Deployment Checks:**
- [ ] API health endpoint responds: `https://<your-domain>/health`
- [ ] Swagger UI accessible: `https://<your-domain>/swagger`
- [ ] Database migrations applied: Check RDS logs
- [ ] Background services running: Check logs

---

**Last Updated**: 2025-10-17
