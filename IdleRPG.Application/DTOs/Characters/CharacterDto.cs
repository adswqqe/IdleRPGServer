namespace IdleRPG.Application.DTOs.Characters
{
    /// <summary>
    /// 캐릭터 응답 DTO (자동 성장 방식, 중첩 구조)
    /// </summary>
    public class CharacterDto
    {
        public Guid Id { get; set; }
        public Guid PlayerId { get; set; }
        public int Level { get; set; }

        /// <summary>
        /// 경험치 (long 타입 - 방치형 게임 특성상 큰 수치 처리)
        /// </summary>
        public long Experience { get; set; }

        /// <summary>
        /// 보유 골드
        /// </summary>
        public long Gold { get; set; }

        /// <summary>
        /// 보유 크리스탈 (프리미엄 화폐)
        /// </summary>
        public long Crystal { get; set; }

        public DateTime LastLoginTime { get; set; }

        /// <summary>
        /// 전투 스탯 (중첩 구조)
        /// </summary>
        public CharacterStatsDto Stats { get; set; } = new CharacterStatsDto();

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}