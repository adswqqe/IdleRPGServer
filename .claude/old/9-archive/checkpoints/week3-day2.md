# Daily Session: 2025-10-22

**시작 시간**: 16:43
**Week 3 Day 2**

---

## 📝 주요 성과 (완료된 작업)

### ✅ 스킬 가챠 API
- `POST /api/skills/gacha` 엔드포인트 구현 완료
- 천장(100회) 시스템 포함
- Unity 문서화 완료

### ✅ LootTable 기반 던전 보상 시스템
- **DropService 제거**: God Object 안티패턴 제거, 각 도메인 서비스가 자체 보상 로직 보유
- **LootCalculator 이동**: `Application/Services/DropCalculator.cs` → `Domain/Services/LootCalculator.cs`
  - 순수 비즈니스 로직만 포함 (DB, 로깅 의존성 제거)
  - 반환 타입 변경: `RewardDto` → `List<(LootItem, int Quantity)>`
- **DungeonStage 확장**: `LootTableId` FK 추가, LootTable Navigation Property
- **LootItem 확장**:
  - `EquipmentSlot?`, `EquipmentRarity?` 필드 추가 (장비 드랍 설정용)
  - `ItemTemplateId` (int?) 필드 추가 (int PK 템플릿 참조용)
- **RewardType.Skill 추가**: 스킬 보상 시스템 지원
- **DungeonService 장비/스킬 드랍 구현**:
  - `ProcessEquipmentDropAsync()`: LootTable 기반 확률 드랍
  - `CreateEquipmentFromLootItem()`: 던전 컨텍스트로 장비 생성 (스테이지 레벨 * 희귀도 배율)
  - `GrantSkillToCharacterAsync()`: 스킬 지급 (미보유 시 신규, 중복 시 무시)
- **LootTableSeeder 생성**:
  - 던전 스테이지별 보상 테이블 자동 생성
  - Stage 1-5: Common 장비 80% + Common 스킬 10%
  - Stage 6-10: Rare 장비 60% + Rare 스킬 15%
  - Stage 11-15: Epic/Legendary 장비 40% + Epic 스킬 20%
  - Gold/Experience 100% 보장, 모든 장비 슬롯 포함
- **DungeonClearResultDto 확장**: `List<EquipmentDto>? DroppedEquipments` 추가
- **migration.sql 업데이트**:
  - `AddLootTableToDungeonStage`: DungeonStage에 LootTableId FK
  - `AddEquipmentFieldsToLootItem`: EquipmentSlot, EquipmentRarity 필드
  - `AddSkillRewardSupport`: ItemTemplateId 필드 (Idempotent 패턴)

---

## 🤝 기술적 결정

### Drop System 설계
- **결정**: 별도 DropService 분리 대신 DungeonService에 통합
- **이유**: 트랜잭션 원자성 보장, 코드 응집도 향상, 유지보수 용이
- **구조**: LootTable (데이터 통합) + LootCalculator (순수 확률 로직) + 각 도메인 서비스 (컨텍스트별 보상 생성)

### LootTable vs God Object 구분
- **LootTable (유지)**: 데이터 구조만 제공, DB 기반 밸런싱, 재사용성 높음
- **DropService (제거)**: 모든 컨텐츠 보상 처리 시도 → God Object로 진화 가능성

### GachaLogicService vs LootTable 역할
- **GachaLogicService**: 스킬 가챠 UI 전용 (크리스탈 소모, 천장 시스템, 단일 추첨)
- **LootTable**: 컨텐츠 클리어 보상 (던전/퀘스트, 무료 복합 보상)

### Clean Architecture 준수
- **Application Layer**: 인터페이스만 (IDropService 등)
- **Infrastructure Layer**: 구현체 (DropService, DungeonService 등)
- **Domain Layer**: 순수 로직 (LootCalculator, GachaLogicService)

### 스킬 가챠 확률
- **SSR 확률**: 1% (Legendary)

### 장비 강화 실패 정책
- **결정**: 레벨 유지, 재화만 소모

### 중복 스킬 처리
- **결정**: 무시 (중복 지급 없음)

---

## 🐛 발견한 이슈

### 해결 완료
- **EquipmentSlot.Accessory 누락**: `EquipmentSlot.Gloves`로 변경
- **Equipment.TotalAttack 프로퍼티 → 메서드**: `GetTotalAttack()` 호출로 수정
- **ISkillService 네임스페이스**: Program.cs에서 전체 네임스페이스 명시
- **LootItem ItemId 타입 불일치**: Guid (장비용) + int (스킬 템플릿용) → `ItemTemplateId` (int?) 필드 추가로 해결
- **LootTable Navigation Property**: `Items` (not `LootItems`)

### 인지된 문제
- **N+1 쿼리 문제**: EF Core `Include` 사용 시 발생 가능, 추후 최적화 필요

---

## 👤 TODO(human)

- **펫 버프 로직 구현**: `PetService.cs`의 `CalculatePetBuff()` 메서드에 펫 레벨별 공격력 증가율 정의 필요

---

## 💡 메모 & 인사이트

- **ValueObject 패턴**: 던전 난이도 계산(`DifficultyMultiplier`)에 효과적
- **Loot Table Pattern**: 게임 보상을 데이터 기반으로 관리하는 업계 표준
- **데이터 중심 밸런싱**: 코드 수정 없이 DB만으로 드랍률/보상 조정 가능
- **도메인별 캡슐화**: 각 서비스가 자체 보상 로직 보유로 God Object 방지

---

## 📊 세션 통계

- **Git Commits**: 3개
  - `refactor: DropService 제거 및 LootCalculator를 Domain Layer로 이동`
  - `feat: LootTable 기반 장비 드랍 시스템 완성`
  - `feat: LootTable 기반 스킬 보상 시스템 및 던전 시딩 데이터 완성`
- **마이그레이션**: 3개 생성 (AddLootTableToDungeonStage, AddEquipmentFieldsToLootItem, AddSkillRewardSupport)
- **새 Seeder**: LootTableSeeder (15개 던전 스테이지 보상 테이블)

---

## 📊 프로젝트 현황

### 전체 진행률
- **Phase 1 (MVP)**: 6/6 완료 (100%) ✅
- **Phase 2 (핵심)**: 1.5/4 완료 (37%) 📋
  - ✅ 시스템 7: 스킬 가챠
  - ✅ Drop System (LootTable 기반 던전 보상)
- **Phase 3 (소셜)**: 0/5 완료 (0%)
- **Phase 4 (운영)**: 0/5 완료 (0%)

**총 시스템**: 7개 / 20개 완료 (35%)

### 데이터베이스
- **API 엔드포인트**: 26개
- **데이터베이스 테이블**: 12개

---

## ⏭️ 다음 작업 우선순위

1. **Combat-Dungeon 통합** ⚡ (가장 높은 우선순위)
   - 던전 스테이지 Monster 스탯을 전투 시스템에 적용
   - BattleLog에 DungeonStageId 연결
   - 던전 클리어가 실제 전투 결과와 연동

2. **스킬 장착/해제 API** ⚔️
   - POST /api/characters/{id}/skills/equip
   - POST /api/characters/{id}/skills/unequip
   - 장착된 스킬이 전투에서 실제 사용

3. **시스템 8: 펫 시스템** 🐾 (Phase 2 다음 메인 시스템)
   - 펫 육성, 스탯 버프, 펫 스킬

---

## 💾 백업 정보

- **원본 백업**: `.claude/memories/2-session/daily-2025-10-22-full.md`
- **압축본**: 현재 파일 (36KB → 압축 후)
- **압축 도구**: Gemini 2.5 Pro (via zen MCP)
---


### ✅ 작업 완료 [16:48:21]

✅ **메모리 로드 완료**

---

## 📊 프로젝트 현황

### 전체 진행률
- **Phase 1 (MVP)**: 6/6 완료 (100%) ✅
- **Phase 2 (핵심)**: 1.5/4 완료 (~38%) 📋
- **Phase 3 (소셜)**: 0/5 완료 (0%)
- **Phase 4 (운영)**: 0/5 완료 (0%)

**총 시스템**: 7개 / 20개 완료 (35%)

### 현재 작업
**완료된 시스템**: 
- ✅ 시스템 1-6: 인증, 캐릭터 성장, 장비, 전투, 오프라인 보상, 던전
- ✅ 시스템 7: 스킬 가챠 시스템
- ✅ **Drop System**: LootTable 기반 던전 보상 (장비 + 스킬 드랍)

### 다음 우선순위 (Immediate)
1. **Combat-Dungeon 통합** ⚡ 최우선
   - DungeonStage Monster 스탯을 전투 시스템에 적용
   - BattleLog에 DungeonStageId 활용
   
2. **스킬 장착/해제 API** ⚔️
   - POST /api/characters/{id}/skills/equip
   - POST /api/characters/{id}/skills/unequip
   
3. **시스템 8: 펫 시스템** 🐾
   - Phase 2의 다음 메인 시스템

### 데이터베이스
- **API 엔드포인트**: 26개
- **데이터베이스 테이블**: 12개

---

## 📅 오늘 세션 정보 (2025-10-22)

### 오늘의 목표
1. [x] ✅ 스킬 가챠 API (완료)
2. [x] ✅ **Drop System** (완료!)
   - LootTable 기반 보상 시스템
   - 장비 드랍 (희귀도별, 슬롯별)
   - 스킬 보상 (RewardType.Skill 추가)
   - LootTableSeeder 생성 (던전 15개 스테이지)
3. [ ] Combat-Dungeon 통합 (다음 작업)

### 최근 완료 작업 (16:28)
- ✅ DropService 제거 (God Object 방지)
- ✅ LootCalculator를 Domain Layer로 이동
- ✅ DungeonStage에 LootTableId FK 추가
- ✅ 장비 드랍 로직 완성 (LootItem 확장)
- ✅ 스킬 보상 시스템 완성 (RewardType.Skill)
- ✅ LootTableSeeder로 던전별 보상 자동 생성
- ✅ Git 커밋 & Push 완료

### TODO(human) 대기 중
- 없음

---

## 💾 로드된 파일
- **Core**: 3개 파일 (architecture.md, game-design.md, tech-stack.md)
- **Current**: 2개 파일 (status.md, roadmap.md)  
- **Session**: 1개 파일 (daily-2025-10-22-full.md)

---



무엇을 도와드릴까요? 🚀


---

## 🏁 세션 종료 (자동)

**종료 시간**: 18:18:48
**세션 파일 크기**: 0MB

**최종 통계**:
- API 엔드포인트: 29개
- Database Tables: 22개
- Git Commits: 9개

