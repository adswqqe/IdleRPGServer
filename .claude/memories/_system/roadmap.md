# 프로젝트 로드맵

> **시스템 상세 명세**: `game-design.md` 참조
> **PRD**: `docs/MUSHROOM_GAME_PRD.md`

**프로젝트 기간**: 8주 (2025-10-14 ~ 2025-12-06)
**현재 진행**: Week 3 Day 3 (2025-10-23)

---

## Phase 1: Foundation (Week 1-3) - MVP 시스템

**시스템**: 1-7 (game-design.md 참조)
**진행률**: **7/7 완료 (100%)** ✅

**완료 시스템**:
- ✅ 1. 인증 시스템 (JWT)
- ✅ 2. 캐릭터 성장 시스템
- ✅ 3. 전투 시스템 (Auto-battle)
- ✅ 4. 오프라인 보상
- ✅ 5. 장비 시스템
- ✅ 6. 던전 시스템
- ✅ 7. 스킬 가챠 시스템

---

## Phase 2: Expansion (Week 4-5) - 핵심 게임플레이

**시스템**: 8-10
**진행률**: **0/3 (0%)**

**다음 작업**:
- 📋 8. 펫 시스템
- 📋 9. 실시간 채팅 (SignalR)
- 📋 10. PVP 아레나

---

## Phase 3: Advanced (Week 6-7) - 소셜 & 수익화

**시스템**: 11-15
**진행률**: **0/5 (0%)**

**예정 시스템**:
- 📋 11. 친구 시스템
- 📋 12. 길드 시스템
- 📋 13. 랭킹 시스템 (Redis)
- 📋 14. 보스 레이드
- 📋 15. 퀘스트 & 업적

---

## Phase 4: Polish (Week 8) - 라이브 운영

**시스템**: 16-20
**진행률**: **0/5 (0%)**

**예정 시스템**:
- 📋 16. 일일 미션 & 출석
- 📋 17. 상점 & VIP
- 📋 18. 우편 시스템
- 📋 19. 이벤트 시스템
- 📋 20. 관리자 도구

---

## 전체 진행률

**완료된 시스템**: 7개 / 20개 (35%)
**진행 중 시스템**: 없음

**Phase 별**:
- Phase 1 (MVP): 7/7 = 100% ✅
- Phase 2 (핵심): 0/3 = 0% 📋
- Phase 3 (소셜): 0/5 = 0%
- Phase 4 (운영): 0/5 = 0%

---

## 주요 마일스톤

### ✅ Week 1 (2025-10-14)
- 시스템 1, 2 완료
- Jenkins CI/CD + AWS 배포

### ✅ Week 2 (2025-10-16)
- 시스템 3, 4 완료
- 전투 로그 시스템

### ✅ Week 3 Day 1 (2025-10-16)
- 시스템 5 완료 (Equipment System - 7 endpoints)
- OwnerId + CharacterId 이중 FK

### ✅ Week 3 Day 2-3 (2025-10-18 ~ 2025-10-22)
- 시스템 6 완료 (Dungeon System - 3 endpoints)
  - 15 Stages, 3 Difficulties
  - ValueObject Pattern
- 시스템 7 완료 (Skill Gacha System)
  - POST /api/skills/gacha
  - 24개 스킬 템플릿, 천장 시스템 100회
  - Unity 문서화 (API_SPEC.md, DTOs.cs)
  - 20개 테스트 (Domain, Infrastructure, API)
- **Phase 1 완료! 🎉**

### 📋 Week 3-4 목표 (Phase 2 시작)
- Drop System (던전 보상)
- Combat-Dungeon 통합
- 스킬 장착/해제 API
- 시스템 8: 펫 시스템 시작

### 📋 Week 4-5 목표
- 시스템 8: 펫 시스템
- 시스템 9: 실시간 채팅 (SignalR 학습)
- 시스템 10: PVP 아레나

### 📋 Week 6-7 목표
- 시스템 11-15: 소셜 기능
- Redis 랭킹 시스템 (Redis 학습)

### 📋 Week 8 목표
- 시스템 16-20: 운영 시스템
- 최종 통합 테스트

---

## Kiro Spec 진행 상황

**완료된 Specs**:
- ✅ `.claude/memories/specs/skill-gacha/` (Requirements, Design, Tasks 모두 완료)

**다음 Spec**:
- 📋 Pet System (시스템 8번)

---

**최종 업데이트**: 2025-10-23
