using IdleRPG.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace IdleRPG.Infrastructure.Service
{
    /// <summary>
    /// 특수 던전 시스템 서비스 구현체
    ///
    /// [확장 가능한 아키텍처]
    /// - StageService와 동일한 패턴 사용
    /// - ICombatService: 전투 시뮬레이션
    /// - IBattleLogService: 전투 로그 생성
    /// - IUnitOfWork: 트랜잭션 관리
    ///
    /// [현재 상태]
    /// - Placeholder 구현체 (메서드 없음)
    /// - Phase 3에서 보스 던전 시스템 구현 예정
    ///
    /// [미래 구현 예시]
    /// - ChallengeBossAsync(): 보스 도전 로직
    ///   1. 입장 조건 검증
    ///   2. CombatService.SimulateCombatAsync()
    ///   3. 승리 시 특수 보상 지급
    ///   4. BattleLogService.CreateAndSaveLogAsync()
    ///   5. UnitOfWork.SaveChangesAsync()
    /// </summary>
    public class SpecialDungeonService : ISpecialDungeonService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SpecialDungeonService> _logger;

        public SpecialDungeonService(
            IUnitOfWork unitOfWork,
            ILogger<SpecialDungeonService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // Placeholder - 미래 메서드 추가 예정
        // Phase 3: 보스 던전 시스템 구현 시 추가
    }
}
