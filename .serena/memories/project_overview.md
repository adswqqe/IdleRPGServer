# IdleRPG Server 프로젝트 개요

## 프로젝트 목적
- **방치형(Idle) RPG 서버**: Unity 클라이언트용 백엔드 API 서버
- Clean Architecture를 따르는 ASP.NET Core 8.0 Web API
- 플레이어가 게임을 종료해도 캐릭터가 자동으로 성장하는 시스템

## 게임 특징
- 방치형 게임플레이 (오프라인 보상 시스템)
- 플레이어-캐릭터 다중 관계 (1명의 플레이어가 여러 캐릭터 보유 가능)
- JWT 기반 인증 시스템
- PostgreSQL 데이터베이스
- Redis 캐싱 (준비됨, 향후 사용 예정)

## 아키텍처 패턴
**Clean Architecture** 4계층 구조:
1. **API Layer** (`IdleRPG.API`): Web API 컨트롤러, 미들웨어
2. **Application Layer** (`IdleRPG.Application`): 비즈니스 로직, DTO, 서비스 인터페이스
3. **Domain Layer** (`IdleRPG.Domain`): 핵심 엔티티, 비즈니스 룰
4. **Infrastructure Layer** (`IdleRPG.Infrastructure`): 데이터 액세스, 외부 서비스

## 현재 상태
- 기본 프로젝트 구조 완성
- JWT 인증 시스템 구현 완료
- 플레이어 가입/로그인 기능 완료
- Entity Framework 마이그레이션 설정 완료
- Docker 개발 환경 구축 완료