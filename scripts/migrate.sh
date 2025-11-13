#!/bin/bash
set -e

echo "[Step 1/4] Restoring dotnet tools..."
dotnet tool restore

echo "[Step 2/4] Running legacy migration.sql..."
if [ -f "IdleRPG.Infrastructure/migration.sql" ]; then
    export PGPASSWORD="$1"
    psql -h idlerpg-dev.chiqweeeuidv.ap-northeast-2.rds.amazonaws.com \
         -U postgres \
         -d idlerpg \
         -f IdleRPG.Infrastructure/migration.sql
    echo "✅ Legacy migration applied"
else
    echo "⚠️ migration.sql not found, skipping legacy migration"
fi

echo "[Step 3/4] Running EF Core migrations..."
dotnet ef database update \
    --project IdleRPG.Infrastructure \
    --connection "Host=idlerpg-dev.chiqweeeuidv.ap-northeast-2.rds.amazonaws.com;Port=5432;Database=idlerpg;Username=postgres;Password=$1;SSL Mode=Require;Trust Server Certificate=true"
echo "✅ EF Core migrations applied"

echo "[Step 4/4] Verifying migrations..."
export PGPASSWORD="$1"
psql -h idlerpg-dev.chiqweeeuidv.ap-northeast-2.rds.amazonaws.com \
     -U postgres \
     -d idlerpg \
     -c "SELECT MigrationId, ProductVersion FROM __EFMigrationsHistory ORDER BY MigrationId DESC LIMIT 5;" \
     || echo "⚠️ Verification skipped"
