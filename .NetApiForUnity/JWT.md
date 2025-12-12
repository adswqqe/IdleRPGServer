# JWT

## 🎯 개념 이해
JSON Web Token - 서버와 클라이언트 간 안전한 인증을 위한 토큰 기반 시스템

## 🔗 연결 관계
- [[Week 2 - Authentication과 Authorization]]
- [[Player Entity]]
- [[Unity Login Integration]]
- [[API Security]]

## 💻 핵심 구조
```
Header.Payload.Signature
```

### 구성 요소
1. **Header**: 토큰 타입 및 암호화 알고리즘
2. **Payload**: 사용자 정보 (Claims)  
3. **Signature**: 토큰 무결성 검증

## 🎮 Unity 통합
```csharp
// Unity에서 토큰 저장 및 사용
PlayerPrefs.SetString("AuthToken", jwtToken);

// API 요청 시 헤더에 토큰 포함
request.SetRequestHeader("Authorization", $"Bearer {token}");
```

## ✅ 장점
- **Stateless**: 서버에서 세션 관리 불필요
- **Scalable**: 분산 서버 환경에 적합
- **Mobile Friendly**: Unity 모바일 게임에 최적

## ⚠️ 보안 고려사항
- **만료 시간** 설정 (보통 1시간-24시간)
- **Refresh Token** 함께 사용
- **HTTPS** 필수 사용

## 📚 관련 학습
- [[Role-based Access Control]]
- [[API Security]]
- [[Unity OAuth Integration]]

---

#JWT #Authentication #Security #토큰인증