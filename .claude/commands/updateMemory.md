# Update Current Memory (1-current/)

프로젝트의 현재 상태를 분석하여 `.claude/memories/1-current/` 폴더의 메모리를 업데이트합니다.

## 작업 순서

### 1. 코드베이스 분석

다음 항목들을 조사하세요:

**API 엔드포인트**:
- `IdleRPG.API/Controllers/` 폴더의 모든 Controller 파일
- 각 Controller의 HttpGet, HttpPost, HttpPut, HttpDelete 메서드
- 엔드포인트 URL, 설명

**Database Tables**:
- `IdleRPG.Infrastructure/Data/GameDBContext.cs`의 DbSet 목록
- 각 테이블의 목적과 관계

**완료된 기능**:
- 최근 구현된 시스템 (Git log 참조)
- 주요 기능 상세 (엔티티, 비즈니스 로직)

**다음 우선순위**:
- `1-current/roadmap.md`의 현재 Phase
- 진행 중인 작업 (TODO 주석, 미완성 기능)

### 2. status.md 업데이트

`.claude/memories/1-current/status.md` 파일을 다음 형식으로 업데이트하세요:

```markdown
# 현재 구현 상태

**업데이트**: YYYY-MM-DD

---

## API 엔드포인트 (N개)

### Authentication (N개)
- POST /api/auth/register
- POST /api/auth/login
...

### [기능명] (N개)
- [Method] [URL]
...

---

## Database Tables (N개)

- **Players**: 플레이어 계정 (GUID PK)
- **Characters**: 게임 캐릭터 (GUID PK, FK → Players)
...

---

## Unity 문서화 체크리스트 ⚠️ CRITICAL

**언제**: API/DTO 추가/수정 시 반드시 실행

1. **폴더**: `../IdleRPGClient/Docs/unity/{feature}/`
2. **API_SPEC.md**: Request/Response 예제 + Unity C# 코드
3. **DTOs.cs**: `[Serializable]`, `[JsonProperty]` (Newtonsoft.Json)
4. **README.md**: 구현 상태 테이블 업데이트

---

## 완료된 기능 (Week X Day Y)

1. ✅ **인증 시스템**: JWT (Access 15분, Refresh 7일), BCrypt
2. ✅ **캐릭터 성장**: 레벨업, 경험치 공식
...

---

## 다음 우선순위

### Immediate (이번 주)
1. [작업명]: [세부사항]
2. [작업명]: [세부사항]
...

### Short-term (다음 주)
...

### Mid-term (Week X-Y)
...
```

### 3. roadmap.md 업데이트

`.claude/memories/1-current/roadmap.md` 파일을 다음 형식으로 업데이트하세요:

```markdown
# 프로젝트 로드맵

> **시스템 상세 명세**: `0-core/game-design.md` 참조

**프로젝트 기간**: 8주 (2025-10-14 ~ 2025-12-06)
**현재 진행**: Week X Day Y (YYYY-MM-DD)

---

## Phase 1: Foundation (Week 1-3) - MVP 시스템

**시스템**: 1-7 (game-design.md 참조)
**진행률**: **X/7 완료 (XX%)**

**완료**: ✅ 1, 2, 3, ...
**진행 중**: 📋 N

---

## Phase 2: Expansion (Week 4-5) - 핵심 게임플레이

**시스템**: 8-11
**진행률**: **X/4 (XX%)**

**다음 작업**: N번 (시스템명 - 현재 상태)

---

## Phase 3: Advanced (Week 6-7) - 소셜 & 수익화

**시스템**: 12-16
**진행률**: **X/5 (XX%)**

---

## Phase 4: Polish (Week 8) - 라이브 운영

**시스템**: 17-21
**진행률**: **X/5 (XX%)**

---

## 전체 진행률

**완료된 시스템**: X개 / 21개 (XX%)
**진행 중 시스템**: N개 (시스템 N)

**Phase 별**:
- Phase 1 (MVP): X/7 = XX% [✅ 또는 📋]
- Phase 2 (핵심): X/4 = XX%
- Phase 3 (소셜): X/5 = XX%
- Phase 4 (운영): X/5 = XX%

---

## 주요 마일스톤

### ✅ Week N (YYYY-MM-DD)
- 시스템 N, M 완료
- [주요 성과]

### 📋 Week N-M 목표
- 시스템 N 완성: [세부사항]
- 시스템 M: [세부사항]
...
```

### 4. 변경사항 요약

업데이트 완료 후, 다음 형식으로 요약을 제공하세요:

```
## 📝 메모리 업데이트 완료

### status.md 변경사항
- API 엔드포인트: N개 → M개 (+X개)
- Database Tables: N개 → M개 (+X개)
- 새로 추가된 기능: [기능명]
- 업데이트된 우선순위: [내용]

### roadmap.md 변경사항
- Phase N 진행률: XX% → YY% (+ZZ%)
- 완료된 마일스톤: [마일스톤명]
- 다음 주 목표: [내용]

### 권장 다음 단계
- [작업 제안]
```

---

## 중요 원칙

1. **정확성**: 실제 코드베이스 상태를 정확히 반영
2. **최신성**: 오늘 날짜로 업데이트
3. **간결성**: 핵심 정보만 포함, 과거 이력은 9-archive/로
4. **일관성**: 기존 형식 유지

---

## 참고 파일

- 기존 status.md: `.claude/memories/1-current/status.md`
- 기존 roadmap.md: `.claude/memories/1-current/roadmap.md`
- Controllers: `IdleRPG.API/Controllers/`
- DbContext: `IdleRPG.Infrastructure/Data/GameDBContext.cs`
- Git log: `git log --oneline -10`
