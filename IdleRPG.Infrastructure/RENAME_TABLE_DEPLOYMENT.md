# CharacterDungeonProgresses → CharacterBattleProgresses 테이블 이름 변경 가이드

## 📋 개요

메인 전투 스테이지를 관리하는 테이블 이름을 `CharacterDungeonProgresses`에서 `CharacterBattleProgresses`로 변경합니다.

- **날짜**: 2025-11-06
- **영향도**: 기존 데이터 보존 (Zero Downtime)
- **Migration ID**: `20251106000000_RenameCharacterDungeonProgressesToBattleProgresses`

## 🎯 변경 내역

### 1. 테이블 이름 변경
- **기존**: `CharacterDungeonProgresses`
- **변경**: `CharacterBattleProgresses`

### 2. 인덱스 이름 변경
- **기존**: `IX_CharacterDungeonProgresses_CharacterId_Unique`
- **변경**: `IX_CharacterBattleProgresses_CharacterId_Unique`

### 3. Foreign Key Constraint 이름 변경
- **기존**: `FK_CharacterDungeonProgresses_Characters_CharacterId`
- **변경**: `FK_CharacterBattleProgresses_Characters_CharacterId`

## 🚀 배포 방법

### Option 1: 독립 SQL 파일 실행 (권장)

AWS EC2에서 직접 실행:

```bash
# 1. SQL 파일을 서버로 복사
scp IdleRPG.Infrastructure/rename_table_migration.sql ec2-user@your-server:/tmp/

# 2. SSH로 서버 접속
ssh ec2-user@your-server

# 3. PostgreSQL에서 마이그레이션 실행
sudo -u postgres psql -d idlerpggame -f /tmp/rename_table_migration.sql

# 4. 실행 완료 후 결과 확인
# - 테이블 이름이 변경되었는지 확인
# - 인덱스와 제약조건이 올바르게 변경되었는지 확인
```

### Option 2: 전체 migration.sql 재실행

```bash
# PostgreSQL에서 전체 migration.sql 실행
sudo -u postgres psql -d idlerpggame -f /path/to/migration.sql
```

**Note**: Idempotent 패턴으로 작성되어 있어 여러 번 실행해도 안전합니다.

## ✅ 검증 방법

### 1. 테이블 확인
```sql
-- CharacterBattleProgresses 테이블이 존재하는지 확인
SELECT table_name
FROM information_schema.tables
WHERE table_schema = 'public'
AND table_name = 'CharacterBattleProgresses';

-- 기존 테이블이 더 이상 존재하지 않는지 확인
SELECT table_name
FROM information_schema.tables
WHERE table_schema = 'public'
AND table_name = 'CharacterDungeonProgresses';
```

### 2. 데이터 보존 확인
```sql
-- 데이터가 손실되지 않았는지 확인
SELECT COUNT(*) FROM "CharacterBattleProgresses";

-- 샘플 데이터 확인
SELECT * FROM "CharacterBattleProgresses" LIMIT 5;
```

### 3. 인덱스 확인
```sql
-- 인덱스가 올바르게 변경되었는지 확인
SELECT indexname, tablename
FROM pg_indexes
WHERE tablename = 'CharacterBattleProgresses';
```

### 4. Foreign Key 확인
```sql
-- Foreign Key Constraint가 올바르게 변경되었는지 확인
SELECT constraint_name, table_name
FROM information_schema.table_constraints
WHERE table_name = 'CharacterBattleProgresses'
AND constraint_type = 'FOREIGN KEY';
```

### 5. 마이그레이션 히스토리 확인
```sql
-- 마이그레이션이 기록되었는지 확인
SELECT * FROM "__EFMigrationsHistory"
WHERE "MigrationId" = '20251106000000_RenameCharacterDungeonProgressesToBattleProgresses';
```

## 🔧 애플리케이션 재시작

마이그레이션 완료 후 애플리케이션을 재시작해야 합니다:

```bash
# Jenkins를 통한 재배포 또는
# Docker 컨테이너 재시작
sudo docker restart idlerpg-api

# 로그 확인
sudo docker logs -f idlerpg-api
```

## 🐛 문제 해결

### 문제 1: "relation CharacterBattleProgresses does not exist" 오류
**원인**: 마이그레이션이 아직 실행되지 않음
**해결**: 위의 배포 방법에 따라 마이그레이션 실행

### 문제 2: 마이그레이션 실행 중 오류
**원인**: 동시 접속이나 트랜잭션 충돌
**해결**:
```sql
-- 활성 연결 확인
SELECT * FROM pg_stat_activity WHERE datname = 'idlerpggame';

-- 필요시 애플리케이션 중지 후 재실행
```

### 문제 3: 데이터 손실 우려
**해결**: 마이그레이션 전 백업
```bash
# 데이터베이스 백업
sudo -u postgres pg_dump idlerpggame > /tmp/backup_before_rename_$(date +%Y%m%d_%H%M%S).sql
```

## 📊 롤백 방법 (필요시)

만약 문제가 발생하면 아래 SQL로 롤백할 수 있습니다:

```sql
BEGIN;

-- 테이블 이름 되돌리기
ALTER TABLE "CharacterBattleProgresses" RENAME TO "CharacterDungeonProgresses";

-- 인덱스 이름 되돌리기
ALTER INDEX "IX_CharacterBattleProgresses_CharacterId_Unique"
RENAME TO "IX_CharacterDungeonProgresses_CharacterId_Unique";

-- Foreign Key 이름 되돌리기
ALTER TABLE "CharacterDungeonProgresses"
RENAME CONSTRAINT "FK_CharacterBattleProgresses_Characters_CharacterId"
TO "FK_CharacterDungeonProgresses_Characters_CharacterId";

-- 마이그레이션 히스토리 제거
DELETE FROM "__EFMigrationsHistory"
WHERE "MigrationId" = '20251106000000_RenameCharacterDungeonProgressesToBattleProgresses';

COMMIT;
```

## 📝 체크리스트

배포 전:
- [ ] 데이터베이스 백업 완료
- [ ] 마이그레이션 SQL 파일 준비
- [ ] 배포 시간 확정 (사용자 적은 시간대 권장)

배포 중:
- [ ] 마이그레이션 SQL 실행
- [ ] 검증 쿼리로 결과 확인
- [ ] 애플리케이션 재시작

배포 후:
- [ ] API 정상 동작 확인
- [ ] 로그에 오류 없는지 확인
- [ ] 전투 시스템 기능 테스트
- [ ] 사용자 진행도 조회 기능 확인

## 🔗 관련 파일

- `IdleRPG.Infrastructure/migration.sql` - 전체 마이그레이션 파일
- `IdleRPG.Infrastructure/rename_table_migration.sql` - 독립 실행 가능한 마이그레이션 파일
- `IdleRPG.Domain/Entities/CharacterBattleProgress.cs` - 변경된 Entity
- `IdleRPG.Infrastructure/Data/Configurations/CharacterBattleProgressConfiguration.cs` - EF Core Configuration

## 📞 문의

문제 발생 시 개발팀에 즉시 연락하세요.
