# API Implementation Status - Current (2025-10-16)

## 📊 전체 요약
- **Total Endpoints**: 14개
- **Completed**: 14개 (100%)
- **In Progress**: 0개
- **Planned**: Equipment API (Week 3)

---

## ✅ Authentication API (5개)
**Controller**: `IdleRPG.API/Controllers/AuthController.cs`

| Endpoint | Method | Status | Description |
|----------|--------|--------|-------------|
| `/api/auth/register` | POST | ✅ 완료 | 회원가입 |
| `/api/auth/login` | POST | ✅ 완료 | 로그인 |
| `/api/auth/refresh` | POST | ✅ 완료 | Access Token 갱신 |
| `/api/auth/logout` | POST | ✅ 완료 | 로그아웃 (Refresh Token 무효화) |
| `/api/auth/profile` | GET | ✅ 완료 | 내 프로필 조회 |

**Week 1 완성**

---

## ✅ Character API (5개)
**Controller**: `IdleRPG.API/Controllers/CharacterController.cs`

| Endpoint | Method | Status | Description |
|----------|--------|--------|-------------|
| `/api/character/Create` | POST | ✅ 완료 | 캐릭터 생성 |
| `/api/character/GetCharacters` | GET | ✅ 완료 | 내 캐릭터 목록 조회 |
| `/api/character/{characterId}` | GET | ✅ 완료 | 캐릭터 상세 조회 |
| `/api/character/{characterId}` | DELETE | ✅ 완료 | 캐릭터 삭제 |
| `/api/character/{characterId}/experience` | POST | ✅ 완료 | 경험치 획득 (자동 레벨업) |

**Week 1 완성**

---

## ✅ Battle API (1개)
**Controller**: `IdleRPG.API/Controllers/BattleController.cs`

| Endpoint | Method | Status | Description |
|----------|--------|--------|-------------|
| `/api/battle/start` | POST | ✅ 완료 | Auto-battle 시뮬레이션 시작 |

**Week 2 완성**

---

## ✅ Monster API (1개)
**Controller**: `IdleRPG.API/Controllers/MonsterController.cs`

| Endpoint | Method | Status | Description |
|----------|--------|--------|-------------|
| `/api/monster/random` | GET | ✅ 완료 | 레벨 범위 내 랜덤 몬스터 선택 |

**Week 2 완성**

---

## ✅ Reward API (2개)
**Controller**: `IdleRPG.API/Controllers/RewardController.cs`

| Endpoint | Method | Status | Description |
|----------|--------|--------|-------------|
| `/api/reward/offline/{characterId}` | GET | ✅ 완료 | 오프라인 보상 조회 (지급 없음) |
| `/api/reward/offline/{characterId}/claim` | POST | ✅ 완료 | 오프라인 보상 수령 (실제 지급) |

**Week 2 완성**

---

## 📋 Planned API (Week 3)

### Equipment API (예정)
**예상 Controller**: `IdleRPG.API/Controllers/EquipmentController.cs`

| Endpoint | Method | Status | Description |
|----------|--------|--------|-------------|
| `/api/equipment/{characterId}` | GET | 📋 예정 | 캐릭터 장착 장비 조회 |
| `/api/equipment/{characterId}/equip` | POST | 📋 예정 | 장비 장착 (Direct-equip) |
| `/api/equipment/{characterId}/sell/{equipmentId}` | DELETE | 📋 예정 | 장비 판매 (골드 획득) |
| `/api/equipment/{characterId}/compare` | POST | 📋 예정 | 장비 스탯 비교 (UI용) |

**Week 3 구현 예정**