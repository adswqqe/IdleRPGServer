# API Implementation Status - Current (Updated 2025-10-16)

## Summary
- **Total Endpoints**: 21 (이전 14 → 현재 21)
- **Completed**: 21
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

## Equipment API (7 endpoints) ← **NEW! (2025-10-16)**
- ✅ POST /api/equipment/create - 장비 생성 (가챠/드랍)
- ✅ GET /api/equipment/equipped/{characterId} - 장착 장비 조회 (5슬롯)
- ✅ GET /api/equipment/inventory/{ownerId} - 인벤토리 조회 (미장착)
- ✅ POST /api/equipment/equip - 장비 장착 (자동 교체)
- ✅ POST /api/equipment/unequip - 장비 해제
- ✅ POST /api/equipment/enhance - 장비 강화 (+0~+10)
- ✅ DELETE /api/equipment/{equipmentId} - 장비 삭제

## Database Tables (7 tables)
- ✅ Players - 인증
- ✅ RefreshTokens - 토큰
- ✅ Characters - 캐릭터
- ✅ Monsters - 몬스터
- ✅ BattleLogs - 전투 로그
- ✅ OfflineRewardTypes - 오프라인 보상 타입
- ✅ Equipments - 장비 ← **NEW!**

## Recent Changes (2025-10-16)

### Equipment System Implementation
**Architecture**: Clean Architecture 4-Layer (Domain → Application → Infrastructure → API)

**Key Features**:
1. **OwnerId + CharacterId 이중 FK**
   - OwnerId: 소유자 (Cascade 삭제)
   - CharacterId: 장착 상태 (SetNull 삭제)
   
2. **자동 장비 교체**
   - 같은 슬롯 장착 시 기존 장비 자동 인벤토리 이동
   - 원자적 트랜잭션 보장 (UnitOfWork)

3. **강화 시스템**
   - 최대 +10까지
   - 강화당 공격+5, 방어+3, HP+10

4. **계산 속성 패턴**
   - Base 스탯만 DB 저장
   - Total 스탯은 실시간 계산 (GetTotalAttack, GetTotalDefense, GetTotalHp)

**Business Rules**:
- 소유자 변경 불가 (OwnerId 불변)
- CharacterId NULL = 인벤토리, NOT NULL = 장착 중
- 같은 슬롯에 중복 장착 불가
- 캐릭터 삭제 시 소유 장비도 삭제 (Cascade)

## Unity Documentation (v1.7)
- ✅ Equipment API 명세서 (11.9KB)
- ✅ Equipment DTO 클래스 (3.9KB)
- ✅ README.md 업데이트 (v1.6 → v1.7)

## Next Implementation
1. Combat System에 Equipment 스탯 통합
2. Gacha System (10연차 장비 생성)
3. Dungeon System (장비 드랍)
