# Update Current Memory (2-current/)

프로젝트의 현재 상태를 분석하여 `.claude/memories/2-current/` 폴더의 메모리를 업데이트합니다.

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
- `2-current/roadmap.md`의 현재 Phase
- 진행 중인 작업 (TODO 주석, 미완성 기능)

### 2. status.md 업데이트

`.claude/memories/2-current/status.md` 파일을 다음 형식으로 업데이트하세요:

```markdown
# 현재 구현 상태 (Week X Day Y 완료)

**업데이트**: YYYY-MM-DD

---

## API 엔드포인트 (N개)

### Authentication (N개)
- ✅ POST /api/auth/register - 회원가입
...

### [기능명] (N개)
- ✅ [Method] [URL] - [설명]
...

---

## Database Tables (N개)

### 카테고리
- ✅ **TableName** - 설명 (PK 타입, FK 관계)
...

---

## 주요 기능 상세

### 1. [기능명]
- [핵심 특징]
- [비즈니스 규칙]
...

---

## 다음 우선순위

### Immediate (이번 주)
1. [작업명]
   - [세부사항]
...

### Short-term (다음 주)
...

---

## 기술 부채

### Critical (즉시 해결)
- [ ] [항목]
...
```

### 3. roadmap.md 업데이트

`.claude/memories/2-current/roadmap.md` 파일을 다음 형식으로 업데이트하세요:

```markdown
# 프로젝트 로드맵

**프로젝트 기간**: 8주 (시작일 ~ 종료일)
**현재 진행**: Week X Day Y (YYYY-MM-DD)

---

## Phase 1: Foundation (Week 1-3)

### 시스템 목록
1. ✅ [완료 시스템] (Week N)
2. 📋 [진행 중 시스템] (진행 중)
...

**진행률**: **X/Y 완료 (ZZ%)**

---

## Phase 2-4: ...

[동일한 형식]

---

## 전체 진행률

**완료된 시스템**: X개 / 21개 (XX%)
**진행 중 시스템**: N개 (시스템명)

**Phase 별**:
- Phase 1 (MVP): X/Y = ZZ% [✅ 또는 📋]
...

---

## 다음 우선순위

[status.md와 동일]

---

## 주요 마일스톤

### ✅ Week N 완료 (YYYY-MM-DD)
- [주요 성과]
...

### 📋 Week N 목표
- [목표 항목]
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

- 기존 status.md: `.claude/memories/2-current/status.md`
- 기존 roadmap.md: `.claude/memories/2-current/roadmap.md`
- Controllers: `IdleRPG.API/Controllers/`
- DbContext: `IdleRPG.Infrastructure/Data/GameDBContext.cs`
- Git log: `git log --oneline -10`
