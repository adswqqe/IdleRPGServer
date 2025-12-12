using IdleRPG.Application.DTOs.Combat;
using IdleRPG.Domain.Enums;

namespace IdleRPG.Application.Interfaces
{
    /// <summary>
    /// 메인 스테이지 시스템 서비스 인터페이스
    ///
    /// [서버 중심 설계 원칙]
    /// - 모든 검증은 서버에서 수행 (레벨, 진행도, 동시성)
    /// - 보상 계산은 100% 서버 로직 (클라이언트는 표시만)
    /// - 치트 방지를 위한 트랜잭션 처리
    ///
    /// [변경사항 - Combat System Refactoring]
    /// - 구 IDungeonService → IStageService로 리네이밍
    /// - CombatService, BattleLogService와 협력하여 전투 처리
    /// - 트랜잭션 경계 관리 책임 (보상 지급 + 로그 저장 + 진행도 업데이트)
    /// </summary>
    public interface IStageService
    {
        /// <summary>
        /// 캐릭터가 도전 가능한 메인 스테이지 목록을 조회합니다.
        ///
        /// [서버 검증]
        /// - 캐릭터 레벨 >= RequiredLevel인 스테이지만 반환
        /// - 난이도별 진행도 기반 필터링 (이전 스테이지 클리어 여부)
        /// - IsAvailable 플래그 설정
        /// </summary>
        /// <param name="characterId">캐릭터 ID</param>
        /// <param name="difficulty">조회할 난이도 (null이면 모든 난이도)</param>
        /// <returns>도전 가능한 스테이지 목록 (난이도 배수 적용된 보상 포함)</returns>
        Task<List<DungeonStageDto>> GetAvailableStagesAsync(Guid characterId, DungeonDifficulty? difficulty = null);

        /// <summary>
        /// 캐릭터의 메인 스테이지 진행 상황을 조회합니다.
        /// 각 난이도별로 최고 클리어한 스테이지를 반환합니다.
        /// </summary>
        /// <param name="characterId">캐릭터 ID</param>
        /// <returns>난이도별 진행 상황</returns>
        Task<CharacterMainBattleProgressDto> GetProgressAsync(Guid characterId);

        /// <summary>
        /// 메인 스테이지 클리어를 처리합니다.
        ///
        /// [서버 검증 단계]
        /// 1. 캐릭터 레벨 검증 (character.Level >= stage.RequiredLevel)
        /// 2. 진행도 검증 (이전 스테이지 클리어 여부)
        /// 3. 동시성 제어 (같은 캐릭터의 중복 요청 방지)
        ///
        /// [트랜잭션 처리 - Combat System Refactoring 반영]
        /// 1. CombatService.SimulateCombatAsync() 호출 → 전투 시뮬레이션
        /// 2. 패배 시 Early Return
        /// 3. 승리 시:
        ///    - 보상 계산 (Gold, Exp with difficulty multiplier)
        ///    - Character 업데이트 (Gold, Exp via CharacterService.ProcessExperienceGain)
        ///    - CharacterDungeonProgress 업데이트
        ///    - BattleLogService.CreateAndSaveLogAsync() 호출
        ///    - UnitOfWork.SaveChangesAsync() → 트랜잭션 커밋
        /// → 하나의 트랜잭션으로 처리, 실패 시 전체 롤백
        /// </summary>
        /// <param name="characterId">캐릭터 ID</param>
        /// <param name="request">클리어 요청 (StageId, Difficulty)</param>
        /// <returns>클리어 결과 (성공 여부, 보상, 새로운 진행도)</returns>
        Task<DungeonClearResultDto> ClearStageAsync(Guid characterId, DungeonClearRequestDto request);
    }
}
