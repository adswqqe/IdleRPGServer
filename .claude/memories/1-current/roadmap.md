# 프로젝트 로드맵

> **시스템 상세 명세**: `0-core/game-design.md` 참조

**프로젝트 기간**: 8주 (2025-10-14 ~ 2025-12-06)
**현재 진행**: Week 3 Day 2 (2025-10-22)

---

## Phase 1: Foundation (Week 1-3) - MVP 시스템

**시스템**: 1-7 (game-design.md 참조)
**진행률**: **6/7 완료 (86%)**

**완료**: ✅ 1, 2, 3, 4, 5, 6
**진행 중**: 📋 7

---

## Phase 2: Expansion (Week 4-5) - 핵심 게임플레이

**시스템**: 8-11
**진행률**: **1/4 (25%)**

**완료**: ✅ 8 (스킬 가챠)
**다음 작업**: 9번 (펫 시스템)

---

## Phase 3: Advanced (Week 6-7) - 소셜 & 수익화

**시스템**: 12-16
**진행률**: **0/5 (0%)**

---

## Phase 4: Polish (Week 8) - 라이브 운영

**시스템**: 17-21
**진행률**: **0/5 (0%)**

---

## 전체 진행률

**완료된 시스템**: 7개 / 21개 (33%)
**진행 중 시스템**: 1개 (시스템 7 - 장비 강화)

**Phase 별**:
- Phase 1 (MVP): 6/7 = 86% 📋
- Phase 2 (핵심): 1/4 = 25% ✅
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

### ✅ Week 3 Day 2 (2025-10-18 ~ 2025-10-22)
- 시스템 6 완료 (Dungeon System - 3 endpoints)
- 15 Stages, 3 Difficulties
- ValueObject Pattern
- 시스템 8 완료 (Skill Gacha System)
  - POST /api/skills/gacha
  - 24개 스킬 템플릿, 천장 시스템 100회
  - Unity 문서화 (API_SPEC.md, DTOs.cs)

### 📋 Week 3-4 목표
- 시스템 7 완성: 장비 강화 (진행 중)
- ✅ ~~시스템 8 완성: 스킬 가챠 API~~ (완료)
- Drop System
- Combat-Dungeon 통합
- 스킬 장착/해제 API

### 📋 Week 4-5 목표
- 시스템 9: 펫 시스템
- 시스템 10: 실시간 채팅 (SignalR)
- 시스템 11: PVP 아레나

### 📋 Week 6-7 목표
- 시스템 12-16: 소셜 기능
- Redis 랭킹 시스템

### 📋 Week 8 목표
- 시스템 17-21: 운영 시스템
- 최종 통합 테스트
