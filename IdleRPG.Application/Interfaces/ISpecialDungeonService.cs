namespace IdleRPG.Application.Interfaces
{
    /// <summary>
    /// 특수 던전 시스템 서비스 인터페이스
    ///
    /// [확장 가능한 아키텍처 설계]
    /// - 보스 던전 (Boss Dungeon)
    /// - 일일 던전 (Daily Dungeon)
    /// - 이벤트 던전 (Event Dungeon)
    ///
    /// [서버 검증 원칙]
    /// - 입장 조건 검증 (레벨, 입장권, 쿨다운)
    /// - CombatService로 전투 시뮬레이션
    /// - BattleLogService로 로그 생성
    /// - 트랜잭션 경계 관리 (StageService 패턴 준수)
    ///
    /// [현재 상태]
    /// - Placeholder Interface (메서드 없음)
    /// - Phase 3에서 구현 예정 (보스 던전 시스템)
    ///
    /// [미래 확장]
    /// - Task&lt;BossChallengeResultDto&gt; ChallengeBossAsync(Guid characterId, Guid bossId)
    /// - Task&lt;DailyDungeonResultDto&gt; ChallengeDailyDungeonAsync(Guid characterId, DailyDungeonType type)
    /// </summary>
    public interface ISpecialDungeonService
    {
        // Placeholder - 미래 메서드 추가 예정
        // 현재는 Clean Architecture 구조 완성을 위한 빈 Interface
    }
}
