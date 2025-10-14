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
- [x] **Feature 1**: Monster 엔티티 및 시딩 ✅ (2025-10-14)
  - Monster 엔티티 생성 (5종: 슬라임, 고블린, 오크, 트롤, 드래곤)
  - MonsterConfiguration (EF Core)
  - 마이그레이션 적용 및 EC2 배포 완료
- [x] **AttackSpeed 구현** ✅ (2025-10-14)
  - CharacterStats Value Object에 AttackSpeed 추가
  - Monster 엔티티에 AttackSpeed, CritRate, CritDamage, Evasion 추가
  - 마이그레이션 생성: `AddAttackSpeedToCharacterAndMonster`
  - 몬스터별 차별화된 전투 스타일 (0.6f ~ 1.2f)
- [x] **Feature 2**: Battle System Core Logic ✅ (2025-10-14)
  - BattleService 구현 (전투 시뮬레이션, DPS 계산)
  - IBattleService 인터페이스 정의
  - BattleResultDto, StartBattleRequest DTO 생성
  - 실시간 DPS 계산 로직 구현 (Attack × AttackSpeed)
  - 크리티컬/회피 확률 처리 구현
  - 승리 시 자동 보상 지급 (경험치 + 골드)
- [x] **Feature 3**: Battle Controller & API ✅ (2025-10-14)
  - BattleController 구현 (`POST /api/battle/start`)
  - 캐릭터 소유권 검증 로직 추가
  - Swagger 문서화 완료
  - 에러 핸들링 (400, 403, 404, 500)
- [ ] **Feature 4**: Character Schema Update (Gold, LastLoginTime) ← 현재 진행 중
  - Gold 필드 추가 (마이그레이션 대기)
  - LastLoginTime 필드 추가 (마이그레이션 대기)
- [ ] **Feature 5**: Offline Reward System
- [ ] **Feature 6**: Idle Progress Background Service
- [ ] **Feature 7**: Battle Log System
- [x] **Feature 8**: Unity Documentation Update ✅ (2025-10-14)
  - API_SPEC_FOR_UNITY.md에 Battle API 추가
  - CharacterStats 구조 변경 반영
  - 스탯 분배 API 삭제 표시
- [ ] **Feature 9**: Unit Tests (20+ 테스트)

## What's Left to Build 📋
### Week 3+: Advanced Features
- [ ] Item & Inventory system
- [ ] Equipment system
- [ ] Quest system
- [ ] Guild/Social features

## Current Status 🎯
**Date**: 2025-10-14 20:50 KST

**Phase**: Week 2 진행 중 - 배포 인프라 정리 완료, Feature 4 진행 중

**Recent Achievement**:
- ✅ **Jenkins CI/CD 배포 문제 해결** (파일 불일치, DB 연결, 보안 설정)
  - Jenkinsfile을 `docker-compose.production.yml` 사용하도록 통일
  - RDS PostgreSQL 연결 환경 변수화 (`.env.production`)
  - pgAdmin 제거로 디스크 공간 177MB 절약
  - `EC2-ENV-SETUP.md` 가이드 문서 작성
- ✅ **Week 2 Feature 1 완료** (Monster 엔티티 및 시딩)
  - 5종 몬스터 시딩 (슬라임, 고블린, 오크, 트롤, 드래곤)
  - AttackSpeed 마이그레이션 적용
- ✅ **Week 2 Feature 2-3 완료** (Battle System & API)
  - BattleService 구현 (전투 시뮬레이션, 보상 지급)
  - BattleController 구현 (`POST /api/battle/start`)
  - Unity 문서 업데이트 (전투 API 섹션 추가)
- ✅ **Unity 문서 최신 상태 유지**
  - API_SPEC_FOR_UNITY.md에 Battle API 문서화 완료
  - CharacterStats 구조 변경 (자동 성장) 반영
  - 스탯 분배 API 삭제됨 표시 (⚠️ BREAKING CHANGE)

**Next Steps**:
1. **Week 2 Feature 4**: Character Schema Update (Gold, LastLoginTime 필드 추가) ← 현재 진행 중
2. EC2에 `.env.production` 파일 생성 (수동 작업 필요)
3. Jenkins 빌드 확인 및 배포 검증
4. **Week 2 Feature 5-9**: Offline Rewards, Background Service, Logs, Tests

**Important Findings**:
- ⚠️ **배포 파일 구조 문제 해결됨**
  - 이전: Jenkinsfile은 `docker-compose.prod.yml` 사용 (gitignored, Git에 없음)
  - 현재: `docker-compose.production.yml` 사용 (Git 추적됨)
  - RDS 연결 정보는 `.env.production`으로 분리 (Git 제외)
- ⚠️ **EC2 수동 작업 필요**
  - `.env.production` 파일 생성 (RDS 연결 정보, JWT/Redis 비밀번호)
  - 가이드: `EC2-ENV-SETUP.md` 참고
- ✅ **Unity 문서 동기화 완료**
  - 스탯 분배 API 제거 표시 완료
  - Battle API 추가 완료

**Tech Debt**:
- 없음 (정리 완료)

**Blocked Issues**:
- EC2 `.env.production` 파일 생성 대기 중 (Jenkins 빌드 실패 예상)
