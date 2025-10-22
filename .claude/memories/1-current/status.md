# 현재 구현 상태

**업데이트**: 2025-10-22

---

## API 엔드포인트 (25개)

### Authentication (5개)
- POST /api/auth/register
- POST /api/auth/login
- POST /api/auth/refresh
- POST /api/auth/logout
- GET /api/auth/profile

### Character (5개)
- POST /api/character/Create
- GET /api/character/GetCharacters
- GET /api/character/{characterId}
- DELETE /api/character/{characterId}
- POST /api/character/{characterId}/experience

### Battle (3개)
- POST /api/battle/start
- GET /api/battle/logs/{characterId}
- GET /api/battle/stats/{characterId}

### Monster (1개)
- GET /api/monster/random

### Reward (2개)
- GET /api/reward/offline/{characterId}
- POST /api/reward/offline/claim

### Equipment (7개)
- POST /api/equipment/create
- GET /api/equipment/equipped/{characterId}
- GET /api/equipment/inventory/{ownerId}
- POST /api/equipment/equip
- POST /api/equipment/unequip
- POST /api/equipment/enhance
- DELETE /api/equipment/{equipmentId}

### Dungeon (3개)
- GET /api/dungeons/stages
- GET /api/dungeons/progress
- POST /api/dungeons/clear

### Skill (1개)
- POST /api/skills/gacha

---

## Database Tables (12개)

### Core Tables (6개)
- **Players**: 플레이어 계정 (GUID PK)
- **RefreshTokens**: JWT Refresh Token (GUID PK, FK → Players)
- **Characters**: 게임 캐릭터 (GUID PK, FK → Players) + Crystal, GachaPityCount
- **BattleLogs**: 전투 로그 (GUID PK, FK → Characters, Monsters, DungeonStages)
- **OfflineRewardTypes**: 오프라인 보상 타입 (int PK)
- **Monsters**: 몬스터 템플릿 (int PK)

### Equipment System (1개)
- **Equipments**: 장비 (GUID PK, FK → Players, Characters)

### Dungeon System (2개)
- **DungeonStages**: 던전 스테이지 (int PK, FK → Monsters)
- **CharacterDungeonProgresses**: 던전 진행도 (GUID PK, FK → Characters)

### Skill System (3개)
- **SkillTemplates**: 스킬 마스터 데이터 (int PK) - 24개 스킬
- **CharacterSkills**: 캐릭터 보유 스킬 (GUID PK, FK → Characters, SkillTemplates)
- **GachaHistories**: 가챠 히스토리 (GUID PK, FK → Characters, SkillTemplates)

---

## Unity 문서화 체크리스트 ⚠️ CRITICAL

**언제**: API/DTO 추가/수정 시 반드시 실행

1. **폴더**: `../IdleRPGClient/Docs/unity/{feature}/`
2. **API_SPEC.md**: Request/Response 예제 + Unity C# 코드
3. **DTOs.cs**: `[Serializable]`, `[JsonProperty]` (Newtonsoft.Json)
4. **README.md**: 구현 상태 테이블 업데이트

---

## 완료된 기능 (Week 3 Day 2)

1. ✅ **인증 시스템**: JWT (Access 15분, Refresh 7일), BCrypt
2. ✅ **캐릭터 성장**: 레벨업, 경험치 공식 `RequiredExp = 100 * (Level ^ 1.5)`
3. ✅ **전투 시스템**: 서버 기반 턴제, 전투 로그
4. ✅ **오프라인 보상**: `CharacterLevel * OfflineHours * 10` (최대 12시간)
5. ✅ **장비 시스템**: 5슬롯, 강화 (+0~+10), `TotalAttack = BaseAttack + (EnhancementLevel * 5)`
6. ✅ **던전 시스템**: 15 Stages, 3 Difficulties (Normal/Hard/Hell), 첫 클리어 보너스 +50%, Progressive Unlock
7. ✅ **스킬 가챠 API**: POST /api/skills/gacha, 확률 (Common 60%, Rare 30%, Epic 9%, Legendary 1%), 천장 시스템 100회, 24개 스킬, Unity 문서화

---

## 다음 우선순위

### Immediate (이번 주)
1. ✅ ~~**스킬 가챠 API**~~ (완료)
2. **Drop System**: 던전 클리어 시 Equipment 드랍, 드랍 확률 테이블
3. **Combat-Dungeon 통합**: DungeonStage Monster 스탯 적용, BattleLog DungeonStageId 활용
4. **장비 강화 완성**: Phase 1 시스템 7 마무리

### Short-term (다음 주)
5. **펫 시스템**: Pet Entity, PetTemplate, 스탯 버프 계산
6. **스킬 장착/해제 API**: CharacterSkill IsEquipped 활용

### Mid-term (Week 4-5)
7. **SignalR 채팅**: Hub 구현 (전체, 길드, 귓속말), Unity Client 연동
8. **PVP 아레나**: ELO 매칭, 랭킹 (Redis 준비)
