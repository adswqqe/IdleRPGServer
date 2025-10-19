# Custom Agents Guide

이 프로젝트에서 사용 가능한 custom agent들입니다. 각 agent는 특정 작업을 자동화하고 CLAUDE.md 정책을 강제합니다.

## 🆕 새로 추가된 Agents (4개)

### 1. **clean-architecture-scaffolder** 🔵
**목적**: 새 게임 시스템을 Clean Architecture 전체 레이어에 자동 생성

**사용 시점**:
- "Pet 시스템 추가해줘"
- "Guild 기능 구현해줘"
- "장비 강화 시스템 만들어줘"

**생성되는 파일**:
```
Domain/Entities/{System}.cs
Application/
  ├── Interfaces/I{System}Repository.cs
  ├── Interfaces/I{System}Service.cs
  ├── DTOs/{System}/{System}Dto.cs
  ├── Services/{System}Service.cs
  └── Validators/Create{System}DtoValidator.cs
Infrastructure/
  ├── Repositories/{System}Repository.cs
  └── Configurations/{System}Configuration.cs
API/Controllers/{System}sController.cs
Tests/Services/{System}ServiceTests.cs
../IdleRPGClient/Docs/unity/{system}/
  ├── API_SPEC.md
  └── DTOs.cs
```

**특징**:
- ✅ 반복 코드 자동 생성 (CRUD, Repository, DTOs)
- 👥 비즈니스 로직에 `TODO(human)` 마커 추가
- ✅ Unity 문서 자동 생성
- ✅ 의존성 등록 가이드

**예시**:
```
사용자: "Pet 시스템 추가해줘"

Claude: [clean-architecture-scaffolder 실행]
✅ Pet 시스템 생성 완료
📦 생성된 파일: 15개
👥 TODO(human) 항목: 3개
  1. Pet.cs - Pet 스탯 정의
  2. PetService.CreatePetAsync - 펫 생성 로직
  3. PetConfiguration - 인덱스 최적화
```

---

### 2. **migration-safety-checker** 🔴
**목적**: Production 배포 전 EF Core Migration 안전성 검증

**사용 시점**:
- Migration 생성 후: "AddPetSystem migration 확인해줘"
- 배포 전: "이 마이그레이션 프로덕션에 올려도 돼?"
- 테이블 수정 후: "Character 테이블 수정했는데 안전한지 체크해줘"

**검증 항목**:
```
🔴 CRITICAL (Block):
  ❌ 데이터 삭제 (DROP TABLE, DROP COLUMN)
  ❌ NOT NULL 제약 추가 (기존 NULL 데이터 확인)
  ❌ 컬럼 크기 축소 (데이터 잘림 위험)
  ❌ Foreign Key Cascade 누락

🟡 HIGH (Warn):
  ⚠️ Foreign Key 인덱스 누락 (성능 저하)
  ⚠️ 대용량 테이블 변경 (Table Lock)
  ⚠️ Enum 변경 시 데이터 마이그레이션 누락

🟢 LOW (Info):
  ✅ Nullable 컬럼 추가
  ✅ 새 테이블 생성
  ✅ 인덱스 추가
```

**CRITICAL 정책 강제**:
```
⚠️ NEVER run 'dotnet ef database update' on release branches!

✅ Correct Workflow:
1. dotnet ef migrations add AddPetSystem
2. Commit and push to GitHub
3. Jenkins automatically applies migration.sql to RDS
```

**출력 예시**:
```
# Migration Safety Report: AddPetSystem

## 📊 Summary
- Risk Level: 🟡 HIGH
- Deployment: ⚠️ RISKY (review required)

## ⚠️ Risk Analysis

### 🟡 High Risks
1. Missing Index on Pets.CharacterId
   - Impact: 200ms+ query time on 10,000+ rows
   - Fix: Add index IX_Pets_CharacterId

2. Large Table Alteration (Characters)
   - Impact: Table lock (~10-30 seconds)
   - Recommendation: Deploy at 2-4 AM KST

## ✅ Deployment Checklist
- [ ] Add missing index IX_Pets_CharacterId
- [ ] Schedule deployment during maintenance window
- [ ] Export Pets table backup
- [ ] Jenkins pipeline ready
```

---

### 3. **unity-doc-enforcer** 🟡
**목적**: API 변경 시 Unity 문서 동기화 강제

**사용 시점**:
- Controller 생성/수정 후: "PetsController 만들었어"
- DTO 변경 후: "CharacterDto에 새 필드 추가했어"
- 커밋 전: "이거 커밋해도 돼?" (자동 체크)

**CRITICAL 정책 강제**:
```
CRITICAL: When adding/modifying APIs or DTOs,
ALWAYS update Unity documentation.
```

**검증 항목**:
```
✅ 폴더 구조 확인: ../IdleRPGClient/Docs/unity/{feature}/
✅ API_SPEC.md 완전성:
   - 모든 엔드포인트 문서화
   - Request/Response 예제
   - Unity C# 코드 예제
✅ DTOs.cs 동기화:
   - 서버 DTO 속성과 일치
   - JsonProperty 어트리뷰트
   - Guid→string, DateTime→string 변환
✅ README.md 업데이트:
   - 구현 상태 테이블
   - 버전 번호 증가
   - 버전 히스토리 추가
```

**출력 예시**:
```
# Unity Documentation Enforcement Report

## 📊 Change Detection
- Modified: PetsController.cs
  - New endpoints: PUT /pets/{id}, DELETE /pets/{id}
- Modified: PetDto.cs
  - New properties: Level, Experience

## 🔍 Documentation Status
- API_SPEC.md: ⚠️ Outdated (missing 2 endpoints)
- DTOs.cs: ⚠️ Outdated (missing 2 properties)
- README.md: ⚠️ Needs version bump (v1.7 → v1.8)

## 🛠️ Required Actions
1. Add PUT /pets/{id} to API_SPEC.md
2. Add DELETE /pets/{id} to API_SPEC.md
3. Add Level, Experience to PetDto in DTOs.cs
4. Update README.md to v1.8

[자동 생성된 코드 제공...]
```

---

### 4. **multi-ai-collaborator** 🟣
**목적**: 다른 AI(Gemini, GPT, Grok)와 협업 시 비판적 사고 강제

**사용 시점**:
- "Gemini한테 Pet 시스템 설계 물어봐"
- "GPT한테 DTO 구조 어떻게 하면 좋을지 물어봐"
- "전투 공식에 대해 Gemini랑 GPT 둘 다 의견 들어봐"

**핵심 프로세스** (CLAUDE.md 필수 단계):
```
1. 🧠 독립 분석 (다른 AI 상담 전)
   → 나는 뭐라고 답할까?
   → 어떤 옵션들이 있을까?
   → 어떤 부분이 불확실할까?

2. 💬 타 AI 의견 수집
   → Gemini/GPT/Grok에게 질문
   → 프로젝트 컨텍스트 제공

3. 🔍 비판적 비교
   → ✅ 동의하는 부분
   → 🔄 조정이 필요한 부분
   → ❌ 불일치/적용 불가능한 부분

4. 📌 옵션 제시 (A/B/C + 장단점)
   → 사용자 선택 대기

5. ✅ 구현 (사용자 선택 후)
```

**특징**:
- ✅ Anchoring Bias 방지 (독립 분석 먼저)
- ✅ 프로젝트 컨텍스트 기반 검증
- ✅ 여러 관점 종합 (Gemini + GPT + Claude)
- 👥 최종 결정권은 사용자

**예시**:
```
사용자: "Gemini한테 Pet DTO 구조 물어봐"

Claude:
[Phase 0] 🧠 내 독립 분석
  → 나는 Flat 구조 추천 (기존 일관성)
  → 불확실: 확장성 vs 간결성 trade-off

[Phase 1] 💬 Gemini 상담
  → Gemini: "Flat 구조 추천, JsonUtility 호환성"

[Phase 2] 🔍 비판적 분석
  ✅ 동의: Flat 구조의 간결성
  🔄 조정: 우리는 Newtonsoft.Json 사용 (JsonUtility 제약 없음)
  ❌ 불일치: Gemini는 확장성 간과함

[Phase 3] 📌 옵션 제시
  [A] Flat 구조 - 기존 일관성 (Gemini 추천)
  [B] Nested 구조 - 확장성 우선 (⭐ 내 추천)
  [C] Hybrid 구조 - 절충안

  의사결정 매트릭스:
  | 기준 | Flat | Nested | Hybrid |
  |------|------|--------|--------|
  | 일관성 | ⭐⭐⭐⭐⭐ | ⭐ | ⭐⭐⭐ |
  | 확장성 | ⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ |
  | 가독성 | ⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ |

[Phase 4] ⏳ 사용자 선택 대기
  "어떤 옵션을 선택하시겠어요?"

[Phase 5] ✅ 구현
  사용자: "B로 해줘"
  Claude: "Nested 구조로 진행합니다!"
```

**CRITICAL 규칙** (CLAUDE.md):
```
❌ 나쁜 예:
Claude: "Gemini에게 물어봤습니다!
        Gemini: 'Flat 구조가 좋습니다.'
        답변 받았습니다!"

✅ 좋은 예:
Claude: "Gemini와 의논한 결과를 정리했습니다.

📋 Gemini 의견: Flat 구조 추천
🤔 내 분석:
  ✅ 동의: [항목]
  🔄 조정: [항목]
  ❌ 불일치: [항목]

⚖️ 종합:
  [Option A] Flat
  [Option B] Nested (⭐ 내 추천)
  [Option C] Hybrid

어떤 옵션을 선택하시겠어요?"
```

**절약 효과**:
- 수동 협업: 30분 (AI 답변 맹목 수용 → 나중에 문제 발견)
- Agent 협업: 15분 + 높은 품질 (비판적 검증 → 문제 사전 방지)

---

## 📚 기존 Agents

### task-orchestrator
복잡한 다단계 작업 조율 및 병렬 실행

### task-executor
개별 작업 실행 (orchestrator가 배포)

### task-checker
Review 상태 작업 검증

---

## 🎯 사용 예시 시나리오

### 시나리오 1: 새 시스템 추가
```
사용자: "Pet 시스템 추가해줘"

Claude:
1. [clean-architecture-scaffolder 실행]
   → 15개 파일 생성 (Entity, Repository, Service, Controller, Tests, Unity docs)

2. [unity-doc-enforcer 자동 실행]
   → Unity 문서 완전성 확인 ✅

3. [사용자에게 TODO(human) 항목 안내]
   → "Pet.cs에서 스탯 정의해주세요"
```

### 시나리오 2: Migration 생성 후
```
사용자: "dotnet ef migrations add AddPetSystem 실행했어"

Claude:
1. [migration-safety-checker 실행]
   → SQL 분석, 위험도 평가

2. [리포트 출력]
   🟡 HIGH RISK: 인덱스 누락
   ⚠️ 해결 방법 제시

3. [배포 체크리스트 제공]
   ✅ Jenkins 준비 완료
   ❌ 절대 'dotnet ef database update' 실행 금지
```

### 시나리오 3: API 수정 후 커밋 시도
```
사용자: "git commit -m 'Add Pet system'"

Claude:
1. [git diff로 Controller 변경 감지]

2. [unity-doc-enforcer 자동 실행]
   ⚠️ Unity 문서 누락 감지

3. [자동 생성 or 경고]
   "Unity 문서 업데이트 필요합니다!"
   [누락된 문서 자동 생성...]

4. [커밋 허용]
   ✅ Unity 문서 동기화 완료
```

---

## 🔧 Agent 호출 방법

### 명시적 호출
```
Claude에게 직접 요청:
"clean-architecture-scaffolder로 Pet 시스템 만들어줘"
"migration-safety-checker로 마이그레이션 확인해줘"
"unity-doc-enforcer로 Unity 문서 체크해줘"
```

### 자동 호출
```
Claude가 컨텍스트 기반 자동 선택:
"Pet 시스템 추가해줘" → scaffolder 자동 선택
"마이그레이션 확인해줘" → safety-checker 자동 선택
"PetsController 만들었어" → doc-enforcer 자동 실행
```

---

## 📖 Agent 추가 방법

새로운 agent를 추가하려면:

1. `.claude/agents/{agent-name}.md` 파일 생성
2. YAML front matter 작성:
   ```yaml
   ---
   name: agent-name
   description: When to use this agent...
   model: sonnet | opus | haiku
   color: blue | red | yellow | green
   ---
   ```
3. Markdown 본문에 상세 프롬프트 작성
4. Claude가 자동으로 인식하여 Task tool에서 사용 가능

---

## 🎓 Agent 설계 원칙

1. **Single Responsibility**: 각 agent는 명확한 단일 목적
2. **Policy Enforcement**: CLAUDE.md 규칙을 코드로 강제
3. **Defensive**: 실수를 사전에 방지
4. **Actionable**: 문제 발견 시 명확한 해결 방법 제공
5. **Automated**: 반복 작업은 최대한 자동화

---

## 📞 문제 해결

**Agent가 실행되지 않을 때**:
1. `.claude/agents/` 폴더에 파일이 있는지 확인
2. YAML front matter 형식이 올바른지 확인
3. Claude를 재시작

**Agent가 예상과 다르게 동작할 때**:
1. Description에 사용 시점이 명확한지 확인
2. Agent의 프롬프트를 읽고 개선점 제안
3. Model을 변경 (복잡한 작업은 opus, 간단한 작업은 sonnet)

---

**프로젝트**: 버섯키우기 완전판 - Idle MMORPG
**아키텍처**: Clean Architecture (ASP.NET Core 8.0)
**문서**: CLAUDE.md 참조
