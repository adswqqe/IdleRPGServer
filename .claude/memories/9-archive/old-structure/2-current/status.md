# 현재 구현 상태 (Week 3 Day 2 완료)

**업데이트**: 2025-10-20

---

## API 엔드포인트 (24개)

### Authentication (5개)
- ✅ POST /api/auth/register - 회원가입
- ✅ POST /api/auth/login - 로그인
- ✅ POST /api/auth/refresh - 토큰 갱신
- ✅ POST /api/auth/logout - 로그아웃
- ✅ GET /api/auth/profile - 프로필 조회

### Character (5개)
- ✅ POST /api/character/Create - 캐릭터 생성
- ✅ GET /api/character/GetCharacters - 캐릭터 목록 조회
- ✅ GET /api/character/{characterId} - 캐릭터 조회
- ✅ DELETE /api/character/{characterId} - 캐릭터 삭제
- ✅ POST /api/character/{characterId}/experience - 경험치 획득

### Battle (3개)
- ✅ POST /api/battle/start - 전투 시작
- ✅ GET /api/battle/logs/{characterId} - 전투 로그 조회
- ✅ GET /api/battle/stats/{characterId} - 전투 통계 조회

### Monster (1개)
- ✅ GET /api/monster/random - 랜덤 몬스터 조회

### Reward (2개)
- ✅ GET /api/reward/offline/{characterId} - 오프라인 보상 계산
- ✅ POST /api/reward/offline/claim - 오프라인 보상 수령

### Equipment (7개)
- ✅ POST /api/equipment/create - 장비 생성 (가챠/드랍)
- ✅ GET /api/equipment/equipped/{characterId} - 장착 장비 조회 (5슬롯)
- ✅ GET /api/equipment/inventory/{ownerId} - 인벤토리 조회 (미장착)
- ✅ POST /api/equipment/equip - 장비 장착 (자동 교체)
- ✅ POST /api/equipment/unequip - 장비 해제
- ✅ POST /api/equipment/enhance - 장비 강화 (+0~+10)
- ✅ DELETE /api/equipment/{equipmentId} - 장비 삭제

### Dungeon (3개)
- ✅ GET /api/dungeons/stages - 던전 스테이지 목록 (난이도별 보상 계산)
- ✅ GET /api/dungeons/progress - 캐릭터 던전 진행도 조회
- ✅ POST /api/dungeons/clear - 던전 클리어 및 보상 지급

---

## Database Tables (9개)

### 인증 & 플레이어
- ✅ **Players** - 플레이어 계정 (GUID PK)
- ✅ **RefreshTokens** - JWT Refresh Token (GUID PK)

### 캐릭터 & 성장
- ✅ **Characters** - 게임 캐릭터 (GUID PK, FK → Players)
- ✅ **BattleLogs** - 전투 로그 (GUID PK, FK → Characters, Monsters, DungeonStages)
- ✅ **OfflineRewardTypes** - 오프라인 보상 타입 (int PK)

### 장비
- ✅ **Equipments** - 장비 (GUID PK, FK → Players, Characters)

### 던전
- ✅ **DungeonStages** - 던전 스테이지 (int PK, FK → Monsters)
- ✅ **CharacterDungeonProgresses** - 던전 진행도 (GUID PK, FK → Characters)

### 마스터 데이터
- ✅ **Monsters** - 몬스터 템플릿 (int PK)

---

## 주요 기능 상세

### 1. 인증 시스템
- JWT Bearer Token (Access 15분, Refresh 7일)
- BCrypt 비밀번호 해싱
- Token Rotation

### 2. 캐릭터 성장
- 레벨업 자동 스탯 증가 (HP +10, Attack +2, Defense +1)
- 다중 캐릭터 지원 (1 Player → N Characters)
- 경험치 공식: `RequiredExp = 100 * (Level ^ 1.5)`

### 3. 전투 시스템
- 서버 기반 턴제 전투 시뮬레이션
- 데미지 계산: `(Attack - Defense) * Random(0.9, 1.1)`
- 전투 로그 기록 (BattleLogs 테이블)

### 4. 오프라인 보상
- 시간 기반 보상 계산: `CharacterLevel * OfflineHours * 10`
- 최대 12시간 (VIP는 48시간 예정)

### 5. 장비 시스템
- 5개 슬롯 (Weapon, Armor, Helmet, Gloves, Boots)
- OwnerId + CharacterId 이중 FK
- 강화 시스템 (+0 ~ +10)
  - 성공 확률: +0→+1 (100%), +5→+6 (50%), +9→+10 (10%)
  - 강화 효과: `TotalAttack = BaseAttack + (EnhancementLevel * 5)`

### 6. 던전 시스템 ⭐ **NEW (2025-10-18)**
- 15 Stages (Lv 1-30)
- 3 Difficulties (Normal, Hard, Hell)
  - Normal: 1.0x (항상 잠금 해제)
  - Hard: 1.5x (Normal 클리어 후)
  - Hell: 2.0x (Hard 클리어 후)
- 첫 클리어 보너스: +50%
- ValueObject 패턴 (DifficultyMultiplier)
- Progressive Difficulty Unlock (동일 스테이지 전 난이도 클리어 필요)

---

## 아키텍처 패턴

### Clean Architecture (4계층)
- **API**: Controllers, Middleware
- **Application**: Services, DTOs
- **Domain**: Entities, Business Rules, ValueObjects
- **Infrastructure**: Repositories, EF Core, External Services

### 사용 중인 패턴
- Repository Pattern
- Service Pattern
- ValueObject Pattern (DDD)
- DTO Pattern

### 대기 중인 패턴
- CQRS with MediatR (복잡도 증가 시)
- AutoMapper (매핑 코드 반복 시)
- FluentValidation (복잡한 검증 시)

---

## 테스트 현황

### 단위 테스트
- **GachaLogicServiceTests** (12개) - 확률 로직 검증
- **기타 테스트** - Repository, Service 테스트

### 테스트 도구
- xUnit
- Moq
- FluentAssertions

---

## Unity 문서화 (v1.8)

**위치**: `../IdleRPGClient/Docs/unity/`

**문서화된 시스템** (7개):
1. auth/ - 인증 시스템
2. character/ - 캐릭터 시스템
3. battle/ - 전투 시스템
4. reward/ - 오프라인 보상
5. equipment/ - 장비 시스템
6. dungeon/ - 던전 시스템 ⭐ NEW
7. monster/ - 몬스터 조회

**구조**:
- `{feature}/API_SPEC.md` - API 명세, Unity C# 예제
- `{feature}/DTOs.cs` - C# DTO 클래스 (JsonProperty)
- `README.md` - 구현 상태 테이블, 버전 히스토리

---

## Production 배포 상태

### AWS 인프라
- **EC2**: t3.micro (Ubuntu 22.04, Docker)
- **RDS**: PostgreSQL 14 (ap-northeast-2)
- **Jenkins**: CI/CD 자동 배포

### 배포 완료
- ✅ Authentication System
- ✅ Character Growth System
- ✅ Combat System
- ✅ Offline Rewards
- ✅ Equipment System
- ✅ Dungeon System ⭐ NEW

### 최근 배포
- **2025-10-18**: Dungeon System 배포 완료
- Migration: `20251018112141_AddDungeonStageSystem`
- Seeder: DungeonStageSeeder (15 stages)

---

## 다음 우선순위

### 즉시 착수 (Week 3-4)
1. **Drop System** - 던전 보상 → Equipment 드랍
2. **Combat-Dungeon 통합** - DungeonStage Monster 스탯 적용
3. **BattleLog 활용** - DungeonStageId 기록 분석

### 진행 중 (Week 3-4)
4. **Skill System** - 스킬 가챠, 스킬 템플릿, PlayerSkill
   - GachaLogicService 구현 완료 (확률 로직 검증 완료)
   - TODO: SkillTemplate Seeder, API 엔드포인트

### 대기 중 (Week 4+)
5. **Enhancement System** - 장비 강화 UI/UX 개선
6. **Pet System** - 펫 육성, 스탯 버프

---

## 기술 부채

### 성능 최적화
- [ ] N+1 쿼리 문제 (Include 최적화)
- [ ] Redis 캐싱 (랭킹, 던전 스테이지 목록)

### 코드 품질
- [ ] MediatR 도입 검토 (CQRS 패턴)
- [ ] AutoMapper 도입 검토 (Entity ↔ DTO)

### 보안
- [ ] Rate Limiting (API 호출 제한)
- [ ] HTTPS 강제 (Production)
- [ ] Swagger 비활성화 (Production)

---

## 메트릭

**총 개발 기간**: Week 3 Day 2 완료
**API 엔드포인트**: 24개
**Database Tables**: 9개
**Unity 문서**: 7개 시스템
**단위 테스트**: 12+ tests
**Migration Files**: 10+ migrations
