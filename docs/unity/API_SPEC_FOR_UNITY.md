# Idle RPG Server API 통신 규약 (Unity 클라이언트용)

> **작성일**: 2025-10-02
> **서버 버전**: v1.8 (Combat System Refactoring)
> **서버 주소**: `https://localhost:7122` (개발 환경)

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
1. [인증 방식](#인증-방식)
2. [공통 응답 형식](#공통-응답-형식)
3. [에러 처리](#에러-처리)
4. [API 엔드포인트](#api-엔드포인트)
   - [인증 API](#1-인증-api)
   - [캐릭터 API](#2-캐릭터-api)
   - [스테이지 API](#3-스테이지-api)
   - [전투 로그 API](#4-전투-로그-api)

---

## 🔐 인증 방식

### JWT Bearer Token 인증
모든 인증이 필요한 API는 HTTP Header에 JWT 토큰을 포함해야 합니다.

```
Authorization: Bearer {access_token}
```

### 토큰 종류
- **Access Token**: API 호출용 (유효기간: 60분)
- **Refresh Token**: Access Token 갱신용 (유효기간: 7일)

### 토큰 갱신 플로우
1. Access Token 만료 시 `401 Unauthorized` 응답
2. Refresh Token으로 `/api/auth/refresh` 호출
3. 새로운 Access Token + Refresh Token 발급

---

## 📦 공통 응답 형식

### 성공 응답
```json
{
  "응답데이터필드명": { ... }
}
```

### 실패 응답
```json
{
  "message": "에러 메시지"
}
```

### HTTP 상태 코드
- `200 OK`: 성공
- `400 Bad Request`: 잘못된 요청 (유효성 검증 실패)
- `401 Unauthorized`: 인증 필요 또는 토큰 만료
- `404 Not Found`: 리소스 없음
- `500 Internal Server Error`: 서버 오류

---

## 🎮 API 엔드포인트

## 1. 인증 API

### 1.1 회원가입
**POST** `/api/auth/register`

**Request Body**
```json
{
  "username": "testuser",
  "email": "test@example.com",
  "password": "Test1234!",
  "confirmPassword": "Test1234!"
}
```

**Response (200 OK)**
```json
{
  "response": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresIn": 3600,
    "userName": "testuser",
    "playerId": "550e8400-e29b-41d4-a716-446655440000"
  }
}
```

**Error Response (400 Bad Request)**
```json
{
  "message": "이미 존재하는 사용자입니다"
}
```

---

### 1.2 로그인
**POST** `/api/auth/login`

**Request Body**
```json
{
  "username": "testuser",
  "password": "Test1234!"
}
```

**Response (200 OK)**
```json
{
  "response": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresIn": 3600,
    "userName": "testuser",
    "playerId": "550e8400-e29b-41d4-a716-446655440000"
  }
}
```

**Error Response (401 Unauthorized)**
```json
{
  "message": "아이디 또는 비밀번호가 일치하지 않습니다"
}
```

---

### 1.3 토큰 갱신
**POST** `/api/auth/refresh`

**Request Body**
```json
{
  "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**Response (200 OK)**
```json
{
  "response": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresIn": 3600,
    "userName": "testuser",
    "playerId": "550e8400-e29b-41d4-a716-446655440000"
  }
}
```

---

## 2. 캐릭터 API

### 2.1 캐릭터 생성
**POST** `/api/character`  
**인증 필요**: ✅

**Headers**
```
Authorization: Bearer {access_token}
Content-Type: application/json
```

**Request Body**
```json
{}
```
> 빈 객체 전송 (기본 캐릭터 자동 생성)

**Response (200 OK)**
```json
{
  "response": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "level": 1,
    "experience": 0,
    "statPoints": 0,
    "stats": {
      "strength": 10,
      "dexterity": 10,
      "intelligence": 10,
      "vitality": 10
    },
    "createdAt": "2025-10-02T12:00:00Z",
    "updatedAt": "2025-10-02T12:00:00Z"
  }
}
```

**Error Response (400 Bad Request)**
```json
{
  "message": "캐릭터는 최대 3개까지만 생성할 수 있습니다"
}
```

---

### 2.2 캐릭터 목록 조회
**GET** `/api/character`  
**인증 필요**: ✅

**Headers**
```
Authorization: Bearer {access_token}
```

**Response (200 OK)**
```json
{
  "response": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "level": 5,
      "experience": 120,
      "statPoints": 3,
      "stats": {
        "strength": 15,
        "dexterity": 12,
        "intelligence": 10,
        "vitality": 13
      },
      "createdAt": "2025-10-01T10:00:00Z",
      "updatedAt": "2025-10-02T12:00:00Z"
    }
  ]
}
```

---

### 2.3 특정 캐릭터 조회
**GET** `/api/character/{characterId}`  
**인증 필요**: ✅

**Headers**
```
Authorization: Bearer {access_token}
```

**Response (200 OK)**
```json
{
  "response": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "level": 5,
    "experience": 120,
    "statPoints": 3,
    "stats": {
      "strength": 15,
      "dexterity": 12,
      "intelligence": 10,
      "vitality": 13
    },
    "createdAt": "2025-10-01T10:00:00Z",
    "updatedAt": "2025-10-02T12:00:00Z"
  }
}
```

**Error Response (404 Not Found)**
```json
{
  "message": "캐릭터를 찾을 수 없습니다"
}
```

---

### 2.4 캐릭터 삭제
**DELETE** `/api/character/{characterId}`  
**인증 필요**: ✅

**Headers**
```
Authorization: Bearer {access_token}
```

**Response (200 OK)**
```json
{
  "message": "캐릭터가 삭제되었습니다"
}
```

---

### 2.5 경험치 획득 (레벨업)
**POST** `/api/character/{characterId}/experience`  
**인증 필요**: ✅

**Headers**
```
Authorization: Bearer {access_token}
Content-Type: application/json
```

**Request Body**
```json
{
  "amount": 150
}
```

**Response (200 OK)**
```json
{
  "response": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "level": 3,
    "experience": 50,
    "statPoints": 10,
    "stats": {
      "strength": 10,
      "dexterity": 10,
      "intelligence": 10,
      "vitality": 10
    },
    "createdAt": "2025-10-01T10:00:00Z",
    "updatedAt": "2025-10-02T12:30:00Z"
  }
}
```

**레벨업 규칙**
- 필요 경험치: `Level * 100`
  - Lv 1 → Lv 2: 100 경험치
  - Lv 2 → Lv 3: 200 경험치
  - Lv 3 → Lv 4: 300 경험치
- 레벨업 시 스탯 포인트 +5
- 초과 경험치는 다음 레벨로 이월

---

### 2.6 스탯 분배
**PUT** `/api/character/{characterId}/stats`  
**인증 필요**: ✅

**Headers**
```
Authorization: Bearer {access_token}
Content-Type: application/json
```

**Request Body**
```json
{
  "strength": 2,
  "dexterity": 1,
  "intelligence": 1,
  "vitality": 1
}
```

**Response (200 OK)**
```json
{
  "response": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "level": 3,
    "experience": 50,
    "statPoints": 5,
    "stats": {
      "strength": 12,
      "dexterity": 11,
      "intelligence": 11,
      "vitality": 11
    },
    "createdAt": "2025-10-01T10:00:00Z",
    "updatedAt": "2025-10-02T12:35:00Z"
  }
}
```

**Error Response (400 Bad Request)**
```json
{
  "message": "보유한 스탯 포인트가 부족합니다"
}
```

**스탯 분배 규칙**
- 요청한 스탯 합계가 현재 `statPoints` 이하여야 함
- 각 스탯은 음수 불가
- 사용한 만큼 `statPoints` 차감

---

## 3. 스테이지 API

### 3.1 스테이지 목록 조회
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

### 3.2 스테이지 진행도 조회
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

### 3.3 스테이지 클리어
**POST** `/api/stages/clear`
**인증 필요**: ✅

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

## 4. 전투 로그 API

### 4.1 전투 로그 조회 (페이징)
**GET** `/api/battle-logs`
**인증 필요**: ✅

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

### 4.2 최근 전투 로그 조회
**GET** `/api/battle-logs/recent`
**인증 필요**: ✅

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

### 4.3 전투 통계 조회
**GET** `/api/battle-logs/stats`
**인증 필요**: ✅

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

### 로그인 예시
```csharp
[System.Serializable]
public class LoginRequest
{
    public string username;
    public string password;
}

[System.Serializable]
public class LoginResponse
{
    public AuthData response;
}

[System.Serializable]
public class AuthData
{
    public string accessToken;
    public string refreshToken;
    public int expiresIn;
    public string userName;
    public string playerId;
}

public IEnumerator Login(string username, string password)
{
    var loginData = new LoginRequest { username = username, password = password };
    string jsonData = JsonUtility.ToJson(loginData);

    using (UnityWebRequest request = new UnityWebRequest($"{BASE_URL}/api/auth/login", "POST"))
    {
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            var response = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);
            accessToken = response.response.accessToken;
            Debug.Log($"로그인 성공: {response.response.userName}");
        }
        else
        {
            Debug.LogError($"로그인 실패: {request.downloadHandler.text}");
        }
    }
}
```

### 캐릭터 생성 예시
```csharp
[System.Serializable]
public class CharacterResponse
{
    public CharacterData response;
}

[System.Serializable]
public class CharacterData
{
    public string id;
    public int level;
    public int experience;
    public int statPoints;
    public CharacterStats stats;
    public string createdAt;
    public string updatedAt;
}

[System.Serializable]
public class CharacterStats
{
    public int strength;
    public int dexterity;
    public int intelligence;
    public int vitality;
}

public IEnumerator CreateCharacter()
{
    using (UnityWebRequest request = new UnityWebRequest($"{BASE_URL}/api/character", "POST"))
    {
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes("{}");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        SetAuthHeader(request);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            var response = JsonUtility.FromJson<CharacterResponse>(request.downloadHandler.text);
            Debug.Log($"캐릭터 생성 성공: Level {response.response.level}");
        }
        else
        {
            Debug.LogError($"캐릭터 생성 실패: {request.downloadHandler.text}");
        }
    }
}
```

---

### 스테이지 목록 조회 예시
```csharp
[System.Serializable]
public class StageListResponseWrapper
{
    public StageData[] stages;
}

[System.Serializable]
public class StageData
{
    public int id;
    public string name;
    public string difficulty;
    public int requiredLevel;
    public int finalGoldReward;
    public int finalExpReward;
    public bool isAvailable;
}

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
[System.Serializable]
public class StageClearRequest
{
    public string characterId;
    public int stageId;
    public string difficulty;
}

[System.Serializable]
public class StageClearResponse
{
    public bool isSuccess;
    public string errorMessage;
    public RewardData reward;
    public int newHighestStage;
    public bool isLevelUp;
    public int currentLevel;
    public BattleStatistics battleStatistics;
}

[System.Serializable]
public class RewardData
{
    public int gold;
    public int experience;
}

[System.Serializable]
public class BattleStatistics
{
    public int totalTurns;
    public int totalDamageDealt;
    public int totalDamageTaken;
    public int criticalHitCount;
    public int evasionCount;
}

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
[System.Serializable]
public class BattleLogListResponse
{
    public BattleLogData[] logs;
    public int currentPage;
    public int pageSize;
    public int totalCount;
    public int totalPages;
}

[System.Serializable]
public class BattleLogData
{
    public string id;
    public string monsterName;
    public int monsterLevel;
    public bool isVictory;
    public int experienceGained;
    public int goldGained;
    public int damageDealt;
    public int damageTaken;
    public string battleDate;
}

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

### 전투 통계 조회 예시
```csharp
[System.Serializable]
public class BattleStatsResponse
{
    public int totalBattles;
    public int victories;
    public int defeats;
    public float winRate;
    public int totalDamageDealt;
    public int totalDamageTaken;
}

public IEnumerator GetBattleStats(string characterId)
{
    string url = $"{BASE_URL}/api/battle-logs/stats?characterId={characterId}";

    using (UnityWebRequest request = UnityWebRequest.Get(url))
    {
        SetAuthHeader(request);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            var response = JsonUtility.FromJson<BattleStatsResponse>(request.downloadHandler.text);

            Debug.Log($"총 전투: {response.totalBattles}회");
            Debug.Log($"승률: {response.winRate}% ({response.victories}승 {response.defeats}패)");
            Debug.Log($"총 데미지: 입힌 데미지 {response.totalDamageDealt}, 받은 데미지 {response.totalDamageTaken}");
        }
        else
        {
            Debug.LogError($"전투 통계 조회 실패: {request.downloadHandler.text}");
        }
    }
}
```

---

## 📝 추가 참고사항

### ApiEndpoints 상수 관리 (v1.8)
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

    // 인증 API
    public const string AUTH_REGISTER = "/api/auth/register";
    public const string AUTH_LOGIN = "/api/auth/login";
    public const string AUTH_REFRESH = "/api/auth/refresh";

    // 캐릭터 API
    public const string CHARACTER = "/api/character";
    public const string CHARACTER_BY_ID = "/api/character/{0}";
    public const string CHARACTER_EXPERIENCE = "/api/character/{0}/experience";
    public const string CHARACTER_STATS = "/api/character/{0}/stats";
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

## 📝 추가 참고사항

### 개발 환경 SSL 인증서 무시 (Unity)
```csharp
// 개발 환경에서만 사용! 프로덕션에서는 제거 필수
#if UNITY_EDITOR
    System.Net.ServicePointManager.ServerCertificateValidationCallback = 
        (sender, certificate, chain, sslPolicyErrors) => true;
#endif
```

### 토큰 저장 (PlayerPrefs)
```csharp
public void SaveTokens(string accessToken, string refreshToken)
{
    PlayerPrefs.SetString("AccessToken", accessToken);
    PlayerPrefs.SetString("RefreshToken", refreshToken);
    PlayerPrefs.Save();
}

public string GetAccessToken()
{
    return PlayerPrefs.GetString("AccessToken", "");
}
```

### 401 에러 처리 (자동 재로그인)
```csharp
if (request.responseCode == 401)
{
    // 토큰 갱신 시도
    yield return RefreshToken();
    
    // 원래 요청 재시도
    yield return RetryRequest(request);
}
```

---

---

## 📦 Unity DTO 클래스 정의

> Unity 프로젝트에서 사용할 DTO 클래스들입니다. 아래 클래스들을 그대로 복사해서 사용하세요.

### AuthDTO.cs
```csharp
using System;

namespace IdleRPG.Network.DTO
{
    // === 요청 DTO ===
    
    [Serializable]
    public class RegisterRequest
    {
        public string username;
        public string email;
        public string password;
        public string confirmPassword;
    }

    [Serializable]
    public class LoginRequest
    {
        public string username;
        public string password;
    }

    [Serializable]
    public class RefreshTokenRequest
    {
        public string refreshToken;
    }

    // === 응답 DTO ===
    
    [Serializable]
    public class AuthResponseWrapper
    {
        public AuthData response;
    }

    [Serializable]
    public class AuthData
    {
        public string accessToken;
        public string refreshToken;
        public int expiresIn;
        public string userName;
        public string playerId;
    }
}
```

### CharacterDTO.cs
```csharp
using System;

namespace IdleRPG.Network.DTO
{
    // === 요청 DTO ===
    
    [Serializable]
    public class CreateCharacterRequest
    {
        // 빈 객체 (서버에서 자동 생성)
    }

    [Serializable]
    public class AddExperienceRequest
    {
        public int amount;
    }

    [Serializable]
    public class AllocateStatsRequest
    {
        public int strength;
        public int dexterity;
        public int intelligence;
        public int vitality;
    }

    // === 응답 DTO ===
    
    [Serializable]
    public class CharacterResponseWrapper
    {
        public CharacterData response;
    }

    [Serializable]
    public class CharacterListResponseWrapper
    {
        public CharacterData[] response;
    }

    [Serializable]
    public class CharacterData
    {
        public string id;
        public int level;
        public int experience;
        public int statPoints;
        public CharacterStats stats;
        public string createdAt;
        public string updatedAt;
    }

    [Serializable]
    public class CharacterStats
    {
        public int strength;
        public int dexterity;
        public int intelligence;
        public int vitality;
    }
}
```

### ErrorDTO.cs
```csharp
using System;

namespace IdleRPG.Network.DTO
{
    [Serializable]
    public class ErrorResponse
    {
        public string message;
    }
}
```

### DeleteCharacterDTO.cs
```csharp
using System;

namespace IdleRPG.Network.DTO
{
    [Serializable]
    public class DeleteCharacterResponseWrapper
    {
        public string message;
    }
}
```

### StageDTO.cs
```csharp
using System;

namespace IdleRPG.Network.DTO
{
    // === 요청 DTO ===

    [Serializable]
    public class StageClearRequest
    {
        public string characterId;
        public int stageId;
        public string difficulty;
    }

    // === 응답 DTO ===

    [Serializable]
    public class StageListResponseWrapper
    {
        public StageData[] stages;
    }

    [Serializable]
    public class StageData
    {
        public int id;
        public string name;
        public string difficulty;
        public int requiredLevel;
        public int finalGoldReward;
        public int finalExpReward;
        public bool isAvailable;
    }

    [Serializable]
    public class StageProgressResponse
    {
        public string characterId;
        public int normalHighestStage;
        public int hardHighestStage;
        public int hellHighestStage;
    }

    [Serializable]
    public class StageClearResponse
    {
        public bool isSuccess;
        public string errorMessage;
        public RewardData reward;
        public int newHighestStage;
        public bool isLevelUp;
        public int currentLevel;
        public EquipmentData[] droppedEquipments;
        public BattleStatistics battleStatistics;
    }

    [Serializable]
    public class RewardData
    {
        public int gold;
        public int experience;
    }

    [Serializable]
    public class BattleStatistics
    {
        public int totalTurns;
        public int totalDamageDealt;
        public int totalDamageTaken;
        public int criticalHitCount;
        public int evasionCount;
    }

    [Serializable]
    public class EquipmentData
    {
        public string id;
        public string name;
        public string type;
        public int attack;
        public int defense;
    }
}
```

### BattleLogDTO.cs
```csharp
using System;

namespace IdleRPG.Network.DTO
{
    // === 응답 DTO ===

    [Serializable]
    public class BattleLogListResponse
    {
        public BattleLogData[] logs;
        public int currentPage;
        public int pageSize;
        public int totalCount;
        public int totalPages;
    }

    [Serializable]
    public class BattleLogData
    {
        public string id;
        public string monsterName;
        public int monsterLevel;
        public bool isVictory;
        public int experienceGained;
        public int goldGained;
        public int damageDealt;
        public int damageTaken;
        public string battleDate;
    }

    [Serializable]
    public class RecentBattleLogData
    {
        public string id;
        public string monsterName;
        public bool isVictory;
        public string battleDate;
    }

    [Serializable]
    public class BattleStatsResponse
    {
        public int totalBattles;
        public int victories;
        public int defeats;
        public float winRate;
        public int totalDamageDealt;
        public int totalDamageTaken;
    }
}
```

---

## 🔄 업데이트 이력
- **v1.8** (2025-10-29): Combat System Refactoring
  - Stage API 엔드포인트 변경 (`/api/dungeons` → `/api/stages`)
  - Battle Log API 엔드포인트 변경 (`/api/battle` → `/api/battle-logs`)
  - StageDTO, BattleLogDTO 추가
  - ApiEndpoints 상수 업데이트
- **v1.0** (2025-10-02): 초기 문서 작성 (인증, 캐릭터 API)
- **v1.1** (예정): 아이템 & 인벤토리 API 추가 예정
