# Checkpoint Progress - 2025-10-14 20:50 KST

## Current Phase
Week 2 - Idle Game Loop & Progression System (Feature 4 진행 중)

## Completed Features
- ✅ **Week 1**: Character System 완료
  - JWT 인증 시스템 (5개 엔드포인트)
  - 캐릭터 CRUD API (6개 엔드포인트)
  - 자동 성장 시스템 (레벨업 시 스탯 자동 증가)
  - CharacterService 단위 테스트 17개

- ✅ **Week 2 Feature 1**: Monster Entity & Repository (2025-10-14)
  - Monster 엔티티 생성 (5종: 슬라임, 고블린, 오크, 트롤, 드래곤)
  - MonsterConfiguration (EF Core)
  - 마이그레이션 적용 및 EC2 배포 완료

- ✅ **Week 2 Feature 2**: Battle System Core Logic (2025-10-14)
  - BattleService 구현 (전투 시뮬레이션, DPS 계산)
  - 실시간 DPS 계산 로직 (Attack × AttackSpeed)
  - 크리티컬/회피 확률 처리
  - 승리 시 자동 보상 지급 (경험치 + 골드)

- ✅ **Week 2 Feature 3**: Battle Controller & API (2025-10-14)
  - BattleController 구현 (`POST /api/battle/start`)
  - 캐릭터 소유권 검증
  - Swagger 문서화 완료

- ✅ **Week 2 Feature 8**: Unity Documentation Update (2025-10-14)
  - API_SPEC_FOR_UNITY.md에 Battle API 추가
  - CharacterStats 구조 변경 반영
  - 스탯 분배 API 삭제 표시

- ✅ **배포 인프라 정리** (2025-10-14)
  - Jenkins CI/CD 파일 불일치 문제 해결
  - docker-compose.production.yml 통일
  - RDS 연결 환경 변수화 (.env.production)
  - pgAdmin 제거로 디스크 공간 177MB 절약
  - EC2-ENV-SETUP.md 가이드 작성

## In Progress
- 🚧 **Week 2 Feature 4**: Character Schema Update
  - Gold 필드 추가 (마이그레이션 대기)
  - LastLoginTime 필드 추가 (마이그레이션 대기)

## Recent Achievements
1. Jenkins CI/CD 배포 문제 완전 해결
   - Jenkinsfile을 docker-compose.production.yml 사용하도록 변경
   - 보안 강화: RDS 비밀번호를 .env.production으로 분리
   
2. Battle System API 완성
   - 서버 시뮬레이션 방식 전투
   - 자동 보상 지급 시스템
   - 레벨업 반영
   
3. Unity 문서 최신화
   - Battle API 문서 추가
   - DTO 구조 변경 반영
   - BREAKING CHANGE 표시

## Next Steps
1. **Feature 4 완료**: Character에 Gold, LastLoginTime 필드 추가
   - EF Core 마이그레이션 생성
   - CharacterDto 업데이트
   - Unity 문서 동기화

2. **EC2 배포 완료**
   - .env.production 파일 생성 (수동 작업)
   - Jenkins 빌드 확인

3. **Week 2 나머지 Feature 진행**
   - Feature 5: Offline Reward System
   - Feature 6: Idle Progress Background Service
   - Feature 7: Battle Log System
   - Feature 9: Unit Tests (20+ 테스트)

## Blocked Issues
- ⚠️ EC2에 .env.production 파일 생성 필요
  - RDS 연결 정보 (DB_HOST, DB_NAME, DB_USER, DB_PASSWORD, DB_PORT)
  - JWT_SECRET_KEY (64자 이상)
  - REDIS_PASSWORD (64자 이상)
  - 가이드: EC2-ENV-SETUP.md 참고
  - Jenkins 빌드는 파일 생성 후 성공 예상

## Tech Debt
- 없음 (배포 인프라 정리 완료)

## Important Notes
- **배포 파일 구조 표준화 완료**
  - Jenkinsfile → docker-compose.production.yml 사용
  - RDS 연결 정보 → .env.production (Git 제외)
  - 로컬 PostgreSQL 제거, RDS만 사용

- **Unity 문서 동기화 전략**
  - 서버 API 변경 시 즉시 API_SPEC_FOR_UNITY.md 업데이트
  - DTO 구조 변경 시 Unity-DTOs.cs 동기화
  - BREAKING CHANGE 명확히 표시

- **Jenkins CI/CD 파이프라인**
  - Step 1: Git Pull (5min)
  - Step 2: Database Migration (5min)
  - Step 3: Docker Build & Deploy (20min)
  - Step 4: Verification (2min)
