# End Daily Session

하루 작업을 마무리하고 세션 내용을 Core/Current 메모리에 반영합니다.

---

## 작업 순서

### 1. 세션 파일 확인

오늘 날짜의 세션 파일을 읽어오세요:
- `.claude/memories/2-session/daily-{오늘날짜}.md`

### 2. 세션 내용 분석

다음 섹션들을 분석하세요:

**완료된 목표**:
- "🎯 오늘의 목표"에서 체크된 항목 (✅)

**변경사항**:
- "⚙️ 진행 중" 섹션의 API, DB, DTO 변경 내역
- "📊 세션 통계" 숫자 확인

**의사결정**:
- "🤝 의사결정 & 협업" 섹션의 결정 사항
- TODO(human) 대기 중 항목

**다음 우선순위**:
- "⏭️ 다음 세션 우선순위" 항목들

### 3. Current 메모리 업데이트

#### 3-1. status.md 업데이트

`.claude/memories/1-current/status.md` 파일을 다음 내용으로 업데이트하세요:

**API 엔드포인트**:
- Controllers 폴더 스캔 + 세션 통계 반영

**Database Tables**:
- GameDBContext.cs 스캔 + 세션 통계 반영

**완료된 기능**:
- 세션에서 완료한 시스템/기능 추가

**다음 우선순위**:
- 세션의 "⏭️ 다음 세션 우선순위" 반영

#### 3-2. roadmap.md 업데이트

`.claude/memories/1-current/roadmap.md` 파일을 다음 내용으로 업데이트하세요:

**현재 진행**:
- Week X Day Y → Week X Day Y+1 (또는 다음 주)

**Phase 진행률**:
- 완료된 시스템 수 재계산

**주요 마일스톤**:
- 오늘 완료한 시스템이 있으면 "✅ Week X Day Y" 추가

### 4. Core 메모리 업데이트 여부 판단

다음 경우에만 0-core/ 업데이트:

**architecture.md**:
- 새로운 계층/패턴 도입 시
- DI 구조 변경 시

**game-design.md**:
- 게임 메커니즘 변경 시 (확률, 경제 밸런스)
- 새로운 시스템 추가 시

**tech-stack.md**:
- 새로운 NuGet 패키지 추가 시
- 인프라 변경 시

### 5. 세션 파일 아카이브

세션 파일을 아카이브로 이동:
- `2-session/daily-{날짜}.md` → `9-archive/checkpoints/week{X}-day{Y}.md`

### 6. 세션 종료 요약 출력

다음 형식으로 요약을 제공하세요:

```markdown
## 📝 세션 종료: {날짜}

### ✅ 완료한 작업
- {완료한 시스템/기능1}
- {완료한 시스템/기능2}

### 📊 코드베이스 변경
- **API 엔드포인트**: 24개 → 27개 (+3)
- **Database Tables**: 9개 → 10개 (+1)
- **Git Commits**: 3개

### 🤝 의사결정 기록
- {결정 사항1}
- {결정 사항2}

### 📋 TODO(human) 대기
- {대기 중인 협업 항목}

### 📈 진행률 업데이트
- **Phase 1**: 85% → 100% (완료!)
- **전체**: 28% → 33% (+5%)

### ⏭️ 다음 세션 우선순위
1. {다음 작업1}
2. {다음 작업2}
3. {다음 작업3}

---

💡 **Insight**: {오늘 세션의 핵심 인사이트}

🎉 **마일스톤**: {달성한 주요 성과}
```

---

## 중요 원칙

1. **정확성**: 세션 파일 내용을 정확히 반영
2. **자동화**: 코드베이스 스캔으로 최신 상태 확인
3. **간결성**: Current 메모리는 현재 상태만, 과거 이력은 Archive로
4. **일관성**: 기존 메모리 형식 유지

---

## 참고 파일

- 세션 파일: `.claude/memories/2-session/daily-{날짜}.md`
- status.md: `.claude/memories/1-current/status.md`
- roadmap.md: `.claude/memories/1-current/roadmap.md`
- Controllers: `IdleRPG.API/Controllers/`
- DbContext: `IdleRPG.Infrastructure/Data/GameDBContext.cs`
