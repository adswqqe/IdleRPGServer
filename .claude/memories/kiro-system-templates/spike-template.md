# Spike: [Technology/Feature Investigation]

**Status**: In Progress | Completed | Abandoned
**Created**: YYYY-MM-DD
**Timebox**: 90-180분
**Owner**: [Your Name]

---

## 1. Question (검증할 질문)

**핵심 질문**: [구체적 질문]

예시:
- "SignalR로 1000명 동시접속 채팅 구현 시 성능 문제 없는가?"
- "EF Core의 Include 쿼리가 대규모 데이터(10만 건)에서 100ms 내 응답 가능한가?"

---

## 2. Hypothesis (가설)

**가설**: [예상되는 답변 또는 결과]

예시:
- "SignalR + Redis Backplane으로 1000명 동시접속 가능할 것으로 예상"
- "Include 대신 Select로 필요한 필드만 가져오면 성능 개선 예상"

---

## 3. Spike Trigger (왜 필요한가?)

이 Spike가 필요한 이유 (다음 중 하나 이상):
- [ ] 새로운 기술/라이브러리 도입
- [ ] 성능 관련 불확실성
- [ ] 여러 설계 대안 중 선택 필요 (2개 이상)
- [ ] 외부 서비스 연동 방식 검증
- [ ] 기술적 제약 확인

---

## 4. Method (검증 방법)

**접근 방법**:
1. [Step 1: 예: 테스트 프로젝트 생성]
2. [Step 2: 예: 부하 테스트 도구로 1000 동시 연결 시뮬레이션]
3. [Step 3: 예: 응답 시간, CPU/메모리 사용량 측정]

**성공 기준**:
- [기준 1: 예: 평균 응답 시간 < 200ms]
- [기준 2: 예: CPU 사용률 < 70%]

---

## 5. Timebox

**예상 시간**: [90분 | 120분 | 180분]
**실제 소요 시간**: [기록]

⚠️ **타임박스 초과 시**: 현재까지 발견한 내용으로 ADR 작성 후 중단

---

## 6. Result (결과)

**실험 결과**:
- [결과 1: 측정값, 관찰 사항]
- [결과 2: 발견된 문제점 또는 제약사항]

**성공 여부**: ✅ Success | ⚠️ Partial Success | ❌ Failed

**주요 발견사항**:
- Finding 1: [설명]
- Finding 2: [설명]

---

## 7. Decision & ADR Link

**최종 결정**: [선택한 방안]

**ADR 링크**: [ADR-XXX: Decision Title](../adrs/YYYYMMDD-decision-title.md)

---

## 8. Next Steps

**즉시 실행**:
- [ ] ADR 작성 완료
- [ ] Design 문서에 결정사항 반영
- [ ] 관련 팀원에게 공유 (해당 시)

**향후 계획**:
- [ ] [Step 1]
- [ ] [Step 2]

---

## 9. Throwaway vs Keep

- [ ] **Throwaway**: 이 Spike 코드는 프로토타입이며 삭제
- [ ] **Keep**: 이 코드를 프로덕션에 통합 (리팩토링 후)
