# EF Core Migrations 팀 가이드

> 이 문서는 EF Core Migrations 도입 후 팀원 온보딩 및 문제 해결 가이드입니다.

---

## 📋 목차

1. [환경 설정](#환경-설정)
2. [EF Core CLI 사용법](#ef-core-cli-사용법)
3. [로컬 개발 워크플로우](#로컬-개발-워크플로우)
4. [문제 해결](#문제-해결)
5. [FAQ](#faq)

---

## 환경 설정

### 1. 프로젝트 클론 후 초기 설정

프로젝트를 클론하거나 최신 코드를 pull한 후, 다음 명령어를 실행하세요:

```bash
# 1. 프로젝트 디렉터리로 이동
cd D:\Proj\IdleGameServer

# 2. .NET 도구 복원 (EF Core CLI 자동 설치)
dotnet tool restore

# 3. EF Core CLI 버전 확인
dotnet ef --version
# 예상 출력: Entity Framework Core .NET Command-line Tools 9.0.0
```

**중요**: `dotnet tool restore` 명령어는 `.config/dotnet-tools.json` 파일을 읽고, 프로젝트에 필요한 모든 로컬 도구를 자동으로 설치합니다.

### 2. 왜 프로젝트 로컬 도구를 사용하나요?

**기존 방식 (Global 설치)**:
```bash
dotnet tool install dotnet-ef --global
```
- ❌ 팀원마다 다른 버전 사용 가능 (버전 불일치)
- ❌ 여러 프로젝트 사용 시 충돌 가능

**새로운 방식 (프로젝트 로컬 도구)**:
```bash
dotnet tool restore
```
- ✅ `.config/dotnet-tools.json`에 버전 명시 (9.0.0)
- ✅ 팀원 모두 동일한 버전 사용 (일관성)
- ✅ Git으로 버전 관리 (코드와 함께 추적)

---

## EF Core CLI 사용법

### 기본 명령어

#### 1. 마이그레이션 목록 조회
```bash
dotnet ef migrations list --project IdleRPG.Infrastructure
```

**출력 예시**:
```
20251111120000_InitialFromExistingDb (Applied)
20251112100000_RemoveTierColumn (Applied)
20251113150000_AddPetSystem (Pending)
```

#### 2. 마이그레이션 생성
```bash
# 새로운 마이그레이션 생성
dotnet ef migrations add <MigrationName> --project IdleRPG.Infrastructure

# 예시: Character 테이블에 Nickname 컬럼 추가
dotnet ef migrations add AddNicknameToCharacter --project IdleRPG.Infrastructure
```

**생성되는 파일**:
- `IdleRPG.Infrastructure/Migrations/{Timestamp}_AddNicknameToCharacter.cs` (Up/Down 메서드)
- `IdleRPG.Infrastructure/Migrations/{Timestamp}_AddNicknameToCharacter.Designer.cs` (메타데이터)
- `IdleRPG.Infrastructure/Migrations/GameDBContextModelSnapshot.cs` (업데이트됨)

#### 3. 마이그레이션 적용 (로컬 DB)
```bash
# 로컬 PostgreSQL에 마이그레이션 적용
dotnet ef database update --project IdleRPG.Infrastructure

# 특정 마이그레이션까지만 적용
dotnet ef database update 20251112100000_RemoveTierColumn --project IdleRPG.Infrastructure

# 모든 마이그레이션 롤백 (초기 상태로)
dotnet ef database update 0 --project IdleRPG.Infrastructure
```

#### 4. 마이그레이션 삭제 (실수로 생성한 경우)
```bash
# 최신 마이그레이션 삭제 (아직 적용 안 된 경우만 가능)
dotnet ef migrations remove --project IdleRPG.Infrastructure
```

⚠️ **주의**: 이미 DB에 적용된 마이그레이션은 삭제할 수 없습니다. 롤백 후 삭제하세요.

---

## 로컬 개발 워크플로우

### 시나리오 1: 새로운 컬럼 추가

**요구사항**: `Character` 테이블에 `Nickname` 컬럼 추가 (varchar(50), nullable)

#### 1단계: Entity 수정
```csharp
// IdleRPG.Domain/Entities/Character.cs
public class Character : BaseEntity
{
    // 기존 필드...
    public string? Nickname { get; set; }  // 새로 추가
}
```

#### 2단계: Entity Configuration 수정 (선택사항)
```csharp
// IdleRPG.Infrastructure/Configurations/CharacterConfiguration.cs
public void Configure(EntityTypeBuilder<Character> builder)
{
    // 기존 설정...

    builder.Property(c => c.Nickname)
        .HasMaxLength(50)
        .IsRequired(false);  // nullable
}
```

#### 3단계: 마이그레이션 생성
```bash
dotnet ef migrations add AddNicknameToCharacter --project IdleRPG.Infrastructure
```

#### 4단계: 생성된 마이그레이션 코드 확인
```csharp
// Migrations/{Timestamp}_AddNicknameToCharacter.cs
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.AddColumn<string>(
        name: "Nickname",
        table: "Character",
        type: "character varying(50)",
        maxLength: 50,
        nullable: true);
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DropColumn(
        name: "Nickname",
        table: "Character");
}
```

✅ **검증 포인트**:
- Up() 메서드가 컬럼을 추가하는가?
- Down() 메서드가 컬럼을 삭제하는가? (정확히 되돌림)
- `nullable: true`가 맞는가? (요구사항 확인)

#### 5단계: 로컬 DB 테스트
```bash
# 1. 로컬 PostgreSQL 실행
docker-compose up -d postgres

# 2. 마이그레이션 적용 (Up)
dotnet ef database update --project IdleRPG.Infrastructure

# 3. DB 확인 (psql 또는 pgAdmin)
psql -h localhost -U postgres -d idlerpg_dev -c "\d Character"
# Nickname 컬럼 존재 확인

# 4. 롤백 테스트 (Down)
dotnet ef database update {PreviousMigration} --project IdleRPG.Infrastructure

# 5. DB 확인 (Nickname 컬럼 삭제됨)
psql -h localhost -U postgres -d idlerpg_dev -c "\d Character"

# 6. 다시 최신으로 (Up)
dotnet ef database update --project IdleRPG.Infrastructure
```

#### 6단계: Git 커밋
```bash
git add IdleRPG.Domain/Entities/Character.cs
git add IdleRPG.Infrastructure/Configurations/CharacterConfiguration.cs
git add IdleRPG.Infrastructure/Migrations/
git commit -m "Add Nickname column to Character"
```

#### 7단계: Pull Request 생성
- GitHub에서 PR 생성
- 리뷰어가 `Migrations/` 폴더 변경 확인
- Up/Down 메서드 검증

#### 8단계: Jenkins 자동 배포
- PR Merge → Jenkins Webhook 트리거
- Jenkins가 자동으로 `dotnet ef database update` 실행
- RDS에 마이그레이션 적용

---

### 시나리오 2: 기존 코드 Pull 후 로컬 DB 동기화

**상황**: 다른 팀원이 마이그레이션을 추가하고 Merge했습니다. 내 로컬 DB를 최신 상태로 업데이트하려면?

```bash
# 1. 최신 코드 Pull
git pull origin master

# 2. .NET 도구 복원 (혹시 dotnet-tools.json이 변경되었을 경우)
dotnet tool restore

# 3. 마이그레이션 적용 (새로운 마이그레이션만 적용됨)
dotnet ef database update --project IdleRPG.Infrastructure

# 4. 확인
dotnet ef migrations list --project IdleRPG.Infrastructure
# (Applied) 표시 확인
```

---

## 문제 해결

### 문제 1: `dotnet ef` 명령어가 인식되지 않아요

**증상**:
```bash
$ dotnet ef --version
'ef'을(를) SDK 명령으로 실행할 수 없습니다.
```

**원인**: EF Core CLI가 설치되지 않음

**해결 방법**:
```bash
# 1. 프로젝트 루트 디렉터리로 이동
cd D:\Proj\IdleGameServer

# 2. .NET 도구 복원
dotnet tool restore

# 3. 재확인
dotnet ef --version
# 예상 출력: 9.0.0
```

**그래도 안 되면**:
```bash
# .config/dotnet-tools.json 파일 존재 확인
cat .config/dotnet-tools.json

# 출력 예상:
# {
#   "version": 1,
#   "isRoot": true,
#   "tools": {
#     "dotnet-ef": {
#       "version": "9.0.0",
#       "commands": ["dotnet-ef"]
#     }
#   }
# }
```

파일이 없으면 Git에서 최신 버전을 Pull하세요:
```bash
git pull origin master
```

---

### 문제 2: 마이그레이션 적용 시 에러 발생

**증상**:
```bash
$ dotnet ef database update --project IdleRPG.Infrastructure
Failed executing DbCommand (0ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
ALTER TABLE "Character" ADD COLUMN "Nickname" character varying(50) NOT NULL;
23502: column "Nickname" of relation "Character" contains null values
```

**원인**: NOT NULL 컬럼을 추가하려는데, 기존 레코드에 NULL 값이 들어갈 자리가 없음

**해결 방법 (2-Step Migration)**:

**Step 1**: Nullable 컬럼으로 추가
```bash
dotnet ef migrations add AddNicknameToCharacter_Step1 --project IdleRPG.Infrastructure
```

수동 수정 (`Up()` 메서드):
```csharp
migrationBuilder.AddColumn<string>(
    name: "Nickname",
    table: "Character",
    type: "character varying(50)",
    maxLength: 50,
    nullable: true);  // 일단 nullable로 추가
```

**Step 2**: 기본값 설정 후 NOT NULL로 변경
```bash
dotnet ef migrations add AddNicknameToCharacter_Step2 --project IdleRPG.Infrastructure
```

수동 수정 (`Up()` 메서드):
```csharp
// 1. 기존 레코드에 기본값 설정
migrationBuilder.Sql(@"
    UPDATE ""Character""
    SET ""Nickname"" = ""Name""
    WHERE ""Nickname"" IS NULL;
");

// 2. NOT NULL 제약조건 추가
migrationBuilder.AlterColumn<string>(
    name: "Nickname",
    table: "Character",
    type: "character varying(50)",
    maxLength: 50,
    nullable: false,  // NOT NULL로 변경
    oldClrType: typeof(string),
    oldType: "character varying(50)",
    oldMaxLength: 50,
    oldNullable: true);
```

---

### 문제 3: 로컬 PostgreSQL이 실행 안 돼요

**증상**:
```bash
$ dotnet ef database update --project IdleRPG.Infrastructure
Npgsql.NpgsqlException: Failed to connect to localhost:5432
```

**원인**: Docker PostgreSQL 컨테이너가 실행 중이지 않음

**해결 방법**:
```bash
# 1. Docker 컨테이너 상태 확인
docker-compose ps

# 2. PostgreSQL 실행
docker-compose up -d postgres

# 3. 실행 확인
docker-compose ps postgres
# State: Up 확인

# 4. 재시도
dotnet ef database update --project IdleRPG.Infrastructure
```

---

### 문제 4: 마이그레이션 파일을 삭제하고 싶어요

**시나리오 A**: 아직 DB에 적용 안 함 (Pending 상태)
```bash
# 마이그레이션 삭제 (마지막 마이그레이션만 가능)
dotnet ef migrations remove --project IdleRPG.Infrastructure
```

**시나리오 B**: 이미 DB에 적용됨 (Applied 상태)
```bash
# 1. 이전 마이그레이션으로 롤백
dotnet ef database update {PreviousMigration} --project IdleRPG.Infrastructure

# 예시
dotnet ef database update 20251112100000_RemoveTierColumn --project IdleRPG.Infrastructure

# 2. 마이그레이션 삭제
dotnet ef migrations remove --project IdleRPG.Infrastructure

# 3. 파일 수동 삭제 (필요 시)
rm IdleRPG.Infrastructure/Migrations/20251113150000_AddPetSystem.*
```

⚠️ **주의**: RDS(운영 DB)에 이미 적용된 마이그레이션은 함부로 삭제하지 마세요! Forward Fix를 권장합니다.

---

## FAQ

### Q1. 마이그레이션을 수동으로 작성해야 하나요?

**A**: 대부분의 경우 자동 생성됩니다. 하지만 다음 경우에는 수동 수정이 필요합니다:
- 데이터 이관 필요 (예: 컬럼 분리/병합)
- 복잡한 스키마 변경 (예: 테이블 이름 변경)
- 커스텀 SQL 필요 (예: 인덱스, 트리거)

**예시 (데이터 이관)**:
```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // 1. 새 컬럼 추가
    migrationBuilder.AddColumn<string>(
        name: "FullName",
        table: "Character",
        nullable: true);

    // 2. 데이터 이관 (FirstName + LastName → FullName)
    migrationBuilder.Sql(@"
        UPDATE ""Character""
        SET ""FullName"" = ""FirstName"" || ' ' || ""LastName"";
    ");

    // 3. 기존 컬럼 삭제
    migrationBuilder.DropColumn(name: "FirstName", table: "Character");
    migrationBuilder.DropColumn(name: "LastName", table: "Character");
}
```

---

### Q2. 레거시 `migration.sql` 파일은 어떻게 되나요?

**A**: 현재는 **하이브리드 방식**으로 병행 실행됩니다:
1. Jenkins가 `migration.sql` 실행 (레거시 안전망)
2. Jenkins가 `dotnet ef database update` 실행 (신규 마이그레이션)

**향후 계획** (2-3주 모니터링 후):
- `migration.sql` 제거 (EF Core Migrations만 사용)
- `docs/migrations/legacy/` 폴더로 이동 (보관)

---

### Q3. RDS(운영 DB)에 마이그레이션이 언제 적용되나요?

**A**: Jenkins 자동 배포 시 적용됩니다:
1. Git Push (master 브랜치)
2. Jenkins Webhook 트리거
3. Jenkins "Database Migration" 스테이지 실행
   - `dotnet tool restore`
   - `psql -f migration.sql` (레거시)
   - `dotnet ef database update` (신규)
   - 검증 쿼리
4. 성공 시 Docker 빌드 + 배포

**수동 롤백** (긴급 상황):
```bash
# EC2 SSH 접속
ssh ec2-user@13.209.66.253

# 이전 마이그레이션으로 롤백
cd /home/ec2-user/IdleRPGServer
dotnet ef database update {PreviousMigration} \
    --project IdleRPG.Infrastructure \
    --connection "Host=idlerpg-dev.chiqweeeuidv.ap-northeast-2.rds.amazonaws.com;Port=5432;Database=idlerpg;Username=postgres;Password=$PGPASSWORD;SSL Mode=Require;Trust Server Certificate=true"
```

---

### Q4. `__EFMigrationsHistory` 테이블은 뭔가요?

**A**: EF Core가 마이그레이션 이력을 추적하는 시스템 테이블입니다.

**구조**:
- `MigrationId` (PK): 마이그레이션 고유 ID (예: `20251111120000_InitialFromExistingDb`)
- `ProductVersion`: EF Core 버전 (예: `9.0.0`)

**조회 방법**:
```sql
SELECT * FROM "__EFMigrationsHistory" ORDER BY "MigrationId";
```

**예상 결과**:
```
MigrationId                            | ProductVersion
---------------------------------------+---------------
20240101000000_LegacyRecord1           | (null)
...
20241110000000_LegacyRecord10          | (null)
20251111120000_InitialFromExistingDb   | 9.0.0
20251112100000_RemoveTierColumn        | 9.0.0
```

**중요**: 이 테이블을 수동으로 수정하지 마세요! EF Core가 자동 관리합니다.

---

### Q5. 로컬 환경과 운영 환경의 연결 문자열이 다른가요?

**A**: 네, 환경별로 다릅니다:

**로컬 (appsettings.Development.json)**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=idlerpg_dev;Username=postgres;Password=password123"
  }
}
```

**운영 (Jenkins, RDS)**:
```
Host=idlerpg-dev.chiqweeeuidv.ap-northeast-2.rds.amazonaws.com;Port=5432;Database=idlerpg;Username=postgres;Password=$PGPASSWORD;SSL Mode=Require;Trust Server Certificate=true
```

**중요**: 로컬에서 `dotnet ef database update`는 자동으로 `appsettings.Development.json`을 읽습니다. RDS에 직접 적용하려면 `--connection` 옵션이 필요합니다.

---

## 참고 자료

### 공식 문서
- [EF Core Migrations 공식 가이드](https://learn.microsoft.com/ef-core/managing-schemas/migrations/)
- [EF Core CLI 레퍼런스](https://learn.microsoft.com/ef-core/cli/dotnet)
- [.NET 프로젝트 로컬 도구](https://learn.microsoft.com/dotnet/core/tools/local-tools-how-to-use)

### 프로젝트 내부 문서
- `docs/jenkins/DEPLOYMENT_GUIDE.md` - Jenkins 배포 가이드
- `docs/jenkins/ROLLBACK_GUIDE.md` - 롤백 가이드 (작성 예정)
- `.claude/memories/specs/ef-core-migrations/design.md` - 설계 문서

### 기타
- [ADR-0009: EF Migrations Hybrid Approach](../../adr/ADR-0009-ef-migrations-hybrid.md)

---

## 지원

문제 발생 시:
1. 이 가이드의 [문제 해결](#문제-해결) 섹션 확인
2. 팀 채널에 질문 (Slack/Discord)
3. GitHub Issue 생성

---

**작성일**: 2025-11-11
**버전**: v1.0
**작성자**: Development Team
**최종 업데이트**: 2025-11-11
