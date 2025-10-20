-- Migration: Add Rarity column to SkillTemplates table
-- Date: 2025-10-20
-- Description: Adds Rarity column (int) to SkillTemplates table for gacha system

-- Add Rarity column
ALTER TABLE "SkillTemplates" 
ADD COLUMN "Rarity" integer NOT NULL DEFAULT 0;

-- Add index for rarity-based queries
CREATE INDEX "IX_SkillTemplates_Rarity" ON "SkillTemplates" ("Rarity");

-- Update existing records (if any) with default Common rarity
-- Common = 0, Rare = 1, Epic = 2, Legendary = 3
UPDATE "SkillTemplates" SET "Rarity" = 0 WHERE "Rarity" IS NULL;

-- ========================================
-- Migration: Add Crystals column to Players table
-- Date: 2025-10-20
-- Description: Adds Crystals column (bigint) to Players table for gacha currency
-- ========================================

-- Add Crystals column
ALTER TABLE "Players"
ADD COLUMN "Crystals" bigint NOT NULL DEFAULT 0;

-- Update existing players with default value (0 crystals)
UPDATE "Players" SET "Crystals" = 0 WHERE "Crystals" IS NULL;

-- ========================================
-- Migration: Add Description and Type columns to SkillTemplates table
-- Date: 2025-10-20
-- Description: Extends SkillTemplate with Description and Type for skill details
-- ========================================

-- Add Description column
ALTER TABLE "SkillTemplates"
ADD COLUMN "Description" varchar(500) NOT NULL DEFAULT '';

-- Add Type column (Passive = 1, Active = 2)
ALTER TABLE "SkillTemplates"
ADD COLUMN "Type" integer NOT NULL DEFAULT 2;

-- Add index for type-based queries
CREATE INDEX "IX_SkillTemplates_Type" ON "SkillTemplates" ("Type");

-- Update existing skills with default values
UPDATE "SkillTemplates" SET "Description" = '스킬 설명이 필요합니다.' WHERE "Description" = '';
UPDATE "SkillTemplates" SET "Type" = 2 WHERE "Type" IS NULL; -- Default to Active
