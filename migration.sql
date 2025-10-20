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
