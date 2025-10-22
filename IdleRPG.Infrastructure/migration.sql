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
    ALTER TABLE "Characters" DROP COLUMN IF EXISTS "Dexterity";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251013133523_RefactorCharacterStatsToAutoGrowth') THEN
    ALTER TABLE "Characters" DROP COLUMN IF EXISTS "Intelligence";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251013133523_RefactorCharacterStatsToAutoGrowth') THEN
    ALTER TABLE "Characters" DROP COLUMN IF EXISTS "StatPoints";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251013133523_RefactorCharacterStatsToAutoGrowth') THEN
    ALTER TABLE "Characters" DROP COLUMN IF EXISTS "Strength";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251013133523_RefactorCharacterStatsToAutoGrowth') THEN
    ALTER TABLE "Characters" DROP COLUMN IF EXISTS "Vitality";
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
    ALTER TABLE "Characters" ADD "CritDamage" real NOT NULL DEFAULT 0.0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251013133523_RefactorCharacterStatsToAutoGrowth') THEN
    ALTER TABLE "Characters" ADD "CritRate" real NOT NULL DEFAULT 0.0;
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
    ALTER TABLE "Characters" ADD "Evasion" real NOT NULL DEFAULT 0.0;
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
    ALTER TABLE "Monsters" ADD "AttackSpeed" real NOT NULL DEFAULT 1.0;
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
    ALTER TABLE "Characters" ADD "AttackSpeed" real NOT NULL DEFAULT 0.0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251014023217_AddAttackSpeedToCharacterAndMonster') THEN
    UPDATE "Monsters" SET "AttackSpeed" = 0.8, "CritDamage" = 1.3, "CritRate" = 0.03, "Evasion" = 0.02
    WHERE "Id" = '11111111-1111-1111-1111-111111111111';

    UPDATE "Monsters" SET "AttackSpeed" = 1.0, "CritDamage" = 1.5, "CritRate" = 0.05, "Evasion" = 0.05
    WHERE "Id" = '22222222-2222-2222-2222-222222222222';

    UPDATE "Monsters" SET "AttackSpeed" = 0.7, "CritDamage" = 1.8, "CritRate" = 0.04, "Evasion" = 0.03
    WHERE "Id" = '33333333-3333-3333-3333-333333333333';

    UPDATE "Monsters" SET "AttackSpeed" = 0.6, "CritDamage" = 2.0, "CritRate" = 0.03, "Evasion" = 0.02
    WHERE "Id" = '44444444-4444-4444-4444-444444444444';

    UPDATE "Monsters" SET "AttackSpeed" = 1.2, "CritDamage" = 2.0, "CritRate" = 0.1, "Evasion" = 0.08
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
    CREATE TABLE "Equipments" (
        "Id" uuid NOT NULL,
        "Name" character varying(100) NOT NULL,
        "Slot" integer NOT NULL,
        "Rarity" integer NOT NULL,
        "OwnerId" uuid NOT NULL,
        "CharacterId" uuid NULL,
        "EnhancementLevel" integer NOT NULL DEFAULT 0,
        "BaseAttack" integer NOT NULL DEFAULT 0,
        "BaseDefense" integer NOT NULL DEFAULT 0,
        "BaseHp" integer NOT NULL DEFAULT 0,
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        "UpdatedAt" timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        CONSTRAINT "PK_Equipments" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Equipments_Characters_OwnerId" FOREIGN KEY ("OwnerId") REFERENCES "Characters" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_Equipments_Characters_CharacterId" FOREIGN KEY ("CharacterId") REFERENCES "Characters" ("Id") ON DELETE SET NULL
    );
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
    CREATE INDEX "IX_Equipments_CharacterId" ON "Equipments" ("CharacterId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251016121735_AddEquipmentTableWithOwner') THEN
    CREATE INDEX "IX_Equipments_CharacterId_Slot" ON "Equipments" ("CharacterId", "Slot");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251016121735_AddEquipmentTableWithOwner') THEN
    CREATE INDEX "IX_Equipments_Rarity" ON "Equipments" ("Rarity");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251016121735_AddEquipmentTableWithOwner') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251016121735_AddEquipmentTableWithOwner', '9.0.9');
    END IF;
END $EF$;

-- ============================================
-- Migration: Add Skill Gacha System
-- Date: 2025-10-22
-- ============================================

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251022000000_AddSkillGachaSystem') THEN
    CREATE TABLE "SkillTemplates" (
        "Id" SERIAL PRIMARY KEY,
        "Name" VARCHAR(100) NOT NULL,
        "Rarity" INT NOT NULL,
        "Description" TEXT,
        "Type" INT NOT NULL,
        CONSTRAINT "CK_SkillTemplates_Rarity" CHECK ("Rarity" >= 0 AND "Rarity" <= 3),
        CONSTRAINT "CK_SkillTemplates_Type" CHECK ("Type" >= 0 AND "Type" <= 1)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251022000000_AddSkillGachaSystem') THEN
    CREATE INDEX "IX_SkillTemplates_Rarity" ON "SkillTemplates" ("Rarity");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251022000000_AddSkillGachaSystem') THEN
    CREATE TABLE "CharacterSkills" (
        "Id" UUID PRIMARY KEY,
        "CharacterId" UUID NOT NULL,
        "SkillTemplateId" INT NOT NULL,
        "IsEquipped" BOOLEAN NOT NULL DEFAULT FALSE,
        "AcquiredAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
        CONSTRAINT "FK_CharacterSkills_Characters" FOREIGN KEY ("CharacterId")
            REFERENCES "Characters"("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_CharacterSkills_SkillTemplates" FOREIGN KEY ("SkillTemplateId")
            REFERENCES "SkillTemplates"("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251022000000_AddSkillGachaSystem') THEN
    CREATE INDEX "IX_CharacterSkills_CharacterId" ON "CharacterSkills" ("CharacterId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251022000000_AddSkillGachaSystem') THEN
    CREATE INDEX "IX_CharacterSkills_SkillTemplateId" ON "CharacterSkills" ("SkillTemplateId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251022000000_AddSkillGachaSystem') THEN
    CREATE INDEX "IX_CharacterSkills_IsEquipped" ON "CharacterSkills" ("CharacterId", "IsEquipped");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251022000000_AddSkillGachaSystem') THEN
    CREATE TABLE "GachaHistories" (
        "Id" UUID PRIMARY KEY,
        "CharacterId" UUID NOT NULL,
        "SkillTemplateId" INT NOT NULL,
        "WasPityPull" BOOLEAN NOT NULL DEFAULT FALSE,
        "PulledAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
        CONSTRAINT "FK_GachaHistories_Characters" FOREIGN KEY ("CharacterId")
            REFERENCES "Characters"("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_GachaHistories_SkillTemplates" FOREIGN KEY ("SkillTemplateId")
            REFERENCES "SkillTemplates"("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251022000000_AddSkillGachaSystem') THEN
    CREATE INDEX "IX_GachaHistories_CharacterId" ON "GachaHistories" ("CharacterId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251022000000_AddSkillGachaSystem') THEN
    CREATE INDEX "IX_GachaHistories_PulledAt" ON "GachaHistories" ("CharacterId", "PulledAt" DESC);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251022000000_AddSkillGachaSystem') THEN
    ALTER TABLE "Characters" ADD COLUMN "Crystal" BIGINT NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251022000000_AddSkillGachaSystem') THEN
    ALTER TABLE "Characters" ADD COLUMN "GachaPityCount" INT NOT NULL DEFAULT 0;
    END IF;
END $EF$;

-- Seed Data: SkillTemplates
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251022000000_AddSkillGachaSystem') THEN
    -- Common 스킬 (10개)
    INSERT INTO "SkillTemplates" ("Name", "Rarity", "Type", "Description") VALUES
    ('화염구', 0, 0, '작은 화염구를 발사하여 적에게 피해를 입힙니다.'),
    ('치유', 0, 0, '체력을 소량 회복합니다.'),
    ('강타', 0, 0, '무기로 강하게 내리쳐 피해를 입힙니다.'),
    ('방어 태세', 0, 1, '방어력이 10% 증가합니다.'),
    ('빠른 발놀림', 0, 1, '회피율이 5% 증가합니다.'),
    ('독 화살', 0, 0, '독이 묻은 화살을 발사하여 지속 피해를 입힙니다.'),
    ('마나 회복', 0, 0, '마나를 소량 회복합니다.'),
    ('전투 본능', 0, 1, '공격력이 5% 증가합니다.'),
    ('얼음 화살', 0, 0, '얼음 화살을 발사하여 적의 이동 속도를 감소시킵니다.'),
    ('정신 집중', 0, 1, '치명타 확률이 3% 증가합니다.');

    -- Rare 스킬 (7개)
    INSERT INTO "SkillTemplates" ("Name", "Rarity", "Type", "Description") VALUES
    ('연쇄 번개', 1, 0, '번개가 여러 적에게 연쇄적으로 피해를 입힙니다.'),
    ('광역 치유', 1, 0, '주변 모든 아군의 체력을 회복합니다.'),
    ('회전 베기', 1, 0, '주변의 모든 적에게 피해를 입힙니다.'),
    ('강철 피부', 1, 1, '받는 피해가 15% 감소합니다.'),
    ('흡혈', 1, 1, '가한 피해의 10%만큼 체력을 회복합니다.'),
    ('화염 폭풍', 1, 0, '화염 폭풍을 일으켜 광범위한 지역에 피해를 입힙니다.'),
    ('전투 광기', 1, 1, '공격 속도가 20% 증가합니다.');

    -- Epic 스킬 (4개)
    INSERT INTO "SkillTemplates" ("Name", "Rarity", "Type", "Description") VALUES
    ('메테오', 2, 0, '하늘에서 거대한 운석을 떨어뜨려 막대한 피해를 입힙니다.'),
    ('부활', 2, 1, '사망 시 1회 부활합니다 (전투당 1회).'),
    ('신성한 축복', 2, 0, '모든 능력치가 일정 시간 동안 크게 증가합니다.'),
    ('광전사의 분노', 2, 1, '체력이 낮을수록 공격력이 증가합니다 (최대 50%).');

    -- Legendary 스킬 (3개)
    INSERT INTO "SkillTemplates" ("Name", "Rarity", "Type", "Description") VALUES
    ('시간 정지', 3, 0, '모든 적의 시간을 정지시켜 행동 불능 상태로 만듭니다.'),
    ('절대 방어', 3, 1, '치명적인 피해를 받을 때 1회 무적 상태가 됩니다 (전투당 1회).'),
    ('신의 심판', 3, 0, '신성한 빛으로 모든 적을 심판하여 즉사시킬 확률이 있습니다.');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251022000000_AddSkillGachaSystem') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251022000000_AddSkillGachaSystem', '9.0.9');
    END IF;
END $EF$;

COMMIT;

