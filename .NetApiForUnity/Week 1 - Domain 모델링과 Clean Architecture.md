# Week 1 - Domain 모델링과 Clean Architecture

## 🎯 이번 주 학습 목표
- Unity의 GameObject 시스템 → [[Entity Framework Core]] 데이터 지속성 모델 완전 이해
- 10,000명 동시 사용자를 지원하는 실무 패턴 마스터
- 방치형 RPG 핵심 데이터 모델 설계 및 구현
- 고동시성 환경을 위한 [[PostgreSQL]] 최적화

## 📚 핵심 학습 내용

### 1. Unity → EF Core 개념 매핑
- [[GameObject vs Entity]]
- [[Component vs Navigation Property]]
- [[ScriptableObject vs Database Table]]
- [[GetComponent vs Include]]

### 2. 방치형 RPG 엔티티 설계
- [[Player Entity]] - 핵심 플레이어 정보
- [[Character System]] - 캐릭터 수집 및 관리
- [[PlayerStats]] - 레벨, 경험치, 골드 시스템
- [[Offline Reward System]] - 방치형 게임의 핵심

### 3. 아키텍처 패턴 구현
- [[Repository Pattern]] - 데이터 액세스 추상화
- [[Unit of Work Pattern]] - 트랜잭션 관리
- [[Specification Pattern]] - 복잡한 쿼리 관리
- [[Clean Architecture]] - 레이어 분리

### 4. 성능 최적화 기초
- [[Connection Pooling]] - 연결 관리 최적화
- [[Query Optimization]] - N+1 문제 해결
- [[Indexing Strategy]] - 데이터베이스 인덱스 설계
- [[AsNoTracking]] - 메모리 효율성

## 🛠️ 실습 과제

### Day 1-2: 프로젝트 설정
- [ ] Clean Architecture 프로젝트 구조 생성
- [ ] [[Entity Framework Core]] 설정 및 [[PostgreSQL]] 연결
- [ ] 첫 번째 마이그레이션 생성

### Day 3-4: 엔티티 모델링
- [ ] [[Player Entity]] 구현
- [ ] [[Character System]] 설계
- [ ] [[Offline Reward System]] 기초 구현
- [ ] 엔티티 간 관계 설정

### Day 5-6: Repository 패턴
- [ ] [[IPlayerRepository]] 인터페이스 정의
- [ ] [[PlayerRepository]] 구현
- [ ] [[Unit of Work]] 패턴 적용
- [ ] [[Specification Pattern]] 구현

### Day 7: 성능 최적화
- [ ] 데이터베이스 인덱스 생성
- [ ] [[Connection Pooling]] 설정
- [ ] 쿼리 성능 테스트
- [ ] [[Unity API Client]] 기초 구현

## 🔗 연관 개념

### 이전 학습
- [[Unity GameObject 시스템 이해]]
- [[C# 비동기 프로그래밍]]
- [[데이터베이스 기초 개념]]

### 다음 학습
- [[Week 2 - Authentication과 Authorization]]
- [[JWT Token 시스템]]
- [[API 보안 기초]]

### 관련 기술
- [[Entity Framework Core]]
- [[PostgreSQL]]
- [[Repository Pattern]]
- [[Clean Architecture]]
- [[Docker]]

## 📊 성공 지표

### 기술적 달성 목표
- [ ] 완전한 도메인 모델 구현
- [ ] 1000개 테스트 데이터 처리 성능 확인
- [ ] API 응답시간 100ms 이하 달성
- [ ] 메모리 누수 없는 코드 작성

### 학습 달성 목표
- [ ] Unity 개념을 서버 개념으로 완전 매핑
- [ ] EF Core 기본 패턴 완전 이해
- [ ] 실무 수준 코드 작성 능력
- [ ] 성능 최적화 기초 지식 습득

## 🚨 주의사항 및 함정

### 자주 하는 실수
- [[N+1 Query 문제]] - Include 사용법 숙지 필요
- [[Memory Leak]] - AsNoTracking 적절한 사용
- [[Sync vs Async]] - 비동기 패턴 일관성 유지
- [[Connection String]] - 보안 및 성능 설정

### 디버깅 팁
- [[EF Core 로깅]] - 쿼리 실행 내역 확인
- [[pgAdmin 사용법]] - 데이터베이스 상태 모니터링
- [[Visual Studio Debugger]] - 중단점 활용

## 📝 이번 주 완료 체크리스트

### 환경 설정
- [ ] .NET 8 SDK 설치 및 확인
- [ ] PostgreSQL + Docker 설정 완료
- [ ] Visual Studio/Rider 개발 환경 구성
- [ ] GitHub 저장소 연결

### 코드 구현
- [ ] Clean Architecture 프로젝트 구조
- [ ] 5개 이상 Entity 클래스 작성
- [ ] Repository 인터페이스 및 구현
- [ ] DbContext 설정 및 최적화

### 테스트 및 검증
- [ ] 마이그레이션 정상 실행
- [ ] 1000개 더미 데이터 생성 및 테스트
- [ ] Unity API 클라이언트 기초 연동
- [ ] 성능 측정 및 최적화 확인

---

## 🔄 다음 단계

이번 주를 완료하면 [[Week 2 - Authentication과 Authorization]]로 넘어가서 보안 시스템을 구축합니다.

주요 연결점:
- [[Player Entity]] → [[User Authentication]]
- [[Repository Pattern]] → [[Secure Data Access]]
- [[Clean Architecture]] → [[Security Layer]]

---

#Week1 #EntityFramework #CleanArchitecture #도메인모델링 #성능최적화