# 기능 명세: 스킬 뽑기(Gacha) 시스템

## 1. 목표
플레이어가 게임 내 재화(크리스탈)를 사용하여 확률에 따라 다양한 등급의 스킬을 획득할 수 있는 뽑기 시스템을 구현한다.

## 2. 핵심 기능
- **스킬 데이터 정의**: 스킬의 기본 속성(ID, 이름, 등급, 효과)을 정의하고, 여러 종류의 스킬을 관리할 수 있어야 한다.
- **확률 테이블**: 스킬 등급(Common, Rare, Epic, Legendary)별 획득 확률을 설정하고 관리할 수 있어야 한다.
- **뽑기 실행**: 1회 뽑기 및 10회 뽑기 기능을 제공한다. 재화가 부족할 경우 에러를 반환해야 한다.
- **결과 처리**: 뽑기 결과로 획득한 스킬을 플레이어의 스킬 인벤토리에 추가해야 한다.
- **API 엔드포인트**: 클라이언트가 스킬 뽑기를 요청하고 결과를 받을 수 있는 API를 제공한다.

## 3. 상세 구현 요소

### 3.1. Domain Layer
- `Skill` Entity: 스킬 ID, 이름, 설명, 등급(Enum), 효과 관련 속성 정의.
- `SkillType` Enum: 스킬의 타입을 정의 (예: Active, Passive).
- `SkillRarity` Enum: 스킬의 등급을 정의 (Common, Rare, Epic, Legendary).
- `PlayerSkill` Entity: 플레이어가 보유한 스킬 정보 (PlayerId, SkillId, Level).
- `GachaPool` Entity: 스킬 뽑기 풀에 포함된 스킬과 확률 정보를 관리.
- `ISkillRepository`: 스킬 데이터 조회를 위한 인터페이스.
- `IPlayerSkillRepository`: 플레이어 보유 스킬 데이터 조작을 위한 인터페이스.
- `IGachaRepository`: 스킬 뽑기 확률 테이블 조회를 위한 인터페이스.
- `GachaService` (Domain Service): 스킬 뽑기 핵심 비즈니스 로직 (확률 기반 스킬 선택).

### 3.2. Application Layer
- `GachaApplicationService`: 뽑기 요청 처리, 재화 차감, 결과 반환 등 워크플로우를 관장.
- `PerformGachaCommand`: 뽑기 실행을 위한 Command 객체 (PlayerId, GachaType).
- `GachaResultDto`: 뽑기 결과를 클라이언트에 전달하기 위한 DTO.
- `IPlayerCurrencyService`: 플레이어 재화 차감을 위한 인터페이스.
- `IUnitOfWork`: 트랜잭션 관리를 위한 Unit of Work 패턴 적용.

### 3.3. Infrastructure Layer
- `SkillRepository`: `ISkillRepository`의 구현체. DB에서 스킬 정보를 조회.
- `PlayerSkillRepository`: `IPlayerSkillRepository`의 구현체.
- `GachaRepository`: `IGachaRepository`의 구현체. JSON 파일 또는 DB에서 확률 테이블 로드.
- `DbContext`에 `Skills`, `PlayerSkills`, `GachaPools` DbSet 추가 및 모델 설정.
- 데이터베이스 마이그레이션 스크립트 작성.

### 3.4. API (Presentation) Layer
- `GachaController`: 스킬 뽑기 API 엔드포인트 정의.
  - `POST /api/gacha/draw`: 스킬 뽑기 실행.

### 3.5. 테스트
- `GachaService` 유닛 테스트: 확률에 따라 스킬이 올바르게 선택되는지 검증.
- `GachaApplicationService` 통합 테스트: 재화 차감, 스킬 지급, 결과 반환까지 전체 워크플로우 검증.
