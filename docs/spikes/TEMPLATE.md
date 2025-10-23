# Spike-XXX: [Technology/Feature Investigation]

**Status**: In Progress | Completed | Abandoned
**Created**: YYYY-MM-DD
**Owner**: [Your Name]

---

## 1. Question (검증할 질문)

**핵심 질문**: [단 하나의 명확한 질문]

예시:
- "SignalR로 1000명 동시접속 시 응답속도 < 200ms 가능한가?"
- "EF Core Include 쿼리가 10만 건 데이터에서 100ms 내 응답 가능한가?"

⚠️ **1 Spike = 1 Question 원칙**: 여러 질문은 별도 Spike로 분리

---

## 2. Spike Trigger (왜 필요한가?)

이 Spike가 필요한 이유 (다음 중 하나 이상 필수):
- [ ] 새로운 기술/라이브러리 첫 도입
- [ ] 성능 검증 필요 (측정 필요)
- [ ] 2개 이상 기술 대안 비교 실험
- [ ] 외부 서비스 연동 실제 테스트
- [ ] 기술적 제약 확인 (문서로 불충분)

❌ **Spike 불필요**: 문서 조사, 아키텍처 결정 (→ ADR 사용)

---

## 3. Method (검증 방법)

**접근 방법**:
1. [Step 1: 예: 테스트 프로젝트 생성]
2. [Step 2: 예: 부하 테스트 1000 동시 연결]
3. [Step 3: 예: 응답 시간, CPU/메모리 측정]

**성공 기준** (Go/No-Go):
- [기준 1: 예: 평균 응답 시간 < 200ms]
- [기준 2: 예: CPU 사용률 < 70%]

---

## 4. Result (결과)

**실험 결과**:
- [측정값, 관찰 사항]

**Verdict**: ✅ **Go** | ⚠️ **Partial** | ❌ **No-Go**

**주요 발견사항**:
- Finding 1: [설명]
- Finding 2: [설명]

---

## 5. Next Steps

**즉시 실행**:
- [ ] ADR 작성 (Go인 경우)
- [ ] Design 문서에 결과 반영

**Throwaway vs Keep**:
- [ ] **Throwaway**: 이 코드는 프로토타입 (삭제)
- [ ] **Keep**: 프로덕션 통합 (리팩토링 후)

**ADR 링크**: [ADR-XXX](../adr/ADR-XXX-topic.md)
