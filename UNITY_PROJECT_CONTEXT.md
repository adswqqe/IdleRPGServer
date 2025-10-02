# Unity 클라이언트를 위한 서버 프로젝트 정보

> **서버 프로젝트명**: Idle RPG Server  
> **작성일**: 2025-10-02  
> **아키텍처**: Clean Architecture  
> **프레임워크**: ASP.NET Core 8.0 Web API

---

## 📋 프로젝트 개요

### 게임 장르
**방치형 RPG (Idle RPG)**
- 오프라인 보상 시스템
- 자동 전투
- 캐릭터 성장 (레벨업, 스탯)
- 아이템 & 인벤토리

### 서버 기술 스택
- **Framework**: ASP.NET Core 8.0 Web API
- **Database**: PostgreSQL + Entity Framework Core 9.0
- **Cache**: Redis (준비됨, 아직 미사용)
- **Authentication**: JWT Bearer Token
- **Architecture Pattern**: Clean Architecture, Repository Pattern, Service Pattern

---

## 🏗 Clean Architecture 구조

### 프로젝트 계층
```
IdleRPG.API           # API Layer (Controllers, Middleware)
IdleRPG.Application   # Business Logic (Services, DTOs, Interfaces)
IdleRPG.Domain        # Core Domain (Entities, Value Objects, Repository Interfaces)
IdleRPG.Infrastructure # Data Access (EF Core, Repositories)
```

### 의존성 방향
```
API → Application → Domain
         ↑
    Infrastructure
```

---

## 🔐 인증 시스템

### JWT 토큰 방식
- **Access Token**: 유효기간 60분
- **Refresh Token**: 유효기간 7일

### 토큰 전송 방식
```
Authorization: Bearer {access_token}
```

### 토큰 갱신 플로우
1. Access Token 만료 → `401 Unauthorized`
2. Refresh Token으로 `/api/auth/refresh` 호출
3. 새 Access Token + Refresh Token 발급

---

## 🎮 현재 구현된 기능 (Week 1)

### 1. 인증 시스템
- ✅ 회원가입 (`POST /api/auth/register`)
- ✅ 로그인 (`POST /api/auth/login`)
- ✅ 토큰 갱신 (`POST /api/auth/refresh`)

### 2. 캐릭터 시스템
- ✅ 캐릭터 생성 (`POST /api/character`)
- ✅ 캐릭터 목록 조회 (`GET /api/character`)
- ✅ 특정 캐릭터 조회 (`GET /api/character/{id}`)
- ✅ 캐릭터 삭제 (`DELETE /api/character/{id}`)
- ✅ 경험치 획득 & 레벨업 (`POST /api/character/{id}/experience`)
- ✅ 스탯 분배 (`PUT /api/character/{id}/stats`)

### 캐릭터 레벨업 규칙
- 필요 경험치: `현재레벨 * 100`
- 레벨업 시 스탯 포인트 +5
- 초과 경험치는 다음 레벨로 이월

### 캐릭터 기본 스탯
```
Strength: 10
Dexterity: 10
Intelligence: 10
Vitality: 10
```

---

## 🔜 개발 예정 기능

### Week 2: 아이템 & 인벤토리 시스템 (진행 예정)
- ItemTemplate (아이템 마스터 데이터)
- PlayerItem (플레이어 소유 아이템)
- 아이템 획득/사용/장착 시스템

### Week 3: 오프라인 보상 시스템
- 로그아웃 시간 계산
- 오프라인 진행 보상
- Background Service

### 추후 개발 예정
- SignalR 실시간 통신
- 길드 시스템
- 채팅 시스템

---

## 🌐 서버 환경

### 개발 환경
- **서버 주소**: `https://localhost:7122`
- **HTTP 주소**: `http://localhost:5172`
- **Database**: PostgreSQL (localhost:5432)
- **pgAdmin**: http://localhost:8082

### Docker 서비스
```bash
./dev-start.sh    # PostgreSQL, Redis, pgAdmin 시작
docker-compose down  # 서비스 중지
```

---

## 📦 응답 형식

### 성공 응답 패턴
```json
{
  "response": { /* 데이터 */ }
}
```
또는
```json
{
  "응답필드명": { /* 데이터 */ }
}
```

### 에러 응답 패턴
```json
{
  "message": "에러 메시지"
}
```

### HTTP 상태 코드
- `200 OK`: 성공
- `400 Bad Request`: 잘못된 요청
- `401 Unauthorized`: 인증 실패/토큰 만료
- `404 Not Found`: 리소스 없음
- `500 Internal Server Error`: 서버 오류

---

## 🔄 데이터베이스 스키마

### Players 테이블
```sql
Id (uuid, PK)
UserName (varchar, unique)
PasswordHash (varchar)
Email (varchar)
IsActive (boolean)
CreatedAt (timestamp)
LastLoginAt (timestamp)
```

### Characters 테이블
```sql
Id (uuid, PK)
PlayerId (uuid, FK)
Level (int)
Experience (int)
StatPoints (int)
Stats (jsonb) -- { strength, dexterity, intelligence, vitality }
CreatedAt (timestamp)
UpdatedAt (timestamp)
```

### RefreshTokens 테이블
```sql
Id (uuid, PK)
PlayerId (uuid, FK)
Token (varchar, unique)
ExpiresAt (timestamp)
CreatedAt (timestamp)
```

---

## 💡 Unity 클라이언트 개발 시 주의사항

### 1. HTTPS 개발 환경
Unity에서 localhost HTTPS 연결 시 SSL 인증서 검증 우회 필요:
```csharp
#if UNITY_EDITOR
    System.Net.ServicePointManager.ServerCertificateValidationCallback = 
        (sender, certificate, chain, sslPolicyErrors) => true;
#endif
```

### 2. 토큰 관리
- Access Token은 메모리에 저장
- Refresh Token은 PlayerPrefs 또는 보안 스토리지에 저장
- 401 응답 시 자동 토큰 갱신 로직 구현 권장

### 3. JSON 직렬화
- Unity의 JsonUtility 사용
- DTO 클래스에 `[Serializable]` 필수
- 배열 응답은 Wrapper 클래스 필요

### 4. Coroutine 패턴
- UnityWebRequest는 Coroutine으로 처리
- async/await 사용 시 UniTask 라이브러리 권장

---

## 📚 추가 API 정보 요청 방법

특정 API 연동 작업 시, 서버 개발자에게 다음과 같이 요청:

```
"[API명] 연동을 위한 상세 스펙 파일 생성해줘"
```

예시:
- "로그인 API 연동을 위한 상세 스펙 파일 생성해줘"
- "캐릭터 생성 API 연동을 위한 상세 스펙 파일 생성해줘"
- "스탯 분배 API 연동을 위한 상세 스펙 파일 생성해줘"

그러면 해당 API만의 상세한 Request/Response 예시와 Unity 구현 가이드를 받을 수 있습니다.

---

## 🔄 업데이트 이력
- **v1.0** (2025-10-02): 초기 문서 작성
- Week 1 완료 상태 (인증 + 캐릭터 시스템)
