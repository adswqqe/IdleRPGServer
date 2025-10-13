# Week 2 진행사항 체크포인트 (2025-10-13)

## 프로젝트 현황

**프로젝트명**: IdleRPG Server - Week 2 Idle Game Loop & Progression System
**날짜**: 2025년 10월 13일
**전체 진행률**: 22% (2/9 Features 완료)

## ✅ 완료된 작업 (Week 2)

### Feature 1: Monster Entity & Repository ✅
**완료일**: 2025-10-13
**내용**:
- Monster 엔티티 생성 (`IdleRPG.Domain/Entities/Monster.cs`)
  - 필드: Id, Name, Level, MaxHealth, Attack, Defense, ExperienceReward, GoldReward
- MonsterConfiguration (EF Core)
  - Level 인덱스 생성 (범위 조회 최적화)
  - 기본값 설정 (CreatedAt, UpdatedAt = CURRENT_TIMESTAMP)
- 5종 몬스터 시딩 데이터
  - 슬라임 (Lv1): HP 50, ATK 10, DEF 5, EXP 10, Gold 5
  - 고블린 (Lv5): HP 150, ATK 30, DEF 15, EXP 50, Gold 25
  - 오크 (Lv10): HP 300, ATK 60, DEF 30, EXP 100, Gold 50
  - 트롤 (Lv15): HP 500, ATK 100, DEF 50, EXP 150, Gold 75
  - 드래곤 (Lv20): HP 1000, ATK 200, DEF 100, EXP 300, Gold 150
- 마이그레이션 생성 및 EC2/RDS 적용 완료

### Feature 4: Character Schema Update (Gold, LastLoginTime) ✅
**완료일**: 2025-10-13
**내용**:
- Character 엔티티 필드 추가
  - Gold (long): 캐릭터 보유 골드, 기본값 0
  - LastLoginTime (DateTime): 마지막 로그인 시간, 기본값 CURRENT_TIMESTAMP
- CharacterConfiguration 업데이트
  - Gold: `HasDefaultValue(0)`
  - LastLoginTime: `HasDefaultValueSql("CURRENT_TIMESTAMP")`
- EF Core 마이그레이션 생성 (`20251013080752_AddGoldAndLastLoginToCharacter`)
- migration.sql 업데이트 (멱등성 보장)
- **Jenkins CI/CD 파이프라인 개선**
  - Git Pull → **Database Migration** → Docker Deploy 순서 확립
  - psql로 RDS PostgreSQL에 직접 마이그레이션 실행
  - 앞으로 Git Push만으로 자동 배포 + 마이그레이션 적용

**기술적 결정**:
- Gold 타입: `long` 선택 (최대 900경, 장기 플레이 대비)
- LastLoginTime 업데이트 시점: 로그인 시에만 (오프라인 보상 계산 기준)

## ⏭️ 다음 작업 (순서대로)

### Feature 2: Battle System Core Logic (다음 작업)
**예상 소요**: 2-3시간
**작업 내용**:
- BattleService 구현 (Application Layer)
- 전투 공식 설계
  - 데미지 계산: Max(1, 공격력 - 방어력)
  - 턴제 전투 시뮬레이션
  - 승패 판정
- BattleResultDto 정의
- 보상 지급 로직 (경험치, 골드)

**설계 필요 항목** (함께 결정):
- 전투 공식 세부사항 (크리티컬, 회피 여부)
- 승패 조건 (HP 0, 타임아웃 등)
- 보상 배율 조정

### Feature 3: Battle Controller & API
**예상 소요**: 1-2시간
**작업 내용**:
- BattleController 생성 (API Layer)
- POST `/api/battle/start` - 전투 시작
- GET `/api/battle/random-monster?level={level}` - 랜덤 몬스터 선택

### Feature 5: Offline Reward System
**예상 소요**: 2-3시간
**작업 내용**:
- OfflineRewardService 구현
- 보상 계산 공식 (분당 Exp/Gold)
- 최대 누적 시간 제한 (8시간)
- API 엔드포인트
  - GET `/api/rewards/offline/{characterId}` - 조회
  - POST `/api/rewards/offline/{characterId}/claim` - 수령

### Feature 6: Idle Progress Background Service
**예상 소요**: 3-4시간
**작업 내용**:
- IHostedService 구현
- 1분마다 활성 캐릭터 자동 전투
- Redis 분산 락 (다중 인스턴스 대비)

### Feature 7: Battle Log System
**예상 소요**: 2시간
**작업 내용**:
- BattleLog 엔티티 생성
- BattleLogRepository 구현
- GET `/api/battle/logs/{characterId}` - 전투 기록 조회

### Feature 8: Unity Documentation Update
**예상 소요**: 1시간
**작업 내용**:
- 모든 신규 API 문서화
- Unity-DTOs.cs 업데이트
- Quick Reference 테이블 업데이트

### Feature 9: Unit Tests
**예상 소요**: 3-4시간
**작업 내용**:
- BattleService 테스트 (10+ 케이스)
- OfflineRewardService 테스트 (10+ 케이스)
- xUnit, Moq, FluentAssertions 사용

## 📊 Week 2 Feature 진행 상태

```
✅ Feature 1: Monster Entity & Repository
✅ Feature 4: Character Schema Update (Gold, LastLoginTime)
⏭️ Feature 2: Battle System Core Logic ← 다음 작업
📋 Feature 3: Battle Controller & API
📋 Feature 5: Offline Reward System
📋 Feature 6: Idle Progress Background Service
📋 Feature 7: Battle Log System
📋 Feature 8: Unity Documentation Update
📋 Feature 9: Unit Tests (20+ 테스트)
```

## 🏗️ 현재 아키텍처 상태

### 배포 환경
- **EC2**: t3.micro (13.209.66.253)
- **RDS PostgreSQL**: idlerpg-dev (ap-northeast-2)
- **Jenkins CI/CD**: 자동 배포 + 마이그레이션
- **Docker Compose**: API + Redis

### CI/CD 파이프라인
```
GitHub Push
  ↓
Jenkins (EC2)
  ├─ 1. Git Pull (5분)
  ├─ 2. Database Migration (5분) ← 새로 추가됨
  ├─ 3. Docker Build & Deploy (20분)
  └─ 4. Verification (2분)
```

### 데이터베이스 스키마 (현재)
- **Players**: 플레이어 계정
- **RefreshTokens**: JWT 토큰
- **Characters**: 캐릭터 (Gold, LastLoginTime 추가됨)
- **Monsters**: 몬스터 (5종 시딩 완료)

## 🎯 학습 목표 (Stage 3: Async Performance)

### 이번 주 학습 내용
- ✅ EF Core 마이그레이션 관리
- ✅ Docker 환경 데이터베이스 마이그레이션 전략
- ✅ Jenkins CI/CD 파이프라인 구축
- ⏭️ IHostedService / BackgroundService (Feature 6에서 학습 예정)
- ⏭️ async/await 심화
- ⏭️ 동시성 처리 (Redis 분산 락)

## 🔧 기술 스택 현황

### 완전 구현됨
- ASP.NET Core 8.0 Web API
- PostgreSQL + EF Core 9.0
- JWT Authentication
- Docker + Docker Compose
- Jenkins CI/CD
- Clean Architecture (4 Layers)

### 준비됨 (아직 미사용)
- Redis (설치됨, 캐싱/세션/분산락 대기)
- MediatR (설치됨, 미설정)
- AutoMapper (설치됨, 미설정)
- FluentValidation (설치됨, 미설정)

## 📝 중요 기술 결정 사항

### 1. 마이그레이션 전략
- **방식**: SQL 스크립트 (`migration.sql`)
- **적용**: Jenkins 파이프라인에서 자동 실행
- **안전성**: IF NOT EXISTS 패턴으로 멱등성 보장
- **이유**: 
  - 다중 인스턴스 환경에서 안전
  - Program.cs의 자동 마이그레이션은 Race Condition 위험

### 2. Gold 필드 타입
- **선택**: long (bigint)
- **이유**: Idle RPG 특성상 장기간 누적, int는 21억 한계

### 3. LastLoginTime 업데이트 시점
- **선택**: 로그인 시에만
- **이유**: 오프라인 보상 계산의 명확한 기준점

### 4. Jenkins vs GitHub Actions
- **현재**: Jenkins (EC2에 설치됨)
- **미래**: 10k+ 동접 시 GitHub Actions로 전환 고려
- **이유**: Jenkins 서버도 스케일해야 하므로 관리형 서비스가 유리

## 🚀 다음 세션 시작 방법

```bash
# 1. Serena 활성화
activate_project IdleRPGServer

# 2. Week 2 메모리 읽기
read_memory week2-workflow-and-collaboration-rules

# 3. 현재 체크포인트 확인
read_memory week2-progress-checkpoint-2025-10-13

# 4. Feature 2 시작
# PRD 확인: .taskmaster/docs/week2-prd.txt (Feature 2 섹션)
```

## 📌 다음 작업 제안

**즉시 시작 가능**: Feature 2 - Battle System Core Logic
- 전투 공식 설계 (함께 논의)
- BattleService 구현 (자동 작업)
- BattleResultDto 생성 (자동 작업)

**예상 소요 시간**: 2-3시간
**협업 필요**: 전투 공식 세부사항 결정 (크리티컬, 회피 등)

---

**작성일**: 2025-10-13
**작성자**: Claude Code
**다음 리뷰**: Feature 2 완료 후
