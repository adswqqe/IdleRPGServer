using IdleRPG.Domain.Entities;

namespace IdleRPG.Domain.Entities;

/// <summary>
/// 정적 구성 데이터를 가진 던전 스테이지를 나타냅니다.
/// 각 스테이지는 보스 몬스터, 레벨 요구사항, 보상 값을 가집니다.
/// </summary>
public class DungeonStage
{
    /// <summary>
    /// 스테이지 번호 (기본 키). 스테이지 1, 2, 3, 등
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 던전 스테이지의 표시 이름
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 이 스테이지에 입장하기 위한 최소 캐릭터 레벨
    /// </summary>
    public int RequiredLevel { get; set; }

    /// <summary>
    /// 이 스테이지의 보스 몬스터 ID
    /// </summary>
    public Guid MonsterId { get; set; }

    /// <summary>
    /// 보스 몬스터로의 탐색 속성
    /// </summary>
    public Monster? Monster { get; set; }

    /// <summary>
    /// 이 스테이지 클리어 시 기본 경험치 보상
    /// 난이도 배수로 곱해집니다.
    /// </summary>
    public int BaseExperience { get; set; }

    /// <summary>
    /// 이 스테이지 클리어 시 기본 골드 보상
    /// 난이도 배수로 곱해집니다.
    /// </summary>
    public int BaseGold { get; set; }

    /// <summary>
    /// 이 스테이지의 첫 클리어 시 부여되는 보너스 경험치 (모든 난이도)
    /// Nullable - 모든 스테이지가 첫 클리어 보너스를 가지지는 않습니다.
    /// </summary>
    public int? FirstClearBonusExp { get; set; }

    /// <summary>
    /// 이 스테이지의 첫 클리어 시 부여되는 보너스 골드 (모든 난이도)
    /// Nullable - 모든 스테이지가 첫 클리어 보너스를 가지지는 않습니다.
    /// </summary>
    public int? FirstClearBonusGold { get; set; }

    /// <summary>
    /// 이 스테이지 클리어 시 적용할 보상 테이블 ID
    /// Nullable - LootTable이 없으면 BaseGold/BaseExperience만 지급
    /// </summary>
    public int? LootTableId { get; set; }

    /// <summary>
    /// 보상 테이블로의 탐색 속성
    /// </summary>
    public LootTable? LootTable { get; set; }

    /// <summary>
    /// 이 스테이지가 데이터베이스에 생성된 시간
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
