# EC2 HTTPS 설정 완료 (2025-01-10)

## 문제 상황
Unity 클라이언트가 기본적으로 HTTP 요청을 차단하여 EC2 서버(http://13.125.206.100:5172)에 접속 불가

**Unity HTTP 차단 이유:**
- iOS: App Transport Security (ATS)로 HTTPS 강제
- Android 9+: Cleartext Traffic 기본 차단
- Unity Player Settings: HTTP 다운로드 기본 차단

## 해결 과정

### 1. 문제 진단
**EC2 로그 확인 결과:**
```
Now listening on: http://[::]:5172
```
→ HTTPS 포트(7122)가 리스닝되지 않음

**원인:**
- `docker-compose.prod.yml`의 `ASPNETCORE_URLS`에 HTTPS URL이 없음
- Docker 포트 매핑(`7122:7122`)만으로는 부족
- 애플리케이션이 실제로 해당 포트를 리스닝해야 함

### 2. docker-compose.prod.yml 수정

**변경 전:**
```yaml
environment:
  ASPNETCORE_URLS: "http://+:5172"
```

**변경 후:**
```yaml
environment:
  ASPNETCORE_URLS: "https://+:7122;http://+:5172"
  ASPNETCORE_Kestrel__Certificates__Default__Password: "IdleRPG2025!"
  ASPNETCORE_Kestrel__Certificates__Default__Path: "/https/aspnetapp.pfx"
volumes:
  - ~/.aspnet/https:/https:ro
```

### 3. EC2에서 자체 서명 인증서 생성

```bash
# 1. 디렉토리 생성
mkdir -p ~/.aspnet/https
cd ~/.aspnet/https

# 2. 자체 서명 인증서 생성 (365일 유효)
openssl req -x509 -newkey rsa:4096 -sha256 -days 365 -nodes \
  -keyout aspnetapp.key -out aspnetapp.crt \
  -subj "/CN=13.125.206.100/O=IdleRPG/C=KR" \
  -addext "subjectAltName=IP:13.125.206.100,DNS:localhost"

# 3. PFX 형식으로 변환
openssl pkcs12 -export -out aspnetapp.pfx \
  -inkey aspnetapp.key -in aspnetapp.crt \
  -password pass:IdleRPG2025!

# 4. 권한 설정
chmod 600 aspnetapp.pfx

# 5. 배포
cd ~/IdleRPGServer
git pull origin master
docker-compose -f docker-compose.prod.yml down
docker-compose -f docker-compose.prod.yml up -d
```

### 4. 배포 결과

**성공 로그:**
```
Now listening on: https://[::]:7122
Now listening on: http://[::]:5172
```

**접속 URL:**
- HTTP: http://13.125.206.100:5172
- HTTPS: https://13.125.206.100:7122 ✅

## Unity 클라이언트 설정

### 자체 서명 인증서 허용 코드

```csharp
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

public class APIManager : MonoBehaviour
{
    void Awake()
    {
        // 개발 환경에서만 사용
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        ServicePointManager.ServerCertificateValidationCallback = 
            TrustAllCertificates;
        #endif
    }

    private static bool TrustAllCertificates(
        object sender, 
        X509Certificate certificate, 
        X509Chain chain, 
        SslPolicyErrors sslPolicyErrors)
    {
        return true; // 모든 인증서 허용 (개발용만!)
    }
}
```

### Unity API 사용

```csharp
// HTTPS 사용
string apiBaseUrl = "https://13.125.206.100:7122";

// 예시
string loginUrl = $"{apiBaseUrl}/api/auth/login";
```

## 주요 학습 내용

### ASP.NET Core HTTPS 설정

1. **ASPNETCORE_URLS**: 애플리케이션이 리스닝할 URL 지정
   - `http://+:5172`: HTTP 모든 IP에서 5172 포트
   - `https://+:7122`: HTTPS 모든 IP에서 7122 포트

2. **Kestrel 인증서 설정**:
   - `Certificates__Default__Path`: PFX 파일 경로
   - `Certificates__Default__Password`: PFX 비밀번호

3. **Docker 볼륨 마운트**: 호스트의 인증서를 컨테이너에 전달
   - `~/.aspnet/https:/https:ro` (읽기 전용)

### 자체 서명 인증서 vs 공식 인증서

**자체 서명 인증서 (현재 사용):**
- ✅ 무료, 즉시 생성 가능
- ✅ 개발/테스트 환경에 적합
- ❌ 브라우저 경고 표시
- ❌ Unity에서 인증서 검증 우회 필요
- ❌ 앱스토어 심사에서 문제 가능

**Let's Encrypt (프로덕션 권장):**
- ✅ 무료 공식 인증서
- ✅ 자동 갱신 (90일)
- ✅ 브라우저 신뢰
- ❌ 도메인 필요 (IP만으로는 발급 불가)
- ❌ 설정이 복잡

### Unity 보안 고려사항

⚠️ **개발 환경에서만 사용:**
```csharp
#if UNITY_EDITOR || DEVELOPMENT_BUILD
ServicePointManager.ServerCertificateValidationCallback = TrustAllCertificates;
#endif
```

⚠️ **프로덕션 빌드에서는:**
- 조건부 컴파일로 인증서 우회 코드 제거됨
- Let's Encrypt 등 공식 인증서 필수

## 다음 단계 제안

### 단기 (개발 중)
- ✅ 자체 서명 인증서로 HTTPS 사용
- Unity 클라이언트에서 HTTPS API 테스트
- 인증서 만료일(365일) 모니터링

### 장기 (프로덕션 출시 시)
1. **도메인 구매**: idlerpg.com 등
2. **Route 53 설정**: 도메인 → EC2 IP 연결
3. **Let's Encrypt 설치**: Certbot으로 자동화
4. **Nginx 리버스 프록시**: 
   - Nginx가 HTTPS 처리
   - API 서버는 HTTP로 내부 통신
5. **Unity 코드 정리**: 인증서 우회 코드 제거

### 대안: AWS ALB 사용
- Application Load Balancer에서 HTTPS 처리
- ACM(AWS Certificate Manager)으로 무료 인증서
- 백엔드는 HTTP 유지
- 비용: ~$16/월

## 중요 파일

- `docker-compose.prod.yml`: HTTPS 설정 포함
- `~/.aspnet/https/aspnetapp.pfx`: EC2 인증서 (Git 제외)
- Unity API Manager: 인증서 검증 우회 코드

## 보안 주의사항

⚠️ `docker-compose.prod.yml`에 평문 비밀번호 포함:
- RDS 비밀번호: `xocjs1547`
- 인증서 비밀번호: `IdleRPG2025!`

**권장 개선:**
- AWS Secrets Manager 사용
- 또는 `.env` 파일로 분리 후 `.gitignore` 추가

⚠️ 자체 서명 인증서는 365일 후 만료:
- 만료일: 2026-01-09
- 갱신 명령어: 위의 openssl 명령어 재실행

## Git 커밋 이력

```
b4ef49c feat: Enable HTTPS port 7122 for Unity client compatibility
```

**변경 파일:**
- `docker-compose.prod.yml`: HTTPS 설정 추가

## 테스트 방법

### 브라우저 테스트
```bash
# HTTP
curl http://13.125.206.100:5172/swagger/index.html

# HTTPS (자체 서명 인증서 경고 무시)
curl -k https://13.125.206.100:7122/swagger/index.html
```

### Unity 테스트
```csharp
// UnityWebRequest 또는 HttpClient 사용
UnityWebRequest.Get("https://13.125.206.100:7122/api/auth/test");
```

---

**작업 완료일**: 2025-01-10  
**EC2 IP**: 13.125.206.100  
**HTTPS 포트**: 7122  
**인증서 유효기간**: 365일 (2026-01-09까지)
