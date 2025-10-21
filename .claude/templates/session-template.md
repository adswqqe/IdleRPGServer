# Daily Session: {DATE}

**시작 시간**: {TIME}
**Week {X} Day {Y}**

---

## 🎯 오늘의 목표

> `1-current/status.md`의 "다음 우선순위" 기반

1. [ ] {목표1}
2. [ ] {목표2}
3. [ ] {목표3}

---

## ⚙️ 진행 중

### {시스템명} - {작업명}

**상태**: 🟡 진행 중 / 🟢 완료 / 🔴 블로킹

**변경사항**:
- API: `POST /api/...` 추가
- DB: `SkillTemplates` 테이블 생성
- DTO: `SkillGachaResultDto.cs` 작성

**코드 위치**:
- `IdleRPG.API/Controllers/SkillController.cs:45`
- `IdleRPG.Domain/Entities/SkillTemplate.cs`

---

## 🤝 의사결정 & 협업

### 결정 사항
- **SSR 확률**: 1% (Legendary)
- **강화 실패 처리**: 레벨 유지, 재화만 소모

### TODO(human) 대기 중
- [ ] `CalculatePetBuff()` 구현 - 펫 스탯 버프 계산 로직
  - 위치: `IdleRPG.Domain/Services/PetService.cs:TODO(human)`
  - 컨텍스트: 펫 레벨별 공격력 증가율 결정 필요

---

## 🐛 발견된 이슈

### Critical
- [ ] {이슈 설명}

### Normal
- {이슈 설명}

---

## 💡 메모 & 인사이트

- ValueObject 패턴이 던전 난이도 계산에 유용했음
- EF Core Include 쿼리 최적화 필요 (N+1 문제)
- Unity 문서 자동 생성 스크립트 고려

---

## 📊 세션 통계

- **API 엔드포인트**: +3개 (24 → 27)
- **DB 테이블**: +1개 (9 → 10)
- **완료한 TODO(human)**: 2개
- **새로운 TODO(human)**: 1개
- **Git Commits**: 3개

---

## ⏭️ 다음 세션 우선순위

1. {다음 작업1}
2. {다음 작업2}
