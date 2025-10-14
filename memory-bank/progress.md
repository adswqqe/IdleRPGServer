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

### Week 2: Idle Game Loop & Progression (진행 중)
- [x] **Feature 1**: Monster 엔티티 및 시딩 ✅
  - Monster 엔티티 생성 (5종: 슬라임, 고블린, 오크, 트롤, 드래곤)
  - MonsterConfiguration (EF Core)
  - 마이그레이션 적용 및 EC2 배포 완료
- [x] **AttackSpeed 구현** ✅
  - CharacterStats Value Object에 AttackSpeed 추가
  - Monster 엔티티에 AttackSpeed, CritRate, CritDamage, Evasion 추가
  - 마이그레이션 생성: `AddAttackSpeedToCharacterAndMonster`
  - 몬스터별 차별화된 전투 스타일 (0.6f ~ 1.2f)
- [ ] **Feature 2**: Battle System Core Logic (진행 중)
  - BattleService 인터페이스 및 DTO 생성 (진행 중)
  - 실시간 DPS 계산 로직 (대기 중)
  - 크리티컬/회피 확률 처리 (대기 중)
- [ ] **Feature 3**: Battle Controller & API
- [ ] **Feature 4**: Character Schema Update (Gold, LastLoginTime)
- [ ] **Feature 5**: Offline Reward System
- [ ] **Feature 6**: Idle Progress Background Service
- [ ] **Feature 7**: Battle Log System
- [ ] **Feature 8**: Unity Documentation Update
- [ ] **Feature 9**: Unit Tests (20+ 테스트)

## What's Left to Build 📋
### Week 3+: Advanced Features
- [ ] Item & Inventory system
- [ ] Equipment system
- [ ] Quest system
- [ ] Guild/Social features

## Current Status 🎯
**Date**: 2025-10-14

**Phase**: Week 2 진행 중 (Feature 2: Battle System Core Logic)

**Recent Achievement**:
- ✅ AttackSpeed 필드 추가 완료 (Character, Monster)
- ✅ 전투 공식 설계 완료 (실시간 DPS 방식)
- ✅ 마이그레이션 생성: `AddAttackSpeedToCharacterAndMonster`
- ✅ CLAUDE.md 업데이트 (Jenkins CI/CD 파이프라인 문서화)
- ✅ Checkpoint 실행 완료 (메모리 저장, Unity 문서 비교)

**Next Steps**:
1. BattleService 인터페이스 및 DTO 생성 (보일러플레이트)
2. 사용자와 함께 BattleService 핵심 전투 로직 구현
3. Unity 문서 업데이트 (스탯 분배 API 제거, Battle API 추가)

**Important Findings**:
- ⚠️ Unity 문서와 서버 구현 불일치 발견
  - `PUT /api/character/{id}/stats` (스탯 분배)가 문서에는 "완료"로 표시되었으나 실제로는 제거됨
  - 이유: 커밋 `8cf2ebe`에서 자동 성장 방식으로 리팩토링
  - 조치 필요: Unity 문서에서 스탯 분배 API 제거 및 BREAKING CHANGE 마킹

**Tech Debt**:
- Unity 문서 동기화 필요 (스탯 분배 API 제거 반영)

**Blocked Issues**: 없음
