# Summarize Session with LLM

세션 파일이 커졌을 때 LLM으로 요약 생성합니다.

---

## 사용법

```bash
/summarizeSession [날짜]
```

**예시**:
```bash
/summarizeSession 2025-10-21
/summarizeSession  # 날짜 생략 시 오늘
```

---

## 작업 순서

### 1. 대상 파일 확인

날짜 파라미터를 확인:
- 제공되면: 해당 날짜 사용
- 생략되면: 오늘 날짜 사용

아카이브 파일 경로:
- `.claude/memories/9-archive/checkpoints/week{X}-day{Y}.md`

### 2. 파일 크기 확인

```bash
FILE_SIZE=$(stat -c%s "$ARCHIVE_FILE")
FILE_SIZE_MB=$((FILE_SIZE / 1048576))

if [ "$FILE_SIZE_MB" -lt 1 ]; then
    echo "파일 크기가 1MB 미만입니다. 요약 불필요."
    exit 0
fi
```

### 3. 세션 내용 읽기

아카이브 파일 전체 내용을 읽어서 변수에 저장.

### 4. LLM 요약 생성

**Zen MCP의 chat 도구 사용**:

```
Model: google/gemini-2.5-pro
Thinking Mode: medium

Prompt:
"다음은 하루 동안의 개발 세션 기록입니다 (${FILE_SIZE_MB}MB).
간결하게 요약해주세요:

## 요약 형식

### 📝 주요 성과
- 구현한 기능/API/시스템 나열

### 🤝 기술적 결정
- 내린 의사결정과 이유

### 🐛 발견한 이슈
- 문제점과 해결 여부

### 👤 TODO(human)
- 협업 대기 중인 항목

### ⏭️ 다음 작업
- 남은 TODO와 우선순위

---
[세션 내용]
"
```

### 5. 요약 결과 저장

원본 아카이브 파일을 백업:
- `week{X}-day{Y}-full.md` (원본)

요약본을 메인 파일로 저장:
- `week{X}-day{Y}.md` (요약본)

### 6. 결과 출력

```
✅ LLM 요약 생성 완료

📊 파일 크기:
- 원본: ${FILE_SIZE_MB}MB
- 요약본: ${SUMMARY_SIZE_MB}MB
- 압축률: ${COMPRESSION_RATE}%

📂 저장 위치:
- 원본: 9-archive/checkpoints/week{X}-day{Y}-full.md
- 요약: 9-archive/checkpoints/week{X}-day{Y}.md

💡 Insight:
[LLM이 생성한 주요 인사이트]
```

---

## 자동 트리거

SessionEnd Hook에서 파일 크기가 1MB 이상이면:
1. `.claude/memories/2-session/.needs-summary` 플래그 파일 생성
2. 사용자에게 알림:
   ```
   ⚠️  오늘 세션 파일이 크므로 LLM 요약을 권장합니다.
   실행: /summarizeSession 2025-10-21
   ```

---

## 주의사항

1. **LLM 비용**: Gemini 2.5 Pro는 1M tokens당 $1.25 (입력)
   - 10MB 세션 ≈ 2.5M tokens ≈ $3.13
   - 필요할 때만 실행 권장

2. **원본 보존**: 항상 `-full.md` 백업 생성

3. **수동 실행**: SessionEnd에서 자동 실행 안 함 (비용 때문)

---

## 향후 개선

1. **자동 요약 옵션**: `.claude/settings.local.json`에 `"autoSummarize": true` 추가
2. **요약 레벨**: "간단/상세/매우상세" 선택 가능
3. **로컬 LLM**: Ollama 연동해서 무료 요약
