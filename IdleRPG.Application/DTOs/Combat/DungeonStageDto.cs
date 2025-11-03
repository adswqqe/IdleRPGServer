using IdleRPG.Domain.Enums;

namespace IdleRPG.Application.DTOs.Combat
{
    /// <summary>
    /// 전투 스테이지 정보 DTO (MainBattle, Dungeon 공통)
    ///
    /// [사용처]
    /// - MainBattle System: GET /api/stages (메인 스테이지 진행)
    /// - Special Dungeon System: GET /api/dungeons (보스 던전, 미래 구현)
    ///
    /// [참고] "Dungeon" prefix는 레거시 이름이며, 모든 전투 컨텐츠에서 재사용됩니다.
    /// </summary>
    public class DungeonStageDto
    {
        /// <summary>
        /// 스테이지 ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 스테이지 이름 (예: "숲의 입구", "어둠의 동굴 1층")
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 스테이지 설명
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 난이도 (Normal, Hard, Hell)
        /// </summary>
        public DungeonDifficulty Difficulty { get; set; }

        /// <summary>
        /// 입장 가능 레벨
        /// </summary>
        public int RequiredLevel { get; set; }

        /// <summary>
        /// 보스 몬스터 ID
        /// </summary>
        public Guid BossMonsterId { get; set; }

        /// <summary>
        /// 보스 몬스터 이름 (조인 결과)
        /// </summary>
        public string? BossMonsterName { get; set; }

        /// <summary>
        /// 기본 골드 보상
        /// 실제 지급 시 난이도 배수가 적용됩니다.
        /// </summary>
        public long BaseGoldReward { get; set; }

        /// <summary>
        /// 기본 경험치 보상
        /// 실제 지급 시 난이도 배수가 적용됩니다.
        /// </summary>
        public long BaseExpReward { get; set; }

        /// <summary>
        /// 클라이언트 표시용 - 난이도별 최종 골드 (배수 적용 후)
        /// 클라이언트가 계산할 수도 있지만, 서버에서 미리 계산해서 보내주면 편리함
        /// </summary>
        public long FinalGoldReward { get; set; }

        /// <summary>
        /// 클라이언트 표시용 - 난이도별 최종 경험치 (배수 적용 후)
        /// </summary>
        public long FinalExpReward { get; set; }

        /// <summary>
        /// 도전 가능 여부 (캐릭터 레벨, 이전 스테이지 클리어 여부 기반)
        /// 서버에서 계산해서 보냄
        /// </summary>
        public bool IsAvailable { get; set; }
    }
}
