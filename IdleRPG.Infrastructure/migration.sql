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

-- Migration: 20251022072205_AddSkillRewardSupport
-- Description: LootItem에 ItemTemplateId 컬럼 추가 (int PK 템플릿 참조용)
-- ⚠️ DISABLED: LootItems 테이블이 아직 구현되지 않음 (Dungeon Drop System 미완성)
-- DO $EF$
-- BEGIN
--     IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251022072205_AddSkillRewardSupport') THEN
--     ALTER TABLE "LootItems" ADD COLUMN "ItemTemplateId" INT NULL;
--     END IF;
-- END $EF$;

-- DO $EF$
-- BEGIN
--     IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251022072205_AddSkillRewardSupport') THEN
--     INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
--     VALUES ('20251022072205_AddSkillRewardSupport', '9.0.9');
--     END IF;
-- END $EF$;

-- Migration: 20251027120000_AddPetSystem
-- Description: 펫 시스템 테이블 추가 (Pets, PetTemplates, EquippedPets, Characters.PetGachaCount)

-- 1. CREATE TABLE PetTemplates (마스터 데이터)
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027120000_AddPetSystem') THEN
    CREATE TABLE "PetTemplates" (
        "Id" SERIAL PRIMARY KEY,
        "Name" VARCHAR(50) NOT NULL,
        "Rarity" INT NOT NULL CHECK ("Rarity" BETWEEN 0 AND 3),
        "BaseAttack" INT NOT NULL CHECK ("BaseAttack" >= 0),
        "BaseMana" INT NOT NULL CHECK ("BaseMana" >= 0),
        CONSTRAINT "UK_PetTemplates_Name" UNIQUE ("Name")
    );
    END IF;
END $EF$;

-- 2. CREATE INDEX IX_PetTemplates_Rarity
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027120000_AddPetSystem') THEN
    CREATE INDEX "IX_PetTemplates_Rarity" ON "PetTemplates" ("Rarity");
    END IF;
END $EF$;

-- 3. CREATE TABLE Pets
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027120000_AddPetSystem') THEN
    CREATE TABLE "Pets" (
        "Id" SERIAL PRIMARY KEY,
        "CharacterId" UUID NOT NULL,
        "TemplateId" INT NOT NULL,
        "Level" INT NOT NULL DEFAULT 1 CHECK ("Level" BETWEEN 1 AND 50),
        "CurrentAttack" INT NOT NULL CHECK ("CurrentAttack" >= 0),
        "CurrentMana" INT NOT NULL CHECK ("CurrentMana" >= 0),
        "CreatedAt" TIMESTAMP NOT NULL DEFAULT NOW(),
        "UpdatedAt" TIMESTAMP NOT NULL DEFAULT NOW(),
        CONSTRAINT "FK_Pets_Characters_CharacterId" FOREIGN KEY ("CharacterId") REFERENCES "Characters" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_Pets_PetTemplates_TemplateId" FOREIGN KEY ("TemplateId") REFERENCES "PetTemplates" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

-- 4. CREATE INDEX IX_Pets_CharacterId
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027120000_AddPetSystem') THEN
    CREATE INDEX "IX_Pets_CharacterId" ON "Pets" ("CharacterId");
    END IF;
END $EF$;

-- 5. CREATE TABLE EquippedPets (Composite PK)
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027120000_AddPetSystem') THEN
    CREATE TABLE "EquippedPets" (
        "CharacterId" UUID NOT NULL,
        "SlotIndex" INT NOT NULL CHECK ("SlotIndex" BETWEEN 1 AND 3),
        "PetId" INT NOT NULL,
        "EquippedAt" TIMESTAMP NOT NULL DEFAULT NOW(),
        CONSTRAINT "PK_EquippedPets" PRIMARY KEY ("CharacterId", "SlotIndex"),
        CONSTRAINT "UK_EquippedPets_PetId" UNIQUE ("PetId"),
        CONSTRAINT "FK_EquippedPets_Characters_CharacterId" FOREIGN KEY ("CharacterId") REFERENCES "Characters" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_EquippedPets_Pets_PetId" FOREIGN KEY ("PetId") REFERENCES "Pets" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

-- 6. ALTER TABLE Characters ADD COLUMN PetGachaCount
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027120000_AddPetSystem') THEN
    ALTER TABLE "Characters" ADD COLUMN "PetGachaCount" INT NOT NULL DEFAULT 0 CHECK ("PetGachaCount" BETWEEN 0 AND 50);
    END IF;
END $EF$;

-- 7. INSERT Migration History
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027120000_AddPetSystem') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251027120000_AddPetSystem', '9.0.9');
    END IF;
END $EF$;

-- ============================================
-- Migration: Add Realtime Chat System
-- Date: 2025-10-30
-- Description: 실시간 채팅 시스템 (Global/Guild/Whisper), SignalR 통합
-- ============================================

-- 1. CREATE TABLE ChatRooms
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251030000000_AddRealtimeChatSystem') THEN
    CREATE TABLE "ChatRooms" (
        "Id" UUID NOT NULL,
        "Type" INT NOT NULL,
        "Name" VARCHAR(100) NOT NULL,
        "GuildId" UUID NULL,
        "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        CONSTRAINT "PK_ChatRooms" PRIMARY KEY ("Id"),
        CONSTRAINT "CK_ChatRooms_Type" CHECK ("Type" BETWEEN 1 AND 3)
    );
    END IF;
END $EF$;

-- 2. CREATE TABLE ChatRoomParticipants
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251030000000_AddRealtimeChatSystem') THEN
    CREATE TABLE "ChatRoomParticipants" (
        "Id" UUID NOT NULL,
        "RoomId" UUID NOT NULL,
        "CharacterId" UUID NOT NULL,
        "JoinedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        CONSTRAINT "PK_ChatRoomParticipants" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ChatRoomParticipants_Rooms_RoomId" FOREIGN KEY ("RoomId") REFERENCES "ChatRooms" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_ChatRoomParticipants_Characters_CharacterId" FOREIGN KEY ("CharacterId") REFERENCES "Characters" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

-- 3. CREATE TABLE ChatMessages
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251030000000_AddRealtimeChatSystem') THEN
    CREATE TABLE "ChatMessages" (
        "Id" UUID NOT NULL,
        "RoomId" UUID NOT NULL,
        "SenderId" UUID NOT NULL,
        "Content" VARCHAR(1000) NOT NULL,
        "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        CONSTRAINT "PK_ChatMessages" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ChatMessages_Rooms_RoomId" FOREIGN KEY ("RoomId") REFERENCES "ChatRooms" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_ChatMessages_Characters_SenderId" FOREIGN KEY ("SenderId") REFERENCES "Characters" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

-- 4. CREATE INDEX IX_ChatRooms_Type
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251030000000_AddRealtimeChatSystem') THEN
    CREATE INDEX "IX_ChatRooms_Type" ON "ChatRooms" ("Type");
    END IF;
END $EF$;

-- 5. CREATE INDEX IX_ChatRooms_GuildId
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251030000000_AddRealtimeChatSystem') THEN
    CREATE INDEX "IX_ChatRooms_GuildId" ON "ChatRooms" ("GuildId") WHERE "GuildId" IS NOT NULL;
    END IF;
END $EF$;

-- 6. CREATE INDEX IX_ChatRoomParticipants_RoomId
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251030000000_AddRealtimeChatSystem') THEN
    CREATE INDEX "IX_ChatRoomParticipants_RoomId" ON "ChatRoomParticipants" ("RoomId");
    END IF;
END $EF$;

-- 7. CREATE INDEX IX_ChatRoomParticipants_CharacterId
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251030000000_AddRealtimeChatSystem') THEN
    CREATE INDEX "IX_ChatRoomParticipants_CharacterId" ON "ChatRoomParticipants" ("CharacterId");
    END IF;
END $EF$;

-- 8. CREATE UNIQUE INDEX IX_ChatRoomParticipants_RoomId_CharacterId
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251030000000_AddRealtimeChatSystem') THEN
    CREATE UNIQUE INDEX "IX_ChatRoomParticipants_RoomId_CharacterId" ON "ChatRoomParticipants" ("RoomId", "CharacterId");
    END IF;
END $EF$;

-- 9. CREATE INDEX IX_ChatMessages_RoomId_CreatedAt (복합 인덱스, Cursor 페이징 최적화)
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251030000000_AddRealtimeChatSystem') THEN
    CREATE INDEX "IX_ChatMessages_RoomId_CreatedAt" ON "ChatMessages" ("RoomId", "CreatedAt" DESC);
    END IF;
END $EF$;

-- 10. INSERT Seed Data: Global ChatRoom
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251030000000_AddRealtimeChatSystem') THEN
    INSERT INTO "ChatRooms" ("Id", "Type", "Name", "GuildId", "CreatedAt")
    VALUES ('00000000-0000-0000-0000-000000000001', 1, '전체 채팅', NULL, TIMESTAMPTZ '2025-10-30T00:00:00Z');
    END IF;
END $EF$;

-- 11. INSERT Migration History
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251030000000_AddRealtimeChatSystem') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251030000000_AddRealtimeChatSystem', '9.0.9');
    END IF;
END $EF$;

-- ============================================
-- Migration: Add Dungeon System and Loot Table Pattern
-- Date: 2025-10-17
-- Description: 던전 템플릿, 난이도, 보상 테이블 시스템 추가
-- ============================================

-- 1. CREATE TABLE DungeonTemplates
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251017042133_AddDungeonSystemAndLootTablePattern') THEN
    CREATE TABLE "DungeonTemplates" (
        "Id" SERIAL PRIMARY KEY,
        "Name" VARCHAR(100) NOT NULL,
        "Description" VARCHAR(500),
        "Category" VARCHAR(50) NOT NULL,
        "MinLevel" INT NOT NULL,
        "IsEnabled" BOOLEAN NOT NULL DEFAULT TRUE
    );
    END IF;
END $EF$;

-- 2. CREATE TABLE ItemTemplates
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251017042133_AddDungeonSystemAndLootTablePattern') THEN
    CREATE TABLE "ItemTemplates" (
        "Id" UUID PRIMARY KEY,
        "Name" VARCHAR(100) NOT NULL,
        "Description" VARCHAR(500),
        "Type" VARCHAR(50) NOT NULL,
        "IconUrl" VARCHAR(500),
        "MaxStackSize" INT NOT NULL DEFAULT 999
    );
    END IF;
END $EF$;

-- 3. CREATE TABLE LootTables
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251017042133_AddDungeonSystemAndLootTablePattern') THEN
    CREATE TABLE "LootTables" (
        "Id" SERIAL PRIMARY KEY,
        "Name" VARCHAR(100) NOT NULL,
        "NumberOfRolls" INT NOT NULL DEFAULT 1
    );
    END IF;
END $EF$;

-- 4. CREATE TABLE UserDungeonDailies
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251017042133_AddDungeonSystemAndLootTablePattern') THEN
    CREATE TABLE "UserDungeonDailies" (
        "Id" UUID PRIMARY KEY,
        "UserId" UUID NOT NULL,
        "DungeonTemplateId" INT NOT NULL,
        "DifficultyCode" VARCHAR(50) NOT NULL,
        "EntryCount" INT NOT NULL DEFAULT 0,
        "Date" DATE NOT NULL,
        CONSTRAINT "FK_UserDungeonDailies_DungeonTemplates_DungeonTemplateId" FOREIGN KEY ("DungeonTemplateId")
            REFERENCES "DungeonTemplates"("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_UserDungeonDailies_Players_UserId" FOREIGN KEY ("UserId")
            REFERENCES "Players"("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

-- 5. CREATE TABLE PlayerItems
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251017042133_AddDungeonSystemAndLootTablePattern') THEN
    CREATE TABLE "PlayerItems" (
        "Id" UUID PRIMARY KEY,
        "CharacterId" UUID NOT NULL,
        "ItemTemplateId" UUID NOT NULL,
        "Quantity" INT NOT NULL,
        "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
        "UpdatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
        CONSTRAINT "FK_PlayerItems_Characters_CharacterId" FOREIGN KEY ("CharacterId")
            REFERENCES "Characters"("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_PlayerItems_ItemTemplates_ItemTemplateId" FOREIGN KEY ("ItemTemplateId")
            REFERENCES "ItemTemplates"("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

-- 6. CREATE TABLE DungeonDifficulties
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251017042133_AddDungeonSystemAndLootTablePattern') THEN
    CREATE TABLE "DungeonDifficulties" (
        "Id" SERIAL PRIMARY KEY,
        "TemplateId" INT NOT NULL,
        "Code" VARCHAR(50) NOT NULL,
        "RecommendedPower" INT NOT NULL,
        "BaseGold" BIGINT NOT NULL,
        "BaseExp" INT NOT NULL,
        "LootTableId" INT,
        "MaxWaves" INT NOT NULL,
        "DailyEntryLimit" INT NOT NULL DEFAULT 3,
        "EntryCostGold" INT NOT NULL DEFAULT 0,
        CONSTRAINT "FK_DungeonDifficulties_DungeonTemplates_TemplateId" FOREIGN KEY ("TemplateId")
            REFERENCES "DungeonTemplates"("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_DungeonDifficulties_LootTables_LootTableId" FOREIGN KEY ("LootTableId")
            REFERENCES "LootTables"("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

-- 7. CREATE TABLE LootItems
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251017042133_AddDungeonSystemAndLootTablePattern') THEN
    CREATE TABLE "LootItems" (
        "Id" SERIAL PRIMARY KEY,
        "LootTableId" INT NOT NULL,
        "Type" VARCHAR(50) NOT NULL,
        "ItemId" UUID,
        "IsGuaranteed" BOOLEAN NOT NULL DEFAULT FALSE,
        "Weight" INT NOT NULL,
        "MinQuantity" INT NOT NULL DEFAULT 1,
        "MaxQuantity" INT NOT NULL DEFAULT 1,
        CONSTRAINT "FK_LootItems_LootTables_LootTableId" FOREIGN KEY ("LootTableId")
            REFERENCES "LootTables"("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

-- 8. CREATE TABLE DungeonProgresses
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251017042133_AddDungeonSystemAndLootTablePattern') THEN
    CREATE TABLE "DungeonProgresses" (
        "Id" UUID PRIMARY KEY,
        "CharacterId" UUID NOT NULL,
        "DifficultyId" INT NOT NULL,
        "CurrentWave" INT NOT NULL DEFAULT 1,
        "CurrentHealth" INT NOT NULL,
        "StartedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
        CONSTRAINT "FK_DungeonProgresses_Characters_CharacterId" FOREIGN KEY ("CharacterId")
            REFERENCES "Characters"("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_DungeonProgresses_DungeonDifficulties_DifficultyId" FOREIGN KEY ("DifficultyId")
            REFERENCES "DungeonDifficulties"("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

-- 9. CREATE TABLE DungeonRunHistories
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251017042133_AddDungeonSystemAndLootTablePattern') THEN
    CREATE TABLE "DungeonRunHistories" (
        "Id" UUID PRIMARY KEY,
        "CharacterId" UUID NOT NULL,
        "DifficultyId" INT NOT NULL,
        "IsCleared" BOOLEAN NOT NULL,
        "ClearedWave" INT NOT NULL,
        "CompletedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
        CONSTRAINT "FK_DungeonRunHistories_Characters_CharacterId" FOREIGN KEY ("CharacterId")
            REFERENCES "Characters"("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_DungeonRunHistories_DungeonDifficulties_DifficultyId" FOREIGN KEY ("DifficultyId")
            REFERENCES "DungeonDifficulties"("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

-- 10. CREATE TABLE DungeonWaves
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251017042133_AddDungeonSystemAndLootTablePattern') THEN
    CREATE TABLE "DungeonWaves" (
        "Id" SERIAL PRIMARY KEY,
        "DifficultyId" INT NOT NULL,
        "WaveNumber" INT NOT NULL,
        "MonsterId" UUID NOT NULL,
        CONSTRAINT "FK_DungeonWaves_DungeonDifficulties_DifficultyId" FOREIGN KEY ("DifficultyId")
            REFERENCES "DungeonDifficulties"("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_DungeonWaves_Monsters_MonsterId" FOREIGN KEY ("MonsterId")
            REFERENCES "Monsters"("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

-- 11. CREATE INDEXES
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251017042133_AddDungeonSystemAndLootTablePattern') THEN
    CREATE INDEX "IX_DungeonDifficulties_LootTableId" ON "DungeonDifficulties" ("LootTableId");
    CREATE INDEX "IX_DungeonDifficulties_TemplateId" ON "DungeonDifficulties" ("TemplateId");
    CREATE UNIQUE INDEX "IX_DungeonProgresses_CharacterId" ON "DungeonProgresses" ("CharacterId");
    CREATE INDEX "IX_DungeonProgresses_DifficultyId" ON "DungeonProgresses" ("DifficultyId");
    CREATE INDEX "IX_DungeonRunHistories_CharacterId_CompletedAt" ON "DungeonRunHistories" ("CharacterId", "CompletedAt");
    CREATE INDEX "IX_DungeonRunHistories_DifficultyId" ON "DungeonRunHistories" ("DifficultyId");
    CREATE INDEX "IX_DungeonWaves_DifficultyId" ON "DungeonWaves" ("DifficultyId");
    CREATE INDEX "IX_DungeonWaves_MonsterId" ON "DungeonWaves" ("MonsterId");
    CREATE INDEX "IX_LootItems_LootTableId" ON "LootItems" ("LootTableId");
    CREATE INDEX "IX_PlayerItems_CharacterId" ON "PlayerItems" ("CharacterId");
    CREATE INDEX "IX_PlayerItems_ItemTemplateId" ON "PlayerItems" ("ItemTemplateId");
    CREATE INDEX "IX_UserDungeonDailies_DungeonTemplateId" ON "UserDungeonDailies" ("DungeonTemplateId");
    CREATE UNIQUE INDEX "IX_UserDungeonDailies_UserId_DungeonTemplateId_DifficultyCode_~" ON "UserDungeonDailies" ("UserId", "DungeonTemplateId", "DifficultyCode", "Date");
    END IF;
END $EF$;

-- 12. INSERT Migration History
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251017042133_AddDungeonSystemAndLootTablePattern') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251017042133_AddDungeonSystemAndLootTablePattern', '9.0.9');
    END IF;
END $EF$;

-- ============================================
-- Migration: Add Dungeon Stage System
-- Date: 2025-10-18
-- Description: Main Battle용 DungeonStage 시스템 (간소화된 던전)
-- ============================================

-- 1. ALTER TABLE BattleLogs ADD COLUMN DungeonStageId
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251018112141_AddDungeonStageSystem') THEN
    ALTER TABLE "BattleLogs" ADD COLUMN "DungeonStageId" INT NULL;
    END IF;
END $EF$;

-- 2. CREATE TABLE CharacterBattleProgresses
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251018112141_AddDungeonStageSystem') THEN
    CREATE TABLE "CharacterBattleProgresses" (
        "Id" UUID PRIMARY KEY,
        "CharacterId" UUID NOT NULL,
        "HighestStageClearedNormal" INT NOT NULL DEFAULT 0,
        "HighestStageClearedHard" INT NOT NULL DEFAULT 0,
        "HighestStageClearedNightmare" INT NOT NULL DEFAULT 0,
        "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL,
        "UpdatedAt" TIMESTAMP WITH TIME ZONE NOT NULL,
        CONSTRAINT "FK_CharacterBattleProgresses_Characters_CharacterId" FOREIGN KEY ("CharacterId")
            REFERENCES "Characters"("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

-- 3. CREATE TABLE DungeonStages
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251018112141_AddDungeonStageSystem') THEN
    CREATE TABLE "DungeonStages" (
        "Id" SERIAL PRIMARY KEY,
        "Name" VARCHAR(100) NOT NULL,
        "RequiredLevel" INT NOT NULL,
        "MonsterId" UUID NOT NULL,
        "BaseExperience" INT NOT NULL,
        "BaseGold" INT NOT NULL,
        "FirstClearBonusExp" INT,
        "FirstClearBonusGold" INT,
        "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL,
        CONSTRAINT "FK_DungeonStages_Monsters_MonsterId" FOREIGN KEY ("MonsterId")
            REFERENCES "Monsters"("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

-- 4. CREATE INDEXES
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251018112141_AddDungeonStageSystem') THEN
    CREATE UNIQUE INDEX "IX_CharacterBattleProgresses_CharacterId_Unique" ON "CharacterBattleProgresses" ("CharacterId");
    CREATE INDEX "IX_DungeonStages_MonsterId" ON "DungeonStages" ("MonsterId");
    CREATE INDEX "IX_DungeonStages_RequiredLevel" ON "DungeonStages" ("RequiredLevel");
    END IF;
END $EF$;

-- 5. INSERT Migration History
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251018112141_AddDungeonStageSystem') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251018112141_AddDungeonStageSystem', '9.0.9');
    END IF;
END $EF$;

-- ============================================
-- Migration: Add LootTable To DungeonStage
-- Date: 2025-10-22
-- Description: DungeonStage에 보상 테이블 연결 (LootTableId 추가)
-- ============================================

-- 1. ALTER TABLE DungeonStages ADD COLUMN LootTableId
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251022062932_AddLootTableToDungeonStage') THEN
    ALTER TABLE "DungeonStages" ADD COLUMN "LootTableId" INT NULL;
    END IF;
END $EF$;

-- 2. CREATE INDEX IX_DungeonStages_LootTableId
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251022062932_AddLootTableToDungeonStage') THEN
    CREATE INDEX "IX_DungeonStages_LootTableId" ON "DungeonStages" ("LootTableId");
    END IF;
END $EF$;

-- 3. ADD FOREIGN KEY CONSTRAINT
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251022062932_AddLootTableToDungeonStage') THEN
    ALTER TABLE "DungeonStages" ADD CONSTRAINT "FK_DungeonStages_LootTables_LootTableId"
        FOREIGN KEY ("LootTableId") REFERENCES "LootTables"("Id");
    END IF;
END $EF$;

-- 4. INSERT Migration History
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251022062932_AddLootTableToDungeonStage') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251022062932_AddLootTableToDungeonStage', '9.0.9');
    END IF;
END $EF$;

-- ============================================
-- Migration: Rename CharacterDungeonProgresses to CharacterBattleProgresses
-- Date: 2025-11-06
-- Description: 테이블 이름을 CharacterDungeonProgresses에서 CharacterBattleProgresses로 변경
-- ============================================

-- 1. RENAME TABLE (기존 데이터 보존)
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251106000000_RenameCharacterDungeonProgressesToBattleProgresses') THEN
        -- 테이블 이름 변경 (기존 데이터 유지)
        IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'CharacterDungeonProgresses') THEN
            ALTER TABLE "CharacterDungeonProgresses" RENAME TO "CharacterBattleProgresses";
        END IF;
    END IF;
END $EF$;

-- 2. RENAME INDEX
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251106000000_RenameCharacterDungeonProgressesToBattleProgresses') THEN
        -- 인덱스 이름 변경
        IF EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_CharacterDungeonProgresses_CharacterId_Unique') THEN
            ALTER INDEX "IX_CharacterDungeonProgresses_CharacterId_Unique"
            RENAME TO "IX_CharacterBattleProgresses_CharacterId_Unique";
        END IF;
    END IF;
END $EF$;

-- 3. RENAME FOREIGN KEY CONSTRAINT
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251106000000_RenameCharacterDungeonProgressesToBattleProgresses') THEN
        -- Foreign Key Constraint 이름 변경
        IF EXISTS (SELECT 1 FROM information_schema.table_constraints
                   WHERE constraint_name = 'FK_CharacterDungeonProgresses_Characters_CharacterId') THEN
            ALTER TABLE "CharacterBattleProgresses"
            RENAME CONSTRAINT "FK_CharacterDungeonProgresses_Characters_CharacterId"
            TO "FK_CharacterBattleProgresses_Characters_CharacterId";
        END IF;
    END IF;
END $EF$;

-- 4. INSERT Migration History
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251106000000_RenameCharacterDungeonProgressesToBattleProgresses') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251106000000_RenameCharacterDungeonProgressesToBattleProgresses', '9.0.9');
    END IF;
END $EF$;

-- ============================================
-- Migration: Add PVP Arena System
-- Date: 2025-11-08
-- Description: PVP 매칭, 랭킹, 시즌 시스템 (Redis 캐싱, ELO 레이팅)
-- ============================================

-- 1. CREATE TABLE PvpSeason (시즌 마스터 데이터)
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251108000000_AddPvpArenaSystem') THEN
    CREATE TABLE "PvpSeasons" (
        "Id" SERIAL PRIMARY KEY,
        "SeasonNumber" INT NOT NULL,
        "StartDate" TIMESTAMP WITH TIME ZONE NOT NULL,
        "EndDate" TIMESTAMP WITH TIME ZONE NOT NULL,
        "IsActive" BOOLEAN NOT NULL DEFAULT FALSE,
        "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
        "UpdatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
        CONSTRAINT "CK_PvpSeasons_DateRange" CHECK ("StartDate" < "EndDate"),
        CONSTRAINT "UK_PvpSeasons_SeasonNumber" UNIQUE ("SeasonNumber")
    );
    END IF;
END $EF$;

-- 2. CREATE INDEX IX_PvpSeasons_IsActive
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251108000000_AddPvpArenaSystem') THEN
    CREATE INDEX "IX_PvpSeasons_IsActive" ON "PvpSeasons" ("IsActive");
    END IF;
END $EF$;

-- 3. CREATE INDEX IX_PvpSeasons_SeasonNumber (Unique Index는 UK 제약으로 자동 생성되지만 명시)
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251108000000_AddPvpArenaSystem') THEN
    -- Note: Unique constraint UK_PvpSeasons_SeasonNumber already creates an index
    END IF;
END $EF$;

-- 4. CREATE TABLE PvpRanking (시즌별 랭킹 정보, Composite PK)
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251108000000_AddPvpArenaSystem') THEN
    CREATE TABLE "PvpRankings" (
        "SeasonId" INT NOT NULL,
        "CharacterId" UUID NOT NULL,
        "Rating" INT NOT NULL DEFAULT 1000,
        "Wins" INT NOT NULL DEFAULT 0,
        "Losses" INT NOT NULL DEFAULT 0,
        "WinStreak" INT NOT NULL DEFAULT 0,
        "Tier" VARCHAR(20) GENERATED ALWAYS AS (
            CASE
                WHEN "Rating" >= 2500 THEN 'Diamond'
                WHEN "Rating" >= 2000 THEN 'Platinum'
                WHEN "Rating" >= 1500 THEN 'Gold'
                WHEN "Rating" >= 1000 THEN 'Silver'
                ELSE 'Bronze'
            END
        ) STORED,
        "IsRewardClaimed" BOOLEAN NOT NULL DEFAULT FALSE,
        "LastMatchAt" TIMESTAMP WITH TIME ZONE,
        "UpdatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
        CONSTRAINT "PK_PvpRankings" PRIMARY KEY ("SeasonId", "CharacterId"),
        CONSTRAINT "CK_PvpRankings_Rating" CHECK ("Rating" >= 0),
        CONSTRAINT "CK_PvpRankings_Wins" CHECK ("Wins" >= 0),
        CONSTRAINT "CK_PvpRankings_Losses" CHECK ("Losses" >= 0),
        CONSTRAINT "CK_PvpRankings_WinStreak" CHECK ("WinStreak" >= 0),
        CONSTRAINT "FK_PvpRankings_PvpSeasons_SeasonId" FOREIGN KEY ("SeasonId")
            REFERENCES "PvpSeasons"("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_PvpRankings_Characters_CharacterId" FOREIGN KEY ("CharacterId")
            REFERENCES "Characters"("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

-- 5. CREATE INDEX IX_PvpRankings_SeasonId_Rating_DESC (복합 인덱스, Top 100 조회 최적화)
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251108000000_AddPvpArenaSystem') THEN
    CREATE INDEX "IX_PvpRankings_SeasonId_Rating_DESC" ON "PvpRankings" ("SeasonId" ASC, "Rating" DESC);
    END IF;
END $EF$;

-- 6. CREATE TABLE PvpMatch (매치 기록)
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251108000000_AddPvpArenaSystem') THEN
    CREATE TABLE "PvpMatches" (
        "Id" UUID PRIMARY KEY,
        "SeasonId" INT NOT NULL,
        "AttackerId" UUID NOT NULL,
        "DefenderId" UUID NOT NULL,
        "WinnerId" UUID NOT NULL,
        "AttackerRatingBefore" INT NOT NULL,
        "AttackerRatingAfter" INT NOT NULL,
        "DefenderRatingBefore" INT NOT NULL,
        "DefenderRatingAfter" INT NOT NULL,
        "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
        CONSTRAINT "CK_PvpMatches_AttackerDefenderDifferent" CHECK ("AttackerId" != "DefenderId"),
        CONSTRAINT "CK_PvpMatches_Ratings" CHECK (
            "AttackerRatingBefore" >= 0 AND "AttackerRatingAfter" >= 0 AND
            "DefenderRatingBefore" >= 0 AND "DefenderRatingAfter" >= 0
        ),
        CONSTRAINT "FK_PvpMatches_PvpSeasons_SeasonId" FOREIGN KEY ("SeasonId")
            REFERENCES "PvpSeasons"("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_PvpMatches_Characters_AttackerId" FOREIGN KEY ("AttackerId")
            REFERENCES "Characters"("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_PvpMatches_Characters_DefenderId" FOREIGN KEY ("DefenderId")
            REFERENCES "Characters"("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_PvpMatches_Characters_WinnerId" FOREIGN KEY ("WinnerId")
            REFERENCES "Characters"("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

-- 7. CREATE INDEX IX_PvpMatches_AttackerId_CreatedAt (공격자 전적 조회 최적화)
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251108000000_AddPvpArenaSystem') THEN
    CREATE INDEX "IX_PvpMatches_AttackerId_CreatedAt" ON "PvpMatches" ("AttackerId", "CreatedAt" DESC);
    END IF;
END $EF$;

-- 8. CREATE INDEX IX_PvpMatches_DefenderId_CreatedAt (방어자 전적 조회 최적화)
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251108000000_AddPvpArenaSystem') THEN
    CREATE INDEX "IX_PvpMatches_DefenderId_CreatedAt" ON "PvpMatches" ("DefenderId", "CreatedAt" DESC);
    END IF;
END $EF$;

-- 9. CREATE INDEX IX_PvpMatches_SeasonId (시즌별 필터링)
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251108000000_AddPvpArenaSystem') THEN
    CREATE INDEX "IX_PvpMatches_SeasonId" ON "PvpMatches" ("SeasonId");
    END IF;
END $EF$;

-- 10. INSERT Migration History
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251108000000_AddPvpArenaSystem') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251108000000_AddPvpArenaSystem', '9.0.9');
    END IF;
END $EF$;

COMMIT;

-- ================================================================
-- Migration: 20251111000000_RemovePvpRankingTierColumn
-- Description: Remove Tier generated column from PvpRanking table
--              Tier is now computed at application level (Rating-based)
-- ================================================================

START TRANSACTION;

-- 1. DROP COLUMN Tier (Generated Column)
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251111000000_RemovePvpRankingTierColumn') THEN
        IF EXISTS (
            SELECT 1 FROM information_schema.columns
            WHERE table_name = 'PvpRankings' AND column_name = 'Tier'
        ) THEN
            ALTER TABLE "PvpRankings" DROP COLUMN "Tier";
        END IF;
    END IF;
END $EF$;

-- 2. INSERT Migration History
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251111000000_RemovePvpRankingTierColumn') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251111000000_RemovePvpRankingTierColumn', '9.0.9');
    END IF;
END $EF$;

COMMIT;

