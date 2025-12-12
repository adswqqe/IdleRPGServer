-- ============================================
-- Migration: Rename CharacterDungeonProgresses to CharacterBattleProgresses
-- Date: 2025-11-06
-- Description: 테이블 이름을 CharacterDungeonProgresses에서 CharacterBattleProgresses로 변경
--
-- 이 스크립트는 기존 데이터를 보존하면서 테이블 이름을 변경합니다.
-- Idempotent: 여러 번 실행해도 안전합니다.
-- ============================================

BEGIN;

-- 1. RENAME TABLE (기존 데이터 보존)
DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251106000000_RenameCharacterDungeonProgressesToBattleProgresses') THEN
        -- 테이블 이름 변경 (기존 데이터 유지)
        IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'CharacterDungeonProgresses') THEN
            RAISE NOTICE 'Renaming table CharacterDungeonProgresses to CharacterBattleProgresses...';
            ALTER TABLE "CharacterDungeonProgresses" RENAME TO "CharacterBattleProgresses";
            RAISE NOTICE 'Table renamed successfully.';
        ELSE
            RAISE NOTICE 'Table CharacterDungeonProgresses does not exist. Skipping rename.';
        END IF;
    ELSE
        RAISE NOTICE 'Migration already applied. Skipping.';
    END IF;
END $$;

-- 2. RENAME INDEX
DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251106000000_RenameCharacterDungeonProgressesToBattleProgresses') THEN
        -- 인덱스 이름 변경
        IF EXISTS (SELECT 1 FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'IX_CharacterDungeonProgresses_CharacterId_Unique') THEN
            RAISE NOTICE 'Renaming index IX_CharacterDungeonProgresses_CharacterId_Unique...';
            ALTER INDEX "IX_CharacterDungeonProgresses_CharacterId_Unique"
            RENAME TO "IX_CharacterBattleProgresses_CharacterId_Unique";
            RAISE NOTICE 'Index renamed successfully.';
        ELSE
            RAISE NOTICE 'Index IX_CharacterDungeonProgresses_CharacterId_Unique does not exist. Skipping rename.';
        END IF;
    END IF;
END $$;

-- 3. RENAME FOREIGN KEY CONSTRAINT
DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251106000000_RenameCharacterDungeonProgressesToBattleProgresses') THEN
        -- Foreign Key Constraint 이름 변경
        IF EXISTS (SELECT 1 FROM information_schema.table_constraints
                   WHERE constraint_schema = 'public'
                   AND constraint_name = 'FK_CharacterDungeonProgresses_Characters_CharacterId') THEN
            RAISE NOTICE 'Renaming foreign key constraint...';
            ALTER TABLE "CharacterBattleProgresses"
            RENAME CONSTRAINT "FK_CharacterDungeonProgresses_Characters_CharacterId"
            TO "FK_CharacterBattleProgresses_Characters_CharacterId";
            RAISE NOTICE 'Foreign key constraint renamed successfully.';
        ELSE
            RAISE NOTICE 'Foreign key constraint does not exist. Skipping rename.';
        END IF;
    END IF;
END $$;

-- 4. INSERT Migration History
DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251106000000_RenameCharacterDungeonProgressesToBattleProgresses') THEN
        RAISE NOTICE 'Recording migration in history...';
        INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
        VALUES ('20251106000000_RenameCharacterDungeonProgressesToBattleProgresses', '9.0.9');
        RAISE NOTICE 'Migration recorded successfully.';
    END IF;
END $$;

COMMIT;

-- 검증 쿼리: 마이그레이션 결과 확인
SELECT
    'CharacterBattleProgresses' as expected_table,
    CASE
        WHEN EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'CharacterBattleProgresses')
        THEN 'EXISTS ✓'
        ELSE 'NOT FOUND ✗'
    END as status;

SELECT
    'IX_CharacterBattleProgresses_CharacterId_Unique' as expected_index,
    CASE
        WHEN EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_CharacterBattleProgresses_CharacterId_Unique')
        THEN 'EXISTS ✓'
        ELSE 'NOT FOUND ✗'
    END as status;

SELECT
    'FK_CharacterBattleProgresses_Characters_CharacterId' as expected_constraint,
    CASE
        WHEN EXISTS (SELECT 1 FROM information_schema.table_constraints
                     WHERE constraint_name = 'FK_CharacterBattleProgresses_Characters_CharacterId')
        THEN 'EXISTS ✓'
        ELSE 'NOT FOUND ✗'
    END as status;
