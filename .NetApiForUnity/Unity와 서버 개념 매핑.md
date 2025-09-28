# Unity와 서버 개념 매핑

## 🎯 개념 브릿지
Unity 개발자가 서버 개발로 전환할 때 필요한 개념 매핑 가이드

## 🔗 연결 관계
- [[Week 1 - Domain 모델링과 Clean Architecture]]
- [[Entity Framework Core]]
- [[Repository Pattern]]
- [[Player Entity]]

## 🎮→🖥️ 핵심 매핑

### 데이터 관리
| Unity 개념 | 서버 개념 | 설명 |
|-----------|----------|------|
| [[GameObject]] | [[Entity]] | 게임 오브젝트 ↔ 데이터베이스 엔티티 |
| [[Component]] | [[Navigation Property]] | 컴포넌트 ↔ 관련 데이터 |
| [[ScriptableObject]] | [[Database Table]] | 설정 데이터 ↔ 테이블 스키마 |
| `GetComponent<>()` | `.Include()` | 컴포넌트 접근 ↔ 관련 데이터 로드 |

### 아키텍처 패턴
| Unity 패턴 | 서버 패턴 | 용도 |
|-----------|----------|------|
| Manager Classes | [[Repository Pattern]] | 데이터 관리 |
| Singleton | Dependency Injection | 인스턴스 관리 |
| Observer Pattern | SignalR Events | 이벤트 처리 |
| Command Pattern | CQRS | 액션 처리 |

### 라이프사이클
| Unity | 서버 | 설명 |
|-------|------|------|
| `Awake()` | Constructor | 초기화 |
| `Start()` | Startup.cs | 설정 구성 |
| `Update()` | Background Services | 반복 작업 |
| `OnDestroy()` | `Dispose()` | 정리 작업 |

### 네트워킹
| Unity | 서버 | 용도 |
|-------|------|------|
| `UnityWebRequest` | HTTP Client | API 호출 |
| Network Events | [[SignalR]] | 실시간 통신 |
| JSON Utility | System.Text.Json | 데이터 직렬화 |

## 💡 사고의 전환
### Unity 사고방식
```csharp
// Unity: 메모리에서 즉시 처리
var player = GameObject.FindWithTag("Player");
player.GetComponent<Health>().TakeDamage(10);
```

### 서버 사고방식  
```csharp
// 서버: 데이터베이스와 비동기 처리
var player = await _playerRepository.GetByIdAsync(playerId);
player.Health.TakeDamage(10);
await _playerRepository.UpdateAsync(player);
```

## 🔄 학습 순서
1. [[Entity Framework Core]] - GameObject → Entity 이해
2. [[Repository Pattern]] - Manager → Repository 전환
3. [[JWT]] - Unity 로그인 → 서버 인증
4. [[SignalR]] - Unity Networking → 실시간 통신

## 📚 추가 학습
- [[Clean Architecture]]
- [[Async Programming]]
- [[Database Design]]

---

#UnityToServer #개념매핑 #아키텍처전환 #학습가이드