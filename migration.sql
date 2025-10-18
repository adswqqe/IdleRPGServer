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

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251013080752_AddGoldAndLastLoginToCharacter') THEN
    ALTER TABLE "Characters" ADD "Gold" bigint NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251013080752_AddGoldAndLastLoginToCharacter') THEN
    ALTER TABLE "Characters" ADD "LastLoginTime" timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251013080752_AddGoldAndLastLoginToCharacter') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251013080752_AddGoldAndLastLoginToCharacter', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251013133523_RefactorCharacterStatsToAutoGrowth') THEN
    ALTER TABLE "Characters" DROP COLUMN "Dexterity";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251013133523_RefactorCharacterStatsToAutoGrowth') THEN
    ALTER TABLE "Characters" DROP COLUMN "Intelligence";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251013133523_RefactorCharacterStatsToAutoGrowth') THEN
    ALTER TABLE "Characters" DROP COLUMN "StatPoints";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251013133523_RefactorCharacterStatsToAutoGrowth') THEN
    ALTER TABLE "Characters" DROP COLUMN "Strength";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251013133523_RefactorCharacterStatsToAutoGrowth') THEN
    ALTER TABLE "Characters" DROP COLUMN "Vitality";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251013133523_RefactorCharacterStatsToAutoGrowth') THEN
    ALTER TABLE "Characters" ADD "Attack" bigint NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251013133523_RefactorCharacterStatsToAutoGrowth') THEN
    ALTER TABLE "Characters" ADD "CritDamage" real NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251013133523_RefactorCharacterStatsToAutoGrowth') THEN
    ALTER TABLE "Characters" ADD "CritRate" real NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251013133523_RefactorCharacterStatsToAutoGrowth') THEN
    ALTER TABLE "Characters" ADD "Defense" bigint NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251013133523_RefactorCharacterStatsToAutoGrowth') THEN
    ALTER TABLE "Characters" ADD "Evasion" real NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251013133523_RefactorCharacterStatsToAutoGrowth') THEN
    ALTER TABLE "Characters" ADD "MaxHealth" bigint NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251013133523_RefactorCharacterStatsToAutoGrowth') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251013133523_RefactorCharacterStatsToAutoGrowth', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251014023217_AddAttackSpeedToCharacterAndMonster') THEN
    ALTER TABLE "Monsters" ADD "AttackSpeed" real NOT NULL DEFAULT 1;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251014023217_AddAttackSpeedToCharacterAndMonster') THEN
    ALTER TABLE "Monsters" ADD "CritDamage" real NOT NULL DEFAULT 1.5;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251014023217_AddAttackSpeedToCharacterAndMonster') THEN
    ALTER TABLE "Monsters" ADD "CritRate" real NOT NULL DEFAULT 0.05;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251014023217_AddAttackSpeedToCharacterAndMonster') THEN
    ALTER TABLE "Monsters" ADD "Evasion" real NOT NULL DEFAULT 0.05;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251014023217_AddAttackSpeedToCharacterAndMonster') THEN
    ALTER TABLE "Characters" ADD "AttackSpeed" real NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251014023217_AddAttackSpeedToCharacterAndMonster') THEN
    UPDATE "Monsters" SET "AttackSpeed" = 0.8, "CritDamage" = 1.3, "CritRate" = 0.03, "Evasion" = 0.02
    WHERE "Id" = '11111111-1111-1111-1111-111111111111';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251014023217_AddAttackSpeedToCharacterAndMonster') THEN
    UPDATE "Monsters" SET "AttackSpeed" = 1, "CritDamage" = 1.5, "CritRate" = 0.05, "Evasion" = 0.05
    WHERE "Id" = '22222222-2222-2222-2222-222222222222';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251014023217_AddAttackSpeedToCharacterAndMonster') THEN
    UPDATE "Monsters" SET "AttackSpeed" = 0.7, "CritDamage" = 1.8, "CritRate" = 0.04, "Evasion" = 0.03
    WHERE "Id" = '33333333-3333-3333-3333-333333333333';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251014023217_AddAttackSpeedToCharacterAndMonster') THEN
    UPDATE "Monsters" SET "AttackSpeed" = 0.6, "CritDamage" = 2, "CritRate" = 0.03, "Evasion" = 0.02
    WHERE "Id" = '44444444-4444-4444-4444-444444444444';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251014023217_AddAttackSpeedToCharacterAndMonster') THEN
    UPDATE "Monsters" SET "AttackSpeed" = 1.2, "CritDamage" = 2, "CritRate" = 0.1, "Evasion" = 0.08
    WHERE "Id" = '55555555-5555-5555-5555-555555555555';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251014023217_AddAttackSpeedToCharacterAndMonster') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251014023217_AddAttackSpeedToCharacterAndMonster', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251015071027_AddBattleLogTable') THEN
    CREATE TABLE "BattleLogs" (
        "Id" uuid NOT NULL,
        "CharacterId" uuid NOT NULL,
        "MonsterId" uuid NOT NULL,
        "IsVictory" boolean NOT NULL,
        "ExperienceGained" integer NOT NULL,
        "GoldGained" integer NOT NULL,
        "DamageDealt" integer NOT NULL,
        "DamageTaken" integer NOT NULL,
        "BattleDate" timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        CONSTRAINT "PK_BattleLogs" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_BattleLogs_Characters_CharacterId" FOREIGN KEY ("CharacterId") REFERENCES "Characters" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_BattleLogs_Monsters_MonsterId" FOREIGN KEY ("MonsterId") REFERENCES "Monsters" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251015071027_AddBattleLogTable') THEN
    CREATE INDEX "IX_BattleLogs_CharacterId_BattleDate" ON "BattleLogs" ("CharacterId", "BattleDate");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251015071027_AddBattleLogTable') THEN
    CREATE INDEX "IX_BattleLogs_MonsterId" ON "BattleLogs" ("MonsterId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251015071027_AddBattleLogTable') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251015071027_AddBattleLogTable', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251015085308_AddOfflineRewardTypeTable') THEN
    CREATE TABLE "OfflineRewardTypes" (
        "Id" uuid NOT NULL,
        "Name" character varying(100) NOT NULL,
        "ExperiencePerMinute" integer NOT NULL,
        "GoldPerMinute" integer NOT NULL,
        "MaxMinutes" integer NOT NULL,
        CONSTRAINT "PK_OfflineRewardTypes" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251015085308_AddOfflineRewardTypeTable') THEN
    INSERT INTO "OfflineRewardTypes" ("Id", "ExperiencePerMinute", "GoldPerMinute", "MaxMinutes", "Name")
    VALUES ('11111111-1111-1111-1111-111111111111', 2, 1, 480, 'Basic Offline Reward');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251015085308_AddOfflineRewardTypeTable') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251015085308_AddOfflineRewardTypeTable', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251016121735_AddEquipmentTableWithOwner') THEN
    ALTER TABLE "Equipments" ADD "OwnerId" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251016121735_AddEquipmentTableWithOwner') THEN
    CREATE INDEX "IX_Equipments_OwnerId" ON "Equipments" ("OwnerId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251016121735_AddEquipmentTableWithOwner') THEN
    ALTER TABLE "Equipments" ADD CONSTRAINT "FK_Equipments_Characters_OwnerId" FOREIGN KEY ("OwnerId") REFERENCES "Characters" ("Id") ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251016121735_AddEquipmentTableWithOwner') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251016121735_AddEquipmentTableWithOwner', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251017042133_AddDungeonSystemAndLootTablePattern') THEN
    CREATE TABLE "DungeonTemplates" (
        "Id" integer GENERATED BY DEFAULT AS IDENTITY,
        "Name" character varying(100) NOT NULL,
        "Description" character varying(500),
        "Category" character varying(50) NOT NULL,
        "MinLevel" integer NOT NULL,
        "IsEnabled" boolean NOT NULL DEFAULT TRUE,
        CONSTRAINT "PK_DungeonTemplates" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251017042133_AddDungeonSystemAndLootTablePattern') THEN
    CREATE TABLE "ItemTemplates" (
        "Id" uuid NOT NULL,
        "Name" character varying(100) NOT NULL,
        "Description" character varying(500),
        "Type" character varying(50) NOT NULL,
        "IconUrl" character varying(500),
        "MaxStackSize" integer NOT NULL DEFAULT 999,
        CONSTRAINT "PK_ItemTemplates" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251017042133_AddDungeonSystemAndLootTablePattern') THEN
    CREATE TABLE "LootTables" (
        "Id" integer GENERATED BY DEFAULT AS IDENTITY,
        "Name" character varying(100) NOT NULL,
        "NumberOfRolls" integer NOT NULL DEFAULT 1,
        CONSTRAINT "PK_LootTables" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251017042133_AddDungeonSystemAndLootTablePattern') THEN
    CREATE TABLE "UserDungeonDailies" (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "DungeonTemplateId" integer NOT NULL,
        "DifficultyCode" character varying(50) NOT NULL,
        "EntryCount" integer NOT NULL DEFAULT 0,
        "Date" date NOT NULL,
        CONSTRAINT "PK_UserDungeonDailies" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_UserDungeonDailies_DungeonTemplates_DungeonTemplateId" FOREIGN KEY ("DungeonTemplateId") REFERENCES "DungeonTemplates" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_UserDungeonDailies_Players_UserId" FOREIGN KEY ("UserId") REFERENCES "Players" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251017042133_AddDungeonSystemAndLootTablePattern') THEN
    CREATE TABLE "PlayerItems" (
        "Id" uuid NOT NULL,
        "CharacterId" uuid NOT NULL,
        "ItemTemplateId" uuid NOT NULL,
        "Quantity" integer NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        "UpdatedAt" timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        CONSTRAINT "PK_PlayerItems" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_PlayerItems_Characters_CharacterId" FOREIGN KEY ("CharacterId") REFERENCES "Characters" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_PlayerItems_ItemTemplates_ItemTemplateId" FOREIGN KEY ("ItemTemplateId") REFERENCES "ItemTemplates" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251017042133_AddDungeonSystemAndLootTablePattern') THEN
    CREATE TABLE "DungeonDifficulties" (
        "Id" integer GENERATED BY DEFAULT AS IDENTITY,
        "TemplateId" integer NOT NULL,
        "Code" character varying(50) NOT NULL,
        "RecommendedPower" integer NOT NULL,
        "BaseGold" bigint NOT NULL,
        "BaseExp" integer NOT NULL,
        "LootTableId" integer,
        "MaxWaves" integer NOT NULL,
        "DailyEntryLimit" integer NOT NULL DEFAULT 3,
        "EntryCostGold" integer NOT NULL DEFAULT 0,
        CONSTRAINT "PK_DungeonDifficulties" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_DungeonDifficulties_DungeonTemplates_TemplateId" FOREIGN KEY ("TemplateId") REFERENCES "DungeonTemplates" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_DungeonDifficulties_LootTables_LootTableId" FOREIGN KEY ("LootTableId") REFERENCES "LootTables" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251017042133_AddDungeonSystemAndLootTablePattern') THEN
    CREATE TABLE "LootItems" (
        "Id" integer GENERATED BY DEFAULT AS IDENTITY,
        "LootTableId" integer NOT NULL,
        "Type" character varying(50) NOT NULL,
        "ItemId" uuid,
        "IsGuaranteed" boolean NOT NULL DEFAULT FALSE,
        "Weight" integer NOT NULL,
        "MinQuantity" integer NOT NULL DEFAULT 1,
        "MaxQuantity" integer NOT NULL DEFAULT 1,
        CONSTRAINT "PK_LootItems" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_LootItems_LootTables_LootTableId" FOREIGN KEY ("LootTableId") REFERENCES "LootTables" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251017042133_AddDungeonSystemAndLootTablePattern') THEN
    CREATE TABLE "DungeonProgresses" (
        "Id" uuid NOT NULL,
        "CharacterId" uuid NOT NULL,
        "DifficultyId" integer NOT NULL,
        "CurrentWave" integer NOT NULL DEFAULT 1,
        "CurrentHealth" integer NOT NULL,
        "StartedAt" timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        CONSTRAINT "PK_DungeonProgresses" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_DungeonProgresses_Characters_CharacterId" FOREIGN KEY ("CharacterId") REFERENCES "Characters" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_DungeonProgresses_DungeonDifficulties_DifficultyId" FOREIGN KEY ("DifficultyId") REFERENCES "DungeonDifficulties" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251017042133_AddDungeonSystemAndLootTablePattern') THEN
    CREATE TABLE "DungeonRunHistories" (
        "Id" uuid NOT NULL,
        "CharacterId" uuid NOT NULL,
        "DifficultyId" integer NOT NULL,
        "IsCleared" boolean NOT NULL,
        "ClearedWave" integer NOT NULL,
        "CompletedAt" timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        CONSTRAINT "PK_DungeonRunHistories" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_DungeonRunHistories_Characters_CharacterId" FOREIGN KEY ("CharacterId") REFERENCES "Characters" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_DungeonRunHistories_DungeonDifficulties_DifficultyId" FOREIGN KEY ("DifficultyId") REFERENCES "DungeonDifficulties" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251017042133_AddDungeonSystemAndLootTablePattern') THEN
    CREATE TABLE "DungeonWaves" (
        "Id" integer GENERATED BY DEFAULT AS IDENTITY,
        "DifficultyId" integer NOT NULL,
        "WaveNumber" integer NOT NULL,
        "MonsterId" uuid NOT NULL,
        CONSTRAINT "PK_DungeonWaves" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_DungeonWaves_DungeonDifficulties_DifficultyId" FOREIGN KEY ("DifficultyId") REFERENCES "DungeonDifficulties" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_DungeonWaves_Monsters_MonsterId" FOREIGN KEY ("MonsterId") REFERENCES "Monsters" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251017042133_AddDungeonSystemAndLootTablePattern') THEN
    CREATE INDEX "IX_DungeonDifficulties_LootTableId" ON "DungeonDifficulties" ("LootTableId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251017042133_AddDungeonSystemAndLootTablePattern') THEN
    CREATE INDEX "IX_DungeonDifficulties_TemplateId" ON "DungeonDifficulties" ("TemplateId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251017042133_AddDungeonSystemAndLootTablePattern') THEN
    CREATE UNIQUE INDEX "IX_DungeonProgresses_CharacterId" ON "DungeonProgresses" ("CharacterId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251017042133_AddDungeonSystemAndLootTablePattern') THEN
    CREATE INDEX "IX_DungeonProgresses_DifficultyId" ON "DungeonProgresses" ("DifficultyId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251017042133_AddDungeonSystemAndLootTablePattern') THEN
    CREATE INDEX "IX_DungeonRunHistories_CharacterId_CompletedAt" ON "DungeonRunHistories" ("CharacterId", "CompletedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251017042133_AddDungeonSystemAndLootTablePattern') THEN
    CREATE INDEX "IX_DungeonRunHistories_DifficultyId" ON "DungeonRunHistories" ("DifficultyId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251017042133_AddDungeonSystemAndLootTablePattern') THEN
    CREATE INDEX "IX_DungeonWaves_DifficultyId" ON "DungeonWaves" ("DifficultyId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251017042133_AddDungeonSystemAndLootTablePattern') THEN
    CREATE INDEX "IX_DungeonWaves_MonsterId" ON "DungeonWaves" ("MonsterId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251017042133_AddDungeonSystemAndLootTablePattern') THEN
    CREATE INDEX "IX_LootItems_LootTableId" ON "LootItems" ("LootTableId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251017042133_AddDungeonSystemAndLootTablePattern') THEN
    CREATE INDEX "IX_PlayerItems_CharacterId" ON "PlayerItems" ("CharacterId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251017042133_AddDungeonSystemAndLootTablePattern') THEN
    CREATE INDEX "IX_PlayerItems_ItemTemplateId" ON "PlayerItems" ("ItemTemplateId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251017042133_AddDungeonSystemAndLootTablePattern') THEN
    CREATE INDEX "IX_UserDungeonDailies_DungeonTemplateId" ON "UserDungeonDailies" ("DungeonTemplateId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251017042133_AddDungeonSystemAndLootTablePattern') THEN
    CREATE UNIQUE INDEX "IX_UserDungeonDailies_UserId_DungeonTemplateId_DifficultyCode_~" ON "UserDungeonDailies" ("UserId", "DungeonTemplateId", "DifficultyCode", "Date");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251017042133_AddDungeonSystemAndLootTablePattern') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251017042133_AddDungeonSystemAndLootTablePattern', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251018112141_AddDungeonStageSystem') THEN
    ALTER TABLE "BattleLogs" ADD "DungeonStageId" integer;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251018112141_AddDungeonStageSystem') THEN
    CREATE TABLE "CharacterDungeonProgresses" (
        "Id" uuid NOT NULL,
        "CharacterId" uuid NOT NULL,
        "HighestStageClearedNormal" integer NOT NULL DEFAULT 0,
        "HighestStageClearedHard" integer NOT NULL DEFAULT 0,
        "HighestStageClearedNightmare" integer NOT NULL DEFAULT 0,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_CharacterDungeonProgresses" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_CharacterDungeonProgresses_Characters_CharacterId" FOREIGN KEY ("CharacterId") REFERENCES "Characters" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251018112141_AddDungeonStageSystem') THEN
    CREATE TABLE "DungeonStages" (
        "Id" integer GENERATED BY DEFAULT AS IDENTITY,
        "Name" character varying(100) NOT NULL,
        "RequiredLevel" integer NOT NULL,
        "MonsterId" uuid NOT NULL,
        "BaseExperience" integer NOT NULL,
        "BaseGold" integer NOT NULL,
        "FirstClearBonusExp" integer,
        "FirstClearBonusGold" integer,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_DungeonStages" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_DungeonStages_Monsters_MonsterId" FOREIGN KEY ("MonsterId") REFERENCES "Monsters" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251018112141_AddDungeonStageSystem') THEN
    CREATE UNIQUE INDEX "IX_CharacterDungeonProgresses_CharacterId_Unique" ON "CharacterDungeonProgresses" ("CharacterId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251018112141_AddDungeonStageSystem') THEN
    CREATE INDEX "IX_DungeonStages_MonsterId" ON "DungeonStages" ("MonsterId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251018112141_AddDungeonStageSystem') THEN
    CREATE INDEX "IX_DungeonStages_RequiredLevel" ON "DungeonStages" ("RequiredLevel");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251018112141_AddDungeonStageSystem') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251018112141_AddDungeonStageSystem', '9.0.9');
    END IF;
END $EF$;
COMMIT;

