# Idle RPG Server API 통신 규약 (Unity 클라이언트용)

> **작성일**: 2025-10-02  
> **서버 버전**: v1.0  
> **서버 주소**: `https://localhost:7122` (개발 환경)

---

## 📋 목차
1. [인증 방식](#인증-방식)
2. [공통 응답 형식](#공통-응답-형식)
3. [에러 처리](#에러-처리)
4. [API 엔드포인트](#api-엔드포인트)
   - [인증 API](#1-인증-api)
   - [캐릭터 API](#2-캐릭터-api)

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

---

## 🔄 업데이트 이력
- **v1.0** (2025-10-02): 초기 문서 작성 (인증, 캐릭터 API)
- **v1.1** (예정): 아이템 & 인벤토리 API 추가 예정
