using IdleRPG.Domain.Enums;

namespace IdleRPG.Domain.ValueObjects;

/// <summary>
/// 주어진 던전 난이도에 대한 배수를 캡슐화하는 값 객체입니다.
/// 이 객체는 불변입니다. 생성 후 값을 변경할 수 없습니다.
/// </summary>
public sealed class DifficultyMultiplier
{
    /// <summary>
    /// 몬스터 스탯에 적용되는 배수 (예: HP, 공격력)
    /// </summary>
    public double MonsterStatMultiplier { get; }

    /// <summary>
    /// 보상에 적용되는 배수 (예: 경험치, 화폐)
    /// </summary>
    public double RewardMultiplier { get; }

    /// <summary>
    /// 희귀 아이템 발견 확률에 적용되는 배수
    /// </summary>
    public double DropChanceMultiplier { get; }

    // 팩토리 메서드를 통한 생성을 강제하기 위해 생성자는 private입니다.
    private DifficultyMultiplier(double monsterStatMultiplier, double rewardMultiplier, double dropChanceMultiplier)
    {
        // 더 복잡한 시나리오에서는 여기에 유효성 검사를 추가할 수 있습니다. 예: 배수가 양수인지 확인
        MonsterStatMultiplier = monsterStatMultiplier;
        RewardMultiplier = rewardMultiplier;
        DropChanceMultiplier = dropChanceMultiplier;
    }

    /// <summary>
    /// 선택된 난이도를 기반으로 DifficultyMultiplier 인스턴스를 생성하는 팩토리 메서드입니다.
    /// 이것은 이 객체를 생성하는 단일 진입점으로, 비즈니스 로직을 중앙화합니다.
    /// </summary>
    /// <param name="difficulty">던전 난이도 enum</param>
    /// <returns>올바른 값을 가진 새로운 DifficultyMultiplier 인스턴스</returns>
    /// <exception cref="ArgumentOutOfRangeException">난이도가 지원되지 않는 경우 발생</exception>
    public static DifficultyMultiplier Create(DungeonDifficulty difficulty)
    {
        return difficulty switch
        {
            DungeonDifficulty.Normal    => new DifficultyMultiplier(1.0, 1.0, 1.0),
            DungeonDifficulty.Hard      => new DifficultyMultiplier(1.5, 1.25, 1.1),
            DungeonDifficulty.Nightmare => new DifficultyMultiplier(2.5, 1.75, 1.25),
            // 가드 절입니다. 향후 새로운 enum 값이 추가되면
            // 이 코드가 명시적으로 실패하여 개발자가 배수를 정의하도록 강제합니다.
            _ => throw new ArgumentOutOfRangeException(nameof(difficulty), $"Difficulty '{difficulty}' is not supported.")
        };
    }
}
