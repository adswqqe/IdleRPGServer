# 스킬 가챠 시스템 설계 및 Task Master 등록 (2025-10-20)

## 📋 세션 요약

### 목표
강화 시스템 대신 **스킬 랜덤 뽑기 (가챠) 시스템** 구현 계획 수립 및 작업 등록

### 사용자 요구사항
1. **완전한 가챠 시스템**: 천장 시스템, 확률 공개, 10연차, 히스토리
2. **혼합형 스킬**: 패시브(스탯 버프) + 액티브(전투 중 발동)
3. **새로운 재화**: 크리스탈 (던전/미션에서 획득)
4. **제한된 슬롯**: 캐릭터당 4-6개 슬롯 (레벨업으로 확장)

---

## 🤖 AI 협업 프로세스

### Phase 1: Multi-AI Collaborator (Gemini 2.5 Pro 상담)

**8가지 핵심 질문**:
1. Clean Architecture 구조 검증
2. 가챠 확률 로직 설계 (Random vs Cryptographic)
3. 중복 스킬 처리 (DB Constraint vs Application)
4. 스킬 슬롯 확장 (Configuration vs 동적 계산)
5. 패시브 스킬 적용 방식 (Entity vs Service)
6. 액티브 스킬 우선순위 (Phase 분리)
7. 크리스탈 경제 밸런스
8. DB 인덱스 전략

**Gemini 핵심 제안**:
- ✅ GachaLogicService → Domain Service (비즈니스 로직)
- ✅ Random.Shared 사용 (누적 확률 0.01% 단위)
- ✅ 천장 리셋: 전설 획득 시 즉시
- ✅ Configuration 테이블 + 메모리 캐싱
- ⭐ **StatCalculationService 중앙화** (핵심 통찰)
- ⭐ **스킬 레벨업 시스템** (중복 → 성장)

---

## 📊 최종 설계안 (Option B - 점진적 접근)

### Phase 1: Week 3 - 가챠 핵심 (24개 작업)
**총 예상 시간**: ~19.5시간

#### Milestone 1: Domain Layer (7개)
#### Milestone 2: Infrastructure Layer (5개)
#### Milestone 3: Application Layer (4개)
#### Milestone 4: API Layer (1개)
#### Milestone 5: Database (3개)
#### Milestone 6: Testing & Documentation (4개)

---

## 📝 Task Master 등록 결과

### Gemini 협업 성과
1. ✅ **PRD 파일 생성**: `skill-gacha-prd.md`
2. ✅ **Task Master 자동 파싱**: `task-master parse-prd --append`
3. ✅ **24개 작업 등록 완료**
4. ✅ **총 작업 수**: 25개 (기존 1개 + 신규 24개)

---

## 🎯 다음 단계

### Option 1: Task Master 순차 진행 (권장)
```bash
task-master list
task-master next
task-master complete <id>
```

**다음 세션 시작 시**: `task-master next` 명령어로 첫 번째 작업 시작
