namespace IdleRPG.Application.DTOs.Battle
{
    /// <summary>
    /// 전투 로그 DTO - 전투 히스토리 조회용
    /// </summary>
    public class BattleLogDto
    {
        /// <summary>
        /// 전투 로그 ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 몬스터 이름
        /// </summary>
        public string MonsterName { get; set; } = string.Empty;

        /// <summary>
        /// 몬스터 레벨
        /// </summary>
        public int MonsterLevel { get; set; }

        /// <summary>
        /// 승리 여부
        /// </summary>
        public bool IsVictory { get; set; }

        /// <summary>
        /// 획득 경험치
        /// </summary>
        public int ExperienceGained { get; set; }

        /// <summary>
        /// 획득 골드
        /// </summary>
        public int GoldGained { get; set; }

        /// <summary>
        /// 입힌 데미지
        /// </summary>
        public int DamageDealt { get; set; }

        /// <summary>
        /// 받은 데미지
        /// </summary>
        public int DamageTaken { get; set; }

        /// <summary>
        /// 전투 일시
        /// </summary>
        public DateTime BattleDate { get; set; }
    }

    /// <summary>
    /// 전투 로그 목록 응답 (페이징 정보 포함)
    /// </summary>
    public class BattleLogsResponse
    {
        /// <summary>
        /// 전투 로그 목록
        /// </summary>
        public List<BattleLogDto> Logs { get; set; } = new();

        /// <summary>
        /// 현재 페이지
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// 페이지 크기
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 총 로그 수
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 총 페이지 수
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
