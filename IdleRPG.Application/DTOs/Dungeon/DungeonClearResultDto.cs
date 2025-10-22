using IdleRPG.Application.DTOs.Battle;
using IdleRPG.Application.DTOs.Equipment;
using IdleRPG.Application.DTOs.Rewards;

namespace IdleRPG.Application.DTOs.Dungeon
{
    /// <summary>
    /// 던전 스테이지 클리어 결과 DTO
    /// 서버가 검증 완료 후 보상을 지급하고 결과를 반환할 때 사용
    ///
    /// [서버 권한 설계]
    /// - IsSuccess: 서버 검증 결과 (레벨, 진행도, 동시성 체크 통과 여부)
    /// - Reward: 서버가 계산한 보상 (난이도 배수 적용 완료)
    /// - DroppedEquipments: 서버가 랜덤 생성한 장비 드랍 목록
    /// - NewHighestStage: 서버가 업데이트한 진행도
    /// - ErrorMessage: 검증 실패 시 이유
    /// </summary>
    public class DungeonClearResultDto
    {
        /// <summary>
        /// 클리어 성공 여부 (서버 검증 통과 여부)
        /// false일 경우 ErrorMessage 확인 필요
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 서버가 지급한 보상 정보 (Gold, Experience)
        /// IsSuccess = false일 경우 null
        /// </summary>
        public RewardDto? Reward { get; set; }

        /// <summary>
        /// 업데이트된 해당 난이도 최고 클리어 스테이지 ID
        /// 예: Normal 난이도 5번 스테이지 클리어 → 5 반환
        /// </summary>
        public int NewHighestStage { get; set; }

        /// <summary>
        /// 실패 시 에러 메시지
        /// 예: "레벨이 부족합니다", "이전 스테이지를 먼저 클리어하세요", "이미 처리 중입니다"
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 레벨업 여부 (보상 경험치로 레벨업 했는지)
        /// 클라이언트 UI 연출용
        /// </summary>
        public bool IsLevelUp { get; set; }

        /// <summary>
        /// 레벨업 후 새로운 레벨 (레벨업 안 했으면 기존 레벨)
        /// </summary>
        public int CurrentLevel { get; set; }

        /// <summary>
        /// 던전 클리어 시 드랍된 장비 목록
        /// LootTable 기반 확률 드랍
        /// </summary>
        public List<EquipmentDto>? DroppedEquipments { get; set; }

        /// <summary>
        /// 전투 통계 (턴 수, 데미지 등)
        /// 전투 패배 시에도 포함됨
        /// </summary>
        public BattleStatisticsDto? BattleStatistics { get; set; }
    }
}
