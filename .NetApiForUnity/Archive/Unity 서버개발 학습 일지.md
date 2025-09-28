 [[🎮 방치형 RPG 서버 개발 완전 가이드 - Week 1]]# 🎮 Unity 개발자를 위한 서버개발 학습 일지

## 📋 프로젝트 개요
- **목표**: 2D RPG 방치형 게임 서버 구축
- **동시 접속자 목표**: 1만명
- **개발자 배경**: 3년차 Unity 클라이언트 게임 프로그래머
- **학습 기간**: 2025년 9월 ~ (Week 1 진행 중)
- **기술 스택**: .NET 8 + ASP.NET Core + PostgreSQL + Docker + GitHub
- **개발 도구**: JetBrains Rider (GUI 방식 선호)

---

## 📈 주간별 학습 계획

### Week 1: 환경 구축 및 기초 설정
- **Day 0**: ✅ **개발 환경 구축** (완료)
- **Day 1-2**: 방치형 게임 데이터베이스 설계
- **Day 3-4**: 플레이어 관리 + 오프라인 보상 API
- **Day 5-7**: Entity Framework Core 심화 + 성능 최적화

### Week 2: 고급 기능 및 최적화 (예정)
- JWT 인증 시스템
- SignalR 실시간 통신
- 가챠 시스템 구현
- 부하 테스트 및 1만 동접 시뮬레이션

---

## 🔗 관련 학습 문서
- [[Week 1 Day 0 - 환경구축 완료]]
- [[방치형 RPG 서버 설계 노트]]
- [[Unity vs ASP.NET Core 개념 매핑]]

---

*시작일: 2025년 9월 26일*  
*현재 진도: Week 1 Day 0 완료 (14%)*


---

## Week 1 Day 1 - Claude MCP 통합 환경 구축 (2025.09.26)

### 🎯 오늘의 성과
Claude Desktop ↔ Claude Code 완벽 연계 환경 구축 완료!

#### 구축한 MCP 서버들
1. **Memory Bank MCP** - Desktop↔Code 연계의 핵심
2. **Sequential Thinking MCP** - 복잡한 문제 단계별 해결
3. **Context7 MCP** - 최신 라이브러리 문서 조회  
4. **Filesystem MCP** - Unity/ASP.NET 프로젝트 파일 관리
5. **Obsidian MCP** - 이 학습 노트들과 연동 (uvx 기반)
6. **Unity MCP** - Unity 프로젝트 전용 관리
7. **GitHub MCP** - 저장소 관리 자동화

#### 핵심 워크플로우 완성
```
집에서 Claude Desktop 질문 → Memory Bank 저장 
→ Git 동기화 → 회사에서 Claude Code로 구현
```

#### 자동화 스크립트 완성
- **setup-claude-mcp.sh**: 기존 프로젝트에 자동 MCP 환경 추가
- **sync.sh**: 집↔회사 환경 전환 스크립트  
- **통합 설정 관리**: 환경변수 기반 동적 설정

### 🐛 발견된 이슈들
1. **Memory Bank Git 동기화 문제**: `.gitignore`로 인한 동기화 불가
2. **MCP 서버 응답 지연**: 일부 서버에서 무한 로딩
3. **Obsidian MCP 요구사항**: Python + uvx 사전 설치 필요

### 🔄 Claude-Continuity 제거 결정
- Memory Bank만으로도 충분한 연계 가능
- 중복 기능으로 인한 복잡성 제거
- 더 심플하고 안정적인 구조로 개선

### 💡 다음 계획
1. Memory Bank Git 동기화 문제 해결
2. ASP.NET Core Web API 본격 학습 시작
3. Unity 클라이언트 ↔ 서버 통신 실습

### 🎓 배운 점
- **MCP 아키텍처 이해**: 각 서버의 역할과 연동 방식
- **환경 동기화 설계**: 개발환경별 설정 분리의 중요성  
- **자동화의 힘**: 복잡한 설정을 스크립트 한 번으로 해결

### 🔗 관련 자료
- [[Claude MCP 통합 환경 구축 가이드]] - 상세 구축 가이드
- [setup-claude-mcp.sh 스크립트](/scripts/setup-claude-mcp.sh)

---


---

## 📚 Clean Architecture 심화 학습 (2025.09.26 추가)

### 완성된 Clean Architecture 문서 시리즈
1. [[Clean Architecture 개념 정리]] - 기본 원칙 및 Unity와의 비교
2. [[Clean Architecture - Domain Layer 상세]] - 게임 엔티티 및 비즈니스 로직
3. [[Clean Architecture - Application Layer 상세]] - 유스케이스 및 서비스 구현  
4. [[Clean Architecture - Infrastructure Layer 상세]] - 데이터베이스, 캐시, 외부 API
5. [[Unity vs Clean Architecture 비교분석]] - Unity 개발자 관점의 상세 비교

### 🎯 학습 성과
- ✅ **Domain 엔티티**: Player, Character, OfflineReward, Item 클래스 설계
- ✅ **Application 서비스**: PlayerService, OfflineRewardService, CharacterService 구현
- ✅ **Infrastructure**: Repository 패턴, Redis 캐시, 백그라운드 작업
- ✅ **Unity 연결점**: GameObject ↔ Entity, Manager ↔ Service, Services ↔ Infrastructure

### 💡 핵심 인사이트
**"Unity 클라이언트 경험이 서버 Clean Architecture 학습에 완벽하게 전이된다!"**
- 아키텍처 설계 사고방식 100% 활용 가능
- 패턴 인식 능력으로 빠른 이해
- 오히려 Unity 제약에서 벗어나 더 자유로운 설계 가능

### 🔄 업데이트된 학습 진도
- **Clean Architecture 이론**: 100% 완료 ✅
- **실무 코드 예제**: 100% 완료 ✅  
- **Unity 연결점 매핑**: 100% 완료 ✅
- **다음 단계**: Entity Framework 실습 준비 완료 🚀

*Clean Architecture 이론 학습 완료: 2025년 9월 26일*