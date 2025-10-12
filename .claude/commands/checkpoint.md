# Checkpoint: Save Progress & Update Unity Docs

현재 작업 진행사항을 메모리에 저장하고 Unity 문서를 자동으로 업데이트합니다.

## 수행 단계

### 1. 현재 작업 진행사항 메모리 저장
- CLAUDE.md의 "Current State" 섹션을 읽어서 현재 작업 상태 확인
- 완료된 기능, 진행 중인 작업, 다음 단계를 요약
- memory-bank에 타임스탬프와 함께 진행사항 저장

### 2. 서버 API 구현 상태 확인
다음 API Controller 파일들을 확인하여 구현 여부 파악:
- `IdleRPG.API/Controllers/AuthController.cs` - 인증 API
- `IdleRPG.API/Controllers/CharacterController.cs` - 캐릭터 API

각 엔드포인트의 구현 상태를 다음 기준으로 분류:
- ✅ **완료**: Controller에 엔드포인트가 구현되고 로직이 완성됨
- 🚧 **개발 중**: Controller에 엔드포인트가 있지만 로직이 미완성
- 📋 **예정**: Controller에 엔드포인트가 없음

### 3. Unity 문서 현재 상태 확인
`../IdleRPGClient/Docs/unity/API_SPEC_FOR_UNITY.md` 파일을 읽어서:
- "구현 상태" 섹션의 API별 구현 상태 테이블 확인
- 각 API의 현재 상태 (✅ 완료, 🚧 개발 중, 📋 예정) 파악

### 4. 서버 vs Unity 문서 비교
서버의 실제 구현 상태와 Unity 문서의 상태를 비교:
- 차이가 있는 API 엔드포인트 식별
- 업데이트가 필요한 항목 리스트 작성

### 5. Unity 문서 자동 업데이트
차이가 있는 경우:
- `API_SPEC_FOR_UNITY.md`의 "구현 상태" 테이블 업데이트
- "마지막 업데이트" 날짜를 오늘 날짜로 변경
- 변경 내역 요약 출력

차이가 없는 경우:
- "Unity 문서가 최신 상태입니다" 메시지 출력

### 6. 결과 보고
다음 정보를 사용자에게 보고:
- 💾 메모리에 저장된 진행사항 요약
- 📊 서버 API 구현 상태 요약
- 📝 Unity 문서 업데이트 여부 및 변경 내역
- 📌 다음 작업 제안 (CLAUDE.md의 "Current Task" 기반)

## 사용 예시
```
/checkpoint
```

## 참고
- 이 명령어는 작업 세션 종료 전이나 주요 기능 완성 후 실행하세요
- 메모리는 `memory-bank/` 폴더에 저장됩니다
- Unity 문서 경로: `../IdleRPGClient/Docs/unity/API_SPEC_FOR_UNITY.md`
