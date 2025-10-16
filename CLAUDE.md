# CLAUDE.md

This file provides guidance to Claude Code when working with code in this repository.

## Language and Communication

**The user is Korean and all responses should be provided in Korean.** When explaining code, architecture, or providing guidance, translate technical explanations to Korean while keeping code examples and commands in their original form.

---

## Project Overview

### 🍄 버섯키우기 완전판 - Idle MMORPG

**AI-collaborative learning project** for mastering .NET backend development. Built with ASP.NET Core 8.0 following **Clean Architecture** principles.

- **Project Type**: Learning project (not commercial)
- **Goal**: Master 20+ modern .NET technologies in 8 weeks
- **Strategy**: T-shaped learning (6 core systems at 95% depth + 14 systems at 60-80% breadth)

**Roadmap & Scope**: See `docs/learning/PROJECT_ROADMAP.md` for full 8-week plan and current progress.

### Architecture Layers

- **IdleRPG.API**: Web API layer (Controllers, Middleware, SignalR Hubs)
- **IdleRPG.Application**: Business logic (Services, Commands/Queries, DTOs) - MediatR, AutoMapper, FluentValidation
- **IdleRPG.Domain**: Core domain entities and business rules (20+ entity types)
- **IdleRPG.Infrastructure**: Data access and external services - EF Core + PostgreSQL

**Dependency Flow**: API → Application → Domain, Infrastructure implements Application interfaces.

### Game Concept

**Genre**: Idle MMORPG (Auto-battle, Offline rewards, Social features)

**Core Loop**: Enter Dungeon → Auto Battle → Earn Rewards → Upgrade → PVP → Guild → Boss Raid

**20 Major Systems**: Authentication, Character, Equipment, Combat, Offline Rewards, Dungeon, Enhancement, Skills, Pets, PVP, Friends, Guild, Chat (SignalR), Boss Raid, Quest, Daily Mission, Gacha, Shop, Ranking (Redis), Mail/Events

---

## Development Environment

### Quick Start

```bash
# Local development (recommended)
./dev-start.sh              # Starts PostgreSQL, Redis, pgAdmin
cd IdleRPG.API
dotnet run                  # Run with hot reload

# Full Docker environment
./docker-start.sh           # Starts everything including API
```

**Access Points**:
- Swagger: http://localhost:5172/swagger (HTTP) or https://localhost:7122/swagger (HTTPS)
- pgAdmin: http://localhost:8082 (admin@idlerpg.com / admin123)
- PostgreSQL: localhost:5432 (gamedev / dev123!)

**Detailed Setup**: See `docs/development/DEV_ENVIRONMENT_SETUP.md`

### Building and Testing

```bash
dotnet build IdleRPGServer.sln      # Build solution
dotnet test                          # Run all tests
```

---

## Database Migrations

### Creating Migrations

```bash
cd IdleRPG.Infrastructure
dotnet ef migrations add <MigrationName> --startup-project ../IdleRPG.API
```

### ⚠️ CRITICAL: Production Deployment

**Jenkins CI/CD handles migrations automatically**.

**NEVER** run `dotnet ef database update` on release branches.

**Workflow**:
1. Create migration locally (command above)
2. Commit and push to GitHub
3. Jenkins automatically applies `migration.sql` to RDS

**Details**: See `docs/jenkins/DEPLOYMENT_GUIDE.md`

---

## Key Technologies

- **Framework**: ASP.NET Core 8.0 Web API
- **Database**: PostgreSQL with Entity Framework Core 9.0
- **Cache**: Redis (Distributed locking, Ranking system)
- **Real-time**: SignalR (Chat system)
- **Patterns**: CQRS with MediatR, Repository pattern
- **Validation**: FluentValidation
- **Mapping**: AutoMapper
- **Logging**: Serilog
- **Testing**: xUnit, Moq, FluentAssertions
- **Background**: IHostedService

---

## Collaboration Rules

### 🤖 Claude 자동 처리 작업

Claude가 독립적으로 완료:
- CRUD 메서드, Repository 패턴
- DTO 생성, Configuration
- EF Core 마이그레이션
- Unity API 문서, Swagger 주석
- 단위 테스트 작성

### 👥 함께 협업하는 작업

설계/구현 전 논의:
- 데이터 설계 (엔티티 관계, 인덱스)
- 비즈니스 로직 (전투 공식, 보상 계산)
- API 설계 (요청/응답, 에러 핸들링)
- 아키텍처 설계 (계층 분리, 의존성)
- 성능 최적화 (쿼리, 캐싱, 동시성)

### 협업 프로세스

1. **설계**: Claude 초안 제시 → 사용자 피드백 → 최종 결정
2. **구현**: 핵심 로직은 TODO(human), 반복 코드는 자동 완성
3. **검토**: 구현 후 주요 변경사항 요약

**상세 규칙**: Serena 메모리 `week2-workflow-and-collaboration-rules` 참조

---

## Development Guidelines

### Adding New Features

1. Create domain entities in `IdleRPG.Domain`
2. Define interfaces and DTOs in `IdleRPG.Application`
3. Implement repositories in `IdleRPG.Infrastructure`
4. Create API controllers in `IdleRPG.API`
5. Use MediatR for CQRS
6. Apply FluentValidation
7. Map with AutoMapper
8. Write unit tests in `IdleRPG.Tests`

### Testing Guidelines

- Use **xUnit**, **Moq**, **FluentAssertions**
- Follow **AAA pattern** (Arrange-Act-Assert)
- Mock repository dependencies
- Test edge cases: null inputs, boundaries, errors
- Run before committing: `dotnet test`

### Authentication

- JWT Bearer tokens (Access: 15 min, Refresh: 7 days)
- BCrypt password hashing
- Game endpoints require `[Authorize]` attribute

### Game Balance Philosophy

- Server-side validation for critical operations
- Client is presentation only (UI, animations)
- All economy logic runs on server
- Server-authoritative design prevents cheating

---

## Unity Client Documentation

### Policy

**CRITICAL**: When adding/modifying APIs or DTOs, **ALWAYS** update Unity documentation.

### Standard Checklist

- [ ] Update `../IdleRPGClient/Docs/unity/API_SPEC_FOR_UNITY.md`
- [ ] Add/update DTO in `../IdleRPGClient/Docs/Unity-DTOs.cs`
- [ ] Update summary table in `../IdleRPGClient/Docs/unity/Unity-Quick-Reference.md`

**Detailed Guide**: See `docs/unity/UNITY_DOCUMENTATION_GUIDE.md`

---

## Reference Documentation

- **Project Roadmap**: `docs/learning/PROJECT_ROADMAP.md` - 8-week learning plan, current status
- **Environment Setup**: `docs/development/DEV_ENVIRONMENT_SETUP.md` - Docker, HTTPS, local dev
- **Deployment Guide**: `docs/jenkins/DEPLOYMENT_GUIDE.md` - CI/CD pipeline, migrations
- **Unity Documentation**: `docs/unity/UNITY_DOCUMENTATION_GUIDE.md` - API docs update rules
- **Complete PRD**: `docs/MUSHROOM_GAME_PRD.md` - 20-system specification

---

## AI 협업 규칙 (mcp__zen__* 도구 사용 시)

다른 AI(Gemini, GPT, Grok 등)와 협업할 때 **반드시** 다음 프로세스를 따릅니다:

### 📋 필수 단계

1. **의견 수집**
   - 다른 AI에게 명확한 질문 전달
   - 프로젝트 컨텍스트 제공 (파일, 현재 구조 등)

2. **비판적 분석**
   - AI의 답변을 프로젝트 컨텍스트로 검증
   - 우리 프로젝트에 적용 가능한지 평가
   - 기존 구현과 충돌하지 않는지 확인

3. **차이점 명시**
   - ✅ 동의하는 부분
   - 🔄 조정이 필요한 부분
   - ❌ 불일치/적용 불가능한 부분

4. **종합 및 옵션 제시**
   - 다른 AI 의견 + Claude 분석을 종합
   - **여러 실행 옵션을 제시** (사용자가 선택)
   - 각 옵션의 장단점 설명

5. **사용자 선택 대기**
   - 사용자가 옵션을 선택할 때까지 대기
   - 선택 후 구체적인 구현 진행

---

### ❌ 나쁜 예 (하지 말 것)

```
Claude: "Gemini에게 물어봤습니다!

Gemini: 'Flat 구조가 좋습니다. 이유는...'

답변을 받았습니다!"
```

**문제점**:
- Claude의 분석이 없음
- 그래서 뭘 하겠다는 건지 불명확
- 사용자가 판단할 정보 부족

---

### ✅ 좋은 예 (올바른 방식)

```
Claude: "Gemini와 의논한 결과를 정리했습니다.

📋 Gemini의 의견:
- Flat 구조 추천
- 이유: Unity JsonUtility 호환성, 간단함
- 단점: 복잡한 데이터는 표현 어려움

🤔 제 분석:
✅ 동의하는 부분:
- 우리 프로젝트는 이미 CharacterDTO가 Flat 구조
- 서버 v1.7도 Flat 구조 사용 중

🔄 추가 고려사항:
- 향후 복잡한 장비 시스템 확장성

❌ 불일치:
- (없음 - Gemini 의견이 프로젝트와 일치)

⚖️ 종합:
Gemini의 Flat 구조 제안이 타당하며,
현재 프로젝트 구조와도 일치합니다.

📌 선택지:

[Option A] Flat 구조 채택 (Gemini 제안 그대로)
- 장점: 간단함, 기존 코드와 일관성
- 단점: 복잡한 데이터 표현 제한
- 적용: EquipmentDTO를 Flat으로 작성

[Option B] Hybrid 구조 (Flat + 부분 중첩)
- 장점: 유연성, 확장성
- 단점: JsonUtility 제약으로 Newtonsoft.Json 필요
- 적용: 기본은 Flat, Stats만 중첩 객체

[Option C] 완전 중첩 구조 (Gemini 의견 거부)
- 장점: 가독성, 논리적 그룹화
- 단점: Unity JsonUtility 불가, 기존 코드 수정 필요
- 적용: 전체 DTO 구조 재설계

어떤 옵션을 선택하시겠어요?"
```

**사용자 선택 후**:
```
사용자: "Option A로 해줘"

Claude: "Option A (Flat 구조)로 진행하겠습니다.

[실제 구현 시작...]"
```

---

### 🎯 핵심 원칙

1. **다른 AI는 조언자, Claude는 분석가**
   - 다른 AI 답변을 맹목적으로 따르지 않음
   - 프로젝트 컨텍스트로 비판적 검증

2. **결정권은 사용자에게**
   - 여러 옵션 제시
   - 각 옵션의 장단점 명확히
   - 사용자 선택 후 구현

3. **투명성**
   - 동의/불일치 부분 명시
   - 판단 근거 설명
   - 숨김없이 공개