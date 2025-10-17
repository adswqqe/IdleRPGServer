using System.Collections.Generic;

namespace IdleRPG.Domain.Entities
{
    /// <summary>
    /// 던전 카테고리 - 던전의 테마나 유형을 분류
    /// </summary>
    public enum DungeonCategory
    {
        /// <summary>
        /// 일반 던전 (스토리 진행용)
        /// </summary>
        Story,

        /// <summary>
        /// 일일 던전 (재료 파밍용)
        /// </summary>
        Daily,

        /// <summary>
        /// 이벤트 던전 (한정 기간)
        /// </summary>
        Event
    }

    /// <summary>
    /// 던전의 기본 정보를 정의하는 마스터 데이터입니다.
    /// Master-Instance 패턴: DungeonTemplate(Master) → DungeonProgress/DungeonRunHistory(Instance)
    ///
    /// 하나의 던전 템플릿은 여러 난이도(DungeonDifficulty)를 가질 수 있으며,
    /// 각 난이도는 독립적인 웨이브와 보상을 가집니다.
    /// </summary>
    public class DungeonTemplate
    {
        /// <summary>
        /// 던전 템플릿의 고유 식별자
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 던전 이름 (UI 표시용, 예: "고블린의 숲", "화염의 동굴")
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 던전 설명 (UI 표시용)
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 던전 카테고리 (스토리, 일일, 이벤트 등)
        /// </summary>
        public DungeonCategory Category { get; set; }

        /// <summary>
        /// 입장 최소 레벨 요구사항
        /// </summary>
        public int MinLevel { get; set; }

        /// <summary>
        /// 던전 활성화 여부
        /// 이벤트 던전이나 점검 중인 콘텐츠를 데이터 삭제 없이 비활성화할 수 있습니다.
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        // Navigation Property
        /// <summary>
        /// 이 던전이 제공하는 난이도 목록 (쉬움, 보통, 어려움 등)
        /// </summary>
        public virtual ICollection<DungeonDifficulty> Difficulties { get; set; } = new List<DungeonDifficulty>();
    }
}
