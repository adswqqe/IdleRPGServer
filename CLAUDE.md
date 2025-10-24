# CLAUDE.md

## 🎓 Project Purpose

**이 프로젝트는 학습 목적의 프로젝트입니다.**

**학습 목표**:
- Clean Architecture 패턴 이해 및 실습
- ASP.NET Core 8.0 백엔드 개발
- EF Core + PostgreSQL 데이터 액세스
- Unity 클라이언트 연동 (서버-클라이언트 통합 학습)
- JWT 인증, 테스트 코드 작성

**핵심 원칙**:
- ✅ **아키텍처와 코드 구조**에 집중
- ✅ 게임 밸런스(확률, 재화 비용 등)는 **AI가 합리적인 기본값 제안** (학습자는 그대로 사용)
- ✅ **TODO(human)**은 아키텍처 학습 포인트에만 사용 (계층 분리, 패턴 선택, 데이터 모델링)

---

## Language and Communication
**모든 응답은 한국어로 제공**. 코드와 명령어는 원문 유지.

**Architecture Layers** (Dependency: API → Application → Domain):
- **API**: Controllers, Middleware, SignalR
- **Application**: Services, DTOs (MediatR, FluentValidation, AutoMapper)
- **Domain**: Entities, Business rules
- **Infrastructure**: Repositories (EF Core + PostgreSQL)

## Critical Rules

### 🗃️ Database Migration
**마이그레이션 형식**: `IdleRPG.Infrastructure/migration.sql` (단일 파일, Idempotent 패턴)

## Development Standards

**핵심 원칙**:
- **Feature Development Order**: Domain → Application → Infrastructure → API → Tests
- **Naming**: PascalCase (클래스), camelCase (변수), _camelCase (private 필드)

## Kiro Workflow (Spec-Driven Development)

**핵심**: 코드 작성 전 요구사항-설계-작업을 체계적으로 문서화하는 방법론

### Spec 사이즈 (AI 자동 판단 + 사용자 승인)
- **S (Small)**: 단순 CRUD, 단일 엔드포인트 → 인라인 구현 + 5줄 PR 템플릿
- **M (Medium)**: 2-3 엔드포인트, 중간 로직 → spec-lite.md (단일 파일)
- **L (Large)**: 복잡한 시스템, 새 기술 → Full Spec (requirements/design/tasks)

**자동 승격 트리거**:
- **S→M**: 비즈니스 로직, 외부 API, 복잡한 쿼리 (Join 3+)
- **M→L**: 새 기술 도입, 스키마 마이그레이션, 외부 서비스 연동, 성능/보안 요구사항

### 워크플로우 요약
```
S: PR 템플릿 → 구현 → Unity 문서
M: spec-lite.md → [Spike/ADR] → /spec-review (70점+) → 구현
L: requirements.md → [Spike 최대 3개] → design.md (/spec-review 70점+) → tasks.md (/spec-review 90점+) → /spec-execute
```

### Spike & ADR (조건부)
- **Spike**: 새 기술 첫 도입, 성능 검증, 대안 비교 실험 시 (사용자 승인 필수)
- **ADR**: 외부 서비스 도입, 스키마 마이그레이션, 인프라 변경, 보안/성능/크로스컷팅 영향 시

> 📖 **상세 규칙**: `.claude/commands/spec-*.md` 참조 (사이즈 판단, 승격 프로세스, 템플릿)

### 품질 검증
- **M**: `/spec-review` → 70점 이상
- **L**: design.md (70점+), tasks.md (90점+)

### 📚 Commands

#### Spec 생성
- `/spec-init {feature-name}` - Requirements 생성 (L 사이즈)
- `/spec-init-lite {feature-name}` - spec-lite.md 생성 (M 사이즈)
- `/spec-design {feature-name}` - Design 생성
- `/spec-tasks {feature-name}` - Tasks 생성

#### Spec 실행 & 관리
- `/spec-execute {feature-name} {task-id}` - Task 실행
- `/spec-review {feature-name} [--format summary|detailed]` - 품질 검증 및 상세 보고서
- `/spec-status {feature-name}` - 기본 상태 조회
- `/spec-list` - 전체 Spec 목록

## Reference

### 시스템 문서 (_system/)
- **Architecture**: `.claude/memories/_system/architecture.md` - Clean Architecture 패턴
- **Game Design**: `.claude/memories/_system/game-design.md` - 게임 시스템 우선순위
- **Tech Stack**: `.claude/memories/_system/tech-stack.md` - 기술 스택
- **API Standards**: `.claude/memories/_system/api-standards.md` - 개발 표준
- **Roadmap**: `.claude/memories/_system/roadmap.md` - 프로젝트 진행 상황 (20개 시스템)

### Kiro 워크플로우
- **Commands**: `.claude/commands/spec-*.md` (8개)
- **Templates**: `.claude/memories/kiro-system-templates/` (requirements, design, tasks, spec-lite, ADR, spike)

### 기타
- **Learning Plan**: `docs/learning/PROJECT_ROADMAP.md` - 8주 학습 전략 (상세)
- **Deployment**: `docs/jenkins/DEPLOYMENT_GUIDE.md`
- **Unity Docs**: `docs/unity/UNITY_DOCUMENTATION_GUIDE.md`
- **PRD**: `docs/MUSHROOM_GAME_PRD.md`