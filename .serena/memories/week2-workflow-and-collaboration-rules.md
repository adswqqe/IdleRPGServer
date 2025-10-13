# Week 2 워크플로우 및 협업 규칙

## Week 2 PRD 위치
- **문서 경로**: `.taskmaster/docs/week2-prd.txt`
- **주제**: Idle Game Loop & Progression System
- **총 9개 Feature**: Monster Entity → Battle System → Offline Rewards → Background Service → Logs → Tests

## 협업 규칙 (Learning Output Style)

### 🤖 Claude가 자동 처리하는 작업
다음 작업은 Claude가 독립적으로 완료:
- **단순 반복 작업**: CRUD 메서드 구현, Repository 패턴 적용
- **보일러플레이트 코드**: DTO 생성, 엔티티 필드 추가, Configuration 작성
- **마이그레이션**: EF Core 마이그레이션 생성 및 SQL 검토
- **문서화**: Unity API 문서 업데이트, Swagger 주석
- **테스트 코드**: 단위 테스트 작성 (시나리오는 함께 검토 가능)
- **코드 정리**: 네이밍, 주석, 포맷팅

### 👥 함께 협업하는 작업 (Learn by Doing)
다음 작업은 설계/구현 전 논의하고 사용자 의견 반영:
- **데이터 설계**: 엔티티 관계, 필드 타입 선택, 인덱스 전략
- **비즈니스 로직**: 전투 공식, 보상 계산, 밸런싱 로직
- **네트워크 로직**: API 설계, 요청/응답 구조, 에러 핸들링 전략
- **아키텍처 설계**: 계층 분리, 서비스 분할, 의존성 구조
- **성능 최적화**: 쿼리 최적화, 캐싱 전략, 동시성 처리
- **보안 설계**: 인증/인가 로직, 데이터 검증 규칙

### 협업 프로세스
1. **설계 단계**: Claude가 초안 제시 → 사용자 피드백 → 최종 결정
2. **구현 단계**: 
   - 핵심 로직은 함께 작성 (TODO(human) 마커 사용)
   - 반복 코드는 Claude가 자동 완성
3. **검토 단계**: Claude가 구현 완료 후 주요 변경사항 요약

## Week 2 Feature 진행 상황

### ✅ 완료
- Feature 1: Monster Entity & Repository (마이그레이션 적용 완료, EC2 배포됨)

### ⏭️ 다음 작업 순서
1. **Feature 4**: Character Schema Update (Gold, LastLoginTime)
   - 협업 필요: Gold 타입 (long vs int), LastLoginTime 업데이트 시점
   - 자동 처리: 필드 추가, Configuration, 마이그레이션

2. **Feature 2**: Battle System Core Logic
   - 협업 필요: 전투 공식, 승패 조건, 크리티컬/회피 여부
   - 자동 처리: BattleService 구현, DTO 생성

3. **Feature 3**: Battle Controller & API
   - 협업 필요: API 엔드포인트 설계, 에러 처리
   - 자동 처리: Controller 코드, Swagger 문서

4. **Feature 5**: Offline Reward System
   - 협업 필요: 보상 공식 밸런싱, 최대 누적 시간
   - 자동 처리: OfflineRewardService 구현

5. **Feature 6**: Idle Progress Background Service
   - 협업 필요: 실행 주기, 대상 캐릭터 선택 기준
   - 자동 처리: IHostedService 구현

6. **Feature 7**: Battle Log System
   - 협업 필요: 로그 보관 기간, 인덱스 전략
   - 자동 처리: BattleLog 엔티티, Repository, API

7. **Feature 8**: Unity Documentation Update
   - 자동 처리: 전체 문서 업데이트

8. **Feature 9**: Unit Tests
   - 협업 필요: 테스트 시나리오 검토
   - 자동 처리: 테스트 코드 작성

## 학습 목표 (Stage 3: Async Performance)
Week 2를 통해 학습할 내용:
- IHostedService / BackgroundService 패턴
- async/await 심화 (ConfigureAwait, Task 관리)
- Timer 기반 스케줄링 (PeriodicTimer, CancellationToken)
- 동시성 처리 (트랜잭션 격리, Race condition 방지)

## 완료 조건 (Definition of Done)
- [ ] 9개 Feature 모두 구현
- [ ] EF Core 마이그레이션 적용 (EC2)
- [ ] Unit Tests 20+ 통과
- [ ] Unity 문서 업데이트
- [ ] Swagger 문서 확인
- [ ] Week 2 완료 체크포인트 메모리 작성

## 참고사항
- Clean Architecture 계층 구조 준수
- 기존 코드 스타일 유지 (GetCurrentUserId 패턴 등)
- 한국어 주석 및 문서 작성
- 에러 처리 일관성 유지
