# How Kiro Works - IdleRPG 프로젝트용 가이드

> **Kiro**는 Amazon에서 사용하는 Spec-Driven Development 방법론입니다.  
> 이 문서는 IdleRPG 프로젝트에 최적화된 Kiro 워크플로우를 설명합니다.

---

## 🎯 Core Philosophy

**"코드를 작성하기 전에, 무엇을 만들지 완벽히 이해하라"**

Kiro는 다음 세 단계로 기능을 체계적으로 구축합니다:

1. **Requirements** - 무엇을 만들 것인가? (What)
2. **Design** - 어떻게 만들 것인가? (How)
3. **Tasks** - 단계별로 어떻게 구현할 것인가? (Step-by-step)

---

## 📁 Spec Structure

각 기능은 `.claude/memories/specs/{feature-name}/` 폴더에 3개의 문서를 가집니다:

```
.claude/memories/
├── kiro-system-templates/       # 템플릿 (재사용)
│   ├── requirements-template.md
│   ├── design-template.md
│   ├── tasks-template.md
│   └── how-kiro-works.md        # 이 문서
└── specs/                       # 실제 스펙
    ├── skill-gacha/
    │   ├── requirements.md       # 요구사항 정의
    │   ├── design.md            # 기술 설계
    │   └── tasks.md             # 구현 작업 목록
    ├── pet-system/
    │   ├── requirements.md
    │   ├── design.md
    │   └── tasks.md
    └── ...
```

---

## 🔄 Workflow Process

### Phase 1: Requirements Gathering 📋

**목표**: 사용자 관점에서 기능의 가치와 수용 기준 정의

**작업 순서:**
1. Claude가 초안 작성 (`requirements-template.md` 기반)
2. **사용자 스토리** 작성:
   - "As a [역할], I want [기능], so that [목적]"
3. **Acceptance Criteria** 작성 (EARS 형식):
   - "WHEN [조건] THEN system SHALL [동작]"
4. **게임 디자인** 결정:
   - 확률, 보상, 밸런스 → **TODO(human)** 마커 표시
5. **사용자 승인** 필요 → "Approved" 상태로 변경

**예시 (스킬 가챠):**
```markdown
### US-1: 스킬 랜덤 획득
**As a** 플레이어  
**I want** 크리스탈을 사용해 스킬을 랜덤으로 획득하고 싶다  
**So that** 캐릭터를 성장시키고 전투력을 높일 수 있다

**Acceptance Criteria:**
- WHEN 플레이어가 1회 가챠를 요청 THEN system SHALL 크리스탈 50개를 차감하고 1개의 스킬을 부여
- WHEN 100회 가챠 후 전설 미획득 THEN system SHALL 천장 시스템으로 전설 등급 보장
- **TODO(human)**: 확률 - Common 60%, Rare 30%, Epic 9%, Legendary 1%
```

**완료 조건:**
- [ ] 모든 사용자 스토리 정의
- [ ] EARS 형식 수용 기준 작성
- [ ] 게임 디자인 결정 완료 (TODO(human) 해소)
- [ ] 사용자 승인 ✅

---

### Phase 2: Design Documentation 🏗️

**목표**: Clean Architecture 계층별로 기술 구현 방법 설계

**작업 순서:**
1. Claude가 초안 작성 (`design-template.md` 기반)
2. **Architecture Overview** 작성:
   - API / Application / Domain / Infrastructure 계층별 책임
3. **Data Model** 설계:
   - EF Core Entity, 관계, 인덱스
4. **API Design**:
   - RESTful 엔드포인트, Request/Response DTO
5. **Business Logic** 설계:
   - 알고리즘, 계산식 → 복잡한 부분은 **TODO(human)**
6. **Testing Strategy** 계획
7. **사용자 승인** 필요

**IdleRPG 특화 고려사항:**
- Clean Architecture 의존성 규칙 준수
- Repository Pattern, Service Pattern 사용
- JWT 인증, BCrypt 암호화
- PostgreSQL + EF Core
- Unity 클라이언트 연동 (DTO 호환성)

**완료 조건:**
- [ ] 모든 Requirements 항목 커버
- [ ] 계층별 컴포넌트 정의
- [ ] TODO(human) 비즈니스 로직 결정
- [ ] 사용자 승인 ✅

---

### Phase 3: Task Planning 📝

**목표**: 구현 가능한 단위 작업으로 분해

**작업 순서:**
1. Claude가 초안 작성 (`tasks-template.md` 기반)
2. **Milestone 기반 작업 분류**:
   - Milestone 1: Domain Layer (Entity, Enum, Domain Service)
   - Milestone 2: Infrastructure Layer (Repository, EF Config)
   - Milestone 3: Application Layer (DTO, Service)
   - Milestone 4: API Layer (Controller)
   - Milestone 5: Database (Migration, Seeder)
   - Milestone 6: Testing & Documentation (Unit Test, Unity Docs)
3. **각 작업에 포함**:
   - [ ] 체크리스트 (완료 여부 추적)
   - ⏱️ 예상 소요 시간
   - Requirements/Design 추적성 (US-1, US-2 참조)
4. **의존성 순서 고려**: Domain → Infrastructure → Application → API
5. **사용자 승인** 필요

**작업 작성 규칙:**
- ✅ 독립적으로 완료 및 테스트 가능한 단위
- ✅ 명확한 결과물 정의 (파일 생성, 메서드 구현 등)
- ✅ Claude가 자동화할 수 있는 수준 (CRUD, Config)
- ❌ "시스템 이해하기", "조사하기" 같은 모호한 작업

**완료 조건:**
- [ ] 모든 Design 컴포넌트가 Task로 변환
- [ ] 작업 순서가 의존성 준수
- [ ] 예상 시간 산정 완료
- [ ] 사용자 승인 ✅

---

## 🚀 Task Execution

### Claude Code의 "Start task" 기능 사용

**방법 1: tasks.md에서 직접 실행**
1. `.claude/memories/specs/{feature}/tasks.md` 열기
2. 각 작업 옆 **"Start task"** 버튼 클릭
3. Claude가 해당 작업만 집중 수행
4. 완료 후 체크박스 체크 ✅

**방법 2: 순차 실행 요청**
```
"skill-gacha의 tasks.md에서 1.1부터 순서대로 실행해줘"
```

**실행 중 원칙:**
- ✅ 한 번에 1개 작업만 수행 (집중)
- ✅ 작업 완료 후 즉시 체크박스 업데이트
- ✅ TODO(human) 발견 시 사용자에게 질문
- ✅ 블로커 발생 시 "Notes - Blockers"에 기록

---

## 🎮 IdleRPG 프로젝트 특화 규칙

### 1. TODO(human) 사용 시점

**Claude 자동 처리:**
- CRUD 로직, Repository, DTO
- EF Core Configuration
- SQL Migration
- 단위 테스트 (기본 시나리오)
- Unity 문서 생성

**사용자와 협업 (TODO(human)):**
- 게임 밸런스 (확률, 보상량, 비용)
- 비즈니스 로직 (강화 실패 시 처리, 매칭 알고리즘)
- 디자인 결정 (UI/UX 관련)

**프로세스:**
1. Requirements/Design에서 TODO(human) 마커 표시
2. 사용자가 결정하면 문서 업데이트 → "Approved"
3. Tasks 실행 중에는 코드에 TODO(human) 주석
4. 사용자가 구현하면 주석 제거

---

### 2. Clean Architecture 계층 순서

**반드시 이 순서로 작업:**
1. **Domain Layer**: Entity, Enum, Domain Service, Value Object
2. **Infrastructure Layer**: Repository 구현, EF Core Config
3. **Application Layer**: DTO, Application Service
4. **API Layer**: Controller

**이유:**
- Domain은 다른 계층에 의존하지 않음
- Infrastructure는 Domain의 인터페이스를 구현
- API는 Application의 서비스를 호출

---

### 3. Database Migration 규칙

**CRITICAL**: Jenkins CI/CD 자동 배포

**프로세스:**
1. `IdleRPG.Infrastructure/migration.sql` 파일에 추가
2. **Idempotent 패턴 사용**:
   ```sql
   DO $EF$ BEGIN
       IF NOT EXISTS(...) THEN
           -- CREATE/ALTER 구문
       END IF;
   END $EF$;
   ```
3. Git 커밋 → Jenkins가 자동 적용
4. **절대 금지**: `dotnet ef database update` 로컬 실행

**로컬 테스트:**
- `IdleRPG.Infrastructure/Migrations/` 폴더에 별도 `.sql` 파일 생성
- 로컬 DB에서 테스트 후 `migration.sql`에 반영

**참고**: `CLAUDE.md - Database Migration`

---

### 4. Unity 문서 필수

**CRITICAL**: API/DTO 추가/수정 시 Unity 문서 자동 생성

**체크리스트:**
1. `../IdleRPGClient/Docs/unity/{feature}/` 폴더 생성
2. `API_SPEC.md` 작성:
   - Endpoint, Request/Response
   - Unity C# UnityWebRequest 예제
3. `DTOs.cs` 작성:
   - `[JsonProperty]` 속성 사용
   - Newtonsoft.Json 호환
4. `unity/README.md` 메인 인덱스 업데이트

**참고**: `docs/unity/UNITY_DOCUMENTATION_GUIDE.md`

---

## 🔍 Iterative Approval

**핵심 원칙**: 각 Phase를 명시적으로 승인해야 다음 단계 진행

### Requirements → Design 전환 조건
- [ ] 모든 사용자 스토리 작성 완료
- [ ] EARS 형식 수용 기준 명확
- [ ] TODO(human) 게임 디자인 결정 완료
- [ ] 사용자가 "Requirements 승인" 명시

### Design → Tasks 전환 조건
- [ ] 모든 Requirements 항목 커버
- [ ] 계층별 컴포넌트 정의 완료
- [ ] TODO(human) 기술 결정 완료
- [ ] 사용자가 "Design 승인" 명시

### Tasks → Implementation 전환 조건
- [ ] 모든 Design 컴포넌트가 Task로 변환
- [ ] 작업 순서 검증
- [ ] 사용자가 "Tasks 승인" 명시

**승인 없이 진행하지 않음!**

---

## 📊 Progress Tracking

### tasks.md의 Progress Overview

```markdown
## 📊 Progress Overview

**전체 진행률**: 12/25 (48%)

| Milestone | 작업 수 | 완료 | 진행률 |
|-----------|---------|------|--------|
| Domain Layer | 7 | 5 | 71% |
| Infrastructure Layer | 5 | 3 | 60% |
| Application Layer | 4 | 2 | 50% |
| API Layer | 3 | 1 | 33% |
| Database | 3 | 1 | 33% |
| Testing & Documentation | 3 | 0 | 0% |
```

**실시간 업데이트:**
- 작업 완료 시 즉시 체크박스 체크
- Progress Overview 표 업데이트
- 블로커 발생 시 "Notes - Blockers" 기록

---

## 🎓 Best Practices

### 1. Requirement Traceability (요구사항 추적성)

**모든 작업은 Requirements로 추적 가능해야 함:**

```markdown
# requirements.md
### US-1: 스킬 랜덤 획득

# design.md
**Requirements**: [US-1]
### API Design - POST /api/skills/gacha

# tasks.md
**Requirements**: [US-1]
### 4.1 Create SkillController
```

**장점:**
- 왜 이 코드가 필요한지 명확
- 불필요한 기능 제거 가능
- 테스트 시나리오 작성 용이

---

### 2. Incremental Development (점진적 개발)

**작은 단위로 자주 커밋:**
- ❌ "Implement skill gacha system" (20개 파일 한 번에)
- ✅ "Add SkillTemplate entity" (1개 파일)
- ✅ "Add GachaLogicService with unit tests" (2개 파일)

**장점:**
- 버그 발생 시 롤백 쉬움
- 코드 리뷰 품질 향상
- 진행 상황 가시성

---

### 3. Code-Focused Tasks (코드 중심 작업)

**Task는 실행 가능한 활동만 포함:**

**Good ✅:**
- "Create SkillTemplate.cs entity with Id, Name, Rarity properties"
- "Implement DetermineRarity() method in GachaLogicService"
- "Write unit tests for probability distribution (60/30/9/1%)"

**Bad ❌:**
- "Understand the existing gacha system"
- "Research best practices for random generation"
- "Think about skill balance"

---

### 4. TODO(human) Resolution (협업 지점)

**Requirements/Design 단계에서 해소:**

```markdown
# requirements.md (초안)
**TODO(human)**: 확률 결정 필요

# requirements.md (승인 후)
**확률**: Common 60%, Rare 30%, Epic 9%, Legendary 1%
```

**Tasks 실행 중 발견 시:**

```csharp
// GachaLogicService.cs
public SkillRarity DetermineRarity(int pityCount)
{
    // TODO(human): 확률 계산 로직 구현
    // - Common: 60%, Rare: 30%, Epic: 9%, Legendary: 1%
}
```

**사용자가 구현 후 주석 제거**

---

## 🆕 Creating a New Spec

### CLI 명령어 (예정)
```bash
# 새 스펙 생성 (향후 자동화)
kiro new skill-gacha

# 템플릿에서 복사
cp -r .claude/memories/kiro-system-templates .claude/memories/specs/skill-gacha
mv .claude/memories/specs/skill-gacha/requirements-template.md .claude/memories/specs/skill-gacha/requirements.md
mv .claude/memories/specs/skill-gacha/design-template.md .claude/memories/specs/skill-gacha/design.md
mv .claude/memories/specs/skill-gacha/tasks-template.md .claude/memories/specs/skill-gacha/tasks.md
```

### Claude에게 요청
```
"펫 시스템 기능을 Kiro 방식으로 시작하고 싶어. 
requirements부터 작성해줄래?"
```

Claude가 자동으로:
1. `.claude/memories/specs/pet-system/` 폴더 생성
2. `requirements-template.md` 복사하여 초안 작성
3. 사용자 스토리, EARS 수용 기준 제안
4. TODO(human) 마커로 결정 필요 부분 표시

---

## 📚 Reference Documents

### 프로젝트 문서
- `CLAUDE.md`: 전체 개발 규칙, Migration, Unity 문서
- `docs/learning/PROJECT_ROADMAP.md`: 로드맵
- `docs/jenkins/DEPLOYMENT_GUIDE.md`: 배포 가이드
- `docs/unity/UNITY_DOCUMENTATION_GUIDE.md`: Unity 문서 작성법

### Kiro 템플릿
- `.claude/memories/kiro-system-templates/requirements-template.md`
- `.claude/memories/kiro-system-templates/design-template.md`
- `.claude/memories/kiro-system-templates/tasks-template.md`

---

## ✅ Quick Start Checklist

### 새 기능 시작 시
- [ ] Feature 이름 결정 (kebab-case: `skill-gacha`)
- [ ] `.claude/memories/specs/{feature}/` 폴더 생성
- [ ] `requirements.md` 작성 및 승인
- [ ] `design.md` 작성 및 승인
- [ ] `tasks.md` 작성 및 승인
- [ ] Task 실행 시작

### 각 Phase 승인 시 확인
- [ ] 모든 TODO(human) 해소됨
- [ ] 이전 Phase 내용 모두 커버됨
- [ ] 문서 상태 "Approved"로 변경
- [ ] 다음 Phase 시작 전 사용자 명시 승인

---

**마지막 업데이트**: 2025-10-23  
**버전**: 1.0  
**IdleRPG 프로젝트 최적화 완료**
