namespace IdleRPG.Domain.Entities;

/// <summary>
/// 캐릭터의 모든 난이도별 전투 스테이지 진행 상황을 추적합니다.
/// 각 난이도별로 최고 클리어한 스테이지를 별도 필드로 추적합니다.
/// Gemini의 "3필드" 개선 방식 - 단순하면서도 유연합니다.
/// </summary>
public class CharacterBattleProgress
{
    /// <summary>
    /// 기본 키
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 이 진행 상황 레코드를 소유한 캐릭터
    /// </summary>
    public Guid CharacterId { get; set; }

    /// <summary>
    /// 캐릭터로의 탐색 속성
    /// </summary>
    public Character? Character { get; set; }

    /// <summary>
    /// 노말 난이도에서 클리어한 최고 스테이지
    /// 0은 아직 클리어한 스테이지가 없음을 의미합니다.
    /// </summary>
    public int HighestStageClearedNormal { get; set; }

    /// <summary>
    /// 하드 난이도에서 클리어한 최고 스테이지
    /// 0은 아직 클리어한 스테이지가 없음을 의미합니다.
    /// </summary>
    public int HighestStageClearedHard { get; set; }

    /// <summary>
    /// 나이트메어 난이도에서 클리어한 최고 스테이지
    /// 0은 아직 클리어한 스테이지가 없음을 의미합니다.
    /// </summary>
    public int HighestStageClearedNightmare { get; set; }

    /// <summary>
    /// 이 진행 상황 레코드가 생성된 시간
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 이 진행 상황 레코드가 마지막으로 업데이트된 시간
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 지정된 난이도의 최고 클리어 스테이지를 조회합니다.
    /// Switch 중복을 제거하기 위한 헬퍼 메서드입니다.
    /// </summary>
    /// <param name="difficulty">조회할 난이도</param>
    /// <returns>해당 난이도의 최고 클리어 스테이지 ID (0이면 클리어한 스테이지 없음)</returns>
    public int GetHighestStageCleared(Enums.DungeonDifficulty difficulty)
    {
        return difficulty switch
        {
            Enums.DungeonDifficulty.Normal => HighestStageClearedNormal,
            Enums.DungeonDifficulty.Hard => HighestStageClearedHard,
            Enums.DungeonDifficulty.Nightmare => HighestStageClearedNightmare,
            _ => 0
        };
    }

    /// <summary>
    /// 지정된 난이도의 최고 클리어 스테이지를 업데이트합니다.
    /// Switch 중복을 제거하기 위한 헬퍼 메서드입니다.
    /// </summary>
    /// <param name="difficulty">업데이트할 난이도</param>
    /// <param name="stageId">새로운 최고 스테이지 ID</param>
    public void SetHighestStageCleared(Enums.DungeonDifficulty difficulty, int stageId)
    {
        switch (difficulty)
        {
            case Enums.DungeonDifficulty.Normal:
                HighestStageClearedNormal = stageId;
                break;
            case Enums.DungeonDifficulty.Hard:
                HighestStageClearedHard = stageId;
                break;
            case Enums.DungeonDifficulty.Nightmare:
                HighestStageClearedNightmare = stageId;
                break;
        }
        UpdatedAt = DateTime.UtcNow;
    }
}
