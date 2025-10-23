# Session Memory (2-session/)

일일 작업 세션을 추적하는 메모리입니다.

---

## 구조

```
2-session/
├── daily-2025-10-21.md   # 오늘의 작업 로그
├── daily-2025-10-22.md   # 다음 날 작업 로그
└── README.md             # 이 파일
```

---

## 사용 방법

### 1. 세션 시작
```bash
/startSession
```
- `daily-{오늘날짜}.md` 파일 생성
- 오늘의 목표 3개 설정 (1-current/status.md 기반)

### 2. 작업 진행 중
세션 파일이 자동으로 업데이트됩니다:
- ✅ API/DTO 추가 → "⚙️ 진행 중" 섹션 업데이트
- ✅ 의사결정 → "🤝 의사결정 & 협업" 섹션 추가
- ✅ TODO(human) → "협업 대기" 섹션 추가

### 3. 세션 종료
```bash
/endSession
```
- 세션 내용 분석
- `1-current/status.md`, `roadmap.md` 업데이트
- 세션 파일을 `9-archive/checkpoints/`로 이동

---

## 세션 파일 구조

각 `daily-YYYY-MM-DD.md` 파일은 다음 섹션을 포함합니다:

1. **🎯 오늘의 목표**: 체크리스트 (3개)
2. **⚙️ 진행 중**: 현재 작업 상태, 변경사항
3. **🤝 의사결정 & 협업**: 결정 사항, TODO(human)
4. **🐛 발견된 이슈**: Critical/Normal 이슈 목록
5. **💡 메모 & 인사이트**: 학습 내용, 개선 아이디어
6. **📊 세션 통계**: API/DB/Commit 수 변화
7. **⏭️ 다음 세션 우선순위**: 내일 할 작업

---

## 자동 업데이트 트리거

세션 파일은 다음 이벤트 발생 시 자동 업데이트됩니다:

- ✅ Controller 파일 생성/수정
- ✅ Entity/DTO 파일 생성
- ✅ Migration 추가
- ✅ TODO(human) 주석 작성
- ✅ Git commit

---

## 보관 정책

- **활성 세션**: `2-session/daily-{오늘날짜}.md`
- **종료된 세션**: `/endSession` 실행 시 `9-archive/checkpoints/week{X}-day{Y}.md`로 이동
- **보관 기간**: 영구 (프로젝트 히스토리로 활용)

---

## 예시 파일

`.claude/templates/session-template.md` 참조
