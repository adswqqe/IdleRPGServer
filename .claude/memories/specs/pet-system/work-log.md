# Work Log: Pet System

> 이 문서는 Pet System 구현 작업 로그입니다.

---

## 2025-10-27 14:30

### Task Completed
- [x] 1.1 Create Pet Entity

### Files Changed
- `IdleRPG.Domain/Entities/Pet.cs` (new file, 57 lines)

### Key Decisions
- **Entity 설계**:
  - Id: int (AUTO_INCREMENT, 대량 펫 관리에 적합)
  - Level: int (1-50 범위, Default 1)
  - CurrentAttack/CurrentMana: 레벨업 시 동적 계산 후 저장 (성능 우선)
  - Navigation Properties: Character, PetTemplate (EF Core 관계 설정)

### Notes
- Character, PetTemplate 엔티티는 이후 Task에서 참조
- XML 문서화 주석 추가로 코드 가독성 향상
- CreatedAt, UpdatedAt: DateTime.UtcNow로 초기화 (UTC 기준 시간)

---
