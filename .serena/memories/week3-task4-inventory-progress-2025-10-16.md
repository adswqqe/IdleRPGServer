# Week 3 Task #4 Inventory System 진행 상황

## 📅 날짜
2025-10-16

## ✅ 완료된 작업

### 1. Task Master 설정
- Task #4 상태: **in-progress** ✅
- Subtask 확장: 5개 subtask 생성 완료 (AI research 모드)

### 2. Subtask 구조 (5개)
```
4.1 Item 및 Inventory 도메인 엔티티 생성 [pending]
4.2 Infrastructure 레이어 구현 및 데이터베이스 설정 [pending] (depends: 4.1)
4.3 Application 서비스 로직 및 DTOs 구현 [pending] (depends: 4.2)
4.4 InventoryController API 엔드포인트 구현 [pending] (depends: 4.3)
4.5 단위 테스트 및 통합 테스트 구현 [pending] (depends: 4.4)
```

### 3. 코드베이스 패턴 분석 완료 ✅

**파일 분석 완료**:
- `Character.cs` - Domain Entity 패턴
- `Monster.cs` - Domain Entity 패턴
- `ICharacterRepository.cs` - Repository 인터페이스 패턴
- `CharacterRepository.cs` - EF Core Repository 구현 패턴
- `IUnitOfWork.cs` - Unit of Work 패턴
- `CharacterService.cs` - Service 레이어 패턴
- `CharacterController.cs` - API Controller 패턴
- `BaseController.cs` - JWT 인증 헬퍼

**주요 패턴 발견**:
1. **Clean Architecture 계층**:
   - Domain: Entity + Repository 인터페이스 (의존성 없음)
   - Application: Service 인터페이스 + DTO
   - Infrastructure: Repository + Service 구현체
   - API: Controller

2. **Unit of Work 패턴**:
   - 모든 Repository가 `IUnitOfWork`에 등록
   - 단일 `SaveChangesAsync()`로 트랜잭션 관리
   - 예: BattleLogs, Characters, Monsters, OfflineRewardTypes, Players, RefreshTokens

3. **EF Core 관계 패턴**:
   - FK + Navigation Property 명시
   - Character.Stats는 Value Object (Owned Entity)
   - Include 없이 사용 (LazyLoading 비활성화)

4. **Controller 패턴**:
   - BaseController 상속
   - `[Authorize]` 적용
   - `GetCurrentUserId()` 헬퍼 사용
   - try-catch with BadRequest 응답

## 🎨 현재 작업: Subtask 4.1 설계 단계

### 설계 결정 대기 중 (TODO(human))

**Context**: Inventory 시스템 데이터 구조 설계 - 마스터 데이터 vs 인스턴스 구조 결정 필요

**3가지 핵심 설계 결정 항목**:

#### 1. 아이템 구조 선택
- **Option A**: Item(마스터) + Inventory(인스턴스)
  - Subtask 4.1 제안: `Item` 엔티티(템플릿) + `Inventory` 엔티티(보유 아이템)
  - 장점: 확장성 good, 나중에 랜덤 옵션 추가 가능
  - 단점: 관계 복잡도 증가
  
- **Option B**: Item만 사용 (Inventory 없이)
  - Item에 CharacterId FK 직접 추가
  - 장점: 단순, 80% DoD에 적합
  - 단점: 마스터 데이터 재사용 불가

#### 2. 장비 장착 구조 선택
- **Option A**: Inventory.IsEquipped + EquipSlot
  - Subtask 4.1 제안
  - 장점: Inventory 테이블에서 모든 정보 조회
  - 단점: "현재 장착 무기" 조회 시 WHERE IsEquipped=true 스캔

- **Option B**: Character에 EquippedWeaponId, EquippedArmorId 등 FK
  - 장점: 장착 장비 조회 빠름 (JOIN 한 번)
  - 단점: Character 테이블 컬럼 증가 (7개 슬롯 = 7개 FK)

#### 3. 강화 레벨 저장 위치
- **Option A**: Inventory.EnhancementLevel (Option 1-A 선택 시)
- **Option B**: Item.EnhancementLevel (Option 1-B 선택 시)

**고려 사항**:
- Week 3 목표: 완전한 게임 루프 완성 (95% 깊이)
- Week 3 후반: Task #7 (Equipment Enhancement) 구현 예정
- 현재 Character는 Value Object 패턴 사용 중
- 기존 코드: FK + Navigation Property 명시 패턴

## 📋 Subtask 4.1 상세 스펙

**생성할 Entity**:
```csharp
// IdleRPG.Domain/Entities/Item.cs
- Id (Guid)
- Name (string)
- Type (ItemType enum)
- Rarity (Rarity enum)
- BaseAttack (int)
- BaseDefense (int)
- BasePrice (int)
- RequiredLevel (int)

// IdleRPG.Domain/Entities/Inventory.cs
- Id (Guid)
- CharacterId (Guid, FK)
- ItemId (Guid, FK)
- IsEquipped (bool)
- EquipSlot (EquipSlot enum)
- EnhancementLevel (int)
- AcquiredAt (DateTime)
```

**생성할 Enum**:
```csharp
// ItemType: Weapon, Armor, Accessory
// Rarity: Common, Rare, Epic, Legendary
// EquipSlot: Weapon, Helmet, Armor, Pants, Boots, Accessory1, Accessory2
```

**관계 설정**:
- Character ↔ Inventory: 1:N (Character.Id → Inventory.CharacterId FK)
- Item ↔ Inventory: 1:N (Item.Id → Inventory.ItemId FK)

## 🔄 다음 단계 (재시작 시)

1. **사용자로부터 3가지 설계 결정 받기**:
   - 아이템 구조: A or B
   - 장비 장착: A or B
   - 강화 레벨: A or B

2. **설계 결정에 따라 Subtask 4.1 구현**:
   - `Item.cs` 엔티티 생성
   - `Inventory.cs` 엔티티 생성 (Option A 선택 시)
   - Enum 타입들 생성
   - Character.cs에 관계 추가

3. **Subtask 4.2 진행**:
   - EF Core Configuration
   - Repository 인터페이스 + 구현
   - IUnitOfWork에 등록
   - 마이그레이션 생성
   - Seed Data 추가

## 📊 TODO 리스트 현재 상태

```
✅ Task #4를 in-progress로 표시
✅ Task #4를 subtask로 확장 (--research)
✅ 기존 코드베이스 패턴 분석
🔄 Inventory 시스템 설계 (엔티티 구조, 관계) - 사용자 결정 대기
⏳ Inventory 엔티티 및 Repository 구현
⏳ InventoryService 구현
⏳ InventoryController API 구현
⏳ Unit Test 작성 및 실행
⏳ Unity 문서 업데이트 (DTO, API Spec)
```

## 🎯 Task #4 전체 목표

**Phase 1 - 95% 깊이**:
- Item 엔티티, ItemTemplate 마스터 데이터
- Inventory 엔티티 (1:N 관계)
- 장비 착용/해제 로직
- 장착 장비 스탯 합산
- InventoryController 5개 엔드포인트

**80% DoD (Definition of Done)**:
- ✅ 아이템 획득/소모
- ✅ 장비 장착/해제
- ❌ 정렬/필터링 (제외)

**Test Strategy**:
- 아이템 CRUD
- 장비 장착 스탯 합산
- 인벤토리 제한 검증

## 📝 중요 참고사항

### 재시작 시 확인할 파일들
- `.taskmaster/tasks/tasks.json` - Task #4 상태 확인
- `IdleRPG.Domain/Entities/` - 기존 엔티티 패턴 참고
- `IdleRPG.Application/Interfaces/IUnitOfWork.cs` - Repository 등록 위치
- `IdleRPG.Infrastructure/Data/GameDBContext.cs` - DbSet 추가 위치

### 코드 스타일 가이드
- 한국어 주석 사용
- XML 주석 (`///`) 적극 활용
- Guid PK, DateTime CreatedAt/UpdatedAt 필수
- Navigation Property 명시
- BaseController 상속 + `[Authorize]`

### Git 커밋 전략
- Subtask 단위로 커밋 (5개 subtask = 최소 5 commits)
- 커밋 메시지: `feat: implement inventory entities (task 4.1)`

## 🔗 연관 Task

- Task #3: Combat System ✅ (완료) - 전투 결과로 아이템 드롭 예정
- Task #5: Offline Rewards ✅ (완료) - 오프라인 보상으로 아이템 획득 예정
- Task #7: Equipment Enhancement (Week 3 후반) - Inventory 의존
- Task #17: Gacha System (Week 8) - Inventory에 아이템 추가

---

**재시작 시 첫 질문**: "3가지 설계 결정을 내려주세요: 아이템 구조, 장비 장착, 강화 레벨"
