# 20주 개발 로드맵 체크리스트

## 📊 전체 진행률: 10% (2/20 시스템 완료)

---

## Phase 1: Foundation (Week 1-6) - MVP 시스템

### Week 1: 인증 & 캐릭터 기초 ✅
**시스템 1: 인증 시스템** ✅
- [x] Player 엔티티
- [x] RefreshToken 엔티티
- [x] JWT 토큰 서비스 (Access + Refresh)
- [x] AuthService (회원가입, 로그인, 토큰 갱신)
- [x] AuthController (5개 엔드포인트)
- [x] BCrypt 비밀번호 해싱
- [x] Jenkins CI/CD 파이프라인
- [x] AWS EC2 + RDS 배포

**시스템 2: 캐릭터 성장** ✅
- [x] Character 엔티티
- [x] CharacterStats (Value Object)
- [x] 자동 스탯 성장 시스템
- [x] CharacterService (CRUD, 경험치, 레벨업)
- [x] CharacterController (6개 엔드포인트)
- [x] Monster 엔티티 (5종 시딩)
- [x] 단위 테스트 (17 tests)
- [x] Unity API 문서 작성

**학습 성과**:
- RESTful API 설계
- EF Core 마이그레이션
- JWT 인증/인가
- Clean Architecture 구조
- xUnit 단위 테스트

---

### Week 2: 전투 & 오프라인 보상 🔄
**시스템 4: 전투 시스템** ⏳
- [x] Monster 엔티티
- [ ] BattleService (턴제 전투 시뮬레이션)
- [ ] 데미지 계산 공식
- [ ] 크리티컬/회피 처리
- [ ] BattleResultDto
- [ ] BattleController (3개 엔드포인트)
- [ ] 경험치/골드 보상 지급
- [ ] 단위 테스트 (10+ tests)

**시스템 5: 오프라인 보상** ⏳
- [x] Gold, LastLoginTime 필드 추가
- [ ] OfflineRewardService
- [ ] 보상 계산 공식 (분당 Exp/Gold)
- [ ] 최대 누적 시간 제한 (VIP 연동)
- [ ] OfflineRewardController (2개 엔드포인트)
- [ ] 단위 테스트 (5+ tests)

**추가 작업**:
- [ ] Background Service (IHostedService) - 자동 사냥
- [ ] BattleLog 시스템
- [ ] Unity 문서 업데이트

**학습 목표**:
- IHostedService / BackgroundService
- async/await 심화
- 게임 밸런싱 (전투 공식)

---

### Week 3: 인벤토리 & 장비 📋
**시스템 3: 인벤토리 & 장비**
- [ ] Item 엔티티 (무기, 방어구, 장신구)
- [ ] ItemTemplate 마스터 데이터
- [ ] Inventory 엔티티 (1:N 관계)
- [ ] 장비 착용/해제 로직
- [ ] 장착 장비 스탯 합산
- [ ] InventoryService
- [ ] InventoryController (6개 엔드포인트)
- [ ] 단위 테스트 (10+ tests)
- [ ] Unity 인벤토리 UI 문서

**학습 목표**:
- 1:N 관계 (Character - Inventory)
- LINQ 쿼리
- 트랜잭션 처리

---

### Week 4-5: 던전 시스템 📋
**시스템 6: 던전 시스템**
- [ ] Dungeon 엔티티
- [ ] DungeonProgress 엔티티
- [ ] 난이도별 던전 (Normal, Hard, Hell)
- [ ] 스테이지 진행 관리
- [ ] 던전 클리어 조건
- [ ] 보스 몬스터 연동
- [ ] DungeonService
- [ ] DungeonController (6개 엔드포인트)
- [ ] 마스터 데이터 시딩
- [ ] 단위 테스트 (10+ tests)

**학습 목표**:
- 마스터 데이터 관리
- Seed Data 전략
- 진행도 관리 시스템

---

### Week 6: 장비 강화 📋
**시스템 7: 장비 강화**
- [ ] Enhancement 로직 (확률 기반)
- [ ] 강화 레벨별 성공률 (+0~+15)
- [ ] 골드/강화석 소모
- [ ] 실패 시 레벨 하락 (+10 이상)
- [ ] 안전 강화 시스템
- [ ] EnhancementService
- [ ] EnhancementController (3개 엔드포인트)
- [ ] 단위 테스트 (15+ tests)

**학습 목표**:
- 확률 계산 시스템
- 트랜잭션 (성공/실패)
- 게임 밸런싱 (강화 확률)

---

## Phase 2: Expansion (Week 7-12) - 핵심 게임플레이

### Week 7-8: 스킬 시스템 📋
**시스템 8: 스킬 시스템**
- [ ] Skill 엔티티 (마스터 데이터)
- [ ] CharacterSkill 엔티티 (M:N 관계)
- [ ] 스킬 타입 (공격, 버프, 힐, 디버프)
- [ ] 스킬 레벨업 시스템
- [ ] 전투 중 스킬 자동 사용
- [ ] 쿨다운 관리
- [ ] SkillService
- [ ] SkillController (5개 엔드포인트)
- [ ] 단위 테스트 (10+ tests)

**학습 목표**:
- 복잡한 데이터 모델링
- 스킬 효과 계산

---

### Week 9: 펫 시스템 📋
**시스템 9: 펫 시스템**
- [ ] Pet 엔티티 (마스터 데이터)
- [ ] CharacterPet 엔티티 (M:N 관계)
- [ ] 펫 레벨업 시스템
- [ ] 펫 스탯 → 캐릭터 스탯 합산
- [ ] 펫 스킬 (전투 중 발동)
- [ ] PetService
- [ ] PetController (6개 엔드포인트)
- [ ] 단위 테스트 (10+ tests)

**학습 목표**:
- M:N 관계
- 스탯 합산 로직

---

### Week 10: PVP 아레나 📋
**시스템 10: PVP 아레나**
- [ ] ArenaBattle 엔티티
- [ ] 상대 캐릭터 스냅샷
- [ ] ELO 매칭 시스템
- [ ] MMR 계산 (승패 시 변화)
- [ ] 대전 시뮬레이션 (AI)
- [ ] 시즌 보상 시스템
- [ ] ArenaService
- [ ] ArenaController (6개 엔드포인트)
- [ ] 단위 테스트 (10+ tests)

**학습 목표**:
- ELO 레이팅 시스템
- 스냅샷 데이터
- AI 전투

---

### Week 11: 친구 시스템 📋
**시스템 11: 친구 시스템**
- [ ] Friendship 엔티티 (M:N 자기 참조)
- [ ] 친구 요청/수락/거절
- [ ] 친구 목록 조회
- [ ] 선물 보내기 (일일 1회)
- [ ] 유저 검색
- [ ] FriendService
- [ ] FriendController (7개 엔드포인트)
- [ ] 단위 테스트 (10+ tests)

**학습 목표**:
- M:N 자기 참조 관계
- 일일 제한 시스템

---

### Week 12: 길드 시스템 (1부) 📋
**시스템 12: 길드 시스템 (기본)**
- [ ] Guild 엔티티
- [ ] GuildMember 엔티티
- [ ] 권한 시스템 (Master, SubMaster, Member)
- [ ] 길드 생성/가입/탈퇴
- [ ] 멤버 관리 (추방, 승급)
- [ ] 길드 레벨/경험치
- [ ] GuildService
- [ ] GuildController (10개 엔드포인트)
- [ ] 단위 테스트 (15+ tests)

**학습 목표**:
- 복잡한 권한 관리
- 길드 로직

---

## Phase 3: Advanced (Week 13-18) - 소셜 & 수익화

### Week 13: 실시간 채팅 (SignalR) 📋
**시스템 13: 실시간 채팅**
- [ ] ChatHub (SignalR Hub)
- [ ] 전체 채팅
- [ ] 길드 채팅 (그룹)
- [ ] 귓속말 (1:1)
- [ ] 채팅 금지 시스템
- [ ] Unity SignalR Client 통합
- [ ] 단위 테스트 (SignalR)

**학습 목표**:
- SignalR Hub
- 실시간 통신
- Unity SignalR Client

---

### Week 14: 길드 레이드 📋
**시스템 14: 보스 레이드 (협동)**
- [ ] Raid 엔티티
- [ ] RaidParticipation 엔티티
- [ ] 보스 HP 공유 상태
- [ ] 기여도 랭킹
- [ ] 보상 차등 지급
- [ ] RaidService
- [ ] RaidController (6개 엔드포인트)
- [ ] 단위 테스트 (10+ tests)

**학습 목표**:
- 공유 상태 관리
- 동시성 처리

---

### Week 15: 퀘스트 & 업적 📋
**시스템 15: 퀘스트 & 업적**
- [ ] Quest 엔티티
- [ ] QuestProgress 엔티티
- [ ] Achievement 엔티티
- [ ] 퀘스트 타입 (메인, 일일, 업적)
- [ ] 진행도 트래킹
- [ ] 보상 시스템
- [ ] QuestService
- [ ] QuestController (6개 엔드포인트)
- [ ] 단위 테스트 (10+ tests)

**학습 목표**:
- 이벤트 트래킹
- 진행도 계산

---

### Week 16: 일일 미션 & 출석 📋
**시스템 16: 일일 미션 & 출석**
- [ ] DailyMission 엔티티
- [ ] AttendanceRecord 엔티티
- [ ] 일일 리셋 로직 (00:00)
- [ ] 연속 출석 보상
- [ ] 28일 사이클
- [ ] DailyMissionService
- [ ] AttendanceController (4개 엔드포인트)
- [ ] Background Service (일일 리셋)
- [ ] 단위 테스트 (10+ tests)

**학습 목표**:
- 일일 리셋 시스템
- DateTime 처리

---

### Week 17: 가챠 시스템 📋
**시스템 17: 가챠 시스템**
- [ ] GachaBanner 엔티티
- [ ] GachaHistory 엔티티
- [ ] 확률 시스템 (일반, 레어, 에픽, 전설)
- [ ] 천장 시스템 (90회)
- [ ] 10연차 보장 (레어 이상 1개)
- [ ] 확률 공개 (법적 요구)
- [ ] GachaService
- [ ] GachaController (5개 엔드포인트)
- [ ] 단위 테스트 (15+ tests)

**학습 목표**:
- 확률 계산
- 보장 시스템

---

### Week 18: 상점 & VIP 📋
**시스템 18: 상점 & VIP**
- [ ] Shop 엔티티
- [ ] VIP 시스템 (레벨별 혜택)
- [ ] 유료 재화 (다이아몬드)
- [ ] VIP 혜택 (오프라인 시간, 스탯 버프)
- [ ] 결제 시스템 (모의)
- [ ] ShopService
- [ ] ShopController (6개 엔드포인트)
- [ ] 단위 테스트 (10+ tests)

**학습 목표**:
- 수익화 시스템 설계
- VIP 혜택 계산

---

## Phase 4: Polish (Week 19-20) - 라이브 운영

### Week 19: 랭킹 시스템 (Redis) 📋
**시스템 19: 랭킹 시스템**
- [ ] Redis Sorted Set 연동
- [ ] 레벨 랭킹
- [ ] PVP 랭킹
- [ ] 길드 랭킹
- [ ] 레이드 랭킹
- [ ] 실시간 업데이트
- [ ] RankingService
- [ ] RankingController (5개 엔드포인트)
- [ ] Redis 캐싱 전략

**학습 목표**:
- Redis Sorted Set
- 랭킹 시스템

---

### Week 20: 우편함 & 이벤트 📋
**시스템 20: 우편함 & 이벤트**
- [ ] Mail 엔티티
- [ ] MailAttachment 엔티티
- [ ] Event 엔티티
- [ ] 시스템 메일 발송
- [ ] 보상 메일
- [ ] 이벤트 스케줄링
- [ ] MailService
- [ ] EventService
- [ ] MailController (6개 엔드포인트)
- [ ] EventController (4개 엔드포인트)

**학습 목표**:
- 메일 시스템
- 이벤트 스케줄링

---

## 📈 학습 마일스톤

### Phase 1 완료 시 (Week 6)
- [x] RESTful API 설계 마스터
- [ ] EF Core 고급 활용
- [ ] JWT 인증/인가
- [ ] Background Services
- [ ] 게임 로직 서버 검증

### Phase 2 완료 시 (Week 12)
- [ ] 복잡한 데이터 관계 (M:N, 자기 참조)
- [ ] 게임 밸런싱
- [ ] ELO 레이팅
- [ ] 트랜잭션 및 동시성

### Phase 3 완료 시 (Week 18)
- [ ] SignalR 실시간 통신
- [ ] Redis 고급 활용
- [ ] 수익화 시스템
- [ ] 치팅 방지

### Phase 4 완료 시 (Week 20)
- [ ] 라이브 운영 시스템
- [ ] 이벤트 관리
- [ ] 성능 모니터링
- [ ] Production 최적화

---

## 🎯 최종 목표

- [ ] 20개 시스템 모두 구현
- [ ] 100+ API 엔드포인트
- [ ] 200+ 단위 테스트
- [ ] Unity 클라이언트 연동
- [ ] 상용 수준 품질
- [ ] 포트폴리오급 결과물

**예상 완료일**: 2026년 3월 (20주 후)
