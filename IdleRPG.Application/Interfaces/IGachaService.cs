using IdleRPG.Application.DTOs.Gacha;

namespace IdleRPG.Application.Interfaces
{
    // TODO: 스킬 가챠 확장 기능 (Phase 2)
    // - 10회 연속 가챠
    // - 스킬 인벤토리 조회
    // - 가챠 히스토리
    // - 스킬 장착/해제

    // 현재는 SkillService로 단순 1회 가챠만 구현

    /*
    /// <summary>
    /// 가챠 서비스 인터페이스
    /// 스킬 가챠, 스킬 인벤토리, 가챠 히스토리 관리
    /// </summary>
    public interface IGachaService
    {
        /// <summary>
        /// 스킬 가챠 실행
        /// - 1회 또는 10회 가챠 지원
        /// - 크리스탈 소모 검증
        /// - 천장 시스템 적용
        /// - 중복 스킬 처리
        /// </summary>
        /// <param name="command">가챠 실행 커맨드 (캐릭터 ID, 가챠 횟수)</param>
        /// <returns>가챠 결과 (획득 스킬 목록, 남은 크리스탈, 현재 천장 카운트)</returns>
        Task<GachaResultDto> PerformGachaAsync(PerformGachaCommand command);

        /// <summary>
        /// 캐릭터의 스킬 인벤토리 조회
        /// - 보유한 모든 스킬 목록
        /// - 등급별 정렬 (Legendary → Epic → Rare → Common)
        /// - 획득 시간 정렬 (최신 순)
        /// </summary>
        /// <param name="characterId">캐릭터 ID</param>
        /// <returns>스킬 목록</returns>
        Task<List<SkillDto>> GetCharacterSkillsAsync(Guid characterId);

        /// <summary>
        /// 캐릭터의 장착 중인 스킬 조회
        /// </summary>
        /// <param name="characterId">캐릭터 ID</param>
        /// <returns>장착된 스킬 목록</returns>
        Task<List<SkillDto>> GetEquippedSkillsAsync(Guid characterId);

        /// <summary>
        /// 캐릭터의 가챠 히스토리 조회
        /// - 최근 N개 가챠 기록
        /// - 뽑은 스킬 정보, 천장 여부 포함
        /// </summary>
        /// <param name="characterId">캐릭터 ID</param>
        /// <param name="count">조회할 개수 (기본값: 50)</param>
        /// <returns>가챠 히스토리 목록</returns>
        Task<List<GachaHistoryDto>> GetGachaHistoryAsync(Guid characterId, int count = 50);

        /// <summary>
        /// 스킬 장착/해제
        /// </summary>
        /// <param name="characterSkillId">캐릭터 스킬 ID</param>
        /// <param name="isEquipped">장착 여부 (true: 장착, false: 해제)</param>
        /// <returns>성공 여부</returns>
        Task<bool> ToggleSkillEquipAsync(Guid characterSkillId, bool isEquipped);
    }
    */
}
