# Checkpoint: Save Progress & Update Unity Docs

현재 작업 진행사항을 메모리에 저장하고 Unity 문서를 자동으로 업데이트합니다.

## 수행 단계

### 1. 현재 작업 진행사항 메모리 저장
- CLAUDE.md의 "Current State" 섹션을 읽어서 현재 작업 상태 확인
- 완료된 기능, 진행 중인 작업, 다음 단계를 요약
- Serena memory에 타임스탬프와 함께 진행사항 저장 (write_memory 사용)

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

#### 5.1 API_SPEC_FOR_UNITY.md 업데이트
차이가 있는 경우:
- `API_SPEC_FOR_UNITY.md`의 "구현 상태" 테이블 업데이트
- JSON 응답 예시 갱신 (DTO 구조 변경 시)
- "마지막 업데이트" 날짜를 오늘 날짜로 변경
- "업데이트 이력" 섹션에 변경사항 기록

차이가 없는 경우:
- "Unity 문서가 최신 상태입니다" 메시지 출력

#### 5.2 Unity-DTOs.cs 동기화 (중요!)
**서버 DTO 구조 변경 시 반드시 동기화 필요:**
- `IdleRPG.Application/DTOs/` 폴더의 실제 DTO 클래스 확인
- `../IdleRPGClient/Docs/Unity-DTOs.cs` 파일의 DTO 클래스와 비교
- 불일치 발견 시:
  - Unity-DTOs.cs의 해당 클래스 업데이트
  - `[Serializable]` 속성 유지 (Unity JsonUtility 필수)
  - XML 주석으로 변경사항 및 BREAKING CHANGE 표시
  - 사용 예시 코드도 함께 수정

**동기화 대상 DTO:**
- AuthResponse, ProfileResponse
- CharacterData, CharacterStats
- AddExperienceRequest
- 기타 요청/응답 DTO 클래스

### 6. 결과 보고
다음 정보를 사용자에게 보고:
- 💾 메모리에 저장된 진행사항 요약
- 📊 서버 API 구현 상태 요약
- 📝 Unity 문서 업데이트 여부 및 변경 내역
  - API_SPEC_FOR_UNITY.md 변경사항
  - Unity-DTOs.cs 동기화 여부 및 변경된 클래스 목록
- 📌 다음 작업 제안 (CLAUDE.md의 "Current Task" 기반)

## 사용 예시
```
/checkpoint
```

## 참고
- 이 명령어는 작업 세션 종료 전이나 주요 기능 완성 후 실행하세요
- 메모리는 Serena memory에 저장됩니다 (`write_memory` 사용)
- Unity 문서 경로:
  - API 스펙: `../IdleRPGClient/Docs/unity/API_SPEC_FOR_UNITY.md`
  - DTO 클래스: `../IdleRPGClient/Docs/Unity-DTOs.cs` ⚠️ 동기화 필수!
- DTO 구조 변경 시 Unity-DTOs.cs를 반드시 함께 업데이트해야 Unity 개발자가 올바른 클래스를 사용할 수 있습니다
