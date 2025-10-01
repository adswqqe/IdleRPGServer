# 프로젝트 클린 재시작 진행 상황

## 현재 상태 (2025-10-01)

### ✅ 완료된 작업
1. GDD 작성 완료 (`.taskmaster/docs/prd.txt`)
   - 서버 + 클라이언트 통합 문서
   - Week 0-10 로드맵 포함
   - API 계약 명세 포함

2. 인프라 타이밍 가이드 설정 완료
   - Week 0 인프라를 필요할 때 추가하도록 설정
   - 자동 제안 시스템 구축

3. 파일 제거 완료
   - Character, ItemTemplate, OfflineReward, PlayerInventory, PlayerStats 엔티티 제거
   - PlayerController, PlayerRepository 제거
   - Application/Players 폴더 제거

4. 핵심 파일 정리 완료
   - Player.cs → 인증 전용으로 간소화
   - GameDBContext.cs → Players, RefreshTokens만 유지
   - AuthResponseDto.cs → PlayerDto 제거

### ⚠️  진행 중 (빌드 에러 해결 필요)

**문제:** `AuthService.cs`의 `GenerateAccessToken` 메서드 시그니처 불일치
- 에러: `인수 2개를 사용하는 'GenerateAccessToken' 메서드에 대한 오버로드가 없습니다`
- 위치: Line 40, 72, 103

**해결 방법:**
1. `IJwtTokenService` 인터페이스 확인
2. `GenerateAccessToken`의 정확한 시그니처 파악
3. `AuthService`에서 올바른 파라미터로 호출

### 📝 다음 단계

1. **빌드 에러 수정**
   ```bash
   # IJwtTokenService 인터페이스 확인
   cat IdleRPG.Application/Tokens/Services/IJwtTokenService.cs
   
   # 시그니처에 맞게 AuthService 수정
   ```

2. **데이터베이스 초기화**
   ```bash
   cd IdleRPG.Infrastructure
   dotnet ef database drop --startup-project ../IdleRPG.API --force
   dotnet ef migrations add Initial --startup-project ../IdleRPG.API
   dotnet ef database update --startup-project ../IdleRPG.API
   ```

3. **빌드 확인**
   ```bash
   dotnet build IdleRPGServer.sln
   ```

4. **Task Master PRD 파싱**
   ```bash
   task-master parse-prd .taskmaster/docs/prd.txt
   ```

5. **Week 1 시작: 캐릭터 시스템**

### 🗂️ 현재 프로젝트 구조

**유지된 파일들 (인증 시스템):**
```
IdleRPG.Domain/
├── Entities/
│   ├── Player.cs (간소화)
│   └── RefreshToken.cs

IdleRPG.Application/
├── Auth/Services/
│   └── IAuthService.cs
├── DTOs/Auth/
│   ├── RegisterDto.cs
│   ├── LoginDto.cs
│   ├── RefreshTokenDto.cs
│   └── AuthResponseDto.cs (수정)
└── Tokens/Services/
    └── IJwtTokenService.cs

IdleRPG.Infrastructure/
├── Data/
│   └── GameDBContext.cs (간소화)
├── Service/
│   ├── AuthService.cs (수정 필요)
│   └── JwtTokenService.cs
└── Migrations/ (비어있음 - 재생성 필요)

IdleRPG.API/
└── Controllers/
    └── AuthController.cs
```

### 💡 빠른 복구 가이드

다음 세션 시작 시:
```bash
# 1. IJwtTokenService 확인
cat IdleRPG.Application/Tokens/Services/IJwtTokenService.cs

# 2. JwtTokenService 구현 확인
cat IdleRPG.Infrastructure/Service/JwtTokenService.cs

# 3. AuthService에서 올바른 시그니처로 호출
# (userId만 필요한지, userName도 필요한지 확인)

# 4. 빌드 성공 후 DB 초기화
dotnet ef migrations add Initial --startup-project ../IdleRPG.API
dotnet ef database update --startup-project ../IdleRPG.API
```

## 참고

- GDD 위치: `.taskmaster/docs/prd.txt`
- 인프라 가이드: `week0_infrastructure_timing_guide` 메모리
- 모든 게임 로직 코드는 제거됨
- 인증 시스템만 남아있음
- Week 1부터 깨끗하게 시작 가능
