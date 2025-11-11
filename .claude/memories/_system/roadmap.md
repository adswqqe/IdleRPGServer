# 프로젝트 로드맵

> **시스템 상세 명세**: `game-design.md` 참조
> **PRD**: `docs/MUSHROOM_GAME_PRD.md`

**프로젝트 기간**: 8주 (2025-10-14 ~ 2025-12-06)
**현재 진행**: Week 4 Day 9 (2025-11-10)

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
**진행률**: **3/3 (100%)** ✅ **Phase 2 완료!** 🎉

**완료 시스템**:
- ✅ 8. 펫 시스템
- ✅ 9. 실시간 채팅 (SignalR)
- ✅ 10. PVP 아레나 (ELO 매칭, Redis 캐싱, 분산락)

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

**완료된 시스템**: 10개 / 20개 (50%)
**진행 중 시스템**: 0개

**Phase 별**:
- Phase 1 (MVP): 7/7 = 100% ✅
- Phase 2 (핵심): 3/3 = 100% ✅ **완료!** 🎉
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

### ✅ Week 3-4 (2025-10-24 ~ 2025-10-28) - Phase 2 시작
- 시스템 8 완료 (Pet System - 8 endpoints) **100% 완료** ✅
  - ✅ Domain: Pet, PetTemplate, EquippedPets, PetGachaService
  - ✅ Application: IPetService + 9개 DTO
  - ✅ Infrastructure: 3개 Repository + PetTemplateSeeder
  - ✅ API: 가챠, 레벨업, 장착/해제, 목록 조회, 삭제
  - ✅ 테스트: 36개 테스트 케이스 (1,322줄)
  - ✅ Spec 문서: Requirements, Design, Tasks, Work-Log
  - ✅ Unity 문서: API_SPEC.md (886줄), DTOs.cs (367줄), INTEGRATION_GUIDE.md (1,081줄)
- **Phase 2 진행률: 33% (1/3)** 🚀

### ✅ Week 4 Day 1-4 (2025-10-28 ~ 2025-11-01)
- 시스템 9 완료 (실시간 채팅 - SignalR) **100% 완료** ✅
  - ✅ Requirements: L Size (18개 대화형 질문)
  - ✅ Design: Full Spec (requirements, design, tasks)
  - ✅ Domain: ChatRoom, ChatMessage, ChatRoomParticipant, RoomType Enum
  - ✅ Application: IChatService + 5개 DTO (ChatMessage, ChatRoom, Error 등)
  - ✅ Infrastructure: 2개 Repository + ChatRoom/ChatMessage Configuration
  - ✅ API: ChatController (2 endpoints) + ChatHub (SignalR 4 methods)
  - ✅ 테스트: 45개 테스트 케이스 (ChatService 20, Controller 14, Repository 11)
  - ✅ Migration: 3개 테이블 (ChatRooms, ChatMessages, ChatRoomParticipants), 9개 인덱스
  - ✅ Unity 문서: API_SPEC.md (680줄), DTOs.cs (320줄), SIGNALR_INTEGRATION_GUIDE.md (560줄), PROFANITY_FILTER.md (480줄), README.md (150줄)
  - ✅ Spec 문서: Requirements, Design, Tasks (27/31, 87%), Work-Log
- **Phase 2 진행률: 67% (2/3)** 🚀

### ✅ Week 4-5 완료 (2025-11-10) **Phase 2 완료!** 🎉
- ✅ 시스템 9: 실시간 채팅 (SignalR) - 완료
- ✅ 시스템 10: PVP 아레나 - **100% 완료** ✅
  - ✅ Domain Layer (6/6): PvpSeason, PvpRanking, PvpMatch, EloRatingService
  - ✅ Infrastructure Layer (9/9): 3개 Repository, 3개 Configuration, RedisCacheService, Seeder
  - ✅ Application Layer (7/7): PvpService, PvpSeasonService, PvpMatchmakingService, 13개 DTO
  - ✅ API Layer (5/5): PvpController (12 endpoints)
  - ✅ Database Migration (3/3): 3개 테이블, 인덱스, FK, 초기 시즌 데이터
  - ✅ Testing & Documentation (3/3): 단위 테스트 (EloRating 8개, PvpService 4개), 통합 테스트, Unity 문서
  - ✅ 코드 리뷰: 92/100점 (A등급), 이슈 수정 완료
  - ✅ 핵심 기능: ELO 매칭, Redis Sorted Set 랭킹, 분산 락, Heartbeat 패턴, N+1 해결

### 📋 Week 6-7 목표
- 시스템 11-15: 소셜 기능
- Redis 랭킹 시스템 (Redis 학습)

### 📋 Week 8 목표
- 시스템 16-20: 운영 시스템
- 최종 통합 테스트

---

## 기술 부채 & 리팩토링

> **목적**: 기능 개발과 별도로 코드 품질 개선, 아키텍처 정리 작업 추적

**완료**:
- ✅ **Combat System Refactoring** (M Size - 2025-10-29 완료)
  - ✅ Phase 1-8 완료: Service 분리, Controller 리네이밍, 테스트 수정, Unity 문서 업데이트
  - ✅ 17개 테스트 모두 통과
  - ✅ Clean Architecture 원칙 준수 (SRP, Dependency Inversion)
  - **결과**: CombatService, BattleLogService, StageService로 명확히 분리
  - **API 변경**: `/api/dungeons/*` → `/api/stages/*`, `/api/battle/*` → `/api/battle-logs/*`
  - **Unity 영향**: Breaking Changes 문서화 완료

**진행 중**:
- (없음)

---

## Kiro Spec 진행 상황

**완료된 Specs**:
- ✅ `.claude/memories/specs/skill-gacha/` (Requirements, Design, Tasks, 구현, Unity 문서 완료)
- ✅ `.claude/memories/specs/pet-system/` (Requirements, Design, Tasks, 구현, Unity 문서 완료)
- ✅ `.claude/memories/specs/combat-system-refactoring/` (spec-lite.md, Phase 1-8 구현 완료 - 2025-10-29)
- ✅ `.claude/memories/specs/realtime-chat/` (Requirements, Design, Tasks 87%, 구현, Unity 문서 완료 - 2025-11-01)

**진행 중 Specs**:
- (없음)

**완료된 Spec (추가)**:
- ✅ `.claude/memories/specs/pvp-arena/` (Requirements, Design, Tasks 33/33 100%, 구현, 코드 리뷰 완료 - 2025-11-10)

**다음 Spec**:
- 📋 Friend System (시스템 11번) - Phase 3 시작!

---

**최종 업데이트**: 2025-11-10 (**Phase 2 완료!** 🎉)
