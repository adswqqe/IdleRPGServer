# 🏢 회사에서 Rider로 RDS 접속 가이드

**작성일**: 2025-10-11
**소요 시간**: 5분
**난이도**: ⭐ (매우 쉬움)

---

## 📋 준비물

### 1. 파일 확인
회사 PC의 프로젝트 폴더에 다음 파일이 있는지 확인:
```
E:\StudyGameProj\IdleRPGServer\idlerpg-key.pem
```

**없으면**: USB나 GitHub private repo로 전송 (절대 public repo에 올리지 말 것!)

### 2. PEM 파일 권한 설정 (Windows)
```powershell
# PowerShell 관리자 권한으로
cd E:\StudyGameProj\IdleRPGServer
icacls idlerpg-key.pem /inheritance:r
icacls idlerpg-key.pem /grant:r "$($env:USERNAME):(R)"
```

---

## 🔧 Rider에서 RDS 연결 설정

### Step 1: Database 창 열기
```
View → Tool Windows → Database
또는 Alt+1
```

### Step 2: Data Source 추가
```
왼쪽 Database 탭에서:
  + 버튼 클릭
  → Data Source
  → PostgreSQL
```

### Step 3: 드라이버 다운로드 (최초 1회)
```
하단에 "Download missing driver files" 버튼이 보이면 클릭
→ 다운로드 완료 대기
```

### Step 4: General 탭 설정

**Name (상단)**:
```
Production RDS
```

**Connection 섹션**:
```
Host: idlerpg-dev.chiqweeeuidv.ap-northeast-2.rds.amazonaws.com
Port: 5432
Authentication: User & Password
User: postgres
Password: AZ9ifIytNg5aRcVkxT7nYF0Ob13oS6hjQ4PGXdJ8DKCzqLUmBvreMw2EplWsHu
Database: idlerpg
```

**옵션**:
- ✓ Save password (체크)

### Step 5: SSH/SSL 탭 설정

**SSH 섹션**:
```
✓ Use SSH tunnel (체크!)

Host: 13.209.66.253
Port: 22
User name: ec2-user
Auth type: Key pair (OpenSSH or PuTTY)
Private key file: E:\StudyGameProj\IdleRPGServer\idlerpg-key.pem
```

**Passphrase**: (비워둠)

**SSL 섹션**: (건드리지 않음)

### Step 6: 연결 테스트
```
하단 "Test Connection" 버튼 클릭
→ "Succeeded" 메시지 확인
→ "OK" 버튼 클릭
```

---

## ✅ 연결 성공 확인

### Database 탭에서 확인할 내용:
```
Production RDS
  └─ idlerpg@idlerpg-dev...
      ├─ schemas
      │   └─ public
      │       ├─ tables
      │       │   ├─ Players
      │       │   ├─ Characters
      │       │   └─ ...
      │       └─ ...
      └─ ...
```

### 테이블 확인:
```
public → tables → Players 우클릭 → Jump to Console (F4)

쿼리 입력:
SELECT * FROM "Players" LIMIT 10;

Ctrl+Enter로 실행
```

---

## 🎯 추가 설정 (선택)

### 로컬 개발 DB도 추가하기

**+ 버튼 → PostgreSQL**

```
Name: Local Development

General 탭:
  Host: localhost
  Port: 5432
  User: gamedev
  Password: dev123!
  Database: idlerpg

SSH/SSL 탭:
  (아무것도 체크 안 함)
```

이렇게 하면:
- **Production RDS**: 실제 운영 데이터
- **Local Development**: 로컬 테스트 데이터

두 개를 **동시에 보면서 비교** 가능!

---

## 🔒 보안 주의사항

### ⚠️ 절대 하지 말아야 할 것:
- ❌ idlerpg-key.pem 파일을 Git에 커밋
- ❌ 비밀번호를 메모장/문서에 평문 저장
- ❌ Production DB에서 DELETE/TRUNCATE 함부로 실행

### ✅ 권장사항:
- ✓ Production DB는 **읽기 전용**으로 주로 사용
- ✓ 데이터 수정은 API를 통해서만
- ✓ 테스트는 Local Development DB에서

---

## 🐛 문제 해결

### 문제 1: "Connection refused"
**원인**: SSH 터널 설정 누락
**해결**: SSH/SSL 탭에서 "Use SSH tunnel" 체크 확인

### 문제 2: "Permission denied (publickey)"
**원인**: PEM 파일 권한 문제
**해결**: 위의 "PEM 파일 권한 설정" 재실행

### 문제 3: "Unknown host"
**원인**: RDS 호스트 주소 오타
**해결**: 호스트 주소 다시 복사/붙여넣기

### 문제 4: "Authentication failed"
**원인**: 비밀번호 오류
**해결**: 비밀번호 전체를 정확히 복사 (공백 주의)

---

## 📚 Rider Database Tools 유용한 기능

### 1. SQL Console
```
Ctrl+Shift+F10: 현재 쿼리 실행
F4: 테이블에서 Console 열기
```

### 2. 데이터 편집
```
테이블 더블클릭 → 데이터 그리드 → 직접 수정 가능
Submit 버튼으로 저장
```

### 3. Export/Import
```
테이블 우클릭 → Export Data
다양한 포맷 지원: CSV, JSON, SQL, Excel
```

### 4. ER Diagram
```
테이블들 선택 → 우클릭 → Diagrams → Show Visualization
```

### 5. Query History
```
Ctrl+Alt+E: 최근 실행한 쿼리 목록
```

---

## 🎓 참고 자료

- [Rider Database Tools 공식 문서](https://www.jetbrains.com/help/rider/Database_tool_window.html)
- [SSH Tunneling 가이드](https://www.jetbrains.com/help/rider/connecting-to-a-database.html#ssh)

---

**마지막 업데이트**: 2025-10-11
**문제 발생 시**: Claude Code에 물어보기! 🤖
