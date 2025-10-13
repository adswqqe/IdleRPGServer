# Progress

## What Works ✅
### Week 0: Infrastructure
- Clean Architecture 프로젝트 구조 완성
- Docker 개발 환경 (PostgreSQL, Redis, pgAdmin)
- EF Core 9.0 + PostgreSQL 연동
- JWT 인증 시스템 완성

### Week 1: Character System
- **인증 API** (5개 엔드포인트)
  - register, login, refresh, logout, profile
- **캐릭터 API** (6개 엔드포인트)
  - create, list, get, delete, addExperience, allocateStats
- **비즈니스 로직**
  - 레벨업 시스템 (Level * 100 경험치 필요)
  - 스탯 분배 시스템
  - 캐릭터 최대 3개 제한
- **테스트**
  - CharacterService 단위 테스트 17개 (xUnit, Moq, FluentAssertions)
- **Unity 문서**
  - API_SPEC_FOR_UNITY.md 완성 및 동기화
  - DTO 클래스 정의 문서화

## What's Left to Build 📋
### Week 2: Idle Game Loop & Progression (다음 단계)
- [ ] Monster 엔티티 및 시딩
- [ ] Idle combat system (자동 전투)
- [ ] Experience/Gold 자동 획득
- [ ] 오프라인 보상 계산
- [ ] 전투 로그 시스템

### Week 3+: Advanced Features
- [ ] Item & Inventory system
- [ ] Equipment system
- [ ] Quest system
- [ ] Guild/Social features

## Current Status 🎯
**Date**: 2025-10-13

**Phase**: Week 1 완료, Week 2 준비 중

**Recent Achievement**:
- Character system 100% 완료
- 17개 unit tests 모두 통과
- Unity 문서 완전 동기화

**Next Steps**:
1. Task Master로 Week 2 작업 확인 (`task-master next`)
2. Monster 엔티티 설계 및 구현
3. Idle combat 로직 설계

**Tech Debt**: 없음 (Week 1에서 깔끔하게 정리됨)

**Blocked Issues**: 없음
