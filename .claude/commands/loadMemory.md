# Load Memory Files

현재 프로젝트의 메모리 파일들을 컨텍스트에 로드합니다.

**로드 대상**:
- `0-core/`: 프로젝트 설계 문서 (architecture, game-design, tech-stack)
- `1-current/`: 현재 진행 상황 (status, roadmap)
- `2-session/`: 오늘의 작업 세션 (있는 경우)

---

## 작업 순서

### 1. Core & Current 메모리 로드

다음 파일들을 읽어주세요:

1. `.claude/memories/0-core/architecture.md`
2. `.claude/memories/0-core/game-design.md`
3. `.claude/memories/0-core/tech-stack.md`
4. `.claude/memories/1-current/status.md`
5. `.claude/memories/1-current/roadmap.md`

### 2. 세션 메모리 확인 (조건부)

오늘 날짜의 세션 파일이 있는지 확인:
- `.claude/memories/2-session/daily-{오늘날짜}.md`

**있으면**: 읽어서 진행 중인 작업 컨텍스트 로드
**없으면**: 건너뛰기

### 3. 요약 정보 출력

모든 파일을 읽은 후 다음 정보를 요약해주세요:

**프로젝트 현황**:
- **전체 진행률**: Phase 1-4 각각의 완료율
- **현재 작업**: 진행 중인 시스템 번호와 이름
- **다음 우선순위**: status.md의 Immediate 항목 (3개)
- **API 엔드포인트 수**: 총 개수
- **데이터베이스 테이블 수**: 총 개수

**세션 정보** (세션 파일이 있는 경우):
- **오늘의 목표**: 체크리스트 진행 상황
- **진행 중인 작업**: 현재 시스템/기능
- **대기 중인 TODO(human)**: 협업 필요 항목

### 4. 완료 메시지

```
✅ 메모리 로드 완료
- Core: 3개 파일 (~3,500 tokens)
- Current: 2개 파일 (~3,500 tokens)
- Session: {있음/없음} ({있으면 토큰 수})
```

---

## 사용 팁

- **세션 시작 전**: `/loadMemory` → 프로젝트 전체 맥락 파악
- **세션 시작 후**: `/startSession` → 오늘 작업 목표 설정
- **작업 재개 시**: `/loadMemory` → 오늘 세션 진행 상황 확인
