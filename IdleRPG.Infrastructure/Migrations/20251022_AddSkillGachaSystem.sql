-- ============================================
-- Migration: Add Skill Gacha System
-- Date: 2025-10-22
-- Author: Claude
-- Description: 스킬 가챠 시스템 추가 (SkillTemplates, CharacterSkills, GachaHistories)
-- ============================================

BEGIN;

-- ============================================
-- 1. SkillTemplates 테이블 생성
-- ============================================
CREATE TABLE "SkillTemplates" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL,
    "Rarity" INT NOT NULL,
    "Description" TEXT,
    "Type" INT NOT NULL,
    CONSTRAINT "CK_SkillTemplates_Rarity" CHECK ("Rarity" >= 0 AND "Rarity" <= 3),
    CONSTRAINT "CK_SkillTemplates_Type" CHECK ("Type" >= 0 AND "Type" <= 1)
);

CREATE INDEX "IX_SkillTemplates_Rarity" ON "SkillTemplates" ("Rarity");

COMMENT ON TABLE "SkillTemplates" IS '스킬 템플릿 마스터 데이터';
COMMENT ON COLUMN "SkillTemplates"."Rarity" IS '0=Common, 1=Rare, 2=Epic, 3=Legendary';
COMMENT ON COLUMN "SkillTemplates"."Type" IS '0=Active, 1=Passive';

-- ============================================
-- 2. CharacterSkills 테이블 생성
-- ============================================
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

CREATE INDEX "IX_CharacterSkills_CharacterId" ON "CharacterSkills" ("CharacterId");
CREATE INDEX "IX_CharacterSkills_SkillTemplateId" ON "CharacterSkills" ("SkillTemplateId");
CREATE INDEX "IX_CharacterSkills_IsEquipped" ON "CharacterSkills" ("CharacterId", "IsEquipped");

COMMENT ON TABLE "CharacterSkills" IS '캐릭터가 보유한 스킬 목록';
COMMENT ON COLUMN "CharacterSkills"."IsEquipped" IS '스킬 장착 여부';

-- ============================================
-- 3. GachaHistories 테이블 생성
-- ============================================
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

CREATE INDEX "IX_GachaHistories_CharacterId" ON "GachaHistories" ("CharacterId");
CREATE INDEX "IX_GachaHistories_PulledAt" ON "GachaHistories" ("CharacterId", "PulledAt" DESC);

COMMENT ON TABLE "GachaHistories" IS '가챠 히스토리 로그';
COMMENT ON COLUMN "GachaHistories"."WasPityPull" IS '천장 시스템으로 획득했는지 여부';

-- ============================================
-- 4. Characters 테이블에 Crystal, GachaPityCount 컬럼 추가
-- ============================================
ALTER TABLE "Characters"
ADD COLUMN "Crystal" BIGINT NOT NULL DEFAULT 0;

ALTER TABLE "Characters"
ADD COLUMN "GachaPityCount" INT NOT NULL DEFAULT 0;

COMMENT ON COLUMN "Characters"."Crystal" IS '프리미엄 재화 (스킬 가챠 재화)';
COMMENT ON COLUMN "Characters"."GachaPityCount" IS '가챠 천장 카운트 (100회 보장)';

-- ============================================
-- 5. SkillTemplates 시드 데이터 삽입
-- ============================================

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

COMMIT;

-- ============================================
-- ROLLBACK (실행 시 주석 해제)
-- ============================================
-- BEGIN;
-- DROP TABLE IF EXISTS "GachaHistories";
-- DROP TABLE IF EXISTS "CharacterSkills";
-- DROP TABLE IF EXISTS "SkillTemplates";
-- ALTER TABLE "Characters" DROP COLUMN IF EXISTS "Crystal";
-- ALTER TABLE "Characters" DROP COLUMN IF EXISTS "GachaPityCount";
-- COMMIT;
