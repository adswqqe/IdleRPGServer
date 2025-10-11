# 🔄 AWS에서 Azure로 마이그레이션 가이드

**작성일**: 2025-10-11
**예상 소요 시간**: 1-2시간
**난이도**: ⭐⭐ (쉬움)

---

## 🎯 왜 Azure로 전환하나?

### .NET 개발자를 위한 최적의 선택

**AWS EC2의 문제:**
- ❌ 복잡한 설정 (Jenkins, Docker, Linux)
- ❌ 수동 관리 필요 (메모리, 보안)
- ❌ .NET과의 괴리

**Azure App Service의 장점:**
- ✅ 5분 설정 (Visual Studio에서 클릭)
- ✅ 관리 제로 (자동 스케일링, 자동 업데이트)
- ✅ .NET 네이티브 지원

---

## 📋 전환 체크리스트

### Phase 1: Azure 준비 (10분)

- [ ] Azure 계정 생성
  - https://azure.microsoft.com/free/
  - 학생이면 https://azure.microsoft.com/free/students/

- [ ] Azure CLI 설치 (선택)
  ```powershell
  winget install Microsoft.AzureCLI
  ```

- [ ] Visual Studio Azure 로그인
  - Tools → Options → Azure Service Authentication

---

### Phase 2: Azure 리소스 생성 (15분)

#### 2-1. Resource Group 생성
```
이름: idlerpg-rg
지역: Korea Central
```

#### 2-2. Azure Database for PostgreSQL 생성
```
서버 이름: idlerpg-db
관리자: postgres
비밀번호: [강력한 비밀번호]
버전: PostgreSQL 15
컴퓨팅: Burstable B1ms (저렴)
스토리지: 32GB
지역: Korea Central
```

**연결 문자열:**
```
Host=idlerpg-db.postgres.database.azure.com;Database=idlerpg;Username=postgres;Password=YOUR_PASSWORD;Port=5432;SSL Mode=Require
```

#### 2-3. Azure Cache for Redis 생성 (선택)
```
이름: idlerpg-redis
캐시 유형: Basic C0 (250MB)
지역: Korea Central
```

#### 2-4. Azure App Service Plan 생성
```
이름: idlerpg-plan
OS: Linux
지역: Korea Central
가격: B1 (Basic) - $13/월
  - 1.75GB RAM
  - 100 total ACU
  - 10GB 스토리지
```

#### 2-5. Azure App Service (Web App) 생성
```
이름: idlerpg-api (전역 고유해야 함)
게시: Code
런타임 스택: .NET 8 (LTS)
OS: Linux
지역: Korea Central
App Service Plan: idlerpg-plan
```

---

### Phase 3: 프로젝트 설정 변경 (20분)

#### 3-1. appsettings.json 확인
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "USE_AZURE_PORTAL_CONFIGURATION"
  },
  "Jwt": {
    "SecretKey": "USE_AZURE_PORTAL_CONFIGURATION"
  }
}
```

#### 3-2. Azure App Service 환경 변수 설정

Azure Portal → App Service → Configuration → Application settings:

```
ConnectionStrings__DefaultConnection = Host=idlerpg-db.postgres.database.azure.com;...
Jwt__SecretKey = [64자리 랜덤 키]
ASPNETCORE_ENVIRONMENT = Production
```

---

### Phase 4: 배포 (10분)

#### 옵션 A: Visual Studio에서 직접 배포 (가장 쉬움) ⭐

1. Solution Explorer → IdleRPG.API 프로젝트 우클릭
2. **Publish** 클릭
3. **Azure** 선택 → **Next**
4. **Azure App Service (Linux)** 선택 → **Next**
5. 위에서 만든 `idlerpg-api` 선택
6. **Finish** → **Publish** 클릭
7. 자동 배포! (1-2분)

**배포 완료 URL:**
```
https://idlerpg-api.azurewebsites.net
```

#### 옵션 B: GitHub Actions (자동 배포)

Azure Portal → App Service → Deployment Center:

1. **Source**: GitHub
2. **Organization**: 본인 계정
3. **Repository**: IdleRPGServer
4. **Branch**: master
5. **Save**

자동으로 `.github/workflows/azure-webapps-dotnet.yml` 생성!

**이제 git push만 하면 자동 배포!**

#### 옵션 C: Azure CLI

```bash
# 로그인
az login

# 배포
az webapp up --name idlerpg-api --resource-group idlerpg-rg --runtime "DOTNETCORE:8.0"
```

---

### Phase 5: 데이터베이스 마이그레이션 (15분)

#### 5-1. AWS RDS에서 데이터 백업
```bash
# AWS EC2에서
pg_dump -h idlerpg-dev.chiqweeeuidv.ap-northeast-2.rds.amazonaws.com \
        -U postgres -d idlerpg -F c -f idlerpg_backup.dump
```

#### 5-2. Azure Database로 복원
```bash
# 로컬에서
pg_restore -h idlerpg-db.postgres.database.azure.com \
           -U postgres -d idlerpg -F c idlerpg_backup.dump
```

#### 5-3. 또는 EF Core 마이그레이션 재실행
```bash
cd IdleRPG.Infrastructure
dotnet ef database update --startup-project ../IdleRPG.API --connection "Host=idlerpg-db.postgres.database.azure.com;..."
```

---

### Phase 6: 테스트 및 확인 (10분)

#### 6-1. API 테스트
```bash
# Swagger 확인
https://idlerpg-api.azurewebsites.net/swagger

# Health check
curl https://idlerpg-api.azurewebsites.net/health
```

#### 6-2. 로그 확인

Azure Portal → App Service → Log stream

실시간 로그를 볼 수 있습니다!

---

## 📊 비용 예측

### Azure 구성 (월간)

| 서비스 | 플랜 | 비용 |
|--------|------|------|
| App Service Plan | B1 | $13 |
| PostgreSQL | Burstable B1ms | $18 |
| Redis (선택) | Basic C0 | $15 |
| **총합** | | **$31-46** |

### 무료 크레딧 적용 시

- 학생: $100/월 크레딧 → **1년 무료!**
- 일반: 30일 $200 크레딧 → **처음 6개월 무료**

---

## ✅ 전환 완료 후 확인사항

- [ ] Swagger UI 접근 가능
- [ ] Database 연결 확인
- [ ] Player 등록/로그인 테스트
- [ ] GitHub push 시 자동 배포 확인
- [ ] 로그 스트리밍 작동 확인

---

## 🎯 AWS 리소스 정리 (비용 절감)

전환 완료 후:

1. **EC2 인스턴스 중지** (재시작 가능)
   ```
   EC2 → Instances → Stop
   ```

2. **또는 완전 삭제** (1주일 후 확인 후)
   ```
   EC2 → Instances → Terminate
   RDS → Database → Delete (백업 생성)
   ```

---

## 💡 Azure의 추가 장점

### 1. Application Insights (무료 기본 제공)
```csharp
// Program.cs에 추가만 하면 끝!
builder.Services.AddApplicationInsightsTelemetry();
```

**자동으로 제공:**
- 요청 추적
- 성능 모니터링
- 에러 로깅
- 대시보드

### 2. Easy Auth (간편 인증)

OAuth, Azure AD 통합이 클릭 몇 번!

### 3. Deployment Slots

무중단 배포:
```
Production Slot: 실제 서비스
Staging Slot: 테스트 서버
→ Swap으로 즉시 전환!
```

### 4. Auto-scaling

트래픽 증가 시 자동으로 인스턴스 추가!

---

## 🔐 보안 개선

### Azure Key Vault 사용 (추천)

```csharp
// Program.cs
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://idlerpg-vault.vault.azure.net/"),
    new DefaultAzureCredential()
);
```

**장점:**
- 시크릿이 코드/환경변수에 없음
- 자동 rotation
- 액세스 로그
- 무료 (10,000개 secret)

---

## 📚 참고 자료

- [Azure App Service 문서](https://learn.microsoft.com/azure/app-service/)
- [ASP.NET Core on Azure](https://learn.microsoft.com/aspnet/core/host-and-deploy/azure-apps/)
- [Azure Database for PostgreSQL](https://learn.microsoft.com/azure/postgresql/)

---

## 🆘 문제 해결

### 배포 실패 시

1. **로그 확인**: Azure Portal → Log stream
2. **환경 변수 확인**: Configuration → Application settings
3. **연결 문자열 확인**: PostgreSQL SSL Mode 필수!

### DB 연결 실패 시

```
SSL Mode=Require  ← 반드시 필요!
```

---

**마지막 업데이트**: 2025-10-11
**예상 완료 시간**: 1-2시간
**어려움**: ⭐⭐ (쉬움)
