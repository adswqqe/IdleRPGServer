# Claude Code Hooks

**100% 완전 자동화 + 전체 대화 추적 시스템** - 제로 수동 명령어!

---

## 설정된 Hook (3개)

### 1. SessionStart Hook (자동 세션 시작)

**트리거**: Claude Code 시작 또는 세션 재개 시

**스크립트**: `auto-start-session.sh`

**동작**:
1. 오늘 날짜 확인 (`YYYY-MM-DD`)
2. 이미 오늘 세션 파일 존재? → 건너뛰기 (재개)
3. 새 세션이면:
   - `roadmap.md`에서 Week/Day 추출
   - `status.md`에서 "다음 우선순위" 3개 추출
   - 템플릿 복사 및 변수 치환
   - `2-session/daily-{날짜}.md` 생성
4. Claude에게 시작 메시지 표시

**결과**: **/startSession 명령어 완전 자동화!**

---

### 2. Stop Hook V2 (전체 대화 저장) 🆕

**트리거**: Claude가 응답 완료 시마다 자동 실행

**스크립트**: `auto-update-session-full.sh`

**동작**:
1. Transcript에서 마지막 응답 추출 (JSONL 파싱)
2. 서브에이전트 응답인지 확인 (Task tool 사용 여부)
   - 서브에이전트면 건너뛰기 (메인 대화만 저장)
3. 중요 키워드 감지 (7개 카테고리):
   - 🔧 **API 변경**: Controller, endpoint, HttpPost...
   - 💾 **DB/DTO**: Entity, Migration, DbSet...
   - 🤝 **의사결정**: 결정, 확률, 설계, choice...
   - 👤 **TODO(human)**: 협업 대기 항목
   - ✅ **작업 완료**: 완료, implemented, 추가...
   - 🐛 **이슈**: error, bug, 문제...
   - ⚙️ **시스템**: Hook, 자동화, automation...
4. 중요 대화면 **전체 응답 내용** 저장 (키워드만 아님!)
5. 파일 크기 10MB 초과 시 경고

**개선점**:
- Before: 타임스탬프 + 이벤트 타입만 기록 ❌
- After: **전체 대화 내용** 저장 ✅
- 서브에이전트 제외로 노이즈 제거 ✅

**결과**: **완전한 대화 컨텍스트 보존!**

---

### 3. SessionEnd Hook V2 (LLM 요약 준비) 🆕

**트리거**: Claude Code 종료 시

**스크립트**: `auto-end-session-llm.sh`

**동작**:
1. 오늘 세션 파일 확인
2. 코드베이스 최종 스캔:
   - API 엔드포인트 수 카운트
   - DB 테이블 수 카운트
   - 오늘 Git Commits 카운트
3. 세션 파일 끝에 "🏁 세션 종료" 섹션 추가:
   - 종료 시간, 파일 크기, 최종 통계
4. **파일 크기 1MB 이상이면**:
   - `.needs-summary` 플래그 생성
   - 사용자에게 LLM 요약 권장 메시지
5. 세션 파일을 `9-archive/checkpoints/` 이동
6. `roadmap.md`, `status.md` 업데이트

**개선점**:
- Before: 무조건 아카이브만 ❌
- After: 큰 파일은 LLM 요약 권장 ✅
- 자동 요약 안 함 (비용 절약) ✅

**결과**: **비용 효율적인 요약 시스템!**

---

## 설정 파일

`.claude/settings.local.json`:
```json
{
  "hooks": {
    "SessionStart": [
      {
        "hooks": [
          {
            "type": "command",
            "command": "bash .claude/hooks/auto-start-session.sh",
            "timeout": 10
          }
        ]
      }
    ],
    "Stop": [
      {
        "hooks": [
          {
            "type": "command",
            "command": "bash .claude/hooks/auto-update-session-full.sh",
            "timeout": 10
          }
        ]
      }
    ],
    "SessionEnd": [
      {
        "hooks": [
          {
            "type": "command",
            "command": "bash .claude/hooks/auto-end-session-llm.sh",
            "timeout": 10
          }
        ]
      }
    ]
  }
}
```

---

## 🎉 완전 자동화 + 전체 대화 추적

### 작동 방식

```
09:00 - Claude Code 실행
      → SessionStart Hook ✅

10:30 - SkillController.cs 작성 완료
      → Stop Hook V2: 전체 응답 저장 (카테고리: 🔧 API 변경) ✅
      → 세션 파일에 응답 전문 append

12:00 - "SSR 확률 1%로 결정한 이유는..."
      → Stop Hook V2: 전체 설명 저장 (카테고리: 🤝 의사결정) ✅

15:00 - TODO(human) 추가 + 컨텍스트 설명
      → Stop Hook V2: 전체 설명 저장 (카테고리: 👤 TODO(human)) ✅

15:30 - "안녕하세요!" (일반 대화)
      → Stop Hook V2: 키워드 없음 → 저장 안 함 (노이즈 제거) ✅

18:00 - Claude Code 종료
      → SessionEnd Hook V2 ✅
      → 파일 크기 8MB → LLM 요약 권장 알림
      → 아카이브: week3-day3.md
```

---

## 📊 파일 크기 관리

### 자동 필터링
```
✅ 중요 대화만 저장 (7개 카테고리)
✅ 일반 대화 제외 (노이즈)
✅ 서브에이전트 제외 (Task tool)
```

### 예상 파일 크기
```
가벼운 날 (3-4시간):  1-2MB
보통 날 (6-8시간):    5-8MB
집중 날 (10시간+):    10-15MB
```

### LLM 요약 (선택적)
```
1MB 이상 → 요약 권장 알림
수동 실행: /summarizeSession 2025-10-21
→ 원본: week3-day3-full.md (8MB)
→ 요약: week3-day3.md (500KB)
→ 압축률: 94%
```

---

## 💡 핵심 개선

### V1 (키워드만 저장)
```
❌ 타임스탬프 + "API 변경" 문자열만
❌ 왜 그렇게 했는지 컨텍스트 손실
❌ 나중에 복원 불가능
```

### V2 (전체 대화 저장)
```
✅ 응답 전체 내용 저장
✅ 완전한 컨텍스트 보존
✅ 카테고리별 자동 분류
✅ 서브에이전트 제외 (노이즈 제거)
✅ LLM 요약으로 나중에 압축 가능
```

---

## 🔋 장점

### 1. 완전한 기록
```
Before: "API 변경 (14:30)" ❌
After:  전체 대화 + 코드 예제 + 이유 ✅
```

### 2. 컨텍스트 보존
```
Before: 1주 후 "왜 1%로 했더라?" ❌
After:  "SSR 1% 결정 이유: 밸런스..." ✅
```

### 3. 노이즈 제거
```
서브에이전트 작업 제외 ✅
일반 대화 제외 ✅
중요 대화만 7개 카테고리로 분류 ✅
```

### 4. 비용 효율
```
자동 요약 안 함 (비용 절약) ✅
필요시 수동 실행 (사용자 컨트롤) ✅
```

---

## 로그 확인

### Stop Hook 로그
```bash
cat .claude/hooks/session-update.log
```

출력 예시:
```
[14:30:25] 🔧 API 변경 (1523 chars)
[15:12:43] 🤝 의사결정 (892 chars)
[16:45:10] 👤 TODO(human) (456 chars)
```

### SessionEnd 로그
```bash
cat .claude/hooks/session-end.log
```

출력 예시:
```
[2025-10-21 18:00:35] 세션 종료: Week 3 Day 3 | Size: 8MB | API: 27 | DB: 10 | Commits: 3
```

---

## 수동 명령어 (선택적)

### LLM 요약 생성
```bash
/summarizeSession 2025-10-21
```

**사용 시점**:
- SessionEnd에서 "LLM 요약 권장" 알림 받았을 때
- 파일 크기가 1MB 이상일 때

**결과**:
- 원본 백업: `week3-day3-full.md` (8MB)
- 요약본: `week3-day3.md` (500KB)
- 압축률: 94%

---

## 문제 해결

### Stop Hook이 너무 많이 저장함
키워드 필터링 강화 (`auto-update-session-full.sh` 수정):
```bash
# 더 엄격한 패턴
if echo "$MESSAGE_TEXT" | grep -qiE "POST.*Controller.*implemented"; then
```

### 세션 파일이 너무 큼
정상입니다. LLM 요약으로 압축하세요:
```bash
/summarizeSession
```

### 서브에이전트 응답이 저장됨
`auto-update-session-full.sh`의 서브에이전트 감지 로직 확인:
```bash
IS_AGENT=$(echo "$LAST_ENTRY" | jq -r '.content[] | select(.type == "tool_use") | select(.name == "Task")')
```

---

## 보안 주의사항

- ✅ Transcript 읽기만 (JSONL 파싱)
- ✅ 세션 파일에만 append
- ❌ 외부 명령어 실행 금지
- ❌ 네트워크 접근 금지

---

## 향후 개선

1. **자동 LLM 요약**: 10MB 초과 시 자동 실행 (옵션)
2. **카테고리 커스터마이징**: 프로젝트별 키워드 설정
3. **로컬 LLM**: Ollama 연동으로 무료 요약
4. **검색 기능**: 세션 파일 전체 검색 명령어
