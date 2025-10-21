# Load Core Memory Files

현재 프로젝트의 핵심 메모리 파일들을 컨텍스트에 로드합니다.

**로드 대상**:
- `0-core/`: 프로젝트 설계 문서 (architecture, game-design, tech-stack)
- `1-current/`: 현재 진행 상황 (status, roadmap)

---

다음 파일들을 읽어주세요:

1. `.claude/memories/0-core/architecture.md`
2. `.claude/memories/0-core/game-design.md`
3. `.claude/memories/0-core/tech-stack.md`
4. `.claude/memories/1-current/status.md`
5. `.claude/memories/1-current/roadmap.md`

모든 파일을 읽은 후 다음 정보를 요약해주세요:
- **전체 진행률**: Phase 1-4 각각의 완료율
- **현재 작업**: 진행 중인 시스템 번호와 이름
- **다음 우선순위**: status.md의 Immediate 항목 (3개)
- **API 엔드포인트 수**: 총 개수
- **데이터베이스 테이블 수**: 총 개수

마지막으로 "✅ 메모리 로드 완료 (5개 파일, ~7,000 tokens)" 메시지를 출력하세요.
