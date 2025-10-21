# 협업 워크플로우

## 🤖 Claude 자동 처리 작업

Claude가 독립적으로 완료하는 작업:

### 1. CRUD 메서드
```csharp
// Repository Pattern
public async Task<Character> GetByIdAsync(Guid id) { }
public async Task<List<Character>> GetAllAsync() { }
public async Task AddAsync(Character entity) { }
public async Task UpdateAsync(Character entity) { }
public async Task DeleteAsync(Guid id) { }
```

### 2. DTO 생성
```csharp
// Request/Response DTO
public class CreateCharacterDto { }
public class CharacterDto { }
public class UpdateCharacterDto { }
```

### 3. Configuration
```csharp
// EF Core Entity Configuration
public class CharacterConfiguration : IEntityTypeConfiguration<Character>
{
    public void Configure(EntityTypeBuilder<Character> builder) { }
}
```

### 4. EF Core 마이그레이션
```bash
dotnet ef migrations add AddCharacterSystem --startup-project ../IdleRPG.API
# migration.sql 파일 생성 (Jenkins 자동 배포)
```

### 5. Unity API 문서
```markdown
# Character API 명세 (API_SPEC.md)
# Character DTO (DTOs.cs with JsonProperty)
# README.md 업데이트
```

### 6. Swagger 주석
```csharp
/// <summary>
/// 캐릭터를 생성합니다.
/// </summary>
[HttpPost]
public async Task<IActionResult> CreateCharacter([FromBody] CreateCharacterDto dto) { }
```

### 7. 단위 테스트
```csharp
// AAA 패턴 기반 테스트
[Fact]
public async Task GetCharacter_ValidId_ReturnsCharacter() { }
```

---

## 👥 함께 협업하는 작업

사용자와 함께 논의하고 결정하는 작업:

### 1. 데이터 설계
- **엔티티 관계**: 1:N, M:N, 자기 참조
- **인덱스 전략**: 어떤 컬럼에 인덱스?
- **Soft Delete 여부**: IsDeleted vs 실제 삭제?

**예시**:
```
Claude: "Character와 Skill은 M:N 관계로 설계하는 게 좋을 것 같습니다.
        PlayerSkill 중간 테이블에 SkillLevel과 SlotIndex를 추가하면
        캐릭터별 스킬 레벨과 장착 슬롯을 관리할 수 있습니다. 어떻게 생각하시나요?"

사용자: "좋아요, 그리고 SlotIndex는 1-6까지만 허용하게 검증 추가해주세요"
```

### 2. 비즈니스 로직
- **전투 공식**: 데미지 계산 방식
- **보상 계산**: 경험치, 골드, 드랍률
- **확률 시스템**: 가챠, 강화 성공률

**예시**:
```
Claude: "던전 난이도별 보상 배율을 제안합니다:
        - Normal: 1.0x
        - Hard: 1.5x
        - Hell: 2.0x
        첫 클리어 시 +50% 보너스를 추가하면 어떨까요?"

사용자: "Hell은 2.5x로 올려주세요. 난이도가 훨씬 높으니까요"
```

### 3. API 설계
- **요청/응답 구조**: DTO 필드 구성
- **에러 핸들링**: 어떤 상황에 어떤 에러?
- **페이징/필터링**: 정렬 기준, 기본값

**예시**:
```
Claude: "캐릭터 목록 API는 이렇게 설계했습니다:
        GET /api/characters?sort=level,desc&page=1&size=20

        기본 정렬은 생성일 내림차순으로 할까요, 레벨 내림차순으로 할까요?"

사용자: "레벨 내림차순이 더 직관적일 것 같아요"
```

### 4. 아키텍처 설계
- **계층 분리**: 어떤 로직을 어느 계층에?
- **의존성 주입**: Scoped vs Singleton
- **패턴 선택**: CQRS 도입 시점

**예시**:
```
Claude: "GachaLogicService를 Domain Service로 배치했습니다.
        순수한 확률 계산 로직이라 Infrastructure 의존이 없어서입니다.
        하지만 Application Layer에 두는 것도 가능한데, 어떻게 생각하시나요?"

사용자: "Domain Service가 맞는 것 같아요. 비즈니스 규칙이니까요"
```

### 5. 성능 최적화
- **쿼리 최적화**: Include vs Projection
- **캐싱 전략**: 무엇을 캐시할 것인가?
- **동시성 처리**: Lock 전략

**예시**:
```
Claude: "랭킹 조회 시 Character 전체를 Include하면 느릴 것 같습니다.
        Select로 Id, Name, Level만 가져오는 게 나을까요?"

사용자: "맞아요, Projection으로 필요한 필드만 가져오세요"
```

---

## 협업 프로세스

### 1. 설계 단계
```
Claude 초안 제시 → 사용자 피드백 → 최종 결정
```

**Claude**:
- 아키텍처 제안
- 데이터 모델 초안
- API 엔드포인트 설계

**사용자**:
- 비즈니스 요구사항 확정
- 밸런스 수치 결정
- 우선순위 조정

### 2. 구현 단계
```
핵심 로직 TODO(human) → 사용자 구현 → Claude 나머지 완성
```

**TODO(human) 마커 사용**:
```csharp
public int CalculateDamage(Character attacker, Character defender)
{
    // TODO(human): 데미지 공식 확정 후 구현
    // 제안: (Attack - Defense) * Random(0.9, 1.1)
    throw new NotImplementedException();
}
```

**반복 코드는 Claude 자동 완성**:
```csharp
// Repository CRUD, DTO Mapping, Configuration 등
```

### 3. 검토 단계
```
Claude 구현 완료 → 주요 변경사항 요약 → 사용자 승인
```

**Claude 요약 형식**:
```
## 구현 완료

### 추가된 파일
- Domain/Entities/Skill.cs
- Application/DTOs/Skills/SkillDto.cs
- Infrastructure/Repositories/SkillRepository.cs

### 핵심 변경사항
- Character-Skill M:N 관계 추가
- PlayerSkill 중간 테이블 (SkillLevel, SlotIndex)
- 스킬 슬롯 6개 제한 (검증 추가)

### 다음 단계
- 스킬 가챠 시스템 구현
- 스킬 레벨업 로직 추가
```

---

## TODO(human) 규칙

### 사용 시점

**비즈니스 로직이 불확실할 때**:
```csharp
// TODO(human): 크리티컬 확률 몇 %로 할까요?
public bool IsCritical()
{
    throw new NotImplementedException();
}
```

**사용자 선호도가 필요할 때**:
```csharp
// TODO(human): 장비 강화 실패 시 레벨 하락? 유지?
public void EnhanceEquipment(Equipment equipment)
{
    throw new NotImplementedException();
}
```

**디자인 결정이 필요할 때**:
```csharp
// TODO(human): PVP 매칭 알고리즘 (ELO? MMR? 레벨 기반?)
public List<Character> FindOpponents(Character character)
{
    throw new NotImplementedException();
}
```

### 제거 시점

**사용자가 구현 완료한 후**:
```csharp
// ✅ TODO(human) 제거됨
public bool IsCritical()
{
    return Random.Shared.Next(100) < 15;  // 15% 확률
}
```

---

## 작업 흐름 예시

### 예시 1: 스킬 시스템 구현

**Phase 1: 설계** (함께 협업)
```
사용자: "스킬 시스템 추가해주세요"

Claude: "스킬 시스템 설계 제안드립니다:
        1. Character-Skill M:N 관계
        2. 슬롯 6개 (레벨별 확장)
        3. 패시브 스킬 (상시 적용)
        4. 액티브 스킬 (전투 중 발동)

        어떻게 생각하시나요?"

사용자: "좋아요. 슬롯은 레벨 10마다 1개씩 추가로 늘어나게 해주세요"

Claude: "네, 슬롯 계산 공식:
        InitialSlots = 4
        MaxSlots = 6
        UnlockAt = [10, 20]

        이렇게 진행하겠습니다"
```

**Phase 2: 구현** (Claude 자동 + TODO(human))
```
Claude: [자동 생성]
        - Skill Entity
        - PlayerSkill Entity (중간 테이블)
        - SkillDto
        - ISkillRepository
        - SkillRepository

        [TODO(human) 마커]
        - CalculatePassiveBonus() 메서드
        - TriggerActiveSkill() 메서드
```

**Phase 3: 검토**
```
Claude: "스킬 시스템 기본 구조 완성했습니다.
        TODO(human) 2개 있으니 확인 부탁드립니다:
        1. 패시브 스킬 보너스 계산식
        2. 액티브 스킬 발동 조건"

사용자: "패시브는 스탯 %로 증가, 액티브는 턴마다 30% 확률로 발동하게 해주세요"
```

### 예시 2: 던전 보상 밸런스 조정

**협업 과정**:
```
Claude: "던전 보상 제안:
        Stage 1: 200 Gold
        Stage 5: 1000 Gold
        Stage 10: 5000 Gold

        선형 증가보다 지수 증가가 좋을까요?"

사용자: "지수로 하되, Stage 1을 100 Gold로 낮춰주세요"

Claude: [자동 수정]
        - DungeonStageSeeder 업데이트
        - 보상 공식: BaseReward * (Stage ^ 1.3)
```

---

## 의사결정 기준

### Claude가 독립적으로 결정할 수 있는 것
- 코드 구조 (Clean Architecture 준수)
- 변수명, 메서드명 (컨벤션 준수)
- 에러 핸들링 방식 (표준 HTTP 상태 코드)
- 테스트 작성 (AAA 패턴)

### 사용자와 논의해야 하는 것
- 게임 밸런스 (확률, 보상, 난이도)
- 비즈니스 규칙 (거래 제한, 레벨 캡)
- 사용자 경험 (UI/UX, 에러 메시지)
- 성능 트레이드오프 (캐싱 vs 정확성)

---

## 커뮤니케이션 팁

### Claude의 제안 형식
```
## 제안

[현재 상황 설명]
[옵션 A] 장점: ... / 단점: ...
[옵션 B] 장점: ... / 단점: ...

추천: 옵션 A (이유: ...)

어떻게 진행할까요?
```

### 사용자 피드백
```
// 명확한 지시
"옵션 A로 진행해주세요"
"Hell 난이도 배율만 2.5x로 변경해주세요"

// 추가 논의
"옵션 A가 좋긴 한데, 성능 걱정됩니다. 대안 있나요?"
"조금 더 자세히 설명해주세요"
```

---

## 작업 체크리스트

### 새 기능 추가 시
- [ ] 데이터 모델 설계 (사용자 승인)
- [ ] 비즈니스 로직 확정 (TODO(human) 구현)
- [ ] Entity/DTO/Repository 생성 (Claude 자동)
- [ ] Service/Controller 구현 (Claude 자동)
- [ ] EF Core Migration 생성 (Claude 자동)
- [ ] 단위 테스트 작성 (Claude 자동)
- [ ] Unity 문서 업데이트 (Claude 자동)
- [ ] README.md 업데이트 (필요시)

### 완료 기준
- [ ] 모든 TODO(human) 구현 완료
- [ ] 단위 테스트 통과
- [ ] 코드 리뷰 승인 (사용자)
- [ ] Unity 문서 최신화
- [ ] Git Commit & Push
