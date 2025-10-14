# 체크포인트: 2025-10-14 프로젝트 진행사항

## 📊 프로젝트 현황 요약

**날짜**: 2025년 10월 14일  
**Week 2 진행률**: 22% (2/9 Features 완료)

## ✅ 완료된 주요 작업

### Week 1: Character System (100% 완료)
- ✅ JWT Authentication (5개 엔드포인트)
- ✅ Character CRUD (6개 엔드포인트)
- ✅ 17개 단위 테스트 통과
- ✅ Unity 문서 작성

### Week 2: Idle Game Loop (진행 중)

#### Feature 1: Monster Entity ✅
- Monster 엔티티 완성
- 5종 몬스터 시딩 (슬라임 → 드래곤)
- EC2/RDS 마이그레이션 적용 완료

#### Feature 4: Character Schema Update ✅
- Gold, LastLoginTime 필드 추가
- AttackSpeed 필드 추가
- 자동 성장 시스템 리팩토링 완료
- Jenkins CI/CD 자동 배포 완료

### 🚨 Breaking Change: 자동 성장 시스템 (2025-10-13)

#### 변경 사항
- **기본 스탯 제거**: Strength, Dexterity, Intelligence, Vitality 삭제
- **전투 스탯 전환**: Attack, Defense, MaxHealth, CritRate, CritDamage, Evasion, AttackSpeed
- **타입 변경**: int → long (방치형 게임 특성)
- **스탯 분배 API 삭제**: `PUT /api/character/{id}/stats` 엔드포인트 제거
- **자동 레벨업**: Level +1 시 Attack +10, Defense +5, MaxHealth +50

#### 영향받은 파일
- Domain: CharacterStats.cs (7 properties)
- Application: CharacterDto.cs
- Infrastructure: CharacterService.cs, CharacterConfiguration.cs
- API: CharacterController.cs (AllocateStats 메서드 제거)
- Tests: CharacterServiceTests.cs (17개 테스트 수정)

## 📝 Unity 문서 업데이트 (2025-10-14)

### 업데이트 내역
**파일**: `../IdleRPGClient/Docs/unity/API_SPEC_FOR_UNITY.md`

#### 변경 사항
1. **구현 상태 테이블 업데이트**
   - 스탯 분배 API: ✅ 완료 → ⚠️ 삭제됨

2. **CharacterData DTO 변경**
   ```diff
   - statPoints: int
   - stats.strength: int
   - stats.dexterity: int
   - stats.intelligence: int
   - stats.vitality: int
   + gold: long
   + lastLoginTime: string
   + stats.attack: long
   + stats.defense: long
   + stats.maxHealth: long
   + stats.critRate: float
   + stats.critDamage: float
   + stats.evasion: float
   + stats.attackSpeed: float
   ```

3. **JSON 응답 예시 갱신**
   - 모든 Character 관련 응답 예시 업데이트
   - 레벨업 규칙 설명 변경 (자동 성장)

4. **버전 이력 추가**
   - v1.2 (2025-10-14): BREAKING CHANGE 기록
   - 자동 성장 시스템 설명 추가

## 🎯 다음 작업 (우선순위순)

### Feature 2: Battle System Core Logic (다음 작업)
**예상 소요**: 2-3시간  
**협업 필요**: 전투 공식 설계

#### 작업 내용
- BattleService 인터페이스 정의
- 전투 공식 구현
  - 데미지 계산: Max(1, Attack - Defense)
  - 크리티컬, 회피 처리
  - 턴제 시뮬레이션
- BattleResultDto 생성
- 경험치/골드 보상 지급

### Feature 3: Battle Controller & API
**예상 소요**: 1-2시간

- `POST /api/battle/start`
- `GET /api/battle/random-monster?level={level}`

### Feature 5-9: 나머지 기능
- Feature 5: Offline Reward System
- Feature 6: Idle Progress Background Service (IHostedService)
- Feature 7: Battle Log System
- Feature 8: Unity Documentation Update
- Feature 9: Unit Tests (20+ 테스트)

## 🏗️ 기술 스택 현황

### 완전 구현됨
- ASP.NET Core 8.0 Web API
- PostgreSQL + EF Core 9.0
- JWT Authentication (Bearer Token)
- Docker + Docker Compose
- Jenkins CI/CD (자동 마이그레이션 + 배포)
- Clean Architecture (4 Layers)
- xUnit + Moq + FluentAssertions

### 설치됨 (미사용)
- Redis (캐싱, 세션, 분산락 대기)
- MediatR (CQRS 준비)
- AutoMapper (DTO 매핑 준비)
- FluentValidation (유효성 검증 준비)

## 📦 배포 환경

### AWS 인프라
- **EC2**: t3.micro (13.209.66.253)
- **RDS PostgreSQL**: idlerpg-dev
- **Jenkins**: 자동 배포 파이프라인

### CI/CD 흐름
```
GitHub Push
  ↓
Jenkins (EC2)
  ├─ 1. Git Pull (5분)
  ├─ 2. Database Migration (5분) ← psql로 RDS 직접 적용
  ├─ 3. Docker Build & Deploy (20분)
  └─ 4. Verification (2분)
```

## 🔍 학습 포인트

### Stage 3: Async Performance (진행 중)
- ✅ EF Core 마이그레이션 전략
- ✅ Docker 환경 DB 관리
- ✅ Jenkins CI/CD 구축
- ⏭️ IHostedService / BackgroundService (Feature 6)
- ⏭️ async/await 심화
- ⏭️ Redis 분산 락

## 📌 중요 결정 사항

### 1. 자동 성장 방식 선택 이유
- 방치형 게임 특성에 맞춤
- 플레이어 경험 단순화
- long 타입으로 exponential growth 지원
- 직업 시스템 확장성 확보

### 2. Gold 타입: long
- 최대값: 922경 (long.MaxValue)
- 방치형 게임 특성상 숫자 급증 대비

### 3. LastLoginTime 업데이트 시점
- 로그인 시에만 업데이트
- 오프라인 보상 계산의 명확한 기준점

### 4. AttackSpeed 추가
- 전투 시스템 준비 (초당 공격 횟수)
- 기본값: 1.0 (1회/초)

## 🔧 다음 세션 준비

### 시작 명령어
```bash
# Serena 활성화
activate_project IdleRPGServer

# Week 2 메모리 읽기
read_memory week2-workflow-and-collaboration-rules
read_memory checkpoint-2025-10-14

# Feature 2 시작
# PRD: .taskmaster/docs/week2-prd.txt
```

### Feature 2 협업 필요 사항
1. 전투 공식 세부 설계
   - 크리티컬 발동 조건
   - 회피 처리 방식
   - 턴 순서 결정 (AttackSpeed 기반?)
2. 승패 조건
   - HP 0 도달
   - 최대 턴 수 제한?
3. 보상 배율
   - 경험치 획득량
   - 골드 획득량

---

**마지막 업데이트**: 2025-10-14  
**다음 체크포인트**: Feature 2 완료 후