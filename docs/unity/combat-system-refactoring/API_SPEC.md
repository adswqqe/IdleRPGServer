# Combat System Refactoring API 명세 (Unity 클라이언트용)

> **작성일**: 2025-10-29
> **서버 버전**: v1.8 (Combat System Refactoring)
> **Breaking Changes**: ⚠️ v1.7에서 API 엔드포인트 변경됨

---

## ⚠️ Breaking Changes (v1.7 → v1.8)

### 변경된 엔드포인트

#### Stage API (구 Dungeon API)
- `GET /api/dungeons/stages` → `GET /api/stages`
- `GET /api/dungeons/progress` → `GET /api/stages/progress`
- `POST /api/dungeons/clear` → `POST /api/stages/clear`

#### Battle Log API (구 Battle API)
- `GET /api/battle/logs` → `GET /api/battle-logs`
- `GET /api/battle/logs/recent` → `GET /api/battle-logs/recent`
- `GET /api/battle/stats` → `GET /api/battle-logs/stats`

### Response 형식
- **변경 없음**: DTO 구조는 동일하게 유지

### Migration Guide
1. Unity 프로젝트에서 API 베이스 URL 상수 업데이트
2. `ApiEndpoints.cs` 파일의 상수 변경
3. 기존 코드에서 `dungeons` → `stages`, `battle` → `battle-logs` 전역 검색/치환

---

## 📋 목차
1. [Stage API](#1-stage-api)
2. [Battle Log API](#2-battle-log-api)
3. [Special Dungeon API](#3-special-dungeon-api-미래-확장)
4. [Unity C# 구현 예시](#-unity-c-구현-예시)

---

## 1. Stage API

### 1.1 스테이지 목록 조회
**GET** `/api/stages`
**인증 필요**: ❌ (Anonymous - Public Read)

**Query Parameters**:
```
characterId: Guid (required) - 캐릭터 ID
difficulty: DungeonDifficulty (optional) - 난이도 필터 (Normal, Hard, Nightmare)
```

**Response (200 OK)**:
```json
[
  {
    "id": 1,
    "name": "숲의 입구",
    "difficulty": "Normal",
    "requiredLevel": 1,
    "finalGoldReward": 100,
    "finalExpReward": 50,
    "isAvailable": true
  },
  {
    "id": 1,
    "name": "숲의 입구",
    "difficulty": "Hard",
    "requiredLevel": 1,
    "finalGoldReward": 150,
    "finalExpReward": 75,
    "isAvailable": false
  }
]
```

**필드 설명**:
- `isAvailable`: 도전 가능 여부 (레벨 조건, 이전 스테이지 클리어 여부)
- `finalGoldReward`: 난이도 배율 적용된 최종 골드 보상
- `finalExpReward`: 난이도 배율 적용된 최종 경험치 보상

---

### 1.2 스테이지 진행도 조회
**GET** `/api/stages/progress`
**인증 필요**: ❌ (Anonymous)

**Query Parameters**:
```
characterId: Guid (required)
```

**Response (200 OK)**:
```json
{
  "characterId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "normalHighestStage": 5,
  "hardHighestStage": 2,
  "hellHighestStage": 0
}
```

**필드 설명**:
- `normalHighestStage`: Normal 난이도 최고 클리어 스테이지
- `hardHighestStage`: Hard 난이도 최고 클리어 스테이지
- `hellHighestStage`: Hell(Nightmare) 난이도 최고 클리어 스테이지

---

### 1.3 스테이지 클리어
**POST** `/api/stages/clear`
**인증 필요**: ✅ (JWT Bearer Token)

**Headers**:
```
Authorization: Bearer {access_token}
Content-Type: application/json
```

**Request Body**:
```json
{
  "characterId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "stageId": 5,
  "difficulty": "Normal"
}
```

**Response (200 OK)**:
```json
{
  "isSuccess": true,
  "reward": {
    "gold": 150,
    "experience": 75
  },
  "newHighestStage": 5,
  "isLevelUp": true,
  "currentLevel": 6,
  "droppedEquipments": [],
  "battleStatistics": {
    "totalTurns": 8,
    "totalDamageDealt": 450,
    "totalDamageTaken": 120,
    "criticalHitCount": 2,
    "evasionCount": 1
  }
}
```

**Error Responses**:
- **400 Bad Request**: 레벨 부족, 이전 스테이지 미클리어
  ```json
  {
    "isSuccess": false,
    "errorMessage": "레벨이 부족합니다. 필요 레벨: 10"
  }
  ```
- **401 Unauthorized**: JWT 토큰 없음 또는 만료
- **403 Forbidden**: 다른 플레이어의 캐릭터
- **404 Not Found**: 캐릭터 또는 스테이지 미존재

---

## 2. Battle Log API

### 2.1 전투 로그 조회 (페이징)
**GET** `/api/battle-logs`
**인증 필요**: ✅ (JWT Bearer Token)

**Query Parameters**:
```
characterId: Guid (required)
page: int (default: 1) - 페이지 번호
pageSize: int (default: 20) - 페이지당 항목 수
```

**Response (200 OK)**:
```json
{
  "logs": [
    {
      "id": "guid",
      "monsterName": "슬라임",
      "monsterLevel": 5,
      "isVictory": true,
      "experienceGained": 50,
      "goldGained": 100,
      "damageDealt": 450,
      "damageTaken": 120,
      "battleDate": "2025-10-29T10:30:00Z"
    }
  ],
  "currentPage": 1,
  "pageSize": 20,
  "totalCount": 150,
  "totalPages": 8
}
```

---

### 2.2 최근 전투 로그 조회
**GET** `/api/battle-logs/recent`
**인증 필요**: ✅ (JWT Bearer Token)

**Query Parameters**:
```
characterId: Guid (required)
count: int (default: 10) - 조회할 최근 로그 개수
```

**Response (200 OK)**:
```json
[
  {
    "id": "guid",
    "monsterName": "슬라임",
    "isVictory": true,
    "battleDate": "2025-10-29T10:30:00Z"
  }
]
```

---

### 2.3 전투 통계 조회
**GET** `/api/battle-logs/stats`
**인증 필요**: ✅ (JWT Bearer Token)

**Query Parameters**:
```
characterId: Guid (required)
```

**Response (200 OK)**:
```json
{
  "totalBattles": 150,
  "victories": 120,
  "defeats": 30,
  "winRate": 80.0,
  "totalDamageDealt": 67500,
  "totalDamageTaken": 18000
}
```

**필드 설명**:
- `winRate`: 승률 (백분율, 0.0 ~ 100.0)
- `totalDamageDealt`: 누적 입힌 데미지
- `totalDamageTaken`: 누적 받은 데미지

---

## 3. Special Dungeon API (미래 확장)

> 💡 **Phase 3에서 구현 예정**: 현재는 Placeholder만 존재

### 3.1 보스 던전 도전
**POST** `/api/special-dungeons/boss/challenge`
**인증 필요**: ✅ (JWT Bearer Token)

**Request Body**:
```json
{
  "characterId": "guid",
  "bossId": "guid"
}
```

**Response (200 OK)**:
```json
{
  "isVictory": true,
  "reward": {
    "gold": 1000,
    "experience": 500
  },
  "battleStatistics": {
    "totalTurns": 25,
    "totalDamageDealt": 5000,
    "totalDamageTaken": 800,
    "criticalHitCount": 10,
    "evasionCount": 3
  }
}
```

---

## 🛠 Unity C# 구현 예시

### HTTP 클라이언트 기본 설정

```csharp
using UnityEngine.Networking;
using System.Collections;

public class ApiClient : MonoBehaviour
{
    private const string BASE_URL = "https://localhost:7122";
    private string accessToken;

    // Authorization 헤더 추가
    private void SetAuthHeader(UnityWebRequest request)
    {
        if (!string.IsNullOrEmpty(accessToken))
        {
            request.SetRequestHeader("Authorization", $"Bearer {accessToken}");
        }
    }
}
```

---

### 스테이지 목록 조회 예시

```csharp
public IEnumerator GetAvailableStages(string characterId)
{
    string url = $"{BASE_URL}/api/stages?characterId={characterId}";

    using (UnityWebRequest request = UnityWebRequest.Get(url))
    {
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            var response = JsonUtility.FromJson<StageListResponseWrapper>(
                "{\"stages\":" + request.downloadHandler.text + "}"
            );

            foreach (var stage in response.stages)
            {
                Debug.Log($"Stage {stage.id}: {stage.name} (Available: {stage.isAvailable})");
            }
        }
        else
        {
            Debug.LogError($"스테이지 조회 실패: {request.downloadHandler.text}");
        }
    }
}
```

---

### 스테이지 클리어 예시

```csharp
public IEnumerator ClearStage(string characterId, int stageId, string difficulty)
{
    var clearRequest = new StageClearRequest
    {
        characterId = characterId,
        stageId = stageId,
        difficulty = difficulty
    };

    string jsonData = JsonUtility.ToJson(clearRequest);

    using (UnityWebRequest request = new UnityWebRequest($"{BASE_URL}/api/stages/clear", "POST"))
    {
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        SetAuthHeader(request);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            var response = JsonUtility.FromJson<StageClearResponse>(request.downloadHandler.text);

            if (response.isSuccess)
            {
                Debug.Log($"스테이지 클리어 성공! 골드: {response.reward.gold}, 경험치: {response.reward.experience}");

                if (response.isLevelUp)
                {
                    Debug.Log($"레벨업! 현재 레벨: {response.currentLevel}");
                }
            }
            else
            {
                Debug.LogWarning($"스테이지 클리어 실패: {response.errorMessage}");
            }
        }
        else if (request.responseCode == 400)
        {
            var errorResponse = JsonUtility.FromJson<StageClearResponse>(request.downloadHandler.text);
            Debug.LogError($"클리어 실패: {errorResponse.errorMessage}");
        }
        else
        {
            Debug.LogError($"API 요청 실패: {request.downloadHandler.text}");
        }
    }
}
```

---

### 전투 로그 조회 예시

```csharp
public IEnumerator GetBattleLogs(string characterId, int page = 1, int pageSize = 20)
{
    string url = $"{BASE_URL}/api/battle-logs?characterId={characterId}&page={page}&pageSize={pageSize}";

    using (UnityWebRequest request = UnityWebRequest.Get(url))
    {
        SetAuthHeader(request);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            var response = JsonUtility.FromJson<BattleLogListResponse>(request.downloadHandler.text);

            Debug.Log($"전투 로그 총 {response.totalCount}개 (현재 페이지: {response.currentPage}/{response.totalPages})");

            foreach (var log in response.logs)
            {
                string result = log.isVictory ? "승리" : "패배";
                Debug.Log($"[{log.battleDate}] vs {log.monsterName} - {result}");
            }
        }
        else
        {
            Debug.LogError($"전투 로그 조회 실패: {request.downloadHandler.text}");
        }
    }
}
```

---

## 📝 추가 참고사항

### ApiEndpoints 상수 관리

```csharp
public static class ApiEndpoints
{
    // v1.8 - Combat System Refactoring
    public const string STAGES_LIST = "/api/stages";
    public const string STAGES_PROGRESS = "/api/stages/progress";
    public const string STAGES_CLEAR = "/api/stages/clear";

    public const string BATTLE_LOGS = "/api/battle-logs";
    public const string BATTLE_LOGS_RECENT = "/api/battle-logs/recent";
    public const string BATTLE_LOGS_STATS = "/api/battle-logs/stats";

    // Phase 3 예정
    public const string SPECIAL_DUNGEONS_BOSS = "/api/special-dungeons/boss/challenge";
}
```

---

### 난이도 Enum

```csharp
public enum DungeonDifficulty
{
    Normal,
    Hard,
    Nightmare  // Unity에서는 "Hell" 대신 "Nightmare" 사용
}
```

---

### 에러 처리 모범 사례

```csharp
private IEnumerator HandleApiRequest(UnityWebRequest request)
{
    yield return request.SendWebRequest();

    switch (request.result)
    {
        case UnityWebRequest.Result.Success:
            // 성공 처리
            break;

        case UnityWebRequest.Result.ConnectionError:
            Debug.LogError("네트워크 연결 실패. 인터넷 연결을 확인하세요.");
            break;

        case UnityWebRequest.Result.ProtocolError:
            if (request.responseCode == 401)
            {
                Debug.LogWarning("토큰 만료. 재로그인 필요.");
                // 토큰 갱신 또는 로그인 화면으로 이동
            }
            else if (request.responseCode == 400)
            {
                Debug.LogError($"잘못된 요청: {request.downloadHandler.text}");
            }
            break;
    }
}
```

---

## 🔄 업데이트 이력
- **v1.8** (2025-10-29): Combat System Refactoring
  - Stage API 엔드포인트 변경 (`/api/dungeons` → `/api/stages`)
  - Battle Log API 엔드포인트 변경 (`/api/battle` → `/api/battle-logs`)
  - Special Dungeon API Placeholder 추가
- **v1.7** (2025-10-20): Dungeon & Battle API 추가
