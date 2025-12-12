using System;

namespace IdleRPG.Network.DTO.Combat
{
    // ============================================================
    // Combat System Refactoring DTOs (v1.8)
    // 작성일: 2025-10-29
    // Breaking Changes: v1.7 → v1.8 (엔드포인트 변경)
    // ============================================================

    // ============================================================
    // 1. Stage API DTOs
    // ============================================================

    #region Stage API

    /// <summary>
    /// 스테이지 정보 DTO
    /// GET /api/stages
    /// </summary>
    [Serializable]
    public class StageDto
    {
        public int id;
        public string name;
        public string difficulty;  // "Normal", "Hard", "Nightmare"
        public int requiredLevel;
        public long finalGoldReward;   // 난이도 배율 적용된 골드
        public long finalExpReward;    // 난이도 배율 적용된 경험치
        public bool isAvailable;       // 도전 가능 여부
    }

    /// <summary>
    /// 스테이지 목록 응답 Wrapper
    /// Unity JsonUtility는 배열을 직접 파싱 못하므로 Wrapper 필요
    /// </summary>
    [Serializable]
    public class StageListResponseWrapper
    {
        public StageDto[] stages;
    }

    /// <summary>
    /// 스테이지 진행도 DTO
    /// GET /api/stages/progress
    /// </summary>
    [Serializable]
    public class StageProgressDto
    {
        public string characterId;       // Guid (string으로 처리)
        public int normalHighestStage;
        public int hardHighestStage;
        public int hellHighestStage;     // 서버는 Nightmare, Unity는 Hell
    }

    /// <summary>
    /// 스테이지 클리어 요청 DTO
    /// POST /api/stages/clear
    /// </summary>
    [Serializable]
    public class StageClearRequest
    {
        public string characterId;   // Guid
        public int stageId;
        public string difficulty;    // "Normal", "Hard", "Nightmare"
    }

    /// <summary>
    /// 보상 정보 DTO
    /// </summary>
    [Serializable]
    public class RewardDto
    {
        public long gold;
        public long experience;
    }

    /// <summary>
    /// 전투 통계 DTO
    /// </summary>
    [Serializable]
    public class BattleStatisticsDto
    {
        public int totalTurns;
        public long totalDamageDealt;
        public long totalDamageTaken;
        public int criticalHitCount;
        public int evasionCount;
    }

    /// <summary>
    /// 스테이지 클리어 응답 DTO
    /// POST /api/stages/clear (Response)
    /// </summary>
    [Serializable]
    public class StageClearResponse
    {
        public bool isSuccess;
        public RewardDto reward;             // null if failed
        public int newHighestStage;
        public bool isLevelUp;
        public int currentLevel;
        public EquipmentDto[] droppedEquipments;  // Drop System 구현 후 사용
        public BattleStatisticsDto battleStatistics;
        public string errorMessage;          // isSuccess = false일 때
    }

    /// <summary>
    /// 장비 드랍 DTO (Drop System 구현 후 사용)
    /// </summary>
    [Serializable]
    public class EquipmentDto
    {
        public string id;
        public string name;
        public string rarity;
        public int attack;
        public int defense;
    }

    #endregion

    // ============================================================
    // 2. Battle Log API DTOs
    // ============================================================

    #region Battle Log API

    /// <summary>
    /// 전투 로그 상세 DTO
    /// GET /api/battle-logs
    /// </summary>
    [Serializable]
    public class BattleLogDto
    {
        public string id;              // Guid
        public string monsterName;
        public int monsterLevel;
        public bool isVictory;
        public int experienceGained;
        public int goldGained;
        public long damageDealt;
        public long damageTaken;
        public string battleDate;      // ISO 8601 형식 (DateTime → string)
    }

    /// <summary>
    /// 전투 로그 목록 응답 DTO (페이징)
    /// GET /api/battle-logs
    /// </summary>
    [Serializable]
    public class BattleLogListResponse
    {
        public BattleLogDto[] logs;
        public int currentPage;
        public int pageSize;
        public int totalCount;
        public int totalPages;
    }

    /// <summary>
    /// 최근 전투 로그 DTO (간소화 버전)
    /// GET /api/battle-logs/recent
    /// </summary>
    [Serializable]
    public class RecentBattleLogDto
    {
        public string id;              // Guid
        public string monsterName;
        public bool isVictory;
        public string battleDate;      // ISO 8601 형식
    }

    /// <summary>
    /// 최근 전투 로그 목록 Wrapper
    /// </summary>
    [Serializable]
    public class RecentBattleLogListWrapper
    {
        public RecentBattleLogDto[] logs;
    }

    /// <summary>
    /// 전투 통계 DTO
    /// GET /api/battle-logs/stats
    /// </summary>
    [Serializable]
    public class BattleStatsDto
    {
        public int totalBattles;
        public int victories;
        public int defeats;
        public float winRate;          // 0.0 ~ 100.0 (백분율)
        public long totalDamageDealt;
        public long totalDamageTaken;
    }

    #endregion

    // ============================================================
    // 3. Special Dungeon API DTOs (Phase 3 예정)
    // ============================================================

    #region Special Dungeon API

    /// <summary>
    /// 보스 던전 도전 요청 DTO
    /// POST /api/special-dungeons/boss/challenge
    /// </summary>
    [Serializable]
    public class BossChallengeRequest
    {
        public string characterId;   // Guid
        public string bossId;        // Guid
    }

    /// <summary>
    /// 보스 던전 도전 응답 DTO
    /// </summary>
    [Serializable]
    public class BossChallengeResponse
    {
        public bool isVictory;
        public RewardDto reward;
        public BattleStatisticsDto battleStatistics;
    }

    #endregion

    // ============================================================
    // 4. Enum 정의
    // ============================================================

    #region Enums

    /// <summary>
    /// 던전 난이도
    /// 서버: Normal, Hard, Nightmare
    /// 참고: Unity에서는 "Hell" 대신 "Nightmare" 사용
    /// </summary>
    public enum DungeonDifficulty
    {
        Normal,
        Hard,
        Nightmare
    }

    #endregion

    // ============================================================
    // 5. 사용 예시 (주석)
    // ============================================================

    /*
    // 예시 1: 스테이지 목록 조회
    string url = $"{BASE_URL}/api/stages?characterId={characterId}";
    using (UnityWebRequest request = UnityWebRequest.Get(url))
    {
        yield return request.SendWebRequest();
        var response = JsonUtility.FromJson<StageListResponseWrapper>(
            "{\"stages\":" + request.downloadHandler.text + "}"
        );
    }

    // 예시 2: 스테이지 클리어
    var clearRequest = new StageClearRequest
    {
        characterId = "guid",
        stageId = 5,
        difficulty = "Normal"
    };
    string jsonData = JsonUtility.ToJson(clearRequest);
    // ... POST 요청

    // 예시 3: 전투 로그 조회
    string url = $"{BASE_URL}/api/battle-logs?characterId={characterId}&page=1&pageSize=20";
    using (UnityWebRequest request = UnityWebRequest.Get(url))
    {
        SetAuthHeader(request);
        yield return request.SendWebRequest();
        var response = JsonUtility.FromJson<BattleLogListResponse>(request.downloadHandler.text);
    }

    // 예시 4: 전투 통계 조회
    string url = $"{BASE_URL}/api/battle-logs/stats?characterId={characterId}";
    using (UnityWebRequest request = UnityWebRequest.Get(url))
    {
        SetAuthHeader(request);
        yield return request.SendWebRequest();
        var stats = JsonUtility.FromJson<BattleStatsDto>(request.downloadHandler.text);
        Debug.Log($"승률: {stats.winRate}%");
    }
    */
}
