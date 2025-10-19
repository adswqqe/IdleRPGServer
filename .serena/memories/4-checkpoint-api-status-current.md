# API Implementation Status - Current (Updated 2025-10-18)

## Summary
- **Total Endpoints**: 24 (이전 21 → 현재 24)
- **Completed**: 24
- **In Progress**: 0
- **Removed**: 1 (스탯 할당 - 자동 성장으로 대체)

## Authentication API (5 endpoints)
- ✅ POST /api/auth/register - 회원가입
- ✅ POST /api/auth/login - 로그인
- ✅ POST /api/auth/refresh - 토큰 갱신
- ✅ POST /api/auth/logout - 로그아웃
- ✅ GET /api/auth/profile - 프로필 조회

## Character API (5 endpoints)
- ✅ POST /api/character/Create - 캐릭터 생성
- ✅ GET /api/character/GetCharacters - 캐릭터 목록 조회
- ✅ GET /api/character/{characterId} - 캐릭터 조회
- ✅ DELETE /api/character/{characterId} - 캐릭터 삭제
- ✅ POST /api/character/{characterId}/experience - 경험치 획득
- ⚠️ ~~PUT /api/character/{characterId}/stats~~ - **삭제됨** (자동 성장)

## Battle API (3 endpoints)
- ✅ POST /api/battle/start - 전투 시작
- ✅ GET /api/battle/logs/{characterId} - 전투 로그 조회
- ✅ GET /api/battle/stats/{characterId} - 전투 통계 조회

## Monster API (1 endpoint)
- ✅ GET /api/monster/random - 랜덤 몬스터 조회

## Reward API (2 endpoints)
- ✅ GET /api/reward/offline/{characterId} - 오프라인 보상 계산
- ✅ POST /api/reward/offline/claim - 오프라인 보상 수령

## Equipment API (7 endpoints)
- ✅ POST /api/equipment/create - 장비 생성 (가챠/드랍)
- ✅ GET /api/equipment/equipped/{characterId} - 장착 장비 조회 (5슬롯)
- ✅ GET /api/equipment/inventory/{ownerId} - 인벤토리 조회 (미장착)
- ✅ POST /api/equipment/equip - 장비 장착 (자동 교체)
- ✅ POST /api/equipment/unequip - 장비 해제
- ✅ POST /api/equipment/enhance - 장비 강화 (+0~+10)
- ✅ DELETE /api/equipment/{equipmentId} - 장비 삭제

## Dungeon API (3 endpoints) ← **NEW! (2025-10-18)**
- ✅ GET /api/dungeons/stages - 던전 스테이지 목록 (난이도별 보상 계산)
- ✅ GET /api/dungeons/progress - 캐릭터 던전 진행도 조회
- ✅ POST /api/dungeons/clear - 던전 클리어 및 보상 지급

## Database Tables (9 tables)
- ✅ Players - 인증
- ✅ RefreshTokens - 토큰
- ✅ Characters - 캐릭터
- ✅ Monsters - 몬스터
- ✅ BattleLogs - 전투 로그 (DungeonStageId 추가)
- ✅ OfflineRewardTypes - 오프라인 보상 타입
- ✅ Equipments - 장비
- ✅ DungeonStages - 던전 스테이지 ← **NEW!**
- ✅ CharacterDungeonProgresses - 던전 진행도 ← **NEW!**

## Recent Changes (2025-10-18)

### Dungeon System Implementation
**Architecture**: Clean Architecture 4-Layer (Domain → Application → Infrastructure → API)

**Key Features**:
1. **15 Dungeon Stages (Lv 1-30)**
   - 초보 던전 (Stage 1-5, Lv 1-10)
   - 중급 던전 (Stage 6-10, Lv 11-20)
   - 고급 던전 (Stage 11-15, Lv 21-30)

2. **3 Difficulty Levels**
   - Normal (1.0x multiplier) - 항상 잠금 해제
   - Hard (1.5x multiplier) - Normal 클리어 후 잠금 해제
   - Hell (2.0x multiplier) - Hard 클리어 후 잠금 해제

3. **Progressive Difficulty Unlock System**
   - Same Stage Prerequisite: Stage 5 Hard는 Stage 5 Normal 클리어 필요
   - Level Gate: CharacterLevel ≥ RequiredLevel

4. **Reward Calculation**
   - Base Reward × Difficulty Multiplier
   - First Clear Bonus: +50% (한 번만)
   - Example: Stage 1 Hell (첫 클리어) = 200G × 1.5 = 300G

5. **ValueObject Pattern**
   - DifficultyMultiplier (Immutable Instance)
   - Factory Method: Create(DungeonDifficulty)

6. **Progress Tracking**
   - 3-field design (각 난이도별 별도 필드)
   - HighestStageClearedNormal/Hard/Hell
   - CharacterId Unique Index

**Business Rules**:
- 레벨 부족 시 입장 불가
- 이전 난이도 미클리어 시 잠금
- 동일 스테이지+난이도 중복 클리어 불가 (재도전 보상 없음)
- 전투 승리 시에만 진행도 업데이트

**Database Schema**:
- DungeonStages: int PK (IDENTITY), FK to Monsters (RESTRICT)
- CharacterDungeonProgresses: Guid PK, FK to Characters (CASCADE)
- Indexes: RequiredLevel, CharacterId (Unique), MonsterId

**Seed Data**:
- DungeonStageSeeder: 15 stages with balanced rewards
- Runs in all environments (idempotent design)
- Monster FK dependency check

## Unity Documentation (v1.8)
- ✅ Dungeon API 명세서 (dungeon/API_SPEC.md)
- ✅ Dungeon DTO 클래스 (dungeon/DTOs.cs)
- ✅ README.md 업데이트 (v1.7 → v1.8)
- ✅ CLAUDE.md Unity 가이드라인 개선 (기능별 폴더 구조 명시)

## Testing Status (2025-10-18)
- ✅ EC2 Production 환경 테스트 완료
- ✅ GET /api/dungeons/stages: 45개 스테이지 반환 (15 × 3 난이도)
- ✅ GET /api/dungeons/progress: 초기 진행도 조회 (0, 0, 0)
- ✅ POST /api/dungeons/clear: Stage 1 클리어 성공 (+100G, +50XP)

## Migration Status
- ✅ Migration 20251018112141_AddDungeonStageSystem 생성
- ✅ EC2 RDS 적용 완료 (fix-dungeon-migration.sql)
- ✅ Seed Data 자동 생성 (Program.cs)

## Next Implementation
1. Drop System에 Dungeon 보상 연동
2. Combat System에 Dungeon Monster 스탯 적용
3. BattleLog에 DungeonStageId 기록 활용
4. Unity 클라이언트 Dungeon UI 구현
