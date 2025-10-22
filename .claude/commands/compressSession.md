# Compress Current Session Memory

현재 진행 중인 세션 메모리를 Gemini를 사용해 실시간으로 압축합니다.

**용도**: SessionEnd를 기다리지 않고 현재 세션을 on-demand로 압축

---

## 사용법

```bash
/compressSession
```

**사용 시점**: 언제든지 원할 때 실행

---

## 작업 순서

### 1. 현재 세션 파일 확인

오늘 날짜의 세션 파일 경로:
```bash
TODAY=$(date +%Y-%m-%d)
SESSION_FILE=".claude/memories/2-session/daily-$TODAY.md"
```

파일 존재 여부 확인:
```bash
if [ ! -f "$SESSION_FILE" ]; then
    echo "❌ 오늘 세션 파일이 없습니다: $SESSION_FILE"
    exit 1
fi
```

### 2. 토큰 수 확인

```bash
CHAR_COUNT=$(wc -m < "$SESSION_FILE")
TOKEN_COUNT=$(awk "BEGIN {printf \"%.0f\", $CHAR_COUNT / 3}")
TOKEN_COUNT_K=$(awk "BEGIN {printf \"%.1f\", $TOKEN_COUNT / 1000}")

echo "📊 현재 세션 메모리: ~${TOKEN_COUNT_K}k tokens"
```

### 3. 세션 내용 읽기

Read 도구로 세션 파일 전체 내용을 읽어서 변수에 저장.

### 4. Gemini로 압축 요약 생성

**mcp__zen__clink 도구 사용**:

```
CLI Name: gemini
Role: default (또는 명시 안 함)

Prompt:
"다음은 현재 진행 중인 개발 세션 기록입니다 (~${TOKEN_COUNT_K}k tokens).

**중요**: 원본의 모든 핵심 정보를 보존하면서 압축해주세요.

## 압축 규칙
1. **보존 필수**:
   - 모든 기술적 결정과 그 이유
   - 완료된 기능/API 목록 (구체적인 코드 예제는 제거)
   - 중요한 TODO(human) 항목
   - 발견한 이슈와 해결 방법
   - 다음 작업 우선순위

2. **제거 가능**:
   - 긴 코드 블록 (핵심 로직만 1-2줄로 요약)
   - 반복적인 설명
   - 일반적인 인사말/감사 메시지
   - 중간 과정의 디버깅 로그

3. **형식 유지**:
   - 카테고리별 섹션 구조 유지 (🔧 API 변경, 💾 DB/DTO 등)
   - 타임스탬프 범위로 압축 (예: [14:30-15:45] 대신 개별 타임스탬프)

## 요약 형식

### 📝 주요 성과 (완료된 작업)
- [기능명]: [간결한 설명 1-2줄]

### 🤝 기술적 결정
- [결정사항]: [이유 1줄]

### 🐛 발견한 이슈
- [이슈]: [해결방법 1줄]

### 👤 TODO(human)
- [협업 대기 항목]

### ⏭️ 다음 작업
- [우선순위 1, 2, 3...]

---
[세션 내용]
"
```

**모델 선택**: `gemini` (Gemini CLI - 자동으로 최신 모델 선택)

### 5. 백업 및 압축본 저장

**백업 생성**:
```bash
BACKUP_FILE="${SESSION_FILE%.md}-full.md"
cp "$SESSION_FILE" "$BACKUP_FILE"
echo "💾 원본 백업: $BACKUP_FILE"
```

**압축본 저장**:
- Gemini가 생성한 요약을 `$SESSION_FILE`에 덮어쓰기
- Write 도구 사용

### 6. 압축 결과 확인

```bash
NEW_CHAR=$(wc -m < "$SESSION_FILE")
NEW_TOKEN=$(awk "BEGIN {printf \"%.0f\", $NEW_CHAR / 3}")
NEW_TOKEN_K=$(awk "BEGIN {printf \"%.1f\", $NEW_TOKEN / 1000}")

COMPRESSION_RATE=$(awk "BEGIN {printf \"%.1f\", (1 - $NEW_TOKEN / $TOKEN_COUNT) * 100}")
```

### 7. 결과 출력

```
✅ 세션 메모리 압축 완료

📊 압축 결과:
- 원본: ~${TOKEN_COUNT_K}k tokens
- 압축: ~${NEW_TOKEN_K}k tokens
- 압축률: ${COMPRESSION_RATE}%

📂 저장 위치:
- 원본 백업: 2-session/daily-{today}-full.md
- 압축본: 2-session/daily-{today}.md (현재 활성 세션)

💡 작업을 계속 진행하세요. 다음 Stop Hook부터는 압축된 세션에 내용이 추가됩니다.
```

---

## /summarizeSession과의 차이

| 기능 | /compressSession | /summarizeSession |
|------|------------------|-------------------|
| **대상** | 현재 활성 세션 (`2-session/`) | 아카이브된 세션 (`9-archive/`) |
| **시점** | 세션 진행 중 언제든지 | SessionEnd 이후 |
| **목적** | 실시간 파일 크기 관리 | 장기 보관용 요약 |
| **백업** | `-full.md` (같은 폴더) | `-full.md` (같은 폴더) |
| **작업 계속** | ✅ 압축 후 세션 계속 진행 | ❌ 이미 종료된 세션 |

---

## 주의사항

1. **LLM 비용**: Gemini CLI 사용 (비용은 zen MCP 설정에 따름)
   - 예상: ~1000k tokens ≈ $1.5-2 (Gemini 2.5 Pro 기준)

2. **원본 보존**: 항상 `-full.md` 백업 생성됨
   - 압축 후 중요한 정보가 누락되었다면 백업 참조

3. **Stop Hook 동작**: 압축 후에도 Stop Hook는 계속 동작
   - 새로운 대화는 압축된 파일에 계속 추가됨
   - 하루 종료 시 다시 압축 가능

4. **여러 번 압축 가능**: 하루에 2-3번 압축 가능
   - 아침 세션 → 압축 → 오후 세션 → 다시 압축
   - 각 압축마다 `-full.md`는 덮어써짐 (가장 최근 원본만 보존)

---

## 향후 개선

1. **자동 압축 옵션**: Stop Hook에서 3MB 초과 시 자동 압축
2. **압축 레벨**: "빠른/보통/철저한" 선택 가능
3. **로컬 LLM**: Ollama 연동해서 무료 압축
4. **증분 압축**: 마지막 압축 이후 추가된 내용만 처리

---

## 예시 워크플로우

```
09:00 - Claude Code 시작
      → SessionStart Hook (세션 파일 생성)

09:00-12:00 - 작업 진행
      → Stop Hook (대화 누적: ~800k tokens)

12:00 - /compressSession 실행
      → 압축: ~800k tokens → ~250k tokens (68% 압축)
      → 백업: daily-2025-10-21-full.md

12:00-18:00 - 작업 계속
      → Stop Hook (압축본에 계속 추가: ~250k → ~650k tokens)

18:00 - /compressSession 다시 실행
      → 압축: ~650k tokens → ~200k tokens (69% 압축)

18:00 - Claude Code 종료
      → SessionEnd Hook (아카이브로 이동)
```
