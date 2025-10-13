# Character 자동 성장 시스템 리팩토링 완료 (2025-10-13)

## 작업 요약

사용자 요청에 따라 Character 시스템을 **수동 스탯 분배 방식**에서 **자동 성장 방식**으로 완전히 리팩토링했습니다.

## 핵심 변경사항

### 1. CharacterStats Value Object 단순화
- **제거**: 기본 스탯 4개 (Strength, Dexterity, Intelligence, Vitality)
- **유지**: 전투 스탯 6개만 (Attack, Defense, MaxHealth, CritRate, CritDamage, Evasion)
- **타입 변경**: `int` → `long` (방치형 게임 특성상 숫자 급증 대비)

### 2. 자동 성장 로직 구현
- 레벨업 시 자동으로 스탯 증가:
  - Attack: +10 per level
  - Defense: +5 per level
  - MaxHealth: +50 per level
  - CritRate, CritDamage, Evasion: 고정값 유지
- 초기 스탯 (Lv1): Attack 10, Defense 5, MaxHealth 100
- 경험치 공식: 레벨 × 100

### 3. 제거된 기능
- `StatPoints` 필드 (Character 엔티티에서 완전 제거)
- `AllocateStatPointsAsync()` 메서드 (ICharacterService, CharacterService)
- `PUT /api/character/{id}/stats` API 엔드포인트 (CharacterController)

### 4. 직업 시스템 확장 준비
- `Character.cs`와 `CharacterService.cs`에 TODO 추가
- 향후 JobType enum 추가 시 직업별 성장 공식 적용 가능하도록 구조화

## 영향받은 파일들

### Domain Layer
- `IdleRPG.Domain/ValueObjects/CharacterStats.cs` - 생성자 파라미터 10개 → 6개
- `IdleRPG.Domain/Entities/Character.cs` - StatPoints 제거

### Application Layer
- `IdleRPG.Application/Character/Services/ICharacterService.cs` - AllocateStatPointsAsync 제거
- `IdleRPG.Application/DTOs/Characters/CharacterDto.cs` - 기본 스탯 → 전투 스탯

### Infrastructure Layer
- `IdleRPG.Infrastructure/Service/CharacterService.cs` - 자동 성장 로직 구현
- `IdleRPG.Infrastructure/Configurations/CharacterConfiguration.cs` - long 타입 매핑
- `IdleRPG.Infrastructure/Migrations/20251013133523_RefactorCharacterStatsToAutoGrowth.cs` - 마이그레이션 생성

### API Layer
- `IdleRPG.API/Controllers/CharacterController.cs` - 스탯 분배 엔드포인트 제거

### Tests
- `IdleRPG.Tests/CharacterServiceTests.cs` - 17개 테스트 전부 자동 성장 시스템에 맞게 수정
  - `CreateTestCharacter()` 헬퍼 메서드: 파라미터 4개 → 3개 (statPoints 제거)
  - AddExperienceAsync 테스트 5개: StatPoints 검증 → 전투 스탯 검증
  - AllocateStatPointsAsync 테스트 4개: 완전 제거
  - CreateCharacterAsync 테스트 1개: 기본 스탯 검증 → 전투 스탯 검증

## 데이터베이스 스키마 변경

### 제거된 컬럼
- Characters.Strength (int)
- Characters.Dexterity (int)
- Characters.Intelligence (int)
- Characters.Vitality (int)
- Characters.StatPoints (int)

### 추가된 컬럼
- Characters.Attack (bigint)
- Characters.Defense (bigint)
- Characters.MaxHealth (bigint)
- Characters.CritRate (real/float)
- Characters.CritDamage (real/float)
- Characters.Evasion (real/float)

## 배포 상태

✅ **Git Push 완료** (Commit: `8cf2ebe`)
- EC2 Jenkins가 자동으로 빌드, 마이그레이션 적용, 배포 처리 중
- 로컬 환경에서 마이그레이션 실행 불필요

## Breaking Changes

### API 변경사항
1. **제거된 엔드포인트**:
   - `PUT /api/character/{id}/stats` - 스탯 분배 API

2. **변경된 DTO**:
   - `CharacterDto` 스키마 완전 변경
   - 제거: StatPoints, Strength, Dexterity, Intelligence, Vitality
   - 추가: Attack, Defense, MaxHealth, CritRate, CritDamage, Evasion (long 타입)

### Unity 클라이언트 영향
- CharacterDto 클래스 업데이트 필요
- 스탯 분배 UI 제거 필요
- 자동 레벨업 UI로 교체 필요

## 다음 작업 (남은 TODO)

1. **Battle System 구현** (다음 우선순위)
   - IBattleService 인터페이스 정의
   - BattleService 전투 로직 구현 (자동 성장된 스탯 활용)
   - BattleController API 생성

2. **Unity Documentation 업데이트**
   - Character API 스펙 업데이트 필요
   - Unity-DTOs.cs 업데이트 필요

3. **추가 기능**
   - Offline Reward System
   - Idle Progress Background Service
   - Battle Log System

## 설계 결정 사항

### 왜 자동 성장 방식을 선택했는가?
1. **방치형 게임 특성**: 플레이어가 복잡한 스탯 분배를 고민할 필요 없음
2. **숫자 인플레이션**: long 타입으로 exponential growth 지원
3. **직업 시스템 확장성**: 직업별로 다른 성장 공식 적용 가능
4. **단순화**: 기본 스탯과 전투 스탯의 이중 구조 제거

### 레벨업 공식 선택 이유
- **경험치 필요량**: Level × 100 (간단하고 예측 가능)
- **스탯 증가량**: 고정값 (+10/+5/+50) - 추후 직업별로 배율 조정 가능
- **크리티컬/회피**: 초기값 유지 - 장비나 버프 시스템에서 증가 예정

## 빌드 상태

✅ **로컬 빌드 성공** (0 errors, 1 warning)
- Warning: EF Core 버전 충돌 (9.0.1 vs 9.0.9) - 무시 가능
✅ **테스트 통과 대상**: 17개 Character 테스트 (수정 완료)
✅ **마이그레이션 생성**: RefactorCharacterStatsToAutoGrowth

## 참고사항

- CI/CD: Jenkins가 Git Push 감지 시 자동 배포
- 데이터베이스: EC2 PostgreSQL에 마이그레이션 자동 적용
- 기존 캐릭터 데이터: 마이그레이션 시 기본 스탯 → 전투 스탯 변환 (기본값 0)
