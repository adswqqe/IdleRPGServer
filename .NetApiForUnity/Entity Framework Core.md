# Entity Framework Core

## 🎯 개념 이해
Unity 개발자를 위한 EF Core - Unity의 GameObject 시스템을 데이터베이스로 확장한 개념

## 🔗 연결 관계
- [[Week 1 - Domain 모델링과 Clean Architecture]]
- [[Player Entity]]
- [[Repository Pattern]]
- [[Query Optimization]]

## 💻 핵심 기능
- **Code First**: Unity Prefab처럼 코드로 데이터 구조 정의
- **Migration**: 데이터베이스 버전 관리
- **Change Tracking**: GameObject 상태 추적과 유사
- **Navigation Properties**: GetComponent와 유사한 관계 탐색

## 🚀 Unity 개발자 관점
```csharp
// Unity: GameObject 찾기
var player = GameObject.FindObjectOfType<Player>();

// EF Core: Entity 찾기  
var player = await context.Players.FirstOrDefaultAsync();
```

## 📚 관련 학습
- [[Repository Pattern]]
- [[Unit of Work Pattern]]
- [[PostgreSQL]]
- [[Query Optimization]]

---

#EntityFramework #EFCore #ORM #데이터베이스