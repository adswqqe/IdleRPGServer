# 작업 완료 시 체크리스트

## 코드 작성 후 필수 단계

### 1. 빌드 확인
```bash
# 전체 솔루션 빌드 확인
dotnet build IdleRPGServer.sln

# 빌드 에러가 있다면 개별 프로젝트 확인
dotnet build IdleRPG.API/IdleRPG.API.csproj
dotnet build IdleRPG.Application/IdleRPG.Application.csproj
dotnet build IdleRPG.Infrastructure/IdleRPG.Infrastructure.csproj
dotnet build IdleRPG.Domain/IdleRPG.Domain.csproj
```

### 2. 데이터베이스 마이그레이션 (스키마 변경 시)
```bash
cd IdleRPG.Infrastructure

# 새 마이그레이션 생성
dotnet ef migrations add <마이그레이션명> --startup-project ../IdleRPG.API

# 데이터베이스 적용
dotnet ef database update --startup-project ../IdleRPG.API
```

### 3. 애플리케이션 실행 테스트
```bash
cd IdleRPG.API
dotnet run

# 확인 사항:
# - 서버 정상 시작 여부
# - http://localhost:5172/swagger 접속 가능 여부
# - API 엔드포인트 정상 작동 여부
```

### 4. 코드 품질 확인
- **컴파일 경고**: 0개 유지
- **Nullable 참조**: 적절한 null 체크
- **Using 구문**: 불필요한 using 제거
- **비동기 패턴**: async/await 올바른 사용

### 5. 보안 검토
- **비밀번호**: 평문 저장하지 않았는지 확인
- **로그**: 민감 정보 로그 출력하지 않았는지 확인
- **SQL Injection**: EF Core 쿼리 안전성 확인
- **JWT 토큰**: 적절한 만료 시간 설정

### 6. API 테스트 (Swagger 또는 수동)
- **새 엔드포인트**: 요청/응답 정상 확인
- **인증 필요 API**: JWT 토큰 검증 확인
- **에러 핸들링**: 예외 상황 적절한 응답 확인

## 현재 사용할 수 없는 도구들 (추후 설정 예정)

### 테스트 (테스트 프로젝트 생성 후)
```bash
dotnet test
```

### 코드 포맷팅 (설정 파일 추가 후)
```bash
dotnet format
```

### 정적 분석 도구
- **SonarAnalyzer** (추후 추가 예정)
- **StyleCop** (추후 추가 예정)

## Git 커밋 전 체크사항
- [ ] 빌드 성공
- [ ] 마이그레이션 적용 완료 (필요시)
- [ ] 애플리케이션 정상 실행
- [ ] Swagger UI 정상 접근
- [ ] 새 기능 API 테스트 완료
- [ ] 로그에 민감 정보 없음 확인