CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251001072028_InitialCreate') THEN
    CREATE TABLE "Players" (
        "Id" uuid NOT NULL,
        "UserName" character varying(50) NOT NULL,
        "PasswordHash" character varying(100) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        "LastLoginAt" timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        "IsActive" boolean NOT NULL DEFAULT TRUE,
        CONSTRAINT "PK_Players" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251001072028_InitialCreate') THEN
    CREATE TABLE "RefreshTokens" (
        "Id" uuid NOT NULL,
        "Token" text NOT NULL,
        "PlayerId" uuid NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "ExpiresAt" timestamp with time zone NOT NULL,
        "RevokedAt" timestamp with time zone,
        CONSTRAINT "PK_RefreshTokens" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_RefreshTokens_Players_PlayerId" FOREIGN KEY ("PlayerId") REFERENCES "Players" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251001072028_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Players_UserName" ON "Players" ("UserName");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251001072028_InitialCreate') THEN
    CREATE INDEX "IX_RefreshTokens_PlayerId" ON "RefreshTokens" ("PlayerId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251001072028_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_RefreshTokens_Token" ON "RefreshTokens" ("Token");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251001072028_InitialCreate') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251001072028_InitialCreate', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251001082640_AddCharacterEntity') THEN
    CREATE TABLE "Characters" (
        "Id" uuid NOT NULL,
        "PlayerId" uuid NOT NULL,
        "Level" integer NOT NULL,
        "Experience" integer NOT NULL,
        "Strength" integer NOT NULL,
        "Dexterity" integer NOT NULL,
        "Intelligence" integer NOT NULL,
        "Vitality" integer NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Characters" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Characters_Players_PlayerId" FOREIGN KEY ("PlayerId") REFERENCES "Players" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251001082640_AddCharacterEntity') THEN
    CREATE INDEX "IX_Characters_PlayerId" ON "Characters" ("PlayerId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251001082640_AddCharacterEntity') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251001082640_AddCharacterEntity', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251002064144_AddStatPointsToCharacter') THEN
    ALTER TABLE "Characters" ADD "StatPoints" integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251002064144_AddStatPointsToCharacter') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251002064144_AddStatPointsToCharacter', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251013061914_AddMonsterEntity') THEN
    CREATE TABLE "Monsters" (
        "Id" uuid NOT NULL,
        "Name" character varying(50) NOT NULL,
        "Level" integer NOT NULL,
        "MaxHealth" integer NOT NULL,
        "Attack" integer NOT NULL,
        "Defense" integer NOT NULL,
        "ExperienceReward" integer NOT NULL,
        "GoldReward" integer NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        "UpdatedAt" timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        CONSTRAINT "PK_Monsters" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251013061914_AddMonsterEntity') THEN
    INSERT INTO "Monsters" ("Id", "Attack", "CreatedAt", "Defense", "ExperienceReward", "GoldReward", "Level", "MaxHealth", "Name", "UpdatedAt")
    VALUES ('11111111-1111-1111-1111-111111111111', 10, TIMESTAMPTZ '2025-01-01T00:00:00Z', 5, 10, 5, 1, 50, '슬라임', TIMESTAMPTZ '2025-01-01T00:00:00Z');
    INSERT INTO "Monsters" ("Id", "Attack", "CreatedAt", "Defense", "ExperienceReward", "GoldReward", "Level", "MaxHealth", "Name", "UpdatedAt")
    VALUES ('22222222-2222-2222-2222-222222222222', 30, TIMESTAMPTZ '2025-01-01T00:00:00Z', 15, 50, 25, 5, 150, '고블린', TIMESTAMPTZ '2025-01-01T00:00:00Z');
    INSERT INTO "Monsters" ("Id", "Attack", "CreatedAt", "Defense", "ExperienceReward", "GoldReward", "Level", "MaxHealth", "Name", "UpdatedAt")
    VALUES ('33333333-3333-3333-3333-333333333333', 60, TIMESTAMPTZ '2025-01-01T00:00:00Z', 30, 100, 50, 10, 300, '오크', TIMESTAMPTZ '2025-01-01T00:00:00Z');
    INSERT INTO "Monsters" ("Id", "Attack", "CreatedAt", "Defense", "ExperienceReward", "GoldReward", "Level", "MaxHealth", "Name", "UpdatedAt")
    VALUES ('44444444-4444-4444-4444-444444444444', 100, TIMESTAMPTZ '2025-01-01T00:00:00Z', 50, 150, 75, 15, 500, '트롤', TIMESTAMPTZ '2025-01-01T00:00:00Z');
    INSERT INTO "Monsters" ("Id", "Attack", "CreatedAt", "Defense", "ExperienceReward", "GoldReward", "Level", "MaxHealth", "Name", "UpdatedAt")
    VALUES ('55555555-5555-5555-5555-555555555555', 200, TIMESTAMPTZ '2025-01-01T00:00:00Z', 100, 300, 150, 20, 1000, '드래곤', TIMESTAMPTZ '2025-01-01T00:00:00Z');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251013061914_AddMonsterEntity') THEN
    CREATE INDEX "IX_Monsters_Level" ON "Monsters" ("Level");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251013061914_AddMonsterEntity') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251013061914_AddMonsterEntity', '9.0.9');
    END IF;
END $EF$;
COMMIT;

